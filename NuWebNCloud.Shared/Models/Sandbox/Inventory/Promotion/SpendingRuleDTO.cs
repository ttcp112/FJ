using NuWebNCloud.Shared.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion
{
    public class SpendingRuleDTO
    {
        public string Condition { get; set; }// AND - OR
        public string ID { get; set; }
        public string ItemDetail { get; set; }

        public byte SpendType { get; set; }
        public List<SelectListItem> ListSpendType { get; set; }

        public byte SpendOnType { get; set; }
        public List<SelectListItem> ListSpendOnType { get; set; }

        [_AttributeForLanguage("The Amount field is required.")]
        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double Amount { get; set; }
        public List<PromotionProductDTO> ListProduct { get; set; }
        public List<PromotionCategoryDTO> ListCategory { get; set; }

        //public List<ItemModels> ListItem { get; set; }

        public int OffSet { get; set; }
        public byte Status { get; set; }

        public int currentgroupOffSet { get; set; }
        public int currentItemOffset { get; set; }

        public SpendingRuleDTO()
        {
            Amount = 0;
            //======
            SpendType = (byte)Commons.ESpendType.BuyItem;
            ListSpendType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendTypeBuyItem), Value = Commons.ESpendType.BuyItem.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendTypeSpendMoney), Value = Commons.ESpendType.SpendMoney.ToString("d")},
            };
            //======
            SpendOnType = (byte)Commons.ESpendOnType.AnyItem;
            ListSpendOnType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendOnTypeAnyItem), Value = Commons.ESpendOnType.AnyItem.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendOnTypeSpecificItem), Value = Commons.ESpendOnType.SpecificItem.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendOnTypeTotalBill), Value = Commons.ESpendOnType.TotalBill.ToString("d")}
            };

            //=====
            ListProduct = new List<PromotionProductDTO>();
            ListCategory = new List<PromotionCategoryDTO>();
        }

    }
}
