using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    public class GeneratePurchaseOrderApiModels
    {
        /// <summary>
        /// AppRegistrationId => "6aaee02a-5dc0-466a-9352-5e9e279c1fe2";
        /// </summary>
        public string AppRegistrationId { get; set; }
        /// <summary>
        /// StoreId => NS-XERO-INTEGRATION
        /// </summary>
        public string StoreId { get; set; }
        /// <summary>
        /// ApiURL => http://nupos.freeddns.org/xero/2.0/
        /// </summary>
        public string ApiURL { get; set; }

        /// <summary>
        /// SupplierId => Contact
        /// </summary>
        public string SupplierId { get; set; }
        /// <summary>
        /// SupplierName => Contact
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// Code => Reference
        /// </summary>
        public string Code { get; set; }

        public List<ItemIngredient> ListItem { get; set; }
        ///// <summary>
        ///// value TaxType from [TaxExempt = 1 | Inclusive = 2 | AddOn = 3]
        ///// </summary>
        //public int TaxType { get; set; }

        public string Note { get; set; }
        public string StoreName { get; set; }
        /// <summary>
        /// Telephone of Supplier
        /// </summary>
        public string Telephone { get; set; }

        public DateTime ClosingDatetime { get; set; }
        public byte LineAmountTypes { get; set; }
        public byte PurchaseOrderStatus { get; set; }
        public GeneratePurchaseOrderApiModels()
        {
            ListItem = new List<ItemIngredient>();
            ClosingDatetime = DateTime.Now;
        }
    }

    public class ItemIngredient
    {
        public string IngredientName { get; set; }
        public string IngredientCode { get; set; }
        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public string AccountCode { get; set; }

        //public int TaxType { get; set; }
    }
}
