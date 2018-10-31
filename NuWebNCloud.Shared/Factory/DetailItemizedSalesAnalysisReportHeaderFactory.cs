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
using System.Data.Entity.SqlServer;
using System.Data.Entity.Core.Objects;
using ClosedXML.Excel;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Shared.Factory
{
    public class DetailItemizedSalesAnalysisReportHeaderFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public DetailItemizedSalesAnalysisReportHeaderFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public bool Insert(List<DetailItemizedSalesAnalysisReportHeaderModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            //_logger.Info("==========================================================");
            //_logger.Info(string.Format("Insert R_DetailItemizedSalesAnalysisReportHeader: StoreId: [{0}]", info.StoreId));
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_DetailItemizedSalesAnalysisReportHeader.Where(ww => ww.StoreId == info.StoreId
                && ww.CreatedDate == info.CreatedDate && ww.ItemId == info.ItemId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Detail Itemized Sales Analysis Report Header data exist");
                    return result;
                }

                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_DetailItemizedSalesAnalysisReportHeader> lstHeader =
                            new List<R_DetailItemizedSalesAnalysisReportHeader>();
                        R_DetailItemizedSalesAnalysisReportHeader itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_DetailItemizedSalesAnalysisReportHeader();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CategoryId = item.CategoryId;
                            itemInsert.CategoryName = item.CategoryName;
                            itemInsert.ItemId = item.ItemId;
                            itemInsert.ItemName = item.ItemName;
                            itemInsert.ItemTypeId = item.ItemTypeId;
                            itemInsert.Qty = item.Qty;
                            itemInsert.Price = item.Price;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.Mode = item.Mode;

                            lstHeader.Add(itemInsert);

                        }
                        cxt.R_DetailItemizedSalesAnalysisReportHeader.AddRange(lstHeader);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Detail Itemized Sales Analysis Report Header data success", lstInfo);
                        //_logger.Info(string.Format("Insert R_DetailItemizedSalesAnalysisReportHeader: StoreId: [{0}] success", info.StoreId));
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Detail Itemized Sales Analysis Report Header data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_DetailItemizedSalesAnalysisReportHeader", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<DetailItemizedSalesAnalysisReportHeaderModels> GetListCategory(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DetailItemizedSalesAnalysisReportHeader
                               where model.ListStores.Contains(tb.StoreId)
                               group tb by new { CategoryId = tb.CategoryId, CategoryName = tb.CategoryName } into g
                               select new DetailItemizedSalesAnalysisReportHeaderModels
                               {
                                   CategoryId = g.Key.CategoryId,
                                   CategoryName = g.Key.CategoryName
                               }).ToList();
                return lstData;
            }
        }

        public List<DetailItemizedSalesAnalysisReportHeaderModels> GetData(BaseReportModel model, List<string> listCategoryIds)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_DetailItemizedSalesAnalysisReportHeader
                               where model.ListStores.Contains(tb.StoreId)
                                     && listCategoryIds.Contains(tb.CategoryId)
                                     && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                               orderby tb.CategoryId, tb.ItemId, tb.CreatedDate
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   CategoryId = tb.CategoryId,
                                   CategoryName = tb.CategoryName,
                                   ItemId = tb.ItemId,
                                   ItemName = tb.ItemName,
                                   CreatedDate = EntityFunctions.TruncateTime(tb.CreatedDate),
                                   Price = tb.Price,
                                   ItemTypeId = tb.ItemTypeId
                               } into g
                               select new DetailItemizedSalesAnalysisReportHeaderModels
                               {
                                   StoreId = g.Key.StoreId,
                                   CategoryId = g.Key.CategoryId,
                                   CategoryName = g.Key.CategoryName,
                                   ItemId = g.Key.ItemId,
                                   ItemName = g.Key.ItemName,
                                   CreatedDate = g.Key.CreatedDate.Value,
                                   Qty = g.Sum(x => x.Qty),
                                   Price = g.Max(x => x.Price),
                                   ItemTypeId = g.Key.ItemTypeId
                               }).ToList();
                return lstData;
            }
        }

        public XLWorkbook ExportExcel(BaseReportModel model, List<DateTime> listDates, List<DetailItemizedSalesAnalysisReportHeaderModels> data, List<StoreModels> lstStore)
        {
            XLWorkbook wb = new XLWorkbook();
            XLColor backgroundTitle = XLColor.FromHtml("#d9d9d9");
            XLColor outsideBorderColor = XLColor.FromHtml("#000000");
            XLColor insideBorderColor = XLColor.FromHtml("#000000");

            //Create worksheet
            IXLWorksheet ws = wb.Worksheets.Add("Detail_Itemized_Sales_Analysis"/*_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Detail_Itemized_Sales_Analysis")*/);

            int startRow = 3;
            int row = startRow;
            int maxCol = 3 + listDates.Count;
            int startCol = 4;
            List<int> listCenterStyles = new List<int>();
            listCenterStyles.Add(1);

            CreateReportHeader(ws, maxCol, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Detail Itemized Sales Analysis"));

            //List<DetailItemizedSalesAnalysisReportHeaderModels> listStores = (from s in data
            //                                                                  group s by s.StoreId into g
            //                                                                  select new DetailItemizedSalesAnalysisReportHeaderModels
            //                                                                  {
            //                                                                      StoreId = g.Key,
            //                                                                  }).ToList();
            if (model.ListStores.Count == 0)
            {
                ws.Cell("A4").Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NO DATA"));
                return wb;
            }
            //table
            row++;
            ws.Cell("A" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"));
            ws.Cell("B" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Total"));
            ws.Cell("C" + row).Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"));
            ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = backgroundTitle;
            ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
            ws.Range(row, 1, row, maxCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 0; i < listDates.Count; i++)
            {
                ws.Cell(row, 4 + i).SetValue(listDates[i]);
                ws.Cell(row, 4 + i).Style.DateFormat.Format = "mm/dd";
            }
            row++;
            string storeName = string.Empty, storeId = string.Empty;
            //table content
            foreach (StoreModels store in lstStore)
            {
                //store name
                storeName = store.Name;
                storeId = store.Id;

                //category name
                List<DetailItemizedSalesAnalysisReportHeaderModels> ListChilds = (from c in data
                                                                                  where c.StoreId == storeId
                                                                                  orderby c.ItemTypeId, c.CategoryName
                                                                                  group c by new
                                                                                  {
                                                                                      CategoryId = c.CategoryId,
                                                                                      CategoryName = c.CategoryName,
                                                                                      ItemTypeId = c.ItemTypeId // trongntn 
                                                                                  } into g
                                                                                  select new DetailItemizedSalesAnalysisReportHeaderModels
                                                                                  {
                                                                                      CategoryId = g.Key.CategoryId,
                                                                                      CategoryName = g.Key.CategoryName,
                                                                                      ItemTypeId = g.Key.ItemTypeId
                                                                                  }).ToList();


                if (ListChilds.Count > 0)
                {
                    ws.Range(row, 1, row, maxCol).Merge().Value = string.Format("{0}", storeName);
                    ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
                    ws.Range(row, 1, row, maxCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    row++;
                }
                foreach (DetailItemizedSalesAnalysisReportHeaderModels cate in ListChilds/*store.ListChilds.OrderBy(c => c.TypeID).ThenBy(c => c.CateName)*/)
                {
                    if (cate.ItemTypeId != 4) // Category for Dish
                    {
                        ws.Range(row, 1, row, maxCol).Merge().Value = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category") + ": {0}", cate.CategoryName);
                        ws.Range(row, 1, row, maxCol).Style.Font.SetBold(true);
                        ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
                        row++;
                    }

                    //=====
                    List<DetailItemizedSalesAnalysisReportHeaderModels> ListdisSet = (from ds in data
                                                                                      where ds.StoreId == storeId && ds.CategoryId == cate.CategoryId
                                                                                      orderby ds.ItemTypeId, ds.ItemName
                                                                                      group ds by new
                                                                                      {
                                                                                          ItemId = ds.ItemId,
                                                                                          ItemName = ds.ItemName,
                                                                                          Price = ds.Price,
                                                                                          ItemTypeId = ds.ItemTypeId
                                                                                      } into g
                                                                                      select new DetailItemizedSalesAnalysisReportHeaderModels
                                                                                      {
                                                                                          ItemId = g.Key.ItemId,
                                                                                          ItemName = g.Key.ItemName,
                                                                                          Qty = g.Sum(x => x.Qty),
                                                                                          Price = g.Key.Price,
                                                                                          ItemTypeId = g.Key.ItemTypeId
                                                                                      }).ToList();
                    foreach (DetailItemizedSalesAnalysisReportHeaderModels disSet in ListdisSet/*cate.ListChilds.OrderBy(c => c.TypeID).ThenBy(c => c.ItemName)*/)
                    {
                        //dish or setmenu data
                        if (disSet.ItemTypeId == 4)
                        {
                            //set font and background color for row setmenu
                            row++;
                            ws.Row(row).Style.Font.SetFontColor(XLColor.Red);
                            ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
                        }

                        //item name
                        ws.Cell(row, 1).Value = string.Format("{0}", disSet.ItemName);
                        ws.Range(row, 1, row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        //item price
                        ws.Cell(row, 3).SetValue(string.Format("${0:0.00}", disSet.Price));
                        ws.Range(row, 3, row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        double total = 0;
                        //quantity
                        List<DetailItemizedSalesAnalysisReportHeaderModels> ListQty = (from q in data
                                                                                       where q.StoreId == storeId
                                                                                             && q.CategoryId == cate.CategoryId
                                                                                             && q.ItemId == disSet.ItemId
                                                                                             && q.Price == disSet.Price
                                                                                       orderby q.CreatedDate
                                                                                       select new DetailItemizedSalesAnalysisReportHeaderModels
                                                                                       {
                                                                                           CreatedDate = q.CreatedDate,
                                                                                           Qty = q.Qty
                                                                                       }).ToList();
                        foreach (DetailItemizedSalesAnalysisReportHeaderModels dateQty in ListQty/*disSet.ListChilds.OrderBy(item => item.DateCreated)*/)
                        {
                            ws.Cell(row, startCol + GetDateIndex(listDates, dateQty.CreatedDate)).Value = string.Format("{0}", dateQty.Qty);
                            ws.Range(row, 1, row, startCol + GetDateIndex(listDates, dateQty.CreatedDate)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            total += dateQty.Qty;
                        }
                        //total
                        ws.Cell(row, 2).Value = string.Format("{0}", total);
                        ws.Range(row, 1, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        row++;

                        //if set menu has child -> setmenu child
                        List<DetailItemizedSalesAnalysisReportHeaderModels> ListSetChilds = (from sc in data
                                                                                             where sc.StoreId == storeId
                                                                                                   && sc.CategoryId == cate.CategoryId
                                                                                                   && sc.ItemId == disSet.ItemId
                                                                                                   && sc.ItemTypeId == 4
                                                                                             orderby sc.CategoryName, sc.ItemName
                                                                                             select new DetailItemizedSalesAnalysisReportHeaderModels
                                                                                             {
                                                                                                 ItemName = sc.ItemName,
                                                                                                 Price = sc.Price
                                                                                             }).ToList();

                        if (disSet.ItemTypeId == 4 && /*disSet.ListSetChilds*/ ListSetChilds != null && /*disSet.ListSetChilds.Count*/ListSetChilds.Count > 0)
                        {
                            foreach (DetailItemizedSalesAnalysisReportHeaderModels setChild in ListSetChilds/*disSet.ListSetChilds.OrderBy(c => c.CateName).ThenBy(c => c.ItemName)*/)
                            {
                                ws.Cell(row, 1).Value = string.Format("{0}", setChild.ItemName);

                                ws.Cell(row, 3).SetValue(string.Format("${0:0.00}", setChild.Price));
                                total = 0;
                                //=====
                                List<DetailItemizedSalesAnalysisReportHeaderModels> scListChilds = (from c in data
                                                                                                    where c.StoreId == storeId
                                                                                                          && c.CategoryId == cate.CategoryId
                                                                                                          && c.ItemId == disSet.ItemId
                                                                                                          && c.ItemName == setChild.ItemName //
                                                                                                          && c.ItemTypeId == 4
                                                                                                    orderby c.CreatedDate
                                                                                                    select new DetailItemizedSalesAnalysisReportHeaderModels
                                                                                                    {
                                                                                                        CreatedDate = c.CreatedDate,
                                                                                                        Qty = c.Qty
                                                                                                    }).ToList();

                                foreach (DetailItemizedSalesAnalysisReportHeaderModels dateQty in scListChilds/*setChild.ListChilds.OrderBy(item => item.DateCreated)*/)
                                {
                                    ws.Cell(row, startCol + GetDateIndex(listDates, dateQty.CreatedDate)).Value = string.Format("{0}", dateQty.Qty);
                                    total += dateQty.Qty;
                                }
                                ws.Cell(row, 2).Value = string.Format("{0}", total);
                                ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = XLColor.FromHtml("#d9d9d9");
                                row++;
                            }
                        }
                    }
                }
            }

            ws.Range(startRow, 1, row - 1, maxCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row - 1, maxCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(startRow, 1, row - 1, maxCol).Style.Border.OutsideBorderColor = outsideBorderColor;
            ws.Range(startRow, 1, row - 1, maxCol).Style.Border.InsideBorderColor = insideBorderColor;

            //set width of cells
            ws.Column(1).Width = 35;
            ws.Columns(2, maxCol).Width = 10;

            return wb;
        }

        private int GetDateIndex(List<DateTime> listDate, DateTime date)
        {
            return listDate.IndexOf(date);
        }
    }
}
