using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory
{
    public class TaxFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public TaxFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public bool Insert(List<TaxModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<G_Tax> lstInsert = new List<G_Tax>();
                        G_Tax itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new G_Tax();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.Name = item.Name;
                            itemInsert.TaxType = item.TaxType;
                            itemInsert.Percent = item.Percent;
                            itemInsert.IsActive = item.IsActive;
                            itemInsert.DateCreated = item.DateCreated;
                            itemInsert.UserCreated = item.UserCreated;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.G_Tax.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Tax data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Tax data fail", ex);
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
            //_baseFactory.InsertTrackingLog("G_Tax", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public int GetDetailTaxForStore(string StoreId)
        {
            NuWebNCloud.Shared.Factory.Settings.TaxFactory _factory = new Settings.TaxFactory();
            var datas = _factory.GetListTax(StoreId);
            if(datas != null && datas.Any())
            {
                var tax = datas.Where(tb => tb.IsActive == true)
                        .Select(ss => ss.TaxType).FirstOrDefault();
                return tax;
            }
            return 0;
        }
        public int GetTaxTypeForStore(string StoreId)
        {
            int result = 0;
            try
            {
                BaseApiRequestModel request = new BaseApiRequestModel();
                request.AppKey = Commons.AppKey;
                request.AppSecret = Commons.AppSecret;
                request.StoreId = StoreId;
                List<TaxModels> lstData = new List<TaxModels>();
                var resultData = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Tax_Get, null, request);
                if (resultData.Success)
                {
                    dynamic data = resultData.Data;
                    var ListData = data["ListTax"];
                    foreach (var item in ListData)
                    {
                        lstData.Add(new TaxModels
                        {
                            IsActive = item["IsActive"],
                            TaxType = item["TaxType"],

                        });
                    }
                }
                result= lstData.Where(ww=>ww.IsActive).OrderBy(oo => oo.Name).Select(ss=>ss.TaxType).FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.Error("GetTaxTypeForStore Report: " + e);
            }
            return result;
        }
    }
}
