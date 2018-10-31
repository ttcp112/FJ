using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class SupplierIngredientModels
    {
        public string Id { get; set; }
        public string SupplierId { get; set; }
        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public bool Checked { get; set; }
    }
}
