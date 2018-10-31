using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    /// <summary>
    /// GENERATE INVOICE RESPONSE
    /// </summary>
    public class GIResponseModels
    {
        public string InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public GIContactModels Contact { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public int LineAmountTypes { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public double SubTotal { get; set; }
        public double TotalTax { get; set; }
        public double Total { get; set; }
        public string CurrencyCode { get; set; }
        public double AmountDue { get; set; }
        public double AmountPaid { get; set; }
        public string Reference { get; set; }
        public List<GILineItemModels> LineItems { get; set; }
        public DateTime UpdatedDateUTC { get; set; }

        public GIResponseModels()
        {
            Contact = new GIContactModels();
            LineItems = new List<GILineItemModels>();
        }
    }
}
