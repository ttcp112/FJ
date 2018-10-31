namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class ReturnNoteDetailModels
    {
        public string Id { get; set; }
        public string ReturnNoteId { get; set; }
        public string ReceiptNoteDetailId { get; set; }
        public double ReceivedQty { get; set; }
        public double ReturnQty { get; set; }
        public bool IsActived { get; set; }
    }
}
