using Newtonsoft.Json;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings.ScreenSaverMode;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class ScreenSaverModeFactory
    {
        public bool InsertOrUpdate(List<ScreenSaverModeApiModel> model, string StoreID, ref string msg)
        {
            try
            {
                ScreenSaverModeApiModels paraBody = new ScreenSaverModeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.ListImageDTO = model;
                paraBody.StoreID = StoreID;
                NSLog.Logger.Info("ScreenSaverModeInsertOrUpdate_Input : ", paraBody);
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.KioskImageCreateOrEdit, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        NSLog.Logger.Info("ScreenSaverModeInsertOrUpdate :", result.Message);
                        msg = result.Message;
                        return false;
                    }
                }
                else
                {
                    NSLog.Logger.Info("ScreenSaverModeInsertOrUpdate", result);
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("ScreenSaverModeInsertOrUpdate_Error : ", e);
                return false;
            }
        }

        public List<ScreenSaverModeModels> GetListScreenSaverMode(string StoreID = null, string ID = null)
        {
            List<ScreenSaverModeModels> listdata = new List<ScreenSaverModeModels>();
            try
            {
                ScreenSaverModeApiModels paraBody = new ScreenSaverModeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;
                paraBody.Mode = 1;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.KioskImageGetList, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListImage"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<ScreenSaverModeModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetListScreenSaverMode_Error: ", e);
                return listdata;
            }
        }

        public bool DeleteScreenSaverMode(string ID, string StoreID, ref string msg)
        {
            try
            {
                ScreenSaverModeApiModels paraBody = new ScreenSaverModeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;
                paraBody.StoreID = StoreID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.KioskImageDelete, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        NSLog.Logger.Info("DeleteScreenSaverMode", result.Message);
                        return false;
                    }
                }
                else
                {
                    NSLog.Logger.Info("DeleteScreenSaverMode", result);
                    return false;
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
                NSLog.Logger.Error("DeleteScreenSaverMode_Error", e);
                return false;
            }
        }
    }
}
