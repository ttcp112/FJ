using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_DailyReceiptReport : BaseEntity
    {
        public DateTime CreatedDate { get; set; }
        public string BusinessDayId { get; set; }

        public string ReceiptId { get; set; }

        public string ReceiptNo { get; set; }

        public int NoOfPerson { get; set; }

        public double ReceiptTotal { get; set; }

        public double Discount { get; set; }

        public double ServiceCharge { get; set; }

        public double GST { get; set; }

        public double Tips { get; set; }

        public double Rounding { get; set; }

        public double PromotionValue { get; set; }

        public double NetSales { get; set; }

        public int Mode { get; set; }
        public string CreditNoteNo { get; set; }
    }
}
