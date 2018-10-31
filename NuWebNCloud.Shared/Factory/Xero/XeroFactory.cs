using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Models.Xero;
using NuWebNCloud.Shared.Models.Xero.GenerateInvoice;
using NuWebNCloud.Shared.Models.Xero.Ingredient;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Xero
{
    public class XeroFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static BaseFactory _baseFactory = new BaseFactory();
        private static IngredientFactory _ingredientFactory = new IngredientFactory();

        #region Xero Third Party
        public static async Task<bool> PushIngredientsToXero(XeroIngredientModel item)
        {
            bool result = false;
            try
            {
                using (var client = new HttpClient())
                {
                    string idOld = item.Id;
                    item.Id = null;
                    client.BaseAddress = new Uri(Commons.XeroURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        //HttpResponseMessage
                        HttpResponseMessage response = await client.PostAsJsonAsync(Commons.XeroApi_InsertInventory, item).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)//sucess
                        {
                            ResponseXeroModelBase content = await response.Content.ReadAsAsync<ResponseXeroModelBase>();
                            if (content != null)
                            {
                                if (content.Data != null)
                                {
                                    _ingredientFactory.UpdateIngredient(idOld, content.Data.Id);
                                    NSLog.Logger.Info(string.Format("Save ingredient [{0}] success!", item.Code));
                                    result = true;
                                }
                            }
                        }
                        else
                        {
                            //log
                            _baseFactory.InsertIngredientTrackLog(item.Code);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return result;
        }
        public static async Task<GetIngredientResponseDTO> GetIngredientsFromXero(XeroBaseModel model)
        {
            using (var client = new HttpClient())
            {
                GetIngredientResponseDTO content = new GetIngredientResponseDTO();
                client.BaseAddress = new Uri(Commons.XeroURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(Commons.XeroApi_GetInventory, model).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        content = await response.Content.ReadAsAsync<GetIngredientResponseDTO>();
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
                return content;
            }
        }

        public static async Task<bool> SyncIngredientsUsageToXero(DateTime dTo, string storeId, IngredientSyncRequestDTO model)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Commons.XeroURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsJsonAsync(Commons.XeroApi_UpdateInventory, model).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        //log
                        NSLog.Logger.Info("SyncIngredientsUsageToXero Store", storeId);
                        NSLog.Logger.Info("SyncIngredientsUsageToXero Data", model);
                        _baseFactory.InsertUsageXeroTrackLog(dTo, storeId);
                    }
                    return response.IsSuccessStatusCode;
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    return false;
                }
            }
        }
        #endregion Xero 

        #region Newstead Technoloies Xero
        /// <summary>
        /// Get List Xero Setting
        /// </summary>
        /// <param name="StoreID"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<XeroDTO> GetListXeroSetting(string StoreID = null, string ThirdPartyID = null, string Url = null, string ID = null)
        {
            List<XeroDTO> lstXeroDTO = new List<XeroDTO>();
            XeroSettingApiModels paraBody = new XeroSettingApiModels();
            paraBody.StoreId = StoreID;
            //paraBody.AppRegistrationId = Commons._XeroRegistrationAppId;
            paraBody.AppRegistrationId = ThirdPartyID;

            try
            {
                NSLog.Logger.Info("GetList_XeroSetting_Request: ", paraBody);
                var result = (ResponseApiModels)ApiResponseXero.Post<ResponseApiModels>(Url + "/" + Commons.XeroApi_GetSetting, null, paraBody);
                NSLog.Logger.Info("GetList_XeroSetting_Response: ", result);
                if (result.Success)
                {
                    dynamic temp = result.RawData;
                    var lstContent = JsonConvert.SerializeObject(temp/*result.RawData*/);
                    lstXeroDTO = JsonConvert.DeserializeObject<List<XeroDTO>>(lstContent);
                }
                return lstXeroDTO;
            }
            catch (Exception e)
            {
                NSLog.Logger.Info("GetList_XeroSetting", e);
                return lstXeroDTO;
            }

        }

        /// <summary>
        /// Function [GenerateInvoice]
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool GenerateInvoice(string _hostXeroApi, GenerateInvoiceModels request, ref string msg)
        {
            try
            {
                NSLog.Logger.Info("GenerateInvoice_Request: ", request);
                var result = (ResponseApiModels)ApiResponseXero.Post<ResponseApiModels>(_hostXeroApi + "/" + Commons.XeroApi_Generate_Invoice, null, request);
                NSLog.Logger.Info("GenerateInvoice_Response: ", result);
                if (result.Success)
                {
                    return true;
                }
                else
                {
                    msg = result.Message;
                    msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GenerateInvoice|Error: ", ex);
                return false;
            }
        }

        /// <summary>
        /// Function [GeneratePO]
        /// </summary>
        /// <param name="_hostXeroApi"></param>
        /// <param name="request"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool GeneratePO(string _hostXeroApi, GeneratePOModels request, ref string msg)
        {
            try
            {
                NSLog.Logger.Info("POInsertUpdate_Request: ", request);
                var result = (ResponseApiModels)ApiResponseXero.Post<ResponseApiModels>(_hostXeroApi + "/" + Commons.XeroApi_PO_Insert_Update, null, request);
                NSLog.Logger.Info("POInsertUpdate_Response: ", result);
                if (result.Success)
                    return true;
                else
                {
                    msg = result.Message;
                    msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("POInsertUpdate|Error: ", ex);
                return false;
            }
        }

        public bool SaleItems(string _hostXeroApi, XeroInvoiceReportDTO request, ref string msg)
        {
            try
            {

                NSLog.Logger.Info("GenerateInvoice_Request: ", request);
                var result = (ResponseApiModels)ApiResponseXero.Post<ResponseApiModels>(_hostXeroApi + Commons.XeroApi_Sales_Item, null, request);
                NSLog.Logger.Info("GenerateInvoice_Response: ", result);
                if (result.Success)
                {
                    //dynamic RawData = result.RawData;
                    //var content = JsonConvert.SerializeObject(RawData);
                    //var data = JsonConvert.DeserializeObject<XeroInvoiceResponseReportDTO>(content);

                    return true;
                }
                else
                {
                    msg = result.Message;
                    msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                    return false;
                }


            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GenerateInvoice|Error: ", ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="AppRegistrationId">[6aaee02a-5dc0-466a-9352-5e9e279c1fe2]</param>
        /// <param name="StoreId">[NS-XERO-INTEGRATION]</param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IngredientInsertUpdate(string _hostXeroApi, List<NSXeroIngredientModels> ListIng, string AppRegistrationId,
            string StoreId, ref string msg, ref NSXeroIngredientResponseModels data)
        {
            try
            {
                NSXeroIngredientRequestModels paraBody = new NSXeroIngredientRequestModels();
                paraBody.AppRegistrationId = AppRegistrationId;
                paraBody.StoreId = StoreId;
                paraBody.Ingredients = ListIng;

                NSLog.Logger.Info("IngredientInsertUpdate_Request: ", paraBody);
                var result = (ResponseApiModels)ApiResponseXero.Post<ResponseApiModels>(_hostXeroApi + Commons.XeroApi_Ingredient_Insert_Update, null, paraBody);
                if (result.Success)
                {
                    dynamic RawData = result.RawData;
                    var content = JsonConvert.SerializeObject(RawData);
                    data = JsonConvert.DeserializeObject<NSXeroIngredientResponseModels>(content);
                    NSLog.Logger.Info("IngredientInsertUpdate_Response: ", data);
                    return true;
                }
                else
                {
                    //Get List Error
                    dynamic RawData = result.RawData;
                    var content = JsonConvert.SerializeObject(RawData);
                    List<IngError> listMsg = JsonConvert.DeserializeObject<List<IngError>>(content);
                    result.Message = string.Join("|", listMsg.Select(x => x.Message));
                    NSLog.Logger.Info("IngredientInsertUpdate|Result[False]|Message: ", result.Message);
                    msg = result.Message;
                    msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                    return false;
                }

            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("IngredientInsertUpdate|Error: ", ex);
                return false;
            }
        }

        public ResultModels GeneratePurchaseOrderAPI(GeneratePurchaseOrderApiModels model)
        {
            NSLog.Logger.Info("Call_GeneratePurchaseOrderAPI_Request: ", model);
            ResultModels result = new ResultModels();
            try
            {
                var xero = Commons.GetIntegrateInfo(model.StoreId);
                /* GENERAL INVOICE XERO */
                if (xero != null)
                {
                    /* find supplier name by supplier id */
                    SupplierXeroFactory xeroSup = new SupplierXeroFactory();
                    string msgSupplier = string.Empty;
                    var sup = xeroSup.GetSupplierXero(model.ApiURL, model.AppRegistrationId, model.StoreId, model.SupplierName, "", false, ref msgSupplier);
                    if (sup != null)
                    {

                    }
                    var SupplierId = model.SupplierId;
                    var SupplierName = model.SupplierName;
                    string _ContactID = string.Empty;

                    XeroFactory _facXero = new XeroFactory();
                    var listItem = new List<InvoiceLineItemModels>();
                    if (model.ListItem != null && model.ListItem.Any())
                    {
                        string _AccountCode = Commons.AccountCode_Inventory;

                        using (var cxt = new NuWebContext())
                        {
                            var StockOnHand = (from _store in cxt.G_SettingOnStore
                                               from _setting in cxt.G_GeneralSetting
                                               where _store.SettingId == _setting.Id && _store.StoreId.Equals(model.StoreId)
                                                     && _store.Status && _setting.Status
                                                     && _setting.Code.Equals((byte)Commons.EGeneralSetting.StockOnHand)
                                               select _store).FirstOrDefault();
                            if (StockOnHand != null)
                                _AccountCode = StockOnHand.Value;
                            //-----------------
                            var objSupplier = cxt.I_Supplier.Where(o => o.Id.Equals(model.SupplierId)).FirstOrDefault();
                            _ContactID = (objSupplier == null ? "" : objSupplier.XeroId);
                        }

                        //GET TAX RATE
                        string TaxType = "TAX002";
                        NuWebNCloud.Shared.Factory.Settings.TaxFactory _taxFactory = new NuWebNCloud.Shared.Factory.Settings.TaxFactory();
                        var lstTaxes = _taxFactory.GetListTaxV2(model.StoreId);
                        var tax = lstTaxes.Where(w => w.IsActive && !string.IsNullOrEmpty(w.Rate)).FirstOrDefault();
                        if (tax != null)
                            TaxType = tax.Rate;

                        foreach (var _item in model.ListItem)
                        {
                            listItem.Add(new InvoiceLineItemModels
                            {
                                Description = _item.IngredientName,
                                ItemCode = _item.IngredientCode,
                                Quantity = Convert.ToDecimal(_item.Qty),
                                UnitAmount = Convert.ToDecimal(_item.UnitPrice),
                                LineAmount = Convert.ToDecimal(_item.Qty * _item.UnitPrice),
                                AccountCode = _AccountCode,
                                TaxType = TaxType //(model.TaxType == (int)Commons.ETax.AddOn ? "OUTPUT" : "INPUT"),
                            });
                        }
                    }
                    var modelXero = new GeneratePOModels
                    {
                        AppRegistrationId = model.AppRegistrationId,
                        StoreId = model.StoreId,
                        CurrencyCode = Commons._XeroCurrencyCode,
                        Reference = model.Code,
                        Contact = new InvoiceContactPOModels
                        {
                            ContactID = _ContactID
                        },
                        ClosingDatetime = model.ClosingDatetime,
                        // PO Time
                        LineAmountTypes = string.IsNullOrEmpty(model.LineAmountTypes.ToString()) ? (byte)Commons.ELineAmountType.Inclusive : model.LineAmountTypes,
                        PurchaseOrderStatus = string.IsNullOrEmpty(model.PurchaseOrderStatus.ToString()) ? (byte)Commons.EPurchaseOrderStatuses.Authorised : model.PurchaseOrderStatus,

                        AttentionTo = model.StoreId,
                        Telephone = model.Telephone,
                        DeliveryInstructions = model.Note,

                        Items = listItem
                    };
                    var msgXero = string.Empty;
                    var data = new GIResponseModels();
                    var resultXero = _facXero.GeneratePO(model.ApiURL, modelXero, ref msgXero);
                    result.IsOk = true;
                    result.Message = msgXero;
                    //======
                    result.RawData = data;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("Call_XeroGeneratePurchaseOrderAPI|Error: ", ex);
                result.IsOk = false;
            }
            return result;
        }

        public ResultModels GenerateReceiptNoteAPI(GenerateReceiptNoteApiModels model)
        {
            NSLog.Logger.Info("Call_GenerateReceiptNoteAPI_Request: ", model);
            ResultModels result = new ResultModels();
            try
            {
                var xero = Commons.GetIntegrateInfo(model.StoreId);
                /* GENERAL INVOICE XERO */
                if (xero != null)
                {
                    /* find supplier name by supplier id */
                    SupplierXeroFactory xeroSup = new SupplierXeroFactory();
                    string msgSupplier = string.Empty;
                    var sup = xeroSup.GetSupplierXero(model.ApiURL, model.AppRegistrationId, model.StoreId, model.SupplierName, "", false, ref msgSupplier);
                    if (sup != null)
                    {

                    }
                    XeroFactory _facXero = new XeroFactory();
                    var listItem = new List<InvoiceLineItemModels>();
                    if (model.ListItem != null && model.ListItem.Any())
                    {
                        string _AccountCode = Commons.AccountCode_Inventory;

                        using (var cxt = new NuWebContext())
                        {
                            var StockOnHand = (from _store in cxt.G_SettingOnStore
                                               from _setting in cxt.G_GeneralSetting
                                               where _store.SettingId == _setting.Id && _store.StoreId.Equals(model.StoreId)
                                                     && _store.Status && _setting.Status
                                                     && _setting.Code.Equals((byte)Commons.EGeneralSetting.StockOnHand)
                                               select _store).FirstOrDefault();
                            if (StockOnHand != null)
                                _AccountCode = StockOnHand.Value;
                        }

                        //get tax rate
                        string TaxType = "TAX002";
                        NuWebNCloud.Shared.Factory.Settings.TaxFactory _taxFactory = new NuWebNCloud.Shared.Factory.Settings.TaxFactory();
                        var lstTaxes = _taxFactory.GetListTaxV2(model.StoreId);
                        var tax = lstTaxes.Where(w => w.IsActive && !string.IsNullOrEmpty(w.Rate)).FirstOrDefault();
                        if (tax != null)
                            TaxType = tax.Rate;

                        foreach (var _item in model.ListItem)
                        {
                            listItem.Add(new InvoiceLineItemModels
                            {
                                Description = _item.IngredientName,
                                ItemCode = _item.IngredientCode,
                                Quantity = Convert.ToDecimal(_item.Qty),
                                UnitAmount = Convert.ToDecimal(_item.UnitPrice),
                                LineAmount = Convert.ToDecimal(_item.Qty * _item.UnitPrice),
                                AccountCode = _AccountCode,
                                TaxType = TaxType//(model.TaxType == (int)Commons.ETax.AddOn ? "OUTPUT" : "INPUT"),
                            });
                        }
                    }
                    var modelXero = new GenerateInvoiceModels
                    {
                        AppRegistrationId = model.AppRegistrationId,
                        StoreId = model.StoreId,
                        CurrencyCode = Commons._XeroCurrencyCode,
                        Reference = model.Code,
                        Contact = new InvoiceContactGRNModels
                        {
                            Name = model.SupplierName
                        },
                        DueDate = model.DueDate,
                        ClosingDatetime = model.ClosingDatetime,

                        InvoiceType = string.IsNullOrEmpty(model.InvoiceType.ToString()) ? (byte)Commons.EInvoiceType.AccountsPayable : model.InvoiceType,
                        LineAmountType = string.IsNullOrEmpty(model.LineAmountType.ToString()) ? (byte)Commons.ELineAmountType.Inclusive : model.LineAmountType,
                        InvoiceStatus = string.IsNullOrEmpty(model.InvoiceStatus.ToString()) ? (byte)Commons.EInvoiceStatus.Authorised : model.InvoiceStatus,

                        Items = listItem
                    };
                    var msgXero = string.Empty;
                    var data = new GIResponseModels();
                    var resultXero = _facXero.GenerateInvoice(model.ApiURL, modelXero, ref msgXero);
                    result.IsOk = true;
                    result.Message = msgXero;
                    //======
                    result.RawData = data;
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("Call_GenerateReceiptNoteAPI|Error: ", ex);
                result.IsOk = false;
            }
            return result;
        }

        #endregion

        #region General Setting
        /// <summary>
        /// Get List Setting
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public List<SettingXeroDTO> GetListSetting(string StoreId)
        {
            NSLog.Logger.Info("GetListXeroSetting");
            List<SettingXeroDTO> result = null;
            try
            {
                using (var cxt = new NuWebContext())
                {
                    var data = cxt.G_SettingOnStore
                        .Join(cxt.G_GeneralSetting, p => p.SettingId, c => c.Id, (p, c) => new { p, c })
                        .Where(o => o.p.StoreId.Equals(StoreId))
                        .Select(x => new SettingXeroDTO
                        {
                            ID = x.p.Id,
                            StoreID = x.p.StoreId,
                            SettingID = x.p.SettingId,
                            Code = x.c.Code,
                            Value = x.p.Value,
                            DisplayName = x.c.DisplayName,
                            Status = x.p.Status,
                            CreatedDate = x.p.CreatedDate,
                            CreatedUser = x.p.CreatedUser,
                            LastDateModified = x.p.LastDateModified,
                            LastUserModified = x.p.LastUserModified,
                        }).ToList();
                    result = data;
                    NSLog.Logger.Info("GetListXeroSetting", new { result });
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ErrorGetListXeroSetting", ex);
            }
            return result;
        }

        public bool CreateOrUpdateGeneralSetting(XeroSettingModels model, string createdUser, ref string msg)
        {
            NSLog.Logger.Info("XeroSettingCreateOrUpdate", model);
            var result = true;
            using (var cxt = new NuWebContext())
            {
                using (var trans = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.ListSettingDTO != null && model.ListSettingDTO.Any())
                        {
                            var e = cxt.G_SettingOnStore.Where(o => o.StoreId.Equals(model.StoreID)).FirstOrDefault();
                            if (e == null)
                            {
                                var _ee = new List<G_SettingOnStore>();
                                model.ListSettingDTO.ForEach(x =>
                                {
                                    var settingID = cxt.G_GeneralSetting.Where(o => o.Code.Equals(x.Code)).Select(s => s.Id).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(settingID))
                                    {
                                        var Id = Guid.NewGuid().ToString();
                                        _ee.Add(new G_SettingOnStore
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            StoreId = model.StoreID,
                                            SettingId = settingID,
                                            Value = x.Value,
                                            Status = true,
                                            CreatedUser = createdUser,
                                            CreatedDate = DateTime.Now,
                                            LastUserModified = createdUser,
                                            LastDateModified = DateTime.Now
                                        });
                                    }
                                });
                                cxt.G_SettingOnStore.AddRange(_ee);
                            }
                            else
                            {
                                model.ListSettingDTO.ForEach(x =>
                                {
                                    var settingID = cxt.G_GeneralSetting.Where(o => o.Code.Equals(x.Code)).Select(s => s.Id).FirstOrDefault();
                                    var _e = cxt.G_SettingOnStore.Where(o => o.StoreId.Equals(model.StoreID) && o.SettingId.Equals(settingID)).FirstOrDefault();
                                    if (_e != null)
                                    {
                                        //_e.StoreId = model.StoreID;
                                        //_e.SettingId = e.SettingId;
                                        _e.Value = !string.IsNullOrEmpty(x.Value) ? x.Value : "";
                                        //_e.Status = (x.Code == (byte)Commons.EGeneralSetting.CostOfGoodSold) ? x.Status : false;
                                        _e.LastDateModified = DateTime.Now;
                                        _e.LastUserModified = createdUser;
                                    }
                                });
                            }
                        }
                        cxt.SaveChanges();
                        trans.Commit();
                        NSLog.Logger.Info("ResponseUpdateXeroSetting", new { result, msg });
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        trans.Rollback();
                        msg = "Can't save this xero setting";
                        NSLog.Logger.Error("ErrorUpdateXeroSetting", ex);
                    }
                    finally
                    {
                        cxt.Dispose();
                    }
                }
            }
            return result;
        }
        #endregion
    }
}
