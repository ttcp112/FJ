using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.EmployeeAddCompany
{
   public class EmployeeAddCompanyRequest
    {
        public string EmployeeID { get; set; }
        public List<CompanyModels> ListCompany { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string CreatedUser { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
    }
}
