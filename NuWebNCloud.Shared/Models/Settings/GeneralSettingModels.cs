using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class GeneralSettingModels
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }

        public string SettingId { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public int Status { get; set; }
        public int Code { get; set; }

        public List<SettingDTO> ListSettings { get; set; }

        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        //===============
        public string ImgBackgroundURL { get; set; }
        public byte[] ImgBackgroundByte { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase ImgBackgroundUpload { get; set; }

        public bool SigInserText { get; set; }
        public string SigMessage { get; set; }

        public string SigImgURL { get; set; }
        public byte[] SigImgByte { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase SigImgUpload { get; set; }
        public bool IsPositionSigImg { get; set; }

        public string ReceiptPrefix { get; set; }
        public bool ResetDailyForReceipt { get; set; }
        public string OrderPrefix { get; set; }
        public bool ResetDailyForOrder { get; set; }
        [_AttributeForLanguage("The Start Number field is required.")]
        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double StartNumber { get; set; }

        public string ReAdvanceForValid { get; set; }
        public string ReAdvanceToRemind { get; set; }
        public string ReBefore { get; set; }
        public string ReHAfter { get; set; }

        public bool AutoPrintGuestCheck { get; set; }
        public bool SingleOrderChit { get; set; }
        public bool TotalOrderChit { get; set; }
        public bool AllowBookingOnline { get; set; }
        public bool PrintReportWhenCloseDrawer { get; set; }
        public bool PrintConsolidated { get; set; }
        public bool PrintAddOnItemsForTotalChit { get; set; }

        public bool ShowValueInCashDrawer { get; set; }
        public bool ShowValueInShift { get; set; }
        public bool ShowValueInStore { get; set; }
        public bool ShowDeletionConfirmationForSentItem { get; set; }
        public bool ShowDeletionConfirmationForWholeOrder { get; set; }
        public bool ShowAllCategories { get; set; }
        public bool MakeTableAvailable { get; set; }        // Updated 08172017

        public bool ShowBarcode { get; set; }               //Show BarCode
        public bool ShowPopUpToPrintReceipt { get; set; }   //Show Pop Up to Print receipt
        public bool PrintReceipt { get; set; }
        public bool IntegrationInclude { get; set; }        //The Sales from Integration Include in Mall Report
                                                            //Delivery Time Setup
        public bool ManualPromotion { get; set; }
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Timeslot must be larger than 0")]
        public int DeliveryTimeSlot { get; set; }
        [_AttributeForLanguage("The Auto Send Order Time field is required.")]
        public int AutoSendOrderTime { get; set; }
        //Time for Auto Send Order (min)
        [_AttributeForLanguage("The Notification Reminder field is required.")]
        public int NotificationReminder { get; set; }
        //Time for Notification Reminder (min)
        [_AttributeForLanguage("The Notification Push Time field is required.")]
        public int NotificationPushTime { get; set; }      //Notification Push Time (min)
        [_AttributeForLanguage("Minium Amount for Delivery Order field is required.")]
        public int MinimumAmountForDeliveryOrder { get; set; }
        [_AttributeForLanguage("After Starting Business field is required.")]
        public int AfterStartingBusiness { get; set; }
        [_AttributeForLanguage("Before Ending Business field is required.")]
        public int BeforeEndingBusiness { get; set; }
        public bool NeedPincodeToConfirmPickupDeliveryNupos { get; set; }
        public bool DeliveryAllowToAutoAccept { get; set; }
        public bool AllowToCreateOnlinePickupOrder { get; set; }
        public bool AllowToCreateOnlineDeliveryOrder { get; set; }
        public bool AllowToCancelDeliveryPickupOrderFromWallet { get; set; }
        public bool ApplyTaxOnDeliveryFee { get; set; }
        public bool PickupNew { get; set; }
        public bool PickupAccepted { get; set; }
        public bool PickupReady { get; set; }
        public bool DeliveryNew { get; set; }
        public bool DeliveryAccepted { get; set; }
        public bool DeliveryReady { get; set; }
        public bool DeliveryDelivering { get; set; }
        public List<DeliverySettingDTO> ListDeliverySetting { get; set; }
        public List<DeliverySettingDTO> ListPostalCode { get; set; }
        public List<DeliverySettingDTO> ListDeliveryFee { get; set; }

        [_AttributeForLanguage("The Time Holding After Payment field is required.")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int TimeHoldingAfterPayment { get; set; }

        public bool NoSaleSetting { get; set; }

        public string FTPFolderReport { get; set; }
        public string FTPUser { get; set; }

        [_AttributeForLanguage("Password is required")]
        [_AttributeForLanguageStringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string FTPPassword { get; set; }
        [_AttributeForLanguage("The Number Of Latest Order field is required.")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int NumberOfLatestOrder { get; set; }       
        [_AttributeForLanguage("The Number Of Days Allow for Refund field is required.")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int NumberofDaysAllowforRefund { get; set; }
        public bool IsSubmitMallReport { get; set; }
        public bool IsNotifySubmitMallReport { get; set; }
        public string MallReportMailTo { get; set; }
        public string MallReportFileNameFormat { get; set; }
        public string MachineID { get; set; }
        public List<SelectListItem> ListMachineID { get; set; }
        public List<SelectListItem> ListDate { get; set; }
        public List<SelectListItem> ListHour { get; set; }
        public List<SelectListItem> ListSeparator { get; set; }
        public string IDNoUse { get; set; }
        public string IDMachineID { get; set; }
        public string IDDate { get; set; }
        public string IDHour { get; set; }
        public string IDSeparator { get; set; }
        public string IDNoUseString { get; set; }
        public string IDMachineIDString { get; set; }
        public string IDDateString { get; set; }
        public string IDHourString { get; set; }
        public string IDSeparatorString { get; set; }
        public List<string> listMallReportFileNameFormat { get; set; }
        //=========================================
        public TimeSpan BreakfastStart { get; set; }
        public TimeSpan BreakfastEnd { get; set; } 
        public TimeSpan LunchStart { get; set; }
        public TimeSpan LunchEnd { get; set; } 
        public TimeSpan DinnerStart { get; set; }
        public TimeSpan DinnerEnd { get; set; }

        //for Invoice Setting
        public string MessageInvoice { get; set; }
        public InvoiceSettingDTO Setting { get; set; }
        public List<InvoiceDTO> ListInvoice { get; set; }
        public string InvoiceID { get; set; }

        [_AttributeForLanguage("Please enter a value greater than or equal to 0")]
        public int PendingTime { get; set; }
        public string ItemBackgroundReady { get; set; }
        public string ItemBackgroundServed { get; set; }
        public bool IsNumOfCellOrderMana { get; set; }
        public string SoundPrepareScreen { get; set; }
        public string SoundServeScreen { get; set; }

        public string SoundPrepareName { get; set; }
        public string SoundServeName { get; set; }
        public List<SelectListItem> ListSounds { get; set; }

        //=====17/07/2018
        public bool AddToTab { get; set; }
        public bool PayToWaiter { get; set; }


        public bool PrintCloseDrawerReport { get; set; }
        public bool PrintEndShiftReport { get; set; }
        public bool PrintEndDayReport { get; set; }
        public bool PrintReceiptCompany { get; set; }
        public bool IncludeShiftData { get; set; }
        public bool IncludeDrawerData { get; set; }
        public bool PrintDayCategorySale { get; set; }
        public bool PrintDayMenuItemSale { get; set; }

        public bool AddExtraPriceToDishSet { get; set; }



        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Timeslot period must be larger than 0")]
        public int TimeslotPeriod { get; set; }
        public int MaximunOfPersonCome { get; set; }
        public int AddOnOfPersonForReservationOrBooking { get; set; }
        public bool AllowDuplicateBooking { get; set; }
        public int NumberofDuplicates { get; set; }
        public string FormatText { get; set; }
        public int ExpiryDate { get; set; }
        public int TimeforNotificationReminder { get; set; }        

        public GeneralSettingModels()
        {
            ListSettings = new List<SettingDTO>();
            ListMachineID = new List<SelectListItem>()
            {
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NoUse"),Value=Commons.EMallFileName.NoUse.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("MerchantID"),Value=Commons.EMallFileName.MerchantID.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date(yyyymmdd)"),Value=Commons.EMallFileName.Date.ToString("d")},
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hour(hhmmss)"),Value=Commons.EMallFileName.Hour.ToString("d")}
            };
            ListDate = new List<SelectListItem>()
            {
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NoUse"),Value=Commons.EMallFileName.NoUse.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("MerchantID"),Value=Commons.EMallFileName.MerchantID.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date(yyyymmdd)"),Value=Commons.EMallFileName.Date.ToString("d")},
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hour(hhmmss)"),Value=Commons.EMallFileName.Hour.ToString("d")}
            };
            ListHour = new List<SelectListItem>()
            {
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NoUse"),Value=Commons.EMallFileName.NoUse.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("MerchantID"),Value=Commons.EMallFileName.MerchantID.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date(yyyymmdd)"),Value=Commons.EMallFileName.Date.ToString("d")},
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hour(hhmmss)"),Value=Commons.EMallFileName.Hour.ToString("d")}
            };
            ListSeparator = new List<SelectListItem>()
            {
                new SelectListItem() {Text="-",Value=Commons.ESeperatorType.Normal.ToString("d") },
                new SelectListItem() {Text="_",Value=Commons.ESeperatorType.Underscore.ToString("d") }
            };
            ListDeliverySetting = new List<DeliverySettingDTO>();
            ListPostalCode = new List<DeliverySettingDTO>();
            ListDeliveryFee = new List<DeliverySettingDTO>();
            ListInvoice = new List<InvoiceDTO>() {
                new InvoiceDTO()
                {
                    ID = "0",
                    Name = "None",
                    IsSelected = false,
                    Setting = new InvoiceSettingDTO(),
                }
            };
            ListSounds = new List<SelectListItem>()
            {
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Prepare"),Value=Commons.ESound.prepare.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Served"),Value=Commons.ESound.served.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Call bill"),Value=Commons.ESound.callBill.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Call service"),Value=Commons.ESound.callService.ToString("d") }
            };
        }
    }

    public class SettingDTO
    {
        public string SettingId { get; set; }
        public string Value { get; set; }
    }

    public class GeneralSettingViewModels
    {
        public string StoreID { get; set; }
        public List<StoreModels> ListItem { get; set; }
        public GeneralSettingViewModels()
        {
            ListItem = new List<StoreModels>();
        }
    }
    public class DeliverySettingDTO
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string ObjectType { get; set; }
        public byte Type { get; set; }
        public byte Status { get; set; }
        public int OffSet { get; set; }
    }
    public class InvoiceDTO
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public InvoiceSettingDTO Setting { get; set; }
    }
    public class InvoiceSettingDTO
    {
        public string PublicImages { get; set; }
        public string Logo { get; set; }
        public string ImageURL { get; set; }
        public byte[] ImgLogoInvoiceByte { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase ImgLogoInvoiceUpload { get; set; }
        public string Title { get; set; }
        public string Title2 { get; set; }
        public string PeriodPrefix { get; set; }
        public string PeriodSuffix { get; set; }
        public string InvoiceFrom { get; set; }
        public string InvoiceTo { get; set; }

        //Edit 4/6/2018 by Tu
        [_AttributeForLanguageStringLength(7, ErrorMessage = "The field not input too 8 characters")]
        //[MaxLength(7, ErrorMessage = "The field not input too 8 characters")]
        public string Headban { get; set; }
        [_AttributeForLanguageStringLength(7, ErrorMessage = "The field not input too 8 characters")]
        //[MaxLength(7, ErrorMessage = "The field not input too 8 characters")]
        public string Branchban { get; set; }
        //--------------------------------------

        public string BusinessID { get; set; }
        public string SellerID { get; set; }
        public string RepresentID { get; set; }
        public List<MonthPrefixDTO> InvoicePrefix { get; set; }
        public byte Status { get; set; }
        public int OffSet { get; set; }
        public InvoiceSettingDTO()
        {
            PublicImages = Commons._PublicImages;
            ImageURL = "";
        }
    }
    public class MonthPrefixDTO
    {
        public int Month { get; set; }
        public string Value { get; set; }
    }
}
