using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ReceiptsbyPaymentMethodsReportModels
    {
        public string StoreId { get; set; }
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string PaymentName { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ReceiptNo { get; set; }
        public double Tip { get; set; }

        public double ReceiptRefund { get; set; }

        public double ReceiptTotal { get; set; }

        public double PaymentAmount { get; set; }
        public string CreditNoteNo { get; set; } // Updated 04102018
        public bool IsGiftCard { get; set; } // Updated 05052018, for refund value
    }
}
