using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_ClosedReceiptReport : BaseEntity
    {
        public DateTime CreatedDate { get; set; }

        public string CashierId { get; set; }
        public string CashierName { get; set; }

        public int NoOfPersion { get; set; }

        public double Total { get; set; }
        public string OrderId { get; set; }
        public string ReceiptNo { get; set; }
        public string TableNo { get; set; }

        public int Mode { get; set; }
        public string CreditNoteNo { get; set; }
    }
}
