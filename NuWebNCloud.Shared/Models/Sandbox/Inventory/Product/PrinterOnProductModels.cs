using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Product
{
    public class PrinterOnProductModels
    {
        public string ID { get; set; }
        public string PrinterID { get; set; }
        public string PrinterName { get; set; }
        /*Integration*/
        public string StoreID { get; set; }
        public bool IsActive { get; set; }
        public int Sequence { get; set; }
        public byte Type { get; set; }
    }
}
