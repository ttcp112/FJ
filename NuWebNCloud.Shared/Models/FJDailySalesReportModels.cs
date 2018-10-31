using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class FJDailySalesReportModels
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

    public class FJDailySalesReportViewModels
    {
        public string PaymentId { get; set; }
        public string PaymentName { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
        public string StoreId { get; set; } // Updated 04112018
    }
    public class FJDailySaleReportSettingModels
    {
        public string StoreId { get; set; }
        public int Seq { get; set; }
        public string GLAccountCode { get; set; }
        public string[] GLAccountCodes { get; set; }
        
    }
}
