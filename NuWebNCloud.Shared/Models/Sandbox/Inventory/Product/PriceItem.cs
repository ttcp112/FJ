using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Product
{
    public class PriceItem
    {
        public string StoreID { get; set; }
        public string ID { get; set; }
        public string PriceTag { get; set; }

        //[Required(ErrorMessage = "Price is required")]
        [_AttributeForLanguage("The Price field is required.")]
        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double Price { get; set; }

        public bool IsDefault { get; set; }
        public string SeasonID { get; set; }
        public string SeasonName { get; set; }
        public string PriceName { get; set; }
        public int TypeID { get; set; }
        public DateTime? Expired { get; set; }
        public List<SelectListItem> ListSeasons { get; set; }

        public PriceItem()
        {
            ListSeasons = new List<SelectListItem>();
        }
    }
}
