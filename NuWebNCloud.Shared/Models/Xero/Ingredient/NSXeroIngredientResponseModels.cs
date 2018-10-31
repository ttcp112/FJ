using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Xero.Ingredient
{
    public class NSXeroIngredientResponseModels
    {
        public List<NSXeroIngredientModels> InsertedItems { get; set; }
        public List<NSXeroIngredientModels> UpdatedItems { get; set; }
    }

    public class IngError
    {
        public string Message { get; set; }
    }
}
