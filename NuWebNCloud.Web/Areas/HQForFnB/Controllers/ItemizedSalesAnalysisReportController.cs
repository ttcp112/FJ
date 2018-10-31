using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.ItemizedSalesAnalysisReport;
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
    public class ItemizedSalesAnalysisReportController : BaseController
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

        public ActionResult Report_Old(ItemizedSalesAnalysisReportModel model)
        {
            try
            {
                var _lstCateChecked = new List<RFilterCategoryModel>();
                var _lstSetChecked = new List<RFilterCategoryModel>();
                /*editor by trongntn 18-01-2017*/
                if (model.EndTime.Hours == 0)
                {
                    model.EndTime = new TimeSpan(23, 59, 59);
                }
                /*end*/
                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0)
                    , dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                if (dFrom > dTo)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                else if (!model.Breakfast && !model.Lunch && !model.Dinner)
                    ModelState.AddModelError("Breakfast", CurrentUser.GetLanguageTextFromKey("Please choose period."));

                //else if ((model.ListCategories == null || model.ListCategories.Count == 0) && (model.ListSetMenu == null || model.ListSetMenu.Count == 0))
                //    ModelState.AddModelError("ListCategory", "Please choose category.");

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

                //ListCategories
                if (model.ListCategories != null && model.ListCategories.Count > 0)
                {
                    _lstCateChecked = model.ListCategories.Where(ww => ww.Checked).ToList();
                    var lstCateChild = model.ListCategories.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstCateChild)
                    {
                        _lstCateChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                    }
                }
                else
                {
                    _lstCateChecked = GetListCategories(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _lstCateChecked.ForEach(x =>
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
                    //====
                    var lstCateChild = _lstCateChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstCateChild)
                    {
                        _lstCateChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                    }
                }

                /*ListSetMenu*/
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

                //if (_lstCateChecked.Count ==0 && _lstSetChecked.Count ==0)
                //    ModelState.AddModelError("ListCategoryAndSet", "Please choose category.");

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View("Index", model);
                }
                //List<string> listSelectedIndexes = model.FilterModel.GetSelectedIndex();
                //List<string> listStoreIndexes = model.FilterModel.GetSelectedStoreIndex();

                var _lstCateCheckedId = _lstCateChecked.Select(ss => ss.Id).ToList();
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                // Get list store selected
                var _lstStoreCate = _lstCateChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoreSet = _lstSetChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoresCateSet = _lstStoreCate.Concat(_lstStoreSet).Distinct().ToList();

                //Get Selected Store
                List<StoreModels> lstStores = new List<StoreModels>();
                if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    lstStores = model.GetSelectedStoreCompany();
                    //model.ListStores = lstStores.Select(ss => ss.Id).ToList();
                    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                    model.ListStores = _lstStoresCateSet;
                }
                else //Store
                {
                    List<SelectListItem> vbStore = ViewBag.Stores;
                    lstStores = model.GetSelectedStore(vbStore);
                    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                    model.ListStores = _lstStoresCateSet;
                }
                //End Get Selected Store

                ItemizedSalesAnalysisModels viewmodel = new ItemizedSalesAnalysisModels();
                viewmodel.ListItemizedSalesAnalysisOuletTotal = new List<ItemizedSalesAnalysisOuletTotalModels>();
                viewmodel.ItemizedSalesAnalysisTotal = new ItemizedSalesAnalysisTotalModels();

                //string StoreIndex = string.Join(",", listStoreIndexes);

                #region Check Period (Breakfast, Lunch, Dinner)
                UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                {
                    Commons.BreakfastStart = currentUser.BreakfastStart;
                    Commons.BreakfastEnd = currentUser.BreakfastEnd;
                    Commons.LunchStart = currentUser.LunchStart;
                    Commons.LunchEnd = currentUser.LunchEnd;
                    Commons.DinnerStart = currentUser.DinnerStart;
                    Commons.DinnerEnd = currentUser.DinnerEnd;
                }

                TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
                TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
                TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
                TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
                TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
                TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);

                #endregion
                //StoreFactory storeFactory = new StoreFactory();
                if (model.StartTime.Hours == 0 && model.StartTime.Minutes == 0 && model.EndTime.Hours == 0 && model.EndTime.Minutes == 0)
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
                }
                else
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                }
                BaseFactory _baseFactory = new BaseFactory();
                ItemizedSalesAnalysisReportFactory factory = new ItemizedSalesAnalysisReportFactory();

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore != null && _lstBusDayAllStore.Count > 0)
                {
                    //var _dFrom = _lstBusDayAllStore.Min(ss => ss.DateFrom);
                    //var _dTo = _lstBusDayAllStore.Max(ss => ss.DateTo);


                    //Get list setmenu checked
                    List<string> ListSetMenu = new List<string>();
                    string strSetMenu = string.Join(",", ListSetMenu);

                    #region MISC DISCOUNT TOTAL BILL
                    DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                    //model.FromDate = DateTimeHelper.SetFromDate(model.FromDate, new TimeSpan(7, 0, 0));
                    //model.ToDate = DateTimeHelper.SetToDate(model.ToDate, new TimeSpan(22, 0, 0));
                    // 7 -> 11: Breakfast | 11 -> 17: Lunch | 17 -> 22: Dinner
                    var listMisc_Discout = miscFactory.GetReceiptDiscountAndMisc(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                    if (listMisc_Discout == null)
                    {
                        listMisc_Discout = new List<DiscountAndMiscReportModels>();
                    }
                    listMisc_Discout.ForEach(ss => ss.DiscountValue = 0);

                    double MISCTotal = 0;
                    DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
                    var lstDiscount = discountDetailFactory.GetDiscountTotal(model.ListStores, model.FromDate, model.ToDate);

                    listMisc_Discout.AddRange(lstDiscount);

                    List<MISCBillDiscountPeriodModels> listMiscDisPeriod = new List<MISCBillDiscountPeriodModels>();
                    if (listMisc_Discout != null)
                    {
                        var listMisc_DiscoutGroupStore = listMisc_Discout.GroupBy(gg => gg.StoreId).ToList();
                        foreach (var item in listMisc_DiscoutGroupStore)
                        {
                            var lstItemInStore = item.ToList();
                            #region CHECK PERIOD IS CHECKED

                            if (model.Breakfast)
                            {
                                var miscP = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("BREAKFAST") && p.StoreId == item.Key);
                                if (miscP == null)
                                {
                                    MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                                    itemP.StoreId = item.Key;
                                    itemP.MiscTotal = 0;
                                    itemP.BillDiscountTotal = 0;
                                    itemP.Period = "BREAKFAST";
                                    listMiscDisPeriod.Add(itemP);
                                }
                            }
                            if (model.Lunch)
                            {
                                var miscP = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("LUNCH") && p.StoreId == item.Key);
                                if (miscP == null)
                                {
                                    MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                                    itemP.StoreId = item.Key;
                                    itemP.MiscTotal = 0;
                                    itemP.BillDiscountTotal = 0;
                                    itemP.Period = "LUNCH";
                                    listMiscDisPeriod.Add(itemP);
                                }
                            }
                            if (model.Dinner)
                            {
                                var miscP = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("DINNER") && p.StoreId == item.Key);
                                if (miscP == null)
                                {
                                    MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                                    itemP.StoreId = item.Key;
                                    itemP.MiscTotal = 0;
                                    itemP.BillDiscountTotal = 0;
                                    itemP.Period = "DINNER";
                                    listMiscDisPeriod.Add(itemP);
                                }
                            }
                            #endregion
                            for (int i = 0; i < lstItemInStore.Count; i++)
                            {
                                //Get Total Misc to + ItemTotal
                                MISCTotal += lstItemInStore[i].MiscValue;

                                TimeSpan timeMisc = new TimeSpan(lstItemInStore[i].Hour, 0, 0);
                                //Total period Misc_Discout
                                #region TOTAL PERIOD MISC AND DISCOUNT BILL TOTAL
                                if (model.Breakfast)
                                {
                                    if (timeMisc >= brearkStart && timeMisc < brearkEnd)
                                    {
                                        var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("BREAKFAST") && p.StoreId == item.Key);
                                        period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                        //listMisc_Discout[i].StoreName;
                                        period.MiscTotal += lstItemInStore[i].MiscValue;
                                        period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                    }
                                }
                                if (model.Lunch)
                                {
                                    if (timeMisc >= lunchStart && timeMisc < lunchEnd)
                                    {
                                        var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("LUNCH") && p.StoreId == item.Key);
                                        //period.StoreName = listMisc_Discout[i].StoreName;
                                        period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                        //period.StoreId += listMisc_Discout[i].StoreId;
                                        period.MiscTotal += lstItemInStore[i].MiscValue;
                                        period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                    }
                                }
                                if (model.Dinner)
                                {
                                    if (dinnerStart > dinnerEnd)//pass day
                                    {
                                        if (timeMisc >= dinnerStart || timeMisc < dinnerEnd)
                                        {
                                            var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("DINNER") && p.StoreId == item.Key);
                                            //period.StoreName = listMisc_Discout[i].StoreName;
                                            period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                            period.MiscTotal += lstItemInStore[i].MiscValue;
                                            period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                        }
                                    }
                                    else//in day
                                    {
                                        if (timeMisc >= dinnerStart && timeMisc < dinnerEnd)
                                        {
                                            var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("DINNER") && p.StoreId == item.Key);
                                            //period.StoreName = listMisc_Discout[i].StoreName;
                                            period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                            //period.StoreId += listMisc_Discout[i].StoreId;
                                            period.MiscTotal += lstItemInStore[i].MiscValue;
                                            period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                        }
                                    }

                                }
                                #endregion
                            }
                        }
                        //

                    }
                    #endregion
                    //Get data in DB by store SP_Report_ItemizedSalesAnalysis
                    #region DATA OF REPORT
                    
                    var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, lstStores, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId, model.Mode);
                    if (lstItemizeds != null)
                    {
                        if (lstItemizeds.Count != 0)
                        {
                            var categorys = lstItemizeds.Select(ss => new CategoryModels()
                            {
                                Id = ss.CategoryId,
                                Name = ss.CategoryName
                            }).Distinct().ToList();

                            var totalAll = viewmodel.ItemizedSalesAnalysisTotal;
                            totalAll.ListPeriodTotal = new List<ListPeriodTotalModels>();
                            //
                            #region GET TOTAL
                            for (int i = 0; i < lstItemizeds.Count(); i++)
                            {
                                totalAll.Qty += lstItemizeds[i].Quantity;
                                totalAll.ItemTotal += lstItemizeds[i].ItemTotal;
                                totalAll.Discount += lstItemizeds[i].Discount;
                                totalAll.SCTotal += lstItemizeds[i].ServiceCharge;
                                totalAll.TaxTotal += lstItemizeds[i].Tax;
                                totalAll.UnitCost += lstItemizeds[i].Cost;
                                totalAll.TotalCost += lstItemizeds[i].TotalCost;
                                totalAll.Promotion += lstItemizeds[i].PromotionAmount;
                            }
                            totalAll.ItemTotal += MISCTotal;
                            #endregion
                            //
                            #region GET PERCENT OF ITEM, SUM PERCENT TO GET TOTAL PERCENT, CHECK PERIOD IS CHECKED AND GET TOTAL
                            for (int i = 0; i < lstItemizeds.Count; i++)
                            {
                                //Percent of items
                                if (lstItemizeds[i].ItemTotal != 0 || totalAll.ItemTotal != 0)
                                {
                                    lstItemizeds[i].Percent = lstItemizeds[i].ItemTotal / totalAll.ItemTotal * 100;
                                }
                                else
                                    lstItemizeds[i].Percent = 0;

                                //Sum percent of items to get total percent
                                totalAll.Percent += lstItemizeds[i].Percent; // Get TotalPercent, Sum all percent of items 

                                //Check period is checked
                                #region Check Period (Breakfast, Lunch, Dinner) is checked

                                if (model.Breakfast)
                                {
                                    var period = totalAll.ListPeriodTotal.FirstOrDefault(p => p.Name.Equals("BREAKFAST"));
                                    if (period == null)
                                    {
                                        ListPeriodTotalModels itemP = new ListPeriodTotalModels();
                                        itemP.Qty = 0;
                                        itemP.ItemTotal = 0;
                                        itemP.Percent = 0;
                                        itemP.Discount = 0;
                                        itemP.UnitCost = 0;
                                        itemP.TotalCost = 0;
                                        itemP.CP = 0;
                                        itemP.Name = "BREAKFAST";
                                        itemP.Promotion = 0;
                                        totalAll.ListPeriodTotal.Add(itemP);
                                    }
                                }
                                if (model.Lunch)
                                {
                                    var period = totalAll.ListPeriodTotal.FirstOrDefault(p => p.Name.Equals("LUNCH"));
                                    if (period == null)
                                    {
                                        ListPeriodTotalModels itemP = new ListPeriodTotalModels();
                                        itemP.Qty = 0;
                                        itemP.ItemTotal = 0;
                                        itemP.Percent = 0;
                                        itemP.Discount = 0;
                                        itemP.UnitCost = 0;
                                        itemP.TotalCost = 0;
                                        itemP.CP = 0;
                                        itemP.Name = "LUNCH";
                                        itemP.Promotion = 0;
                                        totalAll.ListPeriodTotal.Add(itemP);
                                    }
                                }
                                if (model.Dinner)
                                {
                                    var period = totalAll.ListPeriodTotal.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                    if (period == null)
                                    {
                                        ListPeriodTotalModels itemP = new ListPeriodTotalModels();
                                        itemP.Qty = 0;
                                        itemP.ItemTotal = 0;
                                        itemP.Percent = 0;
                                        itemP.Discount = 0;
                                        itemP.UnitCost = 0;
                                        itemP.TotalCost = 0;
                                        itemP.CP = 0;
                                        itemP.Name = "DINNER";
                                        itemP.Promotion = 0;
                                        totalAll.ListPeriodTotal.Add(itemP);
                                    }
                                }
                                #endregion

                                //Total period
                                #region Total period
                                TimeSpan timeTotal = lstItemizeds[i].CreatedDate.TimeOfDay;
                                if (model.Breakfast)
                                {
                                    if (timeTotal >= brearkStart && timeTotal < brearkEnd)
                                    {
                                        var period = totalAll.ListPeriodTotal.FirstOrDefault(p => p.Name.Equals("BREAKFAST"));
                                        period.Qty += lstItemizeds[i].Quantity;
                                        period.ItemTotal += lstItemizeds[i].ItemTotal;
                                        period.Percent += lstItemizeds[i].Percent;
                                        period.Discount += lstItemizeds[i].Discount;
                                        period.UnitCost += lstItemizeds[i].Cost;
                                        period.TotalCost += (double)lstItemizeds[i].Quantity * lstItemizeds[i].Cost;
                                        period.Promotion += lstItemizeds[i].PromotionAmount;
                                    }
                                }
                                if (model.Lunch)
                                {
                                    if (timeTotal >= lunchStart && timeTotal < lunchEnd)
                                    {
                                        var period = totalAll.ListPeriodTotal.FirstOrDefault(p => p.Name.Equals("LUNCH"));
                                        period.Qty += lstItemizeds[i].Quantity;
                                        period.ItemTotal += lstItemizeds[i].ItemTotal;
                                        period.Percent += lstItemizeds[i].Percent;
                                        period.Discount += lstItemizeds[i].Discount;
                                        period.UnitCost += lstItemizeds[i].Cost;
                                        period.TotalCost += (double)lstItemizeds[i].Quantity * lstItemizeds[i].Cost;
                                        period.Promotion += lstItemizeds[i].PromotionAmount;
                                    }
                                }
                                if (model.Dinner)
                                {
                                    if (dinnerStart > dinnerEnd)
                                    {
                                        if (timeTotal >= dinnerStart || timeTotal < dinnerEnd)
                                        {
                                            var period = totalAll.ListPeriodTotal.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                            period.Qty += lstItemizeds[i].Quantity;
                                            period.ItemTotal += lstItemizeds[i].ItemTotal;
                                            period.Percent += lstItemizeds[i].Percent;
                                            period.Discount += lstItemizeds[i].Discount;
                                            period.UnitCost += lstItemizeds[i].Cost;
                                            period.TotalCost += (double)lstItemizeds[i].Quantity * lstItemizeds[i].Cost;
                                            period.Promotion += lstItemizeds[i].PromotionAmount;
                                        }
                                    }
                                    else
                                    {
                                        if (timeTotal >= dinnerStart && timeTotal < dinnerEnd)
                                        {
                                            var period = totalAll.ListPeriodTotal.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                            period.Qty += lstItemizeds[i].Quantity;
                                            period.ItemTotal += lstItemizeds[i].ItemTotal;
                                            period.Percent += lstItemizeds[i].Percent;
                                            period.Discount += lstItemizeds[i].Discount;
                                            period.UnitCost += lstItemizeds[i].Cost;
                                            period.TotalCost += (double)lstItemizeds[i].Quantity * lstItemizeds[i].Cost;
                                            period.Promotion += lstItemizeds[i].PromotionAmount;
                                        }
                                    }
                                }
                                #endregion
                            }
                            #endregion
                            //
                            #region GET PERCENT OF MISC ITEMS AND DISCOUNT ITEM BY DISCOUNT TOTAL BILL - GET TOTAL ITEMTOTAL AND GRANDTOTAL
                            double sumDisPeriod = 0;
                            if (listMiscDisPeriod != null)
                            {
                                for (int m = 0; m < listMiscDisPeriod.Count(); m++)
                                {
                                    //Percent of MISC by period
                                    if (listMiscDisPeriod[m].MiscTotal != 0 || totalAll.ItemTotal != 0)
                                    {
                                        listMiscDisPeriod[m].Percent = listMiscDisPeriod[m].MiscTotal / totalAll.ItemTotal * 100;
                                    }
                                    else
                                        listMiscDisPeriod[m].Percent = 0;
                                    totalAll.Percent += listMiscDisPeriod[m].Percent;

                                    // Get discount item in discount total bill by period
                                    //double totalDis = totalAll.ListPeriodTotal.Where(t => t.Name == listMiscDisPeriod[m].Period).Select(t => t.Discount).FirstOrDefault();
                                    listMiscDisPeriod[m].SubDiscount = Math.Abs(listMiscDisPeriod[m].BillDiscountTotal);
                                    //listMiscDisPeriod[m].SubDiscount = Math.Abs(listMiscDisPeriod[m].BillDiscountTotal) - totalDis;
                                    //
                                    sumDisPeriod += listMiscDisPeriod[m].SubDiscount;
                                    totalAll.Discount += listMiscDisPeriod[m].SubDiscount; // Total discount + discount total bill
                                }
                            }

                            //totalAll.ItemTotal -= sumDisPeriod;
                            totalAll.GrandTotal = totalAll.ItemTotal + totalAll.Discount + totalAll.SCTotal;
                            #endregion
                            //
                            #region LIST STORES AND GET TOTAL OUTLET
                            var totalOutlet = viewmodel.ListItemizedSalesAnalysisOuletTotal;
                            for (int i = 0; i < lstItemizeds.Count(); i++)
                            {
                                var outlet = totalOutlet.FirstOrDefault(s => s.StoreName == lstItemizeds[i].StoreName);
                                if (outlet == null)
                                {
                                    outlet = new ItemizedSalesAnalysisOuletTotalModels();
                                    outlet.StoreName = lstItemizeds[i].StoreName;
                                    outlet.Qty = lstItemizeds[i].Quantity;
                                    outlet.ItemTotal = lstItemizeds[i].ItemTotal;
                                    outlet.Percent = lstItemizeds[i].Percent;
                                    outlet.Discount = lstItemizeds[i].Discount;
                                    outlet.UnitCost = lstItemizeds[i].Cost;
                                    outlet.TotalCost = lstItemizeds[i].TotalCost;
                                    outlet.CP = lstItemizeds[i].CP;
                                    outlet.Promotion = lstItemizeds[i].PromotionAmount;
                                    totalOutlet.Add(outlet);
                                }
                                else
                                {
                                    outlet.Qty += lstItemizeds[i].Quantity;
                                    outlet.ItemTotal += lstItemizeds[i].ItemTotal;
                                    outlet.Percent += lstItemizeds[i].Percent;
                                    outlet.Discount += lstItemizeds[i].Discount;
                                    outlet.UnitCost += lstItemizeds[i].Cost;
                                    outlet.TotalCost += lstItemizeds[i].TotalCost;
                                    outlet.CP += lstItemizeds[i].CP;
                                    outlet.Promotion += lstItemizeds[i].PromotionAmount;
                                }
                            }

                            #endregion
                            //
                            #region LIST SUBTOTAL OF CATEGORY AND SUBTOTAL OF SETMENU
                            for (int i = 0; i < totalOutlet.Count(); i++)
                            {
                                //
                                #region LIST SUBTOTAL OF CATEGORY 
                                if (categorys != null)
                                {
                                    if (categorys.Count != 0)
                                    {
                                        totalOutlet[i].ListItemizedSalesAnalysisSubTotal = new List<ItemizedSalesAnalysisSubTotalModels>();
                                        var cateSubtotal = totalOutlet[i].ListItemizedSalesAnalysisSubTotal;

                                        //var listCateOfDish = categorys
                                        /*model.ListCategoryOfDish.Where(d => d.IsChecked == true).ToList()*/
                                        for (int j = 0; j < categorys.Count; j++)
                                        {
                                            var cateofdish = cateSubtotal.FirstOrDefault(s => s.CategoryID == categorys[j].Id && s.StoreName == totalOutlet[i].StoreName
                                            && s.TypeID == Commons.ItemType_Dish);
                                            if (cateofdish == null)
                                            {
                                                cateofdish = new ItemizedSalesAnalysisSubTotalModels();
                                                cateofdish.TypeID = 1;
                                                cateofdish.CategoryName = categorys[j].Name;
                                                cateofdish.CategoryID = categorys[j].Id;
                                                cateofdish.StoreName = totalOutlet[i].StoreName;
                                                cateofdish.Qty = 0;
                                                cateofdish.ItemTotal = 0;
                                                cateofdish.Percent = 0;
                                                cateofdish.Discount = 0;
                                                cateofdish.UnitCost = 0;
                                                cateofdish.TotalCost = 0;
                                                cateofdish.CP = 0;
                                                cateofdish.Promotion = 0;
                                                cateSubtotal.Add(cateofdish);
                                            }
                                        }
                                        var items = lstItemizeds.Where(q => q.ItemTypeId == Commons.ItemType_Dish && q.CategoryId != "" && q.StoreName == totalOutlet[i].StoreName).ToList();
                                        if (items.Count != 0)
                                        {
                                            for (int q = 0; q < items.Count; q++)
                                            {
                                                var cateofdish = cateSubtotal.FirstOrDefault(s => s.CategoryID == items[q].CategoryId && s.StoreName == totalOutlet[i].StoreName
                                                && s.TypeID == Commons.ItemType_Dish);
                                                if (cateofdish != null)
                                                {
                                                    cateofdish.Qty += items[q].Quantity;
                                                    cateofdish.ItemTotal += items[q].ItemTotal;
                                                    cateofdish.Percent += items[q].Percent;
                                                    cateofdish.Discount += items[q].Discount;
                                                    cateofdish.UnitCost += items[q].Cost;
                                                    cateofdish.TotalCost += items[q].TotalCost;
                                                    cateofdish.Promotion += items[q].PromotionAmount;
                                                }
                                            }
                                        }

                                    }
                                }
                                #endregion
                                //List setmenu
                                #region LIST SUBTOTAL SETMENU

                                totalOutlet[i].ListItemizedSalesAnalysisSubTotalSetMenu = new List<ItemizedSalesAnalysisSubTotalSetMenuModels>();
                                var setmenuSubtotal = totalOutlet[i].ListItemizedSalesAnalysisSubTotalSetMenu;

                                var itemsSetmenu = lstItemizeds.Where(q => q.ItemTypeId == Commons.ItemType_SetMenu && q.StoreName == totalOutlet[i].StoreName).ToList();
                                if (itemsSetmenu != null)
                                {
                                    for (int q = 0; q < itemsSetmenu.Count; q++)
                                    {
                                        var setmenu = setmenuSubtotal.FirstOrDefault(m => m.StoreName == itemsSetmenu[q].StoreName && m.TypeID == Commons.ItemType_SetMenu);
                                        if (setmenu == null)
                                        {
                                            setmenu = new ItemizedSalesAnalysisSubTotalSetMenuModels();
                                            setmenu.TypeID = itemsSetmenu[q].ItemTypeId;
                                            setmenu.StoreName = itemsSetmenu[q].StoreName;
                                            setmenu.Qty = itemsSetmenu[q].Quantity;
                                            setmenu.ItemTotal = itemsSetmenu[q].ItemTotal;
                                            setmenu.Percent = itemsSetmenu[q].Percent;
                                            setmenu.Discount = itemsSetmenu[q].Discount;
                                            setmenu.UnitCost = itemsSetmenu[q].Cost;
                                            setmenu.TotalCost = itemsSetmenu[q].TotalCost;
                                            setmenu.Promotion = itemsSetmenu[q].PromotionAmount;

                                            setmenuSubtotal.Add(setmenu);
                                        }
                                        else
                                        {
                                            setmenu.Qty += itemsSetmenu[q].Quantity;
                                            setmenu.ItemTotal += itemsSetmenu[q].ItemTotal;
                                            setmenu.Percent += itemsSetmenu[q].Percent;
                                            setmenu.Discount += itemsSetmenu[q].Discount;
                                            setmenu.UnitCost += itemsSetmenu[q].Cost;
                                            setmenu.TotalCost += itemsSetmenu[q].TotalCost;
                                            setmenu.Promotion += itemsSetmenu[q].PromotionAmount;
                                        }
                                    }
                                }
                                #endregion
                            }
                            #endregion
                            //
                            #region LIST ITEMS OF DISH AND SETMENU
                            if (viewmodel.ListItemizedSalesAnalysisOuletTotal != null)
                            {
                                for (int i = 0; i < viewmodel.ListItemizedSalesAnalysisOuletTotal.Count(); i++)
                                {
                                    //
                                    #region LIST ITEMS OF DISH IN CATEGORY
                                    if (totalOutlet[i].ListItemizedSalesAnalysisSubTotal != null)
                                    {
                                        if (totalOutlet[i].ListItemizedSalesAnalysisSubTotal.Count != 0)
                                        {
                                            var ItemizeSubTotal = totalOutlet[i].ListItemizedSalesAnalysisSubTotal;

                                            for (int j = 0; j < ItemizeSubTotal.Count; j++)
                                            {
                                                ItemizeSubTotal[j].ListItemizedSalesAnalysisItems = new List<ItemizedSalesAnalysisReportModels>();
                                                ItemizeSubTotal[j].ListPeriod = new List<ListPeriodModels>();

                                                var items = lstItemizeds.Where(q => q.ItemTypeId == Commons.ItemType_Dish && q.CategoryId == ItemizeSubTotal[j].CategoryID
                                                && q.StoreName == totalOutlet[i].StoreName).ToList();
                                                if (items.Count == 0)
                                                    continue;

                                                #region CHECK PERIOD IS CHECKED                                       

                                                if (model.Breakfast)
                                                {
                                                    var period = ItemizeSubTotal[j].ListPeriod.FirstOrDefault(p => p.Name.Equals("BREAKFAST"));
                                                    if (period == null)
                                                    {
                                                        ListPeriodModels itemPeriod = new ListPeriodModels();
                                                        itemPeriod.TypeID = 1;
                                                        itemPeriod.CategoryName = ItemizeSubTotal[j].CategoryName;
                                                        itemPeriod.CategoryID = ItemizeSubTotal[j].CategoryID;
                                                        itemPeriod.StoreName = ItemizeSubTotal[j].StoreName;
                                                        itemPeriod.Qty = 0;
                                                        itemPeriod.ItemTotal = 0;
                                                        itemPeriod.Percent = 0;
                                                        itemPeriod.Discount = 0;
                                                        itemPeriod.UnitCost = 0;
                                                        itemPeriod.TotalCost = 0;
                                                        itemPeriod.CP = 0;
                                                        itemPeriod.Name = "BREAKFAST";
                                                        itemPeriod.Promotion = 0;
                                                        ItemizeSubTotal[j].ListPeriod.Add(itemPeriod);
                                                    }
                                                }
                                                if (model.Lunch)
                                                {
                                                    var period = ItemizeSubTotal[j].ListPeriod.FirstOrDefault(p => p.Name.Equals("LUNCH"));
                                                    if (period == null)
                                                    {
                                                        ListPeriodModels itemPeriod = new ListPeriodModels();
                                                        itemPeriod.TypeID = 1;
                                                        itemPeriod.CategoryName = ItemizeSubTotal[j].CategoryName;
                                                        itemPeriod.CategoryID = ItemizeSubTotal[j].CategoryID;
                                                        itemPeriod.StoreName = ItemizeSubTotal[j].StoreName;
                                                        itemPeriod.Qty = 0;
                                                        itemPeriod.ItemTotal = 0;
                                                        itemPeriod.Percent = 0;
                                                        itemPeriod.Discount = 0;
                                                        itemPeriod.UnitCost = 0;
                                                        itemPeriod.TotalCost = 0;
                                                        itemPeriod.CP = 0;
                                                        itemPeriod.Name = "LUNCH";
                                                        itemPeriod.Promotion = 0;
                                                        ItemizeSubTotal[j].ListPeriod.Add(itemPeriod);
                                                    }
                                                }
                                                if (model.Dinner)
                                                {
                                                    var period = ItemizeSubTotal[j].ListPeriod.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                                    if (period == null)
                                                    {
                                                        ListPeriodModels itemPeriod = new ListPeriodModels();
                                                        itemPeriod.TypeID = 1;
                                                        itemPeriod.CategoryName = ItemizeSubTotal[j].CategoryName;
                                                        itemPeriod.CategoryID = ItemizeSubTotal[j].CategoryID;
                                                        itemPeriod.StoreName = ItemizeSubTotal[j].StoreName;
                                                        itemPeriod.Qty = 0;
                                                        itemPeriod.ItemTotal = 0;
                                                        itemPeriod.Percent = 0;
                                                        itemPeriod.Discount = 0;
                                                        itemPeriod.UnitCost = 0;
                                                        itemPeriod.TotalCost = 0;
                                                        itemPeriod.CP = 0;
                                                        itemPeriod.Name = "DINNER";
                                                        itemPeriod.Promotion = 0;
                                                        ItemizeSubTotal[j].ListPeriod.Add(itemPeriod);
                                                    }
                                                }
                                                #endregion
                                                for (int q = 0; q < items.Count; q++)
                                                {
                                                    //=========Editor by Trongntn 09-11-2016
                                                    ProductFactory proFactory = new ProductFactory();
                                                    double extraPrice = proFactory.GetExtraPrice(items[q].ItemId, (byte)Commons.EProductType.Dish);
                                                    items[q].Price += extraPrice;
                                                    //=======================

                                                    ItemizeSubTotal[j].ListItemizedSalesAnalysisItems.Add(items[q]);

                                                    TimeSpan timeDish = items[q].CreatedDate.TimeOfDay;
                                                    #region TOTAL PERIOD OF CATEGORY
                                                    if (model.Breakfast)
                                                    {
                                                        if (timeDish >= brearkStart && timeDish < brearkEnd)
                                                        {
                                                            var period = ItemizeSubTotal[j].ListPeriod.FirstOrDefault(p => p.Name.Equals("BREAKFAST"));
                                                            period.Qty += items[q].Quantity;
                                                            period.ItemTotal += items[q].ItemTotal;
                                                            period.Percent += items[q].Percent;
                                                            period.Discount += items[q].Discount;
                                                            period.UnitCost += items[q].Cost;
                                                            period.TotalCost += items[q].TotalCost;
                                                            period.CP += items[q].CP;
                                                            period.Promotion += items[q].PromotionAmount;
                                                        }
                                                    }
                                                    if (model.Lunch)
                                                    {
                                                        if (timeDish >= lunchStart && timeDish < lunchEnd)
                                                        {
                                                            var period = ItemizeSubTotal[j].ListPeriod.FirstOrDefault(p => p.Name.Equals("LUNCH"));
                                                            period.Qty += items[q].Quantity;
                                                            period.ItemTotal += items[q].ItemTotal;
                                                            period.Percent += items[q].Percent;
                                                            period.Discount += items[q].Discount;
                                                            period.UnitCost += items[q].Cost;
                                                            period.TotalCost += items[q].TotalCost;
                                                            period.CP += items[q].CP;
                                                            period.Promotion += items[q].PromotionAmount;
                                                        }
                                                    }
                                                    if (model.Dinner)
                                                    {
                                                        if (dinnerStart > dinnerEnd)//pass day
                                                        {
                                                            if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                                            {
                                                                var period = ItemizeSubTotal[j].ListPeriod.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                                                period.Qty += items[q].Quantity;
                                                                period.ItemTotal += items[q].ItemTotal;
                                                                period.Percent += items[q].Percent;
                                                                period.Discount += items[q].Discount;
                                                                period.UnitCost += items[q].Cost;
                                                                period.TotalCost += items[q].TotalCost;
                                                                period.CP += items[q].CP;
                                                                period.Promotion += items[q].PromotionAmount;
                                                            }
                                                        }
                                                        else//one day
                                                        {
                                                            if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                                            {
                                                                var period = ItemizeSubTotal[j].ListPeriod.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                                                period.Qty += items[q].Quantity;
                                                                period.ItemTotal += items[q].ItemTotal;
                                                                period.Percent += items[q].Percent;
                                                                period.Discount += items[q].Discount;
                                                                period.UnitCost += items[q].Cost;
                                                                period.TotalCost += items[q].TotalCost;
                                                                period.CP += items[q].CP;
                                                                period.Promotion += items[q].PromotionAmount;
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    //
                                    #region LIST ITEMS OF SETMENU
                                    if (totalOutlet[i].ListItemizedSalesAnalysisSubTotalSetMenu != null)
                                    {
                                        if (totalOutlet[i].ListItemizedSalesAnalysisSubTotalSetMenu.Count != 0)
                                        {
                                            var ItemizeSubTotalSetMenu = totalOutlet[i].ListItemizedSalesAnalysisSubTotalSetMenu;

                                            for (int j = 0; j < ItemizeSubTotalSetMenu.Count; j++)
                                            {
                                                ItemizeSubTotalSetMenu[j].ListItemizedSalesAnalysisItemsSetMenu = new List<ItemizedSalesAnalysisReportModels>();
                                                ItemizeSubTotalSetMenu[j].ListPeriodSetMenu = new List<ListPeriodSetMenuModels>();

                                                var items = lstItemizeds.Where(q => q.ItemTypeId == Commons.ItemType_SetMenu && q.StoreName == totalOutlet[i].StoreName).ToList();
                                                if (items.Count == 0)
                                                    continue;

                                                #region CHECK PERIOD IS CHECKED

                                                if (model.Breakfast)
                                                {
                                                    var period = ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.FirstOrDefault(p => p.Name.Equals("BREAKFAST"));
                                                    if (period == null)
                                                    {
                                                        ListPeriodSetMenuModels periodSetmenu = new ListPeriodSetMenuModels();
                                                        periodSetmenu.TypeID = 2;
                                                        periodSetmenu.CategoryName = "0";
                                                        periodSetmenu.CategoryID = "0";
                                                        periodSetmenu.StoreName = ItemizeSubTotalSetMenu[j].StoreName;
                                                        periodSetmenu.Qty = 0;
                                                        periodSetmenu.ItemTotal = 0;
                                                        periodSetmenu.Percent = 0;
                                                        periodSetmenu.Discount = 0;
                                                        periodSetmenu.UnitCost = 0;
                                                        periodSetmenu.TotalCost = 0;
                                                        periodSetmenu.CP = 0;
                                                        periodSetmenu.Name = "BREAKFAST";
                                                        periodSetmenu.Promotion = 0;
                                                        ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.Add(periodSetmenu);
                                                    }
                                                }
                                                if (model.Lunch)
                                                {
                                                    var period = ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.FirstOrDefault(p => p.Name.Equals("LUNCH"));
                                                    if (period == null)
                                                    {
                                                        ListPeriodSetMenuModels periodSetmenu = new ListPeriodSetMenuModels();
                                                        periodSetmenu.TypeID = 2;
                                                        periodSetmenu.CategoryName = "0";
                                                        periodSetmenu.CategoryID = "0";
                                                        periodSetmenu.StoreName = ItemizeSubTotalSetMenu[j].StoreName;
                                                        periodSetmenu.Qty = 0;
                                                        periodSetmenu.ItemTotal = 0;
                                                        periodSetmenu.Percent = 0;
                                                        periodSetmenu.Discount = 0;
                                                        periodSetmenu.UnitCost = 0;
                                                        periodSetmenu.TotalCost = 0;
                                                        periodSetmenu.CP = 0;
                                                        periodSetmenu.Name = "LUNCH";
                                                        periodSetmenu.Promotion = 0;
                                                        ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.Add(periodSetmenu);
                                                    }
                                                }
                                                if (model.Dinner)
                                                {
                                                    var period = ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                                    if (period == null)
                                                    {
                                                        ListPeriodSetMenuModels periodSetmenu = new ListPeriodSetMenuModels();
                                                        periodSetmenu.TypeID = 2;
                                                        periodSetmenu.CategoryName = "0";
                                                        periodSetmenu.CategoryID = "0";
                                                        periodSetmenu.StoreName = ItemizeSubTotalSetMenu[j].StoreName;
                                                        periodSetmenu.Qty = 0;
                                                        periodSetmenu.ItemTotal = 0;
                                                        periodSetmenu.Percent = 0;
                                                        periodSetmenu.Discount = 0;
                                                        periodSetmenu.UnitCost = 0;
                                                        periodSetmenu.TotalCost = 0;
                                                        periodSetmenu.CP = 0;
                                                        periodSetmenu.Name = "DINNER";
                                                        periodSetmenu.Promotion = 0;
                                                        ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.Add(periodSetmenu);
                                                    }
                                                }
                                                #endregion
                                                for (int q = 0; q < items.Count; q++)
                                                {
                                                    //=========Editor by Trongntn 09-11-2016
                                                    ProductFactory proFactory = new ProductFactory();
                                                    double extraPrice = proFactory.GetExtraPrice(items[q].ItemId, (byte)Commons.EProductType.SetMenu);
                                                    items[q].Price += extraPrice;
                                                    //=======================
                                                    ItemizeSubTotalSetMenu[j].ListItemizedSalesAnalysisItemsSetMenu.Add(items[q]);
                                                    TimeSpan timeSetmenu = items[q].CreatedDate.TimeOfDay;
                                                    #region TOTAL PERIOD ON SETMENU
                                                    if (model.Breakfast)
                                                    {
                                                        if (timeSetmenu >= brearkStart && timeSetmenu < brearkEnd)
                                                        {
                                                            var period = ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.FirstOrDefault(p => p.Name.Equals("BREAKFAST"));
                                                            period.Qty += items[q].Quantity;
                                                            period.ItemTotal += items[q].ItemTotal;
                                                            period.Percent += items[q].Percent;
                                                            period.Discount += items[q].Discount;
                                                            period.UnitCost += items[q].Cost;
                                                            period.TotalCost += items[q].TotalCost;
                                                            period.CP += items[q].CP;
                                                            period.Promotion += items[q].PromotionAmount;
                                                        }
                                                    }
                                                    if (model.Lunch)
                                                    {
                                                        if (timeSetmenu >= lunchStart && timeSetmenu < lunchEnd)
                                                        {
                                                            var period = ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.FirstOrDefault(p => p.Name.Equals("LUNCH"));
                                                            period.Qty += items[q].Quantity;
                                                            period.ItemTotal += items[q].ItemTotal;
                                                            period.Percent += items[q].Percent;
                                                            period.Discount += items[q].Discount;
                                                            period.UnitCost += items[q].Cost;
                                                            period.TotalCost += items[q].TotalCost;
                                                            period.CP += items[q].CP;
                                                            period.Promotion += items[q].PromotionAmount;
                                                        }
                                                    }
                                                    if (model.Dinner)
                                                    {
                                                        if (dinnerStart > dinnerEnd)//pass day
                                                        {
                                                            if (timeSetmenu >= dinnerStart || timeSetmenu < dinnerEnd)
                                                            {
                                                                var period = ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                                                period.Qty += items[q].Quantity;
                                                                period.ItemTotal += items[q].ItemTotal;
                                                                period.Percent += items[q].Percent;
                                                                period.Discount += items[q].Discount;
                                                                period.UnitCost += items[q].Cost;
                                                                period.TotalCost += items[q].TotalCost;
                                                                period.CP += items[q].CP;
                                                                period.Promotion += items[q].PromotionAmount;
                                                            }
                                                        }
                                                        else //one day
                                                        {
                                                            if (timeSetmenu >= dinnerStart && timeSetmenu < dinnerEnd)
                                                            {
                                                                var period = ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.FirstOrDefault(p => p.Name.Equals("DINNER"));
                                                                period.Qty += items[q].Quantity;
                                                                period.ItemTotal += items[q].ItemTotal;
                                                                period.Percent += items[q].Percent;
                                                                period.Discount += items[q].Discount;
                                                                period.UnitCost += items[q].Cost;
                                                                period.TotalCost += items[q].TotalCost;
                                                                period.CP += items[q].CP;
                                                                period.Promotion += items[q].PromotionAmount;
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion

                    var listStoreId = lstStores.Select(ss => ss.Id).ToList();
                    //Export excel
                    XLWorkbook wb = factory.ExportExcel(viewmodel, model, listMiscDisPeriod, listStoreId);
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                    //var ws = wb.Worksheets.Add("Itemized_Sales_Analysis_Report");
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                _logger.Error("Itemized Sales Analysis Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        //lenh add 22/09/2017
        public ActionResult Report(ItemizedSalesAnalysisReportModel model)
        {
            if (!CurrentUser.IsMerchantExtend)
            {
                return Report_New(model);
            }
            return Report_NewForMerchantExtend(model);
        }
        public ActionResult Report_Current(ItemizedSalesAnalysisReportModel model)
        {
            try
            {
                var _lstCateChecked = new List<RFilterCategoryModel>();
                var _lstSetChecked = new List<RFilterCategoryModel>();
                /*editor by trongntn 18-01-2017*/
                if (model.EndTime.Hours == 0)
                {
                    model.EndTime = new TimeSpan(23, 59, 59);
                }
                /*end*/
                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0)
                    , dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                if (dFrom > dTo)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                else if (!model.Breakfast && !model.Lunch && !model.Dinner)
                    ModelState.AddModelError("Breakfast", CurrentUser.GetLanguageTextFromKey("Please choose period."));

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

                //ListCategories
                /*Editor by Trongntn 10-07-2017*/
                if (model.ListStoreCate != null)
                {
                    foreach (var item in model.ListStoreCate)
                    {
                        foreach (var itemCate in item.ListCategoriesSel)
                        {
                            model.ListCategories.Add(new RFilterCategoryModel
                            {
                                Checked = itemCate.Checked,
                                Id = itemCate.Id,
                                Name = itemCate.Name,
                                StoreId = itemCate.StoreId,
                                StoreName = itemCate.StoreName,
                                ListChilds = itemCate.ListChilds
                            });
                        }
                    }
                }
                var lstChildCheck = new List<RFilterCategoryModel>();
                if (model.ListCategories != null && model.ListCategories.Count > 0)
                {
                    _lstCateChecked = model.ListCategories.Where(ww => ww.Checked).ToList();
                    var lstCateChild = model.ListCategories.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstCateChild)
                    {
                        //_lstCateChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                        _categoriesFactory.GetCategoryCheck(ref lstChildCheck, item.ListChilds);
                    }
                    _lstCateChecked.AddRange(lstChildCheck);
                }
                else
                {
                    _lstCateChecked = GetListCategories(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _lstCateChecked.ForEach(x =>
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
                    //====
                    var lstCateChild = _lstCateChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstCateChild)
                    {
                        //_lstCateChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));
                        _categoriesFactory.GetCategoryCheck(ref lstChildCheck, item.ListChilds);
                    }
                    _lstCateChecked.AddRange(lstChildCheck);
                }

                /*ListSetMenu*/
                /*Editor by Trongntn 10-07-2017*/
                if (model.ListStoreSetMenu != null)
                {
                    foreach (var item in model.ListStoreSetMenu)
                    {
                        foreach (var itemSetMenu in item.ListSetMenuSel)
                        {
                            model.ListSetMenu.Add(new RFilterCategoryModel
                            {
                                Checked = itemSetMenu.Checked,
                                Id = itemSetMenu.Id,
                                Name = itemSetMenu.Name,
                                StoreId = itemSetMenu.StoreId,
                                StoreName = itemSetMenu.StoreName
                            });
                        }
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

                //if (_lstCateChecked.Count ==0 && _lstSetChecked.Count ==0)
                //    ModelState.AddModelError("ListCategoryAndSet", "Please choose category.");

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
                if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    lstStores = model.GetSelectedStoreCompany();
                    //model.ListStores = lstStores.Select(ss => ss.Id).ToList();
                    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                    model.ListStores = _lstStoresCateSet;
                }
                else //Store
                {
                    List<SelectListItem> vbStore = ViewBag.Stores;
                    lstStores = model.GetSelectedStore(vbStore);
                    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                    model.ListStores = _lstStoresCateSet;
                }
                //End Get Selected Store

                ItemizedSalesAnalysisModels viewmodel = new ItemizedSalesAnalysisModels();
                viewmodel.ListItemizedSalesAnalysisOuletTotal = new List<ItemizedSalesAnalysisOuletTotalModels>();
                viewmodel.ItemizedSalesAnalysisTotal = new ItemizedSalesAnalysisTotalModels();

                //string StoreIndex = string.Join(",", listStoreIndexes);

                #region Check Period (Breakfast, Lunch, Dinner)
                UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                {
                    Commons.BreakfastStart = currentUser.BreakfastStart;
                    Commons.BreakfastEnd = currentUser.BreakfastEnd;
                    Commons.LunchStart = currentUser.LunchStart;
                    Commons.LunchEnd = currentUser.LunchEnd;
                    Commons.DinnerStart = currentUser.DinnerStart;
                    Commons.DinnerEnd = currentUser.DinnerEnd;
                }

                TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
                TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
                TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
                TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
                TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
                TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);

                #endregion
                //StoreFactory storeFactory = new StoreFactory();
                if (model.StartTime.Hours == 0 && model.StartTime.Minutes == 0 && model.EndTime.Hours == 0 && model.EndTime.Minutes == 0)
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
                }
                else
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                }
                BaseFactory _baseFactory = new BaseFactory();
                ItemizedSalesAnalysisReportFactory factory = new ItemizedSalesAnalysisReportFactory();
                DateTime _dToFilter = model.ToDate;
                DateTime _dFromFilter = model.FromDate;
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore != null && _lstBusDayAllStore.Count > 0)
                {
                    model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                    model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
                    //Get list setmenu checked
                    List<string> ListSetMenu = new List<string>();
                    string strSetMenu = string.Join(",", ListSetMenu);

                    #region MISC DISCOUNT TOTAL BILL
                    DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
                    //model.FromDate = DateTimeHelper.SetFromDate(model.FromDate, new TimeSpan(7, 0, 0));
                    //model.ToDate = DateTimeHelper.SetToDate(model.ToDate, new TimeSpan(22, 0, 0));
                    // 7 -> 11: Breakfast | 11 -> 17: Lunch | 17 -> 22: Dinner
                    var listMisc_Discout = miscFactory.GetReceiptDiscountAndMisc(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                    if (listMisc_Discout == null)
                    {
                        listMisc_Discout = new List<DiscountAndMiscReportModels>();
                    }
                    listMisc_Discout.ForEach(ss => ss.DiscountValue = 0);

                    //double MISCTotal = 0;
                    DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
                    var lstDiscount = discountDetailFactory.GetDiscountTotal(model.ListStores, model.FromDate, model.ToDate);

                    listMisc_Discout.AddRange(lstDiscount);

                    List<MISCBillDiscountPeriodModels> listMiscDisPeriod = new List<MISCBillDiscountPeriodModels>();
                    if (listMisc_Discout != null)
                    {
                        var listMisc_DiscoutGroupStore = listMisc_Discout.GroupBy(gg => gg.StoreId).ToList();
                        foreach (var item in listMisc_DiscoutGroupStore)
                        {
                            var lstItemInStore = item.ToList();
                            #region CHECK PERIOD IS CHECKED

                            if (model.Breakfast)
                            {
                                var miscP = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("BREAKFAST") && p.StoreId == item.Key);
                                if (miscP == null)
                                {
                                    MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                                    itemP.StoreId = item.Key;
                                    itemP.MiscTotal = 0;
                                    itemP.BillDiscountTotal = 0;
                                    itemP.Period = "BREAKFAST";
                                    listMiscDisPeriod.Add(itemP);
                                }
                            }
                            if (model.Lunch)
                            {
                                var miscP = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("LUNCH") && p.StoreId == item.Key);
                                if (miscP == null)
                                {
                                    MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                                    itemP.StoreId = item.Key;
                                    itemP.MiscTotal = 0;
                                    itemP.BillDiscountTotal = 0;
                                    itemP.Period = "LUNCH";
                                    listMiscDisPeriod.Add(itemP);
                                }
                            }
                            if (model.Dinner)
                            {
                                var miscP = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals("DINNER") && p.StoreId == item.Key);
                                if (miscP == null)
                                {
                                    MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                                    itemP.StoreId = item.Key;
                                    itemP.MiscTotal = 0;
                                    itemP.BillDiscountTotal = 0;
                                    itemP.Period = "DINNER";
                                    listMiscDisPeriod.Add(itemP);
                                }
                            }
                            #endregion
                            for (int i = 0; i < lstItemInStore.Count; i++)
                            {
                                //Get Total Misc to + ItemTotal
                                //MISCTotal += lstItemInStore[i].MiscValue;

                                TimeSpan timeMisc = new TimeSpan(lstItemInStore[i].Hour, 0, 0);
                                //Total period Misc_Discout
                                #region TOTAL PERIOD MISC AND DISCOUNT BILL TOTAL
                                if (model.Breakfast)
                                {
                                    if (timeMisc >= brearkStart && timeMisc < brearkEnd)
                                    {
                                        var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == item.Key);
                                        period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                        //listMisc_Discout[i].StoreName;
                                        period.MiscTotal += lstItemInStore[i].MiscValue;
                                        period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                    }
                                }
                                if (model.Lunch)
                                {
                                    if (timeMisc >= lunchStart && timeMisc < lunchEnd)
                                    {
                                        var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == item.Key);
                                        //period.StoreName = listMisc_Discout[i].StoreName;
                                        period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                        //period.StoreId += listMisc_Discout[i].StoreId;
                                        period.MiscTotal += lstItemInStore[i].MiscValue;
                                        period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                    }
                                }
                                if (model.Dinner)
                                {
                                    if (dinnerStart > dinnerEnd)//pass day
                                    {
                                        if (timeMisc >= dinnerStart || timeMisc < dinnerEnd)
                                        {
                                            var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == item.Key);
                                            //period.StoreName = listMisc_Discout[i].StoreName;
                                            period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                            period.MiscTotal += lstItemInStore[i].MiscValue;
                                            period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                        }
                                    }
                                    else//in day
                                    {
                                        if (timeMisc >= dinnerStart && timeMisc < dinnerEnd)
                                        {
                                            var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == item.Key);
                                            period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                            //period.StoreId += listMisc_Discout[i].StoreId;
                                            period.MiscTotal += lstItemInStore[i].MiscValue;
                                            period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                        }
                                    }

                                }
                                #endregion
                            }
                        }
                        //

                    }
                    #endregion
                    //Get data in DB by store SP_Report_ItemizedSalesAnalysis
                    #region DATA OF REPORT
                    
                    //update 09/09 - Filter payment GC
                    var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                    var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                    if (lstGC == null)
                        lstGC = new List<string>();

                    var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, lstStores
                        , _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId, model.Mode);

                    #endregion

                    //var listStoreId = lstStores.Select(ss => ss.Id).ToList();
                    //Export excel
                    XLWorkbook wb = factory.ExportExcel_New(lstItemizeds, viewmodel, model, listMiscDisPeriod, lstStores, _dToFilter, _dFromFilter, lstGC);
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                    //var ws = wb.Worksheets.Add("Itemized_Sales_Analysis_Report");
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                _logger.Error("Itemized Sales Analysis Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
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

        // Updated 09202017
        // Display category depend on parent & child
        public ActionResult Report_New(ItemizedSalesAnalysisReportModel model)
        {
            try
            {
                if (model.EndTime.Hours == 0)
                {
                    model.EndTime = new TimeSpan(23, 59, 59);
                }

                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0)
                    , dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                if (dFrom > dTo)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                else if (!model.Breakfast && !model.Lunch && !model.Dinner)
                    ModelState.AddModelError("Breakfast", CurrentUser.GetLanguageTextFromKey("Please choose period."));

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

                List<RFilterCategoryV1Model> _lstCateChecked = new List<RFilterCategoryV1Model>();
                List<RFilterCategoryV1ReportModel> lstTotalAllCate = new List<RFilterCategoryV1ReportModel>();

                if (model.ListStoreCateV1 != null && model.ListStoreCateV1.Any())
                {
                    foreach (var store in model.ListStoreCateV1)
                    {
                        if (store.ListCategoriesSel != null && store.ListCategoriesSel.Any())
                        {
                            _lstCateChecked.AddRange(store.ListCategoriesSel.Where(ww => ww.Checked));

                            // Add data to list total cate
                            lstTotalAllCate.AddRange(store.ListCategoriesSel.Select(s => new RFilterCategoryV1ReportModel
                            {
                                CateId = s.Id,
                                CateName = s.Name,
                                Checked = s.Checked,
                                StoreId = s.StoreId,
                                ParentId = s.ParentId,
                                Level = s.Level,
                                Seq = s.Seq,
                                ListCateChildChecked = store.ListCategoriesSel.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.Id).ToList()
                            }));
                        }
                    }
                }
                else
                {
                    _lstCateChecked = GetListCategories_V1(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _lstCateChecked.ForEach(x =>
                    {
                        x.Checked = true;
                    });

                    // Add data to list total cate
                    lstTotalAllCate.AddRange(_lstCateChecked.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.Id,
                        CateName = s.Name,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        Level = s.Level,
                        Seq = s.Seq,
                        ListCateChildChecked = _lstCateChecked.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.Id).ToList()
                    }));

                }
                var _lstCateCheckedId = _lstCateChecked.Select(s => s.Id).ToList();

                /*List SetMenu*/
                List<RFilterCategoryModel> _lstSetChecked = new List<RFilterCategoryModel>();
                List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu = new List<RFilterCategoryV1ReportModel>();

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

                    // Add data to list total cate
                    lstTotalAllSetMenu.AddRange(model.ListSetMenu.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.CategoryID,
                        CateName = s.CategoryName,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        ListCateChildChecked = model.ListSetMenu.Where(w => !string.IsNullOrEmpty(w.ParentId)
                        && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                    }));

                    var lstSetChild = model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstSetChild)
                    {
                        _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));

                        // Add data to list total cate
                        lstTotalAllSetMenu.AddRange(item.ListChilds.Select(s => new RFilterCategoryV1ReportModel
                        {
                            CateId = s.CategoryID,
                            CateName = s.CategoryName,
                            Checked = s.Checked,
                            StoreId = s.StoreId,
                            ParentId = s.ParentId,
                            ListCateChildChecked = item.ListChilds.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id
                            && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                        }));
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

                    // Add data to list total cate
                    lstTotalAllSetMenu.AddRange(_lstSetChecked.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.CategoryID,
                        CateName = s.CategoryName,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        ListCateChildChecked = _lstSetChecked.Where(w => !string.IsNullOrEmpty(w.ParentId)
                        && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq)
                        .ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                    }));
                }
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View("Index", model);
                }

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
                //    //model.ListStores = lstStores.Select(ss => ss.Id).ToList();
                //}
                //else //Store
                //{
                //    //List<SelectListItem> vbStore = ViewBag.Stores;
                //    //lstStores = model.GetSelectedStore(vbStore);
                //    lstStores = ViewBag.StoresIncludeCompany;
                //    if (lstStores != null && lstStores.Any())
                //    {
                //        //lstStores = lstStores.Where(ww => model.ListStores.Contains(ww.Id)).ToList();
                //        lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                //    }
                //    model.ListStores = _lstStoresCateSet;
                //}

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

                ItemizedSalesAnalysisModels viewmodel = new ItemizedSalesAnalysisModels();
                viewmodel.ListItemizedSalesAnalysisOuletTotal = new List<ItemizedSalesAnalysisOuletTotalModels>();
                viewmodel.ItemizedSalesAnalysisTotal = new ItemizedSalesAnalysisTotalModels();

                //if (model.StartTime.Hours == 0 && model.StartTime.Minutes == 0 && model.EndTime.Hours == 0 && model.EndTime.Minutes == 0)
                //{
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
                //}
                //else
                //{
                //    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                //    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                //}
                BaseFactory _baseFactory = new BaseFactory();
                ItemizedSalesAnalysisReportFactory factory = new ItemizedSalesAnalysisReportFactory();
                //DateTime _dToFilter = model.ToDate;
                //DateTime _dFromFilter = model.FromDate;
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore != null && _lstBusDayAllStore.Count > 0)
                {
                    model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                    model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);

                    model.FromDateFilter = dFrom;
                    model.ToDateFilter = dTo;
                    // Filter payment GC
                    var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                    var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                    if (lstGC == null)
                        lstGC = new List<string>();

                    //var lstItemizeds = factory.GetData(model.FromDateFilter, model.ToDateFilter, model.ListStores, lstStores
                    //    , _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId, model.Mode);

                    XLWorkbook wb = null;
                    //Export excel

                    //wb = factory.ExportExcel_V1(lstItemizeds, model, lstStores, _dToFilter, _dFromFilter, lstGC, _lstCateChecked
                    //       , _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu);

                    //wb = factory.ExportExcel_CreditNote(lstItemizeds, model, lstStores, lstGC, _lstCateChecked
                    //        , _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu);

                    wb = factory.ExportExcel_CreditNoteNew(model, lstStores, lstGC, _lstCateChecked
                          , _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu
                           , _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId);

                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                _logger.Error("Itemized Sales Analysis Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Report_NewForMerchantExtend(ItemizedSalesAnalysisReportModel model)
        {
            try
            {
                if (model.EndTime.Hours == 0)
                {
                    model.EndTime = new TimeSpan(23, 59, 59);
                }

                DateTime dFrom = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0)
                    , dTo = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                if (dFrom > dTo)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                else if (!model.Breakfast && !model.Lunch && !model.Dinner)
                    ModelState.AddModelError("Breakfast", CurrentUser.GetLanguageTextFromKey("Please choose period."));

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

                List<RFilterCategoryV1Model> _lstCateChecked = new List<RFilterCategoryV1Model>();
                List<RFilterCategoryV1ReportModel> lstTotalAllCate = new List<RFilterCategoryV1ReportModel>();

                if (model.ListStoreCateV1 != null && model.ListStoreCateV1.Any())
                {
                    foreach (var store in model.ListStoreCateV1)
                    {
                        if (store.ListCategoriesSel != null && store.ListCategoriesSel.Any())
                        {
                            _lstCateChecked.AddRange(store.ListCategoriesSel.Where(ww => ww.Checked));

                            // Add data to list total cate
                            lstTotalAllCate.AddRange(store.ListCategoriesSel.Select(s => new RFilterCategoryV1ReportModel
                            {
                                CateId = s.Id,
                                CateName = s.Name,
                                Checked = s.Checked,
                                StoreId = s.StoreId,
                                ParentId = s.ParentId,
                                Level = s.Level,
                                Seq = s.Seq,
                                ListCateChildChecked = store.ListCategoriesSel.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.Id).ToList()
                            }));
                        }
                    }
                }
                else
                {
                    _lstCateChecked = GetListCategories_V1ForMerchantExtend(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _lstCateChecked.ForEach(x =>
                    {
                        x.Checked = true;
                    });

                    // Add data to list total cate
                    lstTotalAllCate.AddRange(_lstCateChecked.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.Id,
                        CateName = s.Name,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        Level = s.Level,
                        Seq = s.Seq,
                        ListCateChildChecked = _lstCateChecked.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.Id).ToList()
                    }));

                }
                var _lstCateCheckedId = _lstCateChecked.Select(s => s.Id).ToList();

                /*List SetMenu*/
                List<RFilterCategoryModel> _lstSetChecked = new List<RFilterCategoryModel>();
                List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu = new List<RFilterCategoryV1ReportModel>();

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

                    // Add data to list total cate
                    lstTotalAllSetMenu.AddRange(model.ListSetMenu.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.CategoryID,
                        CateName = s.CategoryName,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        ListCateChildChecked = model.ListSetMenu.Where(w => !string.IsNullOrEmpty(w.ParentId) 
                        && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                    }));

                    var lstSetChild = model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstSetChild)
                    {
                        _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));

                        // Add data to list total cate
                        lstTotalAllSetMenu.AddRange(item.ListChilds.Select(s => new RFilterCategoryV1ReportModel
                        {
                            CateId = s.CategoryID,
                            CateName = s.CategoryName,
                            Checked = s.Checked,
                            StoreId = s.StoreId,
                            ParentId = s.ParentId,
                            ListCateChildChecked = item.ListChilds.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id 
                            && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                        }));
                    }
                }
                else
                {
                    _lstSetChecked = GetListSetMenusForMerchantExtend(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
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

                    // Add data to list total cate
                    lstTotalAllSetMenu.AddRange(_lstSetChecked.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.CategoryID,
                        CateName = s.CategoryName,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        ListCateChildChecked = _lstSetChecked.Where(w => !string.IsNullOrEmpty(w.ParentId) 
                        && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq)
                        .ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                    }));
                }
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View("Index", model);
                }

                // Get list store selected
                var _lstStoreCate = _lstCateChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoreSet = _lstSetChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoresCateSet = _lstStoreCate.Concat(_lstStoreSet).Distinct().ToList();

                //Get Selected Store
                List<StoreModels> lstStores = new List<StoreModels>();
                if (model.Type == Commons.TypeCompanySelected) //Company
                {
                    lstStores = model.GetSelectedStoreCompanyForExtend(ViewBag.StoresExtend);
                    ///model.ListStores = lstStores.Select(ss => ss.Id).ToList();
                    ///lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                    model.ListStores = _lstStoresCateSet;
                }
                else //Store
                {
                    List<StoreModels> vbStore = ViewBag.StoresExtend;
                    lstStores = model.GetSelectedStoreForMerchantExtend(vbStore);
                    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                    model.ListStores = _lstStoresCateSet;
                }
                //End Get Selected Store

                ItemizedSalesAnalysisModels viewmodel = new ItemizedSalesAnalysisModels();
                viewmodel.ListItemizedSalesAnalysisOuletTotal = new List<ItemizedSalesAnalysisOuletTotalModels>();
                viewmodel.ItemizedSalesAnalysisTotal = new ItemizedSalesAnalysisTotalModels();

                if (model.StartTime.Hours == 0 && model.StartTime.Minutes == 0 && model.EndTime.Hours == 0 && model.EndTime.Minutes == 0)
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
                }
                else
                {
                    model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                    model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 59);
                }
                BaseFactory _baseFactory = new BaseFactory();
                ItemizedSalesAnalysisReportFactory factory = new ItemizedSalesAnalysisReportFactory();
                DateTime _dToFilter = model.ToDate;
                DateTime _dFromFilter = model.FromDate;
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore != null && _lstBusDayAllStore.Count > 0)
                {
                    model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                    model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);

                    List<RFilterChooseExtBaseModel> lstPaymentMethod = new List<RFilterChooseExtBaseModel>();
                    // Filter payment GC
                    //Group by hostUrl
                    List<string> lstStoreId = new List<string>();
                    var groupByHostUrl = lstStores.GroupBy(gg => gg.HostUrlExtend);
                    foreach (var item in groupByHostUrl)
                    {
                        lstStoreId = item.Select(ss => ss.Id).ToList();
                        var tmp = _baseFactory.GetAllPaymentForMerchantExtendReport(item.Key, new CategoryApiRequestModel() {  ListStoreIds = lstStoreId  });
                        lstPaymentMethod.AddRange(tmp);
                    }
                 
                    var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                    if (lstGC == null)
                        lstGC = new List<string>();

                    var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, lstStores
                        , _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId, model.Mode);

                    //Export excel
                    XLWorkbook wb = factory.ExportExcel_V1ForMerchantExtend(lstItemizeds, model, lstStores, _dToFilter, _dFromFilter, lstGC
                        , _lstCateChecked, _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu);
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                _logger.Error("Itemized Sales Analysis Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public List<RFilterCategoryV1Model> GetListCategories_V1(List<string> lstStoreIds, int typeId = 2)
        {
            List<RFilterCategoryV1Model> result = new List<RFilterCategoryV1Model>();

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
            result = _categoriesFactory.GetAllCategoriesForReport_V1(request);
            return result;
        }

        public ActionResult LoadCategories_V1(List<string> lstStoreIds, int typeId = 2)
        {
            ItemizedSalesAnalysisReportModel model = new ItemizedSalesAnalysisReportModel();
            try
            {
                if (CurrentUser.IsMerchantExtend)
                {
                    model.ListCategoriesV1 = GetListCategories_V1ForMerchantExtend(lstStoreIds, typeId);
                }
                else
                    model.ListCategoriesV1 = GetListCategories_V1(lstStoreIds, typeId);

                model.ListStoreCateV1 = model.ListCategoriesV1
                                        .GroupBy(x => new { StoreId = x.StoreId, StoreName = x.StoreName })
                                        .Select(x => new StoreCateV1
                                        {
                                            StoreID = x.Key.StoreId,
                                            StoreName = x.Key.StoreName,
                                            ListCategoriesSel = new List<RFilterCategoryV1Model>()
                                        }).ToList();
                int OffSet = 0;
                model.ListStoreCateV1.ForEach(x =>
                {
                    x.OffSet = OffSet++;
                    x.ListCategoriesSel = model.ListCategoriesV1.Where(z => z.StoreName.Equals(x.StoreName)).ToList();
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return PartialView("_FilterCategory_V1", model);
        }

        #region For Merchant extend
        public List<RFilterCategoryV1Model> GetListCategories_V1ForMerchantExtend(List<string> lstStoreIds, int typeId = 2)
        {
            
            List<RFilterCategoryV1Model> result = new List<RFilterCategoryV1Model>();
            List< CategoryApiRequestModel> lstRequest = new List<CategoryApiRequestModel>();
            CategoryApiRequestModel request = new CategoryApiRequestModel();
            List<StoreModels> lstStores = ViewBag.StoresExtend;
            if (typeId == 1)//company
            {
                lstStoreIds = lstStores.Where(ww => lstStoreIds.Contains(ww.CompanyId)).Select(ss => ss.Id).ToList();
            }
            List<StoreModels> lstStoreSelect = lstStores.Where(ww => lstStoreIds.Contains(ww.Id)).ToList();
            var lstHostUrl = lstStoreSelect.Select(ss => ss.HostUrlExtend).Distinct().ToList();
            foreach (var item in lstHostUrl)
            {
                request = new CategoryApiRequestModel();
                request.HostUrl = item;
                request.ListStoreIds = lstStoreSelect.Where(ww => ww.HostUrlExtend == item).Select(ss => ss.Id).ToList();
                request.Type = (int)Commons.EProductType.Dish;

                lstRequest.Add(request);
            }
            
            result = _categoriesFactory.GetAllCategoriesForReport_V1ForExtendMerchant(lstRequest);
            return result;
        }

        public List<RFilterCategoryModel> GetListSetMenusForMerchantExtend(List<string> lstStoreIds, int typeId = 2)
        {
            List<RFilterCategoryModel> result = new List<RFilterCategoryModel>();

            ProductFactory _productFactory = new ProductFactory();
            List<CategoryApiRequestModel> lstRequest = new List<CategoryApiRequestModel>();
            CategoryApiRequestModel request = new CategoryApiRequestModel();
            List<StoreModels> lstStores = ViewBag.StoresExtend;
            if (typeId == 1)//company
            {
                lstStoreIds = lstStores.Where(ww => lstStoreIds.Contains(ww.CompanyId)).Select(ss => ss.Id).ToList();
            }
            List<StoreModels> lstStoreSelect = lstStores.Where(ww => lstStoreIds.Contains(ww.Id)).ToList();
            var lstHostUrl = lstStoreSelect.Select(ss => ss.HostUrlExtend).Distinct().ToList();
            foreach (var item in lstHostUrl)
            {
                request = new CategoryApiRequestModel();
                request.HostUrl = item;
                request.ListStoreIds = lstStoreSelect.Where(ww => ww.HostUrlExtend == item).Select(ss => ss.Id).ToList();
                request.Type = (int)Commons.EProductType.SetMenu;

                lstRequest.Add(request);
            }

            result = _productFactory.GetAllSetMenuForReportForMerchantExtend(lstRequest);

            return result;
        }
        #endregion End For Merchant extend

        #region Report new filter time, updated 05232018
        public ActionResult Report_NewFilterTime(ItemizedSalesAnalysisReportModel model)
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
                }
                else
                {
                    dTo = dTo.AddSeconds(59);
                }

                if (!model.Breakfast && !model.Lunch && !model.Dinner)
                    ModelState.AddModelError("Breakfast", CurrentUser.GetLanguageTextFromKey("Please choose period."));

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

                List<RFilterCategoryV1Model> _lstCateChecked = new List<RFilterCategoryV1Model>();
                List<RFilterCategoryV1ReportModel> lstTotalAllCate = new List<RFilterCategoryV1ReportModel>();

                if (model.ListStoreCateV1 != null && model.ListStoreCateV1.Any())
                {
                    foreach (var store in model.ListStoreCateV1)
                    {
                        if (store.ListCategoriesSel != null && store.ListCategoriesSel.Any())
                        {
                            _lstCateChecked.AddRange(store.ListCategoriesSel.Where(ww => ww.Checked));

                            // Add data to list total cate
                            lstTotalAllCate.AddRange(store.ListCategoriesSel.Select(s => new RFilterCategoryV1ReportModel
                            {
                                CateId = s.Id,
                                CateName = s.Name,
                                Checked = s.Checked,
                                StoreId = s.StoreId,
                                ParentId = s.ParentId,
                                Level = s.Level,
                                Seq = s.Seq,
                                ListCateChildChecked = store.ListCategoriesSel.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.Id).ToList()
                            }));
                        }
                    }
                }
                else
                {
                    _lstCateChecked = GetListCategories_V1(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    _lstCateChecked.ForEach(x =>
                    {
                        x.Checked = true;
                    });

                    // Add data to list total cate
                    lstTotalAllCate.AddRange(_lstCateChecked.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.Id,
                        CateName = s.Name,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        Level = s.Level,
                        Seq = s.Seq,
                        ListCateChildChecked = _lstCateChecked.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.Id).ToList()
                    }));

                }
                var _lstCateCheckedId = _lstCateChecked.Select(s => s.Id).ToList();

                /*List SetMenu*/
                List<RFilterCategoryModel> _lstSetChecked = new List<RFilterCategoryModel>();
                List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu = new List<RFilterCategoryV1ReportModel>();

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

                    // Add data to list total cate
                    lstTotalAllSetMenu.AddRange(model.ListSetMenu.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.CategoryID,
                        CateName = s.CategoryName,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        ListCateChildChecked = model.ListSetMenu.Where(w => !string.IsNullOrEmpty(w.ParentId)
                        && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                    }));

                    var lstSetChild = model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Count > 0).ToList();
                    foreach (var item in lstSetChild)
                    {
                        _lstSetChecked.AddRange(item.ListChilds.Where(ww => ww.Checked));

                        // Add data to list total cate
                        lstTotalAllSetMenu.AddRange(item.ListChilds.Select(s => new RFilterCategoryV1ReportModel
                        {
                            CateId = s.CategoryID,
                            CateName = s.CategoryName,
                            Checked = s.Checked,
                            StoreId = s.StoreId,
                            ParentId = s.ParentId,
                            ListCateChildChecked = item.ListChilds.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id
                            && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                        }));
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

                    // Add data to list total cate
                    lstTotalAllSetMenu.AddRange(_lstSetChecked.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.CategoryID,
                        CateName = s.CategoryName,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        ListCateChildChecked = _lstSetChecked.Where(w => !string.IsNullOrEmpty(w.ParentId)
                        && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked).OrderBy(o => o.Seq)
                        .ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                    }));
                }
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View("Index", model);
                }

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
                //    lstStores = ViewBag.StoresIncludeCompany;
                //    if (lstStores != null && lstStores.Any())
                //    {
                //        lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                //    }
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
                if (lstStores != null && lstStores.Any())
                {
                    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                }
                model.ListStores = _lstStoresCateSet;
                //End Get Selected Store

                ItemizedSalesAnalysisModels viewmodel = new ItemizedSalesAnalysisModels();
                viewmodel.ListItemizedSalesAnalysisOuletTotal = new List<ItemizedSalesAnalysisOuletTotalModels>();
                viewmodel.ItemizedSalesAnalysisTotal = new ItemizedSalesAnalysisTotalModels();

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                BaseFactory _baseFactory = new BaseFactory();
                ItemizedSalesAnalysisReportFactory factory = new ItemizedSalesAnalysisReportFactory();

                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
                if (_lstBusDayAllStore != null && _lstBusDayAllStore.Count > 0)
                {
                    model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
                    model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);

                    model.FromDateFilter = dFrom;
                    model.ToDateFilter = dTo;
                    // Filter payment GC
                    var lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new CategoryApiRequestModel() { ListStoreIds = model.ListStores });
                    var lstGC = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
                    if (lstGC == null)
                        lstGC = new List<string>();

                    var lstItemizeds = factory.GetData(model.FromDate, model.ToDate, model.ListStores, lstStores
                        , _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId, model.Mode);
                    if (lstItemizeds != null && lstItemizeds.Any())
                    {
                        switch (model.FilterType)
                        {
                            case (int)Commons.EFilterType.OnDay:
                                lstItemizeds = lstItemizeds.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                                break;
                            case (int)Commons.EFilterType.Days:
                                lstItemizeds = lstItemizeds.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                                break;
                        }
                    }

                    XLWorkbook wb = null;
                    //Export excel
                    wb = factory.ExportExcel_CreditNote(lstItemizeds, model, lstStores, lstGC, _lstCateChecked
                            , _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu);

                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                    ViewBag.wb = wb;
                    string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                _logger.Error("Itemized Sales Analysis Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        #endregion Report new filter time

        #region Report new DB , for Merchant extend and not extend
        public ActionResult Report_NewDB(ItemizedSalesAnalysisReportModel model)
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
                }
                else
                {
                    dTo = dTo.AddSeconds(59);
                }

                if (!model.Breakfast && !model.Lunch && !model.Dinner)
                {
                    ModelState.AddModelError("Breakfast", CurrentUser.GetLanguageTextFromKey("Please choose period."));
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

                bool isExtend = false;
                if (CurrentUser.IsMerchantExtend)
                {
                    isExtend = true;
                }

                // List Category
                List<RFilterCategoryV1Model> _lstCateChecked = new List<RFilterCategoryV1Model>();
                List<RFilterCategoryV1ReportModel> lstTotalAllCate = new List<RFilterCategoryV1ReportModel>();

                if (model.ListStoreCateV1 != null && model.ListStoreCateV1.Any())
                {
                    _lstCateChecked.AddRange(model.ListStoreCateV1.Where(ww => ww.ListCategoriesSel != null && ww.ListCategoriesSel.Any())
                                                                .SelectMany(ss => ss.ListCategoriesSel.Where(w => w.Checked).ToList()).ToList());
                    // Add data to list total cate
                    lstTotalAllCate.AddRange(model.ListStoreCateV1.Where(ww => ww.ListCategoriesSel != null && ww.ListCategoriesSel.Any())
                                                                .SelectMany(ss => ss.ListCategoriesSel.Select(s => new RFilterCategoryV1ReportModel
                                                                {
                                                                    CateId = s.Id,
                                                                    CateName = s.Name,
                                                                    Checked = s.Checked,
                                                                    StoreId = s.StoreId,
                                                                    ParentId = s.ParentId,
                                                                    Level = s.Level,
                                                                    Seq = s.Seq,
                                                                    ListCateChildChecked = ss.ListCategoriesSel.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id
                                                                                                                && w.StoreId == s.StoreId && w.Checked)
                                                                                                                .OrderBy(o => o.Seq).ThenBy(oo => oo.Name)
                                                                                                                .Select(sl => sl.Id).ToList()
                                                                })).ToList());
                }
                else
                {
                    // Select all Dish category
                    if (!isExtend) // Merchant not extend
                    {
                        _lstCateChecked = GetListCategories_V1(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    }
                    else // Merchant extend
                    {
                        _lstCateChecked = GetListCategories_V1ForMerchantExtend(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    }

                    // Add data to list total cate
                    lstTotalAllCate.AddRange(_lstCateChecked.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.Id,
                        CateName = s.Name,
                        Checked = true,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        Level = s.Level,
                        Seq = s.Seq,
                        ListCateChildChecked = _lstCateChecked.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id && w.StoreId == s.StoreId).OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.Id).ToList()
                    }));
                }
                var _lstCateCheckedId = _lstCateChecked.Select(s => s.Id).ToList();

                // List SetMenu
                List<RFilterCategoryModel> _lstSetChecked = new List<RFilterCategoryModel>();
                List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu = new List<RFilterCategoryV1ReportModel>();

                if (model.ListStoreSetMenu != null && model.ListStoreSetMenu.Any())
                {
                    model.ListSetMenu.AddRange(model.ListStoreSetMenu.SelectMany(ss => ss.ListSetMenuSel).ToList());
                }
                if (model.ListSetMenu != null && model.ListSetMenu.Any())
                {
                    _lstSetChecked = model.ListSetMenu.Where(ww => ww.Checked).ToList();

                    // Add data to list total cate set menu
                    lstTotalAllSetMenu.AddRange(model.ListSetMenu.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.CategoryID,
                        CateName = s.CategoryName,
                        Checked = s.Checked,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        ListCateChildChecked = model.ListSetMenu.Where(w => !string.IsNullOrEmpty(w.ParentId)
                                                        && w.ParentId == s.Id && w.StoreId == s.StoreId && w.Checked)
                                                        .OrderBy(o => o.Seq).ThenBy(oo => oo.Name).Select(ss => ss.CategoryID).ToList()
                    }));

                    #region For Set Menu child
                    _lstSetChecked.AddRange(model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Any())
                                                            .SelectMany(ss => ss.ListChilds.Where(ww => ww.Checked)).ToList());
                    // Add data to list total cate set menu
                    lstTotalAllSetMenu.AddRange(model.ListSetMenu.Where(ww => ww.ListChilds != null && ww.ListChilds.Any())
                                                            .SelectMany(ss => ss.ListChilds.Select(s => new RFilterCategoryV1ReportModel
                                                            {
                                                                CateId = s.CategoryID,
                                                                CateName = s.CategoryName,
                                                                Checked = s.Checked,
                                                                StoreId = s.StoreId,
                                                                ParentId = s.ParentId,
                                                                ListCateChildChecked = ss.ListChilds.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == s.Id
                                                                                                        && w.StoreId == s.StoreId && w.Checked)
                                                                                                        .OrderBy(o => o.Seq).ThenBy(oo => oo.Name)
                                                                                                        .Select(sl => sl.CategoryID).ToList()
                                                            }).ToList()));
                    #endregion For Set Menu child

                }
                else
                {
                    // Select all Set Menu category
                    if (!isExtend) // Merchant not extend
                    {
                        _lstSetChecked = GetListSetMenus(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    }
                    else
                    {
                        _lstSetChecked = GetListSetMenusForMerchantExtend(model.Type == 1 ? model.ListCompanys : model.ListStores, model.Type);
                    }
                    _lstSetChecked.AddRange(_lstSetChecked.Where(ww => ww.ListChilds != null && ww.ListChilds.Any()).SelectMany(ss => ss.ListChilds).ToList());

                    // Add data to list total cate set menu
                    lstTotalAllSetMenu.AddRange(_lstSetChecked.Select(s => new RFilterCategoryV1ReportModel
                    {
                        CateId = s.CategoryID,
                        CateName = s.CategoryName,
                        Checked = true,
                        StoreId = s.StoreId,
                        ParentId = s.ParentId,
                        ListCateChildChecked = _lstSetChecked.Where(w => !string.IsNullOrEmpty(w.ParentId)
                                                                    && w.ParentId == s.Id && w.StoreId == s.StoreId)
                                                                    .OrderBy(o => o.Seq).ThenBy(oo => oo.Name)
                                                                    .Select(ss => ss.CategoryID).ToList()
                    }).ToList());
                }
                var _lstSetCheckedId = _lstSetChecked.Select(ss => ss.Id).ToList();

                // Get list store selected
                var _lstStoreCate = _lstCateChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoreSet = _lstSetChecked.Select(ss => ss.StoreId).Distinct().ToList();
                var _lstStoresCateSet = _lstStoreCate.Concat(_lstStoreSet).Distinct().ToList();

                //Get Selected Store
                List<StoreModels> lstStores = new List<StoreModels>();

                //if (!isExtend) // Merchant not extend
                //{
                //    if (model.Type == Commons.TypeCompanySelected) // Company
                //    {
                //        lstStores = model.GetSelectedStoreCompany();
                //    }
                //    else // Store
                //    {
                //        lstStores = ViewBag.StoresIncludeCompany;
                //    }
                //}
                //else // Merchant extend
                //{
                //    if (model.Type == Commons.TypeCompanySelected) // Company
                //    {
                //        lstStores = model.GetSelectedStoreCompanyForExtend(ViewBag.StoresExtend);
                //    }
                //    else // Store
                //    {
                //        lstStores = model.GetSelectedStoreForMerchantExtend(ViewBag.StoresExtend);
                //    }
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

                if (lstStores != null && lstStores.Any())
                {
                    lstStores = lstStores.Where(ww => _lstStoresCateSet.Contains(ww.Id)).ToList();
                }
                model.ListStores = _lstStoresCateSet;

                //End Get Selected Store

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                model.FromDateFilter = dFrom;
                model.ToDateFilter = dTo;

                ItemizedSalesAnalysisReportFactory factory = new ItemizedSalesAnalysisReportFactory();

                XLWorkbook wb = new XLWorkbook();

                wb = factory.ExportExcel_NewDB(model, lstStores, _lstCateChecked, _lstSetChecked, lstTotalAllCate, lstTotalAllSetMenu, isExtend);

                ViewBag.wb = wb;
                string sheetName = string.Format("Report_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
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
                _logger.Error("Itemized Sales Analysis Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        #endregion Report new DB , for Merchant extend and not extend
    }
}