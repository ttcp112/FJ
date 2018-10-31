using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients.TaxPurchasing
{
    public class TaxPurchasingModels
    {
        public string Index { get; set; } /* Index in excel file */

        public string ID { get; set; }
        [_AttributeForLanguage("Tax Name is required")]
        public string Name { get; set; }
        [_AttributeForLanguage("Please choose store")]
        public string StoreID { get; set; }

        public string StoreName { get; set; }
        //[_AttributeForLanguage("Tax Code is required")]
        public string Code { get; set; }
        [_AttributeForLanguage("The Percent field is required.")]
        [_AttributeForLanguageRange(0, 100, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double Percent { get; set; }

        public bool IsActive { get; set; }
        [_AttributeForLanguage("The TaxType field is required")]
        public int TaxType { get; set; }
        public List<SelectListItem> ListTaxType { get; set; }
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        //==============
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }
        //Add on Tax
        //Include Tax in Item Price
        public List<string> ListStores { get; set; }

        /*Update Properties*/
        public string ItemDetail { get; set; }
        public List<ProductOfTaxDTO> ListProductOfTax { get; set; }
        public List<ProductOfTaxDTO> ListProductOfTaxSel { get; set; }
        public int currentItemOffset { get; set; }

        public List<string> ListProductID { get; set; }

        //Using for Tax of Product
        public int OffSet { get; set; }
        public int Status { get; set; }

        public string TaxrateinXero { get; set; }
        public string TaxXero { get; set; }
        public TaxPurchasingModels()
        {
            ListTaxType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETaxAddOnTax.ToString()), Value = Commons.ETax.AddOn.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETaxIncludeTax.ToString()), Value = Commons.ETax.Inclusive.ToString("d")},
            };

            ListProductOfTax = new List<ProductOfTaxDTO>();
            ListProductOfTaxSel = new List<ProductOfTaxDTO>();
            ListProductID = new List<string>();
        }
    }
}
