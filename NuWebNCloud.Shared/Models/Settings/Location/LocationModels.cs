using System.Collections.Generic;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Settings.Location
{
    public class LocationModels
    {
        public string ID { get; set; }
        public string StoreName { get; set; }
        public string Region { get; set; }
        public string RegionName { get; set; }
        public string StoreID { get; set; }
        public bool IsPrintRoundingAmount { get; set; }
        public bool IsPrintTaxCode { get; set; }
        public bool IsPrintSummaryTax { get; set; }
        public bool IsPrintCustomerClaimTax { get; set; }
        public List<SettingDTO> ListSettings { get; set; }

        public List<SelectListItem> ListCountry { get; set; }
        public LocationModels()
        {
            ListSettings = new List<SettingDTO>();
            ListCountry = new List<SelectListItem>();
            //========
            IsPrintRoundingAmount = true;
            IsPrintTaxCode = true;
            IsPrintSummaryTax = true;
            IsPrintCustomerClaimTax = true;
        }
    }
}
