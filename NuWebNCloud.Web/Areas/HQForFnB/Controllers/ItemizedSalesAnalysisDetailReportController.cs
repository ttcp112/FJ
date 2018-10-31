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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class ItemizedSalesAnalysisDetailReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private CategoriesFactory _categoriesFactory = new CategoriesFactory();
        private StoreFactory _storeFactory = new StoreFactory();
        // GET: ItemizedSalesAnalysisReport
        public ActionResult Index()
        {
            ItemizedSalesAnalysisReportModel model = new ItemizedSalesAnalysisReportModel();
            return View(model);
        }

        #region Report old DB
        public ActionResult Report(ItemizedSalesAnalysisReportModel model)
        {
            try
            {
                var _lstCateChecked = new List<RFilterCategoryModel>();
                var _lstSetChecked = new List<RFilterCategoryModel>();

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
                }
                else
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

                if (model.ListStoreCate != null)
                {
                    model.ListCategories.AddRange(model.ListStoreCate.SelectMany(ss => ss.ListCategoriesSel).ToList());
                }

                //// Get list categories
                #region List categories V1, updated 03212018, get all parent & child categories selected
                if (model.ListCategories != null && model.ListCategories.Any())
                {
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, model.ListCategories);
                }
                else
                {
                    var listCategories = GetListCategories(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, listCategories, true);

                }
                #endregion End list categories V1, updated 03212018, get all parent & child categories selected


                if (model.ListStoreSetMenu != null)
                {
                    model.ListSetMenu.AddRange(model.ListStoreSetMenu.SelectMany(ss => ss.ListSetMenuSel).ToList());
                }

                if (model.ListSetMenu != null && model.ListSetMenu.Any())
                {
                    _lstSetChecked = model.ListSetMenu.Where(ww => ww.Checked).ToList();

                    _lstSetChecked.AddRange(model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Any())
                                                                .SelectMany(ss => ss.ListChilds.Where(w => w.Checked).ToList()).ToList());
                }
                else
                {
                    // Select all SetMenu
                    _lstSetChecked = GetListSetMenus(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);

                    _lstSetChecked.AddRange(_lstSetChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Any())
                                                                .SelectMany(ss => ss.ListChilds).ToList());
                }

                if (_lstCateChecked == null)
                    _lstCateChecked = new List<RFilterCategoryModel>();

                if (_lstSetChecked == null)
                    _lstSetChecked = new List<RFilterCategoryModel>();

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View("Index", model);
                }

                var _lstCateCheckedId = _lstCateChecked.Select(ss => ss.Id).ToList();
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                // Get list store selected
                var _lstStoreCate = _lstCateChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoreSet = _lstSetChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoresCateSet = _lstStoreCate.Concat(_lstStoreSet).Distinct().ToList();

                //Get Selected Store
                List<StoreModels> lstStores = new List<StoreModels>();
                //if (model.Type == Commons.TypeCompanySelected) //Company
                //{
                //    lstStores = model.GetSelectedStoreCompany();
                //    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                //    model.ListStores = _lstStoresCateSet;
                //}
                //else //Store
                //{
                //    List<SelectListItem> vbStore = ViewBag.Stores;
                //    lstStores = model.GetSelectedStore(vbStore);
                //    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                //    model.ListStores = _lstStoresCateSet;
                //}
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

                BaseFactory _baseFactory = new BaseFactory();
                ItemizedSalesAnalysisReportDetailFactory factory = new ItemizedSalesAnalysisReportDetailFactory();

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
                {
                    model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                    model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                    var lstBusinessId = _lstBusDayAllStore.Select(ss => ss.Id).ToList();

                    // Get data
                    //#region DATA OF REPORT
                    //var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId, model.Mode);
                    //if (lstItemizeds != null && lstItemizeds.Any())
                    //{
                    //    switch (model.FilterType)
                    //    {
                    //        case (int)Commons.EFilterType.OnDay:
                    //            lstItemizeds = lstItemizeds.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    //            break;
                    //        case (int)Commons.EFilterType.Days:
                    //            lstItemizeds = lstItemizeds.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                    //            break;

                    //    }
                    //}

                    //if(lstItemizeds == null)
                    //{
                    //    lstItemizeds = new List<ItemizedSalesAnalysisReportDetailModels>();
                    //}

                    //DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                    //var listMiscDiscount = miscFactory.GetReceiptDiscountAndMisc(model.FromDate, model.ToDate, model.ListStores, model.Mode, model.FromDateFilter, model.ToDateFilter, model.FilterType);
                    //if (listMiscDiscount == null)
                    //{
                    //    listMiscDiscount = new List<DiscountAndMiscReportModels>();
                    //}
                    //listMiscDiscount.ForEach(ss => ss.DiscountValue = 0);
                    //#endregion

                    //DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
                    //var lstDiscount = discountDetailFactory.GetDiscountTotal(model.ListStores, model.FromDate, model.ToDate, model.Mode, model.FromDateFilter, model.ToDateFilter, model.FilterType);

                    //if (lstDiscount != null && lstDiscount.Any())
                    //{
                    //    listMiscDiscount.AddRange(lstDiscount);
                    //}

                    //Export excel
                    //XLWorkbook wb = factory.ExportExcel(lstItemizeds, model, lstStores, listMiscDiscount);
                    XLWorkbook wb = factory.ExportExcel_New(model, lstStores,  _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId);

                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Detail_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                else
                {
                    //Export excel
                    XLWorkbook wb = factory.ExportExcelEmpty(model);
                    //var ws = wb.Worksheets.Add("Itemized_Sales_Detail_Report");
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Detail_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
            catch (Exception ex)
            {
                _logger.Error("Itemized Sales Analysis Detail Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        #endregion Report old DB

        #region Report new DB
        public ActionResult Report_NewDB(ItemizedSalesAnalysisReportModel model)
        {
            try
            {
                var _lstCateChecked = new List<RFilterCategoryModel>();
                var _lstSetChecked = new List<RFilterCategoryModel>();

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
                }
                else
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

                if (model.ListStoreCate != null)
                {
                    model.ListCategories.AddRange(model.ListStoreCate.SelectMany(ss => ss.ListCategoriesSel).ToList());
                }

                //// Get list categories
                #region List categories V1, updated 03212018, get all parent & child categories selected
                if (model.ListCategories != null && model.ListCategories.Any())
                {
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, model.ListCategories);
                }
                else
                {
                    var listCategories = GetListCategories(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, listCategories, true);

                }
                #endregion End list categories V1, updated 03212018, get all parent & child categories selected


                if (model.ListStoreSetMenu != null)
                {
                    model.ListSetMenu.AddRange(model.ListStoreSetMenu.SelectMany(ss => ss.ListSetMenuSel).ToList());
                }

                if (model.ListSetMenu != null && model.ListSetMenu.Any())
                {
                    _lstSetChecked = model.ListSetMenu.Where(ww => ww.Checked).ToList();

                    _lstSetChecked.AddRange(model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Any())
                                                                .SelectMany(ss => ss.ListChilds.Where(w => w.Checked).ToList()).ToList());
                }
                else
                {
                    // Select all SetMenu
                    _lstSetChecked = GetListSetMenus(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);

                    _lstSetChecked.AddRange(_lstSetChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Any())
                                                                .SelectMany(ss => ss.ListChilds).ToList());
                }

                if (_lstCateChecked == null)
                    _lstCateChecked = new List<RFilterCategoryModel>();

                if (_lstSetChecked == null)
                    _lstSetChecked = new List<RFilterCategoryModel>();

               

                var _lstCateCheckedId = _lstCateChecked.Select(ss => ss.Id).ToList();
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                // Get list store selected
                var _lstStoreCate = _lstCateChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoreSet = _lstSetChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoresCateSet = _lstStoreCate.Concat(_lstStoreSet).Distinct().ToList();

                //Get Selected Store
                List<StoreModels> lstStores = new List<StoreModels>();
                //if (model.Type == Commons.TypeCompanySelected) //Company
                //{
                //    lstStores = model.GetSelectedStoreCompany();
                //    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                //    model.ListStores = _lstStoresCateSet;
                //}
                //else //Store
                //{
                //    List<SelectListItem> vbStore = ViewBag.Stores;
                //    lstStores = model.GetSelectedStore(vbStore);
                //    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                //    model.ListStores = _lstStoresCateSet;
                //}
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

                ItemizedSalesAnalysisReportDetailFactory factory = new ItemizedSalesAnalysisReportDetailFactory();

                //Export excel
                XLWorkbook wb = factory.ExportExcel_NewDB(model, lstStores, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId);

                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Itemized_Sales_Detail_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
            catch (Exception ex)
            {
                _logger.Error("Itemized Sales Analysis Detail Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        #endregion Report new DB

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstStoreIds"></param>
        /// <param name="type">1: company; 2: store</param>
        /// <returns></returns>
        public ActionResult LoadCategories(List<string> lstStoreIds, int typeId = 2)
        {
            ItemizedSalesAnalysisReportModel model = new ItemizedSalesAnalysisReportModel();
            try
            {
                //CategoryApiRequestModel request = new CategoryApiRequestModel();
                //if(typeId == 1)//company
                //{
                //    //get lst store by company
                //    var lstCompany = new List<SelectListItem>();
                //    for (int i = 0; i < lstStoreIds.Count; i++)
                //    {
                //        SelectListItem obj = new SelectListItem() { Value = lstStoreIds[i] };

                //        lstCompany.Add(obj);
                //    }
                //    var lstStores = _storeFactory.GetListStore(lstCompany);
                //    lstStoreIds = lstStores.Select(ss => ss.Value).ToList();
                //}
                //request.ListStoreIds = lstStoreIds;
                //request.Type = (int)Commons.EProductType.Dish;
                //model.ListCategories = _categoriesFactory.GetAllCategoriesForReport(request);

                model.ListCategories = GetListCategories(lstStoreIds, typeId);

                /*Editor by Trongntn 10-07-2017*/
                model.ListStoreCate = model.ListCategories
                                        .GroupBy(x => new { StoreId = x.StoreId, StoreName = x.StoreName })
                                        .Select(x => new StoreCate
                                        {
                                            StoreID = x.Key.StoreId,
                                            StoreName = x.Key.StoreName,
                                            ListCategoriesSel = new List<RFilterCategoryModel>()
                                        }).ToList();
                int OffSet = 0;
                model.ListStoreCate.ForEach(x =>
                {
                    x.OffSet = OffSet++;
                    x.ListCategoriesSel = model.ListCategories.Where(z => z.StoreName.Equals(x.StoreName)).ToList();
                });

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return PartialView("_FilterCategory", model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstStoreIds"></param>
        /// <param name="type">1: company; 2: store</param>
        /// <returns></returns>
        public ActionResult LoadSetMenus(List<string> lstStoreIds, int typeId = 2)
        {
            ItemizedSalesAnalysisReportModel model = new ItemizedSalesAnalysisReportModel();
            try
            {
                //ProductFactory _productFactory = new ProductFactory();
                //CategoryApiRequestModel request = new CategoryApiRequestModel();
                //if (typeId == 1)//company
                //{
                //    //get lst store by company
                //    var lstCompany = new List<SelectListItem>();
                //    for (int i = 0; i < lstStoreIds.Count; i++)
                //    {
                //        SelectListItem obj = new SelectListItem() { Value = lstStoreIds[i] };

                //        lstCompany.Add(obj);
                //    }
                //    var lstStores = _storeFactory.GetListStore(lstCompany);
                //    lstStoreIds = lstStores.Select(ss => ss.Value).ToList();
                //}
                //request.ListStoreIds = lstStoreIds;
                //request.Type = (int)Commons.EProductType.SetMenu;
                //model.ListSetMenu = _productFactory.GetAllSetMenuForReport(request);

                model.ListSetMenu = GetListSetMenus(lstStoreIds, typeId);

                /*Editor by Trongntn 10-07-2017*/
                model.ListStoreSetMenu = model.ListSetMenu
                                        .GroupBy(x => new { StoreId = x.StoreId, StoreName = x.StoreName })
                                        .Select(x => new StoreSetMenu
                                        {
                                            StoreID = x.Key.StoreId,
                                            StoreName = x.Key.StoreName,
                                            ListSetMenuSel = new List<RFilterCategoryModel>()
                                        }).ToList();
                int OffSet = 0;
                model.ListStoreSetMenu.ForEach(x =>
                {
                    x.OffSet = OffSet++;
                    x.ListSetMenuSel = model.ListSetMenu.Where(z => z.StoreName.Equals(x.StoreName)).ToList();
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return PartialView("_FilterSetMenu", model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstStoreIds">lstStoreIds = lstStoreI</param>
        /// <param name="typeId">typeId = Company | Store </param>
        /// <returns></returns>
        public List<RFilterCategoryModel> GetListCategories(List<string> lstStoreIds, int typeId = 2)
        {
            List<RFilterCategoryModel> result = new List<RFilterCategoryModel>();

            CategoryApiRequestModel request = new CategoryApiRequestModel();
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
            request.ListStoreIds = lstStoreIds;
            request.Type = (int)Commons.EProductType.Dish;
            result = _categoriesFactory.GetAllCategoriesForReport(request);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstStoreIds">lstStoreIds = lstStoreI</param>
        /// <param name="typeId">typeId = Company | Store </param>
        /// <returns></returns>
        public List<RFilterCategoryModel> GetListSetMenus(List<string> lstStoreIds, int typeId = 2)
        {
            List<RFilterCategoryModel> result = new List<RFilterCategoryModel>();

            ProductFactory _productFactory = new ProductFactory();
            CategoryApiRequestModel request = new CategoryApiRequestModel();
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
            request.ListStoreIds = lstStoreIds;
            request.Type = (int)Commons.EProductType.SetMenu;
            result = _productFactory.GetAllSetMenuForReport(request);

            return result;
        }

        #region Group categories by perant & child
        public ActionResult LoadCategories_V2(List<string> lstStoreIds, int typeId = 2)
        {
            ItemizedSalesAnalysisReportModel model = new ItemizedSalesAnalysisReportModel();
            try
            {
                model.ListCategories = GetListCategories(lstStoreIds, typeId);

                /*Editor by Trongntn 10-07-2017*/
                model.ListStoreCate = model.ListCategories
                                        .GroupBy(x => new { StoreId = x.StoreId, StoreName = x.StoreName })
                                        .Select(x => new StoreCate
                                        {
                                            StoreID = x.Key.StoreId,
                                            StoreName = x.Key.StoreName,
                                            ListCategoriesSel = new List<RFilterCategoryModel>()
                                        }).ToList();
                int OffSet = 0;
                model.ListStoreCate.ForEach(x =>
                {
                    x.OffSet = OffSet++;
                    x.ListCategoriesSel = model.ListCategories.Where(z => z.StoreName.Equals(x.StoreName)).ToList();
                });

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return PartialView("_FilterCategory_V2", model);
        }

        #endregion
    }
}