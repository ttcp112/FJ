using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    public class GenerateInvoiceModels : XeroBaseModel
    {
        /// <summary>
        /// Get value from EInvoiceType = ACCPAY[AccountsPayable]
        /// </summary>
        public byte InvoiceType { get; set; }

        /// <summary>
        /// Get value from EInvoiceStatus = follow the setting of “Send bill as”
        /// </summary>
        public byte InvoiceStatus { get; set; }
        /// <summary>
        /// Get value from ELineAmountType
        /// </summary>
        public byte LineAmountType { get; set; }

        /// <summary>
        /// Contact Name = supplier name. If Xero doesn’t have that supplier, please auto add 1 record of that supplier at Xero - Contacts - Suppliers.
        /// </summary>
        public InvoiceContactGRNModels Contact { get; set; }

        /// <summary>
        /// Due date = if in xero - Settings - Invoice Settings - Default Settings - Bills Default Due Date has value, 
        /// Due date = current date + the setting, if not, current date
        /// </summary>
        public DateTime DueDate { get; set; }
        /// <summary>
        /// ClosingDatetime = PO Time = Current Date
        /// </summary>
        public DateTime ClosingDatetime { get; set; }

        /// <summary>
        /// //Bill Number = Reference = auto generated with the format BILL-yyyymmdd-xxxx
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// The currency that purchase order has been raised in
        /// </summary>
        public string CurrencyCode { get; set; }

        public string MerchantID { get; set; }
        public string CompanyID { get; set; }

        public List<InvoiceLineItemModels> Items { get; set; }

        public GenerateInvoiceModels()
        {
            Items = new List<InvoiceLineItemModels>();
        }
    }
}

public class InvoiceContactGRNModels
{
    public string Name { get; set; }
}

public class InvoiceLineItemModels
{
    public string Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitAmount { get; set; }
    public string AccountCode { get; set; }
    public string ItemCode { get; set; }
    public string TaxType { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineAmount { get; set; }
    public decimal? DiscountRate { get; set; }
}
