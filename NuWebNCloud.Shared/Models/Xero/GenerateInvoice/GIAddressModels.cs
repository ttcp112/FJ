namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    /// <summary>
    /// GENERATE INVOICE ADDRESS
    /// </summary>
    public class GIAddresseModels
    {
        public int AddressType { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}
