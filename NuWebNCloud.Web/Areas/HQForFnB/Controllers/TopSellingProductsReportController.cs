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
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class TopSellingProductsReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        // GET: TopSellingProductsReport
        public ActionResult Index()
        {
            RPTopSellingProductsModels model = new RPTopSellingProductsModels();
            return View(model);
        }

        //create 06/04/2018
        public ActionResult Report_v2(RPTopSellingProductsModels model)
        {
            try
            {
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
                    return View("Index", model);
                //Get Selected Store
                List<StoreModels> lstStore = new List<StoreModels>();
                //if (CurrentUser.IsMerchantExtend)
                //{
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
                TopSellingProductsReportFactory factory = new TopSellingProductsReportFactory();
                XLWorkbook wb = null;
                wb = factory.ExportExcel_New_v2(model, lstStore, CurrentUser.CurrencySymbol);
                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Top_" + model.TopSell + "_Selling_Products_{0}", DateTime.Now.ToString("MMddyyyy"));
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
                using (var memoryStream = new MemoryStream())
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
                _logger.Error("Top Selling Products Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Report(RPTopSellingProductsModels model)
        {
            try
            {
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
                    return View("Index", model);
                //Get Selected Store
                List<StoreModels> lstStore = new List<StoreModels>();
                //if (CurrentUser.IsMerchantExtend)
                //{
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
                TopSellingProductsReportFactory factory = new TopSellingProductsReportFactory();
                XLWorkbook wb = null;
                #region Report old DB
                wb = factory.ExportExcel_New(model, lstStore, CurrentUser.CurrencySymbol);
                #endregion
                #region Report New DB
                //wb = factory.ExportExcel_NewDB(model, lstStore, CurrentUser.CurrencySymbol);
                #endregion
                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Top_" + model.TopSell + "_Selling_Products_{0}", DateTime.Now.ToString("MMddyyyy"));
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
                using (var memoryStream = new MemoryStream())
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
                _logger.Error("Top Selling Products Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}