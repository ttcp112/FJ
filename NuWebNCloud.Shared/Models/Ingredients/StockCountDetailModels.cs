using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class StockCountDetailModels
    {
        public string Id { get; set; }
        public string BusinessId { get; set; }
        public string StoreId { get; set; }
        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string IngredientCode { get; set; }
        public string BaseUOM { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public double CloseBal { get; set; }
        public double Damage { get; set; }
        public double Wastage { get; set; }
        public double OtherQty { get; set; }
        public string Reasons { get; set; }
        public double OpenBal { get; set; }
        public double Sale { get; set; }
        public bool IsSeftMade { get; set; }
        public bool IsStockAble { get; set; }
    }
}
