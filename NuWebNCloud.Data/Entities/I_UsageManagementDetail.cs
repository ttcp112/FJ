using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class I_UsageManagementDetail 
    {
        public string Id { get; set; }
        public string UsageManagementId { get; set; }
        public string IngredientId { get; set; }
        public double Usage { get; set; }
    }
}
