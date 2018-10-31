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

namespace NuWebNCloud.Shared.Factory
{
    public class TimeClockReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public TimeClockReportFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public bool Insert(List<TimeClockReportModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            //_logger.Info("==========================================================");
            //_logger.Info(string.Format("Insert TimeClock: StoreId: [{0}] | BusinessId: [{1}]", info.StoreId, info.BusinessId));
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_TimeClockReport.Where(ww => ww.StoreId == info.StoreId
                                && ww.UserId == info.UserId && ww.BusinessId == info.BusinessId).FirstOrDefault();

                //var obj = cxt.R_TimeClockReport.Where(ww => ww.StoreId == info.StoreId
                //              && ww.UserId == info.UserId && ww.CreatedDate == info.CreatedDate).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Time Clock data exist");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_TimeClockReport> lstInsert = new List<R_TimeClockReport>();
                        R_TimeClockReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_TimeClockReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.UserId = item.UserId;
                            itemInsert.UserName = item.UserName;
                            itemInsert.DateTimeIn = item.DateTimeIn;
                            itemInsert.DateTimeOut = item.DateTimeOut;
                            itemInsert.DayOfWeeks = item.DayOfWeeks;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.Early = item.Early;
                            itemInsert.Late = item.Late;
                            itemInsert.HoursWork = item.HoursWork;
                            itemInsert.Mode = item.Mode;
                            itemInsert.BusinessId = item.BusinessId;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_TimeClockReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();
                        //_logger.Info(string.Format("Insert TimeClock: StoreId: [{0}] | BusinessId: [{1}] success", info.StoreId, info.BusinessId));
                        NSLog.Logger.Info("Insert Time Clock data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Time Clock data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_TimeClockReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public bool InsertForMigration(List<TimeClockReportModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_TimeClockReport.Where(ww => ww.StoreId == info.StoreId
                              && ww.UserId == info.UserId && ww.CreatedDate == info.CreatedDate).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert [TimeClock for Migration] data exist");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_TimeClockReport> lstInsert = new List<R_TimeClockReport>();
                        R_TimeClockReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_TimeClockReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.UserId = item.UserId;
                            itemInsert.UserName = item.UserName;
                            itemInsert.DateTimeIn = item.DateTimeIn;
                            itemInsert.DateTimeOut = item.DateTimeOut;
                            itemInsert.DayOfWeeks = item.DayOfWeeks;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.Early = item.Early;
                            itemInsert.Late = item.Late;
                            itemInsert.HoursWork = item.HoursWork;
                            itemInsert.Mode = item.Mode;
                            itemInsert.BusinessId = item.BusinessId;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_TimeClockReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert [TimeClock for Migration] data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert [TimeClock for Migration] data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_TimeClockReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }


        public List<TimeClockReportModels> GetTimeClockSummary(List<string> lstStoreId, List<string> lstUserID, List<string> lstBusDayId, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_TimeClockReport
                               where lstStoreId.Contains(tb.StoreId)
                                     && lstUserID.Contains(tb.UserName)
                                     && lstBusDayId.Contains(tb.BusinessId)
                                     && tb.Mode == mode
                               select tb
                              ).Select(ss => new TimeClockReportModels()
                              {
                                  UserId = ss.UserId,
                                  UserName = ss.UserName,
                                  StoreId = ss.StoreId,
                                  DateTimeIn = ss.DateTimeIn,
                                  DateTimeOut = ss.DateTimeOut,
                                  HoursWork = ss.HoursWork,
                                  CreatedDate = ss.CreatedDate,
                                  Late = ss.Late,
                                  Early = ss.Early,
                                  BusinessId = ss.BusinessId
                              }).ToList();
                return lstData;
            }
        }

        public XLWorkbook ExportExcelSummary(BaseReportModel model, List<StoreModels> lstStore)
        {

            string sheetName = "Time_Clock_Summary";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            var lstEmpChecked = model.ListEmployees.Where(m => m.Checked).ToList();
            if (lstEmpChecked == null || !lstEmpChecked.Any())
            {
                // Set header report
                CreateReportHeader(ws, 12, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Clock Summary Report"));
                // Format header report 
                ws.Range(1, 1, 3, 12).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 3, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            List<string> lstUserID = lstEmpChecked.Select(m => m.Name).ToList();
            model.ListStores = lstEmpChecked.Select(m => m.StoreId).ToList();

            DateTimeHelper.GetDateTime(ref model);

            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            // When data null
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Set header report
                CreateReportHeader(ws, 12, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Clock Summary Report"));
                // Format header report 
                ws.Range(1, 1, 3, 12).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 3, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            var _lstBusDayIdAllStore = _lstBusDayAllStore.Select(ss => ss.Id).ToList();

            var data = GetTimeClockSummary(model.ListStores, lstUserID, _lstBusDayIdAllStore, model.Mode);
            // When data null
            if (data == null || !data.Any())
            {
                // Set header report
                CreateReportHeader(ws, 12, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Clock Summary Report"));
                // Format header report 
                ws.Range(1, 1, 3, 12).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 3, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            //Table
            //row 1: Table Header
            CreateReportHeader(ws, 8, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Clock Summary Report"));

            DateTime fromDate = model.FromDate;
            DateTime toDate = model.ToDate;
            int row = 4;
            int startRow = row;
            int startRow_Store = row;
            int endRow_Store = row;
            int startRow_User = row;
            int endRow_User = row;
            string storeId = string.Empty, storeName = string.Empty;
            lstStore = lstStore.Where(ww => model.ListStores.Contains(ww.Id)).OrderBy(oo => oo.Name).ToList();
            for (int i = 0; i < lstStore.Count; i++)
            {
                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name;

                var _lstBusDayIdStore = _lstBusDayAllStore.Where(x => x.StoreId == storeId).Select(ss => ss.Id).ToList();
                if (_lstBusDayIdStore != null && _lstBusDayIdStore.Any())
                {
                    var dataStore = data.Where(x => x.StoreId == storeId && _lstBusDayIdStore.Contains(x.BusinessId)).OrderBy(oo => oo.DateTimeIn).ToList();
                    if (dataStore != null && dataStore.Any())
                    {
                        startRow_Store = row;
                        endRow_Store = row;

                        row++;
                        //Timer Header
                        ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Employee"));
                        ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date In"));
                        ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time In"));
                        ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date Out"));
                        ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Out"));
                        ws.Cell("F" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hours Worked"));
                        ws.Cell("G" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Late(Minutes)"));
                        ws.Cell("H" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Early Leave(Minutes)"));

                        ws.Range("A" + row + ":H" + row).Style.Font.SetBold(true);
                        ws.Range("A" + row + ":H" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        row++;

                        #region======Get Employees
                        List<RPEmployeeItemModels> lstUser = lstEmpChecked.Where(ww => ww.StoreId == storeId).Select(m => new RPEmployeeItemModels
                        {
                            ID = m.ID,
                            Name = m.Name,
                            StoreId = m.StoreId
                        }).OrderBy(oo => oo.Name).ToList();

                        int frmDateOutMerge = 0, toDateOutMerge = 0;
                        List<DateOutInfo> lstDateOuts = new List<DateOutInfo>();
                        foreach (var employee in lstUser)
                        {
                            var dataEmp = dataStore.Where(x => x.UserId == employee.ID).GroupBy(gg => gg.CreatedDate.Date).OrderBy(oo => oo.Key).ToList();
                            if (dataEmp != null && dataEmp.Any())
                            {
                                startRow_User = row;
                                endRow_User = row;

                                #region====Date
                                double _sumHw = 0, _sumLate = 0, _sumEarly = 0;
                                var _checkObj = new TimeClockReportModels();

                                foreach (var itmData in dataEmp)
                                {
                                    int startRow_Date = row;
                                    int endRow_Date = row;
                                    DateTime _date = itmData.Key;

                                    var lstdateDateGroup = itmData.GroupBy(gg => gg.DateTimeIn).OrderBy(oo => oo.Key).ToList();
                                    lstDateOuts = new List<DateOutInfo>();
                                    frmDateOutMerge = row;
                                    toDateOutMerge = row;
                                    //loop data in date
                                    foreach (var itemGroup in lstdateDateGroup)
                                    {
                                        _checkObj = itemGroup.Where(ww => ww.DateTimeOut.Date != Commons.MinDate.Date).FirstOrDefault();
                                        if (_checkObj == null)
                                            _checkObj = itemGroup.FirstOrDefault();

                                        //Time In
                                        ws.Cell("C" + row).SetValue(DateTimeHelper.GetAMPM(_checkObj.DateTimeIn.TimeOfDay));
                                        ws.Cell("C" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                        //check merge time out
                                        if (_checkObj.DateTimeOut.Date != Commons.MinDate.Date)
                                        {
                                            var checkExist = lstDateOuts.Where(ww => ww.DateOut.Date == _checkObj.DateTimeOut.Date).FirstOrDefault();
                                            if (checkExist == null)
                                            {
                                                var info = new DateOutInfo();
                                                info.DateOut = _checkObj.DateTimeOut;
                                                info.FromRow = frmDateOutMerge;
                                                info.ToRow = toDateOutMerge;

                                                lstDateOuts.Add(info);
                                                frmDateOutMerge++;
                                                toDateOutMerge++;
                                            }
                                            else
                                            {
                                                checkExist.ToRow++;
                                            }

                                            //E time out
                                            ws.Cell("E" + row).SetValue(DateTimeHelper.GetAMPM(_checkObj.DateTimeOut.TimeOfDay));
                                            ws.Cell("E" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                        }
                                        else
                                        {
                                            frmDateOutMerge++;
                                            toDateOutMerge++;
                                        }

                                        var _hoursWorked = Math.Round(_checkObj.HoursWork, 2);
                                        // F hours work
                                        ws.Cell("F" + row).SetValue(_hoursWorked);
                                        ws.Cell("F" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                        ws.Cell("F" + row).Style.NumberFormat.Format = "#,##0.00";
                                        _sumHw += _hoursWorked;

                                        ws.Cell("G" + row).SetValue(Math.Round(_checkObj.Late, 2));
                                        ws.Cell("G" + row).Style.NumberFormat.Format = "#,##0.00";
                                        ws.Cell("G" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                        _sumLate += Math.Round(_checkObj.Late, 2);

                                        ws.Cell("H" + row).SetValue(Math.Round(_checkObj.Early, 2));
                                        ws.Cell("H" + row).Style.NumberFormat.Format = "#,##0.00";
                                        ws.Cell("H" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                        _sumEarly += Math.Round(_checkObj.Early, 2);

                                        //========================================================
                                        row++;
                                        endRow_User++;
                                        endRow_Date++;

                                        //Date Out
                                        foreach (var item in lstDateOuts)
                                        {
                                            ws.Range("D" + item.FromRow + ":D" + item.ToRow).Merge().SetValue(item.DateOut.ToString("MM/dd/yyyy"));
                                            ws.Range("D" + item.FromRow + ":D" + item.ToRow).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                            ws.Range("D" + item.FromRow + ":D" + item.ToRow).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                                            ws.Range("D" + item.FromRow + ":D" + item.ToRow).Style.Font.SetBold(true);

                                        }
                                        //==============================================================================
                                        //row++;
                                        //endRow_User++;
                                        //endRow_Date++;
                                    }//end dateDate.Count > 0

                                    ws.Range("B" + startRow_Date + ":B" + (endRow_Date - 1)).Merge().SetValue(_date.ToString("MM/dd/yyyy"));
                                    ws.Range("B" + startRow_Date + ":B" + (endRow_Date - 1)).Style.Font.SetBold(true);
                                    ws.Range("B" + startRow_Date + ":B" + (endRow_Date - 1)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                    ws.Range("B" + startRow_Date + ":B" + (endRow_Date - 1)).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                                }
                                #endregion
                                //===> Total
                                row++;
                                endRow_User++;
                                ws.Cell("B" + (row - 1)).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
                                ws.Cell("B" + (row - 1)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                                ws.Range("B" + (row - 1) + ":E" + (row - 1)).Merge();

                                ws.Cell("F" + (row - 1)).SetValue(_sumHw);
                                ws.Cell("F" + (row - 1)).Style.NumberFormat.Format = "#,##0.00";
                                ws.Cell("F" + (row - 1)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                ws.Cell("G" + (row - 1)).SetValue(_sumLate);
                                ws.Cell("G" + (row - 1)).Style.NumberFormat.Format = "#,##0.00";
                                ws.Cell("G" + (row - 1)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                ws.Cell("H" + (row - 1)).SetValue(_sumEarly);
                                ws.Cell("H" + (row - 1)).Style.NumberFormat.Format = "#,##0.00";
                                ws.Cell("H" + (row - 1)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                ws.Range("B" + (row - 1) + ":H" + (row - 1)).Style.Font.SetBold(true);
                                ws.Range("B" + (row - 1) + ":H" + (row - 1)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                                //========================
                                ws.Range("A" + startRow_User + ":A" + (endRow_User - 1)).Merge().SetValue(employee.Name);
                                ws.Range("A" + startRow_User + ":A" + (endRow_User - 1)).Style.Font.SetBold(true);
                                ws.Range("A" + startRow_User + ":A" + (endRow_User - 1)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                ws.Range("A" + startRow_User + ":A" + (endRow_User - 1)).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                            }
                        }
                        #endregion
                        //Get StoreName

                        ws.Range("A" + startRow_Store + ":H" + endRow_Store).Merge().SetValue(storeName);
                        ws.Range("A" + startRow_Store + ":H" + endRow_Store).Style.Font.SetBold(true);
                        ws.Range("A" + startRow_Store + ":H" + endRow_Store).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorStore));
                    }
                }

            }
            //Set Border 
            ws.Range("A1:H" + (row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A1:H" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            ws.Columns().AdjustToContents();

            ws.Column(1).Width = 30;
            ws.Column(2).Width = 15;
            ws.Column(4).Width = 15;

            return wb;
        }
    }
    public class DateOutInfo
    {
        public DateTime DateOut { get; set; }
        public int FromRow { get; set; }
        public int ToRow { get; set; }
    }
}
