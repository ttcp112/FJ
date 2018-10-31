using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_HourlySalesReport : BaseEntity
    {
        public double ReceiptTotal { get; set; }

        public double NetSales { get; set; }

        public string ReceiptId { get; set; }

        public DateTime CreatedDate { get; set; }

        public int NoOfPerson { get; set; }

        public int Mode { get; set; }
        public string CreditNoteNo { get; set; }
    }
}
