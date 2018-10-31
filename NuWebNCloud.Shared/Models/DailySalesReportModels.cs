using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class DailySalesReportModels
    {
        public string StoreId { get; set; }
        public string PaymentId { get; set; }
        public string PaymentName { get; set; }
        public string BusinessDay { get; set; }
        public DateTime CreatedDate { get; set; }

        public int NoOfReceipt { get; set; }

        public int NoOfPerson { get; set; }

        public double ReceiptTotal { get; set; }

        public double Discount { get; set; }

        public double ServiceCharge { get; set; }

        public double GST { get; set; }

        public double Rounding { get; set; }

        public double Refund { get; set; }

        public double GiftCardSales { get; set; }

        public double VoucherSales { get; set; }

        public double NetSales { get; set; }

        public int NoSale { get; set; }
        public int Mode { get; set; }
    }

    public class DailySalesReportInsertModels
    {
        public string StoreId { get; set; }
        public string OrderId { get; set; }

        public DateTime CreatedDate { get; set; }

        public int NoOfPerson { get; set; }

        public double ReceiptTotal { get; set; }

        public double Discount { get; set; }

        public double ServiceCharge { get; set; }

        public double GST { get; set; }

        public double Rounding { get; set; }

        public double Refund { get; set; }

        public double NetSales { get; set; }
        public double Tip { get; set; }
        public double PromotionValue { get; set; }

        public int Mode { get; set; }
        public string BusinessId { get; set; }
        public string CreditNoteNo { get; set; }
    }

    public class ShiftLogModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime ClosedOn { get; set; }
        public string StartedStaff { get; set; }
        public string ClosedStaff { get; set; }
    }
}
