using NuWebNCloud.Shared.Models.Sandbox.Inventory.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion
{
    public class PromotionCategoryDTO
    {
        public string CategoryID { get; set; }
        public string Name { get; set; }
        public int TotalItem { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public List<ProductModels> ListProduct { get; set; }
    }
}
