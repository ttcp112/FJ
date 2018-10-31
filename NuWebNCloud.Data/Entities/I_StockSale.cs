using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_StockSale : BaseEntity
    {
        public string OrderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
        public int Type { get; set; }
        public string BusinessId { get; set; }
    }
}
