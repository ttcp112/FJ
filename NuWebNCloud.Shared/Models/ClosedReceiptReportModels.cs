using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ClosedReceiptReportModels
    {
        public string StoreId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }

        public int NoOfPersion { get; set; }

        public double Total { get; set; }
        public string OrderId { get; set; }
        public string ReceiptNo { get; set; }
        public string TableNo { get; set; }
        public int Mode { get; set; }
        public string OrderedById { get; set; }
        public string OrderedByName { get; set; }
        public string CreditNoteNo { get; set; }
        // Updated 05042018, for report Top Selling NewDB
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
