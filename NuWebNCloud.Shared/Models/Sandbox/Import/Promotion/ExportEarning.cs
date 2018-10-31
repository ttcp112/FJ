using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import.Promotion
{
    public class ExportEarning
    {
        public int EarnIndex { get; set; }
        public int PromotionIndex { get; set; }
        public string Condition { get; set; }
        public string EarnType { get; set; }
        public double PercentValue { get; set; }
        public string Item { get; set; }
        public double Qty { get; set; }
    }

    public class ExportEarningProduct
    {
        public int EarnProIndex { get; set; }
        public int EarnIndex { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
