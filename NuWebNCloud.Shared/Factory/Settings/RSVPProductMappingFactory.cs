using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings.RSVPProductMapping;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class RSVPProductMappingFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public RSVPProductMappingFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<RSVPStoreProducMappingModels> GetList(List<string> ListStoreID = null, List<string> ListOrgID = null)
        {
            List<RSVPStoreProducMappingModels> listData = new List<RSVPStoreProducMappingModels>();
            try
            {
                RSVPProductMappingApiModels paraBody = new RSVPProductMappingApiModels();
                paraBody.ListStoreID = ListStoreID;
                paraBody.ListOrgID = ListOrgID;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetListRSVP, null, paraBody);
                dynamic data = result.Data;
                var lstData = data["ListStoreProductMapping"];
                var lstContent = JsonConvert.SerializeObject(lstData);
                listData = JsonConvert.DeserializeObject<List<RSVPStoreProducMappingModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("GetListRSVP: " + e);
                return listData;
            }
        }

        public bool InsertOrUpdate(RSVPProductMappingModels model, string CreatedUser, ref string msg)
        {
            try
            {
                RSVPProductMappingApiModels paraBody = new RSVPProductMappingApiModels();
                paraBody.ListRSVPStoreProductMapping = model.ListRSVPStoreProductMapping;
                paraBody.Type = model.Type;
                paraBody.CreatedUser = CreatedUser;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditRSVP, null, paraBody);
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
                    msg = result.ToString();
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("CreateOrEditRSVP: " + e);
                msg = e.ToString();
                return false;
            }
        }
    }
}
