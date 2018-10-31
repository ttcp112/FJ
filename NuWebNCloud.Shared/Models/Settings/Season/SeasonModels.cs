using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion;
using NuWebNCloud.Shared.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Shared.Models.Settings.Season
{
    public class SeasonModels
    {
        //public string StoreID { get; set; }
        //public string StoreName { get; set; }
        //public string ID { get; set; }
        //public string Name { get; set; }
        //public DateTime StartOn { get; set; }
        //public DateTime EndOn { get; set; }
        //public DateTime StartTime { get; set; }
        //public DateTime EndTime { get; set; }

        public string Index { get; set; } /* Index in excel file */

        public string ID { get; set; }
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan TStartTime { get; set; }
        public TimeSpan TEndTime { get; set; }
        public bool Unlimited { get; set; }
        public bool IsPOS { get; set; }
        //[Required(ErrorMessage = "Please choose Store.")]
        public string StoreID { get; set; }

        public string StoreName { get; set; }

        public int RepeatType { get; set; }
        public List<SelectListItem> ListRepeatType { get; set; }
        public List<int> ListDay { get; set; }
        public List<DayItem> ListWeekDayV2 { get; set; }
        public List<DayItem> ListMonthDayV2 { get; set; }

        public SeasonModels SeasonDTO { get; set; }

        //Import and Export
        public List<string> ListStores { get; set; }
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }

        //Use for Inventory
        public int OffSet { get; set; }
        public int Status { get; set; }

        public SeasonModels()
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            //=================
            ListDay = new List<int>();
            ListRepeatType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PromotionDayOfWeek).ToString(), Value = Commons.ERepeatType.DayOfWeek.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PromotionDayOfMonth).ToString(), Value = Commons.ERepeatType.DayOfMonth.ToString("d")}
            };
            ListWeekDayV2 = new List<DayItem>();
            ListMonthDayV2 = new List<DayItem>();
            for (int i = 2; i < 9; i++)
            {
                string dayName = i == 8 ? Enum.GetName(typeof(DayOfWeek), 0) : Enum.GetName(typeof(DayOfWeek), i - 1);
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
}
