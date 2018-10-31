using System;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class Ingredients_SupplierModel
    {
        public string Id { get; set; }
        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string IngredientCode { get; set; }

        public string CompanyId { get; set; }

        public string SupplierId { get; set; }
        public string SupplierName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
        /*===*/

        public string SupplierAddress { get; set; }
        public string SupplierPhone { get; set; }
        public bool IsCheck { get; set; }
    }
}
