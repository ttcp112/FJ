using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class ReceiptNoteSelfMadeFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private InventoryFactory _inventoryFactory = null;

        public ReceiptNoteSelfMadeFactory()
        {
            _baseFactory = new BaseFactory();
            _inventoryFactory = new InventoryFactory();
        }

        public bool Insert(ReceiptNoteSelfMadeModels model, RecipeIngredientUsageModels _objIngredientDependent, ref string msg)
        {
            NSLog.Logger.Info("InsertSelfMade", _objIngredientDependent);
            using (var cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        ResultModels resultModels = new ResultModels();
                        List<InventoryModels> lstInventory = new List<InventoryModels>();

                        var item = new I_ReceiptNoteForSeftMade();
                        string ReceiptNoteForSeftMadeId = Guid.NewGuid().ToString();

                        item.Id = ReceiptNoteForSeftMadeId;
                        item.ReceiptNo = CommonHelper.GetGenNo(Commons.ETableZipCode.ReceiptNoteSelfMade, model.StoreId);
                        item.Status = (int)Commons.EReceiptNoteStatus.Closed;
                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.UpdatedBy = model.UpdatedBy;
                        item.UpdatedDate = model.UpdatedDate;
                        item.StoreId = model.StoreId;
                        item.ReceiptBy = model.ReceiptBy;
                        item.ReceiptDate = model.ReceiptDate;
                        cxt.I_ReceiptNoteForSeftMade.Add(item);

                        //======== Insert Receipt Work Order
                        List<string> ListWOId = new List<string>();
                        List<I_ReceiptSelfMade_Work_Order> RWOModels = new List<I_ReceiptSelfMade_Work_Order>();
                        foreach (var RNDetailParent in model.ListWorkOrder)
                        {
                            RWOModels.Add(new I_ReceiptSelfMade_Work_Order
                            {
                                Id = Guid.NewGuid().ToString(),
                                WorkOrderId = RNDetailParent.Id,
                                RNSelfMadeId = ReceiptNoteForSeftMadeId,
                                CreatedBy = model.CreatedBy,
                                CreatedDate = model.CreatedDate,
                                ModifierBy = model.UpdatedBy,
                                ModifierDate = model.UpdatedDate,
                                IsActived = true
                            });
                            ListWOId.Add(RNDetailParent.Id);
                            //Update WO detail
                            var childs = model.ListItem.Where(ww => ww.WOId == RNDetailParent.Id).ToList();
                            foreach (var RNDetailChild in childs)
                            {

                                var itemUpdate = (from tb in cxt.I_Work_Order_Detail
                                                  where tb.Id == RNDetailChild.Id
                                                  select tb).FirstOrDefault();
                                if (itemUpdate != null)
                                {
                                    itemUpdate.ReceiptNoteQty += RNDetailChild.ReceivingQty;
                                    //RNDetailChild.ReturnReceiptNoteQty = itemUpdate.ReturnReceiptNoteQty.HasValue ? itemUpdate.ReturnReceiptNoteQty.Value : 0;
                                    //RNDetailChild.ReceiptNoteQty = itemUpdate.ReceiptNoteQty.HasValue ? itemUpdate.ReceiptNoteQty.Value : 0;
                                }
                            }

                        }
                        if (RWOModels != null && RWOModels.Any())
                            cxt.I_ReceiptSelfMade_Work_Order.AddRange(RWOModels);

                        //========= Change Status WO
                        var lstObj = cxt.I_Work_Order.Where(ww => ListWOId.Contains(ww.Id)).ToList();
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
                        }
                        List<I_ReceiptNoteForSeftMadeDetail> ListInsertRND = new List<I_ReceiptNoteForSeftMadeDetail>();
                        I_ReceiptNoteForSeftMadeDetail detail = null;

                        List<I_ReceiptNoteForSeftMadeDependentDetail> ListDetailDependent = new List<I_ReceiptNoteForSeftMadeDependentDetail>();
                        I_ReceiptNoteForSeftMadeDependentDetail detailDepen = null;

                        foreach (var RNSelfMadeDetail in model.ListItem)
                        {
                            detail = new I_ReceiptNoteForSeftMadeDetail();
                            detail.Id = Guid.NewGuid().ToString();
                            detail.ReceiptNoteId = ReceiptNoteForSeftMadeId;
                            detail.ReceivingQty = RNSelfMadeDetail.ReceivingQty;
                            detail.IsActived = true;
                            detail.Status = (int)Commons.EStatus.Actived;
                            detail.IngredientId = RNSelfMadeDetail.IngredientId;
                            detail.BaseReceivingQty = (RNSelfMadeDetail.ReceivingQty * RNSelfMadeDetail.BaseQty);

                            detail.WOId = RNSelfMadeDetail.WOId;

                            ListInsertRND.Add(detail);
                            if (_objIngredientDependent.ListChilds == null)
                                _objIngredientDependent.ListChilds = new List<RecipeIngredientUsageModels>();
                            var lstChild = _objIngredientDependent.ListChilds.Where(ww => ww.MixtureIngredientId == RNSelfMadeDetail.IngredientId).ToList();
                            foreach (var subitem in lstChild)
                            {
                                detailDepen = new I_ReceiptNoteForSeftMadeDependentDetail();
                                detailDepen.Id = Guid.NewGuid().ToString();
                                detailDepen.IngredientId = subitem.Id;
                                detailDepen.RNSelfMadeDetailId = detail.Id;
                                detailDepen.StockOutQty = subitem.TotalUsage;
                                detailDepen.IsActived = true;

                                ListDetailDependent.Add(detailDepen);

                                _objIngredientDependent.ListChilds.Remove(subitem);
                            }

                            //detail dependent

                            //ListInsertRND.Add(new I_ReceiptNoteForSeftMadeDetail
                            //{
                            //    Id = Guid.NewGuid().ToString(),
                            //    ReceiptNoteId = ReceiptNoteForSeftMadeId,
                            //    ReceivingQty = RNSelfMadeDetail.ReceivingQty,
                            //    IsActived = true,
                            //    Status = (int)Commons.EStatus.Actived,
                            //    IngredientId = RNSelfMadeDetail.IngredientId,
                            //    BaseReceivingQty = (RNSelfMadeDetail.ReceivingQty * RNSelfMadeDetail.BaseQty)
                            //});

                            if (RNSelfMadeDetail.IsSelfMode && RNSelfMadeDetail.IsStockAble)
                            {
                                lstInventory.Add(new InventoryModels()
                                {
                                    StoreId = model.StoreId,
                                    IngredientId = RNSelfMadeDetail.IngredientId,
                                    Price = 0,
                                    Quantity = (RNSelfMadeDetail.ReceivingQty * RNSelfMadeDetail.BaseQty)
                                });
                            }

                        }
                        cxt.I_ReceiptNoteForSeftMadeDetail.AddRange(ListInsertRND);
                        cxt.I_ReceiptNoteForSeftMadeDependentDetail.AddRange(ListDetailDependent);
                        //=============
                        cxt.SaveChanges();
                        transaction.Commit();
                        //Auto change statu for list PO
                        if (model.ListWorkOrder != null && model.ListWorkOrder.Any())
                            _inventoryFactory.CloseWOAuto(ListWOId);
                        NSLog.Logger.Info("Save RNForSeftMade", model);
                        if (lstInventory.Count > 0)
                        {
                            //Update List Ingredient have Properties Self-made = True and StockAble = true
                            _inventoryFactory.UpdateInventoryForSeftMadeReceiptNoteWhenStockAbleOn(lstInventory, _objIngredientDependent, ReceiptNoteForSeftMadeId, ref resultModels);
                            NSLog.Logger.Info(string.Format("UpdateInventoryForSeftMadeReceiptNoteWhenStockAbleOn:  [{0}] - [{1}]- [{2}]", resultModels.IsOk, ReceiptNoteForSeftMadeId, resultModels.Message));
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
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
                var lstObj = cxt.I_ReceiptNoteForSeftMade.Where(ww => lstId.Contains(ww.Id)).ToList();
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

        public List<ReceiptNoteSelfMadeModels> GetData(ReceiptNoteSelfMadeViewModels model, List<string> ListStoreId)
        {
            List<ReceiptNoteSelfMadeModels> lstData = new List<ReceiptNoteSelfMadeModels>();
            using (var cxt = new NuWebContext())
            {
                lstData = (from rn in cxt.I_ReceiptNoteForSeftMade
                           where ListStoreId.Contains(rn.StoreId) && rn.Status != (int)Commons.EStatus.Deleted
                           select new ReceiptNoteSelfMadeModels()
                           {
                               Id = rn.Id,
                               ReceiptNo = rn.ReceiptNo,

                               Status = rn.Status,
                               ReceiptDate = rn.ReceiptDate,
                               ReceiptBy = rn.ReceiptBy,

                               StoreId = rn.StoreId,
                           }).ToList();
                if (!string.IsNullOrEmpty(model.StoreID))
                {
                    lstData = lstData.Where(ww => ww.StoreId.Equals(model.StoreID.ToLower())).ToList();
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

        public ReceiptNoteSelfMadeModels GetReceiptNoteSelfMadeById(string id)
        {
            using (var cxt = new NuWebContext())
            {
                var model = (from rn in cxt.I_ReceiptNoteForSeftMade
                             where rn.Id.Equals(id) && rn.Status != (int)Commons.EStatus.Deleted
                             select new ReceiptNoteSelfMadeModels()
                             {
                                 Id = rn.Id,
                                 ReceiptNo = rn.ReceiptNo,

                                 Status = rn.Status,
                                 ReceiptDate = rn.ReceiptDate,
                                 ReceiptBy = rn.ReceiptBy,

                                 StoreId = rn.StoreId,

                                 WONo = rn.ReceiptNo
                             }).FirstOrDefault();

                var ListItem = (from RND in cxt.I_ReceiptNoteForSeftMadeDetail
                                from I in cxt.I_Ingredient
                                from UOM in cxt.I_UnitOfMeasure
                                where RND.ReceiptNoteId.Equals(model.Id)
                                       && RND.IngredientId.Equals(I.Id) && I.Status != (int)Commons.EStatus.Deleted
                                       && I.ReceivingUOMId.Equals(UOM.Id) && UOM.Status != (int)Commons.EStatus.Deleted
                                select new ReceiptNoteSelfMadeDetailModels()
                                {
                                    IngredientCode = I.Code,
                                    IngredientName = I.Name,
                                    BaseUOM = UOM.Name,
                                    ReceivingQty = RND.ReceivingQty,
                                    BaseReceivingQty = RND.BaseReceivingQty,

                                    ReceiptNoteId = RND.ReceiptNoteId,
                                    //====
                                    WOId = RND.WOId
                                }).ToList();

                var listWO = (from rn in cxt.I_ReceiptSelfMade_Work_Order
                              join wo in cxt.I_Work_Order on rn.WorkOrderId equals wo.Id
                              where rn.RNSelfMadeId.Equals(id) && wo.Status != (int)Commons.EStatus.Deleted
                              select new ReceiptNoteSelfMadeModels()
                              {
                                  Id = rn.RNSelfMadeId,
                                  WOId = wo.Id,
                                  WONo = wo.Code
                              }).ToList();

                if (model != null)
                {
                    model.ListItem = ListItem;
                    model.ListItem.ForEach(x =>
                    {
                        if (listWO != null && listWO.Any())
                        {
                            var wo = listWO.Where(z => z.Id.Equals(x.ReceiptNoteId) && z.WOId.Equals(x.WOId)).FirstOrDefault();
                            x.WONumber = wo == null ? "" : wo.WONo;
                        }
                    });
                }
                return model;
            }
        }

        public List<WorkOrderModels> LoadWOForRN(string StoreId, List<string> ListWONo, string WONo)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Work_Order
                                     where tb.StoreId.Equals(StoreId)
                                     && (tb.Status == (int)Commons.EPOStatus.Approved || tb.Status == (int)Commons.EPOStatus.InProgress)

                                     orderby tb.Code descending
                                     select new WorkOrderModels()
                                     {
                                         Id = tb.Id,
                                         WONumber = tb.Code,
                                         DateCompleted = tb.DateCompleted,
                                         Code = tb.Code
                                     }).ToList();
                    if (!string.IsNullOrEmpty(WONo))
                    {
                        lstResult = lstResult.Where(x => x.WONumber.Contains(WONo)).ToList();
                    }
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<ReceiptNoteSelfMadeDetailModels> GetData(string WorkOrderId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Work_Order_Detail
                                     from i in cxt.I_Ingredient
                                     from uom in cxt.I_UnitOfMeasure
                                     where tb.WorkOrderId.Equals(WorkOrderId) && tb.IngredientId.Equals(i.Id)
                                            && i.ReceivingUOMId.Equals(uom.Id)
                                            && uom.Status != (int)Commons.EStatus.Deleted
                                            && i.Status != (int)Commons.EStatus.Deleted

                                     select new ReceiptNoteSelfMadeDetailModels()
                                     {
                                         Id = tb.Id,

                                         IngredientId = tb.IngredientId,
                                         BaseUOM = uom.Name,
                                         IngredientCode = i.Code,
                                         IngredientName = i.Name,
                                         IsSelfMode = i.IsSelfMode,
                                         IsStockAble = i.StockAble.Value,
                                         IngReceivingQty = i.ReceivingQty,

                                         BaseQty = (tb.BaseQty ?? 0) / tb.Qty,
                                         QtyTolerance = i.QtyTolerance,
                                         QtyToleranceS = ((i.QtyTolerance / 100) * tb.Qty),
                                         QtyToleranceP = (((i.QtyTolerance / 100) * tb.Qty)),

                                         Qty = tb.Qty, //=>(1)
                                         //UnitPrice = tb.UnitPrice.Value,
                                         //Amount = tb.Amount.Value,

                                         ReceiptNoteQty = tb.ReceiptNoteQty.Value, //=>(2)
                                         ReceivedQty = tb.ReceiptNoteQty.Value,

                                         //// = (1) - (2) 
                                         RemainingQty = (tb.Qty - tb.ReceiptNoteQty.Value),
                                         WOId = WorkOrderId
                                     }).ToList();
                    lstResult.ForEach(x =>
                    {
                        if (x.RemainingQty <= 0) // Full
                        {
                            x.RemainingQty = 0;
                            x.IsVisible = true;
                        }
                        x.ReceivingQty = x.RemainingQty;
                    });
                    if (lstResult != null && lstResult.Any())
                        lstResult = lstResult.Where(ww => !ww.IsVisible).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }
    }
}
