using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion
{
    public class PromotionModels
    {
        public string Index { get; set; } /* Index in excel file */

        public string ID { get; set; }

        [_AttributeForLanguage("Promote Name field is required")]
        public string Name { get; set; }

        [_AttributeForLanguage("Please choose store.")]
        public string StoreID { get; set; }

        public string StoreName { get; set; }

        public string Description { get; set; }

        public string ImageURL { get; set; }
        [DataType(DataType.Upload)]
        [FileSize(10072000)]
        [FileTypes("jpeg,jpg,png")]
        public HttpPostedFileBase PictureUpload { get; set; }
        public byte[] PictureByte { get; set; }

        public string ShortName { get; set; }

        [_AttributeForLanguage("Promote Code field is required")]
        public string PromoteCode { get; set; }

        public byte? PromotionType { get; set; }

        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int? Priority { get; set; }

        public bool? isActive { get; set; }
        public bool? IsAllowedCombined { get; set; }
        public int? MaximumUsedQty { get; set; }

        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double? MaximumEarnAmount { get; set; }
        public int? MaximumQtyPerUser { get; set; }
        public bool? IsRepeated { get; set; }
        public bool? IsLimited { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? FromDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? ToDate { get; set; }

        public TimeSpan TStartTime { get; set; }
        public TimeSpan TEndTime { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public bool IsLimitedTime { get; set; }

        public string DateOfWeek { get; set; }
        public string DateOfMonth { get; set; }

        public bool isSpendOperatorAnd { get; set; }
        public bool isEarnOperatorAnd { get; set; }

        public List<SpendingRuleDTO> ListSpendingRule { get; set; }
        public List<EarningRuleDTO> ListEarningRule { get; set; }

        public int RepeatType { get; set; }
        public List<SelectListItem> ListRepeatType { get; set; }
        public List<int> ListDay { get; set; }
        public List<DayItem> ListWeekDayV2 { get; set; }
        public List<DayItem> ListMonthDayV2 { get; set; }

        public List<string> ListStores { get; set; }

        //===========
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }

        public PromotionModels()
        {

            FromDate = DateTime.Now;
            ToDate = DateTime.Now;
            isActive = false;
            PromotionType = 0;
            Priority = 0;
            IsAllowedCombined = false;
            MaximumUsedQty = 0;
            MaximumEarnAmount = 0;
            MaximumQtyPerUser = 0;
            IsLimited = false;
            IsRepeated = false;

            //=================
            ListDay = new List<int>();
            ListRepeatType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PromotionDayOfWeek), Value = Commons.ERepeatType.DayOfWeek.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PromotionDayOfMonth), Value = Commons.ERepeatType.DayOfMonth.ToString("d")}
            };
            ListWeekDayV2 = new List<DayItem>();
            ListMonthDayV2 = new List<DayItem>();
            for (int i = 1; i < 8; i++)
            {
                string dayName = Enum.GetName(typeof(DayOfWeek), i - 1);
                DayItem dItem = new DayItem
                {
                    Index = i,
                    Name = dayName,
                    IsActive = false,
                    Status = 9
                };
                ListWeekDayV2.Add(dItem);
            }
            for (int i = 1; i < 32; i++)
            {
                DayItem dItem = new DayItem
                {
                    Index = i,
                    Name = i.ToString(),
                    IsActive = false,
                    Status = 9
                };
                ListMonthDayV2.Add(dItem);
            }
        }
    }

    public class PromotionViewModels
    {
        public string StoreID { get; set; }
        public List<PromotionModels> ListItem { get; set; }
        public PromotionViewModels()
        {
            ListItem = new List<PromotionModels>();
        }
    }

    public class DayItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }
    }
}
