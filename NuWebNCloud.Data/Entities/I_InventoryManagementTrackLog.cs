using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_InventoryManagementTrackLog : BaseEntity
    {
        public string IngredientId { get; set; }
        public int TypeCode { get; set; }
        public string TypeCodeId { get; set; }
        public double CurrentQty { get; set; }
        public double NewQty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
