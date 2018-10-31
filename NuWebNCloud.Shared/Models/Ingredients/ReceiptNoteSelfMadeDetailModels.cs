namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class ReceiptNoteSelfMadeDetailModels
    {
        public string Id { get; set; }
        public string ReceiptNoteId { get; set; }

        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string IngredientCode { get; set; }
        public string BaseUOM { get; set; }

        public double ReceivingQty { get; set; }
        public bool IsActived { get; set; }
        public int? Status { get; set; }
        public double? BaseReceivingQty { get; set; }

        //Set value form Ingredient-> ReceivingQty
        public double BaseQty { get; set; }

        /*isVisible*/
        public bool IsVisible { get; set; }

        public bool IsStockAble { get; set; }
        public bool IsSelfMode { get; set; }
        public int OffSet { get; set; }

        public double Qty { get; set; }
        public double RemainingQty { get; set; }
        public double IngReceivingQty { get; set; }
        public double QtyTolerance { get; set; }
        public double QtyToleranceS { get; set; }
        public double QtyToleranceP { get; set; }
        public double ReceiptNoteQty { get; set; }
        public double ReceivedQty { get; set; }
        public string WOId { get; set; }
        public string WONumber { get; set; }
    }
}
