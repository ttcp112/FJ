using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Settings.TransactionReasonSetting
{
    public class ReasonModels
    {
        public string ID { get; set; }

        [_AttributeForLanguage("Reason is required")]
        public string Name { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public bool IsActive { get; set; }
        public byte Code { get; set; }
        public string GLAccountCode { get; set; }
        public List<StoreReasonDTO> ListStore { get; set; }
        public List<SelectListItem> ListStoreView { get; set; }
        public List<StoreOnCompany> ListStoreOnComp { get; set; }
        public List<SelectListItem> ListType { get; set; }

        public ReasonModels()
        {
            ListType = new List<SelectListItem>() {
                new SelectListItem(){Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EReasonCode.VoidItem_Order.ToString()), Value = Commons.EReasonCode.VoidItem_Order.ToString("d")},
                 new SelectListItem(){Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EReasonCode.Refund.ToString()), Value = Commons.EReasonCode.Refund.ToString("d")},
                  new SelectListItem(){Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EReasonCode.NoSale.ToString()), Value = Commons.EReasonCode.NoSale.ToString("d")},
                   new SelectListItem(){Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EReasonCode.Payout.ToString()), Value = Commons.EReasonCode.Payout.ToString("d")},
                    new SelectListItem(){Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EReasonCode.Deposit.ToString()), Value = Commons.EReasonCode.Deposit.ToString("d")},
            };
            ListStoreView = new List<SelectListItem>();
            ListStoreOnComp = new List<StoreOnCompany>();
            ListStore = new List<StoreReasonDTO>();
        }
    }
    public class StoreReasonDTO
    {
        public string ReasonID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public bool IsActive { get; set; }
        public byte Status { get; set; }
        public int OffSet { get; set; }
        public bool IsDelete { get; set; }

    }
    public class StoreOnCompany
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsCompany { get; set; }
        public bool Selected { get; set; }
        public string CompId { get; set; }
        public string CompName { get; set; }
        public bool Disabled { get; set; }
    }
}
