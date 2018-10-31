using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_PosSale : BaseEntity
    {
        public string BusinessId { get; set; }
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime? ReceiptCreatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrderStatus { get; set; }
        public string TableNo { get; set; }
        public int NoOfPerson { get; set; }
        public double CancelAmount { get; set; }
        public double ReceiptTotal { get; set; }
        public double Discount { get; set; }
        public double Tip { get; set; }
        public double PromotionValue { get; set; }
        public double ServiceCharge { get; set; }
        public double GST { get; set; }
        public double Rounding { get; set; }
        public double Refund { get; set; }
        //public double NetSales { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public int Mode { get; set; }
        public string CreditNoteNo { get; set; }
    }
}
