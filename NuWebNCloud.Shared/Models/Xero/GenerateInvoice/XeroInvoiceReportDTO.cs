using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    public class PaymentReportXeroDTO
    {
        public string PaymentID { get; set; }
        public int CurrencyRate { get; set; }
        public string AccountCode { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Reference { get; set; }
    }
    public class BusinessInfoXeroDTO
    {
        public string AppRegistrationId { get; set; }
        public string StoreId { get; set; }
        public string Url { get; set; }
        public string BusinessDayId { get; set; }
        public DateTime ClosingDate { get; set; }
        public string TaxRate { get; set; }
        public string Currency { get; set; }
    }
    public class XeroInvoiceReportDTO
    {
        public string AppRegistrationId { get; set; }
        public string BusinessDayId { get; set; }
        public string StoreId { get; set; }
        public string InvoiceType { get; set; }
        public int LineAmountType { get; set; }
        public string Reference { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime ClosingDatetime { get; set; }
        public List<XeroInvoiceItemReportDTO> LineItems { get; set; }
        public List<XeroInvoicePaymentReportDTO> Payments { get; set; }

        public XeroInvoiceReportDTO()
        {
            LineItems = new List<XeroInvoiceItemReportDTO>();
            Payments = new List<XeroInvoicePaymentReportDTO>();
        }
    }
    public class XeroInvoiceItemReportDTO
    {
        public string Description { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public double LineAmount { get; set; }
        public double UnitAmount { get; set; }
        public string AccountCode { get; set; }
        public string TaxType { get; set; }
        public double TaxAmount { get; set; }

    }

    public class XeroInvoicePaymentReportDTO
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Reference { get; set; }
        public List<XeroInvoiceAccountReportDTO> Account { get; set; }
        public XeroInvoicePaymentReportDTO()
        {
            Account = new List<XeroInvoiceAccountReportDTO>();
        }
    }

    public class XeroInvoiceAccountReportDTO
    {
        public string Code { get; set; }
    }
}
