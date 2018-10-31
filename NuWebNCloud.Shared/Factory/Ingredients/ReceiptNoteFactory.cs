using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Xero.GenerateInvoice;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class ReceiptNoteFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private ReceiptNoteDetailFactory _RNDetailFactory = null;

        private PurchaseOrderDetailFactory _PODetailFactory = null;
        private ReceiptPurchaseOrderFactory _RPOFactory = null;
        private PurchaseOrderFactory _POFactory = null;
        private InventoryFactory _inventoryFactory = null;
        public ReceiptNoteFactory()
        {
            _baseFactory = new BaseFactory();
            _RNDetailFactory = new ReceiptNoteDetailFactory();
            _PODetailFactory = new PurchaseOrderDetailFactory();
            _RPOFactory = new ReceiptPurchaseOrderFactory();
            _POFactory = new PurchaseOrderFactory();
            _inventoryFactory = new InventoryFactory();
            //============
        }

        public bool Insert(ReceiptNoteModels model, List<string> ListStoreId, ref string msg)
        {
            using (var cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        ResultModels resultModels = new ResultModels();
                        List<InventoryModels> lstInventory = new List<InventoryModels>();

                        var item = new I_ReceiptNote();
                        string ReceiptNoteId = Guid.NewGuid().ToString();

                        item.Id = ReceiptNoteId;
                        item.ReceiptNo = CommonHelper.GetGenNo(Commons.ETableZipCode.ReceiptNote, model.StoreId);
                        item.Status = (int)Commons.EReceiptNoteStatus.Closed;
                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.UpdatedBy = model.UpdatedBy;
                        item.UpdatedDate = model.UpdatedDate;
                        item.StoreId = model.StoreId;
                        item.SupplierId = model.IsPurchaseOrder ? model.SupplierId : null;
                        item.ReceiptBy = model.ReceiptBy;
                        item.ReceiptDate = model.ReceiptDate;
                        cxt.I_ReceiptNote.Add(item);

                        List<string> ListPOId = new List<string>();
                        //===============Choose PO
                        if (model.IsPurchaseOrder)
                        {
                            //======== Insert Receipt Purchase Order
                            List<I_Receipt_Purchase_Order> RPOModels = new List<I_Receipt_Purchase_Order>();
                            foreach (var RNDetailParent in model.ListPurchaseOrder)
                            {
                                RPOModels.Add(new I_Receipt_Purchase_Order
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    PurchaseOrderId = RNDetailParent.Id,
                                    ReceiptNoteId = ReceiptNoteId,
                                    CreatedBy = model.CreatedBy,
                                    CreatedDate = model.CreatedDate,
                                    ModifierBy = model.UpdatedBy,
                                    ModifierDate = model.UpdatedDate,
                                    IsActived = true
                                });
                                ListPOId.Add(RNDetailParent.Id);
                            }
                            cxt.I_Receipt_Purchase_Order.AddRange(RPOModels);
                            //_RPOFactory.Insert(RPOModels, ref msg);

                            //========= Change Status PO
                            //_POFactory.ChangeStatus(ListPOId, (int)Commons.EPOStatus.InProgress);
                            var lstObj = cxt.I_Purchase_Order.Where(ww => ListPOId.Contains(ww.Id)).ToList();
                            if (lstObj != null && lstObj.Count > 0)
                            {
                                //check if approve before set in progress
                                foreach (var purchase in lstObj)
                                {
                                    if (purchase.Status == (int)Commons.EPOStatus.Approved)
                                    {
                                        purchase.Status = (int)Commons.EPOStatus.InProgress;
                                    }
                                }
                                //lstObj.ForEach(ss => ss.Status = (int)Commons.EPOStatus.InProgress);
                            }

                            //======== Update ReceiptNoteQty
                            List<PurchaseOrderDetailModels> PODetailModels = new List<PurchaseOrderDetailModels>();
                            foreach (var RNDetailParent in model.ListPurchaseOrder)
                            {
                                foreach (var RNDetailChild in RNDetailParent.ListItem)
                                {
                                    var itemUpdate = (from tb in cxt.I_Purchase_Order_Detail
                                                      where tb.Id == RNDetailChild.Id
                                                      select tb).FirstOrDefault();
                                    if (itemUpdate != null)
                                    {
                                        itemUpdate.ReceiptNoteQty += RNDetailChild.ReceivingQty;
                                        RNDetailChild.ReturnReceiptNoteQty = itemUpdate.ReturnReceiptNoteQty.HasValue ? itemUpdate.ReturnReceiptNoteQty.Value : 0;
                                        RNDetailChild.ReceiptNoteQty = itemUpdate.ReceiptNoteQty.HasValue ? itemUpdate.ReceiptNoteQty.Value : 0;
                                    }
                                }
                            }
                            // _PODetailFactory.UpdateReceiptNoteQty(PODetailModels, ref msg);
                        }
                        //========Insert  I_ReceiptNoteDetail
                        List<I_ReceiptNoteDetail> ListInsertRND = new List<I_ReceiptNoteDetail>();

                        if (model.IsPurchaseOrder)
                        {
                            foreach (var RNDetailParent in model.ListPurchaseOrder)
                            {
                                foreach (var RNDetailChild in RNDetailParent.ListItem)
                                {
                                    ListInsertRND.Add(new I_ReceiptNoteDetail
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IsActived = true,
                                        PurchaseOrderDetailId = RNDetailChild.Id,
                                        ReceiptNoteId = ReceiptNoteId,
                                        ReceivedQty = RNDetailChild.ReceivedQty,
                                        ReceivingQty = RNDetailChild.ReceivingQty,
                                        RemainingQty = (RNDetailChild.Qty - RNDetailChild.ReceiptNoteQty
                                                + RNDetailChild.ReturnReceiptNoteQty) /*+ RNDetailChild.ReturnReceiptNoteQty*/,
                                        Status = (int)Commons.EStatus.Actived,
                                        //=====Base Receiving Qty
                                        BaseReceivingQty = (RNDetailChild.ReceivingQty * RNDetailChild.BaseQty) / RNDetailChild.Qty
                                    });
                                    lstInventory.Add(new InventoryModels()
                                    {
                                        StoreId = model.StoreId,
                                        IngredientId = RNDetailChild.IngredientId,
                                        Price = RNDetailChild.UnitPrice,
                                        Quantity = (RNDetailChild.ReceivingQty * RNDetailChild.BaseQty) / RNDetailChild.Qty
                                    });
                                }
                            }
                        }
                        else //Choose Item
                        {
                            foreach (var itemIngredient in model.ListItem)
                            {
                                ListInsertRND.Add(new I_ReceiptNoteDetail
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IsActived = true,
                                    //PurchaseOrderDetailId = Guid.Empty.ToString(),
                                    IngredientId = itemIngredient.IngredientId,
                                    UOMId = itemIngredient.BaseUOMId,
                                    ReceiptNoteId = ReceiptNoteId,
                                    ReceivedQty = itemIngredient.Qty,
                                    ReceivingQty = itemIngredient.Qty,
                                    RemainingQty = itemIngredient.Qty,
                                    Status = (int)Commons.EStatus.Actived,
                                    //=====Base Receiving Qty
                                    BaseReceivingQty = itemIngredient.BaseReceivingQty
                                });

                                lstInventory.Add(new InventoryModels()
                                {
                                    StoreId = model.StoreId,
                                    IngredientId = itemIngredient.IngredientId,
                                    Price = 0,
                                    Quantity = itemIngredient.BaseReceivingQty
                                });
                            }
                        }

                        //================================
                        //_RNDetailFactory.Insert(models, ref msg);
                        cxt.I_ReceiptNoteDetail.AddRange(ListInsertRND);
                        //=============
                        cxt.SaveChanges();
                        transaction.Commit();

                        if (model.IsPurchaseOrder)
                        {
                            //Auto change statu for list PO
                            _inventoryFactory.ClosePOAuto(ListPOId);
                        }

                        if (model.IsPurchaseOrder)
                        {
                            /* GENERAL INVOICE XERO */
                            var infoXero = Commons.GetIntegrateInfo(model.StoreId);
                            if (infoXero != null)
                            {
                                /* find supplier name by supplier id */
                                var SupplierName = cxt.I_Supplier.Where(o => o.Id.Equals(model.SupplierId)).Select(o => o.Name).FirstOrDefault();
                                XeroFactory _facXero = new XeroFactory();
                                //get tax rate
                                string TaxType = "TAX002";
                                NuWebNCloud.Shared.Factory.Settings.TaxFactory _taxFactory = new NuWebNCloud.Shared.Factory.Settings.TaxFactory();
                                var lstTaxes = _taxFactory.GetListTaxV2(model.StoreId);
                                var tax = lstTaxes.Where(w => w.IsActive && !string.IsNullOrEmpty(w.Rate)).FirstOrDefault();
                                if (tax != null)
                                    TaxType = tax.Rate;

                                /* GET LIST ITEM */
                                var ListIngXero = cxt.I_Ingredient.Where(s => s.Status != (byte)Commons.EStatus.Deleted).ToList();
                                //----------------------------
                                var listItem = new List<InvoiceLineItemModels>();
                                if (model.ListPurchaseOrder != null && model.ListPurchaseOrder.Any())
                                {
                                    //--------------
                                    string _AccountCode = Commons.AccountCode_Inventory;
                                    var StockOnHand = (from _store in cxt.G_SettingOnStore
                                                          from _setting in cxt.G_GeneralSetting
                                                          where _store.SettingId == _setting.Id && ListStoreId.Contains(_store.StoreId)
                                                                && _store.Status && _setting.Status
                                                                && _setting.Code.Equals((byte)Commons.EGeneralSetting.StockOnHand)
                                                          select _store).FirstOrDefault();
                                    if (StockOnHand != null)
                                        _AccountCode = StockOnHand.Value;

                                    foreach (var RNDetailParent in model.ListPurchaseOrder)
                                    {
                                        foreach (var _item in RNDetailParent.ListItem)
                                        {
                                            var objIng = ListIngXero.Where(z => z.Id.Equals(_item.IngredientId)).FirstOrDefault();
                                            listItem.Add(new InvoiceLineItemModels
                                            {
                                                Description = objIng != null ? objIng.Name : "",
                                                ItemCode = objIng != null ? objIng.Code : "",

                                                Quantity = Convert.ToDecimal(_item.Qty),
                                                UnitAmount = Convert.ToDecimal(_item.UnitPrice),
                                                LineAmount = Convert.ToDecimal(_item.Qty * _item.UnitPrice),// = Amount 

                                                AccountCode = _AccountCode,//Commons.AcoountCode_200,
                                                TaxType = TaxType
                                            });
                                        }
                                    }
                                }
                                var modelXero = new GenerateInvoiceModels
                                {
                                    AppRegistrationId = infoXero.ThirdPartyID,
                                    StoreId = infoXero.IPAddress,
                                    CurrencyCode = Commons._XeroCurrencyCode,
                                    Reference = item.ReceiptNo,
                                    Contact = new InvoiceContactGRNModels
                                    {
                                        Name = SupplierName,
                                    },
                                    DueDate = DateTime.Now,
                                    ClosingDatetime = DateTime.Now,

                                    InvoiceType = (byte)Commons.EInvoiceType.AccountsPayable,
                                    LineAmountType = (byte)Commons.ELineAmountType.Inclusive,
                                    InvoiceStatus = (byte)Commons.EInvoiceStatus.Authorised,

                                    Items = listItem
                                };
                                var msgXero = string.Empty;
                                var data = new GIResponseModels();
                                var resultXero = _facXero.GenerateInvoice(infoXero.ApiURL, modelXero, ref msgXero);
                            }
                        }

                        _inventoryFactory.UpdateInventoryForReceiptNote(lstInventory, ReceiptNoteId, ref resultModels);
                        NSLog.Logger.Info(string.Format("UpdateInventoryForReceiptNote:  [{0}] - [{1}]- [{2}]", resultModels.IsOk, ReceiptNoteId, resultModels.Message));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert RN Error:",ex);
                        transaction.Rollback();
                        return false;
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
        }

        public bool ChangeStatus(List<string> lstId, int status)
        {
            using (var cxt = new NuWebContext())
            {
                var lstObj = cxt.I_ReceiptNote.Where(ww => lstId.Contains(ww.Id)).ToList();
                if (lstObj != null && lstObj.Count > 0)
                {
                    lstObj.ForEach(ss => ss.Status = status);
                    cxt.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<ReceiptNoteModels> GetData(ReceiptNoteViewModels model, List<string> ListStoreId, List<string> listCompanyId)
        {
            List<ReceiptNoteModels> lstData = new List<ReceiptNoteModels>();
            using (var cxt = new NuWebContext())
            {
                lstData = (from rn in cxt.I_ReceiptNote
                           from s in cxt.I_Supplier
                           where ListStoreId.Contains(rn.StoreId) && rn.SupplierId.Equals(s.Id)
                                    && listCompanyId.Contains(s.CompanyId)
                                    && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                           select new ReceiptNoteModels()
                           {
                               Id = rn.Id,
                               ReceiptNo = rn.ReceiptNo,
                               ListPONo = "",

                               Status = rn.Status,
                               ReceiptDate = rn.ReceiptDate,
                               SupplierName = s.Name,
                               ReceiptBy = rn.ReceiptBy,

                               StoreId = rn.StoreId,

                               SupplierId = rn.SupplierId
                           }).ToList();
                //================= List Data Not Supplier
                List<ReceiptNoteModels> lstDataNotSupplier = new List<ReceiptNoteModels>();
                lstDataNotSupplier = (from rn in cxt.I_ReceiptNote
                                      where ListStoreId.Contains(rn.StoreId) && (rn.SupplierId.Equals(Guid.Empty.ToString()) || string.IsNullOrEmpty(rn.SupplierId))
                                      select new ReceiptNoteModels()
                                      {
                                          Id = rn.Id,
                                          ReceiptNo = rn.ReceiptNo,
                                          ListPONo = "",
                                          Status = rn.Status,
                                          ReceiptDate = rn.ReceiptDate,
                                          SupplierName = "",
                                          ReceiptBy = rn.ReceiptBy,
                                          StoreId = rn.StoreId,
                                          SupplierId = rn.SupplierId
                                      }).ToList();
                lstData.AddRange(lstDataNotSupplier);
                //=================
                lstData.ForEach(x =>
                {
                    //check RN 
                    var isHaveItem = (from RND in cxt.I_ReceiptNoteDetail
                                      where RND.ReceiptNoteId.Equals(x.Id) && (RND.PurchaseOrderDetailId.Equals(Guid.Empty.ToString()) || string.IsNullOrEmpty(RND.PurchaseOrderDetailId))
                                      select RND.Id).Any();
                    if (isHaveItem)
                        x.IsPurchaseOrder = false;
                    //=============
                    if (x.IsPurchaseOrder)
                    {
                        var lstPO = (from RNO in cxt.I_Receipt_Purchase_Order
                                     from PO in cxt.I_Purchase_Order
                                     where RNO.PurchaseOrderId.Equals(PO.Id) && RNO.ReceiptNoteId.Equals(x.Id)
                                     orderby PO.CreatedDate descending
                                     select PO.Code).ToList();
                        if (lstPO != null)
                        {
                            x.ListPONo = string.Join(", ", lstPO);
                        }
                        ///Get List Retunr Note
                        x.ListReturnNote = (from RT in cxt.I_Return_Note
                                            let ReturnQty = cxt.I_Return_Note_Detail.Where(z => z.ReturnNoteId.Equals(RT.Id)).Select(z => z.ReturnQty).DefaultIfEmpty(0).Sum()
                                            where x.Id.Equals(RT.ReceiptNoteId)
                                            orderby RT.CreatedDate descending
                                            select new ReturnNoteModels()
                                            {
                                                Id = RT.Id,

                                                ReturnNoteNo = RT.ReturnNoteNo,
                                                TotalQty = ReturnQty
                                            }).ToList();
                    }
                });
                if (!string.IsNullOrEmpty(model.StoreID))
                {
                    lstData = lstData.Where(ww => ww.StoreId.Equals(model.StoreID)).ToList();
                }
                if (model.ListSupplierId != null && model.ListSupplierId.Count > 0)
                {
                    lstData = lstData.Where(ww => model.ListSupplierId.Contains(ww.SupplierId)).ToList();
                }
                if (model.ReceiptNoteDate != null)
                {
                    DateTime fromDate = new DateTime(model.ReceiptNoteDate.Value.Year, model.ReceiptNoteDate.Value.Month, model.ReceiptNoteDate.Value.Day, 0, 0, 0);
                    DateTime ToDate = new DateTime(model.ReceiptNoteDate.Value.Year, model.ReceiptNoteDate.Value.Month, model.ReceiptNoteDate.Value.Day, 23, 59, 59);
                    lstData = lstData.Where(ww => ww.ReceiptDate >= fromDate && ww.ReceiptDate <= ToDate).ToList();
                }
                return lstData;
            }
        }

        public ReceiptNoteModels GetReceiptNoteById(string id, List<string> listCompanyId)
        {
            using (var cxt = new NuWebContext())
            {
                var model = (from rn in cxt.I_ReceiptNote
                             from s in cxt.I_Supplier
                             where rn.Id.Equals(id) && rn.SupplierId.Equals(s.Id)
                             && listCompanyId.Contains(s.CompanyId) && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                             select new ReceiptNoteModels()
                             {
                                 Id = rn.Id,
                                 ReceiptNo = rn.ReceiptNo,
                                 ListPONo = "",

                                 Status = rn.Status,
                                 ReceiptDate = rn.ReceiptDate,
                                 SupplierName = s.Name,
                                 ReceiptBy = rn.ReceiptBy,

                                 StoreId = rn.StoreId,
                             }).FirstOrDefault();
                //================= RN Not Supplier
                if (model == null)
                {
                    model = (from rn in cxt.I_ReceiptNote
                             where rn.Id.Equals(id) && (rn.SupplierId.Equals(Guid.Empty.ToString()) || string.IsNullOrEmpty(rn.SupplierId))
                             select new ReceiptNoteModels()
                             {
                                 Id = rn.Id,
                                 ReceiptNo = rn.ReceiptNo,
                                 ListPONo = "",

                                 Status = rn.Status,
                                 ReceiptDate = rn.ReceiptDate,
                                 SupplierName = "",
                                 ReceiptBy = rn.ReceiptBy,

                                 StoreId = rn.StoreId,
                             }).FirstOrDefault();
                }
                //check RN 
                if (model != null)
                {
                    var lstRNItem = (from RND in cxt.I_ReceiptNoteDetail
                                     from I in cxt.I_Ingredient
                                     from UOM in cxt.I_UnitOfMeasure
                                     where RND.IngredientId.Equals(I.Id) && I.Status != (int)Commons.EStatus.Deleted
                                            && RND.UOMId.Equals(UOM.Id) && UOM.Status != (int)Commons.EStatus.Deleted
                                            && RND.ReceiptNoteId.Equals(model.Id)
                                            && (RND.PurchaseOrderDetailId.Equals(Guid.Empty.ToString()) || string.IsNullOrEmpty(RND.PurchaseOrderDetailId))
                                     select new { RND, I, UOM }).ToList();
                    if (lstRNItem != null && lstRNItem.Count > 0)
                    {
                        model.IsPurchaseOrder = false;
                        //Choose Item
                        foreach (var item in lstRNItem)
                        {
                            model.ListItem.Add(new ReceiptNoteIngredient
                            {
                                BaseUOM = item.UOM.Name,
                                IngredientCode = item.I.Code,
                                IngredientName = item.I.Name,
                                Qty = item.RND.ReceivingQty,
                                BaseReceivingQty = item.RND.BaseReceivingQty.Value
                            });
                        }
                    }
                }

                //=====Choose PO
                if (model.IsPurchaseOrder)
                {
                    var lstData = (from RND in cxt.I_ReceiptNoteDetail
                                   from POD in cxt.I_Purchase_Order_Detail
                                   from I in cxt.I_Ingredient
                                   from UOM in cxt.I_UnitOfMeasure
                                   from PO in cxt.I_Purchase_Order
                                   let _ReturnQty = cxt.I_Return_Note_Detail.Where(z => RND.Id.Equals(z.ReceiptNoteDetailId)).Select(z => z.ReturnQty).DefaultIfEmpty(0).Sum()

                                   where RND.ReceiptNoteId.Equals(model.Id)
                                          && RND.PurchaseOrderDetailId.Equals(POD.Id) && POD.Status != (int)Commons.EStatus.Deleted
                                          && POD.PurchaseOrderId.Equals(PO.Id)
                                          && POD.IngredientId.Equals(I.Id) && I.Status != (int)Commons.EStatus.Deleted
                                          && I.ReceivingUOMId.Equals(UOM.Id) && UOM.Status != (int)Commons.EStatus.Deleted
                                   select new { RND, POD, I, UOM, PO/*, RTD*/ , _ReturnQty }).ToList();
                    if (lstData != null)
                    {
                        var lstPO = lstData.GroupBy(x => new { x.PO.Id, x.PO.Code }).
                                            Select(x => new
                                            {
                                                Id = x.Key.Id,
                                                PONumber = x.Key.Code,
                                            }).ToList();
                        model.ListPONo = string.Join(", ", lstPO.Select(x => x.PONumber).ToList());
                        //======Get List PO
                        foreach (var item in lstPO)
                        {
                            var listItemDetail = lstData.Where(x => x.PO.Id.Equals(item.Id)).Select(x => new POIngredient()
                            {
                                Id = x.POD.Id,

                                IngredientId = x.POD.IngredientId,
                                BaseUOM = x.UOM.Name,
                                IngredientCode = x.I.Code,
                                IngredientName = x.I.Name,

                                Qty = x.POD.Qty,
                                UnitPrice = x.POD.UnitPrice,
                                Amount = x.POD.Amount,

                                ReceivingQty = x.RND.ReceivingQty,
                                RemainingQty = x.RND.RemainingQty,
                                //RemainingQty = (x.POD.Qty - x.POD.ReceiptNoteQty.Value + x.POD.ReturnReceiptNoteQty.Value),

                                ReturnQty = /*x.RTD == null ? 0 : x.RTD.*/x._ReturnQty
                            }).ToList();

                            model.ListPurchaseOrder.Add(new PurchaseOrderModels
                            {
                                Id = item.Id,
                                PONumber = item.PONumber,
                                ListItem = listItemDetail
                            });
                        }
                    }
                }
                //============
                return model;
            }
        }

        #region Return Note
        public ReceiptNoteModels GetViewReceiptNoteReturnNote_Old(ReturnNoteReceiptView RTNRmodel)
        {
            using (var cxt = new NuWebContext())
            {
                var model = (from rn in cxt.I_ReceiptNote
                             from s in cxt.I_Supplier
                             where rn.Id.Equals(RTNRmodel.ReceiptId) && rn.SupplierId.Equals(s.Id)
                                    && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                             select new ReceiptNoteModels()
                             {
                                 Id = rn.Id,
                                 ReceiptNo = rn.ReceiptNo,
                                 ReceiptDate = rn.ReceiptDate,
                                 SupplierName = s.Name,
                                 ReceiptBy = rn.ReceiptBy,
                                 StoreId = rn.StoreId,
                             }).FirstOrDefault();

                var lstRT = (from RT in cxt.I_Return_Note
                             from RTD in cxt.I_Return_Note_Detail
                             from RND in cxt.I_ReceiptNoteDetail
                             where RT.Id.Equals(RTD.ReturnNoteId) && RT.ReceiptNoteId.Equals(model.Id)
                                   && RTD.ReceiptNoteDetailId.Equals(RND.Id) && RTNRmodel.ListReturnNoteId.Contains(RT.Id)
                                   && (RND.Status != (int)Commons.EStatus.Deleted && RND.Status != null)
                             select new
                             {
                                 Id = RT.Id,
                                 RTNumber = RT.ReturnNoteNo,
                                 ReturnBy = RT.CreatedBy,
                                 CreatedDate = RT.CreatedDate,
                             }).Distinct().ToList();
                if (lstRT != null)
                {
                    foreach (var itemRT in lstRT)
                    {
                        List<PurchaseOrderModels> ListPurchaseOrder = new List<PurchaseOrderModels>();
                        var lstData = (from RND in cxt.I_ReceiptNoteDetail
                                           //from RTD in cxt.I_Return_Note_Detail
                                           //from RT in cxt.I_Return_Note
                                           //let _ReturnQty = cxt.I_Return_Note_Detail.Where(z => RND.Id.Equals(z.ReceiptNoteDetailId)).Select(z => z.ReturnQty).DefaultIfEmpty(0).Sum()
                                       from rn in cxt.I_Return_Note_Detail.Where(ww => ww.ReceiptNoteDetailId == RND.Id).DefaultIfEmpty()
                                       from POD in cxt.I_Purchase_Order_Detail
                                       from I in cxt.I_Ingredient
                                       from UOM in cxt.I_UnitOfMeasure
                                       from PO in cxt.I_Purchase_Order
                                       where POD.PurchaseOrderId.Equals(PO.Id) && POD.Id.Equals(RND.PurchaseOrderDetailId)
                                              && RND.ReceiptNoteId.Equals(model.Id)
                                             && RTNRmodel.ListReturnNoteId.Contains(rn.ReturnNoteId)
                                              && (RND.Status != (int)Commons.EStatus.Deleted && RND.Status != null)
                                              && POD.IngredientId.Equals(I.Id) && I.Status != (int)Commons.EStatus.Deleted
                                              && I.BaseUOMId.Equals(UOM.Id) && UOM.Status != (int)Commons.EStatus.Deleted
                                       select new { RND, /*RTD, RT,*/ POD, I, UOM, PO, rn }).ToList();

                        if (lstData != null)
                        {
                            var lstPO = lstData.GroupBy(x => new { x.PO.Id, x.PO.Code, x.rn.ReturnNoteId }).
                                       Select(x => new
                                       {
                                           Id = x.Key.Id,
                                           PONumber = x.Key.Code,
                                           ReturnNoteId = x.Key.ReturnNoteId
                                       }).ToList();

                            //======Get List PO
                            foreach (var item in lstPO)
                            {
                                //Get list Purchase Order Detail
                                //var ListItemDetail = _PODetailFactory.GetData(item.Id);

                                var listItemDetail = lstData.Where(x => x.PO.Id.Equals(item.Id)).Select(x => new POIngredient()
                                {
                                    Id = x.POD.Id,

                                    IngredientId = x.POD.IngredientId,
                                    BaseUOM = x.UOM.Name,
                                    IngredientCode = x.I.Code,
                                    IngredientName = x.I.Name,

                                    Qty = x.POD.Qty,
                                    UnitPrice = x.POD.UnitPrice,
                                    Amount = x.POD.Amount,

                                    ReceivingQty = x.RND.ReceivingQty,
                                    RemainingQty = x.RND.RemainingQty,
                                    //RemainingQty = (x.POD.Qty - x.POD.ReceiptNoteQty.Value + x.POD.ReturnReceiptNoteQty.Value),

                                    ReturnQty = x.rn.ReturnQty// x.RTD == null ? 0 : x.RTD.ReturnQty
                                }).ToList();

                                ListPurchaseOrder.Add(new PurchaseOrderModels
                                {
                                    Id = item.Id,
                                    PONumber = item.PONumber,
                                    ListItem = listItemDetail
                                });
                            }

                            model.ListReturnNote.Add(new ReturnNoteModels
                            {
                                Id = itemRT.Id,
                                ReturnNoteNo = itemRT.RTNumber,
                                CreatedBy = itemRT.ReturnBy,
                                CreatedDate = itemRT.CreatedDate,
                                ListPurchaseOrder = ListPurchaseOrder
                            });
                            model.ListReturnNote = model.ListReturnNote.OrderByDescending(x => x.CreatedDate).ToList();
                        }
                    }
                }
                return model;
            }
        }

        public ReceiptNoteModels GetViewReceiptNoteReturnNote(ReturnNoteReceiptView input)
        {
            using (var cxt = new NuWebContext())
            {
                var receiptReturnModel = (from rn in cxt.I_ReceiptNote
                                          from s in cxt.I_Supplier
                                          where rn.Id.Equals(input.ReceiptId) && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                                          select new ReceiptNoteModels()
                                          {
                                              Id = rn.Id,
                                              ReceiptNo = rn.ReceiptNo,
                                              ReceiptDate = rn.ReceiptDate,
                                              SupplierName = s.Name,
                                              ReceiptBy = rn.ReceiptBy,
                                              StoreId = rn.StoreId,
                                          }).FirstOrDefault();

                var lstReturns = (from r in cxt.I_Return_Note
                                  where input.ListReturnNoteId.Contains(r.Id)
                                  select new
                                  {
                                      Id = r.Id,
                                      Code = r.ReturnNoteNo,
                                      ReturnBy = r.CreatedBy,
                                      CreatedDate = r.CreatedDate
                                  }).ToList();


                if (lstReturns != null)
                {
                    var lstReturnDetailts = (from rd in cxt.I_Return_Note_Detail
                                             from rn in cxt.I_ReceiptNoteDetail.Where(ww => ww.Id == rd.ReceiptNoteDetailId).DefaultIfEmpty()
                                             from pod in cxt.I_Purchase_Order_Detail.Where(ww => ww.Id == rn.PurchaseOrderDetailId).DefaultIfEmpty()
                                             from po in cxt.I_Purchase_Order.Where(ww => ww.Id == pod.PurchaseOrderId).DefaultIfEmpty()
                                             from i in cxt.I_Ingredient.Where(ww => ww.Id == pod.IngredientId).DefaultIfEmpty()
                                             from uom in cxt.I_UnitOfMeasure.Where(ww => ww.Id == i.ReceivingUOMId).DefaultIfEmpty()
                                             where input.ListReturnNoteId.Contains(rd.ReturnNoteId)
                                             select new { rd, rn, pod, po, i, uom }).ToList();

                    var lstPO = lstReturnDetailts.Select(ss => new
                    {
                        POId = ss.po.Id,
                        POCode = ss.po.Code,
                        ReturnNoteId = ss.rd.ReturnNoteId,

                    }).Distinct().ToList();

                    List<PurchaseOrderModels> ListPurchaseOrder = new List<PurchaseOrderModels>();
                    List<POIngredient> _lstPOIngredient = new List<POIngredient>();
                    POIngredient pOIngredient = new POIngredient();
                    foreach (var itemRT in lstReturns)
                    {
                        ListPurchaseOrder = new List<PurchaseOrderModels>();
                        _lstPOIngredient = new List<POIngredient>();
                        var lstPOInReturn = lstPO.Where(ww => ww.ReturnNoteId == itemRT.Id).ToList();

                        foreach (var item in lstPOInReturn)
                        {
                            pOIngredient = new POIngredient();
                            var listItemDetail = lstReturnDetailts.Where(x => x.po.Id.Equals(item.POId)
                             && x.rd.ReturnNoteId == itemRT.Id).Select(x => new POIngredient()
                             {
                                 Id = x.pod.Id,

                                 IngredientId = x.pod.IngredientId,
                                 BaseUOM = x.uom.Name,
                                 IngredientCode = x.i.Code,
                                 IngredientName = x.i.Name,

                                 Qty = x.pod.Qty,
                                 UnitPrice = x.pod.UnitPrice,
                                 Amount = x.pod.Amount,

                                 ReceivingQty = x.rn.ReceivingQty,
                                 RemainingQty = x.rn.RemainingQty,
                                 ReturnQty = x.rd.ReturnQty// x.RTD == null ? 0 : x.RTD.ReturnQty
                             }).ToList();

                            ListPurchaseOrder.Add(new PurchaseOrderModels
                            {
                                Id = item.POId,
                                PONumber = item.POCode,
                                ListItem = listItemDetail
                            });
                        }
                        receiptReturnModel.ListReturnNote.Add(new ReturnNoteModels
                        {
                            Id = itemRT.Id,
                            ReturnNoteNo = itemRT.Code,
                            CreatedBy = itemRT.ReturnBy,
                            CreatedDate = itemRT.CreatedDate,
                            ListPurchaseOrder = ListPurchaseOrder
                        });
                        receiptReturnModel.ListReturnNote = receiptReturnModel.ListReturnNote.OrderByDescending(x => x.CreatedDate).ToList();
                    }
                }
                return receiptReturnModel;
            }
        }

        public ReceiptNoteModels GetReceiptNoteReturnNote(string id, List<string> listCompanyId)
        {
            using (var cxt = new NuWebContext())
            {
                var model = (from rn in cxt.I_ReceiptNote
                             from s in cxt.I_Supplier
                             where rn.Id.Equals(id) && rn.SupplierId.Equals(s.Id)
                                    && listCompanyId.Contains(s.CompanyId) && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                             select new ReceiptNoteModels()
                             {
                                 Id = rn.Id,
                                 ReceiptNo = rn.ReceiptNo,
                                 SupplierName = s.Name,
                                 ReceiptDate = rn.ReceiptDate,
                                 ReceiptBy = rn.ReceiptBy,
                                 StoreId = rn.StoreId,
                             }).FirstOrDefault();

                //let totalReturnQty = cxt.I_Return_Note_Detail.Where(z => z.ReceiptNoteDetailId.Equals(RND.Id)).Select(z => z.ReturnQty).DefaultIfEmpty(0).Sum()

                /*From Table [ReceiptNoteDetail]*/
                var dataRN = (from RND in cxt.I_ReceiptNoteDetail
                              from POD in cxt.I_Purchase_Order_Detail
                              from PO in cxt.I_Purchase_Order
                              from I in cxt.I_Ingredient
                              from UOM in cxt.I_UnitOfMeasure

                              let totalReturnQty = cxt.I_Return_Note_Detail.Where(z => z.ReceiptNoteDetailId.Equals(RND.Id)).Select(z => z.ReturnQty).DefaultIfEmpty(0).Sum()
                              where RND.ReceiptNoteId.Equals(model.Id)
                                     && (RND.Status != (int)Commons.EStatus.Deleted && RND.Status != null)
                                     && POD.Id.Equals(RND.PurchaseOrderDetailId)
                                     && PO.Id.Equals(POD.PurchaseOrderId)
                                     && POD.IngredientId.Equals(I.Id)
                                       //&& UOM.IsActive && I.ReceivingUOMId.Equals(UOM.Id)
                                       && I.ReceivingUOMId.Equals(UOM.Id)
                              select new
                              {
                                  POId = PO.Id,
                                  PONumber = PO.Code,

                                  ReceiptNoteDetailId = RND.Id, /*For Column ReceiptNoteDetailId of table Return Note Detail	*/

                                  /*For Column ReceivedQty of table Return Note Detail	*/

                                  ReceivingQty = totalReturnQty == 0 ? RND.ReceivingQty : (RND.ReceivingQty - totalReturnQty),
                                  RemainingQty = totalReturnQty == 0 ? RND.ReceivingQty : (RND.ReceivingQty - totalReturnQty),

                                  PurchaseOrderDetailId = POD.Id,

                                  ReceiptNoteQty = POD.ReceiptNoteQty,
                                  ReturnReceiptNoteQty = POD.ReturnReceiptNoteQty,

                                  IngredientId = I.Id,
                                  IngredientCode = I.Code,
                                  IngredientName = I.Name,
                                  BaseUOM = UOM.Name,
                                  OrderQty = POD.Qty,
                                  BaseQty = I.ReceivingQty == 0 ? 1 : I.ReceivingQty

                              }).ToList();

                var lstPO = dataRN.GroupBy(x => new { x.POId, x.PONumber })
                                    .Select(g => new
                                    {
                                        g.Key.PONumber,
                                        g.Key.POId,
                                    }).ToList();

                if (lstPO != null)
                {
                    foreach (var item in lstPO)
                    {
                        var ListItemDetail = dataRN.Where(x => x.POId.Equals(item.POId)).Select(x => new POIngredient()
                        {
                            Id = x.PurchaseOrderDetailId,
                            IngredientId = x.IngredientId,
                            BaseUOM = x.BaseUOM,
                            IngredientCode = x.IngredientCode,
                            IngredientName = x.IngredientName,
                            Qty = x.OrderQty,

                            ReceiptNoteQty = x.ReceiptNoteQty.Value,
                            ReceivedQty = x.ReceivingQty,
                            ReceivingQty = x.ReceivingQty,
                            RemainingQty = x.RemainingQty,

                            ReturnReceiptNoteQty = x.ReturnReceiptNoteQty.Value,

                            ReceiptNoteDetailId = x.ReceiptNoteDetailId,
                            BaseQty = x.BaseQty,

                            IsVisible = (x.RemainingQty <= 0 ? true : false)
                        }).ToList();

                        model.ListPurchaseOrder.Add(new PurchaseOrderModels
                        {
                            Id = item.POId,
                            PONumber = item.PONumber,
                            ListItem = ListItemDetail
                        });
                    }
                }

                return model;
            }
        }
        #endregion

        public StatusResponse Export(ref IXLWorksheet wsRN, ref IXLWorksheet wsRND, ReceiptNoteModels modelFilter,
            List<SelectListItem> listStore, List<string> listCompanyId)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                using (NuWebContext cxt = new NuWebContext())
                {
                    var lstDataRN = (from rn in cxt.I_ReceiptNote
                                     from s in cxt.I_Supplier
                                     where modelFilter.ListStores.Contains(rn.StoreId) && rn.SupplierId.Equals(s.Id)
                                               && rn.ReceiptDate >= modelFilter.FromDate && rn.ReceiptDate <= modelFilter.ToDate
                                               && listCompanyId.Contains(s.CompanyId) && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                                     orderby rn.ReceiptDate descending
                                     orderby rn.ReceiptNo descending

                                     select new ReceiptNoteModels()
                                     {
                                         Id = rn.Id,
                                         ReceiptNo = rn.ReceiptNo,
                                         ReceiptDate = rn.ReceiptDate,
                                         StoreId = rn.StoreId,
                                         SupplierName = s.Name,
                                         ReceiptBy = rn.ReceiptBy,
                                     }).ToList();
                    //================= RN Not Supplier
                    List<ReceiptNoteModels> lstDataNotSupplier = new List<ReceiptNoteModels>();
                    lstDataNotSupplier = (from rn in cxt.I_ReceiptNote
                                          where modelFilter.ListStores.Contains(rn.StoreId)
                                                  && rn.ReceiptDate >= modelFilter.FromDate && rn.ReceiptDate <= modelFilter.ToDate
                                                  && (rn.SupplierId.Equals(Guid.Empty.ToString()) || string.IsNullOrEmpty(rn.SupplierId))
                                          orderby rn.ReceiptDate descending
                                          orderby rn.ReceiptNo descending
                                          select new ReceiptNoteModels()
                                          {
                                              Id = rn.Id,
                                              ReceiptNo = rn.ReceiptNo,
                                              ReceiptDate = rn.ReceiptDate,
                                              StoreId = rn.StoreId,
                                              SupplierName = "",
                                              ReceiptBy = rn.ReceiptBy,
                                          }).ToList();
                    lstDataRN.AddRange(lstDataNotSupplier);
                    //RN Not Supplier

                    int row = 1;
                    string[] listHeader = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Received Note No"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Received Date"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Received by")
                    };

                    int cols = listHeader.Length;
                    // Report Name
                    wsRN.Cell(row, 1).Value = string.Format("{0}", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GOODS RECEIPT DOCUMENT"));
                    wsRN.Range(row, 1, row, cols).Merge();
                    wsRN.Range(row, 1, row, cols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                    wsRN.Row(row).Style.Font.FontSize = 16;
                    wsRN.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsRN.Row(row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    wsRN.Row(row).Height = 40;
                    // Date 
                    row++;
                    wsRN.Range(row, 1, row, cols).Merge();
                    wsRN.Cell(row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date: From") + " " + modelFilter.FromDate.ToString("dd/MM/yyyy")
                                                + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " " + modelFilter.ToDate.ToString("dd/MM/yyyy");
                    wsRN.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    //Header
                    row++;
                    for (int i = 1; i <= listHeader.Length; i++)
                        wsRN.Cell(row, i).Value = listHeader[i - 1];

                    //Item
                    row++;
                    int countHeader = 1;
                    int countDetail = 1;

                    List<ExportReceiptNoteDetail> lstRND = new List<ExportReceiptNoteDetail>();
                    foreach (var item in lstDataRN)
                    {
                        var store = listStore.Where(x => x.Value.Equals(item.StoreId)).FirstOrDefault();

                        //check RN 
                        var lstRNItem = (from RND in cxt.I_ReceiptNoteDetail
                                         from I in cxt.I_Ingredient
                                         from UOM in cxt.I_UnitOfMeasure
                                         where RND.IngredientId.Equals(I.Id) && I.Status != (int)Commons.EStatus.Deleted
                                                && RND.UOMId.Equals(UOM.Id) && UOM.Status != (int)Commons.EStatus.Deleted
                                                && RND.ReceiptNoteId.Equals(item.Id)
                                                && (RND.PurchaseOrderDetailId.Equals(Guid.Empty.ToString()) || string.IsNullOrEmpty(RND.PurchaseOrderDetailId))
                                         select new { RND, I, UOM }).ToList();
                        if (lstRNItem != null && lstRNItem.Count > 0)
                            item.IsPurchaseOrder = false;

                        wsRN.Cell("A" + row).Value = countHeader;
                        wsRN.Cell("B" + row).Value = item.ReceiptNo;
                        wsRN.Cell("C" + row).Value = "'" + item.ReceiptDate.ToString("dd/MM/yyyy");
                        wsRN.Cell("D" + row).Value = store == null ? "" : store.Text;
                        wsRN.Cell("E" + row).Value = item.IsPurchaseOrder ? item.SupplierName : "";
                        wsRN.Cell("F" + row).Value = item.ReceiptBy;
                        ////==============
                        //var lstPO = (from RNO in cxt.I_Receipt_Purchase_Order
                        //             from PO in cxt.I_Purchase_Order
                        //             where RNO.PurchaseOrderId.Equals(PO.Id) && RNO.ReceiptNoteId.Equals(item.Id)
                        //             select new { Id = PO.Id, PONumber = PO.Code }).ToList();
                        //if (lstPO != null)
                        //{
                        //    foreach (var itemPO in lstPO)
                        //    {
                        //        var ListItemDetail = _PODetailFactory.GetData(itemPO.Id);
                        //        foreach (var itemDetail in ListItemDetail)
                        //        {
                        //            ExportReceiptNoteDetail ExportRND = new ExportReceiptNoteDetail()
                        //            {
                        //                Index = countDetail,
                        //                ReceiptNoteNo = item.ReceiptNo,

                        //                PONumber = itemPO.PONumber,
                        //                ItemCode = itemDetail.IngredientCode,
                        //                ItemName = itemDetail.IngredientName,
                        //                UOM = itemDetail.BaseUOM,
                        //                OrderQuantity = itemDetail.Qty,
                        //                ReceivingQuantity = itemDetail.ReceiptNoteQty,
                        //                RemainingQuantity = itemDetail.RemainingQty
                        //            };
                        //            countDetail++;
                        //            lstRND.Add(ExportRND);
                        //        }
                        //    }
                        //}

                        if (item.IsPurchaseOrder)
                        {
                            var lstData = (from RND in cxt.I_ReceiptNoteDetail
                                           from POD in cxt.I_Purchase_Order_Detail
                                           from I in cxt.I_Ingredient
                                           from UOM in cxt.I_UnitOfMeasure
                                           from PO in cxt.I_Purchase_Order
                                           let _ReturnQty = cxt.I_Return_Note_Detail.Where(z => RND.Id.Equals(z.ReceiptNoteDetailId)).Select(z => z.ReturnQty).DefaultIfEmpty(0).Sum()

                                           where RND.ReceiptNoteId.Equals(item.Id)
                                                  && RND.PurchaseOrderDetailId.Equals(POD.Id) && POD.Status != (int)Commons.EStatus.Deleted
                                                  && POD.PurchaseOrderId.Equals(PO.Id)
                                                  && POD.IngredientId.Equals(I.Id) && I.Status != (int)Commons.EStatus.Deleted
                                                  && I.BaseUOMId.Equals(UOM.Id) && UOM.Status != (int)Commons.EStatus.Deleted
                                           select new { RND, POD, I, UOM, PO, _ReturnQty }).ToList();
                            if (lstData != null)
                            {
                                var lstPO = lstData.GroupBy(x => new { x.PO.Id, x.PO.Code }).
                                                    Select(x => new
                                                    {
                                                        Id = x.Key.Id,
                                                        PONumber = x.Key.Code,
                                                    }).ToList();
                                //======Get List PO
                                foreach (var itemPO in lstPO)
                                {
                                    var listItemDetail = lstData.Where(x => x.PO.Id.Equals(itemPO.Id)).Select(x => new POIngredient()
                                    {
                                        Id = x.POD.Id,

                                        IngredientId = x.POD.IngredientId,
                                        BaseUOM = x.UOM.Name,
                                        IngredientCode = x.I.Code,
                                        IngredientName = x.I.Name,

                                        Qty = x.POD.Qty,
                                        UnitPrice = x.POD.UnitPrice,
                                        Amount = x.POD.Amount,

                                        ReceivingQty = x.RND.ReceivingQty,
                                        //RemainingQty = x.RND.RemainingQty,
                                        RemainingQty = (x.POD.Qty - x.POD.ReceiptNoteQty.Value + x.POD.ReturnReceiptNoteQty.Value),

                                        ReturnQty = x._ReturnQty
                                    }).ToList();

                                    foreach (var itemDetail in listItemDetail)
                                    {
                                        ExportReceiptNoteDetail ExportRND = new ExportReceiptNoteDetail()
                                        {
                                            Index = countDetail,
                                            ReceiptNoteNo = item.ReceiptNo,

                                            PONumber = itemPO.PONumber,

                                            ItemCode = itemDetail.IngredientCode,
                                            ItemName = itemDetail.IngredientName,
                                            UOM = itemDetail.BaseUOM,

                                            OrderQuantity = itemDetail.Qty,
                                            ReceivingQuantity = itemDetail.ReceivingQty,
                                            RemainingQuantity = itemDetail.RemainingQty,

                                            ReturnQty = itemDetail.ReturnQty
                                        };
                                        countDetail++;
                                        lstRND.Add(ExportRND);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Choose Item
                            foreach (var itemIngredient in lstRNItem)
                            {
                                ExportReceiptNoteDetail ExportRND = new ExportReceiptNoteDetail()
                                {
                                    Index = countDetail,
                                    ReceiptNoteNo = item.ReceiptNo,
                                    PONumber = "",
                                    ItemCode = itemIngredient.I.Code,
                                    ItemName = itemIngredient.I.Name,
                                    UOM = itemIngredient.UOM.Name,
                                    ReceivingQuantity = itemIngredient.RND.ReceivingQty,

                                    OrderQuantity = 0,
                                    RemainingQuantity = 0,
                                    ReturnQty = 0
                                };
                                countDetail++;
                                lstRND.Add(ExportRND);
                            }
                        }
                        row++;
                        countHeader++;
                    }
                    //FormatExcelExport(wsRN, row, cols);
                    wsRN.Range(3, 1, 3, cols).Style.Font.SetBold(true);
                    wsRN.Range(3, 1, row, cols).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsRN.Range(3, 1, row, cols).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    wsRN.Range(1, 1, row - 1, cols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    wsRN.Range(1, 1, row - 1, cols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    wsRN.Columns(1, row).AdjustToContents();
                    //========= ReceiptNote Detail
                    row = 1;
                    string[] listDetailHeader = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Note No"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PO Number"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Code") ,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UOM"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Order Quantity"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receiving Quantity"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Remaining Quantity"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Return Quantity")
                    };
                    for (int i = 1; i <= listDetailHeader.Length; i++)
                        wsRND.Cell(row, i).Value = listDetailHeader[i - 1];
                    cols = listDetailHeader.Length;
                    row++;
                    foreach (var item in lstRND)
                    {
                        wsRND.Cell("A" + row).Value = item.Index;
                        wsRND.Cell("B" + row).Value = item.ReceiptNoteNo;
                        wsRND.Cell("C" + row).Value = item.PONumber;
                        wsRND.Cell("D" + row).Value = item.ItemCode;
                        wsRND.Cell("E" + row).Value = item.ItemName;
                        wsRND.Cell("F" + row).Value = item.UOM;
                        wsRND.Cell("G" + row).Value = item.OrderQuantity;
                        wsRND.Cell("H" + row).Value = item.ReceivingQuantity;
                        wsRND.Cell("I" + row).Value = item.RemainingQuantity;
                        wsRND.Cell("J" + row).Value = item.ReturnQty;

                        row++;
                    }
                    FormatExcelExport(wsRND, row, cols);
                    //========
                    Response.Status = true;
                }
            }
            catch (Exception e)
            {
                Response.Status = false;
                Response.MsgError = e.Message;
            }
            finally
            {

            }
            return Response;
        }
    }
}
