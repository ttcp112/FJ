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
using NuWebNCloud.Shared.Utilities;
using ClosedXML.Excel;

namespace NuWebNCloud.Shared.Factory
{
    public class CashInOutReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public CashInOutReportFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<CashInOutReportModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_CashInOutReport.Where(ww => ww.BusinessDayId == info.BusinessDayId && ww.StoreId == info.StoreId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Cash In Out data exist", lstInfo);
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_CashInOutReport> lstInsert = new List<R_CashInOutReport>();
                        R_CashInOutReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_CashInOutReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.BusinessDayId = item.BusinessDayId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.DrawerId = item.DrawerId;
                            itemInsert.DrawerName = item.DrawerName;
                            itemInsert.CashValue = item.CashValue;
                            itemInsert.StartOn = item.StartOn;
                            itemInsert.EndOn = item.EndOn;
                            itemInsert.UserName = item.UserName;
                            itemInsert.CashType = item.CashType;
                            itemInsert.Mode = item.Mode;
                            itemInsert.ShiftStartOn = item.ShiftStartOn;
                            itemInsert.ShiftEndOn = item.ShiftEndOn;
                            itemInsert.Reason = item.Reason;

                            lstInsert.Add(itemInsert);
                        }

                        cxt.R_CashInOutReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Cash In Out data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert Cash In Out data fail", lstInfo);
                        NSLog.Logger.Error("Insert Cash In Out data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_CashInOutReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<CashInOutReportModels> GetDataReport(BaseReportModel input, int typeCash)
        {
            var lstReturns = new List<CashInOutReportModels>();
            //DateTimeHelper.GetDateTime(ref input);
            var cxt = new NuWebContext();
            var query = from data in cxt.R_CashInOutReport
                            //from store in cxt.G_Store.Where(ww => ww.Id == data.StoreId).DefaultIfEmpty()
                        where data.CreatedDate >= input.FromDate && data.CreatedDate <= input.ToDate
                         && input.ListStores.Contains(data.StoreId) && data.CashType == typeCash
                         && data.Mode == input.Mode
                        orderby data.CreatedDate
                        select new { data };
            lstReturns = query
                .Select(ss => new CashInOutReportModels()
                {
                    StoreId = ss.data.StoreId,
                    //StoreName = ss.store != null ? ss.store.Name : "",
                    CreatedDate = ss.data.CreatedDate,
                    DrawerId = ss.data.DrawerId,
                    DrawerName = ss.data.DrawerName,
                    CashValue = ss.data.CashValue,
                    StartOn = ss.data.StartOn,
                    EndOn = ss.data.EndOn,
                    ShiftStartOn = ss.data.ShiftStartOn,
                    ShiftEndOn = ss.data.ShiftEndOn,
                    UserName = ss.data.UserName,
                    CashType = typeCash,
                    Reason = ss.data.Reason
                }).ToList();

            if(lstReturns != null && lstReturns.Any())
            {
                lstReturns.ForEach(o =>
                {
                    if (!string.IsNullOrEmpty(o.Reason))
                    {
                        var _Split = o.Reason.Split('|').ToList();
                        if (_Split.Count > 0)
                            o.Reason = _Split[0];
                        if(_Split.Count > 1)
                            o.Remark = _Split[1];

                    }
                });
            }

            return lstReturns;
        }


        public XLWorkbook ExportExcelCashIn(BaseReportModel model, List<StoreModels> lstStore, int cashType)
        {
            string sheetName = "Cash_In_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            // Set header report
            CreateReportHeaderNew(ws, 8, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash In Report"));
            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            //Get business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return wb;
            }

            // Get data use business day
            model.FromDate = _lstBusDayAllStore.Min(oo => oo.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(oo => oo.DateTo);

            List<CashInOutReportModels> lstCashModel = GetDataReport(model, cashType);
            if (lstCashModel == null || !lstCashModel.Any())
            {
                return wb;
            }

            ws.Cell("A5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date");
            ws.Cell("B5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time");
            ws.Cell("C5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Business Day");
            ws.Cell("D5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Shift");
            ws.Cell("E5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Drawer");
            ws.Cell("F5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount");
            ws.Cell("G5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Reason");
            ws.Cell("H5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("User");

            //set width column
            ws.Column("A").Width = 15;
            ws.Column("B").Width = 15;
            ws.Column("C").Width = 40;
            ws.Column("D").Width = 40;
            ws.Column("E").Width = 25;
            ws.Column("F").Width = 20;
            ws.Column("G").Width = 35;
            ws.Column("H").Width = 15;
            ws.Range("A" + 5 + ":H" + 5).Style.Font.SetBold(true);
            ws.Range("A" + 5 + ":H" + 5).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
            ws.Range(5, 1, 5 , 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int index = 7;
            DateTime dFrom = DateTime.Now, dTo = DateTime.Now;
            string storeId = string.Empty;

            for (int i = 0; i < lstStore.Count(); i++)
            {
                storeId = lstStore[i].Id;
                var lstBusDayByStore = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                if (lstBusDayByStore != null && lstBusDayByStore.Any())
                {
                    dFrom = lstBusDayByStore.Min(ss => ss.DateFrom);
                    dTo = lstBusDayByStore.Max(ss => ss.DateTo);

                    var lstCashModelByStore = lstCashModel.Where(ww => ww.StoreId == storeId && ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo).ToList();
                    if (lstCashModelByStore != null && lstCashModelByStore.Any())
                    {
                        ws.Range("A" + (index - 1) + ":H" + (index - 1)).Merge().SetValue(lstStore[i].Name);
                        ws.Range("A" + (index - 1) + ":H" + (index - 1)).Style.Font.SetBold(true);
                        ws.Range("A" + (index - 1) + ":H" + (index - 1)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                        for (int j = 0; j < lstCashModelByStore.Count; j++)
                        {
                            ws.Cell("A" + index).Value = "'" + (lstCashModelByStore[j].CreatedDate.ToString("MM/dd/yyyy"));
                            ws.Cell("B" + index).Value = "'" + (lstCashModelByStore[j].CreatedDate.ToString("hh:mm tt"));

                            //Business day
                            ws.Cell("C" + index).Value = lstCashModelByStore[j].StartOn.ToString("MM/dd/yyyy hh:mm tt") + " - " + lstCashModelByStore[j].EndOn.ToString("MM/dd/yyyy hh:mm tt");

                            //Shift day
                            ws.Cell("D" + index).Value = lstCashModelByStore[j].ShiftStartOn.ToString("MM/dd/yyyy hh:mm tt") + " - " + lstCashModelByStore[j].ShiftEndOn.ToString("MM/dd/yyyy hh:mm tt");

                            ws.Cell("E" + index).Value = lstCashModelByStore[j].DrawerName;
                            ws.Cell("F" + index).Value = lstCashModelByStore[j].CashValue.ToString("#,##0.00");
                            ws.Cell("G" + index).Value = lstCashModelByStore[j].Reason;
                            ws.Cell("H" + index).Value = lstCashModelByStore[j].UserName;

                            //set Text Aline
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range("F" + (index) + ":F" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws.Range("G" + (index) + ":H" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            index++;
                        }
                        index++;
                    }
                }

            }//End loop store

            ws.Range("F5" + ":F" + index).Style.NumberFormat.Format = "#,##0.00";
                
            //set Border      
            ws.Range("A5:H" + (index - 2)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A5:H" + (index - 2)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();

            return wb;
        }

        public ClosedXML.Excel.XLWorkbook ExportExcelCashOut(BaseReportModel model, List<StoreModels> lstStore, int cashType)
        {
            string sheetName = "Cash_Out_Report";
            var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            // Set header report
            CreateReportHeaderNew(ws, 9, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cash Out Report"));
            ws.Range(1, 1, 4, 9).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            //Get business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return wb;
            }

            // Get data use business day
            model.FromDate = _lstBusDayAllStore.Min(oo => oo.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(oo => oo.DateTo);

            List<CashInOutReportModels> lstCashModel = GetDataReport(model, cashType);
            if (lstCashModel == null || !lstCashModel.Any())
            {
                return wb;
            }

            ws.Cell("A5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date");
            ws.Cell("B5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time");
            ws.Cell("C5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Business Day");
            ws.Cell("D5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Shift");
            ws.Cell("E5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Drawer");
            ws.Cell("F5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount");
            ws.Cell("G5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Reason");
            ws.Cell("H5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("User");
            ws.Cell("I5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Remark");
            //set width column
            ws.Column("A").Width = 15;
            ws.Column("B").Width = 15;
            ws.Column("C").Width = 40;
            ws.Column("D").Width = 40;
            ws.Column("E").Width = 25;
            ws.Column("F").Width = 20;
            ws.Column("G").Width = 35;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 50;
            ws.Range("A" + 5 + ":I" + 5).Style.Font.SetBold(true);
            ws.Range("A" + 5 + ":I" + 5).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
            ws.Range(5, 1, 5 , 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int index = 7;
            DateTime dFrom = DateTime.Now, dTo = DateTime.Now;
            string storeId = string.Empty;
            for (int i = 0; i < lstStore.Count(); i++)
            {
                storeId = lstStore[i].Id;
                var lstBusDayByStore = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                if (lstBusDayByStore != null && lstBusDayByStore.Any())
                {
                    dFrom = lstBusDayByStore.Min(ss => ss.DateFrom);
                    dTo = lstBusDayByStore.Max(ss => ss.DateTo);

                    var lstCashModelByStore = lstCashModel.Where(ww => ww.StoreId == storeId && ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo).ToList();

                    if (lstCashModelByStore != null && lstCashModelByStore.Any())
                    {
                        ws.Range("A" + (index - 1) + ":I" + (index - 1)).Merge().SetValue(lstStore[i].Name);
                        ws.Range("A" + (index - 1) + ":I" + (index - 1)).Style.Font.SetBold(true);
                        ws.Range("A" + (index - 1) + ":I" + (index - 1)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                        for (int j = 0; j < lstCashModelByStore.Count; j++)
                        {
                            ws.Cell("A" + index).Value = "'" + (lstCashModelByStore[j].CreatedDate.ToString("MM/dd/yyyy"));
                            ws.Cell("B" + index).Value = "'" + (lstCashModelByStore[j].CreatedDate.ToString("hh:mm tt"));

                            //Business day
                            ws.Cell("C" + index).Value = lstCashModelByStore[j].StartOn.ToString("MM/dd/yyyy hh:mm tt") + " - " + lstCashModelByStore[j].EndOn.ToString("MM/dd/yyyy hh:mm tt");

                            //Shift day
                            ws.Cell("D" + index).Value = lstCashModelByStore[j].ShiftStartOn.ToString("MM/dd/yyyy hh:mm tt") + " - " + lstCashModelByStore[j].ShiftEndOn.ToString("MM/dd/yyyy hh:mm tt");

                            ws.Cell("E" + index).Value = lstCashModelByStore[j].DrawerName;
                            ws.Cell("F" + index).Value = lstCashModelByStore[j].CashValue.ToString("#,##0.00");
                            ws.Cell("G" + index).Value = lstCashModelByStore[j].Reason;
                            ws.Cell("H" + index).Value = lstCashModelByStore[j].UserName;
                            ws.Cell("I" + index).Value = lstCashModelByStore[j].Remark;
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range("F" + (index) + ":F" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws.Range("G" + (index) + ":I" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            index++;
                        }

                        index++;
                    }
                }

            }//End loop store

            ws.Range("F5" + ":F" + index).Style.NumberFormat.Format = "#,##0.00";

            //set Border      
            ws.Range("A5:I" + (index - 2)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A5:I" + (index - 2)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();

            return wb;
        }
    }
}
