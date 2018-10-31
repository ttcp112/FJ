using NuWebNCloud.Shared.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class SBInventoryBaseModel
    {

        public List<SelectListItem> ListSeasons { get; set; }

        public List<SelectListItem> ListCategories { get; set; }

        public List<SelectListItem> ListTimeSlots { get; set; }

        public List<SelectListItem> ListDefaultStatus { get; set; }

        public List<PriceItem> ListPrices { get; set; }

        public List<SelectListItem> ListPrinters { get; set; }

        public int ItemType { get; set; }

        public int ChildItemType { get; set; }

        public string CategoryName { get; set; }
        public string CurrencySymbol { get; set; }
        public double PriceDefault { get; set; }

        [DataType(DataType.Upload)]
        //[FileSize(10072000)]
        //[FileSize(300000)]
        [FileTypes("jpeg,jpg,png")]

        public HttpPostedFileBase PictureUpload { get; set; }

        public string Picture { get; set; }

        public string URL { get; set; }

        public byte[] PictureByte { get; set; }

        public List<string> ListStores { get; set; }

        public byte[] PictureString
        {
            set
            {
                if (value != null)
                    Picture = Convert.ToBase64String(value);
                else
                    Picture = "";
            }
        }
        // Updated 08282017
        public List<SBInventoryBaseCateGroupViewModel> lstCateGroup { get; set; }
        public SBInventoryBaseModel()
        {

            ListSeasons = new List<SelectListItem>();
            ListCategories = new List<SelectListItem>();
            ListTimeSlots = new List<SelectListItem>();
            ListPrices = new List<PriceItem>();
            ListPrinters = new List<SelectListItem>();
            GetListPrices();
            GetListDefaultStatus();
            // Updated 08282017
            lstCateGroup = new List<SBInventoryBaseCateGroupViewModel>();
        }

        private void GetListDefaultStatus()
        {
            ListDefaultStatus = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PendingStatus), Value = Commons.EItemState.PendingStatus.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ReadyStatus), Value = Commons.EItemState.CompleteStatus.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ServedStatus), Value = Commons.EItemState.ServedStatus.ToString("d")}
            };
        }

        private void GetListPrices()
        {
            for (int i = 0; i < 2; i++) /*editor by trongntn 11-08-2016*/
            {
                PriceItem item = new PriceItem();
                item.PriceName = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price") + " " + (i + 1);
                if (i == 0)
                {
                    item.IsDefault = true;
                    item.PriceTag = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price");
                }
                ListPrices.Add(item);
            }
        }
    }
    // Updated 08282017
    public class SBInventoryBaseCateGroupViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public bool Selected { get; set; }
    }
}
