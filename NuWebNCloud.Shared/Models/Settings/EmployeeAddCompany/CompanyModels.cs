using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.EmployeeAddCompany
{
   public class CompanyModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string OrganizationID { get; set; }
        public string OrganizationName { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
    }
}
