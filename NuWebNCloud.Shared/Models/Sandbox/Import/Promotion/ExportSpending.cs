using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import.Promotion
{
    public class ExportSpending
    {
        public int SpendIndex { get; set; }
        public int PromotionIndex { get; set; }
        public string Condition { get; set; }
        public string SpendType { get; set; }
        public double QtyAmount { get; set; }
        public string Item { get; set; }
    }

    public class ExportSpendingProduct
    {
        public int SpendProIndex { get; set; }
        public int SpendIndex { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
