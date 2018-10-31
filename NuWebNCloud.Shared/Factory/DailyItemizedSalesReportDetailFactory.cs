using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Data.Models;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory
{
    public class DailyItemizedSalesReportDetailFactory : ReportFactory
    {
        private BaseFactory _baseFactory = null;
        public DailyItemizedSalesReportDetailFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public bool Insert(List<DailyItemizedSalesReportDetailPushDataModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();

            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_DailyItemizedSalesReportDetail.Where(ww => ww.StoreId == info.StoreId && ww.BusinessId == info.BusinessId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert daily itemsale detail data exist", lstInfo);
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_DailyItemizedSalesReportDetail> lstInsert = new List<R_DailyItemizedSalesReportDetail>();
                        R_DailyItemizedSalesReportDetail itemInsert = null;

                        List<R_DailyItemizedSalesReportDetailForSet> lstInsertForSet = new List<R_DailyItemizedSalesReportDetailForSet>();
                        R_DailyItemizedSalesReportDetailForSet itemInsertForSet = null;
                        //var business = lstInfo.FirstOrDefault();
                        foreach (var parentItem in lstInfo)
                        {
                            foreach (var item in parentItem.ListDailyItemizedSales)
                            {
                                itemInsert = new R_DailyItemizedSalesReportDetail();
                                itemInsert.Id = Guid.NewGuid().ToString();
                                itemInsert.StoreId = item.StoreId;
                                itemInsert.BusinessId = item.BusinessId;
                                itemInsert.ItemId = item.ItemId;
                                itemInsert.ItemCode = item.ItemCode;
                                itemInsert.ItemName = item.ItemName;
                                itemInsert.ItemTypeId = item.ItemTypeId;
                                itemInsert.CategoryId = item.CategoryId;
                                itemInsert.CategoryName = item.CategoryName;
                                itemInsert.CategoryTypeId = item.CategoryTypeId;
                                itemInsert.Price = item.Price;
                                itemInsert.Quantity = item.Quantity;
                                itemInsert.CreatedDate = item.CreatedDate;
                                itemInsert.Mode = item.Mode;

                                lstInsert.Add(itemInsert);
                            }
                            foreach (var item in parentItem.ListDailyItemizedSalesForSet)
                            {
                                itemInsertForSet = new R_DailyItemizedSalesReportDetailForSet();
                                itemInsertForSet.Id = Guid.NewGuid().ToString();
                                itemInsertForSet.StoreId = item.StoreId;
                                itemInsertForSet.BusinessId = item.BusinessId;
                                itemInsertForSet.CategoryId = item.CategoryId;
                                itemInsertForSet.CategoryName = item.CategoryName;
                                itemInsertForSet.CategoryTypeId = item.CategoryTypeId;
                                itemInsertForSet.Price = item.Price;
                                itemInsertForSet.Quantity = item.Quantity;
                                itemInsertForSet.CreatedDate = item.CreatedDate;
                                itemInsertForSet.Mode = item.Mode;

                                lstInsertForSet.Add(itemInsertForSet);
                            }
                        }

                        cxt.R_DailyItemizedSalesReportDetail.AddRange(lstInsert);
                        cxt.R_DailyItemizedSalesReportDetailForSet.AddRange(lstInsertForSet);
                        cxt.SaveChanges();

                        transaction.Commit();

                        NSLog.Logger.Info("Insert daily itemsale detail data success", lstInfo);

                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert daily itemsale detail data fail", lstInfo);
                        NSLog.Logger.Error("Insert daily itemsale detail data fail", ex);
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
            //_baseFactory.InsertTrackingLog("G_BusinessDay", jsonContent, info.StoreId.ToString(), result);
            return result;
        }

        public DailyItemizedSalesReportDetailPushDataModels GetReportDays(BaseReportModel model, List<string> lstBusDayId)
        {
            DailyItemizedSalesReportDetailPushDataModels objResult = new DailyItemizedSalesReportDetailPushDataModels();
            using (var cxt = new NuWebContext())
            {
                var queryDish = cxt.R_DailyItemizedSalesReportDetail.Where(x => model.ListStores.Contains(x.StoreId) 
                                    && lstBusDayId.Contains(x.BusinessId))
                                                                .Select(x => new DailyItemizedSalesReportDetailModels
                                                                {
                                                                    CategoryId = x.CategoryId,

                                                                    CategoryName = x.CategoryName,
                                                                    ItemCode = x.ItemCode,
                                                                    ItemId = x.ItemId,
                                                                    ItemName = x.ItemName,
                                                                    StoreId = x.StoreId,
                                                                    CreatedDate = x.CreatedDate,
                                                                    BusinessId = x.BusinessId,
                                                                    Quantity = x.Quantity,
                                                                    Price = x.Price,
                                                                    ItemTypeId = (int)Commons.EProductType.Dish,// dish
                                                                    CategoryTypeId = x.CategoryTypeId
                                                                })
                                                                .OrderBy(x => x.CategoryId)
                                                                .ToList();


                var querySet = cxt.R_DailyItemizedSalesReportDetailForSet
                                        .Where(x => model.ListStores.Contains(x.StoreId) 
                                                    && lstBusDayId.Contains(x.BusinessId))
                                        .Select(x => new DailyItemizedSalesReportDetailForSetModels
                                        {
                                            CategoryId = x.CategoryId,
                                            CategoryName = x.CategoryName,
                                            StoreId = x.StoreId,
                                            CreatedDate = x.CreatedDate,
                                            Quantity = x.Quantity,
                                            Price = x.Price,
                                            BusinessId = x.BusinessId,
                                            CategoryTypeId = x.CategoryTypeId
                                        })
                                        .ToList();

                switch (model.FilterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        if (queryDish != null && queryDish.Any())
                        {
                            queryDish = queryDish.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        }
                        if (querySet != null && querySet.Any())
                        {
                            querySet = querySet.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        }
                        break;
                    case (int)Commons.EFilterType.Days:
                        if (queryDish != null && queryDish.Any())
                        {
                            queryDish = queryDish.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        }
                        if (querySet != null && querySet.Any())
                        {
                            querySet = querySet.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                        }
                        break;
                }

                objResult.ListDailyItemizedSales = queryDish;
                objResult.ListDailyItemizedSalesForSet = querySet;
            }
            return objResult;
        }


        public DailyItemizedSalesReportDetailPushDataModels GetReportDays_v2(DateTime dFrom, DateTime dTo, List<string> lstStoreId, int mode, ref DateTime dToBD)
        {
            DailyItemizedSalesReportDetailPushDataModels objReturn = new DailyItemizedSalesReportDetailPushDataModels();
            using (var cxt = new NuWebContext())
            {
                DateTime dToFilter = dTo;
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(dFrom, dTo, lstStoreId, mode);
                if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
                {
                    return null;
                }
                //if (_lstBusDayAllStore != null && _lstBusDayAllStore.Count >0)
                //{
                dTo = _lstBusDayAllStore.Max(aa => aa.DateTo);
                dToBD = dTo;
                //}

                var queryDish = cxt.R_DailyItemizedSalesReportDetail.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(dFrom)
                && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(dTo)
                && lstStoreId.Contains(x.StoreId))
                                                                .Select(x => new DailyItemizedSalesReportDetailModels
                                                                {
                                                                    CategoryId = x.CategoryId,

                                                                    CategoryName = x.CategoryName,
                                                                    ItemCode = x.ItemCode,
                                                                    ItemId = x.ItemId,
                                                                    ItemName = x.ItemName,
                                                                    StoreId = x.StoreId,
                                                                    CreatedDate = x.CreatedDate,
                                                                    BusinessId = x.BusinessId,
                                                                    Quantity = x.Quantity,
                                                                    Price = x.Price,
                                                                    ItemTypeId = (int)Commons.EProductType.Dish,// dish
                                                                    CategoryTypeId = x.CategoryTypeId
                                                                })
                                                                .OrderBy(x => x.CategoryId)
                                                                .ToList();


                var querySet = cxt.R_DailyItemizedSalesReportDetailForSet
                                        .Where(x => (DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(dFrom)
                                                    && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(dTo))
                                                     && lstStoreId.Contains(x.StoreId)
                                                    )
                                        .Select(x => new DailyItemizedSalesReportDetailForSetModels
                                        {
                                            CategoryId = x.CategoryId,
                                            CategoryName = x.CategoryName,
                                            StoreId = x.StoreId,
                                            CreatedDate = x.CreatedDate,
                                            Quantity = x.Quantity,
                                            Price = x.Price,
                                            BusinessId = x.BusinessId,
                                            CategoryTypeId = x.CategoryTypeId
                                        })
                                        .ToList();

                //var _lstBusDayAllStore = _baseFactory.GetBusinessDays(dFrom,dTo, lstStoreId,mode);
                var mListDish = new List<DailyItemizedSalesReportDetailModels>();
                var mListForSet = new List<DailyItemizedSalesReportDetailForSetModels>();
                for (var i = 0; i < lstStoreId.Count; i++)
                {
                    var days = _lstBusDayAllStore.Where(ww => ww.StoreId == lstStoreId[i])
                                                .ToList();
                    if (days != null && days.Count > 0)
                    {
                        var m_FromDay = days.Min(x => x.DateFrom);
                        var m_ToDay = days.Max(x => x.DateTo);

                        var _dish = queryDish.Where(x => x.StoreId == lstStoreId[i]
                                                     && x.CreatedDate >= m_FromDay && x.CreatedDate <= m_ToDay).ToList();

                        var _forSet = querySet.Where(x => x.StoreId == lstStoreId[i]
                                                            && x.CreatedDate >= m_FromDay && x.CreatedDate <= m_ToDay).ToList();
                        mListDish.AddRange(_dish);
                        mListForSet.AddRange(_forSet);
                    }
                }

                objReturn.ListDailyItemizedSales = mListDish;
                objReturn.ListDailyItemizedSalesForSet = mListForSet;
            }
            return objReturn;
        }


        public XLWorkbook ExportExcel(DailyItemizedSalesReportDetailPushDataModels objData, BaseReportModel viewmodel
           , List<StoreModels> lstStores, List<DiscountAndMiscReportModels> listMisc_Discout, DateTime dToInBD)
        {
            const string sheetName = "Daily_Detail_Itemized_Sales";
            var wb = new XLWorkbook();
            ClosedXML.Excel.XLColor backgroundTitle = ClosedXML.Excel.XLColor.FromHtml("#D9D9D9");
            ClosedXML.Excel.XLColor outsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#000000");
            ClosedXML.Excel.XLColor insideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#000000");
            // Create worksheet
            var ws = wb.Worksheets.Add(sheetName);
            var mColumns = new List<string>();

            var mListDate = new List<DateTime>();
            List<int> listCenterStyles = new List<int>();

            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            for (DateTime i = viewmodel.FromDate; i < dToInBD; i = i.AddDays(1))
            {
                mListDate.Add(i);
            }
            int startRow = 5;
            int row = startRow;
            int maxCol = 3 + mListDate.Count;
            int startCol = 4;
            // Merchant Name
            string merchantName = Commons.MerchantName;
            ws.Cell(1, 1).Value = merchantName;
            ws.Range(1, 1, 1, maxCol).Merge();
            ws.Range(1, 1, 1, maxCol).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(1, 1, 1, 1).Style.Font.SetBold(true);

            ws.Range(2, 1, 2, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Detail Itemized Sales Analysis Report"));
            ws.Range(2, 1, 2, maxCol).Style.Font.FontSize = 15;
            ws.Range(2, 1, 2, maxCol).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(2, 1, 2, 1).Style.Font.SetBold(true);
            listCenterStyles.Add(1);
            //Generated on
            ws.Range(3, 1, 3, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time: From") + " {0} " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " {1}", viewmodel.FromDate.ToString("HH:mm"), viewmodel.ToDate.ToString("HH:mm"));
            ws.Range(4, 1, 4, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date: From") + " {0} " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " {1}", viewmodel.FromDate.ToString("dd/MM/yyyy"), viewmodel.ToDate.ToString("dd/MM/yyyy"));
            if (lstStores.Count == 0)
            {
                ws.Cell("A3").Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NO DATA"));
                return wb;
            }

            //table
            //table header
            ws.Cell("A" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
            ws.Cell("B" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
            ws.Cell("C" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = backgroundTitle;
            ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);

            for (int i = 0; i < mListDate.Count; i++)
            {
                ws.Cell(row, 4 + i).SetValue(mListDate[i]);
                ws.Cell(row, 4 + i).Style.DateFormat.Format = "dd/mm";
            }
            row++;

            //table content
            var lstData = objData.ListDailyItemizedSales;
            var lstItemGroupByStore = lstData.GroupBy(gg => new { gg.StoreId });
            foreach (var itemGroupStore in lstItemGroupByStore)
            {
                //Store name
                var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                ws.Range(row, 1, row, maxCol).Merge().Value = string.Format("{0}", storeName);
                ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
                row++;

                //Category name of dish
                var lstItems = lstData.Where(x => x.StoreId == itemGroupStore.Key.StoreId).ToList();

                #region "Dish"
                //group cate of dish
                var lstItemGroupCates = lstItems
                                                .Where(x => x.CategoryTypeId == (int)Commons.EProductType.Dish)
                                                .OrderBy(x => x.CategoryName)
                                                .GroupBy(gg => new { gg.CategoryName, gg.CategoryId, gg.CategoryTypeId })
                                                .ToList();
                foreach (var itemCate in lstItemGroupCates)
                {
                    ws.Range(row, 1, row, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + ": {0}", itemCate.Key.CategoryName);
                    ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
                    ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#EEECE1");
                    row++;
                    // List Item of category 
                    var lstCateItems = lstData.Where(x => x.CategoryId == itemCate.Key.CategoryId && (x.ItemTypeId == (int)Commons.EProductType.Dish))
                                                .OrderBy(x => x.ItemName)
                                                .GroupBy(x => new { x.ItemName, x.ItemId, x.Price })
                                                .Select(x => new
                                                {
                                                    Quantity = x.Sum(y => y.Quantity),
                                                    x.Key.ItemId,
                                                    x.Key.ItemName,
                                                    x.Key.Price,
                                                })
                                                .ToList();
                    var lstItem = lstData.Where(x => x.CategoryId == itemCate.Key.CategoryId && (x.ItemTypeId == (int)Commons.EProductType.Dish))
                                          .OrderBy(x => x.ItemName)
                                          .Select(x => new
                                          {
                                              CreatedDate = new DateTime(x.CreatedDate.Year, x.CreatedDate.Month, x.CreatedDate.Day, 0, 0, 0),
                                              Quantity = x.Quantity,
                                              ItemId = x.ItemId,
                                              Price = x.Price
                                          })
                                          .ToList();
                    foreach (var item in lstCateItems)
                    {
                        // item name 
                        ws.Cell(row, 1).Value = string.Format("{0}", item.ItemName);
                        //Total Quantity
                        ws.Cell(row, 2).Value = string.Format("{0}", item.Quantity);
                        //item price
                        ws.Cell(row, 3).Value = string.Format("{0:0.00}", item.Price);
                        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                        for (var i = 0; i < mListDate.Count; i++)
                        {
                            double _Quantity = 0;

                            var _data = lstItem.Where(x => x.CreatedDate == new DateTime(mListDate[i].Year, mListDate[i].Month, mListDate[i].Day, 0, 0, 0)
                                                            && x.ItemId == item.ItemId && x.Price == item.Price)
                                                            .GroupBy(x => x.CreatedDate)
                                                            .Select(x => new
                                                            {
                                                                Quantity = x.Sum(y => y.Quantity)
                                                            })
                                                            .FirstOrDefault();
                            if (_data != null)
                                _Quantity = _data.Quantity;
                            ws.Cell(row, startCol + i).Value = string.Format("{0}", _Quantity == 0 ? "" : _Quantity.ToString());
                        }

                        row++;
                    }
                }
                #endregion

                #region "For Set"
                var lstItemGroupCatesOfSet = objData.ListDailyItemizedSalesForSet
                                                .Where(x => x.CategoryTypeId == (int)Commons.EProductType.SetMenu
                                                        && x.StoreId == itemGroupStore.Key.StoreId
                                                        )
                                                .OrderBy(x => x.CategoryName)
                                                .GroupBy(gg => new
                                                {
                                                    gg.CategoryId,
                                                    gg.CategoryName,
                                                    gg.CategoryTypeId,
                                                    // gg.BusinessId,
                                                    gg.Price,
                                                    gg.StoreId
                                                }).Select(z => new
                                                {
                                                    CategoryId = z.Key.CategoryId,
                                                    CategoryName = z.Key.CategoryName,
                                                    CategoryTypeId = z.Key.CategoryTypeId,
                                                    QuantityForSet = z.Sum(y => y.Quantity),
                                                    PriceForSet = z.Key.Price,
                                                    //BusinessId = z.Key.BusinessId,
                                                    StoreId = z.Key.StoreId,
                                                })
                                                .ToList();

                foreach (var itemCateOfSet in lstItemGroupCatesOfSet)
                {
                    row++;
                    //Row SET
                    ws.Cell(row, 1).Value = string.Format("{0}", itemCateOfSet.CategoryName);
                    ws.Cell(row, 2).Value = string.Format("{0}", itemCateOfSet.QuantityForSet);
                    ws.Cell(row, 3).Value = string.Format("{0:0.00}", itemCateOfSet.PriceForSet);
                    ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                    ws.Row(row).Style.Font.SetFontColor(ClosedXML.Excel.XLColor.Red);
                    ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#EEECE1");
                    var lstItemForSet = objData.ListDailyItemizedSalesForSet.Where(x => x.CategoryId == itemCateOfSet.CategoryId
                                                             //&& x.BusinessId == itemCateOfSet.BusinessId
                                                             && x.StoreId == itemCateOfSet.StoreId
                                                            && x.CategoryTypeId == (int)Commons.EProductType.SetMenu
                                                            )
                                      .OrderBy(x => x.CategoryName)
                                      .Select(x => new
                                      {
                                          CreatedDate = new DateTime(x.CreatedDate.Year, x.CreatedDate.Month, x.CreatedDate.Day, 0, 0, 0),
                                          Quantity = x.Quantity,
                                          CategoryId = x.CategoryId,
                                          Price = x.Price
                                      })
                                      .ToList();

                    for (var i = 0; i < mListDate.Count; i++)
                    {
                        double _Quantity = 0;

                        var _data = lstItemForSet.Where(x => x.CreatedDate == new DateTime(mListDate[i].Year, mListDate[i].Month, mListDate[i].Day, 0, 0, 0)
                                                            && x.CategoryId == itemCateOfSet.CategoryId && x.Price == itemCateOfSet.PriceForSet)
                                                            .GroupBy(x => x.CreatedDate)
                                                            .Select(x => new
                                                            {
                                                                Quantity = x.Sum(y => y.Quantity)
                                                            })
                                                            .FirstOrDefault();
                        if (_data != null)
                            _Quantity = _data.Quantity;
                        ws.Cell(row, startCol + i).Value = string.Format("{0}", _Quantity == 0 ? "" : _Quantity.ToString());
                    }
                    row++;

                    // Row dish of set
                    var lstCateItems = lstData.Where(x => x.CategoryId == itemCateOfSet.CategoryId
                                                    // && x.BusinessId == itemCateOfSet.BusinessId
                                                    && x.StoreId == itemCateOfSet.StoreId
                                                    && x.CategoryTypeId == (int)Commons.EProductType.SetMenu
                                                    )
                                           .OrderBy(x => x.ItemName)
                                           .GroupBy(x => new { x.ItemName, x.ItemId, x.Price })
                                           .Select(x => new
                                           {
                                               Quantity = x.Sum(y => y.Quantity),
                                               x.Key.ItemId,
                                               x.Key.ItemName,
                                               x.Key.Price,
                                           }).ToList();


                    var lstItem = lstData.Where(x => x.CategoryId == itemCateOfSet.CategoryId
                                                    // && x.BusinessId == itemCateOfSet.BusinessId
                                                    && x.StoreId == itemCateOfSet.StoreId
                                                    && x.CategoryTypeId == (int)Commons.EProductType.SetMenu
                                                    )
                                          .OrderBy(x => x.ItemName)
                                      .Select(x => new
                                      {
                                          CreatedDate = new DateTime(x.CreatedDate.Year, x.CreatedDate.Month, x.CreatedDate.Day, 0, 0, 0),
                                          Quantity = x.Quantity,
                                          ItemId = x.ItemId,
                                          Price = x.Price
                                      })
                                      .ToList();

                    foreach (var item in lstCateItems)
                    {
                        // item name 
                        ws.Cell(row, 1).Value = string.Format("{0}", item.ItemName);
                        //Total Quantity
                        ws.Cell(row, 2).Value = string.Format("{0}", item.Quantity);
                        //item price
                        ws.Cell(row, 3).Value = string.Format("{0:0.00}", item.Price);
                        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                        for (var i = 0; i < mListDate.Count; i++)
                        {
                            double _Quantity = 0;

                            var _data = lstItem.Where(x => x.CreatedDate == new DateTime(mListDate[i].Year, mListDate[i].Month, mListDate[i].Day, 0, 0, 0)
                                                            && x.ItemId == item.ItemId && x.Price == item.Price)
                                                            .GroupBy(x => x.CreatedDate)
                                                            .Select(x => new
                                                            {
                                                                Quantity = x.Sum(y => y.Quantity)
                                                            })
                                                            .FirstOrDefault();
                            if (_data != null)
                                _Quantity = _data.Quantity;
                            ws.Cell(row, startCol + i).Value = string.Format("{0}", _Quantity == 0 ? "" : _Quantity.ToString());
                        }

                        row++;
                    }
                }
                #endregion
            }

            ws.Range(3, 1, row - 1, maxCol).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.OutsideBorderColor = outsideBorderColor;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.InsideBorderColor = insideBorderColor;
            ws.Range(startRow, 1, row - 1, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            //set width of cells
            ws.Column(1).Width = 40;
            //ws.Columns(2, maxCol).Width = 10;

            //specified format
            //column A are left alignment
            ws.Column(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(1, 1, 1, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(2, 1, 2, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            //Generate On... is right alignment
            ws.Range(3, 1, 3, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            ws.Range(4, 1, 4, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

            ws.Columns().AdjustToContents();
            return wb;
        }

        public XLWorkbook ExportExcel_WithFilterTime(BaseReportModel viewmodel, List<StoreModels> lstStores)
        {
            const string sheetName = "Daily_Detail_Itemized_Sales";
            var wb = new XLWorkbook();
            ClosedXML.Excel.XLColor backgroundTitle = ClosedXML.Excel.XLColor.FromHtml("#D9D9D9");
            ClosedXML.Excel.XLColor outsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#000000");
            ClosedXML.Excel.XLColor insideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#000000");
            // Create worksheet
            var ws = wb.Worksheets.Add(sheetName);

            List<int> listCenterStyles = new List<int>();
            int maxCol = 3;

            // Header report
            // merchant name
            string merchantName = Commons.MerchantName;

            if (lstStores == null || !lstStores.Any())
            {
                CreateHeaderForReport(viewmodel,merchantName, maxCol, ref listCenterStyles, ref ws);
                return wb;
            }

            // Get business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(viewmodel.FromDate, viewmodel.ToDate, viewmodel.ListStores, viewmodel.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                CreateHeaderForReport(viewmodel, merchantName, maxCol, ref listCenterStyles, ref ws);
                return wb;
            }
            viewmodel.FromDate = _lstBusDayAllStore.Min(m => m.DateFrom);
            viewmodel.ToDate = _lstBusDayAllStore.Max(m => m.DateTo);

            var mColumns = new List<string>();

            var mListDate = new List<DateTime>();

            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));

            for (DateTime i = viewmodel.FromDate; i < viewmodel.ToDate; i = i.AddDays(1))
                {
                mListDate.Add(i);
            }
            int startRow = 5;
            int row = startRow;
             maxCol += mListDate.Count;
            int startCol = 4;
            CreateHeaderForReport(viewmodel, merchantName, maxCol, ref listCenterStyles, ref ws);

            // Update format header report
            ws.Range(1, 1, 1, maxCol).Merge();
            ws.Range(2, 1, 2, maxCol).Merge();
            ws.Range(3, 1, 3, maxCol).Merge();
            ws.Range(4, 1, 4, maxCol).Merge();

            //table
            //table header
            ws.Cell("A" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
            ws.Cell("B" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
            ws.Cell("C" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = backgroundTitle;
            ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);

            for (int i = 0; i < mListDate.Count; i++)
            {
                ws.Cell(row, 4 + i).SetValue(mListDate[i]);
                ws.Cell(row, 4 + i).Style.DateFormat.Format = "dd/mm";
            }
            row++;

            var lstBusDayIdAllStore = _lstBusDayAllStore.Select(ss => ss.Id).ToList();

            //var objData = GetReportDays(viewmodel, lstBusDayIdAllStore);
            var objData = new DailyItemizedSalesReportDetailPushDataResultModels();
            using (var db = new NuWebContext())
            {
                objData = db.GetDataForDailyItemizedSale(new BaseReportDataModel() { ListStores = viewmodel.ListStores,
                    FromDate = viewmodel.FromDate, ToDate = viewmodel.ToDate, Mode = viewmodel.Mode });
            }
            //table content
            var lstData = objData.ListDailyItemizedSales;
            var lstItemGroupByStore = lstData.GroupBy(gg => new { gg.StoreId });
            foreach (var itemGroupStore in lstItemGroupByStore)
            {
                //Store name
                var storeName = lstStores.Where(ww => ww.Id == itemGroupStore.Key.StoreId).Select(ss => ss.Name).FirstOrDefault();
                ws.Range(row, 1, row, maxCol).Merge().Value = string.Format("{0}", storeName);
                ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
                row++;

                //Category name of dish
                var lstItems = lstData.Where(x => x.StoreId == itemGroupStore.Key.StoreId).ToList();

                #region "Dish"
                //group cate of dish
                var lstItemGroupCates = lstItems
                                                .Where(x => x.CategoryTypeId == (int)Commons.EProductType.Dish)
                                                .OrderBy(x => x.CategoryName)
                                                .GroupBy(gg => new { gg.CategoryName, gg.CategoryId, gg.CategoryTypeId })
                                                .ToList();
                foreach (var itemCate in lstItemGroupCates)
                {
                    ws.Range(row, 1, row, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + ": {0}", itemCate.Key.CategoryName);
                    ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
                    ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#EEECE1");
                    row++;
                    // List Item of category 
                    var lstCateItems = lstData.Where(x => x.CategoryId == itemCate.Key.CategoryId && (x.ItemTypeId == (int)Commons.EProductType.Dish))
                                                .OrderBy(x => x.ItemName)
                                                .GroupBy(x => new { x.ItemName, x.ItemId, x.Price })
                                                .Select(x => new
                                                {
                                                    Quantity = x.Sum(y => y.Quantity),
                                                    x.Key.ItemId,
                                                    x.Key.ItemName,
                                                    x.Key.Price,
                                                })
                                                .ToList();
                    var lstItem = lstData.Where(x => x.CategoryId == itemCate.Key.CategoryId && (x.ItemTypeId == (int)Commons.EProductType.Dish))
                                            .OrderBy(x => x.ItemName)
                                            .Select(x => new
                                            {
                                                CreatedDate = new DateTime(x.CreatedDate.Year, x.CreatedDate.Month, x.CreatedDate.Day, 0, 0, 0),
                                                Quantity = x.Quantity,
                                                ItemId = x.ItemId,
                                                Price = x.Price
                                            })
                                            .ToList();
                    foreach (var item in lstCateItems)
                    {
                        // item name 
                        ws.Cell(row, 1).Value = string.Format("{0}", item.ItemName);
                        //Total Quantity
                        ws.Cell(row, 2).Value = string.Format("{0}", item.Quantity);
                        //item price
                        ws.Cell(row, 3).Value = string.Format("{0:0.00}", item.Price);
                        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                        for (var i = 0; i < mListDate.Count; i++)
                        {
                            double _Quantity = 0;

                            var _data = lstItem.Where(x => x.CreatedDate == new DateTime(mListDate[i].Year, mListDate[i].Month, mListDate[i].Day, 0, 0, 0)
                                                            && x.ItemId == item.ItemId && x.Price == item.Price)
                                                            .GroupBy(x => x.CreatedDate)
                                                            .Select(x => new
                                                            {
                                                                Quantity = x.Sum(y => y.Quantity)
                                                            })
                                                            .FirstOrDefault();
                            if (_data != null)
                                _Quantity = _data.Quantity;
                            ws.Cell(row, startCol + i).Value = string.Format("{0}", _Quantity == 0 ? "" : _Quantity.ToString());
                        }

                        row++;
                    }
                }
                #endregion

                #region "For Set"
                var lstItemGroupCatesOfSet = objData.ListDailyItemizedSalesForSet
                                                .Where(x => x.CategoryTypeId == (int)Commons.EProductType.SetMenu
                                                        && x.StoreId == itemGroupStore.Key.StoreId
                                                        )
                                                .OrderBy(x => x.CategoryName)
                                                .GroupBy(gg => new
                                                {
                                                    gg.CategoryId,
                                                    gg.CategoryName,
                                                    gg.CategoryTypeId,
                                                // gg.BusinessId,
                                                gg.Price,
                                                    gg.StoreId
                                                }).Select(z => new
                                                {
                                                    CategoryId = z.Key.CategoryId,
                                                    CategoryName = z.Key.CategoryName,
                                                    CategoryTypeId = z.Key.CategoryTypeId,
                                                    QuantityForSet = z.Sum(y => y.Quantity),
                                                    PriceForSet = z.Key.Price,
                                                //BusinessId = z.Key.BusinessId,
                                                StoreId = z.Key.StoreId,
                                                })
                                                .ToList();

                foreach (var itemCateOfSet in lstItemGroupCatesOfSet)
                {
                    row++;
                    //Row SET
                    ws.Cell(row, 1).Value = string.Format("{0}", itemCateOfSet.CategoryName);
                    ws.Cell(row, 2).Value = string.Format("{0}", itemCateOfSet.QuantityForSet);
                    ws.Cell(row, 3).Value = string.Format("{0:0.00}", itemCateOfSet.PriceForSet);
                    ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                    ws.Row(row).Style.Font.SetFontColor(ClosedXML.Excel.XLColor.Red);
                    ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#EEECE1");
                    var lstItemForSet = objData.ListDailyItemizedSalesForSet.Where(x => x.CategoryId == itemCateOfSet.CategoryId
                                                                //&& x.BusinessId == itemCateOfSet.BusinessId
                                                                && x.StoreId == itemCateOfSet.StoreId
                                                            && x.CategoryTypeId == (int)Commons.EProductType.SetMenu
                                                            )
                                        .OrderBy(x => x.CategoryName)
                                        .Select(x => new
                                        {
                                            CreatedDate = new DateTime(x.CreatedDate.Year, x.CreatedDate.Month, x.CreatedDate.Day, 0, 0, 0),
                                            Quantity = x.Quantity,
                                            CategoryId = x.CategoryId,
                                            Price = x.Price
                                        })
                                        .ToList();

                    for (var i = 0; i < mListDate.Count; i++)
                    {
                        double _Quantity = 0;

                        var _data = lstItemForSet.Where(x => x.CreatedDate == new DateTime(mListDate[i].Year, mListDate[i].Month, mListDate[i].Day, 0, 0, 0)
                                                            && x.CategoryId == itemCateOfSet.CategoryId && x.Price == itemCateOfSet.PriceForSet)
                                                            .GroupBy(x => x.CreatedDate)
                                                            .Select(x => new
                                                            {
                                                                Quantity = x.Sum(y => y.Quantity)
                                                            })
                                                            .FirstOrDefault();
                        if (_data != null)
                            _Quantity = _data.Quantity;
                        ws.Cell(row, startCol + i).Value = string.Format("{0}", _Quantity == 0 ? "" : _Quantity.ToString());
                    }
                    row++;

                    // Row dish of set
                    var lstCateItems = lstData.Where(x => x.CategoryId == itemCateOfSet.CategoryId
                                                    // && x.BusinessId == itemCateOfSet.BusinessId
                                                    && x.StoreId == itemCateOfSet.StoreId
                                                    && x.CategoryTypeId == (int)Commons.EProductType.SetMenu
                                                    )
                                            .OrderBy(x => x.ItemName)
                                            .GroupBy(x => new { x.ItemName, x.ItemId, x.Price })
                                            .Select(x => new
                                            {
                                                Quantity = x.Sum(y => y.Quantity),
                                                x.Key.ItemId,
                                                x.Key.ItemName,
                                                x.Key.Price,
                                            }).ToList();


                    var lstItem = lstData.Where(x => x.CategoryId == itemCateOfSet.CategoryId
                                                    // && x.BusinessId == itemCateOfSet.BusinessId
                                                    && x.StoreId == itemCateOfSet.StoreId
                                                    && x.CategoryTypeId == (int)Commons.EProductType.SetMenu
                                                    )
                                            .OrderBy(x => x.ItemName)
                                        .Select(x => new
                                        {
                                            CreatedDate = new DateTime(x.CreatedDate.Year, x.CreatedDate.Month, x.CreatedDate.Day, 0, 0, 0),
                                            Quantity = x.Quantity,
                                            ItemId = x.ItemId,
                                            Price = x.Price
                                        })
                                        .ToList();

                    foreach (var item in lstCateItems)
                    {
                        // item name 
                        ws.Cell(row, 1).Value = string.Format("{0}", item.ItemName);
                        //Total Quantity
                        ws.Cell(row, 2).Value = string.Format("{0}", item.Quantity);
                        //item price
                        ws.Cell(row, 3).Value = string.Format("{0:0.00}", item.Price);
                        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                        for (var i = 0; i < mListDate.Count; i++)
                        {
                            double _Quantity = 0;

                            var _data = lstItem.Where(x => x.CreatedDate == new DateTime(mListDate[i].Year, mListDate[i].Month, mListDate[i].Day, 0, 0, 0)
                                                            && x.ItemId == item.ItemId && x.Price == item.Price)
                                                            .GroupBy(x => x.CreatedDate)
                                                            .Select(x => new
                                                            {
                                                                Quantity = x.Sum(y => y.Quantity)
                                                            })
                                                            .FirstOrDefault();
                            if (_data != null)
                                _Quantity = _data.Quantity;
                            ws.Cell(row, startCol + i).Value = string.Format("{0}", _Quantity == 0 ? "" : _Quantity.ToString());
                        }

                        row++;
                    }
                }
                #endregion


            }//foreach (var itemGroupStore in lstItemGroupByStore)
            ws.Range(3, 1, row - 1, maxCol).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.OutsideBorderColor = outsideBorderColor;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.InsideBorderColor = insideBorderColor;
            ws.Range(startRow, 1, row - 1, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            //set width of cells
            ws.Column(1).Width = 40;
            //ws.Columns(2, maxCol).Width = 10;

            //specified format
            //column A are left alignment
            ws.Column(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(1, 1, 1, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(2, 1, 2, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            //Generate On... is right alignment
            ws.Range(3, 1, 3, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            ws.Range(4, 1, 4, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

            ws.Columns().AdjustToContents();
            return wb;
        }
        private void CreateHeaderForReport(BaseReportModel viewmodel, string merchantName, int maxCol, ref List<int> listCenterStyles, ref IXLWorksheet ws)
        {
            ClosedXML.Excel.XLColor outsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#000000");
            ClosedXML.Excel.XLColor insideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#000000");

            ws.Cell(1, 1).Value = merchantName;
            ws.Range(1, 1, 1, maxCol).Merge();
            ws.Range(1, 1, 1, maxCol).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(1, 1, 1, 1).Style.Font.SetBold(true);

            ws.Range(2, 1, 2, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Detail Itemized Sales Analysis Report"));
            ws.Range(2, 1, 2, maxCol).Style.Font.FontSize = 15;
            ws.Range(2, 1, 2, maxCol).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(2, 1, 2, 1).Style.Font.SetBold(true);
            listCenterStyles.Add(1);
            // Generated on
            ws.Range(3, 1, 3, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time: From") + " {0} " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " {1}", viewmodel.FromDateFilter.ToString("HH:mm"), viewmodel.ToDateFilter.ToString("HH:mm"));
            ws.Range(4, 1, 4, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date: From") + " {0} " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " {1}", viewmodel.FromDateFilter.ToString("dd/MM/yyyy"), viewmodel.ToDateFilter.ToString("dd/MM/yyyy"));

            ws.Range(1, 1, 4, maxCol).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxCol).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, maxCol).Style.Border.OutsideBorderColor = outsideBorderColor;
            ws.Range(1, 1, 4, maxCol).Style.Border.InsideBorderColor = insideBorderColor;

            //column A are left alignment
            ws.Column(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(1, 1, 1, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(2, 1, 2, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            //Generate On... is right alignment
            ws.Range(3, 1, 3, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            ws.Range(4, 1, 4, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
        }

        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]

        public DailyItemizedSalesReportDetailPushDataModels Getdata_NewDB(BaseReportModel model , List<string> lstBusDayId)
        {
            DailyItemizedSalesReportDetailPushDataModels objResult = new DailyItemizedSalesReportDetailPushDataModels();
            using (var cxt = new NuWebContext())
            {
                var query = (from ps in cxt.R_PosSale
                             from psd in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                             where model.ListStores.Contains(ps.StoreId)
                                 && lstBusDayId.Contains(psd.BusinessId)
                                 && psd.Mode == model.Mode
                             select new DailyItemizedSalesReportDetailModels
                             {
                                 CategoryId = psd.ItemTypeId == (int)Commons.EProductType.SetMenu ? psd.ItemId : psd.CategoryId,
                                 CategoryName = psd.ItemTypeId == (int)Commons.EProductType.SetMenu ? psd.ItemName : psd.CategoryName,
                                 ItemCode = psd.ItemCode,
                                 ItemId = psd.ItemId,
                                 ItemName = psd.ItemName,
                                 StoreId = ps.StoreId,
                                 CreatedDate = ps.ReceiptCreatedDate.Value,
                                 BusinessId = psd.BusinessId,
                                 Quantity = psd.Quantity,
                                 Price = psd.Price,
                                 ItemTypeId = psd.ItemTypeId,
                                 CategoryTypeId = psd.ItemTypeId,
                                 ParentId = psd.ParentId,
                                 OrderDetailId = psd.OrderDetailId
                             }).ToList();

                if (query != null && query.Any())
                {
                    switch (model.FilterType)
                    {
                        case (int)Commons.EFilterType.OnDay:
                            query = query.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                            break;
                        case (int)Commons.EFilterType.Days:
                            query = query.Where(ww => ww.CreatedDate.TimeOfDay >= model.FromDateFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= model.ToDateFilter.TimeOfDay).ToList();
                            break;
                    }

                    objResult.ListDailyItemizedSales = query.Where(w => w.ItemTypeId == (int)Commons.EProductType.Dish).ToList();

                    objResult.ListDailyItemizedSalesForSet = query.Where(w => w.ItemTypeId == (int)Commons.EProductType.SetMenu)
                            .Select(x => new DailyItemizedSalesReportDetailForSetModels
                            {
                                CategoryId = x.CategoryId,
                                CategoryName = x.CategoryName,
                                StoreId = x.StoreId,
                                CreatedDate = x.CreatedDate,
                                Quantity = x.Quantity,
                                Price = x.Price,
                                BusinessId = x.BusinessId,
                                CategoryTypeId = x.CategoryTypeId,
                                OrderDetailId = x.OrderDetailId
                            }).ToList();
                }
            }
            return objResult;
        }

        public XLWorkbook ExportExcel_NewDB(BaseReportModel viewmodel, List<StoreModels> lstStores)
        {
            const string sheetName = "Daily_Detail_Itemized_Sales";
            var wb = new XLWorkbook();
            ClosedXML.Excel.XLColor backgroundTitle = ClosedXML.Excel.XLColor.FromHtml("#D9D9D9");
            ClosedXML.Excel.XLColor outsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#000000");
            ClosedXML.Excel.XLColor insideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#000000");
            // Create worksheet
            var ws = wb.Worksheets.Add(sheetName);
            List<int> listCenterStyles = new List<int>();

            // Hedaer report
            // merchant name
            string merchantName = Commons.MerchantName;
            ws.Cell(1, 1).Value = merchantName;
            ws.Range(1, 1, 1, 8).Merge();
            ws.Range(1, 1, 1, 8).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(1, 1, 1, 1).Style.Font.SetBold(true);

            ws.Range(2, 1, 2, 8).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Daily Detail Itemized Sales Analysis Report"));
            ws.Range(2, 1, 2, 8).Style.Font.FontSize = 15;
            ws.Range(2, 1, 2, 8).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Range(2, 1, 2, 1).Style.Font.SetBold(true);
            listCenterStyles.Add(1);

            // Generated on
            ws.Range(3, 1, 3, 8).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time: From") + " {0} " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " {1}", viewmodel.FromDateFilter.ToString("HH:mm"), viewmodel.ToDateFilter.ToString("HH:mm"));
            ws.Range(4, 1, 4, 8).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date: From") + " {0} " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " {1}", viewmodel.FromDateFilter.ToString("dd/MM/yyyy"), viewmodel.ToDateFilter.ToString("dd/MM/yyyy"));

            ws.Range(1, 1, 4, 8).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 8).Style.Border.OutsideBorderColor = outsideBorderColor;
            ws.Range(1, 1, 4, 8).Style.Border.InsideBorderColor = insideBorderColor;

            //column A are left alignment
            ws.Column(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(1, 1, 1, 8).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(2, 1, 2, 8).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            //Generate On... is right alignment
            ws.Range(3, 1, 3, 8).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            ws.Range(4, 1, 4, 8).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

            if (lstStores == null || !lstStores.Any())
            {
                //ws.Cell("A3").Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NO DATA"));
                return wb;
            }
            //get business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(viewmodel.FromDate, viewmodel.ToDate, viewmodel.ListStores, viewmodel.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return wb;
            }
            viewmodel.FromDate = _lstBusDayAllStore.Min(mm => mm.DateFrom);
            viewmodel.ToDate = _lstBusDayAllStore.Max(mm => mm.DateTo);

            var mListDate = new List<DateTime>();
            var mColumns = new List<string>();

            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
            mColumns.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            for (DateTime i = viewmodel.FromDate; i < viewmodel.ToDate; i = i.AddDays(1))
            {
                mListDate.Add(i);
            }

            int startRow = 5;
            int row = startRow;
            int maxCol = 3 + mListDate.Count;
            int startCol = 4;

            // Update format for header report
            ws.Range(1, 1, 1, maxCol).Merge();
            ws.Range(2, 1, 2, maxCol).Merge();
            ws.Range(3, 1, 3, maxCol).Merge();
            ws.Range(4, 1, 4, maxCol).Merge();

            //table
            //table header
            ws.Cell("A" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
            ws.Cell("B" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
            ws.Cell("C" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = backgroundTitle;
            ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);

            for (int i = 0; i < mListDate.Count; i++)
            {
                ws.Cell(row, 4 + i).SetValue(mListDate[i]);
                ws.Cell(row, 4 + i).Style.DateFormat.Format = "dd/mm";
            }
            row++;
            
            double itmQty = 0;

            var lstBusDayIdAllStore = _lstBusDayAllStore.Select(ss => ss.Id).ToList();
            var objData = Getdata_NewDB(viewmodel, lstBusDayIdAllStore);

            // table content
            var lstDataDish = objData.ListDailyItemizedSales; // Dish
            var lstDataSetMenu = objData.ListDailyItemizedSalesForSet; // SetMenu

            var lstStoreId = lstDataDish.Select(ss => ss.StoreId).Distinct().ToList();
            lstStores = lstStores.Where(ww => lstStoreId.Contains(ww.Id)).OrderBy(oo => oo.Name).ToList();

            foreach (var store in lstStores)
            {
                // Store name

                ws.Range(row, 1, row, maxCol).Merge().Value = string.Format("{0}", store.Name);
                ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
                row++;

                // Category name of dish
                var lstItmDishStore = lstDataDish.Where(x => x.StoreId == store.Id).ToList();

                #region "Dish"
                // group cate of dish
                var lstItemGroupCates = lstItmDishStore.Where(x => string.IsNullOrEmpty(x.ParentId))
                                                .OrderBy(x => x.CategoryName)
                                                .GroupBy(gg => new { gg.CategoryName, gg.CategoryId})
                                                .ToList();

                foreach (var itemCate in lstItemGroupCates)
                {
                    ws.Range(row, 1, row, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + ": {0}", itemCate.Key.CategoryName);
                    ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
                    ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#EEECE1");
                    row++;

                    var lstItmOfCate = itemCate.OrderBy(x => x.ItemName)
                                            .GroupBy(g => new {
                                                ItemId = g.ItemId,
                                                ItemName = g.ItemName,
                                                Price = g.Price
                                            }).Select(x => new
                                            {
                                                Quantity = x.Sum(s => s.Quantity),
                                                ItemId = x.Key.ItemId,
                                                ItemName = x.Key.ItemName,
                                                Price = x.Key.Price
                                            }).ToList();

                    foreach (var item in lstItmOfCate)
                    {
                        // Item name 
                        ws.Cell(row, 1).Value = string.Format("{0}", item.ItemName);

                        // Total Quantity
                        ws.Cell(row, 2).Value = string.Format("{0}", item.Quantity);

                        // Item price
                        ws.Cell(row, 3).Value = string.Format("{0:0.00}", item.Price);
                        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";

                        for (var i = 0; i < mListDate.Count; i++)
                        {
                            itmQty = itemCate.Where(x => x.CreatedDate.Date == mListDate[i].Date
                                                        && x.ItemId == item.ItemId && x.Price == item.Price).Sum(s => s.Quantity);

                            ws.Cell(row, startCol + i).Value = string.Format("{0}", itmQty == 0 ? "" : itmQty.ToString());
                        }

                        row++;
                    }
                }
                #endregion

                #region "For Set"
                var lstItemGroupCatesOfSet = lstDataSetMenu.Where(x => x.StoreId == store.Id)
                                                .OrderBy(x => x.CategoryName)
                                                .GroupBy(gg => new
                                                {
                                                    gg.CategoryId,
                                                    gg.CategoryName,
                                                    gg.Price
                                                }).Select(z => new
                                                {
                                                    CategoryId = z.Key.CategoryId,
                                                    CategoryName = z.Key.CategoryName,
                                                    QuantityForSet = z.Sum(y => y.Quantity),
                                                    PriceForSet = z.Key.Price,
                                                    ListOrderDetailId = z.Select(ss => ss.OrderDetailId).ToList()
                                                }).ToList();

                foreach (var itemCateOfSet in lstItemGroupCatesOfSet)
                {
                    row++;

                    // Row SetMenu
                    ws.Cell(row, 1).Value = string.Format("{0}", itemCateOfSet.CategoryName);
                    ws.Cell(row, 2).Value = string.Format("{0}", itemCateOfSet.QuantityForSet);
                    ws.Cell(row, 3).Value = string.Format("{0:0.00}", itemCateOfSet.PriceForSet);
                    ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                    ws.Row(row).Style.Font.SetFontColor(ClosedXML.Excel.XLColor.Red);
                    ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#EEECE1");

                    var lstItmSetOfCateSet = lstDataSetMenu.Where(x => x.CategoryId == itemCateOfSet.CategoryId && x.StoreId == store.Id 
                                                                    && x.Price == itemCateOfSet.PriceForSet).ToList();

                    for (var i = 0; i < mListDate.Count; i++)
                    {
                        itmQty = lstItmSetOfCateSet.Where(x => x.CreatedDate.Date == mListDate[i].Date).Sum(s => s.Quantity);

                        ws.Cell(row, startCol + i).Value = string.Format("{0}", itmQty == 0 ? "" : itmQty.ToString());
                    }
                    row++;

                    // Row dish of set, ItemTypeId = Dish && ParentId == OrderDetailId of SetMenu
                    var lstItmDishOfCateSet = lstItmDishStore.Where(x => !string.IsNullOrEmpty(x.ParentId) && itemCateOfSet.ListOrderDetailId.Contains(x.ParentId))
                                            .OrderBy(x => x.ItemName)
                                            .GroupBy(x => new { x.ItemName, x.ItemId, x.Price })
                                            .Select(x => new
                                            {
                                                Quantity = x.Sum(y => y.Quantity),
                                                x.Key.ItemId,
                                                x.Key.ItemName,
                                                x.Key.Price,
                                            }).ToList();

                    foreach (var item in lstItmDishOfCateSet)
                    {
                        // item name 
                        ws.Cell(row, 1).Value = string.Format("{0}", item.ItemName);
                        //Total Quantity
                        ws.Cell(row, 2).Value = string.Format("{0}", item.Quantity);
                        //item price
                        ws.Cell(row, 3).Value = string.Format("{0:0.00}", item.Price);
                        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";

                        for (var i = 0; i < mListDate.Count; i++)
                        {

                            itmQty = lstItmDishStore.Where(x => !string.IsNullOrEmpty(x.ParentId) && itemCateOfSet.ListOrderDetailId.Contains(x.ParentId)
                                                                && x.ItemId == item.ItemId && x.Price == item.Price
                                                                && x.CreatedDate.Date == mListDate[i].Date).Sum(s => s.Quantity);

                            ws.Cell(row, startCol + i).Value = string.Format("{0}", itmQty == 0 ? "" : itmQty.ToString());
                        }

                        row++;
                    }
                }
                #endregion


            }//foreach (var itemGroupStore in lstItemGroupByStore)
            ws.Range(3, 1, row - 1, maxCol).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.OutsideBorderColor = outsideBorderColor;
            ws.Range(3, 1, row - 1, maxCol).Style.Border.InsideBorderColor = insideBorderColor;
            ws.Range(startRow, 1, row - 1, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            //set width of cells
            ws.Column(1).Width = 40;

            //specified format
            //column A are left alignment
            ws.Column(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(1, 1, 1, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            ws.Range(2, 1, 2, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            //Generate On... is right alignment
            ws.Range(3, 1, 3, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            ws.Range(4, 1, 4, maxCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

            ws.Columns().AdjustToContents();
            return wb;
        }

        #endregion
    }
}
