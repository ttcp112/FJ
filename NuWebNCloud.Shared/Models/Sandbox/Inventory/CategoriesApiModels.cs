using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class CategoriesApiModels
    {
        public string StoreID { get; set; }
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string ProductTypeID { get; set; }
        public int ProductType { get; set; }
        public int Type { get; set; }

        public int Sequence { get; set; }
        public bool IsShowInReservation { get; set; }
        public string Name { get; set; }
        public int TotalProducts { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public byte Status { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        //=====
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }
        //====Get Parent
        public bool isGetChild { get; set; }
        public string GLAccountCode { get; set; }
        public bool IsGiftCard { get; set; }
        public bool IsIncludeNetSale { get; set; }
        public bool IsShowInKiosk { get; set; }
        public List<string> ListOrgID { get; set; }
    }
}
