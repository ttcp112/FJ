using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Models.Settings.Season;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory.Product
{
    public class InteProductModels : SBInventoryBaseModel
    {
        public string Index { get; set; } /* Index in excel file */
        public string ID { get; set; }
        //[Required(ErrorMessage = "Default Name is required")]
        [_AttributeForLanguage("Default Name is required")]
        public string Name { get; set; }
        //[Required(ErrorMessage = "Code is required")]
        [_AttributeForLanguage("Code is required")]
        public string ProductCode { get; set; }
        public int ProductType { get; set; }
        public string BarCode { get; set; }
        public string ImageURL { get; set; }
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }
        [DataType(DataType.Upload)]
        [FileSize(10240000)]
        [FileTypes("zip")]
        public HttpPostedFileBase ImageZipUpload { get; set; }
        public List<SelectListItem> ListStoreView { get; set; }
        public List<InteProductStoreModels> ListStore { get; set; }
        /*List Product Item On Store*/
        public List<InteProductItemOnStore> ListProductOnStore { get; set; }
        //Import
        public string Printer { get; set; }
        //add CurrencySymbol
        //public string CurrencySymbol { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }

        public InteProductModels()
        {
            ListStore = new List<InteProductStoreModels>();
            ListStoreView = new List<SelectListItem>();
            ListProductOnStore = new List<InteProductItemOnStore>();
        }
    }

    public class InteProductPriceModels
    {
        public double DefaultPrice { get; set; }
        public double SeasonPrice { get; set; }
        public string SeasonPriceID { get; set; }
        public string SeasonPriceName { get; set; }
    }

    public class InteProductItemOnStore : SBInventoryBaseCateGroupViewModel
    {
        public int OffSet { get; set; }
        public int Status { get; set; }
        public bool IsDeleteTemplate { get; set; }

        public bool IsShowOnStore { get; set; }

        public string StoreID { get; set; }
        public string StoreName { get; set; }

        public string CategoryID { get; set; }
        public string CategoryName { get; set; }
        /**/
        //[Range(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Sequence { get; set; }
        public string ColorCode { get; set; }
        public string KitchenDisplayName { get; set; }
        public string PrintOutName { get; set; }
        [Required]
        //[Range(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double Cost { get; set; }
        public string CurrencySymbol { get; set; }
        //[Range(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Unit { get; set; }
        //[Required]
        public string Measure { get; set; }
        //[Range(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double Quantity { get; set; }
        //[Range(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Limit { get; set; }
        public bool IsAllowDiscount { get; set; }
        public bool IsActive { get; set; }
        public bool IsCheckStock { get; set; }
        public bool IsIngredientLock { get; set; }
        public bool IsPromo { get; set; }
        public bool IsAllowOpenPrice { get; set; }
        public bool IsPrintOnCheck { get; set; }
        public DateTime? ExpiredDate { get; set; }

        public bool IsAutoAddToOrder { get; set; }
        public bool IsExtraFood { get; set; }

        public bool IsComingSoon { get; set; }
        public bool IsForce { get; set; }
        public bool IsOptional { get; set; }
        public bool IsAddition { get; set; }
        [Required]
        public byte DefaultState { get; set; }
        public List<SelectListItem> ListDefaultStatus { get; set; }
        public bool IsShowMessage { get; set; }
        public string Info { get; set; }
        public string Message { get; set; }
        public bool IsShowInReservation { get; set; }

        public InteProductPriceModels PriceOnStore { get; set; }
        public List<InteGroupProductModels> ListProductGroup { get; set; }
        public List<ProductSeasonDTO> ListProductSeason { get; set; }

        /**/
        public List<PriceItem> ListPrices { get; set; }
        public bool ServiceChargeDisabled { get; set; }
        public bool HasServiceCharge { get; set; }
        public string sServiceCharge { get; set; }
        public double ServiceChargeValue { get; set; }

        public string ProductSeasonKiosk { get; set; }
        public List<SeasonModels> ListSeasonKiosk { get; set; }

        public string ProductSeasonPOS { get; set; }
        public List<SeasonModels> ListSeasonPOS { get; set; }

        //Printer
        public string Printer { get; set; }
        public string LabelPrinter { get; set; }
        public List<string> lPrinter { get; set; }
        public List<PrinterOnProductModels> ListProductPrinter { get; set; }
        public List<PrinterModels> LstPrinter { get; set; }
        public List<PrinterModels> LstLabelPrinter { get; set; }

        //
        public List<SelectListItem> ListTimeSlots { get; set; }
        public List<SelectListItem> ListCategories { get; set; }
        public List<SelectListItem> ListSeasons { get; set; }
        public List<SelectListItem> ListPrinters { get; set; }

        // Updated 08282017
        public List<SBInventoryBaseCateGroupViewModel> lstCateGroup { get; set; }
        public string StringPrinterName { get; set; }
        public string StringLabelPrinterName { get; set; }

        //update 04-0202018 - Tax
        public string TaxID { get; set; }
        public string TaxName { get; set; }
        public double Tax { get; set; }
        public List<TaxModels> ListTax { get; set; }
        public bool IsTaxRequired { get; set; }
        //End update 04-0202018 - Tax

        public InteProductItemOnStore()
        {
            ListPrices = new List<PriceItem>();
            GetListPrices();

            ListProductSeason = new List<ProductSeasonDTO>();
            ListSeasonKiosk = new List<SeasonModels>();
            ListSeasonPOS = new List<SeasonModels>();

            lPrinter = new List<string>();
            ListProductPrinter = new List<PrinterOnProductModels>();
            LstPrinter = new List<PrinterModels>();
            LstLabelPrinter = new List<PrinterModels>();

            ListCategories = new List<SelectListItem>();

            ListProductGroup = new List<InteGroupProductModels>();

            GetListDefaultStatus();

            IsShowOnStore = true;
            IsActive = true;
            IsAllowDiscount = true;
            IsPrintOnCheck = true;
            IsPromo = true;
            // Updated 08282017
            lstCateGroup = new List<SBInventoryBaseCateGroupViewModel>();

            ListTax = new List<TaxModels>();
        }

        private void GetListPrices()
        {
            for (int i = 0; i < 2; i++) /*editor by trongntn 11-08-2016*/
            {
                PriceItem item = new PriceItem();
                if (i == 0)
                {
                    item.PriceName = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price")) + (i + 1);
                    item.IsDefault = true;
                    item.PriceTag = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price"));
                }
                else
                {
                    item.PriceName = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season Price"));
                }
                ListPrices.Add(item);
            }
        }

        private void GetListDefaultStatus()
        {
            ListDefaultStatus = new List<SelectListItem>()
            {
                new SelectListItem() { Text = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PendingStatus)), Value = Commons.EItemState.PendingStatus.ToString("d")},
                new SelectListItem() { Text = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ReadyStatus)), Value = Commons.EItemState.CompleteStatus.ToString("d")},
                new SelectListItem() { Text = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ServedStatus)), Value = Commons.EItemState.ServedStatus.ToString("d")}
            };
        }
    }
}
