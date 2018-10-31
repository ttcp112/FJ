using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Xero;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class UsageManagementFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private InventoryFactory _inventoryFactory = new InventoryFactory();
        //private BaseFactory _baseFactory = null;
        private IngredientFactory _ingredientFactory = new IngredientFactory();
        public UsageManagementFactory()
        {
            //_baseFactory = new BaseFactory();
        }

        public async void SaveUsageManagement(string companyId, string storeId, string businessId, DateTime dFrom, DateTime dTo, List<UsageManagementModel> lstUsage)
        {
            using (var cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        I_UsageManagement usageManagement = new I_UsageManagement();
                        List<I_UsageManagementDetail> lstDetail = new List<I_UsageManagementDetail>();
                        List<I_UsageManagementItemDetail> lstItemDetail = new List<I_UsageManagementItemDetail>();
                        I_UsageManagementDetail detail = null;
                        I_UsageManagementItemDetail itemDetail = null;

                        usageManagement.Id = Guid.NewGuid().ToString();
                        usageManagement.StoreId = storeId;
                        usageManagement.DateFrom = dFrom;
                        usageManagement.DateTo = dTo;
                        usageManagement.BusinessId = businessId;
                        usageManagement.IsStockInventory = false;

                        //detail
                        foreach (var item in lstUsage)
                        {
                            detail = new I_UsageManagementDetail();
                            detail.Id = Guid.NewGuid().ToString();
                            detail.UsageManagementId = usageManagement.Id;
                            detail.IngredientId = item.Id;
                            detail.Usage = item.Usage;
                            int indexDetail = 1;
                            foreach (var subItem in item.ListDetail)
                            {
                                itemDetail = new I_UsageManagementItemDetail();
                                itemDetail.Id = Guid.NewGuid().ToString();
                                itemDetail.IndexList = indexDetail;
                                itemDetail.UsageManagementDetailId = detail.Id;
                                itemDetail.BusinessDay = subItem.BusinessDay;
                                itemDetail.ItemId = subItem.ItemId;
                                itemDetail.ItemName = subItem.ItemName;
                                itemDetail.Qty = subItem.Qty;
                                itemDetail.Usage = subItem.Usage;

                                lstItemDetail.Add(itemDetail);

                                indexDetail++;
                            }

                            lstDetail.Add(detail);
                        }
                        cxt.I_UsageManagement.Add(usageManagement);
                        cxt.I_UsageManagementDetail.AddRange(lstDetail);
                        cxt.I_UsageManagementItemDetail.AddRange(lstItemDetail);

                        cxt.SaveChanges();
                        transaction.Commit();
                        NSLog.Logger.Info("SaveUsageManagement", lstUsage);
                        //Cal update inventory
                        _inventoryFactory.UpdateInventoryWhenSale(companyId, lstUsage, storeId, usageManagement.Id, businessId, dFrom, dTo);
                        // update to xero
                        if (Commons.IsXeroIngredient)
                        {
                            StockUsageFactory _stockUsageFactory = new StockUsageFactory();
                            _stockUsageFactory.PushDataToXero(new UsageManagementRequest() { DateTo = dTo, StoreId = storeId }, lstUsage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
        }

        public List<UsageManagementModel> GetUsageManagement(UsageManagementRequest request)
        {
            var result = new List<UsageManagementModel>();
            request.DateFrom = new DateTime(request.DateFrom.Year, request.DateFrom.Month, request.DateFrom.Day, 0, 0, 0);
            request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);
            BaseFactory _baseFactory = new BaseFactory();
            using (var cxt = new NuWebContext())
            {
                //get business day
               
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.StoreId, request.Mode);
                if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
                {
                    var dFrom = _lstBusDayAllStore.Min(ss => ss.DateFrom);
                    var dTo = _lstBusDayAllStore.Max(ss => ss.DateTo);
                    int index = 1;
                    var query = (from u in cxt.I_UsageManagement
                                 join d in cxt.I_UsageManagementDetail on u.Id equals d.UsageManagementId
                                 join i in cxt.I_Ingredient on d.IngredientId equals i.Id
                                 where u.DateFrom >= dFrom && u.DateTo <= dTo
                                 select new { d, i });
                    if (query != null && query.Any())
                    {
                        var lstTmp = new List<UsageManagementModel>();
                        UsageManagementModel obj = null;
                        foreach (var item in query)
                        {
                            obj = new UsageManagementModel();
                            //obj.Index = index;
                            obj.Id = item.d.Id;
                            obj.Code = item.i.Code;
                            obj.Name = item.i.Name;
                            obj.UOMName = item.i.BaseUOMName;
                            obj.Usage = item.d.Usage;

                            lstTmp.Add(obj);
                            //index++;
                        }
                        var lstGroupIngrdient = lstTmp.GroupBy(gg => gg.Code);
                        foreach (var item in lstGroupIngrdient)
                        {
                            obj = new UsageManagementModel();
                            obj.Index = index;
                            obj.ListUsageManagementDetailId = string.Join("|", item.Select(ss => ss.Id));
                            obj.Code = item.Key;
                            obj.Name = item.Select(ss => ss.Name).FirstOrDefault();
                            obj.UOMName = item.Select(ss => ss.UOMName).FirstOrDefault();
                            obj.Usage = item.Sum(ss => ss.Usage);

                            result.Add(obj);
                            index++;
                        }
                        //result = result.OrderBy(oo => oo.Index).ToList();
                    }

                }
            }
            return result;
        }

        public UsageManagementModel GetUsageManagementItemDetail(List<string> lstUsageManagementDetailId)
        {
            UsageManagementModel usageManagementModel = new UsageManagementModel();
            using (var cxt = new NuWebContext())
            {
                var query = cxt.I_UsageManagementItemDetail.Where(ww => lstUsageManagementDetailId.Contains(ww.UsageManagementDetailId)).ToList();
                UsageManagementDetailModel detail = null;
                int index = 1;
                foreach (var item in query)
                {
                    detail = new UsageManagementDetailModel();
                    detail.BusinessDay = item.BusinessDay;
                    detail.Index = index;
                    detail.ItemName = item.ItemName;
                    detail.Qty = item.Qty;
                    detail.Usage = item.Usage;

                    usageManagementModel.ListDetail.Add(detail);
                    index++;
                }

            }
            return usageManagementModel;
        }

        #region Xero

        private bool IsPush(DateTime dTo, string storeId)
        {
            if (Commons.IsXeroIngredient)
            {
                using (var cxt = new NuWebContext())
                {
                    var obj = cxt.I_UsageManagementXeroTrackLog.Where(ww => DbFunctions.TruncateTime(ww.ToDate) == DbFunctions.TruncateTime(dTo)
                    && ww.StoreId == storeId).FirstOrDefault();
                    if (obj != null)
                    {
                        //have push
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public bool PushDataToXero(UsageManagementRequest request)
        {
            bool result = true;
            if (IsPush(request.DateTo, request.StoreId))
            {
                //List<UsageManagementModel> lstCalResult = CalUsageManagementwithoutDetail(request);
                List<UsageManagementModel> lstCalResult = GetUsageManagement(request);
                //lstCalResult.Add(new UsageManagementModel()
                //{
                //    Code = "008",
                //    Usage = 2
                //});
                try
                {
                    IngredientSyncRequestDTO model = new IngredientSyncRequestDTO();
                    model.AppRegistrationId = Commons.XeroRegistrationAppId;
                    model.AccessToken = Commons.XeroAccessToken;
                    model.StoreId = request.StoreId;

                    var lstIngredient = _ingredientFactory.GetIngredient(null);

                    if (lstIngredient != null && lstIngredient.Count > 0)
                    {
                        lstIngredient = lstIngredient.Where(ww => !string.IsNullOrEmpty(ww.XeroId)).ToList();
                        if (lstIngredient != null && lstIngredient.Count > 0)
                        {
                            foreach (var item in lstCalResult)
                            {
                                var obj = lstIngredient.Where(ww => ww.Code.ToUpper().Equals(item.Code.ToUpper())).FirstOrDefault();
                                if (obj != null)
                                {
                                    model.Items.Add(new IngredientUsageSyncItem()
                                    {
                                        Id = obj.XeroId,
                                        Code = obj.Code,
                                        QuantityUsed = (decimal)item.Usage
                                    });
                                }
                            }
                            if (model.Items != null && model.Items.Count > 0)
                            {
                               
                                result = XeroFactory.SyncIngredientsUsageToXero(request.DateTo, request.StoreId, model).Result;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    _logger.Error(ex);
                }
            }
            return result;
        }
        #endregion
    }
}
