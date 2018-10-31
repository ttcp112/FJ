using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory
{
    public class InteCategoriesApiModels
    {
        public string StoreID { get; set; }
        public string ID { get; set; }
        public string ParentName { get; set; }
        public string ParentID { get; set; }
        public string ProductType { get; set; }

        public int Sequence { get; set; }
        public bool IsShowInReservation { get; set; }
        public string Name { get; set; }
        public int TotalProducts { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public byte Status { get; set; }
        public string GLAccountCode { get; set; }
        public bool IsGiftCard { get; set; }
        public bool IsIncludeNetSale { get; set; }
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

        public List<string> ListOrgID { get; set; }

        public List<string> ListStoreID { get; set; }
        public int Type { get; set; }
        public List<CategoryOnStoreWebDTO> ListCategoryOnStore { get; set; }
        public InteCategoriesModels CategoryDetail { get; set; }

        public List<InteCategoriesModels> ListCategory { get; set; }

        //Update 03/23/18
        public List<string> ListCategoryID { get; set; }
        public List<string> ListStoreIDExtendTo { get; set; }

        public InteCategoriesApiModels()
        {
            ListCategory = new List<InteCategoriesModels>();
        }
    }

    public class CategoryOnStoreWebDTO
    {
        public int OffSet { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int Sequence { get; set; }
        public bool IsShowInReservation { get; set; }
        public bool IsShowInKiosk { get; set; }
    }
}
