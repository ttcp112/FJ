using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.AccessControl;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.AccessControl
{
    public class DrawerFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public DrawerFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<DrawerModels> GetListDrawer(string StoreID = null, string ID = null)
        {
            List<DrawerModels> listdata = new List<DrawerModels>();
            try
            {
                DrawerApiModels paraBody = new DrawerApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetDrawer, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListDrawer"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<DrawerModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("Drawer_GetList: " + e);
                return listdata;
            }

        }

        public bool InsertOrUpdateDrawer(DrawerModels model, ref string msg)
        {
            try
            {
                DrawerApiModels paraBody = new DrawerApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                paraBody.Name = model.Name;
                paraBody.IPAddress = model.IPAddress;
                paraBody.Port = model.Port;
                paraBody.KickCode = model.KickCode;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditDrawer, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        _logger.Error(result.Message);
                        msg = result.Message;
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
                _logger.Error("Drawer_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeleteDrawer(string ID, ref string msg)
        {
            try
            {
                DrawerApiModels paraBody = new DrawerApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ID = ID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteDrawer, null, paraBody);
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
                _logger.Error("Drawers_Delete: " + e);
                return false;
            }
        }
    }
}
