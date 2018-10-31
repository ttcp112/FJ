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

namespace NuWebNCloud.Shared.Factory
{
    public class ItemizedCancelRefundFactory : ReportFactory
    {
        //private static Logger _logger = LogManager.GetCurrentClassLogger();
        //private BaseFactory _baseFactory = null;
        //private RefundFactory _refundFactory = null;
        public ItemizedCancelRefundFactory()
        {
        }

        public bool Insert(List<ItemizedCancelRefundModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_ItemizedCancelOrRefundData.Where(ww => ww.StoreId == info.StoreId
                        && ww.BusinessId == info.BusinessId).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert ItemizedCancelRefund data exist", lstInfo);
                    return result;
                }

                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_ItemizedCancelOrRefundData> lstInsert = new List<R_ItemizedCancelOrRefundData>();
                        R_ItemizedCancelOrRefundData itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_ItemizedCancelOrRefundData();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.BusinessId = item.BusinessId;
                            itemInsert.OrderId = item.OrderId;
                            itemInsert.ItemId = item.ItemId;
                            itemInsert.ItemCode = item.ItemCode;
                            itemInsert.ItemTypeId = item.ItemTypeId;
                            itemInsert.ItemName = item.ItemName;
                            itemInsert.Price = item.Price;
                            itemInsert.Quantity = item.Quantity;
                            itemInsert.Amount = item.Amount;
                            itemInsert.CategoryId = item.CategoryId;
                            itemInsert.CategoryName = item.CategoryName;
                            itemInsert.CancelUser = item.CancelUser;
                            itemInsert.RefundUser = item.RefundUser;
                            itemInsert.CreatedDate = item.CreatedDate;

                            itemInsert.Mode = item.Mode;
                            itemInsert.IsRefund = item.IsRefund;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_ItemizedCancelOrRefundData.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert ItemizedCancelRefund data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert ItemizedCancelRefund data fail", lstInfo);
                        NSLog.Logger.Error("Insert ItemizedCancelRefund data fail", ex);
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

        public List<ItemizedCancelRefundModels> GetDataItemizedCancelRefunds(DateTime dateFrom, DateTime dateTo, List<string> lstStoreIds, List<string> lstBusinessInputIds)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from tb in cxt.R_ItemizedCancelOrRefundData
                               where lstStoreIds.Contains(tb.StoreId)
                                     && (tb.CreatedDate >= dateFrom && tb.CreatedDate <= dateTo)
                                     && lstBusinessInputIds.Contains(tb.BusinessId)
                               select new ItemizedCancelRefundModels
                               {
                                   BusinessId = tb.BusinessId,
                                   OrderId = tb.OrderId,
                                   ItemId = tb.ItemId,
                                   ItemName = tb.ItemName,
                                   Amount = tb.Amount,
                                   CancelUser = tb.CancelUser,
                                   RefundUser = tb.RefundUser,
                                   CreatedDate = tb.CreatedDate,
                                   StoreId = tb.StoreId,
                                   IsRefund = tb.IsRefund
                               }).ToList();
                return lstData;
            }
        }

        #region Report with new DB from table [R_PosSale], [R_PosSaleDetail]
        public List<ItemizedCancelRefundModels> GetDataItemizedCancelRefunds_NewDB(DateTime dateFrom, DateTime dateTo, List<string> lstStoreIds, List<string> lstBusinessInputIds)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from ps in cxt.R_PosSale
                               from psd in cxt.R_PosSaleDetail.Where(ww => ww.StoreId == ps.StoreId && ww.OrderId == ps.OrderId)
                               where lstStoreIds.Contains(ps.StoreId)
                                     && (ps.CreatedDate >= dateFrom && ps.CreatedDate <= dateTo)
                                     && lstBusinessInputIds.Contains(ps.BusinessId)
                                     && (psd.Mode == (int)Commons.EStatus.Refund || psd.Mode == (int)Commons.EStatus.Deleted)
                               select new ItemizedCancelRefundModels
                               {
                                   BusinessId = ps.BusinessId,
                                   OrderId = ps.OrderId,
                                   ItemId = psd.ItemId,
                                   ItemName = psd.ItemName,
                                   Amount = psd.TotalAmount,
                                   CancelUser = psd.CancelUser, 
                                   RefundUser = psd.RefundUser,  
                                   CreatedDate = ps.CreatedDate,
                                   StoreId = ps.StoreId,
                                   IsRefund = ((psd.Mode == (int)Commons.EStatus.Refund) ? true : false)
                               }).ToList();
                return lstData;
            }
        }
        #endregion
    }
}
