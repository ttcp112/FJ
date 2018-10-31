using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_ReceiptsbyPaymentMethodsReport : BaseEntity
    {
        public string PaymentId { get; set; }
        public string PaymentName { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ReceiptNo { get; set; }

        public double ReceiptRefund { get; set; }

        public double ReceiptTotal { get; set; }

        public double Total { get; set; }

        public int Mode { get; set; }
    }
}
