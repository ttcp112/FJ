using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

// Updated 08292017
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class SStoreModels
    {
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }

        public string OrganizationID { get; set; }
        public string OrganizationName { get; set; }

        public string IndustryID { get; set; }
        public string IndustryName { get; set; }

        public string Code { get; set; }
        public string ExceptionData { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public string CreatedUser { get; set; }

        [_AttributeForLanguage("The Name field is required")]
        public string StoreID { get; set; }
        public string ID { get; set; }
        public string StoreCode { get; set; }
        public string DeviceName { get; set; }
        public bool IsActive { get; set; }
        [_AttributeForLanguage("The Name field is required")]
        public string Name { get; set; }
        public string Street { get; set; }

        [_AttributeForLanguageRegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }

        public string Address { get; set; }
        public string Country { get; set; }
        public string TimeZone { get; set; }

        public string Zipcode { get; set; }
        public string GSTRegNo { get; set; }
        public string Phone { get; set; }

        public List<ListBusinessHour> ListBusinessHour { get; set; }
        public List<string> ListStores { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public string OfficialNumber { get; set; }
        public string City { get; set; }
        //=============================
        public List<TimeItem> TimeItems { get; set; }
        //public List<RoleModels> ListRole { get; set; }
        [DataType(DataType.Upload)]
        //[FileSize(10072000)]
        [FileTypes("jpeg,jpg,png")]
        public HttpPostedFileBase PictureUpload { get; set; }
        public byte[] PictureByte { get; set; }
        //=======================================
        public string ImageURL { get; set; }

        // Updated 08292017
        public List<SelectListItem> ListCountries { get; set; }

        // Updated 08312017
        public List<SelectListItem> ListTimezones { get; set; }

        public List<RFilterChooseExtBaseModel> ListCategory { get; set; }
        public List<RFilterChooseExtBaseModel> ListPayment { get; set; }
        public List<RFilterChooseExtBaseModel> ListSetMenu { get; set; }

        public SStoreModels()
        {
            //ListBusinessHour = new List<ListBusinessHour>();

            // Updated 08292017
            ListCountries = new List<SelectListItem>();

            // Updated 08312017
            ListTimezones = new List<SelectListItem>();
        }

        public void GetValue()
        {
            //BusinessHour
            ListBusinessHour = new List<ListBusinessHour>();
            for (int i = 2; i <= 8; i++)
            {
                var a = "";
                switch (i)
                {
                    case 2:
                        a = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Mon");
                        break;
                    case 3:
                        a = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tue");
                        break;
                    case 4:
                        a = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Wed");
                        break;
                    case 5:
                        a = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Thu");
                        break;
                    case 6:
                        a = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Fri");
                        break;
                    case 7:
                        a = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sat");
                        break;
                    case 8:
                        a = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sun");
                        break;
                }
                ListBusinessHour.Add(new ListBusinessHour
                {
                    StrDate = a,
                    Day = i,
                    IsOffline = true,
                    From = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF"),
                    To = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF")
                });
            }
            GetTime();
        }

        public void GetTime()
        {
            TimeItems = new List<TimeItem>();
            TimeItem off = new TimeItem();
            off.Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF");
            off.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF");
            TimeItems.Add(off);

            //for (int i = 0; i < 24; i++)
            //{
            //    string ho = i.ToString();
            //    if (ho.Length < 2)
            //        ho = "0" + ho;
            //    for (int j = 0; j < 60; j++)
            //    {
            //        string mi = j.ToString();
            //        if (mi.Length < 2)
            //            mi = "0" + mi;
            //        TimeItem item = new TimeItem();
            //        string time = ho + ":" + mi;
            //        item.Value = time;
            //        item.Text = time;
            //        TimeItems.Add(item);
            //    }
            //}
        }
    }

    public class ListBusinessHour
    {
        public int Day { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public bool IsOffline { get; set; }
        //===================================
        public string StrDate { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }

    public class TimeItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class StoreModelView
    {
        public string StoreID { get; set; }
        public List<StoreGroupByCompany> List_Store_Group { get; set; }
        public List<SStoreModels> List_Store { get; set; }
        public StoreModelView()
        {
            List_Store = new List<SStoreModels>();
            List_Store_Group = new List<StoreGroupByCompany>();
        }
    }
    public class StoreGroupByCompany 
    {
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public List<SStoreModels> List_Store { get; set; }
        public StoreGroupByCompany()
        {
            List_Store = new List<SStoreModels>();
        }
    }
}
