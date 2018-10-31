using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_StockSaleUsageDetail 
    {
        public string Id { get; set; }
        public string StockSaleDetailId { get; set; }
        public string IngredientId { get; set; }
        public decimal Usage { get; set; }

        public virtual I_StockSaleDetail I_StockSaleDetail { get; set; }
        public virtual I_Ingredient I_Ingredient { get; set; }
    }
}
