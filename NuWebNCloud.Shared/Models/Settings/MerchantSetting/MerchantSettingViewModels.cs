using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Settings.MerchantSetting
{
    public class MerchantSettingViewModels
    {
        public List<OrganizationDTO> ListItem { get; set; }
        public MerchantSettingViewModels()
        {
            ListItem = new List<OrganizationDTO>();
        }
    }
}
