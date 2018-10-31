using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models
{
    public class UserSession
    {
        public bool IsAuthenticated { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int UserType { get; set; }

        public string IndustryId { get; set; }

        public List<string> ListOrganizationId { get; set; }
        public List<string> ListIndustry { get; set; }
        public string ImageUrl { get; set; }
        public bool IsSuperAdmin { get; set; }
        public string OrganizationName { get; set; }

        public string StoreId { get; set; }
        public List<string> ListStoreID { get; set; }
        public string BreakfastStart { get; set; }
        public string BreakfastEnd { get; set; }
        public string LunchStart { get; set; }
        public string LunchEnd { get; set; }
        public string DinnerStart { get; set; }
        public string DinnerEnd { get; set; }
        public string FTPHost { get; set; }
        public string FTPUser { get; set; }
        public string FTPPassword { get; set; }
        public string PublicImages { get; set; }
        public string HostApi { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }

        public string[] _daysOfWeek { get; set; }
        public string[] _monthNames { get; set; }

        /// <summary>
        /// Properties Language for Model
        /// </summary>
        public string LanguageId { get; set; }
        public Dictionary<string, string> ListLanguageText { get; set; }

        public bool RememberMe { get; set; }
        public int? POSInstanceVersion { get; set; }

        public string CurrencySymbol { get; set; }

        public List<StoreModelsRespone> listStore { get; set; }
        public List<GeneralSettingDTO> ListSetting { get; set; }
        public string EmployeeConfigId { get; set; }
        public List<MerchantExtendModel> ListMerchantExtends { get; set; }
        public bool IsMerchantExtend { get; set; }
        public string UrlWebHost { get; set; }
        public string CountryCode { get; set; }
        public string  DefaultStoreId { get; set; }
        //Updated 04022018, for set css menu toggle
        public bool IsFromNuPos { get; set; }

        public List<OrganizationDTO> ListOrganizations { get; set; }
        public string WebHostUrl { get; set; }

        public UserSession()
        {
            ListStoreID = new List<string>();
            ListLanguageText = new Dictionary<string, string>();
            listStore = new List<StoreModelsRespone>();
            ListSetting = new List<GeneralSettingDTO>();
            ListMerchantExtends = new List<MerchantExtendModel>();
            ListOrganizations = new List<OrganizationDTO>();
            //=================
            _daysOfWeek = new string[7] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            _monthNames = new string[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        }

        /// <summary>
        /// Set _KEY -> Text for Language when user choose list from page Login
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public string GetLanguageTextFromKey(string _key)
        {
            if (_key == null)
                return "";
            string value = _key;
            if (ListLanguageText != null && ListLanguageText.Count > 0)
            {
                value = !ListLanguageText.ContainsKey(_key) ? _key : ListLanguageText[_key];
            }
            return value;
        }

        /// <summary>
        /// Get Language For DateTimePicker
        /// </summary>
        public void GetLanguageForDateTimePicker()
        {
            for (int i = 0; i < _daysOfWeek.Length; i++)
                Commons._daysOfWeek[i] = GetLanguageTextFromKey(_daysOfWeek[i]);
            for (int i = 0; i < _monthNames.Length; i++)
                Commons._monthNames[i] = GetLanguageTextFromKey(_monthNames[i]);
        }
    }
}
