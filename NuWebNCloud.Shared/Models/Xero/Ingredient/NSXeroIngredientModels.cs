namespace NuWebNCloud.Shared.Models.Xero.Ingredient
{
    public class NSXeroIngredientModels
    {
        public string ItemID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public PurchaseDetailsModels PurchaseDetails { get; set; }
        public SalesDetailsModels SalesDetails { get; set; }
        public string InventoryAssetAccountCode { get; set; }
        public double QuantityOnHand { get; set; }
        public bool IsSold { get; set; }
        public bool IsPurchased { get; set; }
        public string PurchaseDescription { get; set; }
        public bool IsTrackedAsInventory { get; set; }
        public double TotalCostPool { get; set; }
    }
}
