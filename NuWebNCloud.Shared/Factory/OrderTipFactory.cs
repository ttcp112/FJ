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
    public class OrderTipFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public OrderTipFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<OrderTipModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.G_OrderTip.Where(ww => ww.StoreId == info.StoreId
                                && ww.OrderId == info.OrderId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Order Tip data exist");
                    return result;
                }

                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<G_OrderTip> lstInsert = new List<G_OrderTip>();
                        G_OrderTip itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new G_OrderTip();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.OrderId = item.OrderId;
                            itemInsert.Amount = item.Amount;
                            itemInsert.PaymentId = item.PaymentId;
                            itemInsert.PaymentName = item.PaymentName;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.Mode = item.Mode;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.G_OrderTip.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Order Tip data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Order Tip data fail", ex);
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
            //_baseFactory.InsertTrackingLog("G_OrderTip", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<OrderTipModels> GetDataTips(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.G_OrderTip
                               where model.ListStores.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= model.FromDate
                                             && tb.CreatedDate <= model.ToDate)
                               select new OrderTipModels
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                                   PaymentId = tb.PaymentId,
                                   PaymentName = tb.PaymentName,
                                   Amount = tb.Amount,
                                   OrderId = tb.OrderId
                               }).ToList();
                return lstData;
            }
        }

        public List<OrderTipModels> GetDataTips(BaseReportModel model, List<string> lstPaymentIds, DateTime dFrom, DateTime dTo)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.G_OrderTip
                               where model.ListStores.Contains(tb.StoreId)
                                        && lstPaymentIds.Contains(tb.PaymentId)
                                     && (tb.CreatedDate >= dFrom
                                             && tb.CreatedDate <= dTo)
                               select new OrderTipModels
                               {
                                   StoreId = tb.StoreId,
                                   CreatedDate = tb.CreatedDate,
                                   PaymentId = tb.PaymentId,
                                   PaymentName = tb.PaymentName,
                                   Amount = tb.Amount,
                                   OrderId = tb.OrderId
                               }).ToList();
                return lstData;
            }
        }
    }
}
