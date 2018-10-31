using NuWebNCloud.Shared.Factory.Sandbox;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Sandbox
{
    public class EmployeeModels
    {
        public string Index { get; set; } /* Index of Employee in excel file */

        public string StoreID { get; set; }
        public List<string> ListStoreID { get; set; }
        public string StoreName { get; set; }

        //Progress for Multiple Store
        public List<SelectListItem> ListStore { get; set; }
        //====================

        public string ID { get; set; }

        [_AttributeForLanguage("Name field is required")]
        public string Name { get; set; }

        public string ImageURL { get; set; }
        public string ImageData { get; set; }

        [_AttributeForLanguage("Email is required")]
        [_AttributeForLanguageRegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }

        public bool IsActive { get; set; }

        [_AttributeForLanguage("PIN Code is required")]
        [_AttributeForLanguageRegularExpression("([0-9]+)", ErrorMessage = "Please enter valid Number")]
        public string Pincode { get; set; }

        [_AttributeForLanguageRegularExpression("([0-9]+)", ErrorMessage = "Please enter valid Number")]
        public string Phone { get; set; }       
        public bool Gender { get; set; }
        public bool Marital { get; set; }
        public List<SelectListItem> ListMarital { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime HiredDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime BirthDate { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public List<EmployeeOnStoreModels> ListEmpStore { get; set; }
        public List<WorkingTimeModels> ListWorkingTime { get; set; }

        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public List<SelectListItem> ListRole { get; set; }

        public List<TimeItem> TimeItems { get; set; }

        [DataType(DataType.Upload)]
        [FileSize(300000)]
        [FileTypes("jpeg,jpg,png")]
        public HttpPostedFileBase PictureUpload { get; set; }

        public byte[] PictureByte { get; set; }

        //=============
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }

        public List<RoleWorkingTime> ListRoleWorkingTime { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Comfirm Password")]
        public string ConfirmPassword { get; set; }

        public EmployeeModels()
        {
            ListStore = new List<SelectListItem>();

            ListRoleWorkingTime = new List<RoleWorkingTime>();
            ListStoreID = new List<string>();

            ListEmpStore = new List<EmployeeOnStoreModels>();
            ListWorkingTime = new List<WorkingTimeModels>();
            TimeItems = new List<TimeItem>();

            ListMarital = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.MaritalStatusSingle), Value = "False"},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.MaritalStatusMarried), Value = "True"},
            };
            HiredDate = DateTime.Now;
            BirthDate = DateTime.Now;
        }
    }

    public class EmployeeOnStoreModels
    {
        public string ID { get; set; }
        public string EmployeeID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }       
    }

    public class WorkingTimeModels
    {
       
        public string WorkingTimeID { get; set; }
        public string StoreID { get; set; }
        public int Day { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public bool IsOffline { get; set; }
        //==========
        public string StrDate { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }

    public class EmployeeViewModels
    {
        public string StoreID { get; set; }
        public List<EmployeeModels> ListItem { get; set; }
        public EmployeeViewModels()
        {
            ListItem = new List<EmployeeModels>();
        }
    }

    public class TimeItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    /**/
    public class RoleWorkingTime
    {
        public int OffSet { get; set; }
        public int Status { get; set; }
      
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public bool IsSyncPoins { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public List<SelectListItem> ListRole { get; set; }

        public List<TimeItem> TimeItems { get; set; }
        public List<WorkingTimeModels> ListWorkingTime { get; set; }

        public RoleWorkingTime()
        {
            StoreID = "";

            ListWorkingTime = new List<WorkingTimeModels>();
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
                ListWorkingTime.Add(new WorkingTimeModels
                {
                    StrDate = a,
                    Day = i,
                    IsOffline = true,
                    From = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF"),
                    To = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF")
                });
            }
            TimeItems = new List<TimeItem>();
            TimeItem off = new TimeItem();
            off.Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF");
            off.Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF");
            TimeItems.Add(off);
        }

        public void GetListRole(string StoreID, List<string> ListOrganizationId)
        {
            ListRole = new List<SelectListItem>();
            RoleFactory roleFactory = new RoleFactory();
            List<SelectListItem> slcRole = new List<SelectListItem>();
            var listRole = roleFactory.GetListRole(StoreID, null, ListOrganizationId);
            foreach (var item in listRole)
            {
                ListRole.Add(new SelectListItem
                {
                    Text = item.Name,// + " [" + item.StoreName + "]",
                    Value = item.ID
                });
            }
        }
    }
}
