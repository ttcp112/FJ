using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_Purchase_Order_Detail
    {
        public string Id { get; set; }
        public string PurchaseOrderId { get; set; }
        public string IngredientId { get; set; }
        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public double? ReceiptNoteQty { get; set; }
        public double? ReturnReceiptNoteQty { get; set; }
        public int? Status { get; set; }
        public double? BaseQty { get; set; }
        public double? TaxPercen { get; set; }
        public double? TaxAmount { get; set; }
        public string TaxId { get; set; }

    }
}
