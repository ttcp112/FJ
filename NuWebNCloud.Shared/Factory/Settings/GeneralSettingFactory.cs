using Newtonsoft.Json;
using NSLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class GeneralSettingFactory
    {
        private BaseFactory _baseFactory = null;

        public GeneralSettingFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<GeneralSettingModels> GetListGeneralSetting(ref string StoreName, string StoreID = null, string ID = null)
        {
            List<GeneralSettingModels> listdata = new List<GeneralSettingModels>();
            try
            {
                GeneralSettingApiModels paraBody = new GeneralSettingApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;
                paraBody.Mode = 1;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetGeneralSettings, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListSettings"];
                StoreName = data["StoreName"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<GeneralSettingModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GeneralSetting_GetList: ", e);
                return listdata;
            }
        }

        public bool InsertOrUpdateGeneralSetting(GeneralSettingModels model)
        {
            try
            {
                GeneralSettingApiModels paraBody = new GeneralSettingApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                paraBody.ListSettings = model.ListSettings;

                paraBody.Mode = 1;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.SaveGeneralSetting, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        NSLog.Logger.Info("InsertOrUpdate_Setting", result);
                        return false;
                    }
                }
                else
                {
                    NSLog.Logger.Info("InsertOrUpdate_Setting", result);
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GeneralSetting_InsertOrUpdate: ", e);
                return false;
            }
        }

        public List<DeliverySettingDTO> GetListDeliverySetting(string StoreID = null, string ID = null)
        {
            List<DeliverySettingDTO> listdata = new List<DeliverySettingDTO>();
            try
            {
                GeneralSettingApiModels paraBody = new GeneralSettingApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;
                paraBody.Mode = 1;
                paraBody.Type = -1;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.SettingDeliveryGet, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListDeliverySetting"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<DeliverySettingDTO>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("DeliverySetting_GetList: ", e);
                return listdata;
            }
        }

        public bool InsertOrUpdateGeneraDeliverylSetting(GeneralSettingModels model)
        {
            try
            {
                GeneralSettingApiModels paraBody = new GeneralSettingApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                
                paraBody.ListDeliverySetting = model.ListDeliverySetting;
                paraBody.StoreID = model.StoreID;
                paraBody.Mode = (int)Commons.EStatus.Actived;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.SettingDeliveryCreateOrEdit, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        NSLog.Logger.Info("InsertOrUpdate_DeliverylSetting", result);
                        return false;
                    }
                }
                else
                {
                    NSLog.Logger.Info("InsertOrUpdate_DeliverylSetting", result);
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("DeliverySetting_InsertOrUpdate: ", e);
                return false;
            }
        }

        public TimeSpan ConvertStringToTimeSpan(string valueInput)
        {
            TimeSpan timeSpan = new TimeSpan(0,0,0);
            try
            {
                TimeSpan.TryParse(valueInput, out timeSpan);
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ConvertStringToTimeSpan error", ex);
            }
            return timeSpan;
        }

        public  GeneralSettingModels GetInvoicePrintSetting(string StoreID = null, string ID = null)
        {
            GeneralSettingModels resultData = new GeneralSettingModels();
            try
            {
                GeneralSettingApiModels paraBody = new GeneralSettingApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;
                paraBody.Mode = 1;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.SettingGetOtherPrintSetting, null, paraBody);

                if (result.Success)
                {
                    dynamic data = result.Data;
                    var lst = data["ListInvoice"];
                    var lstContent = JsonConvert.SerializeObject(lst/*result.RawData*/);
                    resultData.ListInvoice = JsonConvert.DeserializeObject<List<InvoiceDTO>>(lstContent);
                }
                else
                {
                    resultData.MessageInvoice = result.Message;
                }                
                return resultData;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("DeliverySetting_GetList: ", e);
                return resultData;
            }
        }
        public bool InsertOrUpdateGeneraInvoice(GeneralSettingModels model)
        {
            try
            {
                GeneralSettingApiModels paraBody = new GeneralSettingApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                if (model.ListInvoice[0].Setting != null)
                {
                    paraBody.Setting = model.ListInvoice[0].Setting;
                }
                paraBody.ID = model.InvoiceID;
                paraBody.StoreID = model.StoreID;
                if (model.InvoiceID == "0")
                {
                    paraBody.ID = null;
                    paraBody.Mode = (int)Commons.EStatus.Deleted;
                }                    
                else
                {
                    paraBody.Mode = (int)Commons.EStatus.Actived;
                }

                NSLog.Logger.Info("InsertOrUpdateGeneraInvoice request", paraBody);
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.SettingOtherPrintSettingSave, null, paraBody);
                NSLog.Logger.Info("InsertOrUpdateGeneraInvoice result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        NSLog.Logger.Info("InsertOrUpdateGeneraInvoice fail", result);
                        //_logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    NSLog.Logger.Info("InsertOrUpdateGeneraInvoice error", result);
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("InsertOrUpdateGeneraInvoice ex", e);
                return false;
            }
        }
    }
}
