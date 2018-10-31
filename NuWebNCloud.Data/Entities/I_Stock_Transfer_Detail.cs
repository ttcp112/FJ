using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_Stock_Transfer_Detail
    {
        public string Id { get; set; }
        public string StockTransferId { get; set; }
        public string IngredientId { get; set; }
        public double RequestQty { get; set; }
        public double IssueQty { get; set; }
        public double ReceiveQty { get; set; }
        public string UOMId { get; set; }

        public double? IssueBaseQty { get; set; }
        public double? ReceiveBaseQty { get; set; }
    }
}
