using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class TipServiceChargeFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public TipServiceChargeFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<TipServiceChargeModels> GetListTipServiceCharge(string StoreID = null, string ID = null)
        {
            List<TipServiceChargeModels> listdata = new List<TipServiceChargeModels>();
            try
            {
                TipServiceChargeApiModels paraBody = new TipServiceChargeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetServiceCharge, null, paraBody);
                dynamic data = result.Data;
                if (!string.IsNullOrEmpty(StoreID))
                {
                    var lstZ = data["ServiceCharge"];
                    var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                    var TipServiceChargeModels = JsonConvert.DeserializeObject<TipServiceChargeModels>(lstContent);
                    listdata.Add(TipServiceChargeModels);
                }
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("TipServiceCharge_GetList: " + e);
                return listdata;
            }

        }

        public bool InsertOrUpdateTipServiceCharge(TipServiceChargeModels model, ref string msg)
        {
            try
            {
                TipServiceChargeApiModels paraBody = new TipServiceChargeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                paraBody.IsApplyForEatIn = model.IsApplyForEatIn;
                paraBody.IsApplyForTakeAway = model.IsApplyForTakeAway;
                paraBody.IsIncludedOnBill = model.IsIncludedOnBill;
                paraBody.IsAllowTip = model.IsAllowTip;
                paraBody.IsCurrency = model.IsCurrency;
                paraBody.Value = model.Value;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.UpdateServiceCharge, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        _logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("TipServiceCharge_InsertOrUpdate: " + e);
                return false;
            }
        }
    }
}
