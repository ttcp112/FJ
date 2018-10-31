using NuWebNCloud.Shared.Factory.Sandbox;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox
{
    public class InteEmployeeModels
    {
        public string Index { get; set; } /* Index of Employee in excel file */

        //[Required(ErrorMessage = "Please choose store")]
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
        public List<InteEmployeeOnStoreModels> ListEmpStore { get; set; }
        public List<InteWorkingTimeModels> ListWorkingTime { get; set; }
        public List<SelectListItem> LstCompany { get; set; }
        public List<CompanyModels> ListCompany { get; set; }
        
        //[Required(ErrorMessage = "Please choose role")]
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsSyncPoins { get; set; }
        public bool IsWithPoins { get; set; }
        public List<SelectListItem> ListRole { get; set; }

        public List<InteTimeItem> TimeItems { get; set; }

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

        public List<InteRoleWorkingTime> ListRoleWorkingTime { get; set; }

        public List<string> ListStores { get; set; }

        public InteEmployeeModels()
        {
            ListStore = new List<SelectListItem>();
            ListCompany = new List<CompanyModels>();
            LstCompany = new List<SelectListItem>();
            ListRoleWorkingTime = new List<InteRoleWorkingTime>();
            ListStoreID = new List<string>();

            ListEmpStore = new List<InteEmployeeOnStoreModels>();
            ListWorkingTime = new List<InteWorkingTimeModels>();
            TimeItems = new List<InteTimeItem>();

            ListMarital = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.MaritalStatusSingle), Value = "False"},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.MaritalStatusMarried), Value = "True"},
            };
            HiredDate = DateTime.Now;
            BirthDate = DateTime.Now;
            //ListRole = new List<SelectListItem>();
        }

        public void GetValue()
        {
        }

        public void GetTime()
        {
        }
    }

    public class InteEmployeeOnStoreModels
    {
        public string ID { get; set; }
        public string EmployeeID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsSyncPoins { get; set; }
    }

    public class InteWorkingTimeModels
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

    public class InteEmployeeViewModels
    {
        public string StoreID { get; set; }
        public List<string> ListStores { get; set; }
        public List<InteEmployeeModels> ListItem { get; set; }
        public InteEmployeeViewModels()
        {
            ListItem = new List<InteEmployeeModels>();
        }
    }

    public class InteTimeItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class InteRoleWorkingTime
    {
        public int OffSet { get; set; }
        public int Status { get; set; }
        public bool IsSyncPoins { get; set; }
        public bool IsWithPoins { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public List<SelectListItem> ListRole { get; set; }

        public List<InteTimeItem> TimeItems { get; set; }
        public List<InteWorkingTimeModels> ListWorkingTime { get; set; }

        public InteRoleWorkingTime()
        {
            StoreID = "";

            ListWorkingTime = new List<InteWorkingTimeModels>();
            for (int i = 2; i <= 8; i++)
            {
                var a = "";
                switch (i)
                {
                    case 2:
                        a = "Mon";
                        break;
                    case 3:
                        a = "Tue";
                        break;
                    case 4:
                        a = "Wed";
                        break;
                    case 5:
                        a = "Thu";
                        break;
                    case 6:
                        a = "Fri";
                        break;
                    case 7:
                        a = "Sat";
                        break;
                    case 8:
                        a = "Sun";
                        break;
                }
                ListWorkingTime.Add(new InteWorkingTimeModels
                {
                    StrDate = a,
                    Day = i,
                    IsOffline = true,
                    From = "OFF",
                    To = "OFF"
                });
            }
            TimeItems = new List<InteTimeItem>();
            InteTimeItem off = new InteTimeItem();
            off.Value = "OFF";
            off.Text = "OFF";
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
                    Text = item.Name + " [" + item.StoreName + "]",
                    Value = item.ID
                });
            }
        }
    }


    /*Object API*/
    public class InteSBEmployeeApiModels
    {
        public string StoreID { get; set; }
        public InteEmployeeModels EmployeeDTO { get; set; }

        public string ID { get; set; }
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string ImageData { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string Pincode { get; set; }
        public string Phone { get; set; }
        public bool Gender { get; set; }
        public bool Marital { get; set; }
        public DateTime HiredDate { get; set; }
        public DateTime BirthDate { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public List<InteEmployeeOnStoreModels> ListEmpStore { get; set; }
        public List<InteWorkingTimeModels> ListWorkingTime { get; set; }

        public List<InteEmployeeModels> ListEmployee { get; set; }

        public List<string> ListOrgID { get; set; }
        //=============
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }

        public List<string> ListStoreID { get; set; }
    }
}
