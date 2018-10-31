using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.AccessControl
{
    public class UserRoleModels
    {
        public string Id { get; set; }

        public string RoleID { get; set; }
        public string RoleName { get; set; }

        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }

        public bool IsActive { get; set; }
        public byte Status { get; set; }

        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }

        public List<RoleItem> ListRole { get; set; }

        public UserRoleModels()
        {
            ListRole = new List<RoleItem>();
        }
    }

    public class RoleItem
    {
        public string Id { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Checked { get; set; }
    }
}
