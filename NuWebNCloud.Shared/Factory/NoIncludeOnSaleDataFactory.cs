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
using ClosedXML.Excel;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using System.Data.Entity.SqlServer;

namespace NuWebNCloud.Shared.Factory
{
    public class NoIncludeOnSaleDataFactory 
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public NoIncludeOnSaleDataFactory()
        {
        }

        public bool Insert(List<NoIncludeOnSaleDataReportModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_NoIncludeOnSaleDataReport.Where(ww => ww.StoreId == info.StoreId
                        && ww.BusinessId == info.BusinessId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert IncludeOnSale data exist", lstInfo);
                    return result;
                }

                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_NoIncludeOnSaleDataReport> lstInsert = new List<R_NoIncludeOnSaleDataReport>();
                        R_NoIncludeOnSaleDataReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_NoIncludeOnSaleDataReport();
                            itemInsert.OrderId = item.OrderId;
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.OrderId = item.OrderId;
                            itemInsert.ReceiptNo = item.ReceiptNo;
                            itemInsert.BusinessId = item.BusinessId;
                            itemInsert.CategoryId = item.CategoryId;
                            itemInsert.CategoryName = item.CategoryName;
                            itemInsert.ProductId = item.ProductId;
                            itemInsert.ProductName = item.ProductName;
                            itemInsert.Qty = item.Qty;
                            itemInsert.Price = item.Price;
                            itemInsert.Amount = item.Amount;
                            itemInsert.Tax = item.Tax;
                            itemInsert.ServiceCharged = item.ServiceCharged;
                            itemInsert.DiscountAmount = item.DiscountAmount;
                            itemInsert.PromotionAmount = item.PromotionAmount;
                            itemInsert.Mode = item.Mode;
                            itemInsert.GLAccountCode = item.GLAccountCode;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_NoIncludeOnSaleDataReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();
                        NSLog.Logger.Info("Insert IncludeOnSale data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert IncludeOnSale data fail", lstInfo);
                        NSLog.Logger.Error("Insert IncludeOnSale data fail", ex);
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

            return result;
        }

        public List<NoIncludeOnSaleDataReportModels> GetListCateNoIncludeSaleForDailySale(List<string> lstStoreIds, DateTime dFrom, DateTime dTo, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResults = cxt.R_NoIncludeOnSaleDataReport.Where(ww => lstStoreIds.Contains(ww.StoreId) && 
                ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo && ww.Mode == mode)
                    .Select(ss => new NoIncludeOnSaleDataReportModels() {
                        StoreId = ss.StoreId,
                        CreatedDate = ss.CreatedDate,
                        CategoryId = ss.CategoryId, CategoryName = ss.CategoryName,  Amount = ss.Amount
                        , Tax = ss.Tax, DiscountAmount = ss.DiscountAmount, PromotionAmount = ss.PromotionAmount}).ToList();

                return lstResults;
            }
        }
        public List<NoIncludeOnSaleDataReportModels> GetListCateNoIncludeSaleForDailyReceipt(List<string> lstStoreIds, DateTime dFrom, DateTime dTo, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResults = cxt.R_NoIncludeOnSaleDataReport.Where(ww => lstStoreIds.Contains(ww.StoreId) &&
                ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo && ww.Mode == mode)
                    .Select(ss => new NoIncludeOnSaleDataReportModels()
                    {
                        StoreId = ss.StoreId,
                        CreatedDate = ss.CreatedDate,
                        CategoryId = ss.CategoryId,
                        CategoryName = ss.CategoryName,
                        Amount = ss.Amount,
                        Tax = ss.Tax,
                        DiscountAmount = ss.DiscountAmount,
                        PromotionAmount = ss.PromotionAmount,
                        OrderId = ss.OrderId
                    }).ToList();

                return lstResults;
            }
        }
        public List<NoIncludeOnSaleDataReportModels> GetListCateNoIncludeSaleForHourlySale(List<string> lstStoreIds, DateTime dFrom, DateTime dTo, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResults = cxt.R_NoIncludeOnSaleDataReport.Where(ww => lstStoreIds.Contains(ww.StoreId) &&
                ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo && ww.Mode == mode)
                .GroupBy(gg=> new {StoreId = gg.StoreId, Time = (int?)SqlFunctions.DatePart("HH",gg.CreatedDate) })
                    .Select(ss => new NoIncludeOnSaleDataReportModels()
                    {
                        StoreId = ss.Key.StoreId,
                        Time = ss.Key.Time.HasValue? ss.Key.Time.Value:0,
                        Amount = ss.Sum(aa=>aa.Amount),
                        DiscountAmount = ss.Sum(aa=>aa.DiscountAmount),
                        PromotionAmount = ss.Sum(aa => aa.PromotionAmount),
                        Tax = ss.Sum(aa => aa.Tax)
                    }).ToList();

                return lstResults;
            }
        }

        public List<NoIncludeOnSaleDataReportModels> GetListCateNoIncludeSaleForItemSale(List<string> lstStoreIds, DateTime dFrom, DateTime dTo, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResults = cxt.R_NoIncludeOnSaleDataReport.Where(ww => lstStoreIds.Contains(ww.StoreId) &&
                ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo && ww.Mode == mode)
                .GroupBy(gg => new { StoreId = gg.StoreId })
                    .Select(ss => new NoIncludeOnSaleDataReportModels()
                    {
                        StoreId = ss.Key.StoreId,
                        Amount = ss.Sum(aa => aa.Amount),
                        DiscountAmount = ss.Sum(aa => aa.DiscountAmount),
                        PromotionAmount = ss.Sum(aa => aa.PromotionAmount),
                        Tax = ss.Sum(aa => aa.Tax)
                    }).ToList();

                return lstResults;
            }
        }

        #region Report with new DB  from table [R_PosSale], [R_PosSaleDetail]
        public List<NoIncludeOnSaleDataReportModels> GetListCateNoIncludeSaleForHourlySale_NewDB(List<string> lstStoreIds, DateTime dFrom, DateTime dTo, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResults = cxt.R_PosSaleDetail.Where(ww => lstStoreIds.Contains(ww.StoreId) &&
                ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo && ww.Mode == mode)
                .GroupBy(gg => new { StoreId = gg.StoreId, Time = (int?)SqlFunctions.DatePart("HH", gg.CreatedDate) }).ToList()
                    .Select(ss => new NoIncludeOnSaleDataReportModels()
                    {
                        StoreId = ss.Key.StoreId,
                        Time = ss.Key.Time.HasValue ? ss.Key.Time.Value : 0,
                        Amount = ss.Sum(aa => aa.TotalAmount),
                        //DiscountAmount = ss.Where(w => !w.IsDiscountTotal.HasValue || (w.IsDiscountTotal.HasValue && w.IsDiscountTotal.Value == false)).Sum(aa => aa.Discount),
                        //DiscountAmount = ss.Where(w => !w.IsDiscountTotal.HasValue || (w.IsDiscountTotal.HasValue && w.IsDiscountTotal.Value == false)).Sum(aa => (double?)aa.Discount) == null ? 0 : 1,
                        DiscountAmount = ss.Where(w => (!w.IsDiscountTotal.HasValue || (w.IsDiscountTotal.HasValue && w.IsDiscountTotal.Value == false))).Sum(aa => aa.Discount),
                        PromotionAmount = ss.Sum(aa => aa.PromotionAmount),
                        Tax = ss.Sum(aa => aa.Tax)
                    }).ToList();

                return lstResults;
            }
        }
        #endregion
    }
}
