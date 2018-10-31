using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using NuWebNCloud.Web.Controllers;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static NuWebNCloud.Shared.Commons;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class DailySalesReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        // GET: DailySalesReport
        public ActionResult Index()
        {
            DailySalesViewModel model = new DailySalesViewModel();
            return View(model);
        }

        public ActionResult Report_Current(DailySalesViewModel model)
        {
            try
            {
                /*editor by trongntn 18-01-2017*/
                if (model.EndTime.Hours == 0)
                {
                    model.EndTime = new TimeSpan(23, 59, 59);
                }
                /*end*/
                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0)
                    , dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                DailySalesReportFactory factory = new DailySalesReportFactory();
                if (dFrom > dTo)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                else if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    if (model.ListCompanys == null)
                        ModelState.AddModelError("ListCompanys", CurrentUser.GetLanguageTextFromKey("Please choose company."));
                }
                else //Store
                {
                    if (model.ListStores == null)
                        ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                }

                if (!ModelState.IsValid)
                    return View("Index", model);
                //Get Selected Store
                List<StoreModels> lstStore = new List<StoreModels>();
                //List<StoreModels> _lstCompany = ViewBag.CompanyExtend; 
                if (CurrentUser.IsMerchantExtend)
                {
                    if (model.Type == Commons.TypeCompanySelected) //Company
                    {

                        lstStore = model.GetSelectedStoreCompanyForExtend(ViewBag.StoresExtend);
                        model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                    }
                    else //Store
                    {
                        List<StoreModels> vbStore = ViewBag.StoresExtend;
                        lstStore = model.GetSelectedStoreForMerchantExtend(vbStore);
                    }
                }
                else
                {
                    if (model.Type == Commons.TypeCompanySelected) //Company
                    {
                        lstStore = model.GetSelectedStoreCompany();
                        model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                    }
                    else //Store
                    {
                        //List<SelectListItem> vbStore = ViewBag.Stores;
                        lstStore = ViewBag.StoresIncludeCompany;
                        if (lstStore != null && lstStore.Any())
                        {
                            lstStore = lstStore.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
                        }
                    }
                }
                //update 201-01-31
                //Check list company
                bool isMultiCompany = false;
                if (lstStore != null && lstStore.Any())
                {
                    var lstCompanyIds = lstStore.Select(ss => ss.CompanyId).Distinct().ToList();
                    if (lstCompanyIds != null && lstCompanyIds.Count > 1)
                        isMultiCompany = true;
                }

                //End Get Selected Store

                if (model.StartTime.Hours == 0 && model.StartTime.Minutes == 0 && model.EndTime.Hours == 0 && model.EndTime.Minutes == 0)
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
                }
                else
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                }
                //List<DailySalesReportModels> receiptItems = factory.GetDataReceiptItems(model);
                //List<DailySalesReportModels> paymentItems = factory.GetDataPaymentItems(model);

                //Export excel
                //XLWorkbook wb = factory.ExportExcel(model, lstStore);
                XLWorkbook wb = null;

                #region Export with old DB
                if (CurrentUser.IsMerchantExtend)
                {
                    wb = factory.ExportExcel_NewForMerchantExtend(model, lstStore, model.IsIncludeShift);
                }
                else if (isMultiCompany)
                {
                    wb = factory.ExportExcel_NewForMultiCompany(model, lstStore, model.IsIncludeShift);
                }
                else
                    wb = factory.ExportExcel_New(model, lstStore, model.IsIncludeShift);
                #endregion

                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Daily_Sales_{0}", DateTime.Now.ToString("MMddyyyy"));
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                if (model.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.html", sheetName.Replace(" ", "_")));
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", sheetName.Replace(" ", "_")));
                }
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    if (model.FormatExport.Equals(Commons.Html))
                    {
                        Workbook workbook = new Workbook();
                        workbook.LoadFromStream(memoryStream);
                        //convert Excel to HTML
                        Worksheet sheet = workbook.Worksheets[0];
                        using (var ms = new MemoryStream())
                        {
                            sheet.SaveToHtml(ms);
                            ms.WriteTo(HttpContext.Response.OutputStream);
                            ms.Close();
                        }
                    }
                    else
                    {
                        memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    }
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.Error("Daily Sales Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        #region Report new filter time
        public ActionResult Report(DailySalesViewModel model)
        {
            try
            {
                model.FilterType = (int)EFilterType.OnDay;

                if (model.EndTime.Hours == 0 && model.EndTime.Minutes == 0)
                {
                    model.EndTime = new TimeSpan(23, 59, 59);
                    if (model.StartTime.Hours == 0 && model.StartTime.Minutes == 0)
                    {
                        model.FilterType = (int)EFilterType.None;
                    }
                } else if (model.StartTime > model.EndTime)
                {
                    model.FilterType = (int)EFilterType.Days;
                }
                
                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0)
                    , dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 0);
                if (dFrom >= dTo)
                {
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                }
                else
                {
                    dTo = dTo.AddSeconds(59);
                }

                if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    if (model.ListCompanys == null || !model.ListCompanys.Any())
                        ModelState.AddModelError("ListCompanys", CurrentUser.GetLanguageTextFromKey("Please choose company."));
                }
                else //Store
                {
                    if (model.ListStores == null || !model.ListStores.Any())
                        ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                }

                if (!ModelState.IsValid)
                    return View("Index", model);

                //Get Selected Store
                //List<StoreModels> lstStore = new List<StoreModels>();
                //if (CurrentUser.IsMerchantExtend)
                //{
                //    if (model.Type == Commons.TypeCompanySelected) //Company
                //    {

                //        lstStore = model.GetSelectedStoreCompanyForExtend(ViewBag.StoresExtend);
                //        model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                //    }
                //    else //Store
                //    {
                //        List<StoreModels> vbStore = ViewBag.StoresExtend;
                //        lstStore = model.GetSelectedStoreForMerchantExtend(vbStore);
                //    }
                //}
                //else
                //{
                //    if (model.Type == Commons.TypeCompanySelected) //Company
                //    {
                //        lstStore = model.GetSelectedStoreCompany();
                //        model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                //    }
                //    else //Store
                //    {
                //        lstStore = ViewBag.StoresIncludeCompany;
                //        if (lstStore != null && lstStore.Any())
                //        {
                //            lstStore = lstStore.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
                //        }
                //    }
                //}
                //End Get Selected Store

                //////========= Updated 07172018
                List<StoreModels> lstStore = new List<StoreModels>();
                if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    lstStore = listStoresInfoSession.Where(ww => model.ListCompanys.Contains(ww.CompanyId)).ToList();
                    model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                }
                else //Store
                {
                    lstStore = listStoresInfoSession.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
                }

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                model.FromDateFilter = dFrom;
                model.ToDateFilter = dTo;

                //Export excel
                DailySalesReportFactory factory = new DailySalesReportFactory();
                XLWorkbook wb = null;

                #region Export with Credit Note
                bool isExtend = false;
                if (CurrentUser.IsMerchantExtend)
                {
                    isExtend = true;
                }
                wb = factory.ExportExcel_WithCreditNote(model, lstStore, model.IsIncludeShift, isExtend);
                #endregion

                #region Export with new DB
                //Boolean isExtend = false;
                //if (CurrentUser.IsMerchantExtend)
                //{
                //    isExtend = true;
                //}
                //wb = factory.ExportExcel_NewDB(model, lstStore, model.IsIncludeShift, isExtend);
                #endregion
                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Daily_Sales_{0}", DateTime.Now.ToString("MMddyyyy"));
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                if (model.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.html", sheetName.Replace(" ", "_")));
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", sheetName.Replace(" ", "_")));
                }
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    if (model.FormatExport.Equals(Commons.Html))
                    {
                        Workbook workbook = new Workbook();
                        workbook.LoadFromStream(memoryStream);
                        //convert Excel to HTML
                        Worksheet sheet = workbook.Worksheets[0];
                        using (var ms = new MemoryStream())
                        {
                            sheet.SaveToHtml(ms);
                            ms.WriteTo(HttpContext.Response.OutputStream);
                            ms.Close();
                        }
                    }
                    else
                    {
                        memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    }
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.Error("Daily Sales Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        #endregion Report new filter
    }
}