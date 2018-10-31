using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.ItemizedSalesAnalysisReport;
using NuWebNCloud.Shared;
using ClosedXML.Excel;
using System.IO;
using NuWebNCloud.Web.Areas.HQForFnB.Controllers;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Web.Controllers;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Settings;
using OfficeOpenXml;

namespace NuWebNCloud.Web.Api
{
    public class AutoReportApiController : ApiController
    {
        BaseAutoReportResponseModel resultReport = new BaseAutoReportResponseModel();
        BaseReportModel reportModel = new BaseReportModel();
        DateTime yesterday = DateTime.Now.AddDays(-1);
        List<StoreModels> listStoreInfo = new List<StoreModels>();

        #region Close Receipt report
        public List<BaseAutoReportResponseModel> CloseReceiptReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Close Receipt Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new ClosedReceiptReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);
                
                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                reportModel.ToDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);

                    NSLog.Logger.Info("Start Auto Close Receipt Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        var ws = wb.Worksheets.Add("Sheet1");

                        factory.Report(reportModel, ref ws, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Closed_Receipt_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Close Receipt Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Close Receipt Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Close Receipt Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);

                    NSLog.Logger.Info("Start Auto Close Receipt Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        var ws = wb.Worksheets.Add("Sheet1");

                        factory.Report(reportModel, ref ws, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Closed_Receipt_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Close Receipt Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Close Receipt Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Close Receipt Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);

                    NSLog.Logger.Info("Start Auto Close Receipt Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        var ws = wb.Worksheets.Add("Sheet1");

                        factory.Report(reportModel, ref ws, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Closed_Receipt_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Close Receipt Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Close Receipt Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Close Receipt Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Close Receipt Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Daily Itemized Sales Analysis Detail Report
        public List<BaseAutoReportResponseModel> DailyItemizedSalesAnalysisDetailReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Daily Itemized Sales Analysis Detail Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new DailyItemizedSalesReportDetailFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                reportModel.ToDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                DateTime dToTmp = reportModel.ToDate;
                var lstItemizeds = factory.GetReportDays_v2(reportModel.FromDate, reportModel.ToDate, reportModel.ListStores, reportModel.Mode, ref dToTmp);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Itemized Sales Analysis Detail Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        if (lstItemizeds == null)
                        {
                            IXLWorksheet ws = wb.Worksheets.Add("Daily_Detail_Itemized_Sales");
                            ReportFactory reportFactory = new ReportFactory();
                            reportFactory.CreateReportHeaderNew(ws, 8, reportModel.FromDate, reportModel.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Detail Itemized Sales Analysis Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            wb = factory.ExportExcel(lstItemizeds, reportModel, listStoreInfo, null, dToTmp);
                        }
                        string sheetName = string.Format("Auto_Report_Daily_Itemized_Sales_Analysis_Detail_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Itemized Sales Analysis Detail Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Itemized Sales Analysis Detail Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Daily Itemized Sales Analysis Detail Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Itemized Sales Analysis Detail Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        if (lstItemizeds == null)
                        {
                            IXLWorksheet ws = wb.Worksheets.Add("Daily_Detail_Itemized_Sales");
                            ReportFactory reportFactory = new ReportFactory();
                            reportFactory.CreateReportHeaderNew(ws, 8, reportModel.FromDate, reportModel.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Detail Itemized Sales Analysis Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            wb = factory.ExportExcel(lstItemizeds, reportModel, listStoreInfo, null, dToTmp);
                        }
                        string sheetName = string.Format("Auto_Report_Daily_Itemized_Sales_Analysis_Detail_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Itemized Sales Analysis Detail Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Itemized Sales Analysis Detail Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Daily Itemized Sales Analysis Detail Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Itemized Sales Analysis Detail Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        if (lstItemizeds == null)
                        {
                            IXLWorksheet ws = wb.Worksheets.Add("Daily_Detail_Itemized_Sales");
                            ReportFactory reportFactory = new ReportFactory();
                            reportFactory.CreateReportHeaderNew(ws, 8, reportModel.FromDate, reportModel.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Detail Itemized Sales Analysis Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            wb = factory.ExportExcel(lstItemizeds, reportModel, listStoreInfo, null, dToTmp);
                        }
                        string sheetName = string.Format("Auto_Report_Daily_Itemized_Sales_Analysis_Detail_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Itemized Sales Analysis Detail Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Itemized Sales Analysis Detail Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Daily Itemized Sales Analysis Detail Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Daily Itemized Sales Analysis Detail Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region FJ Daily Sales Report
        public List<BaseAutoReportResponseModel> FJDailySalesReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto FJ Daily Sales Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new FJDailySalesReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                reportModel.ToDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Get setting time 
                //POSMerchantConfigFactory _pOSMerchantConfigFactory = new POSMerchantConfigFactory();
                //MerchantConfigApiModels pOSMerchantConfig = _pOSMerchantConfigFactory.GetTimeSettingForFJDailySale(CurrentUser.HostApi);
                MerchantConfigApiModels pOSMerchantConfig = new MerchantConfigApiModels();
                pOSMerchantConfig.MorningStart = new TimeSpan(7, 0, 0);
                pOSMerchantConfig.MorningEnd = new TimeSpan(11, 0, 0);
                pOSMerchantConfig.MidDayStart = new TimeSpan(11, 0, 0);
                pOSMerchantConfig.MidDayEnd = new TimeSpan(16, 0, 0);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto FJ Daily Sales Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel(reportModel, listStoreInfo, pOSMerchantConfig);
                        string sheetName = string.Format("Auto_Report_FJ_Daily_Sales_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto FJ Daily Sales Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto FJ Daily Sales Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto FJ Daily Sales Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto FJ Daily Sales Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel(reportModel, listStoreInfo, pOSMerchantConfig);
                        string sheetName = string.Format("Auto_Report_FJ_Daily_Sales_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto FJ Daily Sales Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto FJ Daily Sales Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto FJ Daily Sales Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    reportModel.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto FJ Daily Sales Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel(reportModel, listStoreInfo, pOSMerchantConfig);
                        string sheetName = string.Format("Auto_Report_FJ_Daily_Sales_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto FJ Daily Sales Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto FJ Daily Sales Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto FJ Daily Sales Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto FJ Daily Sales Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Audit Trail Report
        public List<BaseAutoReportResponseModel> AuditTrailReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Audit Trail Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new AuditTrailReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Audit Trail Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.Report_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Audit_Trail_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Audit Trail Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Audit Trail Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Audit Trail Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Audit Trail Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.Report_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Audit_Trail_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Audit Trail Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Audit Trail Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Audit Trail Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Audit Trail Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.Report_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Audit_Trail_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Audit Trail Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Audit Trail Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Audit Trail Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Audit Trail Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Cash Out Report
        public List<BaseAutoReportResponseModel> CashOutReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Cash Out Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new CashInOutReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Cash Out Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelCashOut(reportModel, listStoreInfo, Commons.CashOut);
                        string sheetName = string.Format("Auto_Report_Cash_Out_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Cash Out Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Cash Out Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Cash Out Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Cash Out Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelCashOut(reportModel, listStoreInfo, Commons.CashOut);
                        string sheetName = string.Format("Auto_Report_Cash_Out_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Cash Out Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Cash Out Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Cash Out Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Cash Out Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelCashOut(reportModel, listStoreInfo, Commons.CashOut);
                        string sheetName = string.Format("Auto_Report_Cash_Out_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Cash Out Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Cash Out Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Cash Out Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Cash Out Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Cash In Report
        public List<BaseAutoReportResponseModel> CashInReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Cash In Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new CashInOutReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Cash In Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelCashIn(reportModel, listStoreInfo, Commons.CashIn);
                        string sheetName = string.Format("Auto_Report_Cash_In_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Cash In Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Cash In Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Cash In Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Cash In Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelCashIn(reportModel, listStoreInfo, Commons.CashIn);
                        string sheetName = string.Format("Auto_Report_Cash_In_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Cash In Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Cash In Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Cash In Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Cash In Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelCashIn(reportModel, listStoreInfo, Commons.CashIn);
                        string sheetName = string.Format("Auto_Report_Cash_In_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Cash In Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Cash In Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Cash In Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Cash In Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Daily Receipts Report
        public List<BaseAutoReportResponseModel> DailyReceiptReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Daily Receipts Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new DailyReceiptReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Receipts Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Daily_Receipts_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Receipts Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Receipts Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Daily Receipts Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Receipts Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Daily_Receipts_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Receipts Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Receipts Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Daily Receipts Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Receipts Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Daily_Receipts_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Receipts Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Receipts Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Daily Receipts Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Daily Receipts Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Discount Details Report
        public List<BaseAutoReportResponseModel> DiscountDetailsReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Discount Details Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new DiscountDetailsReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Discount Details Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.Report(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Discount_Details_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Discount Details Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Discount Details Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Discount Details Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Discount Details Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.Report(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Discount_Details_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Discount Details Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Discount Details Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Discount Details Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Discount Details Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.Report(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Discount_Details_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Discount Details Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Discount Details Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Discount Details Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Discount Details Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region No Sale Detail Report 
        public List<BaseAutoReportResponseModel> NoSaleDetailReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto No Sale Detail Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new NoSaleDetailReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto No Sale Detail Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_No_Sale_Detail_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto No Sale Detail Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto No Sale Detail Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto No Sale Detail Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto No Sale Detail Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_No_Sale_Detail_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto No Sale Detail Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto No Sale Detail Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto No Sale Detail Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto No Sale Detail Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_No_Sale_Detail_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto No Sale Detail Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto No Sale Detail Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto No Sale Detail Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto No Sale Detail Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Time Clock Summary Report 
        public List<BaseAutoReportResponseModel> TimeClockSummaryReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Time Clock Summary Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new TimeClockReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                // Get list employee
                BaseReportController baseReportController = new BaseReportController();
                reportModel.ListEmployees = baseReportController.GetListEmployee(reportModel.ListStores, 0, true);

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Time Clock Summary Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelSummary(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Time_Clock_Summary_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Time Clock Summary Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Time Clock Summary Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Time Clock Summary Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Time Clock Summary Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelSummary(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Time_Clock_Summary_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Time Clock Summary Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Time Clock Summary Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Time Clock Summary Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Time Clock Summary Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcelSummary(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Time_Clock_Summary_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Time Clock Summary Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Time Clock Summary Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Time Clock Summary Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Time Clock Summary Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Hourly Sales Report
        public List<BaseAutoReportResponseModel> HourlySalesReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Hourly Sales Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new HourlySalesReportFactory();
                ExcelPackage wb = new ExcelPackage();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                RPHourlySalesModels model = new RPHourlySalesModels();
                model.ListStores = RequestModel.ListStoreIds;
                model.Mode = RequestModel.Mode;
                model.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                model.FromHour = new TimeSpan(0, 0, 0);
                model.ToHour = new TimeSpan(23, 59, 59);
                model.FormatExport = "Excel";

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    model.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Hourly Sales Report Daily.......................", model);
                    try
                    {
                        // Export report
                        wb = new ExcelPackage();
                        wb = factory.ReportChart_New(model, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Hourly_Sales_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        Stream stream = File.Create(fullPath);
                        wb.SaveAs(stream);
                        stream.Close();
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Hourly Sales Report Daily.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Hourly Sales Report Daily fail.......................", model);
                        NSLog.Logger.Error("Close Auto Hourly Sales Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Hourly Sales Report Weekly.......................", model);
                    try
                    {
                        // Export report
                        wb = new ExcelPackage();
                        wb = factory.ReportChart_New(model, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Hourly_Sales_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        Stream stream = File.Create(fullPath);
                        wb.SaveAs(stream);
                        stream.Close();
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Hourly Sales Report Weekly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Hourly Sales Report Weekly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Hourly Sales Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Hourly Sales Report Monthly.......................", model);
                    try
                    {
                        // Export report
                        wb = new ExcelPackage();
                        wb = factory.ReportChart_New(model, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Hourly_Sales_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        Stream stream = File.Create(fullPath);
                        wb.SaveAs(stream);
                        stream.Close();
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Hourly Sales Report Monthly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Hourly Sales Report Monthly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Hourly Sales Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Hourly Sales Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Receipts By Payment Methods Report
        public List<BaseAutoReportResponseModel> ReceiptsbyPaymentMethodsReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Receipts By Payment Methods Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new ReceiptsbyPaymentMethodsReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                reportModel = new BaseReportModel();
                reportModel.ListStores = RequestModel.ListStoreIds;
                reportModel.Mode = RequestModel.Mode;
                reportModel.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                // Get list payment method
                ReceiptsbyPaymentMethodsReportController receiptsbyPaymentMethodsReportController = new ReceiptsbyPaymentMethodsReportController();
                reportModel.ListPaymentMethod = receiptsbyPaymentMethodsReportController.GetListPaymentMethods(reportModel.ListStores, 0);
                reportModel.ListPaymentMethod.ForEach(x =>
                {
                    x.Checked = true;
                    if (x.ListChilds != null && x.ListChilds.Any())
                    {
                        x.ListChilds.ForEach(z =>
                        {
                            z.Checked = true;
                        });
                    };
                });

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    reportModel.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Receipts By Payment Methods Report Daily.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Receipts_By_Payment_Methods_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Receipts By Payment Methods Report Daily.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Receipts By Payment Methods Report Daily fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Receipts By Payment Methods Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Receipts By Payment Methods Report Weekly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Receipts_By_Payment_Methods_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Receipts By Payment Methods Report Weekly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Receipts By Payment Methods Report Weekly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Receipts By Payment Methods Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    reportModel.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Receipts By Payment Methods Report Monthly.......................", reportModel);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel(reportModel, listStoreInfo);
                        string sheetName = string.Format("Auto_Report_Receipts_By_Payment_Methods_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Receipts By Payment Methods Report Monthly.......................", reportModel);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Receipts By Payment Methods Report Monthly fail.......................", reportModel);
                        NSLog.Logger.Error("Close Auto Receipts By Payment Methods Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Receipts By Payment Methods Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Hourly Itemized Sales Report
        public List<BaseAutoReportResponseModel> HourlyItemizedSalesReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Hourly Itemized Sales Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new HourlyItemizedSalesReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                RPHourlyItemizedSalesModels model = new RPHourlyItemizedSalesModels();
                model.ListStores = RequestModel.ListStoreIds;
                model.Mode = RequestModel.Mode;
                model.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                model.FromTime = new TimeSpan(0, 0, 0);
                model.ToTime = new TimeSpan(23, 59, 59);
                // Get list category & list set menu
                HourlyItemizedSalesReportController hourlyItemizedSalesReportController = new HourlyItemizedSalesReportController();
                CategoriesFactory _categoriesFactory = new CategoriesFactory();
                var _lstCateChecked = new List<RFilterCategoryModel>();
                var _lstSetChecked = new List<RFilterCategoryModel>();
                var lstChildCheck = new List<RFilterCategoryModel>();
                //// List category
                var listCategories = hourlyItemizedSalesReportController.GetListCategories(model.ListStores, 0);
                if (listCategories != null && listCategories.Any())
                {
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, listCategories, true);
                }

                //// List setmenu
                _lstSetChecked = hourlyItemizedSalesReportController.GetListSetMenus(model.ListStores, 0);
                if (_lstSetChecked != null)
                {
                    if (_lstSetChecked.Any())
                    {
                        _lstSetChecked.ForEach(x =>
                        {
                            x.Checked = true;
                            if (x.ListChilds != null && x.ListChilds.Any())
                            {
                                x.ListChilds.ForEach(z =>
                                {
                                    z.Checked = true;
                                });
                            };
                        });
                        //=======
                        var lstSetChild = _lstSetChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).ToList();
                        lstSetChild.ForEach(item => {
                            _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                        });
                    }
                } else
                {
                    _lstSetChecked = new List<RFilterCategoryModel>();
                }

                DateTime _dToFilter = model.ToDate;
                var _lstCateCheckedId = _lstCateChecked.Select(ss => ss.Id).ToList();
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    model.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Hourly Itemized Sales Report Daily.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            IXLWorksheet ws = wb.Worksheets.Add("Hourly_Itemized_Sales_Report");
                            // Set header report
                            ReportFactory reportFactory = new ReportFactory();
                            reportFactory.CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Itemized Sales Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                            // Get list items by category, datetime, storeId
                            var listTemp = factory.GetDataHour(model, model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId);

                            // Get list MISC
                            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                            var listDiscountMisc = miscFactory.GetReceiptDiscountAndMiscByDateTime(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                            wb = factory.Export(listTemp, model.FromDate, _dToFilter,
                               _lstCateChecked, _lstSetChecked, listDiscountMisc, listStoreInfo, model.Mode, _lstBusDayAllStore, model);
                        }
                        string sheetName = string.Format("Auto_Report_Hourly_Itemized_Sales_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Hourly Itemized Sales Report Daily.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Hourly Itemized Sales Report Daily fail.......................", model);
                        NSLog.Logger.Error("Close Auto Hourly Itemized Sales Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Hourly Itemized Sales Report Weekly.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            IXLWorksheet ws = wb.Worksheets.Add("Hourly_Itemized_Sales_Report");
                            // Set header report
                            ReportFactory reportFactory = new ReportFactory();
                            reportFactory.CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Itemized Sales Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                            // Get list items by category, datetime, storeId
                            var listTemp = factory.GetDataHour(model, model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId);

                            // Get list MISC
                            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                            var listDiscountMisc = miscFactory.GetReceiptDiscountAndMiscByDateTime(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                            wb = factory.Export(listTemp, model.FromDate, _dToFilter,
                               _lstCateChecked, _lstSetChecked, listDiscountMisc, listStoreInfo, model.Mode, _lstBusDayAllStore, model);
                        }
                        string sheetName = string.Format("Auto_Report_Hourly_Itemized_Sales_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Hourly Itemized Sales Report Weekly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Hourly Itemized Sales Report Weekly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Hourly Itemized Sales Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Hourly Itemized Sales Report Monthly.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            IXLWorksheet ws = wb.Worksheets.Add("Hourly_Itemized_Sales_Report");
                            // Set header report
                            ReportFactory reportFactory = new ReportFactory();
                            reportFactory.CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Itemized Sales Report").ToUpper());
                            // Format header report
                            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                            // Get list items by category, datetime, storeId
                            var listTemp = factory.GetDataHour(model, model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId);

                            // Get list MISC
                            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                            var listDiscountMisc = miscFactory.GetReceiptDiscountAndMiscByDateTime(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                            wb = factory.Export(listTemp, model.FromDate, _dToFilter,
                               _lstCateChecked, _lstSetChecked, listDiscountMisc, listStoreInfo, model.Mode, _lstBusDayAllStore, model);
                        }
                        string sheetName = string.Format("Auto_Report_Hourly_Itemized_Sales_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Hourly Itemized Sales Report Monthly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Hourly Itemized Sales Report Monthly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Hourly Itemized Sales Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Hourly Itemized Sales Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Itemized Sales Analysis Detail Report
        public List<BaseAutoReportResponseModel> ItemizedSalesAnalysisDetailReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Itemized Sales Analysis Detail Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new ItemizedSalesAnalysisReportDetailFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                ItemizedSalesAnalysisReportModel model = new ItemizedSalesAnalysisReportModel();
                model.ListStores = RequestModel.ListStoreIds;
                model.Mode = RequestModel.Mode;
                model.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                model.ToDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);

                // Get list category & list set menu
                ItemizedSalesAnalysisDetailReportController itemizedSalesAnalysisDetailReportController = new ItemizedSalesAnalysisDetailReportController();
                CategoriesFactory _categoriesFactory = new CategoriesFactory();
                var _lstCateChecked = new List<RFilterCategoryModel>();
                var _lstSetChecked = new List<RFilterCategoryModel>();

                //// List category
                var listCategories = itemizedSalesAnalysisDetailReportController.GetListCategories(model.ListStores, 0);
                if (listCategories != null && listCategories.Any())
                {
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, listCategories, true);
                }
                //// List setmenu
                _lstSetChecked = itemizedSalesAnalysisDetailReportController.GetListSetMenus(model.ListStores, 0);
                if (_lstSetChecked != null)
                {
                    if (_lstSetChecked.Any())
                    {
                        _lstSetChecked.ForEach(x =>
                        {
                            x.Checked = true;
                            if (x.ListChilds != null && x.ListChilds.Any())
                            {
                                x.ListChilds.ForEach(z =>
                                {
                                    z.Checked = true;
                                });
                            };
                        });
                        //=======
                        var lstSetChild = _lstSetChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).ToList();
                        lstSetChild.ForEach(item => {
                            _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                        });
                    }
                }
                else
                {
                    _lstSetChecked = new List<RFilterCategoryModel>();
                }

                //DateTime _dToFilter = model.ToDate;
                var _lstCateCheckedId = _lstCateChecked.Select(ss => ss.Id).ToList();
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    model.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Itemized Sales Analysis Detail Report Daily.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            wb = factory.ExportExcelEmpty(model);
                        }
                        else
                        {
                            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

                            // Get data for report
                            var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId, model.Mode);
                            if (lstItemizeds != null && lstItemizeds.Any())
                            {
                                lstItemizeds = lstItemizeds.Where(w => w.CreatedDate >= model.FromDateFilter && w.CreatedDate <= model.ToDateFilter).ToList();
                            }
                            else
                            {
                                lstItemizeds = new List<ItemizedSalesAnalysisReportDetailModels>();
                            }
                            //// Misc
                            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                            var listMiscDiscount = miscFactory.GetReceiptDiscountAndMisc(model.FromDate, model.ToDate, model.ListStores, model.Mode, model.FromDateFilter, model.ToDateFilter);
                            if (listMiscDiscount == null)
                            {
                                listMiscDiscount = new List<DiscountAndMiscReportModels>();
                            }
                            listMiscDiscount.ForEach(ss => ss.DiscountValue = 0);
                            //// Discount
                            DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
                            var lstDiscount = discountDetailFactory.GetDiscountTotal(model.ListStores, model.FromDate, model.ToDate, model.Mode, model.FromDateFilter, model.ToDateFilter);
                            if (lstDiscount != null && lstDiscount.Any())
                            {
                                listMiscDiscount.AddRange(lstDiscount);
                            }

                            wb = factory.ExportExcel(lstItemizeds, model, listStoreInfo, listMiscDiscount);
                        }
                        string sheetName = string.Format("Auto_Report_Itemized_Sales_Analysis_Detail_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Detail Report Daily.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Detail Report Daily fail.......................", model);
                        NSLog.Logger.Error("Close Auto Itemized Sales Analysis Detail Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Itemized Sales Analysis Detail Report Weekly.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            wb = factory.ExportExcelEmpty(model);
                        }
                        else
                        {
                            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

                            // Get data for report
                            var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId, model.Mode);
                            if (lstItemizeds != null && lstItemizeds.Any())
                            {
                                lstItemizeds = lstItemizeds.Where(w => w.CreatedDate >= model.FromDateFilter && w.CreatedDate <= model.ToDateFilter).ToList();
                            }
                            else
                            {
                                lstItemizeds = new List<ItemizedSalesAnalysisReportDetailModels>();
                            }
                            //// Misc
                            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                            var listMiscDiscount = miscFactory.GetReceiptDiscountAndMisc(model.FromDate, model.ToDate, model.ListStores, model.Mode, model.FromDateFilter, model.ToDateFilter);
                            if (listMiscDiscount == null)
                            {
                                listMiscDiscount = new List<DiscountAndMiscReportModels>();
                            }
                            listMiscDiscount.ForEach(ss => ss.DiscountValue = 0);
                            //// Discount
                            DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
                            var lstDiscount = discountDetailFactory.GetDiscountTotal(model.ListStores, model.FromDate, model.ToDate, model.Mode, model.FromDateFilter, model.ToDateFilter);
                            if (lstDiscount != null && lstDiscount.Any())
                            {
                                listMiscDiscount.AddRange(lstDiscount);
                            }

                            wb = factory.ExportExcel(lstItemizeds, model, listStoreInfo, listMiscDiscount);
                        }
                        string sheetName = string.Format("Auto_Report_Itemized_Sales_Analysis_Detail_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Detail Report Weekly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Detail Report Weekly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Itemized Sales Analysis Detail Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Itemized Sales Analysis Detail Report Monthly.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            wb = factory.ExportExcelEmpty(model);
                        }
                        else
                        {
                            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

                            // Get data for report
                            var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId, model.Mode);
                            if (lstItemizeds != null && lstItemizeds.Any())
                            {
                                lstItemizeds = lstItemizeds.Where(w => w.CreatedDate >= model.FromDateFilter && w.CreatedDate <= model.ToDateFilter).ToList();
                            }
                            else
                            {
                                lstItemizeds = new List<ItemizedSalesAnalysisReportDetailModels>();
                            }
                            //// Misc
                            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                            var listMiscDiscount = miscFactory.GetReceiptDiscountAndMisc(model.FromDate, model.ToDate, model.ListStores, model.Mode, model.FromDateFilter, model.ToDateFilter);
                            if (listMiscDiscount == null)
                            {
                                listMiscDiscount = new List<DiscountAndMiscReportModels>();
                            }
                            listMiscDiscount.ForEach(ss => ss.DiscountValue = 0);
                            //// Discount
                            DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
                            var lstDiscount = discountDetailFactory.GetDiscountTotal(model.ListStores, model.FromDate, model.ToDate, model.Mode, model.FromDateFilter, model.ToDateFilter);
                            if (lstDiscount != null && lstDiscount.Any())
                            {
                                listMiscDiscount.AddRange(lstDiscount);
                            }

                            wb = factory.ExportExcel(lstItemizeds, model, listStoreInfo, listMiscDiscount);
                        }
                        string sheetName = string.Format("Auto_Report_Itemized_Sales_Analysis_Detail_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Detail Report Monthly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Detail Report Monthly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Itemized Sales Analysis Detail Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Itemized Sales Analysis Detail Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Itemized Sales Analysis Report
        public List<BaseAutoReportResponseModel> ItemizedSalesAnalysisReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Itemized Sales Analysis Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new ItemizedSalesAnalysisReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                ItemizedSalesAnalysisReportModel model = new ItemizedSalesAnalysisReportModel();
                model.ListStores = RequestModel.ListStoreIds;
                model.Mode = RequestModel.Mode;
                model.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                model.ToDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                model.Breakfast = true;
                model.Lunch = true;
                model.Dinner = true;

                // Get list category & list set menu
                ItemizedSalesAnalysisReportController itemizedSalesAnalysisReportController = new ItemizedSalesAnalysisReportController();
                List<RFilterCategoryV1Model> _lstCateChecked = new List<RFilterCategoryV1Model>();
                List<RFilterCategoryV1ReportModel> lstTotalAllCate = new List<RFilterCategoryV1ReportModel>();
                //// List category
                _lstCateChecked = itemizedSalesAnalysisReportController.GetListCategories_V1(model.ListStores, 0);
                _lstCateChecked.ForEach(x =>
                {
                    x.Checked = true;
                });
                //////  Add data to list total cate
                lstTotalAllCate.AddRange(_lstCateChecked.Select(s => new RFilterCategoryV1ReportModel
                {
                    CateId = s.Id,
                    CateName = s.Name,
                    Checked = s.Checked,
                    StoreId = s.StoreId,
                    ParentId = s.ParentId,
                    Level = s.Level,
                    Seq = s.Seq,
                    ListCateChildChecked = _lstCateChecked.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.Id).ToList()
                }));
                var _lstCateCheckedId = _lstCateChecked.Select(s => s.Id).ToList();
                //// List set menu
                List<RFilterCategoryModel> _lstSetChecked = new List<RFilterCategoryModel>();
                List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu = new List<RFilterCategoryV1ReportModel>();
                _lstSetChecked = itemizedSalesAnalysisReportController.GetListSetMenus(model.ListStores, 0);
                _lstSetChecked.ForEach(x =>
                {
                    x.Checked = true;
                    if (x.ListChilds != null && x.ListChilds.Count > 0)
                    {
                        x.ListChilds.ForEach(z =>
                        {
                            z.Checked = true;
                        });
                    };
                });
                //=======
                var lstSetChild = _lstSetChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).ToList();
                foreach (var item in lstSetChild)
                {
                    _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                }
                ////// Add data to list total set menu
                lstTotalAllSetMenu.AddRange(_lstSetChecked.Select(s => new RFilterCategoryV1ReportModel
                {
                    CateId = s.CategoryID,
                    CateName = s.CategoryName,
                    Checked = s.Checked,
                    StoreId = s.StoreId,
                    ParentId = s.ParentId,
                    ListCateChildChecked = _lstSetChecked.Where(w => !string.IsNullOrEmpty(w.ParentId)
                    && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq)
                    .ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                }));
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                ItemizedSalesAnalysisModels viewmodel = new ItemizedSalesAnalysisModels();
                viewmodel.ListItemizedSalesAnalysisOuletTotal = new List<ItemizedSalesAnalysisOuletTotalModels>();
                viewmodel.ItemizedSalesAnalysisTotal = new ItemizedSalesAnalysisTotalModels();
                DateTime _dToFilter = model.ToDate;
                

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    model.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    DateTime _dFromFilter = model.FromDate;
                    NSLog.Logger.Info("Start Auto Itemized Sales Analysis Report Daily.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            wb = factory.ExportExcelEmpty(model);
                        }
                        else
                        {
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                            // Filter payment GC
                            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                            if (lstGC == null)
                                lstGC = new List<string>();
                            // Get data for report
                            var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, listStoreInfo
                                , model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId, model.Mode);

                            wb = factory.ExportExcel_V1(lstItemizeds, model, listStoreInfo, _dToFilter, _dFromFilter, lstGC, _lstCateChecked, _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu);
                        }
                        string sheetName = string.Format("Auto_Report_Itemized_Sales_Analysis_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Report Daily.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Report Daily fail.......................", model);
                        NSLog.Logger.Error("Close Auto Itemized Sales Analysis Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    DateTime _dFromFilter = model.FromDate;
                    NSLog.Logger.Info("Start Auto Itemized Sales Analysis Report Weekly.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            wb = factory.ExportExcelEmpty(model);
                        }
                        else
                        {
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                            // Filter payment GC
                            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                            if (lstGC == null)
                                lstGC = new List<string>();
                            // Get data for report
                            var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, listStoreInfo
                                , model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId, model.Mode);

                            wb = factory.ExportExcel_V1(lstItemizeds, model, listStoreInfo, _dToFilter, _dFromFilter, lstGC, _lstCateChecked, _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu);
                        }
                        string sheetName = string.Format("Auto_Report_Itemized_Sales_Analysis_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Report Weekly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Report Weekly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Itemized Sales Analysis Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    DateTime _dFromFilter = model.FromDate;
                    NSLog.Logger.Info("Start Auto Itemized Sales Analysis Report Monthly.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        // Get business day
                        BaseFactory _baseFactory = new BaseFactory();
                        var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                        if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                        {
                            wb = factory.ExportExcelEmpty(model);
                        }
                        else
                        {
                            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                            // Filter payment GC
                            var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                            var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                            if (lstGC == null)
                                lstGC = new List<string>();
                            // Get data for report
                            var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, listStoreInfo
                                , model.ListStores, model.ListStores, _lstCateCheckedId, _lstSetCheckedId, model.Mode);

                            wb = factory.ExportExcel_V1(lstItemizeds, model, listStoreInfo, _dToFilter, _dFromFilter, lstGC, _lstCateChecked, _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu);
                        }
                        string sheetName = string.Format("Auto_Report_Itemized_Sales_Analysis_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Report Monthly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Itemized Sales Analysis Report Monthly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Itemized Sales Analysis Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Itemized Sales Analysis Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Daily Sales Report
        public List<BaseAutoReportResponseModel> DailySalesReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Daily Sales Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new DailySalesReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                DailySalesViewModel model = new DailySalesViewModel();
                model.ListStores = RequestModel.ListStoreIds;
                model.Mode = RequestModel.Mode;
                model.ToDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                model.ToDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                model.IsIncludeShift = true;

                // Daily report
                if (RequestModel.IsDaily)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    model.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Sales Report Daily.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(model, listStoreInfo, model.IsIncludeShift);
                        string sheetName = string.Format("Auto_Report_Daily_Sales_{0}.xlsx", yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Sales Report Daily.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Sales Report Daily fail.......................", model);
                        NSLog.Logger.Error("Close Auto Daily Sales Report Daily fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Weekly report
                if (RequestModel.IsWeekly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Week");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Sales Report Weekly.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(model, listStoreInfo, model.IsIncludeShift);
                        string sheetName = string.Format("Auto_Report_Daily_Sales_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Sales Report Weekly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Sales Report Weekly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Daily Sales Report Weekly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }

                // Monthly report
                if (RequestModel.IsMonthly)
                {
                    resultReport = new BaseAutoReportResponseModel();
                    // Set input report data
                    DateTime fromDate = GetDateBefore(yesterday, "Month");
                    model.FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    model.FromDateFilter = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    NSLog.Logger.Info("Start Auto Daily Sales Report Monthly.......................", model);
                    try
                    {
                        // Export report
                        wb = new XLWorkbook();
                        wb = factory.ExportExcel_New(model, listStoreInfo, model.IsIncludeShift);
                        string sheetName = string.Format("Auto_Report_Daily_Sales_From_{0}_To_{1}.xlsx", fromDate.ToString("MMddyyyy"), yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                        string fullPath = Commons.ServerExportPath + sheetName;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                        wb.SaveAs(fullPath);
                        // Return data
                        resultReport.IsOk = File.Exists(fullPath);
                        if (resultReport.IsOk)
                        {
                            resultReport.URLPath = fullPath;
                        }
                        NSLog.Logger.Info("Close Auto Daily Sales Report Monthly.......................", model);
                    }
                    catch (Exception e)
                    {
                        NSLog.Logger.Info("Close Auto Daily Sales Report Monthly fail.......................", model);
                        NSLog.Logger.Error("Close Auto Daily Sales Report Monthly fail.......................", e);
                        // Return data
                        resultReport.IsOk = false;
                        resultReport.Message = e.Message;
                    }
                    result.Add(resultReport);
                }
            }

            NSLog.Logger.Info("Close Auto Daily Sales Report.......................", RequestModel);
            return result;
        }
        #endregion

        #region Top Selling Products Report
        public List<BaseAutoReportResponseModel> TopSellingProductsReport(BaseAutoReportRequestModel RequestModel)
        {
            NSLog.Logger.Info("Start Auto Top Selling Products Report.......................", RequestModel);
            List<BaseAutoReportResponseModel> result = new List<BaseAutoReportResponseModel>();

            if ((RequestModel.ListStoreIds != null && RequestModel.ListStoreIds.Any()) && (RequestModel.IsDaily || RequestModel.IsWeekly || RequestModel.IsMonthly))
            {
                var factory = new TopSellingProductsReportFactory();
                XLWorkbook wb = new XLWorkbook();
                // Get stores information
                listStoreInfo = GetStoreInformation(RequestModel.ListStoreIds);

                // Set input report data
                RPTopSellingProductsModels model = new RPTopSellingProductsModels();
                model.ListStores = RequestModel.ListStoreIds;
                model.Mode = RequestModel.Mode;
                model.FromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59);
                model.TopSell = 10;

                // Daily report
                resultReport = new BaseAutoReportResponseModel();
                NSLog.Logger.Info("Start Auto Top Selling Products Report Daily.......................", model);
                try
                {
                    // Get CurrencySymbol
                    string currencySymbol = GetCurrencySymbol();
                    // Export report
                    wb = new XLWorkbook();
                    wb = factory.ExportExcel_New(model, listStoreInfo, currencySymbol);
                    string sheetName = string.Format("Auto_Report_Top_{0}_Selling_Products_{1}.xlsx", model.TopSell, yesterday.ToString("MMddyyyy")).Replace(" ", "_");
                    string fullPath = Commons.ServerExportPath + sheetName;
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                    wb.SaveAs(fullPath);
                    // Return data
                    resultReport.IsOk = File.Exists(fullPath);
                    if (resultReport.IsOk)
                    {
                        resultReport.URLPath = fullPath;
                    }
                    NSLog.Logger.Info("Close Auto Top Selling Products Report Daily.......................", model);
                }
                catch (Exception e)
                {
                    NSLog.Logger.Info("Close Auto Top Selling Products Report Daily fail.......................", model);
                    NSLog.Logger.Error("Close Auto Top Selling Products Report Daily fail.......................", e);
                    // Return data
                    resultReport.IsOk = false;
                    resultReport.Message = e.Message;
                }
                result.Add(resultReport);
            }

            NSLog.Logger.Info("Close Auto Top Selling Products Report.......................", RequestModel);
            return result;
        }
        #endregion

        /// <summary>
        /// Get list store information from list store id
        /// </summary>
        /// <param name="ListStoreIds"></param>
        /// <returns></returns>
        private List<StoreModels> GetStoreInformation(List<string> ListStoreIds)
        {
            List<StoreModels> result = new List<StoreModels>();
            StoreFactory storeFactory = new StoreFactory();
            List<string> listOrganizationId = new List<string>();
            if (System.Web.HttpContext.Current.Session["User"] != null)
            {
                var currentUser =  (UserSession)System.Web.HttpContext.Current.Session["User"];
                listOrganizationId = currentUser.ListOrganizationId;
            }
            var data = storeFactory.GetListStores(null, null, listOrganizationId);
            if (data != null && data.Any())
            {
                var listStores = data.Where(w => ListStoreIds.Contains(w.ID)).ToList();
                if (listStores != null && listStores.Any())
                {
                    listStores.ForEach(store =>
                    {
                        result.Add(new StoreModels
                        {
                            Id = store.ID,
                            Name = store.Name,
                            CompanyId = store.CompanyID,
                            CompanyName = store.CompanyName
                        });
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Get date before from a date
        /// </summary>
        /// <param name="FromDate"></param>
        /// <param name="Type">Type is Week/Month</param>
        /// <returns></returns>
        private DateTime GetDateBefore(DateTime FromDate, string Type)
        {
            DateTime date = new DateTime();
            if (FromDate != null)
            {
                switch (Type)
                {
                    case "Week":
                        FromDate.AddDays(-6);
                        break;
                    case "Month":
                        FromDate.AddDays(-29);
                        break;
                }
            }
            return date;
        }

        /// <summary>
        /// Get currency symbol
        /// </summary>
        /// <returns></returns>
        private string GetCurrencySymbol()
        {
            string symbol = "$";
            return symbol;
        }
    }
}
