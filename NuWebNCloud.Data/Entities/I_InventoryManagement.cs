using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class I_InventoryManagement:BaseEntity
    {
        public string IngredientId { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }

        //Update field for Ingredient Ver2
        public double? POQty { get; set; }

    }
}
