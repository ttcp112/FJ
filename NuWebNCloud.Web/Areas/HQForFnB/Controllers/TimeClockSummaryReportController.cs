using ClosedXML.Excel;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Utilities;
using Spire.Xls;
using System;
using System.IO;
using System.Web.Mvc;
using System.Linq;
using System.Text;
using NuWebNCloud.Web.App_Start;
using NuWebNCloud.Shared.Models.Reports;
using System.Collections.Generic;
using NLog;
using NuWebNCloud.Web.Controllers;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class TimeClockSummaryReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseReportController _BaseReportController = null;

        public TimeClockSummaryReportController()
        {
            _BaseReportController = new BaseReportController();
        }

        // GET: TimeClockSummaryReport
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

                

                //if (model.ListEmployees.All(x => x.Checked == false) || model.ListEmployees.Count() == 0)
                //{
                //    ModelState.AddModelError("ListEmployees", "Please choose employee.");
                //    return View("Index", model);
                //}
                //else 
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

                /*Editor by Trongntn 10-07-2017*/
                if (model.ListStoreEmpl != null)
                {
                    foreach (var item in model.ListStoreEmpl)
                    {
                        foreach (var itemEmpl in item.ListEmployeesSel)
                        {
                            model.ListEmployees.Add(new RPEmployeeItemModels
                            {
                                Checked = itemEmpl.Checked,
                                ID = itemEmpl.ID,
                                Name = itemEmpl.Name,
                                StoreId = itemEmpl.StoreId,
                                StoreName = itemEmpl.StoreName
                            });
                        }
                    }
                }

                if (model.ListEmployees.All(x => x.Checked == false) || model.ListEmployees.Count() == 0)
                {
                    model.ListEmployees = _BaseReportController.GetListEmployee(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type, true);
                }

                //Get Selected Store
                List<StoreModels> lstStore = new List<StoreModels>();
                //if (model.Type == Commons.TypeCompanySelected) //Company
                //{
                //    lstStore = model.GetSelectedStoreCompany();
                //    model.ListStores = lstStore.Select(ss => ss.Id).ToList();
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
                TimeClockReportFactory factory = new TimeClockReportFactory();
                XLWorkbook wb = factory.ExportExcelSummary(model, lstStore);
                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Time_Clock_Summary_{0}", DateTime.Now.ToString("MMddyyyy"));
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
                _logger.Error("Time Clock Summary Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

       
    }
}