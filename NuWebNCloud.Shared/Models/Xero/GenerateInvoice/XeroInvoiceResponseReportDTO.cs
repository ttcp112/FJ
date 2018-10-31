using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    class XeroInvoiceResponseReportDTO
    {
        public string InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public GIContactModels Contact { get; set; }
        public int Type { get; set; }

        public int Status { get; set; }
        public double LineAmountTypes { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DueDate { get; set; }
      
        public DateTime? PlannedPaymentDate { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? Total { get; set; }
        public decimal? TotalDiscount { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? CurrencyRate { get; set; }
        public DateTime? FullyPaidOnDate { get; set; }
        public decimal? AmountDue { get; set; }
        public decimal? AmountPaid { get; set; }
        public decimal? AmountCredited { get; set; }
        public bool? HasAttachments { get; set; }
        public Guid? BrandingThemeId { get; set; }
        public string Url { get; set; }
        public string Reference { get; set; }
        public List<XeroInvoiceItemReportDTO> LineItems { get; set; }

        public bool? SentToContact { get; set; }
        public List<PaymentReportXeroDTO> Payments { get; set; }
    }


}
