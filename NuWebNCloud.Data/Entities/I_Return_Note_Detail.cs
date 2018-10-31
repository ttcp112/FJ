namespace NuWebNCloud.Data.Entities
{
    public class I_Return_Note_Detail
    {
        public string Id { get; set; }
        public string ReturnNoteId { get; set; }
        public string ReceiptNoteDetailId { get; set; }
        public double ReceivedQty { get; set; }
        public double ReturnQty { get; set; }
        public bool IsActived { get; set; }
        public double? ReturnBaseQty { get; set; }
    }
}
