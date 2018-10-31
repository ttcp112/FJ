namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class PurchaseOrderDetailModels
    {
        public string Id { get; set; }
        public string PurchaseOrderId { get; set; }
        public string IngredientId { get; set; }

        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }
        public string BaseUOM { get; set; }

        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public double? ReceiptNoteQty { get; set; }
        public double? ReturnReceiptNoteQty { get; set; }
    }


    public class ExportPurchaseOrderDetail
    {
        public int Index { get; set; }
        public string PONumber { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public string UOM { get; set; }
        public double UnitPrice { get; set; }
        public double ItemTotal { get; set; }
    }
}
