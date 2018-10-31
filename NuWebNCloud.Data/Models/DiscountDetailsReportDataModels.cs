using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class DiscountDetailsReportDataModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string BusinessDayId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ReceiptId { get; set; }
        public string ReceiptNo { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public string DiscountId { get; set; }
        public string DiscountName { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double Qty { get; set; }
        public double ItemPrice { get; set; }
        public double DiscountAmount { get; set; }
        public int DiscountType { get; set; }
        public bool IsDiscountValue { get; set; }
        public double BillTotal { get; set; }
        public int Mode { get; set; }
        public string UserDiscount { get; set; }
        //2018-05-02
        public string PromotionId { get; set; }
        public string PromotionName { get; set; }
        public double PromotionValue { get; set; }
        public int PromotionType { get; set; }
    }
}
