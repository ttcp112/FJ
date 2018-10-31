using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class StoreApiModels
    {
        public string ID { get; set; }
        public string CompanyID { get; set; }
        public string StoreName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string GSTRegNo { get; set; }
        public string TimeZone { get; set; }
        public bool IsActive { get; set; }
        public string IndustryName { get; set; }
        public string IndustryID { get; set; }

        //public string AppKey { get; set; }
        //public string AppSecret { get; set; }
        //public string CreatedUser { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
    }
    public class GetStoreWeb2Request
    {
        public List<string> ListStoreID { get; set; }
        public List<string> ListOrgID { get; set; }
       
    }
}
