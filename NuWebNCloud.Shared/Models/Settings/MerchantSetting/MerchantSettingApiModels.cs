using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Settings.MerchantSetting
{
    public class MerchantSettingApiModels
    {
        //Payment
        public StoreSettingModels StoreSetting { get; set; }
        //Wallet
        public List<CompanyMerSettingModels> ListCompany { get; set; }

        public List<string> ListOrganizations { get; set; }
        public string MerchantID { get; set; }
        public string CreatedUser { get; set; }

        public string AppKey { get; set; }
        public string AppSecret { get; set; }
    }
}
