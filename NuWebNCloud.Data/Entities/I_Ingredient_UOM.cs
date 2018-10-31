using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public partial class I_Ingredient_UOM
    {
        public string Id { get; set; }
        public string IngredientId { get; set; }
        public string UOMId { get; set; }
        public double ReceivingQty { get; set; }
        public bool IsActived { get; set; }

        public double BaseUOM { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

    }
}
