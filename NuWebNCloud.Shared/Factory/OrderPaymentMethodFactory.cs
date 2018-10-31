using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;

namespace NuWebNCloud.Shared.Factory
{
    public class OrderPaymentMethodFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public OrderPaymentMethodFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public bool Insert(List<PaymentModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            //_logger.Info("==========================================================");
            //_logger.Info(string.Format("Insert order Paid: StoreId: [{0}]", info.StoreId));
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.G_PaymentMenthod.Where(ww => ww.StoreId == info.StoreId
                                && ww.OrderId == info.OrderId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert order paid sale data exist", lstInfo);
                    //_logger.Info("============================order Paid" + info.StoreId + "exist data======================================");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<G_PaymentMenthod> lstInsert = new List<G_PaymentMenthod>();
                        G_PaymentMenthod itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new G_PaymentMenthod();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.PaymentId = item.PaymentId;
                            itemInsert.PaymentCode = item.PaymentCode;
                            itemInsert.PaymentName = item.PaymentName;
                            itemInsert.Amount = item.Amount;
                            itemInsert.OrderId = item.OrderId;

                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.CreatedUser = item.CreatedUser;
                            itemInsert.LastUserModified = item.LastUserModified;
                            itemInsert.LastDateModified = item.LastDateModified;
                            itemInsert.Mode = item.Mode;
                            itemInsert.IsInclude = item.IsInclude;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.G_PaymentMenthod.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert order paid sale data success", lstInfo);
                        //_logger.Info(string.Format("Insert order Paid: StoreId: [{0}] Success", info.StoreId));
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert order paid sale data fail", lstInfo);
                        NSLog.Logger.Error("Insert order paid sale data fail", ex);
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
            //NSLog.Logger.Info("insert order paid sale data success", lstInfo);
            //var jsonContent = JsonConvert.SerializeObject(lstInfo);
            //_baseFactory.InsertTrackingLog("G_PaymentMenthod", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<PaymentModels> GetDataPaymentItemsByGC(BaseReportModel model,List<string> lstGCId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.G_PaymentMenthod
                               where model.ListStores.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= model.FromDate && tb.CreatedDate <= model.ToDate)
                                     && tb.Mode == model.Mode
                                     && (lstGCId.Contains(tb.PaymentId) || tb.PaymentCode == (int)Commons.EPaymentCode.GiftCard)
                                     //&& (tb.IsInclude ==null ||(tb.IsInclude.HasValue && tb.IsInclude.Value))
                               select new PaymentModels
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                                   PaymentId = tb.PaymentId,
                                   PaymentName = tb.PaymentName,
                                   Amount = tb.Amount,
                                   OrderId = tb.OrderId,
                                   IsInclude = tb.IsInclude
                               }).ToList();
                return lstData;
            }
        }
        public List<PaymentModels> GetDataPaymentItemsByGCForHourlySale(BaseReportModel model, List<string> lstGCId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = cxt.G_PaymentMenthod.Where (ww=>model.ListStores.Contains(ww.StoreId)
                                     && (ww.CreatedDate >= model.FromDate
                                             && ww.CreatedDate <= model.ToDate) && ww.Mode == model.Mode
                                             && (lstGCId.Contains(ww.PaymentId) || ww.PaymentCode == (int)Commons.EPaymentCode.GiftCard))
                                       .GroupBy(gg => new { StoreId = gg.StoreId, Time = (int?)SqlFunctions.DatePart("HH", gg.CreatedDate) })
                               .Select(tb=> new PaymentModels
                               {
                                   StoreId = tb.Key.StoreId,
                                   Time = tb.Key.Time.HasValue ? tb.Key.Time.Value : 0,
                                   Amount = tb.Sum(ss=>ss.Amount)
                               }).ToList();
                return lstData;
            }
        }
    }
}
