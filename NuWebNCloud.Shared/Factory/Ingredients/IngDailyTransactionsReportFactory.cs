using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Xero;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using NuWebNCloud.Shared.Models.Reports;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class IngDailyTransactionsReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public IngDailyTransactionsReportFactory()
        {
            _baseFactory = new BaseFactory();
        }
        
        public List<DailyTransactionsReportModels> GetListIngredientUsage(BaseReportModel model, string StoreId, List<string> lstBusinessInputIds)
        {
            //lstBusinessInputIds = new List<string>();
            //lstBusinessInputIds.Add("9f3716d3-e38a-4336-a6da-a06807fdc898");
            var lstResult = new List<DailyTransactionsReportModels>();
            if (string.IsNullOrEmpty(StoreId))
                return lstResult;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {

                    lstResult = (from h in cxt.I_StockCount
                                 //from d in cxt.I_StockCountDetail.Where(ww => ww.StockCountId == h.Id).DefaultIfEmpty()
                                 join d in cxt.I_StockCountDetail on h.Id equals d.StockCountId
                                 from i in cxt.I_Ingredient.Where(ww => ww.Id == d.IngredientId).DefaultIfEmpty()
                                 join baseUOM in cxt.I_UnitOfMeasure on   i.BaseUOMId equals baseUOM.Id
                                 where h.StoreId == StoreId
                                 && lstBusinessInputIds.Contains(h.BusinessId)
                                  && h.Status == (int)Commons.EStockCountStatus.Approved
                                 select new DailyTransactionsReportModels()
                                 {
                                     Id = d.Id,
                                     Date = h.StockCountDate,
                                     StoreId = h.StoreId,
                                     IngredientId = d.IngredientId,
                                     IngredientCode = i.Code,
                                     IngredientName = i.Name,
                                     BaseUOMName = baseUOM.Name,
                                     TypeName = i.IsPurchase? "Purchase" : "Self-made",
                                     OpenBal = d.OpenBal.HasValue ? d.OpenBal.Value : 0,
                                     CloseBal = d.CloseBal,
                                     BusinessId = h.BusinessId,
                                     StartedOn = h.StartedOn,
                                     ClosedOn = h.ClosedOn,
                                     AutoCloseBal = d.AutoCloseBal,
                                     IsAutoCreated = h.IsAutoCreated.HasValue? h.IsAutoCreated.Value: false

                                 }).Distinct().ToList();

                    //Check Stock In
                    var lstBusinessIds = lstResult.Select(ss => ss.BusinessId).Distinct().ToList();
                    //Get received
                    var queryReceived = (from d in cxt.I_ReceiptNoteDetail
                                 from h in cxt.I_ReceiptNote.Where(ww => ww.Id == d.ReceiptNoteId).DefaultIfEmpty()
                                 from p in cxt.I_Purchase_Order_Detail.Where(ww => ww.Id == d.PurchaseOrderDetailId).DefaultIfEmpty()
                                 where h.StoreId == StoreId && lstBusinessIds.Contains(h.BusinessId)
                                 select new DailyTransactionsReportModels()
                                 {
                                     BusinessId = h.BusinessId,
                                     IngredientId = p.IngredientId,
                                     StockIn = d.BaseReceivingQty.HasValue ? d.BaseReceivingQty.Value : 0,
                                     Received = d.BaseReceivingQty.HasValue ? d.BaseReceivingQty.Value : 0,
                                     StoreId = h.StoreId,
                                 }).ToList();
                    //Get TransferIn
                    var queryTransferIn = (from d in cxt.I_Stock_Transfer_Detail
                                           from h in cxt.I_Stock_Transfer.Where(ww => ww.Id == d.StockTransferId).DefaultIfEmpty()
                                           where h.ReceiveStoreId == StoreId && lstBusinessIds.Contains(h.BusinessReceiveId)
                                           select new DailyTransactionsReportModels()
                                           {
                                               BusinessId = h.BusinessReceiveId,
                                               IngredientId = d.IngredientId,
                                               StockIn = d.ReceiveBaseQty.HasValue ? d.ReceiveBaseQty.Value : 0,
                                               TransferIn = d.ReceiveBaseQty.HasValue ? d.ReceiveBaseQty.Value : 0,
                                               StoreId = h.ReceiveStoreId,
                                           }).ToList();

                    //return
                    var queryReturn = (from d in cxt.I_Return_Note_Detail
                                       join h in cxt.I_Return_Note on d.ReturnNoteId equals h.Id
                                       join rd in cxt.I_ReceiptNoteDetail on d.ReceiptNoteDetailId equals rd.Id
                                       join pd in cxt.I_Purchase_Order_Detail on rd.PurchaseOrderDetailId equals pd.Id
                                       join rh in cxt.I_ReceiptNote on h.ReceiptNoteId equals rh.Id
                                       where lstBusinessIds.Contains(h.BusinessId) && rh.StoreId == StoreId
                                       select new DailyTransactionsReportModels()
                                       {
                                           IngredientId = pd.IngredientId,
                                           StockOut = d.ReturnBaseQty.HasValue ? d.ReturnBaseQty.Value : 0,
                                           Return = d.ReturnBaseQty.HasValue ? d.ReturnBaseQty.Value : 0,
                                           BusinessId = h.BusinessId,
                                           StoreId = rh.StoreId
                                       }).ToList();

                    //Transfer out
                    var queryTransfer = (from d in cxt.I_Stock_Transfer_Detail
                                         from h in cxt.I_Stock_Transfer.Where(ww => ww.Id == d.StockTransferId).DefaultIfEmpty()
                                         where h.IssueStoreId == StoreId && lstBusinessIds.Contains(h.BusinessId)
                                         select new DailyTransactionsReportModels()
                                         {
                                             BusinessId = h.BusinessId,
                                             IngredientId = d.IngredientId,
                                             StockOut = d.IssueBaseQty.HasValue ? d.IssueBaseQty.Value : 0,
                                             TransferOut = d.IssueBaseQty.HasValue ? d.IssueBaseQty.Value : 0,
                                             StoreId = h.IssueStoreId,
                                         }).ToList();

                    //data entry
                    var queryDataEntry = (from d in cxt.I_DataEntryDetail
                                          from h in cxt.I_DataEntry.Where(ww => ww.Id == d.DataEntryId).DefaultIfEmpty()
                                          where h.StoreId == StoreId && lstBusinessIds.Contains(h.BusinessId)
                                          select new DailyTransactionsReportModels()
                                          {
                                              BusinessId = h.BusinessId,
                                              IngredientId = d.IngredientId,
                                              StockOut = (d.Damage.HasValue ? d.Damage.Value : 0) + (d.Wastage.HasValue ? d.Wastage.Value : 0)
                                                     + (d.OrderQty.HasValue ? d.OrderQty.Value : 0),
                                              Damage = d.Damage.HasValue ? d.Damage.Value : 0,
                                              Wast = d.Wastage.HasValue ? d.Wastage.Value : 0,
                                              Others = d.OrderQty.HasValue ? d.OrderQty.Value : 0,
                                              StoreId = h.StoreId
                                          }).ToList();


                    //Receive Note Safe-Made
                    var queryRNSeftMade = (from d in cxt.I_ReceiptNoteForSeftMadeDetail
                                           join h in cxt.I_ReceiptNoteForSeftMade on d.ReceiptNoteId equals h.Id
                                           join i in cxt.I_Ingredient on d.IngredientId equals i.Id
                                           where lstBusinessIds.Contains(h.BusinessId) && h.StoreId == StoreId
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
                                                 where lstBusinessIds.Contains(h.BusinessId) && h.StoreId == StoreId
                                                 select new IngredientsUsageModels()
                                                 {
                                                     Id = h.Id,
                                                     BusinessId = h.BusinessId,
                                                     IngredientId = dd.IngredientId,
                                                     StockOut = dd.StockOutQty,
                                                     StoreId = h.StoreId

                                                 }).ToList();

                  
                    //Check usage
                    var lstUsage = (from d in cxt.I_UsageManagementDetail
                                    from h in cxt.I_UsageManagement.Where(ww => ww.Id == d.UsageManagementId).DefaultIfEmpty()
                                    join i in cxt.I_Ingredient on d.IngredientId equals i.Id
                                    where h.StoreId == StoreId
                                  && lstBusinessIds.Contains(h.BusinessId)
                                    select new UsageManagementDetailModel()
                                    {
                                        ItemId = d.IngredientId,
                                        Usage = d.Usage,
                                        ApplyDate = h.DateFrom,
                                        BusinessId = h.BusinessId,
                                        IsSeftMade = i.IsSelfMode,
                                        StockAble = i.StockAble.HasValue ? i.StockAble.Value : false
                                    }).ToList();
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
                         
                        }

                        lstUsage = lstUsage.Where(ww => !lstTmpId.Contains(ww.ItemId)).ToList();
                    }
                    //Check allocation
                    var lstAllocation = (from d in cxt.I_AllocationDetail
                                         from h in cxt.I_Allocation.Where(ww => ww.Id == d.AllocationId).DefaultIfEmpty()
                                         where h.StoreId == StoreId
                                            && lstBusinessIds.Contains(h.BusinessId)
                                         select new DailyTransactionsReportModels()
                                         {
                                             IngredientId = d.IngredientId,
                                             StoreId = h.StoreId,
                                             Damage = d.Damage,
                                             Wast = d.Wast,
                                             Others = d.Others,
                                             BusinessId = h.BusinessId
                                         }).ToList();
                    if (lstAllocation == null)
                        lstAllocation = new List<DailyTransactionsReportModels>();
                 

                    var allocations = new List<DailyTransactionsReportModels>();
                    var dataEntrys = new List<DailyTransactionsReportModels>();
                    //decimal openBal = 0; decimal stockIn = 0; decimal closeBal = 0; decimal stockOut = 0;
                    foreach (var item in lstResult)
                    {
                        double obj = (double)lstUsage.Where(ww => ww.ItemId == item.IngredientId
                            && item.BusinessId == ww.BusinessId).Sum(ss=> (decimal)ss.Usage);
                        item.Sales = Math.Round(obj, 2);
                        //received
                        item.Received = (double)queryReceived.Where(ww => ww.IngredientId == item.IngredientId
                            && item.BusinessId == ww.BusinessId).Sum(ss => (decimal)ss.Received);

                        item.Received += (double)queryRNSeftMade.Where(ww => ww.IngredientId == item.IngredientId
                            && item.BusinessId == ww.BusinessId).Sum(ss => (decimal)ss.StockIn);
                      //Transfer In
                      item.TransferIn = (double)queryTransferIn.Where(ww => ww.IngredientId == item.IngredientId
                            && item.BusinessId == ww.BusinessId).Sum(ss => (decimal)ss.TransferIn);
                        //return
                        item.Return = (double)queryReturn.Where(ww => ww.IngredientId == item.IngredientId
                           && item.BusinessId == ww.BusinessId).Sum(ss => (decimal)ss.Return);
                        //Transfer out
                        item.TransferOut = (double)queryTransfer.Where(ww => ww.IngredientId == item.IngredientId
                           && item.BusinessId == ww.BusinessId).Sum(ss => (decimal)ss.TransferOut);

                        item.UseForSelfMade = queryRNSeftMadeDepens.Where(ww => ww.IngredientId == item.IngredientId && item.BusinessId == ww.BusinessId).Sum(ss => (decimal)ss.StockOut);

                        allocations = lstAllocation.Where(ww => ww.IngredientId == item.IngredientId && ww.StoreId == item.StoreId
                        && ww.BusinessId == item.BusinessId).ToList();

                        if (allocations != null && allocations.Any())
                        {
                            item.Damage = (double)allocations.Sum(ss => (decimal)ss.Damage);
                            item.Wast = (double)allocations.Sum(ss => (decimal)ss.Wast);
                            item.Others = (double)allocations.Sum(ss => (decimal)ss.Others);
                            item.IsExistAllocation = true;
                        }
                        //decimal.TryParse(item.OpenBal.ToString(), out openBal);
                        //decimal.TryParse(item.StockIn.ToString(), out stockIn);
                        //decimal.TryParse(item.CloseBal.ToString(), out closeBal);
                        //decimal.TryParse(item.StockOut.ToString(), out stockOut);
                        //if (!item.IsExistAllocation)
                        //{
                            dataEntrys = queryDataEntry.Where(ww => ww.IngredientId == item.IngredientId && ww.StoreId == item.StoreId && ww.BusinessId == item.BusinessId).ToList();
                            if (dataEntrys != null && dataEntrys.Any())
                            {
                                item.Damage += (double)dataEntrys.Sum(ss => (decimal)ss.Damage);
                                item.Wast += (double)dataEntrys.Sum(ss => (decimal)ss.Wast);
                                item.Others += (double)dataEntrys.Sum(ss => (decimal)ss.Others);
                            }
                        //}

                        //item.DateDisplay = item.Date.ToString("MM/dd/yyyy");
                        //Close balance = (open balance + received + transfer in) - (return + transfer out + damage + wastage + others + Sale)
                        if (item.IsAutoCreated)
                        {
                            item.CloseBal = 0;
                            item.AdjustValue = 0;
                        }
                        else
                            item.AdjustValue = item.AutoCloseBal.HasValue ? Math.Round((item.CloseBal - item.AutoCloseBal.Value), 4) : 0;
                        //item.ManualCloseBal = ((decimal)item.OpenBal + (decimal)item.Received + (decimal)item.TransferIn) 
                        //    - ((decimal)item.Return + (decimal)item.TransferOut + (decimal)item.Damage + (decimal)item.Wast + (decimal)item.Others + (decimal)item.Sales +  item.UseForSelfMade);
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

    
        private void FormatStoreHeader(string storeName, ref IXLWorksheet ws, ref int row)
        {
            int startRow = row;
            ws.Range(row, 1, row, 17).Merge().Value = storeName;
            ws.Range(row, 1, row++, 17).Style.Fill.BackgroundColor = XLColor.FromHtml("#c6efce");

            ws.Range(row, 1, row + 1, 1).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code"));
            ws.Range(row, 2, row + 1, 2).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Name"));
            ws.Range(row, 3, row + 1, 3).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Base UOM"));
            ws.Range(row, 4, row + 1, 4).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type"));
            ws.Range(row, 5, row + 1, 5).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Open Balance"));
            ws.Range(row, 6, row, 7).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Stock In"));
            ws.Cell(row + 1, 6).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Received"));
            ws.Cell(row + 1, 7).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Transfer In"));
            ws.Range(row, 8, row, 13).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Stock Out"));
            ws.Cell(row + 1, 8).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Return"));
            ws.Cell(row + 1, 9).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Transfer Out"));
            ws.Cell(row + 1, 10).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Damage"));
            ws.Cell(row + 1, 11).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Wastage"));
            ws.Cell(row + 1, 12).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Others"));
            ws.Cell(row + 1, 13).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Used for Self Made Ingredient"));
            ws.Cell(row + 1, 13).Style.Alignment.WrapText = true;
            ws.Range(row, 14, row + 1, 14).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sale"));
            ws.Range(row, 15, row + 1, 15).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Close Balance"));
            ws.Range(row, 16, row + 1, 16).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Manual Close Balance"));
            ws.Range(row, 17, row + 1, 17).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Adjust"));

            ws.Range(startRow + 1, 1, startRow + 2, 17).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(startRow + 1, 1, startRow + 2, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRow + 1, 1, startRow + 2, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(startRow, 1, startRow + 2, 17).Style.Font.Bold = true;
            ws.Range(startRow, 1, startRow + 2, 17).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, startRow + 2, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, startRow + 2, 17).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(startRow, 1, startRow + 2, 17).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
            row++;
        }

        public XLWorkbook Report(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Daily_Transactions_Report");
            CreateReportHeaderNew(ws, 17, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Transactions Report").ToUpper());
            ws.Range(1, 1, 4, 17).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 17).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(1, 1, 4, 17).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");

            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            int row = 5;
            string storeName = string.Empty, storeId = string.Empty;
            for (int i = 0; i < lstStore.Count; i++)
            {
                //Get StoreName
                StoreModels store = lstStore[i];
                storeName = store.Name;
                storeId = store.Id;
                //header report
                FormatStoreHeader(storeName, ref ws, ref row);
                row++;
                int startRow = row;
                var businessInStoreId = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).Select(ss=>ss.Id).ToList();
                List<DailyTransactionsReportModels> ingredients = null;
                if (businessInStoreId != null && businessInStoreId.Any())
                {
                    ingredients = GetListIngredientUsage(model, storeId, businessInStoreId);
                }
                
                var listDataSumary = new List<DailyTransactionsReportModels>();

                if (ingredients != null && ingredients.Count > 0)
                {
                    //Get all business day in store
                    var businessInStore = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                    List<DailyTransactionsReportModels> lstDataInDate = null;
                    for (int d = 0; d < businessInStore.Count; d++)
                    {
                        //List<DailyTransactionsReportModels> lstDataInDate = 
                        //    ingredients.Where(m => 
                        //    m.StartedOn >= businessInStore[d].DateFrom 
                        //    && m.ClosedOn <= businessInStore[d].DateTo 

                        //    && m.StoreId == storeId)
                        //    .OrderBy(m => m.TypeName).ThenBy(m => m.IngredientName).ToList();

                        lstDataInDate =
                           ingredients.Where(m =>
                           m.BusinessId == businessInStore[d].Id
                           
                           && m.StoreId == storeId)
                           .OrderBy(m => m.TypeName).ThenBy(m => m.IngredientName).ToList();

                        listDataSumary.AddRange(lstDataInDate);
                        // Business day
                        if (lstDataInDate.Count > 0)
                        {
                            ws.Range(row, 1, row, 17).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Business day") + ": " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Open") + " " + businessInStore[d].DateFrom.ToString("MM/dd/yyyy - HH:mm") + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Close") + " " + businessInStore[d].DateTo.ToString("MM/dd/yyyy - HH:mm"));
                            ws.Range(row, 1, row, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws.Range(row, 1, row++, 17).Style.Font.Bold = true;
                            int rowTbl = row;
                            for (int j = 0; j < lstDataInDate.Count; j++)
                            {
                                ws.Cell("A" + row).Value = "'" + lstDataInDate[j].IngredientCode;
                                ws.Cell("B" + row).Value = lstDataInDate[j].IngredientName;
                                ws.Cell("C" + row).Value = lstDataInDate[j].BaseUOMName;
                                ws.Cell("D" + row).Value = lstDataInDate[j].TypeName;

                                ws.Cell("E" + row).Value = lstDataInDate[j].OpenBal;
                                ws.Cell("F" + row).Value = lstDataInDate[j].Received;
                                ws.Cell("G" + row).Value = lstDataInDate[j].TransferIn;
                                ws.Cell("H" + row).Value = lstDataInDate[j].Return;
                                ws.Cell("I" + row).Value = lstDataInDate[j].TransferOut;
                                ws.Cell("J" + row).Value = lstDataInDate[j].Damage;
                                ws.Cell("K" + row).Value = lstDataInDate[j].Wast;
                                ws.Cell("L" + row).Value = lstDataInDate[j].Others;
                                ws.Cell("M" + row).Value = lstDataInDate[j].UseForSelfMade;

                                ws.Cell("N" + row).Value = lstDataInDate[j].Sales;
                                ws.Cell("O" + row).Value = lstDataInDate[j].AutoCloseBal;//auto
                                ws.Cell("P" + row).Value = lstDataInDate[j].CloseBal;
                                ws.Cell("Q" + row++).Value = lstDataInDate[j].AdjustValue;
                            }
                            ws.Range(rowTbl, 5, row - 1, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws.Range(rowTbl, 5, row - 1, 17).Style.NumberFormat.Format = "#,##0.000";
                        }
                    }
                }
                ws.Range(startRow, 1, row - 1, 17).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startRow, 1, row - 1, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startRow, 1, row - 1, 17).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
                ws.Range(startRow, 1, row - 1, 17).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
                row += 2;
                // Summary
                int rowSum = row;
                ws.Range(row, 1, row, 11).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Summary").ToUpper());
                ws.Range(row, 1, row++, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code");
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Name");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Base UOM");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Open Balance");
                ws.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Purchase");
                ws.Cell("G" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Transfer");
                ws.Cell("H" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Data Entry");
                ws.Cell("I" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Used for Self Made Ingredient");
                ws.Cell("I" + row).Style.Alignment.WrapText = true;
                ws.Cell("J" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sale");
                ws.Cell("K" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Close Balance");
                ws.Range(row - 1, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
                ws.Range(row, 1, row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(row - 1, 1, row++, 11).Style.Font.Bold = true;

                var listSumary = listDataSumary.GroupBy(o => new { o.IngredientCode, o.IngredientName, o.BaseUOMName, o.TypeName })
                    .Select(o => new DailyTransactionsReportModels()
                    {
                        IngredientCode = o.Key.IngredientCode,
                        IngredientName = o.Key.IngredientName,
                        BaseUOMName = o.Key.BaseUOMName,
                        TypeName = o.Key.TypeName
                    }).OrderBy(o => o.TypeName).ThenBy(o => o.IngredientName).ToList();

                decimal openBalance = 0;
                decimal purchase = 0;
                decimal transfer = 0;
                decimal dataEntry = 0;
                decimal sale = 0;
                decimal usaForSelfMade = 0;
                int rowTblSum = row;
                foreach (var data in listSumary)
                {
                    var listChild = listDataSumary.Where(o => o.IngredientCode == data.IngredientCode 
                    && o.IngredientName == data.IngredientName && o.BaseUOMName == data.BaseUOMName).OrderBy(o=>o.StartedOn).ToList();

                    openBalance = listChild.Select(o => (decimal)o.OpenBal).FirstOrDefault();
                    purchase = listChild.Sum(o => (decimal)o.Received -(decimal)o.Return);
                    transfer = listChild.Sum(o => (decimal)o.TransferIn- (decimal)o.TransferOut);
                    dataEntry = listChild.Sum(o => (decimal)o.Damage+ (decimal)o.Wast+ (decimal)o.Others);
                    sale = listChild.Sum(o => (decimal)o.Sales);
                    usaForSelfMade = listChild.Sum(o => (decimal)o.UseForSelfMade);

                    ws.Cell("A" + row).Value = "'" + data.IngredientCode;
                    ws.Cell("B" + row).Value = data.IngredientName;
                    ws.Cell("C" + row).Value = data.BaseUOMName;
                    ws.Cell("D" + row).Value = data.TypeName;

                    ws.Cell("E" + row).Value = openBalance;
                    ws.Cell("F" + row).Value = purchase;
                    ws.Cell("G" + row).Value = transfer;
                    ws.Cell("H" + row).Value = dataEntry;
                    ws.Cell("I" + row).Value = usaForSelfMade;

                    ws.Cell("J" + row).Value = sale;
                    ws.Cell("K" + row++).Value = (openBalance + purchase + transfer - dataEntry - sale - usaForSelfMade);
                }
                ws.Range(rowSum, 1, row - 1, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(rowSum, 1, row - 1, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(rowSum, 1, row - 1, 11).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
                ws.Range(rowSum, 1, row - 1, 11).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
                ws.Range(rowTblSum, 5, row - 1, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(rowTblSum, 5, row - 1, 11).Style.NumberFormat.Format = "#,##0.000";
                row++;

                List<int> lstWidCol = new List<int>() { 20, 20, 20, 20, 18, 18, 18, 18, 30, 18, 18, 18, 30, 18, 20, 25 };
                for (int y = 0; y < lstWidCol.Count; y++)
                {
                    ws.Column(y + 1).Width = lstWidCol[y];
                }
            }
            return wb;
        }
    }
}
