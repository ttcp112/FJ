using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class DiscountApiModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Value { get; set; }
        public byte Type { get; set; }
        public bool IsAllowOpenValue { get; set; }
        public bool IsActive { get; set; }
        public byte Status { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsApplyTotalBill { get; set; }
        //=====
        public List<string> ListStoreID { get; set; }
        public List<DiscountModels> ListDiscount { get; set; }
        //=====
        public byte Mode { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
