using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Settings.MerchantSetting
{
    public class MerchantSettingModels
    {
        //Payment
        public StoreSettingModels StoreSetting { get; set; }
        //Wallet
        public List<CompanyMerSettingModels> ListCompany { get; set; }


        public List<CompanyMerSettingModels> ListCompnayShow { get; set; }
        public string CreatedUser { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }

        public string Id { get; set; }

        public MerchantSettingModels()
        {
            ListCompany = new List<CompanyMerSettingModels>();
            ListCompnayShow = new List<CompanyMerSettingModels>();
        }
    }
}
