using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using Spire.Xls;
using NLog;
using ClosedXML.Excel;
using NuWebNCloud.Web.Controllers;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class CashInReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        // GET: CashInReport
        public ActionResult Index()
        {
            BaseReportModel model = new BaseReportModel();
            return View(model);
        }

        public ActionResult Report(BaseReportModel model)
        {
            try
            {
                if (model.FromDate > model.ToDate)
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
                ////if (model.Type == Commons.TypeCompanySelected) //Company
                ////{
                ////    lstStore = model.GetSelectedStoreCompany();
                ////    model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                ////}
                ////else //Store
                ////{
                ////    List<SelectListItem> vbStore = ViewBag.Stores;
                ////    lstStore = model.GetSelectedStore(vbStore);
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
                //Get data
                var cashInOutReportFactory = new CashInOutReportFactory();
                DateTimeHelper.GetDateTime(ref model);
                //Export excel
                XLWorkbook wb = cashInOutReportFactory.ExportExcelCashIn(model, lstStore, Commons.CashIn);
                ViewBag.wb = wb;

                string sheetName = string.Format("Report_Cash_In_{0}", DateTime.Now.ToString("MMddyyyy"));
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
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
                _logger.Error("Cash In Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }


    }
}