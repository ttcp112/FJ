using NuWebNCloud.Shared.Models.Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class CompanyApiModels
    {
        public string ID { get; set; }
        public List<string> ListOrganizationID { get; set; }

        public string CompanyName { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string CreatedUser { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public Byte Status { get; set; }
        public int Mode { get; set; }

        public List<CustomerModels> ListCustomer { get; set; }
    }
}
