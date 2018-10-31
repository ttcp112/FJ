using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Data;
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
    public class PurchaseOrderFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private InventoryFactory _inventoryFactory = null;
        public PurchaseOrderFactory()
        {
            _baseFactory = new BaseFactory();
            _inventoryFactory = new InventoryFactory();
        }

        public bool Insert(PurchaseOrderModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        I_Purchase_Order item = new I_Purchase_Order();
                        item.Id = Guid.NewGuid().ToString();

                        item.StoreId = model.StoreID;
                        item.Code = CommonHelper.GetGenNo(Commons.ETableZipCode.PurchaseOrder, model.StoreID);
                        item.SupplierId = model.SupplierId;
                        item.PODate = model.PODate;
                        item.DeliveryDate = model.DeliveryDate;
                        item.TaxType = model.TaxType;
                        item.TaxPercen = model.TaxValue;
                        item.SubTotal = model.SubTotal;
                        item.TaxAmount = model.TaxAmount;
                        item.Additional = model.Additional;
                        item.AdditionalReason = model.AdditionalReason;
                        item.Total = model.Total;
                        item.Status = model.POStatus;
                        item.Note = model.Note;

                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.ModifierBy = model.ModifierBy;
                        item.ModifierDate = model.ModifierDate;
                        item.IsActived = model.IsActived;
                        //=================
                        List<I_Purchase_Order_Detail> ListInsert = new List<I_Purchase_Order_Detail>();
                        //List<InventoryModels> lstInventory = new List<InventoryModels>();
                        foreach (var PODetail in model.ListItem)
                        {
                            ListInsert.Add(new I_Purchase_Order_Detail
                            {
                                Id = Guid.NewGuid().ToString(),
                                PurchaseOrderId = item.Id,
                                IngredientId = PODetail.IngredientId,
                                Qty = PODetail.Qty,
                                UnitPrice = PODetail.UnitPrice,
                                Amount = PODetail.Qty * PODetail.UnitPrice,
                                ReceiptNoteQty = 0,
                                ReturnReceiptNoteQty = 0,
                                BaseQty = PODetail.Qty * (PODetail.IngReceivingQty == 0 ? 1 : PODetail.IngReceivingQty),
                                Status = (int)Commons.EStatus.Actived,

                                //update TaxtIntegrate
                                TaxAmount = model.StoreIntegrate ? PODetail.TaxAmount : 0,
                                TaxId = model.StoreIntegrate ? PODetail.TaxId : null,
                                TaxPercen = model.StoreIntegrate ? PODetail.TaxPercent : 0
                            });
                            //lstInventory.Add(new InventoryModels()
                            //{
                            //    StoreId = model.StoreID,
                            //    IngredientId = PODetail.IngredientId,
                            //    Price = PODetail.UnitPrice,
                            //    Quantity = PODetail.Qty * (PODetail.IngReceivingQty == 0 ? 1 : PODetail.IngReceivingQty)
                            //});
                        }

                        cxt.I_Purchase_Order.Add(item);
                        cxt.I_Purchase_Order_Detail.AddRange(ListInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        ///* GENERAL INVOICE XERO */
                        //var infoXero = Commons.GetIntegrateInfo(model.StoreID);
                        //if (infoXero != null)
                        //{
                        //    /* find supplier name by supplier id */
                        //    string SupplierName = "";
                        //    string Telephone = "";
                        //    var objSupplier = cxt.I_Supplier.Where(o => o.Id.Equals(model.SupplierId)).FirstOrDefault();
                        //    if (objSupplier != null)
                        //    {
                        //        SupplierName = objSupplier.Name;
                        //        Telephone = objSupplier.Phone1 + " - " + objSupplier.Phone2;
                        //    }
                        //    XeroFactory _facXero = new XeroFactory();
                        //    /* get list item */
                        //    var listItem = new List<InvoiceLineItemModels>();
                        //    if (model.ListItem != null && model.ListItem.Any())
                        //    {
                        //        foreach (var _item in model.ListItem)
                        //        {
                        //            listItem.Add(new InvoiceLineItemModels
                        //            {
                        //                Description = _item.IngredientName,
                        //                ItemCode = _item.IngredientCode,
                        //                Quantity = Convert.ToDecimal(_item.Qty),
                        //                UnitAmount = Convert.ToDecimal(_item.UnitPrice),
                        //                LineAmount = Convert.ToDecimal(_item.Qty * _item.UnitPrice),// = Amount 
                        //                AccountCode = Commons.AcoountCode_200,
                        //                TaxType = model.TaxType == (int)Commons.ETax.AddOn ? "OUTPUT" : "INPUT",
                        //            });
                        //        }
                        //    }
                        //    var modelXero = new GeneratePOModels
                        //    {
                        //        AppRegistrationId = infoXero.ThirdPartyID,
                        //        StoreId = infoXero.IPAddress,
                        //        Reference = item.Code,
                        //        ClosingDatetime = DateTime.Now,
                        //        PurchaseOrderStatus = (byte)Commons.EPurchaseOrderStatuses.Authorised,
                        //        CurrencyCode = Commons._XeroCurrencyCode,
                        //        LineAmountTypes = (byte)Commons.ELineAmountType.Inclusive,
                        //        AttentionTo = model.StoreName,
                        //        Telephone = Telephone,
                        //        DeliveryInstructions = model.Note,
                        //        Contact = new InvoiceContactModels
                        //        {
                        //            ContactID = objSupplier == null ? "" : objSupplier.XeroId,
                        //        },
                        //        Items = listItem
                        //    };
                        //    var msgXero = string.Empty;
                        //    var data = new GIResponseModels();
                        //    var resultXero = _facXero.GeneratePO(infoXero.ApiURL, modelXero, ref msgXero);
                        //}

                        //Update inventory
                        //var isUpdateInentoryPO = _inventoryFactory.UpdateInventoryForPO(lstInventory);
                        //_logger.Info(string.Format("UpdateInventoryForPO: [{0}]", isUpdateInentoryPO));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            return result;
        }

        public bool Update(PurchaseOrderModels model, List<string> listIdDelete, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var itemUpdate = (from tb in cxt.I_Purchase_Order
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();

                        itemUpdate.StoreId = model.StoreID;
                        itemUpdate.SupplierId = model.SupplierId;

                        itemUpdate.PODate = model.PODate;
                        itemUpdate.DeliveryDate = model.DeliveryDate;

                        itemUpdate.TaxType = model.TaxType;
                        itemUpdate.TaxPercen = model.TaxValue;

                        itemUpdate.SubTotal = model.SubTotal;
                        itemUpdate.TaxAmount = model.TaxAmount;

                        itemUpdate.Additional = model.Additional;
                        itemUpdate.AdditionalReason = model.AdditionalReason;

                        itemUpdate.Total = model.Total;
                        itemUpdate.Note = model.Note;

                        itemUpdate.ModifierBy = model.ModifierBy;
                        itemUpdate.ModifierDate = model.ModifierDate;

                        //=================
                        //For inventory
                        //List<InventoryModels> lstInventory = new List<InventoryModels>();
                        //InventoryModels inventory = null;

                        if (model.ListItem != null && model.ListItem.Count > 0)
                        {
                            /*Delete*/
                            List<I_Purchase_Order_Detail> listDelete = (from tb in cxt.I_Purchase_Order_Detail
                                                                        where listIdDelete.Contains(tb.Id)
                                                                        select tb).ToList();
                            if (listDelete != null && listDelete.Count > 0)
                            {
                                foreach (var item in listDelete)
                                {
                                    item.Status = (int)Commons.EStatus.Deleted;
                                    //inventory
                                    //inventory = new InventoryModels();
                                    //inventory.StoreId = model.StoreID;
                                    //inventory.IngredientId = item.IngredientId;
                                    //inventory.Price = item.UnitPrice;
                                    //inventory.Quantity = -(item.BaseQty.HasValue ? item.BaseQty.Value : 0);

                                    //lstInventory.Add(inventory);
                                }
                            }

                            //Update and Insert
                            List<I_Purchase_Order_Detail> listInsert = new List<I_Purchase_Order_Detail>();
                            foreach (var PODetail in model.ListItem)
                            {
                                //inventory = new InventoryModels();
                                //inventory.StoreId = model.StoreID;
                                //inventory.IngredientId = PODetail.IngredientId;
                                //inventory.Price = PODetail.UnitPrice;

                                var itemUpdatePOD = (from tb in cxt.I_Purchase_Order_Detail
                                                     where tb.Id.Equals(PODetail.Id)
                                                     select tb).FirstOrDefault();
                                if (itemUpdatePOD != null) //Update
                                {
                                    //inventory.Quantity = ((PODetail.Qty - itemUpdatePOD.Qty) * (PODetail.IngReceivingQty == 0 ? 1 : PODetail.IngReceivingQty));

                                    itemUpdatePOD.Qty = PODetail.Qty;
                                    itemUpdatePOD.UnitPrice = PODetail.UnitPrice;
                                    itemUpdatePOD.Amount = PODetail.Qty * PODetail.UnitPrice;
                                    itemUpdatePOD.BaseQty = PODetail.Qty * (PODetail.IngReceivingQty == 0 ? 1 : PODetail.IngReceivingQty);

                                    //update TaxtIntegrate
                                    itemUpdatePOD.TaxAmount = model.StoreIntegrate ? PODetail.TaxAmount : 0;
                                    itemUpdatePOD.TaxId = model.StoreIntegrate ? PODetail.TaxId : null;
                                    itemUpdatePOD.TaxPercen = model.StoreIntegrate ? PODetail.TaxPercent : 0;
                                }
                                else //Insert
                                {
                                    itemUpdatePOD = new I_Purchase_Order_Detail()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        PurchaseOrderId = itemUpdate.Id,
                                        IngredientId = PODetail.IngredientId,
                                        Qty = PODetail.Qty,
                                        UnitPrice = PODetail.UnitPrice,
                                        Amount = PODetail.Qty * PODetail.UnitPrice,
                                        ReceiptNoteQty = 0,
                                        ReturnReceiptNoteQty = 0,
                                        BaseQty = PODetail.Qty * (PODetail.IngReceivingQty == 0 ? 1 : PODetail.IngReceivingQty),
                                        Status = (int)Commons.EStatus.Actived,

                                        //update TaxtIntegrate
                                        TaxAmount = model.StoreIntegrate ? PODetail.TaxAmount : 0,
                                        TaxId = model.StoreIntegrate ? PODetail.TaxId : null,
                                        TaxPercen = model.StoreIntegrate ? PODetail.TaxPercent : 0
                                    };
                                    listInsert.Add(itemUpdatePOD);
                                    //inventory.Quantity = PODetail.Qty * (PODetail.IngReceivingQty == 0 ? 1 : PODetail.IngReceivingQty);
                                }
                                //lstInventory.Add(inventory);
                            }
                            if (listInsert.Count > 0)
                            {
                                cxt.I_Purchase_Order_Detail.AddRange(listInsert);
                            }
                        }
                        cxt.SaveChanges();
                        transaction.Commit();
                        //Update inventory
                        //_inventoryFactory.UpdateInventoryForPO(lstInventory);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            return result;
        }

        public bool Delete(string Id, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    if (IsCanDelete(Id))
                    {
                        I_Purchase_Order itemDelete = (from tb in cxt.I_Purchase_Order
                                                       where tb.Id == Id
                                                       select tb).FirstOrDefault();
                        if (itemDelete != null)
                        {
                            itemDelete.Status = (int)Commons.EStatus.Deleted;

                            var listPOD = (from pod in cxt.I_Purchase_Order_Detail
                                           where pod.PurchaseOrderId.Equals(itemDelete.Id)
                                           select pod).ToList();
                            listPOD.ForEach(x => x.Status = (int)Commons.EStatus.Deleted);

                            cxt.SaveChanges();
                        }
                    }
                    else
                    {
                        msg = "This Purchar Order has been in used. Can't Delete";
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        private bool IsCanDelete(string id)
        {
            using (var cxt = new NuWebContext())
            {
                //var isExists = cxt.I_Purchase_Order_Detail.Any(x => x.PurchaseOrderId == id && x.Status != (int)Commons.EStatus.Deleted);
                //if (!isExists)
                //{
                var isExists = cxt.I_Receipt_Purchase_Order.Any(x => x.PurchaseOrderId == id && x.IsActived);
                //}
                return !isExists;
            }
        }

        public List<PurchaseOrderModels> LoadPOForRN(string StoreId, string SupplierId, List<string> ListPONo, string PONo)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Purchase_Order
                                     from s in cxt.I_Supplier
                                     where tb.SupplierId.Equals(s.Id)
                                        && tb.StoreId.Equals(StoreId) && tb.SupplierId.Equals(SupplierId)
                                        && (tb.Status == (int)Commons.EPOStatus.Approved || tb.Status == (int)Commons.EPOStatus.InProgress)
                                        && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                                     orderby tb.DeliveryDate descending
                                     orderby tb.Code descending
                                     select new PurchaseOrderModels()
                                     {
                                         Id = tb.Id,
                                         PONumber = tb.Code,
                                         SupplierName = s.Name,
                                         DeliveryDate = tb.DeliveryDate,
                                     }).ToList();
                    if (!string.IsNullOrEmpty(PONo))
                    {
                        lstResult = lstResult.Where(x => x.PONumber.Contains(PONo)).ToList();
                    }
                    //if (ListPONo.Count != 0)
                    //{
                    //    lstResult = lstResult.Where(x => ListPONo.Contains(x.PONumber)).ToList();
                    //}
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<PurchaseOrderModels> GetData(string StoreId, List<string> listCompanyId, DateTime? dFrom, DateTime? dTo)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {

                    var query = (from tb in cxt.I_Purchase_Order
                                 from s in cxt.I_Supplier
                                 where tb.SupplierId.Equals(s.Id) && tb.Status != (int)Commons.EStatus.Deleted
                                        && listCompanyId.Contains(s.CompanyId) && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                                 select new { tb, s });

                    DateTime currentDate = DateTime.Now;
                    if (StoreId != null)
                    {
                        query = query.Where(x => x.tb.StoreId.Equals(StoreId));
                    }
                    if (dFrom.HasValue)
                    {
                        dFrom = new DateTime(dFrom.Value.Year, dFrom.Value.Month, dFrom.Value.Day, 0, 0, 0);
                        query = query.Where(x => x.tb.PODate >= dFrom.Value);
                    }
                    if (dTo.HasValue)
                    {
                        dTo = new DateTime(dTo.Value.Year, dTo.Value.Month, dTo.Value.Day, 23, 59, 59);
                        query = query.Where(x => x.tb.PODate <= dTo.Value);
                    }

                    var lstResult = query.Select(ss => new PurchaseOrderModels()
                    {
                        Id = ss.tb.Id,
                        PONumber = ss.tb.Code,
                        SupplierName = ss.s.Name,
                        DeliveryDate = ss.tb.DeliveryDate,
                        PODate = ss.tb.PODate,
                        Total = ss.tb.Total,
                        POStatus = ss.tb.Status,
                        StoreID = ss.tb.StoreId,
                        ColorAlert = "",
                        Symbol = ""
                    }).ToList();

                    lstResult.ForEach(x =>
                    {
                        if (x.POStatus == (int)Commons.EPOStatus.Open || x.POStatus == (int)Commons.EPOStatus.Approved)
                        {
                            if ((currentDate.Date - x.DeliveryDate.Date).Days >= 1)
                            {
                                x.ColorAlert = "red";
                            }
                            else if (currentDate.Date >= x.DeliveryDate.Date.AddDays(-3) || currentDate.Date <= x.DeliveryDate.Date)
                            {
                                x.ColorAlert = "yellow";
                            }
                        }
                        //==============
                        x.ListReceiptPO = (from RNPO in cxt.I_Receipt_Purchase_Order
                                           from RN in cxt.I_ReceiptNote

                                               //let RecievingQty = cxt.I_ReceiptNoteDetail.Where(z => z.ReceiptNoteId.Equals(RNPO.ReceiptNoteId)).Sum(z => z.ReceivingQty)

                                           let RecievingQty = cxt.I_ReceiptNoteDetail.Where(z
                                                                                    => z.ReceiptNoteId.Equals(RNPO.ReceiptNoteId)).Select(z
                                                                                    => z.ReceivingQty).DefaultIfEmpty(0).Sum()

                                           where x.Id.Equals(RNPO.PurchaseOrderId) && RNPO.ReceiptNoteId.Equals(RN.Id) && RN.Status != (int)Commons.EStatus.Deleted
                                           select new ReceiptPurchaseOrderModels()
                                           {
                                               ReceiptNoteNo = RN.ReceiptNo,
                                               RecievingQty = RecievingQty
                                           }).ToList();
                    });

                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public PurchaseOrderModels GetDetail(string ID, List<string> listCompanyId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Purchase_Order
                                 from s in cxt.I_Supplier
                                 where tb.Id.Equals(ID) && tb.SupplierId.Equals(s.Id)
                                 && listCompanyId.Contains(s.CompanyId) && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                                 select new PurchaseOrderModels()
                                 {
                                     Id = tb.Id,
                                     PONumber = tb.Code,

                                     SupplierId = tb.SupplierId,
                                     Note = tb.Note,

                                     PODate = tb.PODate,
                                     DeliveryDate = tb.DeliveryDate,
                                     TaxType = tb.TaxType,
                                     TaxValue = tb.TaxPercen,
                                     SubTotal = tb.SubTotal,
                                     TaxAmount = tb.TaxAmount,
                                     Additional = tb.Additional,
                                     AdditionalReason = tb.AdditionalReason,
                                     Total = tb.Total,
                                     POStatus = tb.Status,

                                     CreatedBy = tb.CreatedBy,
                                     CreatedDate = tb.CreatedDate,
                                     ModifierBy = tb.ModifierBy,
                                     ModifierDate = tb.ModifierDate,
                                     IsActived = tb.IsActived,
                                     StoreID = tb.StoreId
                                 }).FirstOrDefault();

                    var index = 0;
                    model.ListItem = (from POD in cxt.I_Purchase_Order_Detail
                                      from uom in cxt.I_UnitOfMeasure
                                      from I in cxt.I_Ingredient
                                      where POD.IngredientId.Equals(I.Id) && POD.PurchaseOrderId.Equals(ID)
                                      && (POD.Status != (int)Commons.EStatus.Deleted && POD.Status != null)
                                      && uom.IsActive && I.ReceivingUOMId.Equals(uom.Id)
                                      select new POIngredient()
                                      {
                                          Id = POD.Id, //Update
                                          OffSet = index + 1,

                                          Amount = POD.Amount,
                                          BaseUOM = uom.Name,
                                          Description = I.Description,
                                          IngredientCode = I.Code,
                                          IngredientId = POD.IngredientId,
                                          IngredientName = I.Name,
                                          Qty = POD.Qty,
                                          UnitPrice = POD.UnitPrice,
                                          IngReceivingQty = POD.BaseQty.HasValue ? (POD.BaseQty.Value / POD.Qty) : 1,

                                          //---updated Tax
                                          TaxId = POD.TaxId,
                                          TaxAmount = POD.TaxAmount.HasValue ? POD.TaxAmount.Value : 0,
                                          TaxPercent = POD.TaxPercen.HasValue ? POD.TaxPercen.Value : 0
                                      }).ToList();
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public bool ChangeStatus(List<string> lstId, int status, List<SelectListItem> lstStore)
        {
            using (var cxt = new NuWebContext())
            {
                var lstObj = cxt.I_Purchase_Order.Where(ww => lstId.Contains(ww.Id)).ToList();
                if (lstObj != null && lstObj.Count > 0)
                {
                    lstObj.ForEach(ss => ss.Status = status);
                    cxt.SaveChanges();
                    if (status == (int)Commons.EPOStatus.Approved)
                    {
                        var ListIngXero = cxt.I_Ingredient.Where(s => s.Status != (byte)Commons.EStatus.Deleted).ToList();
                        //----------------------------
                        List<InventoryModels> _lstPODetail = new List<InventoryModels>();
                        foreach (var item in lstObj)
                        {
                            //var storeId = lstObj.Select(ss => ss.StoreId).FirstOrDefault();
                            //update inventory
                            var lstPODetail = cxt.I_Purchase_Order_Detail.Where(ww => ww.PurchaseOrderId == item.Id).Select(ss => new InventoryModels
                            {
                                StoreId = item.StoreId,
                                IngredientId = ss.IngredientId,
                                Price = ss.UnitPrice,
                                Quantity = ss.BaseQty.HasValue ? ss.BaseQty.Value : 0
                            }).ToList();

                            _lstPODetail.AddRange(lstPODetail);

                            /* GENERAL INVOICE XERO */
                            var objPurchaseOrder = cxt.I_Purchase_Order.Where(x => x.Id == item.Id && x.Status != (byte)Commons.EStatus.Deleted).FirstOrDefault();
                            if (objPurchaseOrder != null)
                            {
                                var infoXero = Commons.GetIntegrateInfo(objPurchaseOrder.StoreId);
                                if (infoXero != null)
                                {
                                    /* find supplier name by supplier id */
                                    string SupplierName = "";
                                    string Telephone = "";
                                    var objSupplier = cxt.I_Supplier.Where(o => o.Id.Equals(objPurchaseOrder.SupplierId)).FirstOrDefault();
                                    if (objSupplier != null)
                                    {
                                        SupplierName = objSupplier.Name;
                                        Telephone = objSupplier.Phone1 + " - " + objSupplier.Phone2;
                                    }
                                    XeroFactory _facXero = new XeroFactory();
                                    /* GET LIST ITEM */
                                    var listItem = new List<InvoiceLineItemModels>();
                                    var ListPODetail = cxt.I_Purchase_Order_Detail.Where(ww => ww.PurchaseOrderId == item.Id).ToList();
                                    if (ListPODetail != null && ListPODetail.Any())
                                    {
                                        string _AccountCode = Commons.AccountCode_Inventory;
                                        List<string> lstStoreId = lstStore.Select(z => z.Value).ToList();
                                        var StockOnHand = (from _store in cxt.G_SettingOnStore
                                                           from _setting in cxt.G_GeneralSetting
                                                           where _store.SettingId == _setting.Id && lstStoreId.Contains(_store.StoreId)
                                                                 && _store.Status && _setting.Status
                                                                 && _setting.Code.Equals((byte)Commons.EGeneralSetting.StockOnHand)
                                                           select _store).FirstOrDefault();
                                        if (StockOnHand != null)
                                            _AccountCode = StockOnHand.Value;

                                        //GET TAX RATE
                                        string TaxType = "TAX002";
                                        NuWebNCloud.Shared.Factory.Settings.TaxFactory _taxFactory = new NuWebNCloud.Shared.Factory.Settings.TaxFactory();
                                        var lstTaxes = _taxFactory.GetListTaxV2(item.StoreId);
                                        var tax = lstTaxes.Where(w => w.IsActive && !string.IsNullOrEmpty(w.Rate)).FirstOrDefault();
                                        if (tax != null)
                                            TaxType = tax.Rate;

                                        foreach (var _item in ListPODetail)
                                        {
                                            var objIng = ListIngXero.Where(z => z.Id.Equals(_item.IngredientId)).FirstOrDefault();
                                            listItem.Add(new InvoiceLineItemModels
                                            {
                                                Description = objIng != null ? objIng.Name : "",
                                                ItemCode = objIng != null ? objIng.Code : "",
                                                Quantity = Convert.ToDecimal(_item.Qty),
                                                UnitAmount = Convert.ToDecimal(_item.UnitPrice),
                                                LineAmount = Convert.ToDecimal(_item.Qty * _item.UnitPrice),// = Amount 
                                                AccountCode = _AccountCode,//Commons.AccountCode_Inventory,
                                                TaxType = TaxType   //objPurchaseOrder.TaxType == (int)Commons.ETax.AddOn ? "OUTPUT" : "INPUT",
                                            });
                                        }
                                    }
                                    //---------
                                    string StoreName = string.Empty;
                                    var objStore = lstStore.Where(ww => ww.Value == objPurchaseOrder.StoreId).FirstOrDefault();
                                    if (objStore != null)
                                        StoreName = objStore.Text;
                                    //---------
                                    var modelXero = new GeneratePOModels
                                    {
                                        AppRegistrationId = infoXero.ThirdPartyID,
                                        StoreId = infoXero.IPAddress,

                                        Reference = item.Code,
                                        ClosingDatetime = DateTime.Now,
                                        PurchaseOrderStatus = (byte)Commons.EPurchaseOrderStatuses.Authorised,
                                        CurrencyCode = Commons._XeroCurrencyCode,
                                        LineAmountTypes = (byte)Commons.ELineAmountType.Inclusive,

                                        AttentionTo = StoreName,
                                        Telephone = Telephone,
                                        DeliveryInstructions = objPurchaseOrder.Note,
                                        Contact = new InvoiceContactPOModels
                                        {
                                            ContactID = objSupplier == null ? "" : objSupplier.XeroId,
                                        },
                                        Items = listItem
                                    };
                                    var msgXero = string.Empty;
                                    var data = new GIResponseModels();
                                    var resultXero = _facXero.GeneratePO(infoXero.ApiURL, modelXero, ref msgXero);
                                }
                            }
                        }
                        //Update inventory
                        var isUpdateInentoryPO = _inventoryFactory.UpdateInventoryForPO(_lstPODetail);
                        NSLog.Logger.Info(string.Format("UpdateInventoryForPO: [{0}] when approved", isUpdateInentoryPO));
                    }
                    else //close
                    {
                        List<InventoryModels> _lstPODetail = new List<InventoryModels>();
                        foreach (var item in lstObj)
                        {
                            var lstPODetail = cxt.I_Purchase_Order_Detail.Where(ww => ww.PurchaseOrderId == item.Id).Select(ss => new InventoryModels
                            {
                                StoreId = item.StoreId,
                                IngredientId = ss.IngredientId,
                                Price = ss.UnitPrice,
                                Quantity = ss.BaseQty.HasValue ? ss.BaseQty.Value : 0,
                                POQty = ss.Qty,
                                ReceiptQty = ss.ReceiptNoteQty.HasValue ? ss.ReceiptNoteQty.Value : 0,
                                ReturnQty = ss.ReturnReceiptNoteQty.HasValue ? ss.ReturnReceiptNoteQty.Value : 0
                            }).ToList();

                            _lstPODetail.AddRange(lstPODetail);
                        }
                        //Update inventory
                        var isUpdateInentoryPO = _inventoryFactory.UpdateInventoryForPOForCloseManual(_lstPODetail);
                        _logger.Info(string.Format("UpdateInventoryForPO: [{0}] when close manual", isUpdateInentoryPO));
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<string> listItemOnData(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<string> Data = (from POD in cxt.I_Purchase_Order_Detail
                                         where POD.PurchaseOrderId.Equals(ID)
                                            && (POD.Status != (int)Commons.EStatus.Deleted && POD.Status != null)
                                         select POD.Id).ToList();
                    return Data;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public StatusResponse Export(ref IXLWorksheet wsPO, ref IXLWorksheet wsPOD, /*ref IXLWorksheet wsPOR,*/ PurchaseOrderModels modelFilter,
            List<SelectListItem> listStore, List<string> listCompanyId)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                using (NuWebContext cxt = new NuWebContext())
                {
                    var listData = (from tb in cxt.I_Purchase_Order
                                    from s in cxt.I_Supplier
                                    where tb.SupplierId.Equals(s.Id) && modelFilter.ListStores.Contains(tb.StoreId)
                                            && tb.PODate >= modelFilter.PODate && tb.PODate <= modelFilter.DeliveryDate
                                            && listCompanyId.Contains(s.CompanyId) && s.IsActived && s.Status != (int)Commons.EStatus.Deleted
                                    select new PurchaseOrderModels()
                                    {
                                        Id = tb.Id,
                                        PONumber = tb.Code,
                                        PODate = tb.PODate,
                                        DeliveryDate = tb.DeliveryDate,

                                        SubTotal = tb.SubTotal,
                                        TaxAmount = tb.TaxAmount,
                                        TaxType = tb.TaxType,
                                        TaxValue = tb.TaxPercen,
                                        Additional = tb.Additional,
                                        AdditionalReason = tb.AdditionalReason,
                                        Total = tb.Total,
                                        SupplierName = s.Name,
                                        Note = tb.Note,
                                        POStatus = tb.Status,
                                        StoreID = tb.StoreId,
                                    }).ToList();

                    int row = 1;
                    string[] listHeader = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PO Number"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PO Date"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Delivery Date"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Subtotal"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Amount"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Additional Amount"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Addition Reason"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Note"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Purchasing Company"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier")
                    };

                    int cols = listHeader.Length;
                    // Report Name
                    wsPO.Cell(row, 1).Value = string.Format("{0}", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PURCHASE ORDER").ToUpper());
                    wsPO.Range(row, 1, row, cols).Merge();
                    wsPO.Range(row, 1, row, cols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                    wsPO.Row(row).Style.Font.FontSize = 16;
                    wsPO.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsPO.Row(row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    wsPO.Row(row).Height = 40;
                    // Date 
                    row++;
                    wsPO.Range(row, 1, row, cols).Merge();
                    wsPO.Cell(row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date: From") + " " + modelFilter.PODate.ToString("dd/MM/yyyy") + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " " + modelFilter.DeliveryDate.ToString("dd/MM/yyyy");
                    wsPO.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    //Header
                    row++;
                    for (int i = 1; i <= listHeader.Length; i++)
                        wsPO.Cell(row, i).Value = listHeader[i - 1];

                    //Item
                    row++;
                    int countHeader = 1;
                    int countDetail = 1;

                    List<ExportPurchaseOrderDetail> lstPOD = new List<ExportPurchaseOrderDetail>();
                    foreach (var item in listData)
                    {
                        var store = listStore.Where(x => x.Value.Equals(item.StoreID)).FirstOrDefault();
                        string POStatus = "";
                        if (item.POStatus == (int)Commons.EPOStatus.Open)
                        {
                            POStatus = Commons.EPOStatus.Open.ToString();
                        }
                        else if (item.POStatus == (int)Commons.EPOStatus.Approved)
                        {
                            POStatus = Commons.EPOStatus.Approved.ToString();
                        }
                        else if (item.POStatus == (int)Commons.EPOStatus.InProgress)
                        {
                            POStatus = Commons.EPOStatus.InProgress.ToString();
                        }
                        else if (item.POStatus == (int)Commons.EPOStatus.Closed)
                        {
                            POStatus = Commons.EPOStatus.Closed.ToString();
                        }
                        wsPO.Cell("A" + row).Value = countHeader;
                        wsPO.Cell("B" + row).Value = item.PONumber;
                        wsPO.Cell("C" + row).Value = "'" + item.PODate.ToString("dd/MM/yyyy");
                        wsPO.Cell("D" + row).Value = "'" + item.DeliveryDate.ToString("dd/MM/yyyy");
                        wsPO.Cell("E" + row).Value = item.SubTotal;
                        wsPO.Cell("F" + row).Value = item.TaxType == (byte)Commons.ETax.AddOn ?
                                            _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETax.AddOn.ToString())
                                            : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETax.Inclusive.ToString());

                        wsPO.Cell("G" + row).Value = item.TaxAmount;
                        wsPO.Cell("H" + row).Value = item.Additional;
                        wsPO.Cell("I" + row).Value = item.AdditionalReason;
                        wsPO.Cell("J" + row).Value = item.Total;
                        wsPO.Cell("K" + row).Value = item.Note;
                        wsPO.Cell("L" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(POStatus);
                        wsPO.Cell("M" + row).Value = store == null ? "" : store.Text;
                        wsPO.Cell("N" + row).Value = item.SupplierName;

                        var ListPODetail = (from POD in cxt.I_Purchase_Order_Detail
                                            from I in cxt.I_Ingredient
                                            from UOM in cxt.I_UnitOfMeasure
                                            where POD.PurchaseOrderId.Equals(item.Id)
                                                    && I.IsActive && I.Id.Equals(POD.IngredientId)
                                                    && (POD.Status != (int)Commons.EStatus.Deleted && POD.Status != null)
                                                    && UOM.IsActive && UOM.Id.Equals(I.BaseUOMId)
                                            select new
                                            {
                                                POD,
                                                I,
                                                UOM
                                            }).ToList();

                        foreach (var itemDetail in ListPODetail)
                        {
                            ExportPurchaseOrderDetail ExportPOD = new ExportPurchaseOrderDetail()
                            {
                                Index = countDetail,
                                PONumber = item.PONumber,
                                ItemCode = itemDetail.I.Code,
                                ItemName = itemDetail.I.Name,
                                Description = itemDetail.I.Description,
                                Quantity = itemDetail.POD.Qty,
                                UOM = itemDetail.UOM.Name,
                                UnitPrice = itemDetail.POD.UnitPrice,
                                ItemTotal = itemDetail.POD.Amount
                            };
                            countDetail++;
                            lstPOD.Add(ExportPOD);
                        }
                        row++;
                        countHeader++;
                    }
                    //FormatExcelExport(wsPO, row, cols);

                    wsPO.Range(3, 1, 3, cols).Style.Font.SetBold(true);
                    wsPO.Range(3, 1, row, cols).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsPO.Range(3, 1, row, cols).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    wsPO.Range(1, 1, row - 1, cols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    wsPO.Range(1, 1, row - 1, cols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    wsPO.Columns(1, row).AdjustToContents();
                    //========= Purchasing Order Detail
                    row = 1;
                    string[] listDetailHeader = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PO Number"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Code") ,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Description"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UOM"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Price"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total")
                    };
                    for (int i = 1; i <= listDetailHeader.Length; i++)
                        wsPOD.Cell(row, i).Value = listDetailHeader[i - 1];
                    cols = listDetailHeader.Length;
                    row++;
                    foreach (var item in lstPOD)
                    {
                        wsPOD.Cell("A" + row).Value = item.Index;
                        wsPOD.Cell("B" + row).Value = item.PONumber;
                        wsPOD.Cell("C" + row).Value = item.ItemCode;
                        wsPOD.Cell("D" + row).Value = item.ItemName;
                        wsPOD.Cell("E" + row).Value = item.Description;
                        wsPOD.Cell("F" + row).Value = item.Quantity;
                        wsPOD.Cell("G" + row).Value = item.UOM;
                        wsPOD.Cell("H" + row).Value = item.UnitPrice;
                        wsPOD.Cell("I" + row).Value = item.ItemTotal;
                        row++;
                    }
                    FormatExcelExport(wsPOD, row, cols);
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
