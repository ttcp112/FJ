using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using ClosedXML.Excel;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Shared.Factory
{
    public class AuditTrailReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private RefundFactory _refundFactory = null;
        private DiscountDetailsReportFactory _discountFactory = null;
        private ItemizedCancelRefundFactory _cancelRefundFactory = null;

        public AuditTrailReportFactory()
        {
            _baseFactory = new BaseFactory();
            _refundFactory = new RefundFactory();
            _discountFactory = new DiscountDetailsReportFactory();
            _cancelRefundFactory = new ItemizedCancelRefundFactory();

        }
        public bool Insert(List<AuditTrailReportModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //_logger.Info("==========================================================");
                //_logger.Info(string.Format("Insert AuditTrailReport: StoreId: [{0}] | BusinessId: [{1}]", info.StoreId, info.BusinessDayId));
                //Check Exist
                var obj = cxt.R_AuditTrailReport.Where(ww => ww.BusinessDayId == info.BusinessDayId && ww.StoreId == info.StoreId).FirstOrDefault();
                if(obj != null)
                {
                    NSLog.Logger.Info("Insert Audit Trail data exist", lstInfo);
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_AuditTrailReport> lstInsert = new List<R_AuditTrailReport>();
                        R_AuditTrailReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_AuditTrailReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.BusinessDayId = item.BusinessDayId;
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.ReceiptDate = item.ReceiptDate;
                            itemInsert.ReceiptID = item.ReceiptID;
                            itemInsert.ReceiptNo = item.ReceiptNo;
                            itemInsert.ReceiptStatus = item.ReceiptStatus;
                            itemInsert.OrderNo = item.OrderNo;
                            itemInsert.CashierId = item.CashierId;
                            itemInsert.CashierName = item.CashierName;
                            itemInsert.Discount = item.DiscountAmount;
                            itemInsert.ReceiptTotal = item.ReceiptTotal;
                            itemInsert.CancelAmount = item.CancelAmount;
                            itemInsert.Mode = item.Mode;

                            //07/08/2014
                            itemInsert.PromotionAmount = item.PromotionAmount;
                            itemInsert.CreditNoteNo = item.CreditNoteNo;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_AuditTrailReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        //_logger.Info(string.Format("Insert AuditTrailReport: StoreId: [{0}] | BusinessId: [{1}] Success", info.StoreId, info.BusinessDayId));
                        NSLog.Logger.Info("Insert Audit Trail data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert Audit Trail data fail", lstInfo);
                        NSLog.Logger.Error("Insert Audit Trail data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_AuditTrailReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<AuditTrailReportModels> GetData(BaseReportModel model, string StoreId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_AuditTrailReport
                               where tb.StoreId == StoreId
                                     && (tb.ReceiptDate >= model.FromDate && tb.ReceiptDate <= model.ToDate)
                               orderby tb.OrderNo, tb.ReceiptNo
                               select new AuditTrailReportModels
                               {
                                   CancelAmount = tb.CancelAmount,
                                   CashierId = tb.CashierId,
                                   CashierName = tb.CashierName,
                                   DiscountAmount = tb.Discount,
                                   ReceiptDate = tb.ReceiptDate,
                                   ReceiptID = tb.ReceiptID,
                                   ReceiptNo = tb.ReceiptNo,
                                   ReceiptStatus = tb.ReceiptStatus,
                                   ReceiptTotal = tb.ReceiptTotal,
                                   StoreId = tb.StoreId,
                                   OrderNo = tb.OrderNo
                               }).ToList();
                return lstData;

            }
        }

        private void FormatStoreHeader(string storeName, ref IXLWorksheet ws, ref int row)
        {
            int startRow = row;
            ws.Range(row, 1, row, 11).Merge().Value = storeName;
            ws.Range(row, 1, row++, 11).Style.Font.Bold = true;

            ws.Range(row, 1, row + 1, 1).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
            ws.Range(row, 2, row + 1, 2).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
            ws.Range(row, 3, row + 1, 3).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Order No"));
            ws.Range(row, 4, row + 1, 4).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No"));
            ws.Range(row, 5, row + 1, 5).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cashier"));
            ws.Range(row, 6, row + 1, 6).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Amount"));
            ws.Range(row, 7, row + 1, 7).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cancel Amount"));
            ws.Range(row, 8, row + 1, 8).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund Amount"));
            ws.Range(row, 9, row, 10).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
            ws.Range(row, 9, row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row + 1, 9).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("On"));
            ws.Cell(row + 1, 10).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("At"));
            ws.Range(row, 11, row + 1, 11).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Amount"));
            ws.Range(row, 1, row + 1, 11).Style.Font.Bold = true;

            row++;
            ws.Range(startRow, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(startRow, 1, row, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row, 11).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(startRow, 1, row++, 11).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
        }

        private void FormatStoreData(ref IXLWorksheet ws, int startRow, int endRow)
        {
            ws.Range(startRow, 1, endRow - 1, 1).Style.DateFormat.Format = "MM/dd/yyyy";
            ws.Range(startRow, 2, endRow - 1, 2).Style.DateFormat.Format = "hh:mm AM/PM";
            ws.Range(startRow, 1, endRow - 1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Range(endRow, 1, endRow, 11).Style.Font.Bold = true;
            ws.Range(endRow, 1, endRow, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(startRow, 6, endRow, 8).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRow, 11, endRow, 11).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(startRow, 6, endRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(startRow, 11, endRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Range(startRow, 1, endRow, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, endRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, endRow, 11).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(startRow, 1, endRow, 11).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
        }

        private void FormatSummaryData(ref IXLWorksheet ws, int startRow, int endRow)
        {
            ws.Range(startRow, 1, endRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(startRow, 1, endRow, 3).Style.Font.Bold = true;
            ws.Range(startRow, 1, endRow, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(startRow, 1, endRow, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, endRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, endRow, 3).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(startRow, 1, endRow, 3).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
        }

        private void FormatTotalData(ref IXLWorksheet ws, int row)
        {
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 4, row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            ws.Range(row, 1, row, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 5).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(row, 1, row, 5).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
        }

        public XLWorkbook Report(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Audit_Trail"/*_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Audit_Trail")*/);

            CreateReportHeaderNew(ws, 11, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Audit Trail Report"));
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            var _lstRefunds = _refundFactory.GetListRefundWithoutDetails(model.ListStores, model.FromDate, model.ToDate, model.Mode);
            int row = 5;
            string storeName = string.Empty, storeId = string.Empty;
            double totalRefund = 0;
            int receiptCount = 0;
            double receiptAmount = 0, totalSales = 0, refundReceiptCount = 0, refundAmount = 0
                , cancelReceiptCount = 0, cancelAmount = 0, discountReceiptCount = 0, discountAmount = 0;
            for (int i = 0; i < lstStore.Count; i++)
            {
                refundReceiptCount = 0;
                refundAmount = 0;
                //Get StoreName
                StoreModels store = lstStore[i];
                storeName = store.Name;
                storeId = store.Id;
                FormatStoreHeader(storeName, ref ws, ref row);

                List<AuditTrailReportModels> receipts = GetData(model, storeId);
                receiptCount = 0;
                receiptAmount = 0;
                totalSales = 0;
                cancelReceiptCount = 0;
                cancelAmount = 0;
                discountReceiptCount = 0;
                discountAmount = 0;

                if (receipts != null && receipts.Count > 0)
                {
                    //Get all business day in store
                    var businessInStore = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                    int startRow = row;
                    for (int d = 0; d < businessInStore.Count; d++)
                    {
                        List<AuditTrailReportModels> lstDataInDate = receipts.Where(m => m.ReceiptDate >= businessInStore[d].DateFrom
                        && m.ReceiptDate <= businessInStore[d].DateTo && m.StoreId == storeId).ToList();

                        for (int j = 0; j < lstDataInDate.Count; j++)
                        {
                            //ws.Cell("A" + row).Value = businessInStore[d].DateFrom;
                            ws.Cell("A" + row).Value = lstDataInDate[j].ReceiptDate;
                            ws.Cell("B" + row).Value = lstDataInDate[j].ReceiptDate.TimeOfDay;
                            ws.Cell("C" + row).Value = lstDataInDate[j].OrderNo;
                            ws.Cell("D" + row).Value = lstDataInDate[j].ReceiptNo;
                            ws.Cell("E" + row).Value = lstDataInDate[j].CashierName;
                            ws.Cell("F" + row).Value = lstDataInDate[j].DiscountAmount.ToString("N");
                            ws.Cell("G" + row).Value = lstDataInDate[j].CancelAmount.ToString("N");
                            ws.Cell("K" + row).Value = lstDataInDate[j].ReceiptTotal.ToString("N");
                            //Refund
                            try
                            {
                                totalRefund = 0;
                                totalRefund = _lstRefunds.Where(ww => ww.OrderId == lstDataInDate[j].ReceiptID && ww.BusinessDayId == businessInStore[d].Id).Sum(ss => ss.TotalRefund);
                                if (totalRefund != 0)
                                    refundReceiptCount++;
                                refundAmount += totalRefund;
                                ws.Cell("H" + row).Value = totalRefund.ToString("N");
                                var refundDay = _lstRefunds.Where(ww => ww.OrderId == lstDataInDate[j].ReceiptID && ww.BusinessDayId == businessInStore[d].Id).FirstOrDefault();
                                if (refundDay != null)
                                {
                                    ws.Cell("I" + row).Value = refundDay.CreatedDate;
                                    ws.Cell("J" + row).Value = refundDay.CreatedDate.TimeOfDay;

                                    ws.Cell("I" + row).Style.DateFormat.Format = "MM/dd/yyyy";
                                    ws.Cell("J" + row).Style.DateFormat.Format = "hh:mm AM/PM";
                                }
                            }
                            catch
                            { }
                            if (!string.IsNullOrEmpty(lstDataInDate[j].ReceiptNo))
                                receiptCount++;
                            receiptAmount += lstDataInDate[j].ReceiptTotal;
                            totalSales += lstDataInDate[j].ReceiptTotal;
                            if (lstDataInDate[j].CancelAmount != 0 && !string.IsNullOrEmpty(lstDataInDate[j].ReceiptNo))
                            {
                                cancelReceiptCount++;
                                cancelAmount += lstDataInDate[j].CancelAmount;
                            }
                            if (lstDataInDate[j].DiscountAmount != 0 && !string.IsNullOrEmpty(lstDataInDate[j].ReceiptNo))
                                discountReceiptCount++;
                            discountAmount += lstDataInDate[j].DiscountAmount;


                            row++;
                        }
                    }
                    //receiptCount = receipts.Where(x => !string.IsNullOrEmpty(x.ReceiptNo)).Count();
                    //receiptAmount = receipts.Sum(r => r.ReceiptTotal);
                    //totalSales = receipts.Sum(r => r.ReceiptTotal);
                    ////refundReceiptCount = receipts.Where(r => r.TotalRefund != 0).Count();
                    ////refundAmount = receipts.Sum(r => r.TotalRefund);
                    //cancelReceiptCount = receipts.Where(r => r.CancelAmount != 0).Count();
                    //cancelAmount = receipts.Sum(r => r.CancelAmount);
                    //discountReceiptCount = receipts.Where(r => r.DiscountAmount != 0).Count();
                    //discountAmount = receipts.Sum(r => r.DiscountAmount);

                    // Total Row
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    ws.Cell("F" + row).Value = discountAmount.ToString("N");
                    ws.Cell("G" + row).Value = cancelAmount.ToString("N");
                    ws.Cell("H" + row).Value = refundAmount.ToString("N");
                    ws.Cell("K" + row).Value = receiptAmount.ToString("N");
                    FormatStoreData(ref ws, startRow, row);
                    row++;

                    // Total Sales
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL SALES") + ": " + totalSales.ToString("N");
                    ws.Cell("B" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL TC") + ": " + receiptCount;
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.TC") + " = " + cancelReceiptCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.TC") + "% = " + (cancelReceiptCount / receiptCount * 100).ToString("N");
                    ws.Cell("C" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.AMT") + "% = " + (cancelAmount / totalSales * 100).ToString("N");
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.TC") + " = " + refundReceiptCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.TC") + "% = " + (refundReceiptCount / receiptCount * 100).ToString("N");
                    ws.Cell("C" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.AMT") + "% = " + (refundAmount / totalSales * 100).ToString("N");
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.TC") + " = " + discountReceiptCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.TC") + "% = " + (discountReceiptCount / receiptCount * 100).ToString("N");
                    ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.AMT") + "% = " + (discountAmount / totalSales * 100).ToString("N");
                    FormatSummaryData(ref ws, row - 3, row);
                }
                row += 2;
            }
            //ws.Columns().AdjustToContents();

            List<int> lstWidCol = new List<int>() { 28, 17, 17, 17, 20, 18, 18, 18, 15, 10, 17 };
            for (int i = 0; i < lstWidCol.Count; i++)
            {
                ws.Column(i + 1).Width = lstWidCol[i];
            }
            return wb;
        }

        public List<AuditTrailReportModels> GetData_New(DateTime dateFrom, DateTime dateTo, List<string> lstStoreIds, List<string> lstBusinessInputIds, int mode = 1)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_AuditTrailReport
                               where lstStoreIds.Contains(tb.StoreId)
                                     && (tb.ReceiptDate >= dateFrom && tb.ReceiptDate <= dateTo) 
                                     //&& tb.Mode == mode 
                                     && lstBusinessInputIds.Contains(tb.BusinessDayId)
                               orderby tb.OrderNo, tb.ReceiptNo
                               select new AuditTrailReportModels
                               {
                                   ReceiptDate = tb.ReceiptDate,
                                   BusinessDayId = tb.BusinessDayId,
                                   ReceiptID = tb.ReceiptID,
                                   ReceiptNo = tb.ReceiptNo,
                                   CashierId = tb.CashierId,
                                   CashierName = tb.CashierName,
                                   ReceiptTotal = tb.ReceiptTotal,
                                   StoreId = tb.StoreId,
                                   OrderNo = tb.OrderNo,
                                   PromotionAmount = tb.ReceiptStatus == (int)Commons.EStatus.Deleted? 0: (tb.PromotionAmount.HasValue ? tb.PromotionAmount.Value : 0)
                               })
                               .ToList();
                return lstData;

            }
        }
        public XLWorkbook Report_New(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Audit_Trail_Report");

            CreateReportHeaderNew(ws, 22, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Audit Trail Report").ToUpper());

            // Get list business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Format header report
                ws.Range(1, 1, 4, 22).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            // Get data use business day
            var _lstBusDayIdsAllStore = _lstBusDayAllStore.Select(s => s.Id).ToList();
            model.ToDate = _lstBusDayAllStore.Max(oo => oo.DateTo);

            // Get list data receipt
            List<AuditTrailReportModels> listReceipts = GetData_New(model.FromDate, model.ToDate, model.ListStores, _lstBusDayIdsAllStore, model.Mode);
            //listReceipts = listReceipts.Where(ww => ww.OrderNo == "OR20170828-002").ToList();

            // Get list data discount
            var listDiscounts = _discountFactory.GetDataDiscounts(model.FromDate, model.ToDate, model.ListStores, _lstBusDayIdsAllStore, model.Mode);

            // Get list data cancel & refund
            var listCancelRefunds = _cancelRefundFactory.GetDataItemizedCancelRefunds(model.FromDate, model.ToDate, model.ListStores, _lstBusDayIdsAllStore);

            int row = 5;
            string storeName = string.Empty, storeId = string.Empty;

            for (int i = 0; i < lstStore.Count; i++)
            {
              
                StoreModels store = lstStore[i];
                storeName = store.Name;
                storeId = store.Id;

                // Set StoreName
                int startRowHeader = row;
                ws.Range(row, 1, row, 22).Merge().Value = storeName;
                ws.Range(row, 1, row++, 22).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorStore);

                // Set title column
                ws.Range(row, 1, row + 1, 1).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Range(row, 2, row + 1, 2).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
                ws.Range(row, 3, row + 1, 3).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Order No"));
                ws.Range(row, 4, row + 1, 4).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No"));
                ws.Range(row, 5, row + 1, 5).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cashier"));
                ws.Range(row, 6, row + 1, 6).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Amount"));

                ws.Range(row, 7, row, 11).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                ws.Cell(row + 1, 7).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"));
                ws.Cell(row + 1, 8).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell(row + 1, 9).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("On"));
                ws.Cell(row + 1, 10).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("At"));
                ws.Cell(row + 1, 11).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("By"));

                ws.Range(row, 12, row, 16).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cancelled"));
                ws.Cell(row + 1, 12).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Items"));
                ws.Cell(row + 1, 13).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell(row + 1, 14).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("On"));
                ws.Cell(row + 1, 15).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("At"));
                ws.Cell(row + 1, 16).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("By"));

                ws.Range(row, 17, row, 21).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refunded"));
                ws.Cell(row + 1, 17).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Items"));
                ws.Cell(row + 1, 18).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell(row + 1, 19).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("On"));
                ws.Cell(row + 1, 20).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("At"));
                ws.Cell(row + 1, 21).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("By"));

                ws.Range(row, 22, row + 1, 22).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Amount"));
                ws.Range(row, 1, row + 1, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row + 1, 22).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                row++;
                ws.Range(row - 1, 1, row, 22).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorHeader);
                ws.Range(startRowHeader, 1, row, 22).Style.Font.Bold = true;

                ws.Range(startRowHeader, 1, row, 22).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startRowHeader, 1, row, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startRowHeader, 1, row, 22).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
                ws.Range(startRowHeader, 1, row++, 22).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");


                // Get list receipts in a store
                var listReceiptsInStore = listReceipts.Where(w => w.StoreId == storeId).OrderBy(o => o.ReceiptDate).ToList();

                if (listReceiptsInStore.Count > 0)
                {
                    decimal promotionTotal = 0;
                    decimal discountTotal = 0;
                    decimal cancelTotal = 0;
                    decimal refundTotal = 0;
                    decimal receiptTotal = 0;

                    int countDate = 0;
                    int countTime = 0;
                    int countReceipt = 0;
                    int countItmDiscount = 0;
                    int countItmCancel = 0;
                    int countItmRefund = 0;

                    decimal promotionCount = 0;
                    decimal discountCount = 0;
                    decimal cancelOrderCount = 0;
                    decimal refundCount = 0;
                    decimal receiptCount = 0;

                    // Get list receipt date
                    var listDate = listReceiptsInStore.Select(s => new { s.ReceiptDate.Year, s.ReceiptDate.Month, s.ReceiptDate.Day }).Distinct().ToList();

                    foreach(var date in listDate)
                    {
                        int startRowDate = row;
                        countDate = 0;

                        // Set value for column Date
                        ws.Cell("A" + row).SetValue(new DateTime(date.Year, date.Month, date.Day, 1, 1, 1).ToString("MM/dd/yyyy"));

                        var listReceiptsInDate = listReceiptsInStore.Where(w => w.ReceiptDate.Year == date.Year && w.ReceiptDate.Month == date.Month && w.ReceiptDate.Day == date.Day).OrderBy(o => o.ReceiptDate).ToList();

                        // Get list receipt time
                        var listTime = listReceiptsInDate.Select(s => new { s.ReceiptDate.Hour, s.ReceiptDate.Minute }).Distinct().ToList();

                        foreach(var time in listTime)
                        {
                            int startRowTime = row;
                            countTime = 0;

                            // Set value for column Time
                            ws.Cell("B" + row).SetValue(new DateTime(1, 1, 1, time.Hour, time.Minute, 0).ToString("t"));

                            var listReceiptsInTime = listReceiptsInDate.Where(w => w.ReceiptDate.Hour == time.Hour && w.ReceiptDate.Minute == time.Minute).OrderBy(o => o.OrderNo).ToList();

                            foreach(var receipt in listReceiptsInTime)
                            {
                                int startRowOrder = row;
                                countReceipt = 0;

                                // Set value for column Order No
                                ws.Cell("C" + row).SetValue(receipt.OrderNo);

                                // Set value for column Receipt No
                                ws.Cell("D" + row).SetValue(receipt.ReceiptNo);
                                if (!string.IsNullOrEmpty(receipt.ReceiptNo)) receiptCount++;

                                // Set value for column Cashier
                                ws.Cell("E" + row).SetValue(receipt.CashierName);

                                // Set value for column Promotion Amount
                                ws.Cell("F" + row).SetValue(receipt.PromotionAmount);
                                promotionTotal += (decimal)receipt.PromotionAmount;
                                if (receipt.PromotionAmount > 0) promotionCount++;

                                // Get list data discount by receipt id
                                var listDiscountInReceipt = listDiscounts.Where(w => w.ReceiptId == receipt.ReceiptID && w.StoreId == storeId)
                                    .GroupBy(g => new
                                    {
                                        g.DiscountId,
                                        g.CreatedDate,
                                        g.UserDiscount
                                    }).OrderBy(o => o.Key.CreatedDate)
                                    .Select(
                                        s => new
                                        {
                                            DiscountId = s.FirstOrDefault().DiscountId,
                                            DiscountName = s.FirstOrDefault().DiscountName,
                                            DiscountAmount = s.Sum(ss => (decimal)ss.DiscountAmount),
                                            CreatedDate = s.FirstOrDefault().CreatedDate,
                                            UserDiscount = s.FirstOrDefault().UserDiscount
                                        }
                                    )
                                    .ToList();
                                if (listDiscountInReceipt != null && listDiscountInReceipt.Any()) discountCount++;

                                int startRowDiscount = row;
                                countItmDiscount = 0;

                                bool cancelAmountTotal = false;
                                bool refundAmountTotal = false;
                                // Set value for Discount
                                foreach (var discount in listDiscountInReceipt)
                                {
                                    // Name
                                    ws.Cell("G" + row).SetValue(discount.DiscountName);
                                    // Amount
                                    ws.Cell("H" + row).SetValue(discount.DiscountAmount);
                                    discountTotal += (decimal)discount.DiscountAmount;
                                    // On
                                    ws.Cell("I" + row).SetValue(discount.CreatedDate.ToString("MM/dd/yyyy"));
                                    // At
                                    ws.Cell("J" + row).SetValue(discount.CreatedDate.ToString("t"));
                                    // By
                                    ws.Cell("K" + row).SetValue(discount.UserDiscount);

                                    if(discount != listDiscountInReceipt.Last()) row++;
                                    countItmDiscount++;
                                }

                                // Get list data cancel & refund by receipt id
                                var listCancelRefundInReceipt = listCancelRefunds.Where(w => w.OrderId == receipt.ReceiptID && w.StoreId == storeId).OrderBy(o => o.CreatedDate).ToList();

                                if (listCancelRefundInReceipt.Count > 0)
                                {
                                    cancelAmountTotal = listCancelRefundInReceipt.Any(w => !w.IsRefund.HasValue || (w.IsRefund.HasValue && !w.IsRefund.Value));
                                        
                                    if (cancelAmountTotal) cancelOrderCount++;

                                    refundAmountTotal = listCancelRefundInReceipt.Any(w => w.IsRefund.HasValue && w.IsRefund.Value);
                                    if (refundAmountTotal) refundCount++;

                                    row = startRowDiscount;
                                }

                                //int startRowCancelRefund = startRowDiscount;
                                countItmCancel = 0;
                                countItmRefund = 0;
                                // Set value for Cancel & Refund
                                foreach (var cancelRefund in listCancelRefundInReceipt)
                                {
                                    if (cancelRefund.IsRefund == true)
                                    {
                                        // Set value for Refund
                                        // Name
                                        ws.Cell("Q" + row).SetValue(cancelRefund.ItemName);
                                        // Amount
                                        ws.Cell("R" + row).SetValue(cancelRefund.Amount);
                                        refundTotal += (decimal)cancelRefund.Amount;
                                        // On
                                        ws.Cell("S" + row).SetValue(cancelRefund.CreatedDate.ToString("MM/dd/yyyy"));
                                        // At
                                        ws.Cell("T" + row).SetValue(cancelRefund.CreatedDate.ToString("t"));
                                        // By
                                        ws.Cell("U" + row).SetValue(cancelRefund.RefundUser);

                                        countItmRefund++;
                                    } else
                                    {
                                        // Set value for Cancel
                                        // Name
                                        ws.Cell("L" + row).SetValue(cancelRefund.ItemName);
                                        // Amount
                                        ws.Cell("M" + row).SetValue(cancelRefund.Amount);
                                        cancelTotal += (decimal)cancelRefund.Amount;
                                        // On
                                        ws.Cell("N" + row).SetValue(cancelRefund.CreatedDate.ToString("MM/dd/yyyy"));
                                        // At
                                        ws.Cell("O" + row).SetValue(cancelRefund.CreatedDate.ToString("t"));
                                        // By
                                        ws.Cell("P" + row).SetValue(cancelRefund.CancelUser);

                                        countItmCancel++;
                                    }

                                    if (cancelRefund != listCancelRefundInReceipt.Last()) row++;
                                }
                                
                                // Set value column Receipt Amount
                                ws.Cell("V" + startRowDiscount).SetValue(receipt.ReceiptTotal);
                                receiptTotal += (decimal)receipt.ReceiptTotal;
                                row = startRowDiscount;

                                // Format merge cell
                                int maxCountCell = Math.Max(Math.Max(countItmDiscount, countItmCancel), countItmRefund);
                                row += (maxCountCell >0? maxCountCell:1);
                                if (maxCountCell > 1)
                                {
                                    countReceipt += maxCountCell;
                                    ws.Range(startRowOrder, 3, startRowOrder + maxCountCell - 1, 3).Merge();
                                    ws.Range(startRowOrder, 4, startRowOrder + maxCountCell - 1, 4).Merge();
                                    ws.Range(startRowOrder, 5, startRowOrder + maxCountCell - 1, 5).Merge();
                                    ws.Range(startRowOrder, 6, startRowOrder + maxCountCell - 1, 6).Merge();
                                    ws.Range(startRowOrder, 22, startRowOrder + maxCountCell - 1, 22).Merge();


                                    if (countItmDiscount < maxCountCell)
                                    {
                                        List<int> lstCol = new List<int>() { 7, 8, 9, 10, 11 };
                                        foreach (int col in lstCol)
                                        {
                                            ws.Range(startRowDiscount + countItmDiscount, col, startRowDiscount + maxCountCell - 1, col).Merge();
                                        }
                                    }
                                    if (countItmCancel < maxCountCell)
                                    {
                                        List<int> lstCol = new List<int>() { 12, 13, 14, 15, 16 };
                                        foreach (int col in lstCol)
                                        {
                                            ws.Range(startRowDiscount + countItmCancel, col, startRowDiscount + maxCountCell - 1, col).Merge();
                                        }
                                    }
                                    if (countItmRefund < maxCountCell)
                                    {
                                        List<int> lstCol = new List<int>() { 17, 18, 19, 20, 21 };
                                        foreach (int col in lstCol)
                                        {
                                            ws.Range(startRowDiscount + countItmRefund, col, startRowDiscount + maxCountCell - 1, col).Merge();
                                        }
                                    }
                                } else
                                {
                                    countReceipt++;
                                }

                                countTime += countReceipt;

                            }
                            
                            ws.Range(startRowTime, 2, startRowTime + countTime - 1, 2).Merge();
                            countDate += countTime;
                        }
                        ws.Range(startRowDate, 1, startRowDate + countDate - 1, 1).Merge();

                    }

                    // Total Row
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    ws.Cell("F" + row).Value = promotionTotal.ToString("N");
                    ws.Cell("H" + row).Value = discountTotal.ToString("N");
                    ws.Cell("M" + row).Value = cancelTotal.ToString("N");
                    ws.Cell("R" + row).Value = refundTotal.ToString("N");
                    ws.Cell("V" + row).Value = receiptTotal.ToString("N");
                    ws.Range(row, 1, row, 22).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorHeader);
                    ws.Range(row, 1, row++, 22).Style.Font.Bold = true;

                    // Total Sales
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL SALES") + ": " + receiptTotal.ToString("N");
                    ws.Cell("B" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL TC") + ": " + receiptCount;
                    // Cancel
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.TC") + " = " + cancelOrderCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.TC") + "% = " + (cancelOrderCount / (receiptCount == 0 ? 1 : receiptCount) * 100).ToString("N");
                    ws.Cell("C" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.AMT") + "% = " + (cancelTotal / (receiptTotal == 0 ? 1 : receiptTotal) * 100).ToString("N");
                    // Refund
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.TC") + " = " + refundCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.TC") + "% = " + (refundCount / (receiptCount == 0 ? 1 : receiptCount) * 100).ToString("N");
                    ws.Cell("C" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.AMT") + "% = " + (refundTotal / (receiptTotal == 0 ? 1 : receiptTotal) * 100).ToString("N");
                    // Discount
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.TC") + " = " + discountCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.TC") + "% = " + (discountCount / (receiptCount == 0 ? 1 : receiptCount) * 100).ToString("N");
                    ws.Cell("C" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.AMT") + "% = " + (discountTotal / (receiptTotal == 0 ? 1 : receiptTotal) * 100).ToString("N");
                    // Promotion
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("P.TC") + " = " + promotionCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("P.TC") + "% = " + (promotionCount / (receiptCount == 0 ? 1 : receiptCount) * 100).ToString("N");
                    ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("P.AMT") + "% = " + (promotionTotal / (receiptTotal==0?1: receiptTotal) * 100).ToString("N");

                    ws.Range(row - 5, 1, row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorHeader);
                    ws.Range(row - 5, 1, row, 3).Style.Font.Bold = true;
                    ws.Range(row - 5, 1, row, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row - 5, 1, row, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row - 5, 1, row, 3).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
                    ws.Range(row - 5, 1, row, 3).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");

                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
                    
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(startRowHeader, 1, startRowHeader, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("A" + (row - 5)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
                    ws.Column(8).Style.NumberFormat.Format = "#,##0.00";
                    ws.Column(13).Style.NumberFormat.Format = "#,##0.00";
                    ws.Column(18).Style.NumberFormat.Format = "#,##0.00";
                    ws.Column(22).Style.NumberFormat.Format = "#,##0.00";
                }
                row += 2;
            }
            //ws.Columns().AdjustToContents();

            List<int> lstWidCol = new List<int>() { 30, 35, 35, 17, 20, 18, 18, 18, 15, 10, 17, 40, 18, 15, 10, 17, 40, 18, 15, 10, 17, 18 };
            for (int i = 0; i < lstWidCol.Count; i++)
            {
                ws.Column(i + 1).Width = lstWidCol[i];
            }
            return wb;
        }

        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]
        public List<AuditTrailReportModels> GetData_NewDB(DateTime dateFrom, DateTime dateTo, List<string> lstStoreIds, List<string> lstBusinessInputIds, int mode = 1)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_PosSale
                               where lstStoreIds.Contains(tb.StoreId)
                                     && (tb.ReceiptCreatedDate >= dateFrom && tb.ReceiptCreatedDate <= dateTo)
                                     //&& tb.Mode == mode 
                                     && lstBusinessInputIds.Contains(tb.BusinessId)
                               orderby tb.OrderNo, tb.ReceiptNo
                               select new AuditTrailReportModels
                               {
                                   ReceiptDate = tb.CreatedDate, 
                                   BusinessDayId = tb.BusinessId,
                                   ReceiptID = tb.OrderId,
                                   ReceiptNo = tb.ReceiptNo,
                                   CashierId = tb.CashierId,
                                   CashierName = tb.CashierName,
                                   ReceiptTotal = tb.ReceiptTotal,
                                   StoreId = tb.StoreId,
                                   OrderNo = tb.OrderNo,
                                   PromotionAmount = tb.OrderStatus == (int)Commons.EStatus.Deleted ? 0 : tb.PromotionValue
                               })
                               .ToList();
                return lstData;

            }
        }
        public XLWorkbook Report_NewBD(BaseReportModel model, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Audit_Trail_Report");

            CreateReportHeaderNew(ws, 22, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Audit Trail Report").ToUpper());

            // Get list business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Format header report
                ws.Range(1, 1, 4, 22).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            // Get data use business day
            var _lstBusDayIdsAllStore = _lstBusDayAllStore.Select(s => s.Id).ToList();
            model.ToDate = _lstBusDayAllStore.Max(oo => oo.DateTo);

            // Get list data receipt
            List<AuditTrailReportModels> listReceipts = GetData_NewDB(model.FromDate, model.ToDate, model.ListStores, _lstBusDayIdsAllStore, model.Mode);

            // Get list data discount
            var listDiscounts = _discountFactory.GetDataDiscounts(model.FromDate, model.ToDate, model.ListStores, _lstBusDayIdsAllStore, model.Mode);

            // Get list data cancel & refund
            var listCancelRefunds = _cancelRefundFactory.GetDataItemizedCancelRefunds_NewDB(model.FromDate, model.ToDate, model.ListStores, _lstBusDayIdsAllStore);

            int row = 5;
            string storeName = string.Empty, storeId = string.Empty;

            for (int i = 0; i < lstStore.Count; i++)
            {

                StoreModels store = lstStore[i];
                storeName = store.Name;
                storeId = store.Id;

                // Set StoreName
                int startRowHeader = row;
                ws.Range(row, 1, row, 22).Merge().Value = storeName;
                ws.Range(row, 1, row++, 22).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorStore);

                // Set title column
                ws.Range(row, 1, row + 1, 1).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Range(row, 2, row + 1, 2).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
                ws.Range(row, 3, row + 1, 3).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Order No"));
                ws.Range(row, 4, row + 1, 4).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No"));
                ws.Range(row, 5, row + 1, 5).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cashier"));
                ws.Range(row, 6, row + 1, 6).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Amount"));

                ws.Range(row, 7, row, 11).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                ws.Cell(row + 1, 7).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"));
                ws.Cell(row + 1, 8).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell(row + 1, 9).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("On"));
                ws.Cell(row + 1, 10).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("At"));
                ws.Cell(row + 1, 11).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("By"));

                ws.Range(row, 12, row, 16).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cancelled"));
                ws.Cell(row + 1, 12).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Items"));
                ws.Cell(row + 1, 13).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell(row + 1, 14).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("On"));
                ws.Cell(row + 1, 15).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("At"));
                ws.Cell(row + 1, 16).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("By"));

                ws.Range(row, 17, row, 21).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refunded"));
                ws.Cell(row + 1, 17).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Items"));
                ws.Cell(row + 1, 18).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                ws.Cell(row + 1, 19).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("On"));
                ws.Cell(row + 1, 20).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("At"));
                ws.Cell(row + 1, 21).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("By"));

                ws.Range(row, 22, row + 1, 22).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Amount"));
                ws.Range(row, 1, row + 1, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 1, row + 1, 22).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                row++;
                ws.Range(row - 1, 1, row, 22).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorHeader);
                ws.Range(startRowHeader, 1, row, 22).Style.Font.Bold = true;

                ws.Range(startRowHeader, 1, row, 22).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startRowHeader, 1, row, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startRowHeader, 1, row, 22).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
                ws.Range(startRowHeader, 1, row++, 22).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");


                // Get list receipts in a store
                var listReceiptsInStore = listReceipts.Where(w => w.StoreId == storeId).OrderBy(o => o.ReceiptDate).ToList();

                if (listReceiptsInStore.Count > 0)
                {
                    decimal promotionTotal = 0;
                    decimal discountTotal = 0;
                    decimal cancelTotal = 0;
                    decimal refundTotal = 0;
                    decimal receiptTotal = 0;

                    int countDate = 0;
                    int countTime = 0;
                    int countReceipt = 0;
                    int countItmDiscount = 0;
                    int countItmCancel = 0;
                    int countItmRefund = 0;

                    decimal promotionCount = 0;
                    decimal discountCount = 0;
                    decimal cancelOrderCount = 0;
                    decimal refundCount = 0;
                    decimal receiptCount = 0;

                    // Get list receipt date
                    var listDate = listReceiptsInStore.Select(s => new { s.ReceiptDate.Year, s.ReceiptDate.Month, s.ReceiptDate.Day }).Distinct().ToList();

                    foreach (var date in listDate)
                    {
                        int startRowDate = row;
                        countDate = 0;

                        // Set value for column Date
                        ws.Cell("A" + row).SetValue(new DateTime(date.Year, date.Month, date.Day, 1, 1, 1).ToString("MM/dd/yyyy"));

                        var listReceiptsInDate = listReceiptsInStore.Where(w => w.ReceiptDate.Year == date.Year && w.ReceiptDate.Month == date.Month && w.ReceiptDate.Day == date.Day).OrderBy(o => o.ReceiptDate).ToList();

                        // Get list receipt time
                        var listTime = listReceiptsInDate.Select(s => new { s.ReceiptDate.Hour, s.ReceiptDate.Minute }).Distinct().ToList();

                        foreach (var time in listTime)
                        {
                            int startRowTime = row;
                            countTime = 0;

                            // Set value for column Time
                            ws.Cell("B" + row).SetValue(new DateTime(1, 1, 1, time.Hour, time.Minute, 0).ToString("t"));

                            var listReceiptsInTime = listReceiptsInDate.Where(w => w.ReceiptDate.Hour == time.Hour && w.ReceiptDate.Minute == time.Minute).OrderBy(o => o.OrderNo).ToList();

                            foreach (var receipt in listReceiptsInTime)
                            {
                                int startRowOrder = row;
                                countReceipt = 0;

                                // Set value for column Order No
                                ws.Cell("C" + row).SetValue(receipt.OrderNo);

                                // Set value for column Receipt No
                                ws.Cell("D" + row).SetValue(receipt.ReceiptNo);
                                if (!string.IsNullOrEmpty(receipt.ReceiptNo)) receiptCount++;

                                // Set value for column Cashier
                                ws.Cell("E" + row).SetValue(receipt.CashierName);

                                // Set value for column Promotion Amount
                                ws.Cell("F" + row).SetValue(receipt.PromotionAmount);
                                promotionTotal += (decimal)receipt.PromotionAmount;
                                if (receipt.PromotionAmount > 0) promotionCount++;

                                // Get list data discount by receipt id
                                var listDiscountInReceipt = listDiscounts.Where(w => w.ReceiptId == receipt.ReceiptID && w.StoreId == storeId)
                                    .GroupBy(g => new
                                    {
                                        g.DiscountId,
                                        g.CreatedDate,
                                        g.UserDiscount
                                    }).OrderBy(o => o.Key.CreatedDate)
                                    .Select(
                                        s => new
                                        {
                                            DiscountId = s.FirstOrDefault().DiscountId,
                                            DiscountName = s.FirstOrDefault().DiscountName,
                                            DiscountAmount = s.Sum(ss => (decimal)ss.DiscountAmount),
                                            CreatedDate = s.FirstOrDefault().CreatedDate,
                                            UserDiscount = s.FirstOrDefault().UserDiscount
                                        }
                                    )
                                    .ToList();
                                if (listDiscountInReceipt != null && listDiscountInReceipt.Any()) discountCount++;

                                int startRowDiscount = row;
                                countItmDiscount = 0;

                                bool cancelAmountTotal = false;
                                bool refundAmountTotal = false;
                                // Set value for Discount
                                foreach (var discount in listDiscountInReceipt)
                                {
                                    // Name
                                    ws.Cell("G" + row).SetValue(discount.DiscountName);
                                    // Amount
                                    ws.Cell("H" + row).SetValue(discount.DiscountAmount);
                                    discountTotal += (decimal)discount.DiscountAmount;
                                    // On
                                    ws.Cell("I" + row).SetValue(discount.CreatedDate.ToString("MM/dd/yyyy"));
                                    // At
                                    ws.Cell("J" + row).SetValue(discount.CreatedDate.ToString("t"));
                                    // By
                                    ws.Cell("K" + row).SetValue(discount.UserDiscount);

                                    if (discount != listDiscountInReceipt.Last()) row++;
                                    countItmDiscount++;
                                }

                                // Get list data cancel & refund by receipt id
                                var listCancelRefundInReceipt = listCancelRefunds.Where(w => w.OrderId == receipt.ReceiptID && w.StoreId == storeId).OrderBy(o => o.CreatedDate).ToList();

                                if (listCancelRefundInReceipt.Count > 0)
                                {
                                    cancelAmountTotal = listCancelRefundInReceipt.Any(w => !w.IsRefund.HasValue || (w.IsRefund.HasValue && !w.IsRefund.Value));

                                    if (cancelAmountTotal) cancelOrderCount++;

                                    refundAmountTotal = listCancelRefundInReceipt.Any(w => w.IsRefund.HasValue && w.IsRefund.Value);
                                    if (refundAmountTotal) refundCount++;

                                    row = startRowDiscount;
                                }

                                //int startRowCancelRefund = startRowDiscount;
                                countItmCancel = 0;
                                countItmRefund = 0;
                                // Set value for Cancel & Refund
                                foreach (var cancelRefund in listCancelRefundInReceipt)
                                {
                                    if (cancelRefund.IsRefund == true)
                                    {
                                        // Set value for Refund
                                        // Name
                                        ws.Cell("Q" + row).SetValue(cancelRefund.ItemName);
                                        // Amount
                                        ws.Cell("R" + row).SetValue(cancelRefund.Amount);
                                        refundTotal += (decimal)cancelRefund.Amount;
                                        // On
                                        ws.Cell("S" + row).SetValue(cancelRefund.CreatedDate.ToString("MM/dd/yyyy"));
                                        // At
                                        ws.Cell("T" + row).SetValue(cancelRefund.CreatedDate.ToString("t"));
                                        // By
                                        ws.Cell("U" + row).SetValue(cancelRefund.RefundUser);

                                        countItmRefund++;
                                    }
                                    else
                                    {
                                        // Set value for Cancel
                                        // Name
                                        ws.Cell("L" + row).SetValue(cancelRefund.ItemName);
                                        // Amount
                                        ws.Cell("M" + row).SetValue(cancelRefund.Amount);
                                        cancelTotal += (decimal)cancelRefund.Amount;
                                        // On
                                        ws.Cell("N" + row).SetValue(cancelRefund.CreatedDate.ToString("MM/dd/yyyy"));
                                        // At
                                        ws.Cell("O" + row).SetValue(cancelRefund.CreatedDate.ToString("t"));
                                        // By
                                        ws.Cell("P" + row).SetValue(cancelRefund.CancelUser);

                                        countItmCancel++;
                                    }

                                    if (cancelRefund != listCancelRefundInReceipt.Last()) row++;
                                }

                                // Set value column Receipt Amount
                                ws.Cell("V" + startRowDiscount).SetValue(receipt.ReceiptTotal);
                                receiptTotal += (decimal)receipt.ReceiptTotal;
                                row = startRowDiscount;

                                // Format merge cell
                                int maxCountCell = Math.Max(Math.Max(countItmDiscount, countItmCancel), countItmRefund);
                                row += (maxCountCell > 0 ? maxCountCell : 1);
                                if (maxCountCell > 1)
                                {
                                    countReceipt += maxCountCell;
                                    ws.Range(startRowOrder, 3, startRowOrder + maxCountCell - 1, 3).Merge();
                                    ws.Range(startRowOrder, 4, startRowOrder + maxCountCell - 1, 4).Merge();
                                    ws.Range(startRowOrder, 5, startRowOrder + maxCountCell - 1, 5).Merge();
                                    ws.Range(startRowOrder, 6, startRowOrder + maxCountCell - 1, 6).Merge();
                                    ws.Range(startRowOrder, 22, startRowOrder + maxCountCell - 1, 22).Merge();


                                    if (countItmDiscount < maxCountCell)
                                    {
                                        List<int> lstCol = new List<int>() { 7, 8, 9, 10, 11 };
                                        foreach (int col in lstCol)
                                        {
                                            ws.Range(startRowDiscount + countItmDiscount, col, startRowDiscount + maxCountCell - 1, col).Merge();
                                        }
                                    }
                                    if (countItmCancel < maxCountCell)
                                    {
                                        List<int> lstCol = new List<int>() { 12, 13, 14, 15, 16 };
                                        foreach (int col in lstCol)
                                        {
                                            ws.Range(startRowDiscount + countItmCancel, col, startRowDiscount + maxCountCell - 1, col).Merge();
                                        }
                                    }
                                    if (countItmRefund < maxCountCell)
                                    {
                                        List<int> lstCol = new List<int>() { 17, 18, 19, 20, 21 };
                                        foreach (int col in lstCol)
                                        {
                                            ws.Range(startRowDiscount + countItmRefund, col, startRowDiscount + maxCountCell - 1, col).Merge();
                                        }
                                    }
                                }
                                else
                                {
                                    countReceipt++;
                                }

                                countTime += countReceipt;

                            }

                            ws.Range(startRowTime, 2, startRowTime + countTime - 1, 2).Merge();
                            countDate += countTime;
                        }
                        ws.Range(startRowDate, 1, startRowDate + countDate - 1, 1).Merge();

                    }

                    // Total Row
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                    ws.Cell("F" + row).Value = promotionTotal.ToString("N");
                    ws.Cell("H" + row).Value = discountTotal.ToString("N");
                    ws.Cell("M" + row).Value = cancelTotal.ToString("N");
                    ws.Cell("R" + row).Value = refundTotal.ToString("N");
                    ws.Cell("V" + row).Value = receiptTotal.ToString("N");
                    ws.Range(row, 1, row, 22).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorHeader);
                    ws.Range(row, 1, row++, 22).Style.Font.Bold = true;

                    // Total Sales
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL SALES") + ": " + receiptTotal.ToString("N");
                    ws.Cell("B" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL TC") + ": " + receiptCount;
                    // Cancel
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.TC") + " = " + cancelOrderCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.TC") + "% = " + (cancelOrderCount / (receiptCount == 0 ? 1 : receiptCount) * 100).ToString("N");
                    ws.Cell("C" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C.AMT") + "% = " + (cancelTotal / (receiptTotal == 0 ? 1 : receiptTotal) * 100).ToString("N");
                    // Refund
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.TC") + " = " + refundCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.TC") + "% = " + (refundCount / (receiptCount == 0 ? 1 : receiptCount) * 100).ToString("N");
                    ws.Cell("C" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("R.AMT") + "% = " + (refundTotal / (receiptTotal == 0 ? 1 : receiptTotal) * 100).ToString("N");
                    // Discount
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.TC") + " = " + discountCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.TC") + "% = " + (discountCount / (receiptCount == 0 ? 1 : receiptCount) * 100).ToString("N");
                    ws.Cell("C" + row++).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("D.AMT") + "% = " + (discountTotal / (receiptTotal == 0 ? 1 : receiptTotal) * 100).ToString("N");
                    // Promotion
                    ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("P.TC") + " = " + promotionCount;
                    ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("P.TC") + "% = " + (promotionCount / (receiptCount == 0 ? 1 : receiptCount) * 100).ToString("N");
                    ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("P.AMT") + "% = " + (promotionTotal / (receiptTotal == 0 ? 1 : receiptTotal) * 100).ToString("N");

                    ws.Range(row - 5, 1, row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorHeader);
                    ws.Range(row - 5, 1, row, 3).Style.Font.Bold = true;
                    ws.Range(row - 5, 1, row, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row - 5, 1, row, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row - 5, 1, row, 3).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
                    ws.Range(row - 5, 1, row, 3).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");

                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");

                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(startRowHeader, 1, row - 5, 22).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(startRowHeader, 1, startRowHeader, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("A" + (row - 5)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
                    ws.Column(8).Style.NumberFormat.Format = "#,##0.00";
                    ws.Column(13).Style.NumberFormat.Format = "#,##0.00";
                    ws.Column(18).Style.NumberFormat.Format = "#,##0.00";
                    ws.Column(22).Style.NumberFormat.Format = "#,##0.00";
                }
                row += 2;
            }
            //ws.Columns().AdjustToContents();

            List<int> lstWidCol = new List<int>() { 30, 35, 35, 17, 20, 18, 18, 18, 15, 10, 17, 40, 18, 15, 10, 17, 40, 18, 15, 10, 17, 18 };
            for (int i = 0; i < lstWidCol.Count; i++)
            {
                ws.Column(i + 1).Width = lstWidCol[i];
            }
            return wb;
        }
        #endregion
    }
}
