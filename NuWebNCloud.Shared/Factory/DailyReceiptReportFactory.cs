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
using ClosedXML.Excel;
using OfficeOpenXml.Style;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Data.Models;

namespace NuWebNCloud.Shared.Factory
{
    public class DailyReceiptReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private TaxFactory _taxFactory = null;
        private RefundFactory _refundFactory = null;
        private NoIncludeOnSaleDataFactory _noIncludeOnSaleDataFactory;
        //update 18/09/2017
        //private DiscountAndMiscReportFactory _DiscountAndMiscReportFactory;
        // Updated 09122017
        private ItemizedSalesAnalysisReportFactory _itemizedSalesAnalysisFactory;
        private DailySalesReportFactory _dailySalesReportFactory;

        public DailyReceiptReportFactory()
        {
            _baseFactory = new BaseFactory();
            _taxFactory = new TaxFactory();
            _refundFactory = new RefundFactory();
            _noIncludeOnSaleDataFactory = new NoIncludeOnSaleDataFactory();
            // Updated 09122017
            _itemizedSalesAnalysisFactory = new ItemizedSalesAnalysisReportFactory();
            _dailySalesReportFactory = new DailySalesReportFactory();
        }

        public bool Insert(List<DailyReceiptReportModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_DailyReceiptReport.Where(ww => ww.BusinessDayId == info.BusinessDayId && ww.StoreId == info.StoreId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Daily Receipt data exist", lstInfo);
                    return result;
                }

                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_DailyReceiptReport> lstInsert = new List<R_DailyReceiptReport>();
                        R_DailyReceiptReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_DailyReceiptReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.BusinessDayId = item.BusinessDayId;
                            itemInsert.ReceiptId = item.ReceiptId;
                            itemInsert.ReceiptNo = item.ReceiptNo;
                            itemInsert.NoOfPerson = item.NoOfPerson;
                            itemInsert.ReceiptTotal = item.ReceiptTotal;
                            itemInsert.Discount = item.Discount;
                            itemInsert.ServiceCharge = item.ServiceCharge;
                            itemInsert.GST = item.GST;
                            itemInsert.Tips = item.Tips;
                            itemInsert.Rounding = item.Rounding;
                            itemInsert.PromotionValue = item.PromotionValue;
                            itemInsert.NetSales = item.NetSales;
                            itemInsert.Mode = item.Mode;
                            itemInsert.CreditNoteNo = item.CreditNoteNo;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_DailyReceiptReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Daily Receipt data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert Daily Receipt data fail", lstInfo);
                        NSLog.Logger.Error("Insert Daily Receipt data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_DailyReceiptReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<DailyReceiptReportModels> GetDataReceiptItems(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DailyReceiptReport
                               where model.ListStores.Contains(tb.StoreId)
                                     && tb.CreatedDate >= model.FromDate
                                             && tb.CreatedDate <= model.ToDate
                                             && tb.Mode == model.Mode
                               select new DailyReceiptReportModels
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
                                   Tips = tb.Tips,
                                   Rounding = tb.Rounding,
                                   NetSales = tb.NetSales,
                                   PromotionValue = tb.PromotionValue
                               }).ToList();
                return lstData;
            }
        }

        public List<DailyReceiptPaymentReportDTO> GetDataPaymentItems(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.G_PaymentMenthod
                               where model.ListStores.Contains(tb.StoreId)
                                     && tb.CreatedDate >= model.FromDate
                                             && tb.CreatedDate <= model.ToDate && tb.Mode == model.Mode
                               select new DailyReceiptPaymentReportDTO
                               {
                                   OrderId = tb.OrderId,
                                   StoreId = tb.StoreId,
                                   PaymentId = tb.PaymentId,
                                   PaymentName = tb.PaymentName,
                                   Amount = tb.Amount,
                                   CreatedDate = tb.CreatedDate
                               }).ToList();
                return lstData;
            }
        }

        public XLWorkbook ExportExcel(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "Daily_Receipt_Report";// _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily_Receipt_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dToFilter = model.ToDate;
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

            List<DailyReceiptReportModels> reportItems = GetDataReceiptItems(model);
            List<DailyReceiptPaymentReportDTO> payByMethods = GetDataPaymentItems(model);
            var _lstRefund = _refundFactory.GetListRefund(model.ListStores, model.FromDate, model.ToDate, model.Mode);
            if (_lstRefund == null)
                _lstRefund = new List<RefundReportDTO>();

            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            //Edit check cash & GC 2017/07/24
            var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
            if (cash == null)
                cash = new RFilterChooseExtBaseModel();
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            //get list noinclude sale
            var _lstNoIncludeOnSale = _noIncludeOnSaleDataFactory.GetListCateNoIncludeSaleForDailyReceipt(model.ListStores, model.FromDate, model.ToDate, model.Mode);
            var _lstNoIncludeOnSaleGroupByCate = new List<NoIncludeOnSaleDataReportModels>();

            ////get list misc
            //var lstMiscs = _itemizedSalesAnalysisFactory.GetListMiscForDailyReceipt(model);

            //string sheetName = "Daily_Receipt_Report";// _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily_Receipt_Report");
            //var wb = new XLWorkbook();
            //var ws = wb.Worksheets.Add(sheetName);
            int maxColumn = 0;
            int row = 4, column = 0, taxType = 0;
            double _refund = 0;
            double payAmountByCate = 0;
            double payAmountByCateForNetSale = 0;
            double payAmountByCateTotal = 0;
            string storeName = string.Empty, storeId = string.Empty;
            //var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            bool _isTaxInclude = true;

            for (int i = 0; i < lstStore.Count; i++)
            {
                _isTaxInclude = true;
                int rowByStore = row;
                taxType = _taxFactory.GetTaxTypeForStore(lstStore[i].Id);
                storeName = lstStore[i].Name;
                storeId = lstStore[i].Id;
                column = 1;
                row++;
                // store name
                ws.Range(string.Format("A{0}:C{0}", row)).Merge().SetValue(storeName);
                #region Columns Names
                row++;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge"));
                if (taxType == (int)Commons.ETax.AddOn) //Temp
                {
                    _isTaxInclude = false;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                }
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                //=============================================================
                //Check no include on sale
                //=============================================================
                _lstNoIncludeOnSaleGroupByCate = new List<NoIncludeOnSaleDataReportModels>();
                if (_lstNoIncludeOnSale != null && _lstNoIncludeOnSale.Any())
                {
                    var lstNoIncludeOnSaleGroupByCate = _lstNoIncludeOnSale.GroupBy(gg => new { CateId = gg.CategoryId, CateName = gg.CategoryName }).OrderBy(x => x.Key.CateName).ToList();
                    ws.Range(row, column, row, (column + lstNoIncludeOnSaleGroupByCate.Count - 1)).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NAV"));
                    for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                    {
                        _lstNoIncludeOnSaleGroupByCate.Add(new NoIncludeOnSaleDataReportModels()
                        {
                            CategoryId = lstNoIncludeOnSaleGroupByCate[y].Key.CateId,
                            CategoryName = lstNoIncludeOnSaleGroupByCate[y].Key.CateName,

                        });
                        ws.Cell(row + 1, column).SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.CateName);
                        column++;
                    }
                }
                ///End Check no include on sale
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                int startPayColumn = column;

                List<string> lstNotInPaymentCash = new List<string>() { "Cash", };
                List<string> lstNotInPaymentCashNew = new List<string>() { cash != null ? cash.Id : "" };

                List<string> storeCards = new List<string>();

                var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && ww.Name.ToLower() != "cash"
                   && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();

                for (int p = 0; p < lstPaymentParents.Count; p++)
                {
                    lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                    && ww.StoreId.Equals(lstStore[i].Id)).OrderBy(ww => ww.Name).ToList();
                    if (lstPaymentParents[p].ListChilds != null && lstPaymentParents[p].ListChilds.Count > 0)
                    {
                        ws.Range(row, column, row, (column + lstPaymentParents[p].ListChilds.Count - 1)).Merge().SetValue(lstPaymentParents[p].Name);
                        for (int c = 0; c < lstPaymentParents[p].ListChilds.Count; c++)
                        {
                            ws.Cell(row + 1, column).SetValue(lstPaymentParents[p].ListChilds[c].Name);
                            storeCards.Add(lstPaymentParents[p].ListChilds[c].Id);
                            column++;
                        }
                    }
                    else
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(lstPaymentParents[p].Name);
                        storeCards.Add(lstPaymentParents[p].Id);
                    }
                }

                #region Old
                //var payments = (from p in payByMethods
                //                where !lstNotInPaymentCash.Contains(p.PaymentName)
                //                group p by new { ID = p.PaymentId, Name = p.PaymentName } into _p
                //                select new DailyReceiptPaymentReportDTO
                //                {
                //                    PaymentId = _p.Key.ID,
                //                    PaymentName = _p.Key.Name
                //                }).ToList();


                //List<DailyReceiptPaymentReportDTO> cards = payments;
                //if (cards.Count > 0)
                //{
                //    ws.Range(row, column, row, column + cards.Count - 1).Merge().SetValue("External");
                //    ws.Range(row, column, row, column + cards.Count - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //    ws.Range(row, column, row, column + cards.Count - 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                //    for (int k = 0; k < cards.Count; k++)
                //    {
                //        column += (k == 0 ? 0 : 1);
                //        ws.Column(GetColNameFromIndex(column)).Width = 13;
                //        ws.Cell(row + 1, column).SetValue(cards[k].PaymentName);
                //        storeCards.Add(cards[k].PaymentId);
                //    }
                //}
                //else
                //{
                //    storeCards.Add("");
                //    ws.Range(row, column, row + 1, column).Merge().SetValue("External");
                //    ws.Range(row, column, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //    ws.Range(row, column, row + 1, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                //}
                #endregion End-Old

                //column++;

                int endPayColumn = column - 1;
                // end group payments
                //ws.Column(GetColNameFromIndex(column)).Width = 15;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                //ws.Column(GetColNameFromIndex(column)).Width = 15;
                ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                #endregion
                // set value for last column
                if (maxColumn < column)
                    maxColumn = column;
                #region Format Store Header
                ws.Range(row - 1, 1, row + 1, column).Style.Font.SetBold(true);
                ws.Range(row - 1, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                #endregion
                var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == lstStore[i].Id).ToList();
                if (lstBusDay != null && lstBusDay.Count > 0)
                {
                    lstBusDay = lstBusDay.OrderBy(oo => oo.DateFrom).ToList();
                    for (int b = 0; b < lstBusDay.Count; b++)
                    {

                        //var dFrom = lstBusDay.Min(ss => ss.DateFrom);
                        //var dTo = lstBusDay.Max(ss => ss.DateTo);
                        var dFrom = lstBusDay[b].DateFrom;
                        var dTo = lstBusDay[b].DateTo;

                        List<DailyReceiptReportModels> lstDataByStore = reportItems.Where(ww => ww.StoreId == lstStore[i].Id
                        && ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo).ToList();

                        if (lstDataByStore != null && lstDataByStore.Count > 0)
                        {
                            lstDataByStore = lstDataByStore.OrderBy(oo => oo.ReceiptNo).ToList();
                            row += 2;
                            int startRow = row;
                            // Fill data
                            double payTotal = 0, payAmount = 0;
                            double totalNetSales = 0;
                            Dictionary<int, double> total = new Dictionary<int, double>();
                            //List<DailyReceiptReportModels> lstItemsBusinessDayByStore = new List<DailyReceiptReportModels>();
                            for (int j = 0; j < lstDataByStore.Count; j++)
                            {
                                #region Export
                                column = 1;
                                payTotal = 0;
                                payAmount = 0;
                                //ws.Cell(row, column++).Value = "'" + lstDataByStore[j].CreatedDate.ToString("dd/MM/yyyy");
                                ws.Cell(row, column++).Value = lstDataByStore[j].CreatedDate;
                                ws.Cell(row, 1).Style.DateFormat.Format = "MM/dd/yyyy";

                                if (!total.ContainsKey(column))
                                    total.Add(column, 0);
                                ws.Cell(row, column++).Value = lstDataByStore[j].ReceiptNo;
                                if (!total.ContainsKey(column))
                                    total.Add(column, 0);
                                //PAX
                                ws.Cell(row, column++).Value = lstDataByStore[j].NoOfPerson;
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].NoOfPerson);
                                else
                                    total[column] += lstDataByStore[j].NoOfPerson;

                                //ReceiptTotal - Refund
                                ws.Cell(row, column++).Value = lstDataByStore[j].ReceiptTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].ReceiptTotal);
                                else
                                    total[column] += lstDataByStore[j].ReceiptTotal;

                                //Refund
                                _refund = _lstRefund.Where(ww => ww.OrderId == lstDataByStore[j].ReceiptId && ww.CreatedDate.Date == lstDataByStore[j].CreatedDate.Date).Sum(ss => ss.TotalRefund);
                                ws.Cell(row, column++).Value = _refund.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, _refund);
                                else
                                    total[column] += _refund;

                                //Discount
                                ws.Cell(row, column++).Value = lstDataByStore[j].Discount.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].Discount);
                                else
                                    total[column] += lstDataByStore[j].Discount;

                                //Promotion
                                ws.Cell(row, column++).Value = lstDataByStore[j].PromotionValue.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].PromotionValue);
                                else
                                    total[column] += lstDataByStore[j].PromotionValue;

                                //ServiceCharge
                                ws.Cell(row, column++).Value = lstDataByStore[j].ServiceCharge.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].ServiceCharge);
                                else
                                    total[column] += lstDataByStore[j].ServiceCharge;
                                //Tax
                                ws.Cell(row, column++).Value = lstDataByStore[j].GST.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].GST);
                                else
                                    total[column] += lstDataByStore[j].GST;

                                //Tips
                                ws.Cell(row, column++).Value = lstDataByStore[j].Tips.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].Tips);
                                else
                                    total[column] += lstDataByStore[j].Tips;

                                //=======================================================
                                //NAV - NoIncludeOnSale (- discount - promotion)
                                payAmountByCateTotal = 0;
                                for (int a = 0; a < _lstNoIncludeOnSaleGroupByCate.Count; a++)
                                {

                                    //payAmountByCate = _lstNoIncludeOnSale.Where(p => p.StoreId == lstStore[i].Id
                                    //&& p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                    //                            && p.CategoryId == _lstNoIncludeOnSaleGroupByCate[a].CategoryId
                                    //                            && p.OrderId == lstDataByStore[j].ReceiptId)
                                    //                            .Sum(p => p.Amount);
                                    payAmountByCate = _lstNoIncludeOnSale.Where(p => p.StoreId == lstStore[i].Id
                                   && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                               && p.CategoryId == _lstNoIncludeOnSaleGroupByCate[a].CategoryId
                                                               && p.OrderId == lstDataByStore[j].ReceiptId)
                                                               .Sum(p => (double)((decimal)p.Amount));

                                    if (_isTaxInclude)
                                    {
                                        payAmountByCateForNetSale = _lstNoIncludeOnSale.Where(p => p.StoreId == lstStore[i].Id
                       && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                   && p.CategoryId == _lstNoIncludeOnSaleGroupByCate[a].CategoryId
                                                    && p.OrderId == lstDataByStore[j].ReceiptId)
                                                   .Sum(p => (double)((decimal)p.Amount - (decimal)p.DiscountAmount - (decimal)p.PromotionAmount - (decimal)p.Tax));
                                    }
                                    else
                                    {
                                        payAmountByCateForNetSale = _lstNoIncludeOnSale.Where(p => p.StoreId == lstStore[i].Id
                             && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                         && p.CategoryId == _lstNoIncludeOnSaleGroupByCate[a].CategoryId
                                                          && p.OrderId == lstDataByStore[j].ReceiptId)
                                                         .Sum(p => (double)((decimal)p.Amount - (decimal)p.DiscountAmount - (decimal)p.PromotionAmount));
                                    }
                                    //payAmount += tip;
                                    ws.Cell(row, column++).Value = payAmountByCate.ToString("F");
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payAmountByCate);
                                    }
                                    else
                                    {
                                        total[column] += payAmountByCate;
                                    }
                                    //grand total
                                    //if (!grandTotal.ContainsKey(column))
                                    //{
                                    //    grandTotal.Add(column, payAmountByCate);
                                    //}
                                    //else
                                    //{
                                    //    grandTotal[column] += payAmountByCate;
                                    //}


                                    //payAmountByCateTotal += payAmountByCate;
                                    payAmountByCateTotal += payAmountByCateForNetSale;
                                }
                                //NAV - End NoIncludeOnSale
                                //=======================================================
                                var payGC = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                        && lstGC.Contains(p.PaymentId)).Sum(p => p.Amount);

                                //NetSales -NAV - GC
                                ws.Cell(row, column++).Value = (lstDataByStore[j].NetSales - payAmountByCateTotal - payGC).ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (lstDataByStore[j].NetSales - payAmountByCateTotal - payGC));
                                else
                                    total[column] += (lstDataByStore[j].NetSales - payAmountByCateTotal - payGC);


                                totalNetSales += (lstDataByStore[j].NetSales - payAmountByCateTotal - payGC);

                                //Rounding
                                ws.Cell(row, column++).Value = lstDataByStore[j].Rounding.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].Rounding);
                                else
                                    total[column] += lstDataByStore[j].Rounding;

                                // pay by cash (subtract refund)
                                //double payByCash = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                //                            && p.PaymentName == lstNotInPaymentCash[0]).Sum(p => p.Amount) - _refund;//TotalPayment

                                double payByCash = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                           && (p.PaymentName == lstNotInPaymentCash[0] || p.PaymentId == lstNotInPaymentCashNew[0])
                                                           ).Sum(p => p.Amount) - _refund;//TotalPayment


                                ws.Cell(row, column++).Value = payByCash.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, payByCash);
                                else
                                    total[column] += payByCash;
                                payTotal += payByCash;

                                //payByGiftcard
                                //double payByGiftcard = payByMethods.Where(p => p.StoreId == storeId && p.ReceiptNo == itemsByStore[j].ReceiptNo
                                //                            && p.PaymentName == lstNotInPayMent[1]).Select(p => p.ReceiptTotal).FirstOrDefault();//TotalPayment
                                //ws.Cell(row, column++).Value = payByGiftcard.ToString("F");
                                //if (!total.ContainsKey(column))
                                //    total.Add(column, payByGiftcard);
                                //else
                                //    total[column] += payByGiftcard;
                                //payTotal = payByCash + payByGiftcard;

                                //payment other methods
                                for (int k = 0; k < storeCards.Count; k++)
                                {
                                    if (storeCards[k] != "")
                                        payAmount = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                        && p.PaymentId == storeCards[k]).Sum(p => p.Amount); //TotalPayment
                                    else
                                        payAmount = 0;
                                    ws.Cell(row, column++).Value = payAmount.ToString("F");
                                    if (!total.ContainsKey(column))
                                        total.Add(column, payAmount);
                                    else
                                        total[column] += payAmount;
                                    payTotal += payAmount;
                                }

                                //payTotal
                                ws.Cell(row, column++).Value = payTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, payTotal);
                                else
                                    total[column] += payTotal;

                                //excess
                                //double excess = (payTotal - lstDataByStore[j].ReceiptTotal - lstDataByStore[j].Tips);
                                double excess = (payTotal - lstDataByStore[j].ReceiptTotal);
                                //ws.Cell(row, column).Value = (excess > 0) ? excess.ToString("F") : "";
                                ws.Cell(row, column).Value = excess.ToString("F");
                                if (!total.ContainsKey(column + 1))
                                    total.Add(column + 1, excess);
                                else
                                    total[column + 1] += (excess > 0) ? excess : 0;
                                row++;


                                #endregion
                            }
                            // Total Row
                            ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                            for (int j = 2; j <= column; j++)
                            {
                                if (total.ContainsKey(j + 1))
                                {
                                    ws.Cell(row, j).Value = total[j + 1];
                                }

                            }
                            row++;
                            int countReceipt = lstDataByStore.Count();
                            ws.Cell(row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC") + " = " + countReceipt;
                            if (countReceipt == 0 || totalNetSales == 0)
                                ws.Range(row, 2, row, maxColumn).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA") + " = 0");
                            else
                                ws.Range(row, 2, row, maxColumn).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA") + " = " + (totalNetSales / countReceipt).ToString("F"));

                            ws.Range((row - 1), 1, row, column).Style.Font.SetBold(true);
                            ws.Range(row - 1, 1, row, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                            ws.Range(startRow, 4, row, column).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range(startRow, 1, row, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(startRow, 1, row, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            ws.Range("A" + (row + 1) + ":T" + (row + 1) + "").Merge().SetValue("");
                            row++;
                        }//End for data
                    }
                }//End business day
                ws.Range(rowByStore, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(rowByStore, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            }//End store loop

            #region Table Header
            CreateReportHeaderNew(ws, maxColumn, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
            ws.Range(1, 1, 4, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #endregion
            //set Width for Colum 
            List<int> lstWidCol = new List<int>() { 13, 16, 5, 15, 10, 12, 15, 13, 12, 12, 20, 15, 15 };
            for (int i = 0; i < maxColumn; i++)
            {
                if (i <= 12)
                    ws.Column(i + 1).Width = lstWidCol[i];
                else
                    ws.Column(i + 1).Width = 14;
            }
            return wb;
        }


        public static string GetColNameFromIndex(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        // (A = 1, B = 2...AA = 27...AAA = 703...)
        public static int GetColNumberFromName(string columnName)
        {
            char[] characters = columnName.ToUpperInvariant().ToCharArray();
            int sum = 0;
            for (int i = 0; i < characters.Length; i++)
            {
                sum *= 26;
                sum += (characters[i] - 'A' + 1);
            }
            return sum;
        }

        public XLWorkbook ExportExcel_New(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "Daily_Receipt_Report";// _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily_Receipt_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dToFilter = model.ToDate;
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
            List<DailyReceiptReportDataModels> reportItems = new List<DailyReceiptReportDataModels>();
            List<PaymentDataModels> payByMethods = new List<PaymentDataModels>();
            var _lstRefund = new List<RefundDataReportDTO>();
            List<ItemizedSalesAnalysisReportDataModels> lstItemNoIncludeSale = new List<ItemizedSalesAnalysisReportDataModels>();
            var lstMiscs = new List<ItemizedSalesAnalysisReportDataModels>();
            using (var db = new NuWebContext())
            {
                var request = new BaseReportDataModel() { ListStores = model.ListStores, FromDate = model.FromDate, ToDate = model.ToDate, Mode = model.Mode };
                reportItems = db.GetDataForDailyReceiptReport(request);
                payByMethods = db.GetDataPaymentItems(request);
                _lstRefund = db.GetListRefundWithoutDetailsByReceiptId(request);
                lstItemNoIncludeSale = db.GetItemsNoIncludeSale_New(request);
                lstMiscs = db.GetListMisc(request);
            }
            #region Old
            // Data of Receipt
            //List<DailyReceiptReportModels> reportItems = GetDataReceiptItems(model);

            // Data of Payment
            /*List<DailyReceiptPaymentReportDTO> payByMethods = GetDataPaymentItems(model);*/

            // Data of Refund
            //var _lstRefund = _refundFactory.GetListRefund(model.ListStores, model.FromDate, model.ToDate, model.Mode);
            //if (_lstRefund == null)
            //    _lstRefund = new List<RefundReportDTO>();
            #endregion 

            // Data of Payment information
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            //Edit check cash & GC 2017/07/24
            var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
            if (cash == null)
                cash = new RFilterChooseExtBaseModel();
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            // List no include sale
            //List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSale = _itemizedSalesAnalysisFactory.GetItemsForDailyReceiptReports(model);
            var lstItemNoIncludeSaleForCate = lstItemNoIncludeSale.Where(w => !string.IsNullOrEmpty(w.GLAccountCode)).ToList();
            var lstItemNoIncludeSaleForNetSales = lstItemNoIncludeSale.Where(w => w.IsIncludeSale == false).ToList();
            var lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();

            //get list misc
            // var lstMiscs = _itemizedSalesAnalysisFactory.GetListMiscForDailyReceipt(model);

            int maxColumn = 0;
            int maxColumnStore1 = 0;
            int row = 4, column = 0, taxType = 0;
            double _refund = 0;
            double payAmountByCate = 0;
            double payAmountByCateForNetSale = 0;
            double payAmountByCateTotal = 0;
            string storeName = string.Empty, storeId = string.Empty;
            bool _isTaxInclude = true;
            //for netsale
            double _netSale = 0;
            double payGC = 0;
            double _taxOfPayGCNotInclude = 0;
            double _svcOfPayGCNotInclude = 0;
            double totalCreditNote = 0;

            for (int i = 0; i < lstStore.Count; i++)
            {
                _isTaxInclude = true;
                int rowByStore = row;
                taxType = _taxFactory.GetTaxTypeForStore(lstStore[i].Id);
                storeName = lstStore[i].Name;
                storeId = lstStore[i].Id;
                column = 1;
                maxColumn = 0;
                row++;
                // store name
                ws.Range(string.Format("A{0}:C{0}", row)).Merge().SetValue(storeName);
                #region Columns Names
                row++;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge"));
                if (taxType == (int)Commons.ETax.AddOn) //Temp
                {
                    _isTaxInclude = false;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                }
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                //=============================================================
                //Check no include on sale
                //=============================================================
                lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
                if (lstItemNoIncludeSaleForCate != null && lstItemNoIncludeSaleForCate.Any())
                {
                    var lstNoIncludeOnSaleGroupByCate = lstItemNoIncludeSaleForCate.GroupBy(gg => gg.GLAccountCode).OrderBy(x => x.Key).ToList();
                    for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                    {
                        lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                        {
                            GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                        });
                        ws.Range(row, column, row + 1, column).Style.Alignment.SetWrapText(true);

                        if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                        {
                            ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                        }
                        else
                            ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                    }
                }
                ///End Check no include on sale
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                int startPayColumn = column;

                List<string> lstNotInPaymentCash = new List<string>() { "Cash", };
                List<string> lstNotInPaymentCashNew = new List<string>() { cash != null ? cash.Id : "" };

                List<string> storeCards = new List<string>();

                var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && ww.Name.ToLower() != "cash"
                   && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();

                for (int p = 0; p < lstPaymentParents.Count; p++)
                {
                    lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                    && ww.StoreId.Equals(lstStore[i].Id)).OrderBy(ww => ww.Name).ToList();
                    if (lstPaymentParents[p].ListChilds != null && lstPaymentParents[p].ListChilds.Count > 0)
                    {
                        ws.Range(row, column, row, (column + lstPaymentParents[p].ListChilds.Count - 1)).Merge().SetValue(lstPaymentParents[p].Name);
                        for (int c = 0; c < lstPaymentParents[p].ListChilds.Count; c++)
                        {
                            ws.Cell(row + 1, column).SetValue(lstPaymentParents[p].ListChilds[c].Name);
                            ws.Cell(row + 1, column).Style.Alignment.SetWrapText(true);
                            storeCards.Add(lstPaymentParents[p].ListChilds[c].Id);
                            column++;
                        }
                    }
                    else
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(lstPaymentParents[p].Name);
                        storeCards.Add(lstPaymentParents[p].Id);
                    }
                }

                int endPayColumn = column - 1;
                // end group payments
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                #endregion
                // set value for last column
                if (maxColumn < column)
                    maxColumn = column;

                // Get value for set format report header depend on max column of store 1
                if (i == 0)
                {
                    maxColumnStore1 = maxColumn;
                }

                #region Format Store Header
                ws.Range(row - 1, 1, row + 1, column).Style.Font.SetBold(true);
                ws.Range(row - 1, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                #endregion

                double _roudingTotal = 0;
                //var lstTmp = new List<double>();
                //var lstTmpRound = new List<double>();
                var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == lstStore[i].Id).ToList();
                if (lstBusDay != null && lstBusDay.Count > 0)
                {
                    lstBusDay = lstBusDay.OrderBy(oo => oo.DateFrom).ToList();
                    for (int b = 0; b < lstBusDay.Count; b++)
                    {
                        var dFrom = lstBusDay[b].DateFrom;
                        var dTo = lstBusDay[b].DateTo;

                        List<DailyReceiptReportDataModels> lstDataByStore = reportItems.Where(ww => ww.StoreId == lstStore[i].Id
                        && ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo).ToList();

                        if (lstDataByStore != null && lstDataByStore.Count > 0)
                        {
                            lstDataByStore = lstDataByStore.OrderBy(oo => oo.ReceiptNo).ToList();
                            row += 2;
                            int startRow = row;
                            // Fill data
                            double payTotal = 0, payAmount = 0;
                            double totalNetSales = 0;
                            Dictionary<int, decimal> total = new Dictionary<int, decimal>();
                            for (int j = 0; j < lstDataByStore.Count; j++)
                            {
                                #region Export
                                column = 1;
                                payTotal = 0;
                                payAmount = 0;

                                ws.Cell(row, column++).Value = lstDataByStore[j].CreatedDate;
                                ws.Cell(row, 1).Style.DateFormat.Format = "MM/dd/yyyy";

                                if (!total.ContainsKey(column))
                                    total.Add(column, 0);
                                ws.Cell(row, column++).Value = lstDataByStore[j].ReceiptNo;
                                if (!total.ContainsKey(column))
                                    total.Add(column, 0);

                                //PAX
                                ws.Cell(row, column++).Value = lstDataByStore[j].NoOfPerson;
                                if (!total.ContainsKey(column))
                                    total.Add(column, lstDataByStore[j].NoOfPerson);
                                else
                                    total[column] += lstDataByStore[j].NoOfPerson;

                                //ReceiptTotal - Refund
                                ws.Cell(row, column++).Value = lstDataByStore[j].ReceiptTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].ReceiptTotal);
                                else
                                    total[column] += (decimal)lstDataByStore[j].ReceiptTotal;

                                //Refund
                                _refund = _lstRefund.Where(ww => ww.OrderId == lstDataByStore[j].ReceiptId && ww.CreatedDate.Date == lstDataByStore[j].CreatedDate.Date).Sum(ss => ss.TotalRefund);
                                ws.Cell(row, column++).Value = _refund.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)_refund);
                                else
                                    total[column] += (decimal)_refund;

                                //Discount
                                ws.Cell(row, column++).Value = lstDataByStore[j].Discount.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].Discount);
                                else
                                    total[column] += (decimal)lstDataByStore[j].Discount;

                                //Promotion
                                ws.Cell(row, column++).Value = lstDataByStore[j].PromotionValue.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].PromotionValue);
                                else
                                    total[column] += (decimal)lstDataByStore[j].PromotionValue;

                                //ServiceCharge
                                ws.Cell(row, column++).Value = lstDataByStore[j].ServiceCharge.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].ServiceCharge);
                                else
                                    total[column] += (decimal)lstDataByStore[j].ServiceCharge;

                                //Tax
                                ws.Cell(row, column++).Value = lstDataByStore[j].GST.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].GST);
                                else
                                    total[column] += (decimal)lstDataByStore[j].GST;

                                //Tips
                                ws.Cell(row, column++).Value = lstDataByStore[j].Tips.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].Tips);
                                else
                                    total[column] += (decimal)lstDataByStore[j].Tips;

                                _roudingTotal = lstDataByStore[j].Rounding;
                                //=======================================================
                                //NAV - NoIncludeOnSale (- discount - promotion)
                                // GL account code and group by it
                                //Note: value is not include tax, right? It means
                                //Tax is include on item price -> value = [[(price of items x qty ) - (disc or promo )] +SVC ] -Tax
                                //Tax is add on ->value = [(price of items x qty ) - (disc or promo )] +SVC
                                payAmountByCateTotal = 0;
                                double _tax = 0;
                                for (int a = 0; a < lstItemNoIncludeSaleGroupByCate.Count; a++)
                                {

                                    //if (_isTaxInclude)
                                    //{
                                    //    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    //&& p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                    //                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                    //                            .Sum(p => ((decimal)p.TotalAmount - (decimal)p.TotalDiscount - (decimal)p.PromotionAmount
                                    //                            - (decimal)p.Tax));
                                    //}
                                    //else
                                    //{
                                    //    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    //    && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                    //                                && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                    //                                .Sum(p => ((decimal)p.TotalAmount - (decimal)p.TotalDiscount - (decimal)p.PromotionAmount));
                                    //}


                                    _tax = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                        && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                                    && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.ReceiptId == lstDataByStore[j].ReceiptId
                                                            && p.TaxType == (int)Commons.ETax.Inclusive)
                                                            .Sum(p => p.Tax);


                                    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                        && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                                    && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                                           .Sum(p => ((decimal)p.TotalAmount
                                                               - (decimal)p.TotalDiscount
                                                               - (decimal)p.PromotionAmount));
                                    payAmountByCate -= _tax;


                                    if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                    {
                                        payAmountByCate += lstMiscs.Where(ww => ww.StoreId == lstStore[i].Id && ww.ReceiptId == lstDataByStore[j].ReceiptId
                                        && ww.CreatedDate >= lstBusDay[b].DateFrom && ww.CreatedDate <= lstBusDay[b].DateTo).Sum(ss => ss.TotalPrice);

                                        payAmountByCate += _roudingTotal;

                                    }

                                    ws.Cell(row, column++).Value = ((decimal)payAmountByCate).ToString("F");
                                    //lstTmp.Add(payAmountByCate);
                                    //lstTmpRound.Add(Math.Round(payAmountByCate, 2));
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, (decimal)payAmountByCate);
                                    }
                                    else
                                    {
                                        total[column] += (decimal)CommonHelper.RoundUp2Decimal(payAmountByCate);
                                    }
                                }

                                //if (!_isTaxInclude)
                                //{
                                //    payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == lstStore[i].Id
                                //   && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                //                               .Sum(p => (decimal)p.TotalAmount
                                //                               - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount);
                                //}
                                //else
                                //    payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == lstStore[i].Id
                                //        && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                //                                    .Sum(p => (decimal)p.TotalAmount
                                //                                    - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount
                                //                                    - (decimal)p.Tax);

                                //payAmountByCateTotal += payAmountByCateForNetSale;

                                //NAV - End NoIncludeOnSale
                                //=======================================================
                                //var payGC = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                //                        && lstGC.Contains(p.PaymentId)).Sum(p => p.Amount);

                                //var payGC = payByMethods.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= lstBusDay[b].DateFrom
                                //                                && p.CreatedDate <= lstBusDay[b].DateTo
                                //                             && lstGC.Contains(p.PaymentId) && p.OrderId == lstDataByStore[j].ReceiptId).Sum(p => p.Amount);

                                //NetSales -NAV - GC
                                //ws.Cell(row, column++).Value = (lstDataByStore[j].NetSales - payAmountByCateTotal - payGC + _roudingTotal).ToString("F");
                                //new -2018-08-08
                                //  _netSale = totalReceipt - totalCreditNote - totalSVC - totalTax - giftCardSell - _noincludeSale
                                //-(payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);
                                #region netsale
                                totalCreditNote = 0;
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                    totalCreditNote = lstItemNoIncludeSale.Where(ww => lstDataByStore[j].ReceiptId == ww.ReceiptId
                                     && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                       .Sum(ss => ss.ItemTotal);

                                double giftCardSell = lstItemNoIncludeSale.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                   && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                   && lstDataByStore[j].ReceiptId == ww.ReceiptId
                                                   && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                                var taxInclude = lstItemNoIncludeSale.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                && lstDataByStore[j].ReceiptId == ww.ReceiptId
                                ).Sum(ss => ss.Tax);

                                var _noincludeSale = lstItemNoIncludeSale.Where(ww => ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                && lstDataByStore[j].ReceiptId == ww.ReceiptId
                                ).Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                                _noincludeSale -= taxInclude;

                                var lstPaymentsInStore = payByMethods.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= lstBusDay[b].DateFrom
                                                                && p.CreatedDate <= lstBusDay[b].DateTo
                                                             && lstGC.Contains(p.PaymentId) && p.OrderId == lstDataByStore[j].ReceiptId
                                                            && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();

                                //var lstPaymentsInStore = lstPayments.Where(p => p.StoreId == _currentStore.Id
                                //      && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                //      && lstGC.Contains(p.PaymentId)
                                //       && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();
                                payGC = 0;
                                _taxOfPayGCNotInclude = 0;
                                _svcOfPayGCNotInclude = 0;
                                if (lstPaymentsInStore != null && lstPaymentsInStore.Any())
                                {
                                    double refundAmount = 0;
                                    double _amount = 0;
                                    foreach (var item in lstPaymentsInStore)
                                    {
                                        _amount = item.Amount;
                                        refundAmount = 0;
                                        var lstGCRefunds = _lstRefund.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCRefunds != null && lstGCRefunds.Any())
                                        {
                                            refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                            _amount -= refundAmount;
                                        }
                                        payGC += _amount;

                                        var receipt = lstDataByStore[j];
                                        if (receipt != null)
                                        {
                                            double tax = 0;
                                            double svc = 0;
                                            if (receipt.GST != 0)
                                                tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                            if (receipt.ServiceCharge != 0)
                                                svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                            _taxOfPayGCNotInclude += tax;
                                            _svcOfPayGCNotInclude += svc;
                                        }
                                    }
                                }


                                _netSale = (string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo) ? lstDataByStore[j].ReceiptTotal : 0) - totalCreditNote
                                    - (string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo) ? lstDataByStore[j].ServiceCharge : 0)
                                    - (string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo) ? lstDataByStore[j].GST : 0)
                                    - giftCardSell - _noincludeSale
                                    - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);

                                #endregion end netsale
                                ws.Cell(row, column++).Value = _netSale.ToString("F");

                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)_netSale);
                                else
                                    total[column] += (decimal)_netSale;


                                //totalNetSales += (lstDataByStore[j].NetSales - payAmountByCateTotal - payGC + _roudingTotal);
                              
                                totalNetSales += _netSale;

                                //Rounding
                                //_roudingTotal = lstDataByStore[j].Rounding;
                                ws.Cell(row, column++).Value = lstDataByStore[j].Rounding.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].Rounding);
                                else
                                    total[column] += (decimal)lstDataByStore[j].Rounding;

                                // pay by cash (subtract refund)
                                double payByCash = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                           && (p.PaymentName == lstNotInPaymentCash[0] || p.PaymentId == lstNotInPaymentCashNew[0])
                                                           ).Sum(p => p.Amount) - _refund;//TotalPayment


                                ws.Cell(row, column++).Value = payByCash.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)payByCash);
                                else
                                    total[column] += (decimal)payByCash;
                                payTotal += payByCash;

                                //payment other methods
                                for (int k = 0; k < storeCards.Count; k++)
                                {
                                    if (storeCards[k] != "")
                                        payAmount = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                        && p.PaymentId == storeCards[k]).Sum(p => p.Amount); //TotalPayment
                                    else
                                        payAmount = 0;
                                    ws.Cell(row, column++).Value = payAmount.ToString("F");
                                    if (!total.ContainsKey(column))
                                        total.Add(column, (decimal)payAmount);
                                    else
                                        total[column] += (decimal)payAmount;
                                    payTotal += payAmount;
                                }

                                //payTotal
                                ws.Cell(row, column++).Value = payTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)payTotal);
                                else
                                    total[column] += (decimal)payTotal;

                                //excess
                                //double excess = (payTotal - lstDataByStore[j].ReceiptTotal - lstDataByStore[j].Tips);
                                //ws.Cell(row, column).Value = (excess > 0) ? excess.ToString("F") : "";
                                double excess = (payTotal - lstDataByStore[j].ReceiptTotal);
                                ws.Cell(row, column).Value = excess.ToString("F");
                                if (!total.ContainsKey(column + 1))
                                    total.Add(column + 1, (decimal)excess);
                                else
                                    total[column + 1] += (excess > 0) ? (decimal)excess : 0;
                                row++;

                                #endregion
                            }
                            // Total Row
                            ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                            for (int j = 2; j <= column; j++)
                            {
                                if (total.ContainsKey(j + 1))
                                {
                                    ws.Cell(row, j).Value = total[j + 1];
                                }

                            }
                            row++;
                            int countReceipt = lstDataByStore.Count();
                            ws.Cell(row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC") + " = " + countReceipt;
                            if (countReceipt == 0 || totalNetSales == 0)
                                ws.Range(row, 2, row, maxColumn).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA") + " = 0");
                            else
                                ws.Range(row, 2, row, maxColumn).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA") + " = " + (totalNetSales / countReceipt).ToString("F"));

                            ws.Range((row - 1), 1, row, column).Style.Font.SetBold(true);
                            ws.Range(row - 1, 1, row, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                            ws.Range(startRow, 4, row, column).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range(startRow, 1, row, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(startRow, 1, row, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            //ws.Range("A" + (row + 1) + ":T" + (row + 1) + "").Merge().SetValue("");
                            //row++;
                            ws.Range(row + 1, 1, row + 1, maxColumn).Merge().SetValue("");

                        }//End for data
                    }
                }//End business day
                ws.Range(rowByStore, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(rowByStore, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;


            }//End store loop

            #region Table Header
            CreateReportHeaderNew(ws, maxColumnStore1, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
            ws.Range(1, 1, 4, maxColumnStore1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumnStore1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #endregion
            //set Width for Colum 
            List<int> lstWidCol = new List<int>() { 13, 16, 5, 15, 10, 12, 15, 13, 12, 12, 20, 15, 15 };
            for (int i = 0; i < maxColumn; i++)
            {
                if (i <= 12)
                    ws.Column(i + 1).Width = lstWidCol[i];
                else
                    ws.Column(i + 1).Width = 14;
            }
            return wb;
        }

        #region Report with refund gift card
        public List<DailyReceiptReportModels> GetDataReceiptItems_RefundGC(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DailyReceiptReport
                               where model.ListStores.Contains(tb.StoreId)
                                     && tb.CreatedDate >= model.FromDate
                                             && tb.CreatedDate <= model.ToDate
                                             && tb.Mode == model.Mode
                               select new DailyReceiptReportModels
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
                                   Tips = tb.Tips,
                                   Rounding = tb.Rounding,
                                   NetSales = tb.NetSales,
                                   PromotionValue = tb.PromotionValue,
                                   CreditNoteNo = tb.CreditNoteNo
                               }).ToList();
                return lstData;
            }
        }

        public XLWorkbook ExportExcel_WithCreditNote(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "Daily_Receipt_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dFromFilter = model.FromDate;
            DateTime _dToFilter = model.ToDate;
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, _dFromFilter, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
            //model.ToDate = new DateTime(2018, 4, 10, 17, 0, 0);

            // Data of Receipt
            List<DailyReceiptReportModels> reportItems = GetDataReceiptItems_RefundGC(model);

            // Data of Payment
            //List<DailyReceiptPaymentReportDTO> payByMethods = GetDataPaymentItems(model);
            var payByMethods = _dailySalesReportFactory.GetDataPaymentItems(model);

            // Data of Refund
            var _lstRefund = _refundFactory.GetListRefundWithoutDetail(model);
            if (_lstRefund == null)
                _lstRefund = new List<RefundReportDTO>();

            // Data of Payment information
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            // Edit check cash & GC 2017/07/24
            var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
            if (cash == null)
                cash = new RFilterChooseExtBaseModel();
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            // List items no include sale
            var lstReceiptId = reportItems.Select(ss => ss.ReceiptId).Distinct().ToList();
            var lstCreditNoteId = reportItems.Where(w => !string.IsNullOrEmpty(w.CreditNoteNo)).Select(ss => ss.ReceiptId).Distinct().ToList();

            List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSale = _itemizedSalesAnalysisFactory.GetItemsNoIncludeSale(lstReceiptId, model.ListStores, model.Mode);
            var lstItemNoIncludeSaleForCate = lstItemNoIncludeSale.Where(w => !string.IsNullOrEmpty(w.GLAccountCode)).ToList();
            var lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();

            var lstItemNoIncludeSaleForNetSalesReceipt = lstItemNoIncludeSale.Where(w => w.IsIncludeSale == false && !lstCreditNoteId.Contains(w.ReceiptId)).ToList();
            var lstItemNoIncludeSaleForCNNetSales = lstItemNoIncludeSale.Where(w => w.IsIncludeSale == false && lstCreditNoteId.Contains(w.ReceiptId)).ToList();

            // Get list misc
            var lstMiscs = _itemizedSalesAnalysisFactory.GetListMiscForDailyReceipt(model);

            int maxColumn = 0;
            int maxColumnStore1 = 0;
            int row = 4, column = 0, taxType = 0;
            double _refundByCash = 0; double _refundGC = 0; double _refund = 0;
            double payAmountByCate = 0;
            string storeName = string.Empty, storeId = string.Empty;

            int PAX = 0;
            decimal receiptTotal = 0;
            double payByCash = 0, _roudingTotal = 0, netSale = 0;
            int maxCol = 0;
            double payGC = 0;
            double _taxOfPayGCNotInclude = 0;
            double _svcOfPayGCNotInclude = 0;
            for (int i = 0; i < lstStore.Count; i++)
            {
                int rowByStore = row;
                taxType = _taxFactory.GetTaxTypeForStore(lstStore[i].Id);
                storeName = lstStore[i].Name;
                storeId = lstStore[i].Id;
                column = 1;
                maxColumn = 0;
                row++;
                // store name
                ws.Range(string.Format("A{0}:C{0}", row)).Merge().SetValue(storeName);
                #region Columns Names
                row++;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge"));
                if (taxType == (int)Commons.ETax.AddOn) //Temp
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                }
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                //=============================================================
                //Check no include on sale
                //=============================================================
                lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();

                if (lstItemNoIncludeSaleForCate != null && lstItemNoIncludeSaleForCate.Any())
                {
                    var lstNoIncludeOnSaleGroupByCate = lstItemNoIncludeSaleForCate.GroupBy(gg => gg.GLAccountCode).OrderBy(x => x.Key).ToList();
                    for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                    {
                        lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                        {
                            GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                        });
                        ws.Range(row, column, row + 1, column).Style.Alignment.SetWrapText(true);

                        if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                        {
                            ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                        }
                        else
                            ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                    }
                }

                ///End Check no include on sale
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                int startPayColumn = column;

                List<string> lstNotInPaymentCash = new List<string>() { "Cash", };
                List<string> lstNotInPaymentCashNew = new List<string>() { cash != null ? cash.Id : "" };

                List<string> storeCards = new List<string>();

                var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && ww.Name.ToLower() != "cash"
                   && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();

                for (int p = 0; p < lstPaymentParents.Count; p++)
                {
                    lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                    && ww.StoreId.Equals(lstStore[i].Id)).OrderBy(ww => ww.Name).ToList();
                    if (lstPaymentParents[p].ListChilds != null && lstPaymentParents[p].ListChilds.Count > 0)
                    {
                        ws.Range(row, column, row, (column + lstPaymentParents[p].ListChilds.Count - 1)).Merge().SetValue(lstPaymentParents[p].Name);
                        for (int c = 0; c < lstPaymentParents[p].ListChilds.Count; c++)
                        {
                            ws.Cell(row + 1, column).SetValue(lstPaymentParents[p].ListChilds[c].Name);
                            ws.Cell(row + 1, column).Style.Alignment.SetWrapText(true);
                            storeCards.Add(lstPaymentParents[p].ListChilds[c].Id);
                            column++;
                        }
                    }
                    else
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(lstPaymentParents[p].Name);
                        storeCards.Add(lstPaymentParents[p].Id);
                    }
                }

                int endPayColumn = column - 1;
                // end group payments
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                #endregion
                // set value for last column
                if (maxColumn < column)
                    maxColumn = column;

                // Get value for set format report header depend on max column of store 1
                if (i == 0)
                {
                    maxColumnStore1 = maxColumn;
                }

                #region Format Store Header
                ws.Range(row - 1, 1, row + 1, column).Style.Font.SetBold(true);
                ws.Range(row - 1, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                #endregion

                var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == lstStore[i].Id).ToList();
                if (lstBusDay != null && lstBusDay.Count > 0)
                {
                    lstBusDay = lstBusDay.OrderBy(oo => oo.DateFrom).ToList();
                    for (int b = 0; b < lstBusDay.Count; b++)
                    {
                        var dFrom = lstBusDay[b].DateFrom;
                        var dTo = lstBusDay[b].DateTo;

                        List<DailyReceiptReportModels> lstDataByStore = reportItems.Where(ww => ww.StoreId == lstStore[i].Id
                        && ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo).ToList();

                        if (lstDataByStore != null && lstDataByStore.Any())
                        {
                            lstDataByStore = lstDataByStore.OrderBy(oo => oo.CreatedDate).ToList();
                            row += 2;
                            int startRow = row;

                            // Fill data
                            double payTotal = 0, payAmount = 0;
                            double totalNetSales = 0;
                            Dictionary<int, decimal> total = new Dictionary<int, decimal>();
                            for (int j = 0; j < lstDataByStore.Count; j++)
                            {
                                #region Export
                                column = 1;
                                payTotal = 0;
                                payAmount = 0;

                                // Date
                                ws.Cell(row, column++).Value = lstDataByStore[j].CreatedDate;
                                ws.Cell(row, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                                if (!total.ContainsKey(column))
                                    total.Add(column, 0);

                                // Receipt No. or Credit Note
                                ws.Cell(row, column++).Value = String.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo) ? lstDataByStore[j].ReceiptNo : lstDataByStore[j].CreditNoteNo;
                                if (!total.ContainsKey(column))
                                    total.Add(column, 0);

                                // PAX, if Credit Note => PAX = 0
                                PAX = lstDataByStore[j].NoOfPerson;
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    PAX = 0;
                                }
                                ws.Cell(row, column++).Value = PAX;
                                if (!total.ContainsKey(column))
                                    total.Add(column, PAX);
                                else
                                    total[column] += PAX;

                                // ReceiptTotal, if Credit Note => ReceiptTotal = 0
                                receiptTotal = (decimal)lstDataByStore[j].ReceiptTotal;
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    receiptTotal = 0;
                                }
                                ws.Cell(row, column++).Value = receiptTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, receiptTotal);
                                else
                                    total[column] += receiptTotal;

                                //Refund
                                _refundByCash = _lstRefund.Where(ww => lstDataByStore[j].ReceiptId == ww.OrderId && !ww.IsGiftCard).Sum(ss => ss.TotalRefund);
                                _refundGC = _lstRefund.Where(ww => lstDataByStore[j].ReceiptId == ww.OrderId && ww.IsGiftCard).Sum(ss => ss.TotalRefund);

                                // Refund, if Credit Note , Refund = total credit note
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    _refund = lstDataByStore[j].ReceiptTotal;
                                }
                                else
                                {
                                    _refund = _refundByCash + _refundGC;
                                }
                                ws.Cell(row, column++).Value = _refund.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)_refund);
                                else
                                    total[column] += (decimal)_refund;

                                // Discount
                                ws.Cell(row, column++).Value = lstDataByStore[j].Discount.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].Discount);
                                else
                                    total[column] += (decimal)lstDataByStore[j].Discount;

                                // Promotion
                                ws.Cell(row, column++).Value = lstDataByStore[j].PromotionValue.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].PromotionValue);
                                else
                                    total[column] += (decimal)lstDataByStore[j].PromotionValue;

                                // ServiceCharge
                                ws.Cell(row, column++).Value = lstDataByStore[j].ServiceCharge.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].ServiceCharge);
                                else
                                    total[column] += (decimal)lstDataByStore[j].ServiceCharge;

                                // Tax
                                ws.Cell(row, column++).Value = lstDataByStore[j].GST.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].GST);
                                else
                                    total[column] += (decimal)lstDataByStore[j].GST;

                                // Tips
                                ws.Cell(row, column++).Value = lstDataByStore[j].Tips.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].Tips);
                                else
                                    total[column] += (decimal)lstDataByStore[j].Tips;

                                _roudingTotal = lstDataByStore[j].Rounding;
                                //=======================================================
                                //NAV - NoIncludeOnSale (- discount - promotion)
                                // GL account code and group by it
                                //Note: value is not include tax, right? It means
                                //Tax is include on item price -> value = [[(price of items x qty ) - (disc or promo )] +SVC ] -Tax
                                //Tax is add on ->value = [(price of items x qty ) - (disc or promo )] +SVC
                                //update 2018-04-19 tax follow dish
                                double _tax = 0;
                                for (int a = 0; a < lstItemNoIncludeSaleGroupByCate.Count; a++)
                                {
                                    _tax = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                         && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.TaxType == (int)Commons.ETax.Inclusive
                                                         && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                                         .Sum(p => p.Tax);

                                    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                                && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                                                .Sum(p => ((decimal)p.TotalAmount - (decimal)p.TotalDiscount - (decimal)p.PromotionAmount));

                                    payAmountByCate -= _tax;

                                    if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                    {
                                        payAmountByCate += lstMiscs.Where(ww => ww.StoreId == lstStore[i].Id && ww.ReceiptId == lstDataByStore[j].ReceiptId
                                        && ww.CreatedDate >= lstBusDay[b].DateFrom && ww.CreatedDate <= lstBusDay[b].DateTo).Sum(ss => ss.TotalPrice);

                                        payAmountByCate += _roudingTotal;

                                    }
                                    if (payAmountByCate < 0)
                                    {
                                        payAmountByCate = 0;
                                    }

                                    #region Cate Old
                                    //if (_isTaxInclude)
                                    //{
                                    //    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    //&& p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                    //                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                    //                            .Sum(p => ((decimal)p.TotalAmount - (decimal)p.TotalDiscount - (decimal)p.PromotionAmount
                                    //                            - (decimal)p.Tax));
                                    //}
                                    //else
                                    //{
                                    //    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    //    && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                    //                                && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                    //                                .Sum(p => ((decimal)p.TotalAmount - (decimal)p.TotalDiscount - (decimal)p.PromotionAmount));
                                    //}


                                    //if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                    //{
                                    //    payAmountByCate += lstMiscs.Where(ww => ww.StoreId == lstStore[i].Id && ww.ReceiptId == lstDataByStore[j].ReceiptId
                                    //    && ww.CreatedDate >= lstBusDay[b].DateFrom && ww.CreatedDate <= lstBusDay[b].DateTo).Sum(ss => ss.TotalPrice);

                                    //    payAmountByCate += _roudingTotal;

                                    //}
                                    //if (payAmountByCate < 0)
                                    //{
                                    //    payAmountByCate = 0;
                                    //}
                                    #endregion End Cate Old

                                    ws.Cell(row, column++).Value = ((decimal)payAmountByCate).ToString("F");

                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, (decimal)payAmountByCate);
                                    }
                                    else
                                    {
                                        total[column] += (decimal)CommonHelper.RoundUp2Decimal(payAmountByCate);
                                    }
                                }

                                // NetSales -NAV - GC
                                //payAmountNoIncludeSaleForNetSale = 0;
                                //netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (1)
                                //(1)= GC pay not include - tax of pay GC not include - SCV of pay GC not include 
                                double totalCreditNote = 0;
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    totalCreditNote = lstItemNoIncludeSale.Where(ww => lstDataByStore[j].ReceiptId == ww.ReceiptId
                                    && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                        .Sum(ss => ss.ItemTotal);
                                }
                                double giftCardSell = lstItemNoIncludeSale.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                   && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                   && ww.ReceiptId == lstDataByStore[j].ReceiptId
                                                   && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                                var taxInclude = lstItemNoIncludeSale.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                               && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                              && ww.ReceiptId == lstDataByStore[j].ReceiptId).Sum(ss => ss.Tax);

                                var _noincludeSale = lstItemNoIncludeSale.Where(ww => ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                               && ww.ReceiptId == lstDataByStore[j].ReceiptId).Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                                _noincludeSale -= taxInclude;


                                var lstPaymentGCs = payByMethods.Where(p => p.StoreId == lstDataByStore[j].StoreId
                                  && p.OrderId == lstDataByStore[j].ReceiptId
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
                                        var lstGCRefunds = _lstRefund.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCRefunds != null && lstGCRefunds.Any())
                                        {
                                            var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                            _amount -= refundAmount;
                                        }
                                        payGC += _amount;
                                        double tax = 0;
                                        double svc = 0;
                                        if (lstDataByStore[j].GST != 0)
                                            tax = _amount * lstDataByStore[j].GST / (lstDataByStore[j].ReceiptTotal == 0 ? 1 : lstDataByStore[j].ReceiptTotal);
                                        if (lstDataByStore[j].ServiceCharge != 0)
                                            svc = (_amount - tax) * lstDataByStore[j].ServiceCharge
                                                / ((lstDataByStore[j].ReceiptTotal - lstDataByStore[j].GST) == 0 ? 1 : (lstDataByStore[j].ReceiptTotal - lstDataByStore[j].GST));

                                        _taxOfPayGCNotInclude += tax;
                                        _svcOfPayGCNotInclude += svc;

                                    }
                                }
                                netSale = (string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo) ? lstDataByStore[j].ReceiptTotal : 0) - totalCreditNote - lstDataByStore[j].ServiceCharge - lstDataByStore[j].GST - giftCardSell - _noincludeSale
                                - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);

                                totalNetSales += netSale;

                                #region netsale Old
                                //if (string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo)) // Receipt
                                //{
                                //    // Total Amount of items no include sale
                                //    if (!_isTaxInclude)
                                //    {
                                //        payAmountNoIncludeSaleForNetSale = (double)lstItemNoIncludeSaleForNetSalesReceipt.Where(p => p.StoreId == lstStore[i].Id
                                //            && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                //                                   .Sum(p => (decimal)p.TotalAmount
                                //                                   - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount);
                                //    }
                                //    else
                                //    {

                                //        payAmountNoIncludeSaleForNetSale = (double)lstItemNoIncludeSaleForNetSalesReceipt.Where(p => p.StoreId == lstStore[i].Id
                                //            && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                //                                    .Sum(p => (decimal)p.TotalAmount
                                //                                    - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount
                                //                                    - (decimal)p.Tax);
                                //    }

                                //    var payGC = payByMethods.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= lstBusDay[b].DateFrom
                                //                                && p.CreatedDate <= lstBusDay[b].DateTo
                                //                             && lstGC.Contains(p.PaymentId) && p.OrderId == lstDataByStore[j].ReceiptId).Sum(p => p.Amount);

                                //    netSale = lstDataByStore[j].NetSales - payAmountNoIncludeSaleForNetSale - payGC + _roudingTotal;

                                //    // For TA, only get total NetSale of receipt
                                //    totalNetSales += netSale;
                                //}
                                //else //Credit Note
                                //{
                                //    if (!_isTaxInclude)
                                //    {
                                //        payAmountNoIncludeSaleForNetSale = (double)lstItemNoIncludeSaleForCNNetSales.Where(p => p.StoreId == lstStore[i].Id
                                //            && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                //                                   .Sum(p => (decimal)p.TotalAmount
                                //                                   - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount);

                                //    }
                                //    else
                                //    {
                                //        payAmountNoIncludeSaleForNetSale = (double)lstItemNoIncludeSaleForCNNetSales.Where(p => p.StoreId == lstStore[i].Id
                                //            && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                //                                   .Sum(p => (decimal)p.TotalAmount
                                //                                   - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount
                                //                                   - (decimal)p.Tax);
                                //    }

                                //    netSale = (lstDataByStore[j].NetSales - payAmountNoIncludeSaleForNetSale) * -1;
                                //}
                                #endregion End NetSale Old

                                ws.Cell(row, column++).Value = netSale.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)netSale);
                                else
                                    total[column] += (decimal)netSale;

                                // Rounding
                                ws.Cell(row, column++).Value = _roudingTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)_roudingTotal);
                                else
                                    total[column] += (decimal)_roudingTotal;

                                // Pay by cash (subtract refund)
                                payByCash = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                           && (p.PaymentName == lstNotInPaymentCash[0] || p.PaymentId == lstNotInPaymentCashNew[0])
                                                           ).Sum(p => p.Amount) - _refundByCash;//TotalPayment


                                ws.Cell(row, column++).Value = payByCash.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)payByCash);
                                else
                                    total[column] += (decimal)payByCash;
                                payTotal += payByCash;

                                //payment other methods
                                for (int k = 0; k < storeCards.Count; k++)
                                {
                                    if (storeCards[k] != "")
                                    {
                                        payAmount = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                        && p.PaymentId == storeCards[k]).Sum(p => p.Amount); //TotalPayment
                                        if (lstGC.Contains(storeCards[k]))
                                        {
                                            payAmount -= _refundGC;
                                        }
                                    }
                                    else
                                        payAmount = 0;
                                    ws.Cell(row, column++).Value = payAmount.ToString("F");
                                    if (!total.ContainsKey(column))
                                        total.Add(column, (decimal)payAmount);
                                    else
                                        total[column] += (decimal)payAmount;
                                    payTotal += payAmount;
                                }

                                //payTotal
                                ws.Cell(row, column++).Value = payTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)payTotal);
                                else
                                    total[column] += (decimal)payTotal;

                                //excess
                                //double excess = (payTotal - lstDataByStore[j].ReceiptTotal - lstDataByStore[j].Tips);
                                //if (excess < 0)
                                //{
                                //    excess = 0;
                                //}
                                double excess = 0;
                                if (string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    excess = (payTotal - lstDataByStore[j].ReceiptTotal);
                                }
                                ws.Cell(row, column).Value = excess.ToString("F");
                                if (!total.ContainsKey(column + 1))
                                    total.Add(column + 1, (decimal)excess);
                                else
                                    total[column + 1] += (decimal)excess;

                                row++;

                                #endregion
                            }
                            // Total Row
                            ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                            for (int j = 2; j <= column; j++)
                            {
                                if (total.ContainsKey(j + 1))
                                {
                                    ws.Cell(row, j).Value = total[j + 1];
                                }

                            }
                            row++;
                            int countReceipt = lstDataByStore.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Count();
                            ws.Cell(row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC") + " = " + countReceipt;
                            if (countReceipt == 0 || totalNetSales == 0)
                                ws.Range(row, 2, row, maxColumn).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA") + " = 0");
                            else
                                ws.Range(row, 2, row, maxColumn).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA") + " = " + (totalNetSales / countReceipt).ToString("F"));

                            ws.Range((row - 1), 1, row, column).Style.Font.SetBold(true);
                            ws.Range(row - 1, 1, row, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                            ws.Range(startRow, 4, row, column).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range(startRow, 1, row, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(startRow, 1, row, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            ws.Range(row + 1, 1, row + 1, maxColumn).Merge().SetValue("");

                        }//End for data
                    }
                }//End business day
                ws.Range(rowByStore, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(rowByStore, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;

                // For set column width
                if (maxCol < maxColumn)
                {
                    maxCol = maxColumn;
                }

            }//End store loop

            #region Table Header
            CreateReportHeaderNew(ws, maxColumnStore1, _dFromFilter, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
            ws.Range(1, 1, 4, maxColumnStore1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumnStore1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #endregion
            // Set Width for Column 
            ws.Columns().AdjustToContents();

            ws.Column(1).Width = 20;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 9;

            for (int i = 3; i < maxCol; i++)
            {
                ws.Column(i + 1).Width = 17;
            }

            return wb;
        }
        #endregion End report with refund gift card
        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]
        public List<DailyReceiptReportModels> GetDataReceiptItems_NewDB(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_PosSale
                               where model.ListStores.Contains(tb.StoreId)
                                     && tb.ReceiptCreatedDate >= model.FromDate
                                             && tb.ReceiptCreatedDate <= model.ToDate
                                             && tb.Mode == model.Mode
                               select new DailyReceiptReportModels
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
                                   Tips = tb.Tip,
                                   Rounding = tb.Rounding,
                                   //NetSales = tb.NetSales,
                                   PromotionValue = tb.PromotionValue,
                                   CreditNoteNo = tb.CreditNoteNo
                               }).ToList();
                return lstData;
            }
        }

        public XLWorkbook ExportExcel_NewDB(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "Daily_Receipt_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dFromFilter = model.FromDate;
            DateTime _dToFilter = model.ToDate;
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, _dFromFilter, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

            // Data of Receipt
            List<DailyReceiptReportModels> reportItems = GetDataReceiptItems_NewDB(model);
            if (reportItems == null || !reportItems.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, _dFromFilter, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            // Data of Payment
            var payByMethods = _dailySalesReportFactory.GetDataPaymentItems(model);

            // Data of Payment information
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            // Edit check cash & GC 2017/07/24
            var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
            if (cash == null)
                cash = new RFilterChooseExtBaseModel();
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            // Data of Refund
            var lstReceiptId = reportItems.Select(ss => ss.ReceiptId).Distinct().ToList();
            var _lstRefund = _refundFactory.GetListRefundWithoutDetailsByReceiptId(model.ListStores, lstReceiptId);
            if (_lstRefund == null)
                _lstRefund = new List<RefundReportDTO>();

            PosSaleFactory posSaleFactory = new PosSaleFactory();
            // List items no include sale
            List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSale = posSaleFactory.GetItemsNoIncludeSale(lstReceiptId, model.ListStores, model.Mode);
            var lstItemNoIncludeSaleForCate = lstItemNoIncludeSale.Where(w => !string.IsNullOrEmpty(w.GLAccountCode)).ToList();
            var lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();

            // Get list misc
            var lstMiscs = posSaleFactory.GetListMiscForDailyReceipt(model.ListStores, lstReceiptId, model.Mode);

            int maxColumn = 0;
            int maxColumnStore1 = 0;
            int row = 4, column = 0, taxType = 0;
            double _refundByCash = 0; double _refundGC = 0; double _refund = 0;
            double payAmountByCate = 0;
            string storeName = string.Empty, storeId = string.Empty;

            int PAX = 0;
            decimal receiptTotal = 0;
            double payByCash = 0, _roudingTotal = 0, netSale = 0;
            int maxCol = 0;
            double payGC = 0;
            double _taxOfPayGCNotInclude = 0;
            double _svcOfPayGCNotInclude = 0;
            for (int i = 0; i < lstStore.Count; i++)
            {
                int rowByStore = row;
                taxType = _taxFactory.GetTaxTypeForStore(lstStore[i].Id);
                storeName = lstStore[i].Name;
                storeId = lstStore[i].Id;
                column = 1;
                maxColumn = 0;
                row++;
                // store name
                ws.Range(string.Format("A{0}:C{0}", row)).Merge().SetValue(storeName);
                #region Columns Names
                row++;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge"));
                if (taxType == (int)Commons.ETax.AddOn) //Temp
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                }
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                //=============================================================
                //Check no include on sale
                //=============================================================
                lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();

                if (lstItemNoIncludeSaleForCate != null && lstItemNoIncludeSaleForCate.Any())
                {
                    var lstNoIncludeOnSaleGroupByCate = lstItemNoIncludeSaleForCate.GroupBy(gg => gg.GLAccountCode).OrderBy(x => x.Key).ToList();
                    for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                    {
                        lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                        {
                            GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                        });
                        ws.Range(row, column, row + 1, column).Style.Alignment.SetWrapText(true);

                        if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                        {
                            ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                        }
                        else
                            ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                    }
                }

                ///End Check no include on sale
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                int startPayColumn = column;

                List<string> lstNotInPaymentCash = new List<string>() { "Cash", };
                List<string> lstNotInPaymentCashNew = new List<string>() { cash != null ? cash.Id : "" };

                List<string> storeCards = new List<string>();

                var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && ww.Name.ToLower() != "cash"
                   && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();

                for (int p = 0; p < lstPaymentParents.Count; p++)
                {
                    lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                    && ww.StoreId.Equals(lstStore[i].Id)).OrderBy(ww => ww.Name).ToList();
                    if (lstPaymentParents[p].ListChilds != null && lstPaymentParents[p].ListChilds.Count > 0)
                    {
                        ws.Range(row, column, row, (column + lstPaymentParents[p].ListChilds.Count - 1)).Merge().SetValue(lstPaymentParents[p].Name);
                        for (int c = 0; c < lstPaymentParents[p].ListChilds.Count; c++)
                        {
                            ws.Cell(row + 1, column).SetValue(lstPaymentParents[p].ListChilds[c].Name);
                            ws.Cell(row + 1, column).Style.Alignment.SetWrapText(true);
                            storeCards.Add(lstPaymentParents[p].ListChilds[c].Id);
                            column++;
                        }
                    }
                    else
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(lstPaymentParents[p].Name);
                        storeCards.Add(lstPaymentParents[p].Id);
                    }
                }

                int endPayColumn = column - 1;
                // end group payments
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                #endregion
                // set value for last column
                if (maxColumn < column)
                    maxColumn = column;

                // Get value for set format report header depend on max column of store 1
                if (i == 0)
                {
                    maxColumnStore1 = maxColumn;
                }

                #region Format Store Header
                ws.Range(row - 1, 1, row + 1, column).Style.Font.SetBold(true);
                ws.Range(row - 1, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                #endregion

                var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == lstStore[i].Id).OrderBy(oo => oo.DateFrom).ToList();
                if (lstBusDay != null && lstBusDay.Any())
                {
                    for (int b = 0; b < lstBusDay.Count; b++)
                    {
                        List<DailyReceiptReportModels> lstDataByStore = reportItems.Where(ww => ww.StoreId == lstStore[i].Id
                        && lstBusDay[b].Id == ww.BusinessDayId).ToList();

                        if (lstDataByStore != null && lstDataByStore.Any())
                        {
                            lstDataByStore = lstDataByStore.OrderBy(oo => oo.CreatedDate).ToList();
                            row += 2;
                            int startRow = row;

                            // Fill data
                            double payTotal = 0, payAmount = 0;
                            double totalNetSales = 0;
                            Dictionary<int, decimal> total = new Dictionary<int, decimal>();
                            for (int j = 0; j < lstDataByStore.Count; j++)
                            {
                                #region Export
                                column = 1;
                                payTotal = 0;
                                payAmount = 0;

                                // Date
                                ws.Cell(row, column++).Value = lstDataByStore[j].CreatedDate;
                                ws.Cell(row, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                                if (!total.ContainsKey(column))
                                    total.Add(column, 0);

                                // Receipt No. or Credit Note
                                ws.Cell(row, column++).Value = String.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo) ? lstDataByStore[j].ReceiptNo : lstDataByStore[j].CreditNoteNo;
                                if (!total.ContainsKey(column))
                                    total.Add(column, 0);

                                // PAX, if Credit Note => PAX = 0
                                PAX = lstDataByStore[j].NoOfPerson;
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    PAX = 0;
                                }
                                ws.Cell(row, column++).Value = PAX;
                                if (!total.ContainsKey(column))
                                    total.Add(column, PAX);
                                else
                                    total[column] += PAX;

                                // ReceiptTotal, if Credit Note => ReceiptTotal = 0
                                receiptTotal = (decimal)lstDataByStore[j].ReceiptTotal;
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    receiptTotal = 0;
                                }
                                ws.Cell(row, column++).Value = receiptTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, receiptTotal);
                                else
                                    total[column] += receiptTotal;

                                //Refund
                                _refundByCash = _lstRefund.Where(ww => lstDataByStore[j].ReceiptId == ww.OrderId && !ww.IsGiftCard).Sum(ss => ss.TotalRefund);
                                _refundGC = _lstRefund.Where(ww => lstDataByStore[j].ReceiptId == ww.OrderId && ww.IsGiftCard).Sum(ss => ss.TotalRefund);
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    _refund = lstDataByStore[j].ReceiptTotal;
                                }
                                else
                                {
                                    _refund = _refundByCash + _refundGC;
                                }
                                ws.Cell(row, column++).Value = _refund.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)_refund);
                                else
                                    total[column] += (decimal)_refund;

                                // Discount
                                decimal discount = (decimal)lstDataByStore[j].Discount * (-1);
                                ws.Cell(row, column++).Value = discount.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, discount);
                                else
                                    total[column] += discount;

                                // Promotion
                                decimal promo = (decimal)lstDataByStore[j].PromotionValue * (-1);
                                ws.Cell(row, column++).Value = promo.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, promo);
                                else
                                    total[column] += promo;

                                // ServiceCharge
                                ws.Cell(row, column++).Value = lstDataByStore[j].ServiceCharge.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].ServiceCharge);
                                else
                                    total[column] += (decimal)lstDataByStore[j].ServiceCharge;

                                // Tax
                                ws.Cell(row, column++).Value = lstDataByStore[j].GST.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].GST);
                                else
                                    total[column] += (decimal)lstDataByStore[j].GST;

                                // Tips
                                ws.Cell(row, column++).Value = lstDataByStore[j].Tips.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)lstDataByStore[j].Tips);
                                else
                                    total[column] += (decimal)lstDataByStore[j].Tips;

                                _roudingTotal = lstDataByStore[j].Rounding;
                                //=======================================================
                                //NAV - NoIncludeOnSale (- discount - promotion)
                                // GL account code and group by it
                                //Note: value is not include tax, right? It means
                                //Tax is include on item price -> value = [[(price of items x qty ) - (disc or promo )] +SVC ] -Tax
                                //Tax is add on ->value = [(price of items x qty ) - (disc or promo )] +SVC
                                //update 2018-04-19 tax follow dish
                                double _tax = 0;
                                for (int a = 0; a < lstItemNoIncludeSaleGroupByCate.Count; a++)
                                {
                                    _tax = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                         && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.TaxType == (int)Commons.ETax.Inclusive
                                                         && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                                         .Sum(p => p.Tax);

                                    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    && p.CreatedDate >= lstBusDay[b].DateFrom && p.CreatedDate <= lstBusDay[b].DateTo
                                                                && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.ReceiptId == lstDataByStore[j].ReceiptId)
                                                                .Sum(p => ((decimal)p.TotalAmount - (decimal)p.TotalDiscount - (decimal)p.PromotionAmount));

                                    payAmountByCate -= _tax;

                                    if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                    {
                                        payAmountByCate += lstMiscs.Where(ww => ww.StoreId == lstStore[i].Id && ww.ReceiptId == lstDataByStore[j].ReceiptId
                                        && ww.CreatedDate >= lstBusDay[b].DateFrom && ww.CreatedDate <= lstBusDay[b].DateTo).Sum(ss => ss.TotalPrice);

                                        payAmountByCate += _roudingTotal;

                                    }
                                    if (payAmountByCate < 0)
                                    {
                                        payAmountByCate = 0;
                                    }

                                    ws.Cell(row, column++).Value = ((decimal)payAmountByCate).ToString("F");

                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, (decimal)payAmountByCate);
                                    }
                                    else
                                    {
                                        total[column] += (decimal)CommonHelper.RoundUp2Decimal(payAmountByCate);
                                    }
                                }

                                // NetSales -NAV - GC
                                //payAmountNoIncludeSaleForNetSale = 0;
                                //netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (1)
                                //(1)= GC pay not include - tax of pay GC not include - SCV of pay GC not include 
                                double totalCreditNote = 0;
                                if (!string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    totalCreditNote = lstItemNoIncludeSale.Where(ww => lstDataByStore[j].ReceiptId == ww.ReceiptId
                                    && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                        .Sum(ss => ss.ItemTotal);
                                }
                                double giftCardSell = lstItemNoIncludeSale.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                   && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                   && ww.ReceiptId == lstDataByStore[j].ReceiptId
                                                   && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                                var taxInclude = lstItemNoIncludeSale.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                               && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                              && ww.ReceiptId == lstDataByStore[j].ReceiptId).Sum(ss => ss.Tax);

                                var _noincludeSale = lstItemNoIncludeSale.Where(ww => ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                               && ww.ReceiptId == lstDataByStore[j].ReceiptId).Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                                _noincludeSale -= taxInclude;


                                var lstPaymentGCs = payByMethods.Where(p => p.StoreId == lstDataByStore[j].StoreId
                                  && p.OrderId == lstDataByStore[j].ReceiptId
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
                                        var lstGCRefunds = _lstRefund.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCRefunds != null && lstGCRefunds.Any())
                                        {
                                            var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                            _amount -= refundAmount;
                                        }
                                        payGC += _amount;
                                        double tax = 0;
                                        double svc = 0;
                                        if (lstDataByStore[j].GST != 0)
                                            tax = _amount * lstDataByStore[j].GST / (lstDataByStore[j].ReceiptTotal == 0 ? 1 : lstDataByStore[j].ReceiptTotal);
                                        if (lstDataByStore[j].ServiceCharge != 0)
                                            svc = (_amount - tax) * lstDataByStore[j].ServiceCharge
                                                / ((lstDataByStore[j].ReceiptTotal - lstDataByStore[j].GST) == 0 ? 1 : (lstDataByStore[j].ReceiptTotal - lstDataByStore[j].GST));

                                        _taxOfPayGCNotInclude += tax;
                                        _svcOfPayGCNotInclude += svc;

                                    }
                                }
                                netSale = (string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo) ? lstDataByStore[j].ReceiptTotal : 0) - totalCreditNote - lstDataByStore[j].ServiceCharge - lstDataByStore[j].GST - giftCardSell - _noincludeSale
                                - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);

                                totalNetSales += netSale;

                                ws.Cell(row, column++).Value = netSale.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)netSale);
                                else
                                    total[column] += (decimal)netSale;

                                // Rounding
                                ws.Cell(row, column++).Value = _roudingTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)_roudingTotal);
                                else
                                    total[column] += (decimal)_roudingTotal;

                                // Pay by cash (subtract refund)
                                payByCash = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                           && (p.PaymentName == lstNotInPaymentCash[0] || p.PaymentId == lstNotInPaymentCashNew[0])
                                                           ).Sum(p => p.Amount) - _refundByCash;


                                ws.Cell(row, column++).Value = payByCash.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)payByCash);
                                else
                                    total[column] += (decimal)payByCash;
                                payTotal += payByCash;

                                //payment other methods
                                for (int k = 0; k < storeCards.Count; k++)
                                {
                                    if (storeCards[k] != "")
                                    {
                                        payAmount = payByMethods.Where(p => p.StoreId == storeId && p.OrderId == lstDataByStore[j].ReceiptId
                                                        && p.PaymentId == storeCards[k]).Sum(p => p.Amount); //TotalPayment
                                        if (lstGC.Contains(storeCards[k]))
                                        {
                                            payAmount -= _refundGC;
                                        }
                                    }
                                    else
                                        payAmount = 0;
                                    ws.Cell(row, column++).Value = payAmount.ToString("F");
                                    if (!total.ContainsKey(column))
                                        total.Add(column, (decimal)payAmount);
                                    else
                                        total[column] += (decimal)payAmount;
                                    payTotal += payAmount;
                                }

                                //payTotal
                                ws.Cell(row, column++).Value = payTotal.ToString("F");
                                if (!total.ContainsKey(column))
                                    total.Add(column, (decimal)payTotal);
                                else
                                    total[column] += (decimal)payTotal;

                                //excess
                                double excess = 0;
                                if (string.IsNullOrEmpty(lstDataByStore[j].CreditNoteNo))
                                {
                                    excess = (payTotal - lstDataByStore[j].ReceiptTotal);
                                }
                                ws.Cell(row, column).Value = excess.ToString("F");
                                if (!total.ContainsKey(column + 1))
                                    total.Add(column + 1, (decimal)excess);
                                else
                                    total[column + 1] += (decimal)excess;

                                row++;

                                #endregion
                            }
                            // Total Row
                            ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                            for (int j = 2; j <= column; j++)
                            {
                                if (total.ContainsKey(j + 1))
                                {
                                    ws.Cell(row, j).Value = total[j + 1];
                                }

                            }
                            row++;
                            int countReceipt = lstDataByStore.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Count();
                            ws.Cell(row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC") + " = " + countReceipt;
                            if (countReceipt == 0 || totalNetSales == 0)
                                ws.Range(row, 2, row, maxColumn).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA") + " = 0");
                            else
                                ws.Range(row, 2, row, maxColumn).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TA") + " = " + (totalNetSales / countReceipt).ToString("F"));

                            ws.Range((row - 1), 1, row, column).Style.Font.SetBold(true);
                            ws.Range(row - 1, 1, row, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                            ws.Range(startRow, 4, row, column).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range(startRow, 1, row, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(startRow, 1, row, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            ws.Range(row + 1, 1, row + 1, maxColumn).Merge().SetValue("");

                        }//End for data
                    }
                }//End business day
                ws.Range(rowByStore, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(rowByStore, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;

                // For set column width
                if (maxCol < maxColumn)
                {
                    maxCol = maxColumn;
                }

            }//End store loop

            #region Table Header
            CreateReportHeaderNew(ws, maxColumnStore1, _dFromFilter, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Receipt Report"));
            ws.Range(1, 1, 4, maxColumnStore1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumnStore1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #endregion
            // Set Width for Column 
            ws.Columns().AdjustToContents();

            ws.Column(1).Width = 20;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 9;

            for (int i = 3; i < maxCol; i++)
            {
                ws.Column(i + 1).Width = 17;
            }

            return wb;
        }
        #endregion
    }
}
