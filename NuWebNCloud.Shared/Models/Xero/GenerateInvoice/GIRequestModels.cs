using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    /// <summary>
    /// GENERATE INVOICE REQUEST
    /// </summary>
    public class GIRequestModels : XeroBaseModel
    {
        public byte InvoiceType { get; set; }       // Get value from EInvoiceType
        public byte InvoiceStatus { get; set; }     // Get value from EInvoiceStatus
        public byte LineAmountType { get; set; }    // Get value from ELineAmountType
        public InvoiceContactGRNModels Contact { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime ClosingDatetime { get; set; }
        public string Reference { get; set; }
        public string CurrencyCode { get; set; }
        public string MerchantID { get; set; }
        public string CompanyID { get; set; }
        public List<InvoiceLineItemModels> Items { get; set; }
    }
}
