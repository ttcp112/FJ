using System.Collections.Generic;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory.Product
{
    public class InteProductViewModels
    {
        public string StoreID { get; set; }
        public List<string> ListStoreID { get; set; }

        public List<InteProductLiteModels> ListItem { get; set; }
        public bool IsCheckAll { get; set; }

        public SelectList ListStoreTo { get; set; } // Updated 04192018, for list stores group by company 
        public InteProductViewModels()
        {
            ListItem = new List<InteProductLiteModels>();
        }
    }
}
