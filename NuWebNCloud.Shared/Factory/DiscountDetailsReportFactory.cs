using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using ClosedXML.Excel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using NuWebNCloud.Data.Models;

namespace NuWebNCloud.Shared.Factory
{
    public class DiscountDetailsReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private ClosedReceiptReportFactory _ClosedReceiptReportFactory = null;
        public DiscountDetailsReportFactory()
        {
            _baseFactory = new BaseFactory();
            _ClosedReceiptReportFactory = new ClosedReceiptReportFactory();

        }
        public bool Insert(List<DiscountDetailsReportModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            //_logger.Info("==========================================================");
            //_logger.Info(string.Format("Insert DiscountDetailsReport: StoreId: [{0}] | BusinessId: [{1}]", info.StoreId, info.BusinessDayId));
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_DiscountDetailsReport.Where(ww => ww.StoreId == info.StoreId && ww.BusinessDayId == info.BusinessDayId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Discount Details data exist");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_DiscountDetailsReport> lstInsert = new List<R_DiscountDetailsReport>();
                        R_DiscountDetailsReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_DiscountDetailsReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.BusinessDayId = item.BusinessDayId;
                            itemInsert.ReceiptId = item.ReceiptId;
                            itemInsert.ReceiptNo = item.ReceiptNo;
                            itemInsert.CashierId = item.CashierId;
                            itemInsert.CashierName = item.CashierName;
                            itemInsert.DiscountId = item.DiscountId;
                            itemInsert.DiscountName = item.DiscountName;
                            itemInsert.ItemId = item.ItemId;
                            itemInsert.ItemCode = item.ItemCode;
                            itemInsert.ItemName = item.ItemName;
                            itemInsert.Qty = item.Qty;
                            itemInsert.ItemPrice = item.ItemPrice;
                            itemInsert.DiscountAmount = item.DiscountAmount;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.DiscountType = item.DiscountType;
                            itemInsert.IsDiscountValue = item.IsDiscountValue;
                            itemInsert.BillTotal = item.BillTotal;
                            itemInsert.Mode = item.Mode;
                            //07/08/2017
                            itemInsert.UserDiscount = item.UserDiscount;
                            itemInsert.PromotionId = item.PromotionId;
                            itemInsert.PromotionName = item.PromotionName;
                            itemInsert.PromotionValue = item.PromotionValue;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_DiscountDetailsReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        //_logger.Info(string.Format("Insert DiscountDetailsReport: StoreId: [{0}] | BusinessId: [{1}] success", info.StoreId, info.BusinessDayId));
                        NSLog.Logger.Info("Insert Discount Details data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Discount Details data fail", ex);
                        //_logger.Error(ex);
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
            //var jsonContent = JsonConvert.SerializeObject(lstInfo);
            //_baseFactory.InsertTrackingLog("R_DiscountDetailsReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<DiscountDetailsReportModels> GetData(BaseReportModel model, string StoreId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DiscountDetailsReport
                               where tb.StoreId == StoreId
                                     && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                                     && ((!string.IsNullOrEmpty(tb.PromotionId) && tb.PromotionValue != 0) || (!string.IsNullOrEmpty(tb.DiscountId) && tb.DiscountAmount != 0))
                                     && tb.Mode == model.Mode
                               select new DiscountDetailsReportModels
                               {
                                   Id = tb.Id,
                                   CashierId = tb.CashierId,
                                   CashierName = tb.CashierName,
                                   CreatedDate = tb.CreatedDate,
                                   DiscountAmount = tb.DiscountAmount,
                                   DiscountId = tb.DiscountId,
                                   DiscountName = tb.DiscountName,
                                   ItemId = tb.ItemId,
                                   ItemCode = tb.ItemCode,
                                   ItemName = tb.ItemName,
                                   ItemPrice = tb.ItemPrice,
                                   Qty = tb.Qty,
                                   ReceiptNo = tb.ReceiptNo,
                                   StoreId = tb.StoreId,
                                   IsDiscountValue = tb.IsDiscountValue,
                                   DiscountType = tb.DiscountType,
                                   BillTotal = tb.BillTotal,

                                   ReceiptId = tb.ReceiptId,
                                   PromotionId = tb.PromotionId,
                                   PromotionName = tb.PromotionName,
                                   PromotionValue = tb.PromotionValue.HasValue ? tb.PromotionValue.Value : 0
                               }).ToList();
                return lstData;
            }
        }

        public List<DiscountDetailsReportModels> GetDataDiscounts(DateTime dateFrom, DateTime dateTo, List<string> lstStoreIds, List<string> lstBusinessInputIds, int mode = 1)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DiscountDetailsReport
                               where lstStoreIds.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= dateFrom && tb.CreatedDate <= dateTo) && tb.Mode == mode
                                     && lstBusinessInputIds.Contains(tb.BusinessDayId)
                               select new DiscountDetailsReportModels
                               {
                                   BusinessDayId = tb.BusinessDayId,
                                   CreatedDate = tb.CreatedDate,
                                   ReceiptId = tb.ReceiptId,
                                   DiscountId = tb.DiscountId,
                                   DiscountName = tb.DiscountName,
                                   DiscountAmount = tb.DiscountAmount,
                                   StoreId = tb.StoreId,
                                   UserDiscount = tb.UserDiscount
                               }).ToList();
                return lstData;
            }
        }

        public List<DiscountAndMiscReportModels> GetDiscountTotal(List<string> lstBusinessIds, int mode = 1)
        {
            List<DiscountAndMiscReportModels> lstData = new List<DiscountAndMiscReportModels>();
            using (var cxt = new NuWebContext())
            {
                var query = (from tb in cxt.R_DiscountDetailsReport
                             where lstBusinessIds.Contains(tb.BusinessDayId)
                              && tb.DiscountType == Commons.DiscountTotalBill
                           && tb.Mode == mode
                             group tb by new
                             {
                                 Hour = (int?)SqlFunctions.DatePart("HH", tb.CreatedDate),
                                 StoreId = tb.StoreId,
                             } into g
                             select g).ToList();
                if (query != null && query.Any())
                {
                    lstData = query.Select(g => new DiscountAndMiscReportModels
                    {
                        StoreId = g.Key.StoreId,
                        Hour = g.Key.Hour.Value,
                        TimeSpanHour = new TimeSpan(g.Key.Hour.Value, 0, 0),
                        DiscountValue = g.Sum(ss => ss.DiscountAmount)
                    }).ToList();
                }
                return lstData;
            }
        }

        /// <summary>
        /// For ItemizedSalesAnalysisReport filter time from input (report old DB)
        /// </summary>
        /// <param name="lstStoreId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="mode"></param>
        /// <param name="dtFromFilter"></param>
        /// <param name="dtToFilter"></param>
        /// <param name="filterType"></param>
        /// <returns></returns>
        public List<DiscountAndMiscReportModels> GetDiscountTotal(List<string> lstStoreId, DateTime fromDate, DateTime toDate, int mode, DateTime dtFromFilter, DateTime dtToFilter, int filterType = 0)
        {
            List<DiscountAndMiscReportModels> lstData = new List<DiscountAndMiscReportModels>();
            using (var cxt = new NuWebContext())
            {
                var query = (from tb in cxt.R_DiscountDetailsReport.AsNoTracking()
                             where lstStoreId.Contains(tb.StoreId)
                              && tb.CreatedDate >= fromDate && tb.CreatedDate <= toDate
                              && tb.Mode == mode
                              && tb.DiscountType == (int)Commons.DiscountTotalBill
                             select new { tb.StoreId, tb.CreatedDate, tb.DiscountAmount }).ToList();
                switch (filterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        query = query.Where(ww => ww.CreatedDate.TimeOfDay >= dtFromFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= dtToFilter.TimeOfDay).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        query = query.Where(ww => ww.CreatedDate.TimeOfDay >= dtFromFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= dtToFilter.TimeOfDay).ToList();
                        break;
                }
                if (query != null && query.Any())
                {
                    lstData = query.GroupBy(gg => new
                    {
                        Hour = gg.CreatedDate.Hour,
                        StoreId = gg.StoreId,
                    })
                            .Select(g => new DiscountAndMiscReportModels
                            {
                                StoreId = g.Key.StoreId,
                                Hour = g.Key.Hour,
                                TimeSpanHour = new TimeSpan(g.Key.Hour, 0, 0),
                                DiscountValue = g.Sum(ss => ss.DiscountAmount)
                            }).ToList();
                }
                return lstData;
            }
        }

        public List<DiscountAndMiscReportModels> GetDiscountTotal(List<string> lstStore, DateTime dFrom, DateTime dTo, int mode = 1)
        {
            List<DiscountAndMiscReportModels> lstData = new List<DiscountAndMiscReportModels>();
            using (var cxt = new NuWebContext())
            {
                var query = (from tb in cxt.R_DiscountDetailsReport
                             where tb.CreatedDate >= dFrom && tb.CreatedDate <= dTo
                              && tb.DiscountType == Commons.DiscountTotalBill
                              && lstStore.Contains(tb.StoreId)
                           && tb.Mode == mode
                             group tb by new
                             {
                                 Hour = (int?)SqlFunctions.DatePart("HH", tb.CreatedDate),
                                 StoreId = tb.StoreId,
                             } into g
                             select g).ToList();
                if (query != null && query.Any())
                {
                    lstData = query.Select(g => new DiscountAndMiscReportModels
                    {
                        StoreId = g.Key.StoreId,
                        Hour = g.Key.Hour.Value,
                        TimeSpanHour = new TimeSpan(g.Key.Hour.Value, 0, 0),
                        DiscountValue = g.Sum(ss => ss.DiscountAmount)
                    }).ToList();
                }
                return lstData;
            }
        }

        #region For Report
        private void FormatStoreHeader(string storeName, ref IXLWorksheet ws, ref int row)
        {
            int startRow = row;
            ws.Range(row, 1, row, 11).Merge().Value = storeName;
            ws.Range(row, 1, row++, 11).Style.Font.Bold = true;

            ws.Range(row, 1, row + 1, 1).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
            ws.Range(row, 2, row + 1, 2).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
            ws.Range(row, 3, row + 1, 3).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No"));
            ws.Range(row, 4, row + 1, 4).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
            ws.Range(row, 5, row + 1, 5).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cashier"));
            //ws.Range(row, 5, row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ws.Cell(row + 1, 5).SetValue("ID");
            //ws.Cell(row + 1, 6).SetValue("Name");
            ws.Range(row, 6, row, 7).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item"));
            ws.Range(row, 6, row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row + 1, 6).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Code"));
            ws.Cell(row + 1, 7).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"));
            ws.Range(row, 8, row + 1, 8).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Range(row, 9, row + 1, 9).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
            ws.Range(row, 10, row, 11).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
            ws.Range(row, 10, row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row + 1, 10).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"));
            ws.Cell(row + 1, 11).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
            ws.Range(row, 1, row + 1, 11).Style.Font.Bold = true;

            row++;
            ws.Range(startRow, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(startRow, 1, row, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row, 11).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(startRow, 1, row++, 11).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");

            List<int> lstWidCol = new List<int>() { 30, 20, 15, 8, 20, 15, 20, 10, 15, 25, 15 };
            for (int i = 0; i < lstWidCol.Count; i++)
            {
                ws.Column(i + 1).Width = lstWidCol[i];
            }
        }

        private void FormatSummaryHeader(ref IXLWorksheet ws, ref int row)
        {
            ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Summary");
            ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount");
            ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Count");
            ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount");
            ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("% of Sales");

            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(row, 1, row, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 5).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(row, 1, row, 5).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");

            row++;
        }

        private void FormatStoreData(ref IXLWorksheet ws, int startRow, int endRow, bool isSumRow = false)
        {
            if (!isSumRow)
            {
                ws.Range(startRow, 4, endRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(startRow, 7, endRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(startRow, 9, endRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(startRow, 9, endRow, 9).Style.NumberFormat.Format = "#,##0.00";
                ws.Range(startRow, 11, endRow, 11).Style.NumberFormat.Format = "#,##0.00";
            }
            else
            {
                ws.Range(startRow, 9, endRow, 11).Style.NumberFormat.Format = "#,##0.00";
                ws.Range(startRow, 1, endRow, 11).Style.Font.Bold = true;
                ws.Range(startRow, 1, endRow, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            }
            ws.Range(startRow, 1, endRow, 1).Style.DateFormat.Format = "MM/dd/yyyy";
            ws.Range(startRow, 2, endRow, 2).Style.DateFormat.Format = "hh:mm AM/PM";
            ws.Range(startRow, 1, endRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(startRow, 1, endRow, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, endRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, endRow, 11).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(startRow, 1, endRow, 11).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
        }

        private void FormatSummaryData(ref IXLWorksheet ws, int startRow, int endRow)
        {
            ws.Range(startRow, 1, endRow, 1).Style.DateFormat.Format = "mm/dd/yyyy";
            ws.Range(startRow, 1, endRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(startRow, 4, endRow, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(endRow, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(endRow, 1, endRow, 5).Style.Font.Bold = true;
            ws.Range(endRow, 1, endRow, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(startRow, 1, endRow, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, endRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, endRow, 5).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(startRow, 1, endRow, 5).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
        }

        private void FormatTotalData(ref IXLWorksheet ws, int row)
        {
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 4, row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(row, 1, row, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 5).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(row, 1, row, 5).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
        }

        public XLWorkbook Report_current(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Discount_Details"/*_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount_Details")*/);
            CreateReportHeaderNew(ws, 11, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Details Report"));

            // Format header report
            ws.Range(1, 1, 4, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var dFrom = model.FromDate;
            var dTo = model.ToDate;
            //int rowEmpty = 5;
            //Get all business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Count > 0)
            {
                dFrom = _lstBusDayAllStore.Min(ss => ss.DateFrom);
                dTo = _lstBusDayAllStore.Max(ss => ss.DateTo);
                // Get data use business day
                //model.FromDate = dFrom;
                model.ToDate = dTo;

                int row = 5;
                double grandReceiptCount = 0;
                double grandSaleTotal = 0;
                double grandDiscountAmout = 0;

                for (int i = 0; i < lstStore.Count; i++)
                {
                    //Get StoreName
                    StoreModels store = lstStore[i];
                    string storeId = store.Id;
                    FormatStoreHeader(store.Name, ref ws, ref row);

                    DiscountDetailsReportFactory factory = new DiscountDetailsReportFactory();
                    List<DiscountDetailsReportModels> data = factory.GetData(model, storeId);
                    List<DiscountDetailsReportDetailModels> discountGroups = (from d in data
                                                                              group d by new
                                                                              {
                                                                                  DiscountId = d.DiscountId,
                                                                                  DiscountName = d.DiscountName,
                                                                                  CreatedDate = d.CreatedDate.Date
                                                                              } into g
                                                                              select new DiscountDetailsReportDetailModels
                                                                              {
                                                                                  DiscountId = g.Key.DiscountId,
                                                                                  DiscountName = g.Key.DiscountName,
                                                                                  CreatedDate = g.Key.CreatedDate,
                                                                                  Count = g.Select(x => x.ReceiptNo).Distinct().Count(),
                                                                                  Amount = g.Sum(x => x.ItemPrice * x.Qty),
                                                                                  Discount = 0
                                                                              }).ToList();

                    var listReceiptId = data.Select(x => x.ReceiptId).Distinct().ToList();
                    var listPax = _ClosedReceiptReportFactory.GetListNoOfPersion(listReceiptId);
                    for (int j = 0; j < discountGroups.Count; j++)
                    {
                        int startRow = row;
                        discountGroups[j].Discount = 0;
                        List<DiscountDetailsReportModels> reportItems = (from tb in data
                                                                         where tb.DiscountId == discountGroups[j].DiscountId
                                                                          && tb.CreatedDate.Date == discountGroups[j].CreatedDate
                                                                         orderby tb.DiscountType descending
                                                                         orderby tb.ReceiptNo
                                                                         select new DiscountDetailsReportModels
                                                                         {
                                                                             Id = tb.Id,
                                                                             CashierId = tb.CashierId,
                                                                             CashierName = tb.CashierName,
                                                                             CreatedDate = tb.CreatedDate,
                                                                             DiscountAmount = tb.DiscountAmount,
                                                                             DiscountId = tb.DiscountId,
                                                                             DiscountName = tb.DiscountName,
                                                                             ItemCode = tb.ItemCode,
                                                                             ItemName = tb.ItemName,
                                                                             ItemPrice = tb.ItemPrice,
                                                                             Qty = tb.Qty,
                                                                             ReceiptNo = tb.ReceiptNo,
                                                                             StoreId = tb.StoreId,
                                                                             IsDiscountValue = tb.IsDiscountValue,
                                                                             DiscountType = tb.DiscountType,
                                                                             BillTotal = tb.BillTotal,
                                                                             ReceiptId = tb.ReceiptId
                                                                         }).ToList();
                        for (int k = 0; k < reportItems.Count; k++)
                        {
                            double discountAmount = 0;
                            ws.Cell("A" + row).Value = reportItems[k].CreatedDate.Date;
                            ws.Cell("B" + row).Value = reportItems[k].CreatedDate.TimeOfDay;
                            ws.Cell("C" + row).Value = reportItems[k].ReceiptNo;
                            ws.Cell("D" + row).Value = 0;
                            //ws.Cell("E" + row).Value = reportItems[k].CashierId;
                            ws.Cell("E" + row).Value = reportItems[k].CashierName;
                            ws.Cell("F" + row).Value = "'" + reportItems[k].ItemCode;
                            ws.Cell("G" + row).Value = reportItems[k].ItemName;
                            ws.Cell("H" + row).Value = reportItems[k].Qty;
                            ws.Cell("I" + row).Value = reportItems[k].ItemPrice * reportItems[k].Qty; //.ItemAmount
                            ws.Cell("J" + row).Value = reportItems[k].DiscountName;

                            discountAmount = reportItems[k].DiscountAmount;
                            if (reportItems[k].DiscountType == Commons.DiscountTotalBill)
                            {
                                double itemTotalAmount = reportItems[k].ItemPrice * reportItems[k].Qty;
                                // get another item details in the same receipt with current item and have same total bill discount 
                                List<DiscountDetailsReportModels> receiptDetailItems = reportItems.Where(r => r.ReceiptNo == reportItems[k].ReceiptNo
                                    && r.DiscountType == Commons.DiscountTotalBill && r.Id != reportItems[k].Id).ToList(); /*&& r.ReceiptDetailID != reportItems[k].ReceiptDetailID*/
                                if (receiptDetailItems.Count > 0)
                                {
                                    for (int l = 0; l < receiptDetailItems.Count; l++)
                                    {
                                        ws.Cell("A" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.Date;
                                        ws.Cell("B" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.TimeOfDay;
                                        ws.Cell("C" + (row + l + 1)).Value = receiptDetailItems[l].ReceiptNo;

                                        var objClosedReceipt = listPax.Where(x => x.OrderId.Equals(receiptDetailItems[l].ReceiptId)).FirstOrDefault();
                                        ws.Cell("D" + (row + l + 1)).Value = objClosedReceipt == null ? 0 : objClosedReceipt.NoOfPersion;

                                        //ws.Cell("E" + (row + l + 1)).Value = receiptDetailItems[l].CashierId;
                                        ws.Cell("E" + (row + l + 1)).Value = receiptDetailItems[l].CashierName;
                                        ws.Cell("F" + (row + l + 1)).Value = "'" + receiptDetailItems[l].ItemCode;
                                        ws.Cell("G" + (row + l + 1)).Value = receiptDetailItems[l].ItemName;
                                        ws.Cell("H" + (row + l + 1)).Value = receiptDetailItems[l].Qty;
                                        ws.Cell("I" + (row + l + 1)).Value = receiptDetailItems[l].ItemPrice * receiptDetailItems[l].Qty; //.ItemAmount
                                        ws.Cell("J" + (row + l + 1)).Value = receiptDetailItems[l].DiscountName;

                                        discountAmount += receiptDetailItems[l].DiscountAmount;
                                    }
                                    itemTotalAmount += receiptDetailItems.Sum(r => r.ItemPrice * r.Qty); //.ItemAmount
                                }

                                //if (reportItems[k].IsDiscountValue == Commons.DiscountValueType_Percent)
                                //    discountAmount = (double)Math.Abs(RoundingHelper.RoundUp2Decimal(-(decimal)(itemTotalAmount * reportItems[k].DiscountAmount / 100)));

                                ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Merge().Value = discountAmount;
                                ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                row += receiptDetailItems.Count;
                                k += receiptDetailItems.Count;
                                discountGroups[j].Discount += discountAmount;
                            }
                            else
                            {
                                //discountAmount = (double)Math.Abs(RoundingHelper.RoundUp2Decimal(-(decimal)reportItems[k].DiscountAmount));
                                ws.Cell("K" + row).Value = discountAmount;
                                discountGroups[j].Discount += discountAmount;
                            }
                            row++;
                        }
                        FormatStoreData(ref ws, startRow, row - 1);
                        //discountGroups[j].Discount = (double)Math.Abs((RoundingHelper.RoundUp2Decimal(-(decimal)discountGroups[j].Discount)));

                        ws.Cell("A" + row).Value = discountGroups[j].CreatedDate.Date;
                        ws.Cell("B" + row).Value = discountGroups[j].DiscountName;
                        ws.Cell("C" + row).Value = discountGroups[j].Count;
                        ws.Cell("I" + row).Value = discountGroups[j].Amount;
                        ws.Cell("K" + row).Value = discountGroups[j].Discount;
                        FormatStoreData(ref ws, row, row, true);
                        row += 2;
                    }
                    row++;
                    FormatSummaryHeader(ref ws, ref row);
                    List<DateTime> dates = discountGroups.OrderBy(g => g.CreatedDate.Date).Select(g => g.CreatedDate.Date).Distinct().ToList();
                    //var lstReceipt = data.Select(ss => new { ReceiptId = ss.ReceiptId, ReceiptDate = ss.CreatedDate, BillTotal = ss.BillTotal }).Distinct();
                    for (int j = 0; j < dates.Count; j++)
                    {
                        int startRow = row;
                        double totalSales = GetTotalSales(storeId, dates[j]);
                        List<DiscountDetailsReportDetailModels> groups = discountGroups.Where(g => g.CreatedDate.Date == dates[j].Date).OrderBy(x => x.DiscountName).ToList();
                        for (int k = 0; k < groups.Count; k++)
                        {
                            ws.Cell("A" + row).Value = dates[j].Date;
                            ws.Cell("B" + row).Value = groups[k].DiscountName;
                            ws.Cell("C" + row).Value = groups[k].Count;
                            ws.Cell("D" + row).Value = groups[k].Discount;
                            if (totalSales == 0)
                            {
                                ws.Cell("E" + row).Value = 100;
                            }
                            else
                            {
                                ws.Cell("E" + row).Value = groups[k].Discount / totalSales * 100;
                            }
                            row++;
                        }
                        ws.Cell("A" + row).Value = dates[j].Date;
                        ws.Cell("B" + row).Value = totalSales;
                        ws.Cell("C" + row).Value = groups.Sum(g => g.Count);
                        ws.Cell("D" + row).Value = groups.Sum(g => g.Discount);
                        if (totalSales == 0)
                        {
                            ws.Cell("E" + row).Value = 100;
                        }
                        else
                        {
                            ws.Cell("E" + row).Value = groups.Sum(g => g.Discount) / totalSales * 100;
                        }

                        FormatSummaryData(ref ws, startRow, row);
                        row += 2;
                    }
                    row -= 1;
                    double storeSaleTotal = GetStoreTotalSales(storeId, dFrom, dTo);
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store") + ": " + store.Name;
                    ws.Cell("B" + row).Value = storeSaleTotal;
                    ws.Cell("C" + row).Value = discountGroups.Sum(g => g.Count);
                    ws.Cell("D" + row).Value = discountGroups.Sum(g => g.Discount);
                    if (storeSaleTotal == 0)
                    {
                        ws.Cell("E" + row).Value = 100;
                    }
                    else
                    {
                        ws.Cell("E" + row).Value = discountGroups.Sum(g => g.Discount) / storeSaleTotal * 100;
                    }
                    FormatTotalData(ref ws, row);

                    grandSaleTotal += storeSaleTotal;
                    grandReceiptCount += discountGroups.Sum(g => g.Count);
                    grandDiscountAmout += discountGroups.Sum(g => g.Discount);
                    row += 3;
                }

                row -= 2;
                //ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Grand Total");
                ws.Cell("A" + row).Value = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL").ToLower());
                ws.Cell("B" + row).Value = grandSaleTotal;
                ws.Cell("C" + row).Value = grandReceiptCount;
                ws.Cell("D" + row).Value = grandDiscountAmout;
                if (grandSaleTotal == 0)
                {
                    ws.Cell("E" + row).Value = 100;
                }
                else
                {
                    ws.Cell("E" + row).Value = grandDiscountAmout / grandSaleTotal * 100;
                }
                FormatTotalData(ref ws, row);

                //ws.Columns().AdjustToContents();
            }
            else //no data
            {
                //FormatStoreHeader("", ref ws, ref rowEmpty);
                //FormatSummaryHeader(ref ws, ref rowEmpty);
            }
            return wb;
        }

        /// <summary>
        /// 2018-05-02 add promotion
        /// </summary>
        /// <param name="model"></param>
        /// <param name="lstStore"></param>
        /// <returns></returns>
        public XLWorkbook Report(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Discount_Details");
            CreateReportHeaderNew(ws, 11, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Details Report"));

            // Format header report
            ws.Range(1, 1, 4, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            //Get all business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Count > 0)
            {

                // Get data use business day
                model.FromDate = _lstBusDayAllStore.Min(ss => ss.DateFrom);
                model.ToDate = _lstBusDayAllStore.Max(ss => ss.DateTo);

                int row = 5;
                double grandCount = 0;
                double grandSaleTotal = 0;
                double grandDiscountAmout = 0;

                double countByStore = 0;
                double saleTotalByStore = 0;
                double discAndProAmountByStore = 0;
                //get data
                List<DiscountDetailsReportDataModels> lstData = new List<DiscountDetailsReportDataModels>();
                var _lstPax = new List<ClosedReceiptReportDataModels>();
                using (var db = new NuWebContext())
                {
                    lstData = db.GetDataForDiscountDetail(new BaseReportDataModel() { ListStores = model.ListStores
                        , FromDate = model.FromDate, ToDate = model.ToDate, Mode = model.Mode });

                    _lstPax = db.GetDataForCloseReceiptReport(new BaseReportDataModel()
                    {
                        ListStores = model.ListStores
                        ,
                        FromDate = model.FromDate,
                        ToDate = model.ToDate,
                        Mode = model.Mode
                    });
                }
                for (int i = 0; i < lstStore.Count; i++)
                {
                    countByStore = 0;
                    saleTotalByStore = 0;
                    discAndProAmountByStore = 0;

                    //Get StoreName
                    StoreModels store = lstStore[i];
                    string storeId = store.Id;
                    FormatStoreHeader(store.Name, ref ws, ref row);

                    //List<DiscountDetailsReportModels> data = GetData(model, storeId);
                    var data = lstData.Where(ww => ww.StoreId == storeId).ToList();

                    var listReceiptId = data.Select(x => x.ReceiptId).Distinct().ToList();
                    //var listPax = _ClosedReceiptReportFactory.GetListNoOfPersion(listReceiptId);
                    var listPax = _lstPax.Where(ww => listReceiptId.Contains(ww.OrderId) && ww.StoreId == storeId).ToList();

                    var lstDate = data.OrderBy(oo => oo.CreatedDate.Date).Select(ss => ss.CreatedDate.Date).Distinct().ToList();

                    double discountAmount = 0;
                    double promotionAmount = 0;
                    int startTotalRow = 0;
                    int startSummaryProRow = 0;
                    for (int j = 0; j < lstDate.Count; j++)
                    {
                        int startRow = row;

                        #region Discount
                        var discountGroups = (from d in data
                                              where d.CreatedDate.Date == lstDate[j] && d.DiscountAmount != 0
                                              group d by new
                                              {
                                                  DiscountId = d.DiscountId,
                                                  DiscountName = d.DiscountName
                                              } into g
                                              orderby g.Key.DiscountName

                                              select new DiscountDetailsReportDetailModels
                                              {
                                                  DiscountId = g.Key.DiscountId,
                                                  DiscountName = g.Key.DiscountName,
                                                  Count = g.Select(x => x.ReceiptNo).Distinct().Count(),
                                                  Amount = g.Sum(x => x.ItemPrice * x.Qty),
                                                  Discount = 0
                                              }).ToList();
                        for (int h = 0; h < discountGroups.Count; h++)
                        {


                            List<DiscountDetailsReportModels> reportItemsGroup = (from tb in data
                                                                                  where tb.CreatedDate.Date == lstDate[j]
                                                                                  && tb.DiscountId == discountGroups[h].DiscountId
                                                                                  orderby tb.DiscountName
                                                                                  orderby tb.ReceiptNo
                                                                                  select new DiscountDetailsReportModels
                                                                                  {
                                                                                      Id = tb.Id,
                                                                                      CashierId = tb.CashierId,
                                                                                      CashierName = tb.CashierName,
                                                                                      CreatedDate = tb.CreatedDate,
                                                                                      DiscountAmount = tb.DiscountAmount,
                                                                                      DiscountId = tb.DiscountId,
                                                                                      DiscountName = tb.DiscountName,
                                                                                      ItemCode = tb.ItemCode,
                                                                                      ItemName = tb.ItemName,
                                                                                      ItemPrice = tb.ItemPrice,
                                                                                      Qty = tb.Qty,
                                                                                      ReceiptNo = tb.ReceiptNo,
                                                                                      StoreId = tb.StoreId,
                                                                                      IsDiscountValue = tb.IsDiscountValue,
                                                                                      DiscountType = tb.DiscountType,
                                                                                      BillTotal = tb.BillTotal,
                                                                                      ReceiptId = tb.ReceiptId
                                                                                  }).ToList();
                            for (int k = 0; k < reportItemsGroup.Count; k++)
                            {
                                discountAmount = 0;

                                ws.Cell("A" + row).Value = reportItemsGroup[k].CreatedDate.Date;
                                ws.Cell("B" + row).Value = reportItemsGroup[k].CreatedDate.TimeOfDay;
                                ws.Cell("C" + row).Value = reportItemsGroup[k].ReceiptNo;
                                ws.Cell("D" + row).Value = 0;
                                ws.Cell("E" + row).Value = reportItemsGroup[k].CashierName;
                                ws.Cell("F" + row).Value = "'" + reportItemsGroup[k].ItemCode;
                                ws.Cell("G" + row).Value = reportItemsGroup[k].ItemName;
                                ws.Cell("H" + row).Value = reportItemsGroup[k].Qty;
                                ws.Cell("I" + row).Value = reportItemsGroup[k].ItemPrice * reportItemsGroup[k].Qty; //.ItemAmount
                                ws.Cell("J" + row).Value = reportItemsGroup[k].DiscountName;

                                discountAmount = reportItemsGroup[k].DiscountAmount;
                                if (reportItemsGroup[k].DiscountType == Commons.DiscountTotalBill)
                                {
                                    double itemTotalAmount = reportItemsGroup[k].ItemPrice * reportItemsGroup[k].Qty;
                                    // get another item details in the same receipt with current item and have same total bill discount 
                                    List<DiscountDetailsReportModels> receiptDetailItems = reportItemsGroup.Where(r => r.ReceiptNo == reportItemsGroup[k].ReceiptNo
                                        && r.DiscountType == Commons.DiscountTotalBill && r.Id != reportItemsGroup[k].Id).ToList();
                                    if (receiptDetailItems.Count > 0)
                                    {
                                        for (int l = 0; l < receiptDetailItems.Count; l++)
                                        {
                                            ws.Cell("A" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.Date;
                                            ws.Cell("B" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.TimeOfDay;
                                            ws.Cell("C" + (row + l + 1)).Value = receiptDetailItems[l].ReceiptNo;

                                            var objClosedReceipt = listPax.Where(x => x.OrderId.Equals(receiptDetailItems[l].ReceiptId)).FirstOrDefault();
                                            ws.Cell("D" + (row + l + 1)).Value = objClosedReceipt == null ? 0 : objClosedReceipt.NoOfPersion;

                                            //ws.Cell("E" + (row + l + 1)).Value = receiptDetailItems[l].CashierId;
                                            ws.Cell("E" + (row + l + 1)).Value = receiptDetailItems[l].CashierName;
                                            ws.Cell("F" + (row + l + 1)).Value = "'" + receiptDetailItems[l].ItemCode;
                                            ws.Cell("G" + (row + l + 1)).Value = receiptDetailItems[l].ItemName;
                                            ws.Cell("H" + (row + l + 1)).Value = receiptDetailItems[l].Qty;
                                            ws.Cell("I" + (row + l + 1)).Value = receiptDetailItems[l].ItemPrice * receiptDetailItems[l].Qty; //.ItemAmount
                                            ws.Cell("J" + (row + l + 1)).Value = receiptDetailItems[l].DiscountName;

                                            discountAmount += receiptDetailItems[l].DiscountAmount;
                                        }
                                        itemTotalAmount += receiptDetailItems.Sum(r => r.ItemPrice * r.Qty); //.ItemAmount
                                    }

                                    ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Merge().Value = discountAmount;
                                    ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                    row += receiptDetailItems.Count;
                                    k += receiptDetailItems.Count;
                                    discountGroups[h].Discount += discountAmount;
                                }
                                else
                                {
                                    ws.Cell("K" + row).Value = discountAmount;
                                    discountGroups[h].Discount += discountAmount;
                                }
                                row++;
                            }//end  for (int k = 0; k < reportItemsGroup.Count; k++)
                            //group follow discount
                            FormatStoreData(ref ws, startRow, row - 1);
                            ws.Cell("A" + row).Value = lstDate[j];
                            ws.Cell("B" + row).Value = discountGroups[h].DiscountName;
                            ws.Cell("C" + row).Value = discountGroups[h].Count;
                            ws.Cell("I" + row).Value = discountGroups[h].Amount;
                            ws.Cell("K" + row).Value = discountGroups[h].Discount;
                            FormatStoreData(ref ws, row, row, true);
                            row += 2;
                        }//end for (int h = 0; h < discountGroups.Count; h++)
                        #endregion End Discount

                        #region Promotion
                        List<DiscountDetailsReportDetailModels> promotionGroups = (from d in data
                                                                                   where d.CreatedDate.Date == lstDate[j] && d.PromotionValue != 0
                                                                                   group d by new
                                                                                   {
                                                                                       PromotionId = d.PromotionId,
                                                                                       PromotionName = d.PromotionName
                                                                                   } into g
                                                                                   orderby g.Key.PromotionName
                                                                                   select new DiscountDetailsReportDetailModels
                                                                                   {
                                                                                       PromotionId = g.Key.PromotionId,
                                                                                       PromotionName = g.Key.PromotionName,
                                                                                       Count = g.Select(x => x.ReceiptNo).Distinct().Count(),
                                                                                       Amount = g.Sum(x => x.ItemPrice * x.Qty),
                                                                                       PromotionValue = 0
                                                                                   }).ToList();
                        for (int h = 0; h < promotionGroups.Count; h++)
                        {
                            List<DiscountDetailsReportModels> reportPromotionItems = (from tb in data
                                                                                      where tb.CreatedDate.Date == lstDate[j]
                                                                                      && tb.PromotionId == promotionGroups[h].PromotionId
                                                                                      orderby tb.PromotionName
                                                                                      orderby tb.ReceiptNo
                                                                                      select new DiscountDetailsReportModels
                                                                                      {
                                                                                          Id = tb.Id,
                                                                                          CashierId = tb.CashierId,
                                                                                          CashierName = tb.CashierName,
                                                                                          CreatedDate = tb.CreatedDate,
                                                                                          ItemCode = tb.ItemCode,
                                                                                          ItemName = tb.ItemName,
                                                                                          ItemPrice = tb.ItemPrice,
                                                                                          Qty = tb.Qty,
                                                                                          ReceiptNo = tb.ReceiptNo,
                                                                                          StoreId = tb.StoreId,
                                                                                          BillTotal = tb.BillTotal,
                                                                                          ReceiptId = tb.ReceiptId,

                                                                                          PromotionValue = tb.PromotionValue,
                                                                                          PromotionId = tb.PromotionId,
                                                                                          PromotionName = tb.PromotionName,
                                                                                          PromotionType = tb.PromotionType,
                                                                                      }).ToList();

                            for (int k = 0; k < reportPromotionItems.Count; k++)
                            {
                                promotionAmount = 0;

                                ws.Cell("A" + row).Value = reportPromotionItems[k].CreatedDate.Date;
                                ws.Cell("B" + row).Value = reportPromotionItems[k].CreatedDate.TimeOfDay;
                                ws.Cell("C" + row).Value = reportPromotionItems[k].ReceiptNo;
                                ws.Cell("D" + row).Value = 0;
                                ws.Cell("E" + row).Value = reportPromotionItems[k].CashierName;
                                ws.Cell("F" + row).Value = "'" + reportPromotionItems[k].ItemCode;
                                ws.Cell("G" + row).Value = reportPromotionItems[k].ItemName;
                                ws.Cell("H" + row).Value = reportPromotionItems[k].Qty;
                                ws.Cell("I" + row).Value = reportPromotionItems[k].ItemPrice * reportPromotionItems[k].Qty; //.ItemAmount
                                ws.Cell("J" + row).Value = reportPromotionItems[k].PromotionName;

                                promotionAmount = reportPromotionItems[k].PromotionValue;
                                if (reportPromotionItems[k].PromotionType == Commons.PromotionTotalBill)
                                {
                                    double itemTotalAmount = reportPromotionItems[k].ItemPrice * reportPromotionItems[k].Qty;
                                    // get another item details in the same receipt with current item and have same total bill discount 
                                    List<DiscountDetailsReportModels> receiptDetailItems = reportPromotionItems.Where(r => r.ReceiptNo == reportPromotionItems[k].ReceiptNo
                                        && r.PromotionType == Commons.PromotionTotalBill && r.Id != reportPromotionItems[k].Id).ToList();
                                    if (receiptDetailItems.Count > 0)
                                    {
                                        for (int l = 0; l < receiptDetailItems.Count; l++)
                                        {
                                            ws.Cell("A" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.Date;
                                            ws.Cell("B" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.TimeOfDay;
                                            ws.Cell("C" + (row + l + 1)).Value = receiptDetailItems[l].ReceiptNo;

                                            var objClosedReceipt = listPax.Where(x => x.OrderId.Equals(receiptDetailItems[l].ReceiptId)).FirstOrDefault();
                                            ws.Cell("D" + (row + l + 1)).Value = objClosedReceipt == null ? 0 : objClosedReceipt.NoOfPersion;

                                            //ws.Cell("E" + (row + l + 1)).Value = receiptDetailItems[l].CashierId;
                                            ws.Cell("E" + (row + l + 1)).Value = receiptDetailItems[l].CashierName;
                                            ws.Cell("F" + (row + l + 1)).Value = "'" + receiptDetailItems[l].ItemCode;
                                            ws.Cell("G" + (row + l + 1)).Value = receiptDetailItems[l].ItemName;
                                            ws.Cell("H" + (row + l + 1)).Value = receiptDetailItems[l].Qty;
                                            ws.Cell("I" + (row + l + 1)).Value = receiptDetailItems[l].ItemPrice * receiptDetailItems[l].Qty; //.ItemAmount
                                            ws.Cell("J" + (row + l + 1)).Value = receiptDetailItems[l].PromotionName;

                                            promotionAmount += receiptDetailItems[l].PromotionValue;
                                        }
                                        itemTotalAmount += receiptDetailItems.Sum(r => r.ItemPrice * r.Qty); //.ItemAmount
                                    }

                                    ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Merge().Value = promotionAmount;
                                    ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                    row += receiptDetailItems.Count;
                                    k += receiptDetailItems.Count;
                                    promotionGroups[h].PromotionValue += promotionAmount;
                                }
                                else
                                {
                                    ws.Cell("K" + row).Value = promotionAmount;
                                    promotionGroups[h].PromotionValue += promotionAmount;
                                }
                                row++;
                            }

                            FormatStoreData(ref ws, startRow, row - 1);
                            ws.Cell("A" + row).Value = lstDate[j];
                            ws.Cell("B" + row).Value = promotionGroups[h].PromotionName;
                            ws.Cell("C" + row).Value = promotionGroups[h].Count;
                            ws.Cell("I" + row).Value = promotionGroups[h].Amount;
                            ws.Cell("K" + row).Value = promotionGroups[h].PromotionValue;
                            FormatStoreData(ref ws, row, row, true);
                            row += 2;
                        }
                        #endregion End Promotion

                    }//end for (int j = 0; j < lstDate.Count; j++)
                     ////Summary
                    row++;
                    FormatSummaryHeader(ref ws, ref row);
                    for (int j = 0; j < lstDate.Count; j++)
                    {
                        startTotalRow = row;
                        double totalSales = GetTotalSales(storeId, lstDate[j]);
                        saleTotalByStore += totalSales;
                        grandSaleTotal += totalSales;

                        var discountGroups = (from d in data
                                              where d.CreatedDate.Date == lstDate[j] && d.DiscountAmount != 0
                                              group d by new
                                              {
                                                  DiscountId = d.DiscountId,
                                                  DiscountName = d.DiscountName
                                              } into g
                                              orderby g.Key.DiscountName
                                              select new DiscountDetailsReportDetailModels
                                              {
                                                  DiscountId = g.Key.DiscountId,
                                                  DiscountName = g.Key.DiscountName,
                                                  Count = g.Select(x => x.ReceiptNo).Distinct().Count(),
                                                  Amount = g.Sum(x => x.ItemPrice * x.Qty),
                                                  Discount = g.Sum(x=>x.DiscountAmount), 
                                                  CreatedDate = g.FirstOrDefault().CreatedDate.Date
                                              }).ToList();
                        if (discountGroups != null && discountGroups.Any())
                        {
                            var groups = discountGroups.Where(g => g.CreatedDate.Date == lstDate[j].Date)
                                .OrderBy(x => x.DiscountName).ToList();
                            for (int k = 0; k < groups.Count; k++)
                            {
                                countByStore += groups[k].Count;
                                discAndProAmountByStore += groups[k].Discount;

                                grandCount += groups[k].Count;
                                grandDiscountAmout += groups[k].Discount;

                                ws.Cell("A" + row).Value = lstDate[j].Date;
                                ws.Cell("B" + row).Value = groups[k].DiscountName;
                                ws.Cell("C" + row).Value = groups[k].Count;
                                ws.Cell("D" + row).Value = groups[k].Discount;
                                if (totalSales == 0)
                                {
                                    if (groups[k].Discount != 0)
                                    {
                                        ws.Cell("E" + row).Value = 100;
                                    } else
                                    {
                                        ws.Cell("E" + row).Value = 0;
                                    }
                                }
                                else
                                {
                                    ws.Cell("E" + row).Value = groups[k].Discount / totalSales * 100;
                                }
                                row++;
                            }
                            ws.Cell("A" + row).Value = lstDate[j].Date;
                            ws.Cell("B" + row).Value = totalSales;
                            ws.Cell("C" + row).Value = groups.Sum(g => g.Count);
                            ws.Cell("D" + row).Value = groups.Sum(g => g.Discount);
                            if (totalSales == 0)
                            {
                                if (groups.Sum(g => g.Discount) != 0)
                                {
                                    ws.Cell("E" + row).Value = 100;
                                } else
                                {
                                    ws.Cell("E" + row).Value = 0;
                                }
                            }
                            else
                            {
                                ws.Cell("E" + row).Value = groups.Sum(g => g.Discount) / totalSales * 100;
                            }

                            FormatSummaryData(ref ws, startTotalRow, row);
                        }
                        //Promotion
                        row++;
                        startSummaryProRow = row;
                        #region Promotion-summary
                        var promotionGroups = (from d in data
                                               where d.CreatedDate.Date == lstDate[j] && d.PromotionValue != 0
                                               group d by new
                                               {
                                                   PromotionId = d.PromotionId,
                                                   PromotionName = d.PromotionName
                                               } into g
                                               orderby g.Key.PromotionName
                                               select new DiscountDetailsReportDetailModels
                                               {
                                                   PromotionId = g.Key.PromotionId,
                                                   PromotionName = g.Key.PromotionName,
                                                   Count = g.Select(x => x.ReceiptNo).Distinct().Count(),
                                                   Amount = g.Sum(x => x.ItemPrice * x.Qty),
                                                   PromotionValue = g.Sum(x=>x.PromotionValue),
                                                   CreatedDate = g.FirstOrDefault().CreatedDate.Date
                                               }).ToList();
                        if (promotionGroups != null && promotionGroups.Any())
                        {
                            var groupPromotionSummary = promotionGroups.Where(g => g.CreatedDate.Date == lstDate[j].Date)
                                .OrderBy(x => x.PromotionName).ToList();
                            for (int k = 0; k < groupPromotionSummary.Count; k++)
                            {
                                countByStore += groupPromotionSummary[k].Count;
                                discAndProAmountByStore += groupPromotionSummary[k].PromotionValue;

                                grandCount += groupPromotionSummary[k].Count;
                                grandDiscountAmout += groupPromotionSummary[k].PromotionValue;


                                ws.Cell("A" + row).Value = lstDate[j].Date;
                                ws.Cell("B" + row).Value = groupPromotionSummary[k].PromotionName;
                                ws.Cell("C" + row).Value = groupPromotionSummary[k].Count;
                                ws.Cell("D" + row).Value = groupPromotionSummary[k].PromotionValue;
                                if (totalSales == 0)
                                {
                                    if (groupPromotionSummary[k].PromotionValue != 0)
                                    {
                                        ws.Cell("E" + row).Value = 100;
                                    }
                                    else
                                    {
                                        ws.Cell("E" + row).Value = 0;
                                    }
                                }
                                else
                                {
                                    ws.Cell("E" + row).Value = groupPromotionSummary[k].PromotionValue / totalSales * 100;
                                }
                                row++;
                            }
                            ws.Cell("A" + row).Value = lstDate[j].Date;
                            ws.Cell("B" + row).Value = totalSales;
                            ws.Cell("C" + row).Value = groupPromotionSummary.Sum(g => g.Count);
                            ws.Cell("D" + row).Value = groupPromotionSummary.Sum(g => g.PromotionValue);
                            if (totalSales == 0)
                            {
                                if (groupPromotionSummary.Sum(g => g.PromotionValue) != 0)
                                {
                                    ws.Cell("E" + row).Value = 100;
                                }
                                else
                                {
                                    ws.Cell("E" + row).Value = 0;
                                }
                            }
                            else
                            {
                                ws.Cell("E" + row).Value = groupPromotionSummary.Sum(g => g.PromotionValue) / totalSales * 100;
                            }

                            FormatSummaryData(ref ws, startSummaryProRow, row);
                        }
                        #endregion End Promotion-summary

                        row ++;
                    }
                    //Store
                    if (listReceiptId != null && listReceiptId.Any())
                    {
                        row -= 1;
                    }
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store") + ": " + store.Name;
                    ws.Cell("B" + row).Value = saleTotalByStore;
                    ws.Cell("C" + row).Value = countByStore;
                    ws.Cell("D" + row).Value = discAndProAmountByStore;
                    if (saleTotalByStore == 0)
                    {
                        if (discAndProAmountByStore != 0)
                        {
                            ws.Cell("E" + row).Value = 100;
                        }
                        else
                        {
                            ws.Cell("E" + row).Value = 0;
                        }
                    }
                    else
                    {
                        ws.Cell("E" + row).Value = discAndProAmountByStore / saleTotalByStore * 100;
                    }
                    FormatTotalData(ref ws, row);
                    row += 3;
                }//end for (int i = 0; i < lstStore.Count; i++)

                row -= 2;
                //ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Grand Total");
                ws.Cell("A" + row).Value = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL").ToLower());
                ws.Cell("B" + row).Value = grandSaleTotal;
                ws.Cell("C" + row).Value = grandCount;
                ws.Cell("D" + row).Value = grandDiscountAmout;
                if (grandSaleTotal == 0)
                {
                    if (grandDiscountAmout != 0)
                    {
                        ws.Cell("E" + row).Value = 100;
                    }
                    else
                    {
                        ws.Cell("E" + row).Value = 0;
                    }
                }
                else
                {
                    ws.Cell("E" + row).Value = grandDiscountAmout / grandSaleTotal * 100;
                }
                FormatTotalData(ref ws, row);

                ws.Columns().AdjustToContents();
            }
            else //no data
            {
                //FormatStoreHeader("", ref ws, ref rowEmpty);
                //FormatSummaryHeader(ref ws, ref rowEmpty);
            }
            return wb;
        }

        private double GetTotalSales(string storeId, DateTime receiptDate)
        {
            using (var cxt = new NuWebContext())
            {
                double amount = 0;
                try
                {
                    amount = cxt.R_ClosedReceiptReport.Where(ww => ww.StoreId == storeId
                && DbFunctions.TruncateTime(ww.CreatedDate) == receiptDate).Sum(ss => ss.Total);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                return amount;
            }
        }

        private double GetStoreTotalSales(string storeId, DateTime fromDate, DateTime toDate)
        {
            using (var cxt = new NuWebContext())
            {
                double amount = 0;
                try
                {
                    amount = cxt.R_ClosedReceiptReport
                    .Where(ww => ww.StoreId == storeId && ww.CreatedDate >= fromDate && ww.CreatedDate <= toDate).Sum(ss => ss.Total);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                return amount;
            }
        }
        #endregion End-ForReport

        #region Discount Summary Report, updated 06/04/2018
        public List<DiscountSummaryReportModels> GetDataTotalSales(BaseReportModel model)
        {
            List<DiscountSummaryReportModels> result = new List<DiscountSummaryReportModels>();
            using (var cxt = new NuWebContext())
            {
                result = (from tb in cxt.R_ClosedReceiptReport
                          where model.ListStores.Contains(tb.StoreId)
                                && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                                && tb.Mode == model.Mode
                                && string.IsNullOrEmpty(tb.CreditNoteNo) // Only receipts
                          select new DiscountSummaryReportModels
                          {
                              ReceiptId = tb.OrderId,
                              StoreId = tb.StoreId,
                              CreateDate = tb.CreatedDate,
                              Amount = tb.Total,
                              IsTotalStore = true
                          }).ToList();

                if (result != null && result.Any())
                {
                    var tips = cxt.G_OrderTip.Where(ww => model.ListStores.Contains(ww.StoreId) && ww.CreatedDate >= model.FromDate
                    && ww.CreatedDate <= model.ToDate && ww.Mode == model.Mode).ToList();

                    result = result.OrderBy(oo => oo.CreateDate).ToList();
                    foreach (var item in result)
                    {
                        item.Amount += tips.Where(ww => ww.OrderId == item.ReceiptId).Sum(ss => ss.Amount);
                    }
                }
            }
            return result;
        }

        public List<DiscountSummaryReportModels> GetDataDiscount(BaseReportModel model)
        {
            List<DiscountSummaryReportModels> result = new List<DiscountSummaryReportModels>();
            using (var cxt = new NuWebContext())
            {
                result = (from tb in cxt.R_DiscountDetailsReport
                          where model.ListStores.Contains(tb.StoreId)
                              && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                              && tb.Mode == model.Mode
                              && !string.IsNullOrEmpty(tb.DiscountId)
                              && tb.DiscountAmount != 0
                          select new DiscountSummaryReportModels
                          {
                              ReceiptId = tb.ReceiptId,
                              StoreId = tb.StoreId,
                              CreateDate = tb.CreatedDate,
                              Amount = tb.DiscountAmount,
                              DiscountId = tb.DiscountId,
                              DiscountName = tb.DiscountName
                          }).ToList();
            }
            return result;
        }

        public XLWorkbook DiscountSummaryReport(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Discount_Summary");
            CreateReportHeaderNew(ws, 3, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Summary Report"));

            // Format header report
            ws.Range(1, 1, 4, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Get all business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                model.FromDate = _lstBusDayAllStore.Min(ss => ss.DateFrom);
                model.ToDate = _lstBusDayAllStore.Max(ss => ss.DateTo);

                // Get data sales
                var lstDataSales = new List<DiscountSummaryReportDataModels>();//= GetDataTotalSales(model);
                var lstDataDiscount = new List<DiscountSummaryReportDataModels>();
                //use context
                using (var db = new NuWebContext())
                {
                    var request = new BaseReportDataModel() { ListStores = model.ListStores, FromDate = model.FromDate, ToDate = model.ToDate, Mode = model.Mode};
                    lstDataSales = db.GetDataTotalSalesForDiscountSummary(request);
                    lstDataDiscount = db.GetDataDiscountForDiscountSummary(request);
                }
                if (lstDataSales == null || !lstDataSales.Any())
                {
                    return wb;
                }
                model.ListStores = lstDataSales.Select(ss => ss.StoreId).Distinct().ToList();
                lstStore = lstStore.Where(ww => model.ListStores.Contains(ww.Id)).OrderBy(oo => oo.Name).ToList();

                // Get data discount
               // var lstDataDiscount = GetDataDiscount(model);
                var lstDiscountInfo = new List<DiscountSummaryReportDataModels>();
                if (lstDataDiscount != null && lstDataDiscount.Any())
                {
                    lstDiscountInfo = lstDataDiscount.GroupBy(gg => new { gg.DiscountName }).Select(ss => new DiscountSummaryReportDataModels()
                    {
                        DiscountName = ss.Key.DiscountName
                    }).OrderBy(oo => oo.DiscountName).ToList();
                }
                var saleInfo = new DiscountSummaryReportDataModels();

                int row = 5;
                int column = 2;

                // Title of report
                foreach (var discount in lstDiscountInfo)
                {
                    ws.Range(row, column, row, column + 1).Merge().SetValue(discount.DiscountName);
                    column += 2;
                }
                ws.Range(row, column, row, column + 1).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL SALES"));
                column++;
                int totalCol = column;
                // Format title 
                ws.Range(1, 1, 1, totalCol).Merge();
                ws.Range(2, 1, 2, totalCol).Merge();
                ws.Range(3, 1, 3, totalCol).Merge();
                ws.Range(4, 1, 4, totalCol).Merge();

                ws.Range(row, 1, row, totalCol).Style.Alignment.SetWrapText(true);
                ws.Range(row, 1, row, totalCol).Style.Font.SetBold(true);
                ws.Range(row, 1, row, totalCol).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Range(row, 1, row, totalCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
                var lstItmSalesStore = new List<DiscountSummaryReportDataModels>();
                var lstItmDisStore = new List<DiscountSummaryReportDataModels>();
                DateTime dfrom = new DateTime();
                DateTime dTo = new DateTime();
                List<string> lstReceiptIdStore = new List<string>();
                int TC = 0;
                double amount = 0;

                foreach (var store in lstStore)
                {
                    column = 1;

                    var lstBusDayStore = _lstBusDayAllStore.Where(ww => ww.StoreId == store.Id).ToList();
                    if (lstBusDayStore != null && lstBusDayStore.Any())
                    {
                        dfrom = lstBusDayStore.Min(mm => mm.DateFrom);
                        dTo = lstBusDayStore.Max(mm => mm.DateTo);

                        lstItmSalesStore = lstDataSales.Where(ww => store.Id == ww.StoreId && ww.CreateDate >= dfrom && ww.CreateDate <= dTo).ToList();
                        if (lstItmSalesStore != null && lstItmSalesStore.Any())
                        {
                            lstReceiptIdStore = lstItmSalesStore.Select(ss => ss.ReceiptId).Distinct().ToList();

                            // Store name
                            ws.Cell(row, column++).SetValue(store.Name);

                            // Discount data
                            foreach (var discount in lstDiscountInfo)
                            {
                                lstItmDisStore = lstDataDiscount.Where(ww => ww.StoreId == store.Id
                                                            && discount.DiscountName == ww.DiscountName && lstReceiptIdStore.Contains(ww.ReceiptId)).ToList();
                                if (lstItmDisStore != null && lstItmDisStore.Any())
                                {
                                    // TC
                                    TC = lstItmDisStore.GroupBy(gg => new { gg.DiscountName, gg.ReceiptId }).Count();
                                    ws.Cell(row, column).SetValue(TC);
                                    ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0";

                                    // Amount
                                    amount = lstItmDisStore.Sum(su => su.Amount);
                                    ws.Cell(row, column).SetValue(amount);
                                    ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0.00";

                                    discount.TC += TC;
                                    discount.Amount += amount;
                                }
                                else
                                {
                                    column += 2;
                                }
                            }

                            // Total sales
                            // TC
                            TC = lstItmSalesStore.Count();
                            ws.Cell(row, column).SetValue(TC);
                            ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0";

                            // Amount
                            amount = lstItmSalesStore.Sum(su => su.Amount);
                            ws.Cell(row, column).SetValue(amount);
                            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";

                            ws.Range(row, column - 1, row, column).Style.Font.SetBold(true);

                            saleInfo.TC += TC;
                            saleInfo.Amount += amount;

                            row++;
                        }
                    }
                }

                // Summary
                column = 1;
                ws.Cell(row, column++).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                // Discount data
                foreach (var discount in lstDiscountInfo)
                {
                    // TC
                    ws.Cell(row, column).SetValue(discount.TC);
                    ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0";

                    // Amount
                    ws.Cell(row, column).SetValue(discount.Amount);
                    ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0.00";
                }

                // Total sales
                // TC
                ws.Cell(row, column).SetValue(saleInfo.TC);
                ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0";

                // Amount
                ws.Cell(row, column).SetValue(saleInfo.Amount);
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";

                ws.Range(row, 1, row, totalCol).Style.Font.SetBold(true);

                ws.Range(1, 1, row, totalCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, row, totalCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                ws.Column(1).Width = 20;
                ws.Column(1).AdjustToContents();
                for (int i = 2; i < totalCol - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        ws.Column(i).Width = 15;
                    }
                }
                ws.Column(totalCol - 1).Width = 12;
                ws.Column(totalCol).Width = 18;
            }
            return wb;
        }

        #endregion Discount Summary Report, updated 06/04/2018

        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]
        #region Discount Detail
        public List<DiscountDetailsReportModels> GetData_DiscountDetail(BaseReportModel model, List<string> listStoreId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DiscountDetailsReport
                               where listStoreId.Contains(tb.StoreId)
                                     && tb.Mode == model.Mode
                                     && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                                     && ((!string.IsNullOrEmpty(tb.DiscountId) && tb.DiscountAmount != 0)
                                        || (!string.IsNullOrEmpty(tb.PromotionId) && tb.PromotionValue != 0))
                               select new DiscountDetailsReportModels
                               {
                                   Id = tb.Id,
                                   CashierId = tb.CashierId,
                                   CashierName = tb.CashierName,
                                   CreatedDate = tb.CreatedDate,
                                   DiscountAmount = tb.DiscountAmount,
                                   DiscountId = tb.DiscountId,
                                   DiscountName = tb.DiscountName,
                                   ItemId = tb.ItemId,
                                   ItemCode = tb.ItemCode,
                                   ItemName = tb.ItemName,
                                   ItemPrice = tb.ItemPrice,
                                   Qty = tb.Qty,
                                   ReceiptNo = tb.ReceiptNo,
                                   StoreId = tb.StoreId,
                                   IsDiscountValue = tb.IsDiscountValue,
                                   DiscountType = tb.DiscountType,
                                   BillTotal = tb.BillTotal,
                                   ReceiptId = tb.ReceiptId,
                                   PromotionId = tb.PromotionId,
                                   PromotionName = tb.PromotionName,
                                   PromotionValue = tb.PromotionValue.HasValue ? tb.PromotionValue.Value : 0
                               }).ToList();
                return lstData;
            }
        }

        private List<PosSaleModels> GetStoreTotalSales_NewDB(List<string> listStoreId, DateTime fromDate, DateTime toDate)
        {
            using (var cxt = new NuWebContext())
            {
                List<PosSaleModels> result = new List<PosSaleModels>();
                try
                {
                    // Receipt
                    result = cxt.R_PosSale.Where(ww => listStoreId.Contains(ww.StoreId) && ww.ReceiptCreatedDate.HasValue && ww.ReceiptCreatedDate.Value >= fromDate && ww.ReceiptCreatedDate.Value <= toDate && string.IsNullOrEmpty(ww.CreditNoteNo))
                        .Select(ss => new PosSaleModels()
                        {
                            StoreId = ss.StoreId,
                            ReceiptCreatedDate = ss.ReceiptCreatedDate,
                            ReceiptTotal = ss.ReceiptTotal,
                            OrderId = ss.OrderId,
                            NoOfPerson = ss.NoOfPerson
                        }).ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                return result;
            }
        }

        public XLWorkbook Report_NewDB(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Discount_Details");
            CreateReportHeaderNew(ws, 11, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Details Report"));

            // Format header report
            ws.Range(1, 1, 4, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            DateTime dFrom = new DateTime();
            DateTime dTo = new DateTime();

            //Get all business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                model.FromDate = _lstBusDayAllStore.Min(ss => ss.DateFrom);
                model.ToDate = _lstBusDayAllStore.Max(ss => ss.DateTo);

                int row = 5;
                double grandReceiptCount = 0;
                double grandSaleTotal = 0;
                double grandDiscountAmout = 0;

                // Get data discount
                List<DiscountDetailsReportModels> data = GetData_DiscountDetail(model, model.ListStores);
                if (data == null || !data.Any())
                {
                    return wb;
                }

                model.ListStores = data.Select(ss => ss.StoreId).Distinct().ToList();

                // Get data sale
                var lstDataSale = GetStoreTotalSales_NewDB(model.ListStores, model.FromDate, model.ToDate);

                lstStore = lstStore.Where(ww => model.ListStores.Contains(ww.Id)).ToList();

                for (int i = 0; i < lstStore.Count; i++)
                {
                    //Get StoreName
                    StoreModels store = lstStore[i];

                    FormatStoreHeader(store.Name, ref ws, ref row);

                    var lstBusDayByStore = _lstBusDayAllStore.Where(ww => ww.StoreId == store.Id).ToList();
                    if (lstBusDayByStore != null && lstBusDayByStore.Any())
                    {
                        dFrom = lstBusDayByStore.Min(mm => mm.DateFrom);
                        dTo = lstBusDayByStore.Max(mm => mm.DateTo);

                        var dataByStore = data.Where(ww => ww.StoreId == store.Id && ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo).ToList();
                        row += 2;
                        if (dataByStore != null && dataByStore.Any())
                        {
                            row -= 2;

                            #region Discount
                            List<DiscountDetailsReportDetailModels> discountGroups = (from d in dataByStore
                                                                                      where !string.IsNullOrEmpty(d.DiscountId) && d.DiscountAmount != 0
                                                                                      group d by new
                                                                                      {
                                                                                          DiscountId = d.DiscountId,
                                                                                          DiscountName = d.DiscountName,
                                                                                          CreatedDate = d.CreatedDate.Date
                                                                                      } into g
                                                                                      orderby g.Key.CreatedDate, g.Key.DiscountName
                                                                                      select new DiscountDetailsReportDetailModels
                                                                                      {
                                                                                          DiscountId = g.Key.DiscountId,
                                                                                          DiscountName = g.Key.DiscountName,
                                                                                          CreatedDate = g.Key.CreatedDate,
                                                                                          Count = g.Select(x => x.ReceiptNo).Distinct().Count(),
                                                                                          Amount = g.Sum(x => x.ItemPrice * x.Qty),
                                                                                          Discount = 0
                                                                                      }).ToList();

                            for (int j = 0; j < discountGroups.Count; j++)
                            {
                                int startRow = row;
                                discountGroups[j].Discount = 0;
                                List<DiscountDetailsReportModels> reportItems = (from tb in dataByStore
                                                                                 where tb.DiscountId == discountGroups[j].DiscountId
                                                                                  && tb.CreatedDate.Date == discountGroups[j].CreatedDate
                                                                                 orderby tb.DiscountName, tb.ReceiptNo
                                                                                 select new DiscountDetailsReportModels
                                                                                 {
                                                                                     Id = tb.Id,
                                                                                     CashierId = tb.CashierId,
                                                                                     CashierName = tb.CashierName,
                                                                                     CreatedDate = tb.CreatedDate,
                                                                                     DiscountAmount = tb.DiscountAmount,
                                                                                     DiscountId = tb.DiscountId,
                                                                                     DiscountName = tb.DiscountName,
                                                                                     ItemCode = tb.ItemCode,
                                                                                     ItemName = tb.ItemName,
                                                                                     ItemPrice = tb.ItemPrice,
                                                                                     Qty = tb.Qty,
                                                                                     ReceiptNo = tb.ReceiptNo,
                                                                                     StoreId = tb.StoreId,
                                                                                     IsDiscountValue = tb.IsDiscountValue,
                                                                                     DiscountType = tb.DiscountType,
                                                                                     BillTotal = tb.BillTotal,
                                                                                     ReceiptId = tb.ReceiptId
                                                                                 }).ToList();
                                for (int k = 0; k < reportItems.Count; k++)
                                {
                                    double discountAmount = 0;
                                    var objClosedReceipt = lstDataSale.Where(x => x.OrderId.Equals(reportItems[k].ReceiptId)).FirstOrDefault();
                                    int PAX = 0;
                                    if (objClosedReceipt != null)
                                    {
                                        PAX = objClosedReceipt.NoOfPerson;
                                    }
                                    ws.Cell("A" + row).Value = reportItems[k].CreatedDate.Date;
                                    ws.Cell("B" + row).Value = reportItems[k].CreatedDate.TimeOfDay;
                                    ws.Cell("C" + row).Value = reportItems[k].ReceiptNo;
                                    ws.Cell("D" + row).Value = PAX;
                                    ws.Cell("E" + row).Value = reportItems[k].CashierName;
                                    ws.Cell("F" + row).Value = "'" + reportItems[k].ItemCode;
                                    ws.Cell("G" + row).Value = reportItems[k].ItemName;
                                    ws.Cell("H" + row).Value = reportItems[k].Qty;
                                    ws.Cell("I" + row).Value = reportItems[k].ItemPrice * reportItems[k].Qty; //.ItemAmount
                                    ws.Cell("J" + row).Value = reportItems[k].DiscountName;

                                    discountAmount = reportItems[k].DiscountAmount;
                                    if (reportItems[k].DiscountType == Commons.DiscountTotalBill)
                                    {
                                        double itemTotalAmount = reportItems[k].ItemPrice * reportItems[k].Qty;

                                        // Get another item details in the same receipt with current item and have same total bill discount 
                                        List<DiscountDetailsReportModels> receiptDetailItems = reportItems.Where(r => r.ReceiptNo == reportItems[k].ReceiptNo && r.ReceiptId == reportItems[k].ReceiptId && r.DiscountType == Commons.DiscountTotalBill && r.Id != reportItems[k].Id).ToList();

                                        if (receiptDetailItems != null && receiptDetailItems.Any())
                                        {
                                            for (int l = 0; l < receiptDetailItems.Count; l++)
                                            {
                                                ws.Cell("A" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.Date;
                                                ws.Cell("B" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.TimeOfDay;
                                                ws.Cell("C" + (row + l + 1)).Value = receiptDetailItems[l].ReceiptNo;
                                                ws.Cell("D" + (row + l + 1)).Value = PAX;
                                                ws.Cell("E" + (row + l + 1)).Value = receiptDetailItems[l].CashierName;
                                                ws.Cell("F" + (row + l + 1)).Value = "'" + receiptDetailItems[l].ItemCode;
                                                ws.Cell("G" + (row + l + 1)).Value = receiptDetailItems[l].ItemName;
                                                ws.Cell("H" + (row + l + 1)).Value = receiptDetailItems[l].Qty;
                                                ws.Cell("I" + (row + l + 1)).Value = receiptDetailItems[l].ItemPrice * receiptDetailItems[l].Qty; //.ItemAmount
                                                ws.Cell("J" + (row + l + 1)).Value = receiptDetailItems[l].DiscountName;

                                                discountAmount += receiptDetailItems[l].DiscountAmount;
                                            }
                                            itemTotalAmount += receiptDetailItems.Sum(r => r.ItemPrice * r.Qty); //.ItemAmount
                                        }

                                        ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Merge().Value = discountAmount;
                                        ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                        row += receiptDetailItems.Count;
                                        k += receiptDetailItems.Count;
                                        discountGroups[j].Discount += discountAmount;
                                    }
                                    else
                                    {
                                        ws.Cell("K" + row).Value = discountAmount;
                                        discountGroups[j].Discount += discountAmount;
                                    }
                                    row++;
                                }
                                FormatStoreData(ref ws, startRow, row - 1);

                                ws.Cell("A" + row).Value = discountGroups[j].CreatedDate.Date;
                                ws.Cell("B" + row).Value = discountGroups[j].DiscountName;
                                ws.Cell("C" + row).Value = discountGroups[j].Count;
                                ws.Cell("I" + row).Value = discountGroups[j].Amount;
                                ws.Cell("K" + row).Value = discountGroups[j].Discount;
                                FormatStoreData(ref ws, row, row, true);
                                row += 2;
                            }

                            #endregion Discount

                            #region Promotion

                            List<DiscountDetailsReportDetailModels> promoGroups = (from d in dataByStore
                                                                                   where !string.IsNullOrEmpty(d.PromotionId) && d.PromotionValue != 0
                                                                                   group d by new
                                                                                   {
                                                                                       PromotionId = d.PromotionId,
                                                                                       PromotionName = d.PromotionName,
                                                                                       CreatedDate = d.CreatedDate.Date
                                                                                   } into g
                                                                                   orderby g.Key.CreatedDate, g.Key.PromotionName
                                                                                   select new DiscountDetailsReportDetailModels
                                                                                   {
                                                                                       PromotionId = g.Key.PromotionId,
                                                                                       PromotionName = g.Key.PromotionName,
                                                                                       CreatedDate = g.Key.CreatedDate,
                                                                                       Count = g.Select(x => x.ReceiptNo).Distinct().Count(),
                                                                                       Amount = g.Sum(x => x.ItemPrice * x.Qty),
                                                                                       PromotionValue = 0
                                                                                   }).ToList();

                            for (int j = 0; j < promoGroups.Count; j++)
                            {
                                int startRow = row;
                                promoGroups[j].PromotionValue = 0;
                                List<DiscountDetailsReportModels> reportItems = (from tb in dataByStore
                                                                                 where tb.PromotionId == promoGroups[j].PromotionId
                                                                                  && tb.CreatedDate.Date == promoGroups[j].CreatedDate
                                                                                 orderby tb.PromotionName, tb.ReceiptNo
                                                                                 select new DiscountDetailsReportModels
                                                                                 {
                                                                                     Id = tb.Id,
                                                                                     CashierId = tb.CashierId,
                                                                                     CashierName = tb.CashierName,
                                                                                     CreatedDate = tb.CreatedDate,
                                                                                     ItemCode = tb.ItemCode,
                                                                                     ItemName = tb.ItemName,
                                                                                     ItemPrice = tb.ItemPrice,
                                                                                     Qty = tb.Qty,
                                                                                     ReceiptNo = tb.ReceiptNo,
                                                                                     StoreId = tb.StoreId,
                                                                                     BillTotal = tb.BillTotal,
                                                                                     ReceiptId = tb.ReceiptId,
                                                                                     PromotionValue = tb.PromotionValue,
                                                                                     PromotionId = tb.PromotionId,
                                                                                     PromotionName = tb.PromotionName,
                                                                                     PromotionType = tb.PromotionType
                                                                                 }).ToList();
                                for (int k = 0; k < reportItems.Count; k++)
                                {
                                    double promoAmount = 0;
                                    var objClosedReceipt = lstDataSale.Where(x => x.OrderId.Equals(reportItems[k].ReceiptId)).FirstOrDefault();
                                    int PAX = 0;
                                    if (objClosedReceipt != null)
                                    {
                                        PAX = objClosedReceipt.NoOfPerson;
                                    }
                                    ws.Cell("A" + row).Value = reportItems[k].CreatedDate.Date;
                                    ws.Cell("B" + row).Value = reportItems[k].CreatedDate.TimeOfDay;
                                    ws.Cell("C" + row).Value = reportItems[k].ReceiptNo;
                                    ws.Cell("D" + row).Value = PAX;
                                    ws.Cell("E" + row).Value = reportItems[k].CashierName;
                                    ws.Cell("F" + row).Value = "'" + reportItems[k].ItemCode;
                                    ws.Cell("G" + row).Value = reportItems[k].ItemName;
                                    ws.Cell("H" + row).Value = reportItems[k].Qty;
                                    ws.Cell("I" + row).Value = reportItems[k].ItemPrice * reportItems[k].Qty; //.ItemAmount
                                    ws.Cell("J" + row).Value = reportItems[k].PromotionName;

                                    promoAmount = reportItems[k].PromotionValue;
                                    if (reportItems[k].PromotionType == Commons.PromotionTotalBill)
                                    {
                                        double itemTotalAmount = reportItems[k].ItemPrice * reportItems[k].Qty;

                                        // Get another item details in the same receipt with current item and have same total bill promotion 
                                        List<DiscountDetailsReportModels> receiptDetailItems = reportItems.Where(r => r.ReceiptNo == reportItems[k].ReceiptNo && r.ReceiptId == reportItems[k].ReceiptId
                                            && r.PromotionType == Commons.PromotionTotalBill && r.Id != reportItems[k].Id).ToList();

                                        if (receiptDetailItems != null && receiptDetailItems.Any())
                                        {
                                            for (int l = 0; l < receiptDetailItems.Count; l++)
                                            {
                                                ws.Cell("A" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.Date;
                                                ws.Cell("B" + (row + l + 1)).Value = receiptDetailItems[l].CreatedDate.TimeOfDay;
                                                ws.Cell("C" + (row + l + 1)).Value = receiptDetailItems[l].ReceiptNo;
                                                ws.Cell("D" + (row + l + 1)).Value = PAX;
                                                ws.Cell("E" + (row + l + 1)).Value = receiptDetailItems[l].CashierName;
                                                ws.Cell("F" + (row + l + 1)).Value = "'" + receiptDetailItems[l].ItemCode;
                                                ws.Cell("G" + (row + l + 1)).Value = receiptDetailItems[l].ItemName;
                                                ws.Cell("H" + (row + l + 1)).Value = receiptDetailItems[l].Qty;
                                                ws.Cell("I" + (row + l + 1)).Value = receiptDetailItems[l].ItemPrice * receiptDetailItems[l].Qty; //.ItemAmount
                                                ws.Cell("J" + (row + l + 1)).Value = receiptDetailItems[l].PromotionName;

                                                promoAmount += receiptDetailItems[l].PromotionValue;
                                            }
                                            itemTotalAmount += receiptDetailItems.Sum(r => r.ItemPrice * r.Qty); //.ItemAmount
                                        }

                                        ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Merge().Value = promoAmount;
                                        ws.Range(string.Format("K{0}:K{1}", row, row + receiptDetailItems.Count)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                        row += receiptDetailItems.Count;
                                        k += receiptDetailItems.Count;
                                        promoGroups[j].PromotionValue += promoAmount;
                                    }
                                    else
                                    {
                                        ws.Cell("K" + row).Value = promoAmount;
                                        promoGroups[j].PromotionValue += promoAmount;
                                    }
                                    row++;
                                }
                                FormatStoreData(ref ws, startRow, row - 1);

                                ws.Cell("A" + row).Value = promoGroups[j].CreatedDate.Date;
                                ws.Cell("B" + row).Value = promoGroups[j].PromotionName;
                                ws.Cell("C" + row).Value = promoGroups[j].Count;
                                ws.Cell("I" + row).Value = promoGroups[j].Amount;
                                ws.Cell("K" + row).Value = promoGroups[j].PromotionValue;
                                FormatStoreData(ref ws, row, row, true);
                                row += 2;
                            }

                            #endregion Promotion

                            FormatSummaryHeader(ref ws, ref row);
                            double storeSaleTotal = 0, totalSales = 0;

                            #region Summary, all Receipts of date has any receipt with discount, promotion
                            var lstDate = dataByStore.GroupBy(gg => gg.CreatedDate.Date).OrderBy(oo => oo.Key).Select(ss => ss.Key).ToList();
                            foreach (var date in lstDate)
                            {
                                totalSales = lstDataSale.Where(ww => ww.StoreId == store.Id && ww.ReceiptCreatedDate.HasValue && ww.ReceiptCreatedDate.Value >= dFrom && ww.ReceiptCreatedDate.Value <= dTo && ww.ReceiptCreatedDate.Value.Date == date).Sum(su => su.ReceiptTotal);
                                storeSaleTotal += totalSales;

                                #region Summary Discount
                                if (discountGroups != null && discountGroups.Any())
                                {
                                    int startRow = row;
                                    List<DiscountDetailsReportDetailModels> groups = discountGroups.Where(g => g.CreatedDate.Date == date).OrderBy(x => x.DiscountName).ToList();
                                    for (int k = 0; k < groups.Count; k++)
                                    {
                                        ws.Cell("A" + row).Value = date;
                                        ws.Cell("B" + row).Value = groups[k].DiscountName;
                                        ws.Cell("C" + row).Value = groups[k].Count;
                                        ws.Cell("D" + row).Value = groups[k].Discount;
                                        if (totalSales == 0)
                                        {
                                            if (groups[k].Discount != 0)
                                            {
                                                ws.Cell("E" + row).Value = 100;
                                            }
                                            else
                                            {
                                                ws.Cell("E" + row).Value = 0;
                                            }
                                        }
                                        else
                                        {
                                            ws.Cell("E" + row).Value = groups[k].Discount / totalSales * 100;
                                        }
                                        row++;
                                    }
                                    ws.Cell("A" + row).Value = date;
                                    ws.Cell("B" + row).Value = totalSales;
                                    ws.Cell("C" + row).Value = groups.Sum(g => g.Count);
                                    ws.Cell("D" + row).Value = groups.Sum(g => g.Discount);
                                    if (totalSales == 0)
                                    {
                                        if (groups.Sum(g => g.Discount) != 0)
                                        {
                                            ws.Cell("E" + row).Value = 100;
                                        }
                                        else
                                        {
                                            ws.Cell("E" + row).Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        ws.Cell("E" + row).Value = groups.Sum(g => g.Discount) / totalSales * 100;
                                    }

                                    FormatSummaryData(ref ws, startRow, row);
                                    row += 2;
                                }
                                #endregion Summary Discount

                                #region Summary Promotion
                                if (promoGroups != null && promoGroups.Any())
                                {
                                    int startRow = row;

                                    List<DiscountDetailsReportDetailModels> groups = promoGroups.Where(g => g.CreatedDate.Date == date).OrderBy(x => x.PromotionName).ToList();
                                    for (int k = 0; k < groups.Count; k++)
                                    {
                                        ws.Cell("A" + row).Value = date;
                                        ws.Cell("B" + row).Value = groups[k].PromotionName;
                                        ws.Cell("C" + row).Value = groups[k].Count;
                                        ws.Cell("D" + row).Value = groups[k].PromotionValue;
                                        if (totalSales == 0)
                                        {
                                            if (groups[k].PromotionValue != 0)
                                            {
                                                ws.Cell("E" + row).Value = 100;
                                            }
                                            else
                                            {
                                                ws.Cell("E" + row).Value = 0;
                                            }
                                        }
                                        else
                                        {
                                            ws.Cell("E" + row).Value = groups[k].PromotionValue / totalSales * 100;
                                        }
                                        row++;
                                    }
                                    ws.Cell("A" + row).Value = date;
                                    ws.Cell("B" + row).Value = totalSales;
                                    ws.Cell("C" + row).Value = groups.Sum(g => g.Count);
                                    ws.Cell("D" + row).Value = groups.Sum(g => g.PromotionValue);
                                    if (totalSales == 0)
                                    {
                                        if (groups.Sum(g => g.PromotionValue) != 0)
                                        {
                                            ws.Cell("E" + row).Value = 100;
                                        }
                                        else
                                        {
                                            ws.Cell("E" + row).Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        ws.Cell("E" + row).Value = groups.Sum(g => g.PromotionValue) / totalSales * 100;
                                    }

                                    FormatSummaryData(ref ws, startRow, row);
                                    row += 2;
                                    //}
                                }
                                #endregion Summary Promotion
                            }



                            #region Summary Store
                            if (dataByStore != null && dataByStore.Any())
                            {
                                row--;
                            }

                            double totalDisPro = discountGroups.Sum(g => g.Discount) + promoGroups.Sum(g => g.PromotionValue);
                            double countDisPro = discountGroups.Sum(g => g.Count) + promoGroups.Sum(g => g.Count);

                            ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store") + ": " + store.Name;
                            ws.Cell("B" + row).Value = storeSaleTotal;
                            ws.Cell("C" + row).Value = countDisPro;
                            ws.Cell("D" + row).Value = totalDisPro;
                            if (storeSaleTotal == 0)
                            {
                                if (totalDisPro != 0)
                                {
                                    ws.Cell("E" + row).Value = 100;
                                }
                                else
                                {
                                    ws.Cell("E" + row).Value = 0;
                                }
                            }
                            else
                            {
                                ws.Cell("E" + row).Value = totalDisPro / storeSaleTotal * 100;
                            }
                            FormatTotalData(ref ws, row);

                            grandSaleTotal += storeSaleTotal;
                            grandReceiptCount += countDisPro;
                            grandDiscountAmout += totalDisPro;
                            row += 3;
                            #endregion Summary Store
                            #endregion Summary, all Receipts of date has any receipt with discount, promotion

                        }
                    }
                }
                row -= 2;
                ws.Cell("A" + row).Value = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL").ToLower());
                ws.Cell("B" + row).Value = grandSaleTotal;
                ws.Cell("C" + row).Value = grandReceiptCount;
                ws.Cell("D" + row).Value = grandDiscountAmout;
                if (grandSaleTotal == 0)
                {
                    if (grandDiscountAmout != 0)
                    {
                        ws.Cell("E" + row).Value = 100;
                    }
                }
                else
                {
                    ws.Cell("E" + row).Value = grandDiscountAmout / grandSaleTotal * 100;
                }
                FormatTotalData(ref ws, row);

                //ws.Columns().AdjustToContents();
            }

            return wb;
        }
        #endregion Discount Detail

        #region Discount Summary
        public List<DiscountSummaryReportModels> GetDataTotalSales_NewDB(BaseReportModel model)
        {
            List<DiscountSummaryReportModels> result = new List<DiscountSummaryReportModels>();
            using (var cxt = new NuWebContext())
            {
                result = (from tb in cxt.R_PosSale
                          where model.ListStores.Contains(tb.StoreId)
                                && (tb.ReceiptCreatedDate >= model.FromDate && tb.ReceiptCreatedDate <= model.ToDate)
                                && tb.Mode == model.Mode
                                && string.IsNullOrEmpty(tb.CreditNoteNo) // Only receipts
                          select new DiscountSummaryReportModels
                          {
                              ReceiptId = tb.OrderId,
                              StoreId = tb.StoreId,
                              CreateDate = tb.ReceiptCreatedDate.Value,
                              Amount = tb.ReceiptTotal,
                              IsTotalStore = true
                          }).ToList();

                if (result != null && result.Any())
                {
                    var tips = cxt.G_OrderTip.Where(ww => model.ListStores.Contains(ww.StoreId) && ww.CreatedDate >= model.FromDate
                    && ww.CreatedDate <= model.ToDate && ww.Mode == model.Mode).ToList();

                    result = result.OrderBy(oo => oo.CreateDate).ToList();
                    foreach (var item in result)
                    {
                        item.Amount += tips.Where(ww => ww.OrderId == item.ReceiptId).Sum(ss => ss.Amount);
                    }
                }
            }
            return result;
        }

        public List<DiscountSummaryReportModels> GetData_DiscountSummary(BaseReportModel model)
        {
            List<DiscountSummaryReportModels> result = new List<DiscountSummaryReportModels>();
            using (var cxt = new NuWebContext())
            {
                result = (from tb in cxt.R_DiscountDetailsReport
                          where model.ListStores.Contains(tb.StoreId)
                              && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                              && tb.Mode == model.Mode
                              && !string.IsNullOrEmpty(tb.DiscountId)
                              && tb.DiscountAmount != 0
                          select new DiscountSummaryReportModels
                          {
                              ReceiptId = tb.ReceiptId,
                              StoreId = tb.StoreId,
                              CreateDate = tb.CreatedDate,
                              Amount = tb.DiscountAmount,
                              DiscountId = tb.DiscountId,
                              DiscountName = tb.DiscountName
                          }).ToList();
            }
            return result;
        }

        public XLWorkbook DiscountSummaryReport_NewDB(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Discount_Summary");
            CreateReportHeaderNew(ws, 3, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Summary Report"));

            // Format header report
            ws.Range(1, 1, 4, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Get all business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                model.FromDate = _lstBusDayAllStore.Min(ss => ss.DateFrom);
                model.ToDate = _lstBusDayAllStore.Max(ss => ss.DateTo);

                // Get data sales
                var lstDataSales = GetDataTotalSales_NewDB(model);
                if (lstDataSales == null || !lstDataSales.Any())
                {
                    return wb;
                }
                model.ListStores = lstDataSales.Select(ss => ss.StoreId).Distinct().ToList();
                lstStore = lstStore.Where(ww => model.ListStores.Contains(ww.Id)).OrderBy(oo => oo.Name).ToList();

                // Get data discount
                var lstDataDiscount = GetData_DiscountSummary(model);
                var lstDiscountInfo = new List<DiscountSummaryReportModels>();
                if (lstDataDiscount != null && lstDataDiscount.Any())
                {
                    lstDiscountInfo = lstDataDiscount.GroupBy(gg => new { gg.DiscountName }).Select(ss => new DiscountSummaryReportModels()
                    {
                        DiscountName = ss.Key.DiscountName
                    }).OrderBy(oo => oo.DiscountName).ToList();
                }
                var saleInfo = new DiscountSummaryReportModels();

                int row = 5;
                int column = 2;

                // Title of report
                foreach (var discount in lstDiscountInfo)
                {
                    ws.Range(row, column, row, column + 1).Merge().SetValue(discount.DiscountName);
                    column += 2;
                }
                ws.Range(row, column, row, column + 1).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL SALES"));
                column++;
                int totalCol = column;
                // Format title 
                ws.Range(1, 1, 1, totalCol).Merge();
                ws.Range(2, 1, 2, totalCol).Merge();
                ws.Range(3, 1, 3, totalCol).Merge();
                ws.Range(4, 1, 4, totalCol).Merge();

                ws.Range(row, 1, row, totalCol).Style.Alignment.SetWrapText(true);
                ws.Range(row, 1, row, totalCol).Style.Font.SetBold(true);
                ws.Range(row, 1, row, totalCol).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Range(row, 1, row, totalCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
                var lstItmSalesStore = new List<DiscountSummaryReportModels>();
                var lstItmDisStore = new List<DiscountSummaryReportModels>();
                DateTime dfrom = new DateTime();
                DateTime dTo = new DateTime();
                List<string> lstReceiptIdStore = new List<string>();
                int TC = 0;
                double amount = 0;

                foreach (var store in lstStore)
                {
                    column = 1;

                    var lstBusDayStore = _lstBusDayAllStore.Where(ww => ww.StoreId == store.Id).ToList();
                    if (lstBusDayStore != null && lstBusDayStore.Any())
                    {
                        dfrom = lstBusDayStore.Min(mm => mm.DateFrom);
                        dTo = lstBusDayStore.Max(mm => mm.DateTo);

                        lstItmSalesStore = lstDataSales.Where(ww => store.Id == ww.StoreId && ww.CreateDate >= dfrom && ww.CreateDate <= dTo).ToList();
                        if (lstItmSalesStore != null && lstItmSalesStore.Any())
                        {
                            lstReceiptIdStore = lstItmSalesStore.Select(ss => ss.ReceiptId).Distinct().ToList();

                            // Store name
                            ws.Cell(row, column++).SetValue(store.Name);

                            // Discount data
                            foreach (var discount in lstDiscountInfo)
                            {
                                lstItmDisStore = lstDataDiscount.Where(ww => ww.StoreId == store.Id
                                                            && discount.DiscountName == ww.DiscountName && lstReceiptIdStore.Contains(ww.ReceiptId)).ToList();
                                if (lstItmDisStore != null && lstItmDisStore.Any())
                                {
                                    // TC
                                    TC = lstItmDisStore.GroupBy(gg => new { gg.DiscountName, gg.ReceiptId }).Count();
                                    ws.Cell(row, column).SetValue(TC);
                                    ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0";

                                    // Amount
                                    amount = lstItmDisStore.Sum(su => su.Amount);
                                    ws.Cell(row, column).SetValue(amount);
                                    ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0.00";

                                    discount.TC += TC;
                                    discount.Amount += amount;
                                }
                                else
                                {
                                    column += 2;
                                }
                            }

                            // Total sales
                            // TC
                            TC = lstItmSalesStore.Count();
                            ws.Cell(row, column).SetValue(TC);
                            ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0";

                            // Amount
                            amount = lstItmSalesStore.Sum(su => su.Amount);
                            ws.Cell(row, column).SetValue(amount);
                            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";

                            ws.Range(row, column - 1, row, column).Style.Font.SetBold(true);

                            saleInfo.TC += TC;
                            saleInfo.Amount += amount;

                            row++;
                        }
                    }
                }

                // Summary
                column = 1;
                ws.Cell(row, column++).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                // Discount data
                foreach (var discount in lstDiscountInfo)
                {
                    // TC
                    ws.Cell(row, column).SetValue(discount.TC);
                    ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0";

                    // Amount
                    ws.Cell(row, column).SetValue(discount.Amount);
                    ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0.00";
                }

                // Total sales
                // TC
                ws.Cell(row, column).SetValue(saleInfo.TC);
                ws.Cell(row, column++).Style.NumberFormat.Format = "#,##0";

                // Amount
                ws.Cell(row, column).SetValue(saleInfo.Amount);
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";

                ws.Range(row, 1, row, totalCol).Style.Font.SetBold(true);

                ws.Range(1, 1, row, totalCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, row, totalCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                ws.Column(1).Width = 20;
                ws.Column(1).AdjustToContents();
                for (int i = 2; i < totalCol - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        ws.Column(i).Width = 15;
                    }
                }
                ws.Column(totalCol - 1).Width = 12;
                ws.Column(totalCol).Width = 18;
            }
            return wb;
        }
        #endregion Discount Summary
        #endregion


    }
}
