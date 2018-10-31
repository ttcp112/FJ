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
using NuWebNCloud.Data.Models;

namespace NuWebNCloud.Shared.Factory
{
    public class ClosedReceiptReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private RefundFactory _refundFactory = null;
        public ClosedReceiptReportFactory()
        {
            _baseFactory = new BaseFactory();
            _refundFactory = new RefundFactory();
        }

        public bool Insert(List<ClosedReceiptReportModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_ClosedReceiptReport.Where(ww => ww.StoreId == info.StoreId
                        && ww.OrderId == info.OrderId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Closed Receipt data exist", lstInfo);
                    return result;
                }

                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_ClosedReceiptReport> lstInsert = new List<R_ClosedReceiptReport>();
                        R_ClosedReceiptReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_ClosedReceiptReport();
                            itemInsert.OrderId = item.OrderId;
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.CashierId = item.CashierId;
                            itemInsert.CashierName = item.CashierName;
                            itemInsert.NoOfPersion = item.NoOfPersion;
                            itemInsert.Total = item.Total;
                            itemInsert.ReceiptNo = item.ReceiptNo;
                            itemInsert.TableNo = item.TableNo;
                            itemInsert.Mode = item.Mode;
                            itemInsert.CreditNoteNo = item.CreditNoteNo;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_ClosedReceiptReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Closed Receipt data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert Closed Receipt data fail", lstInfo);
                        NSLog.Logger.Error("Insert Closed Receipt data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_ClosedReceiptReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<ClosedReceiptReportModels> GetData(BaseReportModel model, List<string> ListEmployees)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_ClosedReceiptReport
                               where model.ListStores.Contains(tb.StoreId)
                                     && ListEmployees.Contains(tb.CashierId)
                                     && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                                     && tb.Mode == model.Mode
                               select new ClosedReceiptReportModels
                               {
                                   CreatedDate = tb.CreatedDate,
                                   ReceiptNo = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.ReceiptNo : tb.CreditNoteNo, // Updated 03302018, refund Gift Card
                                   CashierName = tb.CashierName,
                                   CashierId = tb.CashierId,
                                   TableNo = tb.TableNo,
                                   NoOfPersion = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.NoOfPersion : 0, // if CreditNote, Number of person = 0
                                   Total = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.Total : (tb.Total * (-1)), // Updated 03302018, refund Gift Card, amount with negative value
                                   StoreId = tb.StoreId,
                                   OrderId = tb.OrderId,
                                   CreditNoteNo = tb.CreditNoteNo
                               }).ToList();

                var tips = cxt.G_OrderTip.Where(ww => model.ListStores.Contains(ww.StoreId) && ww.CreatedDate >= model.FromDate
                    && ww.CreatedDate <= model.ToDate && ww.Mode == model.Mode).ToList();

                if (lstData != null && lstData.Count > 0)
                {
                    lstData = lstData.OrderBy(oo => oo.CreatedDate).ToList();
                    foreach (var item in lstData)
                    {
                        item.Total += tips.Where(ww => ww.OrderId == item.OrderId).Sum(ss => ss.Amount);
                    }
                }
                return lstData;
            }
        }

        public List<ClosedReceiptReportModels> GetListNoOfPersion(List<string> OrderIds)
        {
            using (var cxt = new NuWebContext())
            {
                var objClosedReceipts = cxt.R_ClosedReceiptReport.Where(ww => OrderIds.Contains(ww.OrderId))
                    .Select(ss => new ClosedReceiptReportModels()
                    {
                        OrderId = ss.OrderId,
                        NoOfPersion = ss.NoOfPersion,
                    }).ToList();
                return objClosedReceipts;
            }
        }

        public List<ClosedReceiptReportModels> GetDataByIds(List<string> lstIds)
        {
            using (var cxt = new NuWebContext())
            {
                var results = cxt.R_ClosedReceiptReport.Where(ww => lstIds.Contains(ww.OrderId))
                    .Select(ss => new ClosedReceiptReportModels()
                    {
                        ReceiptNo = ss.ReceiptNo,
                        OrderId = ss.OrderId,
                        StoreId = ss.StoreId,
                        CashierId = ss.CashierId,
                        CashierName = ss.CashierName,
                        TableNo = ss.TableNo,
                        NoOfPersion = ss.NoOfPersion,
                        CreatedDate = ss.CreatedDate

                    }).ToList();
                return results;
            }
        }
        public bool ReportWithRefund(BaseReportModel model, ref IXLWorksheet ws, List<StoreModels> lstStore)
        {
            try
            {
                var lstStoreIndex = model.ListStores;
                List<string> lstUserID = model.ListEmployees.Where(m => m.Checked).Select(m => m.ID).ToList();
                //DateTimeHelper.GetDateTime(ref model);
                List<ClosedReceiptReportModels> lstData = GetData(model, lstUserID);
                if (lstData == null)
                    return false;

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreIndex, model.Mode);
                var _lstRefunds = _refundFactory.GetListRefund(lstStoreIndex, model.FromDate, model.ToDate, model.Mode);

                int totalCols = 7;//typeof(ClosedReceiptReportModels).GetProperties().Count() - 1;

                CreateReportHeaderNew(ws, totalCols, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Closed Receipt Report"));

                // Column Name
                #region Column Name
                //List<string> lstColNames = new List<string> { "Business Date", "Receipt No.", "Time In", "Cashier", "Table No.", "Number of Persons", "Total" };
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Business Date"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time In"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cashier"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Table No."), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Persons"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cell(5, i + 1).Value = lstColNames[i];
                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.SetBold(true);
                ws.Row(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(5, 1, 5, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                double totalAmounts = 0;
                int totalPersons = 0;
                int totalReceipts = 0;
                int currentRow = 6;
                List<int> lstStoreTotalRowIndex = new List<int>();
                List<int> lstNoOfReceiptRowIndex = new List<int>();
                List<int> lstNoOfPersionRowIndex = new List<int>();
                string storeName = string.Empty, storeId = string.Empty;
                foreach (StoreModels store in lstStore)
                {
                    #region Store Name
                    storeName = store.Name;
                    storeId = store.Id;

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
                    int totalReceipt = 0;
                    double storeTotal = 0;
                    int storePersonTotal = 0;

                    // Data
                    var days = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                    List<ClosedReceiptReportModels> _lstShowCloseReportRefund = null;
                    ClosedReceiptReportModels _showCloseReportRefund = null;
                    //int days = int.Parse((model.ToDate.Date - model.FromDate.Date).TotalDays.ToString()) + 1;
                    for (int i = 0; i < days.Count; i++) // Row Loop
                    {
                        _lstShowCloseReportRefund = new List<ClosedReceiptReportModels>();
                        DateTime date = model.FromDate.AddDays(i);
                        List<ClosedReceiptReportModels> lstDataInDate = lstData.Where(m => m.CreatedDate >= days[i].DateFrom
                        && m.CreatedDate <= days[i].DateTo && m.StoreId == storeId).ToList();

                        var lstRefundInBusiness = _lstRefunds.Where(ww => ww.BusinessDayId == days[i].Id).ToList();
                        if (lstRefundInBusiness == null)
                            lstRefundInBusiness = new List<RefundReportDTO>();
                        if (lstDataInDate == null)
                            lstDataInDate = new List<ClosedReceiptReportModels>();
                        var lstOrderId = lstDataInDate.Select(ss => ss.OrderId).ToList();

                        var lstRefundDifirentBusDay = lstRefundInBusiness.Where(ww => !lstOrderId.Contains(ww.OrderId)).ToList();
                        if (lstRefundDifirentBusDay != null && lstRefundDifirentBusDay.Count > 0)
                        {
                            var lstIds = lstRefundDifirentBusDay.Select(ss => ss.OrderId).ToList();
                            var lstOrderRefund = GetDataByIds(lstIds);
                            foreach (var item in lstOrderRefund)
                            {
                                _showCloseReportRefund = new ClosedReceiptReportModels();
                                _showCloseReportRefund.ReceiptNo = item.ReceiptNo;
                                _showCloseReportRefund.CreatedDate = item.CreatedDate;
                                _showCloseReportRefund.CashierName = item.CashierName;
                                _showCloseReportRefund.TableNo = item.TableNo;
                                _showCloseReportRefund.NoOfPersion = 0;
                                _showCloseReportRefund.Total = lstRefundDifirentBusDay.Where(ww => ww.OrderId == item.OrderId
                                            && ww.StoreId == item.StoreId).Sum(ss => -ss.TotalRefund);


                                _lstShowCloseReportRefund.Add(_showCloseReportRefund);
                            }
                            lstDataInDate.InsertRange(0, _lstShowCloseReportRefund);
                        }


                        foreach (ClosedReceiptReportModels data in lstDataInDate)
                        {
                            //date
                            ws.Cell(currentRow, 1).Value = days[i].DateDisplay;
                            //receipt no
                            ws.Cell(currentRow, 2).Value = data.ReceiptNo.Trim();
                            //time in
                            ws.Cell(currentRow, 3).Value = "'" + data.CreatedDate.ToString("HH:mm");
                            //Cashier
                            ws.Cell(currentRow, 4).Value = data.CashierName;
                            //Table No
                            ws.Cell(currentRow, 5).Value = data.TableNo;
                            //No of Person
                            ws.Cell(currentRow, 6).Value = data.NoOfPersion;
                            //Total
                            ws.Cell(currentRow, 7).Value = data.Total;
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";

                            storeTotal += data.Total;
                            storePersonTotal += data.NoOfPersion;
                            //loop total column
                            if(data.Total > 0)
                            {
                                totalReceipt++;
                            }
                          
                            currentRow++;
                        }

                    }//end date loop

                    //ws.Range(beginCal, 1, currentRow - 1, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                    // Store's Total
                    #region Store's Total
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store's Total");
                    ws.Cell(currentRow, totalCols).Value = storeTotal;
                    ws.Cell(currentRow, totalCols).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstStoreTotalRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;

                    // Number of Persons
                    #region Number of Persons
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Persons");
                    ws.Cell(currentRow, totalCols).Value = storePersonTotal;
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstNoOfPersionRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;

                    // Number of Receipts
                    #region Number of Persons
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Receipts");
                    ws.Cell(currentRow, totalCols).Value = string.Format("{0}", totalReceipt);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstNoOfReceiptRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;
                    totalAmounts += storeTotal;
                    totalPersons += storePersonTotal;
                    totalReceipts += totalReceipt;
                }//end store loop  

                // Total Amount
                #region Total Amount
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL AMOUNT");

                List<string> lstFormular = new List<string>();
                ws.Cell(currentRow, totalCols).Value = totalAmounts;
                ws.Cell(currentRow, totalCols).Style.NumberFormat.Format = "#,##0.00";
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                // Total Number of Persons
                #region Total Number of Persons
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Number of Persons");

                ws.Cell(currentRow, totalCols).Value = totalPersons;
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                // Total Number of Receipts
                #region Total Number of Receipts
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Number of Receipts");

                ws.Cell(currentRow, totalCols).Value = totalReceipts;
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                ws.Range(1, 1, currentRow - 1, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, currentRow - 1, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 1, totalCols).Style.Border.BottomBorder = XLBorderStyleValues.None;
                ws.Range(2, 1, 2, totalCols).Style.Border.TopBorder = XLBorderStyleValues.None;
                ws.Columns(1, totalCols).AdjustToContents();

                ws.Column(totalCols).Width = 10;
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        public bool Report(BaseReportModel model, ref IXLWorksheet ws, List<StoreModels> lstStore)
        {
            try
            {
                var lstStoreIndex = model.ListStores;
                var lstSelected = model.ListEmployees.Where(m => m.Checked).ToList();
                List<string> lstUserID = model.ListEmployees.Where(m => m.Checked).Select(m => m.ID).ToList();
                List<string> lstEmplOnStore = model.ListEmployees.Where(m => m.Checked).Select(m => m.StoreId).ToList();
                if (lstEmplOnStore != null && lstEmplOnStore.Any())
                    lstEmplOnStore = lstEmplOnStore.Distinct().ToList();

                int totalCols = 7;
                // Set header report
                CreateReportHeaderNew(ws, totalCols, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Closed Receipt Report"));
                ws.Range(1, 1, 4, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, lstStoreIndex, model.Mode);
                
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return false;
                }

                model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                //get data
                List<ClosedReceiptReportDataModels> lstData = new List<ClosedReceiptReportDataModels>();
                using (var db = new NuWebContext())
                {
                    lstData = db.GetDataForCloseReceiptReport(new BaseReportDataModel() { ListStores = model.ListStores, FromDate = model.FromDate
                        , ToDate = model.ToDate, Mode = model.Mode });
                }
                if (lstData!= null && lstData.Any())
                {
                    lstData = lstData.Where(ww => lstUserID.Contains(ww.CashierId)).ToList();
                }

                //List<ClosedReceiptReportModels> lstData = GetData(model, lstUserID);

                if (lstData == null || !lstData.Any())
                    return false;

                // Filter Start Time , End Time if Start Time != 0 & End Time != 0
                switch (model.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(o => o.CreatedDate).ToList();
                        break;
                }

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time In"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cashier"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Table No."), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Persons"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total") };
                for (int i = 0; i < totalCols; i++)
                    ws.Cell(5, i + 1).Value = lstColNames[i];
                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.SetBold(true);
                ws.Row(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(5, 1, 5, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                double totalAmounts = 0;
                int totalPersons = 0;
                int totalReceipts = 0;
                int currentRow = 6;
                List<int> lstStoreTotalRowIndex = new List<int>();
                List<int> lstNoOfReceiptRowIndex = new List<int>();
                List<int> lstNoOfPersionRowIndex = new List<int>();
                string storeName = string.Empty, storeId = string.Empty;
                List<string> lstEmpSelectedInStore = new List<string>();

                List<ClosedReceiptReportDataModels> lstDataInDate = new List<ClosedReceiptReportDataModels>();
                foreach (StoreModels store in lstStore)
                {
                    #region Store Name
                    storeName = store.Name;
                    storeId = store.Id;
                    if (!lstEmplOnStore.Contains(storeId))
                        continue;

                    lstEmpSelectedInStore = lstSelected.Where(ww => ww.StoreId == storeId).Select(ss=>ss.ID).ToList();
                    ws.Cell(currentRow, 1).Value = storeName;

                    ws.Range(currentRow, 1, currentRow, totalCols).Merge();
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Row(currentRow).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    #endregion
                    currentRow++;

                    int beginCal = currentRow;
                    int totalReceipt = 0;
                    double storeTotal = 0;
                    int storePersonTotal = 0;

                    // Data
                    var days = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();

                    for (int i = 0; i < days.Count; i++) // Row Loop
                    {
                        DateTime date = model.FromDate.AddDays(i);
                        lstDataInDate = lstData.Where(m => m.CreatedDate >= days[i].DateFrom
                        && m.CreatedDate <= days[i].DateTo && m.StoreId == storeId && lstEmpSelectedInStore.Contains(m.CashierId)).ToList();
                        if (lstDataInDate != null && lstDataInDate.Any())
                        {
                            foreach (ClosedReceiptReportDataModels data in lstDataInDate)
                            {   //loop total column
                                #region Old
                                //for (int j = 0; j < totalCols; j++)
                                //{
                                //    switch (j)
                                //    {
                                //        case 0:
                                //            // Date
                                //            ws.Cell(currentRow, 1).Value = data.CreatedDate;
                                //            ws.Cell(currentRow, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                                //            break;
                                //        case 1:
                                //            // Receipt No.
                                //            ws.Cell(currentRow, j + 1).Value = data.ReceiptNo.Trim();
                                //            break;
                                //        case 2:
                                //            // Time In
                                //            ws.Cell(currentRow, j + 1).Value = "'" + data.CreatedDate.ToString("HH:mm");
                                //            break;
                                //        case 3:
                                //            // Cashier
                                //            ws.Cell(currentRow, j + 1).Value = data.CashierName;
                                //            break;
                                //        case 4:
                                //            // Table No.
                                //            ws.Cell(currentRow, j + 1).Value = data.TableNo;
                                //            break;
                                //        case 5:
                                //            // Number of Persons
                                //            ws.Cell(currentRow, j + 1).Value = data.NoOfPersion;
                                //            // total person
                                //            storePersonTotal += data.NoOfPersion;
                                //            break;
                                //        case 6:
                                //            // Total
                                //            ws.Cell(currentRow, j + 1).Value = data.Total;
                                //            ws.Cell(currentRow, j + 1).Style.NumberFormat.Format = "#,##0.00";
                                //            // store total
                                //            if (string.IsNullOrEmpty(data.CreditNoteNo)) // Only Receipts
                                //            {
                                //                storeTotal += data.Total;
                                //                totalReceipt++;
                                //            }
                                //            break;
                                //    }
                                //}
                                #endregion end old

                                #region new
                                ws.Cell(currentRow, 1).Value = data.CreatedDate;
                                ws.Cell(currentRow, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                                ws.Cell(currentRow, 2).Value = data.ReceiptNo;
                                ws.Cell(currentRow, 3).Value = "'" + data.CreatedDate.ToString("HH:mm");
                                ws.Cell(currentRow, 4).Value = data.CashierName;
                                // Table No.
                                ws.Cell(currentRow, 5).Value = data.TableNo;
                                // Number of Persons
                                ws.Cell(currentRow, 6).Value = data.NoOfPersion;
                                // total person
                                storePersonTotal += data.NoOfPersion;
                                // Total
                                ws.Cell(currentRow, 7).Value = data.Total;
                                ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
                                // store total
                                if (string.IsNullOrEmpty(data.CreditNoteNo)) // Only Receipts
                                {
                                    storeTotal += data.Total;
                                    totalReceipt++;
                                }
                                #endregion end new
                                currentRow++;
                            }
                        }
                    }//end date loop

                    // Store's Total
                    #region Store's Total
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Total Sales");
                    ws.Cell(currentRow, totalCols).Value = storeTotal;
                    ws.Cell(currentRow, totalCols).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstStoreTotalRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;

                    // Number of Persons
                    #region Number of Persons
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Persons");
                    ws.Cell(currentRow, totalCols).Value = storePersonTotal;
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstNoOfPersionRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;

                    // Number of Receipts
                    #region Number of Persons
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Receipts");
                    ws.Cell(currentRow, totalCols).Value = string.Format("{0}", totalReceipt);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstNoOfReceiptRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;
                    totalAmounts += storeTotal;
                    totalPersons += storePersonTotal;
                    totalReceipts += totalReceipt;
                }//end store loop  

                // Total Amount
                #region Total Amount
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL RECEIPT AMOUNT");

                List<string> lstFormular = new List<string>();
                ws.Cell(currentRow, totalCols).Value = totalAmounts;
                ws.Cell(currentRow, totalCols).Style.NumberFormat.Format = "#,##0.00";
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                // Total Number of Persons
                #region Total Number of Persons
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Number of Persons");

                ws.Cell(currentRow, totalCols).Value = totalPersons;
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                // Total Number of Receipts
                #region Total Number of Receipts
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Number of Receipts");

                ws.Cell(currentRow, totalCols).Value = totalReceipts;
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                ws.Range(5, 1, currentRow - 1, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(5, 1, currentRow - 1, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 1, totalCols).Style.Border.BottomBorder = XLBorderStyleValues.None;
                ws.Range(2, 1, 2, totalCols).Style.Border.TopBorder = XLBorderStyleValues.None;
                ws.Columns(1, totalCols).AdjustToContents();

                ws.Column(totalCols).Width = 10;
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        #region Report with new DB from [R_PosSale], [R_PosSaleDetail]
        public List<ClosedReceiptReportModels> GetData_NewDB(BaseReportModel model, List<string> ListEmployees)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_PosSale
                               where model.ListStores.Contains(tb.StoreId)
                                     && ListEmployees.Contains(tb.CashierId)
                                     && (tb.ReceiptCreatedDate >= model.FromDate && tb.ReceiptCreatedDate <= model.ToDate)
                                     && tb.Mode == model.Mode
                                     && tb.ReceiptCreatedDate.HasValue
                               select new ClosedReceiptReportModels
                               {
                                   CreatedDate = tb.ReceiptCreatedDate.HasValue ? tb.ReceiptCreatedDate.Value : tb.CreatedDate,
                                   ReceiptNo = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.ReceiptNo : tb.CreditNoteNo, // refund Gift Card
                                   CashierName = tb.CashierName,
                                   CashierId = tb.CashierId,
                                   TableNo = tb.TableNo,
                                   NoOfPersion = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.NoOfPerson : 0, // if CreditNote, Number of person = 0
                                   Total = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.ReceiptTotal : (tb.ReceiptTotal * (-1)), // refund Gift Card, amount with negative value
                                   StoreId = tb.StoreId,
                                   OrderId = tb.OrderId,
                                   CreditNoteNo = tb.CreditNoteNo
                               }).ToList();

                var tips = cxt.G_OrderTip.Where(ww => model.ListStores.Contains(ww.StoreId) && ww.CreatedDate >= model.FromDate
                    && ww.CreatedDate <= model.ToDate && ww.Mode == model.Mode).ToList();

                if (lstData != null && lstData.Any())
                {
                    lstData = lstData.OrderBy(oo => oo.CreatedDate).ToList();
                    foreach (var item in lstData)
                    {
                        item.Total += tips.Where(ww => ww.OrderId == item.OrderId).Sum(ss => ss.Amount);
                    }
                }
                return lstData;
            }
        }

        public bool Report_NewDB(BaseReportModel model, ref IXLWorksheet ws, List<StoreModels> lstStore)
        {
            try
            {
                //var lstStoreIndex = model.ListStores;
                var lstEmpSelected = model.ListEmployees.Where(m => m.Checked).ToList();
                if (lstEmpSelected == null || !lstEmpSelected.Any())
                {
                    return false;
                }
                List<string> lstUserID = lstEmpSelected.Select(m => m.ID).ToList();
                List<string> lstStoreOfEmp = lstEmpSelected.Select(m => m.StoreId).Distinct().ToList();
                model.ListStores = lstStoreOfEmp;
                int totalCols = 7;
                // Set header report
                CreateReportHeaderNew(ws, totalCols, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Closed Receipt Report"));
                ws.Range(1, 1, 4, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return false;
                }

                model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

                List<ClosedReceiptReportModels> lstData = GetData_NewDB(model, lstUserID);
                if (lstData == null || !lstData.Any())
                {
                    return false;
                } else
                {
                    switch (model.FilterType)
                    {
                        case (int)Commons.EFilterType.OnDay:
                            lstData = lstData.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(oo => oo.CreatedDate).ToList();
                            if (lstData == null || !lstData.Any())
                            {
                                return false;
                            }
                            break;
                        case (int)Commons.EFilterType.Days:
                            lstData = lstData.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).OrderBy(oo => oo.CreatedDate).ToList();
                            if (lstData == null || !lstData.Any())
                            {
                                return false;
                            }
                            break;
                    }
                }

                // Column Name
                #region Column Name
                List<string> lstColNames = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receipt No."), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time In"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cashier"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Table No."), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Persons"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total") };

                for (int i = 0; i < totalCols; i++)
                    ws.Cell(5, i + 1).Value = lstColNames[i];
                ws.Row(5).Height = 25;
                ws.Row(5).Style.Font.SetBold(true);
                ws.Row(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(5, 1, 5, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                // For all stores
                double totalAmounts = 0;
                int totalPersons = 0;
                int totalReceipts = 0;
                // For a store
                int totalReceipt = 0;
                double storeTotal = 0;
                int storePersonTotal = 0;

                int currentRow = 6;
                List<int> lstStoreTotalRowIndex = new List<int>();
                List<int> lstNoOfReceiptRowIndex = new List<int>();
                List<int> lstNoOfPersionRowIndex = new List<int>();

                List<string> lstEmpSelectedInStore = new List<string>();

                var lstStoreInfo = lstStore.Where(ww => model.ListStores.Contains(ww.Id)).OrderBy(oo => oo.Name).ToList();
                foreach (var store in lstStoreInfo)
                {
                    #region Store Name

                    ws.Cell(currentRow, 1).Value = store.Name;

                    ws.Range(currentRow, 1, currentRow, totalCols).Merge();
                    ws.Row(currentRow).Height = 20;
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Row(currentRow).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    #endregion
                    currentRow++;

                    totalReceipt = 0;
                    storeTotal = 0;
                    storePersonTotal = 0;

                    var days = _lstBusDayAllStore.Where(ww => ww.StoreId == store.Id).OrderBy(oo => oo.DateFrom).ToList();

                    if (days != null && days.Any())
                    {
                        lstEmpSelectedInStore = lstEmpSelected.Where(ww => ww.StoreId == store.Id).Select(ss => ss.ID).ToList();

                        for (int i = 0; i < days.Count; i++) // Row Loop
                        {
                            List<ClosedReceiptReportModels> lstDataInDate = lstData.Where(m => m.CreatedDate >= days[i].DateFrom
                            && m.CreatedDate <= days[i].DateTo && m.StoreId == store.Id && lstEmpSelectedInStore.Contains(m.CashierId)).OrderBy(oo => oo.CreatedDate).ToList();

                            if (lstDataInDate != null && lstDataInDate.Any())
                            {
                                foreach (ClosedReceiptReportModels data in lstDataInDate)
                                {   //loop total column
                                    for (int j = 0; j < totalCols; j++)
                                    {
                                        switch (j)
                                        {
                                            case 0:
                                                // Date
                                                ws.Cell(currentRow, 1).Value = data.CreatedDate;
                                                ws.Cell(currentRow, 1).Style.DateFormat.Format = "MM/dd/yyyy";
                                                break;
                                            case 1:
                                                // Receipt No.
                                                ws.Cell(currentRow, j + 1).Value = data.ReceiptNo.Trim();
                                                break;
                                            case 2:
                                                // Time In
                                                ws.Cell(currentRow, j + 1).Value = "'" + data.CreatedDate.ToString("HH:mm");
                                                break;
                                            case 3:
                                                // Cashier
                                                ws.Cell(currentRow, j + 1).Value = data.CashierName;
                                                break;
                                            case 4:
                                                // Table No.
                                                ws.Cell(currentRow, j + 1).Value = data.TableNo;
                                                break;
                                            case 5:
                                                // Number of Persons
                                                ws.Cell(currentRow, j + 1).Value = data.NoOfPersion;
                                                // total person
                                                storePersonTotal += data.NoOfPersion;
                                                break;
                                            case 6:
                                                // Total
                                                ws.Cell(currentRow, j + 1).Value = data.Total;
                                                ws.Cell(currentRow, j + 1).Style.NumberFormat.Format = "#,##0.00";
                                                // store total
                                                if (string.IsNullOrEmpty(data.CreditNoteNo)) // Only Receipts
                                                {
                                                    storeTotal += data.Total;
                                                    totalReceipt++;
                                                }
                                                break;
                                        }
                                    }
                                    currentRow++;
                                }
                            }
                        }//end date loop
                    }

                    // Store's Total
                    #region Store's Total
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Total Sales");
                    ws.Cell(currentRow, totalCols).Value = storeTotal;
                    ws.Cell(currentRow, totalCols).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstStoreTotalRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;

                    // Number of Persons
                    #region Number of Persons
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Persons");
                    ws.Cell(currentRow, totalCols).Value = storePersonTotal;
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstNoOfPersionRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;

                    // Number of Receipts
                    #region Number of Persons
                    ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Receipts");
                    ws.Cell(currentRow, totalCols).Value = string.Format("{0}", totalReceipt);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                    ws.Row(currentRow).Style.Font.SetBold(true);
                    ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    lstNoOfReceiptRowIndex.Add(currentRow);
                    #endregion
                    currentRow++;
                    totalAmounts += storeTotal;
                    totalPersons += storePersonTotal;
                    totalReceipts += totalReceipt;
                }//end store loop  

                // Total Amount
                #region Total Amount
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL RECEIPT AMOUNT");

                List<string> lstFormular = new List<string>();
                ws.Cell(currentRow, totalCols).Value = totalAmounts;
                ws.Cell(currentRow, totalCols).Style.NumberFormat.Format = "#,##0.00";
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                // Total Number of Persons
                #region Total Number of Persons
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Number of Persons");

                ws.Cell(currentRow, totalCols).Value = totalPersons;
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                // Total Number of Receipts
                #region Total Number of Receipts
                ws.Cell(currentRow, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Number of Receipts");

                ws.Cell(currentRow, totalCols).Value = totalReceipts;
                ws.Row(currentRow).Style.Font.SetBold(true);
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Merge();
                ws.Range(currentRow, 1, currentRow, totalCols - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                #endregion
                currentRow++;

                ws.Range(5, 1, currentRow - 1, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(5, 1, currentRow - 1, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 1, totalCols).Style.Border.BottomBorder = XLBorderStyleValues.None;
                ws.Range(2, 1, 2, totalCols).Style.Border.TopBorder = XLBorderStyleValues.None;
                ws.Columns(1, totalCols).AdjustToContents();

                ws.Column(totalCols).Width = 10;
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }
        #endregion
    }
}
