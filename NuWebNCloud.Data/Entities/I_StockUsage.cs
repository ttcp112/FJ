using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class I_StockUsage: BaseEntity
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
    }
}
