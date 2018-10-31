using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;

    public partial class R_NoIncludeOnSaleDataReport : BaseEntity
    {
        public DateTime CreatedDate { get; set; }
        public string OrderId { get; set; }
        public string ReceiptNo { get; set; }
        public string BusinessId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public double Tax { get; set; }
        public double ServiceCharged { get; set; }
        public double DiscountAmount { get; set; }
        public double PromotionAmount { get; set; }
        public string GLAccountCode { get; set; }
        public int Mode { get; set; }
    }
}
