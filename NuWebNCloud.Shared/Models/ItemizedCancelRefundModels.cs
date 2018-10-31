using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ItemizedCancelRefundModels
    {
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public string OrderId { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public int ItemTypeId { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CancelUser { get; set; }
        public string RefundUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
        public bool? IsRefund { get; set; }
    }
}
