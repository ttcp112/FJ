namespace NuWebNCloud.Data.Entities
{
    public partial class I_ReceiptNoteForSeftMadeDetail
    {
        public string Id { get; set; }
        public string ReceiptNoteId { get; set; }
        public string WOId { get; set; }
        public string IngredientId { get; set; }
        public double ReceivingQty { get; set; }
        public bool IsActived { get; set; }
        public int? Status { get; set; }
        public double? BaseReceivingQty { get; set; }
    }
}
