using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class DiscountDetailsReportDetailModels
    {
        public Guid StoreId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DiscountId { get; set; }
        public string DiscountName { get; set; }
        public double Count { get; set; }
        public double Amount { get; set; }
        public double Discount { get; set; } //TotalDiscountAmount
        public double PercentSales { get; set; }
        public int Mode { get; set; }

        public string PromotionId { get; set; }
        public string PromotionName { get; set; }
        public double PromotionValue { get; set; }
    }
}
