using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox
{
    /*Object API*/
    public class SBEmployeeApiModels
    {
        public string StoreID { get; set; }
        public EmployeeModels EmployeeDTO { get; set; }

        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

        public string ID { get; set; }
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string ImageData { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string Pincode { get; set; }
        public string Phone { get; set; }
        public bool Gender { get; set; }
        public bool Marital { get; set; }
        public DateTime HiredDate { get; set; }
        public DateTime BirthDate { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public List<EmployeeOnStoreModels> ListEmpStore { get; set; }
        public List<WorkingTimeModels> ListWorkingTime { get; set; }

        public List<EmployeeModels> ListEmployee { get; set; }

        public List<string> ListOrgID { get; set; }
        //=============
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }
    }
}
