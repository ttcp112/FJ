using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_DataEntryDetail 
    {
        public string Id { get; set; }
        public string DataEntryId { get; set; }
        public string IngredientId { get; set; }
        public double? Damage { get; set; }
        public double? Wastage { get; set; }
        public double? OrderQty { get; set; }
        public string Reasons { get; set; }

    }
}
