using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_StockSaleDetail 
    {
        public string Id { get; set; }
        public string StockSaleId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Qty { get; set; }
        public bool IsCheckStock { get; set; }

        public virtual I_StockSale I_StockSale { get; set; }
    }
}
