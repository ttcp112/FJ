using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_AllocationDetail
    {
        public string Id { get; set; }
        public string AllocationId { get; set; }
        public string IngredientId { get; set; }
        public double OpenBal { get; set; }
        public double CloseBal { get; set; }
        public double Sales { get; set; }
        public double ActualSold { get; set; }
        public double Damage { get; set; }
        public double Wast { get; set; }
        public double Others { get; set; }
        public string Reasons { get; set; }

    }
}
