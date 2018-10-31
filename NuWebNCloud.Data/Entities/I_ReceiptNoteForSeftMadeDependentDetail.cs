namespace NuWebNCloud.Data.Entities
{
    public partial class I_ReceiptNoteForSeftMadeDependentDetail
    {
        public string Id { get; set; }
        public string RNSelfMadeDetailId { get; set; }
        public string IngredientId { get; set; }
        public double StockOutQty { get; set; }
        public bool IsActived { get; set; }
    }
}
