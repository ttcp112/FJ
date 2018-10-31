using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero.Suppliers
{

    public class ContactModels 
    {
        public string ContactID { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string BankAccountDetails { get; set; }
        public bool IsSupplier { get; set; }
        public bool IsCustomer { get; set; }
        public bool HasAttachments { get; set; }
        public List<AddressesModels> Addresses { get; set; }
        public List<PhonesModels> Phones { get; set; }
        public DateTime UpdatedDateUTC { get; set; }

        public ContactModels()
        {
            Addresses = new List<AddressesModels>();
            Phones = new List<PhonesModels>();
        }
    }

    public class AddressesModels
    {
        public int AddressType { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class PhonesModels
    {
        public int PhoneType { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneAreaCode { get; set; }
        public string PhoneCountryCode { get; set; }
    }
}
