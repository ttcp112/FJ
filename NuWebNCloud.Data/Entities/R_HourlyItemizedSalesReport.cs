using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_HourlyItemizedSalesReport : BaseEntity
    {
        public int ItemTypeId { get; set; }

        public string CategoryId { get; set; }
        public string CategoryName { get; set; }

        public double TotalPrice { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Mode { get; set; }
        public double Discount { get; set; }
        public double Promotion { get; set; }
        public string BusinessId { get; set; }
        //update 08/08/2017
        public string ItemId { get; set; }
        public bool? IsDiscountTotal { get; set; }
    }
}
