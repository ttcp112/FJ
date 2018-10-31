using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class TopSellingProductsReportModels
    {
        public string BusinessDayId { get; set; }
        public string OrderId { get; set; }
        public string OrderDetailId { get; set; }
        public string StoreId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double Qty { get; set; }
        public double Discount { get; set; }
        public double Amount { get; set; }
        public double PromotionAmount { get; set; }
        public double Tax { get; set; }
        public double ServiceCharged { get; set; }
        public int Mode { get; set; }
        // Updated 05042018, for report Top Selling NewDB
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
