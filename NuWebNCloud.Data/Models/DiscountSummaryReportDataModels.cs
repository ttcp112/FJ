using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class DiscountSummaryReportDataModels
    {
        public string DiscountId { get; set; }
        public string DiscountName { get; set; }
        public string StoreId { get; set; }
        public int TC { get; set; }
        public double Amount { get; set; }
        public bool IsTotalStore { get; set; }
        public string ReceiptId { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
