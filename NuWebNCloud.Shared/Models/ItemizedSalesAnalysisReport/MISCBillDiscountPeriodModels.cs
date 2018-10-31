using System;

namespace NuWebNCloud.Shared.Models.ItemizedSalesAnalysisReport
{
    public class MISCBillDiscountPeriodModels
    {
        public string Period { get; set; } // Breakfast, Lunch, Dinner
        public string Type { get; set; } // Type is MISC or Discount

        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public TimeSpan? Time { get; set; }
        public double MiscTotal { get; set; }
        public double BillDiscountTotal { get; set; }
        public double Percent { get; set; }
        public double SubDiscount { get; set; }
        public double SubPromotion { get; set; }
    }
}
