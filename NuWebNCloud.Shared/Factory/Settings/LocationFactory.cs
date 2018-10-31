using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class LocationFactory : BaseFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public LocationFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<GeneralSettingModels> GetRegion(string StoreID = "", string Region = "")
        {
            List<GeneralSettingModels> listdata = new List<GeneralSettingModels>();
            try
            {
                GeneralSettingApiModels paraBody = new GeneralSettingApiModels();
                paraBody.StoreID = StoreID;
                paraBody.Region = Region;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetFollowRegion, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListSettings"];
                var lstContent = JsonConvert.SerializeObject(lstZ);
                listdata = JsonConvert.DeserializeObject<List<GeneralSettingModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("GeneralSetting_GetList: " + e);
                return listdata;
            }
        }
    }
}
