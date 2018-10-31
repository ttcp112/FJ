using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Shared.Factory
{
    public class NoSaleDetailReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public NoSaleDetailReportFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<NoSaleDetailReportModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_NoSaleDetailReport.Where(ww => ww.StoreId == info.StoreId
                                && ww.BusinessId == info.BusinessId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert No Sale data exist");
                    return result;
                }

                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_NoSaleDetailReport> lstInsert = new List<R_NoSaleDetailReport>();
                        R_NoSaleDetailReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_NoSaleDetailReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.DrawerId = item.DrawerId;
                            itemInsert.DrawerName = item.DrawerName;
                            itemInsert.CashierId = item.CashierId;
                            itemInsert.CashierName = item.CashierName;
                            itemInsert.Reason = item.Reason;
                            itemInsert.Mode = item.Mode;
                            itemInsert.BusinessId = item.BusinessId;
                            itemInsert.ShiftId = item.ShiftId;
                            itemInsert.StartedShift = item.StartedShift;
                            itemInsert.ClosedShift = item.ClosedShift;

                            lstInsert.Add(itemInsert);
                        }

                        cxt.R_NoSaleDetailReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert No Sale data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert No Sale data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_NoSaleDetailReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<NoSaleDetailReportModels> GetData(BaseReportModel model, string StoreId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_NoSaleDetailReport
                               where tb.StoreId == StoreId
                                     && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                               orderby tb.CreatedDate
                               select new NoSaleDetailReportModels
                               {
                                   CashierId = tb.CashierId,
                                   CashierName = tb.CashierName,
                                   CreatedDate = tb.CreatedDate,
                                   DrawerId = tb.DrawerId,
                                   DrawerName = tb.DrawerName,
                                   Reason = tb.Reason,
                                   StoreId = tb.StoreId,
                               }).ToList();
                return lstData;
            }
        }

        public int GetNoSale(string storeId, DateTime dFrom, DateTime dTo)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var lstData = cxt.R_NoSaleDetailReport.
                               Where(tb => tb.StoreId == storeId
                                     && tb.CreatedDate >= dFrom && tb.CreatedDate <= dTo).Select(ss => ss.Id).Count();

                return lstData;
            }
        }
        public int GetNoSale(string storeId, DateTime dDate)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var lstData = cxt.R_NoSaleDetailReport.
                               Where(tb => tb.StoreId == storeId
                                     && DbFunctions.TruncateTime(tb.CreatedDate) == DbFunctions.TruncateTime(dDate)).Select(ss => ss.Id).Count();

                return lstData;
            }
        }


        public XLWorkbook ExportExcel(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "No_Sale_Detail";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeader(ws, 5, model.FromDate, model.ToDate, "No Sale Detail Report");
            for (int i = 1; i <= 5; i++)
            {
                if (i == 4)
                    ws.Column(i).Width = 30;
                else
                    ws.Column(i).Width = 15;
            }
            ws.Column(7).Width = 20;
            ws.Column(8).Width = 20;

            DateTime fromDate = model.FromDate;
            DateTime toDate = model.ToDate;

            int row = 4;
            int startRow = row;
            int startRow_Store = row;
            int startRow_User = row;
            int endRow_User = row;
            string storeId = string.Empty, storeName = string.Empty;
            for (int i = 0; i < lstStore.Count; i++)
            {
                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name;

                startRow_Store = row;

                //Get StoreName
                ws.Range(string.Format("A{0}:E{0}", startRow_Store)).Merge().SetValue("STORE: " + storeName);
                ws.Range(string.Format("A{0}:E{0}", startRow_Store)).Style.Font.SetBold(true);
                ws.Range(string.Format("A{0}:E{0}", startRow_Store)).Style.Font.SetFontSize(14);
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                ws.Range("A" + row + ":E" + row).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                ws.Range("A" + row + ":E" + row).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorStore));
                row++;
                ws.Cell("A" + row).SetValue("Date");
                ws.Cell("B" + row).SetValue("Time");
                ws.Cell("C" + row).SetValue("Cashier");
                ws.Cell("D" + row).SetValue("Reason");
                ws.Cell("E" + row).SetValue("Drawer");
                ws.Range("A" + row + ":E" + row).Style.Font.SetBold(true);
                //Set Border 
                ws.Range(string.Format("A{0}:E{1}", startRow_Store, row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(string.Format("A{0}:E{1}", startRow_Store, row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                #region======Get List Data
                List<NoSaleDetailReportModels> _data = GetData(model, storeId).ToList();
                if (_data.Count > 0)
                {
                    getItemData(ref ws, ref row, _data);
                }

                row += 2;
                #endregion

            }
            //ws.Columns().AdjustToContents();
            return wb;
        }

        public void getItemData(ref IXLWorksheet ws, ref int row, List<NoSaleDetailReportModels> data)
        {

            int startSum = row;
            int startRowSum = row;
            row++;
            foreach (var item in data)
            {
                ws.Cell("A" + (row)).SetValue(item.CreatedDate.ToString("MM/dd/yyyy"));
                ws.Cell("B" + (row)).SetValue(item.CreatedDate.ToString("HH:mm"));
                ws.Cell("C" + (row)).SetValue(item.CashierName);
                ws.Cell("D" + (row)).SetValue(item.Reason);
                ws.Cell("E" + (row)).SetValue(item.DrawerName);
                row++;
            }

            //Get Summary
            ws.Range(string.Format("G{0}:H{0}", startRowSum)).Merge().SetValue("Summary");
            ws.Range(string.Format("G{0}:H{0}", startRowSum)).Style.Font.SetBold(true);
            ws.Range(string.Format("G{0}:H{0}", startRowSum)).Style.Font.SetFontSize(16);
            ws.Range(string.Format("G{0}:H{0}", startRowSum)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range(string.Format("G{0}:H{0}", startRowSum)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));

            startRowSum++;
            ws.Cell("G" + (startRowSum)).SetValue("Date");
            ws.Cell("H" + (startRowSum)).SetValue("Total Count");
            ws.Range(string.Format("G{0}:H{0}", startRowSum)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            startRowSum++;
            var noSaleSum = data.GroupBy(x => x.CreatedDate.Date).Select(x => new { date = x.Key, totalCount = x.Count() }).ToList();
            foreach (var item in noSaleSum)
            {
                ws.Cell("G" + (startRowSum)).SetValue(item.date.ToString("MM/dd/yyyy"));
                ws.Cell("H" + (startRowSum)).SetValue(item.totalCount);
                ws.Range(string.Format("G{0}:H{0}", startRowSum)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                startRowSum++;
            }

            startRowSum--;
            //Set Border 
            ws.Range("A" + startSum + ":E" + (row - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A" + startSum + ":E" + (row - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            ws.Range("G" + startSum + ":H" + (startRowSum)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("G" + startSum + ":H" + (startRowSum)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        public List<NoSaleDetailReportModels> GetData_New(List<string> listStoreId, List<string> lstBusinessInputIds, int mode)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_NoSaleDetailReport
                               where listStoreId.Contains(tb.StoreId)
                                     && tb.Mode == mode
                                     && lstBusinessInputIds.Contains(tb.BusinessId)
                               orderby tb.CreatedDate
                               select new NoSaleDetailReportModels
                               {
                                   CashierId = tb.CashierId,
                                   CashierName = tb.CashierName,
                                   CreatedDate = tb.CreatedDate,
                                   DrawerId = tb.DrawerId,
                                   DrawerName = tb.DrawerName,
                                   Reason = tb.Reason,
                                   BusinessId = tb.BusinessId,
                                   ShiftId = tb.ShiftId,
                                   StartedShift = tb.StartedShift,
                                   ClosedShift = tb.ClosedShift
                               }).Distinct().ToList();
                return lstData;
            }
        }

        public XLWorkbook ExportExcel_New(BaseReportModel model, List<StoreModels> lstStore)
        {
            string sheetName = "No_Sale_Detail_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            string titleReport = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No Sale Report");
            CreateReportHeaderNew(ws, 8, model.FromDate, model.ToDate, titleReport.ToUpper());


            // Get list business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            // Get data use business day
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                // Format header report
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }
            //model.FromDate = _lstBusDayAllStore.Min(oo => oo.DateFrom);
            //model.ToDate = _lstBusDayAllStore.Max(oo => oo.DateTo);
            var _lstBusDayIdAllStore = _lstBusDayAllStore.Select(ss => ss.Id).ToList();

            // Get list data
            var listData = GetData_New(model.ListStores, _lstBusDayIdAllStore, model.Mode);
            if (listData == null || !listData.Any())
            {
                // Format header report
                ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                return wb;
            }

            int row = 5;

            string storeId = string.Empty, storeName = string.Empty;

            // Column Title
            ws.Cell("A" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store"));
            ws.Cell("B" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Business Day"));
            ws.Cell("C" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Shift"));
            ws.Cell("D" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Drawer"));
            ws.Cell("E" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time"));
            ws.Cell("F" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Staff"));
            ws.Cell("G" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Reason"));
            ws.Cell("H" + row).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
            ws.Range("A" + row + ":H" + row).Style.Font.SetBold(true);
            ws.Range("A" + row + ":H" + row).Style.Fill.BackgroundColor = XLColor.FromHtml(Commons.BgColorDataRow);
            row++;

            List<NoSaleDetailReportModels> listDataStore = new List<NoSaleDetailReportModels>();

            for (int i = 0; i < lstStore.Count; i++)
            {
                listDataStore = new List<NoSaleDetailReportModels>();

                storeId = lstStore[i].Id;
                storeName = lstStore[i].Name;

                #region
                // List Id of business day in a store
                var businessInStoreId = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).Select(ss => ss.Id).ToList();
                // Get list data depend on list business day in store
                if (businessInStoreId != null && businessInStoreId.Any())
                {
                    listDataStore = listData.Where(ww => businessInStoreId.Contains(ww.BusinessId)).ToList();
                }

                if (listDataStore != null && listDataStore.Any())
                {
                    int startRowStore = row;
                    ws.Cell("A" + row).SetValue(storeName);

                    var businessInLstData = listDataStore.Select(s => s.BusinessId).Distinct().ToList();
                    var businessInfoInLstData = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId && businessInLstData.Contains(ww.Id)).OrderBy(o => o.DateFrom).ToList();

                    int total = 0;
                    int countBusDay = 0;
                    int countShift = 0;
                    int countDrawer = 0;
                    int countTime = 0;
                    int countStaff = 0;

                    foreach (var busDay in businessInfoInLstData)
                    {
                        int startRowBusDay = row;
                        countBusDay = 0;
                        // Set value for column Business day
                        ws.Cell("B" + row).SetValue(busDay.DateFrom.ToString("MM/dd/yyyy HH:mm tt") + " - " + busDay.DateTo.ToString("MM/dd/yyyy HH:mm tt"));

                        var listDataInBusDay = listData.Where(m => m.BusinessId == busDay.Id).OrderBy(m => m.StartedShift).ToList();
                        var listShift = listDataInBusDay
                            .Select(s => new
                            {
                                ShiftId = s.ShiftId,
                                startShift = s.StartedShift.Value,
                                closeShift = s.ClosedShift.Value
                            }).Distinct().ToList();

                        foreach (var shift in listShift)
                        {
                            int startRowShift = row;
                            countShift = 0;
                            // Set value for column Shift
                            ws.Cell("C" + row).SetValue(shift.startShift.ToString("MM/dd/yyyy HH:mm tt") + " - " + shift.closeShift.ToString("MM/dd/yyyy HH:mm tt"));

                            var listDataInShift = listDataInBusDay.Where(w => w.ShiftId == shift.ShiftId).OrderBy(w => w.DrawerName).ToList();
                            var listDrawer = listDataInShift.Select(s => new { DrawerId = s.DrawerId, DrawerName = s.DrawerName }).Distinct().ToList();

                            foreach (var drawer in listDrawer)
                            {
                                int startRowDrawer = row;
                                countDrawer = 0;
                                // Set value for column Drawer
                                ws.Cell("D" + row).SetValue(drawer.DrawerName);

                                var listDataInDrawer = listDataInShift.Where(w => w.DrawerId == drawer.DrawerId).OrderBy(o => o.CreatedDate).ToList();
                                var listTime = listDataInDrawer.Select(s => new { s.CreatedDate.Hour, s.CreatedDate.Minute }).Distinct().ToList();

                                foreach (var time in listTime)
                                {
                                    int startRowTime = row;
                                    countTime = 0;
                                    // Set value for column Time
                                    ws.Cell("E" + row).SetValue(new DateTime(1, 1, 1, time.Hour, time.Minute, 0).ToString("t"));

                                    var listDataInTime = listDataInDrawer.Where(w => w.CreatedDate.Hour == time.Hour && w.CreatedDate.Minute == time.Minute).OrderBy(w => w.CreatedDate).ToList();
                                    var listStaffID = listDataInTime.Select(s => s.CashierId).Distinct().ToList();

                                    foreach (var staffID in listStaffID)
                                    {
                                        int startRowStaff = row;
                                        countStaff = 0;
                                        // Set value for column Staff
                                        ws.Cell("F" + row).SetValue(listDataInTime.Where(w => w.CashierId == staffID).Select(s => s.CashierName).FirstOrDefault());

                                        var listReason = listDataInTime.Where(w => w.CashierId == staffID).OrderBy(w => w.CreatedDate).Select(o => o.Reason).ToList();
                                        foreach (var reason in listReason)
                                        {
                                            total++;
                                            countBusDay++;
                                            countShift++;
                                            countDrawer++;
                                            countTime++;
                                            countStaff++;
                                            // Set value for column Reason
                                            ws.Cell("G" + row).SetValue(reason);
                                            row++;
                                        }
                                        ws.Range(startRowStaff, 6, startRowStaff + countStaff - 1, 6).Merge();
                                    }
                                    ws.Range(startRowTime, 5, startRowTime + countTime - 1, 5).Merge();
                                }
                                ws.Range(startRowDrawer, 4, startRowDrawer + countDrawer - 1, 4).Merge();
                            }
                            ws.Range(startRowShift, 3, startRowShift + countShift - 1, 3).Merge();
                        }
                        ws.Range(startRowBusDay, 2, startRowBusDay + countBusDay - 1, 2).Merge();
                        // Set value for column Total
                        ws.Cell("H" + startRowBusDay).SetValue(countBusDay);
                        ws.Range(startRowBusDay, 8, startRowBusDay + countBusDay - 1, 8).Merge();
                    }
                    ws.Range(startRowStore, 1, startRowStore + total - 1, 1).Merge();
                }
            }
            #endregion
            // Format Report
            ws.Range(5, 1, row - 1 , 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(5, 1, row - 1, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(1, 1, row - 1, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, row - 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, row - 1, 8).Style.Border.InsideBorderColor = XLColor.FromHtml("#000000");
            ws.Range(1, 1, row - 1, 8).Style.Border.OutsideBorderColor = XLColor.FromHtml("#000000");
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 20;
            ws.Column(1).Style.Alignment.WrapText = true;
            ws.Column(2).Width = 40;
            ws.Column(3).Width = 40;
            ws.Column(4).Width = 20;
            ws.Column(5).Width = 10;
            ws.Column(6).Width = 20;
            ws.Column(7).Width = 30;
            return wb;
        }
    }
}
