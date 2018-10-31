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
using ClosedXML.Excel;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using System.Data.Entity.SqlServer;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Chart;
using System.Xml;
using System.Data.Entity;

namespace NuWebNCloud.Shared.Factory
{
    public class HourlySalesReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private NoIncludeOnSaleDataFactory _noIncludeOnSaleDataFactory;
        private OrderPaymentMethodFactory _orderPaymentMethodFactory;
        private DailySalesReportFactory _dailySalesReportFactory;
        private RefundFactory _refundFactory = null;
        public HourlySalesReportFactory()
        {
            _baseFactory = new BaseFactory();
            _noIncludeOnSaleDataFactory = new NoIncludeOnSaleDataFactory();
            _orderPaymentMethodFactory = new OrderPaymentMethodFactory();
            _dailySalesReportFactory = new DailySalesReportFactory();
            _refundFactory = new RefundFactory();
        }
        public bool Insert(List<HourlySalesReportModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_HourlySalesReport.Where(ww => ww.StoreId == info.StoreId
                    && ww.ReceiptId == info.ReceiptId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Hourly Sales data exist");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_HourlySalesReport> lstInsert = new List<R_HourlySalesReport>();
                        R_HourlySalesReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_HourlySalesReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.ReceiptTotal = item.ReceiptTotal;
                            itemInsert.NetSales = item.NetSales;
                            itemInsert.ReceiptId = item.ReceiptId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.NoOfPerson = item.NoOfPerson;
                            itemInsert.Mode = item.Mode;
                            itemInsert.CreditNoteNo = item.CreditNoteNo;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_HourlySalesReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Hourly Sales data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Hourly Sales data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_HourlySalesReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }


        public List<HourlySalesReportModels> GetData(DateTime dFrom, DateTime dTo, List<string> lstStores, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_HourlySalesReport
                               where lstStores.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= dFrom && tb.CreatedDate <= dTo)
                                     && tb.Mode == mode
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   Time = (int?)SqlFunctions.DatePart("HH", tb.CreatedDate),
                                   //Date = DbFunctions.TruncateTime(tb.CreatedDate)
                               } into g
                               select new HourlySalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   Time = g.Key.Time.Value,
                                   ReceiptTotal = g.Sum(x => x.ReceiptTotal),
                                   NoOfReceipt = g.Count(), //
                                   NoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 : g.Sum(x => x.NoOfPerson),
                                   PerNoOfReceipt = (g.Sum(x => x.ReceiptTotal) / (g.Count() == 0 ? 1 : g.Count())),
                                   //PerNoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 : g.Sum(x => x.NoOfPerson) == 0 ? 0
                                   //                 : g.Sum(x => x.ReceiptTotal) / g.Sum(x => (int?)x.NoOfPerson) == null ? 0 : g.Sum(x => x.NoOfPerson),

                                   PerNoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 :
                                    g.Sum(x => x.ReceiptTotal) / (g.Sum(x => x.NoOfPerson) == 0 ? 1 : g.Sum(x => x.NoOfPerson)),
                                   NetSales = g.Sum(x => x.NetSales),
                                   //Date = g.Key.Date.Value
                               }).ToList();
                return lstData;
            }
        }
        //Get rouding for hourly sale report
        public List<HourlySalesReportModels> GetRoundingForHourly(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DailyReceiptReport
                               where model.ListStores.Contains(tb.StoreId)
                                     && tb.CreatedDate >= model.FromDate
                                             && tb.CreatedDate <= model.ToDate
                                             && tb.Mode == model.Mode
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

        public bool Report(RPHourlySalesModels model, ref IXLWorksheet ws, List<StoreModels> lstStore)
        {
            try
            {
                var lstStoreIndex = model.ListStores;
                model.FromDate = DateTimeHelper.SetFromDate(model.FromDate, model.FromHour);
                model.ToDate = DateTimeHelper.SetToDate(model.ToDate, model.ToHour);

                List<HourlySalesReportModels> lstData = null;///GetData(model);
                if (lstData == null)
                    return false;

                int totalCols = typeof(HourlySalesReportModels).GetProperties().Count() - 3;
                int fromHour = model.FromHour.Hours;
                int toHour = (model.ToHour.Minutes == 0) ? model.ToHour.Hours : (model.ToHour.Hours + 1);

                CreateReportHeader(ws, totalCols, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Period"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"), System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX").ToLower()), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Avg/Pax"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cell(4, i + 1).Value = lstColNames[i];

                ws.Row(4).Height = 25;
                ws.Row(4).Style.Font.SetBold(true);
                ws.Row(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(4, 1, 4, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                int currentRow = 5;
                List<int> lstSubTotalRowIndex = new List<int>();
                Dictionary<int, double> total = new Dictionary<int, double>();
                foreach (StoreModels store in lstStore)
                {
                    #region Store Name
                    string storeId = store.Id;
                    string storeName = store.Name;

                    ws.Cell(currentRow, 1).Value = storeName;
                    //FormatStoreNameRow(ws, currentRow, totalCols);
                    ws.Range(currentRow, 1, currentRow, totalCols).Merge();
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Row(currentRow).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                    #endregion
                    currentRow++;
                    int beginCal = currentRow;
                    Dictionary<int, double> subTotal = new Dictionary<int, double>();
                    #region Data
                    for (int i = fromHour; i < toHour; i++) // Row Loop
                    {
                        ws.Cell(currentRow, 1).Value = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("AM") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PM"), i + 1, i < 12 ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("AM") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PM"));
                        for (int j = 2; j <= totalCols; j++) // Column Loop
                        {
                            // Data
                            HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                            //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.StoreId == storeId);
                            if (data == null)
                                data = new HourlySalesReportModels
                                {
                                    ReceiptTotal = 0,
                                    NoOfReceipt = 0,
                                    NoOfPerson = 0,
                                    PerNoOfReceipt = 0,
                                    PerNoOfPerson = 0,
                                    NetSales = 0
                                };
                            try
                            {
                                string propName = data.GetType().GetProperties()[j].Name;
                                ws.Cell(currentRow, j).Value = data.GetType().GetProperty(propName).GetValue(data, null);
                                ws.Cell(currentRow, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                if (!subTotal.ContainsKey(j))
                                {
                                    subTotal.Add(j, (double)ws.Cell(currentRow, j).Value);
                                }
                                else
                                {
                                    subTotal[j] += (double)ws.Cell(currentRow, j).Value;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex);
                            }
                        }
                        currentRow++;
                    }
                    #endregion
                    #region Sub Total
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub. Total");
                    if (lstData.Count > 0)
                    {
                        for (int i = 2; i <= totalCols; i++)
                        {
                            if (i == 5)
                            {
                                if (string.IsNullOrEmpty(ws.Cell("C" + currentRow).Value.ToString()) ||
                                    ws.Cell("C" + currentRow).Value.ToString() == "0")
                                    ws.Cell(currentRow, i).Value = "0";
                                else
                                {
                                    ws.Cell(currentRow, i).Value = subTotal[2] / subTotal[3];
                                    if (!total.ContainsKey(i))
                                    {
                                        total.Add(i, subTotal[2] / subTotal[3]);
                                    }
                                    else
                                    {
                                        total[i] += subTotal[2] / subTotal[3];
                                    }
                                }
                            }
                            else if (i == 6)
                            {
                                if (string.IsNullOrEmpty(ws.Cell("D" + currentRow).Value.ToString()) ||
                                    ws.Cell("D" + currentRow).Value.ToString() == "0")
                                    ws.Cell(currentRow, i).Value = "0";
                                else
                                {
                                    ws.Cell(currentRow, i).Value = subTotal[2] / subTotal[4];
                                    if (!total.ContainsKey(i))
                                    {
                                        total.Add(i, subTotal[2] / subTotal[4]);
                                    }
                                    else
                                    {
                                        total[i] += subTotal[2] / subTotal[4];
                                    }
                                }
                            }
                            else
                            {
                                if (subTotal.ContainsKey(i))
                                {
                                    ws.Cell(currentRow, i).Value = subTotal[i];
                                    if (!total.ContainsKey(i))
                                    {
                                        total.Add(i, subTotal[i]);
                                    }
                                    else
                                    {
                                        total[i] += subTotal[i];
                                    }
                                }
                            }
                            ws.Cell(currentRow, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }
                    }
                    ws.Range(beginCal, 2, currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range(beginCal, 5, currentRow, totalCols).Style.NumberFormat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    lstSubTotalRowIndex.Add(currentRow);
                    //ws.Range(string.Format("A{0}:G{0}", currentRow)).Merge().SetValue("");
                    #endregion
                    currentRow++;
                }//end store loop

                // Total
                #region Total
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total");
                if (lstData.Count > 0)
                    for (int i = 2; i <= totalCols; i++)
                    {
                        if (i == 5)
                        {
                            if (string.IsNullOrEmpty(ws.Cell("C" + currentRow).Value.ToString()) ||
                                ws.Cell("C" + currentRow).Value.ToString() == "0")
                                ws.Cell(currentRow, i).Value = "0";
                            else
                            {
                                ws.Cell(currentRow, i).Value = total[2] / total[3];
                            }
                        }
                        else if (i == 6)
                        {
                            if (string.IsNullOrEmpty(ws.Cell("D" + currentRow).Value.ToString()) ||
                                ws.Cell("D" + currentRow).Value.ToString() == "0")
                                ws.Cell(currentRow, i).Value = "0";
                            else
                            {
                                ws.Cell(currentRow, i).Value = total[2] / total[4];
                            }
                        }
                        else
                        {
                            ws.Cell(currentRow, i).Value = total[i];
                        }
                        ws.Cell(currentRow, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                ws.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                ws.Range(currentRow, 5, currentRow, totalCols).Style.NumberFormat.Format = "#,##0.00";
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;
                //FormatAllReport(ws, currentRow, totalCols);
                ws.Range(1, 1, currentRow - 1, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, currentRow - 1, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 1, totalCols).Style.Border.BottomBorder = XLBorderStyleValues.None;
                ws.Range(2, 1, 2, totalCols).Style.Border.TopBorder = XLBorderStyleValues.None;
                ws.Columns(1, totalCols).AdjustToContents();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /*Export Excel have Chart*/
        [Obsolete("ReportChart_12092017 is deprecated, please use ReportChart_New instead.")]
        public ExcelPackage ReportChart_12092017(RPHourlySalesModels model, List<StoreModels> lstStore)
        {
            ExcelPackage pck = new ExcelPackage();
            try
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly_Sales"));

                var lstStoreIndex = model.ListStores;
                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromHour.Hours, model.FromHour.Minutes, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToHour.Hours, model.ToHour.Minutes, 59);

                //DateTime _dToFilter = model.ToDate;

                int totalCols = 7;
                CreateReportHeaderChart(ws, totalCols, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));
                // Format header report
                ws.Cells[1, 1, 4, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreIndex, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return pck;
                }

                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                var _lstNoIncludeOnSale = _noIncludeOnSaleDataFactory.GetListCateNoIncludeSaleForHourlySale(model.ListStores, model.FromDate, model.ToDate, model.Mode);
                if (_lstNoIncludeOnSale == null)
                    _lstNoIncludeOnSale = new List<NoIncludeOnSaleDataReportModels>();
                //var _lstNoIncludeOnSaleGroupByCate = new List<NoIncludeOnSaleDataReportModels>();

                List<string> lstStores = model.ListStores;

                List<HourlySalesReportModels> lstData = GetData(model.FromDate, model.ToDate, lstStores, model.Mode);
                if (lstData == null)
                    return pck;

                //int totalCols = 7;
                int fromHour = model.FromHour.Hours;
                //int toHour = (model.ToHour.Minutes == 0) ? model.ToHour.Hours : (model.ToHour.Hours + 1);

                //CreateReportHeaderChart(ws, totalCols, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Period"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"), System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX").ToLower()), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Avg/Pax"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cells[5, i + 1].Value = lstColNames[i];

                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.Bold = true;
                ws.Row(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Row(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion
                int currentRow = 6, timeValue = 0;
                List<int> lstSubTotalRowIndex = new List<int>();
                Dictionary<int, double> total = new Dictionary<int, double>();
                bool _isTaxInclude = true;
                //===========
                Dictionary<string, List<string>> listChart = new Dictionary<string, List<string>>();
                string storeId = string.Empty, storeName = string.Empty;
                double total_ReceiptTotal = 0, total_TC = 0, total_Pax = 0, total_NetSale = 0;
                foreach (StoreModels store in lstStore)
                {
                    _isTaxInclude = _baseFactory.IsTaxInclude(store.Id);
                    #region Store Name
                    storeId = store.Id;
                    storeName = store.Name;
                    ws.Cells[currentRow, 1].Value = storeName;
                    //FormatStoreNameRow(ws, currentRow, totalCols);
                    ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));

                    #endregion
                    List<string> listRange = new List<string>();
                    currentRow++;
                    listRange.Add(currentRow.ToString());

                    int beginCal = currentRow;
                    Dictionary<int, double> subTotal = new Dictionary<int, double>();
                    #region Data
                    DateTime dGroup = model.FromDate, dTmpFrom = DateTime.Now, dTmpTo = DateTime.Now;
                    //TimeSpan _timeSpan = new TimeSpan();
                    string time = string.Empty;
                    int totalHour = (int)model.ToDate.Subtract(model.FromDate).TotalHours;
                    double subTotal_ReceiptTotal = 0, subTotal_TC = 0, subTotal_Pax = 0, subTotal_NetSale = 0;
                    double _navValue = 0;
                    if (totalHour > 24)
                    {
                        fromHour = 0;
                        totalHour = 24;
                        #region Show data in report
                        for (int i = fromHour; i < totalHour; i++)
                        {
                            //dTmpTo = dTmpFrom.AddHours(1);
                            time = string.Format("{0}:00 - {1}:00", i, i + 1);
                            //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                            //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                            ws.Cells[currentRow, 1].Value = time;
                            //set data
                            //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                            HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                            if (data == null)
                                data = new HourlySalesReportModels
                                {
                                    ReceiptTotal = 0,
                                    NoOfReceipt = 0,
                                    NoOfPerson = 0,
                                    PerNoOfReceipt = 0,
                                    PerNoOfPerson = 0,
                                    NetSales = 0
                                };
                            ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                            subTotal_ReceiptTotal += data.ReceiptTotal;
                            ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                            subTotal_TC += data.NoOfReceipt;
                            ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                            subTotal_Pax += data.NoOfPerson;
                            ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                            ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                            ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //Re caculate net sale
                            //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);

                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                            if (_isTaxInclude)
                            {
                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                            }
                            ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue);
                            subTotal_NetSale += (data.NetSales - _navValue);
                            ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                            dTmpFrom = dTmpFrom.AddHours(1);
                            dTmpTo = dTmpTo.AddHours(1);
                            currentRow++;
                        }
                        #endregion
                    }
                    else
                    {
                        if (model.FromHour.Hours > model.ToHour.Hours)
                        {
                            for (int lop = 0; lop < 2; lop++)
                            {
                                if (lop == 0)
                                {
                                    #region Show data in report
                                    for (int i = model.FromHour.Hours; i < 24; i++)
                                    {
                                        //dTmpTo = dTmpFrom.AddHours(1);
                                        time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                        //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                        //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                        ws.Cells[currentRow, 1].Value = time;
                                        //set data
                                        //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                        HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                        if (data == null)
                                            data = new HourlySalesReportModels
                                            {
                                                ReceiptTotal = 0,
                                                NoOfReceipt = 0,
                                                NoOfPerson = 0,
                                                PerNoOfReceipt = 0,
                                                PerNoOfPerson = 0,
                                                NetSales = 0
                                            };
                                        ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                        subTotal_ReceiptTotal += data.ReceiptTotal;
                                        ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                        subTotal_TC += data.NoOfReceipt;
                                        ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                        subTotal_Pax += data.NoOfPerson;
                                        ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                        ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        //Re caculate net sale
                                        //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);
                                        _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                        if (_isTaxInclude)
                                        {
                                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                        }
                                        ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue);
                                        subTotal_NetSale += (data.NetSales - _navValue);
                                        ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                        dTmpFrom = dTmpFrom.AddHours(1);
                                        dTmpTo = dTmpTo.AddHours(1);
                                        currentRow++;
                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region Show data in report
                                    for (int i = 0; i < model.ToHour.Hours; i++)
                                    {
                                        //dTmpTo = dTmpFrom.AddHours(1);
                                        time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                        //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                        //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                        ws.Cells[currentRow, 1].Value = time;
                                        //set data
                                        //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                        HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                        if (data == null)
                                            data = new HourlySalesReportModels
                                            {
                                                ReceiptTotal = 0,
                                                NoOfReceipt = 0,
                                                NoOfPerson = 0,
                                                PerNoOfReceipt = 0,
                                                PerNoOfPerson = 0,
                                                NetSales = 0
                                            };
                                        ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                        subTotal_ReceiptTotal += data.ReceiptTotal;
                                        ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                        subTotal_TC += data.NoOfReceipt;
                                        ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                        subTotal_Pax += data.NoOfPerson;
                                        ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                        ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        //Re caculate net sale
                                        //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);
                                        _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                        if (_isTaxInclude)
                                        {
                                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                        }

                                        ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue);
                                        subTotal_NetSale += (data.NetSales - _navValue);

                                        //ws.Cells[currentRow, 7].Value = data.NetSales;
                                        //subTotal_NetSale += data.NetSales;
                                        ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                        dTmpFrom = dTmpFrom.AddHours(1);
                                        dTmpTo = dTmpTo.AddHours(1);
                                        currentRow++;
                                    }
                                    #endregion
                                }
                            }
                        }
                        else //model.FromHour.Hours > model.ToHour.Hours
                        {
                            #region Show data in report
                            int hour = model.ToHour.Hours;
                            if (model.ToHour.Minutes == 59)
                                hour = model.ToHour.Hours + 1;

                            for (int i = model.FromHour.Hours; i < hour; i++)
                            {
                                //dTmpTo = dTmpFrom.AddHours(1);
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                ws.Cells[currentRow, 1].Value = time;
                                //set data
                                //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                if (data == null)
                                    data = new HourlySalesReportModels
                                    {
                                        ReceiptTotal = 0,
                                        NoOfReceipt = 0,
                                        NoOfPerson = 0,
                                        PerNoOfReceipt = 0,
                                        PerNoOfPerson = 0,
                                        NetSales = 0
                                    };
                                ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                subTotal_ReceiptTotal += data.ReceiptTotal;
                                ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                subTotal_TC += data.NoOfReceipt;
                                ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                subTotal_Pax += data.NoOfPerson;
                                ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                //Re caculate net sale
                                //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);

                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                if (_isTaxInclude)
                                {
                                    _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                }

                                ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue);
                                subTotal_NetSale += (data.NetSales - _navValue);

                                //ws.Cells[currentRow, 7].Value = data.NetSales;
                                //subTotal_NetSale += data.NetSales;
                                ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                dTmpFrom = dTmpFrom.AddHours(1);
                                dTmpTo = dTmpTo.AddHours(1);
                                currentRow++;
                            }
                            #endregion
                        }
                    }
                    //dTmpFrom = dGroup;
                    //dTmpTo = dGroup;
                    //End for date
                    listRange.Add((currentRow - 1).ToString());
                    listChart.Add(storeName, listRange);
                    #endregion
                    #region Sub Total
                    ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub. Total");
                    if (lstData.Count > 0)
                    {

                        total_ReceiptTotal += subTotal_ReceiptTotal;
                        total_TC += subTotal_TC;
                        total_Pax += subTotal_Pax;
                        total_NetSale += subTotal_NetSale;
                        // sub total 
                        ws.Cells[currentRow, 2].Value = subTotal_ReceiptTotal;
                        ws.Cells[currentRow, 3].Value = subTotal_TC;
                        ws.Cells[currentRow, 4].Value = subTotal_Pax;
                        ws.Cells[currentRow, 5].Value = subTotal_TC == 0 ? 0 : subTotal_ReceiptTotal / subTotal_TC;
                        ws.Cells[currentRow, 6].Value = subTotal_Pax == 0 ? 0 : subTotal_ReceiptTotal / subTotal_Pax;
                        ws.Cells[currentRow, 7].Value = subTotal_NetSale;

                        ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    }
                    ws.Cells[beginCal, 2, currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[beginCal, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    lstSubTotalRowIndex.Add(currentRow);

                    #endregion
                    currentRow++;
                }//end store loop

                // Total
                #region Total
                ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                if (lstData.Count > 0)
                {
                    // total 
                    ws.Cells[currentRow, 2].Value = total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = total_TC;
                    ws.Cells[currentRow, 4].Value = total_Pax;
                    ws.Cells[currentRow, 5].Value = total_TC == 0 ? 0 : total_ReceiptTotal / total_TC;
                    ws.Cells[currentRow, 6].Value = total_Pax == 0 ? 0 : total_ReceiptTotal / total_Pax;
                    ws.Cells[currentRow, 7].Value = total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                ws.Row(currentRow).Style.Font.Bold = true;
                ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion
                currentRow++;
                //FormatAllReport(ws, currentRow, totalCols);
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells.AutoFitColumns();

                if (model.FormatExport.Equals("Excel"))
                {
                    //Draw chart
                    ExcelChart chart = ws.Drawings.AddChart("HourlySales", eChartType.LineMarkersStacked);
                    chart.SetPosition(1, 0, 8, 0);
                    chart.SetSize(950, 600);
                    chart.AdjustPositionAndSize();
                    chart.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chart Title");
                    chart.Title.Font.Size = 14;

                    chart.XAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Period");
                    chart.XAxis.Title.Font.Size = 10;
                    chart.XAxis.Crosses = eCrosses.Max;
                    //chart.XAxis.Title.Font.Color = Color.Gray;

                    chart.YAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales Amount");
                    chart.YAxis.Title.Font.Size = 10;
                    //chart.YAxis.Title.Font.Color = Color.Gray;

                    ////Format Legend for Chart
                    chart.Legend.Remove();

                    for (int i = 0; i < listChart.Count; i++)
                    {
                        string storename = listChart.ElementAt(i).Key;
                        List<string> listRange = listChart.ElementAt(i).Value;
                        int startRow = 0;
                        int endRow = 0;
                        for (int k = 0; k < listRange.Count; k++)
                        {
                            startRow = int.Parse(listRange[k]);
                            endRow = int.Parse(listRange[k + 1]);
                            break;
                        }
                        string xRow = string.Format("A{0}:A{1}", startRow, endRow);
                        string yRow = string.Format("B{0}:B{1}", startRow, endRow);
                        var serie1 = chart.Series.Add(yRow, xRow);
                        serie1.Header = storename;
                        serie1.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;
                    }

                    //Data Table with Legend
                    var chartXml = chart.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("default", nsuri);

                    //Add the table node
                    var plotAreaNode = chartXml.SelectSingleNode("/default:chartSpace/default:chart/default:plotArea", nsm);
                    var dTableNode = chartXml.CreateNode(XmlNodeType.Element, "dTable", nsuri);
                    plotAreaNode.AppendChild(dTableNode);

                    //With Legend Flag
                    var att = chartXml.CreateAttribute("val");
                    att.Value = "1";

                    var showKeysNode = chartXml.CreateNode(XmlNodeType.Element, "showKeys", nsuri);
                    showKeysNode.Attributes.Append(att);
                    dTableNode.AppendChild(showKeysNode);

                    pck.Save();
                }
                return pck;
            }
            catch (Exception e)
            {
                return pck;
            }
        }
        [Obsolete("ReportChart is deprecated, please use ReportChart_New instead.")]
        public ExcelPackage ReportChart(RPHourlySalesModels model, List<StoreModels> lstStore)
        {
            ExcelPackage pck = new ExcelPackage();
            try
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly_Sales"));

                var lstStoreIndex = model.ListStores;
                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromHour.Hours, model.FromHour.Minutes, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToHour.Hours, model.ToHour.Minutes, 59);

                //DateTime _dToFilter = model.ToDate;

                int totalCols = 7;
                CreateReportHeaderChart(ws, totalCols, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));
                // Format header report
                ws.Cells[1, 1, 4, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreIndex, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return pck;
                }

                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);

                var _lstNoIncludeOnSale = _noIncludeOnSaleDataFactory.GetListCateNoIncludeSaleForHourlySale(model.ListStores, model.FromDate, model.ToDate, model.Mode);
                if (_lstNoIncludeOnSale == null)
                    _lstNoIncludeOnSale = new List<NoIncludeOnSaleDataReportModels>();
                //var _lstNoIncludeOnSaleGroupByCate = new List<NoIncludeOnSaleDataReportModels>();
                //Get info payment by GC
                var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                if (lstGC == null)
                    lstGC = new List<string>();
                List<PaymentModels> lstPayments = _orderPaymentMethodFactory.GetDataPaymentItemsByGCForHourlySale(model, lstGC);

                List<string> lstStores = model.ListStores;

                List<HourlySalesReportModels> lstData = GetData(model.FromDate, model.ToDate, lstStores, model.Mode);
                var _lstRounding = GetRoundingForHourly(model);
                if (lstData == null)
                    return pck;

                //int totalCols = 7;
                int fromHour = model.FromHour.Hours;
                //int toHour = (model.ToHour.Minutes == 0) ? model.ToHour.Hours : (model.ToHour.Hours + 1);

                //CreateReportHeaderChart(ws, totalCols, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Period"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"), System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX").ToLower()), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Avg/Pax"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cells[5, i + 1].Value = lstColNames[i];

                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.Bold = true;
                ws.Row(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Row(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion
                int currentRow = 6, timeValue = 0;
                List<int> lstSubTotalRowIndex = new List<int>();
                Dictionary<int, double> total = new Dictionary<int, double>();
                bool _isTaxInclude = true;
                //===========
                Dictionary<string, List<string>> listChart = new Dictionary<string, List<string>>();
                string storeId = string.Empty, storeName = string.Empty;
                double total_ReceiptTotal = 0, total_TC = 0, total_Pax = 0, total_NetSale = 0;
                double _roundingValue = 0;
                foreach (StoreModels store in lstStore)
                {
                    _isTaxInclude = _baseFactory.IsTaxInclude(store.Id);
                    #region Store Name
                    storeId = store.Id;
                    storeName = store.Name;
                    ws.Cells[currentRow, 1].Value = storeName;
                    //FormatStoreNameRow(ws, currentRow, totalCols);
                    ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));

                    #endregion
                    List<string> listRange = new List<string>();
                    currentRow++;
                    listRange.Add(currentRow.ToString());

                    int beginCal = currentRow;
                    Dictionary<int, double> subTotal = new Dictionary<int, double>();
                    #region Data
                    DateTime dGroup = model.FromDate, dTmpFrom = DateTime.Now, dTmpTo = DateTime.Now;
                    //TimeSpan _timeSpan = new TimeSpan();
                    string time = string.Empty;
                    int totalHour = (int)model.ToDate.Subtract(model.FromDate).TotalHours;
                    double subTotal_ReceiptTotal = 0, subTotal_TC = 0, subTotal_Pax = 0, subTotal_NetSale = 0;
                    double _navValue = 0;
                    if (totalHour > 24)
                    {
                        fromHour = 0;
                        totalHour = 24;
                        #region Show data in report
                        for (int i = fromHour; i < totalHour; i++)
                        {
                            //dTmpTo = dTmpFrom.AddHours(1);
                            time = string.Format("{0}:00 - {1}:00", i, i + 1);
                            //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                            //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                            ws.Cells[currentRow, 1].Value = time;
                            //set data
                            //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                            HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                            if (data == null)
                                data = new HourlySalesReportModels
                                {
                                    ReceiptTotal = 0,
                                    NoOfReceipt = 0,
                                    NoOfPerson = 0,
                                    PerNoOfReceipt = 0,
                                    PerNoOfPerson = 0,
                                    NetSales = 0
                                };
                            ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                            subTotal_ReceiptTotal += data.ReceiptTotal;
                            ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                            subTotal_TC += data.NoOfReceipt;
                            ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                            subTotal_Pax += data.NoOfPerson;
                            ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                            ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                            ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //Re caculate net sale
                            //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);

                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                            if (_isTaxInclude)
                            {
                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                            }
                            //GC value
                            var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i
                                                              ).Sum(p => p.Amount);

                            _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                            ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                            subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                            ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                            dTmpFrom = dTmpFrom.AddHours(1);
                            dTmpTo = dTmpTo.AddHours(1);
                            currentRow++;
                        }
                        #endregion
                    }
                    else
                    {
                        if (model.FromHour.Hours > model.ToHour.Hours)
                        {
                            for (int lop = 0; lop < 2; lop++)
                            {
                                if (lop == 0)
                                {
                                    #region Show data in report
                                    for (int i = model.FromHour.Hours; i < 24; i++)
                                    {
                                        //dTmpTo = dTmpFrom.AddHours(1);
                                        time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                        //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                        //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                        ws.Cells[currentRow, 1].Value = time;
                                        //set data
                                        //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                        HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                        if (data == null)
                                            data = new HourlySalesReportModels
                                            {
                                                ReceiptTotal = 0,
                                                NoOfReceipt = 0,
                                                NoOfPerson = 0,
                                                PerNoOfReceipt = 0,
                                                PerNoOfPerson = 0,
                                                NetSales = 0
                                            };
                                        ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                        subTotal_ReceiptTotal += data.ReceiptTotal;
                                        ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                        subTotal_TC += data.NoOfReceipt;
                                        ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                        subTotal_Pax += data.NoOfPerson;
                                        ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                        ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        //Re caculate net sale
                                        //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);
                                        _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                        if (_isTaxInclude)
                                        {
                                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                        }
                                        //GC value
                                        var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i
                                                                          ).Sum(p => p.Amount);

                                        _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                        ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                        subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                                        ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                        dTmpFrom = dTmpFrom.AddHours(1);
                                        dTmpTo = dTmpTo.AddHours(1);
                                        currentRow++;
                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region Show data in report
                                    for (int i = 0; i < model.ToHour.Hours; i++)
                                    {
                                        //dTmpTo = dTmpFrom.AddHours(1);
                                        time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                        //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                        //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                        ws.Cells[currentRow, 1].Value = time;
                                        //set data
                                        //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                        HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                        if (data == null)
                                            data = new HourlySalesReportModels
                                            {
                                                ReceiptTotal = 0,
                                                NoOfReceipt = 0,
                                                NoOfPerson = 0,
                                                PerNoOfReceipt = 0,
                                                PerNoOfPerson = 0,
                                                NetSales = 0
                                            };
                                        ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                        subTotal_ReceiptTotal += data.ReceiptTotal;
                                        ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                        subTotal_TC += data.NoOfReceipt;
                                        ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                        subTotal_Pax += data.NoOfPerson;
                                        ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                        ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        //Re caculate net sale
                                        //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);
                                        _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                        if (_isTaxInclude)
                                        {
                                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                        }
                                        //GC value
                                        var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i).Sum(p => p.Amount);

                                        _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                        ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                        subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);

                                        //ws.Cells[currentRow, 7].Value = data.NetSales;
                                        //subTotal_NetSale += data.NetSales;
                                        ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                        dTmpFrom = dTmpFrom.AddHours(1);
                                        dTmpTo = dTmpTo.AddHours(1);
                                        currentRow++;
                                    }
                                    #endregion
                                }
                            }
                        }
                        else //model.FromHour.Hours > model.ToHour.Hours
                        {
                            #region Show data in report
                            int hour = model.ToHour.Hours;
                            if (model.ToHour.Minutes == 59)
                                hour = model.ToHour.Hours + 1;

                            for (int i = model.FromHour.Hours; i < hour; i++)
                            {
                                //dTmpTo = dTmpFrom.AddHours(1);
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                ws.Cells[currentRow, 1].Value = time;
                                //set data
                                //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                if (data == null)
                                    data = new HourlySalesReportModels
                                    {
                                        ReceiptTotal = 0,
                                        NoOfReceipt = 0,
                                        NoOfPerson = 0,
                                        PerNoOfReceipt = 0,
                                        PerNoOfPerson = 0,
                                        NetSales = 0
                                    };
                                ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                subTotal_ReceiptTotal += data.ReceiptTotal;
                                ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                subTotal_TC += data.NoOfReceipt;
                                ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                subTotal_Pax += data.NoOfPerson;
                                ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                //Re caculate net sale
                                //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);

                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                if (_isTaxInclude)
                                {
                                    _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                }
                                //GC value
                                var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i).Sum(p => p.Amount);

                                _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);

                                //ws.Cells[currentRow, 7].Value = data.NetSales;
                                //subTotal_NetSale += data.NetSales;
                                ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                dTmpFrom = dTmpFrom.AddHours(1);
                                dTmpTo = dTmpTo.AddHours(1);
                                currentRow++;
                            }
                            #endregion
                        }
                    }
                    //dTmpFrom = dGroup;
                    //dTmpTo = dGroup;
                    //End for date
                    listRange.Add((currentRow - 1).ToString());
                    listChart.Add(storeName, listRange);
                    #endregion
                    #region Sub Total
                    ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub. Total");
                    if (lstData.Count > 0)
                    {

                        total_ReceiptTotal += subTotal_ReceiptTotal;
                        total_TC += subTotal_TC;
                        total_Pax += subTotal_Pax;
                        total_NetSale += subTotal_NetSale;
                        // sub total 
                        ws.Cells[currentRow, 2].Value = subTotal_ReceiptTotal;
                        ws.Cells[currentRow, 3].Value = subTotal_TC;
                        ws.Cells[currentRow, 4].Value = subTotal_Pax;
                        ws.Cells[currentRow, 5].Value = subTotal_TC == 0 ? 0 : subTotal_ReceiptTotal / subTotal_TC;
                        ws.Cells[currentRow, 6].Value = subTotal_ReceiptTotal / (subTotal_Pax == 0 ? 1 : subTotal_Pax);
                        ws.Cells[currentRow, 7].Value = subTotal_NetSale;

                        ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    }
                    ws.Cells[beginCal, 2, currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[beginCal, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    lstSubTotalRowIndex.Add(currentRow);

                    #endregion
                    currentRow++;
                }//end store loop

                // Total
                #region Total
                ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                if (lstData.Count > 0)
                {
                    // total 
                    ws.Cells[currentRow, 2].Value = total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = total_TC;
                    ws.Cells[currentRow, 4].Value = total_Pax;
                    ws.Cells[currentRow, 5].Value = total_TC == 0 ? 0 : total_ReceiptTotal / total_TC;
                    ws.Cells[currentRow, 6].Value = total_ReceiptTotal / (total_Pax == 0 ? 1 : total_Pax);
                    ws.Cells[currentRow, 7].Value = total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                ws.Row(currentRow).Style.Font.Bold = true;
                ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion
                currentRow++;
                //FormatAllReport(ws, currentRow, totalCols);
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells.AutoFitColumns();

                if (model.FormatExport.Equals("Excel"))
                {
                    //Draw chart
                    ExcelChart chart = ws.Drawings.AddChart("HourlySales", eChartType.LineMarkersStacked);
                    chart.SetPosition(1, 0, 8, 0);
                    chart.SetSize(950, 600);
                    chart.AdjustPositionAndSize();
                    chart.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chart Title");
                    chart.Title.Font.Size = 14;

                    chart.XAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Period");
                    chart.XAxis.Title.Font.Size = 10;
                    chart.XAxis.Crosses = eCrosses.Max;
                    //chart.XAxis.Title.Font.Color = Color.Gray;

                    chart.YAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales Amount");
                    chart.YAxis.Title.Font.Size = 10;
                    //chart.YAxis.Title.Font.Color = Color.Gray;

                    ////Format Legend for Chart
                    chart.Legend.Remove();

                    for (int i = 0; i < listChart.Count; i++)
                    {
                        string storename = listChart.ElementAt(i).Key;
                        List<string> listRange = listChart.ElementAt(i).Value;
                        int startRow = 0;
                        int endRow = 0;
                        for (int k = 0; k < listRange.Count; k++)
                        {
                            startRow = int.Parse(listRange[k]);
                            endRow = int.Parse(listRange[k + 1]);
                            break;
                        }
                        string xRow = string.Format("A{0}:A{1}", startRow, endRow);
                        string yRow = string.Format("B{0}:B{1}", startRow, endRow);
                        var serie1 = chart.Series.Add(yRow, xRow);
                        serie1.Header = storename;
                        serie1.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;
                    }

                    //Data Table with Legend
                    var chartXml = chart.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("default", nsuri);

                    //Add the table node
                    var plotAreaNode = chartXml.SelectSingleNode("/default:chartSpace/default:chart/default:plotArea", nsm);
                    var dTableNode = chartXml.CreateNode(XmlNodeType.Element, "dTable", nsuri);
                    plotAreaNode.AppendChild(dTableNode);

                    //With Legend Flag
                    var att = chartXml.CreateAttribute("val");
                    att.Value = "1";

                    var showKeysNode = chartXml.CreateNode(XmlNodeType.Element, "showKeys", nsuri);
                    showKeysNode.Attributes.Append(att);
                    dTableNode.AppendChild(showKeysNode);

                    pck.Save();
                }
                return pck;
            }
            catch (Exception e)
            {
                return pck;
            }
        }
        [Obsolete("ReportChartForMerchantExtend is deprecated, please use ReportChart_New instead.")]
        public ExcelPackage ReportChartForMerchantExtend(RPHourlySalesModels model, List<StoreModels> lstStore)
        {
            ExcelPackage pck = new ExcelPackage();
            try
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly_Sales"));

                var lstStoreIndex = model.ListStores;
                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromHour.Hours, model.FromHour.Minutes, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToHour.Hours, model.ToHour.Minutes, 59);

                //DateTime _dToFilter = model.ToDate;

                int totalCols = 7;
                CreateReportHeaderChart(ws, totalCols, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));
                // Format header report
                ws.Cells[1, 1, 4, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreIndex, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return pck;
                }

                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);

                var _lstNoIncludeOnSale = _noIncludeOnSaleDataFactory.GetListCateNoIncludeSaleForHourlySale(model.ListStores, model.FromDate, model.ToDate, model.Mode);
                if (_lstNoIncludeOnSale == null)
                    _lstNoIncludeOnSale = new List<NoIncludeOnSaleDataReportModels>();
                //var _lstNoIncludeOnSaleGroupByCate = new List<NoIncludeOnSaleDataReportModels>();
                //Get info payment by GC
                var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                if (lstGC == null)
                    lstGC = new List<string>();
                List<PaymentModels> lstPayments = _orderPaymentMethodFactory.GetDataPaymentItemsByGCForHourlySale(model, lstGC);

                List<string> lstStores = model.ListStores;

                List<HourlySalesReportModels> lstData = GetData(model.FromDate, model.ToDate, lstStores, model.Mode);
                var _lstRounding = GetRoundingForHourly(model);
                if (lstData == null)
                    return pck;

                //int totalCols = 7;
                int fromHour = model.FromHour.Hours;
                //int toHour = (model.ToHour.Minutes == 0) ? model.ToHour.Hours : (model.ToHour.Hours + 1);

                //CreateReportHeaderChart(ws, totalCols, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Period"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"), System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX").ToLower()), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Avg/Pax"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cells[5, i + 1].Value = lstColNames[i];

                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.Bold = true;
                ws.Row(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Row(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion
                int currentRow = 6, timeValue = 0;
                List<int> lstSubTotalRowIndex = new List<int>();
                Dictionary<int, double> total = new Dictionary<int, double>();
                bool _isTaxInclude = true;
                //===========
                Dictionary<string, List<string>> listChart = new Dictionary<string, List<string>>();
                string storeId = string.Empty, storeName = string.Empty;
                double total_ReceiptTotal = 0, total_TC = 0, total_Pax = 0, total_NetSale = 0;
                double _roundingValue = 0;
                foreach (StoreModels store in lstStore)
                {
                    _isTaxInclude = _baseFactory.IsTaxInclude(store.Id);
                    #region Store Name
                    storeId = store.Id;
                    storeName = store.NameExtend;
                    ws.Cells[currentRow, 1].Value = storeName;
                    //FormatStoreNameRow(ws, currentRow, totalCols);
                    ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));

                    #endregion
                    List<string> listRange = new List<string>();
                    currentRow++;
                    listRange.Add(currentRow.ToString());

                    int beginCal = currentRow;
                    Dictionary<int, double> subTotal = new Dictionary<int, double>();
                    #region Data
                    DateTime dGroup = model.FromDate, dTmpFrom = DateTime.Now, dTmpTo = DateTime.Now;
                    //TimeSpan _timeSpan = new TimeSpan();
                    string time = string.Empty;
                    int totalHour = (int)model.ToDate.Subtract(model.FromDate).TotalHours;
                    double subTotal_ReceiptTotal = 0, subTotal_TC = 0, subTotal_Pax = 0, subTotal_NetSale = 0;
                    double _navValue = 0;
                    if (totalHour > 24)
                    {
                        fromHour = 0;
                        totalHour = 24;
                        #region Show data in report
                        for (int i = fromHour; i < totalHour; i++)
                        {
                            //dTmpTo = dTmpFrom.AddHours(1);
                            time = string.Format("{0}:00 - {1}:00", i, i + 1);
                            //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                            //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                            ws.Cells[currentRow, 1].Value = time;
                            //set data
                            //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                            HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                            if (data == null)
                                data = new HourlySalesReportModels
                                {
                                    ReceiptTotal = 0,
                                    NoOfReceipt = 0,
                                    NoOfPerson = 0,
                                    PerNoOfReceipt = 0,
                                    PerNoOfPerson = 0,
                                    NetSales = 0
                                };
                            ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                            subTotal_ReceiptTotal += data.ReceiptTotal;
                            ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                            subTotal_TC += data.NoOfReceipt;
                            ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                            subTotal_Pax += data.NoOfPerson;
                            ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                            ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                            ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //Re caculate net sale
                            //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);

                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                            if (_isTaxInclude)
                            {
                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                            }
                            //GC value
                            var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i
                                                              ).Sum(p => p.Amount);

                            _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                            ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                            subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                            ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                            dTmpFrom = dTmpFrom.AddHours(1);
                            dTmpTo = dTmpTo.AddHours(1);
                            currentRow++;
                        }
                        #endregion
                    }
                    else
                    {
                        if (model.FromHour.Hours > model.ToHour.Hours)
                        {
                            for (int lop = 0; lop < 2; lop++)
                            {
                                if (lop == 0)
                                {
                                    #region Show data in report
                                    for (int i = model.FromHour.Hours; i < 24; i++)
                                    {
                                        //dTmpTo = dTmpFrom.AddHours(1);
                                        time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                        //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                        //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                        ws.Cells[currentRow, 1].Value = time;
                                        //set data
                                        //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                        HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                        if (data == null)
                                            data = new HourlySalesReportModels
                                            {
                                                ReceiptTotal = 0,
                                                NoOfReceipt = 0,
                                                NoOfPerson = 0,
                                                PerNoOfReceipt = 0,
                                                PerNoOfPerson = 0,
                                                NetSales = 0
                                            };
                                        ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                        subTotal_ReceiptTotal += data.ReceiptTotal;
                                        ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                        subTotal_TC += data.NoOfReceipt;
                                        ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                        subTotal_Pax += data.NoOfPerson;
                                        ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                        ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        //Re caculate net sale
                                        //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);
                                        _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                        if (_isTaxInclude)
                                        {
                                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                        }
                                        //GC value
                                        var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i
                                                                          ).Sum(p => p.Amount);

                                        _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                        ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                        subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                                        ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                        dTmpFrom = dTmpFrom.AddHours(1);
                                        dTmpTo = dTmpTo.AddHours(1);
                                        currentRow++;
                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region Show data in report
                                    for (int i = 0; i < model.ToHour.Hours; i++)
                                    {
                                        //dTmpTo = dTmpFrom.AddHours(1);
                                        time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                        //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                        //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                        ws.Cells[currentRow, 1].Value = time;
                                        //set data
                                        //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                        HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                        if (data == null)
                                            data = new HourlySalesReportModels
                                            {
                                                ReceiptTotal = 0,
                                                NoOfReceipt = 0,
                                                NoOfPerson = 0,
                                                PerNoOfReceipt = 0,
                                                PerNoOfPerson = 0,
                                                NetSales = 0
                                            };
                                        ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                        subTotal_ReceiptTotal += data.ReceiptTotal;
                                        ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                        subTotal_TC += data.NoOfReceipt;
                                        ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                        subTotal_Pax += data.NoOfPerson;
                                        ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                        ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                        //Re caculate net sale
                                        //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);
                                        _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                        if (_isTaxInclude)
                                        {
                                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                        }
                                        //GC value
                                        var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i).Sum(p => p.Amount);

                                        _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                        ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                        subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);

                                        //ws.Cells[currentRow, 7].Value = data.NetSales;
                                        //subTotal_NetSale += data.NetSales;
                                        ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                        dTmpFrom = dTmpFrom.AddHours(1);
                                        dTmpTo = dTmpTo.AddHours(1);
                                        currentRow++;
                                    }
                                    #endregion
                                }
                            }
                        }
                        else //model.FromHour.Hours > model.ToHour.Hours
                        {
                            #region Show data in report
                            int hour = model.ToHour.Hours;
                            if (model.ToHour.Minutes == 59)
                                hour = model.ToHour.Hours + 1;

                            for (int i = model.FromHour.Hours; i < hour; i++)
                            {
                                //dTmpTo = dTmpFrom.AddHours(1);
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                //timeValue = int.Parse(dTmpFrom.ToString("HH"));
                                //time = string.Format("{0}:00 {1} - {2}:00 {3}", i, i < 12 ? "AM" : "PM", i + 1, i < 12 ? "AM" : "PM");
                                ws.Cells[currentRow, 1].Value = time;
                                //set data
                                //HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == timeValue && m.StoreId == storeId && m.Date == dTmpFrom.Date);
                                HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                if (data == null)
                                    data = new HourlySalesReportModels
                                    {
                                        ReceiptTotal = 0,
                                        NoOfReceipt = 0,
                                        NoOfPerson = 0,
                                        PerNoOfReceipt = 0,
                                        PerNoOfPerson = 0,
                                        NetSales = 0
                                    };
                                ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                subTotal_ReceiptTotal += data.ReceiptTotal;
                                ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                subTotal_TC += data.NoOfReceipt;
                                ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                subTotal_Pax += data.NoOfPerson;
                                ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                //Re caculate net sale
                                //_navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => ss.Amount);

                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                if (_isTaxInclude)
                                {
                                    _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                }
                                //GC value
                                var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i).Sum(p => p.Amount);

                                _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);

                                //ws.Cells[currentRow, 7].Value = data.NetSales;
                                //subTotal_NetSale += data.NetSales;
                                ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                dTmpFrom = dTmpFrom.AddHours(1);
                                dTmpTo = dTmpTo.AddHours(1);
                                currentRow++;
                            }
                            #endregion
                        }
                    }
                    //dTmpFrom = dGroup;
                    //dTmpTo = dGroup;
                    //End for date
                    listRange.Add((currentRow - 1).ToString());
                    listChart.Add(storeName, listRange);
                    #endregion
                    #region Sub Total
                    ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub. Total");
                    if (lstData.Count > 0)
                    {

                        total_ReceiptTotal += subTotal_ReceiptTotal;
                        total_TC += subTotal_TC;
                        total_Pax += subTotal_Pax;
                        total_NetSale += subTotal_NetSale;
                        // sub total 
                        ws.Cells[currentRow, 2].Value = subTotal_ReceiptTotal;
                        ws.Cells[currentRow, 3].Value = subTotal_TC;
                        ws.Cells[currentRow, 4].Value = subTotal_Pax;
                        ws.Cells[currentRow, 5].Value = subTotal_TC == 0 ? 0 : subTotal_ReceiptTotal / subTotal_TC;
                        ws.Cells[currentRow, 6].Value = subTotal_ReceiptTotal / (subTotal_Pax == 0 ? 1 : subTotal_Pax);
                        ws.Cells[currentRow, 7].Value = subTotal_NetSale;

                        ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    }
                    ws.Cells[beginCal, 2, currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[beginCal, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    lstSubTotalRowIndex.Add(currentRow);

                    #endregion
                    currentRow++;
                }//end store loop

                // Total
                #region Total
                ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                if (lstData.Count > 0)
                {
                    // total 
                    ws.Cells[currentRow, 2].Value = total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = total_TC;
                    ws.Cells[currentRow, 4].Value = total_Pax;
                    ws.Cells[currentRow, 5].Value = total_TC == 0 ? 0 : total_ReceiptTotal / total_TC;
                    ws.Cells[currentRow, 6].Value = total_ReceiptTotal / (total_Pax == 0 ? 1 : total_Pax);
                    ws.Cells[currentRow, 7].Value = total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                ws.Row(currentRow).Style.Font.Bold = true;
                ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion
                currentRow++;
                //FormatAllReport(ws, currentRow, totalCols);
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells.AutoFitColumns();

                if (model.FormatExport.Equals("Excel"))
                {
                    //Draw chart
                    ExcelChart chart = ws.Drawings.AddChart("HourlySales", eChartType.LineMarkersStacked);
                    chart.SetPosition(1, 0, 8, 0);
                    chart.SetSize(950, 600);
                    chart.AdjustPositionAndSize();
                    chart.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chart Title");
                    chart.Title.Font.Size = 14;

                    chart.XAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Period");
                    chart.XAxis.Title.Font.Size = 10;
                    chart.XAxis.Crosses = eCrosses.Max;
                    //chart.XAxis.Title.Font.Color = Color.Gray;

                    chart.YAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales Amount");
                    chart.YAxis.Title.Font.Size = 10;
                    //chart.YAxis.Title.Font.Color = Color.Gray;

                    ////Format Legend for Chart
                    chart.Legend.Remove();

                    for (int i = 0; i < listChart.Count; i++)
                    {
                        string storename = listChart.ElementAt(i).Key;
                        List<string> listRange = listChart.ElementAt(i).Value;
                        int startRow = 0;
                        int endRow = 0;
                        for (int k = 0; k < listRange.Count; k++)
                        {
                            startRow = int.Parse(listRange[k]);
                            endRow = int.Parse(listRange[k + 1]);
                            break;
                        }
                        string xRow = string.Format("A{0}:A{1}", startRow, endRow);
                        string yRow = string.Format("B{0}:B{1}", startRow, endRow);
                        var serie1 = chart.Series.Add(yRow, xRow);
                        serie1.Header = storename;
                        serie1.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;
                    }

                    //Data Table with Legend
                    var chartXml = chart.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("default", nsuri);

                    //Add the table node
                    var plotAreaNode = chartXml.SelectSingleNode("/default:chartSpace/default:chart/default:plotArea", nsm);
                    var dTableNode = chartXml.CreateNode(XmlNodeType.Element, "dTable", nsuri);
                    plotAreaNode.AppendChild(dTableNode);

                    //With Legend Flag
                    var att = chartXml.CreateAttribute("val");
                    att.Value = "1";

                    var showKeysNode = chartXml.CreateNode(XmlNodeType.Element, "showKeys", nsuri);
                    showKeysNode.Attributes.Append(att);
                    dTableNode.AppendChild(showKeysNode);

                    pck.Save();
                }
                return pck;
            }
            catch (Exception e)
            {
                return pck;
            }
        }

        // Updated 02/02/2018
        // List stores group by company
        public ExcelPackage ReportChart_New(RPHourlySalesModels model, List<StoreModels> lstStore)
        {
            ExcelPackage pck = new ExcelPackage();
            try
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly_Sales"));

                var lstStoreIndex = model.ListStores;
                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromHour.Hours, model.FromHour.Minutes, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToHour.Hours, model.ToHour.Minutes, 59);

                int totalCols = 7;
                CreateReportHeaderChart(ws, totalCols, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));
                // Format header report
                ws.Cells[1, 1, 4, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreIndex, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return pck;
                }

                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);

                var _lstNoIncludeOnSale = _noIncludeOnSaleDataFactory.GetListCateNoIncludeSaleForHourlySale(model.ListStores, model.FromDate, model.ToDate, model.Mode);
                if (_lstNoIncludeOnSale == null)
                    _lstNoIncludeOnSale = new List<NoIncludeOnSaleDataReportModels>();

                //Get info payment by GC
                var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                if (lstGC == null)
                    lstGC = new List<string>();
                List<PaymentModels> lstPayments = _orderPaymentMethodFactory.GetDataPaymentItemsByGCForHourlySale(model, lstGC);

                List<string> lstStores = model.ListStores;

                List<HourlySalesReportModels> lstData = GetData(model.FromDate, model.ToDate, lstStores, model.Mode);
                var _lstRounding = GetRoundingForHourly(model);
                if (lstData == null || !lstData.Any())
                    return pck;

                int fromHour = model.FromHour.Hours;

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Period"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"), System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX").ToLower()), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Avg/Pax"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cells[5, i + 1].Value = lstColNames[i];

                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.Bold = true;
                ws.Row(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Row(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion
                int currentRow = 6;
                List<int> lstSubTotalRowIndex = new List<int>();
                Dictionary<int, double> total = new Dictionary<int, double>();
                bool _isTaxInclude = true;
                //===========
                Dictionary<string, List<string>> listChart = new Dictionary<string, List<string>>();
                string storeId = string.Empty, storeName = string.Empty;
                // For Grand Total
                double total_ReceiptTotal = 0, total_TC = 0, total_Pax = 0, total_NetSale = 0;
                // For Company Total
                double company_total_ReceiptTotal = 0, company_total_TC = 0, company_total_Pax = 0, company_total_NetSale = 0;
                double _roundingValue = 0;

                // Group list stores by company
                var lstStore_Group = lstStore.GroupBy(g => new { CompanyId = g.CompanyId, CompanyName = g.CompanyName }).OrderBy(o => o.Key.CompanyName).ToList();
                int countCompany = lstStore_Group.Count();

                List<StoreModels> lstStoreOfCompany = new List<StoreModels>();
                foreach (var company in lstStore_Group)
                {
                    company_total_ReceiptTotal = 0;
                    company_total_TC = 0;
                    company_total_Pax = 0;
                    company_total_NetSale = 0;

                    lstStoreOfCompany = new List<StoreModels>();
                    lstStoreOfCompany = company.OrderBy(o => o.Name).ToList();
                    #region Company Name
                    ws.Cells[currentRow, 1].Value = company.Key.CompanyName;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                    #endregion
                    foreach (StoreModels store in lstStoreOfCompany)
                    {
                        _isTaxInclude = _baseFactory.IsTaxInclude(store.Id);
                        #region Store Name
                        storeId = store.Id;
                        storeName = store.Name;
                        ws.Cells[currentRow, 1].Value = storeName;
                        ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                        ws.Row(currentRow).Height = 20;
                        ws.Row(currentRow).Style.Font.Bold = true;
                        ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                        #endregion
                        List<string> listRange = new List<string>();
                        currentRow++;
                        listRange.Add(currentRow.ToString());

                        int beginCal = currentRow;
                        Dictionary<int, double> subTotal = new Dictionary<int, double>();
                        #region Data
                        DateTime dGroup = model.FromDate, dTmpFrom = DateTime.Now, dTmpTo = DateTime.Now;
                        //TimeSpan _timeSpan = new TimeSpan();
                        string time = string.Empty;
                        int totalHour = (int)model.ToDate.Subtract(model.FromDate).TotalHours;
                        double subTotal_ReceiptTotal = 0, subTotal_TC = 0, subTotal_Pax = 0, subTotal_NetSale = 0;
                        double _navValue = 0;
                        if (totalHour > 24)
                        {
                            fromHour = 0;
                            totalHour = 24;
                            #region Show data in report
                            for (int i = fromHour; i < totalHour; i++)
                            {
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                ws.Cells[currentRow, 1].Value = time;
                                //set data
                                HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                if (data == null)
                                    data = new HourlySalesReportModels
                                    {
                                        ReceiptTotal = 0,
                                        NoOfReceipt = 0,
                                        NoOfPerson = 0,
                                        PerNoOfReceipt = 0,
                                        PerNoOfPerson = 0,
                                        NetSales = 0
                                    };
                                ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                subTotal_ReceiptTotal += data.ReceiptTotal;
                                ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                subTotal_TC += data.NoOfReceipt;
                                ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                subTotal_Pax += data.NoOfPerson;
                                ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //Re caculate net sale
                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                if (_isTaxInclude)
                                {
                                    _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                }
                                //GC value
                                var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i
                                                                  ).Sum(p => p.Amount);

                                _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                                ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                dTmpFrom = dTmpFrom.AddHours(1);
                                dTmpTo = dTmpTo.AddHours(1);
                                currentRow++;
                            }
                            #endregion
                        }
                        else
                        {
                            if (model.FromHour.Hours > model.ToHour.Hours)
                            {
                                for (int lop = 0; lop < 2; lop++)
                                {
                                    if (lop == 0)
                                    {
                                        #region Show data in report
                                        for (int i = model.FromHour.Hours; i < 24; i++)
                                        {
                                            time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                            ws.Cells[currentRow, 1].Value = time;
                                            //set data
                                            HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                            if (data == null)
                                                data = new HourlySalesReportModels
                                                {
                                                    ReceiptTotal = 0,
                                                    NoOfReceipt = 0,
                                                    NoOfPerson = 0,
                                                    PerNoOfReceipt = 0,
                                                    PerNoOfPerson = 0,
                                                    NetSales = 0
                                                };
                                            ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                            subTotal_ReceiptTotal += data.ReceiptTotal;
                                            ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                            subTotal_TC += data.NoOfReceipt;
                                            ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                            subTotal_Pax += data.NoOfPerson;
                                            ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                            ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                            ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                            //Re caculate net sale
                                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                            if (_isTaxInclude)
                                            {
                                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                            }
                                            //GC value
                                            var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i
                                                                              ).Sum(p => p.Amount);

                                            _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                            ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                            subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                                            ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                            dTmpFrom = dTmpFrom.AddHours(1);
                                            dTmpTo = dTmpTo.AddHours(1);
                                            currentRow++;
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Show data in report
                                        for (int i = 0; i < model.ToHour.Hours; i++)
                                        {
                                            time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                            ws.Cells[currentRow, 1].Value = time;
                                            //set data
                                            HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                            if (data == null)
                                                data = new HourlySalesReportModels
                                                {
                                                    ReceiptTotal = 0,
                                                    NoOfReceipt = 0,
                                                    NoOfPerson = 0,
                                                    PerNoOfReceipt = 0,
                                                    PerNoOfPerson = 0,
                                                    NetSales = 0
                                                };
                                            ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                            subTotal_ReceiptTotal += data.ReceiptTotal;
                                            ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                            subTotal_TC += data.NoOfReceipt;
                                            ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                            subTotal_Pax += data.NoOfPerson;
                                            ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                            ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                            ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                            //Re caculate net sale
                                            _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                            if (_isTaxInclude)
                                            {
                                                _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                            }
                                            //GC value
                                            var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i).Sum(p => p.Amount);

                                            _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                            ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                            subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                                            ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                            dTmpFrom = dTmpFrom.AddHours(1);
                                            dTmpTo = dTmpTo.AddHours(1);
                                            currentRow++;
                                        }
                                        #endregion
                                    }
                                }
                            }
                            else //model.FromHour.Hours > model.ToHour.Hours
                            {
                                #region Show data in report
                                int hour = model.ToHour.Hours;
                                if (model.ToHour.Minutes == 59)
                                    hour = model.ToHour.Hours + 1;

                                for (int i = model.FromHour.Hours; i < hour; i++)
                                {
                                    time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                    ws.Cells[currentRow, 1].Value = time;
                                    //set data
                                    HourlySalesReportModels data = lstData.FirstOrDefault(m => m.Time == i && m.StoreId == storeId);
                                    if (data == null)
                                        data = new HourlySalesReportModels
                                        {
                                            ReceiptTotal = 0,
                                            NoOfReceipt = 0,
                                            NoOfPerson = 0,
                                            PerNoOfReceipt = 0,
                                            PerNoOfPerson = 0,
                                            NetSales = 0
                                        };
                                    ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                    subTotal_ReceiptTotal += data.ReceiptTotal;
                                    ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                    ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                    subTotal_TC += data.NoOfReceipt;
                                    ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                    ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                    subTotal_Pax += data.NoOfPerson;
                                    ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                    ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                    ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                    ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                    ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                    //Re caculate net sale
                                    _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                    if (_isTaxInclude)
                                    {
                                        _navValue = _lstNoIncludeOnSale.Where(ww => ww.StoreId == storeId && ww.Time == i).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                    }
                                    //GC value
                                    var payGC = lstPayments.Where(p => p.StoreId == storeId && p.Time == i).Sum(p => p.Amount);

                                    _roundingValue = _lstRounding.Where(m => m.Time == i && m.StoreId == storeId).Sum(ss => ss.RoundingValue);

                                    ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                    subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                                    ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                                    dTmpFrom = dTmpFrom.AddHours(1);
                                    dTmpTo = dTmpTo.AddHours(1);
                                    currentRow++;
                                }
                                #endregion
                            }
                        }
                        //End for date
                        listRange.Add((currentRow - 1).ToString());
                        listChart.Add(storeName, listRange);
                        #endregion
                        #region Sub Total
                        company_total_ReceiptTotal += subTotal_ReceiptTotal;
                        company_total_TC += subTotal_TC;
                        company_total_Pax += subTotal_Pax;
                        company_total_NetSale += subTotal_NetSale;

                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub. Total");
                        // sub total 
                        ws.Cells[currentRow, 2].Value = subTotal_ReceiptTotal;
                        ws.Cells[currentRow, 3].Value = subTotal_TC;
                        ws.Cells[currentRow, 4].Value = subTotal_Pax;
                        ws.Cells[currentRow, 5].Value = subTotal_TC == 0 ? 0 : subTotal_ReceiptTotal / subTotal_TC;
                        ws.Cells[currentRow, 6].Value = subTotal_ReceiptTotal / (subTotal_Pax == 0 ? 1 : subTotal_Pax);
                        ws.Cells[currentRow, 7].Value = subTotal_NetSale;

                        ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[beginCal, 2, currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                        ws.Cells[beginCal, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                        ws.Row(currentRow).Style.Font.Bold = true;
                        ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                        lstSubTotalRowIndex.Add(currentRow);
                        #endregion
                        currentRow++;
                    }//end store loop

                    // Company summary
                    #region Company Total
                    if (countCompany > 1) // Multi company selected
                    {
                        total_ReceiptTotal += company_total_ReceiptTotal;
                        total_TC += company_total_TC;
                        total_Pax += company_total_Pax;
                        total_NetSale += company_total_NetSale;

                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("COMPANY TOTAL");
                    }
                    else
                    {
                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    }
                    // Company Total 
                    ws.Cells[currentRow, 2].Value = company_total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = company_total_TC;
                    ws.Cells[currentRow, 4].Value = company_total_Pax;
                    ws.Cells[currentRow, 5].Value = company_total_TC == 0 ? 0 : company_total_ReceiptTotal / company_total_TC;
                    ws.Cells[currentRow, 6].Value = company_total_ReceiptTotal / (company_total_Pax == 0 ? 1 : company_total_Pax);
                    ws.Cells[currentRow, 7].Value = company_total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                    #endregion
                }
                // Total
                #region Total
                if (countCompany > 1) // Multi company selected
                {
                    ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    // total 
                    ws.Cells[currentRow, 2].Value = total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = total_TC;
                    ws.Cells[currentRow, 4].Value = total_Pax;
                    ws.Cells[currentRow, 5].Value = total_TC == 0 ? 0 : total_ReceiptTotal / total_TC;
                    ws.Cells[currentRow, 6].Value = total_ReceiptTotal / (total_Pax == 0 ? 1 : total_Pax);
                    ws.Cells[currentRow, 7].Value = total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                }
                #endregion
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells.AutoFitColumns();

                if (model.FormatExport.Equals("Excel"))
                {
                    //Draw chart
                    ExcelChart chart = ws.Drawings.AddChart("HourlySales", eChartType.LineMarkersStacked);
                    chart.SetPosition(1, 0, 8, 0);
                    chart.SetSize(950, 600);
                    chart.AdjustPositionAndSize();
                    chart.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chart Title");
                    chart.Title.Font.Size = 14;

                    chart.XAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Period");
                    chart.XAxis.Title.Font.Size = 10;
                    chart.XAxis.Crosses = eCrosses.Max;
                    //chart.XAxis.Title.Font.Color = Color.Gray;

                    chart.YAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales Amount");
                    chart.YAxis.Title.Font.Size = 10;
                    //chart.YAxis.Title.Font.Color = Color.Gray;

                    ////Format Legend for Chart
                    chart.Legend.Remove();

                    for (int i = 0; i < listChart.Count; i++)
                    {
                        string storename = listChart.ElementAt(i).Key;
                        List<string> listRange = listChart.ElementAt(i).Value;
                        int startRow = 0;
                        int endRow = 0;
                        for (int k = 0; k < listRange.Count; k++)
                        {
                            startRow = int.Parse(listRange[k]);
                            endRow = int.Parse(listRange[k + 1]);
                            break;
                        }
                        string xRow = string.Format("A{0}:A{1}", startRow, endRow);
                        string yRow = string.Format("B{0}:B{1}", startRow, endRow);
                        var serie1 = chart.Series.Add(yRow, xRow);
                        serie1.Header = storename;
                        serie1.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;
                    }

                    //Data Table with Legend
                    var chartXml = chart.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("default", nsuri);

                    //Add the table node
                    var plotAreaNode = chartXml.SelectSingleNode("/default:chartSpace/default:chart/default:plotArea", nsm);
                    var dTableNode = chartXml.CreateNode(XmlNodeType.Element, "dTable", nsuri);
                    plotAreaNode.AppendChild(dTableNode);

                    //With Legend Flag
                    var att = chartXml.CreateAttribute("val");
                    att.Value = "1";

                    var showKeysNode = chartXml.CreateNode(XmlNodeType.Element, "showKeys", nsuri);
                    showKeysNode.Attributes.Append(att);
                    dTableNode.AppendChild(showKeysNode);

                    pck.Save();
                }
                return pck;
            }
            catch (Exception e)
            {
                return pck;
            }
        }

        #region Report with filter time & get data depend on businessday information of store, updated 03262018
        public List<HourlySalesReportModels> GetData_NewFilter(DateTime dFrom, DateTime dTo, List<string> lstStores, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_HourlySalesReport
                               where lstStores.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= dFrom && tb.CreatedDate <= dTo)
                                     && tb.Mode == mode
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate
                               } into g
                               select new HourlySalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   CreatedDate = g.Key.CreatedDate,
                                   ReceiptTotal = g.Sum(x => x.ReceiptTotal),
                                   NoOfReceipt = g.Count(),
                                   NoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 : g.Sum(x => x.NoOfPerson),
                                   //PerNoOfReceipt = (g.Sum(x => x.ReceiptTotal) / (g.Count() == 0 ? 1 : g.Count())),
                                   //PerNoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 :
                                   // g.Sum(x => x.ReceiptTotal) / (g.Sum(x => x.NoOfPerson) == 0 ? 1 : g.Sum(x => x.NoOfPerson)),
                                   NetSales = g.Sum(x => x.NetSales),
                               }).ToList();
                return lstData;
            }
        }

        //Get rouding for hourly sale report
        public List<HourlySalesReportModels> GetRoundingForHourly_NewFilter(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DailyReceiptReport
                               where model.ListStores.Contains(tb.StoreId)
                                     && tb.CreatedDate >= model.FromDate
                                             && tb.CreatedDate <= model.ToDate
                                             && tb.Mode == model.Mode
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

        public List<NoIncludeOnSaleDataReportModels> GetListCateNoIncludeSaleForHourlySale_NewFilter(List<string> lstStoreIds, DateTime dFrom, DateTime dTo, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResults = cxt.R_NoIncludeOnSaleDataReport.Where(ww => lstStoreIds.Contains(ww.StoreId) &&
                ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo && ww.Mode == mode)
                    .GroupBy(gg => new
                    {
                        StoreId = gg.StoreId,
                        CreatedDate = gg.CreatedDate
                    })
                    .Select(ss => new NoIncludeOnSaleDataReportModels()
                    {
                        StoreId = ss.Key.StoreId,
                        CreatedDate = ss.Key.CreatedDate,
                        Amount = ss.Sum(aa => aa.Amount),
                        DiscountAmount = ss.Sum(aa => aa.DiscountAmount),
                        PromotionAmount = ss.Sum(aa => aa.PromotionAmount),
                        Tax = ss.Sum(aa => aa.Tax)
                    }).ToList();

                return lstResults;
            }
        }

        public List<PaymentModels> GetDataPaymentItemsByGCForHourlySale_NewFilter(BaseReportModel model, List<string> lstGCId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.G_PaymentMenthod.Where(ww => model.ListStores.Contains(ww.StoreId)
                    && (ww.CreatedDate >= model.FromDate
                    && ww.CreatedDate <= model.ToDate) && ww.Mode == model.Mode
                    && (lstGCId.Contains(ww.PaymentId) || ww.PaymentCode == (int)Commons.EPaymentCode.GiftCard))
                    .GroupBy(gg => new
                    {
                        StoreId = gg.StoreId,
                        CreatedDate = gg.CreatedDate
                    })
                    .Select(tb => new PaymentModels
                    {
                        StoreId = tb.Key.StoreId,
                        CreatedDate = tb.Key.CreatedDate,
                        Amount = tb.Sum(ss => ss.Amount)
                    }).ToList();
                return lstData;
            }
        }

        public ExcelPackage ReportChart_NewFilter(RPHourlySalesModels model, List<StoreModels> lstStore)
        {
            ExcelPackage pck = new ExcelPackage();
            try
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly_Sales"));

                int totalCols = 7;

                // Create header for report
                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromHour.Hours, 0, 0);
                DateTime dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToHour.Hours, 0, 0);
                CreateReportHeaderChart(ws, totalCols, dFrom, dTo, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));

                // Format header report
                ws.Cells[1, 1, 4, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return pck;
                }

                // Updated inputs from business information
                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                model.ListStores = _lstBusDayAllStore.Select(ss => ss.StoreId).Distinct().ToList();

                // Get data sale
                List<HourlySalesReportModels> lstData = GetData_NewFilter(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (lstData == null || !lstData.Any())
                {
                    return pck;
                }
                else
                {
                    model.ListStores = lstData.Select(ss => ss.StoreId).Distinct().ToList(); // Get list store from data sale
                }
                var _lstRounding = GetRoundingForHourly_NewFilter(model);

                // Get list time range
                int fromTime = model.FromHour.Hours;
                int toTime = model.ToHour.Hours;
                if (toTime == 0) // ToHour == 0 <=> ToHour == 24
                {
                    toTime = 24;
                }
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

                // Get list categories no include sale
                var _lstNoIncludeOnSale = GetListCateNoIncludeSaleForHourlySale_NewFilter(model.ListStores, model.FromDate, model.ToDate, model.Mode);
                if (_lstNoIncludeOnSale == null)
                    _lstNoIncludeOnSale = new List<NoIncludeOnSaleDataReportModels>();

                // Get info payment by GC
                var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                if (lstGC == null)
                    lstGC = new List<string>();
                List<PaymentModels> lstPayments = GetDataPaymentItemsByGCForHourlySale_NewFilter(model, lstGC);

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Period"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"), System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX").ToLower()), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Avg/Pax"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cells[5, i + 1].Value = lstColNames[i];

                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.Bold = true;
                ws.Row(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Row(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion

                int currentRow = 6;
                List<int> lstSubTotalRowIndex = new List<int>();
                Dictionary<int, double> total = new Dictionary<int, double>();
                bool _isTaxInclude = true;
                //===========
                Dictionary<string, List<string>> listChart = new Dictionary<string, List<string>>();

                // For Grand Total
                double total_ReceiptTotal = 0, total_TC = 0, total_Pax = 0, total_NetSale = 0;

                // For Company Total
                double company_total_ReceiptTotal = 0, company_total_TC = 0, company_total_Pax = 0, company_total_NetSale = 0;
                double _roundingValue = 0;

                // Group list stores by company
                var lstStore_Group = lstStore.GroupBy(g => new { CompanyId = g.CompanyId, CompanyName = g.CompanyName }).OrderBy(o => o.Key.CompanyName).ToList();
                int countCompany = lstStore_Group.Count();

                List<StoreModels> lstStoreOfCompany = new List<StoreModels>();
                var lstBusStore = new List<BusinessDayDisplayModels>();
                DateTime fromDateStore = new DateTime();
                DateTime toDateStore = new DateTime();
                var _lstDataStore = new List<HourlySalesReportModels>();
                var _lstRoundingStore = new List<HourlySalesReportModels>();
                var _lstNoIncludeOnSaleStore = new List<NoIncludeOnSaleDataReportModels>();
                var _lstPaymentsStore = new List<PaymentModels>();

                foreach (var company in lstStore_Group)
                {
                    company_total_ReceiptTotal = 0;
                    company_total_TC = 0;
                    company_total_Pax = 0;
                    company_total_NetSale = 0;

                    // Get list store information of company from list data sale
                    lstStoreOfCompany = new List<StoreModels>();
                    lstStoreOfCompany = company.Where(ww => model.ListStores.Contains(ww.Id)).OrderBy(o => o.Name).ToList();

                    // Company name
                    #region Company Name
                    ws.Cells[currentRow, 1].Value = company.Key.CompanyName;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                    #endregion

                    foreach (StoreModels store in lstStoreOfCompany)
                    {
                        // Get businessday of store
                        lstBusStore = _lstBusDayAllStore.Where(ww => ww.StoreId == store.Id).ToList();
                        fromDateStore = lstBusStore.Min(ww => ww.DateFrom);
                        toDateStore = lstBusStore.Max(ww => ww.DateTo);

                        // Get list data of store
                        _lstDataStore = lstData.Where(ww => ww.StoreId == store.Id && ww.CreatedDate >= fromDateStore && ww.CreatedDate <= toDateStore).ToList();

                        _lstRoundingStore = _lstRounding.Where(ww => ww.StoreId == store.Id && ww.CreatedDate >= fromDateStore && ww.CreatedDate <= toDateStore).ToList();

                        _lstNoIncludeOnSaleStore = _lstNoIncludeOnSale.Where(ww => ww.StoreId == store.Id && ww.CreatedDate >= fromDateStore && ww.CreatedDate <= toDateStore).ToList();

                        _lstPaymentsStore = lstPayments.Where(ww => ww.StoreId == store.Id && ww.CreatedDate >= fromDateStore && ww.CreatedDate <= toDateStore).ToList();


                        if (_lstDataStore != null && _lstDataStore.Any())
                        {
                            _isTaxInclude = _baseFactory.IsTaxInclude(store.Id);

                            // Store name
                            #region Store Name
                            ws.Cells[currentRow, 1].Value = store.Name;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                            ws.Row(currentRow).Height = 20;
                            ws.Row(currentRow).Style.Font.Bold = true;
                            ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                            #endregion

                            List<string> listRange = new List<string>();
                            currentRow++;
                            listRange.Add(currentRow.ToString());

                            int beginCal = currentRow;
                            Dictionary<int, double> subTotal = new Dictionary<int, double>();

                            #region Data
                            DateTime dGroup = model.FromDate, dTmpFrom = DateTime.Now, dTmpTo = DateTime.Now;

                            double subTotal_ReceiptTotal = 0, subTotal_TC = 0, subTotal_Pax = 0, subTotal_NetSale = 0;
                            double _navValue = 0;

                            // Get list data of store depend on list time range
                            _lstDataStore = _lstDataStore.Where(ww => listTimesRange.Contains(ww.CreatedDate.Hour)).ToList();
                            if (_lstDataStore == null)
                                _lstDataStore = new List<HourlySalesReportModels>();
                            //if (_lstDataStore != null && _lstDataStore.Any())
                            //{
                            //var lstTime = _lstDataStore.Select(ss => ss.CreatedDate.Hour).Distinct().ToList();
                            //var lstTimeRangeStore = listTimesRange.Where(ww => lstTime.Contains(ww)).ToList();
                            //var lstTimeRangeStore = listTimesRange;

                            //for (int i = 0; i < listTimesRange.Count; i++)
                            for (int i = 0; i < listTimesRange.Count; i++)
                            {
                                ws.Cells[currentRow, 1].Value = string.Format("{0}:00 - {1}:00", listTimesRange[i], (listTimesRange[i] + 1));

                                // Set data
                                HourlySalesReportModels data = _lstDataStore.Where(m => m.CreatedDate.Hour == listTimesRange[i])
                                    .GroupBy(gg => gg.CreatedDate.Hour)
                                    .Select(s => new HourlySalesReportModels()
                                    {
                                        ReceiptTotal = s.Sum(x => x.ReceiptTotal),
                                        NoOfReceipt = s.Count(),
                                        NoOfPerson = s.Sum(x => (int?)x.NoOfPerson) == null ? 0 : s.Sum(x => x.NoOfPerson),
                                        PerNoOfReceipt = (s.Sum(x => x.ReceiptTotal) / (s.Count() == 0 ? 1 : s.Count())),
                                        PerNoOfPerson = s.Sum(x => (int?)x.NoOfPerson) == null ? 0 :
                                            s.Sum(x => x.ReceiptTotal) / (s.Sum(x => x.NoOfPerson) == 0 ? 1 : s.Sum(x => x.NoOfPerson)),
                                        NetSales = s.Sum(x => x.NetSales),
                                    }).FirstOrDefault();
                                if (data == null)
                                    data = new HourlySalesReportModels();

                                ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                subTotal_ReceiptTotal += data.ReceiptTotal;
                                ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                subTotal_TC += data.NoOfReceipt;
                                ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                subTotal_Pax += data.NoOfPerson;
                                ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                // Re caculate net sale
                                if (_isTaxInclude)
                                {
                                    _navValue = _lstNoIncludeOnSaleStore.Where(ww => ww.CreatedDate.Hour == listTimesRange[i]).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                                }
                                else
                                {
                                    _navValue = _lstNoIncludeOnSaleStore.Where(ww => ww.CreatedDate.Hour == listTimesRange[i]).Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                                }

                                // GC value
                                var payGC = _lstPaymentsStore.Where(p => p.CreatedDate.Hour == listTimesRange[i]
                                                                  ).Sum(p => p.Amount);
                                // Rounding value
                                _roundingValue = _lstRoundingStore.Where(m => m.CreatedDate.Hour == listTimesRange[i]).Sum(ss => ss.RoundingValue);

                                ws.Cells[currentRow, 7].Value = (data.NetSales - _navValue - payGC + _roundingValue);
                                subTotal_NetSale += (data.NetSales - _navValue - payGC + _roundingValue);
                                ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                dTmpFrom = dTmpFrom.AddHours(1);
                                dTmpTo = dTmpTo.AddHours(1);
                                currentRow++;
                            }
                            //}

                            listRange.Add((currentRow - 1).ToString());
                            listChart.Add(store.Name, listRange);
                            #endregion

                            #region Sub Total
                            company_total_ReceiptTotal += subTotal_ReceiptTotal;
                            company_total_TC += subTotal_TC;
                            company_total_Pax += subTotal_Pax;
                            company_total_NetSale += subTotal_NetSale;

                            ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub. Total");
                            // sub total 
                            ws.Cells[currentRow, 2].Value = subTotal_ReceiptTotal;
                            ws.Cells[currentRow, 3].Value = subTotal_TC;
                            ws.Cells[currentRow, 4].Value = subTotal_Pax;
                            ws.Cells[currentRow, 5].Value = subTotal_TC == 0 ? 0 : subTotal_ReceiptTotal / subTotal_TC;
                            ws.Cells[currentRow, 6].Value = subTotal_ReceiptTotal / (subTotal_Pax == 0 ? 1 : subTotal_Pax);
                            ws.Cells[currentRow, 7].Value = subTotal_NetSale;

                            ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[beginCal, 2, currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                            ws.Cells[beginCal, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                            ws.Row(currentRow).Style.Font.Bold = true;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                            lstSubTotalRowIndex.Add(currentRow);
                            #endregion

                            currentRow++;
                        }

                    }//end store loop

                    // Company summary
                    #region Company Total
                    if (countCompany > 1) // Multi company selected
                    {
                        total_ReceiptTotal += company_total_ReceiptTotal;
                        total_TC += company_total_TC;
                        total_Pax += company_total_Pax;
                        total_NetSale += company_total_NetSale;

                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("COMPANY TOTAL");
                    }
                    else
                    {
                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    }
                    // Company Total 
                    ws.Cells[currentRow, 2].Value = company_total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = company_total_TC;
                    ws.Cells[currentRow, 4].Value = company_total_Pax;
                    ws.Cells[currentRow, 5].Value = company_total_TC == 0 ? 0 : company_total_ReceiptTotal / company_total_TC;
                    ws.Cells[currentRow, 6].Value = company_total_ReceiptTotal / (company_total_Pax == 0 ? 1 : company_total_Pax);
                    ws.Cells[currentRow, 7].Value = company_total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                    #endregion
                }
                // Total
                #region Total
                if (countCompany > 1) // Multi company selected
                {
                    ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    // total 
                    ws.Cells[currentRow, 2].Value = total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = total_TC;
                    ws.Cells[currentRow, 4].Value = total_Pax;
                    ws.Cells[currentRow, 5].Value = total_TC == 0 ? 0 : total_ReceiptTotal / total_TC;
                    ws.Cells[currentRow, 6].Value = total_ReceiptTotal / (total_Pax == 0 ? 1 : total_Pax);
                    ws.Cells[currentRow, 7].Value = total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                }
                #endregion
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells.AutoFitColumns();

                if (model.FormatExport.Equals("Excel"))
                {
                    //Draw chart
                    ExcelChart chart = ws.Drawings.AddChart("HourlySales", eChartType.LineMarkersStacked);
                    chart.SetPosition(1, 0, 8, 0);
                    chart.SetSize(950, 600);
                    chart.AdjustPositionAndSize();
                    chart.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chart Title");
                    chart.Title.Font.Size = 14;

                    chart.XAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Period");
                    chart.XAxis.Title.Font.Size = 10;
                    chart.XAxis.Crosses = eCrosses.Max;
                    //chart.XAxis.Title.Font.Color = Color.Gray;

                    chart.YAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales Amount");
                    chart.YAxis.Title.Font.Size = 10;
                    //chart.YAxis.Title.Font.Color = Color.Gray;

                    ////Format Legend for Chart
                    chart.Legend.Remove();

                    int startRow = 0;
                    int endRow = 0;
                    for (int i = 0; i < listChart.Count; i++)
                    {
                        string storename = listChart.ElementAt(i).Key;
                        List<string> listRange = listChart.ElementAt(i).Value;

                        startRow = int.Parse(listRange[0]);
                        endRow = int.Parse(listRange[1]);
                        string xRow = string.Format("A{0}:A{1}", startRow, endRow);
                        string yRow = string.Format("B{0}:B{1}", startRow, endRow);
                        var serie1 = chart.Series.Add(yRow, xRow);
                        serie1.Header = storename;
                        serie1.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;
                    }

                    //Data Table with Legend
                    var chartXml = chart.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("default", nsuri);

                    //Add the table node
                    var plotAreaNode = chartXml.SelectSingleNode("/default:chartSpace/default:chart/default:plotArea", nsm);
                    var dTableNode = chartXml.CreateNode(XmlNodeType.Element, "dTable", nsuri);
                    plotAreaNode.AppendChild(dTableNode);

                    //With Legend Flag
                    var att = chartXml.CreateAttribute("val");
                    att.Value = "1";

                    var showKeysNode = chartXml.CreateNode(XmlNodeType.Element, "showKeys", nsuri);
                    showKeysNode.Attributes.Append(att);
                    dTableNode.AppendChild(showKeysNode);

                    pck.Save();
                }
                return pck;
            }
            catch (Exception e)
            {
                return pck;
            }
        }

        #region Report with Credit Note info (refund gift card), updated 04092018
        public List<HourlySalesReportModels> GetData_WithCreditNote(DateTime dFrom, DateTime dTo, List<string> lstStores, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_HourlySalesReport
                               where lstStores.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= dFrom && tb.CreatedDate <= dTo)
                                     && tb.Mode == mode
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                                   ReceiptId = tb.ReceiptId
                               } into g
                               select new HourlySalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   CreatedDate = g.Key.CreatedDate,
                                   ReceiptId = g.Key.ReceiptId,
                                   ReceiptTotal = g.Sum(x => x.ReceiptTotal),
                                   NoOfReceipt = g.Count(),
                                   NoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 : g.Sum(x => x.NoOfPerson),
                                   NetSales = g.Sum(x => x.NetSales),
                                   CreditNoteNo = g.Select(s => s.CreditNoteNo).FirstOrDefault()
                               }).ToList();
                return lstData;
            }
        }
        public List<HourlySalesReportModels> GetData_WithCreditNote2(RPHourlySalesModels model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DailyReceiptReport
                               where model.ListStores.Contains(tb.StoreId)
                                     && tb.CreatedDate >= model.FromDate
                                             && tb.CreatedDate <= model.ToDate
                                             && tb.Mode == model.Mode
                               select new HourlySalesReportModels
                               {
                                   BusinessDayId = tb.BusinessDayId,
                                   ReceiptId = tb.ReceiptId,
                                   ReceiptNo = tb.ReceiptNo,
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                                   NoOfPerson = tb.NoOfPerson,
                                   ReceiptTotal = tb.ReceiptTotal,      // Total R_Receipt
                                   Discount = tb.Discount,
                                   ServiceCharge = tb.ServiceCharge,
                                   GST = tb.GST,
                                   Rounding = tb.Rounding,
                                   NetSales = tb.NetSales,
                                   PromotionValue = tb.PromotionValue,
                                   CreditNoteNo = tb.CreditNoteNo
                               }).ToList();
                return lstData;
            }
        }

        public ExcelPackage ReportChart_WithCreditNote(RPHourlySalesModels model, List<StoreModels> lstStore)
        {
            ExcelPackage pck = new ExcelPackage();
            try
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly_Sales"));

                int totalCols = 7;

                // Create header for report
                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromHour.Hours, 0, 0);
                DateTime dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToHour.Hours, 0, 0);
                CreateReportHeaderChart(ws, totalCols, dFrom, dTo, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));

                // Format header report
                ws.Cells[1, 1, 4, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return pck;
                }

                // Updated inputs from business information
                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                model.ListStores = _lstBusDayAllStore.Select(ss => ss.StoreId).Distinct().ToList();
                
                // Get data sale
                //List<HourlySalesReportModels> lstData = GetData_WithCreditNote(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                //2018-05-21 get data
                List<HourlySalesReportModels> lstData = GetData_WithCreditNote2(model);
                if (lstData == null || !lstData.Any())
                {
                    return pck;
                }
                else
                {
                    model.ListStores = lstData.Select(ss => ss.StoreId).Distinct().ToList(); // Get list store from data sale
                }
                //var _lstRounding = GetRoundingForHourly_NewFilter(model);

                // Get list time range
                int fromTime = model.FromHour.Hours;
                int toTime = model.ToHour.Hours;
                if (toTime == 0) // ToHour == 0 <=> ToHour == 24
                {
                    toTime = 24;
                }
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

                // Get data no include sale
                var _itemizedSalesAnalysisFactory = new ItemizedSalesAnalysisReportFactory();
                var lstReceiptId = lstData.Select(ss => ss.ReceiptId).ToList();
                var lstItemNoIncludeSale = _itemizedSalesAnalysisFactory.GetItemsNoIncludeSale(lstReceiptId, model.ListStores, model.Mode);
                // Get info payment by GC
                var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                if (lstGC == null)
                    lstGC = new List<string>();
                List<PaymentModels> lstPayments = _dailySalesReportFactory.GetDataPaymentItems(model);

                // Data of Refund
                var _lstRefund = _refundFactory.GetListRefundWithoutDetail(model);
                if (_lstRefund == null)
                    _lstRefund = new List<RefundReportDTO>();

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Period"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"), System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX").ToLower()), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Avg/Pax"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cells[5, i + 1].Value = lstColNames[i];

                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.Bold = true;
                ws.Row(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Row(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion

                int currentRow = 6;
                List<int> lstSubTotalRowIndex = new List<int>();
                Dictionary<int, double> total = new Dictionary<int, double>();
                bool _isTaxInclude = true;
                //===========
                Dictionary<string, List<string>> listChart = new Dictionary<string, List<string>>();

                // For Grand Total
                double total_ReceiptTotal = 0, total_TC = 0, total_Pax = 0, total_NetSale = 0;
                // For Company Total
                double company_total_ReceiptTotal = 0, company_total_TC = 0, company_total_Pax = 0, company_total_NetSale = 0;

                // Group list stores by company
                var lstStore_Group = lstStore.GroupBy(g => new { CompanyId = g.CompanyId, CompanyName = g.CompanyName }).OrderBy(o => o.Key.CompanyName).ToList();
                int countCompany = lstStore_Group.Count();

                List<StoreModels> lstStoreOfCompany = new List<StoreModels>();
                var lstBusStore = new List<BusinessDayDisplayModels>();
                DateTime fromDateStore = new DateTime();
                DateTime toDateStore = new DateTime();
                var _lstDataStore = new List<HourlySalesReportModels>();
                var _lstPaymentsStore = new List<PaymentModels>();
                var data = new HourlySalesReportModels();

                foreach (var company in lstStore_Group)
                {
                    company_total_ReceiptTotal = 0;
                    company_total_TC = 0;
                    company_total_Pax = 0;
                    company_total_NetSale = 0;

                    // Get list store information of company from list data sale
                    lstStoreOfCompany = new List<StoreModels>();
                    lstStoreOfCompany = company.Where(ww => model.ListStores.Contains(ww.Id)).OrderBy(o => o.Name).ToList();

                    // Company name
                    #region Company Name
                    ws.Cells[currentRow, 1].Value = company.Key.CompanyName;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                    #endregion
                    double payGC = 0;
                    double _taxOfPayGCNotInclude = 0;
                    double _svcOfPayGCNotInclude = 0;
                    double _totalCreditNote = 0;
                    List<string> _lstReceiptId = new List<string>();

                    foreach (StoreModels store in lstStoreOfCompany)
                    {
                        // Get businessday of store
                        lstBusStore = _lstBusDayAllStore.Where(ww => ww.StoreId == store.Id).ToList();
                        fromDateStore = lstBusStore.Min(ww => ww.DateFrom);
                        toDateStore = lstBusStore.Max(ww => ww.DateTo);
                        // Get list data of store
                        _lstDataStore = lstData.Where(ww => ww.StoreId == store.Id && ww.CreatedDate >= fromDateStore && ww.CreatedDate <= toDateStore).ToList();
                        _lstPaymentsStore = lstPayments.Where(ww => ww.StoreId == store.Id && ww.CreatedDate >= fromDateStore && ww.CreatedDate <= toDateStore).ToList();

                        if (_lstDataStore != null && _lstDataStore.Any())
                        {
                            _isTaxInclude = _baseFactory.IsTaxInclude(store.Id);

                            // Store name
                            #region Store Name
                            ws.Cells[currentRow, 1].Value = store.Name;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                            ws.Row(currentRow).Height = 20;
                            ws.Row(currentRow).Style.Font.Bold = true;
                            ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                            #endregion

                            List<string> listRange = new List<string>();
                            currentRow++;
                            listRange.Add(currentRow.ToString());

                            int beginCal = currentRow;
                            Dictionary<int, double> subTotal = new Dictionary<int, double>();

                            #region Data
                            DateTime dGroup = model.FromDate, dTmpFrom = DateTime.Now, dTmpTo = DateTime.Now;
                            double subTotal_ReceiptTotal = 0, subTotal_TC = 0, subTotal_Pax = 0, subTotal_NetSale = 0;
                            double _netSale = 0;

                            // Get list data of store depend on list time range
                            _lstDataStore = _lstDataStore.Where(ww => listTimesRange.Contains(ww.CreatedDate.Hour)).ToList();
                            if (_lstDataStore == null)
                                _lstDataStore = new List<HourlySalesReportModels>();
                            //if (_lstDataStore != null && _lstDataStore.Any())
                            //{
                            //var lstTime = _lstDataStore.Select(ss => ss.CreatedDate.Hour).Distinct().ToList();
                            //var lstTimeRangeStore = listTimesRange.Where(ww => lstTime.Contains(ww)).ToList();
                            //var lstTimeRangeStore = listTimesRange;

                            //for (int i = 0; i < listTimesRange.Count; i++)
                            for (int i = 0; i < listTimesRange.Count; i++)
                            {
                                ws.Cells[currentRow, 1].Value = string.Format("{0}:00 - {1}:00", listTimesRange[i], (listTimesRange[i] + 1));

                                // Set data
                                data = _lstDataStore.Where(m => m.CreatedDate.Hour == listTimesRange[i] && string.IsNullOrEmpty(m.CreditNoteNo))
                                    .GroupBy(gg => gg.CreatedDate.Hour)
                                    .Select(s => new HourlySalesReportModels()
                                    {
                                        ReceiptTotal = s.Sum(x => x.ReceiptTotal),
                                        NoOfReceipt = s.Count(),
                                        NoOfPerson = s.Sum(x => (int?)x.NoOfPerson) == null ? 0 : s.Sum(x => x.NoOfPerson),
                                        PerNoOfReceipt = (s.Sum(x => x.ReceiptTotal) / (s.Count() == 0 ? 1 : s.Count())),
                                        PerNoOfPerson = s.Sum(x => (int?)x.NoOfPerson) == null ? 0 :
                                            s.Sum(x => x.ReceiptTotal) / (s.Sum(x => x.NoOfPerson) == 0 ? 1 : s.Sum(x => x.NoOfPerson)),
                                        //CreditNoteTotal = s.Where(ww => ww.CreditNoteNo == null || ww.CreditNoteNo.Length == 0).Sum(x => x.ReceiptTotal),
                                        ServiceCharge = s.Sum(ss => ss.ServiceCharge),
                                        GST = s.Sum(ss => ss.GST)

                                    }).FirstOrDefault();

                                // Credit Note info for get total Netsale
                                _lstReceiptId = _lstDataStore.Where(m => m.CreatedDate.Hour == listTimesRange[i] && !string.IsNullOrEmpty(m.CreditNoteNo))
                                   .Select(x => x.ReceiptId).ToList();


                                if (data == null)
                                    data = new HourlySalesReportModels();

                                //if (dataCreditNote == null)
                                //    dataCreditNote = new HourlySalesReportModels();

                                ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                subTotal_ReceiptTotal += data.ReceiptTotal;
                                ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                subTotal_TC += data.NoOfReceipt;
                                ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                subTotal_Pax += data.NoOfPerson;
                                ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                // NetSale = netsale receipts - netsale credit notes
                                //Old
                                //_netSale = (data.NetSales - totalNoIncludeSaleReceipt) - payGC + _roundingValue - (dataCreditNote.NetSales - totalNoIncludeSaleCreditNote);

                                double giftCardSell = lstItemNoIncludeSale.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                  && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                  && ww.CreatedDate.Hour == listTimesRange[i]
                                                  && ww.StoreId == store.Id
                                                  && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                                var taxInclude = lstItemNoIncludeSale.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                             && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                             && ww.StoreId == store.Id
                            && ww.CreatedDate.Hour == listTimesRange[i]).Sum(ss => ss.Tax);

                                var _noincludeSale = lstItemNoIncludeSale.Where(ww => ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                && ww.StoreId == store.Id
                              && ww.CreatedDate.Hour == listTimesRange[i]).Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                                _noincludeSale -= taxInclude;

                                var lstPaymentGCs = _lstPaymentsStore.Where(p => p.StoreId == store.Id
                                 && p.CreatedDate.Hour == listTimesRange[i]
                                       && lstGC.Contains(p.PaymentId)
                                        && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();
                                payGC = 0;
                                _taxOfPayGCNotInclude = 0;
                                _svcOfPayGCNotInclude = 0;
                                double _amount = 0;
                                if (lstPaymentGCs != null && lstPaymentGCs.Any())
                                {
                                    foreach (var item in lstPaymentGCs)
                                    {
                                        _amount = item.Amount;
                                        var receipt = _lstDataStore.Where(ww => ww.ReceiptId == item.OrderId).FirstOrDefault();
                                        var lstGCRefunds = _lstRefund.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCRefunds != null && lstGCRefunds.Any())
                                        {
                                            var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                            _amount -= refundAmount;
                                        }
                                        payGC += _amount;
                                        double tax = 0;
                                        double svc = 0;
                                        if (receipt.GST != 0)
                                            tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                        if (receipt.ServiceCharge != 0)
                                            svc = (_amount - tax) * receipt.ServiceCharge
                                                / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                        _taxOfPayGCNotInclude += tax;
                                        _svcOfPayGCNotInclude += svc;

                                    }
                                }

                                _totalCreditNote = lstItemNoIncludeSale.Where(ww => _lstReceiptId.Contains(ww.ReceiptId)
                                   && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                        .Sum(ss => ss.ItemTotal);

                                _netSale = data.ReceiptTotal - _totalCreditNote - data.ServiceCharge - data.GST - giftCardSell - _noincludeSale
                              - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);


                                ws.Cells[currentRow, 7].Value = _netSale;
                                subTotal_NetSale += _netSale;
                                ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                dTmpFrom = dTmpFrom.AddHours(1);
                                dTmpTo = dTmpTo.AddHours(1);
                                currentRow++;
                            }
                            //}

                            listRange.Add((currentRow - 1).ToString());
                            listChart.Add(store.Name, listRange);
                            #endregion

                            #region Sub Total
                            company_total_ReceiptTotal += subTotal_ReceiptTotal;
                            company_total_TC += subTotal_TC;
                            company_total_Pax += subTotal_Pax;
                            company_total_NetSale += subTotal_NetSale;

                            ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub. Total");
                            // sub total 
                            ws.Cells[currentRow, 2].Value = subTotal_ReceiptTotal;
                            ws.Cells[currentRow, 3].Value = subTotal_TC;
                            ws.Cells[currentRow, 4].Value = subTotal_Pax;
                            ws.Cells[currentRow, 5].Value = subTotal_TC == 0 ? 0 : subTotal_ReceiptTotal / subTotal_TC;
                            ws.Cells[currentRow, 6].Value = subTotal_ReceiptTotal / (subTotal_Pax == 0 ? 1 : subTotal_Pax);
                            ws.Cells[currentRow, 7].Value = subTotal_NetSale;

                            ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[beginCal, 2, currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                            ws.Cells[beginCal, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                            ws.Row(currentRow).Style.Font.Bold = true;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                            lstSubTotalRowIndex.Add(currentRow);
                            #endregion

                            currentRow++;
                        }

                    }//end store loop

                    // Company summary
                    #region Company Total
                    if (countCompany > 1) // Multi company selected
                    {
                        total_ReceiptTotal += company_total_ReceiptTotal;
                        total_TC += company_total_TC;
                        total_Pax += company_total_Pax;
                        total_NetSale += company_total_NetSale;

                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("COMPANY TOTAL");
                    }
                    else
                    {
                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    }
                    // Company Total 
                    ws.Cells[currentRow, 2].Value = company_total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = company_total_TC;
                    ws.Cells[currentRow, 4].Value = company_total_Pax;
                    ws.Cells[currentRow, 5].Value = company_total_TC == 0 ? 0 : company_total_ReceiptTotal / company_total_TC;
                    ws.Cells[currentRow, 6].Value = company_total_ReceiptTotal / (company_total_Pax == 0 ? 1 : company_total_Pax);
                    ws.Cells[currentRow, 7].Value = company_total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                    #endregion
                }
                // Total
                #region Total
                if (countCompany > 1) // Multi company selected
                {
                    ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    // total 
                    ws.Cells[currentRow, 2].Value = total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = total_TC;
                    ws.Cells[currentRow, 4].Value = total_Pax;
                    ws.Cells[currentRow, 5].Value = total_TC == 0 ? 0 : total_ReceiptTotal / total_TC;
                    ws.Cells[currentRow, 6].Value = total_ReceiptTotal / (total_Pax == 0 ? 1 : total_Pax);
                    ws.Cells[currentRow, 7].Value = total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                }
                #endregion
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells.AutoFitColumns();

                if (model.FormatExport.Equals("Excel"))
                {
                    //Draw chart
                    ExcelChart chart = ws.Drawings.AddChart("HourlySales", eChartType.LineMarkersStacked);
                    chart.SetPosition(1, 0, 8, 0);
                    chart.SetSize(950, 600);
                    chart.AdjustPositionAndSize();
                    chart.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chart Title");
                    chart.Title.Font.Size = 14;

                    chart.XAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Period");
                    chart.XAxis.Title.Font.Size = 10;
                    chart.XAxis.Crosses = eCrosses.Max;
                    //chart.XAxis.Title.Font.Color = Color.Gray;

                    chart.YAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales Amount");
                    chart.YAxis.Title.Font.Size = 10;
                    //chart.YAxis.Title.Font.Color = Color.Gray;

                    ////Format Legend for Chart
                    chart.Legend.Remove();

                    int startRow = 0;
                    int endRow = 0;
                    for (int i = 0; i < listChart.Count; i++)
                    {
                        string storename = listChart.ElementAt(i).Key;
                        List<string> listRange = listChart.ElementAt(i).Value;

                        startRow = int.Parse(listRange[0]);
                        endRow = int.Parse(listRange[1]);
                        string xRow = string.Format("A{0}:A{1}", startRow, endRow);
                        string yRow = string.Format("B{0}:B{1}", startRow, endRow);
                        var serie1 = chart.Series.Add(yRow, xRow);
                        serie1.Header = storename;
                        serie1.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;
                    }

                    //Data Table with Legend
                    var chartXml = chart.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("default", nsuri);

                    //Add the table node
                    var plotAreaNode = chartXml.SelectSingleNode("/default:chartSpace/default:chart/default:plotArea", nsm);
                    var dTableNode = chartXml.CreateNode(XmlNodeType.Element, "dTable", nsuri);
                    plotAreaNode.AppendChild(dTableNode);

                    //With Legend Flag
                    var att = chartXml.CreateAttribute("val");
                    att.Value = "1";

                    var showKeysNode = chartXml.CreateNode(XmlNodeType.Element, "showKeys", nsuri);
                    showKeysNode.Attributes.Append(att);
                    dTableNode.AppendChild(showKeysNode);

                    pck.Save();
                }
                return pck;
            }
            catch (Exception e)
            {
                return pck;
            }
        }
        #endregion

        #endregion

        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]
        public List<HourlySalesReportModels> GetData_NewDB(List<string> lstStores, List<string> lstBudDayId, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_PosSale
                               where lstStores.Contains(tb.StoreId)
                                     && lstBudDayId.Contains(tb.BusinessId)
                                     && tb.Mode == mode
                               select new HourlySalesReportModels
                               {
                                   BusinessDayId = tb.BusinessId,
                                   ReceiptId = tb.OrderId,
                                   ReceiptNo = tb.ReceiptNo,
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.ReceiptCreatedDate.Value,
                                   NoOfPerson = tb.NoOfPerson,
                                   ReceiptTotal = tb.ReceiptTotal,
                                   Discount = tb.Discount,
                                   ServiceCharge = tb.ServiceCharge,
                                   GST = tb.GST,
                                   Rounding = tb.Rounding,
                                   //NetSales = tb.NetSales,
                                   PromotionValue = tb.PromotionValue,
                                   CreditNoteNo = tb.CreditNoteNo
                               }).ToList();
                return lstData;
            }
        }

        public ExcelPackage ReportChart_NewDB(RPHourlySalesModels model, List<StoreModels> lstStore, bool isExtend)
        {
            ExcelPackage pck = new ExcelPackage();
            try
            {
                // Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly_Sales"));

                int totalCols = 7;

                // Create header for report
                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromHour.Hours, 0, 0);
                DateTime dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToHour.Hours, 0, 0);
                CreateReportHeaderChart(ws, totalCols, dFrom, dTo, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Sales Report"));

                // Format header report
                ws.Cells[1, 1, 4, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[1, 1, 4, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return pck;
                }

                // Updated inputs from business information
                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                var lstBusDayIdAllStore = _lstBusDayAllStore.Select(ss => ss.Id).ToList();
                model.ListStores = _lstBusDayAllStore.Select(ss => ss.StoreId).Distinct().ToList();

                // Get data sale
                List<HourlySalesReportModels> lstData = GetData_NewDB(model.ListStores, lstBusDayIdAllStore, model.Mode);
                if (lstData == null || !lstData.Any())
                {
                    return pck;
                }
                else
                {
                    model.ListStores = lstData.Select(ss => ss.StoreId).Distinct().ToList(); // Get list store from data sale
                }

                // Get list time range
                int fromTime = model.FromHour.Hours;
                int toTime = model.ToHour.Hours;
                if (toTime == 0) // ToHour == 0 <=> ToHour == 24
                {
                    toTime = 24;
                }
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

                // Get data no include sale
                PosSaleFactory posSaleFactory = new PosSaleFactory();
                var lstReceiptId = lstData.Select(ss => ss.ReceiptId).ToList();
                var lstItemNoIncludeSale = posSaleFactory.GetItemsNoIncludeSale(lstReceiptId, model.ListStores, model.Mode);

                // Get info payment by GC
                List<RFilterChooseExtBaseModel> lstPaymentMethod = new List<RFilterChooseExtBaseModel>();
                // Payment GC
                if (!isExtend)
                {
                    lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                }
                else
                {
                    //Group by hostUrl
                    List<string> lstStoreIdExt = new List<string>();
                    var groupByHostUrl = lstStore.GroupBy(gg => gg.HostUrlExtend);
                    foreach (var item in groupByHostUrl)
                    {
                        lstStoreIdExt = item.Select(ss => ss.Id).ToList();
                        var tmp = _baseFactory.GetAllPaymentForMerchantExtendReport(item.Key, new Models.Api.CategoryApiRequestModel() { ListStoreIds = lstStoreIdExt });
                        lstPaymentMethod.AddRange(tmp);
                    }
                }
                
                var lstGCId = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                if (lstGCId == null)
                    lstGCId = new List<string>();

                List<PaymentModels> lstPaymentGCs = _orderPaymentMethodFactory.GetDataPaymentItemsByGC(model, lstGCId);

                // Data of Refund
                var _lstRefund = _refundFactory.GetListRefundWithoutDetailsByReceiptId(model.ListStores, lstReceiptId);
                if (_lstRefund == null)
                    _lstRefund = new List<RefundReportDTO>();

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Period"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"), System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX").ToLower()), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Avg/Pax"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cells[5, i + 1].Value = lstColNames[i];

                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.Bold = true;
                ws.Row(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Row(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[5, 1, 5, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                #endregion

                int currentRow = 6;
                List<int> lstSubTotalRowIndex = new List<int>();
                Dictionary<int, double> total = new Dictionary<int, double>();
                bool _isTaxInclude = true;
                //===========
                Dictionary<string, List<string>> listChart = new Dictionary<string, List<string>>();

                // For Grand Total
                double total_ReceiptTotal = 0, total_TC = 0, total_Pax = 0, total_NetSale = 0;

                // For Company Total
                double company_total_ReceiptTotal = 0, company_total_TC = 0, company_total_Pax = 0, company_total_NetSale = 0;

                // Group list stores by company
                lstStore = lstStore.Where(w => model.ListStores.Contains(w.Id)).ToList();
                var lstStore_Group = lstStore.GroupBy(g => new { CompanyId = g.CompanyId, CompanyName = g.CompanyName }).OrderBy(o => o.Key.CompanyName).ToList();
                int countCompany = lstStore_Group.Count();

                List<StoreModels> lstStoreOfCompany = new List<StoreModels>();
                var _lstDataStore = new List<HourlySalesReportModels>();
                var _lstPaymentGCsStore = new List<PaymentModels>();
                var _lstPayGCsNoSaleTime = new List<PaymentModels>();
                var data = new HourlySalesReportModels();

                // For netsale
                double payGC = 0, _taxOfPayGCNotInclude = 0, _svcOfPayGCNotInclude = 0, _totalCreditNote = 0, giftCardSell = 0, taxInclude = 0, _noincludeSale = 0, _amount = 0, _netSale = 0;

                double subTotal_ReceiptTotal = 0, subTotal_TC = 0, subTotal_Pax = 0, subTotal_NetSale = 0;

                List<string> _lstReceiptIdCN = new List<string>();
                List<string> _lstOrderIdStore = new List<string>();

                foreach (var company in lstStore_Group)
                {
                    company_total_ReceiptTotal = 0;
                    company_total_TC = 0;
                    company_total_Pax = 0;
                    company_total_NetSale = 0;

                    // Get list store information of company 
                    lstStoreOfCompany = new List<StoreModels>();
                    lstStoreOfCompany = company.OrderBy(o => o.Name).ToList();

                    // Company name
                    #region Company Name
                    ws.Cells[currentRow, 1].Value = company.Key.CompanyName;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                    #endregion

                    foreach (StoreModels store in lstStoreOfCompany)
                    {
                        // Get list data of store
                        _lstDataStore = lstData.Where(ww => ww.StoreId == store.Id).ToList();
                        _lstOrderIdStore = _lstDataStore.Select(ss => ss.ReceiptId).Distinct().ToList();
                        _lstPaymentGCsStore = lstPaymentGCs.Where(ww => ww.StoreId == store.Id && _lstOrderIdStore.Contains(ww.OrderId)).ToList();

                        if (_lstDataStore != null && _lstDataStore.Any())
                        {
                            _isTaxInclude = _baseFactory.IsTaxInclude(store.Id);

                            // Store name
                            #region Store Name
                            ws.Cells[currentRow, 1].Value = store.Name;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Merge = true;
                            ws.Row(currentRow).Height = 20;
                            ws.Row(currentRow).Style.Font.Bold = true;
                            ws.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            ws.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                            #endregion

                            List<string> listRange = new List<string>();
                            currentRow++;
                            listRange.Add(currentRow.ToString());

                            int beginCal = currentRow;
                            Dictionary<int, double> subTotal = new Dictionary<int, double>();

                            #region Data
                            DateTime dGroup = model.FromDate, dTmpFrom = DateTime.Now, dTmpTo = DateTime.Now;
                            subTotal_ReceiptTotal = 0; subTotal_TC = 0; subTotal_Pax = 0; subTotal_NetSale = 0;
                            _netSale = 0;

                            // Get list data of store depend on list time range
                            _lstDataStore = _lstDataStore.Where(ww => listTimesRange.Contains(ww.CreatedDate.Hour)).ToList();
                            if (_lstDataStore == null)
                                _lstDataStore = new List<HourlySalesReportModels>();

                            for (int i = 0; i < listTimesRange.Count; i++)
                            {
                                ws.Cells[currentRow, 1].Value = string.Format("{0}:00 - {1}:00", listTimesRange[i], (listTimesRange[i] + 1));

                                // Set data
                                data = _lstDataStore.Where(m => m.CreatedDate.Hour == listTimesRange[i] && string.IsNullOrEmpty(m.CreditNoteNo))
                                    .GroupBy(gg => gg.CreatedDate.Hour)
                                    .Select(s => new HourlySalesReportModels()
                                    {
                                        ReceiptTotal = s.Sum(x => x.ReceiptTotal),
                                        NoOfReceipt = s.Count(),
                                        NoOfPerson = s.Sum(x => (int?)x.NoOfPerson) == null ? 0 : s.Sum(x => x.NoOfPerson),
                                        PerNoOfReceipt = (s.Sum(x => x.ReceiptTotal) / (s.Count() == 0 ? 1 : s.Count())),
                                        PerNoOfPerson = s.Sum(x => (int?)x.NoOfPerson) == null ? 0 :
                                            s.Sum(x => x.ReceiptTotal) / (s.Sum(x => x.NoOfPerson) == 0 ? 1 : s.Sum(x => x.NoOfPerson)),
                                        ServiceCharge = s.Sum(ss => ss.ServiceCharge),
                                        GST = s.Sum(ss => ss.GST)

                                    }).FirstOrDefault();

                                // Credit Note info for get total Netsale
                                _lstReceiptIdCN = _lstDataStore.Where(m => m.CreatedDate.Hour == listTimesRange[i] && !string.IsNullOrEmpty(m.CreditNoteNo))
                                   .Select(x => x.ReceiptId).ToList();

                                if (data == null)
                                    data = new HourlySalesReportModels();

                                ws.Cells[currentRow, 2].Value = data.ReceiptTotal;
                                subTotal_ReceiptTotal += data.ReceiptTotal;
                                ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 3].Value = data.NoOfReceipt;
                                subTotal_TC += data.NoOfReceipt;
                                ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 4].Value = data.NoOfPerson;
                                subTotal_Pax += data.NoOfPerson;
                                ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 5].Value = data.PerNoOfReceipt;
                                ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[currentRow, 6].Value = data.PerNoOfPerson;
                                ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                #region NetSale
                                giftCardSell = lstItemNoIncludeSale.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                    && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                    && ww.CreatedDate.Hour == listTimesRange[i] && ww.StoreId == store.Id
                                    && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value))
                                    .Sum(ss => ss.TotalAmount.Value);

                                taxInclude = lstItemNoIncludeSale.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                                    && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                    && ww.StoreId == store.Id && ww.CreatedDate.Hour == listTimesRange[i])
                                    .Sum(ss => ss.Tax);

                                _noincludeSale = lstItemNoIncludeSale.Where(ww => ww.IsIncludeSale == false
                                    && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                    && ww.StoreId == store.Id && ww.CreatedDate.Hour == listTimesRange[i])
                                    .Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));

                                _noincludeSale -= taxInclude;

                                _lstPayGCsNoSaleTime = _lstPaymentGCsStore.Where(p => p.CreatedDate.Hour == listTimesRange[i] 
                                    && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();

                                payGC = 0;
                                _taxOfPayGCNotInclude = 0;
                                _svcOfPayGCNotInclude = 0;
                                _amount = 0;
                                if (_lstPayGCsNoSaleTime != null && _lstPayGCsNoSaleTime.Any())
                                {
                                    foreach (var item in _lstPayGCsNoSaleTime)
                                    {
                                        _amount = item.Amount;
                                        var receipt = _lstDataStore.Where(ww => ww.ReceiptId == item.OrderId).FirstOrDefault();
                                        var lstGCRefunds = _lstRefund.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCRefunds != null && lstGCRefunds.Any())
                                        {
                                            var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                            _amount -= refundAmount;
                                        }
                                        payGC += _amount;
                                        double tax = 0;
                                        double svc = 0;
                                        if (receipt.GST != 0)
                                            tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                        if (receipt.ServiceCharge != 0)
                                            svc = (_amount - tax) * receipt.ServiceCharge
                                                / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                        _taxOfPayGCNotInclude += tax;
                                        _svcOfPayGCNotInclude += svc;

                                    }
                                }

                                _totalCreditNote = lstItemNoIncludeSale.Where(ww => _lstReceiptIdCN.Contains(ww.ReceiptId)
                                   && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                        .Sum(ss => ss.ItemTotal);

                                _netSale = data.ReceiptTotal - _totalCreditNote - data.ServiceCharge - data.GST - giftCardSell - _noincludeSale
                              - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);
                                #endregion NetSale

                                ws.Cells[currentRow, 7].Value = _netSale;
                                subTotal_NetSale += _netSale;
                                ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                dTmpFrom = dTmpFrom.AddHours(1);
                                dTmpTo = dTmpTo.AddHours(1);
                                currentRow++;
                            }

                            listRange.Add((currentRow - 1).ToString());
                            listChart.Add(store.Name, listRange);
                            #endregion

                            #region Sub Total
                            company_total_ReceiptTotal += subTotal_ReceiptTotal;
                            company_total_TC += subTotal_TC;
                            company_total_Pax += subTotal_Pax;
                            company_total_NetSale += subTotal_NetSale;

                            ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub. Total");
                            // sub total 
                            ws.Cells[currentRow, 2].Value = subTotal_ReceiptTotal;
                            ws.Cells[currentRow, 3].Value = subTotal_TC;
                            ws.Cells[currentRow, 4].Value = subTotal_Pax;
                            ws.Cells[currentRow, 5].Value = subTotal_TC == 0 ? 0 : subTotal_ReceiptTotal / subTotal_TC;
                            ws.Cells[currentRow, 6].Value = subTotal_ReceiptTotal / (subTotal_Pax == 0 ? 1 : subTotal_Pax);
                            ws.Cells[currentRow, 7].Value = subTotal_NetSale;

                            ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells[beginCal, 2, currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                            ws.Cells[beginCal, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                            ws.Row(currentRow).Style.Font.Bold = true;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                            lstSubTotalRowIndex.Add(currentRow);
                            #endregion

                            currentRow++;
                        }

                    }//end store loop

                    // Company summary
                    #region Company Total
                    if (countCompany > 1) // Multi company selected
                    {
                        total_ReceiptTotal += company_total_ReceiptTotal;
                        total_TC += company_total_TC;
                        total_Pax += company_total_Pax;
                        total_NetSale += company_total_NetSale;

                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("COMPANY TOTAL");
                    }
                    else
                    {
                        ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    }
                    // Company Total 
                    ws.Cells[currentRow, 2].Value = company_total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = company_total_TC;
                    ws.Cells[currentRow, 4].Value = company_total_Pax;
                    ws.Cells[currentRow, 5].Value = company_total_TC == 0 ? 0 : company_total_ReceiptTotal / company_total_TC;
                    ws.Cells[currentRow, 6].Value = company_total_ReceiptTotal / (company_total_Pax == 0 ? 1 : company_total_Pax);
                    ws.Cells[currentRow, 7].Value = company_total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                    #endregion
                }
                // Total
                #region Total
                if (countCompany > 1) // Multi company selected
                {
                    ws.Cells[currentRow, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    // total 
                    ws.Cells[currentRow, 2].Value = total_ReceiptTotal;
                    ws.Cells[currentRow, 3].Value = total_TC;
                    ws.Cells[currentRow, 4].Value = total_Pax;
                    ws.Cells[currentRow, 5].Value = total_TC == 0 ? 0 : total_ReceiptTotal / total_TC;
                    ws.Cells[currentRow, 6].Value = total_ReceiptTotal / (total_Pax == 0 ? 1 : total_Pax);
                    ws.Cells[currentRow, 7].Value = total_NetSale;

                    ws.Cells[currentRow, 2, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[currentRow, 5, currentRow, totalCols].Style.Numberformat.Format = "#,##0.00";
                    ws.Row(currentRow).Style.Font.Bold = true;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1, currentRow, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                    currentRow++;
                }
                #endregion
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 1, currentRow - 1, totalCols].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells.AutoFitColumns();

                if (model.FormatExport.Equals("Excel"))
                {
                    //Draw chart
                    ExcelChart chart = ws.Drawings.AddChart("HourlySales", eChartType.LineMarkersStacked);
                    chart.SetPosition(1, 0, 8, 0);
                    chart.SetSize(950, 600);
                    chart.AdjustPositionAndSize();
                    chart.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chart Title");
                    chart.Title.Font.Size = 14;

                    chart.XAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Period");
                    chart.XAxis.Title.Font.Size = 10;
                    chart.XAxis.Crosses = eCrosses.Max;
                    //chart.XAxis.Title.Font.Color = Color.Gray;

                    chart.YAxis.Title.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales Amount");
                    chart.YAxis.Title.Font.Size = 10;
                    //chart.YAxis.Title.Font.Color = Color.Gray;

                    ////Format Legend for Chart
                    chart.Legend.Remove();

                    int startRow = 0;
                    int endRow = 0;
                    for (int i = 0; i < listChart.Count; i++)
                    {
                        string storename = listChart.ElementAt(i).Key;
                        List<string> listRange = listChart.ElementAt(i).Value;

                        startRow = int.Parse(listRange[0]);
                        endRow = int.Parse(listRange[1]);
                        string xRow = string.Format("A{0}:A{1}", startRow, endRow);
                        string yRow = string.Format("B{0}:B{1}", startRow, endRow);
                        var serie1 = chart.Series.Add(yRow, xRow);
                        serie1.Header = storename;
                        serie1.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;
                    }

                    //Data Table with Legend
                    var chartXml = chart.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("default", nsuri);

                    //Add the table node
                    var plotAreaNode = chartXml.SelectSingleNode("/default:chartSpace/default:chart/default:plotArea", nsm);
                    var dTableNode = chartXml.CreateNode(XmlNodeType.Element, "dTable", nsuri);
                    plotAreaNode.AppendChild(dTableNode);

                    //With Legend Flag
                    var att = chartXml.CreateAttribute("val");
                    att.Value = "1";

                    var showKeysNode = chartXml.CreateNode(XmlNodeType.Element, "showKeys", nsuri);
                    showKeysNode.Attributes.Append(att);
                    dTableNode.AppendChild(showKeysNode);

                    pck.Save();
                }
                return pck;
            }
            catch (Exception e)
            {
                return pck;
            }
        }
        #endregion End report with new DB from table [R_PosSale], [R_PosSaleDetail]

        #region Hourly Sale Chart report (Home page)
        public List<HourlySalesReportModels> GetDataForChart(DateTime dFrom, DateTime dTo, List<string> lstStores, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_HourlySalesReport
                               where lstStores.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= dFrom && tb.CreatedDate <= dTo)
                                     && tb.Mode == mode
                                     && string.IsNullOrEmpty(tb.CreditNoteNo) // Only receipt
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   Time = (int?)SqlFunctions.DatePart("HH", tb.CreatedDate),
                               } into g
                               select new HourlySalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   Time = g.Key.Time.Value,
                                   ReceiptTotal = g.Sum(x => x.ReceiptTotal),
                                   NoOfReceipt = g.Count(),
                                   NoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 : g.Sum(x => x.NoOfPerson),
                                   PerNoOfReceipt = (g.Sum(x => x.ReceiptTotal) / (g.Count() == 0 ? 1 : g.Count())),
                                   PerNoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 :
                                    g.Sum(x => x.ReceiptTotal) / (g.Sum(x => x.NoOfPerson) == 0 ? 1 : g.Sum(x => x.NoOfPerson)),
                                   NetSales = g.Sum(x => x.NetSales),
                               }).ToList();
                return lstData;
            }
        }
        #endregion Hourly Sale Chart report, Home page
    }
}
