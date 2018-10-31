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
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Shared.Factory
{
    public class ReceiptsbyPaymentMethodsReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private OrderTipFactory _orderTipFactory = null;
        private RefundFactory _refundFactory = null;

        public ReceiptsbyPaymentMethodsReportFactory()
        {
            _baseFactory = new BaseFactory();
            _orderTipFactory = new OrderTipFactory();
            _refundFactory = new RefundFactory();
        }

        public bool Insert(List<ReceiptsbyPaymentMethodsReportModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_ReceiptsbyPaymentMethodsReport> lstInsert = new List<R_ReceiptsbyPaymentMethodsReport>();
                        R_ReceiptsbyPaymentMethodsReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            //itemInsert = new R_ReceiptsbyPaymentMethodsReport();
                            //itemInsert.Id = Guid.NewGuid().ToString();
                            //itemInsert.StoreId = item.StoreId;
                            //itemInsert.ReceiptNo = item.ReceiptNo;
                            //itemInsert.ReceiptTotal = item.ReceiptTotal;
                            //itemInsert.PaymentId = item.PaymentId;
                            //itemInsert.PaymentName = item.PaymentName;
                            //itemInsert.CreatedDate = item.CreatedDate;
                            //itemInsert.ReceiptRefund = item.ReceiptRefund;
                            //itemInsert.Total = item.Total;
                            //itemInsert.Mode = item.Mode;

                            //lstInsert.Add(itemInsert);
                        }
                        cxt.R_ReceiptsbyPaymentMethodsReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Receipts by Payment Methods data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Receipts by Payment Methods data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_ReceiptsbyPaymentMethodsReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<ReceiptsbyPaymentMethodsReportModels> GetOrderPaymentMethod(BaseReportModel model, List<string> lstPaymentIds, DateTime dFrom, DateTime dTo)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.G_PaymentMenthod
                               join cr in cxt.R_ClosedReceiptReport on tb.OrderId equals cr.OrderId
                               where model.ListStores.Contains(tb.StoreId)
                               && lstPaymentIds.Contains(tb.PaymentId)
                                     && tb.CreatedDate >= dFrom
                                             && tb.CreatedDate <= dTo
                                             && tb.Mode == model.Mode
                               select new ReceiptsbyPaymentMethodsReportModels
                               {
                                   OrderId = tb.OrderId,
                                   ReceiptNo = cr.ReceiptNo,
                                   CreatedDate = cr.CreatedDate,
                                   ReceiptTotal = cr.Total,
                                   StoreId = tb.StoreId,
                                   PaymentId = tb.PaymentId,
                                   PaymentName = tb.PaymentName,
                                   PaymentAmount = tb.Amount
                               }).ToList();
                if (lstData != null && lstData.Any())
                {
                    lstData = lstData.OrderBy(oo => oo.CreatedDate).ToList();
                }
                return lstData;
            }
        }

        public XLWorkbook ExportExcel(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "Receipt_By_Payment_Report";// _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt_By_Payment_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            CreateReportHeaderNew(ws, 11, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt by Payment Method Report"));
            // Format header report
            ws.Range(1, 1, 4, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var _lstBusinessDays = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusinessDays == null || !_lstBusinessDays.Any())
            {
                return wb;
            }

            int row = 5;
            int startRow = row, receiptNoPayment = 0;
            double excess = 0, excessSum = 0, actualPayment = 0;
            double refundTotalPayment = 0, tipPaymentSum = 0, paymentAmountSum = 0, actualPaymentAmountSum = 0, tip = 0, refund = 0;
            double paymentAmount = 0;
            int rowBorderHeader = 0;
            List<object> PaymentTotal = new List<object>();
            string storeId = string.Empty, storeName = string.Empty;

            //if (_lstBusinessDays != null && _lstBusinessDays.Count > 0)
            //{
            //var dFrom = _lstBusinessDays.Min(ss => ss.DateFrom);
            //var dTo = _lstBusinessDays.Max(ss => ss.DateTo);

            // Get data use business day
            model.ToDate = _lstBusinessDays.Max(ss => ss.DateTo);
            // Updated 03192018, Only get parent method selected is Cash
            var lstPaymentIds = model.ListPaymentMethod.Where(ww => ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).Select(ss => ss.Id).ToList();
            var lstPaymentChild = model.ListPaymentMethod.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
            foreach (var item in lstPaymentChild)
            {
                lstPaymentIds.AddRange(item.ListChilds.Where(ww => ww.Checked).Select(ss => ss.Id));
            }

            var _lstOrderPayments = GetOrderPaymentMethod(model, lstPaymentIds, model.FromDate, model.ToDate);
            var _lstTips = _orderTipFactory.GetDataTips(model, lstPaymentIds, model.FromDate, model.ToDate);
            //Check cash payment is selected
            var cashPayment = model.ListPaymentMethod.Where(ww => ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).FirstOrDefault();
            if (cashPayment != null)
            {
                #region Refund
                var _lstRefunds = _refundFactory.GetListRefundWithoutDetails(model.ListStores, model.FromDate, model.ToDate, model.Mode);
                if (_lstRefunds != null && _lstRefunds.Count > 0)
                {
                    foreach (var item in _lstRefunds)
                    {
                        var order = _lstOrderPayments.Where(ww => ww.OrderId == item.OrderId).FirstOrDefault();
                        if (order != null)
                        {
                            //_lstOrderPayments.Remove(order);
                            //order.ReceiptRefund = item.TotalRefund;
                            //_lstOrderPayments.Add(order);
                            _lstOrderPayments.Add(new ReceiptsbyPaymentMethodsReportModels()
                            {
                                CreatedDate = order.CreatedDate,
                                ReceiptNo = order.ReceiptNo,
                                ReceiptTotal = 0,//order.ReceiptTotal,
                                ReceiptRefund = item.TotalRefund,
                                PaymentAmount = 0,
                                OrderId = item.OrderId,
                                PaymentId = cashPayment.Id,
                                PaymentName = cashPayment.Name,
                                StoreId = item.StoreId,
                                Tip = 0
                            });
                        }
                    }
                    _lstOrderPayments = _lstOrderPayments.OrderBy(oo => oo.CreatedDate).ToList();
                }
                #endregion End refund
            }

            for (int i = 0; i < lstStore.Count; i++)
            {
                PaymentTotal = new List<object>();

                rowBorderHeader = row;
                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name;

                ws.Range("A" + row + ":K" + row).Merge().SetValue(storeName);
                ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                row++;

                ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
                ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."));
                ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Mode"));
                ws.Cell("F" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tip"));
                ws.Cell("G" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                ws.Cell("H" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Amount"));
                ws.Cell("I" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Actual Payment Amount"));
                ws.Cell("J" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Verification Code"));
                ws.Cell("k" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess"));
                ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                ws.Range("A" + startRow + ":K" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");

                ws.Range(string.Format("A{0}:K{1}", rowBorderHeader, row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(string.Format("A{0}:K{1}", rowBorderHeader, row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                var lstDataByStore = _lstOrderPayments.Where(ww => ww.StoreId == lstStore[i].Id)
                            .GroupBy(gg =>
                                   new { OrderId = gg.OrderId, PaymentId = gg.PaymentId, PaymentName = gg.PaymentName }
                            ).ToList();

                //Get All payment on store
                //var lstPaymentOnStore = model.ListPaymentMethod.Where(ww => ww.StoreId == lstStore[i].Id && ww.Checked)

                //Update 03192018, remove payment parent, except Cash
                //var lstPaymentOnStore = model.ListPaymentMethod.Where(ww => ww.StoreId == lstStore[i].Id && ww.Checked).ToList();
                //if (lstPaymentOnStore == null)
                //    lstPaymentOnStore = new List<RFilterChooseExtBaseModel>();
                var lstPaymentOnStore = new List<RFilterChooseExtBaseModel>();
                lstPaymentOnStore = model.ListPaymentMethod.Where(ww => ww.StoreId == lstStore[i].Id && ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).ToList();
                var lstPaymentHaveChilds = model.ListPaymentMethod.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).ToList();

                foreach (var item in lstPaymentHaveChilds)
                {
                    foreach (var subItem in item.ListChilds)
                    {
                        if (subItem.Checked && lstStore[i].Id == subItem.StoreId)
                        {
                            lstPaymentOnStore.Add(subItem);
                        }
                    }
                }
                lstPaymentOnStore = lstPaymentOnStore.OrderBy(oo => oo.Name).ToList();
                //var lstPaymentOnStore = model.ListPaymentMethod.Where(ww => ww.StoreId == lstStore[i].Id)
                //    .OrderBy(oo => oo.Name).ToList();

                foreach (var item in lstPaymentOnStore)
                {
                    receiptNoPayment = 0;
                    tipPaymentSum = 0;
                    refundTotalPayment = 0;
                    paymentAmountSum = 0;
                    actualPaymentAmountSum = 0;
                    excessSum = 0;

                    var lstDataByPayment = _lstOrderPayments.Where(ww => ww.PaymentId == item.Id && ww.StoreId == lstStore[i].Id).ToList();
                    var lstDataByPaymentGroupOrder = lstDataByPayment.GroupBy(gg => gg.OrderId).ToList();

                    //ReceiptsbyPaymentMethodsReportModels modelShow = new ReceiptsbyPaymentMethodsReportModels();
                    //var lstDataByPaymentGroupOrderEnd = lstDataByPaymentGroupOrder.ToList();
                    foreach (var itemData in lstDataByPaymentGroupOrder)
                    {
                        row++;
                        ws.Cell("A" + row).Value = itemData.FirstOrDefault().CreatedDate;
                        ws.Cell("A" + row).Style.DateFormat.Format = "MM/dd/yyyy";

                        ws.Cell("B" + row).Value = itemData.FirstOrDefault().CreatedDate.ToString("HH:mm:ss");
                        //Receipt no
                        ws.Cell("C" + row).Value = itemData.FirstOrDefault().ReceiptNo;
                        ws.Cell("D" + row).Value = itemData.FirstOrDefault().ReceiptTotal;
                        //payment mode
                        ws.Cell("E" + row).Value = item.Name;
                        //tip
                        tip = _lstTips.Where(ww => ww.StoreId == lstStore[i].Id
                         && ww.OrderId == itemData.Key && ww.PaymentId == item.Id).Sum(ss => ss.Amount);
                        tipPaymentSum += tip;
                        ws.Cell("F" + row).Value = tip;
                        //refund
                        refund = itemData.Sum(ss => ss.ReceiptRefund);
                        ws.Cell("G" + row).Value = refund;
                        refundTotalPayment += refund;
                        //payment amount
                        paymentAmount = itemData.Sum(ss => ss.PaymentAmount);
                        //if (item.Name == "cash")
                        //    paymentAmount = paymentAmount - refund;
                        ws.Cell("H" + row).Value = paymentAmount;
                        paymentAmountSum += paymentAmount;
                        //Actual payment amount
                        actualPayment = (itemData.Sum(ss => ss.PaymentAmount) - refund);
                        actualPaymentAmountSum += actualPayment;
                        ws.Cell("I" + row).Value = actualPayment;
                        //Verification code
                        ws.Cell("J" + row).Value = "";
                        //Excess
                        //excess = actualPayment - itemData.FirstOrDefault().ReceiptTotal - tip;
                        excess = actualPayment - itemData.FirstOrDefault().ReceiptTotal;
                        excessSum += excess;
                        ws.Cell("K" + row).Value = excess;

                        receiptNoPayment++;
                    }// end data payment loop

                    //Group by Payment
                    row++;
                    ws.Cell("A" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC") + " = " + receiptNoPayment.ToString("#,##0");
                    ws.Cell("D" + row).Value = 0;
                    ws.Cell("F" + row).Value = tipPaymentSum;
                    ws.Cell("G" + row).Value = refundTotalPayment;
                    ws.Cell("H" + row).Value = paymentAmountSum;
                    ws.Cell("I" + row).Value = actualPaymentAmountSum;
                    ws.Cell("K" + row).Value = excessSum;

                    //ws.Range("A" + startRow + ":A" + (row - 1)).Style.DateFormat.Format = "dd/MM/yyyy";
                    ws.Range("B" + startRow + ":B" + (row - 1)).Style.DateFormat.Format = "HH:mm:ss";
                    ws.Range("D" + startRow + ":D" + row).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("F" + startRow + ":K" + row).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                    ws.Range("A" + row + ":K" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
                    //=========
                    //int NoOfReceipt = reportItems.Count;
                    if (PaymentTotal.Contains(item.Name))
                    {
                        int index = PaymentTotal.FindIndex(k => k.Equals(item.Name.ToString()));
                        PaymentTotal[index + 2] = int.Parse(PaymentTotal[index + 2].ToString()) + receiptNoPayment;
                        PaymentTotal[index + 3] = double.Parse(PaymentTotal[index + 3].ToString()) + actualPaymentAmountSum;
                        PaymentTotal[index + 4] = double.Parse(PaymentTotal[index + 4].ToString()) + excessSum;
                    }
                    else
                    {
                        PaymentTotal.Add(item.Name);        //PaymentName
                        PaymentTotal.Add(item.Id);          //Index
                        PaymentTotal.Add(receiptNoPayment);                    //NoOfReceipt
                        PaymentTotal.Add(actualPaymentAmountSum);//PayTotal
                        PaymentTotal.Add(excessSum);                  //ExcessTotal
                    }
                    row++;

                    ws.Range(string.Format("A{0}:K{1}", startRow, row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(string.Format("A{0}:K{1}", startRow, row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row += 1;
                startRow = row;


                //=====================================================
                //Summary
                row += 1;
                startRow = row;
                double totalPayAmount = 0, excessTotal = 0;
                int totalReceiptNo = 0;

                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Methods");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Actual Payment Amount");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess");
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("A" + row + ":E" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");

                for (int j = 0; j < PaymentTotal.Count(); j += 5)
                {
                    row++;
                    ws.Cell("A" + row).Value = PaymentTotal[j];
                    ws.Cell("C" + row).Value = PaymentTotal[j + 2];
                    ws.Cell("D" + row).Value = (double)PaymentTotal[j + 3];
                    ws.Cell("E" + row).Value = (double)PaymentTotal[j + 4];

                    totalReceiptNo += (int)PaymentTotal[j + 2];
                    totalPayAmount += (double)PaymentTotal[j + 3];
                    excessTotal += (double)PaymentTotal[j + 4];

                    //ws.Cell("A" + row).Style.Font.SetBold(true);
                }

                row++;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                ws.Cell("C" + row).Value = totalReceiptNo;
                ws.Cell("D" + row).Value = (double)totalPayAmount;
                ws.Cell("E" + row).Value = (double)excessTotal;
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("D" + (startRow + 1) + ":E" + row).Style.NumberFormat.Format = "#,##0.00";

                ws.Range("A" + startRow + ":E" + row).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range("A" + startRow + ":E" + row).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                row += 2;
                startRow = row;
            }//End loop by store

            ws.Columns().AdjustToContents();
            //}//End check bussiness day
            //else//no data
            //{
            //    ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
            //    ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
            //    ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."));
            //    ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
            //    ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Mode"));
            //    ws.Cell("F" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tip"));
            //    ws.Cell("G" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
            //    ws.Cell("H" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Amount"));
            //    ws.Cell("I" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Actual Payment Amount"));
            //    ws.Cell("J" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Verification Code"));
            //    ws.Cell("k" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess"));
            //    ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
            //    ws.Range("A" + startRow + ":K" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
            //}
            return wb;
        }

        public List<RFilterChooseExtBaseModel> GetAllPaymentForReport(CategoryApiRequestModel request)
        {
            var lstData = new List<RFilterChooseExtBaseModel>();
            var lstResult = new List<RFilterChooseExtBaseModel>();
            try
            {
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetPaymentMethodForWeb, null, request);
                if (result.Success)
                {
                    dynamic data = result.Data;
                    var ListCate = data["ListCategories"];

                    var lstContent = JsonConvert.SerializeObject(ListCate);
                    lstData = JsonConvert.DeserializeObject<List<RFilterChooseExtBaseModel>>(lstContent);

                    var lstParentOrNotChild = lstData.Where(ww => string.IsNullOrEmpty(ww.ParentId)).OrderBy(ww => ww.Name).ToList();
                    foreach (var item in lstParentOrNotChild)
                    {
                        if (item.Code == (int)Commons.EPaymentCode.Cash)
                            lstResult.Add(item);
                        else
                        {
                            item.ListChilds = lstData.Where(ww => !string.IsNullOrEmpty(ww.ParentId)
                            && ww.ParentId == item.Id && ww.StoreId.Equals(item.StoreId))
                            .OrderBy(ww => ww.Name).ToList();
                            if (item.ListChilds != null && item.ListChilds.Any())
                                lstResult.Add(item);
                        }

                    }
                }
                lstResult = lstResult.OrderBy(oo => oo.StoreName).ToList();
                return lstResult;
            }
            catch (Exception e)
            {
                _logger.Error("GetCate payment Report: " + e);
                return lstResult;
            }
        }

        public List<RFilterChooseExtBaseModel> GetAllPaymentForReport(List<MerchantExtendModel> lstMerchantExtend)
        {
            var lstData = new List<RFilterChooseExtBaseModel>();
            var lstResult = new List<RFilterChooseExtBaseModel>();
            foreach (var merchant in lstMerchantExtend)
            {
                try
                {
                    var request = new CategoryApiRequestModel() { ListStoreIds = merchant.ListStoreIds };
                    var result = (ResponseApiModels)ApiResponse.PostWithoutHostConfig<ResponseApiModels>(merchant.HostApiURL + "/" + Commons.GetPaymentMethodForWeb
                        , null, request);
                    if (result.Success)
                    {
                        dynamic data = result.Data;
                        var ListCate = data["ListCategories"];

                        var lstContent = JsonConvert.SerializeObject(ListCate);
                        lstData = JsonConvert.DeserializeObject<List<RFilterChooseExtBaseModel>>(lstContent);

                        var lstParentOrNotChild = lstData.Where(ww => string.IsNullOrEmpty(ww.ParentId)).OrderBy(ww => ww.Name).ToList();
                        foreach (var item in lstParentOrNotChild)
                        {
                            if (item.Code == (int)Commons.EPaymentCode.Cash)
                                lstResult.Add(item);
                            else
                            {
                                item.ListChilds = lstData.Where(ww => !string.IsNullOrEmpty(ww.ParentId)
                                && ww.ParentId == item.Id && ww.StoreId.Equals(item.StoreId))
                                .OrderBy(ww => ww.Name).ToList();
                                if (item.ListChilds != null && item.ListChilds.Any())
                                    lstResult.Add(item);
                            }

                        }
                    }
                    lstResult = lstResult.OrderBy(oo => oo.StoreName).ToList();

                }
                catch (Exception e)
                {
                    _logger.Error("GetCate payment Report: " + e);
                }
            }//end foreach merchant
            return lstResult;
        }

        #region Report with Credit Note info (refund gift card), updated 04102018
        public List<ReceiptsbyPaymentMethodsReportModels> GetOrderPaymentMethod_WithCreditNote(List<string> lstStoreId, DateTime dFrom, DateTime dTo, string paymentId, string paymentName, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_ClosedReceiptReport
                               where lstStoreId.Contains(tb.StoreId)
                                    && tb.CreatedDate >= dFrom
                                    && tb.CreatedDate <= dTo
                                    && !string.IsNullOrEmpty(tb.CreditNoteNo)
                                    && tb.Mode == mode
                               select new ReceiptsbyPaymentMethodsReportModels
                               {
                                   OrderId = tb.OrderId,
                                   ReceiptNo = tb.CreditNoteNo,
                                   CreatedDate = tb.CreatedDate,
                                   ReceiptTotal = 0,
                                   ReceiptRefund = tb.Total,
                                   Tip = 0,
                                   StoreId = tb.StoreId,
                                   PaymentId = paymentId,
                                   PaymentName = paymentName,
                                   PaymentAmount = 0,
                                   CreditNoteNo = tb.CreditNoteNo,
                                   IsGiftCard = false // for refund by cash
                               }).ToList();
                if (lstData != null && lstData.Any())
                {
                    lstData = lstData.OrderBy(oo => oo.CreatedDate).ToList();
                }
                return lstData;
            }
        }

        public XLWorkbook ExportExcel_WithCreditNote(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "Receipt_By_Payment_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            CreateReportHeaderNew(ws, 11, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt by Payment Method Report"));

            // Format header report
            ws.Range(1, 1, 4, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var _lstBusinessDays = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusinessDays == null || !_lstBusinessDays.Any())
            {
                return wb;
            }

            // Get data use business day
            model.FromDate = _lstBusinessDays.Min(ss => ss.DateFrom);
            model.ToDate = _lstBusinessDays.Max(ss => ss.DateTo);

            // Updated 03192018, Only get parent method selected is Cash
            var lstPaymentIds = model.ListPaymentMethod.Where(ww => ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).Select(ss => ss.Id).ToList();
            var lstPaymentChild = model.ListPaymentMethod.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
            foreach (var item in lstPaymentChild)
            {
                lstPaymentIds.AddRange(item.ListChilds.Where(ww => ww.Checked).Select(ss => ss.Id));
            }

            var _lstOrderPayments = GetOrderPaymentMethod(model, lstPaymentIds, model.FromDate, model.ToDate);

            //Check cash payment is selected
            var cashPayment = model.ListPaymentMethod.Where(ww => ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).FirstOrDefault();

            if (cashPayment != null)
            {
                #region Refund
                if (_lstOrderPayments != null && _lstOrderPayments.Any())
                {
                    var lstReceiptId = _lstOrderPayments.Select(s => s.OrderId).Distinct().ToList(); // Receipt
                    var _lstRefunds = _refundFactory.GetListRefundWithoutDetailsByReceiptId(model.ListStores, lstReceiptId);
                    if (_lstRefunds != null && _lstRefunds.Any())
                    {
                        foreach (var item in _lstRefunds)
                        {
                            var order = _lstOrderPayments.Where(ww => ww.OrderId == item.OrderId).FirstOrDefault();
                            if (order != null)
                            {
                                _lstOrderPayments.Add(new ReceiptsbyPaymentMethodsReportModels()
                                {
                                    CreatedDate = order.CreatedDate,
                                    ReceiptNo = order.ReceiptNo,
                                    ReceiptTotal = 0,
                                    ReceiptRefund = item.TotalRefund,
                                    PaymentAmount = 0,
                                    OrderId = item.OrderId,
                                    PaymentId = cashPayment.Id,
                                    PaymentName = cashPayment.Name,
                                    StoreId = item.StoreId,
                                    Tip = 0,
                                    IsGiftCard = item.IsGiftCard
                                });
                            }
                        }
                    }
                }
                #endregion End refund

                #region Credit Note, show credit notes in area "Cash", payment mode = Cash
                var _lstCreditNote = GetOrderPaymentMethod_WithCreditNote(model.ListStores, model.FromDate, model.ToDate, cashPayment.Id, cashPayment.Name, model.Mode);
                if (_lstCreditNote != null && _lstCreditNote.Any())
                {
                    _lstOrderPayments.AddRange(_lstCreditNote);
                }
                #endregion Credit Note
            }

            if (_lstOrderPayments == null || !_lstOrderPayments.Any())
            {
                return wb;
            }
            _lstOrderPayments = _lstOrderPayments.OrderBy(oo => oo.CreatedDate).ToList();

            var _lstTips = _orderTipFactory.GetDataTips(model, lstPaymentIds, model.FromDate, model.ToDate);

            int row = 5;
            int startRow = row, receiptNoPayment = 0;
            double excess = 0, excessSum = 0, actualPayment = 0;
            double refundTotalPayment = 0, tipPaymentSum = 0, paymentAmountSum = 0, actualPaymentAmountSum = 0, tip = 0, refund = 0, refundByCash = 0, refundGC = 0;
            double paymentAmount = 0;
            int rowBorderHeader = 0;
            List<object> PaymentTotal = new List<object>();
            string storeId = string.Empty, storeName = string.Empty;
            double receiptTotalSum = 0, receiptTotal = 0;

            for (int i = 0; i < lstStore.Count; i++)
            {
                PaymentTotal = new List<object>();

                rowBorderHeader = row;
                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name;

                ws.Range("A" + row + ":K" + row).Merge().SetValue(storeName);
                ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                row++;

                ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
                ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."));
                ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Mode"));
                ws.Cell("F" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tip"));
                ws.Cell("G" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                ws.Cell("H" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Amount"));
                ws.Cell("I" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Actual Payment Amount"));
                ws.Cell("J" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Verification Code"));
                ws.Cell("k" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess"));
                ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                ws.Range("A" + startRow + ":K" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");

                ws.Range(string.Format("A{0}:K{1}", rowBorderHeader, row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(string.Format("A{0}:K{1}", rowBorderHeader, row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Get All payment on store
                // Update 03192018, remove payment parent, except Cash
                var lstPaymentOnStore = new List<RFilterChooseExtBaseModel>();
                lstPaymentOnStore = model.ListPaymentMethod.Where(ww => ww.StoreId == lstStore[i].Id && ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).ToList();
                var lstPaymentHaveChilds = model.ListPaymentMethod.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).ToList();

                foreach (var item in lstPaymentHaveChilds)
                {
                    foreach (var subItem in item.ListChilds)
                    {
                        if (subItem.Checked && lstStore[i].Id == subItem.StoreId)
                        {
                            lstPaymentOnStore.Add(subItem);
                        }
                    }
                }
                lstPaymentOnStore = lstPaymentOnStore.OrderBy(oo => oo.Name).ToList();

                foreach (var item in lstPaymentOnStore)
                {
                    receiptNoPayment = 0;
                    tipPaymentSum = 0;
                    refundTotalPayment = 0;
                    paymentAmountSum = 0;
                    actualPaymentAmountSum = 0;
                    excessSum = 0;
                    receiptTotalSum = 0;

                    var lstDataByPayment = _lstOrderPayments.Where(ww => ww.PaymentId == item.Id && ww.StoreId == lstStore[i].Id).ToList();
                    var lstDataByPaymentGroupOrder = lstDataByPayment.GroupBy(gg => gg.OrderId).ToList();

                    foreach (var itemData in lstDataByPaymentGroupOrder)
                    {
                        row++;
                        ws.Cell("A" + row).Value = itemData.FirstOrDefault().CreatedDate;
                        ws.Cell("A" + row).Style.DateFormat.Format = "MM/dd/yyyy";

                        ws.Cell("B" + row).Value = itemData.FirstOrDefault().CreatedDate.ToString("HH:mm:ss");

                        //Receipt no
                        ws.Cell("C" + row).Value = itemData.FirstOrDefault().ReceiptNo;

                        //ReceiptTotal
                        receiptTotal = itemData.Sum(s => s.ReceiptTotal);
                        ws.Cell("D" + row).Value = receiptTotal;
                        receiptTotalSum += receiptTotal;

                        //payment mode
                        ws.Cell("E" + row).Value = item.Name;

                        //tip
                        tip = _lstTips.Where(ww => ww.StoreId == lstStore[i].Id
                         && ww.OrderId == itemData.Key && ww.PaymentId == item.Id).Sum(ss => ss.Amount);
                        tipPaymentSum += tip;
                        ws.Cell("F" + row).Value = tip;

                        //refund
                        //refund = itemData.Sum(ss => ss.ReceiptRefund);
                        refundByCash = itemData.Where(ww => !ww.IsGiftCard).Sum(ss => ss.ReceiptRefund);
                        refundGC = itemData.Where(ww => ww.IsGiftCard).Sum(ss => ss.ReceiptRefund);
                        refund = refundByCash + refundGC;

                        ws.Cell("G" + row).Value = refund;
                        refundTotalPayment += refund;

                        //payment amount
                        paymentAmount = itemData.Sum(ss => ss.PaymentAmount);
                        ws.Cell("H" + row).Value = paymentAmount;
                        paymentAmountSum += paymentAmount;

                        //Actual payment amount
                        //actualPayment = (itemData.Sum(ss => ss.PaymentAmount) - refund);
                        actualPayment = (itemData.Sum(ss => ss.PaymentAmount) - refundByCash);
                        actualPaymentAmountSum += actualPayment;
                        ws.Cell("I" + row).Value = actualPayment;

                        //Verification code
                        ws.Cell("J" + row).Value = "";

                        //Excess
                        //excess = actualPayment - receiptTotal - tip;
                        excess = 0;
                        string creditNoteNo = itemData.Select(ss => ss.CreditNoteNo).FirstOrDefault();
                        if (string.IsNullOrEmpty(creditNoteNo)) // For receipt only
                        {
                            excess = actualPayment - receiptTotal;
                            receiptNoPayment++; 
                        }
                        excessSum += excess;
                        ws.Cell("K" + row).Value = excess;

                    }// end data payment loop

                    //Group by Payment
                    row++;
                    ws.Cell("A" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC") + " = " + receiptNoPayment.ToString("#,##0");
                    ws.Cell("D" + row).Value = receiptTotalSum;
                    ws.Cell("F" + row).Value = tipPaymentSum;
                    ws.Cell("G" + row).Value = refundTotalPayment;
                    ws.Cell("H" + row).Value = paymentAmountSum;
                    ws.Cell("I" + row).Value = actualPaymentAmountSum;
                    ws.Cell("K" + row).Value = excessSum;

                    ws.Range("B" + startRow + ":B" + (row - 1)).Style.DateFormat.Format = "HH:mm:ss";
                    ws.Range("D" + startRow + ":D" + row).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("F" + startRow + ":K" + row).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                    ws.Range("A" + row + ":K" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
                    //=========
                    if (PaymentTotal.Contains(item.Name))
                    {
                        int index = PaymentTotal.FindIndex(k => k.Equals(item.Name.ToString()));
                        PaymentTotal[index + 2] = int.Parse(PaymentTotal[index + 2].ToString()) + receiptNoPayment;
                        PaymentTotal[index + 3] = double.Parse(PaymentTotal[index + 3].ToString()) + actualPaymentAmountSum;
                        PaymentTotal[index + 4] = double.Parse(PaymentTotal[index + 4].ToString()) + excessSum;
                    }
                    else
                    {
                        PaymentTotal.Add(item.Name);                //PaymentName
                        PaymentTotal.Add(item.Id);                  //Index
                        PaymentTotal.Add(receiptNoPayment);         //NoOfReceipt
                        PaymentTotal.Add(actualPaymentAmountSum);   //PayTotal
                        PaymentTotal.Add(excessSum);                //ExcessTotal
                    }
                    row++;

                    ws.Range(string.Format("A{0}:K{1}", startRow, row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(string.Format("A{0}:K{1}", startRow, row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row += 1;
                startRow = row;


                //=====================================================
                //Summary
                row += 1;
                startRow = row;
                double totalPayAmount = 0, excessTotal = 0;
                int totalReceiptNo = 0;

                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Methods");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Actual Payment Amount");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess");
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("A" + row + ":E" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");

                for (int j = 0; j < PaymentTotal.Count(); j += 5)
                {
                    row++;
                    ws.Cell("A" + row).Value = PaymentTotal[j];
                    ws.Cell("C" + row).Value = PaymentTotal[j + 2];
                    ws.Cell("D" + row).Value = (double)PaymentTotal[j + 3];
                    ws.Cell("E" + row).Value = (double)PaymentTotal[j + 4];

                    totalReceiptNo += (int)PaymentTotal[j + 2];
                    totalPayAmount += (double)PaymentTotal[j + 3];
                    excessTotal += (double)PaymentTotal[j + 4];
                }

                row++;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                ws.Cell("C" + row).Value = totalReceiptNo;
                ws.Cell("D" + row).Value = (double)totalPayAmount;
                ws.Cell("E" + row).Value = (double)excessTotal;
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("D" + (startRow + 1) + ":E" + row).Style.NumberFormat.Format = "#,##0.00";

                ws.Range("A" + startRow + ":E" + row).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range("A" + startRow + ":E" + row).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                row += 2;
                startRow = row;
            }//End loop by store

            ws.Columns().AdjustToContents();

            return wb;
        }
        #endregion Report with Credit Note info (refund gift card), updated 04102018

        #region Report with new DB, from table [R_PosSale], [R_PosSaleDetail]
        public List<ReceiptsbyPaymentMethodsReportModels> GetData_NewDB(List<string> lstStoreId, List<string> lstPaymentIds, DateTime dFrom, DateTime dTo, string paymentId, string paymentName, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                // Credit Note, show credit notes in area "Cash", payment mode = Cash
                var lstData = (from ps in cxt.R_PosSale
                           join pm in cxt.G_PaymentMenthod on ps.OrderId equals pm.OrderId into psd
                           from pm in psd.DefaultIfEmpty()
                           where lstStoreId.Contains(ps.StoreId) 
                                     && (pm == null || lstPaymentIds.Contains(pm.PaymentId))
                                     && ps.ReceiptCreatedDate >= dFrom && ps.ReceiptCreatedDate <= dTo && ps.Mode == mode
                           orderby ps.ReceiptCreatedDate
                           select new ReceiptsbyPaymentMethodsReportModels
                           {
                               OrderId = ps.OrderId,
                               CreatedDate = ps.ReceiptCreatedDate.Value,
                               CreditNoteNo = ps.CreditNoteNo,
                               ReceiptNo = string.IsNullOrEmpty(ps.CreditNoteNo) ? ps.ReceiptNo : ps.CreditNoteNo,
                               ReceiptTotal = string.IsNullOrEmpty(ps.CreditNoteNo) ? ps.ReceiptTotal : 0,
                               ReceiptRefund = string.IsNullOrEmpty(ps.CreditNoteNo) ? 0 : ps.ReceiptTotal,
                               Tip = 0,
                               StoreId = ps.StoreId,
                               PaymentId = pm != null ? pm.PaymentId : paymentId,
                               PaymentName = pm != null ? pm.PaymentName : paymentName,
                               PaymentAmount = pm != null ? pm.Amount : 0,
                               IsGiftCard = false // for Credit Note => refund by cash
                           }).ToList();
                
                return lstData;
            }
        }
        public XLWorkbook ExportExcel_NewDB(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "Receipt_By_Payment_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            CreateReportHeaderNew(ws, 11, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt by Payment Method Report"));

            // Format header report
            ws.Range(1, 1, 4, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var _lstBusinessDays = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusinessDays == null || !_lstBusinessDays.Any())
            {
                return wb;
            }

            // Get data use business day
            model.FromDate = _lstBusinessDays.Min(ss => ss.DateFrom);
            model.ToDate = _lstBusinessDays.Max(ss => ss.DateTo);

            // Updated 03192018, Only get parent method selected is Cash
            var lstPaymentIds = model.ListPaymentMethod.Where(ww => ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).Select(ss => ss.Id).ToList();
            var lstPaymentChild = model.ListPaymentMethod.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
            foreach (var item in lstPaymentChild)
            {
                lstPaymentIds.AddRange(item.ListChilds.Where(ww => ww.Checked).Select(ss => ss.Id));
            }

            // Check cash payment is selected
            var cashPayment = model.ListPaymentMethod.Where(ww => ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).FirstOrDefault();
            if (cashPayment == null)
            {
                cashPayment = new RFilterChooseExtBaseModel();
            }

            var _lstOrderPayments = GetData_NewDB(model.ListStores, lstPaymentIds, model.FromDate, model.ToDate, cashPayment.Id, cashPayment.Name, model.Mode);
            if (_lstOrderPayments == null || !_lstOrderPayments.Any())
            {
                return wb;
            }

            if (cashPayment != null && !string.IsNullOrEmpty(cashPayment.Id))
            {
                #region Refund, only Receipt
                var lstReceiptId = _lstOrderPayments.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo)).Select(s => s.OrderId).Distinct().ToList();
                if (lstReceiptId != null && lstReceiptId.Any())
                {
                    var _lstRefunds = _refundFactory.GetListRefundWithoutDetailsByReceiptId(model.ListStores, lstReceiptId);
                    if (_lstRefunds != null && _lstRefunds.Any())
                    {
                        foreach (var item in _lstRefunds)
                        {
                            var order = _lstOrderPayments.Where(ww => ww.OrderId == item.OrderId).FirstOrDefault();
                            if (order != null)
                            {
                                _lstOrderPayments.Add(new ReceiptsbyPaymentMethodsReportModels()
                                {
                                    CreatedDate = order.CreatedDate,
                                    ReceiptNo = order.ReceiptNo,
                                    ReceiptTotal = 0,
                                    ReceiptRefund = item.TotalRefund,
                                    PaymentAmount = 0,
                                    OrderId = item.OrderId,
                                    PaymentId = cashPayment.Id,
                                    PaymentName = cashPayment.Name,
                                    StoreId = item.StoreId,
                                    Tip = 0,
                                    IsGiftCard = item.IsGiftCard
                                });
                            }
                        }
                        _lstOrderPayments = _lstOrderPayments.OrderBy(oo => oo.CreatedDate).ToList();
                    }
                }
                #endregion End refund, only Receipt
            }

            var _lstTips = _orderTipFactory.GetDataTips(model, lstPaymentIds, model.FromDate, model.ToDate);

            int row = 5;
            int startRow = row, receiptNoPayment = 0;
            double excess = 0, excessSum = 0, actualPayment = 0;
            double refundTotalPayment = 0, tipPaymentSum = 0, paymentAmountSum = 0, actualPaymentAmountSum = 0, tip = 0, refund = 0, refundByCash = 0, refundGC = 0;
            double paymentAmount = 0;
            int rowBorderHeader = 0;
            List<object> PaymentTotal = new List<object>();
            string storeId = string.Empty, storeName = string.Empty;
            double receiptTotalSum = 0, receiptTotal = 0;

            for (int i = 0; i < lstStore.Count; i++)
            {
                PaymentTotal = new List<object>();

                rowBorderHeader = row;
                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name;

                ws.Range("A" + row + ":K" + row).Merge().SetValue(storeName);
                ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                row++;

                ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"));
                ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
                ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."));
                ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt Total"));
                ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Mode"));
                ws.Cell("F" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tip"));
                ws.Cell("G" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Refund"));
                ws.Cell("H" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Amount"));
                ws.Cell("I" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Actual Payment Amount"));
                ws.Cell("J" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Verification Code"));
                ws.Cell("k" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess"));
                ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                ws.Range("A" + startRow + ":K" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");

                ws.Range(string.Format("A{0}:K{1}", rowBorderHeader, row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(string.Format("A{0}:K{1}", rowBorderHeader, row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Get All payment on store
                // Update 03192018, remove payment parent, except Cash
                var lstPaymentOnStore = new List<RFilterChooseExtBaseModel>();
                lstPaymentOnStore = model.ListPaymentMethod.Where(ww => ww.StoreId == lstStore[i].Id && ww.Checked && ww.Code == (byte)Commons.EPaymentCode.Cash).ToList();
                var lstPaymentHaveChilds = model.ListPaymentMethod.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).ToList();

                foreach (var item in lstPaymentHaveChilds)
                {
                    foreach (var subItem in item.ListChilds)
                    {
                        if (subItem.Checked && lstStore[i].Id == subItem.StoreId)
                        {
                            lstPaymentOnStore.Add(subItem);
                        }
                    }
                }
                lstPaymentOnStore = lstPaymentOnStore.OrderBy(oo => oo.Name).ToList();

                foreach (var item in lstPaymentOnStore)
                {
                    receiptNoPayment = 0;
                    tipPaymentSum = 0;
                    refundTotalPayment = 0;
                    paymentAmountSum = 0;
                    actualPaymentAmountSum = 0;
                    excessSum = 0;
                    receiptTotalSum = 0;

                    var lstDataByPayment = _lstOrderPayments.Where(ww => ww.PaymentId == item.Id && ww.StoreId == lstStore[i].Id).ToList();
                    var lstDataByPaymentGroupOrder = lstDataByPayment.GroupBy(gg => gg.OrderId).ToList();

                    foreach (var itemData in lstDataByPaymentGroupOrder)
                    {
                        row++;
                        ws.Cell("A" + row).Value = itemData.FirstOrDefault().CreatedDate;
                        ws.Cell("A" + row).Style.DateFormat.Format = "MM/dd/yyyy";

                        ws.Cell("B" + row).Value = itemData.FirstOrDefault().CreatedDate.ToString("HH:mm:ss");

                        //Receipt no
                        ws.Cell("C" + row).Value = itemData.FirstOrDefault().ReceiptNo;

                        //ReceiptTotal
                        receiptTotal = itemData.Sum(s => s.ReceiptTotal);
                        ws.Cell("D" + row).Value = receiptTotal;
                        receiptTotalSum += receiptTotal;

                        //payment mode
                        ws.Cell("E" + row).Value = item.Name;

                        //tip
                        tip = _lstTips.Where(ww => ww.StoreId == lstStore[i].Id
                         && ww.OrderId == itemData.Key && ww.PaymentId == item.Id).Sum(ss => ss.Amount);
                        tipPaymentSum += tip;
                        ws.Cell("F" + row).Value = tip;

                        //refund
                        //refund = itemData.Sum(ss => ss.ReceiptRefund);
                        refundByCash = itemData.Where(ww => !ww.IsGiftCard).Sum(ss => ss.ReceiptRefund);
                        refundGC = itemData.Where(ww => ww.IsGiftCard).Sum(ss => ss.ReceiptRefund);
                        refund = refundByCash + refundGC;

                        ws.Cell("G" + row).Value = refund;
                        refundTotalPayment += refund;

                        //payment amount
                        paymentAmount = itemData.Sum(ss => ss.PaymentAmount);
                        ws.Cell("H" + row).Value = paymentAmount;
                        paymentAmountSum += paymentAmount;

                        //Actual payment amount
                        //actualPayment = (itemData.Sum(ss => ss.PaymentAmount) - refund);
                        actualPayment = (itemData.Sum(ss => ss.PaymentAmount) - refundByCash);
                        actualPaymentAmountSum += actualPayment;
                        ws.Cell("I" + row).Value = actualPayment;

                        //Verification code
                        ws.Cell("J" + row).Value = "";

                        //Excess
                        //excess = actualPayment - receiptTotal - tip;
                        excess = 0;
                        string creditNoteNo = itemData.Select(ss => ss.CreditNoteNo).FirstOrDefault();
                        if (string.IsNullOrEmpty(creditNoteNo)) // For receipt only
                        {
                            excess = actualPayment - receiptTotal;
                            receiptNoPayment++;
                        }
                        excessSum += excess;
                        ws.Cell("K" + row).Value = excess;

                    }// end data payment loop

                    //Group by Payment
                    row++;
                    ws.Cell("A" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC") + " = " + receiptNoPayment.ToString("#,##0");
                    ws.Cell("D" + row).Value = receiptTotalSum;
                    ws.Cell("F" + row).Value = tipPaymentSum;
                    ws.Cell("G" + row).Value = refundTotalPayment;
                    ws.Cell("H" + row).Value = paymentAmountSum;
                    ws.Cell("I" + row).Value = actualPaymentAmountSum;
                    ws.Cell("K" + row).Value = excessSum;

                    ws.Range("B" + startRow + ":B" + (row - 1)).Style.DateFormat.Format = "HH:mm:ss";
                    ws.Range("D" + startRow + ":D" + row).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("F" + startRow + ":K" + row).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + row + ":K" + row).Style.Font.SetBold(true);
                    ws.Range("A" + row + ":K" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
                    //=========
                    if (PaymentTotal.Contains(item.Name))
                    {
                        int index = PaymentTotal.FindIndex(k => k.Equals(item.Name.ToString()));
                        PaymentTotal[index + 2] = int.Parse(PaymentTotal[index + 2].ToString()) + receiptNoPayment;
                        PaymentTotal[index + 3] = double.Parse(PaymentTotal[index + 3].ToString()) + actualPaymentAmountSum;
                        PaymentTotal[index + 4] = double.Parse(PaymentTotal[index + 4].ToString()) + excessSum;
                    }
                    else
                    {
                        PaymentTotal.Add(item.Name);                //PaymentName
                        PaymentTotal.Add(item.Id);                  //Index
                        PaymentTotal.Add(receiptNoPayment);         //NoOfReceipt
                        PaymentTotal.Add(actualPaymentAmountSum);   //PayTotal
                        PaymentTotal.Add(excessSum);                //ExcessTotal
                    }
                    row++;

                    ws.Range(string.Format("A{0}:K{1}", startRow, row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(string.Format("A{0}:K{1}", startRow, row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row += 1;
                startRow = row;


                //=====================================================
                //Summary
                row += 1;
                startRow = row;
                double totalPayAmount = 0, excessTotal = 0;
                int totalReceiptNo = 0;

                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Methods");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TC");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Actual Payment Amount");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excess");
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("A" + row + ":E" + row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");

                for (int j = 0; j < PaymentTotal.Count(); j += 5)
                {
                    row++;
                    ws.Cell("A" + row).Value = PaymentTotal[j];
                    ws.Cell("C" + row).Value = PaymentTotal[j + 2];
                    ws.Cell("D" + row).Value = (double)PaymentTotal[j + 3];
                    ws.Cell("E" + row).Value = (double)PaymentTotal[j + 4];

                    totalReceiptNo += (int)PaymentTotal[j + 2];
                    totalPayAmount += (double)PaymentTotal[j + 3];
                    excessTotal += (double)PaymentTotal[j + 4];
                }

                row++;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL");
                ws.Cell("C" + row).Value = totalReceiptNo;
                ws.Cell("D" + row).Value = (double)totalPayAmount;
                ws.Cell("E" + row).Value = (double)excessTotal;
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("D" + (startRow + 1) + ":E" + row).Style.NumberFormat.Format = "#,##0.00";

                ws.Range("A" + startRow + ":E" + row).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range("A" + startRow + ":E" + row).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                row += 2;
                startRow = row;
            }//End loop by store

            ws.Columns().AdjustToContents();

            return wb;
        }
        #endregion Report with new DB, from table [R_PosSale], [R_PosSaleDetail]
    }
}
