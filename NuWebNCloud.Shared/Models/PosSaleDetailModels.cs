using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class PosSaleDetailModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public string OrderId { get; set; }
        public string OrderDetailId { get; set; }
        public string ItemId { get; set; }
        public int ItemTypeId { get; set; }
        public string ParentId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string GLAccountCode { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double ExtraPrice { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double Cost { get; set; }
        public double ServiceCharge { get; set; }
        public double Tax { get; set; }
        public double PromotionAmount { get; set; }
        public string PoinsOrderId { get; set; }
        public string GiftCardId { get; set; }
        public bool IsIncludeSale { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
        public bool? IsDiscountTotal { get; set; }
        public string CancelUser { get; set; }
        public string RefundUser { get; set; }
        public int? TaxType { get; set; }
    }
}
