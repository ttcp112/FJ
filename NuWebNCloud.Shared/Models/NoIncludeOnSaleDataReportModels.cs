using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class NoIncludeOnSaleDataReportModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OrderId { get; set; }
        public string ReceiptNo { get; set; }
        public string BusinessId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string GLAccountCode { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public double Tax { get; set; }
        public double ServiceCharged { get; set; }
        public double DiscountAmount { get; set; }
        public double PromotionAmount { get; set; }
        public int Mode { get; set; }
        public int Time { get; set; }
    }
}
