using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory
{
    public class RefundFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public RefundFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<RefundReportDTO> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            //_logger.Info("==========================================================");
            //_logger.Info(string.Format("Insert TimeClock: StoreId: [{0}] | BusinessId: [{1}]", info.StoreId, info.BusinessDayId));
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Ex+ist
                var obj = cxt.R_Refund.Where(ww => ww.StoreId == info.StoreId
                                && ww.BusinessDayId == info.BusinessDayId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert refund data exist");
                    return result;
                }

                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_Refund> lstInsert = new List<R_Refund>();
                        List<R_RefundDetail> lstDetailInsert = new List<R_RefundDetail>();
                        R_Refund itemInsert = null;
                        R_RefundDetail itemDetailInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_Refund();
                            itemInsert.OrderId = item.OrderId;
                            itemInsert.ReceiptDate = item.ReceiptDate;
                            itemInsert.Id = item.Id;
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.TotalRefund = item.TotalRefund;
                            itemInsert.Description = item.Description;
                            itemInsert.BusinessDayId = item.BusinessDayId;
                            itemInsert.ServiceCharged = item.ServiceCharged;
                            itemInsert.Tax = item.Tax;
                            itemInsert.Discount = item.Discount;
                            itemInsert.Promotion = item.Promotion;
                            itemInsert.CreatedUser = item.CreatedUser;
                            itemInsert.IsGiftCard = item.IsGiftCard;

                            foreach (var subItem in item.ListDetails)
                            {
                                itemDetailInsert = new R_RefundDetail();
                                itemDetailInsert.Id = subItem.Id;
                                itemDetailInsert.RefundId = subItem.RefundId;
                                itemDetailInsert.ItemId = subItem.ItemId;
                                itemDetailInsert.ItemName = subItem.ItemName;
                                itemDetailInsert.ItemType = subItem.ItemType;
                                itemDetailInsert.Qty = subItem.Qty;
                                itemDetailInsert.ServiceCharged = subItem.ServiceCharged;
                                itemDetailInsert.Tax = subItem.Tax;
                                itemDetailInsert.PromotionAmount = subItem.PromotionAmount;
                                itemDetailInsert.PriceValue = subItem.PriceValue;
                                itemDetailInsert.DiscountAmount = subItem.DiscountAmount;

                                lstDetailInsert.Add(itemDetailInsert);
                            }
                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_Refund.AddRange(lstInsert);
                        cxt.R_RefundDetail.AddRange(lstDetailInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        //_logger.Info(string.Format("Insert TimeClock: StoreId: [{0}] | BusinessId: [{1}] success", info.StoreId, info.BusinessDayId));
                        NSLog.Logger.Info("Insert refund data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert refund data fail", ex);
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
            //_baseFactory.InsertTrackingLog("R_Refund", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<RefundReportDTO> GetListRefund(List<string> lstStoreIds, DateTime dFrom, DateTime dTo, int mode)
        {
            var result = new List<RefundReportDTO>();
            //get business day
            var lstBusinessDays = _baseFactory.GetBusinessDays(dFrom, dTo, lstStoreIds, mode);
            if (lstBusinessDays != null && lstBusinessDays.Any())
            {
                var dMin = lstBusinessDays.Min(ss => ss.DateFrom);
                var dMax = lstBusinessDays.Max(ss => ss.DateTo);
                using (var cxt = new NuWebContext())
                {
                    var query = (from r in cxt.R_Refund
                                 join rd in cxt.R_RefundDetail on r.Id equals rd.RefundId
                                 where r.CreatedDate >= dMin && r.CreatedDate <= dMax && lstStoreIds.Contains(r.StoreId)
                                 select new { r, rd });

                    if (query != null && query.Any())
                    {
                        var lstHeader = query.Select(ss => ss.r).Distinct().ToList();

                        var lstRefundDetail = query.Select(ss => new RefundDetailReportDTO()
                        {
                            RefundId = ss.rd.RefundId,
                            ItemId = ss.rd.ItemId,
                            ItemType = ss.rd.ItemType,
                            ItemName = ss.rd.ItemName,
                            PriceValue = ss.rd.PriceValue,
                            Qty = ss.rd.Qty,
                            ServiceCharged = ss.rd.ServiceCharged,
                            Tax = ss.rd.Tax,
                            PromotionAmount = ss.rd.PromotionAmount,
                            DiscountAmount = ss.rd.DiscountAmount
                        }).ToList();

                        RefundReportDTO refund = null;
                        foreach (var item in lstHeader)
                        {
                            refund = new RefundReportDTO();
                            refund.Id = item.Id;
                            refund.BusinessDayId = item.BusinessDayId;
                            refund.CreatedDate = item.CreatedDate;
                            refund.CreatedUser = item.CreatedUser;
                            refund.Description = item.Description;
                            refund.StoreId = item.StoreId;
                            refund.TotalRefund = item.TotalRefund;
                            refund.Promotion = item.Promotion;
                            refund.ServiceCharged = item.ServiceCharged;
                            refund.Tax = item.Tax;
                            refund.Discount = item.Discount;
                            refund.OrderId = item.OrderId;
                            refund.ReceiptDate = item.ReceiptDate;
                            refund.ListDetails = lstRefundDetail.Where(ww => ww.RefundId == refund.Id).ToList();


                            result.Add(refund);
                        }
                    }
                }

            }
            return result;
        }


        public List<RefundReportDTO> GetListRefundWithoutDetails(List<string> lstStoreIds, DateTime dFrom, DateTime dTo, int mode)
        {
            var result = new List<RefundReportDTO>();
            //get business day
            var lstBusinessDays = _baseFactory.GetBusinessDays(dFrom, dTo, lstStoreIds, mode);
            if (lstBusinessDays != null && lstBusinessDays.Any())
            {
                var dMin = lstBusinessDays.Min(ss => ss.DateFrom);
                var dMax = lstBusinessDays.Max(ss => ss.DateTo);
                using (var cxt = new NuWebContext())
                {
                    result = (from r in cxt.R_Refund
                              where r.CreatedDate >= dMin && r.CreatedDate <= dMax && lstStoreIds.Contains(r.StoreId)
                              select new RefundReportDTO()
                              {
                                  Id = r.Id,
                                  BusinessDayId = r.BusinessDayId,
                                  CreatedDate = r.CreatedDate,
                                  Description = r.Description,
                                  StoreId = r.StoreId,
                                  TotalRefund = r.TotalRefund,
                                  Promotion = r.Promotion,
                                  ServiceCharged = r.ServiceCharged,
                                  OrderId = r.OrderId,
                                  IsGiftCard = r.IsGiftCard.HasValue ? r.IsGiftCard.Value : false,

                              }).ToList();

                }

            }
            return result;
        }

        // Updated 03302018
        // Datetime params no need update depend on business day
        public List<RefundReportDTO> GetListRefundByBusinessDay(List<string> lstStoreIds, DateTime dFrom, DateTime dTo)
        {
            var result = new List<RefundReportDTO>();

            using (var cxt = new NuWebContext())
            {
                var query = (from r in cxt.R_Refund
                             join rd in cxt.R_RefundDetail on r.Id equals rd.RefundId
                             where r.CreatedDate >= dFrom && r.CreatedDate <= dTo && lstStoreIds.Contains(r.StoreId)
                             select new { r, rd });

                if (query != null && query.Any())
                {
                    var lstHeader = query.Select(ss => ss.r).Distinct().ToList();

                    var lstRefundDetail = query.Select(ss => new RefundDetailReportDTO()
                    {
                        RefundId = ss.rd.RefundId,
                        ItemId = ss.rd.ItemId,
                        ItemType = ss.rd.ItemType,
                        ItemName = ss.rd.ItemName,
                        PriceValue = ss.rd.PriceValue,
                        Qty = ss.rd.Qty,
                        ServiceCharged = ss.rd.ServiceCharged,
                        Tax = ss.rd.Tax,
                        PromotionAmount = ss.rd.PromotionAmount,
                        DiscountAmount = ss.rd.DiscountAmount
                    }).ToList();

                    RefundReportDTO refund = null;
                    foreach (var item in lstHeader)
                    {
                        refund = new RefundReportDTO();
                        refund.Id = item.Id;
                        refund.BusinessDayId = item.BusinessDayId;
                        refund.CreatedDate = item.CreatedDate;
                        refund.CreatedUser = item.CreatedUser;
                        refund.Description = item.Description;
                        refund.StoreId = item.StoreId;
                        refund.TotalRefund = item.TotalRefund;
                        refund.Promotion = item.Promotion;
                        refund.ServiceCharged = item.ServiceCharged;
                        refund.Tax = item.Tax;
                        refund.Discount = item.Discount;
                        refund.OrderId = item.OrderId;
                        refund.ReceiptDate = item.ReceiptDate;
                        refund.ListDetails = lstRefundDetail.Where(ww => ww.RefundId == refund.Id).ToList();

                        result.Add(refund);
                    }
                }
            }


            return result;
        }

        public List<RefundReportDTO> GetListRefundWithoutDetail(BaseReportModel model)
        {
            var result = new List<RefundReportDTO>();

            using (var cxt = new NuWebContext())
            {
                result = (from r in cxt.R_Refund
                          where r.CreatedDate >= model.FromDate && r.CreatedDate <= model.ToDate && model.ListStores.Contains(r.StoreId)
                          //&& r.IsGiftCard.HasValue && r.IsGiftCard.Value
                          select new RefundReportDTO()
                          {
                              Id = r.Id,
                              BusinessDayId = r.BusinessDayId,
                              CreatedDate = r.CreatedDate,
                              Description = r.Description,
                              StoreId = r.StoreId,
                              TotalRefund = r.TotalRefund,
                              Promotion = r.Promotion,
                              ServiceCharged = r.ServiceCharged,
                              OrderId = r.OrderId,
                              IsGiftCard = r.IsGiftCard.HasValue ? r.IsGiftCard.Value : false,

                          }).ToList();

            }


            return result;
        }

        #region Get list refund data from list Receipt Id
        public List<RefundReportDTO> GetListRefundByBusinessDay(List<string> lstStoreIds, List<string> lstReceiptId)
        {
            var result = new List<RefundReportDTO>();

            using (var cxt = new NuWebContext())
            {
                var query = (from r in cxt.R_Refund
                             join rd in cxt.R_RefundDetail on r.Id equals rd.RefundId
                             where lstReceiptId.Contains(r.OrderId)
                             && lstStoreIds.Contains(r.StoreId)
                             select new { r, rd });

                if (query != null && query.Any())
                {
                    var lstHeader = query.Select(ss => ss.r).Distinct().ToList();

                    var lstRefundDetail = query.Select(ss => new RefundDetailReportDTO()
                    {
                        RefundId = ss.rd.RefundId,
                        ItemId = ss.rd.ItemId,
                        ItemType = ss.rd.ItemType,
                        ItemName = ss.rd.ItemName,
                        PriceValue = ss.rd.PriceValue,
                        Qty = ss.rd.Qty,
                        ServiceCharged = ss.rd.ServiceCharged,
                        Tax = ss.rd.Tax,
                        PromotionAmount = ss.rd.PromotionAmount,
                        DiscountAmount = ss.rd.DiscountAmount
                    }).ToList();

                    RefundReportDTO refund = null;
                    foreach (var item in lstHeader)
                    {
                        refund = new RefundReportDTO();
                        refund.Id = item.Id;
                        refund.BusinessDayId = item.BusinessDayId;
                        refund.CreatedDate = item.CreatedDate;
                        refund.CreatedUser = item.CreatedUser;
                        refund.Description = item.Description;
                        refund.StoreId = item.StoreId;
                        refund.TotalRefund = item.TotalRefund;
                        refund.Promotion = item.Promotion;
                        refund.ServiceCharged = item.ServiceCharged;
                        refund.Tax = item.Tax;
                        refund.Discount = item.Discount;
                        refund.OrderId = item.OrderId;
                        refund.ReceiptDate = item.ReceiptDate;
                        refund.ListDetails = lstRefundDetail.Where(ww => ww.RefundId == refund.Id).ToList();

                        result.Add(refund);
                    }
                }

            }

            return result;
        }

        public List<RefundReportDTO> GetListRefundWithoutDetailsByReceiptId(List<string> lstStoreIds, List<string> lstReceiptId)
        {
            var result = new List<RefundReportDTO>();
            using (var cxt = new NuWebContext())
            {
                result = (from r in cxt.R_Refund
                          where lstReceiptId.Contains(r.OrderId)
                          && lstStoreIds.Contains(r.StoreId)
                          select new RefundReportDTO()
                          {
                              Id = r.Id,
                              BusinessDayId = r.BusinessDayId,
                              CreatedDate = r.CreatedDate,
                              Description = r.Description,
                              StoreId = r.StoreId,
                              TotalRefund = r.TotalRefund,
                              Promotion = r.Promotion,
                              ServiceCharged = r.ServiceCharged,
                              OrderId = r.OrderId,
                              IsGiftCard = r.IsGiftCard.HasValue ? r.IsGiftCard.Value : false,
                          }).ToList();
            }
            return result;
        }
        #endregion

    }
}
