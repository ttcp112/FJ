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
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class ClosedReceiptReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public ClosedReceiptReportFactory factory;
        private BaseReportController _BaseReportController = null;
        public ClosedReceiptReportController()
        {
            factory = new ClosedReceiptReportFactory();
            _BaseReportController = new BaseReportController();
        }

        // GET: ClosedReceiptReport
        public ActionResult Index()
        {
            BaseReportWithTimeMode model = new BaseReportWithTimeMode();
            return View(model);
        }

        public ActionResult Report(BaseReportWithTimeMode model)
        {
            try
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

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                model.FromDateFilter = dFrom;
                model.ToDateFilter = dTo;

                XLWorkbook wb = new XLWorkbook();
                var ws = wb.Worksheets.Add(CurrentUser.GetLanguageTextFromKey("Closed_Receipt_Report"));

                #region Report with Old DB
                factory.Report(model, ref ws, lstStore);
                #endregion Report with Old DB

                #region Report with new DB
                //factory.Report_NewDB(model, ref ws, lstStore);
                #endregion Report with new DB

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = System.Text.Encoding.UTF8.WebName;
                Response.ContentEncoding = System.Text.Encoding.UTF8;

                string sheetName = string.Format("Report_Closed_Receipt_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");

                if (model.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.html", sheetName));
                }
                else if (model.FormatExport.Equals(Commons.PDF))
                {
                    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.pdf", sheetName));
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
                    else if (model.FormatExport.Equals(Commons.PDF))
                    {
                        Workbook workbook = new Workbook();
                        workbook.LoadFromStream(memoryStream);
                        //convert Excel to HTML
                        Worksheet sheet = workbook.Worksheets[0];
                        using (var ms = new MemoryStream())
                        {
                            //sheet.SaveToPdf(ms);
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
                _logger.Error("Closed Receipt Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

    }
}