
using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    public class GeneratePOModels : XeroBaseModel
    {
        public string Reference { get; set; }
        public List<InvoiceLineItemModels> Items { get; set; }
        public InvoiceContactPOModels Contact { get; set; }
        public DateTime ClosingDatetime { get; set; }
        public byte PurchaseOrderStatus { get; set; }
        public string CurrencyCode { get; set; }
        public byte LineAmountTypes { get; set; }
        /// <summary>
        /// Telephone [get value from Supplier Phone]
        /// </summary>
        public string Telephone { get; set; }
        /// <summary>
        /// AttentionTo [get value from Store Name]
        /// </summary>
        public string AttentionTo { get; set; }
        /// <summary>
        /// DeliveryInstructions [get value from Note]
        /// </summary>
        public string DeliveryInstructions { get; set; }
        public string MerchantID { get; set; }
        public string CompanyID { get; set; }

        public GeneratePOModels()
        {
            Items = new List<InvoiceLineItemModels>();
        }
    }

    public class InvoiceContactPOModels
    {
        public string ContactID { get; set; }
    }
}
