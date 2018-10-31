using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Web.App_Start;
using NuWebNCloud.Web.Controllers;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using OfficeOpenXml;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class HourlySalesReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        // GET: HourlySalesReport
        public ActionResult Index()
        {
            RPHourlySalesModels model = new RPHourlySalesModels();
            return View(model);
        }

        public ActionResult ReportOld(RPHourlySalesModels model)
        {
            try
            {
                HourlySalesReportFactory factory = new HourlySalesReportFactory();
                if (model.ToHour == new TimeSpan(0, 0, 0))
                    model.ToHour = new TimeSpan(23, 59, 59);

                if (model.FromDate > model.ToDate)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                if (model.FromHour > model.ToHour)
                    ModelState.AddModelError("FromHour", CurrentUser.GetLanguageTextFromKey("From Time must be less than To Time."));
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
                //if (model.Type == Commons.TypeCompanySelected) //Company
                //{
                //    lstStore = model.GetSelectedStoreCompany();
                //}
                //else //Store
                //{
                //    List<SelectListItem> vbStore = ViewBag.Stores;
                //    lstStore = model.GetSelectedStore(vbStore);
                //}
                ///////======= Updated 072018
                if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    lstStore = listStoresInfoSession.Where(ww => model.ListCompanys.Contains(ww.CompanyId)).ToList();
                    model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                }
                else //Store
                {
                    lstStore = listStoresInfoSession.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
                }
                //End Get Selected Store

                XLWorkbook wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Hourly_Sales"/*CurrentUser.GetLanguageTextFromKey("Hourly_Sales")*/);
                bool res = factory.Report(model, ref ws, lstStore);
                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                string sheetName = string.Format("Report_Hourly_Sales_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
                if (model.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.html", sheetName));
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", sheetName));
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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error("Hourly Sales Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        /*Export Excel have Chart*/

        public ActionResult Report_Old20180327(RPHourlySalesModels model)
        {
            try
            {
                /*editor by trongntn 18-01-2017*/
                if (model.ToHour.Hours == 0)
                {
                    model.ToHour = new TimeSpan(23, 59, 59);
                }
                /*end*/
                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromHour.Hours, model.FromHour.Minutes, 0),
                    dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToHour.Hours, model.ToHour.Minutes, 59);

                HourlySalesReportFactory factory = new HourlySalesReportFactory();

                if (dFrom > dTo)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                //if (model.FromHour > model.ToHour)
                //    ModelState.AddModelError("FromHour", "From Time must be less than To Time.");
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
                ////if (CurrentUser.IsMerchantExtend)
                ////{
                ////    lstStore = model.GetSelectedStoreForMerchantExtend(ViewBag.StoresExtend);
                ////    if (model.Type == Commons.TypeCompanySelected)
                ////    {
                ////        model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                ////    }
                ////}
                ////else
                ////{
                ////    if (model.Type == Commons.TypeCompanySelected) //Company
                ////    {
                ////        lstStore = model.GetSelectedStoreCompany();
                ////        model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                ////    }
                ////    else //Store
                ////    {
                ////        lstStore = ViewBag.StoresIncludeCompany;
                ////        if (lstStore != null && lstStore.Any())
                ////        {
                ////            lstStore = lstStore.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
                ////        }
                ////    }
                ////}
                ///////======= Updated 072018
                if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    lstStore = listStoresInfoSession.Where(ww => model.ListCompanys.Contains(ww.CompanyId)).ToList();
                    model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                }
                else //Store
                {
                    lstStore = listStoresInfoSession.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
                }
                //End Get Selected Store
                model.FromDate = dFrom;
                model.ToDate = dTo;

                ExcelPackage wb = null;
                wb = factory.ReportChart_New(model, lstStore);

                // Report with new DB
                //wb = factory.ReportChart_NewDB(model, lstStore);

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                string sheetName = string.Format("Report_Hourly_Sales_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
                if (model.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.html", sheetName));
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", sheetName));
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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error("Hourly Sales Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        #region Updated 03262018, get data from date input & filter from time input
        public ActionResult Report(RPHourlySalesModels model)
        {
            try
            {
                if (model.FromDate > model.ToDate)
                {
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                }
                else if (model.FromDate == model.ToDate)
                {
                    if ((model.FromHour == model.ToHour && model.FromHour.Hours != 0) || (model.FromHour > model.ToHour))
                    {
                        ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                    }
                }

                if (model.Type == Commons.TypeCompanySelected) //Company
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
                {
                    return View("Index", model);
                }

                bool isExtend = false;
                // Get Selected Store
                List<StoreModels> lstStore = new List<StoreModels>();
                //if (CurrentUser.IsMerchantExtend)
                //{
                //    isExtend = true;

                //    lstStore = model.GetSelectedStoreForMerchantExtend(ViewBag.StoresExtend);
                //    if (model.Type == Commons.TypeCompanySelected)
                //    {
                //        model.ListStores = lstStore.Select(ss => ss.Id).ToList();
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
                if (CurrentUser.IsMerchantExtend)
                {
                    isExtend = true;
                }
                ///////======= Updated 072018
                if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    lstStore = listStoresInfoSession.Where(ww => model.ListCompanys.Contains(ww.CompanyId)).ToList();
                    model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                }
                else //Store
                {
                    lstStore = listStoresInfoSession.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
                }
                // End Get Selected Store

                HourlySalesReportFactory factory = new HourlySalesReportFactory();
                ExcelPackage wb = null;
                //current
                wb = factory.ReportChart_NewFilter(model, lstStore);

                //// Report with Credit Note info
                //wb = factory.ReportChart_WithCreditNote(model, lstStore);

                // Report with new DB
                //wb = factory.ReportChart_NewDB(model, lstStore, isExtend);

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                string sheetName = string.Format("Report_Hourly_Sales_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
                if (model.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.html", sheetName));
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", sheetName));
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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error("Hourly Sales Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        #endregion
    }
}