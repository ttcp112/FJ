using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings.MerchantSetting;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class MerchantSettingFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public MerchantSettingFactory()
        {
            _baseFactory = new BaseFactory();
        }

        #region ====== Wallet
        public List<CompanyMerSettingModels> GetListDataWallet(string MerchantID = null, List<string> ListOrganizations = null)
        {
            List<CompanyMerSettingModels> listdata = new List<CompanyMerSettingModels>();
            try
            {
                MerchantSettingApiModels paraBody = new MerchantSettingApiModels();

                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                //paraBody.Mode = 1;

                paraBody.MerchantID = MerchantID;
                paraBody.ListOrganizations = ListOrganizations;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetMerchantSettings_WalletGet, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListCompany"];
                var lstContent = JsonConvert.SerializeObject(lstZ);
                listdata = JsonConvert.DeserializeObject<List<CompanyMerSettingModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("GetListDataWallet_GetData: " + e);
                return listdata;
            }
        }

        public bool UpdateWallet(MerchantSettingModels model, List<string> ListOrganizations, ref string msg)
        {
            try
            {
                MerchantSettingApiModels paraBody = new MerchantSettingApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ListCompany = model.ListCompany;
                //paraBody.MerchantID = model.Id;
                //paraBody.ListOrganizations = ListOrganizations;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.SaveMerchantSetting_WalletSave, null, paraBody);
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
                _logger.Error("Updated_MerchantSettingWallet: " + e);
                return false;
            }
        }
        #endregion

        #region ====== Payment
        public List<StoreSettingModels> GetListDataPayment(string MerchantID = null, List<string> ListOrganizations = null)
        {
            List<StoreSettingModels> listdata = new List<StoreSettingModels>();
            try
            {
                MerchantSettingApiModels paraBody = new MerchantSettingApiModels();

                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.MerchantID = MerchantID;
                paraBody.ListOrganizations = ListOrganizations;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetMerchantSettings_PaymentGet, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListStore"];
                var lstContent = JsonConvert.SerializeObject(lstZ);
                listdata = JsonConvert.DeserializeObject<List<StoreSettingModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("GetListDataPayment_GetData: " + e);
                return listdata;
            }

        }

        public bool UpdatePayment(MerchantSettingModels model, List<string> ListOrganizations, ref string msg)
        {
            try
            {
                MerchantSettingApiModels paraBody = new MerchantSettingApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.StoreSetting = model.StoreSetting;
                paraBody.MerchantID = model.Id;
                paraBody.ListOrganizations = ListOrganizations;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.SaveMerchantSetting_PaymentSave, null, paraBody);
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
                _logger.Error("Updated_MerchantSettingPayment: " + e);
                return false;
            }
        }
        #endregion

        //public List<SelectListItem> GetListStoreForMerchant(List<string> ListOrganizationId, List<string> ListStoreID)
        //{
        //    List<SelectListItem> ListStore = new List<SelectListItem>();
        //    try
        //    {
        //        StoreFactory _storeFactory = new StoreFactory();
        //        CompanyFactory _companyFactory = new CompanyFactory();
        //        _storeFactory = new StoreFactory();
        //        ListStore = _storeFactory.GetListStore(ListOrganizationId);
        //        //ListStore = _storeFactory.GetListStore(_companyFactory.GetListCompany(listOrganizationId));
        //        /*Editor by trongntn 06-01-2017*/
        //        ListStore = ListStore.Where(x => ListStoreID.Contains(x.Value)).OrderBy(x => x.Text).ToList();
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return ListStore;
        //}
        //public List<CompanyMerSettingModels> GetListCompanyForMerchant(List<string> ListOrganizationId)
        //{
        //    List<CompanyMerSettingModels> ListData = new List<CompanyMerSettingModels>();
        //    try
        //    {
        //        CompanyFactory _companyFactory = new CompanyFactory();
        //        List<string> listOrganization = new List<string>();
        //        listOrganization = ListOrganizationId;
        //        List<SelectListItem> ListCompany = _companyFactory.GetListCompany(listOrganization);
        //        if (ListCompany != null && ListCompany.Count > 0)
        //        {
        //            int OffSet = 0;
        //            foreach (var item in ListCompany)
        //            {
        //                ListData.Add(new CompanyMerSettingModels
        //                {
        //                    CompanyId = item.Value,
        //                    CompanyName = item.Text,
        //                    IsActive = true,
        //                    OffSet = OffSet++
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error("GetListCompanyForMerchant_MerchantSetting: " + e);
        //        return new List<CompanyMerSettingModels>();
        //    }
        //    return ListData;
        //}

    }
}
