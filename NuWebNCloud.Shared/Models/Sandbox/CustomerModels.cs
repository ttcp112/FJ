using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Sandbox
{
    public class CustomerModels : SBInventoryBaseModel
    {
        public string Index { get; set; } /* Index of in excel file */

        public string StoreID { get; set; }
        public string StoreName { get; set; }

        public string ID { get; set; }
        public string IC { get; set; }

        [_AttributeForLanguage("The Name field is required")]
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string ImageData { get; set; }
        public bool IsActive { get; set; }
        public string Phone { get; set; }

        [_AttributeForLanguage("The Email field is required")]
        public string Email { get; set; }

        public bool Gender { get; set; }

        public bool Marital { get; set; }
        public List<SelectListItem> ListMarital { get; set; }

        public DateTime JoinedDate { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime Anniversary { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsMembership { get; set; }

        public string HomeStreet { get; set; }
        public string HomeCity { get; set; }
        public string HomeZipCode { get; set; }
        public string HomeCountry { get; set; }

        public string OfficeStreet { get; set; }
        public string OfficeCity { get; set; }
        public string OfficeZipCode { get; set; }
        public string OfficeCountry { get; set; }

        //For Receipt
        public double TotalPaidAmout { get; set; }
        public double ByCash { get; set; }
        public double ByExTerminal { get; set; }
        public double ByGiftCard { get; set; }
        public double TotalRefund { get; set; }
        public DateTime LastVisited { get; set; }
        public int Reservation { get; set; }
        public int Cancelation { get; set; }
        public int WalkIn { get; set; }
        public List<ReceiptHistory> ListReceiptHistories { get; set; }
        public List<MembershipDTO> ListStore { get; set; }
        //=============
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }
        public bool PrivacyPolicy { get; set; }
        public string CompanyReg { get; set; }
        public CustomerModels()
        {
            ListMarital = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.MaritalStatusSingle), Value = "False"},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.MaritalStatusMarried), Value = "True"},
            };

            JoinedDate = DateTime.Now;
            BirthDate = DateTime.Now;
            Anniversary = DateTime.Now;
            ValidTo = DateTime.Now;
        }
    }

    public class MembershipDTO
    {
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public bool IsMembership { get; set; }
    }

    public class ReceiptHistory
    {
        public string OrderID { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime ReceiptDate { get; set; }
        public double TotalBill { get; set; }
        public double RefundAmount { get; set; }
        public string Remark { get; set; }
    }

    public class CustomerViewModels
    {
        public string StoreID { get; set; }
        public List<CustomerModels> ListItem { get; set; }
        public CustomerViewModels()
        {
            ListItem = new List<CustomerModels>();
        }
    }
}
