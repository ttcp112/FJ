using Newtonsoft.Json;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Xero.Suppliers;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Xero
{
    public class SupplierXeroFactory
    {
        private static BaseFactory _baseFactory = new BaseFactory();


        public bool CreateOrUpdateSupplier(string url, SupplierXeroModels models, string SupplierId, ref string msg)
        {
            try
            {
                SupplierXeroRequestModels paraBody = new SupplierXeroRequestModels();
                paraBody.AccessToken = models.AccessToken;
                paraBody.AppRegistrationId = models.AppRegistrationId;
                paraBody.StoreId = models.StoreId;
                paraBody.Contact = models.Contact;
                

                NSLog.Logger.Info("CreateOrUpdateSupplier_Request:", paraBody);
                var result = (ResponseApiModels)ApiResponseXero.Post<ResponseApiModels>(url + "/" + Commons.XeroApi_InsertOrUpdateSupplier, null, paraBody);
                if (result.Success)
                {
                    dynamic RawData = result.RawData;
                    var content = JsonConvert.SerializeObject(RawData);
                    SupplierXeroResponseModels data = JsonConvert.DeserializeObject<SupplierXeroResponseModels>(content);
                    string ContactID = data.ContactID;
                    //----Update XeroId for Supplier
                    using (NuWebContext cxt = new NuWebContext())
                    {
                        using (var transaction = cxt.Database.BeginTransaction())
                        {
                            try
                            {
                                var itemUpdate = (from tb in cxt.I_Supplier
                                                  where tb.Id == SupplierId
                                                  select tb).FirstOrDefault();
                                if (itemUpdate != null)
                                {
                                    itemUpdate.XeroId = ContactID;
                                    cxt.SaveChanges();
                                    transaction.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                NSLog.Logger.Error("UpdateXeroIdSupplier|Error: ", ex);
                                transaction.Rollback();
                            }
                            finally
                            {
                                if (cxt != null)
                                    cxt.Dispose();
                            }
                        }
                    }

                    NSLog.Logger.Info("CreateOrUpdateSupplier_Response: ", data);
                    return true;
                }
                else
                {
                    NSLog.Logger.Info("CreateOrUpdateSupplier|Result[False]|Message: ", result.Message);
                    msg = result.Message;
                    msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("CreateOrUpdateSupplier|Error: ", ex);
                return false;
            }
        }

        public List<SupplierXeroResponseModels> GetSupplierXero(string url, string AppRegistrationId, string StoreId, string ContactName, string ContactID, bool IsSupplier, ref string msg)
        {
            try
            {
                SupplierXeroRequestModels paraBody = new SupplierXeroRequestModels();
                paraBody.AppRegistrationId = AppRegistrationId;
                paraBody.StoreId = StoreId;
                paraBody.ContactID = ContactID;
                paraBody.ContactName = ContactName;
                paraBody.IsSupplier = IsSupplier;
                NSLog.Logger.Info("GetSupplierXero_Request:", paraBody);
                var result = (ResponseApiModels)ApiResponseXero.Post<ResponseApiModels>(url + "/" + Commons.XeroApi_GetContacts, null, paraBody);
                if (result.Success)
                {
                    dynamic RawData = result.RawData;
                    var content = JsonConvert.SerializeObject(RawData);
                    var data = JsonConvert.DeserializeObject<List<SupplierXeroResponseModels>>(content);
                    NSLog.Logger.Info("GetSupplierXero_Response: ", data);
                    return data;
                }
                else
                {
                    NSLog.Logger.Info("GetSupplierXero|Result[False]|Message: ", result.Message);
                    msg = result.Message;
                    msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                    return null;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetSupplierXero | Error: ", ex);
                return null;
            }
        }
    }
}
