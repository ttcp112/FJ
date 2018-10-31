using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class IngredientsUsageFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private InventoryFactory _inventoryFactory = null;

        public IngredientsUsageFactory()
        {
            _baseFactory = new BaseFactory();
            _inventoryFactory = new InventoryFactory();
        }

        public bool InsertAlocation(List<IngredientsUsageModels> lstInput, string createdBy, ref string msg)
        {
            bool result = true;
            ResultModels resultModels = new ResultModels();
            string allocationId = string.Empty;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<StockCountDetailModels> lstStockCount = new List<StockCountDetailModels>();

                        var lstDate = lstInput.GroupBy(gg => new { BusinessId = gg.BusinessId, StoreId = gg.StoreId }).ToList();
                        List<I_Allocation> lstHeaderInsert = new List<I_Allocation>();
                        I_Allocation allocation = new I_Allocation();
                        List<I_AllocationDetail> lstDetailInsert = new List<I_AllocationDetail>();
                        I_AllocationDetail itemDetail = new I_AllocationDetail();
                        foreach (var item in lstDate)
                        {
                            allocation = new I_Allocation();
                            allocation.ApplyDate = item.Select(ss=>ss.Date).FirstOrDefault();
                            allocation.StoreId = item.Key.StoreId;
                            allocation.BusinessId = item.Key.BusinessId;
                            allocation.IsActived = true;
                            allocation.Id = Guid.NewGuid().ToString();
                            allocationId = allocation.Id;
                            allocation.CreatedBy = createdBy;
                            allocation.ModifierBy = createdBy;
                            allocation.CreatedDate = DateTime.Now;
                            allocation.ModifierDate = DateTime.Now;

                            lstHeaderInsert.Add(allocation);

                            //Details
                            foreach (var subItem in item.ToList())
                            {
                                itemDetail = new I_AllocationDetail();
                                itemDetail.Id = Guid.NewGuid().ToString();
                                itemDetail.AllocationId = allocation.Id;
                                itemDetail.IngredientId = subItem.IngredientId;
                                itemDetail.OpenBal = subItem.OpenBal;
                                itemDetail.CloseBal = subItem.CloseBal;
                                itemDetail.Sales = subItem.Sales;
                                itemDetail.ActualSold = subItem.ActualSold;
                                itemDetail.Damage = subItem.Damage;
                                itemDetail.Wast = subItem.Wast;
                                itemDetail.Others = subItem.Others;
                                
                                if (string.IsNullOrEmpty(subItem.Reasons))
                                {
                                    itemDetail.Reasons = string.Empty;
                                }
                                else
                                    itemDetail.Reasons = subItem.Reasons;


                                lstDetailInsert.Add(itemDetail);

                                //for stockcount
                                lstStockCount.Add(new StockCountDetailModels()
                                {
                                    StoreId = item.Key.StoreId,
                                    BusinessId = item.Key.BusinessId,
                                    IngredientId = subItem.IngredientId,
                                    Damage = subItem.Damage,
                                    Wastage = subItem.Wast,
                                    OtherQty = subItem.Others,
                                    Reasons = itemDetail.Reasons
                                });
                            }
                        }

                        cxt.I_Allocation.AddRange(lstHeaderInsert);
                        cxt.I_AllocationDetail.AddRange(lstDetailInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        //uppdate stockcount
                        StockCountFactory _stockCountFactory = new StockCountFactory();
                        _stockCountFactory.UpdateStockCountWhenInsertAllocation(lstStockCount, ref resultModels);
                        _logger.Info(string.Format("UpdateStockCountWhenInsertAllocation: [{0}] - [{1}]", resultModels.IsOk, resultModels.Message));
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
                    I_Stock_Transfer itemDelete = (from tb in cxt.I_Stock_Transfer
                                                   where tb.Id == Id
                                                   select tb).FirstOrDefault();
                    //cxt.I_Stock_Transfer.Remove(itemDelete);
                    cxt.SaveChanges();
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

        public List<IngredientsUsageModels> GetListIngredientUsage(IngredientsUsageRequestViewModels model)
        {
            var lstResult = new List<IngredientsUsageModels>();
            if (string.IsNullOrEmpty(model.StoreId))
                return lstResult;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {

                    lstResult = (from h in cxt.I_StockCount
                                 join d in cxt.I_StockCountDetail on  h.Id equals d.StockCountId
                                 from i in cxt.I_Ingredient.Where(ww => ww.Id == d.IngredientId).DefaultIfEmpty()
                                 where h.StoreId == model.StoreId && DbFunctions.TruncateTime(h.StockCountDate) >= DbFunctions.TruncateTime(model.ApplyFrom)
                                  && DbFunctions.TruncateTime(h.StockCountDate) <= DbFunctions.TruncateTime(model.ApplyTo)
                                  //&& i.Id == "d470f8bd-dd22-437e-9705-3d916999050e" && h.BusinessId == "b0379b38-169d-4508-bd08-ce1dd7f950a7"
                                  && h.Status == (int)Commons.EStockCountStatus.Approved
                                 select new IngredientsUsageModels()
                                 {
                                     Id = d.Id,
                                     Date = h.StockCountDate,
                                     StoreId = h.StoreId,
                                     IngredientId = d.IngredientId,
                                     IngredientCode = i.Code,
                                     IngredientName = i.Name,
                                     OpenBal = d.OpenBal.HasValue ? d.OpenBal.Value : 0,
                                     CloseBal = d.CloseBal,
                                     BusinessId = h.BusinessId,
                                     StartedOn = h.StartedOn,
                                     ClosedOn = h.ClosedOn,
                                     AutoCloseBal = d.AutoCloseBal

                                 }).Distinct().ToList();


                    //Check Stock In
                    //var lstIngredientId = lstResult.Select(ss => ss.IngredientId).Distinct().ToList();
                    //if (lstIngredientId != null && lstIngredientId.Any())
                    //{
                    var lstBusinessIds = lstResult.Select(ss => ss.BusinessId).Distinct().ToList();
                    var lstStockIn = GetStockInOut(model.StoreId, lstBusinessIds);
                    //lstStockIn = lstStockIn.Where(ww => ww.IngredientId == "38107504-78c4-46a9-831e-5688832c591e").ToList();
                    foreach (var item in lstResult)
                    {
                        var lstObj = lstStockIn.Where(ww => ww.IngredientId == item.IngredientId && ww.BusinessId == item.BusinessId
                            && ww.StoreId == item.StoreId).ToList();
                        if (lstObj != null && lstObj.Any())
                        {
                            item.StockIn = lstObj.Sum(ss=>ss.StockIn);
                            item.StockOut = lstObj.Sum(ss => ss.StockOut);
                        }
                        if (item.StartedOn.HasValue)
                        {
                            item.BusinessDayDisplay = item.StartedOn.Value.ToString("MM/dd/yyyy HH:mm") + " - ";
                            if (item.ClosedOn.HasValue && item.ClosedOn.Value != Commons.MinDate)
                            {
                                item.BusinessDayDisplay += (item.ClosedOn.HasValue ? item.ClosedOn.Value.ToString("MM/dd/yyyy HH:mm") : "");
                            }                               
                        }
                      
                    }
                    //}

                    //Check usage
                    var lstUsage = (from d in cxt.I_UsageManagementDetail
                                    from h in cxt.I_UsageManagement.Where(ww => ww.Id == d.UsageManagementId).DefaultIfEmpty()
                                    join i in cxt.I_Ingredient on d.IngredientId equals i.Id
                                    where h.StoreId == model.StoreId 
                                  //  && DbFunctions.TruncateTime(h.DateFrom) >= DbFunctions.TruncateTime(model.ApplyFrom)
                                  //&& DbFunctions.TruncateTime(h.DateFrom) <= DbFunctions.TruncateTime(model.ApplyTo)

                                  && lstBusinessIds.Contains(h.BusinessId)
                                    select new UsageManagementDetailModel()
                                    {
                                        ItemId = d.IngredientId,
                                        Usage = d.Usage,
                                        ApplyDate = h.DateFrom,
                                        BusinessId  = h.BusinessId,
                                        IsSeftMade = i.IsSelfMode,
                                        StockAble = i.StockAble.HasValue? i.StockAble.Value: false
                                    }).ToList();

                    //if (lstUsage != null && lstUsage.Any())
                    //{
                    //    foreach (var item in lstResult)
                    //    {
                    //        var obj = lstUsage.Where(ww => ww.ItemId == item.IngredientId
                    //        && item.Date.Date == ww.ApplyDate.Date).FirstOrDefault();

                    //        item.Sales = obj != null ? obj.Usage : 0;
                    //    }
                    //}
                    if (lstUsage == null)
                        lstUsage = new List<UsageManagementDetailModel>();

                    var lstTmpId = lstUsage.Where(ww => ww.IsSeftMade && !ww.StockAble).Select(ss => ss.ItemId).ToList();
                    if (lstTmpId != null && lstTmpId.Any())
                    {
                        //check ingredient dependent
                        RecipeFactory _recipeFactory = new RecipeFactory();
                        var _objIngredientDependent = _recipeFactory.GetRecipesByIngredientSeftMade(lstTmpId);
                      
                        var lstIngredientSMNotStockAble = lstUsage.Where(ww => ww.IsSeftMade && !ww.StockAble).ToList();
                        var businessIds = lstIngredientSMNotStockAble.Select(ss => ss.BusinessId).Distinct().ToList();
                        foreach (var item in businessIds)
                        {
                            var tmp = lstIngredientSMNotStockAble.Where(ww => ww.BusinessId == item).ToList();
                            foreach (var subitem in tmp)
                            {
                                var obj = _objIngredientDependent.ListChilds.Where(ww => ww.MixtureIngredientId == subitem.ItemId).ToList();

                                foreach (var item1 in obj)
                                {
                                    lstUsage.Add(new UsageManagementDetailModel()
                                    {
                                        ItemId = item1.Id,
                                        Usage = (item1.BaseUsage * subitem.Usage),
                                        BusinessId = item
                                    });
                                }
                                //obj.ForEach(ss => ss.BaseUsage = ss.BaseUsage * subitem.Usage);
                            }
                            //var lstGroup = _objIngredientDependent.ListChilds.GroupBy(gg =>  gg.Id);
                            //foreach (var item1 in lstGroup)
                            //{
                            //    lstUsage.Add(new UsageManagementDetailModel()
                            //    {
                            //        ItemId = item1.Key,
                            //        Usage = item1.Sum(ss => ss.BaseUsage),
                            //        BusinessId = item
                            //    });
                            //}
                        }
                       
                        lstUsage = lstUsage.Where(ww => !lstTmpId.Contains(ww.ItemId)).ToList();
                    }
                    //Check allocation
                    var lstAllocation = (from d in cxt.I_AllocationDetail
                                         from h in cxt.I_Allocation.Where(ww => ww.Id == d.AllocationId).DefaultIfEmpty()
                                         where h.StoreId == model.StoreId
                                            && lstBusinessIds.Contains(h.BusinessId)
                                         //  && DbFunctions.TruncateTime(h.ApplyDate) >= DbFunctions.TruncateTime(model.ApplyFrom)
                                         //&& DbFunctions.TruncateTime(h.ApplyDate) <= DbFunctions.TruncateTime(model.ApplyTo)
                                         select new IngredientsUsageModels()
                                         {
                                             IngredientId  = d.IngredientId,
                                             StoreId = h.StoreId,
                                             Damage = d.Damage,
                                             Wast = d.Wast,
                                             Others = d.Others,
                                             BusinessId = h.BusinessId
                                         }).ToList();
                    if (lstAllocation == null)
                        lstAllocation = new List<IngredientsUsageModels>();
                    //lst dataEntry
                    var lstDataEntry = (from d in cxt.I_DataEntryDetail
                                        join h in cxt.I_DataEntry on d.DataEntryId equals h.Id
                                        where h.StoreId == model.StoreId
                                          && lstBusinessIds.Contains(h.BusinessId)
                                        //&& DbFunctions.TruncateTime(h.EntryDate) >= DbFunctions.TruncateTime(model.ApplyFrom)
                                        //&& DbFunctions.TruncateTime(h.EntryDate) <= DbFunctions.TruncateTime(model.ApplyTo)
                                        select new IngredientsUsageModels()
                                        {
                                            IngredientId = d.IngredientId,
                                            StoreId = h.StoreId,
                                            Damage = d.Damage.HasValue? d.Damage.Value:0,
                                            Wast = d.Wastage.HasValue? d.Wastage.Value:0,
                                            Others = d.OrderQty.HasValue? d.OrderQty.Value:0,
                                            BusinessId = h.BusinessId
                                        }).ToList();
                    if (lstDataEntry == null)
                        lstDataEntry = new List<IngredientsUsageModels>();

                    var allocations = new List<IngredientsUsageModels>();
                    var dataEntrys = new List<IngredientsUsageModels>();
                    decimal openBal = 0; decimal stockIn = 0; decimal closeBal = 0; decimal stockOut = 0;
                    foreach (var item in lstResult)
                    {
                        var obj = (decimal)lstUsage.Where(ww => ww.ItemId == item.IngredientId
                            //&& item.Date.Date == ww.ApplyDate.Date).FirstOrDefault();
                            && item.BusinessId == ww.BusinessId).Sum(ss=>ss.Usage);
                        item.Sales =  Math.Round((double)obj, 2);
                        allocations = lstAllocation.Where(ww => ww.IngredientId == item.IngredientId && ww.StoreId == item.StoreId && ww.BusinessId == item.BusinessId).ToList();
                        if (allocations != null && allocations.Any())
                        {
                            item.Damage = allocations.Sum(ss => ss.Damage);
                            item.Wast = allocations.Sum(ss => ss.Wast);
                            item.Others = allocations.Sum(ss => ss.Others);
                            item.IsExistAllocation = true;
                        }
                        decimal.TryParse(item.OpenBal.ToString(), out openBal);
                        decimal.TryParse(item.StockIn.ToString(), out stockIn);
                        decimal.TryParse(item.CloseBal.ToString(), out closeBal);
                        decimal.TryParse(item.StockOut.ToString(), out stockOut);
                        item.ActualSold = (double)((openBal + stockIn) - closeBal - stockOut);
                        //item.IsAllocation = Math.Abs(item.ActualSold - item.Sales) != (item.Damage + item.Wast + item.Others);//(item.ActualSold != item.Sales);
                        item.IsAllocation = (item.ActualSold != item.Sales);
                        //if (!item.IsExistAllocation)
                        //{
                            //item.Damage = 0;
                            //item.Wast = 0;
                            //item.Others = 0;

                            //
                            //dataEntrys = lstDataEntry.Where(ww => ww.IngredientId == item.IngredientId && ww.StoreId == item.StoreId && ww.BusinessId == item.BusinessId).ToList();
                            //if (dataEntrys != null && dataEntrys.Any())
                            //{
                            //    item.Damage = dataEntrys.Sum(ss => ss.Damage);
                            //    item.Wast = dataEntrys.Sum(ss => ss.Wast);
                            //    item.Others = dataEntrys.Sum(ss => ss.Others);
                            //}
                        //}
                        item.VarianceQty = Math.Abs((double)((decimal)item.ActualSold - (decimal)item.Sales));
                        //item.IsAllocation = true;
                        item.DateDisplay = item.Date.ToString("MM/dd/yyyy");

                        //item.IsAllocation = true;
                        item.Adjust = item.AutoCloseBal.HasValue ? Math.Round((item.CloseBal - item.AutoCloseBal.Value), 4) : 0;
                    }

                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return lstResult;
                }
            }
        }

        public IngredientsUsageModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Stock_Transfer
                                 where tb.Id == ID
                                 select new IngredientsUsageModels()
                                 {
                                     //Id = tb.Id,
                                     //IssueStoreId = tb.IssueStoreId,
                                     //ReceiveStoreId = tb.ReceiveStoreId,
                                     //RequestBy = tb.RequestBy,
                                     //RequestDate = tb.RequestDate,
                                     //IssueBy = tb.IssueBy,
                                     //IssueDate = tb.IssueDate,
                                     //ReceiveBy = tb.ReceiveBy,
                                     //ReceiveDate = tb.ReceiveDate,
                                     //IsActive = tb.IsActive,
                                     //StockTransferNo = tb.StockTransferNo

                                 }).FirstOrDefault();
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        private List<IngredientsUsageModels> GetStockInOut(string storeId, List<string> lstBusinessIds)
        {
            var lstResults = new List<IngredientsUsageModels>();
            //dFrom = new DateTime(dFrom.Year, dFrom.Month, dFrom.Day, 0, 0, 0);
            //dTo = new DateTime(dTo.Year, dTo.Month, dTo.Day, 23, 59, 59);
            using (NuWebContext cxt = new NuWebContext())
            {
                //Receive Note
                var query = (from d in cxt.I_ReceiptNoteDetail
                             from h in cxt.I_ReceiptNote.Where(ww => ww.Id == d.ReceiptNoteId).DefaultIfEmpty()
                             from p in cxt.I_Purchase_Order_Detail.Where(ww => ww.Id == d.PurchaseOrderDetailId).DefaultIfEmpty()
                             where h.StoreId == storeId && lstBusinessIds.Contains(h.BusinessId)
                             //where h.StoreId == storeId && h.ReceiptDate >= dFrom && h.ReceiptDate <= dTo
                             select new IngredientsUsageModels()
                             {
                                 BusinessId = h.BusinessId,
                                 IngredientId = p.IngredientId,
                                 StockIn = d.BaseReceivingQty.HasValue ? d.BaseReceivingQty.Value : 0,
                                 StoreId = h.StoreId,
                             }).ToList();

                //Transfer out
                var queryTransfer = (from d in cxt.I_Stock_Transfer_Detail
                                     from h in cxt.I_Stock_Transfer.Where(ww => ww.Id == d.StockTransferId).DefaultIfEmpty()
                                     where h.IssueStoreId == storeId && lstBusinessIds.Contains(h.BusinessId)  //&& h.ReceiveDate >= dFrom && h.ReceiveDate <= dTo
                                     select new IngredientsUsageModels()
                                     {
                                         BusinessId = h.BusinessId,
                                         IngredientId = d.IngredientId,
                                         StockOut= d.IssueBaseQty.HasValue ? d.IssueBaseQty.Value : 0,
                                         StoreId = h.IssueStoreId,
                                     }).ToList();


                //Transfer in
                var queryTransferIn = (from d in cxt.I_Stock_Transfer_Detail
                                     from h in cxt.I_Stock_Transfer.Where(ww => ww.Id == d.StockTransferId).DefaultIfEmpty()
                                     where h.ReceiveStoreId == storeId && lstBusinessIds.Contains(h.BusinessReceiveId)  //&& h.ReceiveDate >= dFrom && h.ReceiveDate <= dTo
                                     select new IngredientsUsageModels()
                                     {
                                         BusinessId = h.BusinessReceiveId,
                                         IngredientId = d.IngredientId,
                                         StockIn = d.ReceiveBaseQty.HasValue ? d.ReceiveBaseQty.Value : 0,
                                         StoreId = h.ReceiveStoreId,
                                     }).ToList();

                //dataentry
                var queryDataEntry = (from d in cxt.I_DataEntryDetail
                                     from h in cxt.I_DataEntry.Where(ww => ww.Id == d.DataEntryId).DefaultIfEmpty()
                                     where h.StoreId == storeId && lstBusinessIds.Contains(h.BusinessId)  //&& h.ReceiveDate >= dFrom && h.ReceiveDate <= dTo
                                     select new IngredientsUsageModels()
                                     {
                                         BusinessId = h.BusinessId,
                                         IngredientId = d.IngredientId,
                                         StockOut = (d.Damage.HasValue? d.Damage.Value :0) + (d.Wastage.HasValue ? d.Wastage.Value : 0)
                                                + (d.OrderQty.HasValue ? d.OrderQty.Value : 0),
                                         StoreId = h.StoreId,
                                     }).ToList();

                //return
                //var queryReturn = (from d in cxt.I_Return_Note_Detail
                //                   join rd in  cxt.I_ReceiptNoteDetail on d.ReceiptNoteDetailId equals rd.Id
                //                   join pd in cxt.I_Purchase_Order_Detail on rd.PurchaseOrderDetailId equals pd.Id
                //                   join rh in cxt.I_ReceiptNote on rd.ReceiptNoteId equals rh.Id
                //                   where rh.StoreId == storeId && lstBusinessIds.Contains(rh.BusinessId)
                //                   select new IngredientsUsageModels()
                //                   {
                //                       IngredientId = pd.IngredientId,
                //                       StockOut = d.ReturnBaseQty.HasValue ? d.ReturnBaseQty.Value : 0,
                //                       BusinessId = rh.BusinessId,
                //                       StoreId = rh.StoreId
                //                   }).ToList();

                //return
                var queryReturn = (from d in cxt.I_Return_Note_Detail
                                   join h in cxt.I_Return_Note on d.ReturnNoteId equals h.Id
                                   join rd in cxt.I_ReceiptNoteDetail on d.ReceiptNoteDetailId equals rd.Id
                                   join pd in cxt.I_Purchase_Order_Detail on rd.PurchaseOrderDetailId equals pd.Id
                                   join rh in cxt.I_ReceiptNote on h.ReceiptNoteId equals rh.Id
                                   //where rh.StoreId == storeId && lstBusinessIds.Contains(rh.BusinessId)
                                   where lstBusinessIds.Contains(h.BusinessId) && rh.StoreId == storeId
                                   select new IngredientsUsageModels()
                                   {
                                       IngredientId = pd.IngredientId,
                                       StockOut = d.ReturnBaseQty.HasValue ? d.ReturnBaseQty.Value : 0,
                                       BusinessId = h.BusinessId,
                                       StoreId = rh.StoreId
                                   }).ToList();

                //Receive Note Safe-Made
                var queryRNSeftMade = (from d in cxt.I_ReceiptNoteForSeftMadeDetail
                                       join h in cxt.I_ReceiptNoteForSeftMade on d.ReceiptNoteId equals h.Id
                                       join i in cxt.I_Ingredient on d.IngredientId equals i.Id
                                       where lstBusinessIds.Contains(h.BusinessId) && h.StoreId == storeId
                                       select new IngredientsUsageModels()
                                       {
                                           Id = h.Id,
                                           BusinessId = h.BusinessId,
                                           IngredientId = d.IngredientId,
                                           StockIn = d.BaseReceivingQty.HasValue ? d.BaseReceivingQty.Value : 0,
                                           StoreId = h.StoreId,
                                           IsSeftMade = i.IsSelfMode,
                                           IsStockAble = i.StockAble.HasValue ? i.StockAble.Value : false
                                       }).ToList();

                //Get stock out for  Receive Note Safe-Made dependent
                var queryRNSeftMadeDepens = (from dd in cxt.I_ReceiptNoteForSeftMadeDependentDetail
                                             join d in cxt.I_ReceiptNoteForSeftMadeDetail on dd.RNSelfMadeDetailId equals d.Id
                                             join h in cxt.I_ReceiptNoteForSeftMade on d.ReceiptNoteId equals h.Id
                                             join i in cxt.I_Ingredient on d.IngredientId equals i.Id
                                             where lstBusinessIds.Contains(h.BusinessId) && h.StoreId == storeId
                                             select new IngredientsUsageModels()
                                             {
                                                 Id = h.Id,
                                                 BusinessId = h.BusinessId,
                                                 IngredientId = dd.IngredientId,
                                                 StockOut = dd.StockOutQty,
                                                 StoreId = h.StoreId

                                             }).ToList();


                lstResults.AddRange(query);
                lstResults.AddRange(queryTransfer);
                lstResults.AddRange(queryTransferIn);
                lstResults.AddRange(queryDataEntry);
                lstResults.AddRange(queryReturn);
                lstResults.AddRange(queryRNSeftMade);
                lstResults.AddRange(queryRNSeftMadeDepens);
            }
            return lstResults;
        }
    }
}
