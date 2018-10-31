using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_ItemizedSalesAnalysisReport : BaseEntity
    {
        public string ItemId { get; set; }
        public string ItemCode { get; set; }

        public int ItemTypeId { get; set; }

        public string ItemName { get; set; }

        public double Price { get; set; }

        public double ExtraPrice { get; set; }

        public double TotalPrice { get; set; }

        public double Quantity { get; set; }

        public double Discount { get; set; }

        public double Cost { get; set; }


        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string GLAccountCode { get; set; }

        public double ServiceCharge { get; set; }

        public double Tax { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Mode { get; set; }
        public double PromotionAmount { get; set; }
        public string BusinessId { get; set; }
        public bool? IsIncludeSale { get; set; }
        public double? TotalDiscount { get; set; }
        public double? TotalAmount { get; set; }
        public double? ExtraAmount { get; set; }
        public string ReceiptId { get; set; }
        public string PoinsOrderId { get; set; }
        public string GiftCardId { get; set; }
        public int? TaxType { get; set; }
    }
}
