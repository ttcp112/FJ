using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace NuWebNCloud.Shared.Factory
{
    public class TopSellingProductsReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public TopSellingProductsReportFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<TopSellingProductsReportModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            //_logger.Info("==========================================================");
            //_logger.Info(string.Format("Insert Topsale: StoreId: [{0}] | BusinessId: [{1}]", info.StoreId, info.BusinessDayId));
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_TopSellingProductsReport.Where(ww => ww.StoreId == info.StoreId
                                && ww.BusinessDayId == info.BusinessDayId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Top Selling data exist");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_TopSellingProductsReport> lstInsert = new List<R_TopSellingProductsReport>();
                        R_TopSellingProductsReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_TopSellingProductsReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.BusinessDayId = item.BusinessDayId;
                            itemInsert.OrderId = item.OrderId;
                            itemInsert.OrderDetailId = item.OrderDetailId;
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.ItemId = item.ItemId;
                            itemInsert.ItemCode = item.ItemCode;
                            itemInsert.ItemName = item.ItemName;
                            itemInsert.Qty = item.Qty;
                            itemInsert.Discount = item.Discount;
                            itemInsert.Amount = item.Amount;
                            itemInsert.PromotionAmount = item.PromotionAmount;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.Tax = item.Tax;
                            itemInsert.ServiceCharged = item.ServiceCharged;
                            itemInsert.Mode = item.Mode;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_TopSellingProductsReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Top Selling data success", lstInfo);
                        //_logger.Info(string.Format("Insert Topsale: StoreId: [{0}] | BusinessId: [{1}]", info.StoreId, info.BusinessDayId));
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Top Selling data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_TopSellingProductsReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<TopSellingProductsReportModels> GetDataTopSellByAmount(RPTopSellingProductsModels model, string StoreId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = ((from tb in cxt.R_TopSellingProductsReport
                                where tb.StoreId == StoreId
                                      && (tb.CreatedDate >= model.FromDate
                                              && tb.CreatedDate <= model.ToDate)
                                group tb by new { ItemName = tb.ItemName, ItemId = tb.ItemId } into g
                                select new TopSellingProductsReportModels
                                {
                                    ItemId = g.Key.ItemId,
                                    ItemName = g.Key.ItemName,
                                    Amount = g.Sum(tb => tb.Amount),
                                    Qty = g.Sum(tb => tb.Qty),
                                    Discount = g.Sum(tb => tb.Discount),
                                    //                                    Total = g.Sum(tb => tb.Total)
                                }).OrderByDescending(oo => oo.Amount)
                        .Take(model.TopSell)
                        ).ToList();
                return lstData;
            }
        }
        public List<TopSellingProductsReportModels> GetDataTopSellByQty(RPTopSellingProductsModels model, string StoreId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = ((from tb in cxt.R_TopSellingProductsReport
                                where tb.StoreId == StoreId
                                      && (tb.CreatedDate >= model.FromDate
                                              && tb.CreatedDate <= model.ToDate)
                                group tb by new { ItemName = tb.ItemName, ItemId = tb.ItemId } into g
                                select new TopSellingProductsReportModels
                                {
                                    ItemId = g.Key.ItemId,
                                    ItemName = g.Key.ItemName,
                                    Amount = g.Sum(tb => tb.Amount),
                                    Qty = g.Sum(tb => tb.Qty),
                                    Discount = g.Sum(tb => tb.Discount),
                                    // Total = g.Sum(tb => tb.Total)
                                }).OrderByDescending(oo => oo.Qty)
                        .Take(model.TopSell)
                        ).ToList();
                return lstData;
            }
        }

        //create 06/04/2018
        public List<TopSellingProductsReportModels> GetDataTopSellByQty_v2(RPTopSellingProductsModels model, string StoreId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_PosSaleDetail
                                    .Where(x => x.StoreId.Equals(StoreId)
                                    && (x.CreatedDate >= model.FromDate
                                              && x.CreatedDate <= model.ToDate))
                                   .GroupBy(g => new { g.ItemName, g.ItemId })
                                   .Select(s => new TopSellingProductsReportModels
                                   {
                                       ItemId = s.Key.ItemId,
                                       ItemName = s.Key.ItemName,
                                       Amount = s.Sum(ss => ss.TotalAmount),
                                       Qty = s.Sum(ss => ss.Quantity),
                                       Discount = s.Sum(ss => ss.Discount)
                                   }).OrderByDescending(o => o.Qty)
                                   .Take(model.TopSell).ToList();
                return lstData;
            }
        }

        public List<TopSellingProductsReportModels> GetDataTopSellByAmount_2(RPTopSellingProductsModels model, string StoreId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_PosSaleDetail
                                    .Where(x => x.StoreId.Equals(StoreId)
                                    && (x.CreatedDate >= model.FromDate
                                              && x.CreatedDate <= model.ToDate))
                                   .GroupBy(g => new { g.ItemName, g.ItemId })
                                   .Select(s => new TopSellingProductsReportModels
                                   {
                                       ItemId = s.Key.ItemId,
                                       ItemName = s.Key.ItemName,
                                       Amount = s.Sum(ss => ss.TotalAmount),
                                       Qty = s.Sum(ss => ss.Quantity),
                                       Discount = s.Sum(ss => ss.Discount)
                                   }).OrderByDescending(o => o.Amount)
                                   .Take(model.TopSell).ToList();
                return lstData;
            }
        }

        public double GetTotalReceipt(DateTime dFrom, DateTime dTo, string storeId)
        {
            using (var cxt = new NuWebContext())
            {
                var total = cxt.R_ClosedReceiptReport.Where(ww => ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo 
                    && ww.StoreId == storeId && string.IsNullOrEmpty(ww.CreditNoteNo)) // only Receipt, not Credit Note
                    .Sum(ss => (double?)ss.Total) ?? 0;
                return total;
            }
        }

        [Obsolete("ExportExcel is deprecated, please use ExportExcel_New instead.")]
        public XLWorkbook ExportExcel(RPTopSellingProductsModels model, List<StoreModels> lstStore)
        {
            string sheetName = "Top_" + model.TopSell + "_Selling_Products";
            // _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top_") + model.TopSell + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("_Selling_Products");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            CreateReportHeaderForTopSale(ws, 5, model.FromDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Products Report"));

            //for (int i = 1; i <= 5; i++)
            //{
            //    ws.Column(i).Width = 28;
            //}
            DateTime fromDate = model.FromDate;
            //DateTime toDate = model.ToDate;

            int row = 4;
            int startRow = row;
            int startRow_Store = row;
            int endRow_Store = row;
            int startRow_User = row;
            int endRow_User = row;

            startRow_Store = row;
            endRow_Store = row;
            row++;

            string storeId = string.Empty, storeName = string.Empty;
            for (int i = 0; i < lstStore.Count; i++)
            {
                ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
                ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("% of Sales"));
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("A" + row + ":E" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                row++;

                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name;

                #region======Get List Data
                string title = "";
                double sumTotal = 0;
                model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                for (int m = 0; m < 6; m++)
                {
                    sumTotal = 0;
                    ws.Row(row).Height = 24;
                    List<TopSellingProductsReportModels> _data = null;
                    if (m == 0)//Day
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Quantity) (as of") + " " + model.FromDate.ToString("MM/dd/yyyy") + ")";

                        model.ToDate = model.FromDate;
                        this.SetDate(ref model);
                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 1)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Amount) (as of") + " " + model.FromDate.ToString("MM/dd/yyyy") + ")";
                        model.ToDate = model.FromDate;
                        this.SetDate(ref model);
                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 2)//Month
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Quantity)");

                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);
                        model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, 1, 0, 0, 0);

                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 3)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Amount)");
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);
                        model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, 1, 0, 0, 0);
                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 4) // Year
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Quantity)");

                        model.FromDate = new DateTime(model.FromDate.Year, 1, 1, 0, 0, 0);
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);

                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 5)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Amount)");

                        model.FromDate = new DateTime(model.FromDate.Year, 1, 1, 0, 0, 0);
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);

                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    //_data = _data.OrderBy(o => o.ItemName).ToList();
                    GetItemData(title, ref ws, ref row, _data, sumTotal);
                }
                #endregion
                //Get StoreName 
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Merge().SetValue(storeName);
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetBold(true);
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetFontSize(16);
                ///============
                string _symbol = "$";//HQContext.R_Currencies.Where(s => s.StoreCreate == _store.Index && s.IsSelected == true).Select(s => s.Symbol).FirstOrDefault();
                ws.Cell("E" + startRow_Store).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency") + ": " + _symbol);
                ws.Cell("E" + startRow_Store).Style.Font.SetBold(true);
                ws.Cell("E" + startRow_Store).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                if (model.ListStores.Count > 1)
                {
                    ws.Range("A" + row + ":E" + row + "").Merge().SetValue("");
                    row++;
                    startRow_Store = row;
                    endRow_Store = row;
                    row++;
                }
            }
            //Set Border 
            ws.Range("A1:E" + (row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A1:E" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();
            return wb;
        }
        [Obsolete("ExportExcelForMerchantExtend is deprecated, please use ExportExcel_New instead.")]
        public XLWorkbook ExportExcelForMerchantExtend(RPTopSellingProductsModels model, List<StoreModels> lstStore)
        {
            string sheetName = "Top_" + model.TopSell + "_Selling_Products";
            // _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top_") + model.TopSell + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("_Selling_Products");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            CreateReportHeaderForTopSale(ws, 5, model.FromDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Products Report"));

            //for (int i = 1; i <= 5; i++)
            //{
            //    ws.Column(i).Width = 28;
            //}
            DateTime fromDate = model.FromDate;
            //DateTime toDate = model.ToDate;

            int row = 4;
            int startRow = row;
            int startRow_Store = row;
            int endRow_Store = row;
            int startRow_User = row;
            int endRow_User = row;

            startRow_Store = row;
            endRow_Store = row;
            row++;

            string storeId = string.Empty, storeName = string.Empty;
            for (int i = 0; i < lstStore.Count; i++)
            {
                ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
                ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("% of Sales"));
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("A" + row + ":E" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                row++;

                storeId = lstStore[i].Id;
                storeName = lstStore[i].NameExtend;

                #region======Get List Data
                string title = "";
                double sumTotal = 0;
                model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                for (int m = 0; m < 6; m++)
                {
                    sumTotal = 0;
                    ws.Row(row).Height = 24;
                    List<TopSellingProductsReportModels> _data = null;
                    if (m == 0)//Day
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Quantity) (as of") + " " + model.FromDate.ToString("MM/dd/yyyy") + ")";

                        model.ToDate = model.FromDate;
                        this.SetDate(ref model);
                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 1)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Amount) (as of") + " " + model.FromDate.ToString("MM/dd/yyyy") + ")";
                        model.ToDate = model.FromDate;
                        this.SetDate(ref model);
                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 2)//Month
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Quantity)");

                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);
                        model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, 1, 0, 0, 0);

                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 3)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Amount)");
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);
                        model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, 1, 0, 0, 0);
                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 4) // Year
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Quantity)");

                        model.FromDate = new DateTime(model.FromDate.Year, 1, 1, 0, 0, 0);
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);

                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 5)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Amount)");

                        model.FromDate = new DateTime(model.FromDate.Year, 1, 1, 0, 0, 0);
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);

                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o =>o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    //_data = _data.OrderBy(o => o.ItemName).ToList();
                    GetItemData(title, ref ws, ref row, _data, sumTotal);
                }
                #endregion
                //Get StoreName 
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Merge().SetValue(storeName);
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetBold(true);
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetFontSize(16);
                ///============
                string _symbol = "$";//HQContext.R_Currencies.Where(s => s.StoreCreate == _store.Index && s.IsSelected == true).Select(s => s.Symbol).FirstOrDefault();
                ws.Cell("E" + startRow_Store).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency") + ": " + _symbol);
                ws.Cell("E" + startRow_Store).Style.Font.SetBold(true);
                ws.Cell("E" + startRow_Store).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                if (model.ListStores.Count > 1)
                {
                    ws.Range("A" + row + ":E" + row + "").Merge().SetValue("");
                    row++;
                    startRow_Store = row;
                    endRow_Store = row;
                    row++;
                }
            }
            //Set Border 
            ws.Range("A1:E" + (row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A1:E" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();
            return wb;
        }

        //create 06/04/2018
        public XLWorkbook ExportExcel_New_v2(RPTopSellingProductsModels model, List<StoreModels> lstStore, string CurrencySymbol)
        {
            string sheetName = "Top_" + model.TopSell + "_Selling_Products";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            CreateReportHeaderForTopSale(ws, 5, model.FromDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Products Report"));

            DateTime fromDate = model.FromDate;
            int row = 4;
            int startRow = row;
            int startRow_Store = row;
            int endRow_Store = row;
            int startRow_User = row;
            int endRow_User = row;

            startRow_Store = row;
            endRow_Store = row;
            row++;

            string storeId = string.Empty, storeName = string.Empty;
            lstStore = lstStore.OrderBy(o => o.CompanyName).ThenBy(o => o.Name).ToList();

            for (int i = 0; i < lstStore.Count; i++)
            {
                ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
                ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("% of Sales"));
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("A" + row + ":E" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                row++;

                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("in") + " " + lstStore[i].CompanyName;

                #region======Get List Data
                string title = "";
                double sumTotal = 0;
                model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                for (int m = 0; m < 6; m++)
                {
                    sumTotal = 0;
                    ws.Row(row).Height = 24;
                    List<TopSellingProductsReportModels> _data = null;
                    if (m == 0)//Day
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Quantity) (as of") + " " + model.FromDate.ToString("MM/dd/yyyy") + ")";

                        model.ToDate = model.FromDate;
                        this.SetDate(ref model);
                        _data = GetDataTopSellByQty_v2(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 1)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Amount) (as of") + " " + model.FromDate.ToString("MM/dd/yyyy") + ")";
                        model.ToDate = model.FromDate;
                        this.SetDate(ref model);
                        _data = GetDataTopSellByAmount_2(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 2)//Month
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Quantity)");

                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);
                        model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, 1, 0, 0, 0);

                        _data = GetDataTopSellByQty_v2(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 3)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Amount)");
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);
                        model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, 1, 0, 0, 0);
                        _data = GetDataTopSellByAmount_2(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 4) // Year
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Quantity)");

                        model.FromDate = new DateTime(model.FromDate.Year, 1, 1, 0, 0, 0);
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);

                        _data = GetDataTopSellByQty_v2(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 5)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Amount)");

                        model.FromDate = new DateTime(model.FromDate.Year, 1, 1, 0, 0, 0);
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);

                        _data = GetDataTopSellByAmount_2(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    GetItemData(title, ref ws, ref row, _data, sumTotal);
                }
                #endregion
                //Get StoreName 
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Merge().SetValue(storeName);
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetBold(true);
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetFontSize(16);
                ///============
                ws.Cell("E" + startRow_Store).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency") + ": " + CurrencySymbol);
                ws.Cell("E" + startRow_Store).Style.Font.SetBold(true);
                ws.Cell("E" + startRow_Store).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("E" + startRow_Store).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                if (model.ListStores.Count > 1)
                {
                    ws.Range("A" + row + ":E" + row + "").Merge().SetValue("");
                    row++;
                    startRow_Store = row;
                    endRow_Store = row;
                    row++;
                }
            }
            //Set Border 
            ws.Range("A1:E" + (row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A1:E" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 40;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 20;
            ws.Column(5).Width = 20;
            return wb;
        }


        // Updated 02/01/2018
        public XLWorkbook ExportExcel_New(RPTopSellingProductsModels model, List<StoreModels> lstStore, string CurrencySymbol)
        {
            string sheetName = "Top_" + model.TopSell + "_Selling_Products";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            CreateReportHeaderForTopSale(ws, 5, model.FromDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Products Report"));

            DateTime fromDate = model.FromDate;
            //DateTime toDate = model.ToDate;

            int row = 4;
            int startRow = row;
            int startRow_Store = row;
            int endRow_Store = row;
            int startRow_User = row;
            int endRow_User = row;

            startRow_Store = row;
            endRow_Store = row;
            row++;

            string storeId = string.Empty, storeName = string.Empty;
            lstStore = lstStore.OrderBy(o => o.CompanyName).ThenBy(o => o.Name).ToList();
            for (int i = 0; i < lstStore.Count; i++)
            {
                ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
                ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("% of Sales"));
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("A" + row + ":E" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                row++;

                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("in") + " " + lstStore[i].CompanyName;

                #region======Get List Data
                string title = "";
                double sumTotal = 0;
                model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                for (int m = 0; m < 6; m++)
                {
                    sumTotal = 0;
                    ws.Row(row).Height = 24;
                    List<TopSellingProductsReportModels> _data = null;
                    if (m == 0)//Day
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Quantity) (as of") + " " + model.FromDate.ToString("MM/dd/yyyy") + ")";

                        model.ToDate = model.FromDate;
                        this.SetDate(ref model);
                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 1)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Amount) (as of") + " " + model.FromDate.ToString("MM/dd/yyyy") + ")";
                        model.ToDate = model.FromDate;
                        this.SetDate(ref model);
                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 2)//Month
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Quantity)");

                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);
                        model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, 1, 0, 0, 0);

                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 3)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Amount)");
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);
                        model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, 1, 0, 0, 0);
                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 4) // Year
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Quantity)");

                        model.FromDate = new DateTime(model.FromDate.Year, 1, 1, 0, 0, 0);
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);

                        _data = GetDataTopSellByQty(model, storeId);
                        // Sort by Qty
                        _data = _data.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    else if (m == 5)
                    {
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Amount)");

                        model.FromDate = new DateTime(model.FromDate.Year, 1, 1, 0, 0, 0);
                        model.ToDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 23, 59, 59);

                        _data = GetDataTopSellByAmount(model, storeId);
                        // Sort by Amount
                        _data = _data.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).ToList();
                        sumTotal = GetTotalReceipt(model.FromDate, model.ToDate, storeId);
                    }
                    GetItemData(title, ref ws, ref row, _data, sumTotal);
                }
                #endregion
                //Get StoreName 
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Merge().SetValue(storeName);
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetBold(true);
                ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetFontSize(16);
                ///============
                ws.Cell("E" + startRow_Store).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency") + ": " + CurrencySymbol);
                ws.Cell("E" + startRow_Store).Style.Font.SetBold(true);
                ws.Cell("E" + startRow_Store).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("E" + startRow_Store).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                if (model.ListStores.Count > 1)
                {
                    ws.Range("A" + row + ":E" + row + "").Merge().SetValue("");
                    row++;
                    startRow_Store = row;
                    endRow_Store = row;
                    row++;
                }
            }
            //Set Border 
            ws.Range("A1:E" + (row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A1:E" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 40;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 20;
            ws.Column(5).Width = 20;
            return wb;
        }

        public void GetItemData(string title, ref IXLWorksheet ws, ref int row, IEnumerable<TopSellingProductsReportModels> data, double sumTotal)
        {
            ws.Range(string.Format("A{0}:E{0}", row)).Merge().SetValue(title);
            ws.Range(string.Format("A{0}:E{0}", row)).Style.Font.SetBold(true);
            ws.Range(string.Format("A{0}:E{0}", row)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#ececec"));
            ws.Range(string.Format("A{0}:E{0}", row)).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            row++;
            double percent = 0;
            foreach (var item in data)
            {
                ws.Cell("A" + (row)).SetValue(item.ItemName);
                ws.Cell("B" + (row)).SetValue(item.Qty);
                ws.Cell("B" + (row)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell("B" + (row)).Style.NumberFormat.Format = "#,##0";
                ws.Cell("C" + (row)).SetValue(item.Discount);
                ws.Cell("C" + (row)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell("C" + (row)).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell("D" + (row)).SetValue(item.Amount);
                ws.Cell("D" + (row)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell("D" + (row)).Style.NumberFormat.Format = "#,##0.00";
                percent = Math.Round((item.Amount) * 100 / sumTotal, 2);
                ws.Cell("E" + (row)).SetValue(percent + "%");
                ws.Cell("E" + (row)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                row++;
            }
            ///
            if (data.Count() == 0)
            {
                ws.Range(string.Format("A{0}:E{0}", row)).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Not Data"));
                row++;
            }
        }

        public void SetDate(ref RPTopSellingProductsModels model)
        {
            model.FromDate = DateTimeHelper.SetFromDate(model.FromDate);
            model.ToDate = DateTimeHelper.SetToDate(model.ToDate);
        }

        #region Report with new DB, from table [R_PosSale], [R_PosSaleDetail]
        public List<TopSellingProductsReportModels> GetDataTopSell_NewDB(RPTopSellingProductsModels model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from ps in cxt.R_PosSale
                               from psd in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                               where model.ListStores.Contains(ps.StoreId) && ps.ReceiptCreatedDate >= model.FromDate && ps.ReceiptCreatedDate <= model.ToDate
                                   && !string.IsNullOrEmpty(psd.ItemId) // Not Credit Note order
                                   && psd.Mode == model.Mode
                                   && (psd.ItemTypeId == (int)Commons.EProductType.Dish || psd.ItemTypeId == (int)Commons.EProductType.SetMenu)
                               group psd by new
                               {
                                   StoreId = psd.StoreId,
                                   ItemId = psd.ItemId,
                                   ItemName = psd.ItemName,
                                   Year = ps.ReceiptCreatedDate.Value.Year,
                                   Month = ps.ReceiptCreatedDate.Value.Month,
                                   Day = ps.ReceiptCreatedDate.Value.Day
                               } into gg
                               select new TopSellingProductsReportModels
                               {
                                   StoreId = gg.Key.StoreId,
                                   ItemId = gg.Key.ItemId,
                                   ItemName = gg.Key.ItemName,
                                   Year = gg.Key.Year,
                                   Month = gg.Key.Month,
                                   Day = gg.Key.Day,
                                   Amount = gg.Sum(ss => ss.TotalAmount),
                                   Qty = gg.Sum(ss => ss.Quantity),
                                   Discount = gg.Sum(ss => (ss.IsDiscountTotal != true ? 0 : ss.Discount))
                               }).ToList();
                return lstData;
            }
        }

        public List<ClosedReceiptReportModels> GetTotalReceipt_NewDB(DateTime dFrom, DateTime dTo, List<string> listStoreId, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var data = cxt.R_PosSale.Where(ww => ww.ReceiptCreatedDate >= dFrom && ww.ReceiptCreatedDate <= dTo
                        && listStoreId.Contains(ww.StoreId)
                        && string.IsNullOrEmpty(ww.CreditNoteNo) // Only Receipt, not Credit Note
                        && ww.Mode == mode)
                    .GroupBy(gg => new
                    {
                        StoreId = gg.StoreId,
                        Year = gg.ReceiptCreatedDate.Value.Year,
                        Month = gg.ReceiptCreatedDate.Value.Month,
                        Day = gg.ReceiptCreatedDate.Value.Day,
                    })
                    .Select(ss => new ClosedReceiptReportModels
                    {
                        StoreId = ss.Key.StoreId,
                        Year = ss.Key.Year,
                        Month = ss.Key.Month,
                        Day = ss.Key.Day,
                        Total = ss.Sum(su => su.ReceiptTotal)
                    }).ToList();
                    
                return data;
            }
        }
        public XLWorkbook ExportExcel_NewDB(RPTopSellingProductsModels model, List<StoreModels> lstStore, string CurrencySymbol)
        {
            string sheetName = "Top_" + model.TopSell + "_Selling_Products";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            CreateReportHeaderForTopSale(ws, 5, model.FromDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Products Report"));
            // Format header report
            ws.Range(1, 1, 3, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 3, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            model.ToDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 23, 59, 59); ;
            model.FromDate = new DateTime(model.ToDate.Year, 1, 1, 0, 0, 0);

            // Get data
            var listData = GetDataTopSell_NewDB(model);

            if (listData != null && listData.Any())
            {
                var lstReceiptTotal = GetTotalReceipt_NewDB(model.FromDate, model.ToDate, model.ListStores, model.Mode);

                int row = 4;
                int startRow = row;
                int startRow_Store = row;
                int endRow_Store = row;
                int startRow_User = row;
                int endRow_User = row;
                row++;
                string storeName = "";
                string title = "";
                double sumTotal = 0;
                List<TopSellingProductsReportModels> lstDataTime = new List<TopSellingProductsReportModels>();
                List<TopSellingProductsReportModels> lstItmTime = new List<TopSellingProductsReportModels>();
                List<ClosedReceiptReportModels> lstReceiptTotalTime = new List<ClosedReceiptReportModels>();

                lstStore = lstStore.OrderBy(o => o.CompanyName).ThenBy(o => o.Name).ToList();
                int countStore = model.ListStores.Count;
                foreach (var store in lstStore)
                {
                    var lstDataStore = listData.Where(w => w.StoreId == store.Id).ToList();

                    if (lstDataStore != null && lstDataStore.Any())
                    {
                        ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
                        ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                        ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                        ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                        ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("% of Sales"));
                        ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                        ws.Range("A" + row + ":E" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        row++;

                        storeName = store.Name + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("in") + " " + store.CompanyName;

                        #region Data by day
                        lstDataTime = lstDataStore.Where(ww => ww.Month == model.ToDate.Month && ww.Day == model.ToDate.Day)
                            .GroupBy(gg => new { gg.ItemId, gg.ItemName })
                            .Select(ss => new TopSellingProductsReportModels() {
                                ItemId = ss.Key.ItemId,
                                ItemName = ss.Key.ItemName,
                                Amount = ss.Sum(su => su.Amount),
                                Qty = ss.Sum(su => su.Qty),
                                Discount = ss.Sum(su => su.Discount)
                            }).ToList();
                        lstReceiptTotalTime = lstReceiptTotal.Where(ww => ww.Month == model.ToDate.Month && ww.Day == model.ToDate.Day).ToList();
                        sumTotal = lstReceiptTotalTime.Sum(s => s.Total);
                        ws.Row(row).Height = 24;
                        // Set value
                        //// Sort by Qty
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Quantity) (as of") + " " + model.ToDate.ToString("MM/dd/yyyy") + ")";
                        lstItmTime = lstDataTime.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).Take(model.TopSell).ToList();
                        GetItemData(title, ref ws, ref row, lstItmTime, sumTotal);
                        //// Sort by Amount
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (by Amount) (as of") + " " + model.ToDate.ToString("MM/dd/yyyy") + ")";
                        lstItmTime = lstDataTime.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).Take(model.TopSell).ToList();
                        GetItemData(title, ref ws, ref row, lstItmTime, sumTotal);
                        #endregion Data by day

                        #region Data by month
                        lstDataTime = lstDataStore.Where(ww => ww.Month == model.ToDate.Month)
                            .GroupBy(gg => new { gg.ItemId, gg.ItemName })
                            .Select(ss => new TopSellingProductsReportModels()
                            {
                                ItemId = ss.Key.ItemId,
                                ItemName = ss.Key.ItemName,
                                Amount = ss.Sum(su => su.Amount),
                                Qty = ss.Sum(su => su.Qty),
                                Discount = ss.Sum(su => su.Discount)
                            }).ToList();
                        lstReceiptTotalTime = lstReceiptTotal.Where(ww => ww.Month == model.ToDate.Month).ToList();
                        sumTotal = lstReceiptTotalTime.Sum(s => s.Total);
                        ws.Row(row).Height = 24;
                        // Set value
                        //// Sort by Qty
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Quantity)");
                        lstItmTime = lstDataTime.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).Take(model.TopSell).ToList();
                        GetItemData(title, ref ws, ref row, lstItmTime, sumTotal);
                        //// Sort by Amount
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Month to Date by Amount)");
                        lstItmTime = lstDataTime.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).Take(model.TopSell).ToList();
                        GetItemData(title, ref ws, ref row, lstItmTime, sumTotal);
                        #endregion Data by month

                        #region Data by year
                        lstDataStore = lstDataStore.GroupBy(gg => new { gg.ItemId, gg.ItemName })
                            .Select(ss => new TopSellingProductsReportModels()
                             {
                                 ItemId = ss.Key.ItemId,
                                 ItemName = ss.Key.ItemName,
                                 Amount = ss.Sum(su => su.Amount),
                                 Qty = ss.Sum(su => su.Qty),
                                 Discount = ss.Sum(su => su.Discount)
                             }).ToList();
                        sumTotal = lstReceiptTotal.Sum(s => s.Total);
                        ws.Row(row).Height = 24;
                        // Set value
                        //// Sort by Qty
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Quantity)");
                        lstItmTime = lstDataStore.OrderByDescending(o => o.Qty).ThenBy(o => o.ItemName).Take(model.TopSell).ToList();
                        GetItemData(title, ref ws, ref row, lstItmTime, sumTotal);
                        //// Sort by Amount
                        title = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Top") + " " + model.TopSell + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selling Items (Year to Date by Amount)");
                        lstItmTime = lstDataStore.OrderByDescending(o => o.Amount).ThenBy(o => o.ItemName).Take(model.TopSell).ToList();
                        GetItemData(title, ref ws, ref row, lstItmTime, sumTotal);
                        #endregion Data by year

                        // Get StoreName 
                        ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Merge().SetValue(storeName);
                        ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetBold(true);
                        ws.Range(string.Format("A{0}:D{0}", startRow_Store)).Style.Font.SetFontSize(16);
                        ///============
                        ws.Cell("E" + startRow_Store).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency") + ": " + CurrencySymbol);
                        ws.Cell("E" + startRow_Store).Style.Font.SetBold(true);
                        ws.Cell("E" + startRow_Store).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Cell("E" + startRow_Store).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                        if (countStore > 1)
                        {
                            ws.Range("A" + row + ":E" + row + "").Merge().SetValue("");
                            row++;
                            startRow_Store = row;
                            endRow_Store = row;
                            row++;
                        }
                    }

                }

                // Set Border 
                ws.Range("A1:E" + (row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range("A1:E" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();
                ws.Column(1).Width = 40;
                ws.Column(2).Width = 15;
                ws.Column(3).Width = 20;
                ws.Column(4).Width = 20;
                ws.Column(5).Width = 20;
            }
            
            return wb;
        }
        #endregion Report with new DB, from table [R_PosSale], [R_PosSaleDetail]
    }
}
