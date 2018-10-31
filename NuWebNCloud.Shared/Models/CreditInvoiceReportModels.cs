using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class CreditInvoiceReportModels
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public double TotalAmout { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Reason { get; set; }
        public double Amount { get; set; }
        public string InvoiceNo { get; set; }
        public string Remark { get; set; }
        public List<CreditInvoiceItemReportModels> Item { get; set; }

        public CreditInvoiceReportModels()
        {
            Item = new List<CreditInvoiceItemReportModels>();
        }
    }

    public class CreditInvoiceItemReportModels
    {
        public string InvoiceNo { get; set; }
        public double Amount { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
