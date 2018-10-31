namespace NuWebNCloud.Shared.Models.Xero.Ingredient
{
    public class SalesDetailsModels
    {
        public double UnitPrice { get; set; }
        public string AccountCode { get; set; }
        /// <summary>
        /// Set value from file PDF
        /// </summary>
        public string TaxType { get; set; }
    }
}
