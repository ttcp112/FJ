using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using System.Data.Entity.SqlServer;
using ClosedXML.Excel;
using System.Data.Entity;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Shared.Factory
{
    public class HourlyItemizedSalesReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public HourlyItemizedSalesReportFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public bool Insert(List<HourlyItemizedSalesReportModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_HourlyItemizedSalesReport.Where(ww => ww.StoreId == info.StoreId
                        && ww.BusinessId == info.BusinessId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Hourly Itemized Sales data exist");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_HourlyItemizedSalesReport> lstInsert = new List<R_HourlyItemizedSalesReport>();
                        R_HourlyItemizedSalesReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_HourlyItemizedSalesReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.ItemTypeId = item.ItemTypeId;
                            itemInsert.CategoryId = item.CategoryId;
                            itemInsert.CategoryName = item.CategoryName;
                            itemInsert.TotalPrice = item.TotalPrice;
                            itemInsert.Mode = item.Mode;
                            itemInsert.Discount = item.Discount;
                            itemInsert.Promotion = item.Promotion;
                            itemInsert.BusinessId = item.BusinessId;
                            itemInsert.ItemId = item.ItemId;
                            itemInsert.IsDiscountTotal = item.IsDiscountTotal;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_HourlyItemizedSalesReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Hourly Itemized Sales data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Hourly Itemized Sales data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_HourlyItemizedSalesReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<HourlyItemizedSalesReportModels> GetDataHour(RPHourlyItemizedSalesModels model, List<string>lstStoreCate, List<string>lstStoreSet, List<string> listCategoryIds, List<string> listSetMenuIds)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_HourlyItemizedSalesReport
                                   //where model.ListStores.Contains(tb.StoreId)
                                   //      && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                                   //      && (listCategoryIds.Contains(tb.CategoryId) || (tb.ItemTypeId == (int)Commons.EProductType.SetMenu
                                   //      && listSetMenuIds.Contains(tb.ItemId)))
                                   //      && tb.Mode == model.Mode

                               where (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                                     && ((listCategoryIds.Contains(tb.CategoryId) && lstStoreCate.Contains(tb.StoreId)) || (tb.ItemTypeId == (int)Commons.EProductType.SetMenu
                                     && listSetMenuIds.Contains(tb.ItemId) && lstStoreSet.Contains(tb.StoreId)))
                                     && tb.Mode == model.Mode

                               orderby tb.CreatedDate, tb.ItemTypeId, tb.CategoryName
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   ItemTypeId = tb.ItemTypeId,
                                   Hour = (int?)SqlFunctions.DatePart("hh", tb.CreatedDate),
                                   CategoryId = tb.CategoryId,
                                   CategoryName = tb.CategoryName,
                                   CreatedDate = DbFunctions.TruncateTime(tb.CreatedDate),
                                   BusinessId = tb.BusinessId,
                                   IsDiscountTotal = tb.IsDiscountTotal
                               } into g
                               where g.Key.Hour != null
                               select new HourlyItemizedSalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   ItemTypeId = g.Key.ItemTypeId,
                                   Hour = g.Key.Hour.Value,
                                   CreatedDate = g.Key.CreatedDate.Value,
                                   CategoryId = g.Key.CategoryId,
                                   CategoryName = g.Key.CategoryName,
                                   BusinessId = g.Key.BusinessId,
                                   TotalPrice = g.Sum(x => x.TotalPrice),
                                   Discount = g.Sum(x => x.Discount),
                                   Promotion = g.Sum(x => x.Promotion),
                                   IsDiscountTotal = g.Key.IsDiscountTotal.HasValue ? g.Key.IsDiscountTotal.Value : false
                               }).ToList();
                return lstData;
            }
        }

        public XLWorkbook Export(List<HourlyItemizedSalesReportModels> model, DateTime fromDate, DateTime toDate, List<RFilterCategoryModel> listCateNames
            , List<RFilterCategoryModel> listSetMenuNames, List<DiscountAndMiscReportModels> listMiscDiscountItems,
            List<StoreModels> ListStores, int Mode, List<BusinessDayDisplayModels> _lstBusDayAllStore, RPHourlyItemizedSalesModels baseModel)
        {
            //set first sheet values
            XLWorkbook wb = new XLWorkbook();
            XLColor backgroundTitle = XLColor.FromHtml("#d9d9d9");
            XLColor backgroundStoreTitle = XLColor.FromHtml("#c6efce");
            XLColor outsideBorderColor = XLColor.FromHtml("#000000");
            XLColor insideBorderColor = XLColor.FromHtml("#000000");
            XLColor backgroundTitleDay = XLColor.FromHtml("#ffcc99");

            //Create worksheet
            IXLWorksheet ws = wb.Worksheets.Add("Hourly_Itemized_Sales_Report");

            int endIndexColumnDefault = 2;

            //Header
            CreateReportHeaderNew(ws, endIndexColumnDefault, fromDate, toDate
                , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Itemized Sales Report").ToUpper());

            int index = 5;
            int endIndexColumn = 5;

            List<int> listLeftAlignmentIndexes = new List<int>();
            var listStoresId = ListStores.Select(x => x.Id).ToList();
            //var _lstBusDayAllStore = _baseFactory.GetBusinessDays(fromDate, toDate, listStoresId, Mode);
            string storeId = string.Empty, storeName = string.Empty;
            //double _taxValue = 0;
            //ItemizedSalesAnalysisReportFactory _itemizedSalesAnalysisReportFactory = new ItemizedSalesAnalysisReportFactory();

            //var _lstTaxes = _itemizedSalesAnalysisReportFactory.GetListItemForHourlyItemSale(baseModel);
            TaxFactory _taxFactory = new TaxFactory();

            // Get rounding value
            var lstRoundValue = GetRoundingForHourly(listStoresId, fromDate, toDate, Mode);

            for (int n = 0; n < ListStores.Count; n++)
            {
                StoreModels store = ListStores[n];
                storeId = store.Id;
                storeName = store.Name;
                int startIndex = index;

                //_taxValue = 0;
                var taxType = _taxFactory.GetTaxTypeForStore(storeId);

                ////// List category in store
                //var listCateInStore = listCateNames.Where(yy => yy.StoreId == storeId && yy.ListChilds.Count == 0)
                //    .OrderBy(yy => yy.Name)
                //    .Select(yy => new { yy.Name, yy.Id }).Distinct().ToList();

                // Get all parent & child category, updated 03212018
                var listCateInStore = listCateNames.Where(yy => yy.StoreId == storeId)
                    .OrderBy(yy => yy.Name)
                    .Select(yy => new { yy.Name, yy.Id }).Distinct().ToList();
                var listCateNamesInStore = listCateInStore.OrderBy(oo => oo.Name).Select(ss => ss.Name).ToList<string>();

                // List set menu name in store
                List<string> listSetMenuNamesInStore = listSetMenuNames.Where(yy => yy.StoreId == storeId)
                    .OrderBy(yy => yy.Name).Select(yy => yy.Name)
                    .Distinct().ToList();

                if (listCateNamesInStore.Count > 0)
                {
                    endIndexColumn += listCateNamesInStore.Count;
                }

                // Format title of report
                if (n == 0)
                {
                    ws.Range(1, 1, 1, endIndexColumn).Merge();
                    ws.Range(2, 1, 2, endIndexColumn).Merge();
                    ws.Range(3, 1, 3, endIndexColumn).Merge();
                    ws.Range(4, 1, 4, endIndexColumn).Merge();

                    ws.Range(1, 1, 4, endIndexColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(1, 1, 4, endIndexColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(1, 1, 4, endIndexColumn).Style.Border.OutsideBorderColor = outsideBorderColor;
                    ws.Range(1, 1, 4, endIndexColumn).Style.Border.InsideBorderColor = insideBorderColor;
                    ws.Range(1, 1, 4, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitle);
                }

                // Title Store/Merchant name
                ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(string.Format("{0}", storeName));
                ws.Range(index, 1, index, endIndexColumn).Style.Font.SetFontSize(16).Font.SetBold(true);
                ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundStoreTitle);
                ws.Range(index, 1, index, endIndexColumn).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Row(index).Height = 22;
                listLeftAlignmentIndexes.Add(index);

                index++;

                // Get all business day in store
                var businessInStore = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                var businessIdInStore = businessInStore.Select(aa => aa.Id).ToList();

                // Get data in a store by list business day
                List<HourlyItemizedSalesReportModels> listData = model.Where(xx => xx.StoreId == storeId && businessIdInStore.Contains(xx.BusinessId)).ToList();

                // List date time of list data
                var listDateTimeInStore = listData.GroupBy(zz => new { zz.CreatedDate })
                    .Select(zzz => new
                    {
                        CreatedDate = zzz.Key.CreatedDate,
                        listHour = zzz.OrderBy(aa => aa.Hour).Select(aa => aa.Hour).Distinct().ToList()
                    }).OrderBy(zz => zz.CreatedDate).ToList();

                int minHour = 0;
                int maxHour = 0;
                decimal grandItemTotal = 0;

                // List Category and Set Menu Selected
                ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Category");
                ws.Range(index, 2, index, endIndexColumn).Merge().SetValue(string.Format("{0}", string.Join(", ", listCateNamesInStore)));
                ws.Range(index, 2, index++, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu");
                ws.Range(index, 2, index, endIndexColumn).Merge().SetValue(string.Format("{0}", string.Join(", ", listSetMenuNamesInStore)));
                ws.Range(index, 2, index++, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                for (int d = 0; d < listDateTimeInStore.Count; d++)
                {
                    // Get list time in a day
                    var ListTimes = listDateTimeInStore[d].listHour;
                    minHour = listDateTimeInStore[d].listHour.FirstOrDefault();
                    maxHour = listDateTimeInStore[d].listHour.LastOrDefault();

                    // Current time report
                    //ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(string.Format("{0}:00 - {1}:00 {2}", minHour, maxHour, listDateTimeInStore[d].CreatedDate.ToString("dd/MM/yyyy")));
                    ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(listDateTimeInStore[d].CreatedDate.ToString("MM/dd/yyyy"));
                    ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitleDay);
                    ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range(index, 1, index++, endIndexColumn).Style.Font.SetBold(true);

                    for (int i = 0; i < ListTimes.Count; i++)
                    {
                        // Time title
                        ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(string.Format("{0}:00 - {1}:00", ListTimes[i], (ListTimes[i] + 1)));
                        ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitle);
                        ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);
                        listLeftAlignmentIndexes.Add(index);

                        index++;

                        // List titles
                        List<object> listCateItem = new List<object>();
                        decimal discountTotalBill = 0;
                        decimal promotionTotal = 0;
                        decimal setmenuTotal = 0;
                        decimal miscTotal = 0;
                        decimal discountItemTmp = 0;
                        decimal tmp = 0;
                        decimal roundValue = 0;

                        foreach (var item in listCateInStore)
                        {
                            var itemHours = listData.Where(dd => dd.Hour == ListTimes[i] && dd.CategoryId == item.Id
                            && listDateTimeInStore[d].CreatedDate == dd.CreatedDate).ToList();

                            //_taxValue = 0;
                            //if (taxType == (int)Commons.ETax.Inclusive)
                            //{
                            //    _taxValue = _lstTaxes.Where(dd => dd.Hour == ListTimes[i] && dd.CategoryId == item.Id
                            //&& listDateTimeInStore[d].CreatedDate == dd.CreatedDate).Sum(ss => ss.Tax);
                            //}
                            if (itemHours != null && itemHours.Any())
                            {
                                listCateItem.Add(item.Name);
                                //listCateItem.Add(itemHour.TotalPrice);
                                tmp = itemHours.Sum(ss => (decimal)ss.TotalPrice - (decimal)ss.Promotion);
                                discountItemTmp = itemHours.Where(ww => !ww.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);
                                //if (taxType == (int)Commons.ETax.Inclusive)
                                //    tmp -= (decimal)_taxValue;

                                listCateItem.Add(tmp - discountItemTmp);

                                discountTotalBill += itemHours.Where(ww => ww.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);
                                promotionTotal += itemHours.Sum(ss => (decimal)ss.Promotion);
                            }
                            else
                            {
                                listCateItem.Add(item.Name);
                                listCateItem.Add(0);
                            }
                        }

                        // Get setmenu in store, depend on createDate
                        setmenuTotal = listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                        && dd.ItemTypeId == (int)Commons.EProductType.SetMenu).Sum(s => (decimal)s.TotalPrice
                        //- (decimal)s.Discount 
                        - (s.IsDiscountTotal ? 0 : (decimal)s.Discount)
                        - (decimal)s.Promotion);

                        discountTotalBill += listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                        && dd.ItemTypeId == (int)Commons.EProductType.SetMenu && dd.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);

                        promotionTotal += listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                             && dd.ItemTypeId == (int)Commons.EProductType.SetMenu).Sum(s => (decimal)s.Promotion);
                        // Get Misc in store, depend on createDate
                        miscTotal = listMiscDiscountItems.Where(m => m.Hour == ListTimes[i] && m.StoreId == storeId
                        && listDateTimeInStore[d].CreatedDate.Year == m.CreatedDate.Year
                        && listDateTimeInStore[d].CreatedDate.Month == m.CreatedDate.Month
                        && listDateTimeInStore[d].CreatedDate.Day == m.CreatedDate.Day).Sum(yy => (decimal)yy.MiscValue);

                        decimal hourlyItemTotal = 0;
                        int column = 0;
                        if (listCateItem.Count == 0)
                        {
                            column = 1;
                        }
                        else
                        {
                            int k = 0;
                            for (int j = 0; j < listCateItem.Count; j += 2)
                            {
                                string CategoryName = listCateItem[j].ToString();
                                decimal TotalPrice = decimal.Parse(listCateItem[j + 1].ToString());

                                column = 2 + k;
                                // Category name
                                ws.Cell(index, 2 + k).Value = string.Format("{0}", CategoryName);
                                // Set width of cells
                                ws.Column(2 + k).Width = ws.Cell(index, 2 + k).Value.ToString().Length + 5;

                                // Prices
                                ws.Cell(index + 1, 2 + k).Value = TotalPrice.ToString("F");

                                hourlyItemTotal += TotalPrice;

                                k++;
                            }
                        }

                        // Set Menu total
                        column++;
                        ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu");
                        ws.Cell(index + 1, column).Value = setmenuTotal;
                        ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                        // Misc total
                        column++;
                        ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Misc");
                        ws.Cell(index + 1, column).Value = miscTotal.ToString("F");
                        ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                        // Discount total
                        column++;
                        ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill");
                        ws.Cell(index + 1, column).Value = discountTotalBill.ToString("F");
                        ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                        // Promotion total
                        column++;
                        ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Promotion");
                        ws.Cell(index + 1, column).Value = promotionTotal.ToString("F");
                        ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                        ws.Range(index, 2, index, column).Style.Fill.SetBackgroundColor(backgroundTitle);
                        ws.Range(index, 2, index, column).Style.Font.SetBold(true);

                        // Rounding value of this time
                        roundValue = lstRoundValue.Where(m => m.Time == ListTimes[i] && m.StoreId == storeId).Sum(ss => (decimal)ss.RoundingValue);
                        //hourlyItemTotal += setmenuTotal + miscTotal - discountTotalBill;
                        hourlyItemTotal += setmenuTotal + miscTotal - discountTotalBill + roundValue; // Updated 03032018, + rounding value
                        grandItemTotal += hourlyItemTotal;

                        // Hourly item total
                        index += 2;
                        ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Item Total");

                        ws.Range(index, 2, index, endIndexColumn).Merge().Value = hourlyItemTotal.ToString("F");
                        ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);

                        ws.Range(index - 2, 1, index, endIndexColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Range(index - 2, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Range(index - 1, 2, index, endIndexColumn).Style.NumberFormat.Format = "#,##0.00";
                        index++;
                    }
                }
                //grand item total
                ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Grand Item Total");
                ws.Range(index, 2, index, endIndexColumn).Merge().Value = grandItemTotal.ToString("F");
                ws.Range(index, 2, index, endIndexColumn).Style.NumberFormat.Format = "#,##0.00";
                ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);
                ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.OutsideBorderColor = outsideBorderColor;
                ws.Range(startIndex, 1, index++, endIndexColumn).Style.Border.InsideBorderColor = insideBorderColor;
                index++;
                endIndexColumn = 5;
            }
            foreach (var item in listLeftAlignmentIndexes)
            {
                ws.Range(item, 1, item, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Justify;
            }

            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 25;
            return wb;
        }

        //Get rouding for hourly sale report
        public List<HourlySalesReportModels> GetRoundingForHourly(List<string> ListStoreId, DateTime FromDate, DateTime ToDate, int Mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DailyReceiptReport
                               where ListStoreId.Contains(tb.StoreId)
                                     && tb.CreatedDate >= FromDate
                                             && tb.CreatedDate <= ToDate
                                             && tb.Mode == Mode
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   Time = (int?)SqlFunctions.DatePart("HH", tb.CreatedDate),
                               } into g

                               select new HourlySalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   Time = g.Key.Time.Value,
                                   RoundingValue = g.Sum(xx => xx.Rounding)
                               }).ToList();
                return lstData;
            }
        }

        #region Updated 03232018, get data from date input & filter from time input
        public XLWorkbook Export_NewFilter(List<HourlyItemizedSalesReportModels> model, DateTime fromDate, DateTime toDate, List<RFilterCategoryModel> listCateNames
            , List<RFilterCategoryModel> listSetMenuNames, List<DiscountAndMiscReportModels> listMiscDiscountItems,
            List<StoreModels> ListStores, int Mode, List<BusinessDayDisplayModels> _lstBusDayAllStore, int fromTime, int toTime)
        {
            //set first sheet values
            XLWorkbook wb = new XLWorkbook();

            try
            {
                XLColor backgroundTitle = XLColor.FromHtml("#d9d9d9");
                XLColor backgroundStoreTitle = XLColor.FromHtml("#c6efce");
                XLColor outsideBorderColor = XLColor.FromHtml("#000000");
                XLColor insideBorderColor = XLColor.FromHtml("#000000");
                XLColor backgroundTitleDay = XLColor.FromHtml("#ffcc99");

                //Create worksheet
                IXLWorksheet ws = wb.Worksheets.Add("Hourly_Itemized_Sales_Report");

                int endIndexColumnDefault = 2;

                //Header
                CreateReportHeaderNewFilterTime(ws, endIndexColumnDefault, fromDate, toDate, fromTime, toTime
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Itemized Sales Report").ToUpper());

                if (model != null && model.Any())
                {
                    int index = 5;
                    int endIndexColumn = 5;

                    List<int> listLeftAlignmentIndexes = new List<int>();
                    var lstStoreIdData = model.Select(ss => ss.StoreId).Distinct().ToList();

                    // Filter list stores from list data
                    ListStores = ListStores.Where(ww => lstStoreIdData.Contains(ww.Id)).OrderBy(oo => oo.Name).ToList();
                    var listStoresId = ListStores.Select(x => x.Id).ToList();

                    string storeId = string.Empty, storeName = string.Empty;
                    TaxFactory _taxFactory = new TaxFactory();

                    // Get rounding value
                    var lstRoundValue = GetRoundingForHourly_NewFilter(listStoresId, fromDate, toDate, Mode);

                    // List time range
                    List<int> listTimesRange = new List<int>();
                    // Time range: from < to
                    // Time: from => to
                    if (fromTime < toTime)
                    {
                        for (int i = fromTime; i < toTime; i++)
                        {
                            listTimesRange.Add(i);
                        }
                    }
                    else // Time range: from > to || from = to
                         // Time: from => 24 => 0 => to
                    {
                        // from => 24
                        for (int i = fromTime; i < 24; i++)
                        {
                            listTimesRange.Add(i);
                        }
                        // 0 => to
                        for (int i = 0; i < toTime; i++)
                        {
                            listTimesRange.Add(i);
                        }
                    }

                    for (int n = 0; n < ListStores.Count; n++)
                    {
                        StoreModels store = ListStores[n];
                        storeId = store.Id;
                        storeName = store.Name;
                        int startIndex = index;

                        var taxType = _taxFactory.GetTaxTypeForStore(storeId);

                        ////// List category in store
                        // Get all parent & child category, updated 03212018
                        var listCateInStore = listCateNames.Where(yy => yy.StoreId == storeId)
                            .OrderBy(yy => yy.Name)
                            .Select(yy => new { yy.Name, yy.Id }).Distinct().ToList();
                        var listCateNamesInStore = listCateInStore.OrderBy(oo => oo.Name).Select(ss => ss.Name).ToList<string>();

                        // List set menu name in store
                        List<string> listSetMenuNamesInStore = listSetMenuNames.Where(yy => yy.StoreId == storeId)
                            .OrderBy(yy => yy.Name).Select(yy => yy.Name)
                            .Distinct().ToList();

                        if (listCateNamesInStore.Count > 0)
                        {
                            endIndexColumn += listCateNamesInStore.Count;
                        }

                        // Format title of report
                        if (n == 0)
                        {
                            ws.Range(1, 1, 1, endIndexColumn).Merge();
                            ws.Range(2, 1, 2, endIndexColumn).Merge();
                            ws.Range(3, 1, 3, endIndexColumn).Merge();
                            ws.Range(4, 1, 4, endIndexColumn).Merge();

                            ws.Range(1, 1, 4, endIndexColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, endIndexColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, endIndexColumn).Style.Border.OutsideBorderColor = outsideBorderColor;
                            ws.Range(1, 1, 4, endIndexColumn).Style.Border.InsideBorderColor = insideBorderColor;
                            ws.Range(1, 1, 4, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitle);
                        }

                        // Title Store/Merchant name
                        ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(string.Format("{0}", storeName));
                        ws.Range(index, 1, index, endIndexColumn).Style.Font.SetFontSize(16).Font.SetBold(true);
                        ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundStoreTitle);
                        ws.Range(index, 1, index, endIndexColumn).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                        ws.Row(index).Height = 22;
                        listLeftAlignmentIndexes.Add(index);

                        index++;

                        // Get all business day in store
                        var businessInStore = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                        var businessIdInStore = businessInStore.Select(aa => aa.Id).ToList();

                        // Get data in a store by list business day, with hour in list time range
                        List<HourlyItemizedSalesReportModels> listData = model.Where(xx => xx.StoreId == storeId && businessIdInStore.Contains(xx.BusinessId) && listTimesRange.Contains(xx.Hour)).ToList();

                        // List date time of list data
                        var listDateTimeInStore = listData.GroupBy(zz => new { zz.CreatedDate })
                            .Select(zzz => new
                            {
                                CreatedDate = zzz.Key.CreatedDate,
                                listHour = zzz.OrderBy(aa => aa.Hour).Select(aa => aa.Hour).Distinct().ToList()
                            }).OrderBy(zz => zz.CreatedDate).ToList();

                        decimal grandItemTotal = 0;

                        // List Category and Set Menu Selected
                        ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Category");
                        ws.Range(index, 2, index, endIndexColumn).Merge().SetValue(string.Format("{0}", string.Join(", ", listCateNamesInStore)));
                        ws.Range(index, 2, index++, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu");
                        ws.Range(index, 2, index, endIndexColumn).Merge().SetValue(string.Format("{0}", string.Join(", ", listSetMenuNamesInStore)));
                        ws.Range(index, 2, index++, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        for (int d = 0; d < listDateTimeInStore.Count; d++)
                        {
                            // Get list time in a day
                            var ListTimes = listTimesRange.Where(ww => listDateTimeInStore[d].listHour.Contains(ww)).ToList();

                            // Current time report
                            ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(listDateTimeInStore[d].CreatedDate.ToString("MM/dd/yyyy"));
                            ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitleDay);
                            ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Range(index, 1, index++, endIndexColumn).Style.Font.SetBold(true);

                            for (int i = 0; i < ListTimes.Count; i++)
                            {
                                // Time title
                                ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(string.Format("{0}:00 - {1}:00", ListTimes[i], (ListTimes[i] + 1)));
                                ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitle);
                                ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);
                                listLeftAlignmentIndexes.Add(index);

                                index++;

                                // List titles
                                List<object> listCateItem = new List<object>();
                                decimal discountTotalBill = 0;
                                decimal promotionTotal = 0;
                                decimal setmenuTotal = 0;
                                decimal miscTotal = 0;
                                decimal discountItemTmp = 0;
                                decimal tmp = 0;
                                decimal roundValue = 0;

                                foreach (var item in listCateInStore)
                                {
                                    var itemHours = listData.Where(dd => dd.Hour == ListTimes[i] && dd.CategoryId == item.Id
                                    && listDateTimeInStore[d].CreatedDate == dd.CreatedDate).ToList();

                                    if (itemHours != null && itemHours.Any())
                                    {
                                        listCateItem.Add(item.Name);
                                        tmp = itemHours.Sum(ss => (decimal)ss.TotalPrice - (decimal)ss.Promotion);
                                        discountItemTmp = itemHours.Where(ww => !ww.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);

                                        listCateItem.Add(tmp - discountItemTmp);

                                        discountTotalBill += itemHours.Where(ww => ww.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);
                                        promotionTotal += itemHours.Sum(ss => (decimal)ss.Promotion);
                                    }
                                    else
                                    {
                                        listCateItem.Add(item.Name);
                                        listCateItem.Add(0);
                                    }
                                }

                                // Get setmenu in store, depend on createDate
                                setmenuTotal = listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                                && dd.ItemTypeId == (int)Commons.EProductType.SetMenu).Sum(s => (decimal)s.TotalPrice
                                - (s.IsDiscountTotal ? 0 : (decimal)s.Discount)
                                - (decimal)s.Promotion);

                                discountTotalBill += listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                                && dd.ItemTypeId == (int)Commons.EProductType.SetMenu && dd.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);

                                promotionTotal += listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                                     && dd.ItemTypeId == (int)Commons.EProductType.SetMenu).Sum(s => (decimal)s.Promotion);

                                // Get Misc in store, depend on createDate
                                miscTotal = listMiscDiscountItems.Where(m => m.Hour == ListTimes[i] && m.StoreId == storeId
                                && listDateTimeInStore[d].CreatedDate.Year == m.CreatedDate.Year
                                && listDateTimeInStore[d].CreatedDate.Month == m.CreatedDate.Month
                                && listDateTimeInStore[d].CreatedDate.Day == m.CreatedDate.Day).Sum(yy => (decimal)yy.MiscValue);

                                decimal hourlyItemTotal = 0;
                                int column = 0;
                                if (listCateItem.Count == 0)
                                {
                                    column = 1;
                                }
                                else
                                {
                                    int k = 0;
                                    for (int j = 0; j < listCateItem.Count; j += 2)
                                    {
                                        string CategoryName = listCateItem[j].ToString();
                                        decimal TotalPrice = decimal.Parse(listCateItem[j + 1].ToString());

                                        column = 2 + k;
                                        // Category name
                                        ws.Cell(index, 2 + k).Value = string.Format("{0}", CategoryName);
                                        // Set width of cells
                                        ws.Column(2 + k).Width = ws.Cell(index, 2 + k).Value.ToString().Length + 5;

                                        // Prices
                                        ws.Cell(index + 1, 2 + k).Value = TotalPrice.ToString("F");

                                        hourlyItemTotal += TotalPrice;

                                        k++;
                                    }
                                }

                                // Set Menu total
                                column++;
                                ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu");
                                ws.Cell(index + 1, column).Value = setmenuTotal;
                                ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                                // Misc total
                                column++;
                                ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Misc");
                                ws.Cell(index + 1, column).Value = miscTotal.ToString("F");
                                ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                                // Discount total
                                column++;
                                ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill");
                                ws.Cell(index + 1, column).Value = discountTotalBill.ToString("F");
                                ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                                // Promotion total
                                column++;
                                ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Promotion");
                                ws.Cell(index + 1, column).Value = promotionTotal.ToString("F");
                                ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                                ws.Range(index, 2, index, column).Style.Fill.SetBackgroundColor(backgroundTitle);
                                ws.Range(index, 2, index, column).Style.Font.SetBold(true);

                                // Rounding value of this time
                                roundValue = lstRoundValue.Where(m => m.CreatedDate.Hour == ListTimes[i] && m.StoreId == storeId
                                && listDateTimeInStore[d].CreatedDate.Year == m.CreatedDate.Year
                                && listDateTimeInStore[d].CreatedDate.Month == m.CreatedDate.Month
                                && listDateTimeInStore[d].CreatedDate.Day == m.CreatedDate.Day).Sum(yy => (decimal)yy.RoundingValue);


                                hourlyItemTotal += setmenuTotal + miscTotal - discountTotalBill + roundValue; // Updated 03032018, + rounding value
                                grandItemTotal += hourlyItemTotal;

                                // Hourly item total
                                index += 2;
                                ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Item Total");

                                ws.Range(index, 2, index, endIndexColumn).Merge().Value = hourlyItemTotal.ToString("F");
                                ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);

                                ws.Range(index - 2, 1, index, endIndexColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                ws.Range(index - 2, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                                ws.Range(index - 1, 2, index, endIndexColumn).Style.NumberFormat.Format = "#,##0.00";
                                index++;
                            }
                        }
                        //grand item total
                        ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Grand Item Total");
                        ws.Range(index, 2, index, endIndexColumn).Merge().Value = grandItemTotal.ToString("F");
                        ws.Range(index, 2, index, endIndexColumn).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);
                        ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.OutsideBorderColor = outsideBorderColor;
                        ws.Range(startIndex, 1, index++, endIndexColumn).Style.Border.InsideBorderColor = insideBorderColor;
                        index++;
                        endIndexColumn = 5;
                    }
                    foreach (var item in listLeftAlignmentIndexes)
                    {
                        ws.Range(item, 1, item, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Justify;
                    }
                }
                ws.Columns().AdjustToContents();
                ws.Column(1).Width = 25;
                return wb;
            }
            catch (Exception e)
            {
                return wb;
            }
        }

        //Get rouding for hourly sale report
        public List<HourlySalesReportModels> GetRoundingForHourly_NewFilter(List<string> ListStoreId, DateTime FromDate, DateTime ToDate, int Mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DailyReceiptReport
                               where ListStoreId.Contains(tb.StoreId)
                                     && tb.CreatedDate >= FromDate
                                             && tb.CreatedDate <= ToDate
                                             && tb.Mode == Mode
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                               } into g

                               select new HourlySalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   CreatedDate = g.Key.CreatedDate,
                                   RoundingValue = g.Sum(xx => xx.Rounding)
                               }).ToList();
                return lstData;
            }
        }
        #endregion End Updated 03232018, get data from date input & filter from time input



        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]
        public List<HourlyItemizedSalesReportModels> GetDataHour_NewDB(RPHourlyItemizedSalesModels model, List<string> lstStoreCate, List<string> lstStoreSet, List<string> listCategoryIds, List<string> listSetMenuIds)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from ps in cxt.R_PosSale
                               from tb in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                               where (ps.ReceiptCreatedDate >= model.FromDate && ps.ReceiptCreatedDate <= model.ToDate)
                                     && ((listCategoryIds.Contains(tb.CategoryId) && lstStoreCate.Contains(tb.StoreId)) || ((tb.ItemTypeId == (int)Commons.EProductType.SetMenu
                                     && listSetMenuIds.Contains(tb.ItemId)) && lstStoreSet.Contains(tb.StoreId)))
                                     && tb.Mode == model.Mode
                               orderby ps.ReceiptCreatedDate, tb.ItemTypeId, tb.CategoryName
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   ItemTypeId = tb.ItemTypeId,
                                   Hour = (int?)SqlFunctions.DatePart("hh", ps.ReceiptCreatedDate),
                                   CategoryId = tb.CategoryId,
                                   CategoryName = tb.CategoryName,
                                   CreatedDate = DbFunctions.TruncateTime(ps.ReceiptCreatedDate),
                                   BusinessId = tb.BusinessId,
                                   IsDiscountTotal = tb.IsDiscountTotal
                               } into g
                               where g.Key.Hour != null
                               select new HourlyItemizedSalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   ItemTypeId = g.Key.ItemTypeId,
                                   Hour = g.Key.Hour.Value,
                                   CreatedDate = g.Key.CreatedDate.Value,
                                   CategoryId = g.Key.CategoryId,
                                   CategoryName = g.Key.CategoryName,
                                   BusinessId = g.Key.BusinessId,
                                   TotalPrice = g.Sum(x => x.TotalAmount),
                                   Discount = g.Sum(x => x.Discount),
                                   Promotion = g.Sum(x => x.PromotionAmount),
                                   IsDiscountTotal = g.Key.IsDiscountTotal.HasValue ? g.Key.IsDiscountTotal.Value : false
                               }).ToList();
                return lstData;
            }
        }

        // Get rouding for hourly sale report
        public List<HourlySalesReportModels> GetRoundingForHourly_NewDB(List<string> ListStoreId, DateTime FromDate, DateTime ToDate, int Mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_PosSale
                               where ListStoreId.Contains(tb.StoreId)
                                     && tb.ReceiptCreatedDate >= FromDate
                                             && tb.ReceiptCreatedDate <= ToDate
                                             && tb.Mode == Mode
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.ReceiptCreatedDate,
                               } into g

                               select new HourlySalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   CreatedDate = g.Key.CreatedDate.Value,
                                   RoundingValue = g.Sum(xx => xx.Rounding)
                               }).ToList();
                return lstData;
            }
        }

        public XLWorkbook Export_NewDB(List<HourlyItemizedSalesReportModels> model, DateTime fromDate, DateTime toDate, List<RFilterCategoryModel> listCateNames
            , List<RFilterCategoryModel> listSetMenuNames, List<DiscountAndMiscReportModels> listMiscDiscountItems,
            List<StoreModels> ListStores, int Mode, List<BusinessDayDisplayModels> _lstBusDayAllStore, int fromTime, int toTime)
        {
            //set first sheet values
            XLWorkbook wb = new XLWorkbook();

            try
            {
                XLColor backgroundTitle = XLColor.FromHtml("#d9d9d9");
                XLColor backgroundStoreTitle = XLColor.FromHtml("#c6efce");
                XLColor outsideBorderColor = XLColor.FromHtml("#000000");
                XLColor insideBorderColor = XLColor.FromHtml("#000000");
                XLColor backgroundTitleDay = XLColor.FromHtml("#ffcc99");

                //Create worksheet
                IXLWorksheet ws = wb.Worksheets.Add("Hourly_Itemized_Sales_Report");

                int endIndexColumnDefault = 2;

                //Header
                CreateReportHeaderNewFilterTime(ws, endIndexColumnDefault, fromDate, toDate, fromTime, toTime
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Itemized Sales Report").ToUpper());

                if (model != null && model.Any())
                {
                    int index = 5;
                    int endIndexColumn = 5;

                    List<int> listLeftAlignmentIndexes = new List<int>();
                    var lstStoreIdData = model.Select(ss => ss.StoreId).Distinct().ToList();

                    // Filter list stores from list data
                    ListStores = ListStores.Where(ww => lstStoreIdData.Contains(ww.Id)).OrderBy(oo => oo.Name).ToList();
                    var listStoresId = ListStores.Select(x => x.Id).ToList();

                    string storeId = string.Empty, storeName = string.Empty;

                    // Get rounding value
                    var lstRoundValue = GetRoundingForHourly_NewDB(listStoresId, fromDate, toDate, Mode);

                    // List time range
                    List<int> listTimesRange = new List<int>();
                    // Time range: from < to
                    // Time: from => to
                    if (fromTime < toTime)
                    {
                        for (int i = fromTime; i < toTime; i++)
                        {
                            listTimesRange.Add(i);
                        }
                    }
                    else // Time range: from > to || from = to
                         // Time: from => 24 => 0 => to
                    {
                        // from => 24
                        for (int i = fromTime; i < 24; i++)
                        {
                            listTimesRange.Add(i);
                        }
                        // 0 => to
                        for (int i = 0; i < toTime; i++)
                        {
                            listTimesRange.Add(i);
                        }
                    }

                    for (int n = 0; n < ListStores.Count; n++)
                    {
                        StoreModels store = ListStores[n];
                        storeId = store.Id;
                        storeName = store.Name;
                        int startIndex = index;

                        ////// List category in store
                        // Get all parent & child category, updated 03212018
                        var listCateInStore = listCateNames.Where(yy => yy.StoreId == storeId)
                            .OrderBy(yy => yy.Name)
                            .Select(yy => new { yy.Name, yy.Id }).Distinct().ToList();
                        var listCateNamesInStore = listCateInStore.OrderBy(oo => oo.Name).Select(ss => ss.Name).ToList<string>();

                        // List set menu name in store
                        List<string> listSetMenuNamesInStore = listSetMenuNames.Where(yy => yy.StoreId == storeId)
                            .OrderBy(yy => yy.Name).Select(yy => yy.Name)
                            .Distinct().ToList();

                        if (listCateNamesInStore != null && listCateNamesInStore.Any())
                        {
                            endIndexColumn += listCateNamesInStore.Count;
                        }

                        // Format title of report
                        if (n == 0)
                        {
                            ws.Range(1, 1, 1, endIndexColumn).Merge();
                            ws.Range(2, 1, 2, endIndexColumn).Merge();
                            ws.Range(3, 1, 3, endIndexColumn).Merge();
                            ws.Range(4, 1, 4, endIndexColumn).Merge();

                            ws.Range(1, 1, 4, endIndexColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, endIndexColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, endIndexColumn).Style.Border.OutsideBorderColor = outsideBorderColor;
                            ws.Range(1, 1, 4, endIndexColumn).Style.Border.InsideBorderColor = insideBorderColor;
                            ws.Range(1, 1, 4, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitle);
                        }

                        // Title Store/Merchant name
                        ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(string.Format("{0}", storeName));
                        ws.Range(index, 1, index, endIndexColumn).Style.Font.SetFontSize(16).Font.SetBold(true);
                        ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundStoreTitle);
                        ws.Range(index, 1, index, endIndexColumn).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                        ws.Row(index).Height = 22;
                        listLeftAlignmentIndexes.Add(index);

                        index++;

                        // Get all business day in store
                        var businessInStore = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                        var businessIdInStore = businessInStore.Select(aa => aa.Id).ToList();

                        // Get data in a store by list business day, with hour in list time range
                        List<HourlyItemizedSalesReportModels> listData = model.Where(xx => xx.StoreId == storeId && businessIdInStore.Contains(xx.BusinessId) && listTimesRange.Contains(xx.Hour)).ToList();

                        // List date time of list data
                        var listDateTimeInStore = listData.GroupBy(zz => new { zz.CreatedDate })
                            .Select(zzz => new
                            {
                                CreatedDate = zzz.Key.CreatedDate,
                                listHour = zzz.OrderBy(aa => aa.Hour).Select(aa => aa.Hour).Distinct().ToList()
                            }).OrderBy(zz => zz.CreatedDate).ToList();

                        decimal grandItemTotal = 0;

                        // List Category and Set Menu Selected
                        ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Category");
                        ws.Range(index, 2, index, endIndexColumn).Merge().SetValue(string.Format("{0}", string.Join(", ", listCateNamesInStore)));
                        ws.Range(index, 2, index++, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu");
                        ws.Range(index, 2, index, endIndexColumn).Merge().SetValue(string.Format("{0}", string.Join(", ", listSetMenuNamesInStore)));
                        ws.Range(index, 2, index++, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        for (int d = 0; d < listDateTimeInStore.Count; d++)
                        {
                            // Get list time in a day
                            var ListTimes = listTimesRange.Where(ww => listDateTimeInStore[d].listHour.Contains(ww)).ToList();

                            // Current time report
                            ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(listDateTimeInStore[d].CreatedDate.ToString("MM/dd/yyyy"));
                            ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitleDay);
                            ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Range(index, 1, index++, endIndexColumn).Style.Font.SetBold(true);

                            for (int i = 0; i < ListTimes.Count; i++)
                            {
                                // Time title
                                ws.Range(index, 1, index, endIndexColumn).Merge().SetValue(string.Format("{0}:00 - {1}:00", ListTimes[i], (ListTimes[i] + 1)));
                                ws.Range(index, 1, index, endIndexColumn).Style.Fill.SetBackgroundColor(backgroundTitle);
                                ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);
                                listLeftAlignmentIndexes.Add(index);

                                index++;

                                // List titles
                                List<object> listCateItem = new List<object>();
                                decimal discountTotalBill = 0;
                                decimal promotionTotal = 0;
                                decimal setmenuTotal = 0;
                                decimal miscTotal = 0;
                                decimal discountItemTmp = 0;
                                decimal tmp = 0;
                                decimal roundValue = 0;

                                foreach (var item in listCateInStore)
                                {
                                    var itemHours = listData.Where(dd => dd.Hour == ListTimes[i] && dd.CategoryId == item.Id
                                    && listDateTimeInStore[d].CreatedDate == dd.CreatedDate).ToList();

                                    if (itemHours != null && itemHours.Any())
                                    {
                                        listCateItem.Add(item.Name);
                                        tmp = itemHours.Sum(ss => (decimal)ss.TotalPrice - (decimal)ss.Promotion);
                                        discountItemTmp = itemHours.Where(ww => !ww.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);

                                        listCateItem.Add(tmp - discountItemTmp);

                                        discountTotalBill += itemHours.Where(ww => ww.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);
                                        promotionTotal += itemHours.Sum(ss => (decimal)ss.Promotion);
                                    }
                                    else
                                    {
                                        listCateItem.Add(item.Name);
                                        listCateItem.Add(0);
                                    }
                                }

                                // Get setmenu in store, depend on createDate
                                setmenuTotal = listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                                && dd.ItemTypeId == (int)Commons.EProductType.SetMenu).Sum(s => (decimal)s.TotalPrice
                                - (s.IsDiscountTotal ? 0 : (decimal)s.Discount)
                                - (decimal)s.Promotion);

                                discountTotalBill += listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                                && dd.ItemTypeId == (int)Commons.EProductType.SetMenu && dd.IsDiscountTotal).Sum(ss => (decimal)ss.Discount);

                                promotionTotal += listData.Where(dd => dd.Hour == ListTimes[i] && listDateTimeInStore[d].CreatedDate == dd.CreatedDate
                                     && dd.ItemTypeId == (int)Commons.EProductType.SetMenu).Sum(s => (decimal)s.Promotion);

                                // Get Misc in store, depend on createDate
                                miscTotal = listMiscDiscountItems.Where(m => m.Hour == ListTimes[i] && m.StoreId == storeId
                                && listDateTimeInStore[d].CreatedDate.Year == m.CreatedDate.Year
                                && listDateTimeInStore[d].CreatedDate.Month == m.CreatedDate.Month
                                && listDateTimeInStore[d].CreatedDate.Day == m.CreatedDate.Day).Sum(yy => (decimal)yy.MiscValue);

                                decimal hourlyItemTotal = 0;
                                int column = 0;
                                if (listCateItem.Count == 0)
                                {
                                    column = 1;
                                }
                                else
                                {
                                    int k = 0;
                                    for (int j = 0; j < listCateItem.Count; j += 2)
                                    {
                                        string CategoryName = listCateItem[j].ToString();
                                        decimal TotalPrice = decimal.Parse(listCateItem[j + 1].ToString());

                                        column = 2 + k;
                                        // Category name
                                        ws.Cell(index, 2 + k).Value = string.Format("{0}", CategoryName);
                                        // Set width of cells
                                        ws.Column(2 + k).Width = ws.Cell(index, 2 + k).Value.ToString().Length + 5;

                                        // Prices
                                        ws.Cell(index + 1, 2 + k).Value = TotalPrice.ToString("F");

                                        hourlyItemTotal += TotalPrice;

                                        k++;
                                    }
                                }

                                // Set Menu total
                                column++;
                                ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu");
                                ws.Cell(index + 1, column).Value = setmenuTotal;
                                ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                                // Misc total
                                column++;
                                ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Misc");
                                ws.Cell(index + 1, column).Value = miscTotal.ToString("F");
                                ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                                // Discount total
                                column++;
                                ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill");
                                ws.Cell(index + 1, column).Value = discountTotalBill.ToString("F");
                                ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                                // Promotion total
                                column++;
                                ws.Cell(index, column).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Promotion");
                                ws.Cell(index + 1, column).Value = promotionTotal.ToString("F");
                                ws.Column(column).Width = ws.Cell(index, column).Value.ToString().Length + 5;

                                ws.Range(index, 2, index, column).Style.Fill.SetBackgroundColor(backgroundTitle);
                                ws.Range(index, 2, index, column).Style.Font.SetBold(true);

                                // Rounding value of this time
                                roundValue = lstRoundValue.Where(m => m.CreatedDate.Hour == ListTimes[i] && m.StoreId == storeId
                                && listDateTimeInStore[d].CreatedDate.Year == m.CreatedDate.Year
                                && listDateTimeInStore[d].CreatedDate.Month == m.CreatedDate.Month
                                && listDateTimeInStore[d].CreatedDate.Day == m.CreatedDate.Day).Sum(yy => (decimal)yy.RoundingValue);


                                hourlyItemTotal += setmenuTotal + miscTotal - discountTotalBill + roundValue; // Updated 03032018, + rounding value
                                grandItemTotal += hourlyItemTotal;

                                // Hourly item total
                                index += 2;
                                ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Item Total");

                                ws.Range(index, 2, index, endIndexColumn).Merge().Value = hourlyItemTotal.ToString("F");
                                ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);

                                ws.Range(index - 2, 1, index, endIndexColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                ws.Range(index - 2, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                                ws.Range(index - 1, 2, index, endIndexColumn).Style.NumberFormat.Format = "#,##0.00";
                                index++;
                            }
                        }
                        //grand item total
                        ws.Cell("A" + index).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Grand Item Total");
                        ws.Range(index, 2, index, endIndexColumn).Merge().Value = grandItemTotal.ToString("F");
                        ws.Range(index, 2, index, endIndexColumn).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range(index, 1, index, endIndexColumn).Style.Font.SetBold(true);
                        ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Range(index, 1, index, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Range(startIndex, 1, index, endIndexColumn).Style.Border.OutsideBorderColor = outsideBorderColor;
                        ws.Range(startIndex, 1, index++, endIndexColumn).Style.Border.InsideBorderColor = insideBorderColor;
                        index++;
                        endIndexColumn = 5;
                    }
                    foreach (var item in listLeftAlignmentIndexes)
                    {
                        ws.Range(item, 1, item, endIndexColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Justify;
                    }
                }
                ws.Columns().AdjustToContents();
                ws.Column(1).Width = 25;
                return wb;
            }
            catch (Exception e)
            {
                return wb;
            }
        }
        #endregion

    }
}
