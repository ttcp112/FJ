using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    /// <summary>
    /// GENERATE INVOICE CONTACT
    /// </summary>
    public class GIContactModels
    {
        public string ContactID { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string BankAccountDetails { get; set; }
        public bool IsSupplier { get; set; }
        public bool IsCustomer { get; set; }
        public List<string> ContactPersons { get; set; }
        public List<GIAddresseModels> Addresses { get; set; }
        public List<GIPhoneModels> Phones { get; set; }
        public List<string> ContactGroups { get; set; }
        public DateTime UpdatedDateUTC { get; set; }

        public GIContactModels()
        {
            ContactPersons = new List<string>();
            Addresses = new List<GIAddresseModels>();
            Phones = new List<GIPhoneModels>();
            ContactGroups = new List<string>();
        }
    }
}
