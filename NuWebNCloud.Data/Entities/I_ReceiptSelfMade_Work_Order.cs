using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_ReceiptSelfMade_Work_Order
    {
        public string Id { get; set; }
        public string RNSelfMadeId { get; set; }
        public string WorkOrderId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
    }
}
