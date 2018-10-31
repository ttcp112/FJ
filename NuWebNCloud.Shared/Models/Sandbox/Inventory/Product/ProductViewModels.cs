using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Product
{
    public class ProductViewModels
    {
        public string StoreID { get; set; }

        public List<ProductModels> ListItem { get; set; }

        public ProductViewModels()
        {
            ListItem = new List<ProductModels>();
        }
    }
}
