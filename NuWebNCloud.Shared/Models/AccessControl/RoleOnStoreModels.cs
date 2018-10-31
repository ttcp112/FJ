using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.AccessControl
{
    public class RoleOnStoreModels
    {
        public string Id { get; set; }

        public string RoleId { get; set; }
        public string RoleName { get; set; }

        public string OrganizationName { get; set; }

        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public bool IsActive { get; set; }
        public byte Status { get; set; }

        public List<StoreItem> ListStore { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }

        public RoleOnStoreModels()
        {
            ListStore = new List<StoreItem>();
        }
    }

    public class StoreItem
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public bool Checked { get; set; }
    }

    public class CompanyItem
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool Checked { get; set; }
        public List<StoreItem> ListStore { get; set; }

        public CompanyItem()
        {
            ListStore = new List<StoreItem>();
        }
    }
}
