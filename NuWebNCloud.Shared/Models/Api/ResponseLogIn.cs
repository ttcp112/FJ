using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class ResponseLogIn
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }

        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string StoreNumber { get; set; }

        public string RoleID { get; set; }
        public string RoleName { get; set; }

        public string CompanyID { get; set; }
        public string CompanyName { get; set; }

        public string IndustryID { get; set; }
        public string IndustryName { get; set; }

        public string OrganizationID { get; set; }
        public string OrganizationName { get; set; }

        public string EmployeeID { get; set; }
        public string Name { get; set; }
        public string PinCode { get; set; }
        public string Email { get; set; }
        public string ImageURL { get; set; }

    }
}
