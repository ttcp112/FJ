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
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Models.Api;
using static NuWebNCloud.Shared.Commons;
using System.Diagnostics;
using System.Data.SqlClient;
using NuWebNCloud.Data.Models;

namespace NuWebNCloud.Shared.Factory
{
    public class DailySalesReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private NoSaleDetailReportFactory _noSaleDetailReportFactory;
        private NoIncludeOnSaleDataFactory _noIncludeOnSaleDataFactory;
        private DiscountAndMiscReportFactory _DiscountAndMiscReportFactory;
        private CategoriesFactory _categoriesFactory;
        // Updated 08242017
        private ItemizedSalesAnalysisReportFactory _itemizedSalesAnalysisFactory;
        private RefundFactory _refundFactory;
        public DailySalesReportFactory()
        {
            _baseFactory = new BaseFactory();
            _noSaleDetailReportFactory = new NoSaleDetailReportFactory();
            _noIncludeOnSaleDataFactory = new NoIncludeOnSaleDataFactory();
            // Updated 08242017
            _itemizedSalesAnalysisFactory = new ItemizedSalesAnalysisReportFactory();
            _DiscountAndMiscReportFactory = new DiscountAndMiscReportFactory();
            _categoriesFactory = new CategoriesFactory();
            _refundFactory = new RefundFactory();
        }
        public bool Insert(List<DailySalesReportInsertModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            //_logger.Info("==========================================================");
            //_logger.Info(string.Format("Insert DailySalesReport: StoreId: [{0}]", info.StoreId));

            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_DailySalesReport.Where(ww => ww.StoreId == info.StoreId && ww.OrderId == info.OrderId).FirstOrDefault();
                if (obj != null)
                {
                    //_logger.Info(string.Format("Insert DailySalesReport: StoreId: [{0}] exist", info.StoreId));
                    NSLog.Logger.Info("insert daily sale data exist", lstInfo);
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_DailySalesReport> lstInsert = new List<R_DailySalesReport>();
                        R_DailySalesReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_DailySalesReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.OrderId = item.OrderId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.NoOfPerson = item.NoOfPerson;
                            itemInsert.ReceiptTotal = item.ReceiptTotal;
                            itemInsert.Discount = item.Discount;
                            itemInsert.ServiceCharge = item.ServiceCharge;
                            itemInsert.GST = item.GST;
                            itemInsert.Rounding = item.Rounding;
                            itemInsert.Refund = item.Refund;
                            itemInsert.Tip = item.Tip;
                            itemInsert.PromotionValue = item.PromotionValue;
                            itemInsert.NetSales = item.NetSales;
                            itemInsert.Mode = item.Mode;
                            itemInsert.BusinessId = item.BusinessId;
                            itemInsert.CreditNoteNo = item.CreditNoteNo;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_DailySalesReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("insert daily sale data success", lstInfo);
                        //_logger.Info(string.Format("Insert DailySalesReport: StoreId: [{0}] Success", info.StoreId));
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("insert daily sale data fail", lstInfo);
                        NSLog.Logger.Error("insert daily sale data fail", ex);
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
            //var jsonContent = JsonConvert.SerializeObject(lstInfo);
            //_baseFactory.InsertTrackingLog("R_DailySalesReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public bool InsertShiftLogs(List<ShiftLogModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_ShiftLog.Where(ww => ww.StoreId == info.StoreId && ww.BusinessId == info.BusinessId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("insert shiftlog data exist", lstInfo);
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_ShiftLog> lstInsert = new List<R_ShiftLog>();
                        R_ShiftLog itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_ShiftLog();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.BusinessId = item.BusinessId;
                            itemInsert.StartedOn = item.StartedOn;
                            itemInsert.ClosedOn = item.ClosedOn;
                            itemInsert.StartedStaff = item.StartedStaff;
                            itemInsert.ClosedStaff = item.ClosedStaff;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_ShiftLog.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("insert shiftlog data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("insert shiftlog data fail", lstInfo);
                        NSLog.Logger.Error("insert shiftlog data fail", ex);
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


        //public bool InsertPayment(List<PaymentMenthodModels> lstPaymentInfo)
        //{
        //    bool result = true;
        //    var info = lstPaymentInfo.FirstOrDefault();
        //    using (NuWebContext cxt = new NuWebContext())
        //    {
        //        using (var transaction = cxt.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                // lstPaymentInfo
        //                List<G_PaymentMenthod> lstPaymentInsert = new List<G_PaymentMenthod>();
        //                G_PaymentMenthod paymentInsert = null;
        //                foreach (var item in lstPaymentInfo)
        //                {
        //                    paymentInsert = new G_PaymentMenthod();
        //                    paymentInsert.Id = Guid.NewGuid().ToString();
        //                    paymentInsert.StoreId = item.StoreId;

        //                    paymentInsert.PaymentName = item.PaymentName;
        //                    paymentInsert.CreatedDate = item.CreatedDate;


        //                    lstPaymentInsert.Add(paymentInsert);
        //                }
        //                cxt.G_PaymentMenthod.AddRange(lstPaymentInsert);
        //                cxt.SaveChanges();

        //                transaction.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.Error(ex);
        //                result = false;
        //                transaction.Rollback();
        //            }
        //            finally
        //            {
        //                if (cxt != null)
        //                    cxt.Dispose();
        //            }
        //        }
        //    }

        //    var jsonContentPayment = JsonConvert.SerializeObject(lstPaymentInfo);
        //    _baseFactory.InsertTrackingLog("G_PaymentMenthod", jsonContentPayment, info.StoreId.ToString(), result);

        //    return result;
        //}

        public List<DailySalesReportInsertModels> GetDataReceiptItems(NuWebNCloud.Shared.Models.Reports.BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_DailySalesReport.Where(ww => model.ListStores.Contains(ww.StoreId)
                                     && (ww.CreatedDate >= model.FromDate && ww.CreatedDate <= model.ToDate)
                                     && ww.Mode == model.Mode)
                                     .Select(ss => new DailySalesReportInsertModels()
                                     {
                                         StoreId = ss.StoreId,
                                         CreatedDate = ss.CreatedDate,
                                         NoOfPerson = ss.NoOfPerson,
                                         ReceiptTotal = ss.ReceiptTotal,      // Total R_Receipt
                                         Discount = ss.Discount,
                                         ServiceCharge = ss.ServiceCharge,
                                         GST = ss.GST,
                                         Rounding = ss.Rounding,
                                         Refund = ss.Refund,
                                         NetSales = ss.NetSales,
                                         Tip = ss.Tip,
                                         PromotionValue = ss.PromotionValue

                                     }).ToList();

                return lstData;
            }
        }

        public List<PaymentModels> GetDataPaymentItems(NuWebNCloud.Shared.Models.Reports.BaseReportModel model)
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
                                   IsInclude = tb.IsInclude,
                               }).ToList();
                return lstData;
            }
        }
        //public List<PaymentModels> GetDataPaymentItems_UseProcedure(Models.Reports.BaseReportModel model)
        //{
        //    using (var cxt = new NuWebContext())
        //    {
        //        //var lstData = (from tb in cxt.G_PaymentMenthod
        //        //               where model.ListStores.Contains(tb.StoreId)
        //        //                     && (tb.CreatedDate >= model.FromDate
        //        //                             && tb.CreatedDate <= model.ToDate) && tb.Mode == model.Mode
        //        //               select new PaymentModels
        //        //               {
        //        //                   StoreId = tb.StoreId,
        //        //                   CreatedDate = tb.CreatedDate,
        //        //                   PaymentId = tb.PaymentId,
        //        //                   PaymentName = tb.PaymentName,
        //        //                   Amount = tb.Amount,
        //        //                   OrderId = tb.OrderId,
        //        //                   IsInclude = tb.IsInclude,
        //        //               }).ToList();

        //        string lstStores = string.Join(",", model.ListStores);
        //        var lstData = cxt.Database.SqlQuery<PaymentModels>("EXEC sp_GetDataPaymentItems @mode, @lstStoreIds, @dFrom,@dTo",
        //             new SqlParameter("mode", model.Mode)
        //             , new SqlParameter("lstStoreIds", lstStores)
        //             , new SqlParameter("dFrom", model.FromDate.ToString("yyyy-MM-dd HH:mm:ss.fff"))
        //             , new SqlParameter("dTo", model.ToDate.ToString("yyyy-MM-dd HH:mm:ss.fff"))).ToList();

        //        return lstData;
        //    }
        //}

        //public List<DailySalesReportModels> GetListPayment(BaseReportModel model, List<string> lstNotInPayMent)
        //{
        //    using (var cxt = new NuWebContext())
        //    {
        //        var lstData = (from tb in cxt.R_DailySalesReport
        //                       where model.ListStores.Contains(tb.StoreId)
        //                             && !lstNotInPayMent.Contains(tb.PaymentName)
        //                             && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
        //                       group tb by new
        //                       {
        //                           PaymentId = tb.PaymentId,
        //                           PaymentName = tb.PaymentName
        //                       } into g
        //                       select new DailySalesReportModels
        //                       {
        //                           PaymentId = g.Key.PaymentId,
        //                           PaymentName = g.Key.PaymentName,
        //                       }).ToList();
        //        return lstData;
        //    }
        //}

        public XLWorkbook ExportExcel(Models.Reports.BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "Daily_Sales_Report";// _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily_Sales_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            var lstStoreId = lstStore.Select(ss => ss.Id).ToList();
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreId, model.Mode);
            if (_lstBusDayAllStore == null || _lstBusDayAllStore.Any())
            {
                return wb;
            }
            List<DailySalesReportInsertModels> lstData = GetDataReceiptItems(model);
            List<PaymentModels> lstPayments = GetDataPaymentItems(model);
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
            if (cash == null)
                cash = new RFilterChooseExtBaseModel();
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            OrderTipFactory orderTipFactory = new OrderTipFactory();
            //var _lstTips = orderTipFactory.GetDataTips(model);
            //get list noinclude sale
            var _lstNoIncludeOnSale = _noIncludeOnSaleDataFactory.GetListCateNoIncludeSaleForDailySale(model.ListStores, model.FromDate, model.ToDate, model.Mode);
            var _lstNoIncludeOnSaleGroupByCate = new List<NoIncludeOnSaleDataReportModels>();
            //string sheetName = "Daily_Sales_Report";// _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily_Sales_Report");
            //var wb = new XLWorkbook();
            //var ws = wb.Worksheets.Add(sheetName);
            int maxColumn = 0;

            int row = 4;
            int column = 0;
            double receiptTotal = 0, payTotal = 0, payAmount = 0, noSale = 0, payByCash = 0, _tip = 0;
            double payAmountByCate = 0;
            double payAmountByCateForNetSale = 0;
            double payAmountByCateTotal = 0;
            //DateTime dFrom, dTo;
            Dictionary<int, double> total = new Dictionary<int, double>();
            Dictionary<int, double> grandTotal = new Dictionary<int, double>();
            TopSellingProductsReportFactory topSellFactory = new TopSellingProductsReportFactory();
            //var lstStoreId = lstStore.Select(ss => ss.Id).ToList();
            //var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreId, model.Mode);

            for (int i = 0; i < lstStore.Count; i++)
            {
                column = 1;
                // store name
                row++;
                ws.Range("A" + row, "C" + row).Merge().SetValue(lstStore[i].Name);

                #region Columns Names
                row++;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No Sale"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                int receiptTotalColumn = column;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotions"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Svc Chg"));
                if (GetTaxType(lstStore[i].Id) == (int)Commons.ETax.AddOn)
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                }
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                int refundColumn = column;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
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
                ///
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                int startPayColumn = column;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));
                //ws.Range(row, column, row + 1, column++).Merge().SetValue("Gift Card");
                //List<string> lstNotInPayMent = new List<string>() { "Cash", "Gift Card" };
                List<string> lstNotInPayMent = new List<string>() { "Cash" };
                List<string> lstNotInPayMentNew = new List<string>() { cash != null ? cash.Id : "" };

                List<string> storeCards = new List<string>();
                //var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && ww.Name.ToLower() != "cash"
                //    && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();

                var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && (ww.Id != cash.Id || ww.Name.ToLower() != "cash")
                    && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();

                for (int p = 0; p < lstPaymentParents.Count; p++)
                {
                    lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                    && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();
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

                //var payments = lstPayments.Where(ww => !lstNotInPayMent.Contains(ww.PaymentName)).Select(ss =>
                //        new { PaymentId = ss.PaymentId, PaymentName = ss.PaymentName }).Distinct().ToList();
                //if (payments.Count > 0)
                //{
                //    ws.Range(row, column, row, column + payments.Count - 1).Merge().SetValue("External");
                //    for (int k = 0; k < payments.Count; k++)
                //    {
                //        column += (k == 0 ? 0 : 1);
                //        ws.Cell(row + 1, column).SetValue(payments[k].PaymentName);
                //        storeCards.Add(payments[k].PaymentId);
                //    }
                //}
                //else
                //{
                //    ws.Range(row, column, row + 1, column).Merge().SetValue("External");
                //    storeCards.Add("");
                //}
                // column++;

                int endPayColumn = column;
                // end group payments
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                #endregion

                // set value for last column
                if (maxColumn < column)
                    maxColumn = column;

                #region Format Store Header
                ws.Range(row - 1, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                ws.Range(row - 1, 1, row + 1, column).Style.Font.SetBold(true);
                ws.Range(row - 1, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Range(row, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                #endregion

                row += 2;
                int startRow = row;

                var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == lstStore[i].Id).ToList();
                List<DailySalesReportInsertModels> lstDataByStore = lstData.Where(ww => ww.StoreId == lstStore[i].Id).ToList();
                total = new Dictionary<int, double>();
                if (lstDataByStore != null && lstDataByStore.Count > 0)
                {
                    // Fill data==================================================================
                    //============================================================================
                    payTotal = 0; payAmount = 0;

                    for (int j = 0; j < lstBusDay.Count; j++)
                    {
                        column = 1;
                        payTotal = 0;
                        payAmount = 0;
                        //DateTime date = model.FromDate.AddDays(j);
                        //dFrom = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                        //dTo = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
                        //sumTotal = topSellFactory.GetTotalReceipt(dFrom, dTo, lstStore[i].Id);

                        // ws.Cell(row, column++).SetValue(lstBusDay[j].DateFrom);
                        var reportItems = lstDataByStore.Where(s => s.CreatedDate >= lstBusDay[j].DateFrom && s.CreatedDate <= lstBusDay[j].DateTo).ToList();
                        if (reportItems == null || reportItems.Count == 0)
                            continue;

                        //var lstNoIncludeOnSaleByBusDay = _lstNoIncludeOnSale.Where(ww => ww.CreatedDate >= lstBusDay[j].DateFrom
                        //&& ww.CreatedDate <= lstBusDay[j].DateTo
                        // && ww.StoreId == lstStore[i].Id).ToList();

                        //ws.Cell(row, column++).SetValue(reportItems.OrderBy(ss => ss.CreatedDate).FirstOrDefault().CreatedDate);

                        ws.Cell(row, column++).SetValue(lstBusDay[j].DateFrom);
                        #region add value to cell
                        receiptTotal = reportItems.Sum(ss => ss.ReceiptTotal);
                        //NoSale
                        noSale = _noSaleDetailReportFactory.GetNoSale(lstStore[i].Id, lstBusDay[j].DateFrom, lstBusDay[j].DateTo);
                        ws.Cell(row, column++).Value = noSale;
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, noSale);
                        }
                        else
                        {
                            total[column] += noSale;
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, noSale);
                        }
                        else
                        {
                            grandTotal[column] += noSale;
                        }
                        //TC - NoOfReceipt
                        ws.Cell(row, column++).Value = reportItems.Count;
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, reportItems.Count);
                        }
                        else
                        {
                            total[column] += reportItems.Count;
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, reportItems.Count);
                        }
                        else
                        {
                            grandTotal[column] += reportItems.Count;
                        }

                        //PAX - NoOfPerson
                        ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.NoOfPerson);
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                        }
                        else
                        {
                            total[column] += reportItems.Sum(ss => ss.NoOfPerson);
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                        }
                        else
                        {
                            grandTotal[column] += reportItems.Sum(ss => ss.NoOfPerson);
                        }

                        ws.Cell(row, column++).Value = receiptTotal.ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, receiptTotal);
                        }
                        else
                        {
                            total[column] += receiptTotal;
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, receiptTotal);
                        }
                        else
                        {
                            grandTotal[column] += receiptTotal;
                        }

                        //Discount
                        ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Discount).ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, reportItems.Sum(ss => ss.Discount));
                        }
                        else
                        {
                            total[column] += reportItems.Sum(ss => ss.Discount);
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, reportItems.Sum(ss => ss.Discount));
                        }
                        else
                        {
                            grandTotal[column] += reportItems.Sum(ss => ss.Discount);
                        }
                        //promotions
                        ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.PromotionValue).ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                        }
                        else
                        {
                            total[column] += reportItems.Sum(ss => ss.PromotionValue);
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                        }
                        else
                        {
                            grandTotal[column] += reportItems.Sum(ss => ss.PromotionValue);
                        }
                        //ServiceCharge
                        ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.ServiceCharge).ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                        }
                        else
                        {
                            total[column] += reportItems.Sum(ss => ss.ServiceCharge);
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                        }
                        else
                        {
                            grandTotal[column] += reportItems.Sum(ss => ss.ServiceCharge);
                        }
                        //Tax
                        ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.GST).ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, reportItems.Sum(ss => ss.GST));
                        }
                        else
                        {
                            total[column] += reportItems.Sum(ss => ss.GST);
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, reportItems.Sum(ss => ss.GST));
                        }
                        else
                        {
                            grandTotal[column] += reportItems.Sum(ss => ss.GST);
                        }
                        //Rounding
                        ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Rounding).ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, reportItems.Sum(ss => ss.Rounding));
                        }
                        else
                        {
                            total[column] += reportItems.Sum(ss => ss.Rounding);
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, reportItems.Sum(ss => ss.Rounding));
                        }
                        else
                        {
                            grandTotal[column] += reportItems.Sum(ss => ss.Rounding);
                        }

                        //Refund

                        ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Refund).ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, reportItems.Sum(ss => ss.Refund));
                        }
                        else
                        {
                            total[column] += reportItems.Sum(ss => ss.Refund);
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, reportItems.Sum(ss => ss.Refund));
                        }
                        else
                        {
                            grandTotal[column] += reportItems.Sum(ss => ss.Refund);
                        }

                        //Tip
                        _tip = reportItems.Sum(ss => ss.Tip);
                        ws.Cell(row, column++).Value = _tip.ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, _tip);
                        }
                        else
                        {
                            total[column] += _tip;
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, _tip);
                        }
                        else
                        {
                            grandTotal[column] += _tip;
                        }

                        //=======================================================
                        //NAV - NoIncludeOnSale (- discount - promotion)
                        payAmountByCateTotal = 0;
                        for (int a = 0; a < _lstNoIncludeOnSaleGroupByCate.Count; a++)
                        {

                            payAmountByCate = (double)_lstNoIncludeOnSale.Where(p => p.StoreId == lstStore[i].Id
                            && p.CreatedDate >= lstBusDay[j].DateFrom && p.CreatedDate <= lstBusDay[j].DateTo
                                                        && p.CategoryId == _lstNoIncludeOnSaleGroupByCate[a].CategoryId)
                                                        .Sum(p => ((decimal)p.Amount));

                            payAmountByCateForNetSale = (double)_lstNoIncludeOnSale.Where(p => p.StoreId == lstStore[i].Id
                            && p.CreatedDate >= lstBusDay[j].DateFrom && p.CreatedDate <= lstBusDay[j].DateTo
                                                        && p.CategoryId == _lstNoIncludeOnSaleGroupByCate[a].CategoryId)
                                                        .Sum(p => ((decimal)p.Amount - (decimal)p.DiscountAmount - (decimal)p.PromotionAmount));

                            //   var tip = _lstTips.Where(ww => ww.StoreId == lstStore[i].Id && ww.CreatedDate >= lstBusDay[j].DateFrom && ww.CreatedDate <= lstBusDay[j].DateTo
                            //&& ww.PaymentId == storeCards[k]).Sum(ss => ss.Amount);

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
                            if (!grandTotal.ContainsKey(column))
                            {
                                grandTotal.Add(column, payAmountByCate);
                            }
                            else
                            {
                                grandTotal[column] += payAmountByCate;
                            }
                            //update 11/08/2017
                            //payAmountByCateTotal += payAmountByCate;
                            payAmountByCateTotal += payAmountByCateForNetSale;
                        }
                        //NAV - End NoIncludeOnSale
                        //=======================================================
                        var payGC = lstPayments.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= lstBusDay[j].DateFrom && p.CreatedDate <= lstBusDay[j].DateTo
                                                          && lstGC.Contains(p.PaymentId)).Sum(p => p.Amount);
                        //NetSales -NAV - GC
                        ws.Cell(row, column++).Value = (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC).ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC));
                        }
                        else
                        {
                            total[column] += (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC);
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC));
                        }
                        else
                        {
                            grandTotal[column] += (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC);
                        }
                        // pay by cash (subtract refund)


                        //payByCash = lstPayments.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= lstBusDay[j].DateFrom && p.CreatedDate <= lstBusDay[j].DateTo
                        //                        && p.PaymentName == lstNotInPayMent[0]).Sum(p => p.Amount) - reportItems.Sum(ss => ss.Refund);

                        payByCash = lstPayments.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= lstBusDay[j].DateFrom && p.CreatedDate <= lstBusDay[j].DateTo
                                                && (p.PaymentId == lstNotInPayMentNew[0] || (p.PaymentName == lstNotInPayMent[0]))
                                                ).Sum(p => p.Amount) - reportItems.Sum(ss => ss.Refund);

                        //var tipCash =_lstTips.Where(ww => ww.StoreId == lstStore[i].Id && ww.CreatedDate >= lstBusDay[j].DateFrom && ww.CreatedDate <= lstBusDay[j].DateTo
                        // && ww.PaymentName == lstNotInPayMent[0]).Sum(ss => ss.Amount);
                        ws.Cell(row, column++).Value = payByCash.ToString("F");

                        //payByCash += tipCash;
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, payByCash);
                        }
                        else
                        {
                            total[column] += payByCash;
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, payByCash);
                        }
                        else
                        {
                            grandTotal[column] += payByCash;
                        }

                        payTotal = payByCash;
                        //Payment methods other cash
                        for (int k = 0; k < storeCards.Count; k++)
                        {
                            if (storeCards[k] != "")
                                payAmount = lstPayments.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= lstBusDay[j].DateFrom && p.CreatedDate <= lstBusDay[j].DateTo
                                                            && p.PaymentId == storeCards[k]).Sum(p => p.Amount);
                            else
                                payAmount = 0;
                            //   var tip = _lstTips.Where(ww => ww.StoreId == lstStore[i].Id && ww.CreatedDate >= lstBusDay[j].DateFrom && ww.CreatedDate <= lstBusDay[j].DateTo
                            //&& ww.PaymentId == storeCards[k]).Sum(ss => ss.Amount);

                            //payAmount += tip;
                            ws.Cell(row, column++).Value = payAmount.ToString("F");
                            if (!total.ContainsKey(column))
                            {
                                total.Add(column, payAmount);
                            }
                            else
                            {
                                total[column] += payAmount;
                            }
                            //grand total
                            if (!grandTotal.ContainsKey(column))
                            {
                                grandTotal.Add(column, payAmount);
                            }
                            else
                            {
                                grandTotal[column] += payAmount;
                            }
                            payTotal += payAmount;
                        }
                        //PaymentTotal
                        ws.Cell(row, column++).Value = payTotal.ToString("F");
                        if (!total.ContainsKey(column))
                        {
                            total.Add(column, payTotal);
                        }
                        else
                        {
                            total[column] += payTotal;
                        }
                        //grand total
                        if (!grandTotal.ContainsKey(column))
                        {
                            grandTotal.Add(column, payTotal);
                        }
                        else
                        {
                            grandTotal[column] += payTotal;
                        }
                        //excess
                        //double excess = payTotal - (recieptTotal + _tip);
                        double excess = payTotal - receiptTotal;
                        ws.Cell(row, column).Value = excess.ToString("F");
                        if (!total.ContainsKey(column + 1))
                        {
                            total.Add(column + 1, excess);
                        }
                        else
                        {
                            total[column + 1] += excess;
                        }

                        //grand total
                        if (!grandTotal.ContainsKey(column + 1))
                        {
                            grandTotal.Add(column + 1, excess);
                        }
                        else
                        {
                            grandTotal[column + 1] += excess;
                        }

                        #endregion

                        row++;
                    }
                    //====================================================================================
                    //End loop Bussiness day
                    //====================================================================================
                }
                ws.Range(startRow, 1, row - 1, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                // Total Row
                ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                for (int j = 2; j <= maxColumn; j++)
                {
                    //ws.Cell(row, j).FormulaA1 = string.Format("=SUM({0}{1}:{0}{2})", GetColNameFromIndex(j), startRow, row - 1);
                    if (total.ContainsKey(j + 1))
                        ws.Cell(row, j).Value = total[j + 1];
                }
                ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                ws.Range(startRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                row++;
                //ws.Range(row, 1, row, maxColumn).Merge().SetValue("");
            }//end for list store
            //Grand total
            ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL"));
            for (int j = 2; j <= maxColumn; j++)
            {
                //ws.Cell(row, j).FormulaA1 = string.Format("=SUM({0}{1}:{0}{2})", GetColNameFromIndex(j), startRow, row - 1);
                if (grandTotal.ContainsKey(j + 1))
                    ws.Cell(row, j).Value = grandTotal[j + 1];
            }
            ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
            row++;
            ws.Range(row - 1, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

            ws.Range(5, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(5, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #region Table Header
            CreateReportHeaderNew(ws, maxColumn, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
            #endregion
            //set Width for Colum 
            List<int> lstWidCol = new List<int>() { 13, 8, 8, 12, 15, 10, 14, 8, 15, 10, 15, 14, 16, 16, 9, 12, 12 };
            for (int i = 0; i < maxColumn; i++)
            {
                if (i <= 14)
                {
                    ws.Column(i + 1).Width = lstWidCol[i];
                }
                else
                {
                    ws.Column(i + 1).Width = 14;
                }
            }
            return wb;
        }

        private int GetTaxType(string storeId)
        {
            TaxFactory factory = new TaxFactory();
            var taxes = factory.GetDetailTaxForStore(storeId);
            return taxes;
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

        // Get list Shift information
        public List<ShiftLogModels> GetListShiftsByBusDay(List<string> lstBusinessIds)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_ShiftLog
                               where lstBusinessIds.Contains(tb.BusinessId)
                               select new ShiftLogModels
                               {
                                   BusinessId = tb.BusinessId,
                                   StartedOn = tb.StartedOn,
                                   ClosedOn = tb.ClosedOn,
                                   StoreId = tb.StoreId
                               }).ToList();
                return lstData;
            }
        }

        // Updated 08242017
        public XLWorkbook ExportExcel_New(Models.Reports.BaseReportModel model, List<StoreModels> lstStore, Boolean isIncludeShift)
        {
            string sheetName = "Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dToFilter = model.ToDate;
            var lstStoreId = lstStore.Select(ss => ss.Id).ToList();
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreId, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            // Get list Shift info
            List<ShiftLogModels> lstShiftInfo = new List<ShiftLogModels>();
            List<int> lstShiftRow = new List<int>();
            if (isIncludeShift)
            {
                var lstIdBusDayAllStore = _lstBusDayAllStore.Select(s => s.Id).ToList();
                lstShiftInfo = GetListShiftsByBusDay(lstIdBusDayAllStore);
            }

            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
            List<DailySalesReportInsertModels> lstData = GetDataReceiptItems(model);
            List<PaymentModels> lstPayments = GetDataPaymentItems(model);
            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
            if (cash == null)
                cash = new RFilterChooseExtBaseModel();
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();

            List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSale = _itemizedSalesAnalysisFactory.GetItemsForDailySalesReports(model);
            var lstItemNoIncludeSaleForCate = lstItemNoIncludeSale.Where(w => !string.IsNullOrEmpty(w.GLAccountCode)).ToList();
            var lstItemNoIncludeSaleForNetSales = lstItemNoIncludeSale.Where(w => w.IsIncludeSale == false).ToList();
            var lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
            var lstMiscs = _DiscountAndMiscReportFactory.GetMiscs(model);

            //get list categories
            CategoryApiRequestModel cateRequest = new CategoryApiRequestModel();
            cateRequest.ListStoreIds = model.ListStores;
            var _lstCates = _categoriesFactory.GetAllCategoriesForDailySale(cateRequest);

            //string sheetName = "Daily_Sales_Report";
            //var wb = new XLWorkbook();
            //var ws = wb.Worksheets.Add(sheetName);
            int maxColumn = 0;

            int row = 4;
            int column = 0;
            double receiptTotal = 0, payTotal = 0, payAmount = 0, noSale = 0, payByCash = 0, _tip = 0;
            double payAmountByCate = 0;
            double payAmountByCateRoundBefore = 0;
            double payAmountByCateForNetSale = 0;
            double payAmountByCateTotal = 0;
            bool _isTaxInclude = true;

            double _roudingTotal = 0;
            //DateTime dFrom, dTo;
            Dictionary<int, double> total = new Dictionary<int, double>();
            Dictionary<int, double> grandTotal = new Dictionary<int, double>();
            TopSellingProductsReportFactory topSellFactory = new TopSellingProductsReportFactory();
            //var lstStoreId = lstStore.Select(ss => ss.Id).ToList();
            //var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreId, model.Mode);

            for (int i = 0; i < lstStore.Count; i++)
            {
                _isTaxInclude = true;
                column = 1;
                // store name
                row++;
                ws.Range("A" + row, "C" + row).Merge().SetValue(lstStore[i].Name);

                #region Columns Names
                row++;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No Sale"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                int receiptTotalColumn = column;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotions"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Svc Chg"));
                if (GetTaxType(lstStore[i].Id) == (int)Commons.ETax.AddOn)
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                    _isTaxInclude = false;
                }
                else
                {
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                }
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                int refundColumn = column;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                //=============================================================
                //Check no include on sale
                //=============================================================
                lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
                //update -> show all categories have GLCode (12/12/2017)
                if (_lstCates != null && _lstCates.Any())
                {
                    var lstNoIncludeOnSaleGroupByCate = _lstCates.GroupBy(gg => gg.GLCode).OrderBy(x => x.Key).ToList();
                    for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                    {
                        lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                        {
                            GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                        });
                        if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                        {
                            ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                        }
                        else
                            ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                    }
                }
                else // case truong hop truoc k
                {
                    if (lstItemNoIncludeSaleForCate != null && lstItemNoIncludeSaleForCate.Any())
                    {
                        var lstNoIncludeOnSaleGroupByCate = lstItemNoIncludeSaleForCate.GroupBy(gg => gg.GLAccountCode).OrderBy(x => x.Key).ToList();
                        for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                        {
                            lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                            {
                                GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                            });
                            if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                            {
                                ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                            }
                            else
                                ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                        }
                    }
                }
                ///End Check no include on sale
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                int startPayColumn = column;
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                List<string> lstNotInPayMent = new List<string>() { "Cash" };
                List<string> lstNotInPayMentNew = new List<string>() { cash != null ? cash.Id : "" };

                List<string> storeCards = new List<string>();

                var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && (ww.Id != cash.Id || ww.Name.ToLower() != "cash")
                    && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();

                for (int p = 0; p < lstPaymentParents.Count; p++)
                {
                    lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                    && ww.StoreId == lstStore[i].Id).OrderBy(ww => ww.Name).ToList();
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

                int endPayColumn = column;
                // end group payments
                ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                #endregion

                // set value for last column
                if (maxColumn < column)
                    maxColumn = column;

                #region Format Store Header
                ws.Range(row - 1, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                ws.Range(row - 1, 1, row + 1, column).Style.Font.SetBold(true);
                ws.Range(row - 1, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Range(row, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                #endregion

                row += 2;
                int startRow = row;

                var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == lstStore[i].Id).ToList();
                List<DailySalesReportInsertModels> lstDataByStore = lstData.Where(ww => ww.StoreId == lstStore[i].Id).ToList();
                total = new Dictionary<int, double>();
                if (lstDataByStore != null && lstDataByStore.Count > 0)
                {
                    // Fill data==================================================================
                    //============================================================================
                    payTotal = 0; payAmount = 0;

                    List<BusinessDayDisplayModels> lstBusDayShift = new List<BusinessDayDisplayModels>();

                    for (int j = 0; j < lstBusDay.Count; j++)
                    {
                        lstBusDayShift = new List<BusinessDayDisplayModels>();
                        lstBusDayShift.Add(lstBusDay[j]);
                        if (lstShiftInfo != null && lstShiftInfo.Any())
                        {
                            var lstShiftByBusDayInStore = lstShiftInfo.Where(w => w.BusinessId == lstBusDay[j].Id && w.StoreId == lstBusDay[j].StoreId)
                                .OrderBy(o => o.StartedOn).ToList();

                            if (lstShiftByBusDayInStore != null && lstShiftByBusDayInStore.Any())
                            {
                                foreach (var shift in lstShiftByBusDayInStore)
                                {
                                    var obj = new BusinessDayDisplayModels();
                                    obj.DateFrom = shift.StartedOn;
                                    if (shift.ClosedOn == Commons.MinDate)
                                    {
                                        obj.DateTo = new DateTime(shift.StartedOn.Year, shift.StartedOn.Month, shift.StartedOn.Day, 23, 59, 59);
                                    }
                                    else
                                    {
                                        obj.DateTo = shift.ClosedOn;
                                    }
                                    lstBusDayShift.Add(obj);
                                }
                            }
                        }
                        int numberShift = 0;
                        foreach (var BusDayShift in lstBusDayShift)
                        {
                            column = 1;
                            payTotal = 0;
                            payAmount = 0;

                            var reportItems = lstDataByStore.Where(s => s.CreatedDate >= BusDayShift.DateFrom && s.CreatedDate <= BusDayShift.DateTo).ToList();
                            if (reportItems == null || reportItems.Count == 0)
                                continue;

                            // If this is Shift
                            if (string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                numberShift++;
                                lstShiftRow.Add(row);
                                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Cell(row, column++).SetValue("   Shift" + numberShift + " " + BusDayShift.DateFrom.ToString("HH:mm") + " - " + BusDayShift.DateTo.ToString("HH:mm"));
                            }
                            // If this is Business day
                            else
                            {
                                ws.Cell(row, column++).SetValue(BusDayShift.DateFrom.ToString("MM/dd/yyyy"));
                            }

                            #region add value to cell
                            receiptTotal = reportItems.Sum(ss => ss.ReceiptTotal);
                            //NoSale
                            noSale = _noSaleDetailReportFactory.GetNoSale(lstStore[i].Id, BusDayShift.DateFrom, BusDayShift.DateTo);
                            ws.Cell(row, column++).Value = noSale;
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, noSale);
                                }
                                else
                                {
                                    total[column] += noSale;
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, noSale);
                                }
                                else
                                {
                                    grandTotal[column] += noSale;
                                }
                            }

                            //TC - NoOfReceipt
                            ws.Cell(row, column++).Value = reportItems.Count;
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, reportItems.Count);
                                }
                                else
                                {
                                    total[column] += reportItems.Count;
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, reportItems.Count);
                                }
                                else
                                {
                                    grandTotal[column] += reportItems.Count;
                                }
                            }

                            //PAX - NoOfPerson
                            ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.NoOfPerson);
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                                }
                                else
                                {
                                    total[column] += reportItems.Sum(ss => ss.NoOfPerson);
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                                }
                                else
                                {
                                    grandTotal[column] += reportItems.Sum(ss => ss.NoOfPerson);
                                }
                            }

                            // ReceiptTotal
                            ws.Cell(row, column++).Value = receiptTotal.ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, receiptTotal);
                                }
                                else
                                {
                                    total[column] += receiptTotal;
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, receiptTotal);
                                }
                                else
                                {
                                    grandTotal[column] += receiptTotal;
                                }
                            }

                            //Discount
                            ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Discount).ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, reportItems.Sum(ss => ss.Discount));
                                }
                                else
                                {
                                    total[column] += reportItems.Sum(ss => ss.Discount);
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, reportItems.Sum(ss => ss.Discount));
                                }
                                else
                                {
                                    grandTotal[column] += reportItems.Sum(ss => ss.Discount);
                                }
                            }

                            //promotions
                            ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.PromotionValue).ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                }
                                else
                                {
                                    total[column] += reportItems.Sum(ss => ss.PromotionValue);
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                }
                                else
                                {
                                    grandTotal[column] += reportItems.Sum(ss => ss.PromotionValue);
                                }
                            }

                            //ServiceCharge
                            ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.ServiceCharge).ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                }
                                else
                                {
                                    total[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                }
                                else
                                {
                                    grandTotal[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                }
                            }

                            //Tax
                            ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.GST).ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, reportItems.Sum(ss => ss.GST));
                                }
                                else
                                {
                                    total[column] += reportItems.Sum(ss => ss.GST);
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, reportItems.Sum(ss => ss.GST));
                                }
                                else
                                {
                                    grandTotal[column] += reportItems.Sum(ss => ss.GST);
                                }
                            }

                            _roudingTotal = reportItems.Sum(ss => ss.Rounding);
                            //Rounding
                            ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Rounding).ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, reportItems.Sum(ss => ss.Rounding));
                                }
                                else
                                {
                                    total[column] += reportItems.Sum(ss => ss.Rounding);
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, reportItems.Sum(ss => ss.Rounding));
                                }
                                else
                                {
                                    grandTotal[column] += reportItems.Sum(ss => ss.Rounding);
                                }
                            }

                            //Refund
                            ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Refund).ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, reportItems.Sum(ss => ss.Refund));
                                }
                                else
                                {
                                    total[column] += reportItems.Sum(ss => ss.Refund);
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, reportItems.Sum(ss => ss.Refund));
                                }
                                else
                                {
                                    grandTotal[column] += reportItems.Sum(ss => ss.Refund);
                                }
                            }

                            //Tip
                            _tip = reportItems.Sum(ss => ss.Tip);
                            ws.Cell(row, column++).Value = _tip.ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, _tip);
                                }
                                else
                                {
                                    total[column] += _tip;
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, _tip);
                                }
                                else
                                {
                                    grandTotal[column] += _tip;
                                }
                            }
                            #region NetSales
                            double _netSale = 0;
                            if (!_isTaxInclude)
                            {
                                payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == lstStore[i].Id
                               && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo)
                                                           .Sum(p => (decimal)p.TotalAmount
                                                           - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount);
                            }
                            else
                                payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == lstStore[i].Id
                                    && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo)
                                                                .Sum(p => (decimal)p.TotalAmount
                                                                - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount
                                                                - (decimal)p.Tax);

                            payAmountByCateTotal += payAmountByCateForNetSale;

                            var payGC = lstPayments.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= BusDayShift.DateFrom
                                                            && p.CreatedDate <= BusDayShift.DateTo
                                                         && lstGC.Contains(p.PaymentId)).Sum(p => p.Amount);

                            _netSale = reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal;
                            #endregion End NetSales
                            //=======================================================
                            //NAV - NoIncludeOnSale (- discount - promotion)
                            // GL account code and group by it
                            //Note: value is not include tax, right? It means
                            //Tax is include on item price -> value = [[(price of items x qty ) - (disc or promo )] +SVC ] -Tax
                            //Tax is add on ->value = [(price of items x qty ) - (disc or promo )] +SVC
                            payAmountByCateTotal = 0;
                            decimal difTax = 0;
                            for (int a = 0; a < lstItemNoIncludeSaleGroupByCate.Count; a++)
                            {
                                //tax = 0;
                                if (_isTaxInclude)
                                {

                                    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                            && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                        && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                        .Sum(p => ((decimal)p.TotalAmount
                                                            - (decimal)p.TotalDiscount
                                                            - (decimal)p.PromotionAmount
                                                                - (decimal)p.Tax));


                                    payAmountByCateRoundBefore = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                            && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                        && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                        .Sum(p => ((decimal)p.TotalAmount
                                                            - (decimal)CommonHelper.RoundUp2Decimal(p.TotalDiscount.Value)
                                                            - (decimal)CommonHelper.RoundUp2Decimal(p.PromotionAmount)
                                                                - (decimal)p.Tax));

                                }
                                else
                                {
                                    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                    && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                                .Sum(p => ((decimal)p.TotalAmount -
                                                                (decimal)p.TotalDiscount.Value
                                                                - (decimal)p.PromotionAmount));

                                    payAmountByCateRoundBefore = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                                  && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                              && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                              .Sum(p => ((decimal)p.TotalAmount -
                                                              (decimal)CommonHelper.RoundUp2Decimal(p.TotalDiscount.Value)
                                                              - (decimal)CommonHelper.RoundUp2Decimal(p.PromotionAmount)));

                                }
                                //if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim() == Commons.TENANT_SALES)
                                if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                {
                                    if (payAmountByCate > payAmountByCateRoundBefore)
                                    {
                                        payAmountByCate += (payAmountByCate - payAmountByCateRoundBefore);
                                    }

                                    var taxTotal = reportItems.Sum(ss => ss.GST);
                                    difTax = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == lstStore[i].Id
                          && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                      && !string.IsNullOrEmpty(p.GLAccountCode))
                                                      .Sum(p => ((decimal)p.Tax));

                                    payAmountByCate += lstMiscs.Where(ww => ww.StoreId == lstStore[i].Id && ww.CreatedDate >= BusDayShift.DateFrom
                                            && ww.CreatedDate <= BusDayShift.DateTo).Sum(ss => ss.MiscValue);
                                    payAmountByCate += _roudingTotal;
                                    payAmountByCate -= (double)((decimal)taxTotal - difTax);
                                    if (payAmountByCate > _netSale)
                                    {
                                        payAmountByCate -= (payAmountByCate - _netSale);
                                    }
                                }
                                if (payAmountByCate < 0)
                                {
                                    payAmountByCate = 0;
                                }
                                ws.Cell(row, column++).Value = payAmountByCate.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payAmountByCate);
                                    }
                                    else
                                    {
                                        total[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payAmountByCate);
                                    }
                                    else
                                    {
                                        grandTotal[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                    }
                                }
                            }

                            //if (!_isTaxInclude)
                            //{
                            //    payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == lstStore[i].Id
                            //   && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo)
                            //                               .Sum(p => (decimal)p.ExtraAmount + (decimal)p.TotalAmount
                            //                               - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount);
                            //}
                            //else
                            //    payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == lstStore[i].Id
                            //        && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo)
                            //                                    .Sum(p => (decimal)p.ExtraAmount + (decimal)p.TotalAmount
                            //                                    - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount
                            //                                    - (decimal)p.Tax);

                            //payAmountByCateTotal += payAmountByCateForNetSale;

                            //var payGC = lstPayments.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= BusDayShift.DateFrom
                            //                                && p.CreatedDate <= BusDayShift.DateTo
                            //                             && lstGC.Contains(p.PaymentId)).Sum(p => p.Amount);
                            //NAV - End NoIncludeOnSale
                            //=======================================================
                            //NetSales -NAV - payGC 
                            //ws.Cell(row, column++).Value = (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal).ToString("F");
                            ws.Cell(row, column++).Value = _netSale.ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                //if (!total.ContainsKey(column))
                                //{
                                //    total.Add(column, (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal));
                                //}
                                //else
                                //{
                                //    total[column] += (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal);
                                //}
                                ////grand total
                                //if (!grandTotal.ContainsKey(column))
                                //{
                                //    grandTotal.Add(column, (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal));
                                //}
                                //else
                                //{
                                //    grandTotal[column] += (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal);
                                //}

                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, _netSale);
                                }
                                else
                                {
                                    total[column] += _netSale;
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, _netSale);
                                }
                                else
                                {
                                    grandTotal[column] += _netSale;
                                }
                            }

                            // pay by cash (subtract refund)
                            payByCash = lstPayments.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                    && (p.PaymentId == lstNotInPayMentNew[0] || (p.PaymentName == lstNotInPayMent[0]))
                                                    ).Sum(p => p.Amount) - reportItems.Sum(ss => ss.Refund);

                            ws.Cell(row, column++).Value = payByCash.ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, payByCash);
                                }
                                else
                                {
                                    total[column] += payByCash;
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, payByCash);
                                }
                                else
                                {
                                    grandTotal[column] += payByCash;
                                }
                            }

                            payTotal = payByCash;
                            //Payment methods other cash
                            for (int k = 0; k < storeCards.Count; k++)
                            {
                                if (storeCards[k] != "")
                                    payAmount = lstPayments.Where(p => p.StoreId == lstStore[i].Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                && p.PaymentId == storeCards[k]).Sum(p => p.Amount);
                                else
                                    payAmount = 0;

                                ws.Cell(row, column++).Value = payAmount.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payAmount);
                                    }
                                    else
                                    {
                                        total[column] += payAmount;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payAmount);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payAmount;
                                    }
                                }
                                payTotal += payAmount;
                            }

                            //PaymentTotal
                            ws.Cell(row, column++).Value = payTotal.ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column))
                                {
                                    total.Add(column, payTotal);
                                }
                                else
                                {
                                    total[column] += payTotal;
                                }
                                //grand total
                                if (!grandTotal.ContainsKey(column))
                                {
                                    grandTotal.Add(column, payTotal);
                                }
                                else
                                {
                                    grandTotal[column] += payTotal;
                                }
                            }

                            //excess
                            //double excess = payTotal - (receiptTotal + _tip);
                            double excess = payTotal - receiptTotal;
                            ws.Cell(row, column).Value = excess.ToString("F");
                            // If this is business day
                            if (!string.IsNullOrEmpty(BusDayShift.Id))
                            {
                                if (!total.ContainsKey(column + 1))
                                {
                                    total.Add(column + 1, excess);
                                }
                                else
                                {
                                    total[column + 1] += excess;
                                }

                                //grand total
                                if (!grandTotal.ContainsKey(column + 1))
                                {
                                    grandTotal.Add(column + 1, excess);
                                }
                                else
                                {
                                    grandTotal[column + 1] += excess;
                                }
                            }
                            #endregion

                            row++;
                        }
                    }
                    //====================================================================================
                    //End loop Bussiness day
                    //====================================================================================
                }
                //ws.Range(startRow, 1, row - 1, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                // Total Row
                ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                for (int j = 2; j <= maxColumn; j++)
                {
                    if (total.ContainsKey(j + 1))
                        ws.Cell(row, j).Value = total[j + 1];
                }
                ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                ws.Range(startRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                row++;

            }//end for list store
            //Grand total
            ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL"));
            for (int j = 2; j <= maxColumn; j++)
            {
                if (grandTotal.ContainsKey(j + 1))
                    ws.Cell(row, j).Value = grandTotal[j + 1];
            }
            ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
            row++;
            ws.Range(row - 1, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

            ws.Range(5, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(5, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #region Table Header
            CreateReportHeaderNew(ws, maxColumn, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
            ws.Range(1, 1, 4, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            #endregion

            // Format Shift row
            if (lstShiftRow != null && lstShiftRow.Any())
            {
                foreach (int shiftRow in lstShiftRow)
                {
                    ws.Range(shiftRow, 1, shiftRow, maxColumn).Style.Font.SetFontColor(XLColor.FromHtml("#974706"));
                }
            }

            //set Width for Colum 
            List<int> lstWidCol = new List<int>() { 30, 8, 8, 12, 15, 10, 14, 8, 15, 10, 15, 14, 16, 16, 9, 12, 12 };
            for (int i = 0; i < maxColumn; i++)
            {
                if (i <= 14)
                {
                    ws.Column(i + 1).Width = lstWidCol[i];
                }
                else
                {
                    ws.Column(i + 1).Width = 14;
                }
            }
            return wb;
        }

        public XLWorkbook ExportExcel_NewForMerchantExtend(Models.Reports.BaseReportModel model, List<StoreModels> lstStore, Boolean isIncludeShift)
        {
            string sheetName = "Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dToFilter = model.ToDate;
            var lstStoreId = lstStore.Select(ss => ss.Id).ToList();
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreId, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            // Get list Shift info
            List<ShiftLogModels> lstShiftInfo = new List<ShiftLogModels>();
            List<int> lstShiftRow = new List<int>();
            if (isIncludeShift)
            {
                var lstIdBusDayAllStore = _lstBusDayAllStore.Select(s => s.Id).ToList();
                lstShiftInfo = GetListShiftsByBusDay(lstIdBusDayAllStore);
            }

            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
            List<DailySalesReportInsertModels> lstData = GetDataReceiptItems(model);
            List<PaymentModels> lstPayments = GetDataPaymentItems(model);

            //-----------------------------------------------
            ////Old
            //var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            //var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
            //if (cash == null)
            //    cash = new RFilterChooseExtBaseModel();
            //var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            //if (lstGC == null)
            //    lstGC = new List<string>();
            //-----------------------------------------------

            List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSale = _itemizedSalesAnalysisFactory.GetItemsForDailySalesReports(model);
            var lstItemNoIncludeSaleForCate = lstItemNoIncludeSale.Where(w => !string.IsNullOrEmpty(w.GLAccountCode)).ToList();
            var lstItemNoIncludeSaleForNetSales = lstItemNoIncludeSale.Where(w => w.IsIncludeSale == false).ToList();
            var lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
            var lstMiscs = _DiscountAndMiscReportFactory.GetMiscs(model);

            //string sheetName = "Daily_Sales_Report";
            //var wb = new XLWorkbook();
            //var ws = wb.Worksheets.Add(sheetName);
            int maxColumn = 0;

            int row = 4;
            int column = 0;
            double receiptTotal = 0, payTotal = 0, payAmount = 0, noSale = 0, payByCash = 0, _tip = 0;
            double payAmountByCate = 0;
            double payAmountByCateRoundBefore = 0;
            double payAmountByCateForNetSale = 0;
            double payAmountByCateTotal = 0;
            bool _isTaxInclude = true;

            double _roudingTotal = 0;
            //DateTime dFrom, dTo;
            Dictionary<int, double> total = new Dictionary<int, double>();
            Dictionary<int, double> totalCompany = new Dictionary<int, double>();
            Dictionary<int, double> grandTotal = new Dictionary<int, double>();
            TopSellingProductsReportFactory topSellFactory = new TopSellingProductsReportFactory();

            //Show group company
            var lstCompanies = lstStore.Select(ss => new CompanyModels()
            {
                Id = ss.CompanyId,
                Name = ss.CompanyName,
                ApiUrlExtend = ss.HostUrlExtend
            }).Distinct().OrderBy(oo => oo.Name).ToList();
            //Group company
            StoreModels _currentStore = null;
            int currentCompanyRow = 0;
            foreach (var com in lstCompanies)
            {
                // show company name

                row++;
                ws.Range("A" + row, "C" + row).Merge().SetValue(com.Name);


                totalCompany = new Dictionary<int, double>();
                var lstStoreFollowCompany = lstStore.Where(ww => ww.CompanyId == com.Id).OrderBy(ss => ss.Name).ToList();
                for (int i = 0; i < lstStoreFollowCompany.Count; i++)
                {
                    _currentStore = lstStoreFollowCompany[i];
                    //get payment
                    //Old
                    var lstPaymentMethod = _baseFactory.GetAllPaymentForMerchantExtendReport(com.ApiUrlExtend, new Models.Api.CategoryApiRequestModel() { ListStoreIds = new List<string> { _currentStore.Id } });

                    var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
                    if (cash == null)
                        cash = new RFilterChooseExtBaseModel();
                    var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                    if (lstGC == null)
                        lstGC = new List<string>();
                    //------------------------------------------------------------------------------------------------------------

                    _isTaxInclude = true;
                    column = 1;
                    // show store name
                    row++;
                    ws.Range("A" + row, "C" + row).Merge().SetValue(_currentStore.Name);

                    #region Columns Names
                    row++;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No Sale"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                    int receiptTotalColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotions"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Svc Chg"));
                    if (!_currentStore.IsIncludeTax)
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                        _isTaxInclude = false;
                    }
                    else
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                    }
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                    int refundColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                    //=============================================================
                    //Check no include on sale
                    //=============================================================
                    lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
                    if (lstItemNoIncludeSaleForCate != null && lstItemNoIncludeSaleForCate.Any())
                    {
                        var lstNoIncludeOnSaleGroupByCate = lstItemNoIncludeSaleForCate.Where(ww => ww.StoreId == _currentStore.Id)
                            .GroupBy(gg => gg.GLAccountCode).OrderBy(x => x.Key).ToList();
                        for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                        {
                            lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                            {
                                GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                            });
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
                    int startPayColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                    List<string> lstNotInPayMent = new List<string>() { "Cash" };
                    List<string> lstNotInPayMentNew = new List<string>() { cash != null ? cash.Id : "" };

                    List<string> storeCards = new List<string>();

                    var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && (ww.Id != cash.Id || ww.Name.ToLower() != "cash")
                        && ww.StoreId == _currentStore.Id).OrderBy(ww => ww.Name).ToList();

                    for (int p = 0; p < lstPaymentParents.Count; p++)
                    {
                        lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                        && ww.StoreId == _currentStore.Id).OrderBy(ww => ww.Name).ToList();
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

                    int endPayColumn = column;
                    // end group payments
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                    ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                    #endregion

                    // set value for last column
                    if (maxColumn < column)
                        maxColumn = column;

                    #region Format Store Header
                    ws.Range(row - 2, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                    ws.Range(row - 2, 1, row + 1, column).Style.Font.SetBold(true);
                    ws.Range(row - 2, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                    ws.Range(row, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                    ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    #endregion

                    row += 2;
                    int startRow = row;

                    var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == _currentStore.Id).ToList();
                    List<DailySalesReportInsertModels> lstDataByStore = lstData.Where(ww => ww.StoreId == _currentStore.Id).ToList();
                    total = new Dictionary<int, double>();
                    if (lstDataByStore != null && lstDataByStore.Count > 0)
                    {
                        // Fill data==================================================================
                        //============================================================================
                        payTotal = 0; payAmount = 0;

                        List<BusinessDayDisplayModels> lstBusDayShift = new List<BusinessDayDisplayModels>();

                        for (int j = 0; j < lstBusDay.Count; j++)
                        {
                            lstBusDayShift = new List<BusinessDayDisplayModels>();
                            lstBusDayShift.Add(lstBusDay[j]);
                            if (lstShiftInfo != null && lstShiftInfo.Any())
                            {
                                var lstShiftByBusDayInStore = lstShiftInfo.Where(w => w.BusinessId == lstBusDay[j].Id && w.StoreId == lstBusDay[j].StoreId)
                                    .OrderBy(o => o.StartedOn).ToList();

                                if (lstShiftByBusDayInStore != null && lstShiftByBusDayInStore.Any())
                                {
                                    foreach (var shift in lstShiftByBusDayInStore)
                                    {
                                        var obj = new BusinessDayDisplayModels();
                                        obj.DateFrom = shift.StartedOn;
                                        if (shift.ClosedOn == Commons.MinDate)
                                        {
                                            obj.DateTo = new DateTime(shift.StartedOn.Year, shift.StartedOn.Month, shift.StartedOn.Day, 23, 59, 59);
                                        }
                                        else
                                        {
                                            obj.DateTo = shift.ClosedOn;
                                        }
                                        lstBusDayShift.Add(obj);
                                    }
                                }
                            }
                            int numberShift = 0;
                            foreach (var BusDayShift in lstBusDayShift)
                            {
                                column = 1;
                                payTotal = 0;
                                payAmount = 0;

                                var reportItems = lstDataByStore.Where(s => s.CreatedDate >= BusDayShift.DateFrom && s.CreatedDate <= BusDayShift.DateTo).ToList();
                                if (reportItems == null || reportItems.Count == 0)
                                    continue;

                                // If this is Shift
                                if (string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    numberShift++;
                                    lstShiftRow.Add(row);
                                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell(row, column++).SetValue("   Shift" + numberShift + " " + BusDayShift.DateFrom.ToString("HH:mm") + " - " + BusDayShift.DateTo.ToString("HH:mm"));
                                }
                                // If this is Business day
                                else
                                {
                                    ws.Cell(row, column++).SetValue(BusDayShift.DateFrom.ToString("MM/dd/yyyy"));
                                }

                                #region add value to cell
                                receiptTotal = reportItems.Sum(ss => ss.ReceiptTotal);
                                //NoSale
                                noSale = _noSaleDetailReportFactory.GetNoSale(_currentStore.Id, BusDayShift.DateFrom, BusDayShift.DateTo);
                                ws.Cell(row, column++).Value = noSale;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, noSale);
                                    }
                                    else
                                    {
                                        total[column] += noSale;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, noSale);
                                    }
                                    else
                                    {
                                        totalCompany[column] += noSale;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, noSale);
                                    }
                                    else
                                    {
                                        grandTotal[column] += noSale;
                                    }
                                }

                                //TC - NoOfReceipt
                                ws.Cell(row, column++).Value = reportItems.Count;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Count);
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Count;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Count);
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Count;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Count);
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Count;
                                    }
                                }

                                //PAX - NoOfPerson
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.NoOfPerson);
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.NoOfPerson);
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.NoOfPerson);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.NoOfPerson);
                                    }
                                }

                                // ReceiptTotal
                                ws.Cell(row, column++).Value = receiptTotal.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        total[column] += receiptTotal;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        totalCompany[column] += receiptTotal;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        grandTotal[column] += receiptTotal;
                                    }
                                }

                                //Discount
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Discount).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                }

                                //promotions
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.PromotionValue).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                }

                                //ServiceCharge
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.ServiceCharge).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                }

                                //Tax
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.GST).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                }

                                _roudingTotal = reportItems.Sum(ss => ss.Rounding);
                                //Rounding
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Rounding).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                }

                                //Refund
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Refund).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Refund));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Refund);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Refund));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Refund);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Refund));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Refund);
                                    }
                                }

                                //Tip
                                _tip = reportItems.Sum(ss => ss.Tip);
                                ws.Cell(row, column++).Value = _tip.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, _tip);
                                    }
                                    else
                                    {
                                        total[column] += _tip;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, _tip);
                                    }
                                    else
                                    {
                                        totalCompany[column] += _tip;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, _tip);
                                    }
                                    else
                                    {
                                        grandTotal[column] += _tip;
                                    }
                                }
                                #region NetSales
                                double _netSale = 0;
                                if (!_isTaxInclude)
                                {
                                    payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == _currentStore.Id
                                   && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo)
                                                               .Sum(p => (decimal)p.TotalAmount
                                                               - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount);
                                }
                                else
                                    payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == _currentStore.Id
                                        && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo)
                                                                    .Sum(p => (decimal)p.TotalAmount
                                                                    - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount
                                                                    - (decimal)p.Tax);

                                payAmountByCateTotal += payAmountByCateForNetSale;

                                var payGC = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom
                                                                && p.CreatedDate <= BusDayShift.DateTo
                                                             && lstGC.Contains(p.PaymentId)).Sum(p => p.Amount);

                                _netSale = reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal;
                                #endregion End NetSales
                                //=======================================================
                                //NAV - NoIncludeOnSale (- discount - promotion)
                                // GL account code and group by it
                                //Note: value is not include tax, right? It means
                                //Tax is include on item price -> value = [[(price of items x qty ) - (disc or promo )] +SVC ] -Tax
                                //Tax is add on ->value = [(price of items x qty ) - (disc or promo )] +SVC
                                payAmountByCateTotal = 0;
                                decimal difTax = 0;
                                for (int a = 0; a < lstItemNoIncludeSaleGroupByCate.Count; a++)
                                {
                                    //tax = 0;
                                    if (_isTaxInclude)
                                    {
                                        //        var aa = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                        //&& p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                        //                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode).ToList();

                                        payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                            .Sum(p => ((decimal)p.TotalAmount
                                                                - (decimal)p.TotalDiscount
                                                                - (decimal)p.PromotionAmount
                                                                    - (decimal)p.Tax));


                                        payAmountByCateRoundBefore = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                            .Sum(p => ((decimal)p.TotalAmount
                                                                - (decimal)CommonHelper.RoundUp2Decimal(p.TotalDiscount.Value)
                                                                - (decimal)CommonHelper.RoundUp2Decimal(p.PromotionAmount)
                                                                    - (decimal)p.Tax));

                                    }
                                    else
                                    {
                                        payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                        && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                    && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                                    .Sum(p => ((decimal)p.TotalAmount -
                                                                    (decimal)p.TotalDiscount.Value
                                                                    - (decimal)p.PromotionAmount));

                                        payAmountByCateRoundBefore = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                      && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                  && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                                  .Sum(p => ((decimal)p.TotalAmount -
                                                                  (decimal)CommonHelper.RoundUp2Decimal(p.TotalDiscount.Value)
                                                                  - (decimal)CommonHelper.RoundUp2Decimal(p.PromotionAmount)));

                                    }
                                    //if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim() == Commons.TENANT_SALES)
                                    if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                    {
                                        if (payAmountByCate > payAmountByCateRoundBefore)
                                        {
                                            payAmountByCate += (payAmountByCate - payAmountByCateRoundBefore);
                                        }

                                        var taxTotal = reportItems.Sum(ss => ss.GST);
                                        difTax = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                              && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                          && !string.IsNullOrEmpty(p.GLAccountCode))
                                                          .Sum(p => ((decimal)p.Tax));

                                        payAmountByCate += lstMiscs.Where(ww => ww.StoreId == _currentStore.Id && ww.CreatedDate >= BusDayShift.DateFrom
                                                && ww.CreatedDate <= BusDayShift.DateTo).Sum(ss => ss.MiscValue);
                                        payAmountByCate += _roudingTotal;
                                        payAmountByCate -= (double)((decimal)taxTotal - difTax);
                                        if (payAmountByCate > _netSale)
                                        {
                                            payAmountByCate -= (payAmountByCate - _netSale);
                                        }
                                    }

                                    ws.Cell(row, column++).Value = payAmountByCate.ToString("F");
                                    // If this is business day
                                    if (!string.IsNullOrEmpty(BusDayShift.Id))
                                    {
                                        if (!total.ContainsKey(column))
                                        {
                                            total.Add(column, payAmountByCate);
                                        }
                                        else
                                        {
                                            total[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                        //total company
                                        if (!totalCompany.ContainsKey(column))
                                        {
                                            totalCompany.Add(column, payAmountByCate);
                                        }
                                        else
                                        {
                                            totalCompany[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                        //grand total
                                        if (!grandTotal.ContainsKey(column))
                                        {
                                            grandTotal.Add(column, payAmountByCate);
                                        }
                                        else
                                        {
                                            grandTotal[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                    }
                                }


                                //NAV - End NoIncludeOnSale
                                //=======================================================
                                //NetSales -NAV - payGC 
                                //ws.Cell(row, column++).Value = (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal).ToString("F");
                                ws.Cell(row, column++).Value = _netSale.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {

                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        total[column] += _netSale;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        totalCompany[column] += _netSale;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        grandTotal[column] += _netSale;
                                    }
                                }

                                // pay by cash (subtract refund)
                                payByCash = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                        && (p.PaymentId == lstNotInPayMentNew[0] || (p.PaymentName == lstNotInPayMent[0]))
                                                        ).Sum(p => p.Amount) - reportItems.Sum(ss => ss.Refund);

                                ws.Cell(row, column++).Value = payByCash.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        total[column] += payByCash;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        totalCompany[column] += payByCash;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payByCash;
                                    }
                                }

                                payTotal = payByCash;
                                //Payment methods other cash
                                for (int k = 0; k < storeCards.Count; k++)
                                {
                                    if (storeCards[k] != "")
                                        payAmount = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                    && p.PaymentId == storeCards[k]).Sum(p => p.Amount);
                                    else
                                        payAmount = 0;

                                    ws.Cell(row, column++).Value = payAmount.ToString("F");
                                    // If this is business day
                                    if (!string.IsNullOrEmpty(BusDayShift.Id))
                                    {
                                        if (!total.ContainsKey(column))
                                        {
                                            total.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            total[column] += payAmount;
                                        }
                                        //total company
                                        if (!totalCompany.ContainsKey(column))
                                        {
                                            totalCompany.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            totalCompany[column] += payAmount;
                                        }
                                        //grand total
                                        if (!grandTotal.ContainsKey(column))
                                        {
                                            grandTotal.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            grandTotal[column] += payAmount;
                                        }
                                    }
                                    payTotal += payAmount;
                                }

                                //PaymentTotal
                                ws.Cell(row, column++).Value = payTotal.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        total[column] += payTotal;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        totalCompany[column] += payTotal;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payTotal;
                                    }
                                }

                                //excess
                                //double excess = payTotal - (receiptTotal + _tip);
                                double excess = payTotal - receiptTotal;
                                ws.Cell(row, column).Value = excess.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column + 1))
                                    {
                                        total.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        total[column + 1] += excess;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column + 1))
                                    {
                                        totalCompany.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        totalCompany[column + 1] += excess;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column + 1))
                                    {
                                        grandTotal.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        grandTotal[column + 1] += excess;
                                    }
                                }
                                #endregion

                                row++;
                            }
                        }
                        //====================================================================================
                        //End loop Bussiness day
                        //====================================================================================
                    }
                    //ws.Range(startRow, 1, row - 1, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                    // Total Row
                    ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("STORE TOTAL"));
                    for (int j = 2; j <= maxColumn; j++)
                    {
                        if (total.ContainsKey(j + 1))
                            ws.Cell(row, j).Value = total[j + 1];
                    }
                    ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                    ws.Range(startRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                    row++;
                    currentCompanyRow = row;

                }//end for list store

                //summary company
                ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("COMPANY TOTAL"));
                for (int j = 2; j <= maxColumn; j++)
                {
                    if (totalCompany.ContainsKey(j + 1))
                        ws.Cell(row, j).Value = totalCompany[j + 1];
                }
                ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                ws.Range(currentCompanyRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                row++;
            }
            //end group company
            //Grand total
            ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL"));
            for (int j = 2; j <= maxColumn; j++)
            {
                if (grandTotal.ContainsKey(j + 1))
                    ws.Cell(row, j).Value = grandTotal[j + 1];
            }
            ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
            row++;
            ws.Range(row - 1, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

            ws.Range(5, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(5, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #region Table Header
            CreateReportHeaderNew(ws, maxColumn, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
            ws.Range(1, 1, 4, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            #endregion

            // Format Shift row
            if (lstShiftRow != null && lstShiftRow.Any())
            {
                foreach (int shiftRow in lstShiftRow)
                {
                    ws.Range(shiftRow, 1, shiftRow, maxColumn).Style.Font.SetFontColor(XLColor.FromHtml("#974706"));
                }
            }

            //set Width for Colum 
            List<int> lstWidCol = new List<int>() { 30, 8, 8, 12, 15, 10, 14, 8, 15, 10, 15, 14, 16, 16, 9, 12, 12 };
            for (int i = 0; i < maxColumn; i++)
            {
                if (i <= 14)
                {
                    ws.Column(i + 1).Width = lstWidCol[i];
                }
                else
                {
                    ws.Column(i + 1).Width = 14;
                }
            }
            return wb;
        }

        public XLWorkbook ExportExcel_NewForMultiCompany(BaseReportModel model, List<StoreModels> lstStore, Boolean isIncludeShift)
        {
            string sheetName = "Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            DateTime _dToFilter = model.ToDate;
            var lstStoreId = lstStore.Select(ss => ss.Id).ToList();
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreId, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            // Get list Shift info
            List<ShiftLogModels> lstShiftInfo = new List<ShiftLogModels>();
            List<int> lstShiftRow = new List<int>();
            if (isIncludeShift)
            {
                var lstIdBusDayAllStore = _lstBusDayAllStore.Select(s => s.Id).ToList();
                lstShiftInfo = GetListShiftsByBusDay(lstIdBusDayAllStore);
            }

            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
            List<DailySalesReportInsertModels> lstData = GetDataReceiptItems(model);
            List<PaymentModels> lstPayments = GetDataPaymentItems(model);
            //----------------------------------------------------------------------
            //get payment

            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });

            var cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
            if (cash == null)
                cash = new RFilterChooseExtBaseModel();
            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGC == null)
                lstGC = new List<string>();
            //------------------------------------------------------------------------------------------------------------
            List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSale = _itemizedSalesAnalysisFactory.GetItemsForDailySalesReports(model);
            var lstItemNoIncludeSaleForCate = lstItemNoIncludeSale.Where(w => !string.IsNullOrEmpty(w.GLAccountCode)).ToList();
            var lstItemNoIncludeSaleForNetSales = lstItemNoIncludeSale.Where(w => w.IsIncludeSale == false).ToList();
            var lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
            var lstMiscs = _DiscountAndMiscReportFactory.GetMiscs(model);

            //string sheetName = "Daily_Sales_Report";
            //var wb = new XLWorkbook();
            //var ws = wb.Worksheets.Add(sheetName);
            int maxColumn = 0;

            int row = 4;
            int column = 0;
            double receiptTotal = 0, payTotal = 0, payAmount = 0, noSale = 0, payByCash = 0, _tip = 0;
            double payAmountByCate = 0;
            double payAmountByCateRoundBefore = 0;
            double payAmountByCateForNetSale = 0;
            double payAmountByCateTotal = 0;
            bool _isTaxInclude = true;

            double _roudingTotal = 0;
            //DateTime dFrom, dTo;
            Dictionary<int, double> total = new Dictionary<int, double>();
            Dictionary<int, double> totalCompany = new Dictionary<int, double>();
            Dictionary<int, double> grandTotal = new Dictionary<int, double>();
            TopSellingProductsReportFactory topSellFactory = new TopSellingProductsReportFactory();

            //Show group company
            var lstCompanies = new List<CompanyModels>();
            foreach (var item in lstStore)
            {
                var com = lstCompanies.Where(ww => ww.Id == item.CompanyId).FirstOrDefault();
                if (com == null)
                {
                    lstCompanies.Add(new CompanyModels()
                    {
                        Id = item.CompanyId,
                        Name = item.CompanyName
                    });
                }
            }
            if (lstCompanies != null && lstCompanies.Any())
            {
                lstCompanies = lstCompanies.OrderBy(oo => oo.Name).ToList();
            }

            //Group company
            StoreModels _currentStore = null;
            int currentCompanyRow = 0;
            foreach (var com in lstCompanies)
            {
                // show company name

                row++;
                ws.Range("A" + row, "C" + row).Merge().SetValue(com.Name);

                totalCompany = new Dictionary<int, double>();
                var lstStoreFollowCompany = lstStore.Where(ww => ww.CompanyId == com.Id).OrderBy(ss => ss.Name).ToList();
                for (int i = 0; i < lstStoreFollowCompany.Count; i++)
                {
                    _currentStore = lstStoreFollowCompany[i];


                    _isTaxInclude = true;
                    column = 1;
                    // show store name
                    row++;
                    ws.Range("A" + row, "C" + row).Merge().SetValue(_currentStore.Name);

                    #region Columns Names
                    row++;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No Sale"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                    int receiptTotalColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotions"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Svc Chg"));
                    if (!_currentStore.IsIncludeTax)
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                        _isTaxInclude = false;
                    }
                    else
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                    }
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                    int refundColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                    //=============================================================
                    //Check no include on sale
                    //=============================================================
                    lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
                    if (lstItemNoIncludeSaleForCate != null && lstItemNoIncludeSaleForCate.Any())
                    {
                        var lstNoIncludeOnSaleGroupByCate = lstItemNoIncludeSaleForCate.Where(ww => ww.StoreId == _currentStore.Id)
                            .GroupBy(gg => gg.GLAccountCode).OrderBy(x => x.Key).ToList();
                        for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                        {
                            lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                            {
                                GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                            });
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
                    int startPayColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                    List<string> lstNotInPayMent = new List<string>() { "Cash" };
                    List<string> lstNotInPayMentNew = new List<string>() { cash != null ? cash.Id : "" };

                    List<string> storeCards = new List<string>();

                    var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && (ww.Id != cash.Id || ww.Name.ToLower() != "cash")
                        && ww.StoreId == _currentStore.Id).OrderBy(ww => ww.Name).ToList();

                    for (int p = 0; p < lstPaymentParents.Count; p++)
                    {
                        lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                        && ww.StoreId == _currentStore.Id).OrderBy(ww => ww.Name).ToList();
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

                    int endPayColumn = column;
                    // end group payments
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                    ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                    #endregion

                    // set value for last column
                    if (maxColumn < column)
                        maxColumn = column;

                    #region Format Store Header
                    ws.Range(row - 2, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                    ws.Range(row - 2, 1, row + 1, column).Style.Font.SetBold(true);
                    ws.Range(row - 2, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                    ws.Range(row, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                    ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    #endregion

                    row += 2;
                    int startRow = row;

                    var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == _currentStore.Id).ToList();
                    List<DailySalesReportInsertModels> lstDataByStore = lstData.Where(ww => ww.StoreId == _currentStore.Id).ToList();
                    total = new Dictionary<int, double>();
                    if (lstDataByStore != null && lstDataByStore.Count > 0)
                    {
                        // Fill data==================================================================
                        //============================================================================
                        payTotal = 0; payAmount = 0;

                        List<BusinessDayDisplayModels> lstBusDayShift = new List<BusinessDayDisplayModels>();

                        for (int j = 0; j < lstBusDay.Count; j++)
                        {
                            lstBusDayShift = new List<BusinessDayDisplayModels>();
                            lstBusDayShift.Add(lstBusDay[j]);
                            if (lstShiftInfo != null && lstShiftInfo.Any())
                            {
                                var lstShiftByBusDayInStore = lstShiftInfo.Where(w => w.BusinessId == lstBusDay[j].Id && w.StoreId == lstBusDay[j].StoreId)
                                    .OrderBy(o => o.StartedOn).ToList();

                                if (lstShiftByBusDayInStore != null && lstShiftByBusDayInStore.Any())
                                {
                                    foreach (var shift in lstShiftByBusDayInStore)
                                    {
                                        var obj = new BusinessDayDisplayModels();
                                        obj.DateFrom = shift.StartedOn;
                                        if (shift.ClosedOn == Commons.MinDate)
                                        {
                                            obj.DateTo = new DateTime(shift.StartedOn.Year, shift.StartedOn.Month, shift.StartedOn.Day, 23, 59, 59);
                                        }
                                        else
                                        {
                                            obj.DateTo = shift.ClosedOn;
                                        }
                                        lstBusDayShift.Add(obj);
                                    }
                                }
                            }
                            int numberShift = 0;
                            foreach (var BusDayShift in lstBusDayShift)
                            {
                                column = 1;
                                payTotal = 0;
                                payAmount = 0;

                                var reportItems = lstDataByStore.Where(s => s.CreatedDate >= BusDayShift.DateFrom && s.CreatedDate <= BusDayShift.DateTo).ToList();
                                if (reportItems == null || reportItems.Count == 0)
                                    continue;

                                // If this is Shift
                                if (string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    numberShift++;
                                    lstShiftRow.Add(row);
                                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell(row, column++).SetValue("   Shift" + numberShift + " " + BusDayShift.DateFrom.ToString("HH:mm") + " - " + BusDayShift.DateTo.ToString("HH:mm"));
                                }
                                // If this is Business day
                                else
                                {
                                    ws.Cell(row, column++).SetValue(BusDayShift.DateFrom.ToString("MM/dd/yyyy"));
                                }

                                #region add value to cell
                                receiptTotal = reportItems.Sum(ss => ss.ReceiptTotal);
                                //NoSale
                                noSale = _noSaleDetailReportFactory.GetNoSale(_currentStore.Id, BusDayShift.DateFrom, BusDayShift.DateTo);
                                ws.Cell(row, column++).Value = noSale;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, noSale);
                                    }
                                    else
                                    {
                                        total[column] += noSale;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, noSale);
                                    }
                                    else
                                    {
                                        totalCompany[column] += noSale;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, noSale);
                                    }
                                    else
                                    {
                                        grandTotal[column] += noSale;
                                    }
                                }

                                //TC - NoOfReceipt
                                ws.Cell(row, column++).Value = reportItems.Count;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Count);
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Count;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Count);
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Count;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Count);
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Count;
                                    }
                                }

                                //PAX - NoOfPerson
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.NoOfPerson);
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.NoOfPerson);
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.NoOfPerson);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.NoOfPerson));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.NoOfPerson);
                                    }
                                }

                                // ReceiptTotal
                                ws.Cell(row, column++).Value = receiptTotal.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        total[column] += receiptTotal;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        totalCompany[column] += receiptTotal;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        grandTotal[column] += receiptTotal;
                                    }
                                }

                                //Discount
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Discount).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                }

                                //promotions
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.PromotionValue).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                }

                                //ServiceCharge
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.ServiceCharge).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                }

                                //Tax
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.GST).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                }

                                _roudingTotal = reportItems.Sum(ss => ss.Rounding);
                                //Rounding
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Rounding).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                }

                                //Refund
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Refund).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Refund));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Refund);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Refund));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Refund);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Refund));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Refund);
                                    }
                                }

                                //Tip
                                _tip = reportItems.Sum(ss => ss.Tip);
                                ws.Cell(row, column++).Value = _tip.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, _tip);
                                    }
                                    else
                                    {
                                        total[column] += _tip;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, _tip);
                                    }
                                    else
                                    {
                                        totalCompany[column] += _tip;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, _tip);
                                    }
                                    else
                                    {
                                        grandTotal[column] += _tip;
                                    }
                                }
                                #region NetSales
                                double _netSale = 0;
                                if (!_isTaxInclude)
                                {
                                    payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == _currentStore.Id
                                   && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo)
                                                               .Sum(p => (decimal)p.TotalAmount
                                                               - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount);
                                }
                                else
                                    payAmountByCateForNetSale = (double)lstItemNoIncludeSaleForNetSales.Where(p => p.StoreId == _currentStore.Id
                                        && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo)
                                                                    .Sum(p => (decimal)p.TotalAmount
                                                                    - (decimal)p.PromotionAmount - (decimal)p.TotalDiscount
                                                                    - (decimal)p.Tax);

                                payAmountByCateTotal += payAmountByCateForNetSale;

                                var payGC = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom
                                                                && p.CreatedDate <= BusDayShift.DateTo
                                                             && lstGC.Contains(p.PaymentId)).Sum(p => p.Amount);

                                _netSale = reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal;
                                #endregion End NetSales
                                //=======================================================
                                //NAV - NoIncludeOnSale (- discount - promotion)
                                // GL account code and group by it
                                //Note: value is not include tax, right? It means
                                //Tax is include on item price -> value = [[(price of items x qty ) - (disc or promo )] +SVC ] -Tax
                                //Tax is add on ->value = [(price of items x qty ) - (disc or promo )] +SVC
                                payAmountByCateTotal = 0;
                                decimal difTax = 0;
                                for (int a = 0; a < lstItemNoIncludeSaleGroupByCate.Count; a++)
                                {
                                    //tax = 0;
                                    if (_isTaxInclude)
                                    {
                                        //        var aa = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                        //&& p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                        //                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode).ToList();

                                        payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                            .Sum(p => ((decimal)p.TotalAmount
                                                                - (decimal)p.TotalDiscount
                                                                - (decimal)p.PromotionAmount
                                                                    - (decimal)p.Tax));


                                        payAmountByCateRoundBefore = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                            .Sum(p => ((decimal)p.TotalAmount
                                                                - (decimal)CommonHelper.RoundUp2Decimal(p.TotalDiscount.Value)
                                                                - (decimal)CommonHelper.RoundUp2Decimal(p.PromotionAmount)
                                                                    - (decimal)p.Tax));

                                    }
                                    else
                                    {
                                        payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                        && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                    && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                                    .Sum(p => ((decimal)p.TotalAmount -
                                                                    (decimal)p.TotalDiscount.Value
                                                                    - (decimal)p.PromotionAmount));

                                        payAmountByCateRoundBefore = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                      && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                  && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                                  .Sum(p => ((decimal)p.TotalAmount -
                                                                  (decimal)CommonHelper.RoundUp2Decimal(p.TotalDiscount.Value)
                                                                  - (decimal)CommonHelper.RoundUp2Decimal(p.PromotionAmount)));

                                    }
                                    //if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim() == Commons.TENANT_SALES)
                                    if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                    {
                                        if (payAmountByCate > payAmountByCateRoundBefore)
                                        {
                                            payAmountByCate += (payAmountByCate - payAmountByCateRoundBefore);
                                        }

                                        var taxTotal = reportItems.Sum(ss => ss.GST);
                                        difTax = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                              && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                          && !string.IsNullOrEmpty(p.GLAccountCode))
                                                          .Sum(p => ((decimal)p.Tax));

                                        payAmountByCate += lstMiscs.Where(ww => ww.StoreId == _currentStore.Id && ww.CreatedDate >= BusDayShift.DateFrom
                                                && ww.CreatedDate <= BusDayShift.DateTo).Sum(ss => ss.MiscValue);
                                        payAmountByCate += _roudingTotal;
                                        payAmountByCate -= (double)((decimal)taxTotal - difTax);
                                        if (payAmountByCate > _netSale)
                                        {
                                            payAmountByCate -= (payAmountByCate - _netSale);
                                        }
                                    }

                                    ws.Cell(row, column++).Value = payAmountByCate.ToString("F");
                                    // If this is business day
                                    if (!string.IsNullOrEmpty(BusDayShift.Id))
                                    {
                                        if (!total.ContainsKey(column))
                                        {
                                            total.Add(column, payAmountByCate);
                                        }
                                        else
                                        {
                                            total[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                        //total company
                                        if (!totalCompany.ContainsKey(column))
                                        {
                                            totalCompany.Add(column, payAmountByCate);
                                        }
                                        else
                                        {
                                            totalCompany[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                        //grand total
                                        if (!grandTotal.ContainsKey(column))
                                        {
                                            grandTotal.Add(column, payAmountByCate);
                                        }
                                        else
                                        {
                                            grandTotal[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                    }
                                }


                                //NAV - End NoIncludeOnSale
                                //=======================================================
                                //NetSales -NAV - payGC 
                                //ws.Cell(row, column++).Value = (reportItems.Sum(ss => ss.NetSales) - payAmountByCateTotal - payGC + _roudingTotal).ToString("F");
                                ws.Cell(row, column++).Value = _netSale.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {

                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        total[column] += _netSale;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        totalCompany[column] += _netSale;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        grandTotal[column] += _netSale;
                                    }
                                }

                                // pay by cash (subtract refund)
                                payByCash = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                        && (p.PaymentId == lstNotInPayMentNew[0] || (p.PaymentName == lstNotInPayMent[0]))
                                                        ).Sum(p => p.Amount) - reportItems.Sum(ss => ss.Refund);

                                ws.Cell(row, column++).Value = payByCash.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        total[column] += payByCash;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        totalCompany[column] += payByCash;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payByCash;
                                    }
                                }

                                payTotal = payByCash;
                                //Payment methods other cash
                                for (int k = 0; k < storeCards.Count; k++)
                                {
                                    if (storeCards[k] != "")
                                        payAmount = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                    && p.PaymentId == storeCards[k]).Sum(p => p.Amount);
                                    else
                                        payAmount = 0;

                                    ws.Cell(row, column++).Value = payAmount.ToString("F");
                                    // If this is business day
                                    if (!string.IsNullOrEmpty(BusDayShift.Id))
                                    {
                                        if (!total.ContainsKey(column))
                                        {
                                            total.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            total[column] += payAmount;
                                        }
                                        //total company
                                        if (!totalCompany.ContainsKey(column))
                                        {
                                            totalCompany.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            totalCompany[column] += payAmount;
                                        }
                                        //grand total
                                        if (!grandTotal.ContainsKey(column))
                                        {
                                            grandTotal.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            grandTotal[column] += payAmount;
                                        }
                                    }
                                    payTotal += payAmount;
                                }

                                //PaymentTotal
                                ws.Cell(row, column++).Value = payTotal.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        total[column] += payTotal;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        totalCompany[column] += payTotal;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payTotal;
                                    }
                                }

                                //excess
                                //double excess = payTotal - (receiptTotal + _tip);
                                double excess = payTotal - receiptTotal;
                                ws.Cell(row, column).Value = excess.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column + 1))
                                    {
                                        total.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        total[column + 1] += excess;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column + 1))
                                    {
                                        totalCompany.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        totalCompany[column + 1] += excess;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column + 1))
                                    {
                                        grandTotal.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        grandTotal[column + 1] += excess;
                                    }
                                }
                                #endregion

                                row++;
                            }
                        }
                        //====================================================================================
                        //End loop Bussiness day
                        //====================================================================================
                    }
                    //ws.Range(startRow, 1, row - 1, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                    // Total Row
                    ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("STORE TOTAL"));
                    for (int j = 2; j <= maxColumn; j++)
                    {
                        if (total.ContainsKey(j + 1))
                            ws.Cell(row, j).Value = total[j + 1];
                    }
                    ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                    ws.Range(startRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                    row++;
                    currentCompanyRow = row;

                }//end for list store

                //summary company
                ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("COMPANY TOTAL"));
                for (int j = 2; j <= maxColumn; j++)
                {
                    if (totalCompany.ContainsKey(j + 1))
                        ws.Cell(row, j).Value = totalCompany[j + 1];
                }
                ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                ws.Range(currentCompanyRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                row++;
            }
            //end group company
            //Grand total
            ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL"));
            for (int j = 2; j <= maxColumn; j++)
            {
                if (grandTotal.ContainsKey(j + 1))
                    ws.Cell(row, j).Value = grandTotal[j + 1];
            }
            ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
            row++;
            ws.Range(row - 1, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

            ws.Range(5, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(5, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #region Table Header
            CreateReportHeaderNew(ws, maxColumn, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
            ws.Range(1, 1, 4, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            #endregion

            // Format Shift row
            if (lstShiftRow != null && lstShiftRow.Any())
            {
                foreach (int shiftRow in lstShiftRow)
                {
                    ws.Range(shiftRow, 1, shiftRow, maxColumn).Style.Font.SetFontColor(XLColor.FromHtml("#974706"));
                }
            }

            //set Width for Colum 
            List<int> lstWidCol = new List<int>() { 30, 8, 8, 12, 15, 10, 14, 8, 15, 10, 15, 14, 16, 16, 9, 12, 12 };
            for (int i = 0; i < maxColumn; i++)
            {
                if (i <= 14)
                {
                    ws.Column(i + 1).Width = lstWidCol[i];
                }
                else
                {
                    ws.Column(i + 1).Width = 14;
                }
            }
            return wb;
        }

        #region Updated 04032018, filter data by StartTime, EndTime & get CreditNote info - RefundGC
        public List<DailySalesReportInsertModels> GetDataReceipt_WithCreditNote(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                #region Old

                //var lstData = cxt.R_DailySalesReport.Where(ww => model.ListStores.Contains(ww.StoreId)
                //                     && (ww.CreatedDate >= model.FromDate && ww.CreatedDate <= model.ToDate)
                //                     && ww.Mode == model.Mode)
                //                     .Select(ss => new DailySalesReportInsertModels()
                //                     {
                //                         StoreId = ss.StoreId,
                //                         CreatedDate = ss.CreatedDate,
                //                         NoOfPerson = ss.NoOfPerson,
                //                         ReceiptTotal = ss.ReceiptTotal,      // Total R_Receipt
                //                         Discount = ss.Discount,
                //                         ServiceCharge = ss.ServiceCharge,
                //                         GST = ss.GST,
                //                         Rounding = ss.Rounding,
                //                         Refund = ss.Refund,
                //                         NetSales = ss.NetSales,
                //                         Tip = ss.Tip,
                //                         PromotionValue = ss.PromotionValue,
                //                         CreditNoteNo = ss.CreditNoteNo,
                //                         OrderId = ss.OrderId
                //                     }).ToList();

                //return lstData;
                #endregion EndOld
                #region use store proc
                string lstStores = string.Join(",", model.ListStores);
                var lstData = cxt.Database.SqlQuery<DailySalesReportInsertModels>("EXEC sp_GetDataReceipt_WithCreditNote @mode, @lstStoreIds, @dFrom,@dTo",
                      new SqlParameter("mode", model.Mode)
                      , new SqlParameter("lstStoreIds", lstStores)
                      , new SqlParameter("dFrom", model.FromDate.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                      , new SqlParameter("dTo", model.ToDate.ToString("yyyy-MM-dd HH:mm:ss.fff"))).ToList();
                return lstData;
                #endregion End use store proc
            }
        }

        public XLWorkbook ExportExcel_WithCreditNote(BaseReportModel model, List<StoreModels> lstStore, bool isIncludeShift, bool isExtend)
        {
           // Stopwatch stopwatch = new Stopwatch();

            string sheetName = "Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            var lstStoreId = lstStore.Select(ss => ss.Id).ToList();

            //====== Get business day info
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreId, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            //====== Get list Shift info
            List<ShiftLogModels> lstShiftInfo = new List<ShiftLogModels>();
            List<int> lstShiftRow = new List<int>();
            if (isIncludeShift)
            {
                var lstIdBusDayAllStore = _lstBusDayAllStore.Select(s => s.Id).ToList();
                lstShiftInfo = GetListShiftsByBusDay(lstIdBusDayAllStore);
            }

            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

            //====== Get data for report
            // Data sale
            var lstPayments = new List<PaymentDataModels>();
            var lstItemNoIncludeSale = new List<Data.Models.ItemizedSalesAnalysisReportDataModels> ();
            var lstItemNoIncludeSaleForCate = new List<Data.Models.ItemizedSalesAnalysisReportDataModels>();
            var lstData = new List<Data.Models.DailySalesReportInsertDataModels>();
            var lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
            var _lstNoSales = new List<NoSaleDataModels>();
            using (var db = new NuWebContext())
            {
                // Begin timing.
                //stopwatch.Reset();
                //stopwatch.Start();
                //List<DailySalesReportInsertModels> lstData = GetDataReceipt_WithCreditNote(model);
                lstData = db.GetDataReceipt_WithCreditNote(new Data.Models.BaseReportDataModel() { Mode = model.Mode, FromDate = model.FromDate, ToDate = model.ToDate, ListStores = model.ListStores });
                //stopwatch.Stop();
                // Write result.
               // Debug.WriteLine("GetDataReceipt_WithCreditNote Time elapsed: {0}", stopwatch.Elapsed);

                if (lstData != null && lstData.Any())
                {
                    switch (model.FilterType)
                    {
                        case (int)EFilterType.OnDay:
                            lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                            if (lstData == null || !lstData.Any())
                            {
                                // Set header report
                                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                return wb;
                            }
                            break;
                        case (int)EFilterType.Days:
                            lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                            if (lstData == null || !lstData.Any())
                            {
                                // Set header report
                                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
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
                    CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                    ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    return wb;
                }
                //stopwatch.Reset();
                //stopwatch.Start();
                // Data payment
                 //List<PaymentModels> lstPayments = GetDataPaymentItems_UseProcedure(model);
                lstPayments = db.GetDataPaymentItems(new Data.Models.BaseReportDataModel() { Mode = model.Mode, FromDate = model.FromDate, ToDate = model.ToDate, ListStores = model.ListStores });
                //stopwatch.Stop();
                // Write result.
                //Debug.WriteLine("GetDataPaymentItems Time elapsed: {0}", stopwatch.Elapsed);

                // Data no include sale
                // List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSale = new List<ItemizedSalesAnalysisReportModels>();
                //var lstReceiptId = lstData.Select(s => s.OrderId).Distinct().ToList();

                //if (lstReceiptId != null && lstReceiptId.Any())
                //{
                //    stopwatch.Reset();
                //    stopwatch.Start();
                //   lstItemNoIncludeSale = _itemizedSalesAnalysisFactory.GetItemsNoIncludeSale_UsedProcedure(model);
                //    stopwatch.Stop();
                //    // Write result.
                //    Debug.WriteLine("GetItemsNoIncludeSale Time elapsed: {0}", stopwatch.Elapsed);
                //}
                //stopwatch.Reset();
                //stopwatch.Start();

                lstItemNoIncludeSale = db.GetItemsNoIncludeSale_New(new Data.Models.BaseReportDataModel() { Mode = model.Mode, FromDate = model.FromDate, ToDate = model.ToDate, ListStores = model.ListStores });
               
                if (lstItemNoIncludeSale != null && lstItemNoIncludeSale.Any())
                {
                    lstItemNoIncludeSaleForCate = lstItemNoIncludeSale.Where(w => !string.IsNullOrEmpty(w.GLAccountCode)).ToList();
                }
                //stopwatch.Stop();
                // Write result.
                // Debug.WriteLine("GetItemsNoIncludeSale_New Time elapsed: {0}", stopwatch.Elapsed);

                //stopwatch.Reset();
                //stopwatch.Start();
                _lstNoSales= db.GetNoSaleDatas(new Data.Models.BaseReportDataModel() { Mode = model.Mode, FromDate = model.FromDate, ToDate = model.ToDate, ListStores = model.ListStores });
                //stopwatch.Stop();
                // Write result.
                //Debug.WriteLine("GetItemsNoIncludeSale_New Time elapsed: {0}", stopwatch.Elapsed);

            }//end use context

            // Data Misc
            //stopwatch.Reset();
            //stopwatch.Start();
            var lstMiscs = _DiscountAndMiscReportFactory.GetMiscs(model);

            //stopwatch.Stop();
            // Write result.
            //Debug.WriteLine("GetMiscs Time elapsed: {0}", stopwatch.Elapsed);

            // get lst refund by GC
            //stopwatch.Reset();
            //stopwatch.Start();
            var _lstRefunds = _refundFactory.GetListRefundWithoutDetail(model);
            //stopwatch.Stop();
            //// Write result.
            //Debug.WriteLine("GetListRefundWithoutDetail Time elapsed: {0}", stopwatch.Elapsed);

            // Filter data by time
            switch (model.FilterType)
            {
                case (int)EFilterType.OnDay:
                    if (lstPayments != null && lstPayments.Any())
                    {
                        lstPayments = lstPayments.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (lstMiscs != null && lstMiscs.Any())
                    {
                        lstMiscs = lstMiscs.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (_lstRefunds != null && _lstRefunds.Any())
                    {
                        _lstRefunds = _lstRefunds.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    }
                    break;
                case (int)EFilterType.Days:
                    if (lstPayments != null && lstPayments.Any())
                    {
                        lstPayments = lstPayments.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (lstMiscs != null && lstMiscs.Any())
                    {
                        lstMiscs = lstMiscs.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (_lstRefunds != null && _lstRefunds.Any())
                    {
                        _lstRefunds = _lstRefunds.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    }
                    break;
            }

            //====== Payment method information
            //stopwatch.Reset();
            //stopwatch.Start();

            List<RFilterChooseExtBaseModel> lstPaymentMethod = new List<RFilterChooseExtBaseModel>();
            RFilterChooseExtBaseModel cash = new RFilterChooseExtBaseModel();
            List<string> lstGC = new List<string>();
            // Get payment method if is not merchant extend
            if (!isExtend)
            {
                lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });

                cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
                if (cash == null)
                    cash = new RFilterChooseExtBaseModel();
                lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                if (lstGC == null)
                    lstGC = new List<string>();
            }
            // Write result.
           // Debug.WriteLine("GetAllPaymentForReport NUPOSAPI Time elapsed: {0}", stopwatch.Elapsed);

            //====== Get list categories
            CategoryApiRequestModel cateRequest = new CategoryApiRequestModel();
            cateRequest.ListStoreIds = model.ListStores;

            //stopwatch.Reset();
            //stopwatch.Start();

            var _lstCates = _categoriesFactory.GetAllCategoriesForDailySale(cateRequest);
            //stopwatch.Stop();
            //// Write result.
            //Debug.WriteLine("GetAllCategoriesForDailySale NUPOSAPI Time elapsed: {0}", stopwatch.Elapsed);

            int maxColumn = 0;

            int row = 4;
            int column = 0;
            int TC = 0, PAX = 0;
            double receiptTotal = 0, payTotal = 0, payAmount = 0, noSale = 0, payByCash = 0, _tip = 0, _refundGC = 0;
            double payAmountByCate = 0;
            double _refundByCash = 0;
            double _roudingTotal = 0;

            Dictionary<int, double> total = new Dictionary<int, double>();
            Dictionary<int, double> totalCompany = new Dictionary<int, double>();
            Dictionary<int, double> grandTotal = new Dictionary<int, double>();

            //====== Get company info
            var lstCompanies = new List<CompanyModels>();
            #region Old
            //if (!isExtend)
            //{
            //    foreach (var item in lstStore)
            //    {
            //        var com = lstCompanies.Where(ww => ww.Id == item.CompanyId).FirstOrDefault();
            //        if (com == null)
            //        {
            //            lstCompanies.Add(new CompanyModels()
            //            {
            //                Id = item.CompanyId,
            //                Name = item.CompanyName
            //            });
            //        }
            //    }
            //    if (lstCompanies != null && lstCompanies.Any())
            //    {
            //        lstCompanies = lstCompanies.OrderBy(oo => oo.Name).ToList();
            //    }
            //}
            //else
            //{
            //    lstCompanies = lstStore.Select(ss => new CompanyModels()
            //    {
            //        Id = ss.CompanyId,
            //        Name = ss.CompanyName,
            //        ApiUrlExtend = ss.HostUrlExtend
            //    }).Distinct().OrderBy(oo => oo.Name).ToList();
            //}
            #endregion End Old
            #region New
            var lstStoreGroup = lstStore.GroupBy(gg => gg.CompanyId).ToList();
            foreach (var comp in lstStoreGroup)
            {
                lstCompanies.Add(new CompanyModels()
                {
                    Id = comp.Key,
                    Name = comp.Select(ss => ss.CompanyName).FirstOrDefault(),
                    ApiUrlExtend = comp.Select(ss => ss.HostUrlExtend).FirstOrDefault(),
                    ListStores = comp.ToList()
                });
            }
            #endregion End new

            // Group company
            StoreModels _currentStore = null;
            int currentCompanyRow = 0;

            // Flag check if multi company
            Boolean isMultiComp = false;
            if (lstCompanies != null && lstCompanies.Count() > 1)
            {
                isMultiComp = true;
                //2018-07-17
                lstCompanies = lstCompanies.OrderBy(oo => oo.Name).ToList();
            }
           
            //====== Set data for excel
            foreach (var com in lstCompanies)
            {
                // Company name
                row++;
                ws.Range("A" + row, "C" + row).Merge().SetValue(com.Name);

                totalCompany = new Dictionary<int, double>();

                // var lstStoreFollowCompany = lstStore.Where(ww => ww.CompanyId == com.Id).OrderBy(ss => ss.Name).ToList();
                var lstStoreFollowCompany = com.ListStores.OrderBy(ss => ss.Name).ToList();

                for (int i = 0; i < lstStoreFollowCompany.Count; i++)
                {
                    //stopwatch.Reset();
                    //stopwatch.Start();
                    _currentStore = lstStoreFollowCompany[i];

                    // Get list payment method if Merchant extend
                    if (isExtend)
                    {
                        lstPaymentMethod = _baseFactory.GetAllPaymentForMerchantExtendReport(com.ApiUrlExtend, new Models.Api.CategoryApiRequestModel() { ListStoreIds = new List<string> { _currentStore.Id } });

                        cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
                        if (cash == null)
                            cash = new RFilterChooseExtBaseModel();
                        lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                        if (lstGC == null)
                            lstGC = new List<string>();
                    }

                    column = 1;

                    // Store name
                    row++;
                    ws.Range("A" + row, "C" + row).Merge().SetValue(_currentStore.Name);

                    #region Columns Names
                    row++;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No Sale"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                    int receiptTotalColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotions"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Svc Chg"));
                    if (!_currentStore.IsIncludeTax)
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                    }
                    else
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                    }
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                    int refundColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                    //=============================================================
                    //Check no include on sale
                    //=============================================================
                    lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
                    // Update -> show all categories have GLCode (12/12/2017)
                    if (_lstCates != null && _lstCates.Any())
                    {
                        var lstNoIncludeOnSaleGroupByCate = _lstCates.GroupBy(gg => gg.GLCode).OrderBy(x => x.Key).ToList();
                        for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                        {
                            lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                            {
                                GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                            });
                            if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                            {
                                ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                            }
                            else
                                ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                        }
                    }
                    else
                    {
                        if (lstItemNoIncludeSaleForCate != null && lstItemNoIncludeSaleForCate.Any())
                        {
                            var lstNoIncludeOnSaleGroupByCate = lstItemNoIncludeSaleForCate.Where(ww => ww.StoreId == _currentStore.Id)
                                .GroupBy(gg => gg.GLAccountCode).OrderBy(x => x.Key).ToList();
                            for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                            {
                                lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                                {
                                    GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                                });
                                if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                                {
                                    ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                                }
                                else
                                    ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                            }
                        }
                    }

                    ///End Check no include on sale
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                    int startPayColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                    List<string> lstNotInPayMent = new List<string>() { "Cash" };
                    List<string> lstNotInPayMentNew = new List<string>() { cash != null ? cash.Id : "" };

                    List<string> storeCards = new List<string>();

                    var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && (ww.Id != cash.Id || ww.Name.ToLower() != "cash")
                        && ww.StoreId == _currentStore.Id).OrderBy(ww => ww.Name).ToList();

                    for (int p = 0; p < lstPaymentParents.Count; p++)
                    {
                        lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                        && ww.StoreId == _currentStore.Id).OrderBy(ww => ww.Name).ToList();
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

                    int endPayColumn = column;
                    // end group payments
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                    ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                    #endregion

                    // set value for last column
                    if (maxColumn < column)
                        maxColumn = column;

                    #region Format Store Header
                    ws.Range(row - 2, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                    ws.Range(row - 2, 1, row + 1, column).Style.Font.SetBold(true);
                    ws.Range(row - 2, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                    ws.Range(row, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                    ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    #endregion

                    row += 2;
                    int startRow = row;

                    var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == _currentStore.Id).ToList();
                    List<DailySalesReportInsertDataModels> lstDataByStore = lstData.Where(ww => ww.StoreId == _currentStore.Id).ToList();
                    total = new Dictionary<int, double>();
                    if (lstDataByStore != null && lstDataByStore.Count > 0)
                    {
                        // Fill data==================================================================
                        //============================================================================
                        payTotal = 0; payAmount = 0;

                        List<BusinessDayDisplayModels> lstBusDayShift = new List<BusinessDayDisplayModels>();

                        for (int j = 0; j < lstBusDay.Count; j++)
                        {
                            lstBusDayShift = new List<BusinessDayDisplayModels>();
                            lstBusDayShift.Add(lstBusDay[j]);
                            if (lstShiftInfo != null && lstShiftInfo.Any())
                            {
                                var lstShiftByBusDayInStore = lstShiftInfo.Where(w => w.BusinessId == lstBusDay[j].Id && w.StoreId == lstBusDay[j].StoreId)
                                    .OrderBy(o => o.StartedOn).ToList();

                                if (lstShiftByBusDayInStore != null && lstShiftByBusDayInStore.Any())
                                {
                                    foreach (var shift in lstShiftByBusDayInStore)
                                    {
                                        var obj = new BusinessDayDisplayModels();
                                        obj.DateFrom = shift.StartedOn;
                                        if (shift.ClosedOn == Commons.MinDate)
                                        {
                                            obj.DateTo = new DateTime(shift.StartedOn.Year, shift.StartedOn.Month, shift.StartedOn.Day, 23, 59, 59);
                                        }
                                        else
                                        {
                                            obj.DateTo = shift.ClosedOn;
                                        }
                                        lstBusDayShift.Add(obj);
                                    }
                                }
                            }
                            int numberShift = 0;
                            foreach (var BusDayShift in lstBusDayShift)
                            {
                                column = 1;
                                payTotal = 0;
                                payAmount = 0;

                                var reportItems = lstDataByStore.Where(s => s.CreatedDate >= BusDayShift.DateFrom && s.CreatedDate <= BusDayShift.DateTo).ToList();
                                if (reportItems == null || reportItems.Count == 0)
                                    continue;
                                var _lstOrderIds = reportItems.Select(ww => ww.OrderId).ToList();
                                // If this is Shift
                                if (string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    numberShift++;
                                    lstShiftRow.Add(row);
                                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell(row, column++).SetValue("   Shift" + numberShift + " " + BusDayShift.DateFrom.ToString("HH:mm") + " - " + BusDayShift.DateTo.ToString("HH:mm"));
                                }
                                // If this is Business day
                                else
                                {
                                    ws.Cell(row, column++).SetValue(BusDayShift.DateFrom.ToString("MM/dd/yyyy"));
                                }

                                #region add value to cell
                                //NoSale
                                //noSale = _noSaleDetailReportFactory.GetNoSale(_currentStore.Id, BusDayShift.DateFrom, BusDayShift.DateTo);
                                noSale = _lstNoSales.Where(ww => ww.StoreId == _currentStore.Id && ww.CreatedDate >= BusDayShift.DateFrom && ww.CreatedDate <= BusDayShift.DateTo).Count();
                                ws.Cell(row, column++).Value = noSale;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, noSale);
                                    }
                                    else
                                    {
                                        total[column] += noSale;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, noSale);
                                    }
                                    else
                                    {
                                        totalCompany[column] += noSale;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, noSale);
                                    }
                                    else
                                    {
                                        grandTotal[column] += noSale;
                                    }
                                }

                                //TC - NoOfReceipt
                                TC = reportItems.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Count();
                                ws.Cell(row, column++).Value = TC;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, TC);
                                    }
                                    else
                                    {
                                        total[column] += TC;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, TC);
                                    }
                                    else
                                    {
                                        totalCompany[column] += TC;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, TC);
                                    }
                                    else
                                    {
                                        grandTotal[column] += TC;
                                    }
                                }

                                //PAX - NoOfPerson
                                PAX = reportItems.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Sum(ss => ss.NoOfPerson);
                                ws.Cell(row, column++).Value = PAX;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, PAX);
                                    }
                                    else
                                    {
                                        total[column] += PAX;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, PAX);
                                    }
                                    else
                                    {
                                        totalCompany[column] += PAX;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, PAX);
                                    }
                                    else
                                    {
                                        grandTotal[column] += PAX;
                                    }
                                }

                                // ReceiptTotal
                                receiptTotal = reportItems.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Sum(ss => ss.ReceiptTotal);

                                ws.Cell(row, column++).Value = receiptTotal.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        total[column] += receiptTotal;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        totalCompany[column] += receiptTotal;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        grandTotal[column] += receiptTotal;
                                    }
                                }

                                //Discount
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Discount).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Discount));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Discount);
                                    }
                                }

                                //promotions
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.PromotionValue).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.PromotionValue));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.PromotionValue);
                                    }
                                }

                                //ServiceCharge
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.ServiceCharge).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                }

                                //Tax
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.GST).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                }

                                _roudingTotal = reportItems.Sum(ss => ss.Rounding);
                                //Rounding
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Rounding).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                }

                                //Refund
                                _refundByCash = _lstRefunds.Where(ww => _lstOrderIds.Contains(ww.OrderId) && !ww.IsGiftCard).Sum(ss => ss.TotalRefund);
                                _refundGC = _lstRefunds.Where(ww => _lstOrderIds.Contains(ww.OrderId) && ww.IsGiftCard).Sum(ss => ss.TotalRefund);

                                ws.Cell(row, column++).Value = (_refundByCash + _refundGC).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, (_refundByCash + _refundGC));
                                    }
                                    else
                                    {
                                        total[column] += (_refundByCash + _refundGC);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, (_refundByCash + _refundGC));
                                    }
                                    else
                                    {
                                        totalCompany[column] += (_refundByCash + _refundGC);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, (_refundByCash + _refundGC));
                                    }
                                    else
                                    {
                                        grandTotal[column] += (_refundByCash + _refundGC);
                                    }
                                }

                                //Tip
                                _tip = reportItems.Sum(ss => ss.Tip);
                                ws.Cell(row, column++).Value = _tip.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, _tip);
                                    }
                                    else
                                    {
                                        total[column] += _tip;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, _tip);
                                    }
                                    else
                                    {
                                        totalCompany[column] += _tip;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, _tip);
                                    }
                                    else
                                    {
                                        grandTotal[column] += _tip;
                                    }
                                }
                                #region NetSales
                                double _netSale = 0;
                                double payGC = 0;
                                double _taxOfPayGCNotInclude = 0;
                                double _svcOfPayGCNotInclude = 0;

                                var lstPaymentsInStore = lstPayments.Where(p => p.StoreId == _currentStore.Id
                                        && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                        && lstGC.Contains(p.PaymentId)
                                         && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();
                                if (lstPaymentsInStore != null && lstPaymentsInStore.Any())
                                {
                                    double refundAmount = 0;
                                    double _amount = 0;
                                    foreach (var item in lstPaymentsInStore)
                                    {
                                        _amount = item.Amount;
                                        refundAmount = 0;
                                        var lstGCRefunds = _lstRefunds.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCRefunds != null && lstGCRefunds.Any())
                                        {
                                            refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                            _amount -= refundAmount;
                                        }
                                        payGC += _amount;

                                        var receipt = reportItems.Where(ww => ww.OrderId == item.OrderId).FirstOrDefault();
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

                                //var payGC = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom
                                //                                && p.CreatedDate <= BusDayShift.DateTo
                                //                             && lstGC.Contains(p.PaymentId)).Sum(p => p.Amount);

                                //netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (1)
                                //(1)= GC pay not include - tax of pay GC not include - SCV of pay GC not include 

                                var totalReceipt = reportItems.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo)).Sum(ss => ss.ReceiptTotal);
                                var totalSVC = reportItems.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo)).Sum(ss => ss.ServiceCharge);
                                var totalTax = reportItems.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo)).Sum(ss => ss.GST);

                                var creditNoteIds = reportItems.Where(ww => !string.IsNullOrEmpty(ww.CreditNoteNo)).Select(ss => ss.OrderId).ToList();
                                double totalCreditNote = 0;
                                if (creditNoteIds != null && creditNoteIds.Any())
                                {
                                    totalCreditNote = lstItemNoIncludeSale.Where(ww => creditNoteIds.Contains(ww.ReceiptId)
                                    && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                        .Sum(ss => ss.ItemTotal);
                                }

                                double giftCardSell = lstItemNoIncludeSale.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                     && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                     && ww.BusinessId == lstBusDay[j].Id
                                                     && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                                var taxInclude = lstItemNoIncludeSale.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                && ww.BusinessId == lstBusDay[j].Id).Sum(ss => ss.Tax);

                                var _noincludeSale = lstItemNoIncludeSale.Where(ww => ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                && ww.BusinessId == lstBusDay[j].Id).Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                                _noincludeSale -= taxInclude;


                                _netSale = totalReceipt - totalCreditNote - totalSVC - totalTax - giftCardSell - _noincludeSale
                                    - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);

                                //_netSale = (reportItems.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Sum(ss => ss.NetSales) - totalNoIncludeSaleReceipt) 
                                //    - payGC + _roudingTotal - (reportItems.Where(w => !string.IsNullOrEmpty(w.CreditNoteNo)).Sum(ss => ss.NetSales) - totalNoIncludeSaleCN);

                                #endregion End NetSales
                                //=======================================================
                                //NAV - NoIncludeOnSale (- discount - promotion)
                                // GL account code and group by it
                                //Note: value is not include tax, right? It means
                                //Tax is include on item price -> value = [[(price of items x qty ) - (disc or promo )] +SVC ] -Tax
                                //Tax is add on ->value = [(price of items x qty ) - (disc or promo )] +SVC

                                double _tax = 0;
                                for (int a = 0; a < lstItemNoIncludeSaleGroupByCate.Count; a++)
                                {

                                    _tax = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.TaxType == (int)Commons.ETax.Inclusive)
                                                            .Sum(p => p.Tax);

                                    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                               && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                           && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                           .Sum(p => ((decimal)p.TotalAmount
                                                               - (decimal)p.TotalDiscount
                                                               - (decimal)p.PromotionAmount));

                                    payAmountByCate -= _tax;

                                    #region OldGLCode
                                    //    if (_isTaxInclude)
                                    //    {

                                    //        payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                    //&& p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                    //                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                    //                            .Sum(p => ((decimal)p.TotalAmount
                                    //                                - (decimal)p.TotalDiscount
                                    //                                - (decimal)p.PromotionAmount
                                    //                                    - (decimal)p.Tax));


                                    //        payAmountByCateRoundBefore = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                    //&& p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                    //                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                    //                            .Sum(p => ((decimal)p.TotalAmount
                                    //                                - (decimal)CommonHelper.RoundUp2Decimal(p.TotalDiscount.Value)
                                    //                                - (decimal)CommonHelper.RoundUp2Decimal(p.PromotionAmount)
                                    //                                    - (decimal)p.Tax));

                                    //    }
                                    //    else
                                    //    {
                                    //        payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                    //        && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                    //                                    && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                    //                                    .Sum(p => ((decimal)p.TotalAmount -
                                    //                                    (decimal)p.TotalDiscount.Value
                                    //                                    - (decimal)p.PromotionAmount));

                                    //        payAmountByCateRoundBefore = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                    //      && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                    //                                  && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                    //                                  .Sum(p => ((decimal)p.TotalAmount -
                                    //                                  (decimal)CommonHelper.RoundUp2Decimal(p.TotalDiscount.Value)
                                    //                                  - (decimal)CommonHelper.RoundUp2Decimal(p.PromotionAmount)));

                                    //    }
                                    #endregion End OldGLCode
                                    if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                    {
                                        payAmountByCate += lstMiscs.Where(ww => ww.StoreId == _currentStore.Id && ww.CreatedDate >= BusDayShift.DateFrom
                                                && ww.CreatedDate <= BusDayShift.DateTo).Sum(ss => ss.MiscValue);
                                        payAmountByCate += _roudingTotal;

                                    }
                                    if (payAmountByCate < 0)
                                    {
                                        payAmountByCate = 0;
                                    }
                                    ws.Cell(row, column++).Value = payAmountByCate.ToString("F");
                                    // If this is business day
                                    if (!string.IsNullOrEmpty(BusDayShift.Id))
                                    {
                                        if (!total.ContainsKey(column))
                                        {
                                            total.Add(column, CommonHelper.RoundUp2Decimal(payAmountByCate));
                                        }
                                        else
                                        {
                                            total[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                        //total company
                                        if (!totalCompany.ContainsKey(column))
                                        {
                                            totalCompany.Add(column, CommonHelper.RoundUp2Decimal(payAmountByCate));
                                        }
                                        else
                                        {
                                            totalCompany[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                        //grand total
                                        if (!grandTotal.ContainsKey(column))
                                        {
                                            grandTotal.Add(column, CommonHelper.RoundUp2Decimal(payAmountByCate));
                                        }
                                        else
                                        {
                                            grandTotal[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                    }
                                }

                                //NAV - End NoIncludeOnSale
                                //=======================================================
                                //NetSales -NAV - payGC 

                                ws.Cell(row, column++).Value = _netSale.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {

                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        total[column] += _netSale;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        totalCompany[column] += _netSale;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        grandTotal[column] += _netSale;
                                    }
                                }

                                // pay by cash (subtract refund)
                                payByCash = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                        && (p.PaymentId == lstNotInPayMentNew[0] || (p.PaymentName == lstNotInPayMent[0]))
                                                        ).Sum(p => p.Amount) - _refundByCash;

                                ws.Cell(row, column++).Value = payByCash.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        total[column] += payByCash;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        totalCompany[column] += payByCash;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payByCash;
                                    }
                                }

                                payTotal = payByCash;
                                //Payment methods other cash
                                for (int k = 0; k < storeCards.Count; k++)
                                {
                                    if (storeCards[k] != "")
                                    {
                                        payAmount = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                      && p.PaymentId == storeCards[k]).Sum(p => p.Amount);
                                        //check isGC
                                        if (lstGC.Contains(storeCards[k]))
                                            //var refundGC = _dicGCRefunds.Where(ww => ww.Key == storeCards[k]).Sum(ss => ss.Value);
                                            payAmount -= _refundGC;
                                    }
                                    else
                                        payAmount = 0;

                                    ws.Cell(row, column++).Value = payAmount.ToString("F");
                                    // If this is business day
                                    if (!string.IsNullOrEmpty(BusDayShift.Id))
                                    {
                                        if (!total.ContainsKey(column))
                                        {
                                            total.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            total[column] += payAmount;
                                        }
                                        //total company
                                        if (!totalCompany.ContainsKey(column))
                                        {
                                            totalCompany.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            totalCompany[column] += payAmount;
                                        }
                                        //grand total
                                        if (!grandTotal.ContainsKey(column))
                                        {
                                            grandTotal.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            grandTotal[column] += payAmount;
                                        }
                                    }
                                    payTotal += payAmount;
                                }

                                //PaymentTotal
                                ws.Cell(row, column++).Value = payTotal.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        total[column] += payTotal;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        totalCompany[column] += payTotal;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payTotal;
                                    }
                                }

                                //excess
                                //double excess = payTotal - (receiptTotal + _tip);
                                double excess = payTotal - receiptTotal;
                                ws.Cell(row, column).Value = excess.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column + 1))
                                    {
                                        total.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        total[column + 1] += excess;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column + 1))
                                    {
                                        totalCompany.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        totalCompany[column + 1] += excess;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column + 1))
                                    {
                                        grandTotal.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        grandTotal[column + 1] += excess;
                                    }
                                }
                                #endregion

                                row++;
                            }
                        }//for (int j = 0; j < lstBusDay.Count; j++)
                        //====================================================================================
                        //End loop Bussiness day
                        //====================================================================================
                    }
                    //ws.Range(startRow, 1, row - 1, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                    // Total Row
                    if (isMultiComp)
                    {
                        ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("STORE TOTAL"));
                    }
                    else
                    {
                        ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                    }
                    for (int j = 2; j <= maxColumn; j++)
                    {
                        if (total.ContainsKey(j + 1))
                            ws.Cell(row, j).Value = total[j + 1];
                    }
                    ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                    ws.Range(startRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                    row++;
                    currentCompanyRow = row;

                    //stopwatch.Stop();
                    // Write result.
                   // Debug.WriteLine("Store:{0} Time elapsed: {1}",_currentStore.Name, stopwatch.Elapsed);

                }//end for list store

                //summary company
                if (isMultiComp)
                {
                    ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("COMPANY TOTAL"));
                }
                else
                {
                    ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL"));
                }
                for (int j = 2; j <= maxColumn; j++)
                {
                    if (totalCompany.ContainsKey(j + 1))
                        ws.Cell(row, j).Value = totalCompany[j + 1];
                }
                ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                ws.Range(currentCompanyRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                row++;
            }
            //end group company
            //Grand total
            if (isMultiComp)
            {
                ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL"));
                for (int j = 2; j <= maxColumn; j++)
                {
                    if (grandTotal.ContainsKey(j + 1))
                        ws.Cell(row, j).Value = grandTotal[j + 1];
                }
                ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                row++;
                ws.Range(row - 1, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";
            }

            ws.Range(5, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(5, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #region Table Header
            CreateReportHeaderNew(ws, maxColumn, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
            ws.Range(1, 1, 4, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            #endregion

            // Format Shift row
            if (lstShiftRow != null && lstShiftRow.Any())
            {
                foreach (int shiftRow in lstShiftRow)
                {
                    ws.Range(shiftRow, 1, shiftRow, maxColumn).Style.Font.SetFontColor(XLColor.FromHtml("#974706"));
                }
            }

            ws.Columns().AdjustToContents();

            // Set Width for Colum 
            ws.Column(1).Width = 25;
            ws.Column(2).Width = 9;
            ws.Column(3).Width = 9;
            ws.Column(4).Width = 9;

            for (int i = 4; i < maxColumn; i++)
            {
                ws.Column(i + 1).Width = 17;
            }

            return wb;
        }
        #endregion Updated 04032018, filter data by StartTime, EndTime & get CreditNote info - RefundGC

        #region Use new database from table [R_PosSale]
        /// <summary>
        /// Get list receipt information from table [R_PosSale]
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<DailySalesReportInsertModels> GetReceiptData(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_PosSale.Where(ww => model.ListStores.Contains(ww.StoreId)
                    && (ww.ReceiptCreatedDate >= model.FromDate && ww.ReceiptCreatedDate <= model.ToDate)
                    && ww.Mode == model.Mode)
                    .Select(ss => new DailySalesReportInsertModels()
                    {
                        StoreId = ss.StoreId,
                        CreatedDate = ss.ReceiptCreatedDate.Value,
                        NoOfPerson = ss.NoOfPerson,
                        ReceiptTotal = ss.ReceiptTotal,
                        Discount = ss.Discount,
                        ServiceCharge = ss.ServiceCharge,
                        GST = ss.GST,
                        Rounding = ss.Rounding,
                        Refund = ss.Refund,
                        //NetSales = ss.NetSales,
                        Tip = ss.Tip,
                        PromotionValue = ss.PromotionValue,
                        CreditNoteNo = ss.CreditNoteNo,
                        OrderId = ss.OrderId,
                        BusinessId = ss.BusinessId
                    }).ToList();

                return lstData;
            }
        }

        /// <summary>
        /// Export data for all case
        /// </summary>
        /// <param name="model"></param>
        /// <param name="lstStore"></param>
        /// <param name="isIncludeShift"></param>
        /// <returns></returns>
        public XLWorkbook ExportExcel_NewDB(BaseReportModel model, List<StoreModels> lstStore, bool isIncludeShift, bool isExtend)
        {
            string sheetName = "Daily_Sales_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            var lstStoreId = lstStore.Select(ss => ss.Id).ToList();

            //====== Get business day info
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreId, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            //====== Get list Shift info
            List<ShiftLogModels> lstShiftInfo = new List<ShiftLogModels>();
            List<int> lstShiftRow = new List<int>();
            if (isIncludeShift)
            {
                var lstIdBusDayAllStore = _lstBusDayAllStore.Select(s => s.Id).ToList();
                lstShiftInfo = GetListShiftsByBusDay(lstIdBusDayAllStore);
            }

            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

            //====== Get data for report
            // Data sale
            List<DailySalesReportInsertModels> lstData = GetReceiptData(model);
            if (lstData != null && lstData.Any())
            {
                switch (model.FilterType)
                {
                    case 1:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        if (lstData == null || !lstData.Any())
                        {
                            // Set header report
                            CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            return wb;
                        }
                        break;
                    case 2:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        if (lstData == null || !lstData.Any())
                        {
                            // Set header report
                            CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
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
                CreateReportHeaderNew(ws, 8, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            PosSaleFactory posSaleFactory = new PosSaleFactory();
            // Data no include sale
            List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSale = new List<ItemizedSalesAnalysisReportModels>();
            var lstItemNoIncludeSaleForCate = new List<ItemizedSalesAnalysisReportModels>();
            var lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
            var lstMiscs = new List<DiscountAndMiscReportModels>();
            var _lstRefunds = new List<RefundReportDTO>();

            var lstReceiptId = lstData.Select(s => s.OrderId).Distinct().ToList();

            if (lstReceiptId != null && lstReceiptId.Any())
            {
                // Data no include sale & GLAccount code
                lstItemNoIncludeSale = posSaleFactory.GetItemsNoIncludeSale(lstReceiptId, model.ListStores, model.Mode);

                // Data Misc
                lstMiscs = posSaleFactory.GetMiscs(model.ListStores, lstReceiptId, model.Mode);

                // Get lst refund by GC
                _lstRefunds = _refundFactory.GetListRefundWithoutDetailsByReceiptId(model.ListStores, lstReceiptId);
            }

            if (lstItemNoIncludeSale != null && lstItemNoIncludeSale.Any())
            {
                lstItemNoIncludeSaleForCate = lstItemNoIncludeSale.Where(w => !string.IsNullOrEmpty(w.GLAccountCode)).ToList();
            }

            // Data payment
            List<PaymentModels> lstPayments = GetDataPaymentItems(model);

            // Filter data by time
            switch (model.FilterType)
            {
                case 1:
                    if (lstPayments != null && lstPayments.Any())
                    {
                        lstPayments = lstPayments.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    }
                    break;
                case 2:
                    if (lstPayments != null && lstPayments.Any())
                    {
                        lstPayments = lstPayments.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    }
                    break;
            }

            //====== Payment method information
            List<RFilterChooseExtBaseModel> lstPaymentMethod = new List<RFilterChooseExtBaseModel>();
            RFilterChooseExtBaseModel cash = new RFilterChooseExtBaseModel();
            List<string> lstGC = new List<string>();
            // Get payment method if is not merchant extend
            if (!isExtend)
            {
                lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });

                cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
                if (cash == null)
                    cash = new RFilterChooseExtBaseModel();
                lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                if (lstGC == null)
                    lstGC = new List<string>();
            }

            //====== Get list categories
            CategoryApiRequestModel cateRequest = new CategoryApiRequestModel();
            cateRequest.ListStoreIds = model.ListStores;
            var _lstCates = _categoriesFactory.GetAllCategoriesForDailySale(cateRequest);

            int maxColumn = 0;

            int row = 4;
            int column = 0;
            int TC = 0, PAX = 0;
            double receiptTotal = 0, payTotal = 0, payAmount = 0, noSale = 0, payByCash = 0, _tip = 0, _refundGC = 0;
            double payAmountByCate = 0;
            double _refundByCash = 0;
            double _roudingTotal = 0;

            Dictionary<int, double> total = new Dictionary<int, double>();
            Dictionary<int, double> totalCompany = new Dictionary<int, double>();
            Dictionary<int, double> grandTotal = new Dictionary<int, double>();

            //====== Get company info
            var lstCompanies = new List<CompanyModels>();
            if (!isExtend)
            {
                foreach (var item in lstStore)
                {
                    var com = lstCompanies.Where(ww => ww.Id == item.CompanyId).FirstOrDefault();
                    if (com == null)
                    {
                        lstCompanies.Add(new CompanyModels()
                        {
                            Id = item.CompanyId,
                            Name = item.CompanyName
                        });
                    }
                }
                if (lstCompanies != null && lstCompanies.Any())
                {
                    lstCompanies = lstCompanies.OrderBy(oo => oo.Name).ToList();
                }
            }
            else
            {
                lstCompanies = lstStore.Select(ss => new CompanyModels()
                {
                    Id = ss.CompanyId,
                    Name = ss.CompanyName,
                    ApiUrlExtend = ss.HostUrlExtend
                }).Distinct().OrderBy(oo => oo.Name).ToList();
            }

            // Group company
            StoreModels _currentStore = null;
            int currentCompanyRow = 0;

            // Flag check if multi company
            Boolean isMultiComp = false;
            if (lstCompanies != null && lstCompanies.Count() > 1)
            {
                isMultiComp = true;
            }

            //====== Set data for excel
            foreach (var com in lstCompanies)
            {
                // Company name
                row++;
                ws.Range("A" + row, "C" + row).Merge().SetValue(com.Name);

                totalCompany = new Dictionary<int, double>();

                var lstStoreFollowCompany = lstStore.Where(ww => ww.CompanyId == com.Id).OrderBy(ss => ss.Name).ToList();
                for (int i = 0; i < lstStoreFollowCompany.Count; i++)
                {
                    _currentStore = lstStoreFollowCompany[i];

                    // Get list payment method if Merchant extend
                    if (isExtend)
                    {
                        lstPaymentMethod = _baseFactory.GetAllPaymentForMerchantExtendReport(com.ApiUrlExtend, new Models.Api.CategoryApiRequestModel() { ListStoreIds = new List<string> { _currentStore.Id } });

                        cash = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.Cash || ww.Name.ToLower().Trim() == "cash").FirstOrDefault();
                        if (cash == null)
                            cash = new RFilterChooseExtBaseModel();
                        lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                        if (lstGC == null)
                            lstGC = new List<string>();
                    }

                    column = 1;

                    // Store name
                    row++;
                    ws.Range("A" + row, "C" + row).Merge().SetValue(_currentStore.Name);

                    #region Columns Names
                    row++;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No Sale"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PAX"));
                    int receiptTotalColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discounts"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotions"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Svc Chg"));
                    if (!_currentStore.IsIncludeTax)
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Add on)"));
                    }
                    else
                    {
                        ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax(Inc)"));
                    }
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Rounding"));
                    int refundColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tips"));
                    //=============================================================
                    //Check no include on sale
                    //=============================================================
                    lstItemNoIncludeSaleGroupByCate = new List<ItemizedSalesAnalysisReportModels>();
                    // Update -> show all categories have GLCode (12/12/2017)
                    if (_lstCates != null && _lstCates.Any())
                    {
                        var lstNoIncludeOnSaleGroupByCate = _lstCates.GroupBy(gg => gg.GLCode).OrderBy(x => x.Key).ToList();
                        for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                        {
                            lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                            {
                                GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                            });
                            if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                            {
                                ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                            }
                            else
                                ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                        }
                    }
                    else
                    {
                        if (lstItemNoIncludeSaleForCate != null && lstItemNoIncludeSaleForCate.Any())
                        {
                            var lstNoIncludeOnSaleGroupByCate = lstItemNoIncludeSaleForCate.Where(ww => ww.StoreId == _currentStore.Id)
                                .GroupBy(gg => gg.GLAccountCode).OrderBy(x => x.Key).ToList();
                            for (int y = 0; y < lstNoIncludeOnSaleGroupByCate.Count; y++)
                            {
                                lstItemNoIncludeSaleGroupByCate.Add(new ItemizedSalesAnalysisReportModels()
                                {
                                    GLAccountCode = lstNoIncludeOnSaleGroupByCate[y].Key

                                });
                                if (!string.IsNullOrEmpty(lstNoIncludeOnSaleGroupByCate[y].Key) && lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().StartsWith("D_"))
                                {
                                    ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key.Trim().ToUpper().Replace("D_", ""));
                                }
                                else
                                    ws.Range(row, column, row + 1, column++).Merge().SetValue(lstNoIncludeOnSaleGroupByCate[y].Key);
                            }
                        }
                    }

                    ///End Check no include on sale
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                    int startPayColumn = column;
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash"));

                    List<string> lstNotInPayMent = new List<string>() { "Cash" };
                    List<string> lstNotInPayMentNew = new List<string>() { cash != null ? cash.Id : "" };

                    List<string> storeCards = new List<string>();

                    var lstPaymentParents = lstPaymentMethod.Where(ww => string.IsNullOrEmpty(ww.ParentId) && (ww.Id != cash.Id || ww.Name.ToLower() != "cash")
                        && ww.StoreId == _currentStore.Id).OrderBy(ww => ww.Name).ToList();

                    for (int p = 0; p < lstPaymentParents.Count; p++)
                    {
                        lstPaymentParents[p].ListChilds = lstPaymentMethod.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == lstPaymentParents[p].Id
                        && ww.StoreId == _currentStore.Id).OrderBy(ww => ww.Name).ToList();
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

                    int endPayColumn = column;
                    // end group payments
                    ws.Range(row, column, row + 1, column++).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("PaymentTotal"));
                    ws.Range(row, column, row + 1, column).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess/Shortage"));
                    #endregion

                    // set value for last column
                    if (maxColumn < column)
                        maxColumn = column;

                    #region Format Store Header
                    ws.Range(row - 2, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                    ws.Range(row - 2, 1, row + 1, column).Style.Font.SetBold(true);
                    ws.Range(row - 2, 1, row + 1, column).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                    ws.Range(row, 1, row + 1, column).Style.Alignment.SetWrapText(true);
                    ws.Range(row, 1, row + 1, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(row, 1, row + 1, column).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row + 1, column).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    #endregion

                    row += 2;
                    int startRow = row;

                    var lstBusDay = _lstBusDayAllStore.Where(ww => ww.StoreId == _currentStore.Id).ToList();
                    List<DailySalesReportInsertModels> lstDataByStore = lstData.Where(ww => ww.StoreId == _currentStore.Id).ToList();
                    total = new Dictionary<int, double>();
                    if (lstDataByStore != null && lstDataByStore.Any())
                    {
                        // Fill data==================================================================
                        //============================================================================
                        payTotal = 0; payAmount = 0;

                        List<BusinessDayDisplayModels> lstBusDayShift = new List<BusinessDayDisplayModels>();

                        for (int j = 0; j < lstBusDay.Count; j++)
                        {
                            lstBusDayShift = new List<BusinessDayDisplayModels>();
                            lstBusDayShift.Add(lstBusDay[j]);
                            if (lstShiftInfo != null && lstShiftInfo.Any())
                            {
                                var lstShiftByBusDayInStore = lstShiftInfo.Where(w => w.BusinessId == lstBusDay[j].Id && w.StoreId == lstBusDay[j].StoreId)
                                    .OrderBy(o => o.StartedOn).ToList();

                                if (lstShiftByBusDayInStore != null && lstShiftByBusDayInStore.Any())
                                {
                                    foreach (var shift in lstShiftByBusDayInStore)
                                    {
                                        var obj = new BusinessDayDisplayModels();
                                        obj.DateFrom = shift.StartedOn;
                                        if (shift.ClosedOn == Commons.MinDate)
                                        {
                                            obj.DateTo = new DateTime(shift.StartedOn.Year, shift.StartedOn.Month, shift.StartedOn.Day, 23, 59, 59);
                                        }
                                        else
                                        {
                                            obj.DateTo = shift.ClosedOn;
                                        }
                                        lstBusDayShift.Add(obj);
                                    }
                                }
                            }
                            int numberShift = 0;
                            foreach (var BusDayShift in lstBusDayShift)
                            {
                                column = 1;
                                payTotal = 0;
                                payAmount = 0;

                                var reportItems = lstDataByStore.Where(s => s.CreatedDate >= BusDayShift.DateFrom && s.CreatedDate <= BusDayShift.DateTo).ToList();
                                if (reportItems == null || reportItems.Count == 0)
                                    continue;
                                var _lstOrderIds = reportItems.Select(ww => ww.OrderId).ToList();
                                // If this is Shift
                                if (string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    numberShift++;
                                    lstShiftRow.Add(row);
                                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell(row, column++).SetValue("   Shift" + numberShift + " " + BusDayShift.DateFrom.ToString("HH:mm") + " - " + BusDayShift.DateTo.ToString("HH:mm"));
                                }
                                // If this is Business day
                                else
                                {
                                    ws.Cell(row, column++).SetValue(BusDayShift.DateFrom.ToString("MM/dd/yyyy"));
                                }

                                #region add value to cell
                                //NoSale
                                noSale = _noSaleDetailReportFactory.GetNoSale(_currentStore.Id, BusDayShift.DateFrom, BusDayShift.DateTo);
                                ws.Cell(row, column++).Value = noSale;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, noSale);
                                    }
                                    else
                                    {
                                        total[column] += noSale;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, noSale);
                                    }
                                    else
                                    {
                                        totalCompany[column] += noSale;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, noSale);
                                    }
                                    else
                                    {
                                        grandTotal[column] += noSale;
                                    }
                                }

                                //TC - NoOfReceipt
                                TC = reportItems.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Count();
                                ws.Cell(row, column++).Value = TC;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, TC);
                                    }
                                    else
                                    {
                                        total[column] += TC;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, TC);
                                    }
                                    else
                                    {
                                        totalCompany[column] += TC;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, TC);
                                    }
                                    else
                                    {
                                        grandTotal[column] += TC;
                                    }
                                }

                                //PAX - NoOfPerson
                                PAX = reportItems.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Sum(ss => ss.NoOfPerson);
                                ws.Cell(row, column++).Value = PAX;
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, PAX);
                                    }
                                    else
                                    {
                                        total[column] += PAX;
                                    }
                                    //company total
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, PAX);
                                    }
                                    else
                                    {
                                        totalCompany[column] += PAX;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, PAX);
                                    }
                                    else
                                    {
                                        grandTotal[column] += PAX;
                                    }
                                }

                                // ReceiptTotal
                                receiptTotal = reportItems.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Sum(ss => ss.ReceiptTotal);

                                ws.Cell(row, column++).Value = receiptTotal.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        total[column] += receiptTotal;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        totalCompany[column] += receiptTotal;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, receiptTotal);
                                    }
                                    else
                                    {
                                        grandTotal[column] += receiptTotal;
                                    }
                                }

                                //Discount
                                double discount = reportItems.Sum(ss => ss.Discount) * (-1);
                                ws.Cell(row, column++).Value = discount.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, discount);
                                    }
                                    else
                                    {
                                        total[column] += discount;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, discount);
                                    }
                                    else
                                    {
                                        totalCompany[column] += discount;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, discount);
                                    }
                                    else
                                    {
                                        grandTotal[column] += discount;
                                    }
                                }

                                //promotions
                                double promo = reportItems.Sum(ss => ss.PromotionValue) * (-1);
                                ws.Cell(row, column++).Value = promo.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, promo);
                                    }
                                    else
                                    {
                                        total[column] += promo;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, promo);
                                    }
                                    else
                                    {
                                        totalCompany[column] += promo;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, promo);
                                    }
                                    else
                                    {
                                        grandTotal[column] += promo;
                                    }
                                }

                                //ServiceCharge
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.ServiceCharge).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.ServiceCharge));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.ServiceCharge);
                                    }
                                }

                                //Tax
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.GST).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.GST));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.GST);
                                    }
                                }

                                _roudingTotal = reportItems.Sum(ss => ss.Rounding);
                                //Rounding
                                ws.Cell(row, column++).Value = reportItems.Sum(ss => ss.Rounding).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        total[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        totalCompany[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, reportItems.Sum(ss => ss.Rounding));
                                    }
                                    else
                                    {
                                        grandTotal[column] += reportItems.Sum(ss => ss.Rounding);
                                    }
                                }

                                //Refund
                                _refundByCash = _lstRefunds.Where(ww => _lstOrderIds.Contains(ww.OrderId) && !ww.IsGiftCard).Sum(ss => ss.TotalRefund);
                                _refundGC = _lstRefunds.Where(ww => _lstOrderIds.Contains(ww.OrderId) && ww.IsGiftCard).Sum(ss => ss.TotalRefund);

                                ws.Cell(row, column++).Value = (_refundByCash + _refundGC).ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, (_refundByCash + _refundGC));
                                    }
                                    else
                                    {
                                        total[column] += (_refundByCash + _refundGC);
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, (_refundByCash + _refundGC));
                                    }
                                    else
                                    {
                                        totalCompany[column] += (_refundByCash + _refundGC);
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, (_refundByCash + _refundGC));
                                    }
                                    else
                                    {
                                        grandTotal[column] += (_refundByCash + _refundGC);
                                    }
                                }

                                //Tip
                                _tip = reportItems.Sum(ss => ss.Tip);
                                ws.Cell(row, column++).Value = _tip.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, _tip);
                                    }
                                    else
                                    {
                                        total[column] += _tip;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, _tip);
                                    }
                                    else
                                    {
                                        totalCompany[column] += _tip;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, _tip);
                                    }
                                    else
                                    {
                                        grandTotal[column] += _tip;
                                    }
                                }

                                //=======================================================
                                //NAV - NoIncludeOnSale (- discount - promotion)
                                // GL account code and group by it
                                //Note: value is not include tax, right? It means
                                //Tax is include on item price -> value = [[(price of items x qty ) - (disc or promo )] +SVC ] -Tax
                                //Tax is add on ->value = [(price of items x qty ) - (disc or promo )] +SVC

                                double _tax = 0;
                                for (int a = 0; a < lstItemNoIncludeSaleGroupByCate.Count; a++)
                                {

                                    _tax = lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                                && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                            && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode && p.TaxType == (int)Commons.ETax.Inclusive)
                                                            .Sum(p => p.Tax);

                                    payAmountByCate = (double)lstItemNoIncludeSaleForCate.Where(p => p.StoreId == _currentStore.Id
                               && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                           && p.GLAccountCode == lstItemNoIncludeSaleGroupByCate[a].GLAccountCode)
                                                           .Sum(p => ((decimal)p.TotalAmount
                                                               - (decimal)p.TotalDiscount
                                                               - (decimal)p.PromotionAmount));

                                    payAmountByCate -= _tax;

                                    if (lstItemNoIncludeSaleGroupByCate[a].GLAccountCode.ToUpper().Trim().StartsWith("D_"))
                                    {
                                        payAmountByCate += lstMiscs.Where(ww => ww.StoreId == _currentStore.Id && ww.CreatedDate >= BusDayShift.DateFrom
                                                && ww.CreatedDate <= BusDayShift.DateTo).Sum(ss => ss.MiscValue);
                                        payAmountByCate += _roudingTotal;

                                    }
                                    if (payAmountByCate < 0)
                                    {
                                        payAmountByCate = 0;
                                    }
                                    ws.Cell(row, column++).Value = payAmountByCate.ToString("F");
                                    // If this is business day
                                    if (!string.IsNullOrEmpty(BusDayShift.Id))
                                    {
                                        if (!total.ContainsKey(column))
                                        {
                                            total.Add(column, CommonHelper.RoundUp2Decimal(payAmountByCate));
                                        }
                                        else
                                        {
                                            total[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                        //total company
                                        if (!totalCompany.ContainsKey(column))
                                        {
                                            totalCompany.Add(column, CommonHelper.RoundUp2Decimal(payAmountByCate));
                                        }
                                        else
                                        {
                                            totalCompany[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                        //grand total
                                        if (!grandTotal.ContainsKey(column))
                                        {
                                            grandTotal.Add(column, CommonHelper.RoundUp2Decimal(payAmountByCate));
                                        }
                                        else
                                        {
                                            grandTotal[column] += CommonHelper.RoundUp2Decimal(payAmountByCate);
                                        }
                                    }
                                }

                                //NAV - End NoIncludeOnSale
                                //=======================================================
                                //NetSales -NAV - payGC 
                                #region NetSales
                                double _netSale = 0;
                                double payGC = 0;
                                double _taxOfPayGCNotInclude = 0;
                                double _svcOfPayGCNotInclude = 0;

                                var lstPaymentsInStore = lstPayments.Where(p => p.StoreId == _currentStore.Id
                                        && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                        && lstGC.Contains(p.PaymentId)
                                         && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();
                                if (lstPaymentsInStore != null && lstPaymentsInStore.Any())
                                {
                                    double refundAmount = 0;
                                    double _amount = 0;
                                    foreach (var item in lstPaymentsInStore)
                                    {
                                        _amount = item.Amount;
                                        refundAmount = 0;
                                        var lstGCRefunds = _lstRefunds.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCRefunds != null && lstGCRefunds.Any())
                                        {
                                            refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                            _amount -= refundAmount;
                                        }
                                        payGC += _amount;

                                        var receipt = reportItems.Where(ww => ww.OrderId == item.OrderId).FirstOrDefault();
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
                                //netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (1)
                                //(1)= GC pay not include - tax of pay GC not include - SCV of pay GC not include 

                                var totalReceipt = reportItems.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo)).Sum(ss => ss.ReceiptTotal);
                                var totalSVC = reportItems.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo)).Sum(ss => ss.ServiceCharge);
                                var totalTax = reportItems.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo)).Sum(ss => ss.GST);

                                var creditNoteIds = reportItems.Where(ww => !string.IsNullOrEmpty(ww.CreditNoteNo)).Select(ss => ss.OrderId).ToList();
                                double totalCreditNote = 0;
                                if (creditNoteIds != null && creditNoteIds.Any())
                                {
                                    totalCreditNote = lstItemNoIncludeSale.Where(ww => creditNoteIds.Contains(ww.ReceiptId)
                                    && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                        .Sum(ss => ss.ItemTotal);
                                }

                                double giftCardSell = lstItemNoIncludeSale.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                     && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                     && ww.BusinessId == lstBusDay[j].Id
                                                     && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                                var taxInclude = lstItemNoIncludeSale.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                && ww.BusinessId == lstBusDay[j].Id).Sum(ss => ss.Tax);

                                var _noincludeSale = lstItemNoIncludeSale.Where(ww => ww.IsIncludeSale == false
                                && (ww.ItemTypeId == (int)Commons.EProductType.Dish || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                && ww.BusinessId == lstBusDay[j].Id).Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                                _noincludeSale -= taxInclude;


                                _netSale = totalReceipt - totalCreditNote - totalSVC - totalTax - giftCardSell - _noincludeSale
                                    - (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude);

                                #endregion End NetSales

                                ws.Cell(row, column++).Value = _netSale.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {

                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        total[column] += _netSale;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        totalCompany[column] += _netSale;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, _netSale);
                                    }
                                    else
                                    {
                                        grandTotal[column] += _netSale;
                                    }
                                }

                                // pay by cash (subtract refund)
                                payByCash = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                        && (p.PaymentId == lstNotInPayMentNew[0] || (p.PaymentName == lstNotInPayMent[0]))
                                                        ).Sum(p => p.Amount) - _refundByCash;

                                ws.Cell(row, column++).Value = payByCash.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        total[column] += payByCash;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        totalCompany[column] += payByCash;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payByCash);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payByCash;
                                    }
                                }

                                payTotal = payByCash;
                                //Payment methods other cash
                                for (int k = 0; k < storeCards.Count; k++)
                                {
                                    if (storeCards[k] != "")
                                    {
                                        payAmount = lstPayments.Where(p => p.StoreId == _currentStore.Id && p.CreatedDate >= BusDayShift.DateFrom && p.CreatedDate <= BusDayShift.DateTo
                                                                      && p.PaymentId == storeCards[k]).Sum(p => p.Amount);
                                        //check isGC
                                        if (lstGC.Contains(storeCards[k]))
                                            payAmount -= _refundGC;
                                    }
                                    else
                                        payAmount = 0;

                                    ws.Cell(row, column++).Value = payAmount.ToString("F");
                                    // If this is business day
                                    if (!string.IsNullOrEmpty(BusDayShift.Id))
                                    {
                                        if (!total.ContainsKey(column))
                                        {
                                            total.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            total[column] += payAmount;
                                        }
                                        //total company
                                        if (!totalCompany.ContainsKey(column))
                                        {
                                            totalCompany.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            totalCompany[column] += payAmount;
                                        }
                                        //grand total
                                        if (!grandTotal.ContainsKey(column))
                                        {
                                            grandTotal.Add(column, payAmount);
                                        }
                                        else
                                        {
                                            grandTotal[column] += payAmount;
                                        }
                                    }
                                    payTotal += payAmount;
                                }

                                //PaymentTotal
                                ws.Cell(row, column++).Value = payTotal.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column))
                                    {
                                        total.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        total[column] += payTotal;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column))
                                    {
                                        totalCompany.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        totalCompany[column] += payTotal;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column))
                                    {
                                        grandTotal.Add(column, payTotal);
                                    }
                                    else
                                    {
                                        grandTotal[column] += payTotal;
                                    }
                                }

                                //excess
                                double excess = payTotal - receiptTotal;
                                ws.Cell(row, column).Value = excess.ToString("F");
                                // If this is business day
                                if (!string.IsNullOrEmpty(BusDayShift.Id))
                                {
                                    if (!total.ContainsKey(column + 1))
                                    {
                                        total.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        total[column + 1] += excess;
                                    }
                                    //total company
                                    if (!totalCompany.ContainsKey(column + 1))
                                    {
                                        totalCompany.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        totalCompany[column + 1] += excess;
                                    }
                                    //grand total
                                    if (!grandTotal.ContainsKey(column + 1))
                                    {
                                        grandTotal.Add(column + 1, excess);
                                    }
                                    else
                                    {
                                        grandTotal[column + 1] += excess;
                                    }
                                }
                                #endregion

                                row++;
                            }
                        }
                        //====================================================================================
                        //End loop Bussiness day
                        //====================================================================================
                    }

                    // Total Row
                    if (isMultiComp)
                    {
                        ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("STORE TOTAL"));
                    }
                    else
                    {
                        ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                    }
                    for (int j = 2; j <= maxColumn; j++)
                    {
                        if (total.ContainsKey(j + 1))
                            ws.Cell(row, j).Value = total[j + 1];
                    }
                    ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                    ws.Range(startRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                    row++;
                    currentCompanyRow = row;

                }//end for list store

                //summary company
                if (isMultiComp)
                {
                    ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("COMPANY TOTAL"));
                }
                else
                {
                    ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL"));
                }
                for (int j = 2; j <= maxColumn; j++)
                {
                    if (totalCompany.ContainsKey(j + 1))
                        ws.Cell(row, j).Value = totalCompany[j + 1];
                }
                ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                ws.Range(currentCompanyRow, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";

                row++;
            }
            //end group company
            //Grand total
            if (isMultiComp)
            {
                ws.Cell(row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GRAND TOTAL"));
                for (int j = 2; j <= maxColumn; j++)
                {
                    if (grandTotal.ContainsKey(j + 1))
                        ws.Cell(row, j).Value = grandTotal[j + 1];
                }
                ws.Range(row, 1, row, maxColumn).Style.Font.SetBold(true);
                row++;
                ws.Range(row - 1, 5, row, maxColumn).Style.NumberFormat.Format = "#,##0.00";
            }

            ws.Range(5, 1, row - 1, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(5, 1, row - 1, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            #region Table Header
            CreateReportHeaderNew(ws, maxColumn, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Sales Closing Report"));
            ws.Range(1, 1, 4, maxColumn).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxColumn).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            #endregion

            // Format Shift row
            if (lstShiftRow != null && lstShiftRow.Any())
            {
                foreach (int shiftRow in lstShiftRow)
                {
                    ws.Range(shiftRow, 1, shiftRow, maxColumn).Style.Font.SetFontColor(XLColor.FromHtml("#974706"));
                }
            }

            ws.Columns().AdjustToContents();

            // Set Width for Colum 
            ws.Column(1).Width = 25;
            ws.Column(2).Width = 9;
            ws.Column(3).Width = 9;
            ws.Column(4).Width = 9;

            for (int i = 4; i < maxColumn; i++)
            {
                ws.Column(i + 1).Width = 17;
            }

            return wb;
        }
        #endregion
    }
}
