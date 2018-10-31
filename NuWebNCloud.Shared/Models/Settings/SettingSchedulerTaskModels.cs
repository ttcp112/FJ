using NuWebNCloud.Shared.Factory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class SettingSchedulerTaskModels
    {
        public string ID { get; set; }
        public string ReportName { get; set; }
        [Required(ErrorMessage = "Please choose at Store!!!")]
        public string StoreId { get; set; }

        public string StoreName { get; set; }

        public string Cc { get; set; }
        public string Bcc { get; set; }

        //[Required(ErrorMessage = "Please choose at day of week!!!")]
        public string DayOfWeeks { get; set; }

        public DateTime LastSuccessUtc { get; set; }
        public DateTime LastDateModified { get; set; }
        public DateTime CreatedDate { get; set; }

        public string CreatedUser { get; set; }
        public string LastUserModified { get; set; }
        
        public string ReportId { get; set; }
        [Required(ErrorMessage = "Please choose at Report!!!")]
        public List<string> ListReportID { get; set; }

        [Required(ErrorMessage = "The Subject field is required.")]
        public string EmailSubject { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [Range(0, 23)]
        public int Hour { get; set; } = 8;
        [Required]
        [Range(0, 59)]
        public int Minute { get; set; } = 10;

        [Required]
        public bool Enabled { get; set; } = true;
        [Required]
        public bool IsDaily { get; set; } = true;
        [Required]
        public bool IsMonth { get; set; }
        public string Description { get; set; }

        public List<SelectListItem> ListNameReport { get; set; }
        public List<SelectListItem> ListDayOfWeeks { get; set; }
        public List<SelectListItem> ListStore { get; set; }

        //private void GetListNameReport()
        //{
        //    ListNameReport = new List<SelectListItem>()
        //    {
        //        new SelectListItem() {
        //            Text = Regex.Replace(Commons.Report_TimeClockSummary, "([A-Z])", " $1"),
        //            Value = Commons.Report_TimeClockSummary
        //        },

        //        new SelectListItem() {
        //            Text = Regex.Replace(Commons.Report_ItemizeSalesAnalysis, "([A-Z])", " $1"),
        //            Value = Commons.Report_ItemizeSalesAnalysis
        //        },

        //        new SelectListItem() {
        //            Text = Regex.Replace(Commons.Report_InventoryReport, "([A-Z])", " $1"),
        //            Value = Commons.Report_InventoryReport
        //        },

        //        new SelectListItem() {
        //            Text = Regex.Replace(Commons.Report_HourlySales, "([A-Z])", " $1"),
        //            Value = Commons.Report_HourlySales
        //        },

        //        new SelectListItem() {
        //            Text = Regex.Replace(Commons.Report_DailySale, "([A-Z])", " $1"),
        //            Value = Commons.Report_DailySale
        //        },

        //        new SelectListItem() {
        //            Text =Regex.Replace(Commons.Report_ClosedReceipt, "([A-Z])", " $1"),
        //            Value = Commons.Report_ClosedReceipt
        //        }
        //    };

        //}

        public SettingSchedulerTaskModels()
        {
            ListReportID = new List<string>();
            GetListReportName();
            //GetListNameReport();
            //GetDateOfWeek();
            ID = "0";
        }

        public void GetDateOfWeek()
        {
            ListDayOfWeeks = new List<SelectListItem>();
            DateOfWeeksFactory facDay = new DateOfWeeksFactory();
            var lstDay = facDay.GetData();
            foreach (var item in lstDay)
            {
                SelectListItem selectItem = new SelectListItem();
                selectItem.Text = item.DayName;
                selectItem.Value = item.DayNumber.ToString();
                selectItem.Selected = true;
                ListDayOfWeeks.Add(selectItem);
            }
        }

        public void GetListReportName()
        {
            ListNameReport = new List<SelectListItem>();
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(ScheduleReports));
            string path = HttpContext.Current.Server.MapPath("/App_Data/ScheduleReport.xml");
            var xmlInputData = File.ReadAllText(path);
            ScheduleReports temp = new ScheduleReports();
            using (StringReader sr = new StringReader(xmlInputData))
            {
                temp = (ScheduleReports)ser.Deserialize(sr);
            }
            if (temp.Reports.Any())
            {
                temp.Reports.ForEach(o =>
                {
                    ListNameReport.Add(new SelectListItem()
                    {
                        Value = o.Key,
                        Text = o.Value
                    });
                });               
            }
        }

        public List<SelectListItem> GetDataStore(List<string> listOrganizationId)
        {
            StoreFactory _storeFactory = new StoreFactory();
            CompanyFactory _companyFactory = new CompanyFactory();
            ListStore = new List<SelectListItem>();
            ListStore = _storeFactory.GetListStore(listOrganizationId);
            //ListStore = _storeFactory.GetListStore(_companyFactory.GetListCompany(listOrganizationId));
            return ListStore;
        }
    }
    public class ScheduleReports
    {
        public List<Report> Reports { get; set; }
    }

    public class Report
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
