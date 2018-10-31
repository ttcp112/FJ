using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class CategoriesModels : SBInventoryBaseModel
    {
        public string ID { get; set; }

        [_AttributeForLanguage("Please choose store.")]
        public string StoreID { get; set; }

        public string StoreName { get; set; }

        [_AttributeForLanguage("The Sequence field is required.")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Sequence { get; set; }

        [_AttributeForLanguage("Name field is required")]
        public string Name { get; set; }

        public int TotalProducts { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }

        public string ParentID { get; set; }
        public string ParentName { get; set; }

        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }

        public bool IsShowInReservation { get; set; }

        public string ProductTypeID { get; set; }
        public int ProductType { get; set; }
        public string ProductTypeName { get; set; }
        public int Type { get; set; }

        public string GLAccountCode { get; set; }
        public bool IsGiftCard { get; set; }
        public bool IsIncludeNetSale { get; set; }

        public List<SelectListItem> ListProductType { get; set; }

        public List<CategoriesModels> ListChild { get; set; }

        public bool IsShowInKiosk { get; set; }

        public CategoriesModels()
        {
            ListProductType = new List<SelectListItem>()
            {
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish"), Value=Commons.EProductType.Dish.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier"), Value=Commons.EProductType.Modifier.ToString("d")},
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu"), Value=Commons.EProductType.SetMenu.ToString("d")}
            };
        }
        //public void GetListProductType()
        //{
        //    ListProductType = new List<SelectListItem>();

        //    ProductTypeFactory ProTypeFactory = new ProductTypeFactory();
        //    var listPT = ProTypeFactory.GetListProductType();
        //    foreach (var item in listPT)
        //    {
        //        if (item.Name.ToLower().Equals("promotion") || item.Name.ToLower().Equals("discount"))
        //            continue;
        //        ListProductType.Add(new SelectListItem
        //        {
        //            Text = item.Name,
        //            Value = item.ID
        //        });
        //    }
        //}
    }
}
