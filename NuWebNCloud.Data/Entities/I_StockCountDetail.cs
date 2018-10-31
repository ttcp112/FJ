using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_StockCountDetail
    {
        public string Id { get; set; }
        public string StockCountId { get; set; }
        public string IngredientId { get; set; }
        public double CloseBal { get; set; }

        public double? OpenBal { get; set; }
        public double? Damage { get; set; }
        public double? Wastage { get; set; }
        public double? OtherQty { get; set; }
        public string Reasons { get; set; }
        public double? AutoCloseBal { get; set; }
    }
}
