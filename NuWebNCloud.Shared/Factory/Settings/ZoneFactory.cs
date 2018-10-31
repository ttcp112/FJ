using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings.Zone;
using NuWebNCloud.Shared.Utilities;
//using ServiceStack.Html;
using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class ZoneFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public ZoneFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public List<ZoneModels> GetListZone(string StoreID = null, string ID = null, List<string> ListOrganizationId = null)
        {
            List<ZoneModels> listdata = new List<ZoneModels>();
            try
            {
                ZoneApiModels paraBody = new ZoneApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;
                paraBody.ListOrgID = ListOrganizationId;


                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetZones, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListZone"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<ZoneModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("Zone_GetList: " + e);
                return listdata;
            }

        }

        public bool InsertOrUpdateZones(ZoneModels model, ref string msg)
        {
            try
            {
                ZoneApiModels paraBody = new ZoneApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;

                paraBody.Name = model.Name;
                paraBody.Description = model.Description;
                paraBody.Width = model.Width;
                paraBody.Height = model.Height;
                //====================
                
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditZones, null, paraBody);
                
                if (result != null)
                {
                    if (result.Success)
                        return true;
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
                _logger.Error("Zone_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeleteZones(string ID, ref string msg)
        {
            try
            {
                ZoneApiModels paraBody = new ZoneApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteZones, null, paraBody);
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
                _logger.Error("Zones_Delete: " + e);
                return false;
            }
        }
    }
}
