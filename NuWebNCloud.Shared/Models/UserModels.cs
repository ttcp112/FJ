using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class UserModels
    {
        public string StoreId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserCreated { get; set; }

        public List<string> ListStoreID { get; set; }

        public string StoreName { get; set; }

        public List<string> ListOrganizationId { get; set; }

        public List<string> ListIndustry { get; set; }
        public string ImageURL { get; set; }
        public bool IsSuperAdmin { get; set; }
        public List<OrganizationDTO> ListOrganizations { get; set; }
        public string OrganizationName { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }

        public List<StoreModelsRespone> listStore { get; set; }
        public List<GeneralSettingDTO> ListSetting { get; set; }

        public UserModels()
        {
            ListStoreID = new List<string>();
            ListOrganizations = new List<OrganizationDTO>();
            listStore = new List<StoreModelsRespone>();
            ListSetting = new List<GeneralSettingDTO>();
        }
    }

    public class OrganizationDTO
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
    public class GeneralSettingDTO
    {
        public string StoreID { get; set; }
        public string SettingId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public int Status { get; set; }
        public int Code { get; set; }
    }

    public class MerchantExtendModel
    {
        public string HostApiURL { get; set; }
        public List<string> ListStoreIds { get; set; }

        public MerchantExtendModel()
        {
            ListStoreIds = new List<string>();
        }
    }
}
