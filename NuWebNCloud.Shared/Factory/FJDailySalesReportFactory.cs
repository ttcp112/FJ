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
using System.Data.Entity.Core.Objects;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Data.Models;
using System.Diagnostics;

namespace NuWebNCloud.Shared.Factory
{
    public class FJDailySalesReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private ItemizedSalesAnalysisReportFactory _itemizedSalesAnalysisFactory;
        private OrderPaymentMethodFactory _orderPaymentMethodFactory;
        public FJDailySalesReportFactory()
        {
            _baseFactory = new BaseFactory();
            _itemizedSalesAnalysisFactory = new ItemizedSalesAnalysisReportFactory();
            _orderPaymentMethodFactory = new OrderPaymentMethodFactory();
        }
        public List<FJDailySalesReportModels> GetDataReceiptItems(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_DailySalesReport.Where(ww => model.ListStores.Contains(ww.StoreId)
                                     && (ww.CreatedDate >= model.FromDate && ww.CreatedDate <= model.ToDate)
                                     && ww.Mode == model.Mode)
                                     .Select(ss => new FJDailySalesReportModels()
                                     {
                                         StoreId = ss.StoreId,
                                         CreatedDate = ss.CreatedDate,
                                         ReceiptTotal = ss.ReceiptTotal,      // Total R_Receipt
                                         Discount = ss.Discount,
                                         ServiceCharge = ss.ServiceCharge,
                                         GST = ss.GST,
                                         Rounding = ss.Rounding,
                                         Refund = ss.Refund,
                                         NetSales = ss.NetSales

                                     }).ToList();

                return lstData;
            }
        }

        public List<FJDailySaleReportSettingModels> GetListStoreSetting(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.G_FJDailySaleReportSetting.Where(ww => model.ListStores.Contains(ww.StoreId)
                                     && ww.IsActived)
                                     .Select(ss => new FJDailySaleReportSettingModels()
                                     {
                                         StoreId = ss.StoreId,
                                         GLAccountCode = ss.GLAccountCodes,

                                     }).ToList();

                lstData.ForEach(ss => ss.GLAccountCodes = ss.GLAccountCode.Split(';'));
                //foreach (var item in lstData)
                //{

                //}
                return lstData;
            }
        }
        public List<PaymentModels> GetDataPaymentItems(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.G_PaymentMenthod
                               where model.ListStores.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= model.FromDate
                                             && tb.CreatedDate <= model.ToDate) && tb.Mode == model.Mode
                               select new PaymentModels
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                                   PaymentId = tb.PaymentId,
                                   PaymentName = tb.PaymentName,
                                   Amount = tb.Amount,
                                   OrderId = tb.OrderId,
                                   IsInclude = tb.IsInclude
                               }).ToList();
                return lstData;
            }
        }

        public XLWorkbook ExportExcel_Old(BaseReportModel model, List<StoreModels> lstStore, MerchantConfigApiModels pOSMerchantConfig)
        {
            string sheetName = "FJ_Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dToFilter = model.ToDate;
            // List business day in all store
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                // Format header report
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
            List<FJDailySalesReportModels> lstData = GetDataReceiptItems(model);
            List<PaymentModels> lstPayments = GetDataPaymentItems(model);
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            List<ItemizedSalesAnalysisReportModels> lstItemSales = _itemizedSalesAnalysisFactory.GetItemsForDailySalesReports(model);
            bool _isTaxInclude = _baseFactory.IsTaxInclude(model.ListStores.FirstOrDefault());

            //GC.............................................
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            //string sheetName = "FJ_Daily_Sales_Report";
            //var wb = new XLWorkbook();
            //var ws = wb.Worksheets.Add(sheetName);
            int maxColumn = 0;

            int row = 5;
            int column = 1;

            // Set title column report
            // Stall NO.
            ws.Range(row, column, row + 1, column++).Merge().Value = "Stall NO.";
            // Stall Name
            ws.Range(row, column, row + 1, column++).Merge().Value = "Stall Name";
            // Group Sales
            ws.Range(row, column, row, column + 2).Merge().Value = "Sales";
            ws.Range(row + 1, column, row + 1, column++).Value = "Morning Sales";
            ws.Range(row + 1, column, row + 1, column++).Value = "Mid-day Sales";
            ws.Range(row + 1, column, row + 1, column++).Value = "Full-day Sales";
            // Total Vouchers Amount
            ws.Range(row, column, row + 1, column++).Merge().Value = "Total Vouchers Amount";

            // Group Payment methods
            // List information of payments: cash (Commons.EPaymentCode.Cash)
            var lstPaymentMethodCashId = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash).Select(s => s.Id).ToList();

            var lstPaymentMethodExCash = lstPaymentMethod.Where(ww => ww.Code != (int)Commons.EPaymentCode.Cash).OrderBy(o => o.Name).ToList();

            // List information of payments: Vouchers (FixedPayAmount > 0)
            List<FJDailySalesReportViewModels> listInfoVounchers = new List<FJDailySalesReportViewModels>();

            var listChildsVoucher = lstPaymentMethodExCash.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.FixedPayAmount > 0).ToList();

            var lstVoucherParentIds = listChildsVoucher.Select(s => s.ParentId).Distinct().ToList();

            var lstVoucherParents = lstPaymentMethodExCash.Where(ww => lstVoucherParentIds.Contains(ww.Id)).OrderBy(o => o.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

            foreach (var paymentParent in lstVoucherParents)
            {
                var listChilds = listChildsVoucher.Where(ww => ww.ParentId == paymentParent.Id).OrderBy(ww => ww.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

                if (listChilds != null && listChilds.Any())
                {
                    ws.Range(row, column, row, (column + listChilds.Count - 1)).Merge().Value = paymentParent.Name;

                    foreach (var paymentChild in listChilds)
                    {
                        ws.Cell(row + 1, column++).Value = paymentChild.Name;
                        listInfoVounchers.Add(
                            new FJDailySalesReportViewModels
                            {
                                PaymentId = paymentChild.Id,
                                PaymentName = paymentChild.Name
                            });
                    }
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().Value = paymentParent.Name;
                    listInfoVounchers.Add(
                        new FJDailySalesReportViewModels
                        {
                            PaymentId = paymentParent.Id,
                            PaymentName = paymentParent.Name
                        });
                }
            }

            // List information of payments: card 
            int startColCard = column;
            List<FJDailySalesReportViewModels> listInfoCards = new List<FJDailySalesReportViewModels>();

            var listChildsCard = lstPaymentMethodExCash.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.FixedPayAmount == 0).ToList();

            var lstCardParentIds = listChildsCard.Select(s => s.ParentId).Distinct().ToList();

            var lstCardParents = lstPaymentMethodExCash.Where(ww => lstCardParentIds.Contains(ww.Id)).OrderBy(o => o.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

            foreach (var paymentParent in lstCardParents)
            {
                var listChilds = lstPaymentMethod.Where(ww => ww.ParentId == paymentParent.Id).OrderBy(ww => ww.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

                if (listChilds != null && listChilds.Any())
                {
                    ws.Range(row, column, row, (column + listChilds.Count - 1)).Merge().Value = paymentParent.Name;

                    foreach (var paymentChild in listChilds)
                    {
                        ws.Cell(row + 1, column++).Value = paymentChild.Name;
                        listInfoCards.Add(
                            new FJDailySalesReportViewModels
                            {
                                PaymentId = paymentChild.Id,
                                PaymentName = paymentChild.Name,
                                Count = 0,
                                Total = 0
                            });
                    }
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().Value = paymentParent.Name;
                    listInfoCards.Add(
                        new FJDailySalesReportViewModels
                        {
                            PaymentId = paymentParent.Id,
                            PaymentName = paymentParent.Name,
                            Count = 0,
                            Total = 0
                        });
                }
            }

            // Cash
            ws.Range(row, column, row + 1, column++).Merge().Value = "Cash";
            // Tax
            if (_isTaxInclude)
            {
                ws.Range(row, column, row + 1, column).Merge().Value = "Tax (Inc)";
            }
            else
                ws.Range(row, column, row + 1, column).Merge().Value = "Tax (Add)";

            if (maxColumn < column)
                maxColumn = column;

            // Set header report
            CreateReportHeaderNew(ws, maxColumn, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());

            // Format title column report
            ws.Range(row, 1, row + 1, maxColumn).Style.Font.SetBold(true);
            ws.Range(row, 1, row + 1, maxColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.SetWrapText(true);
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(row, 1, row + 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row + 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row += 2;

            //// List business day in all store
            //var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            BusinessDayDisplayModels minBD = new BusinessDayDisplayModels();
            BusinessDayDisplayModels maxBD = new BusinessDayDisplayModels();
            BusinessDayDisplayModels minBDStall14 = null;
            BusinessDayDisplayModels maxBDStall14 = null;

            // For a store
            decimal morningSales = 0;
            decimal midDaySales = 0;
            decimal fullDaySales = 0;
            decimal vouchersAmount = 0;
            int countVoucher = 0;
            decimal voucherAmount = 0;
            decimal cardAmount = 0;
            decimal cashAmount = 0;
            decimal taxAmount = 0;
            decimal netSales = 0;

            // For all store
            decimal sumMorningSales = 0;
            decimal sumMidDaySales = 0;
            decimal sumFullDaySales = 0;
            decimal sumVouchersAmount = 0;
            decimal sumCashAmount = 0;
            decimal sumTaxAmount = 0;
            decimal sumNetSales = 0;

            int noStall = 0;
            int startRow = row;
            if (lstData != null && lstData.Any() && _lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                for (int i = 0; i < lstStore.Count; i++)
                {
                    // Business day in store
                    var lstBusDayInStore = _lstBusDayAllStore.Where(w => w.StoreId == lstStore[i].Id).ToList();
                    //if (lstBusDayInStore != null && lstBusDayInStore.Any())
                    //{
                    noStall++;
                    if (lstBusDayInStore != null && lstBusDayInStore.Any())
                    {
                        minBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).FirstOrDefault();
                        maxBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).LastOrDefault();
                        // Data for stall #14
                        if (lstStore[i].Id == Commons.Stall14StoreId)
                        {
                            minBDStall14 = minBD;
                            maxBDStall14 = maxBD;
                        }
                    }

                    // Get value from setting of store
                    // Get list data depend on business day
                    var lstDataInStore = lstData.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();

                    var lstPaymentsInStore = lstPayments.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();

                    var lstItemsNoIncludeSaleInStore = lstItemSales.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom && w.IsIncludeSale == false).ToList();

                    // Morning Sales
                    morningSales = lstDataInStore.Where(w =>
                    w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd
                    && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart).Sum(s => (decimal)s.ReceiptTotal);

                    // Mid-day Sales
                    midDaySales = lstDataInStore.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd
                && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart).Sum(s => (decimal)s.ReceiptTotal);
                    // Full-day Sales
                    fullDaySales = lstDataInStore.Sum(s => (decimal)s.ReceiptTotal);

                    if (_isTaxInclude)
                    {
                        //Sub tax
                        //morningSales -= lstDataInStore.Where(w =>
                        //                w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd
                        //                && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart).Sum(s => (decimal)s.GST);

                        //midDaySales -= lstDataInStore.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd
                        //                 && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart).Sum(s => (decimal)s.GST);


                        //fullDaySales -= lstDataInStore.Sum(s => (decimal)s.GST);

                        if (lstStore[i].Id == Commons.Stall14StoreId)
                        {
                            var liquorSaleMorning = lstItemSales.Where(w => w.StoreId == Commons.Stall14StoreId
                            && w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd
                            && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart
                            && w.GLAccountCode == Commons.Stall14GLAccountCode2)
                            .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                            var liquorSaleMidDay = lstItemSales.Where(w => w.StoreId == Commons.Stall14StoreId
                           && w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd
                            && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart
                           && w.GLAccountCode == Commons.Stall14GLAccountCode2)
                           .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                            var liquorSaleFullDay = lstItemSales.Where(w => w.StoreId == Commons.Stall14StoreId
                          && w.CreatedDate <= maxBDStall14.DateTo && w.CreatedDate >= minBDStall14.DateFrom
                          && w.GLAccountCode == Commons.Stall14GLAccountCode2)
                          .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);


                            morningSales -= liquorSaleMorning;
                            midDaySales -= liquorSaleMidDay;
                            fullDaySales -= liquorSaleFullDay;
                        }
                    }

                    // Set value for a store
                    column = 1;
                    // Column Store No.
                    ws.Range(row, column, row, column++).Value = noStall;
                    // Column Store name
                    ws.Range(row, column, row, column++).Value = lstStore[i].Name;
                    // Column Morning Sales
                    ws.Range(row, column, row, column++).Value = morningSales;
                    sumMorningSales += morningSales;
                    // Column Mid-day Sales
                    ws.Range(row, column, row, column++).Value = midDaySales;
                    sumMidDaySales += midDaySales;
                    // Column Full-day Sales
                    ws.Range(row, column, row, column++).Value = fullDaySales;
                    sumFullDaySales += fullDaySales;
                    // Group vouchers
                    var listVouncherIds = listInfoVounchers.Select(s => s.PaymentId).ToList();
                    var lstPaymentOfVounchers = lstPaymentsInStore.Where(w => listVouncherIds.Contains(w.PaymentId)).ToList();
                    // Column Total Vouchers Amount
                    vouchersAmount = lstPaymentOfVounchers.Sum(s => (decimal)s.Amount);
                    ws.Range(row, column, row, column++).Value = vouchersAmount;
                    sumVouchersAmount += vouchersAmount;
                    foreach (var vouncher in listInfoVounchers)
                    {
                        countVoucher = 0;
                        voucherAmount = 0;
                        var lstInfo = lstPaymentOfVounchers.Where(w => w.PaymentId == vouncher.PaymentId).ToList();
                        if (lstInfo != null && lstInfo.Any())
                        {
                            countVoucher = lstInfo.Count;
                            voucherAmount = lstInfo.Sum(s => (decimal)s.Amount);
                        }
                        // Set value for column
                        ws.Range(row, column, row, column++).Value = countVoucher;
                        vouncher.Count += countVoucher;
                        vouncher.Total += voucherAmount;

                    }
                    // Group cards
                    var listCardIds = listInfoCards.Select(s => s.PaymentId).ToList();
                    var lstPaymentOfCards = lstPaymentsInStore.Where(w => listCardIds.Contains(w.PaymentId)).ToList();
                    foreach (var card in listInfoCards)
                    {
                        cardAmount = 0;
                        var lstInfo = lstPaymentOfCards.Where(w => w.PaymentId == card.PaymentId).ToList();
                        if (lstInfo != null && lstInfo.Any())
                        {
                            cardAmount = lstInfo.Sum(s => (decimal)s.Amount);
                        }
                        // Set value
                        ws.Range(row, column, row, column++).Value = cardAmount;
                        card.Total += cardAmount;
                    }
                    // Column Cash
                    cashAmount = 0;
                    cashAmount = lstPaymentsInStore.Where(w => lstPaymentMethodCashId.Contains(w.PaymentId)).Sum(s => (decimal)s.Amount) - lstDataInStore.Sum(s => (decimal)s.Refund);
                    ws.Range(row, column, row, column).Value = cashAmount;
                    ws.Range(row, column, row, column++).Style.NumberFormat.Format = "#,##0.00";
                    sumCashAmount += cashAmount;
                    // Column Tax (Inc)
                    taxAmount = 0;
                    taxAmount = lstDataInStore.Sum(s => (decimal)s.GST);
                    ws.Range(row, column, row, column).Value = taxAmount;
                    ws.Range(row, column, row++, column).Style.NumberFormat.Format = "#,##0.00";
                    sumTaxAmount += taxAmount;
                    // rouding
                    var roudingAmount = lstDataInStore.Sum(s => (decimal)s.Rounding);
                    // Net sales
                    netSales = lstDataInStore.Sum(s => (decimal)s.NetSales) + roudingAmount;

                    var notIncludeOnSale = lstItemsNoIncludeSaleInStore.Sum(s => (decimal)s.TotalAmount - (decimal)s.PromotionAmount - (decimal)s.TotalDiscount);
                    if (_isTaxInclude)
                        notIncludeOnSale = lstItemsNoIncludeSaleInStore.Sum(s => (decimal)s.TotalAmount - (decimal)s.PromotionAmount - (decimal)s.TotalDiscount - (decimal)s.Tax);

                    //GC
                    var payGC = lstPaymentsInStore.Where(p => lstGC.Contains(p.PaymentId)).Sum(p => (decimal)p.Amount);

                    sumNetSales += netSales - notIncludeOnSale - payGC;
                    //}
                }
            }

            // Set value total
            column = 1;
            ws.Range(row, column, row, column + 1).Merge().Value = "TOTAL";
            column += 2;
            // Total morning sales
            ws.Range(row, column, row, column++).Value = sumMorningSales;
            // Total mid-day sales
            ws.Range(row, column, row, column++).Value = sumMidDaySales;
            // Total full-day sales
            ws.Range(row, column, row, column++).Value = sumFullDaySales;
            // Total vouchers amount
            ws.Range(row, column, row, column++).Value = sumVouchersAmount;
            // Total for each in vouchers
            foreach (var voucher in listInfoVounchers)
            {
                ws.Range(row, column, row, column++).Value = voucher.Count;
            }
            // Total for each in cards
            foreach (var card in listInfoCards)
            {
                ws.Range(row, column, row, column++).Value = card.Total;
            }
            // Total cash
            ws.Range(row, column, row, column++).Value = sumCashAmount;
            // Total tax
            ws.Range(row, column, row, column).Value = sumTaxAmount;
            // Format column money
            ws.Range(startRow, 3, row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRow, startColCard, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";
            // Format row total
            ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
            ws.Range(startRow, 1, row, maxColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRow, 1, row, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            // Format column name
            ws.Range(startRow, 2, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(row, 1, row++, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Set value Net Sales
            ws.Range(row, 1, row, 1).Value = "NET SALES";
            ws.Range(row, 2, row, 2).Value = sumNetSales;
            ws.Range(row, 2, row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 2, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            // Format row Net sales
            ws.Range(row, 1, row, 2).Style.Font.SetBold(true);
            ws.Range(row, 1, row, 2).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 1, row, 2).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Set value Summary
            row += 2;
            ws.Range(row, 1, row, 3).Merge().Value = "SUMMARY";
            ws.Range(row, 1, row, 3).Style.Font.SetBold(true);
            ws.Range(row, 1, row++, 3).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 2, row, 2).Value = "Vouchers Count";
            ws.Range(row, 3, row, 3).Value = "Amount";
            ws.Range(row, 2, row, 3).Style.Font.SetBold(true);
            ws.Range(row, 2, row++, 3).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            int startRowSummary = row;
            // Vouchers
            foreach (var voucher in listInfoVounchers)
            {
                ws.Range(row, 1, row, 1).Value = voucher.PaymentName;
                ws.Range(row, 2, row, 2).Value = voucher.Count;
                ws.Range(row, 3, row++, 3).Value = voucher.Total;
            }
            // Cards
            foreach (var card in listInfoCards)
            {
                ws.Range(row, 1, row, 1).Value = card.PaymentName;
                ws.Range(row, 3, row++, 3).Value = card.Total;
            }
            // Cash
            ws.Range(row, 1, row, 1).Value = "Cash";
            ws.Range(row, 3, row, 3).Value = sumCashAmount;
            // Format summary
            ws.Range(startRowSummary, 1, row, 1).Style.Font.SetBold(true);
            ws.Range(startRowSummary, 1, row, 1).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(startRowSummary, 3, row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRowSummary - 1, 1, row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRowSummary - 2, 1, row, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRowSummary - 2, 1, row, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Data for Stall #14
            if (model.ListStores.Contains(Commons.Stall14StoreId))
            {
                decimal totalStall14GLAccountCode1 = 0;
                decimal totalStall14GLAccountCode2 = 0;
                if (maxBDStall14 != null && minBDStall14 != null)
                {
                    List<string> lstGLAccountCode = new List<string>();
                    lstGLAccountCode.Add(Commons.Stall14GLAccountCode1);
                    lstGLAccountCode.Add(Commons.Stall14GLAccountCode2);
                    var lstDataGLAccountCode = lstItemSales.Where(w => w.StoreId == Commons.Stall14StoreId && w.CreatedDate <= maxBDStall14.DateTo && w.CreatedDate >= minBDStall14.DateFrom && lstGLAccountCode.Contains(w.GLAccountCode)).ToList();
                    if (lstDataGLAccountCode != null && lstDataGLAccountCode.Any())
                    {
                        var dataStall14GLAccountCode1 = lstDataGLAccountCode.Where(w => w.GLAccountCode == Commons.Stall14GLAccountCode1).ToList();
                        //if (_isTaxInclude)
                        //    totalStall14GLAccountCode1 = dataStall14GLAccountCode1.Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount - (decimal)s.Tax);
                        //else
                        totalStall14GLAccountCode1 = dataStall14GLAccountCode1.Sum(s => (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                        var dataStall14GLAccountCode2 = lstDataGLAccountCode.Where(w => w.GLAccountCode == Commons.Stall14GLAccountCode2).ToList();
                        totalStall14GLAccountCode2 = dataStall14GLAccountCode2.Sum(s => (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);
                    }
                }

                row = startRowSummary - 2;
                ws.Range(row, 5, row, 7).Merge().Value = "For " + Commons.Stall14StoreName;
                ws.Range(row, 5, row, 7).Style.Font.SetBold(true);
                ws.Range(row, 5, row++, 7).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                ws.Range(row, 5, row + 1, 5).Merge();
                ws.Range(row, 6, row, 6).Value = "Drink Stall";
                ws.Range(row, 7, row, 7).Value = "FJ Cards";
                ws.Range(row, 6, row, 7).Style.Font.SetBold(true);
                ws.Range(row, 6, row++, 7).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                ws.Range(row, 6, row, 6).Value = Commons.Stall14GLAccountCode1;
                ws.Range(row, 7, row++, 7).Value = Commons.Stall14GLAccountCode2;
                ws.Range(row, 5, row, 5).Value = "Total Amount";
                ws.Range(row, 5, row, 5).Style.Font.SetBold(true);
                ws.Range(row, 5, row, 5).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                // Amount of Stall14GLAccountCode1
                ws.Range(row, 6, row, 6).Value = totalStall14GLAccountCode1;
                // Amount of Stall14GLAccountCode2
                ws.Range(row, 7, row, 7).Value = totalStall14GLAccountCode2;
                ws.Range(row, 6, row, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Range(row - 2, 6, row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row - 3, 5, row, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row - 3, 5, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Set Width for some column 
            List<int> lstWidCol = new List<int>() { 25, 40, 20, 20, 20, 20, 15, 20, 20, 20, 20, 20 };

            for (int i = 0; i < lstWidCol.Count; i++)
            {
                ws.Column(i + 1).Width = lstWidCol[i];
            }

            return wb;
        }
        public XLWorkbook ExportExcel(BaseReportModel model, List<StoreModels> lstStore, MerchantConfigApiModels pOSMerchantConfig)
        {
            string sheetName = "FJ_Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dToFilter = model.ToDate;
            // List business day in all store
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                // Format header report
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
            List<FJDailySalesReportModels> lstData = GetDataReceiptItems(model);
            List<PaymentModels> lstPayments = GetDataPaymentItems(model);
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            List<ItemizedSalesAnalysisReportModels> lstItemSales = _itemizedSalesAnalysisFactory.GetItemsForDailySalesReports(model);
            bool _isTaxInclude = _baseFactory.IsTaxInclude(model.ListStores.FirstOrDefault());

            var lstStoreSettings = GetListStoreSetting(model);
            //GC.............................................
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            //string sheetName = "FJ_Daily_Sales_Report";
            //var wb = new XLWorkbook();
            //var ws = wb.Worksheets.Add(sheetName);
            int maxColumn = 0;

            int row = 5;
            int column = 1;

            // Set title column report
            // Stall NO.
            ws.Range(row, column, row + 1, column++).Merge().Value = "Stall NO.";
            // Stall Name
            ws.Range(row, column, row + 1, column++).Merge().Value = "Stall Name";
            // Group Sales
            ws.Range(row, column, row, column + 2).Merge().Value = "Sales";
            ws.Range(row + 1, column, row + 1, column++).Value = "Morning Sales";
            ws.Range(row + 1, column, row + 1, column++).Value = "Mid-day Sales";
            ws.Range(row + 1, column, row + 1, column++).Value = "Full-day Sales";
            // Total Vouchers Amount
            ws.Range(row, column, row + 1, column++).Merge().Value = "Total Vouchers Amount";

            // Group Payment methods
            // List information of payments: cash (Commons.EPaymentCode.Cash)
            var lstPaymentMethodCashId = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash).Select(s => s.Id).ToList();

            var lstPaymentMethodExCash = lstPaymentMethod.Where(ww => ww.Code != (int)Commons.EPaymentCode.Cash).OrderBy(o => o.Name).ToList();

            // List information of payments: Vouchers (FixedPayAmount > 0)
            List<FJDailySalesReportViewModels> listInfoVounchers = new List<FJDailySalesReportViewModels>();

            var listChildsVoucher = lstPaymentMethodExCash.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.FixedPayAmount > 0).ToList();

            var lstVoucherParentIds = listChildsVoucher.Select(s => s.ParentId).Distinct().ToList();

            var lstVoucherParents = lstPaymentMethodExCash.Where(ww => lstVoucherParentIds.Contains(ww.Id)).OrderBy(o => o.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

            foreach (var paymentParent in lstVoucherParents)
            {
                var listChilds = listChildsVoucher.Where(ww => ww.ParentId == paymentParent.Id).OrderBy(ww => ww.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

                if (listChilds != null && listChilds.Any())
                {
                    ws.Range(row, column, row, (column + listChilds.Count - 1)).Merge().Value = paymentParent.Name;

                    foreach (var paymentChild in listChilds)
                    {
                        ws.Cell(row + 1, column++).Value = paymentChild.Name;
                        listInfoVounchers.Add(
                            new FJDailySalesReportViewModels
                            {
                                PaymentId = paymentChild.Id,
                                PaymentName = paymentChild.Name
                            });
                    }
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().Value = paymentParent.Name;
                    listInfoVounchers.Add(
                        new FJDailySalesReportViewModels
                        {
                            PaymentId = paymentParent.Id,
                            PaymentName = paymentParent.Name
                        });
                }
            }

            // List information of payments: card 
            int startColCard = column;
            List<FJDailySalesReportViewModels> listInfoCards = new List<FJDailySalesReportViewModels>();

            var lstVourcherIds = new List<string>();
            if (lstVoucherParents != null && lstVoucherParents.Any())
                lstVourcherIds = lstVoucherParents.Select(ss => ss.Id).ToList();

            var listChildsCard = lstPaymentMethodExCash.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.FixedPayAmount == 0).ToList();
            var lstCardParentIds = listChildsCard.Select(s => s.ParentId).Distinct().ToList();

            var lstCardParents = lstPaymentMethodExCash.Where(ww => (lstCardParentIds.Contains(ww.Id)
            || (string.IsNullOrEmpty(ww.ParentId) && (ww.ListChilds == null || ww.ListChilds.Count == 0)))
                    && !lstVourcherIds.Contains(ww.Id)
                    ).OrderBy(o => o.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

            foreach (var paymentParent in lstCardParents)
            {
                var listChilds = lstPaymentMethod.Where(ww => ww.ParentId == paymentParent.Id).OrderBy(ww => ww.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

                if (listChilds != null && listChilds.Any())
                {
                    ws.Range(row, column, row, (column + listChilds.Count - 1)).Merge().Value = paymentParent.Name;

                    foreach (var paymentChild in listChilds)
                    {
                        ws.Cell(row + 1, column++).Value = paymentChild.Name;
                        listInfoCards.Add(
                            new FJDailySalesReportViewModels
                            {
                                PaymentId = paymentChild.Id,
                                PaymentName = paymentChild.Name,
                                Count = 0,
                                Total = 0
                            });
                    }
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().Value = paymentParent.Name;
                    listInfoCards.Add(
                        new FJDailySalesReportViewModels
                        {
                            PaymentId = paymentParent.Id,
                            PaymentName = paymentParent.Name,
                            Count = 0,
                            Total = 0
                        });
                }
            }

            // Cash
            ws.Range(row, column, row + 1, column++).Merge().Value = "Cash";
            // Tax
            if (_isTaxInclude)
            {
                ws.Range(row, column, row + 1, column).Merge().Value = "Tax (Inc)";
            }
            else
                ws.Range(row, column, row + 1, column).Merge().Value = "Tax (Add)";

            if (maxColumn < column)
                maxColumn = column;

            // Set header report
            CreateReportHeaderNew(ws, maxColumn, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());

            // Format title column report
            ws.Range(row, 1, row + 1, maxColumn).Style.Font.SetBold(true);
            ws.Range(row, 1, row + 1, maxColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.SetWrapText(true);
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(row, 1, row + 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row + 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row += 2;

            //// List business day in all store
            //var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            BusinessDayDisplayModels minBD = new BusinessDayDisplayModels();
            BusinessDayDisplayModels maxBD = new BusinessDayDisplayModels();
            //BusinessDayDisplayModels minBDStall14 = null;
            //BusinessDayDisplayModels maxBDStall14 = null;

            // For a store
            decimal morningSales = 0;
            decimal midDaySales = 0;
            decimal fullDaySales = 0;
            decimal vouchersAmount = 0;
            int countVoucher = 0;
            decimal voucherAmount = 0;
            decimal cardAmount = 0;
            decimal cashAmount = 0;
            decimal taxAmount = 0;
            decimal netSales = 0;

            // For all store
            decimal sumMorningSales = 0;
            decimal sumMidDaySales = 0;
            decimal sumFullDaySales = 0;
            decimal sumVouchersAmount = 0;
            decimal sumCashAmount = 0;
            decimal sumTaxAmount = 0;
            decimal sumNetSales = 0;

            int noStall = 0;
            int startRow = row;
            int seq = 1;
            if (lstData != null && lstData.Any() && _lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                for (int i = 0; i < lstStore.Count; i++)
                {
                    // Business day in store
                    var lstBusDayInStore = _lstBusDayAllStore.Where(w => w.StoreId == lstStore[i].Id).ToList();
                    //if (lstBusDayInStore != null && lstBusDayInStore.Any())
                    //{
                    noStall++;
                    if (lstBusDayInStore != null && lstBusDayInStore.Any())
                    {
                        minBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).FirstOrDefault();
                        maxBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).LastOrDefault();

                        //update seq for storeSetting
                        if (lstStoreSettings != null && lstStoreSettings.Any())
                        {
                            var storeSetting = lstStoreSettings.Where(ww => ww.StoreId == lstStore[i].Id).FirstOrDefault();
                            if (storeSetting != null)
                            {
                                storeSetting.Seq = seq;
                                seq++;
                            }
                        }

                    }

                    // Get value from setting of store
                    // Get list data depend on business day
                    var lstDataInStore = lstData.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();

                    var lstPaymentsInStore = lstPayments.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();

                    var lstItemsNoIncludeSaleInStore = lstItemSales.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom && w.IsIncludeSale == false).ToList();

                    // Morning Sales
                    morningSales = lstDataInStore.Where(w =>
                    w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd
                    && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart).Sum(s => (decimal)s.ReceiptTotal);

                    // Mid-day Sales
                    midDaySales = lstDataInStore.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd
                && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart).Sum(s => (decimal)s.ReceiptTotal);
                    // Full-day Sales
                    fullDaySales = lstDataInStore.Sum(s => (decimal)s.ReceiptTotal);

                    if (_isTaxInclude)
                    {
                        var obj = lstStoreSettings.Where(ww => ww.StoreId == lstStore[i].Id).FirstOrDefault();
                        if (obj != null)
                        {
                            if (obj.GLAccountCodes.Count() > 1)
                            {
                                var lstGLAccountCodes = obj.GLAccountCodes.ToList();
                                lstGLAccountCodes.RemoveAt(0);

                                var liquorSaleMorning = lstItemSales.Where(w => w.StoreId == lstStore[i].Id
                               && w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd
                               && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart
                               && lstGLAccountCodes.Contains(w.GLAccountCode))
                               .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                                var liquorSaleMidDay = lstItemSales.Where(w => w.StoreId == lstStore[i].Id
                               && w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd
                                && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart
                               && lstGLAccountCodes.Contains(w.GLAccountCode))
                               .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                                var liquorSaleFullDay = lstItemSales.Where(w => w.StoreId == lstStore[i].Id
                              && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom
                              && lstGLAccountCodes.Contains(w.GLAccountCode))
                              .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                                morningSales -= liquorSaleMorning;
                                midDaySales -= liquorSaleMidDay;
                                fullDaySales -= liquorSaleFullDay;
                            }
                        }
                    }
                    // Set value for a store
                    column = 1;
                    // Column Store No.
                    ws.Range(row, column, row, column++).Value = noStall;
                    // Column Store name
                    ws.Range(row, column, row, column++).Value = lstStore[i].Name;
                    // Column Morning Sales
                    ws.Range(row, column, row, column++).Value = morningSales;
                    sumMorningSales += morningSales;
                    // Column Mid-day Sales
                    ws.Range(row, column, row, column++).Value = midDaySales;
                    sumMidDaySales += midDaySales;
                    // Column Full-day Sales
                    ws.Range(row, column, row, column++).Value = fullDaySales;
                    sumFullDaySales += fullDaySales;
                    // Group vouchers
                    var listVouncherIds = listInfoVounchers.Select(s => s.PaymentId).ToList();
                    var lstPaymentOfVounchers = lstPaymentsInStore.Where(w => listVouncherIds.Contains(w.PaymentId)).ToList();
                    // Column Total Vouchers Amount
                    vouchersAmount = lstPaymentOfVounchers.Sum(s => (decimal)s.Amount);
                    ws.Range(row, column, row, column++).Value = vouchersAmount;
                    sumVouchersAmount += vouchersAmount;
                    foreach (var vouncher in listInfoVounchers)
                    {
                        countVoucher = 0;
                        voucherAmount = 0;
                        var lstInfo = lstPaymentOfVounchers.Where(w => w.PaymentId == vouncher.PaymentId).ToList();
                        if (lstInfo != null && lstInfo.Any())
                        {
                            countVoucher = lstInfo.Count;
                            voucherAmount = lstInfo.Sum(s => (decimal)s.Amount);
                        }
                        // Set value for column
                        ws.Range(row, column, row, column++).Value = countVoucher;
                        vouncher.Count += countVoucher;
                        vouncher.Total += voucherAmount;

                    }
                    // Group cards
                    var listCardIds = listInfoCards.Select(s => s.PaymentId).ToList();
                    var lstPaymentOfCards = lstPaymentsInStore.Where(w => listCardIds.Contains(w.PaymentId)).ToList();
                    foreach (var card in listInfoCards)
                    {
                        cardAmount = 0;
                        var lstInfo = lstPaymentOfCards.Where(w => w.PaymentId == card.PaymentId).ToList();
                        if (lstInfo != null && lstInfo.Any())
                        {
                            cardAmount = lstInfo.Sum(s => (decimal)s.Amount);
                        }
                        // Set value
                        ws.Range(row, column, row, column++).Value = cardAmount;
                        card.Total += cardAmount;
                    }
                    // Column Cash
                    cashAmount = 0;
                    cashAmount = lstPaymentsInStore.Where(w => lstPaymentMethodCashId.Contains(w.PaymentId)).Sum(s => (decimal)s.Amount) - lstDataInStore.Sum(s => (decimal)s.Refund);
                    ws.Range(row, column, row, column).Value = cashAmount;
                    ws.Range(row, column, row, column++).Style.NumberFormat.Format = "#,##0.00";
                    sumCashAmount += cashAmount;
                    // Column Tax (Inc)
                    taxAmount = 0;
                    taxAmount = lstDataInStore.Sum(s => (decimal)s.GST);
                    ws.Range(row, column, row, column).Value = taxAmount;
                    ws.Range(row, column, row++, column).Style.NumberFormat.Format = "#,##0.00";
                    sumTaxAmount += taxAmount;
                    // rouding
                    var roudingAmount = lstDataInStore.Sum(s => (decimal)s.Rounding);
                    // Net sales
                    netSales = lstDataInStore.Sum(s => (decimal)s.NetSales) + roudingAmount;

                    var notIncludeOnSale = lstItemsNoIncludeSaleInStore.Sum(s => (decimal)s.TotalAmount - (decimal)s.PromotionAmount - (decimal)s.TotalDiscount);
                    if (_isTaxInclude)
                        notIncludeOnSale = lstItemsNoIncludeSaleInStore.Sum(s => (decimal)s.TotalAmount - (decimal)s.PromotionAmount - (decimal)s.TotalDiscount - (decimal)s.Tax);

                    //GC
                    var payGC = lstPaymentsInStore.Where(p => lstGC.Contains(p.PaymentId)).Sum(p => (decimal)p.Amount);

                    sumNetSales += netSales - notIncludeOnSale - payGC;
                    //}
                }
            }

            // Set value total
            column = 1;
            ws.Range(row, column, row, column + 1).Merge().Value = "TOTAL";
            column += 2;
            // Total morning sales
            ws.Range(row, column, row, column++).Value = sumMorningSales;
            // Total mid-day sales
            ws.Range(row, column, row, column++).Value = sumMidDaySales;
            // Total full-day sales
            ws.Range(row, column, row, column++).Value = sumFullDaySales;
            // Total vouchers amount
            ws.Range(row, column, row, column++).Value = sumVouchersAmount;
            // Total for each in vouchers
            foreach (var voucher in listInfoVounchers)
            {
                ws.Range(row, column, row, column++).Value = voucher.Count;
            }
            // Total for each in cards
            foreach (var card in listInfoCards)
            {
                ws.Range(row, column, row, column++).Value = card.Total;
            }
            // Total cash
            ws.Range(row, column, row, column++).Value = sumCashAmount;
            // Total tax
            ws.Range(row, column, row, column).Value = sumTaxAmount;
            // Format column money
            ws.Range(startRow, 3, row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRow, startColCard, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";
            // Format row total
            ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
            ws.Range(startRow, 1, row, maxColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRow, 1, row, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            // Format column name
            ws.Range(startRow, 2, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(row, 1, row++, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Set value Net Sales
            ws.Range(row, 1, row, 1).Value = "NET SALES";
            ws.Range(row, 2, row, 2).Value = sumNetSales;
            ws.Range(row, 2, row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 2, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            // Format row Net sales
            ws.Range(row, 1, row, 2).Style.Font.SetBold(true);
            ws.Range(row, 1, row, 2).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 1, row, 2).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Set value Summary
            row += 2;
            ws.Range(row, 1, row, 3).Merge().Value = "SUMMARY";
            ws.Range(row, 1, row, 3).Style.Font.SetBold(true);
            ws.Range(row, 1, row++, 3).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 2, row, 2).Value = "Vouchers Count";
            ws.Range(row, 3, row, 3).Value = "Amount";
            ws.Range(row, 2, row, 3).Style.Font.SetBold(true);
            ws.Range(row, 2, row++, 3).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            int startRowSummary = row;
            // Vouchers
            foreach (var voucher in listInfoVounchers)
            {
                ws.Range(row, 1, row, 1).Value = voucher.PaymentName;
                ws.Range(row, 2, row, 2).Value = voucher.Count;
                ws.Range(row, 3, row++, 3).Value = voucher.Total;
            }
            // Cards
            foreach (var card in listInfoCards)
            {
                ws.Range(row, 1, row, 1).Value = card.PaymentName;
                ws.Range(row, 3, row++, 3).Value = card.Total;
            }
            // Cash
            ws.Range(row, 1, row, 1).Value = "Cash";
            ws.Range(row, 3, row, 3).Value = sumCashAmount;
            // Format summary
            ws.Range(startRowSummary, 1, row, 1).Style.Font.SetBold(true);
            ws.Range(startRowSummary, 1, row, 1).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(startRowSummary, 3, row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRowSummary - 1, 1, row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRowSummary - 2, 1, row, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRowSummary - 2, 1, row, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Data for Stall #14 & extend
            if (lstStoreSettings != null && lstStoreSettings.Any())
            {
                lstStoreSettings = lstStoreSettings.OrderBy(oo => oo.Seq).ToList();

                row = startRowSummary - 2;
                foreach (var item in lstStoreSettings)
                {
                    var storeName = lstStore.Where(ww => ww.Id == item.StoreId).Select(ss => ss.Name).FirstOrDefault();
                    minBD = null; maxBD = null;
                    var lstBusDayInStore = _lstBusDayAllStore.Where(w => w.StoreId == item.StoreId).ToList();

                    if (lstBusDayInStore != null && lstBusDayInStore.Any())
                    {
                        minBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).FirstOrDefault();
                        maxBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).LastOrDefault();

                        var lstDataGLAccountCode = lstItemSales.Where(w => w.StoreId == item.StoreId
                        && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom
                        && item.GLAccountCodes.Contains(w.GLAccountCode)).ToList();

                        if (lstDataGLAccountCode != null && lstDataGLAccountCode.Any())
                        {

                            int lstCellColumn = 6;
                            ws.Range(row, 5, row, 7).Merge().Value = "For " + storeName;
                            ws.Range(row, 5, row, 7).Style.Font.SetBold(true);
                            //ws.Range(row, 5, row++, 7).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            int storeRow = row;
                            int categoryRow = 0;
                            row++;
                            ws.Range(row, 5, row + 1, 5).Merge();
                            ws.Range(row, 6, row, 6).Value = "Drink Stall";
                          
                            if (item.GLAccountCodes.Count() >= 1)
                            {
                                row++;
                                categoryRow = row;
                                ws.Range(row, 6, row, 6).Value = item.GLAccountCodes[0];
                               
                                ws.Range(row+1, 6, row+1, 6).Value = lstDataGLAccountCode.Where(w => w.GLAccountCode == item.GLAccountCodes[0])
                                    .Sum(s => (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                                var lstGLAccountCode = item.GLAccountCodes.ToList();
                                lstGLAccountCode.Remove(item.GLAccountCodes[0]);
                                
                                if (lstGLAccountCode != null && lstGLAccountCode.Any())
                                {
                                    for (int i = 0; i < lstGLAccountCode.Count; i++)
                                    {
                                        lstCellColumn++;
                                        ws.Range(categoryRow, lstCellColumn, categoryRow, lstCellColumn).Value = lstGLAccountCode[i];
                                        ws.Range(row + 1, 7, row + 1, 7).Value = lstDataGLAccountCode.Where(w => w.GLAccountCode == lstGLAccountCode[i]).Sum(s => (decimal)s.TotalAmount
                                            - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);
                                    }
                                    ws.Range(row-1, lstCellColumn, row-1, lstCellColumn).Merge().Value = "FJ Cards";

                                }

                            }
                            row++;
                            ws.Range(row, 5, row, 5).Value = "Total Amount";
                            ws.Range(row, 5, row, 5).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row, 6, row, lstCellColumn).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range(row, 5, row, 5).Style.Font.SetBold(true);
                            ws.Range(storeRow, 5, storeRow, lstCellColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row, 6, row, lstCellColumn).Style.Font.SetBold(true);
                            ws.Range(categoryRow-1, 6, categoryRow-1, lstCellColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row - 2, 6, row, lstCellColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(row - 3, 5, row, lstCellColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(row - 3, 5, row, lstCellColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            row += 2;
                        }
                    }
                }

            }

            // Set Width for some column 
            List<int> lstWidCol = new List<int>() { 25, 40, 20, 20, 20, 20, 15, 20, 20, 20, 20, 20 };

            for (int i = 0; i < lstWidCol.Count; i++)
            {
                ws.Column(i + 1).Width = lstWidCol[i];
            }

            return wb;
        }

        #region Report with filter data by time & Credit Note info (refund gift card), updated 04102018
        public List<FJDailySalesReportModels> GetDataReceiptItems_WithCreditNote(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_DailySalesReport.Where(ww => model.ListStores.Contains(ww.StoreId)
                                     && (ww.CreatedDate >= model.FromDate && ww.CreatedDate <= model.ToDate)
                                     && ww.Mode == model.Mode)
                                     .Select(ss => new FJDailySalesReportModels()
                                     {
                                         StoreId = ss.StoreId,
                                         CreatedDate = ss.CreatedDate,
                                         ReceiptTotal = ss.ReceiptTotal,      // Total R_Receipt
                                         Discount = ss.Discount,
                                         ServiceCharge = ss.ServiceCharge,
                                         GST = ss.GST,
                                         Rounding = ss.Rounding,
                                         Refund = ss.Refund,
                                         NetSales = ss.NetSales,
                                         CreditNoteNo = ss.CreditNoteNo,
                                         ReceiptId = ss.OrderId
                                     }).ToList();

                return lstData;
            }
        }

        public XLWorkbook ExportExcel_WithCreditNote(BaseReportModel model, List<StoreModels> lstStore, MerchantConfigApiModels pOSMerchantConfig)
        {
            string sheetName = "FJ_Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            // List business day in all store
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                // Format header report
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

            List<FJDailySalesReportDataModels> lstData = new List<FJDailySalesReportDataModels>();
            List<PaymentDataModels> lstPayments = new List<PaymentDataModels>();
            List<ItemizedSalesAnalysisReportDataModels> lstItemSales = new List<ItemizedSalesAnalysisReportDataModels>();
            var _lstRefunds = new List<RefundDataReportDTO>();
            //Stopwatch _stopwatch = new Stopwatch();
            // Get data sale
            using (var db = new NuWebContext())
            {
                //_stopwatch.Start();

                var request = new BaseReportDataModel() { ListStores = model.ListStores, FromDate = model.FromDate, ToDate = model.ToDate, Mode = model.Mode };
                lstData = db.GetDataReceipt_WithCreditNoteForFJ(request);
                lstPayments = db.GetDataPaymentItems(request);
                lstItemSales = db.GetItemsNoIncludeSale_New(request);
                _lstRefunds = db.GetListRefundWithoutDetailsByReceiptId(request);

                //_stopwatch.Stop();
               // Debug.WriteLine("FJ Get data: ", _stopwatch.Elapsed);
            }
            // List<FJDailySalesReportModels> lstData = GetDataReceiptItems_WithCreditNote(model);
            if (lstData != null && lstData.Any())
            {
                switch (model.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();

                        if (lstData == null || !lstData.Any())
                        {
                            // Set header report
                            CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            return wb;
                        }
                        break;
                    case (int)Commons.EFilterType.Days:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();

                        if (lstData == null || !lstData.Any())
                        {
                            // Set header report
                            CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            return wb;
                        }
                        break;
                }
            }
            else
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                // Format header report
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            // Get data payment
            //List<PaymentModels> lstPayments = GetDataPaymentItems(model);
            if (lstPayments != null && lstPayments.Any())
            {
                switch (model.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        lstPayments = lstPayments.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        lstPayments = lstPayments.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();
                        break;
                }
            }

            // Get list no include sale && GLAccountCode
           // var lstReceiptId = lstData.Select(s => s.ReceiptId).Distinct().ToList();

            //List<ItemizedSalesAnalysisReportModels> lstItemSales = _itemizedSalesAnalysisFactory.GetItemsNoIncludeSale(lstReceiptId, model.ListStores, model.Mode);

            // Get lst refund by GC
            RefundFactory _refundFactory = new RefundFactory();
            //var _lstRefunds = _refundFactory.GetListRefundWithoutDetailsByReceiptId(model.ListStores, lstReceiptId);

            //bool _isTaxInclude = _baseFactory.IsTaxInclude(model.ListStores.FirstOrDefault());
            bool _isTaxInclude = lstStore.Select(ss => ss.IsIncludeTax).FirstOrDefault();
            var lstStoreSettings = GetListStoreSetting(model);

            int maxColumn = 0;

            int row = 5;
            int column = 1;

            // Set title column report
            // Stall NO.
            ws.Range(row, column, row + 1, column++).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Stall NO.");
            // Stall Name
            ws.Range(row, column, row + 1, column++).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Stall Name");
            // Group Sales
            ws.Range(row, column, row, column + 2).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales");
            ws.Range(row + 1, column, row + 1, column++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Morning Sales");
            ws.Range(row + 1, column, row + 1, column++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Mid-day Sales");
            ws.Range(row + 1, column, row + 1, column++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Full-day Sales");
            // Total Vouchers Amount
            ws.Range(row, column, row + 1, column++).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Vouchers Amount");

            // Get payment mothod info
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });

            // GC
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            // Group Payment methods
            // List information of payments: cash (Commons.EPaymentCode.Cash)
            var lstPaymentMethodCashId = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash).Select(s => s.Id).ToList();

            var lstPaymentMethodExCash = lstPaymentMethod.Where(ww => ww.Code != (int)Commons.EPaymentCode.Cash).OrderBy(o => o.Name).ToList();

            // List information of payments: Vouchers (FixedPayAmount > 0)
            // ALl payment: FixedPayAmount > 0, voucher parent maybe FixedPayAmount = 0
            List<FJDailySalesReportViewModels> listInfoVounchers = new List<FJDailySalesReportViewModels>();
            var lstAllVoucher = lstPaymentMethodExCash.Where(ww => ww.FixedPayAmount > 0).ToList();
            var lstVoucherParentId = lstAllVoucher.Where(w => !string.IsNullOrEmpty(w.ParentId)).Select(s => s.ParentId).Distinct().ToList();
            var lstVoucherIdNotChild = lstAllVoucher.Where(w => string.IsNullOrEmpty(w.ParentId)).Select(s => s.Id).Distinct().ToList();
            var lstVoucherParent = lstPaymentMethodExCash.Where(w => lstVoucherParentId.Contains(w.Id) || lstVoucherIdNotChild.Contains(w.Id)).OrderBy(o => o.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();
            foreach (var voucherParent in lstVoucherParent)
            {
                var lstChilds = lstAllVoucher.Where(ww => ww.ParentId == voucherParent.Id).OrderBy(ww => ww.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

                if (lstChilds != null && lstChilds.Any())
                {
                    ws.Range(row, column, row, (column + lstChilds.Count - 1)).Merge().Value = voucherParent.Name;

                    foreach (var voucherChild in lstChilds)
                    {
                        ws.Cell(row + 1, column++).Value = voucherChild.Name;
                        listInfoVounchers.Add(
                            new FJDailySalesReportViewModels
                            {
                                PaymentId = voucherChild.Id,
                                PaymentName = voucherChild.Name
                            });
                    }
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().Value = voucherParent.Name;
                    listInfoVounchers.Add(
                        new FJDailySalesReportViewModels
                        {
                            PaymentId = voucherParent.Id,
                            PaymentName = voucherParent.Name
                        });
                }
            }

            // List information of payments: Cards (FixedPayAmount = 0)
            // ALl payment: FixedPayAmount = 0, voucher parent maybe FixedPayAmount != 0
            int startColCard = column;
            List<FJDailySalesReportViewModels> listInfoCards = new List<FJDailySalesReportViewModels>();
            var lstAllCard = lstPaymentMethodExCash.Where(ww => ww.FixedPayAmount == 0).ToList();
            var lstCardParentId = lstAllCard.Where(w => !string.IsNullOrEmpty(w.ParentId)).Select(s => s.ParentId).Distinct().ToList();
            var lstCardIdNotChild = lstAllCard.Where(w => string.IsNullOrEmpty(w.ParentId) && lstVoucherParent.All(ww => ww.Id != w.Id)).Select(s => s.Id).Distinct().ToList();
            var lstCardParent = lstPaymentMethodExCash.Where(w => lstCardParentId.Contains(w.Id) || lstCardIdNotChild.Contains(w.Id)).OrderBy(o => o.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();
            foreach (var cardParent in lstCardParent)
            {
                var lstChilds = lstAllCard.Where(ww => ww.ParentId == cardParent.Id).OrderBy(ww => ww.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

                if (lstChilds != null && lstChilds.Any())
                {
                    ws.Range(row, column, row, (column + lstChilds.Count - 1)).Merge().Value = cardParent.Name;

                    foreach (var cardChild in lstChilds)
                    {
                        ws.Cell(row + 1, column++).Value = cardChild.Name;
                        listInfoCards.Add(
                            new FJDailySalesReportViewModels
                            {
                                PaymentId = cardChild.Id,
                                PaymentName = cardChild.Name
                            });
                    }
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().Value = cardParent.Name;
                    listInfoCards.Add(
                        new FJDailySalesReportViewModels
                        {
                            PaymentId = cardParent.Id,
                            PaymentName = cardParent.Name
                        });
                }
            }

            // Cash
            ws.Range(row, column, row + 1, column++).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash");

            // Tax
            if (_isTaxInclude)
            {
                ws.Range(row, column, row + 1, column).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax (Inc)");
            }
            else
                ws.Range(row, column, row + 1, column).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax (Add)");

            if (maxColumn < column)
                maxColumn = column;

            // Set header report
            CreateReportHeaderNew(ws, maxColumn, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());

            // Format title column report
            ws.Range(row, 1, row + 1, maxColumn).Style.Font.SetBold(true);
            ws.Range(row, 1, row + 1, maxColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.SetWrapText(true);
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(row, 1, row + 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row + 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row += 2;

            //// List business day in all store
            BusinessDayDisplayModels minBD = new BusinessDayDisplayModels();
            BusinessDayDisplayModels maxBD = new BusinessDayDisplayModels();

            // For a store
            decimal morningSales = 0;
            decimal midDaySales = 0;
            decimal fullDaySales = 0;
            decimal vouchersAmount = 0;
            int countVoucher = 0;
            decimal voucherAmount = 0;
            decimal cardAmount = 0;
            decimal cashAmount = 0;
            decimal taxAmount = 0;
            decimal netSales = 0;
            decimal _refundByCash = 0;
            decimal _refundGC = 0;

            // For all store
            decimal sumMorningSales = 0;
            decimal sumMidDaySales = 0;
            decimal sumFullDaySales = 0;
            decimal sumVouchersAmount = 0;
            decimal sumCashAmount = 0;
            decimal sumTaxAmount = 0;
            decimal sumNetSales = 0;

            int noStall = 0;
            int startRow = row;
            int seq = 1;

            List<FJDailySalesReportDataModels> lstDataInStoreReceipt = new List<FJDailySalesReportDataModels>();
            List<FJDailySalesReportDataModels> lstDataInStoreCN = new List<FJDailySalesReportDataModels>();
            List<string> lstReceiptIdInStore = new List<string>();

            decimal payGC = 0;
            decimal _taxOfPayGCNotInclude = 0;
            decimal _svcOfPayGCNotInclude = 0;
            decimal tax = 0;
            decimal svc = 0;
            decimal totalReceipt = 0;
            decimal totalSVC = 0;
            decimal totalTax = 0;

            for (int i = 0; i < lstStore.Count; i++)
            {
                morningSales = 0;
                midDaySales = 0;
                fullDaySales = 0;
                vouchersAmount = 0;
                cashAmount = 0;
                taxAmount = 0;
                netSales = 0;
                cashAmount = 0;
                _refundByCash = 0;
                _refundGC = 0;

                // Business day in store
                var lstBusDayInStore = _lstBusDayAllStore.Where(w => w.StoreId == lstStore[i].Id).ToList();
                noStall++;

                if (lstBusDayInStore != null && lstBusDayInStore.Any())
                {
                    minBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).FirstOrDefault();
                    maxBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).LastOrDefault();

                    // update seq for storeSetting
                    if (lstStoreSettings != null && lstStoreSettings.Any())
                    {
                        var storeSetting = lstStoreSettings.Where(ww => ww.StoreId == lstStore[i].Id).FirstOrDefault();
                        if (storeSetting != null)
                        {
                            storeSetting.Seq = seq;
                            seq++;
                        }
                    }

                }

                // Get value from setting of store
                // Get list data depend on business day
                var lstDataInStore = lstData.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();
                lstDataInStoreReceipt = new List<FJDailySalesReportDataModels>();
                lstDataInStoreCN = new List<FJDailySalesReportDataModels>();
                lstReceiptIdInStore = new List<string>();

                if (lstDataInStore != null && lstDataInStore.Any())
                {
                    lstReceiptIdInStore = lstDataInStore.Select(ss => ss.ReceiptId).ToList();

                    // Data for receipts
                    lstDataInStoreReceipt = lstDataInStore.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).ToList();

                    // Data for CreditNote
                    lstDataInStoreCN = lstDataInStore.Where(w => !string.IsNullOrEmpty(w.CreditNoteNo)).ToList();

                    // Morning Sales
                    morningSales = lstDataInStoreReceipt.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart).Sum(s => (decimal)s.ReceiptTotal);

                    // Mid-day Sales
                    midDaySales = lstDataInStoreReceipt.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart).Sum(s => (decimal)s.ReceiptTotal);

                    // Full-day Sales
                    fullDaySales = lstDataInStoreReceipt.Sum(s => (decimal)s.ReceiptTotal);
                }

                var lstPaymentsInStore = lstPayments.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();

                var lstItemSalesInStore = lstItemSales.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();

                if (_isTaxInclude)
                {
                    var obj = lstStoreSettings.Where(ww => ww.StoreId == lstStore[i].Id).FirstOrDefault();
                    if (obj != null)
                    {
                        if (obj.GLAccountCodes.Count() > 1)
                        {
                            var lstGLAccountCodes = obj.GLAccountCodes.ToList();
                            lstGLAccountCodes.RemoveAt(0);

                            var liquorSaleMorning = lstItemSalesInStore.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd
                            && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart
                            && lstGLAccountCodes.Contains(w.GLAccountCode))
                            .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                            var liquorSaleMidDay = lstItemSalesInStore.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd
                            && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart
                            && lstGLAccountCodes.Contains(w.GLAccountCode))
                            .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                            var liquorSaleFullDay = lstItemSalesInStore.Where(w => w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom
                            && lstGLAccountCodes.Contains(w.GLAccountCode))
                            .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                            morningSales -= liquorSaleMorning;
                            midDaySales -= liquorSaleMidDay;
                            fullDaySales -= liquorSaleFullDay;
                        }
                    }
                }

                // Set value for a store
                column = 1;

                // Column Store No.
                ws.Range(row, column, row, column++).Value = noStall;

                // Column Store name
                ws.Range(row, column, row, column++).Value = lstStore[i].Name;

                // Column Morning Sales
                ws.Range(row, column, row, column++).Value = morningSales;
                sumMorningSales += morningSales;

                // Column Mid-day Sales
                ws.Range(row, column, row, column++).Value = midDaySales;
                sumMidDaySales += midDaySales;

                // Column Full-day Sales
                ws.Range(row, column, row, column++).Value = fullDaySales;
                sumFullDaySales += fullDaySales;

                //====== Group vouchers
                var listVouncherIds = listInfoVounchers.Select(s => s.PaymentId).ToList();
                var lstPaymentOfVounchersInStore = lstPaymentsInStore.Where(w => listVouncherIds.Contains(w.PaymentId)).ToList();

                // Column Total Vouchers Amount
                vouchersAmount = lstPaymentOfVounchersInStore.Sum(s => (decimal)s.Amount);
                ws.Range(row, column, row, column++).Value = vouchersAmount;
                sumVouchersAmount += vouchersAmount;

                

                // Set value for each column voucher
                foreach (var vouncher in listInfoVounchers)
                {
                    countVoucher = 0;
                    voucherAmount = 0;

                    var lstInfo = lstPaymentOfVounchersInStore.Where(w => w.PaymentId == vouncher.PaymentId).ToList();
                    
                    if (lstInfo != null && lstInfo.Any())
                    {
                        countVoucher = lstInfo.Count;
                        voucherAmount = lstInfo.Sum(s => (decimal)s.Amount);
                    }

                    // Set value for column
                    ws.Range(row, column, row, column++).Value = countVoucher;
                    vouncher.Count += countVoucher;
                    vouncher.Total += voucherAmount;
                }

                //====== Group cards
                var listCardIds = listInfoCards.Select(s => s.PaymentId).ToList();
                _refundGC = _lstRefunds.Where(ww => lstReceiptIdInStore.Contains(ww.OrderId) && ww.IsGiftCard).Sum(ss => (decimal)ss.TotalRefund);
                foreach (var card in listInfoCards)
                {
                    cardAmount = 0;

                    var lstInfo = lstPaymentsInStore.Where(w => w.PaymentId == card.PaymentId).ToList();

                    if (lstInfo != null && lstInfo.Any())
                    {
                        cardAmount = lstInfo.Sum(s => (decimal)s.Amount);

                        // Check isGC
                        if (lstGC.Contains(card.PaymentId))
                            cardAmount -= _refundGC;
                    }
                    // Set value
                    ws.Range(row, column, row, column++).Value = cardAmount;
                    card.Total += cardAmount;
                }

                // Column Cash

                _refundByCash = _lstRefunds.Where(ww => lstReceiptIdInStore.Contains(ww.OrderId) && !ww.IsGiftCard).Sum(ss => (decimal)ss.TotalRefund);

                // pay by cash (subtract refund)
                cashAmount = lstPaymentsInStore.Where(p => lstPaymentMethodCashId.Contains(p.PaymentId))
                                        .Sum(p => (decimal)p.Amount) - _refundByCash;

                ws.Range(row, column, row, column).Value = cashAmount;
                ws.Range(row, column, row, column++).Style.NumberFormat.Format = "#,##0.00";
                sumCashAmount += cashAmount;

                // Column Tax (Inc)
                taxAmount = lstDataInStore.Sum(s => (decimal)s.GST);
                ws.Range(row, column, row, column).Value = taxAmount;
                ws.Range(row, column, row++, column).Style.NumberFormat.Format = "#,##0.00";
                sumTaxAmount += taxAmount;

                // Net sales
                #region Old NetSale 
                // Rouding
                //decimal roudingAmountReceipt = lstDataInStore.Sum(s => (decimal)s.Rounding);

                //decimal notIncludeOnSaleReceipt = 0;
                //decimal notIncludeOnSaleCN = 0;
                //netSales = 0;
                //if (_isTaxInclude)
                //{
                //    notIncludeOnSaleReceipt = lstItemsNoIncludeSaleReceiptInStore.Sum(s => (decimal)s.TotalAmount - (decimal)s.PromotionAmount - (decimal)s.TotalDiscount - (decimal)s.Tax);

                //    notIncludeOnSaleCN = lstItemsNoIncludeSaleCNInStore.Sum(s => (decimal)s.TotalAmount - (decimal)s.PromotionAmount - (decimal)s.TotalDiscount - (decimal)s.Tax);
                //}
                //else
                //{
                //    notIncludeOnSaleReceipt = lstItemsNoIncludeSaleReceiptInStore.Sum(s => (decimal)s.TotalAmount - (decimal)s.PromotionAmount - (decimal)s.TotalDiscount);

                //    notIncludeOnSaleCN = lstItemsNoIncludeSaleCNInStore.Sum(s => (decimal)s.TotalAmount - (decimal)s.PromotionAmount - (decimal)s.TotalDiscount);
                //}

                //// GC
                //var payGC = lstPaymentsInStore.Where(p => lstGC.Contains(p.PaymentId)).Sum(p => (decimal)p.Amount);

                //// NetSale = NetSale receipt - NetSale CreditNote
                //netSales = (lstDataInStore.Sum(s => (decimal)s.NetSales) - notIncludeOnSaleReceipt - payGC + roudingAmountReceipt) - (lstDataCreditNoteInStore.Sum(s => (decimal)s.NetSales) - notIncludeOnSaleCN);
                #endregion Old NetSale

                #region New NetSale
                payGC = 0;
                _taxOfPayGCNotInclude = 0;
                _svcOfPayGCNotInclude = 0;
                totalReceipt = 0;
                totalSVC = 0;
                totalTax = 0;

                var lstPaymentsNoIncludeSaleInStore = lstPaymentsInStore.Where(p => lstGC.Contains(p.PaymentId)
                            && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();

                if (lstPaymentsNoIncludeSaleInStore != null && lstPaymentsNoIncludeSaleInStore.Any())
                {
                    decimal refundAmount = 0;
                    decimal _amount = 0;

                    foreach (var item in lstPaymentsNoIncludeSaleInStore)
                    {
                        _amount = (decimal)item.Amount;
                        refundAmount = 0;

                        var lstGCRefunds = _lstRefunds.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                        if (lstGCRefunds != null && lstGCRefunds.Any())
                        {
                            refundAmount = lstGCRefunds.Sum(ss => (decimal)ss.TotalRefund);
                            _amount -= refundAmount;
                        }
                        payGC += _amount;

                        var receipt = lstDataInStore.Where(ww => ww.ReceiptId == item.OrderId).FirstOrDefault();
                        if (receipt != null)
                        {
                            tax = 0;
                            svc = 0;
                            if (receipt.GST != 0)
                                tax = _amount * (decimal)receipt.GST / ((decimal)receipt.ReceiptTotal == 0 ? 1 : (decimal)receipt.ReceiptTotal);
                            if (receipt.ServiceCharge != 0)
                                svc = (_amount - tax) * (decimal)receipt.ServiceCharge / ((decimal)(receipt.ReceiptTotal- receipt.GST) == 0 ? 1 : ((decimal)receipt.ReceiptTotal - (decimal)receipt.GST));

                            _taxOfPayGCNotInclude += tax;
                            _svcOfPayGCNotInclude += svc;
                        }
                    }
                }
                //netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (1)
                //(1)= GC pay not include - tax of pay GC not include - SCV of pay GC not include 
                totalReceipt = lstDataInStoreReceipt.Sum(ss => (decimal)ss.ReceiptTotal);
                totalSVC = lstDataInStoreReceipt.Sum(ss => (decimal)ss.ServiceCharge);
                totalTax = lstDataInStoreReceipt.Sum(ss => (decimal)ss.GST);

                var creditNoteIds = lstDataInStoreCN.Select(ss => ss.ReceiptId).ToList();
                decimal totalCreditNote = 0;
                if (creditNoteIds != null && creditNoteIds.Any())
                {
                    totalCreditNote = lstItemSalesInStore.Where(ww => creditNoteIds.Contains(ww.ReceiptId)
                    && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                        .Sum(ss => (decimal)ss.ItemTotal);
                }

                decimal giftCardSell = lstItemSalesInStore.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                        && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                        && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => (decimal)ss.TotalAmount.Value);

                decimal taxInclude = lstItemSalesInStore.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).Sum(ss => (decimal)ss.Tax);

                decimal _noincludeSale = lstItemSalesInStore.Where(ww => ww.IsIncludeSale == false
                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).Sum(ss => ((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));

                _noincludeSale -= taxInclude;

                netSales = totalReceipt - totalCreditNote - totalSVC - totalTax - giftCardSell - _noincludeSale
                    - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);

                #endregion New NetSale 
                sumNetSales += netSales;
            }

            // Set value total
            column = 1;
            ws.Range(row, column, row, column + 1).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
            column += 2;
            // Total morning sales
            ws.Range(row, column, row, column++).Value = sumMorningSales;
            // Total mid-day sales
            ws.Range(row, column, row, column++).Value = sumMidDaySales;
            // Total full-day sales
            ws.Range(row, column, row, column++).Value = sumFullDaySales;
            // Total vouchers amount
            ws.Range(row, column, row, column++).Value = sumVouchersAmount;
            // Total for each in vouchers
            foreach (var voucher in listInfoVounchers)
            {
                ws.Range(row, column, row, column++).Value = voucher.Count;
            }
            // Total for each in cards
            foreach (var card in listInfoCards)
            {
                ws.Range(row, column, row, column++).Value = card.Total;
            }
            // Total cash
            ws.Range(row, column, row, column++).Value = sumCashAmount;
            // Total tax
            ws.Range(row, column, row, column).Value = sumTaxAmount;
            // Format column money
            ws.Range(startRow, 3, row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRow, startColCard, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";
            // Format row total
            ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
            ws.Range(startRow, 1, row, maxColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRow, 1, row, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            // Format column name
            ws.Range(startRow, 2, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(row, 1, row++, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Set value Net Sales
            ws.Range(row, 1, row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NET SALES");
            ws.Range(row, 2, row, 2).Value = sumNetSales;
            ws.Range(row, 2, row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 2, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            // Format row Net sales
            ws.Range(row, 1, row, 2).Style.Font.SetBold(true);
            ws.Range(row, 1, row, 2).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 1, row, 2).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Set value Summary
            row += 2;
            ws.Range(row, 1, row, 3).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SUMMARY");
            ws.Range(row, 1, row, 3).Style.Font.SetBold(true);
            ws.Range(row, 1, row++, 3).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 2, row, 2).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Vouchers Count");
            ws.Range(row, 3, row, 3).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount");
            ws.Range(row, 2, row, 3).Style.Font.SetBold(true);
            ws.Range(row, 2, row++, 3).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            int startRowSummary = row;

            // Vouchers
            foreach (var voucher in listInfoVounchers)
            {
                ws.Range(row, 1, row, 1).Value = voucher.PaymentName;
                ws.Range(row, 2, row, 2).Value = voucher.Count;
                ws.Range(row, 3, row++, 3).Value = voucher.Total;
            }

            // Cards
            foreach (var card in listInfoCards)
            {
                ws.Range(row, 1, row, 1).Value = card.PaymentName;
                ws.Range(row, 3, row++, 3).Value = card.Total;
            }

            // Cash
            ws.Range(row, 1, row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash");
            ws.Range(row, 3, row, 3).Value = sumCashAmount;

            // Format summary
            ws.Range(startRowSummary, 1, row, 1).Style.Font.SetBold(true);
            ws.Range(startRowSummary, 1, row, 1).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(startRowSummary, 3, row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRowSummary - 1, 1, row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRowSummary - 2, 1, row, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRowSummary - 2, 1, row, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Data for Stall #14 & extend
            if (lstStoreSettings != null && lstStoreSettings.Any())
            {
                lstStoreSettings = lstStoreSettings.OrderBy(oo => oo.Seq).ToList();

                row = startRowSummary - 2;
                foreach (var item in lstStoreSettings)
                {
                    var storeName = lstStore.Where(ww => ww.Id == item.StoreId).Select(ss => ss.Name).FirstOrDefault();
                    minBD = null; maxBD = null;
                    var lstBusDayInStore = _lstBusDayAllStore.Where(w => w.StoreId == item.StoreId).ToList();

                    if (lstBusDayInStore != null && lstBusDayInStore.Any())
                    {
                        minBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).FirstOrDefault();
                        maxBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).LastOrDefault();

                        var lstDataGLAccountCode = lstItemSales.Where(w => w.StoreId == item.StoreId
                        && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom
                        && item.GLAccountCodes.Contains(w.GLAccountCode)).ToList();

                        if (lstDataGLAccountCode != null && lstDataGLAccountCode.Any())
                        {

                            int lstCellColumn = 6;
                            ws.Range(row, 5, row, 7).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("For") + " " + storeName;
                            ws.Range(row, 5, row, 7).Style.Font.SetBold(true);

                            int storeRow = row;
                            int categoryRow = 0;
                            row++;
                            ws.Range(row, 5, row + 1, 5).Merge();
                            ws.Range(row, 6, row, 6).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Drink Stall");

                            if (item.GLAccountCodes.Count() >= 1)
                            {
                                row++;
                                categoryRow = row;
                                ws.Range(row, 6, row, 6).Value = item.GLAccountCodes[0];

                                ws.Range(row + 1, 6, row + 1, 6).Value = lstDataGLAccountCode.Where(w => w.GLAccountCode == item.GLAccountCodes[0])
                                    .Sum(s => (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                                var lstGLAccountCode = item.GLAccountCodes.ToList();
                                lstGLAccountCode.Remove(item.GLAccountCodes[0]);

                                if (lstGLAccountCode != null && lstGLAccountCode.Any())
                                {
                                    for (int i = 0; i < lstGLAccountCode.Count; i++)
                                    {
                                        lstCellColumn++;
                                        ws.Range(categoryRow, lstCellColumn, categoryRow, lstCellColumn).Value = lstGLAccountCode[i];
                                        ws.Range(row + 1, 7, row + 1, 7).Value = lstDataGLAccountCode.Where(w => w.GLAccountCode == lstGLAccountCode[i]).Sum(s => (decimal)s.TotalAmount
                                            - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);
                                    }
                                    ws.Range(row - 1, lstCellColumn, row - 1, lstCellColumn).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Cards");
                                }
                            }
                            row++;
                            ws.Range(row, 5, row, 5).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Amount");
                            ws.Range(row, 5, row, 5).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row, 6, row, lstCellColumn).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range(row, 5, row, 5).Style.Font.SetBold(true);
                            ws.Range(storeRow, 5, storeRow, lstCellColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row, 6, row, lstCellColumn).Style.Font.SetBold(true);
                            ws.Range(categoryRow - 1, 6, categoryRow - 1, lstCellColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row - 2, 6, row, lstCellColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(row - 3, 5, row, lstCellColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(row - 3, 5, row, lstCellColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            row += 2;
                        }
                    }
                }
            }

            // Set Width for some column 
            List<int> lstWidCol = new List<int>() { 25, 40, 20, 20, 20, 20, 15, 20, 20, 20, 20, 20 };
            int countWidCol = lstWidCol.Count();
            for (int i = 0; i < maxColumn; i++)
            {
                
                if (i < countWidCol)
                {
                    ws.Column(i + 1).Width = lstWidCol[i];
                } else
                {
                    ws.Column(i + 1).Width = 17;
                }
            } 

            return wb;
        }
        #endregion Report with filter data by time & Credit Note info (refund gift card), updated 04102018

        #region Report with New DB
        public List<FJDailySalesReportModels> GetData_NewDB(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_PosSale.Where(ww => model.ListStores.Contains(ww.StoreId)
                                     && (ww.ReceiptCreatedDate >= model.FromDate && ww.ReceiptCreatedDate <= model.ToDate)
                                     && ww.Mode == model.Mode)
                                     .Select(ss => new FJDailySalesReportModels()
                                     {
                                         StoreId = ss.StoreId,
                                         CreatedDate = ss.ReceiptCreatedDate.Value,
                                         ReceiptTotal = ss.ReceiptTotal,      // Total R_Receipt
                                         Discount = ss.Discount,
                                         ServiceCharge = ss.ServiceCharge,
                                         GST = ss.GST,
                                         Rounding = ss.Rounding,
                                         Refund = ss.Refund,
                                         //NetSales = ss.NetSales,
                                         CreditNoteNo = ss.CreditNoteNo,
                                         ReceiptId = ss.OrderId
                                     }).ToList();
                return lstData;
            }
        }
        public XLWorkbook ExportExcel_NewDB(BaseReportModel model, List<StoreModels> lstStore, MerchantConfigApiModels pOSMerchantConfig)
        {
            string sheetName = "FJ_Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            // List business day in all store
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                // Format header report
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

            // Get data sale
            List<FJDailySalesReportModels> lstData = GetData_NewDB(model);
            if (lstData != null && lstData.Any())
            {
                switch (model.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();

                        if (lstData == null || !lstData.Any())
                        {
                            // Set header report
                            CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            return wb;
                        }
                        break;

                    case (int)Commons.EFilterType.Days:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();

                        if (lstData == null || !lstData.Any())
                        {
                            // Set header report
                            CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            return wb;
                        }
                        break;
                }
            }
            else
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());
                // Format header report
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            // Get data payment
            List<PaymentModels> lstPayments = GetDataPaymentItems(model);
            if (lstPayments != null && lstPayments.Any())
            {
                switch (model.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        lstPayments = lstPayments.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        lstPayments = lstPayments.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();
                        break;
                }
            }

            // Get list no include sale && GLAccountCode
            var lstReceiptId = lstData.Select(s => s.ReceiptId).Distinct().ToList();

            PosSaleFactory posSaleFactory = new PosSaleFactory();
            List<ItemizedSalesAnalysisReportModels> lstItemSales = posSaleFactory.GetItemsNoIncludeSale(lstReceiptId, model.ListStores, model.Mode);

            // Get lst refund by GC
            RefundFactory _refundFactory = new RefundFactory();
            var _lstRefunds = _refundFactory.GetListRefundWithoutDetailsByReceiptId(model.ListStores, lstReceiptId);

            bool _isTaxInclude = _baseFactory.IsTaxInclude(model.ListStores.FirstOrDefault());

            var lstStoreSettings = GetListStoreSetting(model);

            int maxColumn = 0;

            int row = 5;
            int column = 1;

            // Set title column report
            // Stall NO.
            ws.Range(row, column, row + 1, column++).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Stall NO.");
            // Stall Name
            ws.Range(row, column, row + 1, column++).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Stall Name");
            // Group Sales
            ws.Range(row, column, row, column + 2).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sales");
            ws.Range(row + 1, column, row + 1, column++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Morning Sales");
            ws.Range(row + 1, column, row + 1, column++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Mid-day Sales");
            ws.Range(row + 1, column, row + 1, column++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Full-day Sales");
            // Total Vouchers Amount
            ws.Range(row, column, row + 1, column++).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Vouchers Amount");

            // Get payment mothod info
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });

            // GC
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            // Group Payment methods
            // List information of payments: cash (Commons.EPaymentCode.Cash)
            var lstPaymentMethodCashId = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash).Select(s => s.Id).ToList();

            var lstPaymentMethodExCash = lstPaymentMethod.Where(ww => ww.Code != (int)Commons.EPaymentCode.Cash).OrderBy(o => o.Name).ToList();

            // List information of payments: Vouchers (FixedPayAmount > 0)
            // ALl payment: FixedPayAmount > 0, voucher parent maybe FixedPayAmount = 0
            List<FJDailySalesReportViewModels> listInfoVounchers = new List<FJDailySalesReportViewModels>();
            var lstAllVoucher = lstPaymentMethodExCash.Where(ww => ww.FixedPayAmount > 0).ToList();
            var lstVoucherParentId = lstAllVoucher.Where(w => !string.IsNullOrEmpty(w.ParentId)).Select(s => s.ParentId).Distinct().ToList();
            var lstVoucherIdNotChild = lstAllVoucher.Where(w => string.IsNullOrEmpty(w.ParentId)).Select(s => s.Id).Distinct().ToList();
            var lstVoucherParent = lstPaymentMethodExCash.Where(w => lstVoucherParentId.Contains(w.Id) || lstVoucherIdNotChild.Contains(w.Id)).OrderBy(o => o.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();
            foreach (var voucherParent in lstVoucherParent)
            {
                var lstChilds = lstAllVoucher.Where(ww => ww.ParentId == voucherParent.Id).OrderBy(ww => ww.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

                if (lstChilds != null && lstChilds.Any())
                {
                    ws.Range(row, column, row, (column + lstChilds.Count - 1)).Merge().Value = voucherParent.Name;

                    foreach (var voucherChild in lstChilds)
                    {
                        ws.Cell(row + 1, column++).Value = voucherChild.Name;
                        listInfoVounchers.Add(
                            new FJDailySalesReportViewModels
                            {
                                PaymentId = voucherChild.Id,
                                PaymentName = voucherChild.Name
                            });
                    }
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().Value = voucherParent.Name;
                    listInfoVounchers.Add(
                        new FJDailySalesReportViewModels
                        {
                            PaymentId = voucherParent.Id,
                            PaymentName = voucherParent.Name
                        });
                }
            }

            // List information of payments: Cards (FixedPayAmount = 0)
            // ALl payment: FixedPayAmount = 0, voucher parent maybe FixedPayAmount != 0
            int startColCard = column;
            List<FJDailySalesReportViewModels> listInfoCards = new List<FJDailySalesReportViewModels>();
            var lstAllCard = lstPaymentMethodExCash.Where(ww => ww.FixedPayAmount == 0).ToList();
            var lstCardParentId = lstAllCard.Where(w => !string.IsNullOrEmpty(w.ParentId)).Select(s => s.ParentId).Distinct().ToList();
            var lstCardIdNotChild = lstAllCard.Where(w => string.IsNullOrEmpty(w.ParentId) && lstVoucherParent.All(ww => ww.Id != w.Id)).Select(s => s.Id).Distinct().ToList();
            var lstCardParent = lstPaymentMethodExCash.Where(w => lstCardParentId.Contains(w.Id) || lstCardIdNotChild.Contains(w.Id)).OrderBy(o => o.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();
            foreach (var cardParent in lstCardParent)
            {
                var lstChilds = lstAllCard.Where(ww => ww.ParentId == cardParent.Id).OrderBy(ww => ww.Name).Select(s => new { s.Id, s.Name }).Distinct().ToList();

                if (lstChilds != null && lstChilds.Any())
                {
                    ws.Range(row, column, row, (column + lstChilds.Count - 1)).Merge().Value = cardParent.Name;

                    foreach (var cardChild in lstChilds)
                    {
                        ws.Cell(row + 1, column++).Value = cardChild.Name;
                        listInfoCards.Add(
                            new FJDailySalesReportViewModels
                            {
                                PaymentId = cardChild.Id,
                                PaymentName = cardChild.Name
                            });
                    }
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().Value = cardParent.Name;
                    listInfoCards.Add(
                        new FJDailySalesReportViewModels
                        {
                            PaymentId = cardParent.Id,
                            PaymentName = cardParent.Name
                        });
                }
            }

            // Cash
            ws.Range(row, column, row + 1, column++).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash");

            // Tax
            if (_isTaxInclude)
            {
                ws.Range(row, column, row + 1, column).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax (Inc)");
            }
            else
                ws.Range(row, column, row + 1, column).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax (Add)");

            if (maxColumn < column)
                maxColumn = column;

            // Set header report
            CreateReportHeaderNew(ws, maxColumn, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Daily Sales Report").ToUpper());

            // Format title column report
            ws.Range(row, 1, row + 1, maxColumn).Style.Font.SetBold(true);
            ws.Range(row, 1, row + 1, maxColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.SetWrapText(true);
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(row, 1, row + 1, maxColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(row, 1, row + 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row + 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row += 2;

            //// List business day in all store
            BusinessDayDisplayModels minBD = new BusinessDayDisplayModels();
            BusinessDayDisplayModels maxBD = new BusinessDayDisplayModels();

            // For a store
            decimal morningSales = 0;
            decimal midDaySales = 0;
            decimal fullDaySales = 0;
            decimal vouchersAmount = 0;
            int countVoucher = 0;
            decimal voucherAmount = 0;
            decimal cardAmount = 0;
            decimal cashAmount = 0;
            decimal taxAmount = 0;
            decimal netSales = 0;
            decimal _refundByCash = 0;
            decimal _refundGC = 0;

            // For all store
            decimal sumMorningSales = 0;
            decimal sumMidDaySales = 0;
            decimal sumFullDaySales = 0;
            decimal sumVouchersAmount = 0;
            decimal sumCashAmount = 0;
            decimal sumTaxAmount = 0;
            decimal sumNetSales = 0;

            int noStall = 0;
            int startRow = row;
            int seq = 1;

            List<FJDailySalesReportModels> lstDataInStoreReceipt = new List<FJDailySalesReportModels>();
            List<FJDailySalesReportModels> lstDataInStoreCN = new List<FJDailySalesReportModels>();
            List<string> lstReceiptIdInStore = new List<string>();

            decimal payGC = 0;
            decimal _taxOfPayGCNotInclude = 0;
            decimal _svcOfPayGCNotInclude = 0;
            decimal tax = 0;
            decimal svc = 0;
            decimal totalReceipt = 0;
            decimal totalSVC = 0;
            decimal totalTax = 0;

            for (int i = 0; i < lstStore.Count; i++)
            {
                morningSales = 0;
                midDaySales = 0;
                fullDaySales = 0;
                vouchersAmount = 0;
                cashAmount = 0;
                taxAmount = 0;
                netSales = 0;
                cashAmount = 0;
                _refundByCash = 0;
                _refundGC = 0;

                // Business day in store
                var lstBusDayInStore = _lstBusDayAllStore.Where(w => w.StoreId == lstStore[i].Id).ToList();
                noStall++;

                if (lstBusDayInStore != null && lstBusDayInStore.Any())
                {
                    minBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).FirstOrDefault();
                    maxBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).LastOrDefault();

                    // update seq for storeSetting
                    if (lstStoreSettings != null && lstStoreSettings.Any())
                    {
                        var storeSetting = lstStoreSettings.Where(ww => ww.StoreId == lstStore[i].Id).FirstOrDefault();
                        if (storeSetting != null)
                        {
                            storeSetting.Seq = seq;
                            seq++;
                        }
                    }

                }

                // Get value from setting of store
                // Get list data depend on business day
                var lstDataInStore = lstData.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();
                lstDataInStoreReceipt = new List<FJDailySalesReportModels>();
                lstDataInStoreCN = new List<FJDailySalesReportModels>();
                lstReceiptIdInStore = new List<string>();

                if (lstDataInStore != null && lstDataInStore.Any())
                {
                    lstReceiptIdInStore = lstDataInStore.Select(ss => ss.ReceiptId).ToList();

                    // Data for receipts
                    lstDataInStoreReceipt = lstDataInStore.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).ToList();

                    // Data for CreditNote
                    lstDataInStoreCN = lstDataInStore.Where(w => !string.IsNullOrEmpty(w.CreditNoteNo)).ToList();

                    // Morning Sales
                    morningSales = lstDataInStoreReceipt.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart).Sum(s => (decimal)s.ReceiptTotal);

                    // Mid-day Sales
                    midDaySales = lstDataInStoreReceipt.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart).Sum(s => (decimal)s.ReceiptTotal);

                    // Full-day Sales
                    fullDaySales = lstDataInStoreReceipt.Sum(s => (decimal)s.ReceiptTotal);
                }

                var lstPaymentsInStore = lstPayments.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();

                var lstItemSalesInStore = lstItemSales.Where(w => w.StoreId == lstStore[i].Id && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom).ToList();

                if (_isTaxInclude)
                {
                    var obj = lstStoreSettings.Where(ww => ww.StoreId == lstStore[i].Id).FirstOrDefault();
                    if (obj != null)
                    {
                        if (obj.GLAccountCodes.Count() > 1)
                        {
                            var lstGLAccountCodes = obj.GLAccountCodes.ToList();
                            lstGLAccountCodes.RemoveAt(0);

                            var liquorSaleMorning = lstItemSalesInStore.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MorningEnd
                            && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MorningStart
                            && lstGLAccountCodes.Contains(w.GLAccountCode))
                            .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                            var liquorSaleMidDay = lstItemSalesInStore.Where(w => w.CreatedDate.TimeOfDay <= pOSMerchantConfig.MidDayEnd
                            && w.CreatedDate.TimeOfDay >= pOSMerchantConfig.MidDayStart
                            && lstGLAccountCodes.Contains(w.GLAccountCode))
                            .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                            var liquorSaleFullDay = lstItemSalesInStore.Where(w => w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom
                            && lstGLAccountCodes.Contains(w.GLAccountCode))
                            .Sum(s => (decimal)s.ExtraAmount + (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                            morningSales -= liquorSaleMorning;
                            midDaySales -= liquorSaleMidDay;
                            fullDaySales -= liquorSaleFullDay;
                        }
                    }
                }

                // Set value for a store
                column = 1;

                // Column Store No.
                ws.Range(row, column, row, column++).Value = noStall;

                // Column Store name
                ws.Range(row, column, row, column++).Value = lstStore[i].Name;

                // Column Morning Sales
                ws.Range(row, column, row, column++).Value = morningSales;
                sumMorningSales += morningSales;

                // Column Mid-day Sales
                ws.Range(row, column, row, column++).Value = midDaySales;
                sumMidDaySales += midDaySales;

                // Column Full-day Sales
                ws.Range(row, column, row, column++).Value = fullDaySales;
                sumFullDaySales += fullDaySales;

                //====== Group vouchers
                var listVouncherIds = listInfoVounchers.Select(s => s.PaymentId).ToList();
                var lstPaymentOfVounchersInStore = lstPaymentsInStore.Where(w => listVouncherIds.Contains(w.PaymentId)).ToList();

                // Column Total Vouchers Amount
                vouchersAmount = lstPaymentOfVounchersInStore.Sum(s => (decimal)s.Amount);
                ws.Range(row, column, row, column++).Value = vouchersAmount;
                sumVouchersAmount += vouchersAmount;

                // Set value for each column voucher
                foreach (var vouncher in listInfoVounchers)
                {
                    countVoucher = 0;
                    voucherAmount = 0;

                    var lstInfo = lstPaymentOfVounchersInStore.Where(w => w.PaymentId == vouncher.PaymentId).ToList();

                    if (lstInfo != null && lstInfo.Any())
                    {
                        countVoucher = lstInfo.Count;
                        voucherAmount = lstInfo.Sum(s => (decimal)s.Amount);
                    }

                    // Set value for column
                    ws.Range(row, column, row, column++).Value = countVoucher;
                    vouncher.Count += countVoucher;
                    vouncher.Total += voucherAmount;
                }

                //====== Group cards
                var listCardIds = listInfoCards.Select(s => s.PaymentId).ToList();
                _refundGC = _lstRefunds.Where(ww => lstReceiptIdInStore.Contains(ww.OrderId) && ww.IsGiftCard).Sum(ss => (decimal)ss.TotalRefund);
                foreach (var card in listInfoCards)
                {
                    cardAmount = 0;

                    var lstInfo = lstPaymentsInStore.Where(w => w.PaymentId == card.PaymentId).ToList();

                    if (lstInfo != null && lstInfo.Any())
                    {
                        cardAmount = lstInfo.Sum(s => (decimal)s.Amount);

                        // Check isGC
                        if (lstGC.Contains(card.PaymentId))
                            cardAmount -= _refundGC;
                    }
                    // Set value
                    ws.Range(row, column, row, column++).Value = cardAmount;
                    card.Total += cardAmount;
                }

                // Column Cash
                _refundByCash = _lstRefunds.Where(ww => lstReceiptIdInStore.Contains(ww.OrderId) && !ww.IsGiftCard).Sum(ss => (decimal)ss.TotalRefund);

                // pay by cash (subtract refund)
                cashAmount = lstPaymentsInStore.Where(p => lstPaymentMethodCashId.Contains(p.PaymentId))
                                        .Sum(p => (decimal)p.Amount) - _refundByCash;

                ws.Range(row, column, row, column).Value = cashAmount;
                ws.Range(row, column, row, column++).Style.NumberFormat.Format = "#,##0.00";
                sumCashAmount += cashAmount;

                // Column Tax (Inc)
                taxAmount = lstDataInStore.Sum(s => (decimal)s.GST);
                ws.Range(row, column, row, column).Value = taxAmount;
                ws.Range(row, column, row++, column).Style.NumberFormat.Format = "#,##0.00";
                sumTaxAmount += taxAmount;

                // Net sales
                #region New NetSale
                payGC = 0;
                _taxOfPayGCNotInclude = 0;
                _svcOfPayGCNotInclude = 0;
                totalReceipt = 0;
                totalSVC = 0;
                totalTax = 0;

                var lstPaymentsNoIncludeSaleInStore = lstPaymentsInStore.Where(p => lstGC.Contains(p.PaymentId)
                            && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();

                if (lstPaymentsNoIncludeSaleInStore != null && lstPaymentsNoIncludeSaleInStore.Any())
                {
                    decimal refundAmount = 0;
                    decimal _amount = 0;

                    foreach (var item in lstPaymentsNoIncludeSaleInStore)
                    {
                        _amount = (decimal)item.Amount;
                        refundAmount = 0;

                        var lstGCRefunds = _lstRefunds.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                        if (lstGCRefunds != null && lstGCRefunds.Any())
                        {
                            refundAmount = lstGCRefunds.Sum(ss => (decimal)ss.TotalRefund);
                            _amount -= refundAmount;
                        }
                        payGC += _amount;

                        var receipt = lstDataInStore.Where(ww => ww.ReceiptId == item.OrderId).FirstOrDefault();
                        if (receipt != null)
                        {
                            tax = 0;
                            svc = 0;
                            if (receipt.GST != 0)
                                tax = _amount * (decimal)receipt.GST / ((decimal)receipt.ReceiptTotal == 0 ? 1 : (decimal)receipt.ReceiptTotal);
                            if (receipt.ServiceCharge != 0)
                                svc = (_amount - tax) * (decimal)receipt.ServiceCharge / ((decimal)(receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : ((decimal)receipt.ReceiptTotal - (decimal)receipt.GST));

                            _taxOfPayGCNotInclude += tax;
                            _svcOfPayGCNotInclude += svc;
                        }
                    }
                }
                //netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (1)
                //(1)= GC pay not include - tax of pay GC not include - SCV of pay GC not include 
                totalReceipt = lstDataInStoreReceipt.Sum(ss => (decimal)ss.ReceiptTotal);
                totalSVC = lstDataInStoreReceipt.Sum(ss => (decimal)ss.ServiceCharge);
                totalTax = lstDataInStoreReceipt.Sum(ss => (decimal)ss.GST);

                var creditNoteIds = lstDataInStoreCN.Select(ss => ss.ReceiptId).ToList();
                decimal totalCreditNote = 0;
                if (creditNoteIds != null && creditNoteIds.Any())
                {
                    totalCreditNote = lstItemSalesInStore.Where(ww => creditNoteIds.Contains(ww.ReceiptId)
                    && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                        .Sum(ss => (decimal)ss.ItemTotal);
                }

                decimal giftCardSell = lstItemSalesInStore.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                        && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                        && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => (decimal)ss.TotalAmount.Value);

                decimal taxInclude = lstItemSalesInStore.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).Sum(ss => (decimal)ss.Tax);

                decimal _noincludeSale = lstItemSalesInStore.Where(ww => ww.IsIncludeSale == false
                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).Sum(ss => ((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));

                _noincludeSale -= taxInclude;

                netSales = totalReceipt - totalCreditNote - totalSVC - totalTax - giftCardSell - _noincludeSale
                    - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);

                #endregion New NetSale 
                sumNetSales += netSales;
            }

            // Set value total
            column = 1;
            ws.Range(row, column, row, column + 1).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
            column += 2;
            // Total morning sales
            ws.Range(row, column, row, column++).Value = sumMorningSales;
            // Total mid-day sales
            ws.Range(row, column, row, column++).Value = sumMidDaySales;
            // Total full-day sales
            ws.Range(row, column, row, column++).Value = sumFullDaySales;
            // Total vouchers amount
            ws.Range(row, column, row, column++).Value = sumVouchersAmount;
            // Total for each in vouchers
            foreach (var voucher in listInfoVounchers)
            {
                ws.Range(row, column, row, column++).Value = voucher.Count;
            }
            // Total for each in cards
            foreach (var card in listInfoCards)
            {
                ws.Range(row, column, row, column++).Value = card.Total;
            }
            // Total cash
            ws.Range(row, column, row, column++).Value = sumCashAmount;
            // Total tax
            ws.Range(row, column, row, column).Value = sumTaxAmount;
            // Format column money
            ws.Range(startRow, 3, row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRow, startColCard, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";
            // Format row total
            ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
            ws.Range(startRow, 1, row, maxColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRow, 1, row, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            // Format column name
            ws.Range(startRow, 2, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(row, 1, row++, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Set value Net Sales
            ws.Range(row, 1, row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NET SALES");
            ws.Range(row, 2, row, 2).Value = sumNetSales;
            ws.Range(row, 2, row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 2, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            // Format row Net sales
            ws.Range(row, 1, row, 2).Style.Font.SetBold(true);
            ws.Range(row, 1, row, 2).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 1, row, 2).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Set value Summary
            row += 2;
            ws.Range(row, 1, row, 3).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SUMMARY");
            ws.Range(row, 1, row, 3).Style.Font.SetBold(true);
            ws.Range(row, 1, row++, 3).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(row, 2, row, 2).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Vouchers Count");
            ws.Range(row, 3, row, 3).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount");
            ws.Range(row, 2, row, 3).Style.Font.SetBold(true);
            ws.Range(row, 2, row++, 3).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            int startRowSummary = row;

            // Vouchers
            foreach (var voucher in listInfoVounchers)
            {
                ws.Range(row, 1, row, 1).Value = voucher.PaymentName;
                ws.Range(row, 2, row, 2).Value = voucher.Count;
                ws.Range(row, 3, row++, 3).Value = voucher.Total;
            }

            // Cards
            foreach (var card in listInfoCards)
            {
                ws.Range(row, 1, row, 1).Value = card.PaymentName;
                ws.Range(row, 3, row++, 3).Value = card.Total;
            }

            // Cash
            ws.Range(row, 1, row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash");
            ws.Range(row, 3, row, 3).Value = sumCashAmount;

            // Format summary
            ws.Range(startRowSummary, 1, row, 1).Style.Font.SetBold(true);
            ws.Range(startRowSummary, 1, row, 1).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(startRowSummary, 3, row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRowSummary - 1, 1, row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(startRowSummary - 2, 1, row, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRowSummary - 2, 1, row, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Data for Stall #14 & extend
            if (lstStoreSettings != null && lstStoreSettings.Any())
            {
                lstStoreSettings = lstStoreSettings.OrderBy(oo => oo.Seq).ToList();

                row = startRowSummary - 2;
                foreach (var item in lstStoreSettings)
                {
                    var storeName = lstStore.Where(ww => ww.Id == item.StoreId).Select(ss => ss.Name).FirstOrDefault();
                    minBD = null; maxBD = null;
                    var lstBusDayInStore = _lstBusDayAllStore.Where(w => w.StoreId == item.StoreId).ToList();

                    if (lstBusDayInStore != null && lstBusDayInStore.Any())
                    {
                        minBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).FirstOrDefault();
                        maxBD = lstBusDayInStore.OrderBy(oo => oo.DateFrom).LastOrDefault();

                        var lstDataGLAccountCode = lstItemSales.Where(w => w.StoreId == item.StoreId
                        && w.CreatedDate <= maxBD.DateTo && w.CreatedDate >= minBD.DateFrom
                        && item.GLAccountCodes.Contains(w.GLAccountCode)).ToList();

                        if (lstDataGLAccountCode != null && lstDataGLAccountCode.Any())
                        {

                            int lstCellColumn = 6;
                            ws.Range(row, 5, row, 7).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("For") + " " + storeName;
                            ws.Range(row, 5, row, 7).Style.Font.SetBold(true);

                            int storeRow = row;
                            int categoryRow = 0;
                            row++;
                            ws.Range(row, 5, row + 1, 5).Merge();
                            ws.Range(row, 6, row, 6).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Drink Stall");

                            if (item.GLAccountCodes.Count() >= 1)
                            {
                                row++;
                                categoryRow = row;
                                ws.Range(row, 6, row, 6).Value = item.GLAccountCodes[0];

                                ws.Range(row + 1, 6, row + 1, 6).Value = lstDataGLAccountCode.Where(w => w.GLAccountCode == item.GLAccountCodes[0])
                                    .Sum(s => (decimal)s.TotalAmount - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);

                                var lstGLAccountCode = item.GLAccountCodes.ToList();
                                lstGLAccountCode.Remove(item.GLAccountCodes[0]);

                                if (lstGLAccountCode != null && lstGLAccountCode.Any())
                                {
                                    for (int i = 0; i < lstGLAccountCode.Count; i++)
                                    {
                                        lstCellColumn++;
                                        ws.Range(categoryRow, lstCellColumn, categoryRow, lstCellColumn).Value = lstGLAccountCode[i];
                                        ws.Range(row + 1, 7, row + 1, 7).Value = lstDataGLAccountCode.Where(w => w.GLAccountCode == lstGLAccountCode[i]).Sum(s => (decimal)s.TotalAmount
                                            - (decimal)s.TotalDiscount - (decimal)s.PromotionAmount);
                                    }
                                    ws.Range(row - 1, lstCellColumn, row - 1, lstCellColumn).Merge().Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FJ Cards");
                                }
                            }
                            row++;
                            ws.Range(row, 5, row, 5).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Amount");
                            ws.Range(row, 5, row, 5).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row, 6, row, lstCellColumn).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range(row, 5, row, 5).Style.Font.SetBold(true);
                            ws.Range(storeRow, 5, storeRow, lstCellColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row, 6, row, lstCellColumn).Style.Font.SetBold(true);
                            ws.Range(categoryRow - 1, 6, categoryRow - 1, lstCellColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                            ws.Range(row - 2, 6, row, lstCellColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(row - 3, 5, row, lstCellColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(row - 3, 5, row, lstCellColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            row += 2;
                        }
                    }
                }
            }

            // Set Width for some column 
            List<int> lstWidCol = new List<int>() { 25, 40, 20, 20, 20, 20, 15, 20, 20, 20, 20, 20 };
            int countWidCol = lstWidCol.Count();
            for (int i = 0; i < maxColumn; i++)
            {

                if (i < countWidCol)
                {
                    ws.Column(i + 1).Width = lstWidCol[i];
                }
                else
                {
                    ws.Column(i + 1).Width = 17;
                }
            }

            return wb;
        }
        #endregion Report with New DB
    }
}
