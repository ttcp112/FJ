using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Web.App_Start;
using NuWebNCloud.Web.Controllers;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class DailyItemizedSalesAnalysisDetailReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DailyItemizedSalesReportDetailFactory _dailyItemizedSalesReportDetailFactory = new DailyItemizedSalesReportDetailFactory();
        private StoreFactory _storeFactory = new StoreFactory();
        public ActionResult Index()
        {
            DailyItemizedSalesReportDetailPushDataModels model = new DailyItemizedSalesReportDetailPushDataModels();
            return View(model);
        }

        public ActionResult Report(BaseReportModel model)
        {
            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public ActionResult Report(DailyItemizedSalesReportDetailPushDataModels model)
        {
            model.FilterType = (int)Commons.EFilterType.OnDay;

            if (model.EndTime.Hours == 0 && model.EndTime.Minutes == 0)
            {
                model.EndTime = new TimeSpan(23, 59, 59);
                if (model.StartTime.Hours == 0 && model.StartTime.Minutes == 0)
                {
                    model.FilterType = (int)Commons.EFilterType.None;
                }
            }
            else if (model.StartTime > model.EndTime)
            {
                model.FilterType = (int)Commons.EFilterType.Days;
            }

            DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0)
                , dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 0);
            if (dFrom >= dTo)
            {
                ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
            } else
            {
                dTo = dTo.AddSeconds(59);
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
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return View("Index", model);
            }

            //Get Selected Store
            List<StoreModels> lstStores = new List<StoreModels>();
            ////if (model.Type == Commons.TypeCompanySelected) //Company
            ////{
            ////    lstStores = model.GetSelectedStoreCompany();
            ////    model.ListStores = lstStores.Select(ss => ss.Id).ToList();

            ////}
            ////else //Store
            ////{
            ////    List<SelectListItem> vbStore = ViewBag.Stores;
            ////    lstStores = model.GetSelectedStore(vbStore);
            ////}
            ///////======= Updated 072018
            if (model.Type == Commons.TypeCompanySelected) //Company
            {
                lstStores = listStoresInfoSession.Where(ww => model.ListCompanys.Contains(ww.CompanyId)).ToList();
                model.ListStores = lstStores.Select(ss => ss.Id).ToList();
            }
            else //Store
            {
                lstStores = listStoresInfoSession.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
            }

            model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
            model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
            model.FromDateFilter = dFrom;
            model.ToDateFilter = dTo;

            //Export excel
            DailyItemizedSalesReportDetailFactory factory = new DailyItemizedSalesReportDetailFactory();

            XLWorkbook wb = new XLWorkbook();

            #region Report old DB
            wb = factory.ExportExcel_WithFilterTime(model, lstStores);
            #endregion

            #region Report New DB
            //wb = factory.ExportExcel_NewDB(model, lstStores);
            #endregion

            ViewBag.wb = wb;
            string sheetName = string.Format("Daily_Detail_Itemized_Sales_Analysis_Report_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            if (model.FormatExport.Equals(Commons.Html))
            {
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.html", sheetName));
            }
            else
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", sheetName));
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
    }
}