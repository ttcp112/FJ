using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class RefundDataReportDTO
    {
        public string Id { get; set; }
        public string BusinessDayId { get; set; }
        public string StoreId { get; set; }
        public string OrderId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public double TotalRefund { get; set; }
        public double ServiceCharged { get; set; }
        public double Tax { get; set; }
        public double Discount { get; set; }
        public double Promotion { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public bool IsGiftCard { get; set; }
    }
}
