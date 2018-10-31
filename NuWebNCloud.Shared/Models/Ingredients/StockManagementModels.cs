using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class StockManagementModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public string IngredientId { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }

        public double Quantity { get; set; }
        public double Price { get; set; }

        public string BaseUOM { get; set; }
        public string ReceivingUOM { get; set; }
        public double POQty { get; set; }
        public double Rate { get; set; }
        public int Status { get; set; }
        public string Type { get; set; }
    }

    public class StockManagementViewModels
    {
        public List<string> ListStore { get; set; }
        public List<StockManagementModels> ListItem { get; set; }
        public StockManagementViewModels()
        {
            ListItem = new List<StockManagementModels>();
        }
    }
}
