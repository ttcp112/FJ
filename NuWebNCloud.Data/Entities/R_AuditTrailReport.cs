using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_AuditTrailReport: BaseEntity
    {
        public DateTime ReceiptDate { get; set; }
        public string BusinessDayId { get; set; }
        public string ReceiptID { get; set; }
        public string ReceiptNo { get; set; }
        public int ReceiptStatus { get; set; }
        public string OrderNo { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public double Discount { get; set; }
        public double ReceiptTotal { get; set; }
        public double CancelAmount { get; set; }
        public int Mode { get; set; }
        public double? PromotionAmount { get; set; }
        public string CreditNoteNo { get; set; }
    }
}
