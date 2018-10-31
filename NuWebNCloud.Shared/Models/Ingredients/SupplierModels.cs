using Newtonsoft.Json;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
//using ServiceStack.Html;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

// Updated 08292017
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class SupplierModels
    {
        public string Index { get; set; }

        public string Id { get; set; }
        [_AttributeForLanguage("Name is required!")]
        public string Name { get; set; }

        [_AttributeForLanguage("Address is required")]
        public string Address { get; set; }

        public string Country { get; set; }

        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }

        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        public string ContactInfo { get; set; }
        public bool IsActived { get; set; }
        
        [_AttributeForLanguage("Please choose company.")]
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public string ModifierBy { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string IngredientId { get; set; }

        // use to save listIngredients get detail list ingredients of I_Ingredients_Supplier
        public List<Ingredients_SupplierModel> ListSupIng { get; set; }
        public List<Ingredients_SupplierModel> ListSupIngUnSelected { get; set; }


        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }

        //export or import 
        public string IngName { get; set; }
        public string IngrID { get; set; }
        public List<string> ListCompanys { get; set; }
        public List<SelectListItem> ListType { get; set; }

        // Updated 08292017
        public List<SelectListItem> ListCountries { get; set; }

        public SupplierModels()
        {
            IsActived = true;
            ListSupIng = new List<Ingredients_SupplierModel>();
            ListSupIngUnSelected = new List<Ingredients_SupplierModel>();

            // Updated 08292017
            ListCountries = new List<SelectListItem>();
        }
    }

    public class SupplierViewModels
    {
        public string CompanyId { get; set; }
        public string Id { get; set; }
        public List<SupplierModels> ListItem { get; set; }
        public SupplierViewModels()
        {
            ListItem = new List<SupplierModels>();
        }
    }

}
