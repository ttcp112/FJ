using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Xero.Ingredient
{
    public class NSXeroIngredientRequestModels : XeroBaseModel
    {
        public List<NSXeroIngredientModels> Ingredients { get; set; }
    }
}
