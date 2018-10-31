using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class SRegisterAccountFactory
    {
        public SRegisterAccountFactory()
        {
        }

        public string GetQRCode(string id)
        {
            try
            {
                RequestBaseModels paraBody = new RequestBaseModels();
                paraBody.ID = id;
                NSLog.Logger.Info("GetQRCode Request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetRegisterAccount, null, paraBody);
                NSLog.Logger.Info("GetQRCode Result", result);
                if (result != null)
                {
                    if (result.Success)
                        return result.Message;
                    else
                    {
                        NSLog.Logger.Info("GetQRCode_Fail: ", result.Message);
                        return string.Empty;
                    }
                }
                else
                {
                    NSLog.Logger.Info("GetQRCode_Fail: ", result);
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetQRCode_Fail: ", e);
                return string.Empty;
            }
        }
        public bool CreateQRCode(string AppRegisteredID, string DeviceName, string CreatedUser, ref string msg)
        {
            try
            {
                RequestBaseModels paraBody = new RequestBaseModels();
                paraBody.AppRegisteredID = AppRegisteredID;
                paraBody.DeviceName = DeviceName;
                paraBody.CreatedUser = CreatedUser;
                NSLog.Logger.Info("RegisterAccount Request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.RegisterAccount, null, paraBody);
                NSLog.Logger.Info("RegisterAccount Result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        NSLog.Logger.Info("RegisterAccount_Fail: ", result.Message);
                        return false;
                    }
                }
                else
                {
                    NSLog.Logger.Info("RegisterAccount_Fail: ", result);
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("RegisterAccount_Fail: ", e);
                return false;
            }
        }
    }
}
