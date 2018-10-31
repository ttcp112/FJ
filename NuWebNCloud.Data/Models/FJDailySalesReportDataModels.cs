using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class FJDailySalesReportDataModels
    {
        public string StoreId { get; set; }
        public string PaymentId { get; set; }
        public string PaymentName { get; set; }
        public string BusinessDay { get; set; }
        public DateTime CreatedDate { get; set; }

        public double ReceiptTotal { get; set; }

        public double Discount { get; set; }

        public double ServiceCharge { get; set; }

        public double GST { get; set; }

        public double Rounding { get; set; }

        public double Refund { get; set; }

        public double GiftCardSales { get; set; }

        public double VoucherSales { get; set; }

        public double NetSales { get; set; }

        public int Mode { get; set; }

        public string ReceiptId { get; set; } // Updated 04102018
        public string CreditNoteNo { get; set; } // Updated 04102018
    }
}
