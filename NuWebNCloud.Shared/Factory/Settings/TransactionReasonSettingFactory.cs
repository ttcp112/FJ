using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings.TransactionReasonSetting;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class TransactionReasonSettingFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public TransactionReasonSettingFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public List<ReasonModels> GetListReason()
        {
            List<ReasonModels> listdata = new List<ReasonModels>();
            try
            {
                ReasonRequestModels paraBody = new ReasonRequestModels();
                

                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetListTransactionReasonSettings, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListReason"];
                var lstContent = JsonConvert.SerializeObject(lstZ);
                listdata = JsonConvert.DeserializeObject<List<ReasonModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("Reason_GetList: " + e);
                return listdata;
            }

        }

        public ReasonModels GetDetailReason(string ID)
        {
            ReasonModels Reason = new ReasonModels();
            try
            {
                ReasonRequestModels paraBody = new ReasonRequestModels();


                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.ID = ID;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetDetailTransactionReasonSettings, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["Reason"];
                var lstContent = JsonConvert.SerializeObject(lstZ);
                Reason = JsonConvert.DeserializeObject<ReasonModels>(lstContent);
                return Reason;
            }
            catch (Exception e)
            {
                _logger.Error("Reason_GetList: " + e);
                return Reason;
            }

        }

        public bool InsertOrUpdateReason(ReasonModels model, ref string msg)
        {
            try
            {
                ReasonRequestModels paraBody = new ReasonRequestModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.Reason = model;
                
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrUpdateTransactionReasonSettings, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                    {
                        msg = result.Message;
                        return true;
                        
                    }
                    else
                    {
                        _logger.Error(result.Message);
                        msg = result.Message;
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
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
                _logger.Error("Reason_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeleteReason(string ID, ref string msg)
        {
            try
            {
                ReasonRequestModels paraBody = new ReasonRequestModels();
                paraBody.ID = ID;
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteTransactionReasonSettings, null, paraBody);
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
                msg = e.ToString();
                _logger.Error("Reason_Delete: " + e);
                return false;
            }
        }
    }
}
