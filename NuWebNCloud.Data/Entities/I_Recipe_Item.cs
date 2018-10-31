using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class I_Recipe_Item : BaseEntity
    {
        public string IngredientId { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; }
        public string UOMId { get; set; }
        public double Usage { get; set; }
        public byte Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public double? BaseUsage { get; set; }
    }
}
