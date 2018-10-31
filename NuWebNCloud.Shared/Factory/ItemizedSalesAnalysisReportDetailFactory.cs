using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.ItemizedSalesAnalysisReport;
using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Data.Models;

namespace NuWebNCloud.Shared.Factory
{
    public class ItemizedSalesAnalysisReportDetailFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public ItemizedSalesAnalysisReportDetailFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<ItemizedSalesAnalysisReportDetailModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_ItemizedSalesAnalysisReportDetail.Where(ww => ww.StoreId == info.StoreId
                                && ww.BusinessId == info.BusinessId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Itemized Sales Detail data exist");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_ItemizedSalesAnalysisReportDetail> lstInsert = new List<R_ItemizedSalesAnalysisReportDetail>();
                        R_ItemizedSalesAnalysisReportDetail itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_ItemizedSalesAnalysisReportDetail();
                            itemInsert.Id = item.Id;
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.BusinessId = item.BusinessId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.ItemCode = item.ItemCode;
                            itemInsert.ItemId = item.ItemId;
                            itemInsert.ItemTypeId = item.ItemTypeId;
                            itemInsert.ItemName = item.ItemName;
                            itemInsert.ParentId = item.ParentId;
                            itemInsert.Price = item.Price;
                            itemInsert.TotalAmount = item.TotalAmount;
                            itemInsert.Quantity = item.Quantity;
                            itemInsert.CategoryId = item.CategoryId;
                            itemInsert.CategoryName = item.CategoryName;
                            itemInsert.Mode = item.Mode;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_ItemizedSalesAnalysisReportDetail.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Itemized Sales Detail data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Itemized Sales Detail data fail", ex);
                        //_logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            //var jsonContent = JsonConvert.SerializeObject(lstInfo);
            //_baseFactory.InsertTrackingLog("R_ItemizedSalesAnalysisReportDetail", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<ItemizedSalesAnalysisReportDetailModels> GetData(DateTime dFrom, DateTime dTo
            , List<string> lstStoreId, List<string> lstStoreCates = null
            , List<string> lstStoreSets = null, List<string> lstCateIds = null, List<string> lstCateSetIds = null, int mode = 1)
        {
            using (var cxt = new NuWebContext())
            {
                List<ItemizedSalesAnalysisReportDetailModels> lstReturn = new List<ItemizedSalesAnalysisReportDetailModels>();
                if (lstStoreId != null && lstStoreId.Any())
                {
                    List<ItemizedSalesAnalysisReportDetailModels> lstData = new List<ItemizedSalesAnalysisReportDetailModels>();
                    List<ItemizedSalesAnalysisReportDetailModels> lstDish = new List<ItemizedSalesAnalysisReportDetailModels>();
                    List<ItemizedSalesAnalysisReportDetailModels> lstSet = new List<ItemizedSalesAnalysisReportDetailModels>();

                    var query = (from tb in cxt.R_ItemizedSalesAnalysisReportDetail
                                 where lstStoreId.Contains(tb.StoreId)
                                       && tb.CreatedDate >= dFrom && tb.CreatedDate <= dTo
                                       && tb.ItemTypeId != (int)Commons.EProductType.Misc
                                       && tb.Mode == mode
                                 select tb);
                    if (query.Any())
                    {
                        ItemizedSalesAnalysisReportDetailModels obj = null;
                        foreach (var item in query)
                        {
                            obj = new ItemizedSalesAnalysisReportDetailModels();
                            obj.Id = item.Id;
                            obj.CreatedDate = item.CreatedDate;
                            obj.BusinessId = item.BusinessId;
                            obj.StoreId = item.StoreId;
                            obj.CategoryId = item.CategoryId;
                            obj.CategoryName = item.CategoryName;
                            obj.ItemTypeId = item.ItemTypeId;
                            obj.ItemId = item.ItemId;
                            obj.ItemCode = item.ItemCode;
                            obj.ItemName = item.ItemName;
                            obj.ParentId = item.ParentId;
                            obj.Quantity = item.Quantity;
                            obj.Price = item.Price;
                            obj.TotalAmount = item.TotalAmount;

                            lstData.Add(obj);
                        }
                    }
                    if (lstData.Count > 0)
                    {
                        if (lstStoreCates != null && lstStoreCates.Any() && lstCateIds != null && lstCateIds.Any())
                        {
                            lstDish = lstData.Where(ww => lstCateIds.Contains(ww.CategoryId) && lstStoreCates.Contains(ww.StoreId)).ToList();
                        }
                        if (lstStoreSets != null && lstStoreSets.Any() && lstCateSetIds != null && lstCateSetIds.Any())
                            lstSet = lstData.Where(ww => lstCateSetIds.Contains(ww.ItemId) && lstStoreSets.Contains(ww.StoreId)).ToList();
                    }
                    lstReturn.AddRange(lstDish);
                    lstReturn.AddRange(lstSet);
                }

                return lstReturn;
            }
        }


        public XLWorkbook ExportExcel(List<ItemizedSalesAnalysisReportDetailModels> lstData, BaseReportModel viewmodel
            , List<StoreModels> lstStores, List<DiscountAndMiscReportModels> listMisc_Discout)
        {
            string sheetName = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized_Sales_Detail_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 7, viewmodel.FromDateFilter, viewmodel.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Detail Itemized Sales Analysis Report"));

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dishes"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifiers"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Range("A" + 5 + ":G" + 5).Style.Font.SetBold(true);
            ws.Row(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            int index = 6;

            #region Check Period (Breakfast, Lunch, Dinner)
            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];


            TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
            TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
            TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
            TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
            TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
            TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);

            #endregion
            //Total
            ItemizedSalesDetailTotal itemTotal = new ItemizedSalesDetailTotal();
            var breakfastTotal = new ItemizedSalesDetailValueTotal();
            var lunchTotal = new ItemizedSalesDetailValueTotal();
            var dinnerTotal = new ItemizedSalesDetailValueTotal();

            ItemizedSalesDetailTotal itemSubTotal = new ItemizedSalesDetailTotal();

            List<MISCBillDiscountPeriodModels> listMiscDisPeriod = new List<MISCBillDiscountPeriodModels>();
            if (listMisc_Discout != null && listMisc_Discout.Any())
            {
                var listMisc_DiscoutGroupStore = listMisc_Discout.GroupBy(gg => gg.StoreId).ToList();
                foreach (var item in listMisc_DiscoutGroupStore)
                {
                    var lstItemInStore = item.ToList();
                    #region CHECK PERIOD IS CHECKED

                    var miscP = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == item.Key);
                    if (miscP == null)
                    {
                        MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                        itemP.StoreId = item.Key;
                        itemP.MiscTotal = 0;
                        itemP.BillDiscountTotal = 0;
                        itemP.Period = Commons.BREAKFAST;
                        listMiscDisPeriod.Add(itemP);
                    }

                    var miscPLunch = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == item.Key);
                    if (miscPLunch == null)
                    {
                        MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                        itemP.StoreId = item.Key;
                        itemP.MiscTotal = 0;
                        itemP.BillDiscountTotal = 0;
                        itemP.Period = Commons.LUNCH;
                        listMiscDisPeriod.Add(itemP);
                    }

                    var miscPDinner = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == item.Key);
                    if (miscPDinner == null)
                    {
                        MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                        itemP.StoreId = item.Key;
                        itemP.MiscTotal = 0;
                        itemP.BillDiscountTotal = 0;
                        itemP.Period = Commons.DINNER;
                        listMiscDisPeriod.Add(itemP);
                    }
                    #endregion
                }
            }

            if (lstData != null && lstData.Any())
            {
                TimeSpan timeDish = new TimeSpan();

                double miscTotal = 0, miscBreakfast = 0, miscLunch = 0, miscDinner = 0;
                double outletSubTotal = 0;
                double discountTotal = 0, discountBreakfast = 0, discountLunch = 0, discountDinner = 0;

                var lstStoreIdData = lstData.Select(s => s.StoreId).Distinct().ToList();
                lstStores = lstStores.Where(w => lstStoreIdData.Contains(w.Id)).OrderBy(o => o.Name).ToList();

                foreach (var store in lstStores)
                {
                    itemTotal = new ItemizedSalesDetailTotal();
                    breakfastTotal = new ItemizedSalesDetailValueTotal();
                    lunchTotal = new ItemizedSalesDetailValueTotal();
                    dinnerTotal = new ItemizedSalesDetailValueTotal();

                    // Get general setting of store
                    if (currentUser != null)
                    {
                        // If store already setting, get value from store setting
                        // Else get value from current user setting
                        Commons.BreakfastStart = currentUser.BreakfastStart;
                        Commons.BreakfastEnd = currentUser.BreakfastEnd;
                        Commons.LunchStart = currentUser.LunchStart;
                        Commons.LunchEnd = currentUser.LunchEnd;
                        Commons.DinnerStart = currentUser.DinnerStart;
                        Commons.DinnerEnd = currentUser.DinnerEnd;

                        // Get time period from general setting
                        if (currentUser.ListSetting != null && currentUser.ListSetting.Any())
                        {
                            var settingPeriodOfStore = currentUser.ListSetting.Where(w => w.StoreID == store.Id).ToList();

                            foreach (var itm in settingPeriodOfStore)
                            {
                                switch (itm.Name)
                                {
                                    case "BreakfastStart":
                                        Commons.BreakfastStart = itm.Value;
                                        break;
                                    case "BreakfastEnd":
                                        Commons.BreakfastEnd = itm.Value;
                                        break;
                                    case "LunchStart":
                                        Commons.LunchStart = itm.Value;
                                        break;
                                    case "LunchEnd":
                                        Commons.LunchEnd = itm.Value;
                                        break;
                                    case "DinnerStart":
                                        Commons.DinnerStart = itm.Value;
                                        break;
                                    case "DinnerEnd":
                                        Commons.DinnerEnd = itm.Value;
                                        break;
                                }
                            }
                        }
                        brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
                        brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
                        lunchStart = TimeSpan.Parse(Commons.LunchStart);
                        lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
                        dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
                        dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);
                    }

                    // Get MISC & Discount of store
                    var lstMiscDiscountInStore = listMisc_Discout.Where(w => w.StoreId == store.Id).ToList();

                    for (int i = 0; i < lstMiscDiscountInStore.Count; i++)
                    {
                        // Get Total Misc to + ItemTotal
                        TimeSpan timeMisc = new TimeSpan(lstMiscDiscountInStore[i].Hour, 0, 0);
                        // Total period Misc_Discout
                        // TOTAL PERIOD MISC AND DISCOUNT BILL TOTAL
                        //// BREAKFAST
                        if (timeMisc >= brearkStart && timeMisc < brearkEnd)
                        {
                            var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == store.Id);
                            period.MiscTotal += lstMiscDiscountInStore[i].MiscValue;
                            period.BillDiscountTotal += lstMiscDiscountInStore[i].DiscountValue;
                        }
                        //// LUNCH
                        if (timeMisc >= lunchStart && timeMisc < lunchEnd)
                        {
                            var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == store.Id);
                            period.MiscTotal += lstMiscDiscountInStore[i].MiscValue;
                            period.BillDiscountTotal += lstMiscDiscountInStore[i].DiscountValue;
                        }
                        //// DINNER
                        if (dinnerStart > dinnerEnd)//pass day
                        {
                            if (timeMisc >= dinnerStart || timeMisc < dinnerEnd)
                            {
                                var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == store.Id);
                                period.MiscTotal += lstMiscDiscountInStore[i].MiscValue;
                                period.BillDiscountTotal += lstMiscDiscountInStore[i].DiscountValue;
                            }
                        }
                        else//in day
                        {
                            if (timeMisc >= dinnerStart && timeMisc < dinnerEnd)
                            {
                                var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == store.Id);
                                period.MiscTotal += lstMiscDiscountInStore[i].MiscValue;
                                period.BillDiscountTotal += lstMiscDiscountInStore[i].DiscountValue;
                            }
                        }
                    }

                    // Store name
                    ws.Range("A" + (index) + ":G" + (index)).Merge().SetValue(String.Format(store.Name));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":G" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    index++;

                    var lstItems = lstData.Where(w => w.StoreId == store.Id).ToList();
                    if (lstItems != null && lstItems.Any())
                    {
                        // Group cate
                        var lstItemGroupCates = lstItems.OrderBy(o => o.CategoryName).GroupBy(gg => gg.CategoryName).ToList();
                        foreach (var itemCate in lstItemGroupCates)
                        {
                            itemSubTotal = new ItemizedSalesDetailTotal();

                            ws.Range("A" + (index) + ":G" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Select(ss => ss.CategoryName).FirstOrDefault()));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":G" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            var lstCateItems = itemCate.ToList();
                            var lstCateDishOrSet = lstCateItems.Where(ww => ww.ItemTypeId == (int)Commons.EProductType.SetMenu
                                                    || ww.ItemTypeId == (int)Commons.EProductType.Dish && ww.ParentId == string.Empty).ToList();

                            var breakfast = new ItemizedSalesDetailValueTotal();
                            var lunch = new ItemizedSalesDetailValueTotal();
                            var dinner = new ItemizedSalesDetailValueTotal();

                            foreach (var item in lstCateDishOrSet)
                            {
                                timeDish = item.CreatedDate.TimeOfDay;
                                if (timeDish >= brearkStart && timeDish < brearkEnd)
                                {
                                    breakfast.Qty += item.Quantity;
                                    breakfast.Amount += item.TotalAmount;

                                    breakfastTotal.Qty += item.Quantity;
                                    breakfastTotal.Amount += item.TotalAmount;
                                }
                                if (timeDish >= lunchStart && timeDish < lunchEnd)
                                {
                                    lunch.Qty += item.Quantity;
                                    lunch.Amount += item.TotalAmount;

                                    lunchTotal.Qty += item.Quantity;
                                    lunchTotal.Amount += item.TotalAmount;
                                }
                                if (dinnerStart > dinnerEnd)//pass day
                                {
                                    if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                    {
                                        dinner.Qty += item.Quantity;
                                        dinner.Amount += item.TotalAmount;

                                        dinnerTotal.Qty += item.Quantity;
                                        dinnerTotal.Amount += item.TotalAmount;
                                    }
                                }
                                else//in day
                                {
                                    if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                    {
                                        dinner.Qty += item.Quantity;
                                        dinner.Amount += item.TotalAmount;

                                        dinnerTotal.Qty += item.Quantity;
                                        dinnerTotal.Amount += item.TotalAmount;
                                    }
                                }
                            }
                            var lstResult = GroupItemSales(lstCateDishOrSet, lstItems);

                            foreach (var item in lstResult)
                            {
                                ws.Cell("A" + index).Value = item.ItemCode;
                                ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                if (item.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                {
                                    ws.Cell("B" + index).Value = item.ItemName;
                                    ws.Cell("E" + index).Value = item.Price;
                                    ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                    ws.Cell("F" + index).Value = item.Quantity;
                                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                    ws.Cell("G" + index).Value = item.TotalAmount;
                                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;

                                    foreach (var itemDish in item.ListChilds)
                                    {
                                        ws.Cell("A" + index).Value = itemDish.ItemCode;
                                        ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        ws.Cell("C" + index).Value = itemDish.ItemName;
                                        ws.Cell("E" + index).Value = itemDish.Price;
                                        ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                        ws.Cell("F" + index).Value = itemDish.Quantity;
                                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                        ws.Cell("G" + index).Value = itemDish.TotalAmount;
                                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                        index++;

                                        foreach (var itemModifierDish in itemDish.ListChilds)
                                        {
                                            ws.Cell("A" + index).Value = itemModifierDish.ItemCode;
                                            ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                            ws.Cell("D" + index).Value = itemModifierDish.ItemName;
                                            ws.Cell("E" + index).Value = itemModifierDish.Price;
                                            ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                            ws.Cell("F" + index).Value = itemModifierDish.Quantity;
                                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                            ws.Cell("G" + index).Value = itemModifierDish.TotalAmount;
                                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                            index++;

                                        }
                                    }

                                }//End set
                                else //dish
                                {
                                    ws.Cell("C" + index).Value = item.ItemName;
                                    ws.Cell("E" + index).Value = item.Price;
                                    ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                    ws.Cell("F" + index).Value = item.Quantity;
                                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                    ws.Cell("G" + index).Value = item.TotalAmount;
                                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;

                                    foreach (var itemModifierDish in item.ListChilds)
                                    {
                                        ws.Cell("A" + index).Value = itemModifierDish.ItemCode;
                                        ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        ws.Cell("D" + index).Value = itemModifierDish.ItemName;
                                        ws.Cell("E" + index).Value = itemModifierDish.Price;
                                        ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                        ws.Cell("F" + index).Value = itemModifierDish.Quantity;
                                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                        ws.Cell("G" + index).Value = itemModifierDish.TotalAmount;
                                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                        index++;

                                    }
                                }
                            }//end loop cate

                            itemSubTotal.BREAKFAST = breakfast;
                            itemSubTotal.LUNCH = lunch;
                            itemSubTotal.DINNER = dinner;

                            // show excel
                            ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total")));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            ws.Cell("F" + (index)).SetValue(itemSubTotal.BREAKFAST.Qty + itemSubTotal.LUNCH.Qty + itemSubTotal.DINNER.Qty);
                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                            ws.Cell("G" + (index)).SetValue(itemSubTotal.BREAKFAST.Amount + itemSubTotal.LUNCH.Amount + itemSubTotal.DINNER.Amount);
                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                            ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST")));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("F" + (index)).SetValue(itemSubTotal.BREAKFAST.Qty);
                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                            ws.Cell("G" + (index)).SetValue(itemSubTotal.BREAKFAST.Amount);
                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                            ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH")));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("F" + (index)).SetValue(itemSubTotal.LUNCH.Qty);
                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                            ws.Cell("G" + (index)).SetValue(itemSubTotal.LUNCH.Amount);
                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                            ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER")));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("F" + (index)).SetValue(itemSubTotal.DINNER.Qty);
                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                            ws.Cell("G" + (index)).SetValue(itemSubTotal.DINNER.Amount);
                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                        }// end cate loop
                    }

                    int startM = index;
                    #region MISC && Discount 
                    miscTotal = 0; miscBreakfast = 0; miscLunch = 0; miscDinner = 0;
                    outletSubTotal = 0;
                    discountTotal = 0; discountBreakfast = 0; discountLunch = 0; discountDinner = 0;

                    if (listMiscDisPeriod != null && listMiscDisPeriod.Any())
                    {
                        var listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreId == store.Id).GroupBy(gg => gg.Period).ToList();
                        if (listMiscDisPeriodByStore != null && listMiscDisPeriodByStore.Any())
                        {
                            foreach (var miscDisc in listMiscDisPeriodByStore)
                            {
                                switch (miscDisc.Key)
                                {
                                    case "BREAKFAST":
                                        miscBreakfast = miscDisc.Sum(ss => ss.MiscTotal);
                                        discountBreakfast = miscDisc.Sum(ss => ss.BillDiscountTotal);
                                        break;

                                    case "LUNCH":
                                        miscLunch = miscDisc.Sum(ss => ss.MiscTotal);
                                        discountLunch = miscDisc.Sum(ss => ss.BillDiscountTotal);
                                        break;

                                    case "DINNER":
                                        miscDinner = miscDisc.Sum(ss => ss.MiscTotal);
                                        discountDinner = miscDisc.Sum(ss => ss.BillDiscountTotal);
                                        break;
                                }
                            }
                        }
                    }
                    miscTotal = miscBreakfast + miscLunch + miscDinner;

                    breakfastTotal.Amount += miscBreakfast;
                    lunchTotal.Amount += miscLunch;
                    dinnerTotal.Amount += miscDinner;

                    outletSubTotal = breakfastTotal.Amount + lunchTotal.Amount + dinnerTotal.Amount;

                    discountTotal = discountBreakfast + discountLunch + discountDinner;

                    breakfastTotal.Amount -= discountBreakfast;
                    lunchTotal.Amount -= discountLunch;
                    dinnerTotal.Amount -= discountDinner;

                    #region MISC
                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                    ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("G" + index).SetValue(miscTotal);
                    index++;
                    if (miscTotal != 0)
                    {
                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST"));
                        ws.Cell("G" + index).SetValue(miscBreakfast);
                        index++;

                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH"));
                        ws.Cell("G" + index).SetValue(miscLunch);
                        index++;

                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER"));
                        ws.Cell("G" + index).SetValue(miscDinner);
                        index++;
                    }
                    #endregion MISC

                    #region Outlet Sub
                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub Total"));
                    ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("G" + index).SetValue(outletSubTotal);
                    index++;
                    #endregion Outlet Sub

                    #region Discount
                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                    ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("G" + index).SetValue(discountTotal * (-1));
                    index++;
                    if (discountTotal != 0)
                    {
                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST"));
                        ws.Cell("G" + index).SetValue(discountBreakfast * (-1));
                        index++;

                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH"));
                        ws.Cell("G" + index).SetValue(discountLunch * (-1));
                        index++;

                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER"));
                        ws.Cell("G" + index).SetValue(discountDinner * (-1));
                        index++;
                    }
                    #endregion Discount
                    #endregion

                    ws.Range("A" + startM + ":G" + (index - 1)).Style.Font.SetBold(true);
                    ws.Range("A" + startM + ":G" + (index - 1)).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + startM + ":D" + (index - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range("E" + startM + ":G" + (index - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    itemTotal.BREAKFAST = breakfastTotal;
                    itemTotal.LUNCH = lunchTotal;
                    itemTotal.DINNER = dinnerTotal;
                    index++;

                    //Total
                    ws.Range("A" + (index - 1) + ":G" + (index - 1)).Merge();

                    ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL")));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell("F" + (index)).SetValue(itemTotal.BREAKFAST.Qty + itemTotal.LUNCH.Qty + itemTotal.DINNER.Qty);
                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Cell("G" + (index)).SetValue(itemTotal.BREAKFAST.Amount + itemTotal.LUNCH.Amount + itemTotal.DINNER.Amount);
                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                    index++;

                    ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST")));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("F" + (index)).SetValue(itemTotal.BREAKFAST.Qty);
                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Cell("G" + (index)).SetValue(itemTotal.BREAKFAST.Amount);
                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                    index++;

                    ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH")));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("F" + (index)).SetValue(itemTotal.LUNCH.Qty);
                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Cell("G" + (index)).SetValue(itemTotal.LUNCH.Amount);
                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                    index++;

                    ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER")));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("F" + (index)).SetValue(itemTotal.DINNER.Qty);
                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Cell("G" + (index)).SetValue(itemTotal.DINNER.Amount);
                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                    index++;
                }
            }
            ws.Columns().AdjustToContents();
            ws.Range("A" + 1 + ":G" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A" + 1 + ":G" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            return wb;
        }

        /// <summary>
        /// Optimized
        /// </summary>
        /// <returns></returns>
        public XLWorkbook ExportExcel_New(BaseReportModel viewmodel
           , List<StoreModels> lstStores,  List<string> lstStoreCates = null
            , List<string> lstStoreSets = null, List<string> lstCateIds = null, List<string> lstCateSetIds = null)
        {
            string sheetName = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized_Sales_Detail_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 7, viewmodel.FromDateFilter, viewmodel.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Detail Itemized Sales Analysis Report"));

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dishes"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifiers"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Range("A" + 5 + ":G" + 5).Style.Font.SetBold(true);
            ws.Row(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            int index = 6;

            //Get data
            List<ItemizedSalesAnalysisReportDetailDataModels> lstData = new List<ItemizedSalesAnalysisReportDetailDataModels>();
            List<ItemizedSalesAnalysisReportDetailDataModels> lstTmp = new List<ItemizedSalesAnalysisReportDetailDataModels>();
            List<DiscountAndMiscReportDataModels> listMisc_Discout = new List<DiscountAndMiscReportDataModels>();
            using (var db = new NuWebContext())
            {
                var request = new BaseReportDataModel() { ListStores = viewmodel.ListStores, FromDate = viewmodel.FromDate, ToDate = viewmodel.ToDate, Mode = viewmodel.Mode };
                lstTmp = db.GetDataForItemSaleDetail(request);
                //discount & misc
                listMisc_Discout = db.GetReceiptDiscountAndMisc(request);
                if(listMisc_Discout == null)
                    listMisc_Discout = new List<DiscountAndMiscReportDataModels>();
                switch (viewmodel.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        listMisc_Discout = listMisc_Discout.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        listMisc_Discout = listMisc_Discout.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                        break;
                }

                listMisc_Discout.ForEach(ss => ss.DiscountValue = 0);
                var lstDiscount = db.GetDiscountTotal(request);
                if (lstDiscount != null && lstDiscount.Any())
                {
                    switch (viewmodel.FilterType)
                    {
                        case (int)Commons.EFilterType.OnDay:
                            lstDiscount = lstDiscount.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                            break;
                        case (int)Commons.EFilterType.Days:
                            lstDiscount = lstDiscount.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                            break;
                    }
                    listMisc_Discout.AddRange(lstDiscount);
                }
            }
            if (lstTmp != null && lstTmp.Any())
            {
                if (lstStoreCates != null && lstStoreCates.Any() && lstCateIds != null && lstCateIds.Any())
                {
                    var lstDish = lstTmp.Where(ww => lstCateIds.Contains(ww.CategoryId) && lstStoreCates.Contains(ww.StoreId)).ToList();
                    lstData.AddRange(lstDish);
                }
                if (lstStoreSets != null && lstStoreSets.Any() && lstCateSetIds != null && lstCateSetIds.Any())
                {
                    var lstSet = lstTmp.Where(ww => lstCateSetIds.Contains(ww.ItemId) && lstStoreSets.Contains(ww.StoreId)).ToList();

                    lstData.AddRange(lstSet);
                }

                switch (viewmodel.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                        break;

                }
            }
            #region Check Period (Breakfast, Lunch, Dinner)
            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];


            TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
            TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
            TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
            TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
            TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
            TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);

            #endregion
            //Total
            ItemizedSalesDetailTotal itemTotal = new ItemizedSalesDetailTotal();
            var breakfastTotal = new ItemizedSalesDetailValueTotal();
            var lunchTotal = new ItemizedSalesDetailValueTotal();
            var dinnerTotal = new ItemizedSalesDetailValueTotal();

            ItemizedSalesDetailTotal itemSubTotal = new ItemizedSalesDetailTotal();

            List<MISCBillDiscountPeriodModels> listMiscDisPeriod = new List<MISCBillDiscountPeriodModels>();
            if (listMisc_Discout != null && listMisc_Discout.Any())
            {
                var listMisc_DiscoutGroupStore = listMisc_Discout.GroupBy(gg => gg.StoreId).ToList();
                foreach (var item in listMisc_DiscoutGroupStore)
                {
                    var lstItemInStore = item.ToList();
                    #region CHECK PERIOD IS CHECKED

                    var miscP = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == item.Key);
                    if (miscP == null)
                    {
                        MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                        itemP.StoreId = item.Key;
                        itemP.MiscTotal = 0;
                        itemP.BillDiscountTotal = 0;
                        itemP.Period = Commons.BREAKFAST;
                        listMiscDisPeriod.Add(itemP);
                    }

                    var miscPLunch = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == item.Key);
                    if (miscPLunch == null)
                    {
                        MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                        itemP.StoreId = item.Key;
                        itemP.MiscTotal = 0;
                        itemP.BillDiscountTotal = 0;
                        itemP.Period = Commons.LUNCH;
                        listMiscDisPeriod.Add(itemP);
                    }

                    var miscPDinner = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == item.Key);
                    if (miscPDinner == null)
                    {
                        MISCBillDiscountPeriodModels itemP = new MISCBillDiscountPeriodModels();
                        itemP.StoreId = item.Key;
                        itemP.MiscTotal = 0;
                        itemP.BillDiscountTotal = 0;
                        itemP.Period = Commons.DINNER;
                        listMiscDisPeriod.Add(itemP);
                    }
                    #endregion
                }
            }

            if (lstData != null && lstData.Any())
            {
                TimeSpan timeDish = new TimeSpan();

                double miscTotal = 0, miscBreakfast = 0, miscLunch = 0, miscDinner = 0;
                double outletSubTotal = 0;
                double discountTotal = 0, discountBreakfast = 0, discountLunch = 0, discountDinner = 0;

                var lstStoreIdData = lstData.Select(s => s.StoreId).Distinct().ToList();
                lstStores = lstStores.Where(w => lstStoreIdData.Contains(w.Id)).OrderBy(o => o.Name).ToList();

                foreach (var store in lstStores)
                {
                    itemTotal = new ItemizedSalesDetailTotal();
                    breakfastTotal = new ItemizedSalesDetailValueTotal();
                    lunchTotal = new ItemizedSalesDetailValueTotal();
                    dinnerTotal = new ItemizedSalesDetailValueTotal();

                    // Get general setting of store
                    if (currentUser != null)
                    {
                        // If store already setting, get value from store setting
                        // Else get value from current user setting
                        Commons.BreakfastStart = currentUser.BreakfastStart;
                        Commons.BreakfastEnd = currentUser.BreakfastEnd;
                        Commons.LunchStart = currentUser.LunchStart;
                        Commons.LunchEnd = currentUser.LunchEnd;
                        Commons.DinnerStart = currentUser.DinnerStart;
                        Commons.DinnerEnd = currentUser.DinnerEnd;

                        // Get time period from general setting
                        if (currentUser.ListSetting != null && currentUser.ListSetting.Any())
                        {
                            var settingPeriodOfStore = currentUser.ListSetting.Where(w => w.StoreID == store.Id).ToList();

                            foreach (var itm in settingPeriodOfStore)
                            {
                                switch (itm.Name)
                                {
                                    case "BreakfastStart":
                                        Commons.BreakfastStart = itm.Value;
                                        break;
                                    case "BreakfastEnd":
                                        Commons.BreakfastEnd = itm.Value;
                                        break;
                                    case "LunchStart":
                                        Commons.LunchStart = itm.Value;
                                        break;
                                    case "LunchEnd":
                                        Commons.LunchEnd = itm.Value;
                                        break;
                                    case "DinnerStart":
                                        Commons.DinnerStart = itm.Value;
                                        break;
                                    case "DinnerEnd":
                                        Commons.DinnerEnd = itm.Value;
                                        break;
                                }
                            }
                        }
                        brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
                        brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
                        lunchStart = TimeSpan.Parse(Commons.LunchStart);
                        lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
                        dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
                        dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);
                    }

                    // Get MISC & Discount of store
                    var lstMiscDiscountInStore = listMisc_Discout.Where(w => w.StoreId == store.Id).ToList();

                    for (int i = 0; i < lstMiscDiscountInStore.Count; i++)
                    {
                        // Get Total Misc to + ItemTotal
                        TimeSpan timeMisc = new TimeSpan(lstMiscDiscountInStore[i].Hour, 0, 0);
                        // Total period Misc_Discout
                        // TOTAL PERIOD MISC AND DISCOUNT BILL TOTAL
                        //// BREAKFAST
                        if (timeMisc >= brearkStart && timeMisc < brearkEnd)
                        {
                            var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == store.Id);
                            period.MiscTotal += lstMiscDiscountInStore[i].MiscValue;
                            period.BillDiscountTotal += lstMiscDiscountInStore[i].DiscountValue;
                        }
                        //// LUNCH
                        if (timeMisc >= lunchStart && timeMisc < lunchEnd)
                        {
                            var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == store.Id);
                            period.MiscTotal += lstMiscDiscountInStore[i].MiscValue;
                            period.BillDiscountTotal += lstMiscDiscountInStore[i].DiscountValue;
                        }
                        //// DINNER
                        if (dinnerStart > dinnerEnd)//pass day
                        {
                            if (timeMisc >= dinnerStart || timeMisc < dinnerEnd)
                            {
                                var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == store.Id);
                                period.MiscTotal += lstMiscDiscountInStore[i].MiscValue;
                                period.BillDiscountTotal += lstMiscDiscountInStore[i].DiscountValue;
                            }
                        }
                        else//in day
                        {
                            if (timeMisc >= dinnerStart && timeMisc < dinnerEnd)
                            {
                                var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == store.Id);
                                period.MiscTotal += lstMiscDiscountInStore[i].MiscValue;
                                period.BillDiscountTotal += lstMiscDiscountInStore[i].DiscountValue;
                            }
                        }
                    }

                    // Store name
                    ws.Range("A" + (index) + ":G" + (index)).Merge().SetValue(String.Format(store.Name));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":G" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    index++;

                    var lstItems = lstData.Where(w => w.StoreId == store.Id).ToList();
                    if (lstItems != null && lstItems.Any())
                    {
                        // Group cate
                        var lstItemGroupCates = lstItems.OrderBy(o => o.CategoryName).GroupBy(gg => gg.CategoryName).ToList();
                        foreach (var itemCate in lstItemGroupCates)
                        {
                            itemSubTotal = new ItemizedSalesDetailTotal();

                            ws.Range("A" + (index) + ":G" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Select(ss => ss.CategoryName).FirstOrDefault()));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":G" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            var lstCateItems = itemCate.ToList();
                            var lstCateDishOrSet = lstCateItems.Where(ww => ww.ItemTypeId == (int)Commons.EProductType.SetMenu
                                                    || ww.ItemTypeId == (int)Commons.EProductType.Dish && ww.ParentId == string.Empty).ToList();

                            var breakfast = new ItemizedSalesDetailValueTotal();
                            var lunch = new ItemizedSalesDetailValueTotal();
                            var dinner = new ItemizedSalesDetailValueTotal();

                            foreach (var item in lstCateDishOrSet)
                            {
                                timeDish = item.CreatedDate.TimeOfDay;
                                if (timeDish >= brearkStart && timeDish < brearkEnd)
                                {
                                    breakfast.Qty += item.Quantity;
                                    breakfast.Amount += item.TotalAmount;

                                    breakfastTotal.Qty += item.Quantity;
                                    breakfastTotal.Amount += item.TotalAmount;
                                }
                                if (timeDish >= lunchStart && timeDish < lunchEnd)
                                {
                                    lunch.Qty += item.Quantity;
                                    lunch.Amount += item.TotalAmount;

                                    lunchTotal.Qty += item.Quantity;
                                    lunchTotal.Amount += item.TotalAmount;
                                }
                                if (dinnerStart > dinnerEnd)//pass day
                                {
                                    if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                    {
                                        dinner.Qty += item.Quantity;
                                        dinner.Amount += item.TotalAmount;

                                        dinnerTotal.Qty += item.Quantity;
                                        dinnerTotal.Amount += item.TotalAmount;
                                    }
                                }
                                else//in day
                                {
                                    if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                    {
                                        dinner.Qty += item.Quantity;
                                        dinner.Amount += item.TotalAmount;

                                        dinnerTotal.Qty += item.Quantity;
                                        dinnerTotal.Amount += item.TotalAmount;
                                    }
                                }
                            }
                            var lstResult = GroupItemSales_New(lstCateDishOrSet, lstItems);

                            foreach (var item in lstResult)
                            {
                                ws.Cell("A" + index).Value = item.ItemCode;
                                ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                if (item.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                {
                                    ws.Cell("B" + index).Value = item.ItemName;
                                    ws.Cell("E" + index).Value = item.Price;
                                    ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                    ws.Cell("F" + index).Value = item.Quantity;
                                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                    ws.Cell("G" + index).Value = item.TotalAmount;
                                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;

                                    foreach (var itemDish in item.ListChilds)
                                    {
                                        ws.Cell("A" + index).Value = itemDish.ItemCode;
                                        ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        ws.Cell("C" + index).Value = itemDish.ItemName;
                                        ws.Cell("E" + index).Value = itemDish.Price;
                                        ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                        ws.Cell("F" + index).Value = itemDish.Quantity;
                                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                        ws.Cell("G" + index).Value = itemDish.TotalAmount;
                                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                        index++;

                                        foreach (var itemModifierDish in itemDish.ListChilds)
                                        {
                                            ws.Cell("A" + index).Value = itemModifierDish.ItemCode;
                                            ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                            ws.Cell("D" + index).Value = itemModifierDish.ItemName;
                                            ws.Cell("E" + index).Value = itemModifierDish.Price;
                                            ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                            ws.Cell("F" + index).Value = itemModifierDish.Quantity;
                                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                            ws.Cell("G" + index).Value = itemModifierDish.TotalAmount;
                                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                            index++;

                                        }
                                    }

                                }//End set
                                else //dish
                                {
                                    ws.Cell("C" + index).Value = item.ItemName;
                                    ws.Cell("E" + index).Value = item.Price;
                                    ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                    ws.Cell("F" + index).Value = item.Quantity;
                                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                    ws.Cell("G" + index).Value = item.TotalAmount;
                                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;

                                    foreach (var itemModifierDish in item.ListChilds)
                                    {
                                        ws.Cell("A" + index).Value = itemModifierDish.ItemCode;
                                        ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        ws.Cell("D" + index).Value = itemModifierDish.ItemName;
                                        ws.Cell("E" + index).Value = itemModifierDish.Price;
                                        ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";

                                        ws.Cell("F" + index).Value = itemModifierDish.Quantity;
                                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";

                                        ws.Cell("G" + index).Value = itemModifierDish.TotalAmount;
                                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                        index++;

                                    }
                                }
                            }//end loop cate

                            itemSubTotal.BREAKFAST = breakfast;
                            itemSubTotal.LUNCH = lunch;
                            itemSubTotal.DINNER = dinner;

                            // show excel
                            ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total")));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            ws.Cell("F" + (index)).SetValue(itemSubTotal.BREAKFAST.Qty + itemSubTotal.LUNCH.Qty + itemSubTotal.DINNER.Qty);
                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                            ws.Cell("G" + (index)).SetValue(itemSubTotal.BREAKFAST.Amount + itemSubTotal.LUNCH.Amount + itemSubTotal.DINNER.Amount);
                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                            ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST")));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("F" + (index)).SetValue(itemSubTotal.BREAKFAST.Qty);
                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                            ws.Cell("G" + (index)).SetValue(itemSubTotal.BREAKFAST.Amount);
                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                            ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH")));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("F" + (index)).SetValue(itemSubTotal.LUNCH.Qty);
                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                            ws.Cell("G" + (index)).SetValue(itemSubTotal.LUNCH.Amount);
                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                            ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER")));
                            ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("F" + (index)).SetValue(itemSubTotal.DINNER.Qty);
                            ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                            ws.Cell("G" + (index)).SetValue(itemSubTotal.DINNER.Amount);
                            ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                        }// end cate loop
                    }

                    int startM = index;
                    #region MISC && Discount 
                    miscTotal = 0; miscBreakfast = 0; miscLunch = 0; miscDinner = 0;
                    outletSubTotal = 0;
                    discountTotal = 0; discountBreakfast = 0; discountLunch = 0; discountDinner = 0;

                    if (listMiscDisPeriod != null && listMiscDisPeriod.Any())
                    {
                        var listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreId == store.Id).GroupBy(gg => gg.Period).ToList();
                        if (listMiscDisPeriodByStore != null && listMiscDisPeriodByStore.Any())
                        {
                            foreach (var miscDisc in listMiscDisPeriodByStore)
                            {
                                switch (miscDisc.Key)
                                {
                                    case "BREAKFAST":
                                        miscBreakfast = miscDisc.Sum(ss => ss.MiscTotal);
                                        discountBreakfast = miscDisc.Sum(ss => ss.BillDiscountTotal);
                                        break;

                                    case "LUNCH":
                                        miscLunch = miscDisc.Sum(ss => ss.MiscTotal);
                                        discountLunch = miscDisc.Sum(ss => ss.BillDiscountTotal);
                                        break;

                                    case "DINNER":
                                        miscDinner = miscDisc.Sum(ss => ss.MiscTotal);
                                        discountDinner = miscDisc.Sum(ss => ss.BillDiscountTotal);
                                        break;
                                }
                            }
                        }
                    }
                    miscTotal = miscBreakfast + miscLunch + miscDinner;

                    breakfastTotal.Amount += miscBreakfast;
                    lunchTotal.Amount += miscLunch;
                    dinnerTotal.Amount += miscDinner;

                    outletSubTotal = breakfastTotal.Amount + lunchTotal.Amount + dinnerTotal.Amount;

                    discountTotal = discountBreakfast + discountLunch + discountDinner;

                    breakfastTotal.Amount -= discountBreakfast;
                    lunchTotal.Amount -= discountLunch;
                    dinnerTotal.Amount -= discountDinner;

                    #region MISC
                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                    ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("G" + index).SetValue(miscTotal);
                    index++;
                    if (miscTotal != 0)
                    {
                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST"));
                        ws.Cell("G" + index).SetValue(miscBreakfast);
                        index++;

                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH"));
                        ws.Cell("G" + index).SetValue(miscLunch);
                        index++;

                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER"));
                        ws.Cell("G" + index).SetValue(miscDinner);
                        index++;
                    }
                    #endregion MISC

                    #region Outlet Sub
                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub Total"));
                    ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("G" + index).SetValue(outletSubTotal);
                    index++;
                    #endregion Outlet Sub

                    #region Discount
                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                    ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("G" + index).SetValue(discountTotal * (-1));
                    index++;
                    if (discountTotal != 0)
                    {
                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST"));
                        ws.Cell("G" + index).SetValue(discountBreakfast * (-1));
                        index++;

                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH"));
                        ws.Cell("G" + index).SetValue(discountLunch * (-1));
                        index++;

                        ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER"));
                        ws.Cell("G" + index).SetValue(discountDinner * (-1));
                        index++;
                    }
                    #endregion Discount
                    #endregion

                    ws.Range("A" + startM + ":G" + (index - 1)).Style.Font.SetBold(true);
                    ws.Range("A" + startM + ":G" + (index - 1)).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + startM + ":D" + (index - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range("E" + startM + ":G" + (index - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    itemTotal.BREAKFAST = breakfastTotal;
                    itemTotal.LUNCH = lunchTotal;
                    itemTotal.DINNER = dinnerTotal;
                    index++;

                    //Total
                    ws.Range("A" + (index - 1) + ":G" + (index - 1)).Merge();

                    ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL")));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell("F" + (index)).SetValue(itemTotal.BREAKFAST.Qty + itemTotal.LUNCH.Qty + itemTotal.DINNER.Qty);
                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Cell("G" + (index)).SetValue(itemTotal.BREAKFAST.Amount + itemTotal.LUNCH.Amount + itemTotal.DINNER.Amount);
                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                    index++;

                    ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST")));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("F" + (index)).SetValue(itemTotal.BREAKFAST.Qty);
                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Cell("G" + (index)).SetValue(itemTotal.BREAKFAST.Amount);
                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                    index++;

                    ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH")));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("F" + (index)).SetValue(itemTotal.LUNCH.Qty);
                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Cell("G" + (index)).SetValue(itemTotal.LUNCH.Amount);
                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                    index++;

                    ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER")));
                    ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("F" + (index)).SetValue(itemTotal.DINNER.Qty);
                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Cell("G" + (index)).SetValue(itemTotal.DINNER.Amount);
                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                    index++;
                }
            }
            ws.Columns().AdjustToContents();
            ws.Range("A" + 1 + ":G" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A" + 1 + ":G" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            return wb;
        }

        public XLWorkbook ExportExcelEmpty(BaseReportModel viewmodel)
        {
            string sheetName = "Itemized_Sales_Detail_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 7, viewmodel.FromDate, viewmodel.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Detail Itemized Sales Analysis Report"));

            // Format header report
            ws.Range(1, 1, 4, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            return wb;
        }

        public List<ItemizedSalesAnalysisReportDetailModels> GroupItemSalesOld(List<ItemizedSalesAnalysisReportDetailModels> lstItemParent, List<ItemizedSalesAnalysisReportDetailModels> lstItems)
        {
            var result = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstParents = new List<ItemizedSalesAnalysisReportDetailModels>();
            ItemizedSalesAnalysisReportDetailModels parent = null;
            foreach (var item in lstItemParent)
            {
                parent = new ItemizedSalesAnalysisReportDetailModels();
                parent.Id = item.Id;
                parent.StoreId = item.StoreId;
                parent.BusinessId = item.BusinessId;
                parent.ItemId = item.ItemId;
                parent.ItemCode = item.ItemCode;
                parent.ItemTypeId = item.ItemTypeId;
                parent.ItemName = item.ItemName;
                parent.ParentId = item.ParentId;
                parent.Price = item.Price;
                parent.Quantity = item.Quantity;
                parent.TotalAmount = item.TotalAmount;
                parent.CategoryId = item.CategoryId;
                parent.CategoryName = item.CategoryName;
                parent.CreatedDate = item.CreatedDate;
                parent.Mode = item.Mode;

                lstParents.Add(parent);
            }

            //for (int a = lstItemParent.Count - 1; a >= 0; a--)
            for (int a = 0; a < lstItemParent.Count; a++)
            {
                var itemParent = lstParents.Where(ww => ww.Id == lstItemParent[a].Id).FirstOrDefault();

                lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                if (itemParent.ItemTypeId == (int)Commons.EProductType.SetMenu)
                {
                    #region SetMenu
                    lstChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == itemParent.Id).ToList();
                    if (lstChild != null && lstChild.Count > 0)
                    {
                        lstChild = lstChild.OrderBy(oo => oo.ItemName).ToList();
                        foreach (var dishItem in lstChild)
                        {
                            lstGrandChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == dishItem.Id).ToList();
                            if (lstGrandChild != null && lstGrandChild.Count > 0)
                            {
                                lstGrandChild = lstGrandChild.OrderBy(oo => oo.ItemName).ToList();
                                dishItem.ListChilds = lstGrandChild;
                            }

                        }
                    }
                    itemParent.ListChilds.AddRange(lstChild);
                    //check order setMenu
                    var lstOrderSet = lstItemParent.Where(ww => ww.ItemTypeId == (int)Commons.EProductType.SetMenu && ww.Id != itemParent.Id
                                && ww.ItemId == itemParent.ItemId).ToList();

                    if (lstOrderSet != null && lstOrderSet.Count > 0)
                    {
                        foreach (var itemSet1 in lstOrderSet)
                        {
                            var itemSet = new ItemizedSalesAnalysisReportDetailModels();
                            itemSet.Id = itemSet1.Id;
                            itemSet.TotalAmount = itemSet1.TotalAmount;
                            itemSet.Quantity = itemSet1.Quantity;

                            //itemSet.ListChilds = new List<ItemizedSalesAnalysisReportDetailModels>();
                            lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                            lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                            lstChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == itemSet.Id).ToList();
                            if (lstChild != null && lstChild.Count > 0)
                            {
                                lstChild = lstChild.OrderBy(oo => oo.ItemName).ToList();
                                foreach (var dishItem in lstChild)
                                {
                                    lstGrandChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == dishItem.Id).ToList();
                                    if (lstGrandChild != null && lstGrandChild.Count > 0)
                                    {
                                        lstGrandChild = lstGrandChild.OrderBy(oo => oo.ItemName).ToList();
                                        dishItem.ListChilds = lstGrandChild;
                                    }
                                }
                            }
                            itemSet.ListChilds.AddRange(lstChild);
                            if (itemParent.ListChilds.Count == itemSet.ListChilds.Count)
                            {
                                bool isGroup = true;
                                for (int i = 0; i < itemParent.ListChilds.Count; i++)
                                {
                                    if (itemParent.ListChilds[i].ItemId == itemSet.ListChilds[i].ItemId)
                                    {
                                        if (itemParent.ListChilds[i].ListChilds.Count == itemSet.ListChilds[i].ListChilds.Count)
                                        {
                                            for (int y = 0; y < itemParent.ListChilds[i].ListChilds.Count; y++)
                                            {
                                                if (itemParent.ListChilds[i].ListChilds[y].ItemId == itemSet.ListChilds[i].ListChilds[y].ItemId)
                                                {

                                                }
                                                else
                                                    isGroup = false;
                                            }
                                        }
                                        else
                                            isGroup = false;
                                    }
                                    else
                                        isGroup = false;
                                }



                                if (isGroup)
                                {
                                    itemParent.TotalAmount += itemSet.TotalAmount;
                                    itemParent.Quantity += itemSet.Quantity;
                                    for (int i = 0; i < itemParent.ListChilds.Count; i++)
                                    {
                                        itemParent.ListChilds[i].Quantity += itemSet.ListChilds[i].Quantity;
                                        itemParent.ListChilds[i].TotalAmount += itemSet.ListChilds[i].TotalAmount;
                                        for (int y = 0; y < itemParent.ListChilds[i].ListChilds.Count; y++)
                                        {
                                            itemParent.ListChilds[i].ListChilds[y].Quantity += itemSet.ListChilds[i].ListChilds[y].Quantity;
                                            itemParent.ListChilds[i].ListChilds[y].TotalAmount += itemSet.ListChilds[i].ListChilds[y].TotalAmount;
                                        }
                                    }

                                    var itemRemove = lstItemParent.Where(ww => ww.Id == itemSet.Id).FirstOrDefault();
                                    if (itemRemove != null)
                                        lstItemParent.Remove(itemRemove);
                                }
                            }

                        }
                    }
                    #endregion End SetMenu
                }
                else //dish
                {
                    lstChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == itemParent.Id).ToList();
                    if (lstChild != null && lstChild.Count > 0)
                    {
                        lstChild = lstChild.OrderBy(oo => oo.ItemName).ToList();
                        foreach (var dishItem in lstChild)
                        {
                            lstGrandChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == dishItem.Id).ToList();
                            if (lstGrandChild != null && lstGrandChild.Count > 0)
                            {
                                lstGrandChild = lstGrandChild.OrderBy(oo => oo.ItemName).ToList();
                                dishItem.ListChilds = lstGrandChild;
                            }

                        }
                    }
                    itemParent.ListChilds.AddRange(lstChild);
                    //check order dish
                    var lstOrderDish = lstItemParent.Where(ww => ww.ItemTypeId == itemParent.ItemTypeId && ww.Id != itemParent.Id
                                && ww.ItemId == itemParent.ItemId).ToList();

                    if (lstOrderDish != null && lstOrderDish.Count > 0)
                    {
                        foreach (var itemSet1 in lstOrderDish)
                        {
                            var itemSet = new ItemizedSalesAnalysisReportDetailModels();
                            itemSet.Id = itemSet1.Id;
                            itemSet.TotalAmount = itemSet1.TotalAmount;
                            itemSet.Quantity = itemSet1.Quantity;

                            //itemSet.ListChilds = new List<ItemizedSalesAnalysisReportDetailModels>();
                            lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                            lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                            lstChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == itemSet.Id).ToList();
                            if (lstChild != null && lstChild.Count > 0)
                            {
                                lstChild = lstChild.OrderBy(oo => oo.ItemName).ToList();
                                foreach (var dishItem in lstChild)
                                {
                                    lstGrandChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == dishItem.Id).ToList();
                                    if (lstGrandChild != null && lstGrandChild.Count > 0)
                                    {
                                        lstGrandChild = lstGrandChild.OrderBy(oo => oo.ItemName).ToList();
                                        dishItem.ListChilds = lstGrandChild;
                                    }
                                }
                            }
                            itemSet.ListChilds.AddRange(lstChild);


                            if (itemParent.ListChilds.Count == itemSet.ListChilds.Count)
                            {
                                bool isGroup = true;
                                for (int i = 0; i < itemParent.ListChilds.Count; i++)
                                {
                                    if (itemParent.ListChilds[i].ItemId == itemSet.ListChilds[i].ItemId)
                                    {
                                    }
                                    else
                                        isGroup = false;
                                }
                                if (isGroup)
                                {
                                    itemParent.TotalAmount += itemSet.TotalAmount;
                                    itemParent.Quantity += itemSet.Quantity;
                                    for (int i = 0; i < itemParent.ListChilds.Count; i++)
                                    {
                                        itemParent.ListChilds[i].Quantity += itemSet.ListChilds[i].Quantity;
                                        itemParent.ListChilds[i].TotalAmount += itemSet.ListChilds[i].TotalAmount;
                                        for (int y = 0; y < itemParent.ListChilds[i].ListChilds.Count; y++)
                                        {
                                            itemParent.ListChilds[i].ListChilds[y].Quantity += itemSet.ListChilds[i].ListChilds[y].Quantity;
                                            itemParent.ListChilds[i].ListChilds[y].TotalAmount += itemSet.ListChilds[i].ListChilds[y].TotalAmount;
                                        }
                                    }

                                    var itemRemove = lstItemParent.Where(ww => ww.Id == itemSet.Id).FirstOrDefault();
                                    if (itemRemove != null)
                                        lstItemParent.Remove(itemRemove);
                                }

                            }
                        }

                    }
                }

                result.Add(itemParent);
            }
            result = result.OrderBy(oo => oo.ItemTypeId).OrderBy(aa => aa.ItemName).ToList();
            return result;
        }

        public List<ItemizedSalesAnalysisReportDetailModels> GroupItemSales(List<ItemizedSalesAnalysisReportDetailModels> lstItemParent, List<ItemizedSalesAnalysisReportDetailModels> lstItems)
        {
            var result = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstTmp = new List<ItemizedSalesAnalysisReportDetailModels>();

            var lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstParents = new List<ItemizedSalesAnalysisReportDetailModels>();
            ItemizedSalesAnalysisReportDetailModels parent = null;

            try
            {
                foreach (var item in lstItemParent)
                {
                    parent = new ItemizedSalesAnalysisReportDetailModels();
                    parent.Id = item.Id;
                    parent.StoreId = item.StoreId;
                    parent.ItemId = item.ItemId;
                    parent.ItemCode = item.ItemCode;
                    parent.ItemTypeId = item.ItemTypeId;
                    parent.ItemName = item.ItemName;
                    parent.Price = item.Price;
                    parent.Quantity = item.Quantity;
                    parent.TotalAmount = item.TotalAmount;
                    parent.CategoryId = item.CategoryId;
                    parent.CategoryName = item.CategoryName;
                    parent.Mode = item.Mode;

                    //add childs
                    lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                    lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();

                    lstChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == item.Id).Select(ss => new ItemizedSalesAnalysisReportDetailModels()
                    {
                        Id = ss.Id,
                        ItemId = ss.ItemId,
                        ItemCode = ss.ItemCode,
                        ItemTypeId = ss.ItemTypeId,
                        ItemName = ss.ItemName,
                        Price = ss.Price,
                        Quantity = ss.Quantity,
                        TotalAmount = ss.TotalAmount,
                        CategoryId = ss.CategoryId,
                        CategoryName = ss.CategoryName,
                        Mode = ss.Mode
                    }).ToList();
                    if (lstChild != null && lstChild.Count > 0)
                    {
                        lstChild = lstChild.OrderBy(oo => oo.ItemName).ToList();
                        foreach (var dishItem in lstChild)
                        {
                            lstGrandChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == dishItem.Id).Select(ss => new ItemizedSalesAnalysisReportDetailModels()
                            {
                                ItemId = ss.ItemId,
                                ItemCode = ss.ItemCode,
                                ItemTypeId = ss.ItemTypeId,
                                ItemName = ss.ItemName,
                                Price = ss.Price,
                                Quantity = ss.Quantity,
                                TotalAmount = ss.TotalAmount,
                                CategoryId = ss.CategoryId,
                                CategoryName = ss.CategoryName,
                                Mode = ss.Mode
                            }).ToList();
                            dishItem.Id = string.Empty;
                            if (lstGrandChild != null && lstGrandChild.Count > 0)
                            {
                                lstGrandChild = lstGrandChild.OrderBy(oo => oo.ItemName).ToList();
                                dishItem.ListChilds = lstGrandChild;
                            }

                        }
                    }
                    parent.ListChilds.AddRange(lstChild);
                    lstParents.Add(parent);
                }
                bool isFirst = true;
                while (lstParents.Any())
                {
                    parent = new ItemizedSalesAnalysisReportDetailModels();
                    if (isFirst)
                    {
                        parent = lstParents.FirstOrDefault();
                        result.Add(parent);
                        lstParents.Remove(parent);
                    }
                    else
                    {
                        parent = result.LastOrDefault();
                        lstTmp = new List<ItemizedSalesAnalysisReportDetailModels>();
                        lstTmp.Add(parent);

                        ItemComparer valueComparer = new ItemComparer();
                        var ItemExists = lstParents.Where(ww => lstTmp.Contains(ww, valueComparer));
                        if (ItemExists != null && ItemExists.Any())
                        {
                            var lstIds = ItemExists.Select(ss => ss.Id).ToList();
                            parent.TotalAmount += ItemExists.Sum(ss => ss.TotalAmount);
                            parent.Quantity += ItemExists.Sum(ss => ss.Quantity);
                            for (int i = 0; i < parent.ListChilds.Count; i++)
                            {
                                parent.ListChilds[i].Quantity += ItemExists.Sum(ss => ss.ListChilds[i].Quantity);
                                parent.ListChilds[i].TotalAmount += ItemExists.Sum(ss => ss.ListChilds[i].TotalAmount);
                                for (int y = 0; y < parent.ListChilds[i].ListChilds.Count; y++)
                                {
                                    parent.ListChilds[i].ListChilds[y].Quantity += ItemExists.Sum(ss => ss.ListChilds[i].ListChilds[y].Quantity);
                                    parent.ListChilds[i].ListChilds[y].TotalAmount += ItemExists.Sum(ss => ss.ListChilds[i].ListChilds[y].TotalAmount);
                                }
                            }
                            lstParents.RemoveAll(rr => lstIds.Any(aa => aa == rr.Id));

                        }
                        else
                        {
                            parent = new ItemizedSalesAnalysisReportDetailModels();
                            parent = lstParents.FirstOrDefault();
                            result.Add(parent);

                            lstParents.Remove(parent);
                        }
                    }
                    isFirst = false;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("Detail item sale", ex);
            }

            result = result.OrderBy(oo => oo.ItemTypeId).OrderBy(aa => aa.ItemName).ToList();
            return result;
        }
        /// <summary>
        /// For optimized
        /// </summary>
        /// <param name="lstItemParent"></param>
        /// <param name="lstItems"></param>
        /// <returns></returns>
        public List<ItemizedSalesAnalysisReportDetailModels> GroupItemSales_New(List<ItemizedSalesAnalysisReportDetailDataModels> lstItemParent
            , List<ItemizedSalesAnalysisReportDetailDataModels> lstItems)
        {
            var result = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstTmp = new List<ItemizedSalesAnalysisReportDetailModels>();

            var lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();
            var lstParents = new List<ItemizedSalesAnalysisReportDetailModels>();
            ItemizedSalesAnalysisReportDetailModels parent = null;

            try
            {
                foreach (var item in lstItemParent)
                {
                    parent = new ItemizedSalesAnalysisReportDetailModels();
                    parent.Id = item.Id;
                    parent.StoreId = item.StoreId;
                    parent.ItemId = item.ItemId;
                    parent.ItemCode = item.ItemCode;
                    parent.ItemTypeId = item.ItemTypeId;
                    parent.ItemName = item.ItemName;
                    parent.Price = item.Price;
                    parent.Quantity = item.Quantity;
                    parent.TotalAmount = item.TotalAmount;
                    parent.CategoryId = item.CategoryId;
                    parent.CategoryName = item.CategoryName;
                    parent.Mode = item.Mode;

                    //add childs
                    lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                    lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();

                    lstChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == item.Id)
                        .Select(ss => new ItemizedSalesAnalysisReportDetailModels()
                    {
                        Id = ss.Id,
                        ItemId = ss.ItemId,
                        ItemCode = ss.ItemCode,
                        ItemTypeId = ss.ItemTypeId,
                        ItemName = ss.ItemName,
                        Price = ss.Price,
                        Quantity = ss.Quantity,
                        TotalAmount = ss.TotalAmount,
                        CategoryId = ss.CategoryId,
                        CategoryName = ss.CategoryName,
                        Mode = ss.Mode
                    }).ToList();
                    if (lstChild != null && lstChild.Count > 0)
                    {
                        lstChild = lstChild.OrderBy(oo => oo.ItemName).ToList();
                        foreach (var dishItem in lstChild)
                        {
                            lstGrandChild = lstItems.Where(ww => ww.ParentId != string.Empty && ww.ParentId == dishItem.Id).Select(ss => new ItemizedSalesAnalysisReportDetailModels()
                            {
                                ItemId = ss.ItemId,
                                ItemCode = ss.ItemCode,
                                ItemTypeId = ss.ItemTypeId,
                                ItemName = ss.ItemName,
                                Price = ss.Price,
                                Quantity = ss.Quantity,
                                TotalAmount = ss.TotalAmount,
                                CategoryId = ss.CategoryId,
                                CategoryName = ss.CategoryName,
                                Mode = ss.Mode
                            }).ToList();
                            dishItem.Id = string.Empty;
                            if (lstGrandChild != null && lstGrandChild.Count > 0)
                            {
                                lstGrandChild = lstGrandChild.OrderBy(oo => oo.ItemName).ToList();
                                dishItem.ListChilds = lstGrandChild;
                            }

                        }
                    }
                    parent.ListChilds.AddRange(lstChild);
                    lstParents.Add(parent);
                }
                bool isFirst = true;
                while (lstParents.Any())
                {
                    parent = new ItemizedSalesAnalysisReportDetailModels();
                    if (isFirst)
                    {
                        parent = lstParents.FirstOrDefault();
                        result.Add(parent);
                        lstParents.Remove(parent);
                    }
                    else
                    {
                        parent = result.LastOrDefault();
                        lstTmp = new List<ItemizedSalesAnalysisReportDetailModels>();
                        lstTmp.Add(parent);

                        ItemComparer valueComparer = new ItemComparer();
                        var ItemExists = lstParents.Where(ww => lstTmp.Contains(ww, valueComparer));
                        if (ItemExists != null && ItemExists.Any())
                        {
                            var lstIds = ItemExists.Select(ss => ss.Id).ToList();
                            parent.TotalAmount += ItemExists.Sum(ss => ss.TotalAmount);
                            parent.Quantity += ItemExists.Sum(ss => ss.Quantity);
                            for (int i = 0; i < parent.ListChilds.Count; i++)
                            {
                                parent.ListChilds[i].Quantity += ItemExists.Sum(ss => ss.ListChilds[i].Quantity);
                                parent.ListChilds[i].TotalAmount += ItemExists.Sum(ss => ss.ListChilds[i].TotalAmount);
                                for (int y = 0; y < parent.ListChilds[i].ListChilds.Count; y++)
                                {
                                    parent.ListChilds[i].ListChilds[y].Quantity += ItemExists.Sum(ss => ss.ListChilds[i].ListChilds[y].Quantity);
                                    parent.ListChilds[i].ListChilds[y].TotalAmount += ItemExists.Sum(ss => ss.ListChilds[i].ListChilds[y].TotalAmount);
                                }
                            }
                            lstParents.RemoveAll(rr => lstIds.Any(aa => aa == rr.Id));

                        }
                        else
                        {
                            parent = new ItemizedSalesAnalysisReportDetailModels();
                            parent = lstParents.FirstOrDefault();
                            result.Add(parent);

                            lstParents.Remove(parent);
                        }
                    }
                    isFirst = false;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("Detail item sale", ex);
            }

            result = result.OrderBy(oo => oo.ItemTypeId).OrderBy(aa => aa.ItemName).ToList();
            return result;
        }

        public class ItemComparer : IEqualityComparer<ItemizedSalesAnalysisReportDetailModels>
        {
            public bool EqualsOld(ItemizedSalesAnalysisReportDetailModels x, ItemizedSalesAnalysisReportDetailModels y)
            {
                try
                {


                    if (x.ItemName == y.ItemName && x.ItemTypeId == y.ItemTypeId)
                    {
                        if (x.ListChilds.Count == y.ListChilds.Count)
                        {
                            if (x.ItemTypeId == (int)Commons.EProductType.SetMenu)
                            {
                                for (int i = 0; i < x.ListChilds.Count; i++)
                                {
                                    if (x.ListChilds[i].ItemId != y.ListChilds[i].ItemId)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        for (int a = 0; a < x.ListChilds[i].ListChilds.Count; a++)
                                        {
                                            if (x.ListChilds[i].ListChilds.Count == y.ListChilds[i].ListChilds.Count)
                                            {
                                                if (x.ListChilds[i].ListChilds[a].ItemId != y.ListChilds[i].ListChilds[a].ItemId)
                                                {
                                                    return false;
                                                }
                                            }
                                            else
                                                return false;
                                        }
                                    }

                                }
                            }
                            else //dish
                            {
                                for (int i = 0; i < x.ListChilds.Count; i++)
                                {
                                    if (x.ListChilds[i].ItemId != y.ListChilds[i].ItemId)
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        else
                            return false;

                    }
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("Comparer item sale", ex);
                }
                return false;
            }
            public bool Equals(ItemizedSalesAnalysisReportDetailModels x, ItemizedSalesAnalysisReportDetailModels y)
            {
                try
                {
                    if (x.ItemName == y.ItemName && x.ItemTypeId == y.ItemTypeId && x.Price == y.Price)
                    {
                        if (x.ListChilds.Count == y.ListChilds.Count)
                        {
                            if (x.ItemTypeId == (int)Commons.EProductType.SetMenu)
                            {
                                for (int i = 0; i < x.ListChilds.Count; i++)
                                {
                                    if (x.ListChilds[i].ItemName != y.ListChilds[i].ItemName || x.ListChilds[i].Price != y.ListChilds[i].Price)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        for (int a = 0; a < x.ListChilds[i].ListChilds.Count; a++)
                                        {
                                            if (x.ListChilds[i].ListChilds.Count == y.ListChilds[i].ListChilds.Count)
                                            {
                                                if (x.ListChilds[i].ListChilds[a].ItemName != y.ListChilds[i].ListChilds[a].ItemName || x.ListChilds[i].ListChilds[a].Price != y.ListChilds[i].ListChilds[a].Price)
                                                {
                                                    return false;
                                                }
                                            }
                                            else
                                                return false;
                                        }
                                    }

                                }
                            }
                            else //dish
                            {
                                for (int i = 0; i < x.ListChilds.Count; i++)
                                {
                                    if (x.ListChilds[i].ItemName != y.ListChilds[i].ItemName || x.ListChilds[i].Price != y.ListChilds[i].Price)
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        else
                            return false;

                    }
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("Comparer item sale", ex);
                }
                return false;
            }
            public int GetHashCode(ItemizedSalesAnalysisReportDetailModels obj)
            {
                return 0;
            }
        }

        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]
        public List<ItemizedSalesAnalysisReportDetailModels> GetData_NewDB(DateTime dFrom, DateTime dTo
            , List<string> lstStoreCate, List<string> lstStoreSet, List<string> lstCateIds, List<string> lstCateSetIds, List<string> lstBusDayId, int mode = 1)
        {
            using (var cxt = new NuWebContext())
            {
                List<ItemizedSalesAnalysisReportDetailModels> lstReturn = new List<ItemizedSalesAnalysisReportDetailModels>();

                if ((lstStoreCate != null && lstStoreCate.Any() && lstCateIds != null && lstCateIds.Any())
                    || (lstStoreSet != null && lstStoreSet.Any() && lstCateSetIds != null && lstCateSetIds.Any()))
                {
                    lstReturn = (from ps in cxt.R_PosSale
                                 from psd in cxt.R_PosSaleDetail.Where(w => w.StoreId == ps.StoreId && w.OrderId == ps.OrderId)
                                 where (((lstCateSetIds.Contains(psd.ItemId) && lstStoreSet.Contains(psd.StoreId))
                                          || (lstCateIds.Contains(psd.CategoryId) && lstStoreCate.Contains(psd.StoreId)))
                                          && psd.Mode == mode
                                          && psd.ItemTypeId != (int)Commons.EProductType.Misc
                                          && ps.ReceiptCreatedDate >= dFrom && ps.ReceiptCreatedDate <= dTo
                                          && lstBusDayId.Contains(psd.BusinessId))
                                 select new ItemizedSalesAnalysisReportDetailModels
                                 {
                                     Id = psd.Id,
                                     CreatedDate = ps.ReceiptCreatedDate.Value,
                                     BusinessId = psd.BusinessId,
                                     StoreId = psd.StoreId,
                                     CategoryId = psd.CategoryId,
                                     CategoryName = psd.CategoryName,
                                     ItemTypeId = psd.ItemTypeId,
                                     ItemId = psd.ItemId,
                                     ItemCode = psd.ItemCode,
                                     ItemName = psd.ItemName,
                                     OrderDetailId = psd.OrderDetailId,
                                     ParentId = psd.ParentId,
                                     Quantity = psd.Quantity,
                                     Price = psd.Price,
                                     TotalAmount = (psd.TotalAmount - (psd.IsDiscountTotal.HasValue && psd.IsDiscountTotal.Value ? 0 : psd.Discount) - psd.PromotionAmount) + psd.ExtraPrice,
                                     ReceiptId = psd.OrderId,
                                     CreditNoteNo = ps.CreditNoteNo
                                 }).ToList();
                }

                return lstReturn;
            }
        }

        public XLWorkbook ExportExcel_NewDB(BaseReportModel model, List<StoreModels> lstStores, List<string> _lstStoreCate, List<string> _lstStoreSet, List<string> _lstCateCheckedId, List<string> _lstSetCheckedId)
        {
            string sheetName = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized_Sales_Detail_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 7, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Detail Itemized Sales Analysis Report"));

            // Format header report
            ws.Range(1, 1, 4, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            if ((_lstStoreCate == null || !_lstStoreCate.Any() || _lstCateCheckedId == null || !_lstCateCheckedId.Any())
                    && (_lstStoreSet == null || !_lstStoreSet.Any() || _lstSetCheckedId == null || !_lstSetCheckedId.Any()))
            {
                return wb;
            }

            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return wb;
            }

            model.ToDate = _lstBusDayAllStore.Max(aa => aa.DateTo);
            model.FromDate = _lstBusDayAllStore.Min(aa => aa.DateFrom);
            var lstBusinessId = _lstBusDayAllStore.Select(ss => ss.Id).ToList();

            // Get data
            var lstData = GetData_NewDB(model.FromDate, model.ToDate, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId, lstBusinessId, model.Mode);
            if (lstData != null && lstData.Any())
            {
                switch (model.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        lstData = lstData.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        break;
                }
            }
            if (lstData == null || !lstData.Any())
            {
                return wb;
            }
            var lstReceiptId = lstData.Select(s => s.ReceiptId).Distinct().ToList();

            PosSaleFactory posSaleFactory = new PosSaleFactory();
            var listMisc_Discount = posSaleFactory.GetMiscDiscount(model.ListStores, lstReceiptId, model.Mode);

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dishes"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifiers"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Range("A" + 5 + ":G" + 5).Style.Font.SetBold(true);
            ws.Row(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            int index = 6;

            #region Check Period (Breakfast, Lunch, Dinner)
            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];

            // Get time period from common
            TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
            TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
            TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
            TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
            TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
            TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);
            #endregion

            // For a store
            ItemizedSalesDetailTotal itemTotal = new ItemizedSalesDetailTotal();
            var breakfastTotal = new ItemizedSalesDetailValueTotal();
            var lunchTotal = new ItemizedSalesDetailValueTotal();
            var dinnerTotal = new ItemizedSalesDetailValueTotal();
            var listMisc_DiscountOfStore = new List<DiscountAndMiscReportModels>();
            double miscTotal = 0, outletSubTotal = 0, discountTotal = 0;

            // For a category
            ItemizedSalesDetailTotal itemSubTotal = new ItemizedSalesDetailTotal();
            var breakfast = new ItemizedSalesDetailValueTotal();
            var lunch = new ItemizedSalesDetailValueTotal();
            var dinner = new ItemizedSalesDetailValueTotal();

            // For item period
            List<ItemizedSalesAnalysisReportDetailModels> lstItmPeriod = new List<ItemizedSalesAnalysisReportDetailModels>();
            double qtyItmPeriod = 0, amountItmPeriod = 0;

            // For Misc & Discount period
            var listMisc_DiscountPeriod = new List<DiscountAndMiscReportModels>();
            double miscBreakfast = 0, miscLunch = 0, miscDinner = 0;
            double discountBreakfast = 0, discountLunch = 0, discountDinner = 0;

            var lstStoreIdData = lstData.Select(s => s.StoreId).Distinct().ToList();
            lstStores = lstStores.Where(w => lstStoreIdData.Contains(w.Id)).OrderBy(o => o.Name).ToList();

            foreach (var store in lstStores)
            {
                itemTotal = new ItemizedSalesDetailTotal();
                breakfastTotal = new ItemizedSalesDetailValueTotal();
                lunchTotal = new ItemizedSalesDetailValueTotal();
                dinnerTotal = new ItemizedSalesDetailValueTotal();

                // Get store setting : time of Period
                if (currentUser != null)
                {
                    // If store already setting, get value from store setting
                    // Else get value from current user setting
                    Commons.BreakfastStart = currentUser.BreakfastStart;
                    Commons.BreakfastEnd = currentUser.BreakfastEnd;
                    Commons.LunchStart = currentUser.LunchStart;
                    Commons.LunchEnd = currentUser.LunchEnd;
                    Commons.DinnerStart = currentUser.DinnerStart;
                    Commons.DinnerEnd = currentUser.DinnerEnd;

                    // Get time period from general setting
                    if (currentUser.ListSetting != null && currentUser.ListSetting.Any())
                    {
                        var settingPeriodOfStore = currentUser.ListSetting.Where(w => w.StoreID == store.Id).ToList();

                        foreach (var itm in settingPeriodOfStore)
                        {
                            switch (itm.Name)
                            {
                                case "BreakfastStart":
                                    Commons.BreakfastStart = itm.Value;
                                    break;
                                case "BreakfastEnd":
                                    Commons.BreakfastEnd = itm.Value;
                                    break;
                                case "LunchStart":
                                    Commons.LunchStart = itm.Value;
                                    break;
                                case "LunchEnd":
                                    Commons.LunchEnd = itm.Value;
                                    break;
                                case "DinnerStart":
                                    Commons.DinnerStart = itm.Value;
                                    break;
                                case "DinnerEnd":
                                    Commons.DinnerEnd = itm.Value;
                                    break;
                            }
                        }
                    }
                    brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
                    brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
                    lunchStart = TimeSpan.Parse(Commons.LunchStart);
                    lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
                    dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
                    dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);
                }

                // Store name
                ws.Range("A" + (index) + ":G" + (index)).Merge().SetValue(String.Format(store.Name));
                ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                ws.Range("A" + (index) + ":G" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                ws.Range("A" + (index) + ":G" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                index++;

                var lstItemsStore = lstData.Where(w => w.StoreId == store.Id).ToList();
                if (lstItemsStore != null && lstItemsStore.Any())
                {
                    // Group cate
                    var lstItemGroupCates = lstItemsStore.OrderBy(o => o.CategoryName).GroupBy(gg => new { gg.CategoryId, gg.CategoryName }).ToList();
                    foreach (var itemCate in lstItemGroupCates)
                    {
                        itemSubTotal = new ItemizedSalesDetailTotal();
                        breakfast = new ItemizedSalesDetailValueTotal();
                        lunch = new ItemizedSalesDetailValueTotal();
                        dinner = new ItemizedSalesDetailValueTotal();

                        ws.Range("A" + (index) + ":G" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Key.CategoryName));
                        ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":G" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A" + (index) + ":G" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        index++;

                        var lstCateDishOrSet = itemCate.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.SetMenu
                                                || ww.ItemTypeId == (int)Commons.EProductType.Dish) && ww.ParentId == string.Empty).OrderBy(o => o.ItemName).ToList();

                        // Breakfast
                        lstItmPeriod = lstCateDishOrSet.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd).ToList();
                        qtyItmPeriod = lstItmPeriod.Sum(ss => ss.Quantity);
                        amountItmPeriod = lstItmPeriod.Sum(ss => ss.TotalAmount);

                        breakfast.Qty += qtyItmPeriod;
                        breakfast.Amount += amountItmPeriod;

                        breakfastTotal.Qty += qtyItmPeriod;
                        breakfastTotal.Amount += amountItmPeriod;

                        // Lunch
                        lstItmPeriod = lstCateDishOrSet.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd).ToList();
                        qtyItmPeriod = lstItmPeriod.Sum(ss => ss.Quantity);
                        amountItmPeriod = lstItmPeriod.Sum(ss => ss.TotalAmount);

                        lunch.Qty += qtyItmPeriod;
                        lunch.Amount += amountItmPeriod;

                        lunchTotal.Qty += qtyItmPeriod;
                        lunchTotal.Amount += amountItmPeriod;

                        // Dinner
                        lstItmPeriod = lstCateDishOrSet.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                    || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd))).ToList(); // pass day
                        qtyItmPeriod = lstItmPeriod.Sum(ss => ss.Quantity);
                        amountItmPeriod = lstItmPeriod.Sum(ss => ss.TotalAmount);

                        dinner.Qty += qtyItmPeriod;
                        dinner.Amount += amountItmPeriod;

                        dinnerTotal.Qty += qtyItmPeriod;
                        dinnerTotal.Amount += amountItmPeriod;

                        // Group item sale 
                        var lstResult = GroupItemSales_NewDB(lstCateDishOrSet, lstItemsStore);

                        foreach (var item in lstResult)
                        {
                            // Item code
                            ws.Cell("A" + index).Value = item.ItemCode;
                            ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            // If item is a Set Menu
                            if (item.ItemTypeId == (int)Commons.EProductType.SetMenu)
                            {
                                // Item name
                                ws.Cell("B" + index).Value = item.ItemName;
                                // Item price
                                ws.Cell("E" + index).Value = item.Price;
                                ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";
                                // Quantity
                                ws.Cell("F" + index).Value = item.Quantity;
                                ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                                // Amount
                                ws.Cell("G" + index).Value = item.TotalAmount;
                                ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;

                                // Dish in Set Menu
                                foreach (var itemDish in item.ListChilds)
                                {
                                    // Item code
                                    ws.Cell("A" + index).Value = itemDish.ItemCode;
                                    ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    // Item name
                                    ws.Cell("C" + index).Value = itemDish.ItemName;
                                    // Item price
                                    ws.Cell("E" + index).Value = itemDish.Price;
                                    ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";
                                    // Quantity
                                    ws.Cell("F" + index).Value = itemDish.Quantity;
                                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                                    // Amount
                                    ws.Cell("G" + index).Value = itemDish.TotalAmount;
                                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;

                                    // Modifier of Dish in Set Menu
                                    foreach (var itemModifierDish in itemDish.ListChilds)
                                    {
                                        // Item code
                                        ws.Cell("A" + index).Value = itemModifierDish.ItemCode;
                                        ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        // Item name
                                        ws.Cell("D" + index).Value = itemModifierDish.ItemName;
                                        // Item price
                                        ws.Cell("E" + index).Value = itemModifierDish.Price;
                                        ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";
                                        // Quantity
                                        ws.Cell("F" + index).Value = itemModifierDish.Quantity;
                                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                                        // Amount
                                        ws.Cell("G" + index).Value = itemModifierDish.TotalAmount;
                                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                        index++;
                                    }
                                }
                            }
                            else // If item is a Dish
                            {
                                // Item name
                                ws.Cell("C" + index).Value = item.ItemName;
                                // Item price
                                ws.Cell("E" + index).Value = item.Price;
                                ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";
                                // Quantity
                                ws.Cell("F" + index).Value = item.Quantity;
                                ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                                // Amount
                                ws.Cell("G" + index).Value = item.TotalAmount;
                                ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;

                                // Modifier of Dish
                                foreach (var itemModifierDish in item.ListChilds)
                                {
                                    // Item code
                                    ws.Cell("A" + index).Value = itemModifierDish.ItemCode;
                                    ws.Cell("A" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    // Item name
                                    ws.Cell("D" + index).Value = itemModifierDish.ItemName;
                                    // Item price
                                    ws.Cell("E" + index).Value = itemModifierDish.Price;
                                    ws.Cell("E" + index).Style.NumberFormat.Format = "#,##0.00";
                                    // Quantity
                                    ws.Cell("F" + index).Value = itemModifierDish.Quantity;
                                    ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                                    // Amount
                                    ws.Cell("G" + index).Value = itemModifierDish.TotalAmount;
                                    ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;
                                }
                            }
                        }//end loop cate

                        itemSubTotal.BREAKFAST = breakfast;
                        itemSubTotal.LUNCH = lunch;
                        itemSubTotal.DINNER = dinner;

                        // Sub total of a category
                        ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total")));
                        ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell("F" + (index)).SetValue(itemSubTotal.BREAKFAST.Qty + itemSubTotal.LUNCH.Qty + itemSubTotal.DINNER.Qty);
                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Cell("G" + (index)).SetValue(itemSubTotal.BREAKFAST.Amount + itemSubTotal.LUNCH.Amount + itemSubTotal.DINNER.Amount);
                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                        index++;

                        ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST")));
                        ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("F" + (index)).SetValue(itemSubTotal.BREAKFAST.Qty);
                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Cell("G" + (index)).SetValue(itemSubTotal.BREAKFAST.Amount);
                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                        index++;

                        ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH")));
                        ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("F" + (index)).SetValue(itemSubTotal.LUNCH.Qty);
                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Cell("G" + (index)).SetValue(itemSubTotal.LUNCH.Amount);
                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                        index++;

                        ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER")));
                        ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("F" + (index)).SetValue(itemSubTotal.DINNER.Qty);
                        ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Cell("G" + (index)).SetValue(itemSubTotal.DINNER.Amount);
                        ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                        index++;

                    }// end cate loop
                }

                int startM = index;
                #region MISC & Discount 
                // Get list MISC & Discount period of store
                miscTotal = 0; miscBreakfast = 0; miscLunch = 0; miscDinner = 0;
                outletSubTotal = 0;
                discountTotal = 0; discountBreakfast = 0; discountLunch = 0; discountDinner = 0;

                if (listMisc_Discount != null && listMisc_Discount.Any())
                {
                    listMisc_DiscountOfStore = listMisc_Discount.Where(w => w.StoreId == store.Id).ToList();
                    if (listMisc_DiscountOfStore != null && listMisc_DiscountOfStore.Any())
                    {
                        //// Breakfast
                        listMisc_DiscountPeriod = listMisc_DiscountOfStore.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd).ToList();
                        miscBreakfast = listMisc_DiscountPeriod.Where(w => w.ItemTypeId == (int)Commons.EProductType.Misc).Sum(s => s.MiscValue);
                        discountBreakfast = listMisc_DiscountPeriod.Where(w => w.IsDiscountTotal).Sum(s => s.DiscountValue);

                        //// Lunch
                        listMisc_DiscountPeriod = listMisc_DiscountOfStore.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd).ToList();
                        miscLunch = listMisc_DiscountPeriod.Where(w => w.ItemTypeId == (int)Commons.EProductType.Misc).Sum(s => s.MiscValue);
                        discountLunch = listMisc_DiscountPeriod.Where(w => w.IsDiscountTotal).Sum(s => s.DiscountValue);

                        //// Dinner
                        listMisc_DiscountPeriod = listMisc_DiscountOfStore.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                    || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd))).ToList(); // pass day
                        miscDinner = listMisc_DiscountPeriod.Where(w => w.ItemTypeId == (int)Commons.EProductType.Misc).Sum(s => s.MiscValue);
                        discountDinner = listMisc_DiscountPeriod.Where(w => w.IsDiscountTotal).Sum(s => s.DiscountValue);
                    }
                }

                miscTotal = miscBreakfast + miscLunch + miscDinner;

                breakfastTotal.Amount += miscBreakfast;
                lunchTotal.Amount += miscLunch;
                dinnerTotal.Amount += miscDinner;

                outletSubTotal = breakfastTotal.Amount + lunchTotal.Amount + dinnerTotal.Amount;

                discountTotal = discountBreakfast + discountLunch + discountDinner;

                breakfastTotal.Amount -= discountBreakfast;
                lunchTotal.Amount -= discountLunch;
                dinnerTotal.Amount -= discountDinner;

                #region MISC
                ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Cell("G" + index).SetValue(miscTotal);
                index++;

                if (miscTotal != 0)
                {
                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST"));
                    ws.Cell("G" + index).SetValue(miscBreakfast);
                    index++;

                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH"));
                    ws.Cell("G" + index).SetValue(miscLunch);
                    index++;

                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER"));
                    ws.Cell("G" + index).SetValue(miscDinner);
                    index++;
                }
                #endregion MISC

                #region Outlet Sub
                ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub Total"));
                ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Cell("G" + index).SetValue(outletSubTotal);
                index++;
                #endregion Outlet Sub

                #region Discount
                ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                ws.Range("A" + index + ":G" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                ws.Cell("G" + index).SetValue(discountTotal * (-1));
                index++;
                if (discountTotal != 0)
                {
                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST"));
                    ws.Cell("G" + index).SetValue(discountBreakfast * (-1));
                    index++;

                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH"));
                    ws.Cell("G" + index).SetValue(discountLunch * (-1));
                    index++;

                    ws.Range("A" + index + ":E" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER"));
                    ws.Cell("G" + index).SetValue(discountDinner * (-1));
                    index++;
                }
                #endregion Discount

                ws.Range("A" + startM + ":G" + (index - 1)).Style.Font.SetBold(true);
                ws.Range("A" + startM + ":G" + (index - 1)).Style.NumberFormat.Format = "#,##0.00";
                ws.Range("A" + startM + ":D" + (index - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range("E" + startM + ":G" + (index - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                #endregion MISC & Discount 

                itemTotal.BREAKFAST = breakfastTotal;
                itemTotal.LUNCH = lunchTotal;
                itemTotal.DINNER = dinnerTotal;
                index++;

                // Total Store
                ws.Range("A" + (index - 1) + ":G" + (index - 1)).Merge();

                ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL")));
                ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell("F" + (index)).SetValue(itemTotal.BREAKFAST.Qty + itemTotal.LUNCH.Qty + itemTotal.DINNER.Qty);
                ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                ws.Cell("G" + (index)).SetValue(itemTotal.BREAKFAST.Amount + itemTotal.LUNCH.Amount + itemTotal.DINNER.Amount);
                ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                index++;

                ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BREAKFAST")));
                ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("F" + (index)).SetValue(itemTotal.BREAKFAST.Qty);
                ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                ws.Cell("G" + (index)).SetValue(itemTotal.BREAKFAST.Amount);
                ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                index++;

                ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("LUNCH")));
                ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("F" + (index)).SetValue(itemTotal.LUNCH.Qty);
                ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                ws.Cell("G" + (index)).SetValue(itemTotal.LUNCH.Amount);
                ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                index++;

                ws.Range("A" + (index) + ":E" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("DINNER")));
                ws.Range("A" + (index) + ":G" + (index)).Style.Font.SetBold(true);
                ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("F" + (index)).SetValue(itemTotal.DINNER.Qty);
                ws.Cell("F" + index).Style.NumberFormat.Format = "#,##0";
                ws.Cell("G" + (index)).SetValue(itemTotal.DINNER.Amount);
                ws.Cell("G" + index).Style.NumberFormat.Format = "#,##0.00";
                index++;
            }

            ws.Columns().AdjustToContents();
            ws.Range("A" + 1 + ":G" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A" + 1 + ":G" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            return wb;
        }

        public List<ItemizedSalesAnalysisReportDetailModels> GroupItemSales_NewDB(List<ItemizedSalesAnalysisReportDetailModels> lstItemParent, List<ItemizedSalesAnalysisReportDetailModels> lstItems)
        {
            var result = new List<ItemizedSalesAnalysisReportDetailModels>();
            try
            {
                var lstTmp = new List<ItemizedSalesAnalysisReportDetailModels>();

                var lstChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                var lstGrandChild = new List<ItemizedSalesAnalysisReportDetailModels>();
                var lstParents = new List<ItemizedSalesAnalysisReportDetailModels>();

                foreach (var parent in lstItemParent)
                {
                    lstChild = lstItems.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == parent.OrderDetailId)
                        .Select(ss => new ItemizedSalesAnalysisReportDetailModels()
                        {
                            Id = ss.Id,
                            ItemId = ss.ItemId,
                            ItemCode = ss.ItemCode,
                            ItemTypeId = ss.ItemTypeId,
                            ItemName = ss.ItemName,
                            Price = ss.Price,
                            Quantity = ss.Quantity,
                            TotalAmount = ss.TotalAmount,
                            CategoryId = ss.CategoryId,
                            CategoryName = ss.CategoryName,
                            Mode = ss.Mode
                        }).OrderBy(oo => oo.ItemName).ToList();

                    if (lstChild != null && lstChild.Any())
                    {
                        foreach (var dishItem in lstChild)
                        {
                            lstGrandChild = lstItems.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == dishItem.OrderDetailId)
                                .Select(ss => new ItemizedSalesAnalysisReportDetailModels()
                                {
                                    ItemId = ss.ItemId,
                                    ItemCode = ss.ItemCode,
                                    ItemTypeId = ss.ItemTypeId,
                                    ItemName = ss.ItemName,
                                    Price = ss.Price,
                                    Quantity = ss.Quantity,
                                    TotalAmount = ss.TotalAmount,
                                    CategoryId = ss.CategoryId,
                                    CategoryName = ss.CategoryName,
                                    Mode = ss.Mode
                                }).OrderBy(oo => oo.ItemName).ToList();

                            dishItem.Id = string.Empty;
                            if (lstGrandChild != null && lstGrandChild.Any())
                            {
                                dishItem.ListChilds.AddRange(lstGrandChild);
                            }

                            // Sum grand childs amount for child
                            dishItem.TotalAmount = dishItem.TotalAmount + dishItem.ListChilds.Sum(ss => ss.TotalAmount);

                        }
                    }
                    parent.ListChilds.AddRange(lstChild);

                    // Sum childs amount for parent
                    parent.TotalAmount = parent.TotalAmount + parent.ListChilds.Sum(ss => ss.TotalAmount);
                    lstParents.Add(parent);
                }

                bool isFirst = true;
                while (lstParents.Any())
                {
                    ItemizedSalesAnalysisReportDetailModels item = new ItemizedSalesAnalysisReportDetailModels();
                    if (isFirst)
                    {
                        item = lstParents.FirstOrDefault();
                        result.Add(item);
                        lstParents.Remove(item);
                    }
                    else
                    {
                        item = result.LastOrDefault();
                        lstTmp = new List<ItemizedSalesAnalysisReportDetailModels>();
                        lstTmp.Add(item);

                        ItemComparer valueComparer = new ItemComparer();
                        var ItemExists = lstParents.Where(ww => lstTmp.Contains(ww, valueComparer));
                        if (ItemExists != null && ItemExists.Any())
                        {
                            var lstIds = ItemExists.Select(ss => ss.Id).ToList();
                            item.TotalAmount += ItemExists.Sum(ss => ss.TotalAmount);
                            item.Quantity += ItemExists.Sum(ss => ss.Quantity);
                            for (int i = 0; i < item.ListChilds.Count; i++)
                            {
                                item.ListChilds[i].Quantity += ItemExists.Sum(ss => ss.ListChilds[i].Quantity);
                                item.ListChilds[i].TotalAmount += ItemExists.Sum(ss => ss.ListChilds[i].TotalAmount);
                                for (int y = 0; y < item.ListChilds[i].ListChilds.Count; y++)
                                {
                                    item.ListChilds[i].ListChilds[y].Quantity += ItemExists.Sum(ss => ss.ListChilds[i].ListChilds[y].Quantity);
                                    item.ListChilds[i].ListChilds[y].TotalAmount += ItemExists.Sum(ss => ss.ListChilds[i].ListChilds[y].TotalAmount);
                                }
                            }
                            lstParents.RemoveAll(rr => lstIds.Any(aa => aa == rr.Id));
                        }
                        else
                        {
                            item = new ItemizedSalesAnalysisReportDetailModels();
                            item = lstParents.FirstOrDefault();
                            result.Add(item);

                            lstParents.Remove(item);
                        }
                    }
                    isFirst = false;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ItemizedSalesAnalysisReportDetail GroupItemSales_NewDB", ex);
            }

            result = result.OrderBy(oo => oo.ItemTypeId).OrderBy(aa => aa.ItemName).ToList();
            return result;
        }


        #endregion
    }
}
