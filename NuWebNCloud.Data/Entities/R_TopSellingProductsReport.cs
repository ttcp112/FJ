using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_TopSellingProductsReport : BaseEntity
    {
        public string BusinessDayId { get; set; }
        public string OrderId { get; set; }
        public string OrderDetailId { get; set; }

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
    }
}
