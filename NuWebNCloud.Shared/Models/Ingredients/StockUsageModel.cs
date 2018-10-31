using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class StockUsageModel
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
    }
    public class StockUsageRequestModel
    {
        public string CompanyId { get; set; }
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<StockUsageModel> ListDetails { get; set; }
    }
}
