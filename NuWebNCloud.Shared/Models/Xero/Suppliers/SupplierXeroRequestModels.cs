using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero.Suppliers
{
    public class SupplierXeroRequestModels : XeroBaseModel
    {
        public bool IsSupplier { get; set; }
        public string ContactName { get; set; }
        public string ContactID { get; set; }
        public ContactModels Contact { get; set; }
    }
}
