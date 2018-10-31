using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class DiscountViewModels
    {
        public string StoreID { get; set; }

        public List<DiscountModels> ListItem { get; set; }

        public DiscountViewModels()
        {
            ListItem = new List<DiscountModels>();
        }
    }
}
