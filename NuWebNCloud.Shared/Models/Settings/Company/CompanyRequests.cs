using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.Company
{
   public class CreateOrUpdateCompanyRequest
    {
      
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public int Type { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public string ID { get; set; }
        public string CompanyName  { get; set; }
        public string OrganizationID { get; set; }
        public string OrganizationName { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }      
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }

    }
    public class GetCompanyWebRequest : BaseApiRequestModel
    {
        public string ID { get; set; }
        public List<string> ListOrganizationID { get; set; }
    }
}
