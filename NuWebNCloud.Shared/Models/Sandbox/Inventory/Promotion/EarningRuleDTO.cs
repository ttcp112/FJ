using NuWebNCloud.Shared.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion
{
    public class EarningRuleDTO
    {
        public string Condition { get; set; } // AND - OR
        public string ID { get; set; }
        public string ItemDetail { get; set; }

        public byte bDiscountType { get; set; }
        public bool DiscountType { get; set; }
        public List<SelectListItem> ListDiscountType { get; set; }

        [_AttributeForLanguage("The Discount Value field is required.")]
        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double DiscountValue { get; set; }

        public byte EarnType { get; set; }
        public List<SelectListItem> ListEarnType { get; set; }

        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Quantity { get; set; }

        public List<PromotionProductDTO> ListProduct { get; set; }
        public List<PromotionCategoryDTO> ListCategory { get; set; }

        public int OffSet { get; set; }
        public byte Status { get; set; }

        public int currentgroupOffSet { get; set; }
        public int currentItemOffset { get; set; }

        public EarningRuleDTO()
        {
            DiscountValue = 0;
            Quantity = 0;
            //======
            bDiscountType = (byte)Commons.EValueType.Percent;
            DiscountType = false;
            ListDiscountType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DiscountPercent), Value = Commons.EValueType.Percent.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DiscountValue), Value = Commons.EValueType.Currency.ToString("d")},
            };
            //======
            EarnType = (byte)Commons.EEarnType.SpentItem;
            ListEarnType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EarnTypeSpentItem), Value = Commons.EEarnType.SpentItem.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EarnTypeSpecificItem), Value = Commons.EEarnType.SpecificItem.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EarnTypeTotalBill), Value = Commons.EEarnType.TotalBill.ToString("d")}
            };

            //=====
            ListProduct = new List<PromotionProductDTO>();
            ListCategory = new List<PromotionCategoryDTO>();
        }

    }
}
