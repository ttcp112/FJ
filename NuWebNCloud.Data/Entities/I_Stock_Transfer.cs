using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_Stock_Transfer
    {
        public string Id { get; set; }
        public string StockTransferNo { get; set; }
        public string IssueStoreId { get; set; }
        public string ReceiveStoreId { get; set; }
        public string RequestBy { get; set; }
        public DateTime RequestDate { get; set; }
        public string IssueBy { get; set; }
        public DateTime IssueDate { get; set; }
        public string ReceiveBy { get; set; }
        public DateTime ReceiveDate { get; set; }
        public bool IsActive { get; set; }
        public string BusinessId { get; set; }
        public string BusinessReceiveId { get; set; }
    }
}
