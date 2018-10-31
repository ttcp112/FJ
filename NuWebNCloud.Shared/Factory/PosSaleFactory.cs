using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using ClosedXML.Excel;
using NuWebNCloud.Shared.Utilities;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models.Xero.GenerateInvoice;

namespace NuWebNCloud.Shared.Factory
{
    public class PosSaleFactory : ReportFactory
    {
        private BaseFactory _baseFactory = null;
        private XeroFactory _xeroFactory = null;

        public PosSaleFactory()
        {
            _baseFactory = new BaseFactory();
            _xeroFactory = new XeroFactory();
        }
        public bool Insert(PosSaleReportReturnModels datas)
        {
            List<PosSaleModels> lstPosSale = datas.PosSaleReportDTOs;
            List<PosSaleDetailModels> lstPosSaleDetail = datas.PosSaleDetailReportDTOs;
            bool result = true;
            List<R_PosSale> lstPosSales = new List<R_PosSale>();

            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var storeId = string.Empty;
                        if (lstPosSale != null && lstPosSale.Any())
                        {
                            var infoPosSale = lstPosSale.FirstOrDefault();
                            storeId = infoPosSale.StoreId;
                            var obj = cxt.R_PosSale.Where(ww => ww.BusinessId == infoPosSale.BusinessId
                                    && ww.StoreId == infoPosSale.StoreId).FirstOrDefault();
                            if (obj != null)
                            {
                                NSLog.Logger.Info("Insert PosSale data exist", infoPosSale);
                                //isPosSaleData = false;
                            }
                            else
                            {
                                var lstPosSalesEntity = lstPosSale.Select(ss => new R_PosSale()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    StoreId = ss.StoreId,
                                    BusinessId = ss.BusinessId,
                                    OrderId = ss.OrderId,
                                    OrderNo = ss.OrderNo,
                                    ReceiptNo = ss.ReceiptNo,
                                    CreatedDate = ss.CreatedDate,
                                    ReceiptCreatedDate = ss.ReceiptCreatedDate,
                                    OrderStatus = ss.OrderStatus,
                                    TableNo = ss.TableNo,
                                    NoOfPerson = ss.NoOfPerson,
                                    CancelAmount = ss.CancelAmount,
                                    ReceiptTotal = ss.ReceiptTotal,
                                    Discount = ss.Discount,
                                    Tip = ss.Tip,
                                    PromotionValue = ss.PromotionValue,
                                    ServiceCharge = ss.ServiceCharge,
                                    GST = ss.GST,
                                    Rounding = ss.Rounding,
                                    Refund = ss.Refund,
                                    //NetSales = ss.NetSales,
                                    CashierId = ss.CashierId,
                                    CashierName = ss.CashierName,
                                    Mode = ss.Mode,
                                    CreditNoteNo = ss.CreditNoteNo

                                }).ToList();
                                //add to db
                                cxt.R_PosSale.AddRange(lstPosSalesEntity);
                            }
                        }
                        if (lstPosSaleDetail != null && lstPosSaleDetail.Any())
                        {
                            var infoPosSaleDetail = lstPosSaleDetail.FirstOrDefault();
                            var obj = cxt.R_PosSaleDetail.Where(ww => ww.BusinessId == infoPosSaleDetail.BusinessId
                                    && ww.StoreId == infoPosSaleDetail.StoreId).FirstOrDefault();
                            if (obj != null)
                            {
                                NSLog.Logger.Info("Insert PosSaleDetail data exist", infoPosSaleDetail);
                            }
                            else
                            {
                                var lstPosSaleDetailsEntity = lstPosSaleDetail.Select(ss => new R_PosSaleDetail()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    StoreId = ss.StoreId,
                                    BusinessId = ss.BusinessId,
                                    OrderId = ss.OrderId,
                                    OrderDetailId = ss.OrderDetailId,
                                    ItemId = ss.ItemId,
                                    ItemTypeId = ss.ItemTypeId,
                                    ParentId = ss.ParentId,
                                    ItemCode = ss.ItemCode,
                                    ItemName = ss.ItemName,
                                    CategoryId = ss.CategoryId,
                                    CategoryName = ss.CategoryName,
                                    GLAccountCode = ss.GLAccountCode,
                                    Quantity = ss.Quantity,
                                    Price = ss.Price,
                                    ExtraPrice = ss.ExtraPrice,
                                    TotalAmount = ss.TotalAmount,
                                    Discount = ss.Discount,
                                    Cost = ss.Cost,
                                    ServiceCharge = ss.ServiceCharge,
                                    Tax = ss.Tax,
                                    PromotionAmount = ss.PromotionAmount,
                                    PoinsOrderId = ss.PoinsOrderId,
                                    GiftCardId = ss.GiftCardId,
                                    IsIncludeSale = ss.IsIncludeSale,
                                    CreatedDate = ss.CreatedDate,
                                    Mode = ss.Mode,
                                    IsDiscountTotal = ss.IsDiscountTotal,
                                    CancelUser = ss.CancelUser,
                                    RefundUser = ss.RefundUser,
                                    TaxType = ss.TaxType

                                }).ToList();

                                //add to db
                                cxt.R_PosSaleDetail.AddRange(lstPosSaleDetailsEntity);
                            }
                        }
                        cxt.SaveChanges();
                        transaction.Commit();
                        NSLog.Logger.Info("Insert PosSale data success");
                        //push to xero
                        Task.Run(() => PushInvoiceToXexo(storeId, lstPosSaleDetail, datas.PaymentReportXeroDTOs, datas.BusinessInfo));
   
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert PosSale data fail", ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }// end using transaction

            }//end using context

            return result;
        }

        public bool MergeData(List<string> lstStore, int? month = null, int? year = null)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                var _tableDailySales = cxt.R_DailySalesReport.Select(x => new
                {
                    Id = x.Id,
                    OrderId = x.OrderId,
                    BusinessId = x.BusinessId,
                    StoreId = x.StoreId,
                    NetSales = x.NetSales,
                    Refund = x.Refund,
                    CreatedDate = x.CreatedDate,
                    NoOfPerson = x.NoOfPerson,
                    ReceiptTotal = x.ReceiptTotal,
                    Discount = x.Discount,
                    Tip = x.Tip,
                    PromotionValue = x.PromotionValue,
                    ServiceCharge = x.ServiceCharge,
                    GST = x.GST,
                    Rounding = x.Rounding,
                    Mode = x.Mode
                }).Where(x => x.BusinessId != null
                        && (month.HasValue ? x.CreatedDate.Month == month : 1 == 1)
                        && (year.HasValue ? x.CreatedDate.Year == year : 1 == 1)
                        && lstStore.Contains(x.StoreId))
                /*.AsQueryable()*/.ToList();

                var _tableAuditTrail = cxt.R_AuditTrailReport.Select(x => new
                {
                    Id = x.Id,
                    BusinessDayId = x.BusinessDayId,
                    ReceiptID = x.ReceiptID,
                    ReceiptDate = x.ReceiptDate,
                    OrderNo = x.OrderNo,
                    CancelAmount = x.CancelAmount,
                    CashierId = x.CashierId,
                    CashierName = x.CashierName,
                    StoreId = x.StoreId,
                    OrderStatus = x.ReceiptStatus
                })
                .Where(x => (month.HasValue ? x.ReceiptDate.Month == month : 1 == 1)
                            && (year.HasValue ? x.ReceiptDate.Year == year : 1 == 1)
                            && lstStore.Contains(x.StoreId))
                /*.AsQueryable()*/.ToList();

                var _tableDailyReceipt = cxt.R_DailyReceiptReport.Select(x => new
                {
                    Id = x.Id,
                    BusinessDayId = x.BusinessDayId,
                    CreatedDate = x.CreatedDate,
                    ReceiptNo = x.ReceiptNo,
                    StoreId = x.StoreId,
                    ReceiptId = x.ReceiptId,
                })
               .Where(x => (month.HasValue ? x.CreatedDate.Month == month : 1 == 1)
                            && (year.HasValue ? x.CreatedDate.Year == year : 1 == 1)
                            && lstStore.Contains(x.StoreId))
                /*.AsQueryable()*/.ToList();

                var _tableHourlySales = cxt.R_HourlySalesReport.Select(x => new
                {
                    ReceiptId = x.ReceiptId,
                    CreatedDate = x.CreatedDate,
                    Id = x.Id,
                    StoreId = x.StoreId
                }).Where(x => (month.HasValue ? x.CreatedDate.Month == month : 1 == 1)
                            && (year.HasValue ? x.CreatedDate.Year == year : 1 == 1)
                            && lstStore.Contains(x.StoreId))
                  /*.AsQueryable()*/.ToList();

                var _tableCloseReceipt = cxt.R_ClosedReceiptReport.Select(x => new
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    CashierId = x.CashierId,
                    TableNo = x.TableNo,
                    StoreId = x.StoreId,
                    OrderId = x.OrderId
                }).Where(x => (month.HasValue ? x.CreatedDate.Month == month : 1 == 1)
                            && (year.HasValue ? x.CreatedDate.Year == year : 1 == 1)
                            && lstStore.Contains(x.StoreId))
                  /*.AsQueryable()*/.ToList();

                var JoinTable = (from ds in _tableDailySales
                                  join at in _tableAuditTrail on ds.BusinessId equals at.BusinessDayId 
                                  where(at.ReceiptID.Equals(ds.OrderId) && at.StoreId.Equals(ds.StoreId))
                                  join dr in _tableDailyReceipt on ds.BusinessId equals dr.BusinessDayId
                                  where(dr.StoreId.Equals(ds.StoreId) && dr.ReceiptId.Equals(ds.OrderId))
                                  join hs in _tableHourlySales on at.ReceiptID equals hs.ReceiptId 
                                  where(hs.StoreId.Equals(at.StoreId))
                                  join cr in _tableCloseReceipt on at.CashierId equals cr.CashierId 
                                  where(cr.StoreId.Equals(at.StoreId) && cr.OrderId.Equals(at.ReceiptID))
                                  select new
                                  {
                                      Id = ds.Id,
                                      BusinessId = ds.BusinessId,
                                      OrderId = ds.OrderId,
                                      OrderNo = at.OrderNo,
                                      ReceiptNo = dr.ReceiptNo,

                                      CancelAmount = at.CancelAmount ,
                                      ReceiptTotal = ds.ReceiptTotal,
                                      Discount = ds.Discount,
                                      Tip = ds.Tip,
                                      PromotionValue = ds.PromotionValue,
                                      ServiceCharge = ds.ServiceCharge,
                                      GST = ds.GST,
                                      Rounding = ds.Rounding,
                                      Refund = ds.Refund,
                                      NetSales = ds.NetSales,
                                      CashierId = at.CashierId ,
                                      CashierName = at.CashierName,
                                      Mode = ds.Mode,
                                      StoreId = ds.StoreId,
                                      NoOfPerson = ds.NoOfPerson,
                                      OrderStatus = at.OrderStatus,
                                      TableNo = cr.TableNo
                                  }).ToList();

                
                // get data table [R_ItemizedSalesAnalysisReport] , [R_ItemizedSalesAnalysisReportDetail]
                var _tableItemizedSales = cxt.R_ItemizedSalesAnalysisReport.Select(x => new
                {
                    Id = x.Id,
                    ItemId = x.ItemId,
                    ItemTypeId = x.ItemTypeId,
                    BusinessId = x.BusinessId,
                    CreatedDate = x.CreatedDate,
                    StoreId = x.StoreId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    GLAccountCode = x.GLAccountCode,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    ExtraPrice = x.ExtraPrice,
                    TotalAmount = x.TotalAmount,
                    Discount = x.Discount,
                    Cost = x.Cost,
                    ServiceCharge = x.ServiceCharge,
                    Tax = x.Tax,
                    PromotionAmount = x.PromotionAmount,
                    PoinsOrderId = x.PoinsOrderId,
                    GiftCardId = x.GiftCardId,
                    IsIncludeSale = x.IsIncludeSale,
                    Mode = x.Mode,
                    OrderId = x.ReceiptId
                }).Where(x => (month.HasValue ? x.CreatedDate.Month == month : 1 == 1)
                            && (year.HasValue ? x.CreatedDate.Year == year : 1 == 1)
                            && lstStore.Contains(x.StoreId))
                  /*.AsQueryable()*/.ToList();

                var _tableItemizedSalesDetail = cxt.R_ItemizedSalesAnalysisReportDetail.Select(x => new
                {
                    Id = x.Id,
                    BusinessId = x.BusinessId,
                    ParentId = x.ParentId,
                    CreatedDate = x.CreatedDate,
                    StoreId = x.StoreId,


                }).Where(x => (month.HasValue ? x.CreatedDate.Month == month : 1 == 1)
                            && (year.HasValue ? x.CreatedDate.Year == year : 1 == 1)
                            && lstStore.Contains(x.StoreId))
                  /*.AsQueryable()*/.ToList();

                var _tableJoinDetail = _tableItemizedSales.Join(_tableItemizedSalesDetail
                                                                , i => i.BusinessId
                                                                , d => d.BusinessId
                                                                , (i, d) => new { i, d })
                                                                .Select(x => new
                                                                {
                                                                    BusinessId = x.i.BusinessId,
                                                                    ItemId = x.i.ItemId,
                                                                    ItemTypeId = x.i.ItemTypeId,
                                                                    ParentId = x.d.ParentId,
                                                                    ItemCode = x.i.ItemCode,
                                                                    ItemName = x.i.ItemName,
                                                                    CategoryId = x.i.CategoryId,
                                                                    CategoryName = x.i.CategoryName,
                                                                    GLAccountCode = x.i.GLAccountCode,
                                                                    Quantity = x.i.Quantity,
                                                                    Price = x.i.Price,
                                                                    ExtraPrice = x.i.ExtraPrice,
                                                                    TotalAmount = x.i.TotalAmount,
                                                                    Discount = x.i.Discount,
                                                                    Cost = x.i.Cost,
                                                                    ServiceCharge = x.i.ServiceCharge,
                                                                    Tax = x.i.Tax,
                                                                    PromotionAmount = x.i.PromotionAmount,
                                                                    PoinsOrderId = x.i.PoinsOrderId,
                                                                    GiftCardId = x.i.GiftCardId,
                                                                    IsIncludeSale = x.i.IsIncludeSale,
                                                                    Mode = x.i.Mode,
                                                                    StoreId = x.i.StoreId,
                                                                    OrderId = x.i.OrderId,
                                                                })/*.AsEnumerable()*/.ToList();

                using (var trans = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        if (JoinTable != null && JoinTable.Count > 0)
                        {
                            var lstPosSalesEntity = JoinTable.Select(ss => new R_PosSale()
                            {
                                Id = Guid.NewGuid().ToString(),
                                StoreId = ss.StoreId,
                                BusinessId = ss.BusinessId,
                                OrderId = ss.OrderId,
                                OrderNo = ss.OrderNo,
                                ReceiptNo = ss.ReceiptNo,
                                CreatedDate = DateTime.Now,
                                OrderStatus = ss.OrderStatus,
                                TableNo = ss.TableNo,
                                NoOfPerson = ss.NoOfPerson,
                                CancelAmount = ss.CancelAmount,
                                ReceiptTotal = ss.ReceiptTotal,
                                Discount = ss.Discount,
                                Tip = ss.Tip,
                                PromotionValue = ss.PromotionValue,
                                ServiceCharge = ss.ServiceCharge,
                                GST = ss.GST,
                                Rounding = ss.Rounding,
                                Refund = ss.Refund,
                                //NetSales = ss.NetSales,
                                CashierId = ss.CashierId,
                                CashierName = ss.CashierName,
                                Mode = ss.Mode,

                            }).ToList();
                            cxt.R_PosSale.AddRange(lstPosSalesEntity);
                        }

                        if (_tableJoinDetail != null && _tableJoinDetail.Any())
                        {
                            var lstPosSaleDetailsEntity = _tableJoinDetail.Select(ss => new R_PosSaleDetail()
                            {
                                Id = Guid.NewGuid().ToString(),
                                StoreId = ss.StoreId,
                                BusinessId = ss.BusinessId,
                                OrderId = ss.OrderId,
                                OrderDetailId = "", //ss.OrderDetailId,
                                ItemId = ss.ItemId,
                                ItemTypeId = ss.ItemTypeId,
                                ParentId = ss.ParentId,
                                ItemCode = ss.ItemCode,
                                ItemName = ss.ItemName,
                                CategoryId = ss.CategoryId,
                                CategoryName = ss.CategoryName,
                                GLAccountCode = ss.GLAccountCode,
                                Quantity = ss.Quantity,
                                Price = ss.Price,
                                ExtraPrice = ss.ExtraPrice,
                                TotalAmount = ss.TotalAmount.HasValue ? ss.TotalAmount.Value : 0,
                                Discount = ss.Discount,
                                Cost = ss.Cost,
                                ServiceCharge = ss.ServiceCharge,
                                Tax = ss.Tax,
                                PromotionAmount = ss.PromotionAmount,
                                PoinsOrderId = ss.PoinsOrderId,
                                GiftCardId = ss.GiftCardId,
                                IsIncludeSale = ss.IsIncludeSale.HasValue ? ss.IsIncludeSale.Value : false,
                                CreatedDate = DateTime.Now,
                                Mode = ss.Mode

                            }).ToList();

                            //add to db
                            cxt.R_PosSaleDetail.AddRange(lstPosSaleDetailsEntity);
                        }
                        cxt.SaveChanges();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("MergeData", ex);
                        trans.Rollback();
                        result = false;
                    }
                    finally
                    {
                        cxt.Dispose();
                    }
                }
            }

            return result;

        }

        public List<ItemizedSalesAnalysisReportModels> GetItemsNoIncludeSale(List<string> listAllReceiptId, List<string> listStoreId, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                List<ItemizedSalesAnalysisReportModels> lstData = new List<ItemizedSalesAnalysisReportModels>();

                lstData = (from ps in cxt.R_PosSale
                           from psd in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                           where listStoreId.Contains(ps.StoreId)
                               && listAllReceiptId.Contains(ps.OrderId)
                               && psd.Mode == mode
                           select new ItemizedSalesAnalysisReportModels
                           {
                               StoreId = ps.StoreId,
                               CreatedDate = ps.ReceiptCreatedDate.Value,
                               CategoryId = psd.CategoryId,
                               CategoryName = psd.CategoryName,
                               ExtraPrice = psd.ExtraPrice,
                               TotalPrice = psd.TotalAmount - (psd.IsDiscountTotal != true ? psd.Discount : 0) - psd.PromotionAmount,
                               GLAccountCode = psd.GLAccountCode,
                               IsIncludeSale = psd.IsIncludeSale,
                               BusinessId = psd.BusinessId,
                               ServiceCharge = psd.ServiceCharge,
                               Tax = psd.Tax,
                               //ExtraAmount = tb.ExtraAmount.HasValue ? tb.ExtraAmount.Value : 0,
                               TotalAmount = psd.TotalAmount,
                               TotalDiscount = psd.IsDiscountTotal != true ? psd.Discount : 0,
                               ItemTotal = psd.TotalAmount - (psd.IsDiscountTotal != true ? psd.Discount : 0) - psd.PromotionAmount + psd.ExtraPrice,
                               PromotionAmount = psd.PromotionAmount,
                               ReceiptId = psd.OrderId,
                               TaxType = psd.TaxType,
                               GiftCardId = psd.GiftCardId,
                               PoinsOrderId = psd.PoinsOrderId
                           }).ToList();

                return lstData;
            }
        }

        public List<DiscountAndMiscReportModels> GetReceiptDiscountAndMiscByDateTime(DateTime fromDate, DateTime toDate, List<string> StoreIds, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = new List<DiscountAndMiscReportModels>();
                var query = (from ps in cxt.R_PosSale
                             from psd in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                             where StoreIds.Contains(ps.StoreId) && ps.ReceiptCreatedDate >= fromDate && ps.ReceiptCreatedDate <= toDate
                             && psd.Mode == mode
                             group psd by new
                             {
                                 Date = DbFunctions.TruncateTime(ps.ReceiptCreatedDate),
                                 Hour = (int?)SqlFunctions.DatePart("HH", ps.ReceiptCreatedDate),
                                 StoreId = ps.StoreId,
                             } into g
                             select g).ToList();
                if (query != null && query.Any())
                {
                    lstData = query.Select(g => new DiscountAndMiscReportModels
                    {
                        StoreId = g.Key.StoreId,
                        Hour = g.Key.Hour.Value,
                        TimeSpanHour = new TimeSpan(g.Key.Hour.Value, 0, 0),
                        CreatedDate = g.Key.Date.Value,
                        DiscountValue = g.Where(ww => ww.IsDiscountTotal != true).Sum(ss => ss.Discount),
                        MiscValue = g.Where(ww => ww.ItemTypeId == (byte)Commons.EProductType.Misc).Sum(ss => ss.Price)
                    }).ToList();
                }
                return lstData;
            }
        }

        public List<ItemizedSalesAnalysisReportModels> GetListMiscForDailyReceipt(List<string> listStoreId , List<string> listReceiptId, int mode)
        {
            var lstData = new List<ItemizedSalesAnalysisReportModels>();
            using (var cxt = new NuWebContext())
            {
                lstData = (from ps in cxt.R_PosSale
                           from psd in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                           where listStoreId.Contains(ps.StoreId)
                                && listReceiptId.Contains(ps.OrderId)
                                && psd.Mode == mode
                                && (psd.ItemTypeId == (int)Commons.EProductType.Misc)

                           select new ItemizedSalesAnalysisReportModels
                           {
                               StoreId = ps.StoreId,
                               CreatedDate = ps.ReceiptCreatedDate.Value,
                               ReceiptId = ps.OrderId,
                               BusinessId = ps.BusinessId,
                               TotalPrice = psd.Price
                           }).ToList();
                return lstData;
            }
        }

        public List<DiscountAndMiscReportModels> GetMiscs(List<string> listStoreId, List<string> listReceiptId, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from ps in cxt.R_PosSale
                               from psd in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                               where listStoreId.Contains(ps.StoreId) 
                               && listReceiptId.Contains(ps.OrderId)
                               && psd.Mode == mode
                               && (psd.ItemTypeId == (int)Commons.EProductType.Misc)
                               select new DiscountAndMiscReportModels
                               {
                                   StoreId = ps.StoreId,
                                   CreatedDate = ps.ReceiptCreatedDate.Value,
                                   MiscValue = psd.Price
                               }).ToList();
                return lstData;
            }

        }

        public List<DiscountAndMiscReportModels> GetMiscDiscount(List<string> listStoreId, List<string> listReceiptId, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from ps in cxt.R_PosSale
                               from psd in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                               where listStoreId.Contains(ps.StoreId)
                               && listReceiptId.Contains(ps.OrderId)
                               && psd.Mode == mode
                               && (psd.ItemTypeId == (int)Commons.EProductType.Misc || psd.IsDiscountTotal == true)
                               select new DiscountAndMiscReportModels
                               {
                                   StoreId = ps.StoreId,
                                   CreatedDate = ps.ReceiptCreatedDate.Value,
                                   MiscValue = psd.Price,
                                   DiscountValue = psd.Discount,
                                   IsDiscountTotal = (psd.IsDiscountTotal.HasValue && psd.IsDiscountTotal.Value == true) ? true : false,
                                   ItemTypeId = psd.ItemTypeId
                               }).ToList();
                return lstData;
            }

        }

        public List<DailySalesReportInsertModels> GetRoundingAmount(List<string> listStoreId, List<string> listReceiptId, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.R_PosSale.Where(ww => listStoreId.Contains(ww.StoreId)
                                     && listReceiptId.Contains(ww.OrderId)
                                     && ww.Mode == mode)
                                     .Select(ss => new DailySalesReportInsertModels()
                                     {
                                         StoreId = ss.StoreId,
                                         BusinessId = ss.BusinessId,
                                         CreatedDate = ss.ReceiptCreatedDate.Value,
                                         Rounding = ss.Rounding,
                                         OrderId = ss.OrderId,
                                         CreditNoteNo = ss.CreditNoteNo,
                                         GST = ss.GST,
                                         ReceiptTotal = ss.ReceiptTotal,
                                         ServiceCharge = ss.ServiceCharge,
                                         Discount = ss.Discount,
                                         PromotionValue = ss.PromotionValue
                                     }).ToList();

                return lstData;
            }
        }

        #region For Xero
        public void PushInvoiceToXexo(string storeId,List<PosSaleDetailModels> lstPosSaleDetail, List<PaymentReportXeroDTO> paids, BusinessInfoXeroDTO businessInfo)
        {
            if (businessInfo == null || string.IsNullOrEmpty(businessInfo.Url))
                return;
            NSLog.Logger.Info("PushInvoiceToXexo Start");
            var listItemIds = lstPosSaleDetail.Where(ww => ww.ItemTypeId != (int)Commons.EProductType.Misc
            && !string.IsNullOrEmpty(ww.GLAccountCode)).Select(ss => ss.ItemId).Distinct().ToList();
            if (listItemIds == null || listItemIds.Count == 0)
                return;
            //get tax rate
            string taxRate = businessInfo.TaxRate;//"TAX002";
            //string accountCode = string.Empty;
            string accountCodeForMisc = string.Empty;
            //check recipe
            var listItemInRecipe = new List<string>();
            //get tax
            //NuWebNCloud.Shared.Factory.Settings.TaxFactory _taxFactory = new NuWebNCloud.Shared.Factory.Settings.TaxFactory();
            //var lstTaxes = _taxFactory.GetListTaxV2(storeId);
            //var tax = lstTaxes.Where(w => w.IsActive && !string.IsNullOrEmpty(w.Rate)).FirstOrDefault();
            //if (tax != null)
            //    taxRate = tax.Rate;
            ////get currency

            using (var db = new NuWebContext())
            {
                listItemInRecipe = (from ri in db.I_Recipe_Item
                                    join i in db.I_Ingredient on ri.IngredientId equals i.Id
                                    where ri.StoreId == storeId && ri.Status != (int)Commons.EStatus.Deleted
                                    && i.IsCheckStock
                                    group ri by ri.ItemId into g
                                    where g.Count() == 1
                                    select g.Key
                                         ).ToList();
                accountCodeForMisc = (from s in db.G_GeneralSetting
                               join ss in db.G_SettingOnStore on s.Id equals ss.SettingId
                               where s.Code == (int)Commons.EGeneralSetting.Miscellaneous
                               select ss.Value).FirstOrDefault();

            }
            //list item dont recipe
            var listsaleFromPOSId = listItemIds.Except(listItemInRecipe);
            //list have recipe
            var lstItemHaveRecipeId = listItemIds.Intersect(listItemInRecipe).ToList();
            //misc
            var listItemMiscs = lstPosSaleDetail.Where(ww => ww.ItemTypeId == (int)Commons.EProductType.Misc
                ).ToList();
            //list seriviceCharge
            var listSC = lstPosSaleDetail.Where(ww => ww.ServiceCharge > 0).ToList();
            //payment
            //todo
            //---------------------------------------------------------------------
            XeroInvoiceReportDTO xeroInvoice = new XeroInvoiceReportDTO();
            xeroInvoice.CurrencyCode = businessInfo.Currency;
            xeroInvoice.Reference = "NUPOS_XERO_DEMO";
            xeroInvoice.AppRegistrationId = businessInfo.AppRegistrationId;
            xeroInvoice.StoreId = businessInfo.StoreId;
            xeroInvoice.BusinessDayId = businessInfo.BusinessDayId;
            xeroInvoice.ClosingDatetime = businessInfo.ClosingDate;

            List<XeroInvoiceItemReportDTO> lstLineItems = new List<XeroInvoiceItemReportDTO>();
            List<XeroInvoicePaymentReportDTO> payments = new List<XeroInvoicePaymentReportDTO>();

            var lstSaleFromPOS = lstPosSaleDetail.Where(ww => listsaleFromPOSId.Contains(ww.ItemId)).GroupBy(gg => gg.GLAccountCode)
               .ToList();

            var lineItemsForSaleFromPOS = new List<XeroInvoiceItemReportDTO>();
            var lineHaveRecipeFromPOS = new List<XeroInvoiceItemReportDTO>();

            XeroInvoiceItemReportDTO obj = null;
            XeroInvoicePaymentReportDTO paid = null;

            foreach (var item in lstSaleFromPOS)
            {
                obj = new XeroInvoiceItemReportDTO();
                obj.Description = "Sales from NuPOS";
                obj.Quantity = 1;
                obj.LineAmount = Math.Round(item.Sum(a => a.TotalAmount - a.PromotionAmount), 2);
                obj.UnitAmount = Math.Round(item.Sum(a => a.TotalAmount - a.PromotionAmount), 2);
                obj.AccountCode = item.Key;
                obj.TaxType = taxRate;

                lineItemsForSaleFromPOS.Add(obj);
            }
            lstLineItems.AddRange(lineItemsForSaleFromPOS);
            //have recipe
            if (lstItemHaveRecipeId != null && lstItemHaveRecipeId.Any())
            {
                using (var db = new NuWebContext())
                {
                    //list recipe
                    var ingredients = (from ri in db.I_Recipe_Item
                                       join i in db.I_Ingredient on ri.IngredientId equals i.Id
                                       where ri.StoreId == storeId && ri.Status != (int)Commons.EStatus.Deleted
                                       && i.IsCheckStock
                                       && lstItemHaveRecipeId.Contains(ri.ItemId)
                                       select new { Description = i.Name, Code = i.Code, ItemId = ri.ItemId }
                                         ).ToList();
                    for (int i = 0; i < lstItemHaveRecipeId.Count(); i++)
                    {
                        obj = new XeroInvoiceItemReportDTO();
                        obj.Description = ingredients.Where(ww => ww.ItemId == lstItemHaveRecipeId[i]).Select(ss => ss.Description).FirstOrDefault();
                        obj.ItemCode = ingredients.Where(ww => ww.ItemId == lstItemHaveRecipeId[i]).Select(ss => ss.Code).FirstOrDefault();
                        obj.Quantity = 1;
                        obj.LineAmount = Math.Round(lstPosSaleDetail.Where(ww => ww.ItemId == lstItemHaveRecipeId[i]).Sum(a => a.TotalAmount - a.PromotionAmount), 2);
                        obj.UnitAmount = Math.Round(lstPosSaleDetail.Where(ww => ww.ItemId == lstItemHaveRecipeId[i]).Sum(a => a.TotalAmount - a.PromotionAmount), 2);
                        obj.AccountCode = lstPosSaleDetail.Where(ww => ww.ItemId == lstItemHaveRecipeId[i]).Select(ss => ss.GLAccountCode).FirstOrDefault();
                        obj.TaxType = taxRate;
                        lineHaveRecipeFromPOS.Add(obj);
                    }
                }
                lstLineItems.AddRange(lineHaveRecipeFromPOS);
            }
            //service charge
            if (listSC != null && listSC.Any())
            {
                var listSCGroupGLCode = listSC.GroupBy(g => g.GLAccountCode).ToList();
                foreach (var item in listSCGroupGLCode)
                {
                    obj = new XeroInvoiceItemReportDTO();
                    obj.Description = "Svc charge from NuPOS";
                    obj.Quantity = 1;
                    obj.LineAmount = Math.Round(item.Sum(a => a.TotalAmount - a.PromotionAmount), 2);
                    obj.UnitAmount = Math.Round(item.Sum(a => a.TotalAmount - a.PromotionAmount), 2);
                    obj.AccountCode = item.Key;
                    obj.TaxType = taxRate;

                    lstLineItems.Add(obj);
                }
            }

            //check misc
            if (listItemMiscs != null && listItemMiscs.Any())
            {
                obj = new XeroInvoiceItemReportDTO();
                obj.Description = "Misc from NuPOS";
                obj.Quantity = 1;
                obj.LineAmount = Math.Round(listSC.Sum(a => a.TotalAmount - a.PromotionAmount), 2);
                obj.UnitAmount = Math.Round(listSC.Sum(a => a.TotalAmount - a.PromotionAmount), 2);
                obj.AccountCode = accountCodeForMisc;
                obj.TaxType = "NONE";

                lstLineItems.Add(obj);
            }

            NSLog.Logger.Info("lstLineItems: ", lstLineItems);
            //list payments
            #region payments
            foreach (var item in paids)
            {
                paid = new XeroInvoicePaymentReportDTO();
                paid.Account.Add(new XeroInvoiceAccountReportDTO() { Code = item.AccountCode });
                paid.Date = item.Date;
                paid.Amount = item.Amount;
                paid.Reference = item.Reference;

                payments.Add(paid);
            }
            #endregion end payment

            //call api
            xeroInvoice.LineItems = lstLineItems;
            xeroInvoice.Payments = payments;
            string msg = string.Empty;
            var result = _xeroFactory.SaleItems(businessInfo.Url, xeroInvoice, ref msg);
            NSLog.Logger.Info("PushInvoiceToXexo end", result);
        }
        #endregion End for xero
    }
}
