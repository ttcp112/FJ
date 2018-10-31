using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class I_StoreSetting
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string IngredientId { get; set; }
        public double ReorderingQuantity { get; set; }
        public double MinAltert { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
