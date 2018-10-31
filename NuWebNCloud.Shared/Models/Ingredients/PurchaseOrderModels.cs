using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class PurchaseOrderModels
    {
        public string Id { get; set; }
        public string PONumber { get; set; } //Code

        public List<SelectListItem> ListSupplier { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public SupplierModels Supplier { get; set; }

        public int POStatus { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime PODate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DeliveryDate { get; set; }
        [_AttributeForLanguage("Please choose tax")]
        public List<SelectListItem> ListTaxType { get; set; }
        public int TaxType { get; set; }
        //[Range(0, 100, ErrorMessage = "Please enter a value greater than or equal to 0 and maximun 100%")]
        [_AttributeForLanguage("Tax is field required")]
        [_AttributeForLanguageRange(0, 100, ErrorMessage = "please limit it in this area 0 to 100")]
        public double TaxValue { get; set; }
        public double TaxAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public double SubTotal { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public double Total { get; set; }

        public double Additional { get; set; }
        public string AdditionalReason { get; set; }

        public string Note { get; set; }

        //[Required(ErrorMessage = "Please choose store")]
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public bool StoreIntegrate { get; set; }
        public SStoreModels Store { get; set; }

        public List<string> ListStores { get; set; }
        public List<POIngredient> ListItem { get; set; }

        public string CreatedBy { get; set; }
        public string ModifierBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }

        public string ColorAlert { get; set; }
        public string Symbol { get; set; }
        public string TaxAmountInclusive { get; set; }

        public List<ReceiptPurchaseOrderModels> ListReceiptPO { get; set; }

        public int Delete { get; set; }

        //
        public List<POTaxModels> ListItemTax { get; set; }

        public PurchaseOrderModels()
        {
            ListTaxType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETax.Inclusive.ToString()), Value = Commons.ETax.Inclusive.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETax.AddOn.ToString()), Value = Commons.ETax.AddOn.ToString("d")},
            };
            ListTaxType = ListTaxType.OrderBy(o => o.Text).ToList();

            PODate = DateTime.Now;
            DeliveryDate = DateTime.Now;
            ListItem = new List<POIngredient>();
            //========
            ListSupplier = new List<SelectListItem>();
            ListItemTax = new List<POTaxModels>();
        }

        public void GetListSupplierFromCompnay(List<string> ListCompanyId)
        {
            SupplierFactory _SupplierFactory = new SupplierFactory();
            var dataSupplier = _SupplierFactory.GetData().Where(x => x.IsActived && ListCompanyId.Contains(x.CompanyId)).ToList();
            foreach (var item in dataSupplier)
            {
                ListSupplier.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id
                });
            }
        }
    }

    public class PurchaseOrderViewModels
    {
        public List<PurchaseOrderModels> ListItem { get; set; }
        public string StoreID { get; set; }
        public DateTime? ApplyFrom { get; set; }
        public DateTime? ApplyTo { get; set; }
        public PurchaseOrderViewModels()
        {
            ListItem = new List<PurchaseOrderModels>();
        }
    }

    public class POIngredient
    {
        public string Id { get; set; }

        public bool IsSelect { get; set; }
        public string IngredientId { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }
        public string Description { get; set; }
        public double PurchasePrice { get; set; }

        public string BaseUOM { get; set; }
        public double IngReceivingQty { get; set; }

        public double BaseQty { get; set; }

        public double QtyTolerance { get; set; }
        public double QtyToleranceS { get; set; }
        public double QtyToleranceP { get; set; }

        public int OffSet { get; set; }
        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }

        public int Delete { get; set; }

        public double ReceiptNoteQty { get; set; }
        public double ReturnReceiptNoteQty { get; set; }

        public double ReceivedQty { get; set; }
        public double ReceivingQty { get; set; }
        public double RemainingQty { get; set; }

        /*Return Note */
        public string ReceiptNoteDetailId { get; set; }
        public double ReturnQty { get; set; }

        /*isVisible*/
        public bool IsVisible { get; set; }
        /// <summary>
        /// Tax rate
        /// </summary>
        public double TaxPercent { get; set; }
        public double TaxAmount { get; set; }
        public string TaxId { get; set; }
        public string TaxName { get; set; }
        public int TaxType { get; set; }
        public List<SelectListItem> ListTax { get; set; }
        public bool IsShowTax { get; set; }
        public POIngredient()
        {
            ListTax = new List<SelectListItem>();
        }
    }

    public class POIngredientViewModels
    {
        public int Type { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string StoreId { get; set; }
        public List<POIngredient> ListItemView { get; set; }

        public POIngredientViewModels()
        {
            ListItemView = new List<POIngredient>();
        }
    }
    public class LoadIngredientModel
    {
        public List<string> ListItemNew { get; set; }
        public string SupplierId { get; set; }
        public string StoreId { get; set; }
    }

    public class POTaxModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public double Percent { get; set; }
        public double Amount { get; set; }
        public int TaxType { get; set; }
        public int OffSet { get; set; }
        public string Symbol { get; set; }
        public string sTaxType { get; set; }
        public bool IsShow { get; set; }
    }
}
