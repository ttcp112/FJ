using System;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class ReceiptPurchaseOrderModels
    {
        public string Id { get; set; }

        public string ReceiptNoteNo { get; set; }
        public string ReceiptNoteId { get; set; }

        public string PurchaseOrderId { get; set; }
        public string PurchaseOrderNo { get; set; }

        public double RecievingQty { get; set; }

        public string CreatedBy { get; set; }
        public string ModifierBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
    }
}
