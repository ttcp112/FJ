using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class CategoriesViewModels
    {
        public string StoreID { get; set; }

        public List<CategoriesModels> ListItem { get; set; }

        public CategoriesViewModels()
        {
            ListItem = new List<CategoriesModels>();
        }
    }
}
