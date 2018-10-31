using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero.Suppliers
{
    public class SupplierXeroResponseModels
    {
        public string ContactID { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string BankAccountDetails { get; set; }
        public bool IsSupplier { get; set; }
        public bool IsCustomer { get; set; }
        public List<AddressesModels> Addresses { get; set; }
        public List<PhonesModels> Phones { get; set; }
        public DateTime UpdatedDateUTC { get; set; }

        public SupplierXeroResponseModels()
        {
            Addresses = new List<AddressesModels>();
            Phones = new List<PhonesModels>();
        }
    }
}
