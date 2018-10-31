using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_ItemizedCancelOrRefundData : BaseEntity
    {
        public string BusinessId { get; set; }
        public string OrderId { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public int ItemTypeId { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CancelUser { get; set; }
        public string RefundUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsRefund { get; set; }
        public int Mode { get; set; }
       
    }
}
