using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class I_UsageManagementItemDetail
    {
        public string Id { get; set; }
        public int IndexList { get; set; }
        public string UsageManagementDetailId { get; set; }
        public string BusinessDay { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public double Qty { get; set; }
        public double Usage { get; set; }
    }
}
