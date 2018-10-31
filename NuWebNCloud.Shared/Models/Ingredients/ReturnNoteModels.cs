using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class ReturnNoteModels
    {
        public string Id { get; set; }
        public string ReturnNoteNo { get; set; }
        public string ReceiptNoteId { get; set; }

        public string CreatedBy { get; set; }
        public string ModifierBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }

        public double TotalQty { get; set; }

        public List<PurchaseOrderModels> ListPurchaseOrder { get; set; }
    }

    public class ReturnNoteReceiptView
    {
        public string ReceiptId { get; set; }
        public List<string> ListReturnNoteId { get; set; }

        public ReturnNoteReceiptView()
        {
            ListReturnNoteId = new List<string>();
        }
    }
}
