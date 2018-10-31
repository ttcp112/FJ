using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
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
using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class ReceiptsbyPaymentMethodsReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private StoreFactory _storeFactory = new StoreFactory();
        // GET: ReceiptsbyPaymentMethodsReport
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

                //if (model.ListPaymentMethod == null)
                //{
                //    ModelState.AddModelError("ListPaymentMethod", "Please choose payment method.");
                //}
                //else
                //{
                //    var lstPayments = model.ListPaymentMethod.Select(ss => ss.Checked).ToList();
                //    if (lstPayments == null || lstPayments.Count == 0)
                //    {
                //        ModelState.AddModelError("ListPaymentMethod", "Please choose payment method.");
                //    }
                //}

                /*Editor by Trongntn 10-07-2017*/
                if (model.ListStorePay != null)
                {
                    foreach (var item in model.ListStorePay)
                    {
                        foreach (var itemPay in item.ListPaymentMethodSel)
                        {
                            RFilterChooseExtBaseModel payment = new RFilterChooseExtBaseModel();
                            payment.Checked = itemPay.Checked;
                            payment.Id = itemPay.Id;
                            payment.Name = itemPay.Name;
                            payment.Code = itemPay.Code;
                            payment.StoreId = itemPay.StoreId;
                            payment.StoreName = itemPay.StoreName;
                            payment.ListChilds = itemPay.ListChilds;

                            model.ListPaymentMethod.Add(payment);
                        }
                    }
                }

                if (model.ListPaymentMethod == null || !model.ListPaymentMethod.Any())
                {
                    model.ListPaymentMethod = GetListPaymentMethods(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    model.ListPaymentMethod.ForEach(x =>
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
                }

                if (!ModelState.IsValid)
                    return View("Index", model);
                //Get Selected Store
                List<StoreModels> lstStore = new List<StoreModels>();
                //if (model.Type == Commons.TypeCompanySelected) //Company
                //{
                //    lstStore = model.GetSelectedStoreCompany();
                //    model.ListStores = lstStore.Select(ss => ss.Id).ToList();
                //    //filter Check
                //    var lstStoreId = model.ListPaymentMethod.Where(ww=>ww.Checked).Select(ss => ss.StoreId).Distinct().ToList();
                //    if (lstStoreId != null && lstStoreId.Any())
                //    {
                //        lstStore = lstStore.Where(ww => lstStoreId.Contains(ww.Id)).ToList();
                //        model.ListStores = model.ListStores.Where(ww => lstStoreId.Contains(ww)).ToList();
                //    }
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
                //filter with list payment method
                var lstStoreId = model.ListPaymentMethod.Where(ww => ww.Checked).Select(ss => ss.StoreId).Distinct().ToList();
                if (lstStoreId != null && lstStoreId.Any())
                {
                    lstStore = lstStore.Where(ww => lstStoreId.Contains(ww.Id)).ToList();
                    model.ListStores = model.ListStores.Where(ww => lstStoreId.Contains(ww)).ToList();
                }
                //End Get Selected Store

                ReceiptsbyPaymentMethodsReportFactory factory = new ReceiptsbyPaymentMethodsReportFactory();
                DateTimeHelper.GetDateTime(ref model);

                //Export excel
                #region Report old DB
                XLWorkbook wb = factory.ExportExcel(model, lstStore);

                //Report with Credit Note
                //XLWorkbook wb = factory.ExportExcel_WithCreditNote(model, lstStore);
                #endregion

                #region Report new DB
                //XLWorkbook wb = factory.ExportExcel_NewDB(model, lstStore);
                #endregion

                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Receipts_By_Payment_Method_{0}", DateTime.Now.ToString("MMddyyyy"));
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
                _logger.Error("Receipt By Payment Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstStoreIds"></param>
        /// <param name="typeId">1: Company; 2: Store</param>
        /// <returns></returns>
        public ActionResult GetPaymentMethods(List<string> lstStoreIds, int typeId)
        {
            BaseReportModel model = new BaseReportModel();
         
            model.ListPaymentMethod = GetListPaymentMethods(lstStoreIds, typeId);

            /*Editor by Trongntn 10-07-2017*/
            model.ListStorePay = model.ListPaymentMethod
                                    .GroupBy(x => new { StoreId = x.StoreId, StoreName = x.StoreName })
                                    .Select(x => new StorePay
                                    {
                                        StoreID = x.Key.StoreId,
                                        StoreName = x.Key.StoreName,
                                        ListPaymentMethodSel = new List<RFilterChooseExtBaseModel>()
                                    }).ToList();
            int OffSet = 0;
            model.ListStorePay.ForEach(x =>
            {
                x.OffSet = OffSet++;
                x.ListPaymentMethodSel = model.ListPaymentMethod.Where(z => z.StoreId.Equals(x.StoreID)).ToList();
            });

            return PartialView("_FilterPaymentMethod", model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstStoreIds"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public List<RFilterChooseExtBaseModel> GetListPaymentMethods(List<string> lstStoreIds, int typeId)
        {
            List<RFilterChooseExtBaseModel> result = new List<RFilterChooseExtBaseModel>();
            if (typeId == 1)//company
            {
                //get lst store by company
                var lstCompany = new List<SelectListItem>();
                for (int i = 0; i < lstStoreIds.Count; i++)
                {
                    SelectListItem obj = new SelectListItem() { Value = lstStoreIds[i] };

                    lstCompany.Add(obj);
                }
                var lstStores = _storeFactory.GetListStore(lstCompany);
                lstStoreIds = lstStores.Select(ss => ss.Value).ToList();
            }
            var request = new CategoryApiRequestModel() { ListStoreIds = lstStoreIds };
            ReceiptsbyPaymentMethodsReportFactory factory = new ReceiptsbyPaymentMethodsReportFactory();
            result = factory.GetAllPaymentForReport(request);
            return result;
        }
    }
}