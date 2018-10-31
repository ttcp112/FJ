using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_Receipt_Purchase_Order
    {
        public string Id { get; set; }
        public string ReceiptNoteId { get; set; }
        public string PurchaseOrderId { get; set; }
        //public string PurchaseOrderNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
    }
}
