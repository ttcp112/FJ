using NuWebNCloud.Shared.Models.Xero;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class XeroResponseModels
    {
        public bool Success { get; set; }
        public ErrorDTO Error { get; set; }
        public object RawData { get; set; }
        public List<XeroDTO> ListXeroSetting { get; set; }
        public XeroResponseModels()
        {
            ListXeroSetting = new List<XeroDTO>();
        }        
    }

    public class XeroDTO
    {
        public string AccountID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string TaxType { get; set; }
        public string Description { get; set; }
        public int Class { get; set; }
        public bool EnablePaymentsToAccount { get; set; }
        public bool ShowInExpenseClaims { get; set; }
        public string ReportingCode { get; set; }
        public string ReportingCodeName { get; set; }
        public bool HasAttachments { get; set; }
        public string NameDisplayCombobox { get; set; }
    }

    public class XeroDTOChild
    {
        public string ReportingCode { get; set; }
        public string ReportingCodeName { get; set; }
        public List<XeroDTO> LstChild { get; set; }
        public XeroDTOChild()
        {
            LstChild = new List<XeroDTO>();
        }
    }

    public class XeroSettingDTO
    {        
        public List<XeroDTOChild> LstXeroDTOChild { get; set; }
        public XeroSettingDTO()
        {
            LstXeroDTOChild = new List<XeroDTOChild>();
        }
    }

    public class XeroSettingModels
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public string CostOfGoodSold { get; set; }
        public string DisplayCostOfGoodSold { get; set; }
        public string StockOnHand { get; set; }
        public string DisplayStockOnHand { get; set; }
        public bool IsPostToVend { get; set; }
        public string Miscellaneous { get; set; }
        public string DisplayMiscellaneous { get; set; }
        public string SendBillAs { get; set; }
        public string DisplaySendBillAs { get; set; }
        public string RoundingError { get; set; }
        public string DisplayRoundingError { get; set; }
        public string DiscountAccount { get; set; }
        public string DisplayDiscountAccount { get; set; }
        public string LoyaltyLiability { get; set; }
        public string DisplayLoyaltyLiability { get; set; }
        public string Loyaltyexpense { get; set; }
        public string DisplayLoyaltyexpense { get; set; }
        public string GCLiability { get; set; }
        public string DisplayGCLiability { get; set; }
        public string Deposit { get; set; }
        public string DisplayDeposit { get; set; }
        public string Payout { get; set; }
        public string DisplayPayout { get; set; }
        public string TillPaymentDiscrepanceis { get; set; }
        public string DisplayTillPaymentDiscrepanceis { get; set; }
        public string CashFloat { get; set; }
        public string DisplayCashFloat { get; set; }
        public string RefundByGC { get; set; }
        public string DisplayRefundByGC { get; set; }
        public string ReturnGCAsCash { get; set; }
        public string DisplayReturnGCAsCash { get; set; }
        public List<SelectListItem> ListXero { get; set; }
        public List<SelectListItem> ListInvoice { get; set; }
        public List<SettingXeroDTO> ListSettingDTO { get; set; }
        public List<XeroDTO> LisXeroDTO { get; set; }
        public XeroSettingModels()
        {
            ListXero = new List<SelectListItem>();
            ListInvoice = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Authorised"), Value = Commons.EInvoiceStatus.Authorised.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Deleted"), Value = Commons.EInvoiceStatus.Deleted.ToString("d")},
                 new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Draft"), Value = Commons.EInvoiceStatus.Draft.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Paid"), Value = Commons.EInvoiceStatus.Paid.ToString("d")},
                 new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Submitted"), Value = Commons.EInvoiceStatus.Submitted.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Voided"), Value = Commons.EInvoiceStatus.Voided.ToString("d")},
            };
            ListSettingDTO = new List<SettingXeroDTO>();
            LisXeroDTO = new List<XeroDTO>();
        }
    }

    public class XeroSettingViewModels
    {
        public string StoreID { get; set; }
        public List<StoreModels> ListItem { get; set; }
        public XeroSettingViewModels()
        {
            ListItem = new List<StoreModels>();
        }
    }

    public class SettingXeroDTO
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string SettingID { get; set; }
        public int Code { get; set; }
        public string Value { get; set; }
        public string DisplayName { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public DateTime LastDateModified { get; set; }
        public string LastUserModified { get; set; }
    }

    public class AccountComboboxModel
    {
        public string text { get; set; }
        public List<AccountChildrenModel> children { get; set; }

        public AccountComboboxModel()
        {
            children = new List<AccountChildrenModel>();
        }
    }

    public class AccountChildrenModel
    {
        public string id { get; set; }
        public string text { get; set; }
    }
}
