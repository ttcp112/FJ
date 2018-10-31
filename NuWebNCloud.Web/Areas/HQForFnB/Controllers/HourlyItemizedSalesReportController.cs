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
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models.Api;
using System.Net;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class HourlyItemizedSalesReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private CategoriesFactory _categoriesFactory = new CategoriesFactory();
        private StoreFactory _storeFactory = new StoreFactory();
        private BaseFactory _baseFactory = new BaseFactory();
        // GET: HourlyItemizedSalesReport
        public ActionResult Index()
        {
            RPHourlyItemizedSalesModels model = new RPHourlyItemizedSalesModels();
            return View(model);
        }

        public ActionResult Report_Current(RPHourlyItemizedSalesModels model)
        {
            try
            {
                var _lstCateChecked = new List<RFilterCategoryModel>();
                var _lstSetChecked = new List<RFilterCategoryModel>();

                if (model.ToTime == new TimeSpan(0, 0, 0))
                    model.ToTime = new TimeSpan(23, 59, 59);

                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromTime.Hours, model.FromTime.Minutes, 0)
                        , dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToTime.Hours, model.ToTime.Minutes, 59);

                if (model.FromDate > model.ToDate)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                if (dFrom > dTo)
                    ModelState.AddModelError("FromTime", CurrentUser.GetLanguageTextFromKey("From Time must be less than To Time."));
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

                //ListCategories
                if (model.ListStoreCate != null)
                {
                    foreach (var item in model.ListStoreCate)
                    {
                        model.ListCategories.AddRange(item.ListCategoriesSel);
                    }
                }

                // Get list categories selected
                #region List categories old
                //var lstChildCheck = new List<RFilterCategoryModel>();
                //if (model.ListCategories != null && model.ListCategories.Count > 0)
                //{
                //    _lstCateChecked = model.ListCategories.Where(ww => ww.Checked).ToList();
                //    var lstCateChild = model.ListCategories.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                //    foreach (var item in lstCateChild)
                //    {
                //        //_lstCateChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                //        _categoriesFactory.GetCategoryCheck(ref lstChildCheck, item.ListChilds);
                //    }
                //    _lstCateChecked.AddRange(lstChildCheck);
                //}
                //else
                //{
                //    _lstCateChecked = GetListCategories(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                //    _lstCateChecked.ForEach(x =>
                //    {
                //        x.Checked = true;
                //        if (x.ListChilds != null && x.ListChilds.Count > 0)
                //        {
                //            x.ListChilds.ForEach(z =>
                //            {
                //                z.Checked = true;
                //            });
                //        };
                //    });
                //    //====
                //    var lstCateChild = _lstCateChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                //    foreach (var item in lstCateChild)
                //    {
                //        //_lstCateChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                //        _categoriesFactory.GetCategoryCheck(ref lstChildCheck, item.ListChilds);
                //    }
                //    _lstCateChecked.AddRange(lstChildCheck);
                //}
                #endregion End list categories old

                #region List categories V1, updated 03212018, get all parent & child category selected
                if (model.ListCategories != null && model.ListCategories.Any())
                {
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, model.ListCategories);
                }
                else
                {
                    var listCategories = GetListCategories(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, listCategories, true);

                }
                #endregion End list categories V1

                /*ListSetMenu*/
                if (model.ListStoreSetMenu != null)
                {
                    foreach (var item in model.ListStoreSetMenu)
                    {
                        model.ListSetMenu.AddRange(item.ListSetMenuSel);
                    }
                }
                if (model.ListSetMenu != null && model.ListSetMenu.Count > 0)
                {
                    _lstSetChecked = model.ListSetMenu.Where(ww => ww.Checked).ToList();
                    var lstSetChild = model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstSetChild)
                    {
                        _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                    }
                }
                else
                {
                    _lstSetChecked = GetListSetMenus(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _lstSetChecked.ForEach(x =>
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
                    //=======
                    var lstSetChild = _lstSetChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstSetChild)
                    {
                        _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                    }
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
                // Get list stores selected
                var _lstStoreCate = _lstCateChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoreSet = _lstSetChecked.Select(ss => ss.StoreId).Distinct().ToList();

                // Get ListStores, all stores selected from list cate, list set 
                var lstStoresCateSet = _lstStoreCate.Concat(_lstStoreSet).Distinct().ToList();

                //Get Selected Store
                List<StoreModels> lstStores = new List<StoreModels>();
                //if (model.Type == Commons.TypeCompanySelected) //Company
                //{
                //    lstStores = model.GetSelectedStoreCompany();
                //    lstStores = lstStores.Where(w => lstStoresCateSet.Contains(w.Id)).ToList();
                //    model.ListStores = lstStoresCateSet;
                //    //model.ListStores = lstStores.Select(ss => ss.Id).ToList();
                //}
                //else //Store
                //{
                //    List<SelectListItem> vbStore = ViewBag.Stores;
                //    lstStores = model.GetSelectedStore(vbStore);
                //    lstStores = lstStores.Where(w => lstStoresCateSet.Contains(w.Id)).ToList();
                //    model.ListStores = lstStoresCateSet;
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

                //End Get Selected Store

                HourlyItemizedSalesReportFactory factory = new HourlyItemizedSalesReportFactory();

                DateTime fromDate = model.FromDate;
                DateTime toDate = model.ToDate;
                TimeSpan fromTime = model.FromTime;
                TimeSpan toTime = model.ToTime;

                //model.FromDate = DateTimeHelper.SetFromDate(model.FromDate, fromTime);
                //model.ToDate = DateTimeHelper.SetToDate(model.ToDate, toTime);
                if (model.FromTime.Hours == 0 && model.FromTime.Minutes == 0 && model.ToTime.Hours == 0 && model.ToTime.Minutes == 0)
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
                }
                else
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.FromTime.Hours, model.FromTime.Minutes, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.ToTime.Hours, model.ToTime.Minutes, 59);
                }

                DateTime _dToFilter = model.ToDate;
                

                XLWorkbook wb = new XLWorkbook();
                //Get business day
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    IXLWorksheet ws = wb.Worksheets.Add("Hourly_Itemized_Sales_Report");
                    // Set header report
                    ReportFactory reportFactory = new ReportFactory();
                    reportFactory.CreateReportHeaderNew(ws, 8, model.FromDate, _dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Itemized Sales Report").ToUpper());
                    // Format header report
                    ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                else
                {
                    model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

                    #region Report with old DB
                    // Get list items by category, datetime, storeId
                    var listTemp = factory.GetDataHour(model, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId);

                    // Get list MISC
                    DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                    var listDiscountMisc = miscFactory.GetReceiptDiscountAndMiscByDateTime(model.FromDate, model.ToDate, model.ListStores, model.Mode);

                    // Export data
                    wb = factory.Export(listTemp, model.FromDate, _dToFilter,
                       _lstCateChecked, _lstSetChecked, listDiscountMisc, lstStores, model.Mode, _lstBusDayAllStore, model);
                    #endregion End report with old DB

                    #region Report with new DB
                    // Get list items by category, datetime, storeId
                    //var listTemp = factory.GetDataHour_NewDB(model, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId);

                    // Get list MISC
                    //DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                    //var listDiscountMisc = miscFactory.GetReceiptDiscountAndMiscByDateTime(model.FromDate, model.ToDate, model.ListStores, model.Mode);

                    // Export data 
                    //wb = factory.Export_NewDB(listTemp, model.FromDate, _dToFilter,
                    //   _lstCateChecked, _lstSetChecked, listDiscountMisc, lstStores, model.Mode, _lstBusDayAllStore);
                    #endregion End report with new DB

                }
                string sheetName = string.Format("Report_Hourly_Itemized_Sales_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                //HttpContext.Response.Clear();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                if (model.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", string.Format(@"attachment;filename={0}.html", sheetName));
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", string.Format(@"attachment;filename={0}.xlsx", sheetName));
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
                _logger.Error("Hourly Itemized Sales Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult LoadCategories(List<string> lstStoreIds, int typeId = 2)
        {
            RPHourlyItemizedSalesModels model = new RPHourlyItemizedSalesModels();
            try
            {
                model.ListCategories = GetListCategories(lstStoreIds, typeId);
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

        public ActionResult LoadSetMenus(List<string> lstStoreIds, int typeId = 2)
        {
            RPHourlyItemizedSalesModels model = new RPHourlyItemizedSalesModels();
            try
            {
                model.ListSetMenu = GetListSetMenus(lstStoreIds, typeId);
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

        #region Group categories by parent & child
        public ActionResult LoadCategories_V2(List<string> lstStoreIds, int typeId = 2)
        {
            RPHourlyItemizedSalesModels model = new RPHourlyItemizedSalesModels();
            try
            {
                model.ListCategories = GetListCategories(lstStoreIds, typeId);
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

        #region Updated 03232018, get data from date input & filter from time input
        public ActionResult Report(RPHourlyItemizedSalesModels model)
        {
            try
            {
                if (model.FromDate > model.ToDate)
                {
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                } else if (model.FromDate == model.ToDate)
                {
                    if ((model.FromTime == model.ToTime && model.FromTime.Hours != 0) || (model.FromTime > model.ToTime))
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
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View("Index", model);
                }

                var _lstCateChecked = new List<RFilterCategoryModel>();
                var _lstSetChecked = new List<RFilterCategoryModel>();

                // Get list categories selected
                // List categories V1, updated 03212018, get all parent & child category selected
                if (model.ListStoreCate != null)
                {
                    foreach (var item in model.ListStoreCate)
                    {
                        model.ListCategories.AddRange(item.ListCategoriesSel);
                    }
                }

                if (model.ListCategories != null && model.ListCategories.Any())
                {
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, model.ListCategories);
                }
                else // Select all categories
                {
                    var listCategories = GetListCategories(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, listCategories, true);

                }

                // List SetMenu selected
                if (model.ListStoreSetMenu != null)
                {
                    foreach (var item in model.ListStoreSetMenu)
                    {
                        model.ListSetMenu.AddRange(item.ListSetMenuSel);
                    }
                }
                if (model.ListSetMenu != null && model.ListSetMenu.Any())
                {
                    _lstSetChecked = model.ListSetMenu.Where(ww => ww.Checked).ToList();
                    var lstSetChild = model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).ToList();
                    foreach (var item in lstSetChild)
                    {
                        _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                    }
                }
                else // Select all setmenu
                {
                    _lstSetChecked = GetListSetMenus(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    var lstSetChild = _lstSetChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).ToList();
                    foreach (var item in lstSetChild)
                    {
                        _lstSetChecked.AddRange(item.ListChilds);
                    }
                }

                if (_lstCateChecked == null)
                    _lstCateChecked = new List<RFilterCategoryModel>();
                if (_lstSetChecked == null)
                    _lstSetChecked = new List<RFilterCategoryModel>();

                var _lstCateCheckedId = _lstCateChecked.Select(ss => ss.Id).ToList();
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                // Get list stores selected from categories, setmenu selected
                var _lstStoreCate = _lstCateChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoreSet = _lstSetChecked.Select(ss => ss.StoreId).Distinct().ToList();

                // Get ListStores, all stores selected from list cate, list set 
                var lstStoresCateSet = _lstStoreCate.Concat(_lstStoreSet).Distinct().ToList();

                // Get Selected Store
                List<StoreModels> lstStores = new List<StoreModels>();
                ////if (model.Type == Commons.TypeCompanySelected) // Company
                ////{
                ////    lstStores = model.GetSelectedStoreCompany();
                ////    lstStores = lstStores.Where(w => lstStoresCateSet.Contains(w.Id)).ToList();
                ////    model.ListStores = lstStoresCateSet;
                ////}
                ////else // Store
                ////{
                ////    List<SelectListItem> vbStore = ViewBag.Stores;
                ////    lstStores = model.GetSelectedStore(vbStore);
                ////    lstStores = lstStores.Where(w => lstStoresCateSet.Contains(w.Id)).ToList();
                ////    model.ListStores = lstStoresCateSet;
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

                HourlyItemizedSalesReportFactory factory = new HourlyItemizedSalesReportFactory();
                XLWorkbook wb = new XLWorkbook();

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                DateTime _dFromFilter = model.FromDate; // From date filter
                DateTime _dToFilter = model.ToDate; // To date filter
                int _hFromFilter = model.FromTime.Hours; // From hour filter
                int _hToFilter = model.ToTime.Hours; // To hour filter
                if (_hToFilter == 0) // ToTime == 00 <=> ToTime == 24
                {
                    _hToFilter = 24;
                }

                //Get business day
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    IXLWorksheet ws = wb.Worksheets.Add("Hourly_Itemized_Sales_Report");
                    // Set header report
                    ReportFactory reportFactory = new ReportFactory();
                    reportFactory.CreateReportHeaderNewFilterTime(ws, 8, _dFromFilter, _dToFilter, _hFromFilter, _hToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hourly Itemized Sales Report").ToUpper());
                }
                else
                {
                    model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                    model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);

                    #region Report with old DB
                    // Get list items by category, datetime, storeId
                    var listTemp = factory.GetDataHour(model, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId);

                    // Get list MISC
                    DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                    var listDiscountMisc = miscFactory.GetReceiptDiscountAndMiscByDateTime(model.FromDate, model.ToDate, model.ListStores, model.Mode);

                    // Export data
                    wb = factory.Export_NewFilter(listTemp, _dFromFilter, _dToFilter,
                       _lstCateChecked, _lstSetChecked, listDiscountMisc, lstStores, model.Mode, _lstBusDayAllStore, _hFromFilter, _hToFilter);
                    #endregion End report with old DB

                    #region Report with new DB
                    // Get list items by category, datetime, storeId
                    //var listTemp = factory.GetDataHour_NewDB(model, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId);

                    // Get list MISC
                    //PosSaleFactory posSaleFactory = new PosSaleFactory();
                    //var listDiscountMisc = posSaleFactory.GetReceiptDiscountAndMiscByDateTime(model.FromDate, model.ToDate, model.ListStores, model.Mode);

                    // Export data 
                    //wb = factory.Export_NewDB(listTemp, _dFromFilter, _dToFilter,
                            //_lstCateChecked, _lstSetChecked, listDiscountMisc, lstStores, model.Mode, _lstBusDayAllStore, _hFromFilter, _hToFilter);
                    #endregion End report with new DB

                }
                string sheetName = string.Format("Report_Hourly_Itemized_Sales_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                //HttpContext.Response.Clear();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                if (model.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", string.Format(@"attachment;filename={0}.html", sheetName));
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", string.Format(@"attachment;filename={0}.xlsx", sheetName));
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
                _logger.Error("Hourly Itemized Sales Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        #endregion
    }
}