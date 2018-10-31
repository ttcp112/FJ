using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using System.Data.Entity.Core.Objects;
using ClosedXML.Excel;
using NuWebNCloud.Shared.Models.ItemizedSalesAnalysisReport;
using System.Data.Entity.SqlServer;
using NuWebNCloud.Shared.Utilities;
using System.Data.Entity;
using NuWebNCloud.Data.Models;

namespace NuWebNCloud.Shared.Factory
{
    public class ItemizedSalesAnalysisReportFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private NoIncludeOnSaleDataFactory _noIncludeOnSaleDataFactory;
        private OrderPaymentMethodFactory _orderPaymentMethodFactory;
        private RefundFactory _refundFactory;
        public ItemizedSalesAnalysisReportFactory()
        {
            _baseFactory = new BaseFactory();
            _noIncludeOnSaleDataFactory = new NoIncludeOnSaleDataFactory();
            _orderPaymentMethodFactory = new OrderPaymentMethodFactory();
            _refundFactory = new RefundFactory();
        }

        public bool Insert(List<ItemizedSalesAnalysisReportModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_ItemizedSalesAnalysisReport.Where(ww => ww.StoreId == info.StoreId
                    && ww.CreatedDate == info.CreatedDate && ww.ItemId == info.ItemId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Itemized Sales data exist", lstInfo);
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_ItemizedSalesAnalysisReport> lstInsert = new List<R_ItemizedSalesAnalysisReport>();
                        R_ItemizedSalesAnalysisReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_ItemizedSalesAnalysisReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.Discount = item.Discount;
                            itemInsert.ServiceCharge = item.ServiceCharge;
                            itemInsert.ItemCode = item.ItemCode;
                            itemInsert.ItemId = item.ItemId;
                            itemInsert.ItemTypeId = item.ItemTypeId;
                            itemInsert.ItemName = item.ItemName;
                            itemInsert.Price = item.Price;
                            itemInsert.ExtraPrice = item.ExtraPrice;
                            itemInsert.TotalPrice = item.TotalPrice;
                            itemInsert.Quantity = item.Quantity;
                            itemInsert.Cost = item.Cost;
                            itemInsert.CategoryId = item.CategoryId;
                            itemInsert.CategoryName = item.CategoryName;
                            itemInsert.Tax = item.Tax;
                            itemInsert.Mode = item.Mode;
                            itemInsert.PromotionAmount = item.PromotionAmount;
                            itemInsert.GLAccountCode = item.GLAccountCode;
                            itemInsert.BusinessId = item.BusinessId;
                            itemInsert.IsIncludeSale = item.IsIncludeSale;
                            itemInsert.TotalAmount = item.TotalAmount;
                            itemInsert.TotalDiscount = item.TotalDiscount;
                            itemInsert.ExtraAmount = item.ExtraAmount;
                            itemInsert.ReceiptId = item.ReceiptId;
                            itemInsert.PoinsOrderId = item.PoinsOrderId;
                            itemInsert.GiftCardId = item.GiftCardId;
                            itemInsert.TaxType = item.TaxType;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_ItemizedSalesAnalysisReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Itemized Sales data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert Itemized Sales data fail", lstInfo);
                        NSLog.Logger.Error("Insert Itemized Sales data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_ItemizedSalesAnalysisReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        //public List<ItemizedSalesAnalysisReportModels> GetListCategory(BaseReportModel model)
        //{
        //    using (var cxt = new NuWebContext())
        //    {
        //        var lstData = (from tb in cxt.R_DetailItemizedSalesAnalysisReportHeader
        //                       where model.ListStores.Contains(tb.StoreId) && tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate
        //                       group tb by new { CategoryId = tb.CategoryId, CategoryName = tb.CategoryName } into g
        //                       select new ItemizedSalesAnalysisReportModels
        //                       {
        //                           CategoryId = g.Key.CategoryId,
        //                           CategoryName = g.Key.CategoryName
        //                       }).ToList();
        //        return lstData;
        //    }
        //}

        public List<ItemizedSalesAnalysisReportModels> GetData(DateTime dFrom, DateTime dTo
            , List<string> lstStoreId, List<StoreModels> lstStores, List<string> lstStoreIdCate, List<string> lstStoreIdSet, List<string> lstCateIds = null, List<string> lstCateSetIds = null, int mode = 1)
        {
            using (var cxt = new NuWebContext())
            {
                List<ItemizedSalesAnalysisReportModels> lstData = new List<ItemizedSalesAnalysisReportModels>();
                List<ItemizedSalesAnalysisReportModels> lstDish = new List<ItemizedSalesAnalysisReportModels>();
                List<ItemizedSalesAnalysisReportModels> lstSet = new List<ItemizedSalesAnalysisReportModels>();
                List<ItemizedSalesAnalysisReportModels> lstGC = new List<ItemizedSalesAnalysisReportModels>();

                List<ItemizedSalesAnalysisReportModels> lstReturn = new List<ItemizedSalesAnalysisReportModels>();
                #region Old
                var query = (from tb in cxt.R_ItemizedSalesAnalysisReport
                             where lstStoreId.Contains(tb.StoreId)
                                   && tb.CreatedDate >= dFrom && tb.CreatedDate <= dTo
                                   && tb.ItemTypeId != (int)Commons.EProductType.Misc
                                   && tb.Mode == mode
                             select tb);
                if (query.Any())
                {

                    ItemizedSalesAnalysisReportModels obj = null;
                    foreach (var item in query)
                    {
                        obj = new ItemizedSalesAnalysisReportModels();
                        obj.CreatedDate = item.CreatedDate;
                        obj.StoreId = item.StoreId;
                        obj.StoreName = lstStores.Where(ww => ww.Id == item.StoreId).Select(ss => ss.Name).FirstOrDefault();
                        obj.CategoryId = item.CategoryId;
                        obj.CategoryName = item.CategoryName;
                        obj.ItemTypeId = item.ItemTypeId;
                        obj.ItemId = item.ItemId;
                        obj.ItemName = item.ItemName;
                        obj.Quantity = item.Quantity;
                        obj.Price = item.Price;
                        obj.Discount = item.Discount;
                        obj.Tax = item.Tax;
                        obj.ServiceCharge = item.ServiceCharge;
                        obj.Cost = item.Cost;
                        obj.ItemTotal = item.TotalPrice + item.ExtraPrice;
                        obj.Percent = 0;
                        obj.TotalCost = item.Cost * (double)item.Quantity;
                        obj.ItemCode = item.ItemCode;
                        obj.PromotionAmount = item.PromotionAmount;

                        // Updated 9202017
                        obj.IsIncludeSale = item.IsIncludeSale;
                        obj.ExtraAmount = item.ExtraAmount;
                        obj.TotalAmount = item.TotalAmount;
                        obj.TotalDiscount = item.TotalDiscount;
                        obj.GiftCardId = item.GiftCardId;
                        obj.PoinsOrderId = item.PoinsOrderId;
                        obj.ReceiptId = item.ReceiptId;
                        obj.TaxType = item.TaxType;

                        lstData.Add(obj);
                    }
                }
                #endregion End Old



                if (lstData.Count > 0)
                {
                    if (lstStoreIdCate != null && lstStoreIdCate.Any() && lstCateIds != null && lstCateIds.Any())
                    {
                        lstDish = lstData.Where(ww => lstCateIds.Contains(ww.CategoryId) && lstStoreIdCate.Contains(ww.StoreId)).ToList();
                    }
                    if (lstStoreIdSet != null && lstStoreIdSet.Any() && lstCateSetIds != null && lstCateSetIds.Count > 0)
                        lstSet = lstData.Where(ww => lstCateSetIds.Contains(ww.ItemId) && lstStoreIdSet.Contains(ww.StoreId)).ToList();
                }
                if (lstDish != null && lstDish.Any())
                {
                    lstDish = lstDish.OrderBy(oo => oo.CategoryName).ThenBy(ss => ss.ItemName).ToList();
                }
                lstReturn.AddRange(lstDish);
                if (lstSet != null && lstSet.Any())
                {
                    lstSet = lstSet.OrderBy(oo => oo.CategoryName).ThenBy(ss => ss.ItemName).ToList();
                }
                lstReturn.AddRange(lstSet);

                //GC-22-09-2017
                lstGC = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId) && !string.IsNullOrEmpty(ww.PoinsOrderId)).ToList();
                //Credit Note
                var lstCreditNote = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                && string.IsNullOrEmpty(ww.PoinsOrderId) && string.IsNullOrEmpty(ww.CategoryId) && string.IsNullOrEmpty(ww.ItemId)).ToList();

                lstReturn.AddRange(lstGC);
                lstReturn.AddRange(lstCreditNote);

                return lstReturn;
            }
        }

        public List<ItemizedSalesAnalysisReportDataModels> GetData_New(DateTime dFrom, DateTime dTo
           , List<string> lstStoreId, List<StoreModels> lstStores, List<string> lstStoreIdCate, List<string> lstStoreIdSet, List<string> lstCateIds = null, List<string> lstCateSetIds = null, int mode = 1)
        {
            using (var cxt = new NuWebContext())
            {
                List<ItemizedSalesAnalysisReportDataModels> lstData = new List<ItemizedSalesAnalysisReportDataModels>();
                List<ItemizedSalesAnalysisReportDataModels> lstDish = new List<ItemizedSalesAnalysisReportDataModels>();
                List<ItemizedSalesAnalysisReportDataModels> lstSet = new List<ItemizedSalesAnalysisReportDataModels>();
                List<ItemizedSalesAnalysisReportDataModels> lstGC = new List<ItemizedSalesAnalysisReportDataModels>();
                List<ItemizedSalesAnalysisReportDataModels> lstReturn = new List<ItemizedSalesAnalysisReportDataModels>();

                var lstStoreInfo = lstStores.Select(ss => new Data.Models.StoreInfo() { Id = ss.Id, Name = ss.Name }).ToList();
                var request = new BaseReportDataModel()
                {
                    ListStores = lstStoreId,
                    FromDate = dFrom,
                    ToDate = dTo,
                    Mode = mode,
                    ListStoreInfos = lstStoreInfo
                };

                lstData = cxt.GetDataForItemSaleReport(request);
                if (lstData == null)
                    lstData = new List<ItemizedSalesAnalysisReportDataModels>();
                //if (lstData.Count > 0)
                //{
                if (lstStoreIdCate != null && lstStoreIdCate.Any() && lstCateIds != null && lstCateIds.Any())
                {
                    lstDish = lstData.Where(ww => lstCateIds.Contains(ww.CategoryId) && lstStoreIdCate.Contains(ww.StoreId)).ToList();
                }
                if (lstStoreIdSet != null && lstStoreIdSet.Any() && lstCateSetIds != null && lstCateSetIds.Count > 0)
                    lstSet = lstData.Where(ww => lstCateSetIds.Contains(ww.ItemId) && lstStoreIdSet.Contains(ww.StoreId)).ToList();
                //}
                if (lstDish != null && lstDish.Any())
                {
                    lstDish = lstDish.OrderBy(oo => oo.CategoryName).ThenBy(ss => ss.ItemName).ToList();
                }
                lstReturn.AddRange(lstDish);
                if (lstSet != null && lstSet.Any())
                {
                    lstSet = lstSet.OrderBy(oo => oo.CategoryName).ThenBy(ss => ss.ItemName).ToList();
                }
                lstReturn.AddRange(lstSet);

                //GC-22-09-2017
                lstGC = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId) && !string.IsNullOrEmpty(ww.PoinsOrderId)).ToList();
                //Credit Note
                var lstCreditNote = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                && string.IsNullOrEmpty(ww.PoinsOrderId) && string.IsNullOrEmpty(ww.CategoryId) && string.IsNullOrEmpty(ww.ItemId)).ToList();

                lstReturn.AddRange(lstGC);
                lstReturn.AddRange(lstCreditNote);

                return lstReturn;
            }
        }


        public List<DiscountAndMiscReportModels> GetReceiptDiscountAndMisc(DateTime fromDate, DateTime toDate, List<string> StoreIds)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = new List<DiscountAndMiscReportModels>();
                var query = (from tb in cxt.R_ItemizedSalesAnalysisReport
                             where StoreIds.Contains(tb.StoreId)
                                   && tb.CreatedDate >= fromDate && tb.CreatedDate <= toDate
                                   && tb.Discount > 0
                             group tb by new
                             {
                                 Hour = (int?)SqlFunctions.DatePart("HH", tb.CreatedDate),
                                 StoreId = tb.StoreId
                             } into g
                             select g).ToList();
                if (query != null && query.Any())
                {

                    lstData = query.Select(g => new DiscountAndMiscReportModels
                    {
                        StoreId = g.Key.StoreId,
                        Hour = g.Key.Hour.Value,
                        TimeSpanHour = new TimeSpan(g.Key.Hour.Value, 0, 0),
                        DiscountValue = g.Sum(ss => ss.Discount),
                        MiscValue = 0,
                    }).ToList();

                }

                var queryMisc = (from tb in cxt.R_ItemizedSalesAnalysisReport
                                 where StoreIds.Contains(tb.StoreId)
                                       && tb.CreatedDate >= fromDate && tb.CreatedDate <= toDate
                                       && tb.ItemTypeId == (int)Commons.EProductType.Misc
                                 group tb by new
                                 {
                                     Hour = (int?)SqlFunctions.DatePart("HH", tb.CreatedDate),
                                     StoreId = tb.StoreId
                                 } into g
                                 select g).ToList();

                var lstDataMisc = queryMisc.Select(g => new DiscountAndMiscReportModels
                {
                    StoreId = g.Key.StoreId,
                    Hour = g.Key.Hour.Value,
                    TimeSpanHour = new TimeSpan(g.Key.Hour.Value, 0, 0),
                    DiscountValue = 0,
                    MiscValue = g.Sum(ss => ss.Price),
                }).ToList();
                if (lstDataMisc != null && lstDataMisc.Count > 0)
                {
                    if (lstData.Count == 0)
                        lstData.AddRange(lstDataMisc);
                    else
                    {
                        foreach (var item in lstData)
                        {
                            var exist = lstDataMisc.Where(ww => ww.Hour == item.Hour && ww.StoreId == item.StoreId).FirstOrDefault();
                            if (exist != null)
                            {
                                item.MiscValue = exist.MiscValue;
                            }
                            else
                            {
                                lstData.Add(exist);
                            }
                        }
                    }
                }

                return lstData;
            }
        }

        private int GetTaxType(string storeId)
        {
            TaxFactory factory = new TaxFactory();
            var taxes = factory.GetDetailTaxForStore(storeId);
            return taxes;
        }

        public XLWorkbook ExportExcel(ItemizedSalesAnalysisModels model, BaseReportModel viewmodel, List<MISCBillDiscountPeriodModels> listMiscDisPeriod, List<string> listStoreIndexes)
        {
            //model.ItemizedSalesAnalysisTotal = new ItemizedSalesAnalysisTotal();
            string sheetName = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized_Sales_Analysis_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 11, viewmodel.FromDate, viewmodel.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized Sales Analysis Report"));

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
            ws.Cell("H5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
            ws.Cell("I5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
            ws.Cell("J5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
            ws.Cell("K5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

            //set width column
            ws.Column("A").Width = 20;
            ws.Column("B").Width = 30;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 10;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 15;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 15;

            int index = 7;
            //string nullValue = "<No Payment!>";
            var total = model.ItemizedSalesAnalysisTotal;
            total.UnitCost = 0;
            total.TotalCost = 0;
            double subDisPeriod = 0, subDisPeriodByStore = 0;
            if (model.ListItemizedSalesAnalysisOuletTotal != null)
            {
                if (model.ListItemizedSalesAnalysisOuletTotal.Count > 0)
                {
                    for (int i = 0; i < model.ListItemizedSalesAnalysisOuletTotal.Count; i++)
                    {
                        var subPeriod = model.ListItemizedSalesAnalysisOuletTotal[i];
                        subPeriod.UnitCost = 0;
                        subPeriod.TotalCost = 0;
                        subDisPeriodByStore = 0;

                        ws.Range("A" + (index - 1) + ":K" + (index - 1)).Merge().SetValue(String.Format(model.ListItemizedSalesAnalysisOuletTotal[i].StoreName));
                        ws.Range("A" + (index - 1) + ":K" + (index - 1)).Style.Font.SetBold(true);
                        ws.Range("A" + (index - 1) + ":K" + (index - 1)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                        ws.Range("A" + (index - 1) + ":K" + (index - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        #region Category of Dish
                        if (model.ListItemizedSalesAnalysisOuletTotal[i].ListItemizedSalesAnalysisSubTotal != null)
                        {
                            for (int j = 0; j < model.ListItemizedSalesAnalysisOuletTotal[i].ListItemizedSalesAnalysisSubTotal.Count(); j++)
                            {
                                var ItemizeSubTotal = model.ListItemizedSalesAnalysisOuletTotal[i].ListItemizedSalesAnalysisSubTotal[j];
                                ItemizeSubTotal.UnitCost = 0;
                                ItemizeSubTotal.TotalCost = 0;

                                ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + ItemizeSubTotal.CategoryName));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                index++;
                                if (ItemizeSubTotal.ListItemizedSalesAnalysisItems != null)
                                {
                                    if (ItemizeSubTotal.ListItemizedSalesAnalysisItems.Count != 0)
                                    {
                                        while (ItemizeSubTotal.ListItemizedSalesAnalysisItems.Count > 0)
                                        {
                                            var itemize = ItemizeSubTotal.ListItemizedSalesAnalysisItems[0];
                                            List<ItemizedSalesAnalysisReportModels> dishItems = ItemizeSubTotal.ListItemizedSalesAnalysisItems.Where(d => d.ItemId == itemize.ItemId
                                            && d.Price == itemize.Price).ToList();
                                            ws.Cell("A" + index).Value = itemize.ItemCode;
                                            ws.Cell("B" + index).Value = itemize.ItemName;
                                            ws.Cell("C" + index).Value = itemize.Price.ToString("F");
                                            ws.Cell("C" + index).Style.NumberFormat.Format = "#,##0.00";
                                            ws.Cell("D" + index).Value = dishItems.Sum(d => d.Quantity);
                                            ws.Cell("E" + index).Value = dishItems.Sum(d => d.ItemTotal).ToString("F");
                                            ws.Cell("F" + index).Value = dishItems.Sum(d => d.Percent).ToString("F") + " %";
                                            ws.Cell("G" + index).Value = (-dishItems.Sum(d => d.Discount)).ToString("F");

                                            ws.Cell("H" + index).Value = (-dishItems.Sum(d => d.PromotionAmount)).ToString("F");

                                            ws.Cell("I" + index).Value = itemize.Cost.ToString("F");
                                            ws.Cell("J" + index).Value = dishItems.Sum(d => d.TotalCost).ToString("F");

                                            if (dishItems.Sum(d => d.TotalCost) == 0 || dishItems.Sum(d => d.ItemTotal) == 0)
                                                itemize.CP = 0;
                                            else
                                                itemize.CP = ((dishItems.Sum(d => d.TotalCost) / dishItems.Sum(d => d.ItemTotal)) * 100);

                                            ws.Cell("K" + index).Value = itemize.CP.ToString("F") + " %";
                                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                            index++;
                                            ItemizeSubTotal.ListItemizedSalesAnalysisItems.RemoveAll(d => d.ItemId == itemize.ItemId && d.Price == itemize.Price);
                                            ItemizeSubTotal.UnitCost += itemize.Cost;
                                            ItemizeSubTotal.TotalCost += dishItems.Sum(d => d.TotalCost);
                                        }
                                    }
                                }
                                subPeriod.UnitCost += ItemizeSubTotal.UnitCost;
                                subPeriod.TotalCost += ItemizeSubTotal.TotalCost;

                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total"));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Cell("D" + index).Value = ItemizeSubTotal.Qty;
                                ws.Cell("E" + index).Value = ItemizeSubTotal.ItemTotal.ToString("F");
                                ws.Cell("F" + index).Value = ItemizeSubTotal.Percent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-ItemizeSubTotal.Discount).ToString("F");
                                ws.Cell("H" + index).Value = (-ItemizeSubTotal.Promotion).ToString("F");
                                ws.Cell("I" + index).Value = ItemizeSubTotal.UnitCost.ToString("F");
                                ws.Cell("J" + index).Value = ItemizeSubTotal.TotalCost.ToString("F");

                                if (ItemizeSubTotal.TotalCost == 0 || ItemizeSubTotal.ItemTotal == 0)
                                    ItemizeSubTotal.CP = 0;
                                else
                                    ItemizeSubTotal.CP = ((ItemizeSubTotal.TotalCost / ItemizeSubTotal.ItemTotal) * 100);

                                ws.Cell("K" + index).Value = ItemizeSubTotal.CP.ToString("F") + " %";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;
                                if (ItemizeSubTotal.ListPeriod != null)
                                {
                                    for (int s = 0; s < ItemizeSubTotal.ListPeriod.Count; s++)
                                    {
                                        var subDishPeriod = ItemizeSubTotal.ListPeriod[s];
                                        ws.Range("A" + index + ":C" + index).Merge().SetValue(subDishPeriod.Name);
                                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                        ws.Cell("D" + index).Value = subDishPeriod.Qty;
                                        ws.Cell("E" + index).Value = subDishPeriod.ItemTotal.ToString("F");
                                        ws.Cell("F" + index).Value = subDishPeriod.Percent.ToString("F") + " %";
                                        ws.Cell("G" + index).Value = (-subDishPeriod.Discount).ToString("F");
                                        ws.Cell("H" + index).Value = (-subDishPeriod.Promotion).ToString("F");
                                        ws.Cell("I" + index).Value = subDishPeriod.UnitCost.ToString("F");
                                        ws.Cell("J" + index).Value = subDishPeriod.TotalCost.ToString("F");
                                        if (subDishPeriod.TotalCost == 0 || subDishPeriod.ItemTotal == 0)
                                            subDishPeriod.CP = 0;
                                        else
                                            subDishPeriod.CP = ((subDishPeriod.TotalCost / subDishPeriod.ItemTotal) * 100);
                                        ws.Cell("K" + index).Value = subDishPeriod.CP.ToString("F") + " %";
                                        ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        index++;
                                    }
                                }
                            }
                        }
                        #endregion
                        #region SetMenu
                        if (model.ListItemizedSalesAnalysisOuletTotal[i].ListItemizedSalesAnalysisSubTotalSetMenu != null)
                        {
                            if (model.ListItemizedSalesAnalysisOuletTotal[i].ListItemizedSalesAnalysisSubTotalSetMenu.Count() != 0)
                            {
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu").ToUpper());
                                ws.Range("A" + index + ":K" + index).Style.Font.SetBold(true);
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                                ws.Range("A" + index + ":K" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                index++;
                                for (int j = 0; j < model.ListItemizedSalesAnalysisOuletTotal[i].ListItemizedSalesAnalysisSubTotalSetMenu.Count(); j++)
                                {
                                    var ItemizeSubTotalSetMenu = model.ListItemizedSalesAnalysisOuletTotal[i].ListItemizedSalesAnalysisSubTotalSetMenu;
                                    if (ItemizeSubTotalSetMenu[j].ListItemizedSalesAnalysisItemsSetMenu != null)
                                    {
                                        if (ItemizeSubTotalSetMenu[j].ListItemizedSalesAnalysisItemsSetMenu.Count != 0)
                                        {
                                            while (ItemizeSubTotalSetMenu[j].ListItemizedSalesAnalysisItemsSetMenu.Count > 0)
                                            {
                                                var itemizeSetMenu = ItemizeSubTotalSetMenu[j].ListItemizedSalesAnalysisItemsSetMenu[0];
                                                List<ItemizedSalesAnalysisReportModels> setmenuItems = ItemizeSubTotalSetMenu[j].ListItemizedSalesAnalysisItemsSetMenu.Where(d => d.ItemId == itemizeSetMenu.ItemId && d.Price == itemizeSetMenu.Price).ToList();
                                                ws.Cell("A" + index).Value = itemizeSetMenu.ItemCode;
                                                ws.Cell("B" + index).Value = itemizeSetMenu.ItemName;
                                                ws.Cell("C" + index).Value = itemizeSetMenu.Price.ToString("F");
                                                ws.Cell("C" + index).Style.NumberFormat.Format = "#,##0.00";
                                                ws.Cell("D" + index).Value = setmenuItems.Sum(s => s.Quantity);
                                                ws.Cell("E" + index).Value = setmenuItems.Sum(s => s.ItemTotal).ToString("F");
                                                ws.Cell("F" + index).Value = setmenuItems.Sum(s => s.Percent).ToString("F") + " %";
                                                ws.Cell("G" + index).Value = (-setmenuItems.Sum(s => s.Discount)).ToString("F");
                                                ws.Cell("H" + index).Value = (-setmenuItems.Sum(s => s.PromotionAmount)).ToString("F");
                                                ws.Cell("I" + index).Value = itemizeSetMenu.Cost.ToString("F");
                                                ws.Cell("J" + index).Value = setmenuItems.Sum(s => s.TotalCost).ToString("F");

                                                if (setmenuItems.Sum(d => d.TotalCost) == 0 || setmenuItems.Sum(d => d.ItemTotal) == 0)
                                                    itemizeSetMenu.CP = 0;
                                                else itemizeSetMenu.CP = ((setmenuItems.Sum(d => d.TotalCost) / setmenuItems.Sum(d => d.ItemTotal)) * 100);

                                                ws.Cell("K" + index).Value = itemizeSetMenu.CP.ToString("F") + " %";
                                                ws.Range("A" + index + ":K" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                                index++;
                                                ItemizeSubTotalSetMenu[j].ListItemizedSalesAnalysisItemsSetMenu.RemoveAll(d => d.ItemId == itemizeSetMenu.ItemId && d.Price == itemizeSetMenu.Price);
                                            }
                                        }
                                    }

                                    var subSet = ItemizeSubTotalSetMenu[j];
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total"));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = subSet.Qty;
                                    ws.Cell("E" + index).Value = subSet.ItemTotal.ToString("F");
                                    ws.Cell("F" + index).Value = subSet.Percent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-subSet.Discount).ToString("F");
                                    ws.Cell("H" + index).Value = (-subSet.Promotion).ToString("F");
                                    ws.Cell("I" + index).Value = subSet.UnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = subSet.TotalCost.ToString("F");

                                    if (subSet.TotalCost == 0 || subSet.ItemTotal == 0)
                                        subSet.CP = 0;
                                    else subSet.CP = ((subSet.TotalCost / subSet.ItemTotal) * 100);

                                    ws.Cell("K" + index).Value = subSet.CP.ToString("F") + " %";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;
                                    if (ItemizeSubTotalSetMenu[j].ListPeriodSetMenu != null)
                                    {
                                        for (int s = 0; s < ItemizeSubTotalSetMenu[j].ListPeriodSetMenu.Count; s++)
                                        {
                                            var smPeriod = ItemizeSubTotalSetMenu[j].ListPeriodSetMenu[s];
                                            ws.Range("A" + index + ":C" + index).Merge().SetValue(smPeriod.Name);
                                            ws.Range("A" + (index) + ":J" + (index)).Style.Font.SetBold(true);
                                            ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                            ws.Cell("D" + index).Value = smPeriod.Qty;
                                            ws.Cell("E" + index).Value = smPeriod.ItemTotal.ToString("F");
                                            ws.Cell("F" + index).Value = smPeriod.Percent.ToString("F") + " %";
                                            ws.Cell("G" + index).Value = (-smPeriod.Discount).ToString("F");
                                            ws.Cell("H" + index).Value = (-smPeriod.Promotion).ToString("F");
                                            ws.Cell("I" + index).Value = smPeriod.UnitCost.ToString("F");
                                            ws.Cell("J" + index).Value = smPeriod.TotalCost.ToString("F");

                                            if (smPeriod.TotalCost == 0 || smPeriod.ItemTotal == 0)
                                                smPeriod.CP = 0;
                                            else smPeriod.CP = ((smPeriod.TotalCost / smPeriod.ItemTotal) * 100);

                                            ws.Cell("K" + index).Value = smPeriod.CP.ToString("F") + " %";
                                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region MISC
                        int startM = index;
                        if (listMiscDisPeriod != null)
                        {
                            var listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreName == model.ListItemizedSalesAnalysisOuletTotal[i].StoreName).ToList();
                            if (listMiscDisPeriodByStore != null)
                            {
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                                index++;
                                for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(listMiscDisPeriodByStore[m].Period);
                                    ws.Cell("E" + index).SetValue(listMiscDisPeriodByStore[m].MiscTotal.ToString("F"));
                                    ws.Cell("F" + index).SetValue(listMiscDisPeriodByStore[m].Percent.ToString("F") + " %");
                                    index++;
                                }
                            }
                        }
                        #endregion
                        #region Discount Total Bill
                        if (listMiscDisPeriod != null)
                        {
                            var listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreName == model.ListItemizedSalesAnalysisOuletTotal[i].StoreName).ToList();
                            if (listMiscDisPeriodByStore != null)
                            {
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                                index++;
                                for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(listMiscDisPeriodByStore[m].Period);
                                    ws.Cell("G" + index).SetValue((-listMiscDisPeriodByStore[m].SubDiscount).ToString("F"));
                                    subDisPeriod += listMiscDisPeriodByStore[m].SubDiscount;
                                    subDisPeriodByStore += listMiscDisPeriodByStore[m].SubDiscount;
                                    index++;

                                }
                            }
                        }

                        ws.Range("A" + startM + ":K" + index).Style.Font.SetBold(true);
                        ws.Range("A" + startM + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + startM + ":D" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Range("E" + startM + ":G" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        #endregion
                        #region Outlet Sub-total
                        var miscPercent = listMiscDisPeriod.Where(m => m.StoreName == model.ListItemizedSalesAnalysisOuletTotal[i].StoreName).ToList();
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub-total"));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = subPeriod.Qty;
                        ws.Cell("E" + index).Value = (subPeriod.ItemTotal + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                        ws.Cell("F" + index).Value = (subPeriod.Percent + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-subPeriod.Discount - subDisPeriodByStore).ToString("F");
                        ws.Cell("H" + index).Value = (-subPeriod.Promotion).ToString("F");
                        ws.Cell("I" + index).Value = subPeriod.UnitCost.ToString("F");
                        ws.Cell("J" + index).Value = subPeriod.TotalCost.ToString("F");

                        if (subPeriod.TotalCost == 0 || subPeriod.ItemTotal == 0)
                            subPeriod.CP = 0;
                        else subPeriod.CP = ((subPeriod.TotalCost / subPeriod.ItemTotal) * 100);

                        ws.Cell("K" + index).Value = subPeriod.CP.ToString("F") + " %";
                        ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                        ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;
                        index++;
                        #endregion

                        total.UnitCost += subPeriod.UnitCost;
                        total.TotalCost = subPeriod.TotalCost;
                    }
                }
            }
            #region TOTAL
            index = index - 1;
            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
            ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell("D" + index).Value = total.Qty;
            ws.Cell("E" + index).Value = total.ItemTotal.ToString("F");
            ws.Cell("F" + index).Value = total.Percent.ToString("F") + " %";
            ws.Cell("G" + index).Value = (-total.Discount).ToString("F");
            ws.Cell("H" + index).Value = (-total.Promotion).ToString("F");
            ws.Cell("I" + index).Value = total.UnitCost.ToString("F");
            ws.Cell("J" + index).Value = total.TotalCost.ToString("F");

            if (total.TotalCost == 0 || total.ItemTotal == 0)
                total.CP = 0;
            else
                total.CP = ((total.TotalCost / total.ItemTotal) * 100);

            ws.Cell("K" + index).Value = total.CP.ToString("F") + " %";
            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            index++;
            if (total.ListPeriodTotal != null)
            {
                double miscTotal = 0, percent = 0, subDiscount = 0;
                for (int i = 0; i < total.ListPeriodTotal.Count; i++)
                {
                    miscTotal = 0; percent = 0; subDiscount = 0;

                    ws.Range("A" + index + ":C" + index).Merge().SetValue(total.ListPeriodTotal[i].Name);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    var totalPeriod = total.ListPeriodTotal[i];
                    ws.Cell("D" + index).Value = totalPeriod.Qty;
                    if (listMiscDisPeriod != null && listMiscDisPeriod.Count > 0)
                    {
                        miscTotal = listMiscDisPeriod.Where(ww => ww.Period == totalPeriod.Name).Sum(ss => ss.MiscTotal);
                        percent = listMiscDisPeriod.Where(ww => ww.Period == totalPeriod.Name).Sum(ss => ss.Percent);
                        subDiscount = listMiscDisPeriod.Where(ww => ww.Period == totalPeriod.Name).Sum(ss => ss.SubDiscount);
                    }
                    ws.Cell("E" + index).Value = (totalPeriod.ItemTotal + miscTotal).ToString("F");
                    ws.Cell("F" + index).Value = (totalPeriod.Percent + percent).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-totalPeriod.Discount - subDiscount).ToString("F");

                    ws.Cell("H" + index).Value = (-totalPeriod.Promotion).ToString("F");
                    ws.Cell("I" + index).Value = totalPeriod.UnitCost.ToString("F");
                    ws.Cell("J" + index).Value = totalPeriod.TotalCost.ToString("F");

                    if (totalPeriod.TotalCost == 0 || totalPeriod.ItemTotal == 0)
                        totalPeriod.CP = 0;
                    else totalPeriod.CP = ((totalPeriod.TotalCost / totalPeriod.ItemTotal) * 100);

                    ws.Cell("K" + index).Value = totalPeriod.CP.ToString("F") + " %";
                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;
                }
            }
            #endregion

            ws.Range("C" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //format file
            //header
            ws.Range("A4:K6").Style.Font.SetBold(true);
            //Set color
            //ws.Range("A1:J2").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
            ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
            ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            //set Border        
            ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A1:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A1:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            index++;
            //
            #region GRAND TOTAL
            bool isTaxInclude = false;
            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
            if (GetTaxType(listStoreIndexes.FirstOrDefault()) == (int)Commons.ETax.AddOn)
            {
                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
            }
            else
            {
                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                isTaxInclude = true;
            }
            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
            ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            index++;
            //ws.Cell("A" + index).Value = (total.ItemTotal - subDisPeriod).ToString("F");
            ws.Cell("A" + index).Value = (total.ItemTotal - subDisPeriod - (isTaxInclude ? total.TaxTotal : 0)).ToString("F");
            ws.Cell("B" + index).Value = total.SCTotal.ToString("F");
            ws.Cell("C" + index).Value = total.TaxTotal.ToString("F");
            ws.Cell("D" + index).Value = (-total.Discount).ToString("F");
            ws.Cell("E" + index).Value = (-total.Promotion).ToString("F");

            ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            #endregion
            ws.Columns().AdjustToContents();
            return wb;
        }

        public XLWorkbook ExportExcel_New(List<ItemizedSalesAnalysisReportModels> lstData, ItemizedSalesAnalysisModels model
            , BaseReportModel viewmodel, List<MISCBillDiscountPeriodModels> listMiscDisPeriod
            , List<StoreModels> lstStores, DateTime dToFilter, DateTime dFromFilter, List<string> lstGCId)
        {
            List<PaymentModels> lstPayments = _orderPaymentMethodFactory.GetDataPaymentItemsByGC(viewmodel, lstGCId);
            var _lstNoIncludeOnSale = _noIncludeOnSaleDataFactory.GetListCateNoIncludeSaleForItemSale(viewmodel.ListStores, viewmodel.FromDate, viewmodel.ToDate
                , viewmodel.Mode);
            string sheetName = "Itemized_Sales_Analysis_Report";// _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized_Sales_Analysis_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 11, dFromFilter, dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized Sales Analysis Report"));

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

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
            ws.Cell("H5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
            ws.Cell("I5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
            ws.Cell("J5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
            ws.Cell("K5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

            //set width column
            ws.Column("A").Width = 20;
            ws.Column("B").Width = 30;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 10;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 15;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 15;

            int index = 7;
            int indexNextStore = 0;
            //Total
            ItemizedSalesNewTotal _itemTotal = new ItemizedSalesNewTotal();
            var _breakfastTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerTotal = new ItemizedSalesPeriodValueTotal();

            var _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
            var lstBusinessId = new List<string>();
            bool _firstStore = true;
            if (lstData != null && lstData.Any())
            {
                //lstBusinessId = lstData.Select(ss => ss.BusinessId).Distinct().ToList();
                //Group storeId
                TimeSpan timeDish = new TimeSpan();

                lstData.ForEach(x =>
                {
                    var store = lstStores.Where(z => z.Id.Equals(x.StoreId)).FirstOrDefault();
                    x.StoreName = store == null ? "" : store.Name;
                });

                var lstItemGroupByStore = lstData.Where(ww => string.IsNullOrEmpty(ww.GiftCardId))
                    .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                foreach (var itemGroupStore in lstItemGroupByStore)
                {
                    var miscInstore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.MiscTotal);

                    double subDisPeriodByStore = 0;
                    double subDisPeriod = 0;
                    _itemTotal = new ItemizedSalesNewTotal();
                    _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
                    _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
                    _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
                    //Store name
                    var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                    ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    index++;
                    var lstItems = itemGroupStore.ToList();
                    //Group item type
                    var lstGroupItemType = lstItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                   || ww.ItemTypeId == (int)Commons.EProductType.SetMenu))
                       .OrderBy(x => x.ItemTypeId).GroupBy(gg => gg.ItemTypeId).ToList();

                    var itemAmountTotal = lstItems.Sum(ss => ss.ItemTotal);
                    foreach (var itemTypeId in lstGroupItemType)
                    {
                        var lstItemGroupCates = lstItems.Where(x => x.ItemTypeId == itemTypeId.Key)
                          .GroupBy(gg => gg.CategoryName).ToList();

                        //Group by category
                        foreach (var itemCate in lstItemGroupCates)
                        {
                            ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Select(ss => ss.CategoryName).FirstOrDefault()));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            var lstCateItems = itemCate.ToList();
                            var lstItemTypes = lstCateItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                                                    || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).ToList();

                            var breakfast = new ItemizedSalesPeriodValueTotal();
                            var lunch = new ItemizedSalesPeriodValueTotal();
                            var dinner = new ItemizedSalesPeriodValueTotal();
                            foreach (var item in lstItemTypes)
                            {
                                timeDish = item.CreatedDate.TimeOfDay;
                                if ((itemAmountTotal + miscInstore) != 0)
                                {
                                    item.Percent = (item.ItemTotal / (itemAmountTotal + miscInstore)) * 100;
                                }
                                //check percent data
                                if (timeDish >= brearkStart && timeDish < brearkEnd)
                                {
                                    breakfast.Qty += item.Quantity;
                                    breakfast.ItemTotal += item.ItemTotal;
                                    breakfast.Percent += item.Percent;
                                    breakfast.Discount += item.Discount;
                                    breakfast.Promotion += item.PromotionAmount;
                                    breakfast.UnitCost += item.Cost;
                                    breakfast.TotalCost += item.TotalCost;

                                    _breakfastOutletTotal.Qty += item.Quantity;
                                    _breakfastOutletTotal.ItemTotal += item.ItemTotal;
                                    _breakfastOutletTotal.Percent += item.Percent;
                                    _breakfastOutletTotal.Discount += item.Discount;
                                    _breakfastOutletTotal.Promotion += item.PromotionAmount;
                                    _breakfastOutletTotal.UnitCost += item.Cost;
                                    _breakfastOutletTotal.TotalCost += item.TotalCost;

                                    _breakfastTotal.Qty += item.Quantity;
                                    _breakfastTotal.ItemTotal += item.ItemTotal;
                                    _breakfastTotal.Percent += item.Percent;
                                    _breakfastTotal.Discount += item.Discount;
                                    _breakfastTotal.Promotion += item.PromotionAmount;
                                    _breakfastTotal.UnitCost += item.Cost;
                                    _breakfastTotal.TotalCost += item.TotalCost;

                                }
                                //lunch
                                if (timeDish >= lunchStart && timeDish < lunchEnd)
                                {
                                    lunch.Qty += item.Quantity;
                                    lunch.ItemTotal += item.ItemTotal;
                                    lunch.Percent += item.Percent;
                                    lunch.Discount += item.Discount;
                                    lunch.Promotion += item.PromotionAmount;
                                    lunch.UnitCost += item.Cost;
                                    lunch.TotalCost += item.TotalCost;

                                    _lunchOutletTotal.Qty += item.Quantity;
                                    _lunchOutletTotal.ItemTotal += item.ItemTotal;
                                    _lunchOutletTotal.Percent += item.Percent;
                                    _lunchOutletTotal.Discount += item.Discount;
                                    _lunchOutletTotal.Promotion += item.PromotionAmount;
                                    _lunchOutletTotal.UnitCost += item.Cost;
                                    _lunchOutletTotal.TotalCost += item.TotalCost;

                                    _lunchTotal.Qty += item.Quantity;
                                    _lunchTotal.ItemTotal += item.ItemTotal;
                                    _lunchTotal.Percent += item.Percent;
                                    _lunchTotal.Discount += item.Discount;
                                    _lunchTotal.Promotion += item.PromotionAmount;
                                    _lunchTotal.UnitCost += item.Cost;
                                    _lunchTotal.TotalCost += item.TotalCost;
                                }
                                if (dinnerStart > dinnerEnd)//pass day
                                {
                                    if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                    {
                                        dinner.Qty += item.Quantity;
                                        dinner.ItemTotal += item.ItemTotal;
                                        dinner.Percent += item.Percent;
                                        dinner.Discount += item.Discount;
                                        dinner.Promotion += item.PromotionAmount;
                                        dinner.UnitCost += item.Cost;
                                        dinner.TotalCost += item.TotalCost;

                                        _dinnerOutletTotal.Qty += item.Quantity;
                                        _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                        _dinnerOutletTotal.Percent += item.Percent;
                                        _dinnerOutletTotal.Discount += item.Discount;
                                        _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                        _dinnerOutletTotal.UnitCost += item.Cost;
                                        _dinnerOutletTotal.TotalCost += item.TotalCost;

                                        _dinnerTotal.Qty += item.Quantity;
                                        _dinnerTotal.ItemTotal += item.ItemTotal;
                                        _dinnerTotal.Percent += item.Percent;
                                        _dinnerTotal.Discount += item.Discount;
                                        _dinnerTotal.Promotion += item.PromotionAmount;
                                        _dinnerTotal.UnitCost += item.Cost;
                                        _dinnerTotal.TotalCost += item.TotalCost;
                                    }
                                }
                                else//in day
                                {
                                    if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                    {
                                        dinner.Qty += item.Quantity;
                                        dinner.ItemTotal += item.ItemTotal;
                                        dinner.Percent += item.Percent;
                                        dinner.Discount += item.Discount;
                                        dinner.Promotion += item.PromotionAmount;
                                        dinner.UnitCost += item.Cost;
                                        dinner.TotalCost += item.TotalCost;

                                        _dinnerOutletTotal.Qty += item.Quantity;
                                        _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                        _dinnerOutletTotal.Percent += item.Percent;
                                        _dinnerOutletTotal.Discount += item.Discount;
                                        _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                        _dinnerOutletTotal.UnitCost += item.Cost;
                                        _dinnerOutletTotal.TotalCost += item.TotalCost;

                                        _dinnerTotal.Qty += item.Quantity;
                                        _dinnerTotal.ItemTotal += item.ItemTotal;
                                        _dinnerTotal.Percent += item.Percent;
                                        _dinnerTotal.Discount += item.Discount;
                                        _dinnerTotal.Promotion += item.PromotionAmount;
                                        _dinnerTotal.UnitCost += item.Cost;
                                        _dinnerTotal.TotalCost += item.TotalCost;
                                    }
                                }
                                _itemTotal.SCTotal += item.ServiceCharge;
                                _itemTotal.ItemTotal += item.ItemTotal;
                                _itemTotal.TaxTotal += item.Tax;
                                _itemTotal.DiscountTotal += item.Discount;
                                _itemTotal.PromotionTotal += item.PromotionAmount;
                            }//End lstItemTypes
                             //Group Item
                            var lstItemTypeGroup = lstItemTypes.GroupBy(gg => new { gg.ItemTypeId, gg.ItemId, gg.Price }).ToList();
                            double cp = 0; double cost = 0;
                            foreach (var item in lstItemTypeGroup)
                            {
                                ws.Cell("A" + index).Value = item.Select(ss => ss.ItemCode).FirstOrDefault();
                                ws.Cell("B" + index).Value = item.Select(ss => ss.ItemName).FirstOrDefault();
                                ws.Cell("C" + index).Value = item.Select(ss => ss.Price).FirstOrDefault().ToString("F");
                                ws.Cell("C" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Cell("D" + index).Value = item.Sum(d => d.Quantity);
                                ws.Cell("E" + index).Value = item.Sum(d => d.ItemTotal).ToString("F");
                                ws.Cell("F" + index).Value = item.Sum(d => d.Percent).ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-item.Sum(d => d.Discount)).ToString("F");

                                ws.Cell("H" + index).Value = (-item.Sum(d => d.PromotionAmount)).ToString("F");
                                ws.Cell("I" + index).Value = item.Select(ss => ss.Cost).FirstOrDefault().ToString("F");
                                cost += item.Select(ss => ss.Cost).FirstOrDefault();
                                ws.Cell("J" + index).Value = item.Sum(d => d.TotalCost).ToString("F");

                                if (item.Sum(d => d.TotalCost) == 0 || item.Sum(d => d.ItemTotal) == 0)
                                    cp = 0;
                                else
                                    cp = ((item.Sum(d => d.TotalCost) / item.Sum(d => d.ItemTotal)) * 100);

                                ws.Cell("K" + index).Value = cp.ToString("F") + " %";
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;
                            }//end lstItemTypeGroup
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total"));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            ws.Cell("D" + index).Value = (breakfast.Qty + lunch.Qty + dinner.Qty);
                            ws.Cell("E" + index).Value = (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal).ToString("F");
                            ws.Cell("F" + index).Value = (breakfast.Percent + lunch.Percent + dinner.Percent).ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-(breakfast.Discount + lunch.Discount + dinner.Discount)).ToString("F");
                            ws.Cell("H" + index).Value = (-(breakfast.Promotion + lunch.Promotion + dinner.Promotion)).ToString("F");
                            ws.Cell("I" + index).Value = cost.ToString("F");
                            ws.Cell("J" + index).Value = (breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost).ToString("F");

                            if ((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) == 0 || (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal) == 0)
                                cp = 0;
                            else
                                cp = (((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) / (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal)) * 100);

                            ws.Cell("K" + index).Value = cp.ToString("F") + " %";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;
                            //Morning
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = breakfast.Qty;
                            ws.Cell("E" + index).Value = breakfast.ItemTotal.ToString("F");
                            ws.Cell("F" + index).Value = breakfast.Percent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-breakfast.Discount).ToString("F");
                            ws.Cell("H" + index).Value = (-breakfast.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = breakfast.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = breakfast.TotalCost.ToString("F");
                            if (breakfast.TotalCost == 0 || breakfast.ItemTotal == 0)
                                breakfast.CP = 0;
                            else
                                breakfast.CP = ((breakfast.TotalCost / breakfast.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = breakfast.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                            //Afternoon
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = lunch.Qty;
                            ws.Cell("E" + index).Value = lunch.ItemTotal.ToString("F");
                            ws.Cell("F" + index).Value = lunch.Percent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-lunch.Discount).ToString("F");
                            ws.Cell("H" + index).Value = (-lunch.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = lunch.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = lunch.TotalCost.ToString("F");
                            if (lunch.TotalCost == 0 || lunch.ItemTotal == 0)
                                lunch.CP = 0;
                            else
                                lunch.CP = ((lunch.TotalCost / lunch.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = lunch.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;

                            //dinner
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = dinner.Qty;
                            ws.Cell("E" + index).Value = dinner.ItemTotal.ToString("F");
                            ws.Cell("F" + index).Value = dinner.Percent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-dinner.Discount).ToString("F");
                            ws.Cell("H" + index).Value = (-dinner.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = dinner.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = dinner.TotalCost.ToString("F");
                            if (dinner.TotalCost == 0 || dinner.ItemTotal == 0)
                                dinner.CP = 0;
                            else
                                dinner.CP = ((dinner.TotalCost / dinner.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = dinner.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;

                        }//End Group by category
                    }//end lstGroupItemType
                    #region MISC
                    int startM = index;
                    if (listMiscDisPeriod != null)
                    {
                        var listMiscPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                        if (listMiscPeriodByStore != null)
                        {
                            ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                            index++;
                            //var miscTotal = listMiscPeriodByStore.Sum(ss => ss.MiscTotal);
                            for (int m = 0; m < listMiscPeriodByStore.Count(); m++)
                            {
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscPeriodByStore[m].Period));
                                ws.Cell("E" + index).SetValue(listMiscPeriodByStore[m].MiscTotal.ToString("F"));
                                listMiscPeriodByStore[m].Percent = ((listMiscPeriodByStore[m].MiscTotal / (itemAmountTotal + miscInstore)) * 100);
                                ws.Cell("F" + index).SetValue(listMiscPeriodByStore[m].Percent.ToString("F") + " %");
                                index++;

                                _itemTotal.ItemTotal += listMiscPeriodByStore[m].MiscTotal;
                            }

                        }
                    }
                    #endregion End Misc
                    List<MISCBillDiscountPeriodModels> listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();
                    #region Discount Total Bill
                    if (listMiscDisPeriod != null)
                    {
                        listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                        if (listMiscDisPeriodByStore != null)
                        {
                            ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            index++;
                            for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                            {
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                ws.Cell("G" + index).SetValue((-listMiscDisPeriodByStore[m].BillDiscountTotal).ToString("F"));
                                subDisPeriod += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                subDisPeriodByStore += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                _itemTotal.DiscountTotal += listMiscDisPeriodByStore[m].BillDiscountTotal;

                                index++;

                            }
                        }
                    }

                    ws.Range("A" + startM + ":K" + index).Style.Font.SetBold(true);
                    ws.Range("A" + startM + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + startM + ":D" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range("E" + startM + ":G" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    #endregion

                    #region Outlet Sub-total
                    //_subletTotal
                    double cpOutlet = 0;

                    var miscPercent = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                    var miscBreafast = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.BREAKFAST).FirstOrDefault();
                    var miscLunch = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.LUNCH).FirstOrDefault();
                    var miscDinner = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.DINNER).FirstOrDefault();

                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub-total"));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                    ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                    ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                        + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                    ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                    ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                    ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                    if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                        || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                        cpOutlet = 0;
                    else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                            / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                    ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                    ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                    ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    #endregion

                    #region TOTAL
                    if (listMiscDisPeriodByStore == null)
                        listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();
                    index = index - 1;
                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                    ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                    ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                    ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                        + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                    //ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount)).ToString("F");
                    ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                    ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                    ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                    if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                    || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                        cpOutlet = 0;
                    else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                            / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                    ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                    ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                    ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    //Morning
                    var discountMonring = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.BREAKFAST && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                    var discountLunch = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.LUNCH && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                    var discountDinner = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.DINNER && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();

                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("D" + index).Value = _breakfastOutletTotal.Qty;
                    ws.Cell("E" + index).Value = (_breakfastOutletTotal.ItemTotal + (miscBreafast != null ? miscBreafast.MiscTotal : 0)).ToString("F");
                    ws.Cell("F" + index).Value = (_breakfastOutletTotal.Percent + (miscBreafast != null ? miscBreafast.Percent : 0)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-_breakfastOutletTotal.Discount - (discountMonring != null ? discountMonring.BillDiscountTotal : 0)).ToString("F");
                    ws.Cell("H" + index).Value = (-_breakfastOutletTotal.Promotion).ToString("F");
                    ws.Cell("I" + index).Value = _breakfastOutletTotal.UnitCost.ToString("F");
                    ws.Cell("J" + index).Value = _breakfastOutletTotal.TotalCost.ToString("F");
                    if (_breakfastOutletTotal.TotalCost == 0 || _breakfastOutletTotal.ItemTotal == 0)
                        _breakfastOutletTotal.CP = 0;
                    else
                        _breakfastOutletTotal.CP = ((_breakfastOutletTotal.TotalCost / _breakfastOutletTotal.ItemTotal) * 100);
                    ws.Cell("K" + index).Value = _breakfastOutletTotal.CP.ToString("F") + " %";
                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    //Afternoon
                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("D" + index).Value = _lunchOutletTotal.Qty;
                    ws.Cell("E" + index).Value = (_lunchOutletTotal.ItemTotal + (miscLunch != null ? miscLunch.MiscTotal : 0)).ToString("F");
                    ws.Cell("F" + index).Value = (_lunchOutletTotal.Percent + (miscLunch != null ? miscLunch.Percent : 0)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-_lunchOutletTotal.Discount - (discountLunch != null ? discountLunch.BillDiscountTotal : 0)).ToString("F");
                    ws.Cell("H" + index).Value = (-_lunchOutletTotal.Promotion).ToString("F");
                    ws.Cell("I" + index).Value = _lunchOutletTotal.UnitCost.ToString("F");
                    ws.Cell("J" + index).Value = _lunchOutletTotal.TotalCost.ToString("F");
                    if (_lunchOutletTotal.TotalCost == 0 || _lunchOutletTotal.ItemTotal == 0)
                        _lunchOutletTotal.CP = 0;
                    else
                        _lunchOutletTotal.CP = ((_lunchOutletTotal.TotalCost / _lunchOutletTotal.ItemTotal) * 100);
                    ws.Cell("K" + index).Value = _lunchOutletTotal.CP.ToString("F") + " %";
                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    //dinner
                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("D" + index).Value = _dinnerOutletTotal.Qty;
                    ws.Cell("E" + index).Value = (_dinnerOutletTotal.ItemTotal + (miscDinner != null ? miscDinner.MiscTotal : 0)).ToString("F");
                    ws.Cell("F" + index).Value = (_dinnerOutletTotal.Percent + (miscDinner != null ? miscDinner.Percent : 0)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-_dinnerOutletTotal.Discount - (discountDinner != null ? discountDinner.BillDiscountTotal : 0)).ToString("F");
                    ws.Cell("H" + index).Value = (-_dinnerOutletTotal.Promotion).ToString("F");
                    ws.Cell("I" + index).Value = _dinnerOutletTotal.UnitCost.ToString("F");
                    ws.Cell("J" + index).Value = _dinnerOutletTotal.TotalCost.ToString("F");
                    if (_dinnerOutletTotal.TotalCost == 0 || _dinnerOutletTotal.ItemTotal == 0)
                        _dinnerOutletTotal.CP = 0;
                    else
                        _dinnerOutletTotal.CP = ((_dinnerOutletTotal.TotalCost / _dinnerOutletTotal.ItemTotal) * 100);
                    ws.Cell("K" + index).Value = _dinnerOutletTotal.CP.ToString("F") + " %";
                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    ws.Range("C" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //format file
                    //header
                    ws.Range("A4:K6").Style.Font.SetBold(true);
                    //Set color
                    ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //set Border        
                    ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    //ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    if (_firstStore)
                    {
                        ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    else
                    {
                        ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    index++;

                    #endregion end total
                    #region Summary
                    double refund = 0;
                    //for (int i = 0; i < lstBusinessId.Count; i++)
                    //{
                    //    lstOrderId = lstData.Where(ww => ww.BusinessId == lstBusinessId[i]).Select(ss => ss.OrderId).ToList();
                    //    if (lstOrderId != null && lstOrderId.Any())
                    //        lstOrderId = lstOrderId.Distinct().ToList();

                    //    var lstRefundForReceiptOld = lstDataRefund.Where(ww => ww.BusinessDayId == lstBusinessId[i]
                    //            && !lstOrderId.Contains(ww.OrderId)).ToList();
                    //    if (lstRefundForReceiptOld != null && lstRefundForReceiptOld.Any())
                    //        refund += lstRefundForReceiptOld.Sum(ss => ss.TotalRefund - ss.Tax - ss.ServiceCharged);
                    //}


                    bool isTaxInclude = false;
                    ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                    ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                    if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                    {
                        ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                    }
                    else
                    {
                        ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                        isTaxInclude = true;
                    }
                    ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                    ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                    ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    index++;
                    //double _noincludeSale = _lstNoIncludeOnSale.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.Amount);
                    double _noincludeSale = _lstNoIncludeOnSale.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId)
                        .Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount));
                    if (isTaxInclude)
                    {
                        _noincludeSale = _lstNoIncludeOnSale.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId)
                        .Sum(ss => (double)((decimal)ss.Amount - (decimal)ss.DiscountAmount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                    }
                    //GC value
                    var payGC = lstPayments.Where(p => p.StoreId == itemGroupStore.Key.StoreId
                                                      ).Sum(p => p.Amount);

                    //ws.Cell("A" + index).Value = (total.ItemTotal - subDisPeriod).ToString("F");
                    //sell GC 22/09/2017
                    double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                            && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                            && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.TotalAmount.Value);
                    ws.Cell("A" + index).Value = (_itemTotal.ItemTotal - subDisPeriod - (isTaxInclude ? _itemTotal.TaxTotal : 0)
                        - refund - _noincludeSale - payGC + giftCardSell).ToString("F");

                    ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                    ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                    ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                    ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                    ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    #endregion

                    index += 2;
                    indexNextStore = index;
                    _firstStore = false;
                }

            }


            ws.Columns(1, 3).AdjustToContents();
            //set Width for Colum 
            ws.Column(4).Width = 20;
            ws.Columns(5, 11).AdjustToContents();
            return wb;
        }

        public XLWorkbook ExportExcelEmpty(BaseReportModel viewmodel)
        {
            string sheetName = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized_Sales_Analysis_Report");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 11, viewmodel.FromDate, viewmodel.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized Sales Analysis Report"));

            //ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            //ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
            //ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            //ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            //ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            //ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
            //ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
            //ws.Cell("H5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
            //ws.Cell("I5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
            //ws.Cell("J5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
            //ws.Cell("K5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

            //set width column
            ws.Column("A").Width = 20;
            ws.Column("B").Width = 30;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 10;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 15;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 15;

            // Format header report
            ws.Range(1, 1, 4, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            return wb;
        }

        // Updated 08242017
        // Get item sales depend on BaseReportModel with GLAccountCode != null || IsIncludeSale == false
        // For DailySalesReport and FJDailySalesReport
        public List<ItemizedSalesAnalysisReportModels> GetItemsForDailySalesReports(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_ItemizedSalesAnalysisReport
                               where model.ListStores.Contains(tb.StoreId)
                                    && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate) && tb.Mode == model.Mode
                                    && (tb.GLAccountCode != null || tb.IsIncludeSale == false)

                               select new ItemizedSalesAnalysisReportModels
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                                   CategoryId = tb.CategoryId,
                                   CategoryName = tb.CategoryName,
                                   ExtraPrice = tb.ExtraPrice,
                                   TotalPrice = tb.TotalPrice,
                                   GLAccountCode = tb.GLAccountCode,
                                   IsIncludeSale = tb.IsIncludeSale,
                                   BusinessId = tb.BusinessId,
                                   ServiceCharge = tb.ServiceCharge,
                                   Tax = tb.Tax,
                                   ExtraAmount = tb.ExtraAmount.HasValue ? tb.ExtraAmount.Value : 0,
                                   TotalAmount = tb.TotalAmount.HasValue ? tb.TotalAmount.Value : 0,
                                   TotalDiscount = tb.TotalDiscount.HasValue ? tb.TotalDiscount : 0,
                                   PromotionAmount = tb.PromotionAmount
                               }).ToList();
                return lstData;
            }
        }

        public List<ItemizedSalesAnalysisReportModels> GetItemsForDailyReceiptReports(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_ItemizedSalesAnalysisReport
                               where model.ListStores.Contains(tb.StoreId)
                                    && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate) && tb.Mode == model.Mode
                                    && (tb.GLAccountCode != null || tb.IsIncludeSale == false)

                               select new ItemizedSalesAnalysisReportModels
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                                   CategoryId = tb.CategoryId,
                                   CategoryName = tb.CategoryName,
                                   ExtraPrice = tb.ExtraPrice,
                                   TotalPrice = tb.TotalPrice,
                                   GLAccountCode = tb.GLAccountCode,
                                   IsIncludeSale = tb.IsIncludeSale,
                                   BusinessId = tb.BusinessId,
                                   ServiceCharge = tb.ServiceCharge,
                                   Tax = tb.Tax,
                                   ExtraAmount = tb.ExtraAmount.HasValue ? tb.ExtraAmount.Value : 0,
                                   TotalAmount = tb.TotalAmount.HasValue ? tb.TotalAmount.Value : 0,
                                   TotalDiscount = tb.TotalDiscount.HasValue ? tb.TotalDiscount : 0,
                                   PromotionAmount = tb.PromotionAmount,
                                   ReceiptId = tb.ReceiptId
                               }).ToList();
                return lstData;
            }
        }

        // Updated 09202017
        public XLWorkbook ExportExcel_CreditNote(List<ItemizedSalesAnalysisReportModels> lstData, ItemizedSalesAnalysisReportModel viewmodel
            , List<StoreModels> lstStores, List<string> lstGCId
            , List<RFilterCategoryV1Model> _lstCateChecked, List<RFilterCategoryModel> _lstSetChecked
            , List<RFilterCategoryV1ReportModel> lstTotalAllCate, List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu)
        {

            string sheetName = "Itemized_Sales_Analysis_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 11, viewmodel.FromDateFilter, viewmodel.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized Sales Analysis Report"));

            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];

            // Get value from setting of common
            TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
            TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
            TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
            TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
            TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
            TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);

            List<PaymentModels> lstPayments = _orderPaymentMethodFactory.GetDataPaymentItemsByGC(viewmodel, lstGCId);

            // MISC DISCOUNT TOTAL BILL
            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
            List<DiscountAndMiscReportModels> listMisc_Discout = new List<DiscountAndMiscReportModels>();
            listMisc_Discout = miscFactory.GetReceiptDiscountAndMisc(viewmodel.FromDate, viewmodel.ToDate, viewmodel.ListStores, viewmodel.Mode, viewmodel.FromDateFilter, viewmodel.ToDateFilter, viewmodel.FilterType);
            listMisc_Discout.ForEach(ss => ss.DiscountValue = 0);

            //Get rounding amount
            var _lstDataDailys = GetRoundingAmount(viewmodel);

            DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
            var lstDiscount = discountDetailFactory.GetDiscountTotal(viewmodel.ListStores, viewmodel.FromDate, viewmodel.ToDate, viewmodel.Mode, viewmodel.FromDateFilter, viewmodel.ToDateFilter, viewmodel.FilterType);

            listMisc_Discout.AddRange(lstDiscount);

            //get list refund by GC
            var lstRefunds = _refundFactory.GetListRefundWithoutDetail(viewmodel);

            // Filter date by time
            switch (viewmodel.FilterType)
            {
                case (int)Commons.EFilterType.OnDay:
                    if (lstPayments != null && lstPayments.Any())
                    {
                        lstPayments = lstPayments.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (_lstDataDailys != null && _lstDataDailys.Any())
                    {
                        _lstDataDailys = _lstDataDailys.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (lstRefunds != null && lstRefunds.Any())
                    {
                        lstRefunds = lstRefunds.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    break;
                case (int)Commons.EFilterType.Days:
                    if (lstPayments != null && lstPayments.Any())
                    {
                        lstPayments = lstPayments.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (_lstDataDailys != null && _lstDataDailys.Any())
                    {
                        _lstDataDailys = _lstDataDailys.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (lstRefunds != null && lstRefunds.Any())
                    {
                        lstRefunds = lstRefunds.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    break;
            }

            List<MISCBillDiscountPeriodModels> listMiscDisPeriod = new List<MISCBillDiscountPeriodModels>();
            if (listMisc_Discout != null)
            {
                var listMisc_DiscoutGroupStore = listMisc_Discout.GroupBy(gg => gg.StoreId).ToList();

                foreach (var item in listMisc_DiscoutGroupStore)
                {
                    // CHECK PERIOD IS CHECKED
                    if (viewmodel.Breakfast)
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
                    if (viewmodel.Lunch)
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
                    if (viewmodel.Dinner)
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

                }
            }

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
            ws.Cell("H5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
            ws.Cell("I5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
            ws.Cell("J5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
            ws.Cell("K5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

            //set width column
            ws.Column("A").Width = 20;
            ws.Column("B").Width = 30;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 10;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 15;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 15;

            int index = 6;
            bool isFirstStore = true;
            int indexNextStore = 0;
            //Total
            ItemizedSalesNewTotal _itemTotal = new ItemizedSalesNewTotal();
            var _breakfastTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerTotal = new ItemizedSalesPeriodValueTotal();

            var _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
            var lstBusinessId = new List<string>();
            bool _firstStore = true;
            if (lstData != null && lstData.Any())
            {
                // Group storeId
                TimeSpan timeDish = new TimeSpan();

                lstData.ForEach(x =>
                {
                    var store = lstStores.Where(z => z.Id.Equals(x.StoreId)).FirstOrDefault();
                    x.StoreName = store == null ? "" : store.Name;
                });

                List<RFilterCategoryV1ReportModel> lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                double subTotalQty = 0;
                double subTotalItem = 0;
                double subTotalPercent = 0;
                double subTotalDiscount = 0;
                double subTotalPromotion = 0;
                double subTotalUnitCost = 0;
                double subTotalTotalCost = 0;
                double subTotalCP = 0;

                double breakfastTotalQty = 0;
                double breakfastTotalItem = 0;
                double breakfastTotalPercent = 0;
                double breakfastTotalDiscount = 0;
                double breakfastTotalPromotion = 0;
                double breakfastTotalUnitCost = 0;
                double breakfastTotalTotalCost = 0;
                double breakfastTotalCP = 0;

                double lunchTotalQty = 0;
                double lunchTotalItem = 0;
                double lunchTotalPercent = 0;
                double lunchTotalDiscount = 0;
                double lunchTotalPromotion = 0;
                double lunchTotalUnitCost = 0;
                double lunchTotalTotalCost = 0;
                double lunchTotalCP = 0;

                double dinnerTotalQty = 0;
                double dinnerTotalItem = 0;
                double dinnerTotalPercent = 0;
                double dinnerTotalDiscount = 0;
                double dinnerTotalPromotion = 0;
                double dinnerTotalUnitCost = 0;
                double dinnerTotalTotalCost = 0;
                double dinnerTotalCP = 0;

                double _noincludeSale = 0;
                double payGC = 0;
                double _taxOfPayGCNotInclude = 0;
                double _svcOfPayGCNotInclude = 0;


                // Get list item for NetSales (IsIncludeSale == false)
                List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportModels>();

                List<MISCBillDiscountPeriodModels> listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();

                var lstItemGroupByStore = lstData.Where(ww => string.IsNullOrEmpty(ww.GiftCardId))
                        .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                if (lstItemGroupByStore != null && lstItemGroupByStore.Any())
                {
                    foreach (var itemGroupStore in lstItemGroupByStore)
                    {
                        lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                        // Get store setting : time of Period
                        if (currentUser != null)
                        {
                            // Get value from setting of mechant
                            Commons.BreakfastStart = currentUser.BreakfastStart;
                            Commons.BreakfastEnd = currentUser.BreakfastEnd;
                            Commons.LunchStart = currentUser.LunchStart;
                            Commons.LunchEnd = currentUser.LunchEnd;
                            Commons.DinnerStart = currentUser.DinnerStart;
                            Commons.DinnerEnd = currentUser.DinnerEnd;

                            // Get value from setting of store
                            if (currentUser.ListSetting != null && currentUser.ListSetting.Any())
                            {
                                var settingPeriodOfStore = currentUser.ListSetting.Where(w => w.StoreID == itemGroupStore.Key.StoreId).ToList();

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

                        // Get listMiscDisPeriod in store
                        var lstItemInStore = listMisc_Discout.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();

                        for (int i = 0; i < lstItemInStore.Count; i++)
                        {
                            lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportModels>();

                            // Get Total Misc to + ItemTotal
                            TimeSpan timeMisc = new TimeSpan(lstItemInStore[i].Hour, 0, 0);
                            // Total period Misc_Discout
                            // TOTAL PERIOD MISC AND DISCOUNT BILL TOTAL
                            if (viewmodel.Breakfast)
                            {
                                if (timeMisc >= brearkStart && timeMisc < brearkEnd)
                                {
                                    var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == itemGroupStore.Key.StoreId);
                                    period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                    period.MiscTotal += lstItemInStore[i].MiscValue;
                                    period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                }
                            }
                            if (viewmodel.Lunch)
                            {
                                if (timeMisc >= lunchStart && timeMisc < lunchEnd)
                                {
                                    var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == itemGroupStore.Key.StoreId);
                                    period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                    period.MiscTotal += lstItemInStore[i].MiscValue;
                                    period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                }
                            }
                            if (viewmodel.Dinner)
                            {
                                if (dinnerStart > dinnerEnd)//pass day
                                {
                                    if (timeMisc >= dinnerStart || timeMisc < dinnerEnd)
                                    {
                                        var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == itemGroupStore.Key.StoreId);
                                        period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                        period.MiscTotal += lstItemInStore[i].MiscValue;
                                        period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                    }
                                }
                                else//in day
                                {
                                    if (timeMisc >= dinnerStart && timeMisc < dinnerEnd)
                                    {
                                        var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == itemGroupStore.Key.StoreId);
                                        period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                        period.MiscTotal += lstItemInStore[i].MiscValue;
                                        period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                    }
                                }
                            }
                        }

                        var miscInstore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.MiscTotal);

                        double subDisPeriodByStore = 0;
                        double subDisPeriod = 0;
                        _itemTotal = new ItemizedSalesNewTotal();
                        _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
                        _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
                        _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
                        if (!isFirstStore)
                        {
                            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
                            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
                            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
                            ws.Cell("F" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
                            ws.Cell("G" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                            ws.Cell("H" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
                            ws.Cell("I" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
                            ws.Cell("J" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
                            ws.Cell("K" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

                            //header
                            ws.Range("A" + index + ":K" + index).Style.Font.SetBold(true);
                            //Set color
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + index + ":K" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Row(index).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            index++;
                        }
                        isFirstStore = false;
                        // Store name
                        var currentStore = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).FirstOrDefault();
                        var storeName = currentStore != null ? currentStore.Name + " in " + currentStore.CompanyName : "";
                        //var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                        ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        index++;

                        var lstItems = itemGroupStore.ToList();
                        // Group item type
                        var lstGroupItemType = lstItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                       || ww.ItemTypeId == (int)Commons.EProductType.SetMenu))
                           .OrderBy(x => x.ItemTypeId).GroupBy(gg => gg.ItemTypeId).ToList();

                        var itemAmountTotal = lstItems.Sum(ss => ss.ItemTotal);
                        foreach (var itemTypeId in lstGroupItemType)
                        {
                            var lstItemOfType = lstItems.Where(x => x.ItemTypeId == itemTypeId.Key).ToList();

                            List<RFilterCategoryModel> lstCateCheckedInStore = new List<RFilterCategoryModel>();

                            if (itemTypeId.Key == (int)Commons.EProductType.Dish)
                            {
                                lstCateCheckedInStore = _lstCateChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId)
                                    .Select(s => new RFilterCategoryModel
                                    {
                                        Id = s.Id,
                                        Name = s.Name
                                    }).ToList();

                                lstTotalDataInStore = lstTotalAllCate.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                            }
                            else //set
                            {
                                //check data old
                                var tmp = lstItemOfType.Where(ww => ww.CategoryId == "SetMenu").FirstOrDefault();
                                //check setmenu have category or not category
                                var categoryExist = _lstSetChecked.Where(ww => string.IsNullOrEmpty(ww.CategoryID)).FirstOrDefault();
                                if (categoryExist != null || tmp != null)//
                                {
                                    lstCateCheckedInStore = new List<RFilterCategoryModel>() {
                                    new RFilterCategoryModel(){Id="SetMenu", Name ="SetMenu"}
                                };
                                }
                                else
                                {
                                    var lstTmp = _lstSetChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId)
                                      .Select(s => new
                                      {
                                          Id = s.CategoryID,
                                          Name = s.CategoryName
                                      }).Distinct().ToList();

                                    lstCateCheckedInStore = lstTmp
                                        .Select(s => new RFilterCategoryModel
                                        {
                                            Id = s.Id,
                                            Name = s.Name
                                        }).ToList();
                                }
                                lstTotalDataInStore = lstTotalAllSetMenu.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                                lstTotalDataInStore.Add(new RFilterCategoryV1ReportModel() { CateId = "SetMenu", CateName = "SetMenu" });
                                //lstCateCheckedInStore = _lstSetChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId).Select(s => new RFilterCategoryModel
                                //{
                                //    Id = s.Id,
                                //    Name = s.Name
                                //}).ToList();

                                //lstTotalDataInStore = lstTotalAllSetMenu.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                            }

                            // Group by category
                            foreach (var itemCate in lstCateCheckedInStore)
                            {
                                subTotalQty = 0;
                                subTotalItem = 0;
                                subTotalPercent = 0;
                                subTotalDiscount = 0;
                                subTotalPromotion = 0;
                                subTotalUnitCost = 0;
                                subTotalTotalCost = 0;
                                subTotalCP = 0;

                                breakfastTotalQty = 0;
                                breakfastTotalItem = 0;
                                breakfastTotalPercent = 0;
                                breakfastTotalDiscount = 0;
                                breakfastTotalPromotion = 0;
                                breakfastTotalUnitCost = 0;
                                breakfastTotalTotalCost = 0;
                                breakfastTotalCP = 0;

                                lunchTotalQty = 0;
                                lunchTotalItem = 0;
                                lunchTotalPercent = 0;
                                lunchTotalDiscount = 0;
                                lunchTotalPromotion = 0;
                                lunchTotalUnitCost = 0;
                                lunchTotalTotalCost = 0;
                                lunchTotalCP = 0;

                                dinnerTotalQty = 0;
                                dinnerTotalItem = 0;
                                dinnerTotalPercent = 0;
                                dinnerTotalDiscount = 0;
                                dinnerTotalPromotion = 0;
                                dinnerTotalUnitCost = 0;
                                dinnerTotalTotalCost = 0;
                                dinnerTotalCP = 0;

                                ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Name));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                index++;

                                var lstCateItems = lstItemOfType.Where(w => w.CategoryId == itemCate.Id).ToList();
                                var lstItemTypes = lstCateItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                                                        || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).ToList();

                                List<ItemizedSalesAnalysisReportModels> lstDataByPeriod = new List<ItemizedSalesAnalysisReportModels>();

                                var breakfast = new ItemizedSalesPeriodValueTotal();
                                var lunch = new ItemizedSalesPeriodValueTotal();
                                var dinner = new ItemizedSalesPeriodValueTotal();
                                foreach (var item in lstItemTypes)
                                {
                                    timeDish = item.CreatedDate.TimeOfDay;
                                    if ((itemAmountTotal + miscInstore) != 0)
                                    {
                                        item.Percent = (item.ItemTotal / (itemAmountTotal + miscInstore)) * 100;
                                    }
                                    // check percent data
                                    // Breakfast
                                    if (viewmodel.Breakfast)
                                    {
                                        if (timeDish >= brearkStart && timeDish < brearkEnd)
                                        {
                                            breakfast.Qty += item.Quantity;
                                            breakfast.ItemTotal += item.ItemTotal;
                                            breakfast.Percent += item.Percent;
                                            breakfast.Discount += item.Discount;
                                            breakfast.Promotion += item.PromotionAmount;
                                            breakfast.UnitCost += item.Cost;
                                            breakfast.TotalCost += item.TotalCost;

                                            _breakfastOutletTotal.Qty += item.Quantity;
                                            _breakfastOutletTotal.ItemTotal += item.ItemTotal;
                                            _breakfastOutletTotal.Percent += item.Percent;
                                            _breakfastOutletTotal.Discount += item.Discount;
                                            _breakfastOutletTotal.Promotion += item.PromotionAmount;
                                            _breakfastOutletTotal.UnitCost += item.Cost;
                                            _breakfastOutletTotal.TotalCost += item.TotalCost;

                                            _breakfastTotal.Qty += item.Quantity;
                                            _breakfastTotal.ItemTotal += item.ItemTotal;
                                            _breakfastTotal.Percent += item.Percent;
                                            _breakfastTotal.Discount += item.Discount;
                                            _breakfastTotal.Promotion += item.PromotionAmount;
                                            _breakfastTotal.UnitCost += item.Cost;
                                            _breakfastTotal.TotalCost += item.TotalCost;

                                            // If CreatedDate is avaible with time of period
                                            lstDataByPeriod.Add(item);
                                        }
                                    }

                                    // lunch
                                    if (viewmodel.Lunch)
                                    {
                                        if (timeDish >= lunchStart && timeDish < lunchEnd)
                                        {
                                            lunch.Qty += item.Quantity;
                                            lunch.ItemTotal += item.ItemTotal;
                                            lunch.Percent += item.Percent;
                                            lunch.Discount += item.Discount;
                                            lunch.Promotion += item.PromotionAmount;
                                            lunch.UnitCost += item.Cost;
                                            lunch.TotalCost += item.TotalCost;

                                            _lunchOutletTotal.Qty += item.Quantity;
                                            _lunchOutletTotal.ItemTotal += item.ItemTotal;
                                            _lunchOutletTotal.Percent += item.Percent;
                                            _lunchOutletTotal.Discount += item.Discount;
                                            _lunchOutletTotal.Promotion += item.PromotionAmount;
                                            _lunchOutletTotal.UnitCost += item.Cost;
                                            _lunchOutletTotal.TotalCost += item.TotalCost;

                                            _lunchTotal.Qty += item.Quantity;
                                            _lunchTotal.ItemTotal += item.ItemTotal;
                                            _lunchTotal.Percent += item.Percent;
                                            _lunchTotal.Discount += item.Discount;
                                            _lunchTotal.Promotion += item.PromotionAmount;
                                            _lunchTotal.UnitCost += item.Cost;
                                            _lunchTotal.TotalCost += item.TotalCost;

                                            // If CreatedDate is avaible with time of period
                                            lstDataByPeriod.Add(item);
                                        }
                                    }

                                    // Dinner
                                    if (viewmodel.Dinner)
                                    {
                                        if (dinnerStart > dinnerEnd) //pass day
                                        {
                                            if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                            {
                                                dinner.Qty += item.Quantity;
                                                dinner.ItemTotal += item.ItemTotal;
                                                dinner.Percent += item.Percent;
                                                dinner.Discount += item.Discount;
                                                dinner.Promotion += item.PromotionAmount;
                                                dinner.UnitCost += item.Cost;
                                                dinner.TotalCost += item.TotalCost;

                                                _dinnerOutletTotal.Qty += item.Quantity;
                                                _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                                _dinnerOutletTotal.Percent += item.Percent;
                                                _dinnerOutletTotal.Discount += item.Discount;
                                                _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                                _dinnerOutletTotal.UnitCost += item.Cost;
                                                _dinnerOutletTotal.TotalCost += item.TotalCost;

                                                _dinnerTotal.Qty += item.Quantity;
                                                _dinnerTotal.ItemTotal += item.ItemTotal;
                                                _dinnerTotal.Percent += item.Percent;
                                                _dinnerTotal.Discount += item.Discount;
                                                _dinnerTotal.Promotion += item.PromotionAmount;
                                                _dinnerTotal.UnitCost += item.Cost;
                                                _dinnerTotal.TotalCost += item.TotalCost;

                                                // If CreatedDate is avaible with time of period
                                                lstDataByPeriod.Add(item);
                                            }
                                        }
                                        else //in day
                                        {
                                            if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                            {
                                                dinner.Qty += item.Quantity;
                                                dinner.ItemTotal += item.ItemTotal;
                                                dinner.Percent += item.Percent;
                                                dinner.Discount += item.Discount;
                                                dinner.Promotion += item.PromotionAmount;
                                                dinner.UnitCost += item.Cost;
                                                dinner.TotalCost += item.TotalCost;

                                                _dinnerOutletTotal.Qty += item.Quantity;
                                                _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                                _dinnerOutletTotal.Percent += item.Percent;
                                                _dinnerOutletTotal.Discount += item.Discount;
                                                _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                                _dinnerOutletTotal.UnitCost += item.Cost;
                                                _dinnerOutletTotal.TotalCost += item.TotalCost;

                                                _dinnerTotal.Qty += item.Quantity;
                                                _dinnerTotal.ItemTotal += item.ItemTotal;
                                                _dinnerTotal.Percent += item.Percent;
                                                _dinnerTotal.Discount += item.Discount;
                                                _dinnerTotal.Promotion += item.PromotionAmount;
                                                _dinnerTotal.UnitCost += item.Cost;
                                                _dinnerTotal.TotalCost += item.TotalCost;

                                                // If CreatedDate is avaible with time of period
                                                lstDataByPeriod.Add(item);
                                            }
                                        }
                                    }

                                }//End lstItemTypes

                                // Get total item
                                _itemTotal.SCTotal += lstDataByPeriod.Sum(s => s.ServiceCharge);
                                _itemTotal.ItemTotal += lstDataByPeriod.Sum(s => s.ItemTotal);
                                _itemTotal.TaxTotal += lstDataByPeriod.Sum(s => s.Tax);
                                _itemTotal.DiscountTotal += lstDataByPeriod.Sum(s => s.Discount);
                                _itemTotal.PromotionTotal += lstDataByPeriod.Sum(s => s.PromotionAmount);

                                // Add items to lst item for NetSales
                                lstItemNoIncludeSaleInStore.AddRange(lstDataByPeriod.Where(w => w.IsIncludeSale == false).ToList());

                                //Group Item
                                var lstItemTypeGroup = lstDataByPeriod.GroupBy(gg => new { gg.ItemTypeId, gg.ItemId, gg.Price }).ToList();
                                double cp = 0; double cost = 0;
                                foreach (var item in lstItemTypeGroup)
                                {
                                    ws.Cell("A" + index).Value = item.Select(ss => ss.ItemCode).FirstOrDefault();
                                    ws.Cell("B" + index).Value = item.Select(ss => ss.ItemName).FirstOrDefault();
                                    ws.Cell("C" + index).Value = item.Select(ss => ss.Price).FirstOrDefault().ToString("F");
                                    ws.Cell("C" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Cell("D" + index).Value = item.Sum(d => d.Quantity);
                                    ws.Cell("E" + index).Value = item.Sum(d => d.ItemTotal).ToString("F");
                                    ws.Cell("F" + index).Value = item.Sum(d => d.Percent).ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-item.Sum(d => d.Discount)).ToString("F");

                                    ws.Cell("H" + index).Value = (-item.Sum(d => d.PromotionAmount)).ToString("F");
                                    ws.Cell("I" + index).Value = item.Select(ss => ss.Cost).FirstOrDefault().ToString("F");
                                    cost += item.Select(ss => ss.Cost).FirstOrDefault();
                                    ws.Cell("J" + index).Value = item.Sum(d => d.TotalCost).ToString("F");

                                    if (item.Sum(d => d.TotalCost) == 0 || item.Sum(d => d.ItemTotal) == 0)
                                        cp = 0;
                                    else
                                        cp = ((item.Sum(d => d.TotalCost) / item.Sum(d => d.ItemTotal)) * 100);

                                    ws.Cell("K" + index).Value = cp.ToString("F") + " %";
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;
                                }//end lstItemTypeGroup

                                subTotalQty = breakfast.Qty + lunch.Qty + dinner.Qty;
                                subTotalItem = breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal;
                                subTotalPercent = breakfast.Percent + lunch.Percent + dinner.Percent;
                                subTotalDiscount = breakfast.Discount + lunch.Discount + dinner.Discount;
                                subTotalPromotion = breakfast.Promotion + lunch.Promotion + dinner.Promotion;
                                subTotalUnitCost = cost;
                                subTotalTotalCost = breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost;
                                if ((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) != 0 && (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal) != 0)
                                {
                                    subTotalCP = (((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) / (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal)) * 100);
                                }

                                breakfastTotalQty = breakfast.Qty;
                                breakfastTotalItem = breakfast.ItemTotal;
                                breakfastTotalPercent = breakfast.Percent;
                                breakfastTotalDiscount = breakfast.Discount;
                                breakfastTotalPromotion = breakfast.Promotion;
                                breakfastTotalUnitCost = breakfast.UnitCost;
                                breakfastTotalTotalCost = breakfast.TotalCost;
                                if (breakfast.TotalCost != 0 && breakfast.ItemTotal != 0)
                                {
                                    breakfastTotalCP = ((breakfast.TotalCost / breakfast.ItemTotal) * 100);
                                }

                                lunchTotalQty = lunch.Qty;
                                lunchTotalItem = lunch.ItemTotal;
                                lunchTotalPercent = lunch.Percent;
                                lunchTotalDiscount = lunch.Discount;
                                lunchTotalPromotion = lunch.Promotion;
                                lunchTotalUnitCost = lunch.UnitCost;
                                lunchTotalTotalCost = lunch.TotalCost;
                                lunchTotalCP = 0;
                                if (lunch.TotalCost != 0 && lunch.ItemTotal != 0)
                                {
                                    lunchTotalCP = ((lunch.TotalCost / lunch.ItemTotal) * 100);
                                }

                                dinnerTotalQty = dinner.Qty;
                                dinnerTotalItem = dinner.ItemTotal;
                                dinnerTotalPercent = dinner.Percent;
                                dinnerTotalDiscount = dinner.Discount;
                                dinnerTotalPromotion = dinner.Promotion;
                                dinnerTotalUnitCost = dinner.UnitCost;
                                dinnerTotalTotalCost = dinner.TotalCost;
                                if (dinner.TotalCost != 0 && dinner.ItemTotal != 0)
                                {
                                    dinnerTotalCP = ((dinner.TotalCost / dinner.ItemTotal) * 100);
                                }

                                // Get total information of current cate
                                var totalDataCurrentCate = lstTotalDataInStore.Where(w => w.CateId == itemCate.Id).FirstOrDefault();
                                // Update total data of current cate
                                totalDataCurrentCate.SubTotalQty += subTotalQty;
                                totalDataCurrentCate.SubTotalItem += subTotalItem;
                                totalDataCurrentCate.SubTotalPercent += subTotalPercent;
                                totalDataCurrentCate.SubTotalDiscount += subTotalDiscount;
                                totalDataCurrentCate.SubTotalPromotion += subTotalPromotion;
                                totalDataCurrentCate.SubTotalUnitCost += subTotalUnitCost;
                                totalDataCurrentCate.SubTotalTotalCost += subTotalTotalCost;
                                totalDataCurrentCate.SubTotalCP += subTotalCP;

                                totalDataCurrentCate.BreakfastTotalQty += breakfastTotalQty;
                                totalDataCurrentCate.BreakfastTotalItem += breakfastTotalItem;
                                totalDataCurrentCate.BreakfastTotalPercent += breakfastTotalPercent;
                                totalDataCurrentCate.BreakfastTotalDiscount += breakfastTotalDiscount;
                                totalDataCurrentCate.BreakfastTotalPromotion += breakfastTotalPromotion;
                                totalDataCurrentCate.BreakfastTotalUnitCost += breakfastTotalUnitCost;
                                totalDataCurrentCate.BreakfastTotalTotalCost += breakfastTotalTotalCost;
                                totalDataCurrentCate.BreakfastTotalCP += breakfastTotalCP;

                                totalDataCurrentCate.LunchTotalQty += lunchTotalQty;
                                totalDataCurrentCate.LunchTotalItem += lunchTotalItem;
                                totalDataCurrentCate.LunchTotalPercent += lunchTotalPercent;
                                totalDataCurrentCate.LunchTotalDiscount += lunchTotalDiscount;
                                totalDataCurrentCate.LunchTotalPromotion += lunchTotalPromotion;
                                totalDataCurrentCate.LunchTotalUnitCost += lunchTotalUnitCost;
                                totalDataCurrentCate.LunchTotalTotalCost += lunchTotalTotalCost;
                                totalDataCurrentCate.LunchTotalCP += lunchTotalCP;

                                totalDataCurrentCate.DinnerTotalQty += dinnerTotalQty;
                                totalDataCurrentCate.DinnerTotalItem += dinnerTotalItem;
                                totalDataCurrentCate.DinnerTotalPercent += dinnerTotalPercent;
                                totalDataCurrentCate.DinnerTotalDiscount += dinnerTotalDiscount;
                                totalDataCurrentCate.DinnerTotalPromotion += dinnerTotalPromotion;
                                totalDataCurrentCate.DinnerTotalUnitCost += dinnerTotalUnitCost;
                                totalDataCurrentCate.DinnerTotalTotalCost += dinnerTotalTotalCost;
                                totalDataCurrentCate.DinnerTotalCP += dinnerTotalCP;

                                // Show total date of current
                                // If it has any cate child which was checked  => title: Sub-Total: CateName
                                // If it doesn't have any cate child which was checked && it doesn't has parent cate which was checked => it's top cate => title: Category Total: CateName
                                if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && !lstTotalDataInStore.Where(w => !string.IsNullOrEmpty(totalDataCurrentCate.ParentId) && w.CateId == totalDataCurrentCate.ParentId && w.Checked).Any())
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Total") + " : " + totalDataCurrentCate.CateName);
                                }
                                else
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total") + " : " + totalDataCurrentCate.CateName);
                                }

                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                                ws.Cell("D" + index).Value = totalDataCurrentCate.SubTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.SubTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.SubTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.SubTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.SubTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.SubTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.SubTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.SubTotalCP.ToString("F") + " %";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;

                                //Morning
                                if (viewmodel.Breakfast)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.BreakfastTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.BreakfastTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.BreakfastTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.BreakfastTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.BreakfastTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.BreakfastTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.BreakfastTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.BreakfastTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                //Afternoon
                                if (viewmodel.Lunch)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.LunchTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.LunchTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.LunchTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.LunchTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.LunchTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.LunchTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.LunchTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.LunchTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                //dinner
                                if (viewmodel.Dinner)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.DinnerTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.DinnerTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.DinnerTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.DinnerTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.DinnerTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.DinnerTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.DinnerTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.DinnerTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                // Check current cate is last chile of parent current cate
                                Boolean isLastChildCate = false;
                                // If current cate has parent cate, check for show total data of parent cate
                                if (!string.IsNullOrEmpty(totalDataCurrentCate.ParentId))
                                {
                                    var parentCurrentCateInfo = lstTotalDataInStore.Where(w => w.CateId == totalDataCurrentCate.ParentId).FirstOrDefault();
                                    // List cate child
                                    var lstCateChildCheckedOfParent = parentCurrentCateInfo.ListCateChildChecked.ToList();
                                    if (lstCateChildCheckedOfParent != null && lstCateChildCheckedOfParent.Any())
                                    {
                                        // Get id of last child cate
                                        string idLastCateChild = parentCurrentCateInfo.ListCateChildChecked.LastOrDefault().ToString();

                                        // Current cate is last cate of parent cate
                                        if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && idLastCateChild == totalDataCurrentCate.CateId)
                                        {
                                            isLastChildCate = true;
                                        }
                                    }

                                    DisplayTotalOfParentCate(ref ws, totalDataCurrentCate, ref index, ref lstTotalDataInStore, isLastChildCate, totalDataCurrentCate, viewmodel);
                                }
                            }//End Group by category
                        }//end lstGroupItemType

                        // MISC
                        int startM = index;
                        listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();

                        if (listMiscDisPeriod != null && listMiscDisPeriod.Any())
                        {
                            listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                            if (listMiscDisPeriodByStore != null && listMiscDisPeriodByStore.Any())
                            {
                                // MISC
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                                index++;
                                for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                    ws.Cell("E" + index).SetValue(listMiscDisPeriodByStore[m].MiscTotal.ToString("F"));
                                    listMiscDisPeriodByStore[m].Percent = ((listMiscDisPeriodByStore[m].MiscTotal / (itemAmountTotal + miscInstore)) * 100);
                                    ws.Cell("F" + index).SetValue(listMiscDisPeriodByStore[m].Percent.ToString("F") + " %");
                                    index++;

                                    _itemTotal.ItemTotal += listMiscDisPeriodByStore[m].MiscTotal;
                                }

                                // Discount Total Bill
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                                index++;
                                for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                    ws.Cell("G" + index).SetValue((-listMiscDisPeriodByStore[m].BillDiscountTotal).ToString("F"));
                                    subDisPeriod += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                    subDisPeriodByStore += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                    _itemTotal.DiscountTotal += listMiscDisPeriodByStore[m].BillDiscountTotal;

                                    index++;

                                }
                            }
                        }

                        ws.Range("A" + startM + ":K" + index).Style.Font.SetBold(true);
                        ws.Range("A" + startM + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + startM + ":D" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Range("E" + startM + ":G" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Outlet Sub-total
                        //_subletTotal
                        double cpOutlet = 0;
                        var miscPercent = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                        var miscBreafast = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.BREAKFAST).FirstOrDefault();
                        var miscLunch = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.LUNCH).FirstOrDefault();
                        var miscDinner = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.DINNER).FirstOrDefault();

                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub-total"));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                        ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                    + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                        ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                            + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                        ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                        ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                        ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                        if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                            || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                            cpOutlet = 0;
                        else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                                / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                        ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                        ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                        ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;

                        // TOTAL
                        if (listMiscDisPeriodByStore == null)
                            listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();
                        index = index - 1;
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                        ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                        ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                    + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                        ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                            + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                        ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                        ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                        ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                        if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                        || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                            cpOutlet = 0;
                        else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                                / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                        ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                        ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                        ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;

                        var discountMonring = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.BREAKFAST && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                        var discountLunch = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.LUNCH && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                        var discountDinner = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.DINNER && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();

                        //Morning
                        if (viewmodel.Breakfast)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _breakfastOutletTotal.Qty;
                            ws.Cell("E" + index).Value = (_breakfastOutletTotal.ItemTotal + (miscBreafast != null ? miscBreafast.MiscTotal : 0)).ToString("F");
                            ws.Cell("F" + index).Value = (_breakfastOutletTotal.Percent + (miscBreafast != null ? miscBreafast.Percent : 0)).ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-_breakfastOutletTotal.Discount - (discountMonring != null ? discountMonring.BillDiscountTotal : 0)).ToString("F");
                            ws.Cell("H" + index).Value = (-_breakfastOutletTotal.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = _breakfastOutletTotal.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = _breakfastOutletTotal.TotalCost.ToString("F");
                            if (_breakfastOutletTotal.TotalCost == 0 || _breakfastOutletTotal.ItemTotal == 0)
                                _breakfastOutletTotal.CP = 0;
                            else
                                _breakfastOutletTotal.CP = ((_breakfastOutletTotal.TotalCost / _breakfastOutletTotal.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = _breakfastOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        //Afternoon
                        if (viewmodel.Lunch)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _lunchOutletTotal.Qty;
                            ws.Cell("E" + index).Value = (_lunchOutletTotal.ItemTotal + (miscLunch != null ? miscLunch.MiscTotal : 0)).ToString("F");
                            ws.Cell("F" + index).Value = (_lunchOutletTotal.Percent + (miscLunch != null ? miscLunch.Percent : 0)).ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-_lunchOutletTotal.Discount - (discountLunch != null ? discountLunch.BillDiscountTotal : 0)).ToString("F");
                            ws.Cell("H" + index).Value = (-_lunchOutletTotal.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = _lunchOutletTotal.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = _lunchOutletTotal.TotalCost.ToString("F");
                            if (_lunchOutletTotal.TotalCost == 0 || _lunchOutletTotal.ItemTotal == 0)
                                _lunchOutletTotal.CP = 0;
                            else
                                _lunchOutletTotal.CP = ((_lunchOutletTotal.TotalCost / _lunchOutletTotal.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = _lunchOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        //dinner
                        if (viewmodel.Dinner)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _dinnerOutletTotal.Qty;
                            ws.Cell("E" + index).Value = (_dinnerOutletTotal.ItemTotal + (miscDinner != null ? miscDinner.MiscTotal : 0)).ToString("F");
                            ws.Cell("F" + index).Value = (_dinnerOutletTotal.Percent + (miscDinner != null ? miscDinner.Percent : 0)).ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-_dinnerOutletTotal.Discount - (discountDinner != null ? discountDinner.BillDiscountTotal : 0)).ToString("F");
                            ws.Cell("H" + index).Value = (-_dinnerOutletTotal.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = _dinnerOutletTotal.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = _dinnerOutletTotal.TotalCost.ToString("F");
                            if (_dinnerOutletTotal.TotalCost == 0 || _dinnerOutletTotal.ItemTotal == 0)
                                _dinnerOutletTotal.CP = 0;
                            else
                                _dinnerOutletTotal.CP = ((_dinnerOutletTotal.TotalCost / _dinnerOutletTotal.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = _dinnerOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        ws.Range("C" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        //format file
                        //header
                        ws.Range("A4:K6").Style.Font.SetBold(true);
                        //Set color
                        ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //set Border        
                        ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        if (_firstStore)
                        {
                            ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        index++;

                        // end total

                        // Summary
                        //double refund = 0;
                        ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                        ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                        if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                        {
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                        }
                        else
                        {
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                        }
                        ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                        ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                        ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        index++;

                        _noincludeSale = 0;
                        if (lstItemNoIncludeSaleInStore != null && lstItemNoIncludeSaleInStore.Any())
                        {
                            var taxInclude = lstItemNoIncludeSaleInStore.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false).Sum(ss => ss.Tax);
                            _noincludeSale = lstItemNoIncludeSaleInStore.Where(ww => ww.IsIncludeSale == false).Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                            _noincludeSale -= taxInclude;
                        }

                        //GC value
                        payGC = 0;
                        _taxOfPayGCNotInclude = 0;
                        _svcOfPayGCNotInclude = 0;
                        double _amount = 0;
                        if (lstPayments != null && lstPayments.Any())
                        {
                            var lstPaymentsInStore = lstPayments.Where(p => p.StoreId == itemGroupStore.Key.StoreId
                            && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();
                            List<PaymentModels> lstPaymentByPeriod = new List<PaymentModels>();

                            foreach (var item in lstPaymentsInStore)
                            {
                                _amount = 0;
                                var receipt = _lstDataDailys.Where(ww => ww.OrderId == item.OrderId).FirstOrDefault();
                                var lstGCRefunds = lstRefunds.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();

                                timeDish = item.CreatedDate.TimeOfDay;
                                // Breakfast
                                if (viewmodel.Breakfast)
                                {
                                    if (timeDish >= brearkStart && timeDish < brearkEnd)
                                    {
                                        if (item.IsInclude == null || (item.IsInclude.HasValue && !item.IsInclude.Value))
                                        {
                                            _amount = item.Amount;
                                            if (lstGCRefunds != null && lstGCRefunds.Any())
                                            {
                                                var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                                _amount -= refundAmount;
                                            }
                                            payGC += _amount;
                                            if (receipt != null)
                                            {
                                                double tax = 0;
                                                double svc = 0;
                                                if (receipt.GST != 0)
                                                    tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                                if (receipt.ServiceCharge != 0)
                                                    svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                                _taxOfPayGCNotInclude += tax;
                                                _svcOfPayGCNotInclude += svc;
                                            }
                                            lstPaymentByPeriod.Add(item);
                                        }
                                    }
                                }

                                // Lunch
                                if (viewmodel.Lunch)
                                {
                                    if (timeDish >= lunchStart && timeDish < lunchEnd)
                                    {
                                        if (item.IsInclude == null || (item.IsInclude.HasValue && !item.IsInclude.Value))
                                        {
                                            _amount = item.Amount;
                                            if (lstGCRefunds != null && lstGCRefunds.Any())
                                            {
                                                var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                                _amount -= refundAmount;
                                            }
                                            payGC += _amount;
                                            if (receipt != null)
                                            {
                                                double tax = 0;
                                                double svc = 0;
                                                if (receipt.GST != 0)
                                                    tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                                if (receipt.ServiceCharge != 0)
                                                    svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                                _taxOfPayGCNotInclude += tax;
                                                _svcOfPayGCNotInclude += svc;
                                            }
                                            lstPaymentByPeriod.Add(item);
                                        }
                                    }
                                }

                                // Dinner
                                if (viewmodel.Dinner)
                                {
                                    if (dinnerStart > dinnerEnd) //pass day
                                    {
                                        if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                        {

                                            if (item.IsInclude == null || (item.IsInclude.HasValue && !item.IsInclude.Value))
                                            {
                                                _amount = item.Amount;
                                                if (lstGCRefunds != null && lstGCRefunds.Any())
                                                {
                                                    var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                                    _amount -= refundAmount;
                                                }
                                                payGC += item.Amount;
                                                if (receipt != null)
                                                {
                                                    double tax = 0;
                                                    double svc = 0;
                                                    if (receipt.GST != 0)
                                                        tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                                    if (receipt.ServiceCharge != 0)
                                                        svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                                    _taxOfPayGCNotInclude += tax;
                                                    _svcOfPayGCNotInclude += svc;
                                                }
                                                lstPaymentByPeriod.Add(item);
                                            }
                                        }
                                    }
                                    else //in day
                                    {
                                        if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                        {
                                            if (item.IsInclude == null || (item.IsInclude.HasValue && !item.IsInclude.Value))
                                            {
                                                _amount = item.Amount;
                                                if (lstGCRefunds != null && lstGCRefunds.Any())
                                                {
                                                    var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                                    _amount -= refundAmount;
                                                }
                                                payGC += _amount;
                                                if (receipt != null)
                                                {
                                                    double tax = 0;
                                                    double svc = 0;
                                                    if (receipt.GST != 0)
                                                        tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                                    if (receipt.ServiceCharge != 0)
                                                        svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                                    _taxOfPayGCNotInclude += tax;
                                                    _svcOfPayGCNotInclude += svc;
                                                }
                                                lstPaymentByPeriod.Add(item);
                                            }
                                        }
                                    }
                                }
                            }

                            //payGC not include

                            //tax in payment
                            //payGC = lstPaymentByPeriod.Sum(p => (double)p.Amount);
                        }

                        //sell GC 22/09/2017
                        //sell GC 22/09/2017 (reduct GC with IsIncludeSale = 0)
                        //double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                        //        && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                        //        && ww.StoreId == itemGroupStore.Key.StoreId
                        //        ).Sum(ss => ss.TotalAmount.Value);

                        double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                              && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                              && ww.StoreId == itemGroupStore.Key.StoreId
                              && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                        //rounding
                        var roudingAmount = _lstDataDailys.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.Rounding);
                        var creditNoteIds = _lstDataDailys.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && !string.IsNullOrEmpty(ww.CreditNoteNo))
                            .Select(ss => ss.OrderId).ToList();
                        double totalCreditNote = 0;
                        if (creditNoteIds != null && creditNoteIds.Any())
                        {
                            totalCreditNote = lstData.Where(ww => creditNoteIds.Contains(ww.ReceiptId) && ww.StoreId == itemGroupStore.Key.StoreId && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                .Sum(ss => ss.ItemTotal);
                        }

                        var totalReceipt = _lstDataDailys.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo) && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.ReceiptTotal);
                        var totalSVC = _lstDataDailys.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo) && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.ServiceCharge);
                        var totalTax = _lstDataDailys.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo) && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.GST);

                        //netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (1)
                        //(1)= GC pay not include - tax of pay GC not include - SCV of pay GC not include 

                        ws.Cell("A" + index).Value = (totalReceipt - totalCreditNote - totalSVC - totalTax - giftCardSell - _noincludeSale -
                            (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude)).ToString("F");
                        //- _noincludeSale - payGC + giftCardSell + roudingAmount - (totalCreditNote)).ToString("F");

                        //Current
                        //ws.Cell("A" + index).Value = (_itemTotal.ItemTotal - subDisPeriod - (isTaxInclude ? _itemTotal.TaxTotal : 0)
                        //    - _noincludeSale - payGC + giftCardSell + roudingAmount - (totalCreditNote)).ToString("F");

                        ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                        ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                        ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                        ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                        ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        index += 2;
                        indexNextStore = index;
                        _firstStore = false;
                    }//end foreach (var itemGroupStore in lstItemGroupByStore)
                }
                //if (lstItemGroupByStore == null || lstItemGroupByStore.Count() == 0)
                else
                {
                    //check only sale GC
                    var lstItemGroupByStoreWithGC = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId))
                       .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                    if (lstItemGroupByStoreWithGC != null && lstItemGroupByStoreWithGC.Any())
                    {
                        //header
                        ws.Range("A4:K6").Style.Font.SetBold(true);
                        //Set color
                        ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //set Border        
                        ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                        foreach (var itemGroupStore in lstItemGroupByStoreWithGC)
                        {
                            if (_firstStore)
                            {
                                ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            }
                            else
                            {
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }
                            // Store name
                            var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                            ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                            if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                            }
                            else
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                            }
                            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            index++;

                            double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                 && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                 && ww.StoreId == itemGroupStore.Key.StoreId && (ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                            ws.Cell("A" + index).Value = (giftCardSell).ToString("F");
                            ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                            ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                            ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                            ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                            ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            index += 2;
                            indexNextStore = index;
                            _firstStore = false;
                        }
                    }

                }

            }

            ws.Columns(1, 3).AdjustToContents();
            //set Width for Colum 
            ws.Column(4).Width = 20;
            ws.Columns(5, 11).AdjustToContents();
            return wb;
        }

        //L update 2018-07-23
        public XLWorkbook ExportExcel_CreditNoteNew(ItemizedSalesAnalysisReportModel viewmodel
          , List<StoreModels> lstStores, List<string> lstGCId
          , List<RFilterCategoryV1Model> _lstCateChecked, List<RFilterCategoryModel> _lstSetChecked
          , List<RFilterCategoryV1ReportModel> lstTotalAllCate, List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu
            , List<string> lstStoreIdCate, List<string> lstStoreIdSet, List<string> lstCateIds = null, List<string> lstCateSetIds = null)
        {
            //get data
            List<ItemizedSalesAnalysisReportDataModels> lstData = GetData_New(viewmodel.FromDate, viewmodel.ToDate, viewmodel.ListStores, lstStores
                , lstStoreIdCate, lstStoreIdSet, lstCateIds, lstCateSetIds);
            string sheetName = "Itemized_Sales_Analysis_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 11, viewmodel.FromDateFilter, viewmodel.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized Sales Analysis Report"));

            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];

            // Get value from setting of common
            TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
            TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
            TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
            TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
            TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
            TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);
            var _lstDataDailys = new List<DailySalesReportInsertDataModels>();
            List<PaymentDataModels> lstPayments = new List<PaymentDataModels>();
            var lstRefunds = new List<RefundDataReportDTO>();
            //
            using (var db = new NuWebContext())
            {
                var request = new BaseReportDataModel() { ListStores = viewmodel.ListStores, FromDate = viewmodel.FromDate, ToDate = viewmodel.ToDate, Mode = viewmodel.Mode };
                //GetRoundingAmount
                _lstDataDailys = db.GetDataReceipt_WithCreditNote(request);
                lstPayments = db.GetDataPaymentItems(request);
                lstRefunds = db.GetListRefundWithoutDetailsByReceiptId(request);
            }
            if (lstPayments != null && lstPayments.Any())
            {
                lstPayments = lstPayments.Where(ww => lstGCId.Contains(ww.PaymentId) || ww.PaymentCode == (int)Commons.EPaymentCode.GiftCard).ToList();
            }
            //List<PaymentModels> lstPayments = _orderPaymentMethodFactory.GetDataPaymentItemsByGC(viewmodel, lstGCId);

            //Get rounding amount
            //var _lstDataDailys = GetRoundingAmount(viewmodel);


            // MISC DISCOUNT TOTAL BILL
            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
            List<DiscountAndMiscReportModels> listMisc_Discout = new List<DiscountAndMiscReportModels>();
            listMisc_Discout = miscFactory.GetReceiptDiscountAndMisc(viewmodel.FromDate, viewmodel.ToDate, viewmodel.ListStores, viewmodel.Mode, viewmodel.FromDateFilter, viewmodel.ToDateFilter, viewmodel.FilterType);
            listMisc_Discout.ForEach(ss => ss.DiscountValue = 0);


            DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
            var lstDiscount = discountDetailFactory.GetDiscountTotal(viewmodel.ListStores, viewmodel.FromDate, viewmodel.ToDate, viewmodel.Mode, viewmodel.FromDateFilter, viewmodel.ToDateFilter, viewmodel.FilterType);

            listMisc_Discout.AddRange(lstDiscount);

            //get list refund by GC
            //var lstRefunds = _refundFactory.GetListRefundWithoutDetail(viewmodel);

            //using (var db = new NuWebContext())
            //{

            //}
            // Filter date by time
            switch (viewmodel.FilterType)
            {
                case (int)Commons.EFilterType.OnDay:
                    if (lstPayments != null && lstPayments.Any())
                    {
                        lstPayments = lstPayments.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (_lstDataDailys != null && _lstDataDailys.Any())
                    {
                        _lstDataDailys = _lstDataDailys.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (lstRefunds != null && lstRefunds.Any())
                    {
                        lstRefunds = lstRefunds.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    break;
                case (int)Commons.EFilterType.Days:
                    if (lstPayments != null && lstPayments.Any())
                    {
                        lstPayments = lstPayments.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (_lstDataDailys != null && _lstDataDailys.Any())
                    {
                        _lstDataDailys = _lstDataDailys.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    if (lstRefunds != null && lstRefunds.Any())
                    {
                        lstRefunds = lstRefunds.Where(ww => ww.CreatedDate.TimeOfDay >= viewmodel.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= viewmodel.ToDateFilter.TimeOfDay).ToList();
                    }
                    break;
            }

            List<MISCBillDiscountPeriodModels> listMiscDisPeriod = new List<MISCBillDiscountPeriodModels>();
            if (listMisc_Discout != null)
            {
                var listMisc_DiscoutGroupStore = listMisc_Discout.GroupBy(gg => gg.StoreId).ToList();

                foreach (var item in listMisc_DiscoutGroupStore)
                {
                    // CHECK PERIOD IS CHECKED
                    if (viewmodel.Breakfast)
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
                    if (viewmodel.Lunch)
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
                    if (viewmodel.Dinner)
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

                }
            }

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
            ws.Cell("H5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
            ws.Cell("I5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
            ws.Cell("J5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
            ws.Cell("K5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

            //set width column
            ws.Column("A").Width = 20;
            ws.Column("B").Width = 30;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 10;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 15;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 15;

            int index = 6;
            bool isFirstStore = true;
            int indexNextStore = 0;
            //Total
            ItemizedSalesNewTotal _itemTotal = new ItemizedSalesNewTotal();
            var _breakfastTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerTotal = new ItemizedSalesPeriodValueTotal();

            var _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
            var lstBusinessId = new List<string>();
            bool _firstStore = true;
            if (lstData != null && lstData.Any())
            {
                // Group storeId
                TimeSpan timeDish = new TimeSpan();

                lstData.ForEach(x =>
                {
                    var store = lstStores.Where(z => z.Id.Equals(x.StoreId)).FirstOrDefault();
                    x.StoreName = store == null ? "" : store.Name;
                });

                List<RFilterCategoryV1ReportModel> lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                double subTotalQty = 0;
                double subTotalItem = 0;
                double subTotalPercent = 0;
                double subTotalDiscount = 0;
                double subTotalPromotion = 0;
                double subTotalUnitCost = 0;
                double subTotalTotalCost = 0;
                double subTotalCP = 0;

                double breakfastTotalQty = 0;
                double breakfastTotalItem = 0;
                double breakfastTotalPercent = 0;
                double breakfastTotalDiscount = 0;
                double breakfastTotalPromotion = 0;
                double breakfastTotalUnitCost = 0;
                double breakfastTotalTotalCost = 0;
                double breakfastTotalCP = 0;

                double lunchTotalQty = 0;
                double lunchTotalItem = 0;
                double lunchTotalPercent = 0;
                double lunchTotalDiscount = 0;
                double lunchTotalPromotion = 0;
                double lunchTotalUnitCost = 0;
                double lunchTotalTotalCost = 0;
                double lunchTotalCP = 0;

                double dinnerTotalQty = 0;
                double dinnerTotalItem = 0;
                double dinnerTotalPercent = 0;
                double dinnerTotalDiscount = 0;
                double dinnerTotalPromotion = 0;
                double dinnerTotalUnitCost = 0;
                double dinnerTotalTotalCost = 0;
                double dinnerTotalCP = 0;

                double _noincludeSale = 0;
                double payGC = 0;
                double _taxOfPayGCNotInclude = 0;
                double _svcOfPayGCNotInclude = 0;


                // Get list item for NetSales (IsIncludeSale == false)
                List<ItemizedSalesAnalysisReportDataModels> lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportDataModels>();

                List<MISCBillDiscountPeriodModels> listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();

                var lstItemGroupByStore = lstData.Where(ww => string.IsNullOrEmpty(ww.GiftCardId))
                        .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                if (lstItemGroupByStore != null && lstItemGroupByStore.Any())
                {
                    foreach (var itemGroupStore in lstItemGroupByStore)
                    {
                        lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                        // Get store setting : time of Period
                        if (currentUser != null)
                        {
                            // Get value from setting of mechant
                            Commons.BreakfastStart = currentUser.BreakfastStart;
                            Commons.BreakfastEnd = currentUser.BreakfastEnd;
                            Commons.LunchStart = currentUser.LunchStart;
                            Commons.LunchEnd = currentUser.LunchEnd;
                            Commons.DinnerStart = currentUser.DinnerStart;
                            Commons.DinnerEnd = currentUser.DinnerEnd;

                            // Get value from setting of store
                            if (currentUser.ListSetting != null && currentUser.ListSetting.Any())
                            {
                                var settingPeriodOfStore = currentUser.ListSetting.Where(w => w.StoreID == itemGroupStore.Key.StoreId).ToList();

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

                        // Get listMiscDisPeriod in store
                        var lstItemInStore = listMisc_Discout.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();

                        for (int i = 0; i < lstItemInStore.Count; i++)
                        {
                            lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportDataModels>();

                            // Get Total Misc to + ItemTotal
                            TimeSpan timeMisc = new TimeSpan(lstItemInStore[i].Hour, 0, 0);
                            // Total period Misc_Discout
                            // TOTAL PERIOD MISC AND DISCOUNT BILL TOTAL
                            if (viewmodel.Breakfast)
                            {
                                if (timeMisc >= brearkStart && timeMisc < brearkEnd)
                                {
                                    var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == itemGroupStore.Key.StoreId);
                                    period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                    period.MiscTotal += lstItemInStore[i].MiscValue;
                                    period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                }
                            }
                            if (viewmodel.Lunch)
                            {
                                if (timeMisc >= lunchStart && timeMisc < lunchEnd)
                                {
                                    var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == itemGroupStore.Key.StoreId);
                                    period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                    period.MiscTotal += lstItemInStore[i].MiscValue;
                                    period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                }
                            }
                            if (viewmodel.Dinner)
                            {
                                if (dinnerStart > dinnerEnd)//pass day
                                {
                                    if (timeMisc >= dinnerStart || timeMisc < dinnerEnd)
                                    {
                                        var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == itemGroupStore.Key.StoreId);
                                        period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                        period.MiscTotal += lstItemInStore[i].MiscValue;
                                        period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                    }
                                }
                                else//in day
                                {
                                    if (timeMisc >= dinnerStart && timeMisc < dinnerEnd)
                                    {
                                        var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == itemGroupStore.Key.StoreId);
                                        period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                        period.MiscTotal += lstItemInStore[i].MiscValue;
                                        period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                    }
                                }
                            }
                        }

                        var miscInstore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.MiscTotal);

                        double subDisPeriodByStore = 0;
                        double subDisPeriod = 0;
                        _itemTotal = new ItemizedSalesNewTotal();
                        _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
                        _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
                        _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
                        if (!isFirstStore)
                        {
                            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
                            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
                            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
                            ws.Cell("F" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
                            ws.Cell("G" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                            ws.Cell("H" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
                            ws.Cell("I" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
                            ws.Cell("J" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
                            ws.Cell("K" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

                            //header
                            ws.Range("A" + index + ":K" + index).Style.Font.SetBold(true);
                            //Set color
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + index + ":K" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Row(index).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            index++;
                        }
                        isFirstStore = false;
                        // Store name
                        var currentStore = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).FirstOrDefault();
                        var storeName = currentStore != null ? currentStore.Name + " in " + currentStore.CompanyName : "";
                        //var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                        ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        index++;

                        var lstItems = itemGroupStore.ToList();
                        // Group item type
                        var lstGroupItemType = lstItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                       || ww.ItemTypeId == (int)Commons.EProductType.SetMenu))
                           .OrderBy(x => x.ItemTypeId).GroupBy(gg => gg.ItemTypeId).ToList();

                        var itemAmountTotal = lstItems.Sum(ss => ss.ItemTotal);
                        foreach (var itemTypeId in lstGroupItemType)
                        {
                            var lstItemOfType = lstItems.Where(x => x.ItemTypeId == itemTypeId.Key).ToList();

                            List<RFilterCategoryModel> lstCateCheckedInStore = new List<RFilterCategoryModel>();

                            if (itemTypeId.Key == (int)Commons.EProductType.Dish)
                            {
                                lstCateCheckedInStore = _lstCateChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId)
                                    .Select(s => new RFilterCategoryModel
                                    {
                                        Id = s.Id,
                                        Name = s.Name
                                    }).ToList();

                                lstTotalDataInStore = lstTotalAllCate.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                            }
                            else //set
                            {
                                //check data old
                                var tmp = lstItemOfType.Where(ww => ww.CategoryId == "SetMenu").FirstOrDefault();
                                //check setmenu have category or not category
                                var categoryExist = _lstSetChecked.Where(ww => string.IsNullOrEmpty(ww.CategoryID)).FirstOrDefault();
                                if (categoryExist != null || tmp != null)//
                                {
                                    lstCateCheckedInStore = new List<RFilterCategoryModel>() {
                                    new RFilterCategoryModel(){Id="SetMenu", Name ="SetMenu"}
                                };
                                }
                                else
                                {
                                    var lstTmp = _lstSetChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId)
                                      .Select(s => new
                                      {
                                          Id = s.CategoryID,
                                          Name = s.CategoryName
                                      }).Distinct().ToList();

                                    lstCateCheckedInStore = lstTmp
                                        .Select(s => new RFilterCategoryModel
                                        {
                                            Id = s.Id,
                                            Name = s.Name
                                        }).ToList();
                                }
                                lstTotalDataInStore = lstTotalAllSetMenu.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                                lstTotalDataInStore.Add(new RFilterCategoryV1ReportModel() { CateId = "SetMenu", CateName = "SetMenu" });
                                //lstCateCheckedInStore = _lstSetChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId).Select(s => new RFilterCategoryModel
                                //{
                                //    Id = s.Id,
                                //    Name = s.Name
                                //}).ToList();

                                //lstTotalDataInStore = lstTotalAllSetMenu.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                            }

                            // Group by category
                            foreach (var itemCate in lstCateCheckedInStore)
                            {
                                subTotalQty = 0;
                                subTotalItem = 0;
                                subTotalPercent = 0;
                                subTotalDiscount = 0;
                                subTotalPromotion = 0;
                                subTotalUnitCost = 0;
                                subTotalTotalCost = 0;
                                subTotalCP = 0;

                                breakfastTotalQty = 0;
                                breakfastTotalItem = 0;
                                breakfastTotalPercent = 0;
                                breakfastTotalDiscount = 0;
                                breakfastTotalPromotion = 0;
                                breakfastTotalUnitCost = 0;
                                breakfastTotalTotalCost = 0;
                                breakfastTotalCP = 0;

                                lunchTotalQty = 0;
                                lunchTotalItem = 0;
                                lunchTotalPercent = 0;
                                lunchTotalDiscount = 0;
                                lunchTotalPromotion = 0;
                                lunchTotalUnitCost = 0;
                                lunchTotalTotalCost = 0;
                                lunchTotalCP = 0;

                                dinnerTotalQty = 0;
                                dinnerTotalItem = 0;
                                dinnerTotalPercent = 0;
                                dinnerTotalDiscount = 0;
                                dinnerTotalPromotion = 0;
                                dinnerTotalUnitCost = 0;
                                dinnerTotalTotalCost = 0;
                                dinnerTotalCP = 0;

                                ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Name));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                index++;

                                var lstCateItems = lstItemOfType.Where(w => w.CategoryId == itemCate.Id).ToList();
                                var lstItemTypes = lstCateItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                                                        || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).ToList();

                                List<ItemizedSalesAnalysisReportDataModels> lstDataByPeriod = new List<ItemizedSalesAnalysisReportDataModels>();

                                var breakfast = new ItemizedSalesPeriodValueTotal();
                                var lunch = new ItemizedSalesPeriodValueTotal();
                                var dinner = new ItemizedSalesPeriodValueTotal();
                                foreach (var item in lstItemTypes)
                                {
                                    timeDish = item.CreatedDate.TimeOfDay;
                                    if ((itemAmountTotal + miscInstore) != 0)
                                    {
                                        item.Percent = (item.ItemTotal / (itemAmountTotal + miscInstore)) * 100;
                                    }
                                    // check percent data
                                    // Breakfast
                                    if (viewmodel.Breakfast)
                                    {
                                        if (timeDish >= brearkStart && timeDish < brearkEnd)
                                        {
                                            breakfast.Qty += item.Quantity;
                                            breakfast.ItemTotal += item.ItemTotal;
                                            breakfast.Percent += item.Percent;
                                            breakfast.Discount += item.Discount;
                                            breakfast.Promotion += item.PromotionAmount;
                                            breakfast.UnitCost += item.Cost;
                                            breakfast.TotalCost += item.TotalCost;

                                            _breakfastOutletTotal.Qty += item.Quantity;
                                            _breakfastOutletTotal.ItemTotal += item.ItemTotal;
                                            _breakfastOutletTotal.Percent += item.Percent;
                                            _breakfastOutletTotal.Discount += item.Discount;
                                            _breakfastOutletTotal.Promotion += item.PromotionAmount;
                                            _breakfastOutletTotal.UnitCost += item.Cost;
                                            _breakfastOutletTotal.TotalCost += item.TotalCost;

                                            _breakfastTotal.Qty += item.Quantity;
                                            _breakfastTotal.ItemTotal += item.ItemTotal;
                                            _breakfastTotal.Percent += item.Percent;
                                            _breakfastTotal.Discount += item.Discount;
                                            _breakfastTotal.Promotion += item.PromotionAmount;
                                            _breakfastTotal.UnitCost += item.Cost;
                                            _breakfastTotal.TotalCost += item.TotalCost;

                                            // If CreatedDate is avaible with time of period
                                            lstDataByPeriod.Add(item);
                                        }
                                    }

                                    // lunch
                                    if (viewmodel.Lunch)
                                    {
                                        if (timeDish >= lunchStart && timeDish < lunchEnd)
                                        {
                                            lunch.Qty += item.Quantity;
                                            lunch.ItemTotal += item.ItemTotal;
                                            lunch.Percent += item.Percent;
                                            lunch.Discount += item.Discount;
                                            lunch.Promotion += item.PromotionAmount;
                                            lunch.UnitCost += item.Cost;
                                            lunch.TotalCost += item.TotalCost;

                                            _lunchOutletTotal.Qty += item.Quantity;
                                            _lunchOutletTotal.ItemTotal += item.ItemTotal;
                                            _lunchOutletTotal.Percent += item.Percent;
                                            _lunchOutletTotal.Discount += item.Discount;
                                            _lunchOutletTotal.Promotion += item.PromotionAmount;
                                            _lunchOutletTotal.UnitCost += item.Cost;
                                            _lunchOutletTotal.TotalCost += item.TotalCost;

                                            _lunchTotal.Qty += item.Quantity;
                                            _lunchTotal.ItemTotal += item.ItemTotal;
                                            _lunchTotal.Percent += item.Percent;
                                            _lunchTotal.Discount += item.Discount;
                                            _lunchTotal.Promotion += item.PromotionAmount;
                                            _lunchTotal.UnitCost += item.Cost;
                                            _lunchTotal.TotalCost += item.TotalCost;

                                            // If CreatedDate is avaible with time of period
                                            lstDataByPeriod.Add(item);
                                        }
                                    }

                                    // Dinner
                                    if (viewmodel.Dinner)
                                    {
                                        if (dinnerStart > dinnerEnd) //pass day
                                        {
                                            if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                            {
                                                dinner.Qty += item.Quantity;
                                                dinner.ItemTotal += item.ItemTotal;
                                                dinner.Percent += item.Percent;
                                                dinner.Discount += item.Discount;
                                                dinner.Promotion += item.PromotionAmount;
                                                dinner.UnitCost += item.Cost;
                                                dinner.TotalCost += item.TotalCost;

                                                _dinnerOutletTotal.Qty += item.Quantity;
                                                _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                                _dinnerOutletTotal.Percent += item.Percent;
                                                _dinnerOutletTotal.Discount += item.Discount;
                                                _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                                _dinnerOutletTotal.UnitCost += item.Cost;
                                                _dinnerOutletTotal.TotalCost += item.TotalCost;

                                                _dinnerTotal.Qty += item.Quantity;
                                                _dinnerTotal.ItemTotal += item.ItemTotal;
                                                _dinnerTotal.Percent += item.Percent;
                                                _dinnerTotal.Discount += item.Discount;
                                                _dinnerTotal.Promotion += item.PromotionAmount;
                                                _dinnerTotal.UnitCost += item.Cost;
                                                _dinnerTotal.TotalCost += item.TotalCost;

                                                // If CreatedDate is avaible with time of period
                                                lstDataByPeriod.Add(item);
                                            }
                                        }
                                        else //in day
                                        {
                                            if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                            {
                                                dinner.Qty += item.Quantity;
                                                dinner.ItemTotal += item.ItemTotal;
                                                dinner.Percent += item.Percent;
                                                dinner.Discount += item.Discount;
                                                dinner.Promotion += item.PromotionAmount;
                                                dinner.UnitCost += item.Cost;
                                                dinner.TotalCost += item.TotalCost;

                                                _dinnerOutletTotal.Qty += item.Quantity;
                                                _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                                _dinnerOutletTotal.Percent += item.Percent;
                                                _dinnerOutletTotal.Discount += item.Discount;
                                                _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                                _dinnerOutletTotal.UnitCost += item.Cost;
                                                _dinnerOutletTotal.TotalCost += item.TotalCost;

                                                _dinnerTotal.Qty += item.Quantity;
                                                _dinnerTotal.ItemTotal += item.ItemTotal;
                                                _dinnerTotal.Percent += item.Percent;
                                                _dinnerTotal.Discount += item.Discount;
                                                _dinnerTotal.Promotion += item.PromotionAmount;
                                                _dinnerTotal.UnitCost += item.Cost;
                                                _dinnerTotal.TotalCost += item.TotalCost;

                                                // If CreatedDate is avaible with time of period
                                                lstDataByPeriod.Add(item);
                                            }
                                        }
                                    }

                                }//End lstItemTypes

                                // Get total item
                                _itemTotal.SCTotal += lstDataByPeriod.Sum(s => s.ServiceCharge);
                                _itemTotal.ItemTotal += lstDataByPeriod.Sum(s => s.ItemTotal);
                                _itemTotal.TaxTotal += lstDataByPeriod.Sum(s => s.Tax);
                                _itemTotal.DiscountTotal += lstDataByPeriod.Sum(s => s.Discount);
                                _itemTotal.PromotionTotal += lstDataByPeriod.Sum(s => s.PromotionAmount);

                                // Add items to lst item for NetSales
                                lstItemNoIncludeSaleInStore.AddRange(lstDataByPeriod.Where(w => w.IsIncludeSale == false).ToList());

                                //Group Item
                                var lstItemTypeGroup = lstDataByPeriod.GroupBy(gg => new { gg.ItemTypeId, gg.ItemId, gg.Price }).ToList();
                                double cp = 0; double cost = 0;
                                foreach (var item in lstItemTypeGroup)
                                {
                                    ws.Cell("A" + index).Value = item.Select(ss => ss.ItemCode).FirstOrDefault();
                                    ws.Cell("B" + index).Value = item.Select(ss => ss.ItemName).FirstOrDefault();
                                    ws.Cell("C" + index).Value = item.Select(ss => ss.Price).FirstOrDefault().ToString("F");
                                    ws.Cell("C" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Cell("D" + index).Value = item.Sum(d => d.Quantity);
                                    ws.Cell("E" + index).Value = item.Sum(d => d.ItemTotal).ToString("F");
                                    ws.Cell("F" + index).Value = item.Sum(d => d.Percent).ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-item.Sum(d => d.Discount)).ToString("F");

                                    ws.Cell("H" + index).Value = (-item.Sum(d => d.PromotionAmount)).ToString("F");
                                    ws.Cell("I" + index).Value = item.Select(ss => ss.Cost).FirstOrDefault().ToString("F");
                                    cost += item.Select(ss => ss.Cost).FirstOrDefault();
                                    ws.Cell("J" + index).Value = item.Sum(d => d.TotalCost).ToString("F");

                                    if (item.Sum(d => d.TotalCost) == 0 || item.Sum(d => d.ItemTotal) == 0)
                                        cp = 0;
                                    else
                                        cp = ((item.Sum(d => d.TotalCost) / item.Sum(d => d.ItemTotal)) * 100);

                                    ws.Cell("K" + index).Value = cp.ToString("F") + " %";
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;
                                }//end lstItemTypeGroup

                                subTotalQty = breakfast.Qty + lunch.Qty + dinner.Qty;
                                subTotalItem = breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal;
                                subTotalPercent = breakfast.Percent + lunch.Percent + dinner.Percent;
                                subTotalDiscount = breakfast.Discount + lunch.Discount + dinner.Discount;
                                subTotalPromotion = breakfast.Promotion + lunch.Promotion + dinner.Promotion;
                                subTotalUnitCost = cost;
                                subTotalTotalCost = breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost;
                                if ((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) != 0 && (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal) != 0)
                                {
                                    subTotalCP = (((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) / (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal)) * 100);
                                }

                                breakfastTotalQty = breakfast.Qty;
                                breakfastTotalItem = breakfast.ItemTotal;
                                breakfastTotalPercent = breakfast.Percent;
                                breakfastTotalDiscount = breakfast.Discount;
                                breakfastTotalPromotion = breakfast.Promotion;
                                breakfastTotalUnitCost = breakfast.UnitCost;
                                breakfastTotalTotalCost = breakfast.TotalCost;
                                if (breakfast.TotalCost != 0 && breakfast.ItemTotal != 0)
                                {
                                    breakfastTotalCP = ((breakfast.TotalCost / breakfast.ItemTotal) * 100);
                                }

                                lunchTotalQty = lunch.Qty;
                                lunchTotalItem = lunch.ItemTotal;
                                lunchTotalPercent = lunch.Percent;
                                lunchTotalDiscount = lunch.Discount;
                                lunchTotalPromotion = lunch.Promotion;
                                lunchTotalUnitCost = lunch.UnitCost;
                                lunchTotalTotalCost = lunch.TotalCost;
                                lunchTotalCP = 0;
                                if (lunch.TotalCost != 0 && lunch.ItemTotal != 0)
                                {
                                    lunchTotalCP = ((lunch.TotalCost / lunch.ItemTotal) * 100);
                                }

                                dinnerTotalQty = dinner.Qty;
                                dinnerTotalItem = dinner.ItemTotal;
                                dinnerTotalPercent = dinner.Percent;
                                dinnerTotalDiscount = dinner.Discount;
                                dinnerTotalPromotion = dinner.Promotion;
                                dinnerTotalUnitCost = dinner.UnitCost;
                                dinnerTotalTotalCost = dinner.TotalCost;
                                if (dinner.TotalCost != 0 && dinner.ItemTotal != 0)
                                {
                                    dinnerTotalCP = ((dinner.TotalCost / dinner.ItemTotal) * 100);
                                }

                                // Get total information of current cate
                                var totalDataCurrentCate = lstTotalDataInStore.Where(w => w.CateId == itemCate.Id).FirstOrDefault();
                                // Update total data of current cate
                                totalDataCurrentCate.SubTotalQty += subTotalQty;
                                totalDataCurrentCate.SubTotalItem += subTotalItem;
                                totalDataCurrentCate.SubTotalPercent += subTotalPercent;
                                totalDataCurrentCate.SubTotalDiscount += subTotalDiscount;
                                totalDataCurrentCate.SubTotalPromotion += subTotalPromotion;
                                totalDataCurrentCate.SubTotalUnitCost += subTotalUnitCost;
                                totalDataCurrentCate.SubTotalTotalCost += subTotalTotalCost;
                                totalDataCurrentCate.SubTotalCP += subTotalCP;

                                totalDataCurrentCate.BreakfastTotalQty += breakfastTotalQty;
                                totalDataCurrentCate.BreakfastTotalItem += breakfastTotalItem;
                                totalDataCurrentCate.BreakfastTotalPercent += breakfastTotalPercent;
                                totalDataCurrentCate.BreakfastTotalDiscount += breakfastTotalDiscount;
                                totalDataCurrentCate.BreakfastTotalPromotion += breakfastTotalPromotion;
                                totalDataCurrentCate.BreakfastTotalUnitCost += breakfastTotalUnitCost;
                                totalDataCurrentCate.BreakfastTotalTotalCost += breakfastTotalTotalCost;
                                totalDataCurrentCate.BreakfastTotalCP += breakfastTotalCP;

                                totalDataCurrentCate.LunchTotalQty += lunchTotalQty;
                                totalDataCurrentCate.LunchTotalItem += lunchTotalItem;
                                totalDataCurrentCate.LunchTotalPercent += lunchTotalPercent;
                                totalDataCurrentCate.LunchTotalDiscount += lunchTotalDiscount;
                                totalDataCurrentCate.LunchTotalPromotion += lunchTotalPromotion;
                                totalDataCurrentCate.LunchTotalUnitCost += lunchTotalUnitCost;
                                totalDataCurrentCate.LunchTotalTotalCost += lunchTotalTotalCost;
                                totalDataCurrentCate.LunchTotalCP += lunchTotalCP;

                                totalDataCurrentCate.DinnerTotalQty += dinnerTotalQty;
                                totalDataCurrentCate.DinnerTotalItem += dinnerTotalItem;
                                totalDataCurrentCate.DinnerTotalPercent += dinnerTotalPercent;
                                totalDataCurrentCate.DinnerTotalDiscount += dinnerTotalDiscount;
                                totalDataCurrentCate.DinnerTotalPromotion += dinnerTotalPromotion;
                                totalDataCurrentCate.DinnerTotalUnitCost += dinnerTotalUnitCost;
                                totalDataCurrentCate.DinnerTotalTotalCost += dinnerTotalTotalCost;
                                totalDataCurrentCate.DinnerTotalCP += dinnerTotalCP;

                                // Show total date of current
                                // If it has any cate child which was checked  => title: Sub-Total: CateName
                                // If it doesn't have any cate child which was checked && it doesn't has parent cate which was checked => it's top cate => title: Category Total: CateName
                                if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && !lstTotalDataInStore.Where(w => !string.IsNullOrEmpty(totalDataCurrentCate.ParentId) && w.CateId == totalDataCurrentCate.ParentId && w.Checked).Any())
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Total") + " : " + totalDataCurrentCate.CateName);
                                }
                                else
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total") + " : " + totalDataCurrentCate.CateName);
                                }

                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                                ws.Cell("D" + index).Value = totalDataCurrentCate.SubTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.SubTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.SubTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.SubTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.SubTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.SubTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.SubTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.SubTotalCP.ToString("F") + " %";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;

                                //Morning
                                if (viewmodel.Breakfast)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.BreakfastTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.BreakfastTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.BreakfastTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.BreakfastTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.BreakfastTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.BreakfastTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.BreakfastTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.BreakfastTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                //Afternoon
                                if (viewmodel.Lunch)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.LunchTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.LunchTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.LunchTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.LunchTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.LunchTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.LunchTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.LunchTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.LunchTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                //dinner
                                if (viewmodel.Dinner)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.DinnerTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.DinnerTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.DinnerTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.DinnerTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.DinnerTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.DinnerTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.DinnerTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.DinnerTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                // Check current cate is last chile of parent current cate
                                Boolean isLastChildCate = false;
                                // If current cate has parent cate, check for show total data of parent cate
                                if (!string.IsNullOrEmpty(totalDataCurrentCate.ParentId))
                                {
                                    var parentCurrentCateInfo = lstTotalDataInStore.Where(w => w.CateId == totalDataCurrentCate.ParentId).FirstOrDefault();
                                    // List cate child
                                    var lstCateChildCheckedOfParent = parentCurrentCateInfo.ListCateChildChecked.ToList();
                                    if (lstCateChildCheckedOfParent != null && lstCateChildCheckedOfParent.Any())
                                    {
                                        // Get id of last child cate
                                        string idLastCateChild = parentCurrentCateInfo.ListCateChildChecked.LastOrDefault().ToString();

                                        // Current cate is last cate of parent cate
                                        if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && idLastCateChild == totalDataCurrentCate.CateId)
                                        {
                                            isLastChildCate = true;
                                        }
                                    }

                                    DisplayTotalOfParentCate(ref ws, totalDataCurrentCate, ref index, ref lstTotalDataInStore, isLastChildCate, totalDataCurrentCate, viewmodel);
                                }
                            }//End Group by category
                        }//end lstGroupItemType

                        // MISC
                        int startM = index;
                        listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();

                        if (listMiscDisPeriod != null && listMiscDisPeriod.Any())
                        {
                            listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                            if (listMiscDisPeriodByStore != null && listMiscDisPeriodByStore.Any())
                            {
                                // MISC
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                                index++;
                                for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                    ws.Cell("E" + index).SetValue(listMiscDisPeriodByStore[m].MiscTotal.ToString("F"));
                                    listMiscDisPeriodByStore[m].Percent = ((listMiscDisPeriodByStore[m].MiscTotal / (itemAmountTotal + miscInstore)) * 100);
                                    ws.Cell("F" + index).SetValue(listMiscDisPeriodByStore[m].Percent.ToString("F") + " %");
                                    index++;

                                    _itemTotal.ItemTotal += listMiscDisPeriodByStore[m].MiscTotal;
                                }

                                // Discount Total Bill
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                                index++;
                                for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                    ws.Cell("G" + index).SetValue((-listMiscDisPeriodByStore[m].BillDiscountTotal).ToString("F"));
                                    subDisPeriod += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                    subDisPeriodByStore += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                    _itemTotal.DiscountTotal += listMiscDisPeriodByStore[m].BillDiscountTotal;

                                    index++;

                                }
                            }
                        }

                        ws.Range("A" + startM + ":K" + index).Style.Font.SetBold(true);
                        ws.Range("A" + startM + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + startM + ":D" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Range("E" + startM + ":G" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Outlet Sub-total
                        //_subletTotal
                        double cpOutlet = 0;
                        var miscPercent = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                        var miscBreafast = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.BREAKFAST).FirstOrDefault();
                        var miscLunch = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.LUNCH).FirstOrDefault();
                        var miscDinner = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.DINNER).FirstOrDefault();

                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub-total"));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                        ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                    + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                        ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                            + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                        ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                        ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                        ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                        if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                            || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                            cpOutlet = 0;
                        else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                                / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                        ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                        ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                        ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;

                        // TOTAL
                        if (listMiscDisPeriodByStore == null)
                            listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();
                        index = index - 1;
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                        ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                        ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                    + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                        ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                            + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                        ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                        ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                        ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                        if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                        || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                            cpOutlet = 0;
                        else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                                / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                        ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                        ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                        ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;

                        var discountMonring = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.BREAKFAST && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                        var discountLunch = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.LUNCH && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                        var discountDinner = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.DINNER && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();

                        //Morning
                        if (viewmodel.Breakfast)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _breakfastOutletTotal.Qty;
                            ws.Cell("E" + index).Value = (_breakfastOutletTotal.ItemTotal + (miscBreafast != null ? miscBreafast.MiscTotal : 0)).ToString("F");
                            ws.Cell("F" + index).Value = (_breakfastOutletTotal.Percent + (miscBreafast != null ? miscBreafast.Percent : 0)).ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-_breakfastOutletTotal.Discount - (discountMonring != null ? discountMonring.BillDiscountTotal : 0)).ToString("F");
                            ws.Cell("H" + index).Value = (-_breakfastOutletTotal.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = _breakfastOutletTotal.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = _breakfastOutletTotal.TotalCost.ToString("F");
                            if (_breakfastOutletTotal.TotalCost == 0 || _breakfastOutletTotal.ItemTotal == 0)
                                _breakfastOutletTotal.CP = 0;
                            else
                                _breakfastOutletTotal.CP = ((_breakfastOutletTotal.TotalCost / _breakfastOutletTotal.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = _breakfastOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        //Afternoon
                        if (viewmodel.Lunch)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _lunchOutletTotal.Qty;
                            ws.Cell("E" + index).Value = (_lunchOutletTotal.ItemTotal + (miscLunch != null ? miscLunch.MiscTotal : 0)).ToString("F");
                            ws.Cell("F" + index).Value = (_lunchOutletTotal.Percent + (miscLunch != null ? miscLunch.Percent : 0)).ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-_lunchOutletTotal.Discount - (discountLunch != null ? discountLunch.BillDiscountTotal : 0)).ToString("F");
                            ws.Cell("H" + index).Value = (-_lunchOutletTotal.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = _lunchOutletTotal.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = _lunchOutletTotal.TotalCost.ToString("F");
                            if (_lunchOutletTotal.TotalCost == 0 || _lunchOutletTotal.ItemTotal == 0)
                                _lunchOutletTotal.CP = 0;
                            else
                                _lunchOutletTotal.CP = ((_lunchOutletTotal.TotalCost / _lunchOutletTotal.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = _lunchOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        //dinner
                        if (viewmodel.Dinner)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _dinnerOutletTotal.Qty;
                            ws.Cell("E" + index).Value = (_dinnerOutletTotal.ItemTotal + (miscDinner != null ? miscDinner.MiscTotal : 0)).ToString("F");
                            ws.Cell("F" + index).Value = (_dinnerOutletTotal.Percent + (miscDinner != null ? miscDinner.Percent : 0)).ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-_dinnerOutletTotal.Discount - (discountDinner != null ? discountDinner.BillDiscountTotal : 0)).ToString("F");
                            ws.Cell("H" + index).Value = (-_dinnerOutletTotal.Promotion).ToString("F");
                            ws.Cell("I" + index).Value = _dinnerOutletTotal.UnitCost.ToString("F");
                            ws.Cell("J" + index).Value = _dinnerOutletTotal.TotalCost.ToString("F");
                            if (_dinnerOutletTotal.TotalCost == 0 || _dinnerOutletTotal.ItemTotal == 0)
                                _dinnerOutletTotal.CP = 0;
                            else
                                _dinnerOutletTotal.CP = ((_dinnerOutletTotal.TotalCost / _dinnerOutletTotal.ItemTotal) * 100);
                            ws.Cell("K" + index).Value = _dinnerOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        ws.Range("C" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        //format file
                        //header
                        ws.Range("A4:K6").Style.Font.SetBold(true);
                        //Set color
                        ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //set Border        
                        ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        if (_firstStore)
                        {
                            ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        index++;

                        // end total

                        // Summary
                        //double refund = 0;
                        ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                        ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                        if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                        {
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                        }
                        else
                        {
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                        }
                        ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                        ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                        ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        index++;

                        _noincludeSale = 0;
                        if (lstItemNoIncludeSaleInStore != null && lstItemNoIncludeSaleInStore.Any())
                        {
                            var taxInclude = lstItemNoIncludeSaleInStore.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive && ww.IsIncludeSale == false).Sum(ss => ss.Tax);
                            _noincludeSale = lstItemNoIncludeSaleInStore.Where(ww => ww.IsIncludeSale == false).Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                            _noincludeSale -= taxInclude;
                        }

                        //GC value
                        payGC = 0;
                        _taxOfPayGCNotInclude = 0;
                        _svcOfPayGCNotInclude = 0;
                        double _amount = 0;
                        if (lstPayments != null && lstPayments.Any())
                        {
                            var lstPaymentsInStore = lstPayments.Where(p => p.StoreId == itemGroupStore.Key.StoreId
                            && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();
                            List<PaymentDataModels> lstPaymentByPeriod = new List<PaymentDataModels>();

                            foreach (var item in lstPaymentsInStore)
                            {
                                _amount = 0;
                                var receipt = _lstDataDailys.Where(ww => ww.OrderId == item.OrderId).FirstOrDefault();
                                var lstGCRefunds = lstRefunds.Where(ww => ww.OrderId == item.OrderId && ww.IsGiftCard).ToList();

                                timeDish = item.CreatedDate.TimeOfDay;
                                // Breakfast
                                if (viewmodel.Breakfast)
                                {
                                    if (timeDish >= brearkStart && timeDish < brearkEnd)
                                    {
                                        if (item.IsInclude == null || (item.IsInclude.HasValue && !item.IsInclude.Value))
                                        {
                                            _amount = item.Amount;
                                            if (lstGCRefunds != null && lstGCRefunds.Any())
                                            {
                                                var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                                _amount -= refundAmount;
                                            }
                                            payGC += _amount;
                                            if (receipt != null)
                                            {
                                                double tax = 0;
                                                double svc = 0;
                                                if (receipt.GST != 0)
                                                    tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                                if (receipt.ServiceCharge != 0)
                                                    svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                                _taxOfPayGCNotInclude += tax;
                                                _svcOfPayGCNotInclude += svc;
                                            }
                                            lstPaymentByPeriod.Add(item);
                                        }
                                    }
                                }

                                // Lunch
                                if (viewmodel.Lunch)
                                {
                                    if (timeDish >= lunchStart && timeDish < lunchEnd)
                                    {
                                        if (item.IsInclude == null || (item.IsInclude.HasValue && !item.IsInclude.Value))
                                        {
                                            _amount = item.Amount;
                                            if (lstGCRefunds != null && lstGCRefunds.Any())
                                            {
                                                var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                                _amount -= refundAmount;
                                            }
                                            payGC += _amount;
                                            if (receipt != null)
                                            {
                                                double tax = 0;
                                                double svc = 0;
                                                if (receipt.GST != 0)
                                                    tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                                if (receipt.ServiceCharge != 0)
                                                    svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                                _taxOfPayGCNotInclude += tax;
                                                _svcOfPayGCNotInclude += svc;
                                            }
                                            lstPaymentByPeriod.Add(item);
                                        }
                                    }
                                }

                                // Dinner
                                if (viewmodel.Dinner)
                                {
                                    if (dinnerStart > dinnerEnd) //pass day
                                    {
                                        if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                        {

                                            if (item.IsInclude == null || (item.IsInclude.HasValue && !item.IsInclude.Value))
                                            {
                                                _amount = item.Amount;
                                                if (lstGCRefunds != null && lstGCRefunds.Any())
                                                {
                                                    var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                                    _amount -= refundAmount;
                                                }
                                                payGC += item.Amount;
                                                if (receipt != null)
                                                {
                                                    double tax = 0;
                                                    double svc = 0;
                                                    if (receipt.GST != 0)
                                                        tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                                    if (receipt.ServiceCharge != 0)
                                                        svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                                    _taxOfPayGCNotInclude += tax;
                                                    _svcOfPayGCNotInclude += svc;
                                                }
                                                lstPaymentByPeriod.Add(item);
                                            }
                                        }
                                    }
                                    else //in day
                                    {
                                        if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                        {
                                            if (item.IsInclude == null || (item.IsInclude.HasValue && !item.IsInclude.Value))
                                            {
                                                _amount = item.Amount;
                                                if (lstGCRefunds != null && lstGCRefunds.Any())
                                                {
                                                    var refundAmount = lstGCRefunds.Sum(ss => ss.TotalRefund);
                                                    _amount -= refundAmount;
                                                }
                                                payGC += _amount;
                                                if (receipt != null)
                                                {
                                                    double tax = 0;
                                                    double svc = 0;
                                                    if (receipt.GST != 0)
                                                        tax = _amount * receipt.GST / (receipt.ReceiptTotal == 0 ? 1 : receipt.ReceiptTotal);
                                                    if (receipt.ServiceCharge != 0)
                                                        svc = (_amount - tax) * receipt.ServiceCharge / ((receipt.ReceiptTotal - receipt.GST) == 0 ? 1 : (receipt.ReceiptTotal - receipt.GST));

                                                    _taxOfPayGCNotInclude += tax;
                                                    _svcOfPayGCNotInclude += svc;
                                                }
                                                lstPaymentByPeriod.Add(item);
                                            }
                                        }
                                    }
                                }
                            }

                            //payGC not include

                            //tax in payment
                            //payGC = lstPaymentByPeriod.Sum(p => (double)p.Amount);
                        }

                        //sell GC 22/09/2017
                        //sell GC 22/09/2017 (reduct GC with IsIncludeSale = 0)
                        //double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                        //        && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                        //        && ww.StoreId == itemGroupStore.Key.StoreId
                        //        ).Sum(ss => ss.TotalAmount.Value);

                        double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                              && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                              && ww.StoreId == itemGroupStore.Key.StoreId
                              && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                        //rounding
                        var roudingAmount = _lstDataDailys.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.Rounding);
                        var creditNoteIds = _lstDataDailys.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && !string.IsNullOrEmpty(ww.CreditNoteNo))
                            .Select(ss => ss.OrderId).ToList();
                        double totalCreditNote = 0;
                        if (creditNoteIds != null && creditNoteIds.Any())
                        {
                            totalCreditNote = lstData.Where(ww => creditNoteIds.Contains(ww.ReceiptId) && ww.StoreId == itemGroupStore.Key.StoreId && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                                .Sum(ss => ss.ItemTotal);
                        }

                        var totalReceipt = _lstDataDailys.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo) && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.ReceiptTotal);
                        var totalSVC = _lstDataDailys.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo) && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.ServiceCharge);
                        var totalTax = _lstDataDailys.Where(ww => string.IsNullOrEmpty(ww.CreditNoteNo) && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.GST);

                        //netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (1)
                        //(1)= GC pay not include - tax of pay GC not include - SCV of pay GC not include 

                        ws.Cell("A" + index).Value = (totalReceipt - totalCreditNote - totalSVC - totalTax - giftCardSell - _noincludeSale -
                            (payGC - _taxOfPayGCNotInclude - _svcOfPayGCNotInclude)).ToString("F");
                        //- _noincludeSale - payGC + giftCardSell + roudingAmount - (totalCreditNote)).ToString("F");

                        //Current
                        //ws.Cell("A" + index).Value = (_itemTotal.ItemTotal - subDisPeriod - (isTaxInclude ? _itemTotal.TaxTotal : 0)
                        //    - _noincludeSale - payGC + giftCardSell + roudingAmount - (totalCreditNote)).ToString("F");

                        ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                        ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                        ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                        ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                        ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        index += 2;
                        indexNextStore = index;
                        _firstStore = false;
                    }//end foreach (var itemGroupStore in lstItemGroupByStore)
                }
                //if (lstItemGroupByStore == null || lstItemGroupByStore.Count() == 0)
                else
                {
                    //check only sale GC
                    var lstItemGroupByStoreWithGC = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId))
                       .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                    if (lstItemGroupByStoreWithGC != null && lstItemGroupByStoreWithGC.Any())
                    {
                        //header
                        ws.Range("A4:K6").Style.Font.SetBold(true);
                        //Set color
                        ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //set Border        
                        ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                        foreach (var itemGroupStore in lstItemGroupByStoreWithGC)
                        {
                            if (_firstStore)
                            {
                                ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            }
                            else
                            {
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }
                            // Store name
                            var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                            ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                            if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                            }
                            else
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                            }
                            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            index++;

                            double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                 && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                 && ww.StoreId == itemGroupStore.Key.StoreId && (ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                            ws.Cell("A" + index).Value = (giftCardSell).ToString("F");
                            ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                            ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                            ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                            ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                            ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            index += 2;
                            indexNextStore = index;
                            _firstStore = false;
                        }
                    }

                }

            }

            ws.Columns(1, 3).AdjustToContents();
            //set Width for Colum 
            ws.Column(4).Width = 20;
            ws.Columns(5, 11).AdjustToContents();
            return wb;
        }

        public XLWorkbook ExportExcel_V1(List<ItemizedSalesAnalysisReportModels> lstData, ItemizedSalesAnalysisReportModel viewmodel
      , List<StoreModels> lstStores, DateTime dToFilter, DateTime dFromFilter, List<string> lstGCId
      , List<RFilterCategoryV1Model> _lstCateChecked, List<RFilterCategoryModel> _lstSetChecked
      , List<RFilterCategoryV1ReportModel> lstTotalAllCate, List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu)
        {
            List<PaymentModels> lstPayments = _orderPaymentMethodFactory.GetDataPaymentItemsByGC(viewmodel, lstGCId);

            string sheetName = "Itemized_Sales_Analysis_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 11, dFromFilter, dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized Sales Analysis Report"));

            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];

            // Get value from setting of common
            TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
            TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
            TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
            TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
            TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
            TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);

            // MISC DISCOUNT TOTAL BILL
            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
            List<DiscountAndMiscReportModels> listMisc_Discout = new List<DiscountAndMiscReportModels>();
            listMisc_Discout = miscFactory.GetReceiptDiscountAndMisc(viewmodel.FromDate, viewmodel.ToDate, viewmodel.ListStores, viewmodel.Mode);
            listMisc_Discout.ForEach(ss => ss.DiscountValue = 0);

            //Get rounding amount
            var _lstRoundings = GetRoundingAmount(viewmodel);
            DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
            var lstDiscount = discountDetailFactory.GetDiscountTotal(viewmodel.ListStores, viewmodel.FromDate, viewmodel.ToDate);

            listMisc_Discout.AddRange(lstDiscount);

            //update 2018-04-06 get list credit note
            //var lstCreditNoteId = GetListCreditNoteId(viewmodel);

            List<MISCBillDiscountPeriodModels> listMiscDisPeriod = new List<MISCBillDiscountPeriodModels>();
            if (listMisc_Discout != null)
            {
                var listMisc_DiscoutGroupStore = listMisc_Discout.GroupBy(gg => gg.StoreId).ToList();

                foreach (var item in listMisc_DiscoutGroupStore)
                {
                    // CHECK PERIOD IS CHECKED
                    if (viewmodel.Breakfast)
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
                    if (viewmodel.Lunch)
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
                    if (viewmodel.Dinner)
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

                }
            }

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
            ws.Cell("H5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
            ws.Cell("I5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
            ws.Cell("J5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
            ws.Cell("K5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

            //set width column
            ws.Column("A").Width = 20;
            ws.Column("B").Width = 30;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 10;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 15;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 15;

            int index = 6;
            bool isFirstStore = true;
            int indexNextStore = 0;
            //Total
            ItemizedSalesNewTotal _itemTotal = new ItemizedSalesNewTotal();
            var _breakfastTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerTotal = new ItemizedSalesPeriodValueTotal();

            var _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
            var lstBusinessId = new List<string>();
            bool _firstStore = true;
            if (lstData != null && lstData.Any())
            {
                // Group storeId
                TimeSpan timeDish = new TimeSpan();

                lstData.ForEach(x =>
                {
                    var store = lstStores.Where(z => z.Id.Equals(x.StoreId)).FirstOrDefault();
                    x.StoreName = store == null ? "" : store.Name;
                });

                List<RFilterCategoryV1ReportModel> lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                double subTotalQty = 0;
                double subTotalItem = 0;
                double subTotalPercent = 0;
                double subTotalDiscount = 0;
                double subTotalPromotion = 0;
                double subTotalUnitCost = 0;
                double subTotalTotalCost = 0;
                double subTotalCP = 0;

                double breakfastTotalQty = 0;
                double breakfastTotalItem = 0;
                double breakfastTotalPercent = 0;
                double breakfastTotalDiscount = 0;
                double breakfastTotalPromotion = 0;
                double breakfastTotalUnitCost = 0;
                double breakfastTotalTotalCost = 0;
                double breakfastTotalCP = 0;

                double lunchTotalQty = 0;
                double lunchTotalItem = 0;
                double lunchTotalPercent = 0;
                double lunchTotalDiscount = 0;
                double lunchTotalPromotion = 0;
                double lunchTotalUnitCost = 0;
                double lunchTotalTotalCost = 0;
                double lunchTotalCP = 0;

                double dinnerTotalQty = 0;
                double dinnerTotalItem = 0;
                double dinnerTotalPercent = 0;
                double dinnerTotalDiscount = 0;
                double dinnerTotalPromotion = 0;
                double dinnerTotalUnitCost = 0;
                double dinnerTotalTotalCost = 0;
                double dinnerTotalCP = 0;

                double _noincludeSale = 0;
                double payGC = 0;

                // Get list item for NetSales (IsIncludeSale == false)
                List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportModels>();

                List<MISCBillDiscountPeriodModels> listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();

                var lstItemGroupByStore = lstData.Where(ww => string.IsNullOrEmpty(ww.GiftCardId))
                        .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                foreach (var itemGroupStore in lstItemGroupByStore)
                {
                    lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                    // Get store setting : time of Period
                    if (currentUser != null)
                    {
                        // Get value from setting of mechant
                        Commons.BreakfastStart = currentUser.BreakfastStart;
                        Commons.BreakfastEnd = currentUser.BreakfastEnd;
                        Commons.LunchStart = currentUser.LunchStart;
                        Commons.LunchEnd = currentUser.LunchEnd;
                        Commons.DinnerStart = currentUser.DinnerStart;
                        Commons.DinnerEnd = currentUser.DinnerEnd;

                        // Get value from setting of store
                        if (currentUser.ListSetting != null && currentUser.ListSetting.Any())
                        {
                            var settingPeriodOfStore = currentUser.ListSetting.Where(w => w.StoreID == itemGroupStore.Key.StoreId).ToList();

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

                    // Get listMiscDisPeriod in store
                    var lstItemInStore = listMisc_Discout.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();

                    for (int i = 0; i < lstItemInStore.Count; i++)
                    {
                        lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportModels>();

                        // Get Total Misc to + ItemTotal
                        TimeSpan timeMisc = new TimeSpan(lstItemInStore[i].Hour, 0, 0);
                        // Total period Misc_Discout
                        // TOTAL PERIOD MISC AND DISCOUNT BILL TOTAL
                        if (viewmodel.Breakfast)
                        {
                            if (timeMisc >= brearkStart && timeMisc < brearkEnd)
                            {
                                var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == itemGroupStore.Key.StoreId);
                                period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                period.MiscTotal += lstItemInStore[i].MiscValue;
                                period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                            }
                        }
                        if (viewmodel.Lunch)
                        {
                            if (timeMisc >= lunchStart && timeMisc < lunchEnd)
                            {
                                var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == itemGroupStore.Key.StoreId);
                                period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                period.MiscTotal += lstItemInStore[i].MiscValue;
                                period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                            }
                        }
                        if (viewmodel.Dinner)
                        {
                            if (dinnerStart > dinnerEnd)//pass day
                            {
                                if (timeMisc >= dinnerStart || timeMisc < dinnerEnd)
                                {
                                    var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == itemGroupStore.Key.StoreId);
                                    period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                    period.MiscTotal += lstItemInStore[i].MiscValue;
                                    period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                }
                            }
                            else//in day
                            {
                                if (timeMisc >= dinnerStart && timeMisc < dinnerEnd)
                                {
                                    var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == itemGroupStore.Key.StoreId);
                                    period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                    period.MiscTotal += lstItemInStore[i].MiscValue;
                                    period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                }
                            }
                        }
                    }

                    var miscInstore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.MiscTotal);

                    double subDisPeriodByStore = 0;
                    double subDisPeriod = 0;
                    _itemTotal = new ItemizedSalesNewTotal();
                    _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
                    _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
                    _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
                    if (!isFirstStore)
                    {
                        ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
                        ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
                        ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
                        ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                        ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
                        ws.Cell("F" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
                        ws.Cell("G" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                        ws.Cell("H" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
                        ws.Cell("I" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
                        ws.Cell("J" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
                        ws.Cell("K" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

                        //header
                        ws.Range("A" + index + ":K" + index).Style.Font.SetBold(true);
                        //Set color
                        ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A" + index + ":K" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(index).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        index++;
                    }
                    isFirstStore = false;
                    // Store name
                    var currentStore = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).FirstOrDefault();
                    var storeName = currentStore != null ? currentStore.Name + " in " + currentStore.CompanyName : "";
                    //var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                    ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    index++;

                    var lstItems = itemGroupStore.ToList();
                    // Group item type
                    var lstGroupItemType = lstItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                   || ww.ItemTypeId == (int)Commons.EProductType.SetMenu))
                       .OrderBy(x => x.ItemTypeId).GroupBy(gg => gg.ItemTypeId).ToList();

                    var itemAmountTotal = lstItems.Sum(ss => ss.ItemTotal);
                    foreach (var itemTypeId in lstGroupItemType)
                    {
                        var lstItemOfType = lstItems.Where(x => x.ItemTypeId == itemTypeId.Key).ToList();

                        List<RFilterCategoryModel> lstCateCheckedInStore = new List<RFilterCategoryModel>();

                        if (itemTypeId.Key == (int)Commons.EProductType.Dish)
                        {
                            lstCateCheckedInStore = _lstCateChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId)
                                .Select(s => new RFilterCategoryModel
                                {
                                    Id = s.Id,
                                    Name = s.Name
                                }).ToList();

                            lstTotalDataInStore = lstTotalAllCate.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                        }
                        else //set
                        {
                            //check data old
                            var tmp = lstItemOfType.Where(ww => ww.CategoryId == "SetMenu").FirstOrDefault();
                            //check setmenu have category or not category
                            var categoryExist = _lstSetChecked.Where(ww => string.IsNullOrEmpty(ww.CategoryID)).FirstOrDefault();
                            if (categoryExist != null || tmp != null)//
                            {
                                lstCateCheckedInStore = new List<RFilterCategoryModel>() {
                                    new RFilterCategoryModel(){Id="SetMenu", Name ="SetMenu"}
                                };
                            }
                            else
                            {
                                var lstTmp = _lstSetChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId)
                                  .Select(s => new
                                  {
                                      Id = s.CategoryID,
                                      Name = s.CategoryName
                                  }).Distinct().ToList();

                                lstCateCheckedInStore = lstTmp
                                    .Select(s => new RFilterCategoryModel
                                    {
                                        Id = s.Id,
                                        Name = s.Name
                                    }).ToList();
                            }
                            lstTotalDataInStore = lstTotalAllSetMenu.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                            lstTotalDataInStore.Add(new RFilterCategoryV1ReportModel() { CateId = "SetMenu", CateName = "SetMenu" });
                            //lstCateCheckedInStore = _lstSetChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId).Select(s => new RFilterCategoryModel
                            //{
                            //    Id = s.Id,
                            //    Name = s.Name
                            //}).ToList();

                            //lstTotalDataInStore = lstTotalAllSetMenu.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                        }

                        // Group by category
                        foreach (var itemCate in lstCateCheckedInStore)
                        {
                            subTotalQty = 0;
                            subTotalItem = 0;
                            subTotalPercent = 0;
                            subTotalDiscount = 0;
                            subTotalPromotion = 0;
                            subTotalUnitCost = 0;
                            subTotalTotalCost = 0;
                            subTotalCP = 0;

                            breakfastTotalQty = 0;
                            breakfastTotalItem = 0;
                            breakfastTotalPercent = 0;
                            breakfastTotalDiscount = 0;
                            breakfastTotalPromotion = 0;
                            breakfastTotalUnitCost = 0;
                            breakfastTotalTotalCost = 0;
                            breakfastTotalCP = 0;

                            lunchTotalQty = 0;
                            lunchTotalItem = 0;
                            lunchTotalPercent = 0;
                            lunchTotalDiscount = 0;
                            lunchTotalPromotion = 0;
                            lunchTotalUnitCost = 0;
                            lunchTotalTotalCost = 0;
                            lunchTotalCP = 0;

                            dinnerTotalQty = 0;
                            dinnerTotalItem = 0;
                            dinnerTotalPercent = 0;
                            dinnerTotalDiscount = 0;
                            dinnerTotalPromotion = 0;
                            dinnerTotalUnitCost = 0;
                            dinnerTotalTotalCost = 0;
                            dinnerTotalCP = 0;

                            ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Name));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            var lstCateItems = lstItemOfType.Where(w => w.CategoryId == itemCate.Id).ToList();
                            var lstItemTypes = lstCateItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                                                    || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).ToList();

                            List<ItemizedSalesAnalysisReportModels> lstDataByPeriod = new List<ItemizedSalesAnalysisReportModels>();

                            var breakfast = new ItemizedSalesPeriodValueTotal();
                            var lunch = new ItemizedSalesPeriodValueTotal();
                            var dinner = new ItemizedSalesPeriodValueTotal();
                            foreach (var item in lstItemTypes)
                            {
                                timeDish = item.CreatedDate.TimeOfDay;
                                if ((itemAmountTotal + miscInstore) != 0)
                                {
                                    item.Percent = (item.ItemTotal / (itemAmountTotal + miscInstore)) * 100;
                                }
                                // check percent data
                                // Breakfast
                                if (viewmodel.Breakfast)
                                {
                                    if (timeDish >= brearkStart && timeDish < brearkEnd)
                                    {
                                        breakfast.Qty += item.Quantity;
                                        breakfast.ItemTotal += item.ItemTotal;
                                        breakfast.Percent += item.Percent;
                                        breakfast.Discount += item.Discount;
                                        breakfast.Promotion += item.PromotionAmount;
                                        breakfast.UnitCost += item.Cost;
                                        breakfast.TotalCost += item.TotalCost;

                                        _breakfastOutletTotal.Qty += item.Quantity;
                                        _breakfastOutletTotal.ItemTotal += item.ItemTotal;
                                        _breakfastOutletTotal.Percent += item.Percent;
                                        _breakfastOutletTotal.Discount += item.Discount;
                                        _breakfastOutletTotal.Promotion += item.PromotionAmount;
                                        _breakfastOutletTotal.UnitCost += item.Cost;
                                        _breakfastOutletTotal.TotalCost += item.TotalCost;

                                        _breakfastTotal.Qty += item.Quantity;
                                        _breakfastTotal.ItemTotal += item.ItemTotal;
                                        _breakfastTotal.Percent += item.Percent;
                                        _breakfastTotal.Discount += item.Discount;
                                        _breakfastTotal.Promotion += item.PromotionAmount;
                                        _breakfastTotal.UnitCost += item.Cost;
                                        _breakfastTotal.TotalCost += item.TotalCost;

                                        // If CreatedDate is avaible with time of period
                                        lstDataByPeriod.Add(item);
                                    }
                                }

                                // lunch
                                if (viewmodel.Lunch)
                                {
                                    if (timeDish >= lunchStart && timeDish < lunchEnd)
                                    {
                                        lunch.Qty += item.Quantity;
                                        lunch.ItemTotal += item.ItemTotal;
                                        lunch.Percent += item.Percent;
                                        lunch.Discount += item.Discount;
                                        lunch.Promotion += item.PromotionAmount;
                                        lunch.UnitCost += item.Cost;
                                        lunch.TotalCost += item.TotalCost;

                                        _lunchOutletTotal.Qty += item.Quantity;
                                        _lunchOutletTotal.ItemTotal += item.ItemTotal;
                                        _lunchOutletTotal.Percent += item.Percent;
                                        _lunchOutletTotal.Discount += item.Discount;
                                        _lunchOutletTotal.Promotion += item.PromotionAmount;
                                        _lunchOutletTotal.UnitCost += item.Cost;
                                        _lunchOutletTotal.TotalCost += item.TotalCost;

                                        _lunchTotal.Qty += item.Quantity;
                                        _lunchTotal.ItemTotal += item.ItemTotal;
                                        _lunchTotal.Percent += item.Percent;
                                        _lunchTotal.Discount += item.Discount;
                                        _lunchTotal.Promotion += item.PromotionAmount;
                                        _lunchTotal.UnitCost += item.Cost;
                                        _lunchTotal.TotalCost += item.TotalCost;

                                        // If CreatedDate is avaible with time of period
                                        lstDataByPeriod.Add(item);
                                    }
                                }

                                // Dinner
                                if (viewmodel.Dinner)
                                {
                                    if (dinnerStart > dinnerEnd) //pass day
                                    {
                                        if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                        {
                                            dinner.Qty += item.Quantity;
                                            dinner.ItemTotal += item.ItemTotal;
                                            dinner.Percent += item.Percent;
                                            dinner.Discount += item.Discount;
                                            dinner.Promotion += item.PromotionAmount;
                                            dinner.UnitCost += item.Cost;
                                            dinner.TotalCost += item.TotalCost;

                                            _dinnerOutletTotal.Qty += item.Quantity;
                                            _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                            _dinnerOutletTotal.Percent += item.Percent;
                                            _dinnerOutletTotal.Discount += item.Discount;
                                            _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                            _dinnerOutletTotal.UnitCost += item.Cost;
                                            _dinnerOutletTotal.TotalCost += item.TotalCost;

                                            _dinnerTotal.Qty += item.Quantity;
                                            _dinnerTotal.ItemTotal += item.ItemTotal;
                                            _dinnerTotal.Percent += item.Percent;
                                            _dinnerTotal.Discount += item.Discount;
                                            _dinnerTotal.Promotion += item.PromotionAmount;
                                            _dinnerTotal.UnitCost += item.Cost;
                                            _dinnerTotal.TotalCost += item.TotalCost;

                                            // If CreatedDate is avaible with time of period
                                            lstDataByPeriod.Add(item);
                                        }
                                    }
                                    else //in day
                                    {
                                        if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                        {
                                            dinner.Qty += item.Quantity;
                                            dinner.ItemTotal += item.ItemTotal;
                                            dinner.Percent += item.Percent;
                                            dinner.Discount += item.Discount;
                                            dinner.Promotion += item.PromotionAmount;
                                            dinner.UnitCost += item.Cost;
                                            dinner.TotalCost += item.TotalCost;

                                            _dinnerOutletTotal.Qty += item.Quantity;
                                            _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                            _dinnerOutletTotal.Percent += item.Percent;
                                            _dinnerOutletTotal.Discount += item.Discount;
                                            _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                            _dinnerOutletTotal.UnitCost += item.Cost;
                                            _dinnerOutletTotal.TotalCost += item.TotalCost;

                                            _dinnerTotal.Qty += item.Quantity;
                                            _dinnerTotal.ItemTotal += item.ItemTotal;
                                            _dinnerTotal.Percent += item.Percent;
                                            _dinnerTotal.Discount += item.Discount;
                                            _dinnerTotal.Promotion += item.PromotionAmount;
                                            _dinnerTotal.UnitCost += item.Cost;
                                            _dinnerTotal.TotalCost += item.TotalCost;

                                            // If CreatedDate is avaible with time of period
                                            lstDataByPeriod.Add(item);
                                        }
                                    }
                                }

                            }//End lstItemTypes

                            // Get total item
                            _itemTotal.SCTotal += lstDataByPeriod.Sum(s => s.ServiceCharge);
                            _itemTotal.ItemTotal += lstDataByPeriod.Sum(s => s.ItemTotal);
                            _itemTotal.TaxTotal += lstDataByPeriod.Sum(s => s.Tax);
                            _itemTotal.DiscountTotal += lstDataByPeriod.Sum(s => s.Discount);
                            _itemTotal.PromotionTotal += lstDataByPeriod.Sum(s => s.PromotionAmount);

                            // Add items to lst item for NetSales
                            lstItemNoIncludeSaleInStore.AddRange(lstDataByPeriod.Where(w => w.IsIncludeSale == false).ToList());

                            //Group Item
                            var lstItemTypeGroup = lstDataByPeriod.GroupBy(gg => new { gg.ItemTypeId, gg.ItemId, gg.Price }).ToList();
                            double cp = 0; double cost = 0;
                            foreach (var item in lstItemTypeGroup)
                            {
                                ws.Cell("A" + index).Value = item.Select(ss => ss.ItemCode).FirstOrDefault();
                                ws.Cell("B" + index).Value = item.Select(ss => ss.ItemName).FirstOrDefault();
                                ws.Cell("C" + index).Value = item.Select(ss => ss.Price).FirstOrDefault().ToString("F");
                                ws.Cell("C" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Cell("D" + index).Value = item.Sum(d => d.Quantity);
                                ws.Cell("E" + index).Value = item.Sum(d => d.ItemTotal).ToString("F");
                                ws.Cell("F" + index).Value = item.Sum(d => d.Percent).ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-item.Sum(d => d.Discount)).ToString("F");

                                ws.Cell("H" + index).Value = (-item.Sum(d => d.PromotionAmount)).ToString("F");
                                ws.Cell("I" + index).Value = item.Select(ss => ss.Cost).FirstOrDefault().ToString("F");
                                cost += item.Select(ss => ss.Cost).FirstOrDefault();
                                ws.Cell("J" + index).Value = item.Sum(d => d.TotalCost).ToString("F");

                                if (item.Sum(d => d.TotalCost) == 0 || item.Sum(d => d.ItemTotal) == 0)
                                    cp = 0;
                                else
                                    cp = ((item.Sum(d => d.TotalCost) / item.Sum(d => d.ItemTotal)) * 100);

                                ws.Cell("K" + index).Value = cp.ToString("F") + " %";
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;
                            }//end lstItemTypeGroup

                            subTotalQty = breakfast.Qty + lunch.Qty + dinner.Qty;
                            subTotalItem = breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal;
                            subTotalPercent = breakfast.Percent + lunch.Percent + dinner.Percent;
                            subTotalDiscount = breakfast.Discount + lunch.Discount + dinner.Discount;
                            subTotalPromotion = breakfast.Promotion + lunch.Promotion + dinner.Promotion;
                            subTotalUnitCost = cost;
                            subTotalTotalCost = breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost;
                            if ((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) != 0 && (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal) != 0)
                            {
                                subTotalCP = (((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) / (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal)) * 100);
                            }

                            breakfastTotalQty = breakfast.Qty;
                            breakfastTotalItem = breakfast.ItemTotal;
                            breakfastTotalPercent = breakfast.Percent;
                            breakfastTotalDiscount = breakfast.Discount;
                            breakfastTotalPromotion = breakfast.Promotion;
                            breakfastTotalUnitCost = breakfast.UnitCost;
                            breakfastTotalTotalCost = breakfast.TotalCost;
                            if (breakfast.TotalCost != 0 && breakfast.ItemTotal != 0)
                            {
                                breakfastTotalCP = ((breakfast.TotalCost / breakfast.ItemTotal) * 100);
                            }

                            lunchTotalQty = lunch.Qty;
                            lunchTotalItem = lunch.ItemTotal;
                            lunchTotalPercent = lunch.Percent;
                            lunchTotalDiscount = lunch.Discount;
                            lunchTotalPromotion = lunch.Promotion;
                            lunchTotalUnitCost = lunch.UnitCost;
                            lunchTotalTotalCost = lunch.TotalCost;
                            lunchTotalCP = 0;
                            if (lunch.TotalCost != 0 && lunch.ItemTotal != 0)
                            {
                                lunchTotalCP = ((lunch.TotalCost / lunch.ItemTotal) * 100);
                            }

                            dinnerTotalQty = dinner.Qty;
                            dinnerTotalItem = dinner.ItemTotal;
                            dinnerTotalPercent = dinner.Percent;
                            dinnerTotalDiscount = dinner.Discount;
                            dinnerTotalPromotion = dinner.Promotion;
                            dinnerTotalUnitCost = dinner.UnitCost;
                            dinnerTotalTotalCost = dinner.TotalCost;
                            if (dinner.TotalCost != 0 && dinner.ItemTotal != 0)
                            {
                                dinnerTotalCP = ((dinner.TotalCost / dinner.ItemTotal) * 100);
                            }

                            // Get total information of current cate
                            var totalDataCurrentCate = lstTotalDataInStore.Where(w => w.CateId == itemCate.Id).FirstOrDefault();
                            // Update total data of current cate
                            totalDataCurrentCate.SubTotalQty += subTotalQty;
                            totalDataCurrentCate.SubTotalItem += subTotalItem;
                            totalDataCurrentCate.SubTotalPercent += subTotalPercent;
                            totalDataCurrentCate.SubTotalDiscount += subTotalDiscount;
                            totalDataCurrentCate.SubTotalPromotion += subTotalPromotion;
                            totalDataCurrentCate.SubTotalUnitCost += subTotalUnitCost;
                            totalDataCurrentCate.SubTotalTotalCost += subTotalTotalCost;
                            totalDataCurrentCate.SubTotalCP += subTotalCP;

                            totalDataCurrentCate.BreakfastTotalQty += breakfastTotalQty;
                            totalDataCurrentCate.BreakfastTotalItem += breakfastTotalItem;
                            totalDataCurrentCate.BreakfastTotalPercent += breakfastTotalPercent;
                            totalDataCurrentCate.BreakfastTotalDiscount += breakfastTotalDiscount;
                            totalDataCurrentCate.BreakfastTotalPromotion += breakfastTotalPromotion;
                            totalDataCurrentCate.BreakfastTotalUnitCost += breakfastTotalUnitCost;
                            totalDataCurrentCate.BreakfastTotalTotalCost += breakfastTotalTotalCost;
                            totalDataCurrentCate.BreakfastTotalCP += breakfastTotalCP;

                            totalDataCurrentCate.LunchTotalQty += lunchTotalQty;
                            totalDataCurrentCate.LunchTotalItem += lunchTotalItem;
                            totalDataCurrentCate.LunchTotalPercent += lunchTotalPercent;
                            totalDataCurrentCate.LunchTotalDiscount += lunchTotalDiscount;
                            totalDataCurrentCate.LunchTotalPromotion += lunchTotalPromotion;
                            totalDataCurrentCate.LunchTotalUnitCost += lunchTotalUnitCost;
                            totalDataCurrentCate.LunchTotalTotalCost += lunchTotalTotalCost;
                            totalDataCurrentCate.LunchTotalCP += lunchTotalCP;

                            totalDataCurrentCate.DinnerTotalQty += dinnerTotalQty;
                            totalDataCurrentCate.DinnerTotalItem += dinnerTotalItem;
                            totalDataCurrentCate.DinnerTotalPercent += dinnerTotalPercent;
                            totalDataCurrentCate.DinnerTotalDiscount += dinnerTotalDiscount;
                            totalDataCurrentCate.DinnerTotalPromotion += dinnerTotalPromotion;
                            totalDataCurrentCate.DinnerTotalUnitCost += dinnerTotalUnitCost;
                            totalDataCurrentCate.DinnerTotalTotalCost += dinnerTotalTotalCost;
                            totalDataCurrentCate.DinnerTotalCP += dinnerTotalCP;

                            // Show total date of current
                            // If it has any cate child which was checked  => title: Sub-Total: CateName
                            // If it doesn't have any cate child which was checked && it doesn't has parent cate which was checked => it's top cate => title: Category Total: CateName
                            if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && !lstTotalDataInStore.Where(w => !string.IsNullOrEmpty(totalDataCurrentCate.ParentId) && w.CateId == totalDataCurrentCate.ParentId && w.Checked).Any())
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Total") + " : " + totalDataCurrentCate.CateName);
                            }
                            else
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total") + " : " + totalDataCurrentCate.CateName);
                            }

                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            ws.Cell("D" + index).Value = totalDataCurrentCate.SubTotalQty;
                            ws.Cell("E" + index).Value = totalDataCurrentCate.SubTotalItem.ToString("F");
                            ws.Cell("F" + index).Value = totalDataCurrentCate.SubTotalPercent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-totalDataCurrentCate.SubTotalDiscount).ToString("F");
                            ws.Cell("H" + index).Value = (-totalDataCurrentCate.SubTotalPromotion).ToString("F");
                            ws.Cell("I" + index).Value = totalDataCurrentCate.SubTotalUnitCost.ToString("F");
                            ws.Cell("J" + index).Value = totalDataCurrentCate.SubTotalTotalCost.ToString("F");
                            ws.Cell("K" + index).Value = totalDataCurrentCate.SubTotalCP.ToString("F") + " %";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                            //Morning
                            if (viewmodel.Breakfast)
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Cell("D" + index).Value = totalDataCurrentCate.BreakfastTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.BreakfastTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.BreakfastTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.BreakfastTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.BreakfastTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.BreakfastTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.BreakfastTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.BreakfastTotalCP.ToString("F") + " %";
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                index++;
                            }

                            //Afternoon
                            if (viewmodel.Lunch)
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Cell("D" + index).Value = totalDataCurrentCate.LunchTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.LunchTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.LunchTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.LunchTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.LunchTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.LunchTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.LunchTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.LunchTotalCP.ToString("F") + " %";
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                index++;
                            }

                            //dinner
                            if (viewmodel.Dinner)
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Cell("D" + index).Value = totalDataCurrentCate.DinnerTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.DinnerTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.DinnerTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.DinnerTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.DinnerTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.DinnerTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.DinnerTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.DinnerTotalCP.ToString("F") + " %";
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                index++;
                            }

                            // Check current cate is last chile of parent current cate
                            Boolean isLastChildCate = false;
                            // If current cate has parent cate, check for show total data of parent cate
                            if (!string.IsNullOrEmpty(totalDataCurrentCate.ParentId))
                            {
                                var parentCurrentCateInfo = lstTotalDataInStore.Where(w => w.CateId == totalDataCurrentCate.ParentId).FirstOrDefault();
                                // List cate child
                                var lstCateChildCheckedOfParent = parentCurrentCateInfo.ListCateChildChecked.ToList();
                                if (lstCateChildCheckedOfParent != null && lstCateChildCheckedOfParent.Any())
                                {
                                    // Get id of last child cate
                                    string idLastCateChild = parentCurrentCateInfo.ListCateChildChecked.LastOrDefault().ToString();

                                    // Current cate is last cate of parent cate
                                    if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && idLastCateChild == totalDataCurrentCate.CateId)
                                    {
                                        isLastChildCate = true;
                                    }
                                }

                                DisplayTotalOfParentCate(ref ws, totalDataCurrentCate, ref index, ref lstTotalDataInStore, isLastChildCate, totalDataCurrentCate, viewmodel);
                            }
                        }//End Group by category
                    }//end lstGroupItemType

                    // MISC
                    int startM = index;
                    listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();

                    if (listMiscDisPeriod != null && listMiscDisPeriod.Any())
                    {
                        listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                        if (listMiscDisPeriodByStore != null && listMiscDisPeriodByStore.Any())
                        {
                            // MISC
                            ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                            index++;
                            for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                            {
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                ws.Cell("E" + index).SetValue(listMiscDisPeriodByStore[m].MiscTotal.ToString("F"));
                                listMiscDisPeriodByStore[m].Percent = ((listMiscDisPeriodByStore[m].MiscTotal / (itemAmountTotal + miscInstore)) * 100);
                                ws.Cell("F" + index).SetValue(listMiscDisPeriodByStore[m].Percent.ToString("F") + " %");
                                index++;

                                _itemTotal.ItemTotal += listMiscDisPeriodByStore[m].MiscTotal;
                            }

                            // Discount Total Bill
                            ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            index++;
                            for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                            {
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                ws.Cell("G" + index).SetValue((-listMiscDisPeriodByStore[m].BillDiscountTotal).ToString("F"));
                                subDisPeriod += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                subDisPeriodByStore += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                _itemTotal.DiscountTotal += listMiscDisPeriodByStore[m].BillDiscountTotal;

                                index++;

                            }
                        }
                    }

                    ws.Range("A" + startM + ":K" + index).Style.Font.SetBold(true);
                    ws.Range("A" + startM + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + startM + ":D" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range("E" + startM + ":G" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Outlet Sub-total
                    //_subletTotal
                    double cpOutlet = 0;
                    var miscPercent = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                    var miscBreafast = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.BREAKFAST).FirstOrDefault();
                    var miscLunch = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.LUNCH).FirstOrDefault();
                    var miscDinner = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.DINNER).FirstOrDefault();

                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub-total"));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                    ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                    ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                        + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                    ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                    ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                    ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                    if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                        || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                        cpOutlet = 0;
                    else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                            / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                    ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                    ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                    ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    // TOTAL
                    if (listMiscDisPeriodByStore == null)
                        listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();
                    index = index - 1;
                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                    ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                    ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                    ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                        + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                    ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                    ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                    ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                    if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                    || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                        cpOutlet = 0;
                    else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                            / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                    ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                    ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                    ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    var discountMonring = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.BREAKFAST && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                    var discountLunch = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.LUNCH && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                    var discountDinner = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.DINNER && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();

                    //Morning
                    if (viewmodel.Breakfast)
                    {
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = _breakfastOutletTotal.Qty;
                        ws.Cell("E" + index).Value = (_breakfastOutletTotal.ItemTotal + (miscBreafast != null ? miscBreafast.MiscTotal : 0)).ToString("F");
                        ws.Cell("F" + index).Value = (_breakfastOutletTotal.Percent + (miscBreafast != null ? miscBreafast.Percent : 0)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-_breakfastOutletTotal.Discount - (discountMonring != null ? discountMonring.BillDiscountTotal : 0)).ToString("F");
                        ws.Cell("H" + index).Value = (-_breakfastOutletTotal.Promotion).ToString("F");
                        ws.Cell("I" + index).Value = _breakfastOutletTotal.UnitCost.ToString("F");
                        ws.Cell("J" + index).Value = _breakfastOutletTotal.TotalCost.ToString("F");
                        if (_breakfastOutletTotal.TotalCost == 0 || _breakfastOutletTotal.ItemTotal == 0)
                            _breakfastOutletTotal.CP = 0;
                        else
                            _breakfastOutletTotal.CP = ((_breakfastOutletTotal.TotalCost / _breakfastOutletTotal.ItemTotal) * 100);
                        ws.Cell("K" + index).Value = _breakfastOutletTotal.CP.ToString("F") + " %";
                        ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;
                    }

                    //Afternoon
                    if (viewmodel.Lunch)
                    {
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = _lunchOutletTotal.Qty;
                        ws.Cell("E" + index).Value = (_lunchOutletTotal.ItemTotal + (miscLunch != null ? miscLunch.MiscTotal : 0)).ToString("F");
                        ws.Cell("F" + index).Value = (_lunchOutletTotal.Percent + (miscLunch != null ? miscLunch.Percent : 0)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-_lunchOutletTotal.Discount - (discountLunch != null ? discountLunch.BillDiscountTotal : 0)).ToString("F");
                        ws.Cell("H" + index).Value = (-_lunchOutletTotal.Promotion).ToString("F");
                        ws.Cell("I" + index).Value = _lunchOutletTotal.UnitCost.ToString("F");
                        ws.Cell("J" + index).Value = _lunchOutletTotal.TotalCost.ToString("F");
                        if (_lunchOutletTotal.TotalCost == 0 || _lunchOutletTotal.ItemTotal == 0)
                            _lunchOutletTotal.CP = 0;
                        else
                            _lunchOutletTotal.CP = ((_lunchOutletTotal.TotalCost / _lunchOutletTotal.ItemTotal) * 100);
                        ws.Cell("K" + index).Value = _lunchOutletTotal.CP.ToString("F") + " %";
                        ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;
                    }

                    //dinner
                    if (viewmodel.Dinner)
                    {
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = _dinnerOutletTotal.Qty;
                        ws.Cell("E" + index).Value = (_dinnerOutletTotal.ItemTotal + (miscDinner != null ? miscDinner.MiscTotal : 0)).ToString("F");
                        ws.Cell("F" + index).Value = (_dinnerOutletTotal.Percent + (miscDinner != null ? miscDinner.Percent : 0)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-_dinnerOutletTotal.Discount - (discountDinner != null ? discountDinner.BillDiscountTotal : 0)).ToString("F");
                        ws.Cell("H" + index).Value = (-_dinnerOutletTotal.Promotion).ToString("F");
                        ws.Cell("I" + index).Value = _dinnerOutletTotal.UnitCost.ToString("F");
                        ws.Cell("J" + index).Value = _dinnerOutletTotal.TotalCost.ToString("F");
                        if (_dinnerOutletTotal.TotalCost == 0 || _dinnerOutletTotal.ItemTotal == 0)
                            _dinnerOutletTotal.CP = 0;
                        else
                            _dinnerOutletTotal.CP = ((_dinnerOutletTotal.TotalCost / _dinnerOutletTotal.ItemTotal) * 100);
                        ws.Cell("K" + index).Value = _dinnerOutletTotal.CP.ToString("F") + " %";
                        ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;
                    }

                    ws.Range("C" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //format file
                    //header
                    ws.Range("A4:K6").Style.Font.SetBold(true);
                    //Set color
                    ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //set Border        
                    ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    if (_firstStore)
                    {
                        ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    else
                    {
                        ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    index++;

                    // end total

                    // Summary
                    //double refund = 0;
                    bool isTaxInclude = false;
                    ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                    ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                    if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                    {
                        ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                    }
                    else
                    {
                        ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                        isTaxInclude = true;
                    }
                    ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                    ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                    ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    index++;

                    _noincludeSale = 0;
                    if (lstItemNoIncludeSaleInStore != null && lstItemNoIncludeSaleInStore.Any())
                    {
                        if (isTaxInclude)
                        {
                            _noincludeSale = lstItemNoIncludeSaleInStore.Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                        }
                        else
                        {
                            _noincludeSale = lstItemNoIncludeSaleInStore.Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                        }
                    }

                    //GC value
                    payGC = 0;
                    if (lstPayments != null && lstPayments.Any())
                    {
                        var lstPaymentsInStore = lstPayments.Where(p => p.StoreId == itemGroupStore.Key.StoreId).ToList();
                        List<PaymentModels> lstPaymentByPeriod = new List<PaymentModels>();
                        foreach (var item in lstPaymentsInStore)
                        {
                            timeDish = item.CreatedDate.TimeOfDay;
                            // Breakfast
                            if (viewmodel.Breakfast)
                            {
                                if (timeDish >= brearkStart && timeDish < brearkEnd)
                                {
                                    lstPaymentByPeriod.Add(item);
                                }
                            }

                            // Lunch
                            if (viewmodel.Lunch)
                            {
                                if (timeDish >= lunchStart && timeDish < lunchEnd)
                                {
                                    lstPaymentByPeriod.Add(item);
                                }
                            }

                            // Dinner
                            if (viewmodel.Dinner)
                            {
                                if (dinnerStart > dinnerEnd) //pass day
                                {
                                    if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                    {
                                        lstPaymentByPeriod.Add(item);
                                    }
                                }
                                else //in day
                                {
                                    if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                    {
                                        lstPaymentByPeriod.Add(item);
                                    }
                                }
                            }
                        }
                        payGC = lstPaymentByPeriod.Sum(p => (double)p.Amount);
                    }

                    //sell GC 22/09/2017
                    //sell GC 22/09/2017
                    double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                            && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                            && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.TotalAmount.Value);

                    //rounding
                    var roudingAmount = _lstRoundings.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.Rounding);
                    var creditNoteIds = _lstRoundings.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && !string.IsNullOrEmpty(ww.CreditNoteNo))
                        .Select(ss => ss.OrderId).ToList();
                    double totalCreditNote = 0;
                    //if (creditNoteIds != null && creditNoteIds.Any())
                    //{
                    //    totalCreditNote = lstData.Where(ww => creditNoteIds.Contains(ww.ReceiptId) && ww.StoreId == itemGroupStore.Key.StoreId && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)
                    //        .Sum(ss => ss.ItemTotal + ss.ExtraPrice);
                    //}

                    //netsale
                    ws.Cell("A" + index).Value = (_itemTotal.ItemTotal - subDisPeriod - (isTaxInclude ? _itemTotal.TaxTotal : 0)
                        - _noincludeSale - payGC + giftCardSell + roudingAmount - totalCreditNote).ToString("F");

                    ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                    ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                    ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                    ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                    ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    index += 2;
                    indexNextStore = index;
                    _firstStore = false;
                }//end foreach (var itemGroupStore in lstItemGroupByStore)
                if (lstItemGroupByStore == null || lstItemGroupByStore.Count() == 0)
                {
                    //check only sale GC
                    var lstItemGroupByStoreWithGC = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId))
                       .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                    if (lstItemGroupByStoreWithGC != null && lstItemGroupByStoreWithGC.Any())
                    {
                        //header
                        ws.Range("A4:K6").Style.Font.SetBold(true);
                        //Set color
                        ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //set Border        
                        ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                        foreach (var itemGroupStore in lstItemGroupByStoreWithGC)
                        {
                            if (_firstStore)
                            {
                                ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            }
                            else
                            {
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }
                            // Store name
                            var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                            ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                            if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                            }
                            else
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                            }
                            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            index++;

                            double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                 && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                 && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.TotalAmount.Value);

                            ws.Cell("A" + index).Value = (giftCardSell).ToString("F");
                            ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                            ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                            ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                            ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                            ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            index += 2;
                            indexNextStore = index;
                            _firstStore = false;
                        }
                    }

                }

            }

            ws.Columns(1, 3).AdjustToContents();
            //set Width for Colum 
            ws.Column(4).Width = 20;
            ws.Columns(5, 11).AdjustToContents();
            return wb;
        }

        //public List<ItemizedSalesAnalysisReportModels> GetItemsForDailyReceiptReports(BaseReportModel model)
        //{
        //    using (var cxt = new NuWebContext())
        //    {
        //        var lstData = (from tb in cxt.R_ItemizedSalesAnalysisReport
        //                       where model.ListStores.Contains(tb.StoreId)
        //                            && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate) && tb.Mode == model.Mode
        //                            && (tb.GLAccountCode != null || tb.IsIncludeSale == false)

        //                       select new ItemizedSalesAnalysisReportModels
        //                       {
        //                           StoreId = tb.StoreId,
        //                           CreatedDate = tb.CreatedDate,
        //                           CategoryId = tb.CategoryId,
        //                           CategoryName = tb.CategoryName,
        //                           ExtraPrice = tb.ExtraPrice,
        //                           TotalPrice = tb.TotalPrice,
        //                           GLAccountCode = tb.GLAccountCode,
        //                           IsIncludeSale = tb.IsIncludeSale,
        //                           BusinessId = tb.BusinessId,
        //                           ServiceCharge = tb.ServiceCharge,
        //                           Tax = tb.Tax,
        //                           ExtraAmount = tb.ExtraAmount.HasValue ? tb.ExtraAmount.Value : 0,
        //                           TotalAmount = tb.TotalAmount.HasValue ? tb.TotalAmount.Value : 0,
        //                           TotalDiscount = tb.TotalDiscount.HasValue ? tb.TotalDiscount : 0,
        //                           PromotionAmount = tb.PromotionAmount,
        //                           ReceiptId = tb.ReceiptId
        //                       }).ToList();
        //        return lstData;
        //    }
        //}

        // Updated 09202017
        public XLWorkbook ExportExcel_V1ForMerchantExtend(List<ItemizedSalesAnalysisReportModels> lstData, ItemizedSalesAnalysisReportModel viewmodel
            , List<StoreModels> lstStores, DateTime dToFilter, DateTime dFromFilter, List<string> lstGCId
            , List<RFilterCategoryV1Model> _lstCateChecked, List<RFilterCategoryModel> _lstSetChecked
            , List<RFilterCategoryV1ReportModel> lstTotalAllCate, List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu)
        {
            List<PaymentModels> lstPayments = _orderPaymentMethodFactory.GetDataPaymentItemsByGC(viewmodel, lstGCId);

            string sheetName = "Itemized_Sales_Analysis_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 11, dFromFilter, dToFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized Sales Analysis Report"));

            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];

            // Get value from setting of common
            TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
            TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
            TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
            TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
            TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
            TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);

            // MISC DISCOUNT TOTAL BILL
            DiscountAndMiscReportFactory miscFactory = new DiscountAndMiscReportFactory();
            List<DiscountAndMiscReportModels> listMisc_Discout = new List<DiscountAndMiscReportModels>();
            listMisc_Discout = miscFactory.GetReceiptDiscountAndMisc(viewmodel.FromDate, viewmodel.ToDate, viewmodel.ListStores, viewmodel.Mode);
            listMisc_Discout.ForEach(ss => ss.DiscountValue = 0);

            //Get rounding amount
            var _lstRoundings = GetRoundingAmount(viewmodel);
            DiscountDetailsReportFactory discountDetailFactory = new DiscountDetailsReportFactory();
            var lstDiscount = discountDetailFactory.GetDiscountTotal(viewmodel.ListStores, viewmodel.FromDate, viewmodel.ToDate);

            listMisc_Discout.AddRange(lstDiscount);

            List<MISCBillDiscountPeriodModels> listMiscDisPeriod = new List<MISCBillDiscountPeriodModels>();
            if (listMisc_Discout != null)
            {
                var listMisc_DiscoutGroupStore = listMisc_Discout.GroupBy(gg => gg.StoreId).ToList();

                foreach (var item in listMisc_DiscoutGroupStore)
                {
                    // CHECK PERIOD IS CHECKED
                    if (viewmodel.Breakfast)
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
                    if (viewmodel.Lunch)
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
                    if (viewmodel.Dinner)
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

                }
            }

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
            ws.Cell("H5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
            ws.Cell("I5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
            ws.Cell("J5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
            ws.Cell("K5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

            //set width column
            ws.Column("A").Width = 20;
            ws.Column("B").Width = 30;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 10;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 15;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 15;

            int index = 6;
            int indexNextStore = 0;
            //Total
            ItemizedSalesNewTotal _itemTotal = new ItemizedSalesNewTotal();
            var _breakfastTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerTotal = new ItemizedSalesPeriodValueTotal();

            var _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
            var lstBusinessId = new List<string>();
            bool _firstStore = true;
            if (lstData != null && lstData.Any())
            {
                // Group storeId
                TimeSpan timeDish = new TimeSpan();

                lstData.ForEach(x =>
                {
                    var store = lstStores.Where(z => z.Id.Equals(x.StoreId)).FirstOrDefault();
                    x.StoreName = store == null ? "" : store.Name;
                });

                List<RFilterCategoryV1ReportModel> lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                double subTotalQty = 0;
                double subTotalItem = 0;
                double subTotalPercent = 0;
                double subTotalDiscount = 0;
                double subTotalPromotion = 0;
                double subTotalUnitCost = 0;
                double subTotalTotalCost = 0;
                double subTotalCP = 0;

                double breakfastTotalQty = 0;
                double breakfastTotalItem = 0;
                double breakfastTotalPercent = 0;
                double breakfastTotalDiscount = 0;
                double breakfastTotalPromotion = 0;
                double breakfastTotalUnitCost = 0;
                double breakfastTotalTotalCost = 0;
                double breakfastTotalCP = 0;

                double lunchTotalQty = 0;
                double lunchTotalItem = 0;
                double lunchTotalPercent = 0;
                double lunchTotalDiscount = 0;
                double lunchTotalPromotion = 0;
                double lunchTotalUnitCost = 0;
                double lunchTotalTotalCost = 0;
                double lunchTotalCP = 0;

                double dinnerTotalQty = 0;
                double dinnerTotalItem = 0;
                double dinnerTotalPercent = 0;
                double dinnerTotalDiscount = 0;
                double dinnerTotalPromotion = 0;
                double dinnerTotalUnitCost = 0;
                double dinnerTotalTotalCost = 0;
                double dinnerTotalCP = 0;

                double _noincludeSale = 0;
                double payGC = 0;

                // Get list item for NetSales (IsIncludeSale == false)
                List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportModels>();

                List<MISCBillDiscountPeriodModels> listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();

                var lstItemGroupByStore = lstData.Where(ww => string.IsNullOrEmpty(ww.GiftCardId))
                        .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                foreach (var itemGroupStore in lstItemGroupByStore)
                {
                    lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                    // Get store setting : time of Period
                    if (currentUser != null)
                    {
                        // Get value from setting of mechant
                        Commons.BreakfastStart = currentUser.BreakfastStart;
                        Commons.BreakfastEnd = currentUser.BreakfastEnd;
                        Commons.LunchStart = currentUser.LunchStart;
                        Commons.LunchEnd = currentUser.LunchEnd;
                        Commons.DinnerStart = currentUser.DinnerStart;
                        Commons.DinnerEnd = currentUser.DinnerEnd;

                        // Get value from setting of store
                        if (currentUser.ListSetting != null && currentUser.ListSetting.Any())
                        {
                            var settingPeriodOfStore = currentUser.ListSetting.Where(w => w.StoreID == itemGroupStore.Key.StoreId).ToList();

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

                    // Get listMiscDisPeriod in store
                    var lstItemInStore = listMisc_Discout.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();

                    for (int i = 0; i < lstItemInStore.Count; i++)
                    {
                        lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportModels>();

                        // Get Total Misc to + ItemTotal
                        TimeSpan timeMisc = new TimeSpan(lstItemInStore[i].Hour, 0, 0);
                        // Total period Misc_Discout
                        // TOTAL PERIOD MISC AND DISCOUNT BILL TOTAL
                        if (viewmodel.Breakfast)
                        {
                            if (timeMisc >= brearkStart && timeMisc < brearkEnd)
                            {
                                var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.BREAKFAST) && p.StoreId == itemGroupStore.Key.StoreId);
                                period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                period.MiscTotal += lstItemInStore[i].MiscValue;
                                period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                            }
                        }
                        if (viewmodel.Lunch)
                        {
                            if (timeMisc >= lunchStart && timeMisc < lunchEnd)
                            {
                                var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.LUNCH) && p.StoreId == itemGroupStore.Key.StoreId);
                                period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                period.MiscTotal += lstItemInStore[i].MiscValue;
                                period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                            }
                        }
                        if (viewmodel.Dinner)
                        {
                            if (dinnerStart > dinnerEnd)//pass day
                            {
                                if (timeMisc >= dinnerStart || timeMisc < dinnerEnd)
                                {
                                    var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == itemGroupStore.Key.StoreId);
                                    period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                    period.MiscTotal += lstItemInStore[i].MiscValue;
                                    period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                }
                            }
                            else//in day
                            {
                                if (timeMisc >= dinnerStart && timeMisc < dinnerEnd)
                                {
                                    var period = listMiscDisPeriod.FirstOrDefault(p => p.Period.Equals(Commons.DINNER) && p.StoreId == itemGroupStore.Key.StoreId);
                                    period.StoreName = lstStores.Where(ww => ww.Id == period.StoreId).Select(ss => ss.Name).FirstOrDefault();
                                    period.MiscTotal += lstItemInStore[i].MiscValue;
                                    period.BillDiscountTotal += lstItemInStore[i].DiscountValue;
                                }
                            }
                        }
                    }

                    var miscInstore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.MiscTotal);

                    double subDisPeriodByStore = 0;
                    double subDisPeriod = 0;
                    _itemTotal = new ItemizedSalesNewTotal();
                    _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
                    _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
                    _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();

                    // Store name
                    var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.NameExtend).FirstOrDefault();
                    ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    index++;

                    var lstItems = itemGroupStore.ToList();
                    // Group item type
                    var lstGroupItemType = lstItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                   || ww.ItemTypeId == (int)Commons.EProductType.SetMenu))
                       .OrderBy(x => x.ItemTypeId).GroupBy(gg => gg.ItemTypeId).ToList();

                    var itemAmountTotal = lstItems.Sum(ss => ss.ItemTotal);
                    foreach (var itemTypeId in lstGroupItemType)
                    {
                        var lstItemOfType = lstItems.Where(x => x.ItemTypeId == itemTypeId.Key).ToList();

                        List<RFilterCategoryModel> lstCateCheckedInStore = new List<RFilterCategoryModel>();

                        if (itemTypeId.Key == (int)Commons.EProductType.Dish)
                        {
                            lstCateCheckedInStore = _lstCateChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId)
                                .Select(s => new RFilterCategoryModel
                                {
                                    Id = s.Id,
                                    Name = s.Name
                                }).ToList();

                            lstTotalDataInStore = lstTotalAllCate.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                        }
                        else
                        {
                            //check data old
                            var tmp = lstItemOfType.Where(ww => ww.CategoryId == "SetMenu").FirstOrDefault();
                            //check setmenu have category or not category
                            var categoryExist = _lstSetChecked.Where(ww => string.IsNullOrEmpty(ww.CategoryID)).FirstOrDefault();
                            if (categoryExist != null || tmp != null)//
                            {
                                lstCateCheckedInStore = new List<RFilterCategoryModel>() {
                                    new RFilterCategoryModel(){Id="SetMenu", Name ="SetMenu"}
                                };
                            }
                            else
                            {
                                var lstTmp = _lstSetChecked.Where(w => w.StoreId == itemGroupStore.Key.StoreId)
                                    .Select(s => new
                                    {
                                        Id = s.CategoryID,
                                        Name = s.CategoryName
                                    }).Distinct().ToList();

                                lstCateCheckedInStore = lstTmp
                                    .Select(s => new RFilterCategoryModel
                                    {
                                        Id = s.Id,
                                        Name = s.Name
                                    }).ToList();

                            }
                            lstTotalDataInStore = lstTotalAllSetMenu.Where(w => w.StoreId == itemGroupStore.Key.StoreId).ToList();
                            lstTotalDataInStore.Add(new RFilterCategoryV1ReportModel() { CateId = "SetMenu", CateName = "SetMenu" });
                        }

                        // Group by category
                        foreach (var itemCate in lstCateCheckedInStore)
                        {
                            subTotalQty = 0;
                            subTotalItem = 0;
                            subTotalPercent = 0;
                            subTotalDiscount = 0;
                            subTotalPromotion = 0;
                            subTotalUnitCost = 0;
                            subTotalTotalCost = 0;
                            subTotalCP = 0;

                            breakfastTotalQty = 0;
                            breakfastTotalItem = 0;
                            breakfastTotalPercent = 0;
                            breakfastTotalDiscount = 0;
                            breakfastTotalPromotion = 0;
                            breakfastTotalUnitCost = 0;
                            breakfastTotalTotalCost = 0;
                            breakfastTotalCP = 0;

                            lunchTotalQty = 0;
                            lunchTotalItem = 0;
                            lunchTotalPercent = 0;
                            lunchTotalDiscount = 0;
                            lunchTotalPromotion = 0;
                            lunchTotalUnitCost = 0;
                            lunchTotalTotalCost = 0;
                            lunchTotalCP = 0;

                            dinnerTotalQty = 0;
                            dinnerTotalItem = 0;
                            dinnerTotalPercent = 0;
                            dinnerTotalDiscount = 0;
                            dinnerTotalPromotion = 0;
                            dinnerTotalUnitCost = 0;
                            dinnerTotalTotalCost = 0;
                            dinnerTotalCP = 0;

                            ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Name));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            var lstCateItems = lstItemOfType.Where(w => w.CategoryId == itemCate.Id).ToList();
                            var lstItemTypes = lstCateItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                                                    || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).ToList();

                            List<ItemizedSalesAnalysisReportModels> lstDataByPeriod = new List<ItemizedSalesAnalysisReportModels>();

                            var breakfast = new ItemizedSalesPeriodValueTotal();
                            var lunch = new ItemizedSalesPeriodValueTotal();
                            var dinner = new ItemizedSalesPeriodValueTotal();
                            foreach (var item in lstItemTypes)
                            {
                                timeDish = item.CreatedDate.TimeOfDay;
                                if ((itemAmountTotal + miscInstore) != 0)
                                {
                                    item.Percent = (item.ItemTotal / (itemAmountTotal + miscInstore)) * 100;
                                }
                                // check percent data
                                // Breakfast
                                if (viewmodel.Breakfast)
                                {
                                    if (timeDish >= brearkStart && timeDish < brearkEnd)
                                    {
                                        breakfast.Qty += item.Quantity;
                                        breakfast.ItemTotal += item.ItemTotal;
                                        breakfast.Percent += item.Percent;
                                        breakfast.Discount += item.Discount;
                                        breakfast.Promotion += item.PromotionAmount;
                                        breakfast.UnitCost += item.Cost;
                                        breakfast.TotalCost += item.TotalCost;

                                        _breakfastOutletTotal.Qty += item.Quantity;
                                        _breakfastOutletTotal.ItemTotal += item.ItemTotal;
                                        _breakfastOutletTotal.Percent += item.Percent;
                                        _breakfastOutletTotal.Discount += item.Discount;
                                        _breakfastOutletTotal.Promotion += item.PromotionAmount;
                                        _breakfastOutletTotal.UnitCost += item.Cost;
                                        _breakfastOutletTotal.TotalCost += item.TotalCost;

                                        _breakfastTotal.Qty += item.Quantity;
                                        _breakfastTotal.ItemTotal += item.ItemTotal;
                                        _breakfastTotal.Percent += item.Percent;
                                        _breakfastTotal.Discount += item.Discount;
                                        _breakfastTotal.Promotion += item.PromotionAmount;
                                        _breakfastTotal.UnitCost += item.Cost;
                                        _breakfastTotal.TotalCost += item.TotalCost;

                                        // If CreatedDate is avaible with time of period
                                        lstDataByPeriod.Add(item);
                                    }
                                }

                                // lunch
                                if (viewmodel.Lunch)
                                {
                                    if (timeDish >= lunchStart && timeDish < lunchEnd)
                                    {
                                        lunch.Qty += item.Quantity;
                                        lunch.ItemTotal += item.ItemTotal;
                                        lunch.Percent += item.Percent;
                                        lunch.Discount += item.Discount;
                                        lunch.Promotion += item.PromotionAmount;
                                        lunch.UnitCost += item.Cost;
                                        lunch.TotalCost += item.TotalCost;

                                        _lunchOutletTotal.Qty += item.Quantity;
                                        _lunchOutletTotal.ItemTotal += item.ItemTotal;
                                        _lunchOutletTotal.Percent += item.Percent;
                                        _lunchOutletTotal.Discount += item.Discount;
                                        _lunchOutletTotal.Promotion += item.PromotionAmount;
                                        _lunchOutletTotal.UnitCost += item.Cost;
                                        _lunchOutletTotal.TotalCost += item.TotalCost;

                                        _lunchTotal.Qty += item.Quantity;
                                        _lunchTotal.ItemTotal += item.ItemTotal;
                                        _lunchTotal.Percent += item.Percent;
                                        _lunchTotal.Discount += item.Discount;
                                        _lunchTotal.Promotion += item.PromotionAmount;
                                        _lunchTotal.UnitCost += item.Cost;
                                        _lunchTotal.TotalCost += item.TotalCost;

                                        // If CreatedDate is avaible with time of period
                                        lstDataByPeriod.Add(item);
                                    }
                                }

                                // Dinner
                                if (viewmodel.Dinner)
                                {
                                    if (dinnerStart > dinnerEnd) //pass day
                                    {
                                        if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                        {
                                            dinner.Qty += item.Quantity;
                                            dinner.ItemTotal += item.ItemTotal;
                                            dinner.Percent += item.Percent;
                                            dinner.Discount += item.Discount;
                                            dinner.Promotion += item.PromotionAmount;
                                            dinner.UnitCost += item.Cost;
                                            dinner.TotalCost += item.TotalCost;

                                            _dinnerOutletTotal.Qty += item.Quantity;
                                            _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                            _dinnerOutletTotal.Percent += item.Percent;
                                            _dinnerOutletTotal.Discount += item.Discount;
                                            _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                            _dinnerOutletTotal.UnitCost += item.Cost;
                                            _dinnerOutletTotal.TotalCost += item.TotalCost;

                                            _dinnerTotal.Qty += item.Quantity;
                                            _dinnerTotal.ItemTotal += item.ItemTotal;
                                            _dinnerTotal.Percent += item.Percent;
                                            _dinnerTotal.Discount += item.Discount;
                                            _dinnerTotal.Promotion += item.PromotionAmount;
                                            _dinnerTotal.UnitCost += item.Cost;
                                            _dinnerTotal.TotalCost += item.TotalCost;

                                            // If CreatedDate is avaible with time of period
                                            lstDataByPeriod.Add(item);
                                        }
                                    }
                                    else //in day
                                    {
                                        if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                        {
                                            dinner.Qty += item.Quantity;
                                            dinner.ItemTotal += item.ItemTotal;
                                            dinner.Percent += item.Percent;
                                            dinner.Discount += item.Discount;
                                            dinner.Promotion += item.PromotionAmount;
                                            dinner.UnitCost += item.Cost;
                                            dinner.TotalCost += item.TotalCost;

                                            _dinnerOutletTotal.Qty += item.Quantity;
                                            _dinnerOutletTotal.ItemTotal += item.ItemTotal;
                                            _dinnerOutletTotal.Percent += item.Percent;
                                            _dinnerOutletTotal.Discount += item.Discount;
                                            _dinnerOutletTotal.Promotion += item.PromotionAmount;
                                            _dinnerOutletTotal.UnitCost += item.Cost;
                                            _dinnerOutletTotal.TotalCost += item.TotalCost;

                                            _dinnerTotal.Qty += item.Quantity;
                                            _dinnerTotal.ItemTotal += item.ItemTotal;
                                            _dinnerTotal.Percent += item.Percent;
                                            _dinnerTotal.Discount += item.Discount;
                                            _dinnerTotal.Promotion += item.PromotionAmount;
                                            _dinnerTotal.UnitCost += item.Cost;
                                            _dinnerTotal.TotalCost += item.TotalCost;

                                            // If CreatedDate is avaible with time of period
                                            lstDataByPeriod.Add(item);
                                        }
                                    }
                                }

                            }//End lstItemTypes

                            // Get total item
                            _itemTotal.SCTotal += lstDataByPeriod.Sum(s => s.ServiceCharge);
                            _itemTotal.ItemTotal += lstDataByPeriod.Sum(s => s.ItemTotal);
                            _itemTotal.TaxTotal += lstDataByPeriod.Sum(s => s.Tax);
                            _itemTotal.DiscountTotal += lstDataByPeriod.Sum(s => s.Discount);
                            _itemTotal.PromotionTotal += lstDataByPeriod.Sum(s => s.PromotionAmount);

                            // Add items to lst item for NetSales
                            lstItemNoIncludeSaleInStore.AddRange(lstDataByPeriod.Where(w => w.IsIncludeSale == false).ToList());

                            //Group Item
                            var lstItemTypeGroup = lstDataByPeriod.GroupBy(gg => new { gg.ItemTypeId, gg.ItemId, gg.Price }).ToList();
                            double cp = 0; double cost = 0;
                            foreach (var item in lstItemTypeGroup)
                            {
                                ws.Cell("A" + index).Value = item.Select(ss => ss.ItemCode).FirstOrDefault();
                                ws.Cell("B" + index).Value = item.Select(ss => ss.ItemName).FirstOrDefault();
                                ws.Cell("C" + index).Value = item.Select(ss => ss.Price).FirstOrDefault().ToString("F");
                                ws.Cell("C" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Cell("D" + index).Value = item.Sum(d => d.Quantity);
                                ws.Cell("E" + index).Value = item.Sum(d => d.ItemTotal).ToString("F");
                                ws.Cell("F" + index).Value = item.Sum(d => d.Percent).ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-item.Sum(d => d.Discount)).ToString("F");

                                ws.Cell("H" + index).Value = (-item.Sum(d => d.PromotionAmount)).ToString("F");
                                ws.Cell("I" + index).Value = item.Select(ss => ss.Cost).FirstOrDefault().ToString("F");
                                cost += item.Select(ss => ss.Cost).FirstOrDefault();
                                ws.Cell("J" + index).Value = item.Sum(d => d.TotalCost).ToString("F");

                                if (item.Sum(d => d.TotalCost) == 0 || item.Sum(d => d.ItemTotal) == 0)
                                    cp = 0;
                                else
                                    cp = ((item.Sum(d => d.TotalCost) / item.Sum(d => d.ItemTotal)) * 100);

                                ws.Cell("K" + index).Value = cp.ToString("F") + " %";
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;
                            }//end lstItemTypeGroup

                            subTotalQty = breakfast.Qty + lunch.Qty + dinner.Qty;
                            subTotalItem = breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal;
                            subTotalPercent = breakfast.Percent + lunch.Percent + dinner.Percent;
                            subTotalDiscount = breakfast.Discount + lunch.Discount + dinner.Discount;
                            subTotalPromotion = breakfast.Promotion + lunch.Promotion + dinner.Promotion;
                            subTotalUnitCost = cost;
                            subTotalTotalCost = breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost;
                            if ((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) != 0 && (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal) != 0)
                            {
                                subTotalCP = (((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) / (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal)) * 100);
                            }

                            breakfastTotalQty = breakfast.Qty;
                            breakfastTotalItem = breakfast.ItemTotal;
                            breakfastTotalPercent = breakfast.Percent;
                            breakfastTotalDiscount = breakfast.Discount;
                            breakfastTotalPromotion = breakfast.Promotion;
                            breakfastTotalUnitCost = breakfast.UnitCost;
                            breakfastTotalTotalCost = breakfast.TotalCost;
                            if (breakfast.TotalCost != 0 && breakfast.ItemTotal != 0)
                            {
                                breakfastTotalCP = ((breakfast.TotalCost / breakfast.ItemTotal) * 100);
                            }

                            lunchTotalQty = lunch.Qty;
                            lunchTotalItem = lunch.ItemTotal;
                            lunchTotalPercent = lunch.Percent;
                            lunchTotalDiscount = lunch.Discount;
                            lunchTotalPromotion = lunch.Promotion;
                            lunchTotalUnitCost = lunch.UnitCost;
                            lunchTotalTotalCost = lunch.TotalCost;
                            lunchTotalCP = 0;
                            if (lunch.TotalCost != 0 && lunch.ItemTotal != 0)
                            {
                                lunchTotalCP = ((lunch.TotalCost / lunch.ItemTotal) * 100);
                            }

                            dinnerTotalQty = dinner.Qty;
                            dinnerTotalItem = dinner.ItemTotal;
                            dinnerTotalPercent = dinner.Percent;
                            dinnerTotalDiscount = dinner.Discount;
                            dinnerTotalPromotion = dinner.Promotion;
                            dinnerTotalUnitCost = dinner.UnitCost;
                            dinnerTotalTotalCost = dinner.TotalCost;
                            if (dinner.TotalCost != 0 && dinner.ItemTotal != 0)
                            {
                                dinnerTotalCP = ((dinner.TotalCost / dinner.ItemTotal) * 100);
                            }

                            // Get total information of current cate
                            var totalDataCurrentCate = lstTotalDataInStore.Where(w => w.CateId == itemCate.Id).FirstOrDefault();
                            // Update total data of current cate
                            totalDataCurrentCate.SubTotalQty += subTotalQty;
                            totalDataCurrentCate.SubTotalItem += subTotalItem;
                            totalDataCurrentCate.SubTotalPercent += subTotalPercent;
                            totalDataCurrentCate.SubTotalDiscount += subTotalDiscount;
                            totalDataCurrentCate.SubTotalPromotion += subTotalPromotion;
                            totalDataCurrentCate.SubTotalUnitCost += subTotalUnitCost;
                            totalDataCurrentCate.SubTotalTotalCost += subTotalTotalCost;
                            totalDataCurrentCate.SubTotalCP += subTotalCP;

                            totalDataCurrentCate.BreakfastTotalQty += breakfastTotalQty;
                            totalDataCurrentCate.BreakfastTotalItem += breakfastTotalItem;
                            totalDataCurrentCate.BreakfastTotalPercent += breakfastTotalPercent;
                            totalDataCurrentCate.BreakfastTotalDiscount += breakfastTotalDiscount;
                            totalDataCurrentCate.BreakfastTotalPromotion += breakfastTotalPromotion;
                            totalDataCurrentCate.BreakfastTotalUnitCost += breakfastTotalUnitCost;
                            totalDataCurrentCate.BreakfastTotalTotalCost += breakfastTotalTotalCost;
                            totalDataCurrentCate.BreakfastTotalCP += breakfastTotalCP;

                            totalDataCurrentCate.LunchTotalQty += lunchTotalQty;
                            totalDataCurrentCate.LunchTotalItem += lunchTotalItem;
                            totalDataCurrentCate.LunchTotalPercent += lunchTotalPercent;
                            totalDataCurrentCate.LunchTotalDiscount += lunchTotalDiscount;
                            totalDataCurrentCate.LunchTotalPromotion += lunchTotalPromotion;
                            totalDataCurrentCate.LunchTotalUnitCost += lunchTotalUnitCost;
                            totalDataCurrentCate.LunchTotalTotalCost += lunchTotalTotalCost;
                            totalDataCurrentCate.LunchTotalCP += lunchTotalCP;

                            totalDataCurrentCate.DinnerTotalQty += dinnerTotalQty;
                            totalDataCurrentCate.DinnerTotalItem += dinnerTotalItem;
                            totalDataCurrentCate.DinnerTotalPercent += dinnerTotalPercent;
                            totalDataCurrentCate.DinnerTotalDiscount += dinnerTotalDiscount;
                            totalDataCurrentCate.DinnerTotalPromotion += dinnerTotalPromotion;
                            totalDataCurrentCate.DinnerTotalUnitCost += dinnerTotalUnitCost;
                            totalDataCurrentCate.DinnerTotalTotalCost += dinnerTotalTotalCost;
                            totalDataCurrentCate.DinnerTotalCP += dinnerTotalCP;

                            // Show total date of current
                            // If it has any cate child which was checked  => title: Sub-Total: CateName
                            // If it doesn't have any cate child which was checked && it doesn't has parent cate which was checked => it's top cate => title: Category Total: CateName
                            if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && !lstTotalDataInStore.Where(w => !string.IsNullOrEmpty(totalDataCurrentCate.ParentId) && w.CateId == totalDataCurrentCate.ParentId && w.Checked).Any())
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Total") + " : " + totalDataCurrentCate.CateName);
                            }
                            else
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total") + " : " + totalDataCurrentCate.CateName);
                            }

                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            ws.Cell("D" + index).Value = totalDataCurrentCate.SubTotalQty;
                            ws.Cell("E" + index).Value = totalDataCurrentCate.SubTotalItem.ToString("F");
                            ws.Cell("F" + index).Value = totalDataCurrentCate.SubTotalPercent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-totalDataCurrentCate.SubTotalDiscount).ToString("F");
                            ws.Cell("H" + index).Value = (-totalDataCurrentCate.SubTotalPromotion).ToString("F");
                            ws.Cell("I" + index).Value = totalDataCurrentCate.SubTotalUnitCost.ToString("F");
                            ws.Cell("J" + index).Value = totalDataCurrentCate.SubTotalTotalCost.ToString("F");
                            ws.Cell("K" + index).Value = totalDataCurrentCate.SubTotalCP.ToString("F") + " %";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            index++;

                            //Morning
                            if (viewmodel.Breakfast)
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Cell("D" + index).Value = totalDataCurrentCate.BreakfastTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.BreakfastTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.BreakfastTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.BreakfastTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.BreakfastTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.BreakfastTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.BreakfastTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.BreakfastTotalCP.ToString("F") + " %";
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                index++;
                            }

                            //Afternoon
                            if (viewmodel.Lunch)
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Cell("D" + index).Value = totalDataCurrentCate.LunchTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.LunchTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.LunchTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.LunchTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.LunchTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.LunchTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.LunchTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.LunchTotalCP.ToString("F") + " %";
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                index++;
                            }

                            //dinner
                            if (viewmodel.Dinner)
                            {
                                ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Cell("D" + index).Value = totalDataCurrentCate.DinnerTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.DinnerTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.DinnerTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.DinnerTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.DinnerTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.DinnerTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.DinnerTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.DinnerTotalCP.ToString("F") + " %";
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                index++;
                            }

                            // Check current cate is last chile of parent current cate
                            Boolean isLastChildCate = false;
                            // If current cate has parent cate, check for show total data of parent cate
                            if (!string.IsNullOrEmpty(totalDataCurrentCate.ParentId))
                            {
                                var parentCurrentCateInfo = lstTotalDataInStore.Where(w => w.CateId == totalDataCurrentCate.ParentId).FirstOrDefault();
                                // List cate child
                                var lstCateChildCheckedOfParent = parentCurrentCateInfo.ListCateChildChecked.ToList();
                                if (lstCateChildCheckedOfParent != null && lstCateChildCheckedOfParent.Any())
                                {
                                    // Get id of last child cate
                                    string idLastCateChild = parentCurrentCateInfo.ListCateChildChecked.LastOrDefault().ToString();

                                    // Current cate is last cate of parent cate
                                    if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && idLastCateChild == totalDataCurrentCate.CateId)
                                    {
                                        isLastChildCate = true;
                                    }
                                }

                                DisplayTotalOfParentCate(ref ws, totalDataCurrentCate, ref index, ref lstTotalDataInStore, isLastChildCate, totalDataCurrentCate, viewmodel);
                            }
                        }//End Group by category
                    }//end lstGroupItemType

                    // MISC
                    int startM = index;
                    listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();

                    if (listMiscDisPeriod != null && listMiscDisPeriod.Any())
                    {
                        listMiscDisPeriodByStore = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                        if (listMiscDisPeriodByStore != null && listMiscDisPeriodByStore.Any())
                        {
                            // MISC
                            ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                            index++;
                            for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                            {
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                ws.Cell("E" + index).SetValue(listMiscDisPeriodByStore[m].MiscTotal.ToString("F"));
                                listMiscDisPeriodByStore[m].Percent = ((listMiscDisPeriodByStore[m].MiscTotal / (itemAmountTotal + miscInstore)) * 100);
                                ws.Cell("F" + index).SetValue(listMiscDisPeriodByStore[m].Percent.ToString("F") + " %");
                                index++;

                                _itemTotal.ItemTotal += listMiscDisPeriodByStore[m].MiscTotal;
                            }

                            // Discount Total Bill
                            ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            index++;
                            for (int m = 0; m < listMiscDisPeriodByStore.Count(); m++)
                            {
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(listMiscDisPeriodByStore[m].Period));
                                ws.Cell("G" + index).SetValue((-listMiscDisPeriodByStore[m].BillDiscountTotal).ToString("F"));
                                subDisPeriod += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                subDisPeriodByStore += listMiscDisPeriodByStore[m].BillDiscountTotal;
                                _itemTotal.DiscountTotal += listMiscDisPeriodByStore[m].BillDiscountTotal;

                                index++;

                            }
                        }
                    }

                    ws.Range("A" + startM + ":K" + index).Style.Font.SetBold(true);
                    ws.Range("A" + startM + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + startM + ":D" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range("E" + startM + ":G" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Outlet Sub-total
                    //_subletTotal
                    double cpOutlet = 0;
                    var miscPercent = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).ToList();
                    var miscBreafast = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.BREAKFAST).FirstOrDefault();
                    var miscLunch = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.LUNCH).FirstOrDefault();
                    var miscDinner = listMiscDisPeriod.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId && ww.Period == Commons.DINNER).FirstOrDefault();

                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Outlet Sub-total"));
                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                    ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                    ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                        + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                    ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                    ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                    ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                    if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                        || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                        cpOutlet = 0;
                    else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                            / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                    ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                    ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                    ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    // TOTAL
                    if (listMiscDisPeriodByStore == null)
                        listMiscDisPeriodByStore = new List<MISCBillDiscountPeriodModels>();
                    index = index - 1;
                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                    ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("D" + index).Value = (_breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty);
                    ws.Cell("E" + index).Value = ((_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)
                                + miscPercent.Sum(m => m.MiscTotal)).ToString("F");
                    ws.Cell("F" + index).Value = ((_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent)
                        + miscPercent.Sum(m => m.Percent)).ToString("F") + " %";
                    ws.Cell("G" + index).Value = (-(_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) - subDisPeriodByStore).ToString("F");
                    ws.Cell("H" + index).Value = (-(_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion)).ToString("F");
                    ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost).ToString("F");
                    ws.Cell("J" + index).Value = (_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost).ToString("F");

                    if ((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost) == 0
                    || (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal) == 0)
                        cpOutlet = 0;
                    else cpOutlet = (((_breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost)
                            / (_breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal)) * 100);

                    ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                    ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + index + ":J" + index).Style.Font.SetBold(true);
                    ws.Range("A" + index + ":J" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    index++;

                    var discountMonring = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.BREAKFAST && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                    var discountLunch = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.LUNCH && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();
                    var discountDinner = listMiscDisPeriodByStore.Where(ww => ww.Period == Commons.DINNER && ww.StoreId == itemGroupStore.Key.StoreId).FirstOrDefault();

                    //Morning
                    if (viewmodel.Breakfast)
                    {
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = _breakfastOutletTotal.Qty;
                        ws.Cell("E" + index).Value = (_breakfastOutletTotal.ItemTotal + (miscBreafast != null ? miscBreafast.MiscTotal : 0)).ToString("F");
                        ws.Cell("F" + index).Value = (_breakfastOutletTotal.Percent + (miscBreafast != null ? miscBreafast.Percent : 0)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-_breakfastOutletTotal.Discount - (discountMonring != null ? discountMonring.BillDiscountTotal : 0)).ToString("F");
                        ws.Cell("H" + index).Value = (-_breakfastOutletTotal.Promotion).ToString("F");
                        ws.Cell("I" + index).Value = _breakfastOutletTotal.UnitCost.ToString("F");
                        ws.Cell("J" + index).Value = _breakfastOutletTotal.TotalCost.ToString("F");
                        if (_breakfastOutletTotal.TotalCost == 0 || _breakfastOutletTotal.ItemTotal == 0)
                            _breakfastOutletTotal.CP = 0;
                        else
                            _breakfastOutletTotal.CP = ((_breakfastOutletTotal.TotalCost / _breakfastOutletTotal.ItemTotal) * 100);
                        ws.Cell("K" + index).Value = _breakfastOutletTotal.CP.ToString("F") + " %";
                        ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;
                    }

                    //Afternoon
                    if (viewmodel.Lunch)
                    {
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = _lunchOutletTotal.Qty;
                        ws.Cell("E" + index).Value = (_lunchOutletTotal.ItemTotal + (miscLunch != null ? miscLunch.MiscTotal : 0)).ToString("F");
                        ws.Cell("F" + index).Value = (_lunchOutletTotal.Percent + (miscLunch != null ? miscLunch.Percent : 0)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-_lunchOutletTotal.Discount - (discountLunch != null ? discountLunch.BillDiscountTotal : 0)).ToString("F");
                        ws.Cell("H" + index).Value = (-_lunchOutletTotal.Promotion).ToString("F");
                        ws.Cell("I" + index).Value = _lunchOutletTotal.UnitCost.ToString("F");
                        ws.Cell("J" + index).Value = _lunchOutletTotal.TotalCost.ToString("F");
                        if (_lunchOutletTotal.TotalCost == 0 || _lunchOutletTotal.ItemTotal == 0)
                            _lunchOutletTotal.CP = 0;
                        else
                            _lunchOutletTotal.CP = ((_lunchOutletTotal.TotalCost / _lunchOutletTotal.ItemTotal) * 100);
                        ws.Cell("K" + index).Value = _lunchOutletTotal.CP.ToString("F") + " %";
                        ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;
                    }

                    //dinner
                    if (viewmodel.Dinner)
                    {
                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = _dinnerOutletTotal.Qty;
                        ws.Cell("E" + index).Value = (_dinnerOutletTotal.ItemTotal + (miscDinner != null ? miscDinner.MiscTotal : 0)).ToString("F");
                        ws.Cell("F" + index).Value = (_dinnerOutletTotal.Percent + (miscDinner != null ? miscDinner.Percent : 0)).ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-_dinnerOutletTotal.Discount - (discountDinner != null ? discountDinner.BillDiscountTotal : 0)).ToString("F");
                        ws.Cell("H" + index).Value = (-_dinnerOutletTotal.Promotion).ToString("F");
                        ws.Cell("I" + index).Value = _dinnerOutletTotal.UnitCost.ToString("F");
                        ws.Cell("J" + index).Value = _dinnerOutletTotal.TotalCost.ToString("F");
                        if (_dinnerOutletTotal.TotalCost == 0 || _dinnerOutletTotal.ItemTotal == 0)
                            _dinnerOutletTotal.CP = 0;
                        else
                            _dinnerOutletTotal.CP = ((_dinnerOutletTotal.TotalCost / _dinnerOutletTotal.ItemTotal) * 100);
                        ws.Cell("K" + index).Value = _dinnerOutletTotal.CP.ToString("F") + " %";
                        ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;
                    }

                    ws.Range("C" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //format file
                    //header
                    ws.Range("A4:K6").Style.Font.SetBold(true);
                    //Set color
                    ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //set Border        
                    ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    if (_firstStore)
                    {
                        ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    else
                    {
                        ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    index++;

                    // end total

                    // Summary
                    //double refund = 0;
                    bool isTaxInclude = false;
                    ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                    ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                    if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                    {
                        ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                    }
                    else
                    {
                        ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                        isTaxInclude = true;
                    }
                    ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                    ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                    ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    index++;

                    _noincludeSale = 0;
                    if (lstItemNoIncludeSaleInStore != null && lstItemNoIncludeSaleInStore.Any())
                    {
                        if (isTaxInclude)
                        {
                            _noincludeSale = lstItemNoIncludeSaleInStore.Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount - (decimal)ss.Tax));
                        }
                        else
                        {
                            _noincludeSale = lstItemNoIncludeSaleInStore.Sum(ss => (double)((decimal)ss.TotalAmount - (decimal)ss.TotalDiscount - (decimal)ss.PromotionAmount));
                        }
                    }

                    //GC value
                    payGC = 0;
                    if (lstPayments != null && lstPayments.Any())
                    {
                        var lstPaymentsInStore = lstPayments.Where(p => p.StoreId == itemGroupStore.Key.StoreId).ToList();
                        List<PaymentModels> lstPaymentByPeriod = new List<PaymentModels>();
                        foreach (var item in lstPaymentsInStore)
                        {
                            timeDish = item.CreatedDate.TimeOfDay;
                            // Breakfast
                            if (viewmodel.Breakfast)
                            {
                                if (timeDish >= brearkStart && timeDish < brearkEnd)
                                {
                                    lstPaymentByPeriod.Add(item);
                                }
                            }

                            // Lunch
                            if (viewmodel.Lunch)
                            {
                                if (timeDish >= lunchStart && timeDish < lunchEnd)
                                {
                                    lstPaymentByPeriod.Add(item);
                                }
                            }

                            // Dinner
                            if (viewmodel.Dinner)
                            {
                                if (dinnerStart > dinnerEnd) //pass day
                                {
                                    if (timeDish >= dinnerStart || timeDish < dinnerEnd)
                                    {
                                        lstPaymentByPeriod.Add(item);
                                    }
                                }
                                else //in day
                                {
                                    if (timeDish >= dinnerStart && timeDish < dinnerEnd)
                                    {
                                        lstPaymentByPeriod.Add(item);
                                    }
                                }
                            }
                        }
                        payGC = lstPaymentByPeriod.Sum(p => (double)p.Amount);
                    }

                    //sell GC 22/09/2017
                    //sell GC 22/09/2017
                    double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                            && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                            && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.TotalAmount.Value);

                    //rounding
                    var roudingAmount = _lstRoundings.Where(ww => ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.Rounding);
                    //netsale
                    ws.Cell("A" + index).Value = (_itemTotal.ItemTotal - subDisPeriod - (isTaxInclude ? _itemTotal.TaxTotal : 0)
                        - _noincludeSale - payGC + giftCardSell + roudingAmount).ToString("F");

                    ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                    ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                    ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                    ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                    ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                    ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    index += 2;
                    indexNextStore = index;
                    _firstStore = false;
                }//end foreach (var itemGroupStore in lstItemGroupByStore)
                if (lstItemGroupByStore == null || lstItemGroupByStore.Count() == 0)
                {
                    //check only sale GC
                    var lstItemGroupByStoreWithGC = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId))
                       .GroupBy(gg => new { StoreId = gg.StoreId, StoreName = gg.StoreName }).OrderBy(x => x.Key.StoreName);
                    if (lstItemGroupByStoreWithGC != null && lstItemGroupByStoreWithGC.Any())
                    {
                        //header
                        ws.Range("A4:K6").Style.Font.SetBold(true);
                        //Set color
                        ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //set Border        
                        ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                        foreach (var itemGroupStore in lstItemGroupByStoreWithGC)
                        {
                            if (_firstStore)
                            {
                                ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            }
                            else
                            {
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }
                            // Store name
                            var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                            ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                            if (GetTaxType(itemGroupStore.Key.StoreId) == (int)Commons.ETax.AddOn)
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                            }
                            else
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                            }
                            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            index++;

                            double giftCardSell = lstData.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                 && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                                 && ww.StoreId == itemGroupStore.Key.StoreId).Sum(ss => ss.TotalAmount.Value);

                            ws.Cell("A" + index).Value = (giftCardSell).ToString("F");
                            ws.Cell("B" + index).Value = _itemTotal.SCTotal.ToString("F");
                            ws.Cell("C" + index).Value = _itemTotal.TaxTotal.ToString("F");
                            ws.Cell("D" + index).Value = (-_itemTotal.DiscountTotal).ToString("F");
                            ws.Cell("E" + index).Value = (-_itemTotal.PromotionTotal).ToString("F");

                            ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            index += 2;
                            indexNextStore = index;
                            _firstStore = false;
                        }
                    }

                }

            }

            ws.Columns(1, 3).AdjustToContents();
            //set Width for Colum 
            ws.Column(4).Width = 20;
            ws.Columns(5, 11).AdjustToContents();
            return wb;
        }

        public void DisplayTotalOfParentCate(ref IXLWorksheet ws, RFilterCategoryV1ReportModel totalDataCurrentCate, ref int index, ref List<RFilterCategoryV1ReportModel> lstTotalDataInStore, Boolean isLastChildCate, RFilterCategoryV1ReportModel totalData, ItemizedSalesAnalysisReportModel viewmodel)
        {
            // Update total value for parent of current cate
            var totalDataParentOfCurrentCate = lstTotalDataInStore.Where(w => w.CateId == totalDataCurrentCate.ParentId).FirstOrDefault();

            totalDataParentOfCurrentCate.SubTotalQty += totalData.SubTotalQty;
            totalDataParentOfCurrentCate.SubTotalItem += totalData.SubTotalItem;
            totalDataParentOfCurrentCate.SubTotalPercent += totalData.SubTotalPercent;
            totalDataParentOfCurrentCate.SubTotalDiscount += totalData.SubTotalDiscount;
            totalDataParentOfCurrentCate.SubTotalPromotion += totalData.SubTotalPromotion;
            totalDataParentOfCurrentCate.SubTotalUnitCost += totalData.SubTotalUnitCost;
            totalDataParentOfCurrentCate.SubTotalTotalCost += totalData.SubTotalTotalCost;
            totalDataParentOfCurrentCate.SubTotalCP += totalData.SubTotalCP;

            totalDataParentOfCurrentCate.BreakfastTotalQty += totalData.BreakfastTotalQty;
            totalDataParentOfCurrentCate.BreakfastTotalItem += totalData.BreakfastTotalItem;
            totalDataParentOfCurrentCate.BreakfastTotalPercent += totalData.BreakfastTotalPercent;
            totalDataParentOfCurrentCate.BreakfastTotalDiscount += totalData.BreakfastTotalDiscount;
            totalDataParentOfCurrentCate.BreakfastTotalPromotion += totalData.BreakfastTotalPromotion;
            totalDataParentOfCurrentCate.BreakfastTotalUnitCost += totalData.BreakfastTotalUnitCost;
            totalDataParentOfCurrentCate.BreakfastTotalTotalCost += totalData.BreakfastTotalTotalCost;
            totalDataParentOfCurrentCate.BreakfastTotalCP += totalData.BreakfastTotalCP;

            totalDataParentOfCurrentCate.LunchTotalQty += totalData.LunchTotalQty;
            totalDataParentOfCurrentCate.LunchTotalItem += totalData.LunchTotalItem;
            totalDataParentOfCurrentCate.LunchTotalPercent += totalData.LunchTotalPercent;
            totalDataParentOfCurrentCate.LunchTotalDiscount += totalData.LunchTotalDiscount;
            totalDataParentOfCurrentCate.LunchTotalPromotion += totalData.LunchTotalPromotion;
            totalDataParentOfCurrentCate.LunchTotalUnitCost += totalData.LunchTotalUnitCost;
            totalDataParentOfCurrentCate.LunchTotalTotalCost += totalData.LunchTotalTotalCost;
            totalDataParentOfCurrentCate.LunchTotalCP += totalData.LunchTotalCP;

            totalDataParentOfCurrentCate.DinnerTotalQty += totalData.DinnerTotalQty;
            totalDataParentOfCurrentCate.DinnerTotalItem += totalData.DinnerTotalItem;
            totalDataParentOfCurrentCate.DinnerTotalPercent += totalData.DinnerTotalPercent;
            totalDataParentOfCurrentCate.DinnerTotalDiscount += totalData.DinnerTotalDiscount;
            totalDataParentOfCurrentCate.DinnerTotalPromotion += totalData.DinnerTotalPromotion;
            totalDataParentOfCurrentCate.DinnerTotalUnitCost += totalData.DinnerTotalUnitCost;
            totalDataParentOfCurrentCate.DinnerTotalTotalCost += totalData.DinnerTotalTotalCost;
            totalDataParentOfCurrentCate.DinnerTotalCP += totalData.DinnerTotalCP;

            // If cate is checked
            if (totalDataParentOfCurrentCate.Checked && isLastChildCate)
            {
                // List child cate
                var lstCateChildChecked = totalDataParentOfCurrentCate.ListCateChildChecked.ToList();

                if (lstCateChildChecked != null && lstCateChildChecked.Any())
                {
                    // Get id of last child cate
                    string idLastCateChildOfParent = lstCateChildChecked.LastOrDefault().ToString();

                    // Current cate is last cate of parent cate => show total data of parent cate
                    if (idLastCateChildOfParent == totalDataCurrentCate.CateId)
                    {
                        // Show total date of current
                        // If it has parent cate which was checked => title: Sub-Category Total : CateName
                        // If it doesn't have parent cate || If it doesn't have parent cate which was checked => it's top cate => title: Category Total: CateName
                        if (string.IsNullOrEmpty(totalDataParentOfCurrentCate.ParentId) || !lstTotalDataInStore.Where(w => w.CateId == totalDataParentOfCurrentCate.ParentId && w.Checked).Any())
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Total") + " : " + totalDataParentOfCurrentCate.CateName);
                        }
                        else
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-Category Total") + " : " + totalDataParentOfCurrentCate.CateName);
                        }

                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell("D" + index).Value = totalDataParentOfCurrentCate.SubTotalQty;
                        ws.Cell("E" + index).Value = totalDataParentOfCurrentCate.SubTotalItem.ToString("F");
                        ws.Cell("F" + index).Value = totalDataParentOfCurrentCate.SubTotalPercent.ToString("F") + " %";
                        ws.Cell("G" + index).Value = (-totalDataParentOfCurrentCate.SubTotalDiscount).ToString("F");
                        ws.Cell("H" + index).Value = (-totalDataParentOfCurrentCate.SubTotalPromotion).ToString("F");
                        ws.Cell("I" + index).Value = totalDataParentOfCurrentCate.SubTotalUnitCost.ToString("F");
                        ws.Cell("J" + index).Value = totalDataParentOfCurrentCate.SubTotalTotalCost.ToString("F");
                        ws.Cell("K" + index).Value = totalDataParentOfCurrentCate.SubTotalCP.ToString("F") + " %";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                        index++;

                        //Morning
                        if (viewmodel.Breakfast)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = totalDataParentOfCurrentCate.BreakfastTotalQty;
                            ws.Cell("E" + index).Value = totalDataParentOfCurrentCate.BreakfastTotalItem.ToString("F");
                            ws.Cell("F" + index).Value = totalDataParentOfCurrentCate.BreakfastTotalPercent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-totalDataParentOfCurrentCate.BreakfastTotalDiscount).ToString("F");
                            ws.Cell("H" + index).Value = (-totalDataParentOfCurrentCate.BreakfastTotalPromotion).ToString("F");
                            ws.Cell("I" + index).Value = totalDataParentOfCurrentCate.BreakfastTotalUnitCost.ToString("F");
                            ws.Cell("J" + index).Value = totalDataParentOfCurrentCate.BreakfastTotalTotalCost.ToString("F");
                            ws.Cell("K" + index).Value = totalDataParentOfCurrentCate.BreakfastTotalCP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        //Afternoon
                        if (viewmodel.Lunch)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = totalDataParentOfCurrentCate.LunchTotalQty;
                            ws.Cell("E" + index).Value = totalDataParentOfCurrentCate.LunchTotalItem.ToString("F");
                            ws.Cell("F" + index).Value = totalDataParentOfCurrentCate.LunchTotalPercent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-totalDataParentOfCurrentCate.LunchTotalDiscount).ToString("F");
                            ws.Cell("H" + index).Value = (-totalDataParentOfCurrentCate.LunchTotalPromotion).ToString("F");
                            ws.Cell("I" + index).Value = totalDataParentOfCurrentCate.LunchTotalUnitCost.ToString("F");
                            ws.Cell("J" + index).Value = totalDataParentOfCurrentCate.LunchTotalTotalCost.ToString("F");
                            ws.Cell("K" + index).Value = totalDataParentOfCurrentCate.LunchTotalCP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        // Dinner
                        if (viewmodel.Dinner)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = totalDataParentOfCurrentCate.DinnerTotalQty;
                            ws.Cell("E" + index).Value = totalDataParentOfCurrentCate.DinnerTotalItem.ToString("F");
                            ws.Cell("F" + index).Value = totalDataParentOfCurrentCate.DinnerTotalPercent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = (-totalDataParentOfCurrentCate.DinnerTotalDiscount).ToString("F");
                            ws.Cell("H" + index).Value = (-totalDataParentOfCurrentCate.DinnerTotalPromotion).ToString("F");
                            ws.Cell("I" + index).Value = totalDataParentOfCurrentCate.DinnerTotalUnitCost.ToString("F");
                            ws.Cell("J" + index).Value = totalDataParentOfCurrentCate.DinnerTotalTotalCost.ToString("F");
                            ws.Cell("K" + index).Value = totalDataParentOfCurrentCate.DinnerTotalCP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(totalDataParentOfCurrentCate.ParentId))
            {
                DisplayTotalOfParentCate(ref ws, totalDataParentOfCurrentCate, ref index, ref lstTotalDataInStore, isLastChildCate, totalData, viewmodel);
            }
        }

        public List<ItemizedSalesAnalysisReportModels> GetListMiscForDailyReceipt(BaseReportModel model)
        {
            var lstData = new List<ItemizedSalesAnalysisReportModels>();
            using (var cxt = new NuWebContext())
            {
                lstData = (from tb in cxt.R_ItemizedSalesAnalysisReport
                           where model.ListStores.Contains(tb.StoreId)
                                && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate) && tb.Mode == model.Mode
                                && (tb.ItemTypeId == (int)Commons.EProductType.Misc)

                           select new ItemizedSalesAnalysisReportModels
                           {
                               StoreId = tb.StoreId,
                               CreatedDate = tb.CreatedDate,
                               ReceiptId = tb.ReceiptId,
                               BusinessId = tb.BusinessId,
                               TotalPrice = tb.TotalPrice
                           }).ToList();
                return lstData;
            }
        }

        //18/10/2017
        //public List<HourlyItemizedSalesReportModels> GetDataHour(RPHourlyItemizedSalesModels model, List<string> listCategoryIds, List<string> listSetMenuIds)
        //{
        //    using (var cxt = new NuWebContext())
        //    {
        //        var lstData = (from tb in cxt.R_HourlyItemizedSalesReport
        //                       where model.ListStores.Contains(tb.StoreId)
        //                             && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
        //                             && (listCategoryIds.Contains(tb.CategoryId) || (tb.ItemTypeId == (int)Commons.EProductType.SetMenu && listSetMenuIds.Contains(tb.ItemId)))
        //                       orderby tb.CreatedDate, tb.ItemTypeId, tb.CategoryName
        //                       group tb by new
        //                       {
        //                           StoreId = tb.StoreId,
        //                           ItemTypeId = tb.ItemTypeId,
        //                           Hour = (int?)SqlFunctions.DatePart("hh", tb.CreatedDate),
        //                           CategoryId = tb.CategoryId,
        //                           CategoryName = tb.CategoryName,
        //                           CreatedDate = DbFunctions.TruncateTime(tb.CreatedDate),
        //                           BusinessId = tb.BusinessId,
        //                           IsDiscountTotal = tb.IsDiscountTotal
        //                       } into g
        //                       where g.Key.Hour != null
        //                       select new HourlyItemizedSalesReportModels
        //                       {
        //                           StoreId = g.Key.StoreId,
        //                           ItemTypeId = g.Key.ItemTypeId,
        //                           Hour = g.Key.Hour.Value,
        //                           CreatedDate = g.Key.CreatedDate.Value,
        //                           CategoryId = g.Key.CategoryId,
        //                           CategoryName = g.Key.CategoryName,
        //                           BusinessId = g.Key.BusinessId,
        //                           TotalPrice = g.Sum(x => x.TotalPrice),
        //                           Discount = g.Sum(x => x.Discount),
        //                           Promotion = g.Sum(x => x.Promotion),
        //                           IsDiscountTotal = g.Key.IsDiscountTotal.HasValue ? g.Key.IsDiscountTotal.Value : false
        //                       }).ToList();
        //        return lstData;
        //    }
        //}

        public List<ItemizedSalesAnalysisReportModels> GetListItemForHourlyItemSale(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_ItemizedSalesAnalysisReport
                               where model.ListStores.Contains(tb.StoreId)
                                    && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate) && tb.Mode == model.Mode

                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   ItemTypeId = tb.ItemTypeId,
                                   Hour = (int?)SqlFunctions.DatePart("hh", tb.CreatedDate),
                                   CategoryId = tb.CategoryId,
                                   CategoryName = tb.CategoryName,
                                   CreatedDate = DbFunctions.TruncateTime(tb.CreatedDate),
                                   BusinessId = tb.BusinessId
                               } into g
                               where g.Key.Hour != null
                               select new ItemizedSalesAnalysisReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   ItemTypeId = g.Key.ItemTypeId,
                                   Hour = g.Key.Hour.Value,
                                   CategoryId = g.Key.CategoryId,
                                   CategoryName = g.Key.CategoryName,
                                   BusinessId = g.Key.BusinessId,
                                   Tax = g.Sum(x => x.Tax),
                               }).ToList();
                return lstData;
            }
        }

        public List<DailySalesReportInsertModels> GetRoundingAmount(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_DailySalesReport.Where(ww => model.ListStores.Contains(ww.StoreId)
                                     && (ww.CreatedDate >= model.FromDate && ww.CreatedDate <= model.ToDate)
                                     && ww.Mode == model.Mode)
                                     .Select(ss => new DailySalesReportInsertModels()
                                     {
                                         StoreId = ss.StoreId,
                                         BusinessId = ss.BusinessId,
                                         CreatedDate = ss.CreatedDate,
                                         Rounding = ss.Rounding,
                                         OrderId = ss.OrderId,
                                         CreditNoteNo = ss.CreditNoteNo,
                                         GST = ss.GST,
                                         ReceiptTotal = ss.ReceiptTotal,
                                         ServiceCharge = ss.ServiceCharge

                                     }).ToList();

                return lstData;
            }
        }

        #region Updated 04042018, get data item no include sale for reports (Receipt & Credit Note from table [R_ItemizedSalesAnalysisReport]) 
        public List<ItemizedSalesAnalysisReportModels> GetItemsNoIncludeSale(List<string> listAllReceiptId, List<string> listStoreId, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                List<ItemizedSalesAnalysisReportModels> lstData = new List<ItemizedSalesAnalysisReportModels>();

                lstData = (from tb in cxt.R_ItemizedSalesAnalysisReport
                           where listStoreId.Contains(tb.StoreId)
                               && listAllReceiptId.Contains(tb.ReceiptId)
                               && tb.Mode == mode
                           //&& (!string.IsNullOrEmpty(tb.GLAccountCode) || tb.IsIncludeSale == false)

                           select new ItemizedSalesAnalysisReportModels
                           {
                               StoreId = tb.StoreId,
                               CreatedDate = tb.CreatedDate,
                               CategoryId = tb.CategoryId,
                               CategoryName = tb.CategoryName,
                               ExtraPrice = tb.ExtraPrice,
                               TotalPrice = tb.TotalPrice,
                               GLAccountCode = tb.GLAccountCode,
                               IsIncludeSale = tb.IsIncludeSale,
                               BusinessId = tb.BusinessId,
                               ServiceCharge = tb.ServiceCharge,
                               Tax = tb.Tax,
                               ExtraAmount = tb.ExtraAmount.HasValue ? tb.ExtraAmount.Value : 0,
                               TotalAmount = tb.TotalAmount.HasValue ? tb.TotalAmount.Value : 0,
                               TotalDiscount = tb.TotalDiscount.HasValue ? tb.TotalDiscount : 0,
                               ItemTotal = tb.TotalPrice + tb.ExtraPrice,
                               PromotionAmount = tb.PromotionAmount,
                               ReceiptId = tb.ReceiptId,
                               TaxType = tb.TaxType,
                               GiftCardId = tb.GiftCardId,
                               PoinsOrderId = tb.PoinsOrderId,
                               ItemTypeId = tb.ItemTypeId
                           }).ToList();

                return lstData;
            }
        }
        #endregion Updated 04042018, get data item no include sale for reports (Receipt & Credit Note) from table [R_ItemizedSalesAnalysisReport]

        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]

        public List<ItemizedSalesAnalysisReportModels> GetData_NewDB(DateTime dFrom, DateTime dTo, List<string> lstStoreIdCate, List<string> lstStoreIdSet, List<string> lstCateIds, List<string> lstCateSetIds, List<string> lstBusDayId, int filterType = 0, int mode = 1)
        {
            using (var cxt = new NuWebContext())
            {
                List<ItemizedSalesAnalysisReportModels> lstReturn = new List<ItemizedSalesAnalysisReportModels>();

                if ((lstStoreIdCate != null && lstStoreIdCate.Any() && lstCateIds != null && lstCateIds.Any())
                    || (lstStoreIdSet != null && lstStoreIdSet.Any() && lstCateSetIds != null && lstCateSetIds.Any()))
                {
                    var data = (from ps in cxt.R_PosSale.Where(ww => ww.ReceiptCreatedDate.HasValue && lstBusDayId.Contains(ww.BusinessId))
                                from psd in cxt.R_PosSaleDetail.Where(w => w.StoreId == ps.StoreId && w.OrderId == ps.OrderId)
                                where ((lstStoreIdSet.Contains(psd.StoreId) || lstStoreIdCate.Contains(psd.StoreId))
                                   && psd.Mode == mode
                                   && psd.ItemTypeId != (int)Commons.EProductType.Misc
                                   )
                                orderby psd.CategoryName, psd.ItemName
                                select new ItemizedSalesAnalysisReportModels
                                {
                                    CreatedDate = ps.ReceiptCreatedDate.Value,
                                    StoreId = ps.StoreId,
                                    CategoryId = psd.CategoryId,
                                    CategoryName = psd.CategoryName,
                                    ItemTypeId = psd.ItemTypeId,
                                    ItemId = psd.ItemId,
                                    ItemName = psd.ItemName,
                                    Quantity = psd.Quantity,
                                    Price = psd.Price,
                                    Discount = psd.Discount,
                                    Tax = psd.Tax,
                                    ServiceCharge = psd.ServiceCharge,
                                    Cost = psd.Cost,
                                    ItemTotal = psd.TotalAmount - (psd.IsDiscountTotal.HasValue && psd.IsDiscountTotal.Value ? 0 : psd.Discount) - psd.PromotionAmount + psd.ExtraPrice,
                                    Percent = 0,
                                    TotalCost = psd.Cost * (double)psd.Quantity,
                                    ItemCode = psd.ItemCode,
                                    PromotionAmount = psd.PromotionAmount,
                                    IsIncludeSale = psd.IsIncludeSale,
                                    //ExtraAmount = psd.ExtraPrice,
                                    TotalAmount = psd.TotalAmount,
                                    TotalDiscount = psd.Discount,
                                    GiftCardId = psd.GiftCardId,
                                    PoinsOrderId = psd.PoinsOrderId,
                                    ReceiptId = psd.OrderId,
                                    TaxType = psd.TaxType,
                                    IsDiscountTotal = psd.IsDiscountTotal.HasValue && psd.IsDiscountTotal.Value ? true : false,
                                    CreditNoteNo = ps.CreditNoteNo,
                                    ParentId = psd.ParentId,
                                    OrderDetailId = psd.OrderDetailId,
                                    BusinessId = psd.BusinessId
                                }).ToList();
                    if (data != null && data.Any())
                    {
                        // Filter data by time
                        switch (filterType)
                        {
                            case (int)Commons.EFilterType.OnDay:
                                data = data.Where(ww => ww.CreatedDate.TimeOfDay >= dFrom.TimeOfDay && ww.CreatedDate.TimeOfDay <= dTo.TimeOfDay).ToList();
                                if (data == null || !data.Any())
                                {
                                    return lstReturn;
                                }
                                break;
                            case (int)Commons.EFilterType.Days:
                                data = data.Where(ww => ww.CreatedDate.TimeOfDay >= dFrom.TimeOfDay || ww.CreatedDate.TimeOfDay <= dTo.TimeOfDay).ToList();
                                if (data == null || !data.Any())
                                {
                                    return lstReturn;
                                }
                                break;
                        }

                        var lstItem = new List<ItemizedSalesAnalysisReportModels>();

                        // List Dish & SetMenu
                        //// Update new ItemTotal for Dish (+ Modifier) & Set Menu (+ Dish + Modifier)
                        lstItem = data.Where(ww => ((ww.ItemTypeId == (int)Commons.EProductType.Dish && lstCateIds.Contains(ww.CategoryId)
                                                    && lstStoreIdCate.Contains(ww.StoreId)) // Dish
                                                    || (ww.ItemTypeId == (int)Commons.EProductType.SetMenu && lstCateSetIds.Contains(ww.ItemId)
                                                    && lstStoreIdSet.Contains(ww.StoreId))) // SetMenu
                                                    && string.IsNullOrEmpty(ww.ParentId)).ToList();
                        if (lstItem != null && lstItem.Any())
                        {
                            foreach (var parent in lstItem)
                            {
                                var lstChild = data.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == parent.OrderDetailId && ww.StoreId == parent.StoreId).ToList();
                                foreach (var child in lstChild)
                                {
                                    parent.ItemTotal += child.ItemTotal;
                                    parent.Discount += child.Discount;
                                    parent.PromotionAmount += child.PromotionAmount;
                                    parent.Cost += child.Cost;
                                    parent.TotalCost += child.TotalCost;

                                    var lstChild1 = data.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == child.OrderDetailId && ww.StoreId == child.StoreId).ToList();
                                    foreach (var child1 in lstChild1)
                                    {
                                        parent.ItemTotal += child1.ItemTotal;
                                        parent.Discount += child1.Discount;
                                        parent.PromotionAmount += child1.PromotionAmount;
                                        parent.Cost += child1.Cost;
                                        parent.TotalCost += child1.TotalCost;
                                    }
                                }
                            }
                            lstReturn.AddRange(lstItem);
                        }
                        // List Gift Card
                        lstReturn.AddRange(data.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId) && !string.IsNullOrEmpty(ww.PoinsOrderId)).ToList());
                        // List Credit Note
                        lstReturn.AddRange(data.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId) && string.IsNullOrEmpty(ww.PoinsOrderId) && string.IsNullOrEmpty(ww.CategoryId) && string.IsNullOrEmpty(ww.ItemId)).ToList());
                    }
                }
                return lstReturn;
            }
        }

        public XLWorkbook ExportExcel_NewDB(ItemizedSalesAnalysisReportModel model
            , List<StoreModels> lstStores, List<RFilterCategoryV1Model> _lstCateChecked, List<RFilterCategoryModel> _lstSetChecked
            , List<RFilterCategoryV1ReportModel> lstTotalAllCate, List<RFilterCategoryV1ReportModel> lstTotalAllSetMenu, bool isExtend)
        {
            string sheetName = "Itemized_Sales_Analysis_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            //Table
            CreateReportHeaderNew(ws, 11, model.FromDateFilter, model.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Itemized Sales Analysis Report"));
            // Format header report
            ws.Range(1, 1, 4, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);

            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return wb;
            }
            model.FromDate = _lstBusDayAllStore.Min(mm => mm.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(mm => mm.DateTo);

            var lstBusDayIdAllStore = _lstBusDayAllStore.Select(s => s.Id).ToList();
            List<ItemizedSalesAnalysisReportModels> lstData = new List<ItemizedSalesAnalysisReportModels>();

            var _lstStoreCate = _lstCateChecked.Select(s => s.StoreId).Distinct().ToList();
            var _lstStoreSet = _lstSetChecked.Select(s => s.StoreId).Distinct().ToList();

            var _lstCateCheckedId = _lstCateChecked.Select(s => s.Id).Distinct().ToList();
            var _lstSetCheckedId = _lstSetChecked.Select(s => s.Id).Distinct().ToList();

            lstData = GetData_NewDB(model.FromDateFilter, model.ToDateFilter, _lstStoreCate, _lstStoreSet, _lstCateCheckedId, _lstSetCheckedId, lstBusDayIdAllStore, model.FilterType, model.Mode);
            if (lstData == null || !lstData.Any())
            {
                return wb;
            }

            var lstReceiptId = lstData.Where(w => string.IsNullOrEmpty(w.CreditNoteNo)).Select(s => s.ReceiptId).Distinct().ToList(); // Only Receipt 
            var lstOrderId = lstData.Select(s => s.ReceiptId).Distinct().ToList(); // Receipt & CreditNote

            List<RFilterChooseExtBaseModel> lstPaymentMethod = new List<RFilterChooseExtBaseModel>();
            // Filter payment GC
            if (!isExtend)
            {
                lstPaymentMethod = _baseFactory.GetAllPaymentForReport(new Models.Api.CategoryApiRequestModel() { ListStoreIds = model.ListStores });
            }
            else
            {
                //Group by hostUrl
                List<string> lstStoreIdExt = new List<string>();
                var groupByHostUrl = lstStores.GroupBy(gg => gg.HostUrlExtend);
                foreach (var item in groupByHostUrl)
                {
                    lstStoreIdExt = item.Select(ss => ss.Id).ToList();
                    var tmp = _baseFactory.GetAllPaymentForMerchantExtendReport(item.Key, new Models.Api.CategoryApiRequestModel() { ListStoreIds = lstStoreIdExt });
                    lstPaymentMethod.AddRange(tmp);
                }
            }
            var lstGCId = lstPaymentMethod.Where(ww => ww.Code == (int)Commons.EPaymentCode.GiftCard).Select(ss => ss.Id).ToList();
            if (lstGCId == null)
                lstGCId = new List<string>();

            List<PaymentModels> lstPaymentGCs = _orderPaymentMethodFactory.GetDataPaymentItemsByGC(model, lstGCId);
            if (lstPaymentGCs != null && lstPaymentGCs.Any())
            {
                switch (model.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        lstPaymentGCs = lstPaymentGCs.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        lstPaymentGCs = lstPaymentGCs.Where(w => w.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || w.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        break;
                }
            }

            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
            // Get value from setting of common
            TimeSpan brearkStart = TimeSpan.Parse(Commons.BreakfastStart);
            TimeSpan brearkEnd = TimeSpan.Parse(Commons.BreakfastEnd);
            TimeSpan lunchStart = TimeSpan.Parse(Commons.LunchStart);
            TimeSpan lunchEnd = TimeSpan.Parse(Commons.LunchEnd);
            TimeSpan dinnerStart = TimeSpan.Parse(Commons.DinnerStart);
            TimeSpan dinnerEnd = TimeSpan.Parse(Commons.DinnerEnd);

            PosSaleFactory posSaleFactory = new PosSaleFactory();
            List<DiscountAndMiscReportModels> listMisc_Discount = posSaleFactory.GetMiscDiscount(model.ListStores, lstReceiptId, model.Mode); // Only receipt id

            // Get order info: receipt & credit note
            var lstOrderInfo = posSaleFactory.GetRoundingAmount(model.ListStores, lstOrderId, model.Mode);

            // Get list refund by GC
            var lstRefunds = _refundFactory.GetListRefundWithoutDetailsByReceiptId(model.ListStores, lstReceiptId);

            ws.Cell("A5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Cell("B5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
            ws.Cell("C5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Cell("D5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
            ws.Cell("E5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
            ws.Cell("F5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
            ws.Cell("G5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
            ws.Cell("H5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
            ws.Cell("I5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
            ws.Cell("J5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
            ws.Cell("K5").SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

            // Set width column
            ws.Column("A").Width = 20;
            ws.Column("B").Width = 30;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 10;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 10;
            ws.Column("G").Width = 15;
            ws.Column("H").Width = 15;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;
            ws.Column("K").Width = 15;

            int index = 6;
            bool isFirstStore = true;
            int indexNextStore = 0;
            //Total
            ItemizedSalesNewTotal _itemTotal = new ItemizedSalesNewTotal();
            var _breakfastTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerTotal = new ItemizedSalesPeriodValueTotal();

            var _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
            var _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();
            var lstBusinessId = new List<string>();
            bool _firstStore = true;

            List<RFilterCategoryV1ReportModel> lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

            double subTotalQty = 0;
            double subTotalItem = 0;
            double subTotalPercent = 0;
            double subTotalDiscount = 0;
            double subTotalPromotion = 0;
            double subTotalUnitCost = 0;
            double subTotalTotalCost = 0;
            double subTotalCP = 0;

            double breakfastTotalQty = 0;
            double breakfastTotalItem = 0;
            double breakfastTotalPercent = 0;
            double breakfastTotalDiscount = 0;
            double breakfastTotalPromotion = 0;
            double breakfastTotalUnitCost = 0;
            double breakfastTotalTotalCost = 0;
            double breakfastTotalCP = 0;

            double lunchTotalQty = 0;
            double lunchTotalItem = 0;
            double lunchTotalPercent = 0;
            double lunchTotalDiscount = 0;
            double lunchTotalPromotion = 0;
            double lunchTotalUnitCost = 0;
            double lunchTotalTotalCost = 0;
            double lunchTotalCP = 0;

            double dinnerTotalQty = 0;
            double dinnerTotalItem = 0;
            double dinnerTotalPercent = 0;
            double dinnerTotalDiscount = 0;
            double dinnerTotalPromotion = 0;
            double dinnerTotalUnitCost = 0;
            double dinnerTotalTotalCost = 0;
            double dinnerTotalCP = 0;

            // MISC & Discount
            int startM = 0;
            double miscTotalStore = 0, miscBreakfastP = 0, miscLunchP = 0, miscDinnerP = 0;
            double discountBreakfastP = 0, discountLunchP = 0, discountDinnerP = 0;
            List<DiscountAndMiscReportModels> listMisc_DiscountOfStore = new List<DiscountAndMiscReportModels>();
            List<DiscountAndMiscReportModels> listMisc_DiscountPeriod = new List<DiscountAndMiscReportModels>();

            double giftCardSellIncludeSale = 0;
            double giftCardSellNoIncludeSale = 0;

            // For total items
            double quantityIT = 0, itemTotalIT = 0, percentIT = 0, discountIT = 0, promotionAmountIT = 0, costIT = 0, totalCostIT = 0;

            // Get list item for NetSales (IsIncludeSale == false)
            List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSaleInStore = new List<ItemizedSalesAnalysisReportModels>();

            List<ItemizedSalesAnalysisReportModels> lstItems = new List<ItemizedSalesAnalysisReportModels>();

            // List item sale (receipt)
            var lstAllItems = new List<ItemizedSalesAnalysisReportModels>();

            // For NetSale
            lstItemNoIncludeSaleInStore = lstAllItems.Where(w => w.IsIncludeSale == false).ToList();
            List<ItemizedSalesAnalysisReportModels> lstItemNoIncludeSaleInStoreP = new List<ItemizedSalesAnalysisReportModels>();
            double _noIncludeSale = 0;

            //// Pay GC with GC no include sale
            double payGCNoIncludeSale = 0;
            double taxOfPayGCNoIncludeSale = 0;
            double svcOfPayGCNoIncludeSale = 0;
            double tax = 0, svc = 0;

            List<PaymentModels> lstPaymentGCsNoIncludeSaleInStore = new List<PaymentModels>();
            List<PaymentModels> lstPaymentGCsNoIncludeSaleInStoreP = new List<PaymentModels>();

            var receiptGCsStoreP = new DailySalesReportInsertModels();
            var lstGCsRefundsStoreP = new List<RefundReportDTO>();
            double amountGC = 0;

            //// List order id
            List<string> lstReceiptIdFilter = new List<string>(); // Receipt id, filter period
            List<string> lstCNIdFilter = new List<string>(); // CeditNote id, filter period

            double creditNoteTotal = 0, netsale = 0, receiptTotal = 0, svcTotal = 0, taxTotal = 0, discountTotal = 0, promoTotal = 0;

            double totalAmountMiscStore = 0;

            List<ItemizedSalesAnalysisReportModels> lstDataByPeriod = new List<ItemizedSalesAnalysisReportModels>();
            List<ItemizedSalesAnalysisReportModels> lstDataP = new List<ItemizedSalesAnalysisReportModels>();

            var breakfast = new ItemizedSalesPeriodValueTotal();
            var lunch = new ItemizedSalesPeriodValueTotal();
            var dinner = new ItemizedSalesPeriodValueTotal();

            double cp = 0; double cost = 0;
            double miscPercentP = 0;
            double totalCost = 0;
            double totalItem = 0;
            double cpOutlet = 0;

            var lstStoreIdData = lstData.Select(s => s.StoreId).Distinct().ToList();
            lstStores = lstStores.Where(w => lstStoreIdData.Contains(w.Id)).OrderBy(o => o.Name).ToList();

            foreach (var store in lstStores)
            {
                var lstDataStore = lstData.Where(ww => ww.StoreId == store.Id).ToList();
                if (lstDataStore != null && lstDataStore.Any())
                {
                    // Get store setting : time of Period
                    if (currentUser != null)
                    {
                        // Get value from setting of mechant
                        Commons.BreakfastStart = currentUser.BreakfastStart;
                        Commons.BreakfastEnd = currentUser.BreakfastEnd;
                        Commons.LunchStart = currentUser.LunchStart;
                        Commons.LunchEnd = currentUser.LunchEnd;
                        Commons.DinnerStart = currentUser.DinnerStart;
                        Commons.DinnerEnd = currentUser.DinnerEnd;

                        // Get value from setting of store
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
                    // List item sale (receipt)
                    lstAllItems = lstDataStore.Where(ww => string.IsNullOrEmpty(ww.GiftCardId)).ToList();

                    if (lstAllItems != null && lstAllItems.Any())
                    {
                        lstTotalDataInStore = new List<RFilterCategoryV1ReportModel>();

                        lstItems = new List<ItemizedSalesAnalysisReportModels>();

                        _itemTotal = new ItemizedSalesNewTotal();
                        _breakfastOutletTotal = new ItemizedSalesPeriodValueTotal();
                        _lunchOutletTotal = new ItemizedSalesPeriodValueTotal();
                        _dinnerOutletTotal = new ItemizedSalesPeriodValueTotal();

                        if (!isFirstStore)
                        {
                            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
                            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Description"));
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
                            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty"));
                            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Total"));
                            ws.Cell("F" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"));
                            ws.Cell("G" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount"));
                            ws.Cell("H" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion"));
                            ws.Cell("I" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit Cost"));
                            ws.Cell("J" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total Cost"));
                            ws.Cell("K" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("C/P"));

                            //header
                            ws.Range("A" + index + ":K" + index).Style.Font.SetBold(true);
                            //Set color
                            ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + index + ":K" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Row(index).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            index++;
                        }
                        isFirstStore = false;

                        // Store name
                        string storeName = "";
                        if (isExtend)
                        {
                            storeName = store.NameExtend;
                        }
                        else
                        {
                            storeName = store.Name + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("in") + " " + store.CompanyName;
                        }

                        ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                        ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        index++;

                        #region Get data depend on Period
                        // For NetSale
                        lstItemNoIncludeSaleInStore = lstAllItems.Where(w => w.IsIncludeSale == false).ToList();
                        lstItemNoIncludeSaleInStoreP = new List<ItemizedSalesAnalysisReportModels>();
                        _noIncludeSale = 0;

                        //// Pay GC with GC no include sale
                        payGCNoIncludeSale = 0;
                        taxOfPayGCNoIncludeSale = 0;
                        svcOfPayGCNoIncludeSale = 0;
                        tax = 0; svc = 0;

                        lstPaymentGCsNoIncludeSaleInStore = new List<PaymentModels>();
                        lstPaymentGCsNoIncludeSaleInStoreP = new List<PaymentModels>();

                        receiptGCsStoreP = new DailySalesReportInsertModels();
                        lstGCsRefundsStoreP = new List<RefundReportDTO>();
                        amountGC = 0;

                        if (lstPaymentGCs != null && lstPaymentGCs.Any())
                        {
                            lstPaymentGCsNoIncludeSaleInStore = lstPaymentGCs.Where(p => p.StoreId == store.Id && (p.IsInclude == null || (p.IsInclude.HasValue && !p.IsInclude.Value))).ToList();
                        }

                        //// Gift card sell
                        giftCardSellNoIncludeSale = 0;

                        //// List order id
                        lstReceiptIdFilter = new List<string>(); // Receipt id, filter period
                        lstCNIdFilter = new List<string>(); // CeditNote id, filter period

                        // Get list MISC & Discount period of store
                        miscTotalStore = 0; miscBreakfastP = 0; miscLunchP = 0; miscDinnerP = 0;
                        discountBreakfastP = 0; discountLunchP = 0; discountDinnerP = 0;
                        listMisc_DiscountOfStore = new List<DiscountAndMiscReportModels>();

                        if (listMisc_Discount != null && listMisc_Discount.Any())
                        {
                            listMisc_DiscountOfStore = listMisc_Discount.Where(w => w.StoreId == store.Id).ToList();
                        }

                        // Breakfast
                        if (model.Breakfast)
                        {
                            // List all item 
                            lstItems.AddRange(lstAllItems.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd).ToList());

                            //// Item no include sale
                            if (lstItemNoIncludeSaleInStore != null && lstItemNoIncludeSaleInStore.Any())
                            {
                                lstItemNoIncludeSaleInStoreP = lstItemNoIncludeSaleInStore.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd).ToList();
                                if (lstItemNoIncludeSaleInStoreP != null && lstItemNoIncludeSaleInStoreP.Any())
                                {
                                    _noIncludeSale += lstItemNoIncludeSaleInStoreP.Sum(ss => ss.TotalAmount.HasValue ? ss.TotalAmount.Value : 0);
                                    _noIncludeSale -= lstItemNoIncludeSaleInStoreP.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive).Sum(ss => ss.Tax); // Total tax include of item no include sale
                                }
                            }

                            //// Pay GC with GC no include sale
                            if (lstPaymentGCsNoIncludeSaleInStore != null && lstPaymentGCsNoIncludeSaleInStore.Any())
                            {
                                lstPaymentGCsNoIncludeSaleInStoreP = lstPaymentGCsNoIncludeSaleInStore.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd).ToList();
                                if (lstPaymentGCsNoIncludeSaleInStoreP != null && lstPaymentGCsNoIncludeSaleInStoreP.Any())
                                {
                                    foreach (var order in lstPaymentGCsNoIncludeSaleInStoreP)
                                    {
                                        amountGC = order.Amount;

                                        // Refund info
                                        lstGCsRefundsStoreP = lstRefunds.Where(ww => ww.StoreId == store.Id && order.OrderId == ww.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCsRefundsStoreP != null && lstGCsRefundsStoreP.Any())
                                        {
                                            amountGC -= lstGCsRefundsStoreP.Sum(s => s.TotalRefund);

                                        }
                                        payGCNoIncludeSale += amountGC;

                                        // Receipt info 
                                        receiptGCsStoreP = lstOrderInfo.Where(ww => ww.StoreId == store.Id && order.OrderId == ww.OrderId).FirstOrDefault();
                                        if (receiptGCsStoreP != null)
                                        {
                                            tax = 0; svc = 0;
                                            if (receiptGCsStoreP.GST != 0)
                                            {
                                                tax = (amountGC * receiptGCsStoreP.GST / (receiptGCsStoreP.ReceiptTotal == 0 ? 1 : receiptGCsStoreP.ReceiptTotal));
                                            }
                                            if (receiptGCsStoreP.ServiceCharge != 0)
                                            {
                                                svc = (amountGC - tax) * receiptGCsStoreP.ServiceCharge / ((receiptGCsStoreP.ReceiptTotal - receiptGCsStoreP.GST) == 0 ? 1 : (receiptGCsStoreP.ReceiptTotal - receiptGCsStoreP.GST));
                                            }
                                            taxOfPayGCNoIncludeSale += tax;
                                            svcOfPayGCNoIncludeSale += svc;
                                        }
                                    }
                                }
                            }

                            //// Gift card sell which not include sale
                            giftCardSellNoIncludeSale += lstDataStore.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd
                                          && !string.IsNullOrEmpty(ww.GiftCardId)
                                          && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                          && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                            //// List order id
                            if (lstOrderInfo != null && lstOrderInfo.Any())
                            {
                                lstReceiptIdFilter.AddRange(lstOrderInfo.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd && store.Id == ww.StoreId && string.IsNullOrEmpty(ww.CreditNoteNo)).Select(ss => ss.OrderId).Distinct().ToList());

                                lstCNIdFilter.AddRange(lstOrderInfo.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd && store.Id == ww.StoreId && !string.IsNullOrEmpty(ww.CreditNoteNo)).Select(ss => ss.OrderId).Distinct().ToList());
                            }

                            //// MISC & Discount
                            if (listMisc_DiscountOfStore != null && listMisc_DiscountOfStore.Any())
                            {
                                listMisc_DiscountPeriod = listMisc_DiscountOfStore.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd).ToList();
                                miscBreakfastP = listMisc_DiscountPeriod.Where(w => w.ItemTypeId == (int)Commons.EProductType.Misc).Sum(s => s.MiscValue);
                                discountBreakfastP = listMisc_DiscountPeriod.Where(w => w.IsDiscountTotal).Sum(s => s.DiscountValue);
                            }
                        }
                        // Lunch
                        if (model.Lunch)
                        {
                            // List all item 
                            lstItems.AddRange(lstAllItems.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd).ToList());

                            //// Item no include sale
                            if (lstItemNoIncludeSaleInStore != null && lstItemNoIncludeSaleInStore.Any())
                            {
                                lstItemNoIncludeSaleInStoreP = lstItemNoIncludeSaleInStore.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd).ToList();
                                if (lstItemNoIncludeSaleInStoreP != null && lstItemNoIncludeSaleInStoreP.Any())
                                {
                                    _noIncludeSale += lstItemNoIncludeSaleInStoreP.Sum(ss => ss.TotalAmount.HasValue ? ss.TotalAmount.Value : 0);
                                    _noIncludeSale -= lstItemNoIncludeSaleInStoreP.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive).Sum(ss => ss.Tax); // Total tax include of item no include sale
                                }
                            }

                            //// Pay GC with GC no include sale
                            if (lstPaymentGCsNoIncludeSaleInStore != null && lstPaymentGCsNoIncludeSaleInStore.Any())
                            {
                                lstPaymentGCsNoIncludeSaleInStoreP = lstPaymentGCsNoIncludeSaleInStore.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd).ToList();
                                if (lstPaymentGCsNoIncludeSaleInStoreP != null && lstPaymentGCsNoIncludeSaleInStoreP.Any())
                                {
                                    foreach (var order in lstPaymentGCsNoIncludeSaleInStoreP)
                                    {
                                        amountGC = order.Amount;

                                        // Refund info
                                        lstGCsRefundsStoreP = lstRefunds.Where(ww => ww.StoreId == store.Id && order.OrderId == ww.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCsRefundsStoreP != null && lstGCsRefundsStoreP.Any())
                                        {
                                            amountGC -= lstGCsRefundsStoreP.Sum(s => s.TotalRefund);

                                        }
                                        payGCNoIncludeSale += amountGC;

                                        // Receipt info 
                                        receiptGCsStoreP = lstOrderInfo.Where(ww => ww.StoreId == store.Id && order.OrderId == ww.OrderId).FirstOrDefault();
                                        if (receiptGCsStoreP != null)
                                        {
                                            tax = 0; svc = 0;
                                            if (receiptGCsStoreP.GST != 0)
                                            {
                                                tax = (amountGC * receiptGCsStoreP.GST / (receiptGCsStoreP.ReceiptTotal == 0 ? 1 : receiptGCsStoreP.ReceiptTotal));
                                            }
                                            if (receiptGCsStoreP.ServiceCharge != 0)
                                            {
                                                svc = (amountGC - tax) * receiptGCsStoreP.ServiceCharge / ((receiptGCsStoreP.ReceiptTotal - receiptGCsStoreP.GST) == 0 ? 1 : (receiptGCsStoreP.ReceiptTotal - receiptGCsStoreP.GST));
                                            }
                                            taxOfPayGCNoIncludeSale += tax;
                                            svcOfPayGCNoIncludeSale += svc;
                                        }
                                    }
                                }
                            }

                            //// Gift card sell which not include sale
                            giftCardSellNoIncludeSale += lstDataStore.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd
                                          && !string.IsNullOrEmpty(ww.GiftCardId)
                                          && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                          && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);


                            //// List order id
                            if (lstOrderInfo != null && lstOrderInfo.Any())
                            {
                                lstReceiptIdFilter.AddRange(lstOrderInfo.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd && store.Id == ww.StoreId && string.IsNullOrEmpty(ww.CreditNoteNo)).Select(ss => ss.OrderId).Distinct().ToList());

                                lstCNIdFilter.AddRange(lstOrderInfo.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd && store.Id == ww.StoreId && !string.IsNullOrEmpty(ww.CreditNoteNo)).Select(ss => ss.OrderId).Distinct().ToList());
                            }

                            //// MISC & Discount
                            if (listMisc_DiscountOfStore != null && listMisc_DiscountOfStore.Any())
                            {
                                listMisc_DiscountPeriod = listMisc_DiscountOfStore.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd).ToList();
                                miscLunchP = listMisc_DiscountPeriod.Where(w => w.ItemTypeId == (int)Commons.EProductType.Misc).Sum(s => s.MiscValue);
                                discountLunchP = listMisc_DiscountPeriod.Where(w => w.IsDiscountTotal).Sum(s => s.DiscountValue);
                            }
                        }
                        // Dinner
                        if (model.Dinner)
                        {
                            // List all item 
                            lstItems.AddRange(lstAllItems.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                    || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd))).ToList()); // pass day

                            //// Item no include sale
                            if (lstItemNoIncludeSaleInStore != null && lstItemNoIncludeSaleInStore.Any())
                            {
                                lstItemNoIncludeSaleInStoreP = lstItemNoIncludeSaleInStore.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                            || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd))).ToList(); // pass day
                                if (lstItemNoIncludeSaleInStoreP != null && lstItemNoIncludeSaleInStoreP.Any())
                                {
                                    _noIncludeSale += lstItemNoIncludeSaleInStoreP.Sum(ss => ss.TotalAmount.HasValue ? ss.TotalAmount.Value : 0);
                                    _noIncludeSale -= lstItemNoIncludeSaleInStoreP.Where(ww => ww.TaxType == (int)Commons.ETax.Inclusive).Sum(ss => ss.Tax); // Total tax include of item no include sale
                                }
                            }

                            //// Pay GC with GC no include sale
                            if (lstPaymentGCsNoIncludeSaleInStore != null && lstPaymentGCsNoIncludeSaleInStore.Any())
                            {
                                lstPaymentGCsNoIncludeSaleInStoreP = lstPaymentGCsNoIncludeSaleInStore.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                            || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd))).ToList(); // pass day
                                if (lstPaymentGCsNoIncludeSaleInStoreP != null && lstPaymentGCsNoIncludeSaleInStoreP.Any())
                                {
                                    foreach (var order in lstPaymentGCsNoIncludeSaleInStoreP)
                                    {
                                        amountGC = order.Amount;

                                        // Refund info
                                        lstGCsRefundsStoreP = lstRefunds.Where(ww => ww.StoreId == store.Id && order.OrderId == ww.OrderId && ww.IsGiftCard).ToList();
                                        if (lstGCsRefundsStoreP != null && lstGCsRefundsStoreP.Any())
                                        {
                                            amountGC -= lstGCsRefundsStoreP.Sum(s => s.TotalRefund);

                                        }
                                        payGCNoIncludeSale += amountGC;

                                        // Receipt info 
                                        receiptGCsStoreP = lstOrderInfo.Where(ww => ww.StoreId == store.Id && order.OrderId == ww.OrderId).FirstOrDefault();
                                        if (receiptGCsStoreP != null)
                                        {
                                            tax = 0; svc = 0;
                                            if (receiptGCsStoreP.GST != 0)
                                            {
                                                tax = (amountGC * receiptGCsStoreP.GST / (receiptGCsStoreP.ReceiptTotal == 0 ? 1 : receiptGCsStoreP.ReceiptTotal));
                                            }
                                            if (receiptGCsStoreP.ServiceCharge != 0)
                                            {
                                                svc = (amountGC - tax) * receiptGCsStoreP.ServiceCharge / ((receiptGCsStoreP.ReceiptTotal - receiptGCsStoreP.GST) == 0 ? 1 : (receiptGCsStoreP.ReceiptTotal - receiptGCsStoreP.GST));
                                            }
                                            taxOfPayGCNoIncludeSale += tax;
                                            svcOfPayGCNoIncludeSale += svc;
                                        }
                                    }
                                }
                            }

                            //// Gift card sell which not include sale
                            giftCardSellNoIncludeSale += lstDataStore.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                          || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd)) // pass day
                                          && !string.IsNullOrEmpty(ww.GiftCardId)
                                          && !string.IsNullOrEmpty(ww.PoinsOrderId) && ww.TotalAmount.HasValue
                                          && (ww.IsIncludeSale.HasValue && !ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);

                            //// List order id
                            if (lstOrderInfo != null && lstOrderInfo.Any())
                            {
                                lstReceiptIdFilter.AddRange(lstOrderInfo.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                          || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd)) // pass day 
                                          && store.Id == ww.StoreId && string.IsNullOrEmpty(ww.CreditNoteNo)).Select(ss => ss.OrderId).Distinct().ToList());

                                lstCNIdFilter.AddRange(lstOrderInfo.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                          || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd)) // pass day 
                                            && store.Id == ww.StoreId && !string.IsNullOrEmpty(ww.CreditNoteNo)).Select(ss => ss.OrderId).Distinct().ToList());
                            }

                            //// MISC & Discount
                            if (listMisc_DiscountOfStore != null && listMisc_DiscountOfStore.Any())
                            {
                                listMisc_DiscountPeriod = listMisc_DiscountOfStore.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                            || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd))).ToList(); // pass day
                                miscDinnerP = listMisc_DiscountPeriod.Where(w => w.ItemTypeId == (int)Commons.EProductType.Misc).Sum(s => s.MiscValue);
                                discountDinnerP = listMisc_DiscountPeriod.Where(w => w.IsDiscountTotal).Sum(s => s.DiscountValue);
                            }
                        }

                        miscTotalStore = miscBreakfastP + miscLunchP + miscDinnerP;

                        creditNoteTotal = 0;
                        if (lstCNIdFilter != null && lstCNIdFilter.Any())
                        {
                            creditNoteTotal = lstData.Where(ww => ww.StoreId == store.Id && lstCNIdFilter.Contains(ww.ReceiptId)
                                                                && ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value).Sum(ss => ss.TotalAmount.HasValue ? ss.TotalAmount.Value : 0);
                        }
                        netsale = 0; receiptTotal = 0; svcTotal = 0; taxTotal = 0; discountTotal = 0; promoTotal = 0;

                        var lstOrderInfoFilter = lstOrderInfo.Where(ww => ww.StoreId == store.Id && lstReceiptIdFilter.Contains(ww.OrderId)).ToList();

                        receiptTotal = lstOrderInfoFilter.Sum(ss => ss.ReceiptTotal);
                        svcTotal = lstOrderInfoFilter.Sum(ss => ss.ServiceCharge);
                        taxTotal = lstOrderInfoFilter.Sum(ss => ss.GST);
                        discountTotal = lstOrderInfoFilter.Sum(ss => ss.Discount);
                        promoTotal = lstOrderInfoFilter.Sum(ss => ss.PromotionValue);

                        // netsale = total receipt  - totalCreditNote (with  inlude GC on sale) - SVC- total tax - GC sell not include -_noincludeSale (without GC, depent category)- (GC pay not include - tax of pay GC not include - SCV of pay GC not include )
                        netsale = receiptTotal - creditNoteTotal - svcTotal - taxTotal - giftCardSellNoIncludeSale - _noIncludeSale - (payGCNoIncludeSale - taxOfPayGCNoIncludeSale - svcOfPayGCNoIncludeSale);

                        #endregion Get data depend on Period

                        // Group item type
                        var lstGroupItemType = lstItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                               || ww.ItemTypeId == (int)Commons.EProductType.SetMenu))
                                   .OrderBy(x => x.ItemTypeId).GroupBy(gg => gg.ItemTypeId).ToList();

                        var itemAmountTotal = lstItems.Sum(ss => ss.ItemTotal);

                        foreach (var itemTypeId in lstGroupItemType)
                        {
                            List<RFilterCategoryModel> lstCateCheckedInStore = new List<RFilterCategoryModel>();

                            if (itemTypeId.Key == (int)Commons.EProductType.Dish) // Dish
                            {
                                lstCateCheckedInStore = _lstCateChecked.Where(w => w.StoreId == store.Id)
                                    .Select(s => new RFilterCategoryModel
                                    {
                                        Id = s.Id,
                                        Name = s.Name
                                    }).ToList();

                                lstTotalDataInStore = lstTotalAllCate.Where(w => w.StoreId == store.Id).ToList();
                            }
                            else // SetMenu
                            {
                                // Check data old
                                var tmp = itemTypeId.Where(ww => ww.CategoryId == "SetMenu").FirstOrDefault();
                                // Check setmenu have category or not category
                                var categoryExist = _lstSetChecked.Where(ww => string.IsNullOrEmpty(ww.CategoryID)).FirstOrDefault();
                                if (categoryExist != null || tmp != null)
                                {
                                    lstCateCheckedInStore = new List<RFilterCategoryModel>() {
                                        new RFilterCategoryModel(){Id="SetMenu", Name ="SetMenu"}
                                    };
                                }
                                else
                                {
                                    var lstTmp = _lstSetChecked.Where(w => w.StoreId == store.Id)
                                      .Select(s => new
                                      {
                                          Id = s.CategoryID,
                                          Name = s.CategoryName
                                      }).Distinct().ToList();

                                    lstCateCheckedInStore = lstTmp
                                        .Select(s => new RFilterCategoryModel
                                        {
                                            Id = s.Id,
                                            Name = s.Name
                                        }).ToList();
                                }
                                lstTotalDataInStore = lstTotalAllSetMenu.Where(w => w.StoreId == store.Id).ToList();
                                lstTotalDataInStore.Add(new RFilterCategoryV1ReportModel() { CateId = "SetMenu", CateName = "SetMenu" });
                            }

                            // Group by category
                            foreach (var itemCate in lstCateCheckedInStore)
                            {
                                subTotalQty = 0;
                                subTotalItem = 0;
                                subTotalPercent = 0;
                                subTotalDiscount = 0;
                                subTotalPromotion = 0;
                                subTotalUnitCost = 0;
                                subTotalTotalCost = 0;
                                subTotalCP = 0;

                                breakfastTotalQty = 0;
                                breakfastTotalItem = 0;
                                breakfastTotalPercent = 0;
                                breakfastTotalDiscount = 0;
                                breakfastTotalPromotion = 0;
                                breakfastTotalUnitCost = 0;
                                breakfastTotalTotalCost = 0;
                                breakfastTotalCP = 0;

                                lunchTotalQty = 0;
                                lunchTotalItem = 0;
                                lunchTotalPercent = 0;
                                lunchTotalDiscount = 0;
                                lunchTotalPromotion = 0;
                                lunchTotalUnitCost = 0;
                                lunchTotalTotalCost = 0;
                                lunchTotalCP = 0;

                                dinnerTotalQty = 0;
                                dinnerTotalItem = 0;
                                dinnerTotalPercent = 0;
                                dinnerTotalDiscount = 0;
                                dinnerTotalPromotion = 0;
                                dinnerTotalUnitCost = 0;
                                dinnerTotalTotalCost = 0;
                                dinnerTotalCP = 0;

                                ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + " - " + itemCate.Name));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                index++;

                                var lstCateItems = itemTypeId.Where(w => w.CategoryId == itemCate.Id).ToList();
                                var lstItemTypes = lstCateItems.Where(ww => (ww.ItemTypeId == (int)Commons.EProductType.Dish
                                                        || ww.ItemTypeId == (int)Commons.EProductType.SetMenu)).ToList();

                                totalAmountMiscStore = itemAmountTotal + miscTotalStore;

                                lstDataByPeriod = new List<ItemizedSalesAnalysisReportModels>();
                                lstDataP = new List<ItemizedSalesAnalysisReportModels>();

                                breakfast = new ItemizedSalesPeriodValueTotal();
                                lunch = new ItemizedSalesPeriodValueTotal();
                                dinner = new ItemizedSalesPeriodValueTotal();

                                // Breakfast
                                if (model.Breakfast)
                                {
                                    lstDataP = lstItemTypes.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd).ToList();
                                    quantityIT = 0; itemTotalIT = 0; percentIT = 0; discountIT = 0; promotionAmountIT = 0; costIT = 0; totalCostIT = 0;

                                    if (lstDataP != null && lstDataP.Any())
                                    {
                                        quantityIT = lstDataP.Sum(s => s.Quantity);
                                        itemTotalIT = lstDataP.Sum(s => s.ItemTotal);
                                        discountIT = lstDataP.Sum(s => s.Discount);
                                        promotionAmountIT = lstDataP.Sum(s => s.PromotionAmount);
                                        costIT = lstDataP.Sum(s => s.Cost);
                                        totalCostIT = lstDataP.Sum(s => s.TotalCost);
                                        if (totalAmountMiscStore != 0)
                                        {
                                            percentIT = lstDataP.Sum(s => (s.ItemTotal / totalAmountMiscStore * 100));
                                        }
                                    }

                                    breakfast.Qty += quantityIT;
                                    breakfast.ItemTotal += itemTotalIT;
                                    breakfast.Percent += percentIT;
                                    breakfast.Discount += discountIT;
                                    breakfast.Promotion += promotionAmountIT;
                                    breakfast.UnitCost += costIT;
                                    breakfast.TotalCost += totalCostIT;

                                    _breakfastOutletTotal.Qty += quantityIT;
                                    _breakfastOutletTotal.ItemTotal += itemTotalIT;
                                    _breakfastOutletTotal.Percent += percentIT;
                                    _breakfastOutletTotal.Discount += discountIT;
                                    _breakfastOutletTotal.Promotion += promotionAmountIT;
                                    _breakfastOutletTotal.UnitCost += costIT;
                                    _breakfastOutletTotal.TotalCost += totalCostIT;

                                    _breakfastTotal.Qty += quantityIT;
                                    _breakfastTotal.ItemTotal += itemTotalIT;
                                    _breakfastTotal.Percent += percentIT;
                                    _breakfastTotal.Discount += discountIT;
                                    _breakfastTotal.Promotion += promotionAmountIT;
                                    _breakfastTotal.UnitCost += costIT;
                                    _breakfastTotal.TotalCost += totalCostIT;
                                }
                                // Lunch
                                if (model.Lunch)
                                {
                                    lstDataP = lstItemTypes.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd).ToList();
                                    quantityIT = 0; itemTotalIT = 0; percentIT = 0; discountIT = 0; promotionAmountIT = 0; costIT = 0; totalCostIT = 0;

                                    if (lstDataP != null && lstDataP.Any())
                                    {
                                        quantityIT = lstDataP.Sum(s => s.Quantity);
                                        itemTotalIT = lstDataP.Sum(s => s.ItemTotal);
                                        discountIT = lstDataP.Sum(s => s.Discount);
                                        promotionAmountIT = lstDataP.Sum(s => s.PromotionAmount);
                                        costIT = lstDataP.Sum(s => s.Cost);
                                        totalCostIT = lstDataP.Sum(s => s.TotalCost);
                                        if (totalAmountMiscStore != 0)
                                        {
                                            percentIT = lstDataP.Sum(s => (s.ItemTotal / totalAmountMiscStore * 100));
                                        }
                                    }

                                    lunch.Qty += quantityIT;
                                    lunch.ItemTotal += itemTotalIT;
                                    lunch.Percent += percentIT;
                                    lunch.Discount += discountIT;
                                    lunch.Promotion += promotionAmountIT;
                                    lunch.UnitCost += costIT;
                                    lunch.TotalCost += totalCostIT;

                                    _lunchOutletTotal.Qty += quantityIT;
                                    _lunchOutletTotal.ItemTotal += itemTotalIT;
                                    _lunchOutletTotal.Percent += percentIT;
                                    _lunchOutletTotal.Discount += discountIT;
                                    _lunchOutletTotal.Promotion += promotionAmountIT;
                                    _lunchOutletTotal.UnitCost += costIT;
                                    _lunchOutletTotal.TotalCost += totalCostIT;

                                    _lunchTotal.Qty += quantityIT;
                                    _lunchTotal.ItemTotal += itemTotalIT;
                                    _lunchTotal.Percent += percentIT;
                                    _lunchTotal.Discount += discountIT;
                                    _lunchTotal.Promotion += promotionAmountIT;
                                    _lunchTotal.UnitCost += costIT;
                                    _lunchTotal.TotalCost += totalCostIT;
                                }
                                // Dinner
                                if (model.Dinner)
                                {
                                    lstDataP = lstItemTypes.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                          || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd))).ToList(); // pass day
                                    quantityIT = 0; itemTotalIT = 0; percentIT = 0; discountIT = 0; promotionAmountIT = 0; costIT = 0; totalCostIT = 0;

                                    if (lstDataP != null && lstDataP.Any())
                                    {
                                        quantityIT = lstDataP.Sum(s => s.Quantity);
                                        itemTotalIT = lstDataP.Sum(s => s.ItemTotal);
                                        discountIT = lstDataP.Sum(s => s.Discount);
                                        promotionAmountIT = lstDataP.Sum(s => s.PromotionAmount);
                                        costIT = lstDataP.Sum(s => s.Cost);
                                        totalCostIT = lstDataP.Sum(s => s.TotalCost);
                                        if (totalAmountMiscStore != 0)
                                        {
                                            percentIT = lstDataP.Sum(s => (s.ItemTotal / totalAmountMiscStore * 100));
                                        }
                                    }

                                    dinner.Qty += quantityIT;
                                    dinner.ItemTotal += itemTotalIT;
                                    dinner.Percent += percentIT;
                                    dinner.Discount += discountIT;
                                    dinner.Promotion += promotionAmountIT;
                                    dinner.UnitCost += costIT;
                                    dinner.TotalCost += totalCostIT;

                                    _dinnerOutletTotal.Qty += quantityIT;
                                    _dinnerOutletTotal.ItemTotal += itemTotalIT;
                                    _dinnerOutletTotal.Percent += percentIT;
                                    _dinnerOutletTotal.Discount += discountIT;
                                    _dinnerOutletTotal.Promotion += promotionAmountIT;
                                    _dinnerOutletTotal.UnitCost += costIT;
                                    _dinnerOutletTotal.TotalCost += totalCostIT;

                                    _dinnerTotal.Qty += quantityIT;
                                    _dinnerTotal.ItemTotal += itemTotalIT;
                                    _dinnerTotal.Percent += percentIT;
                                    _dinnerTotal.Discount += discountIT;
                                    _dinnerTotal.Promotion += promotionAmountIT;
                                    _dinnerTotal.UnitCost += costIT;
                                    _dinnerTotal.TotalCost += totalCostIT;
                                }

                                // Get total item
                                _itemTotal.SCTotal += lstItemTypes.Sum(s => s.ServiceCharge);
                                _itemTotal.ItemTotal += lstItemTypes.Sum(s => s.ItemTotal);
                                _itemTotal.TaxTotal += lstItemTypes.Sum(s => s.Tax);
                                _itemTotal.DiscountTotal += lstItemTypes.Sum(s => s.Discount);
                                _itemTotal.PromotionTotal += lstItemTypes.Sum(s => s.PromotionAmount);

                                //Group Item
                                var lstItemTypeGroup = lstItemTypes.OrderBy(oo => oo.ItemName).ThenBy(oo => oo.ItemCode).GroupBy(gg => new { gg.ItemTypeId, gg.ItemId, gg.Price }).ToList();

                                cp = 0; cost = 0;

                                foreach (var item in lstItemTypeGroup)
                                {
                                    ws.Cell("A" + index).Value = item.Select(ss => ss.ItemCode).FirstOrDefault();
                                    ws.Cell("B" + index).Value = item.Select(ss => ss.ItemName).FirstOrDefault();
                                    ws.Cell("C" + index).Value = item.Select(ss => ss.Price).FirstOrDefault().ToString("F");
                                    ws.Cell("C" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Cell("D" + index).Value = item.Sum(d => d.Quantity);
                                    ws.Cell("E" + index).Value = item.Sum(d => d.ItemTotal).ToString("F");
                                    ws.Cell("F" + index).Value = ((totalAmountMiscStore != 0) ? item.Sum(d => (d.ItemTotal / totalAmountMiscStore * 100)) : 0).ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (item.Sum(d => d.Discount) * (-1)).ToString("F");
                                    ws.Cell("H" + index).Value = (item.Sum(d => d.PromotionAmount) * (-1)).ToString("F");
                                    ws.Cell("I" + index).Value = item.Select(ss => ss.Cost).FirstOrDefault().ToString("F");
                                    cost += item.Select(ss => ss.Cost).FirstOrDefault();
                                    ws.Cell("J" + index).Value = item.Sum(d => d.TotalCost).ToString("F");

                                    if (item.Sum(d => d.TotalCost) == 0 || item.Sum(d => d.ItemTotal) == 0)
                                        cp = 0;
                                    else
                                        cp = ((item.Sum(d => d.TotalCost) / item.Sum(d => d.ItemTotal)) * 100);

                                    ws.Cell("K" + index).Value = cp.ToString("F") + " %";
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    index++;
                                } //end lstItemTypeGroup

                                subTotalQty = breakfast.Qty + lunch.Qty + dinner.Qty;
                                subTotalItem = breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal;
                                subTotalPercent = breakfast.Percent + lunch.Percent + dinner.Percent;
                                subTotalDiscount = breakfast.Discount + lunch.Discount + dinner.Discount;
                                subTotalPromotion = breakfast.Promotion + lunch.Promotion + dinner.Promotion;
                                subTotalUnitCost = cost;
                                subTotalTotalCost = breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost;
                                if ((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) != 0 && (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal) != 0)
                                {
                                    subTotalCP = (((breakfast.TotalCost + lunch.TotalCost + dinner.TotalCost) / (breakfast.ItemTotal + lunch.ItemTotal + dinner.ItemTotal)) * 100);
                                }

                                breakfastTotalQty = breakfast.Qty;
                                breakfastTotalItem = breakfast.ItemTotal;
                                breakfastTotalPercent = breakfast.Percent;
                                breakfastTotalDiscount = breakfast.Discount;
                                breakfastTotalPromotion = breakfast.Promotion;
                                breakfastTotalUnitCost = breakfast.UnitCost;
                                breakfastTotalTotalCost = breakfast.TotalCost;
                                if (breakfast.TotalCost != 0 && breakfast.ItemTotal != 0)
                                {
                                    breakfastTotalCP = ((breakfast.TotalCost / breakfast.ItemTotal) * 100);
                                }

                                lunchTotalQty = lunch.Qty;
                                lunchTotalItem = lunch.ItemTotal;
                                lunchTotalPercent = lunch.Percent;
                                lunchTotalDiscount = lunch.Discount;
                                lunchTotalPromotion = lunch.Promotion;
                                lunchTotalUnitCost = lunch.UnitCost;
                                lunchTotalTotalCost = lunch.TotalCost;
                                lunchTotalCP = 0;
                                if (lunch.TotalCost != 0 && lunch.ItemTotal != 0)
                                {
                                    lunchTotalCP = ((lunch.TotalCost / lunch.ItemTotal) * 100);
                                }

                                dinnerTotalQty = dinner.Qty;
                                dinnerTotalItem = dinner.ItemTotal;
                                dinnerTotalPercent = dinner.Percent;
                                dinnerTotalDiscount = dinner.Discount;
                                dinnerTotalPromotion = dinner.Promotion;
                                dinnerTotalUnitCost = dinner.UnitCost;
                                dinnerTotalTotalCost = dinner.TotalCost;
                                if (dinner.TotalCost != 0 && dinner.ItemTotal != 0)
                                {
                                    dinnerTotalCP = ((dinner.TotalCost / dinner.ItemTotal) * 100);
                                }

                                // Get total information of current cate
                                var totalDataCurrentCate = lstTotalDataInStore.Where(w => w.CateId == itemCate.Id).FirstOrDefault();
                                // Update total data of current cate
                                totalDataCurrentCate.SubTotalQty += subTotalQty;
                                totalDataCurrentCate.SubTotalItem += subTotalItem;
                                totalDataCurrentCate.SubTotalPercent += subTotalPercent;
                                totalDataCurrentCate.SubTotalDiscount += subTotalDiscount;
                                totalDataCurrentCate.SubTotalPromotion += subTotalPromotion;
                                totalDataCurrentCate.SubTotalUnitCost += subTotalUnitCost;
                                totalDataCurrentCate.SubTotalTotalCost += subTotalTotalCost;
                                totalDataCurrentCate.SubTotalCP += subTotalCP;

                                totalDataCurrentCate.BreakfastTotalQty += breakfastTotalQty;
                                totalDataCurrentCate.BreakfastTotalItem += breakfastTotalItem;
                                totalDataCurrentCate.BreakfastTotalPercent += breakfastTotalPercent;
                                totalDataCurrentCate.BreakfastTotalDiscount += breakfastTotalDiscount;
                                totalDataCurrentCate.BreakfastTotalPromotion += breakfastTotalPromotion;
                                totalDataCurrentCate.BreakfastTotalUnitCost += breakfastTotalUnitCost;
                                totalDataCurrentCate.BreakfastTotalTotalCost += breakfastTotalTotalCost;
                                totalDataCurrentCate.BreakfastTotalCP += breakfastTotalCP;

                                totalDataCurrentCate.LunchTotalQty += lunchTotalQty;
                                totalDataCurrentCate.LunchTotalItem += lunchTotalItem;
                                totalDataCurrentCate.LunchTotalPercent += lunchTotalPercent;
                                totalDataCurrentCate.LunchTotalDiscount += lunchTotalDiscount;
                                totalDataCurrentCate.LunchTotalPromotion += lunchTotalPromotion;
                                totalDataCurrentCate.LunchTotalUnitCost += lunchTotalUnitCost;
                                totalDataCurrentCate.LunchTotalTotalCost += lunchTotalTotalCost;
                                totalDataCurrentCate.LunchTotalCP += lunchTotalCP;

                                totalDataCurrentCate.DinnerTotalQty += dinnerTotalQty;
                                totalDataCurrentCate.DinnerTotalItem += dinnerTotalItem;
                                totalDataCurrentCate.DinnerTotalPercent += dinnerTotalPercent;
                                totalDataCurrentCate.DinnerTotalDiscount += dinnerTotalDiscount;
                                totalDataCurrentCate.DinnerTotalPromotion += dinnerTotalPromotion;
                                totalDataCurrentCate.DinnerTotalUnitCost += dinnerTotalUnitCost;
                                totalDataCurrentCate.DinnerTotalTotalCost += dinnerTotalTotalCost;
                                totalDataCurrentCate.DinnerTotalCP += dinnerTotalCP;

                                // Show total data of current
                                // If it has any cate child which was checked  => title: Sub-Total: CateName
                                // If it doesn't have any cate child which was checked && it doesn't has parent cate which was checked => it's top cate => title: Category Total: CateName
                                if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && !lstTotalDataInStore.Where(w => !string.IsNullOrEmpty(totalDataCurrentCate.ParentId) && w.CateId == totalDataCurrentCate.ParentId && w.Checked).Any())
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Total") + " : " + totalDataCurrentCate.CateName);
                                }
                                else
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sub-total") + " : " + totalDataCurrentCate.CateName);
                                }

                                ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                                ws.Cell("D" + index).Value = totalDataCurrentCate.SubTotalQty;
                                ws.Cell("E" + index).Value = totalDataCurrentCate.SubTotalItem.ToString("F");
                                ws.Cell("F" + index).Value = totalDataCurrentCate.SubTotalPercent.ToString("F") + " %";
                                ws.Cell("G" + index).Value = (-totalDataCurrentCate.SubTotalDiscount).ToString("F");
                                ws.Cell("H" + index).Value = (-totalDataCurrentCate.SubTotalPromotion).ToString("F");
                                ws.Cell("I" + index).Value = totalDataCurrentCate.SubTotalUnitCost.ToString("F");
                                ws.Cell("J" + index).Value = totalDataCurrentCate.SubTotalTotalCost.ToString("F");
                                ws.Cell("K" + index).Value = totalDataCurrentCate.SubTotalCP.ToString("F") + " %";
                                ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                index++;

                                // Morning
                                if (model.Breakfast)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.BreakfastTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.BreakfastTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.BreakfastTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.BreakfastTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.BreakfastTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.BreakfastTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.BreakfastTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.BreakfastTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                // Afternoon
                                if (model.Lunch)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.LunchTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.LunchTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.LunchTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.LunchTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.LunchTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.LunchTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.LunchTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.LunchTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                // Dinner
                                if (model.Dinner)
                                {
                                    ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                                    ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    ws.Cell("D" + index).Value = totalDataCurrentCate.DinnerTotalQty;
                                    ws.Cell("E" + index).Value = totalDataCurrentCate.DinnerTotalItem.ToString("F");
                                    ws.Cell("F" + index).Value = totalDataCurrentCate.DinnerTotalPercent.ToString("F") + " %";
                                    ws.Cell("G" + index).Value = (-totalDataCurrentCate.DinnerTotalDiscount).ToString("F");
                                    ws.Cell("H" + index).Value = (-totalDataCurrentCate.DinnerTotalPromotion).ToString("F");
                                    ws.Cell("I" + index).Value = totalDataCurrentCate.DinnerTotalUnitCost.ToString("F");
                                    ws.Cell("J" + index).Value = totalDataCurrentCate.DinnerTotalTotalCost.ToString("F");
                                    ws.Cell("K" + index).Value = totalDataCurrentCate.DinnerTotalCP.ToString("F") + " %";
                                    ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                    ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    index++;
                                }

                                // Check current cate is last chile of parent current cate
                                Boolean isLastChildCate = false;
                                // If current cate has parent cate, check for show total data of parent cate
                                if (!string.IsNullOrEmpty(totalDataCurrentCate.ParentId))
                                {
                                    var parentCurrentCateInfo = lstTotalDataInStore.Where(w => w.CateId == totalDataCurrentCate.ParentId).FirstOrDefault();
                                    // List cate child
                                    var lstCateChildCheckedOfParent = parentCurrentCateInfo.ListCateChildChecked.ToList();
                                    if (lstCateChildCheckedOfParent != null && lstCateChildCheckedOfParent.Any())
                                    {
                                        // Get id of last child cate
                                        string idLastCateChild = parentCurrentCateInfo.ListCateChildChecked.LastOrDefault().ToString();

                                        // Current cate is last cate of parent cate
                                        if ((totalDataCurrentCate.ListCateChildChecked == null || !totalDataCurrentCate.ListCateChildChecked.Any()) && idLastCateChild == totalDataCurrentCate.CateId)
                                        {
                                            isLastChildCate = true;
                                        }
                                    }

                                    DisplayTotalOfParentCate(ref ws, totalDataCurrentCate, ref index, ref lstTotalDataInStore, isLastChildCate, totalDataCurrentCate, model);
                                }
                            }//End Group by category
                        }//end lstGroupItemType

                        #region MISC & Discount
                        miscPercentP = 0;
                        if (listMisc_DiscountOfStore != null && listMisc_DiscountOfStore.Any())
                        {
                            if (miscTotalStore != 0)
                            {
                                startM = index;

                                // MISC
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Misc Items"));
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorDataRow));
                                index++;

                                //// BREAKFAST
                                if (model.Breakfast)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                                    ws.Cell("E" + index).SetValue(miscBreakfastP);
                                    miscPercentP = miscBreakfastP / (miscTotalStore + itemAmountTotal) * 100;
                                    _breakfastOutletTotal.Percent += miscPercentP;
                                    ws.Cell("F" + index).SetValue(miscPercentP.ToString("F") + " %");
                                    index++;
                                }

                                //// LUNCH
                                if (model.Lunch)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                                    ws.Cell("E" + index).SetValue(miscLunchP);
                                    miscPercentP = miscLunchP / (miscTotalStore + itemAmountTotal) * 100;
                                    _lunchOutletTotal.Percent += miscPercentP;
                                    ws.Cell("F" + index).SetValue(miscPercentP.ToString("F") + " %");
                                    index++;
                                }

                                //// DINNER
                                if (model.Dinner)
                                {
                                    ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                                    ws.Cell("E" + index).SetValue(miscDinnerP);
                                    miscPercentP = miscDinnerP / (miscTotalStore + itemAmountTotal) * 100;
                                    _dinnerOutletTotal.Percent += miscPercentP;
                                    ws.Cell("F" + index).SetValue(miscPercentP.ToString("F") + " %");
                                    index++;
                                }
                                _itemTotal.ItemTotal += miscTotalStore;

                                ws.Range("A" + startM + ":K" + index).Style.Font.SetBold(true);
                                ws.Range("A" + startM + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Range("A" + startM + ":D" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Range("E" + startM + ":G" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }

                            if (discountBreakfastP != 0 || discountLunchP != 0 || discountDinnerP != 0)
                            {
                                startM = index;

                                // Discount Total Bill
                                ws.Range("A" + index + ":K" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Total Bill"));
                                ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                                index++;

                                //// BREAKFAST
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                                ws.Cell("G" + index).SetValue(discountBreakfastP);
                                _itemTotal.DiscountTotal += discountBreakfastP;
                                _breakfastOutletTotal.Discount += discountBreakfastP;
                                index++;

                                //// LUNCH
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                                ws.Cell("G" + index).SetValue(discountLunchP);
                                _itemTotal.DiscountTotal += discountLunchP;
                                _lunchOutletTotal.Discount += discountLunchP;
                                index++;

                                //// DINNER
                                ws.Range("A" + index + ":D" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                                ws.Cell("G" + index).SetValue(discountDinnerP);
                                _itemTotal.DiscountTotal += discountDinnerP;
                                _dinnerOutletTotal.Discount += discountDinnerP;
                                index++;

                                ws.Range("A" + startM + ":K" + index).Style.Font.SetBold(true);
                                ws.Range("A" + startM + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                                ws.Range("A" + startM + ":D" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                ws.Range("E" + startM + ":G" + index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                        }
                        #endregion MISC & Discount

                        totalCost = _breakfastOutletTotal.TotalCost + _lunchOutletTotal.TotalCost + _dinnerOutletTotal.TotalCost;
                        totalItem = _breakfastOutletTotal.ItemTotal + _lunchOutletTotal.ItemTotal + _dinnerOutletTotal.ItemTotal;

                        ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL"));
                        ws.Range("A" + (index) + ":C" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("D" + index).Value = _breakfastOutletTotal.Qty + _lunchOutletTotal.Qty + _dinnerOutletTotal.Qty;
                        ws.Cell("E" + index).Value = (totalItem + miscTotalStore);
                        ws.Cell("F" + index).Value = (_breakfastOutletTotal.Percent + _lunchOutletTotal.Percent + _dinnerOutletTotal.Percent).ToString("F") + " %";
                        ws.Cell("G" + index).Value = ((_breakfastOutletTotal.Discount + _lunchOutletTotal.Discount + _dinnerOutletTotal.Discount) * (-1));
                        ws.Cell("H" + index).Value = ((_breakfastOutletTotal.Promotion + _lunchOutletTotal.Promotion + _dinnerOutletTotal.Promotion) * (-1));
                        ws.Cell("I" + index).Value = (_breakfastOutletTotal.UnitCost + _lunchOutletTotal.UnitCost + _dinnerOutletTotal.UnitCost);
                        ws.Cell("J" + index).Value = totalCost;

                        cpOutlet = 0;
                        if (totalCost != 0 && totalItem != 0)
                        {
                            cpOutlet = totalCost / totalItem * 100;
                        }

                        ws.Cell("K" + index).Value = cpOutlet.ToString("F") + " %";
                        ws.Range("E" + index + ":J" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + index + ":K" + index).Style.Font.SetBold(true);
                        ws.Range("A" + index + ":K" + index).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Cell("D" + index).Style.NumberFormat.Format = "#,##0";
                        ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        index++;

                        // Breakfast
                        if (model.Breakfast)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.BREAKFAST));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _breakfastOutletTotal.Qty;
                            ws.Cell("E" + index).Value = _breakfastOutletTotal.ItemTotal + miscBreakfastP;
                            ws.Cell("F" + index).Value = _breakfastOutletTotal.Percent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = _breakfastOutletTotal.Discount * (-1);
                            ws.Cell("H" + index).Value = _breakfastOutletTotal.Promotion * (-1);
                            ws.Cell("I" + index).Value = _breakfastOutletTotal.UnitCost;
                            ws.Cell("J" + index).Value = _breakfastOutletTotal.TotalCost;

                            if (_breakfastOutletTotal.TotalCost != 0 && _breakfastOutletTotal.ItemTotal != 0)
                            {
                                _breakfastOutletTotal.CP = _breakfastOutletTotal.TotalCost / _breakfastOutletTotal.ItemTotal * 100;
                            }
                            ws.Cell("K" + index).Value = _breakfastOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        // LUNCH
                        if (model.Lunch)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.LUNCH));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _lunchOutletTotal.Qty;
                            ws.Cell("E" + index).Value = _lunchOutletTotal.ItemTotal + miscLunchP;
                            ws.Cell("F" + index).Value = _lunchOutletTotal.Percent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = _lunchOutletTotal.Discount * (-1);
                            ws.Cell("H" + index).Value = _lunchOutletTotal.Promotion * (-1);
                            ws.Cell("I" + index).Value = _lunchOutletTotal.UnitCost;
                            ws.Cell("J" + index).Value = _lunchOutletTotal.TotalCost;

                            if (_lunchOutletTotal.TotalCost != 0 && _lunchOutletTotal.ItemTotal != 0)
                            {
                                _lunchOutletTotal.CP = _lunchOutletTotal.TotalCost / _lunchOutletTotal.ItemTotal * 100;
                            }

                            ws.Cell("K" + index).Value = _lunchOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        // DINNER
                        if (model.Dinner)
                        {
                            ws.Range("A" + index + ":C" + index).Merge().SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DINNER));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            ws.Cell("D" + index).Value = _dinnerOutletTotal.Qty;
                            ws.Cell("E" + index).Value = _dinnerOutletTotal.ItemTotal + miscDinnerP;
                            ws.Cell("F" + index).Value = _dinnerOutletTotal.Percent.ToString("F") + " %";
                            ws.Cell("G" + index).Value = _dinnerOutletTotal.Discount * (-1);
                            ws.Cell("H" + index).Value = _dinnerOutletTotal.Promotion * (-1);
                            ws.Cell("I" + index).Value = _dinnerOutletTotal.UnitCost;
                            ws.Cell("J" + index).Value = _dinnerOutletTotal.TotalCost;

                            if (_dinnerOutletTotal.TotalCost != 0 && _dinnerOutletTotal.ItemTotal != 0)
                            {
                                _dinnerOutletTotal.CP = _dinnerOutletTotal.TotalCost / _dinnerOutletTotal.ItemTotal * 100;
                            }

                            ws.Cell("K" + index).Value = _dinnerOutletTotal.CP.ToString("F") + " %";
                            ws.Range("E" + index + ":K" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("D" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            index++;
                        }

                        ws.Range("C" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Format file
                        // Header
                        ws.Range("A4:K6").Style.Font.SetBold(true);
                        // Set color
                        ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        // Set Border        
                        ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        if (_firstStore)
                        {
                            ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        else
                        {
                            ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        index++;
                        // end total

                        // Summary
                        ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                        ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                        if (GetTaxType(store.Id) == (int)Commons.ETax.AddOn)
                        {
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                        }
                        else
                        {
                            ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                        }
                        ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                        ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                        ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        index++;

                        ws.Cell("A" + index).Value = netsale;
                        ws.Cell("B" + index).Value = svcTotal;
                        ws.Cell("C" + index).Value = taxTotal;
                        ws.Cell("D" + index).Value = discountTotal * (-1);
                        ws.Cell("E" + index).Value = promoTotal * (-1);

                        ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                        ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                        ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        index += 2;
                        indexNextStore = index;
                        _firstStore = false;
                    }
                    else
                    {
                        //check only sale GC
                        var lstGCSell = lstDataStore.Where(ww => !string.IsNullOrEmpty(ww.GiftCardId)
                                                        && !string.IsNullOrEmpty(ww.PoinsOrderId)).ToList();
                        if (lstGCSell != null && lstGCSell.Any())
                        {
                            // Header
                            ws.Range("A4:K6").Style.Font.SetBold(true);
                            // Set color
                            ws.Range("A5:K5").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A5:K5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Row(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            // Set Border        
                            ws.Range("A1:K3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A1:K3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            if (_firstStore)
                            {
                                ws.Range("A4:K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                ws.Range("A4:K" + (index - 1)).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            }
                            else
                            {
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                ws.Range("A" + (indexNextStore) + ":K" + (index - 1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }
                            // Store name
                            var storeName = store.Name + " in " + store.CompanyName;
                            ws.Range("A" + (index) + ":K" + (index)).Merge().SetValue(String.Format(storeName));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":K" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DAEEF3"));
                            ws.Range("A" + (index) + ":K" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            index++;

                            ws.Cell("A" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Net Sales"));
                            ws.Cell("B" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC Total"));
                            if (GetTaxType(store.Id) == (int)Commons.ETax.AddOn)
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Add on)"));
                            }
                            else
                            {
                                ws.Cell("C" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Total (Inc)"));
                            }
                            ws.Cell("D" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Disc Total"));
                            ws.Cell("E" + index).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Total"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            index++;

                            giftCardSellIncludeSale = 0;
                            // Breakfast
                            if (model.Breakfast)
                            {
                                //// Gift card sell which include sale
                                giftCardSellIncludeSale += lstGCSell.Where(ww => ww.CreatedDate.TimeOfDay >= brearkStart && ww.CreatedDate.TimeOfDay < brearkEnd
                                              && ww.TotalAmount.HasValue && (ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);
                            }
                            // Lunch
                            if (model.Lunch)
                            {
                                //// Gift card sell which include sale
                                giftCardSellIncludeSale += lstGCSell.Where(ww => ww.CreatedDate.TimeOfDay >= lunchStart && ww.CreatedDate.TimeOfDay < lunchEnd
                                              && ww.TotalAmount.HasValue && (ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);
                            }
                            // Dinner
                            if (model.Dinner)
                            {
                                //// Gift card sell which include sale
                                giftCardSellIncludeSale += lstGCSell.Where(ww => (dinnerStart <= dinnerEnd && ww.CreatedDate.TimeOfDay >= dinnerStart && ww.CreatedDate.TimeOfDay < dinnerEnd) // in day
                                              || (dinnerStart > dinnerEnd && (ww.CreatedDate.TimeOfDay >= dinnerStart || ww.CreatedDate.TimeOfDay < dinnerEnd)) // pass day
                                              && ww.TotalAmount.HasValue && (ww.IsIncludeSale.HasValue && ww.IsIncludeSale.Value)).Sum(ss => ss.TotalAmount.Value);
                            }

                            ws.Cell("A" + index).Value = giftCardSellIncludeSale;
                            ws.Cell("B" + index).Value = 0;
                            ws.Cell("C" + index).Value = 0;
                            ws.Cell("D" + index).Value = 0;
                            ws.Cell("E" + index).Value = 0;

                            ws.Range("A" + index + ":E" + index).Style.NumberFormat.Format = "#,##0.00";
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index - 1) + ":E" + (index)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            ws.Range("A" + (index) + ":E" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            index += 2;
                            indexNextStore = index;
                            _firstStore = false;

                        }

                    }
                }

                ws.Columns(1, 3).AdjustToContents();
                // Set Width for Colum 
                ws.Column(4).Width = 20;
                ws.Columns(5, 11).AdjustToContents();
            }

            return wb;
        }
        #endregion

      
    }
}

