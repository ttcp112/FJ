using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class AuditTrailReportModels 
    {
        public string BusinessDayId { get; set; }
        public string StoreId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string ReceiptID { get; set; }
        public string ReceiptNo { get; set; }
        public int ReceiptStatus { get; set; }
        public string OrderNo { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public double DiscountAmount { get; set; }
        public double PromotionAmount { get; set; }
        public double TotalRefund { get; set; }
        public double ReceiptTotal { get; set; }
        public double CancelAmount { get; set; }
        public int Mode { get; set; }
        public string CreditNoteNo { get; set; }
    }
}
