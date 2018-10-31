using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Web.App_Start;
using NuWebNCloud.Web.Controllers;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Linq;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class IngDailyTransactionsReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        // GET: IngDailyTransactionsReport
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
                DateTimeHelper.GetDateTime(ref model);
                IngDailyTransactionsReportFactory factory = new IngDailyTransactionsReportFactory();
                XLWorkbook wb = factory.Report(model, lstStore);
                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Daily_Transactions_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = System.Text.Encoding.UTF8.WebName;
                Response.ContentEncoding = System.Text.Encoding.UTF8;

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

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.Error("Daily Transactions Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}