using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory
{
    public class InteCategoriesViewModels
    {
        public string StoreID { get; set; }
        public List<string> ListStoreID { get; set; }

        public List<InteCategoriesModels> ListItem { get; set; }
        public string StoreExtendFrom { get; set; }
        public List<string> StoreExtendTo { get; set; }

        public SelectList ListStoreTo { get; set; } // Updated 04192018, for list stores group by company 

        public InteCategoriesViewModels()
        {
            ListItem = new List<InteCategoriesModels>();
        }
    }
}
