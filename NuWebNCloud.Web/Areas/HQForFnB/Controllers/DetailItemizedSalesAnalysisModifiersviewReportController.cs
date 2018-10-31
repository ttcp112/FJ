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
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class DetailItemizedSalesAnalysisModifiersviewReportController : BaseController
    {
        DetailItemizedSalesAnalysisModifiersviewReportFactory _factory = new DetailItemizedSalesAnalysisModifiersviewReportFactory();
        private CategoriesFactory _categoriesFactory = new CategoriesFactory();
        private StoreFactory _storeFactory = new StoreFactory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public DetailItemizedSalesAnalysisModifiersviewReportController()
        {
            _factory = new DetailItemizedSalesAnalysisModifiersviewReportFactory();

        }
        // GET: HQForFnB/DetailItemizedSalesAnalysisModifiersviewReport
        public ActionResult Index()
        {
            DetailItemizedSalesAnalysisModifiersviewModels model = new DetailItemizedSalesAnalysisModifiersviewModels();
            return View(model);
        }
        public ActionResult Report(DetailItemizedSalesAnalysisModifiersviewModels model)
        {
            {
                try
                {
                    var _lstCateChecked = new List<RFilterCategoryModel>();
                    List<Modifier> ListModifierChoose = new List<Modifier>();

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
                    if (model.ListCategories != null && model.ListCategories.Any())
                    {
                        _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, model.ListCategories);
                    }
                    else
                    {
                        var listCategories = GetListCategories(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                        _categoriesFactory.GetCategoryCheck_V1(ref _lstCateChecked, listCategories, true);

                    }

                    if (model.ListStoreModifier != null)
                    {
                        ListModifierChoose = new List<Modifier>();
                        Modifier obj = null;
                        model.ListStoreModifier.ForEach(x =>
                        {
                            x.ListModifierSel = x.ListModifierSel.Where(y => y.Checked == true).ToList();
                            x.ListModifierSel.ForEach(z =>
                            {
                                obj = new Modifier();
                                obj.ModifierId = z.Id;
                                obj.ModifierName = z.Name;
                                obj.StoreId = z.StoreId;
                                ListModifierChoose.Add(obj);
                            });
                        });
                    }

                    if (_lstCateChecked == null)
                        _lstCateChecked = new List<RFilterCategoryModel>();

                    if (ListModifierChoose == null)
                        ListModifierChoose = new List<Modifier>();

                    if (!ModelState.IsValid)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return View("Index", model);
                    }
                    var _lstCateCheckedId = _lstCateChecked.Select(ss => ss.Id).ToList();
                    // Get list store selected
                    var _lstStoreCate = _lstCateChecked.Select(ss => ss.StoreId).Distinct().ToList();

                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
                    model.FromDateFilter = dFrom;
                    model.ToDateFilter = dTo;

                    ////create datat to test=====================================================
                    ///list store => list category => list dish
                    CategoryOnStore CategoryOnStore = null;
                    List<CategoryOnStore> ListCatgoryOnstoreChoose = new List<CategoryOnStore>();
                    CategoryOfDish categoryOfDish = null;
                    List<CategoryOfDish> lstCategoryOfDish = new List<CategoryOfDish>();
                    Dish _dish = null;
                    List<Dish> ListDish = new List<Dish>();
                    List<ModifierOnDish> ListModifierOnDish = new List<ModifierOnDish>();
                    ModifierOnDish modifieronDish = null;
                    //List<Dish> LstDish = new List<Dish>();

                    for(int k =0;k < 4; k++)
                    {
                        modifieronDish = new ModifierOnDish();
                        modifieronDish.ModifierId = "ModifierNameId" + k;
                        modifieronDish.ModifierName = "ModifierName" + k;
                        modifieronDish.DishAmount = 9.9884;
                        modifieronDish.DishQuantity = 5.70;
                        ListModifierOnDish.Add(modifieronDish);
                        
                    }
                    for (int z = 0; z < 2; z++)
                    {
                        _dish = new Dish();
                        _dish.DishId = "DishId" + z;
                        _dish.DishName = "DishName" + z;
                        _dish.ListModifierOnDish.AddRange(ListModifierOnDish);
                        ListDish.Add(_dish);
                    }
                    for (int j = 0; j < 2; j++)
                    {                      

                        categoryOfDish = new CategoryOfDish();
                        categoryOfDish.CategoryId = "CategoryId" + j;
                        categoryOfDish.CategoryName = "CategoryName" + j;
                        categoryOfDish.ListDish.AddRange(ListDish);
                        lstCategoryOfDish.Add(categoryOfDish);

                    }
                    for (int i = 0; i < 2; i++)
                    {                               
                        CategoryOnStore = new CategoryOnStore();
                        CategoryOnStore.StoreId = "StoreId" + i;
                        CategoryOnStore.StoreName = "Store Name" + i;
                        CategoryOnStore.ListCategory.AddRange(lstCategoryOfDish);
                        ListCatgoryOnstoreChoose.Add(CategoryOnStore);
                    }
                    


                    ////end create data test=======================================================


                    XLWorkbook wb = _factory.Report(model, ListCatgoryOnstoreChoose, ListModifierChoose);

                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Detail Itemized Sales Analysis Modifiers view_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                    _logger.Error("Report_Detail Itemized Sales Analysis Modifiers view error: " + ex);
                    return new HttpStatusCodeResult(400, ex.Message);
                }
            }
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
        public ActionResult LoadCategories_V2(List<string> lstStoreIds, int typeId = 2)
        {
            DetailItemizedSalesAnalysisModifiersviewModels model = new DetailItemizedSalesAnalysisModifiersviewModels();
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
        public ActionResult LoadModifier(List<string> lstStoreIds, int typeId = 2)
        {
            DetailItemizedSalesAnalysisModifiersviewModels model = new DetailItemizedSalesAnalysisModifiersviewModels();
            try
            {
                model.ListModifier = GetListModifier(lstStoreIds, typeId);

                /*Editor by Trongntn 10-07-2017*/
                model.ListStoreModifier = model.ListModifier
                                        .GroupBy(x => new { StoreId = x.StoreId, StoreName = x.StoreName })
                                        .Select(x => new StoreModifier
                                        {
                                            StoreID = x.Key.StoreId,
                                            StoreName = x.Key.StoreName,
                                            ListModifierSel = new List<RFilterCategoryModel>()
                                        }).ToList();
                int OffSet = 0;
                model.ListStoreModifier.ForEach(x =>
                {
                    x.OffSet = OffSet++;
                    x.ListModifierSel = model.ListModifier.Where(z => z.StoreName.Equals(x.StoreName)).ToList();
                });

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return PartialView("_FilterModifier", model);
        }
        public List<RFilterCategoryModel> GetListModifier(List<string> lstStoreIds, int typeId = 2)
        {
            List<RFilterCategoryModel> data = new List<RFilterCategoryModel>();
            List<RFilterCategoryModel> results = new List<RFilterCategoryModel>();
            CategoryApiRequestModel request = new CategoryApiRequestModel();
            if (typeId == 1)
            {
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
            request.Type = (int)Commons.EProductType.Modifier;
            ProductFactory _factoryPro = new ProductFactory();
            RFilterCategoryModel objt = null;
            var result = _factoryPro.GetListProduct(null, (byte)Commons.EProductType.Modifier, CurrentUser.ListOrganizationId);
            if (result != null && result.Count > 0)
            {
                result = result.Where(x => x.IsActive).ToList();
                foreach (var item in result)
                {
                    objt = new RFilterCategoryModel();
                    objt.Id = item.ID;
                    objt.Name = item.Name;
                    objt.StoreId = item.StoreID;
                    objt.StoreName = item.StoreName;
                    objt.ParentId = item.ParentID;
                    data.Add(objt);
                }
                var lstParentOrNotChild = data.Where(ww => string.IsNullOrEmpty(ww.ParentId)).OrderBy(ww => ww.Seq).ThenBy(aa => aa.Name).ToList();
                var lstChilds = data.Where(ww => !string.IsNullOrEmpty(ww.ParentId)).ToList();
                foreach (var item in lstParentOrNotChild)
                {
                    results.Add(GetCategoryModel(item, lstChilds));
                }
            }



            return results;
        }
        private RFilterCategoryModel GetCategoryModel(RFilterCategoryModel item, List<RFilterCategoryModel> listFull, int count = 0)
        {
            count++;
            var lstGrandChilds = listFull.Where(ww => ww.ParentId == item.Id && ww.StoreId == item.StoreId).OrderBy(ww => ww.Seq).ToList();
            if (lstGrandChilds.Count > 0)
            {
                foreach (var child in lstGrandChilds)
                    item.ListChilds.Add(GetCategoryModel(child, listFull, count));
            }
            return item;
        }

    }
}