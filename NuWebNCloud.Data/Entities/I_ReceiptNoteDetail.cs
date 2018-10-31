namespace NuWebNCloud.Data.Entities
{
    public partial class I_ReceiptNoteDetail
    {
        public string Id { get; set; }

        public string PurchaseOrderDetailId { get; set; }
        public string IngredientId { get; set; }
        //public string IngredientCode { get; set; }
        public string UOMId { get; set; }
        public double ReceivedQty { get; set; }
        public double ReceivingQty { get; set; }
        public double RemainingQty { get; set; }
        public bool IsActived { get; set; }


        public string ReceiptNoteId { get; set; }
        public int? Status { get; set; }
        //public string IngredientId { get; set; }
        public double? BaseReceivingQty { get; set; }
        //public double Price { get; set; }
    }
}
