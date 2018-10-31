using NuWebNCloud.Shared.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Linq;
using NuWebNCloud.Shared.Factory;

namespace NuWebNCloud.Shared
{
    public static class Commons
    {

        public static DateTime _ExpiredDate = new DateTime(9999, 12, 31);
        public static DateTime _MinDate = new DateTime(1900, 1, 1, 0, 0, 0);
        public static DateTime _UnlimitedDate = new DateTime(1899, 12, 31, 0, 0, 0);

        public static string _MsgDoesNotMatchFileExcel = "Import file format invalid, please check before importing.";
        public static string _MsgAllowedSizeImg = "Your Photo is too large, maximum allowed size is : 300KB";
        public static int _MaxSizeFileUploadImg = 300000;

        public const string Company = "Company";
        public const string Store = "Store";

        public const int TypeCompanySelected = 1; // 1: Company | 2: Store
        public static DateTime MinDate = new DateTime(1900, 01, 01, 00, 00, 00);

        public const string Html = "HTML";
        public const string Excel = "Excel";
        public const string PDF = "PDF";

        public const string FormatDate = "dd/MM/yyyy";
        public const string FormatTime = "HH:mm";

        public const string OrganizationID = "bdd6c222-f16a-492a-b091-4d4a11092040";
        public const string OrganizationName = "Asian";

        //NSXero
        /// <summary>
        /// Set Value [6aaee02a-5dc0-466a-9352-5e9e279c1fe2] From file nsxro-io.pdf
        /// </summary>
        //public const string NSXeroAppRegistrationId = "6aaee02a-5dc0-466a-9352-5e9e279c1fe2";
        /// <summary>
        /// Set Value [NS-XERO-INTEGRATION] From file nsxro-io.pdf
        /// </summary>
        //public const string NSXeroStoreId = "NS-XERO-INTEGRATION";
        /// <summary>
        /// Set Value [630] From file nsxro-io.pdf
        /// </summary>
        public const string NSXeroIngreInventoryAssetAccountCode = "630";
        /// <summary>
        /// Set Value [310] From file nsxro-io.pdf
        /// </summary>
        public const string NSXeroIngrePDAcoountCode = "310";
        /// <summary>
        /// Set Value [200] From file nsxro-io.pdf
        /// </summary>
        public const string NSXeroIngreSDAcoountCode = "200";

        public static string MerchantName
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["User"] != null)
                    return ((UserSession)System.Web.HttpContext.Current.Session["User"]).OrganizationName;
                else
                    return "";
            }
        }
        public const string EmpDefault = "Employee System";

        public const int CashIn = 1;
        public const int CashOut = 2;
        public const int CreditInvoice = 3;

        public static string PendingStatus = "Pending";
        public static string ReadyStatus = "Ready";
        public static string ServedStatus = "Served";

        public static string RPT_EveryDay = "Every Day";
        public static string RPT_DayOfWeek = "Day Of Week";
        public static string RPT_DayOfMonth = "Day Of Month";

        public static string PromotionDayOfWeek = "Specific days in a week";
        public static string PromotionDayOfMonth = "Specific days in a month";

        public static string MaritalStatusSingle = "Single";
        public static string MaritalStatusMarried = "Married";

        public static string ETaxAddOnTax = "Add on Tax";
        public static string ETaxIncludeTax = "Include Tax in Item Price";

        public static string ModifierForced = "Forced"; //1
        public static string ModifierOptional = "Optional"; //2

        public static string SpendTypeBuyItem = "Buy Item"; //1
        public static string SpendTypeSpendMoney = "Spend Money"; //2

        public static string SpendOnTypeAnyItem = "Any Item"; //1
        public static string SpendOnTypeSpecificItem = "Specific Item";//2
        public static string SpendOnTypeTotalBill = "Total Bill";//2

        public static string EarnTypeSpentItem = "Spent Item"; //2
        public static string EarnTypeSpecificItem = "Specific Item";//3
        public static string EarnTypeTotalBill = "Total Bill";//1

        public static string DiscountPercent = "Discount %";
        public static string DiscountValue = "Discount Value";

        public const int StoreLevel = 3;

        public const bool DiscountValueType_Percent = false;

        public const int DiscountTotalBill = 1;
        public const int DiscountOnItem = 2;
        public const int PromotionTotalBill = 1;
        public const int PromotionOnItem = 2;
        public const int NoDiscount = 0;

        public const int ItemType_Dish = 1;
        public const int ItemType_SetMenu = 4;

        public const string Image100_100 = "https://dummyimage.com/100x100";
        public const string Image200_100 = "https://dummyimage.com/200x100";

        //public const int TopSell_DayByAmount = 1;
        //public const int TopSell_DayByQty = 2;
        //public const int TopSell_MonthByAmount = 3;
        //public const int TopSell_MonthByQty = 4;
        //public const int TopSell_YearByAmount = 5;
        //public const int TopSell_YearByQty = 6;

        public const string BgColorHeader = "#d9d9d9";
        public const string BgColorDataRow = "#d9d9d9";
        public const string BgColorStore = "#eeece1";

        /*Icon Left Menu*/
        public const string IconMenuReport = "<i class=\"fa fa-bar-chart-o\"></i>";
        public const string IconMenuInventory = "<i class=\"fa fa-cube\"></i>";
        public const string IconMenuSandBox = "<i class=\"fa fa-shopping-cart\"></i>";
        public const string IconMenuSettings = "<i class=\"fa fa-cogs\"></i>";
        public const string IconMenuAccessControl = "<i class=\"fa fa-users\"></i>";

        public static string[] _daysOfWeek = new string[7];
        public static string[] _monthNames = new string[12];

        public static bool _RememberMe = false;
        public static string _LanguageId = "";

        //=============API
        /*Config AppSettings*/
        public static string CreateUser
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["User"] != null)
                    return ((UserSession)System.Web.HttpContext.Current.Session["User"]).UserId;
                else
                    return "Admin";
            }
        }
        public static string AppKey
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["User"] != null)
                    return ((UserSession)System.Web.HttpContext.Current.Session["User"]).AppKey;
                else
                    return "";
            }
        }
        public static string AppSecret
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["User"] != null)
                    return ((UserSession)System.Web.HttpContext.Current.Session["User"]).AppSecret;
                else
                    return "";
            }
        }
        //public static string AppKey = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AppKey"]) ? "" : ConfigurationManager.AppSettings["AppKey"];
        // public static string AppSecret = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AppSecret"]) ? "" : ConfigurationManager.AppSettings["AppSecret"];

        public static string BreakfastStart = string.IsNullOrEmpty(ConfigurationManager.AppSettings["BreakfastStart"]) ? "" : ConfigurationManager.AppSettings["BreakfastStart"];
        public static string BreakfastEnd = string.IsNullOrEmpty(ConfigurationManager.AppSettings["BreakfastEnd"]) ? "" : ConfigurationManager.AppSettings["BreakfastEnd"];
        public static string LunchStart = string.IsNullOrEmpty(ConfigurationManager.AppSettings["LunchStart"]) ? "" : ConfigurationManager.AppSettings["LunchStart"];
        public static string LunchEnd = string.IsNullOrEmpty(ConfigurationManager.AppSettings["LunchEnd"]) ? "" : ConfigurationManager.AppSettings["LunchEnd"];
        public static string DinnerStart = string.IsNullOrEmpty(ConfigurationManager.AppSettings["DinnerStart"]) ? "" : ConfigurationManager.AppSettings["DinnerStart"];
        public static string DinnerEnd = string.IsNullOrEmpty(ConfigurationManager.AppSettings["DinnerEnd"]) ? "" : ConfigurationManager.AppSettings["DinnerEnd"];

        public static string _ftpHost = string.IsNullOrEmpty(ConfigurationManager.AppSettings["FTPHost"]) ? "" : ConfigurationManager.AppSettings["FTPHost"];
        public static string _userName = string.IsNullOrEmpty(ConfigurationManager.AppSettings["FTPUser"]) ? "" : ConfigurationManager.AppSettings["FTPUser"];
        public static string _password = string.IsNullOrEmpty(ConfigurationManager.AppSettings["FTPPassword"]) ? "" : ConfigurationManager.AppSettings["FTPPassword"];
        public static string _PublicImages = string.IsNullOrEmpty(ConfigurationManager.AppSettings["PublicImages"]) ? "" : ConfigurationManager.AppSettings["PublicImages"];

        public static string _hostApi = string.IsNullOrEmpty(ConfigurationManager.AppSettings["HostPosAPI"]) ? "" : ConfigurationManager.AppSettings["HostPosAPI"];

        
        public static string _XeroCurrencyCode =  "USD" ;

        /*End Config AppSettings*/
        public static string BREAKFAST = "BREAKFAST";
        public static string LUNCH = "LUNCH";
        public static string DINNER = "DINNER";

        public const string BgColorGroup = "#C6EFCE";
        //Organization
        //=================================================================
        public const string _GetOrganization = "api/v1/Organization/Get";
        //=================================================================
        //Company
        public const string GetDetailCompany = "api/v1/Company/Get/Web";
        public const string UpdateCompany = "api/v1/Company/CreateOrEdit";

        //=================================================================
        public const string GetCompany = "api/v1/Company/Get/Web";
        //=================================================================
        //Store
        //=================================================================
        public const string GetStore = "api/v1/Store/Get/Web";
        public const string GetStores = "api/v1/Store/Get";
        public const string CreateOrEditStore = "api/v1/Store/CreateOrEdit";
        public const string DeleteStore = "api/v1/Store/Delete";
        public const string ExportStore = "api/v1/Store/Export";
        public const string Store_Get_Web2 = "api/v1/Store/Get/Web2";
        //Industry
        //=================================================================
        public const string GetIndustry = "api/v1/Industry/Get";

        //Employee
        //=================================================================
        //public const string Login = "api/v1/Employee/LogIn";
        public const string Login = "api/v1/Employee/LogInWeb";
        public const string LoginExtend = "api/UserApi/LoginExtend";
        public const string GetAllEmployee = "api/v1/Employee/Get/Web";

        public const string GetEmployee = "api/v1/Employee/Get";
        public const string CreateOrEditEmployee = "api/v1/Employee/CreateOrEdit/Web";//"api/v1/Employee/CreateOrEdit";
        public const string DeleteEmployee = "api/v1/Employee/Delete";
        public const string ImportEmployee = "api/v1/Employee/Import";
        public const string ExportEmployee = "api/v1/Employee/Export";
        public const string ChangePassword = "api/v1/Employee/ChangePassword";
        //Customer
        //=================================================================
        public const string GetCustomer = "api/v1/Customer/Get";
        public const string CreateOrEditCustomer = "api/v1/Customer/CreateOrEdit";
        public const string DeleteCustomer = "api/v1/Customer/Delete";
        public const string ImportCustomer = "api/v1/Customer/Import";
        public const string ExportCustomer = "api/v1/Customer/Export";

        //Season
        //=================================================================
        public const string GetSeason = "api/v1/Season/Get";
        public const string CreateOrEditSeason = "api/v1/Season/CreateOrEdit";
        public const string DeleteSeason = "api/v1/Season/Delete";
        public const string ImportSeason = "api/v1/Season/Import";
        public const string ExportSeason = "api/v1/Season/Export";

        //Role
        public const string GetRole = "api/v1/Role/Get";
        public const string CreateOrEditRole = "api/v1/Role/CreateOrEdit";
        public const string DeleteRole = "api/v1/Role/Delete";

        //Module
        public const string GetModule = "api/v1/Module/Get";

        //Drawer
        public const string GetDrawer = "api/v1/Drawer/Get";
        public const string CreateOrEditDrawer = "api/v1/Drawer/CreateOrEdit";
        public const string DeleteDrawer = "api/v1/Drawer/Delete";

        //Table
        public const string GetTable = "api/v1/Table/Get";
        public const string CreateOrEditTable = "api/v1/Table/CreateOrEdit";
        public const string DeleteTable = "api/v1/Table/Delete";
        public const string ImportTable = "api/v1/Table/Import";
        public const string ExportTable = "api/v1/Table/Export";

        //Tax
        public const string GetTax = "api/v1/Tax/Get";
        public const string CreateOrEditTax = "api/v1/Tax/CreateOrEdit";
        public const string Tax_InsertOrUpdate_Web = "api/v1/Tax/CreateOrEdit/Web";
        public const string DeleteTax = "api/v1/Tax/Delete";
        //Ver3 -v2
        public const string GetTax_V2 = "api/v2/Tax/Get";
        //public const string Tax_InsertOrUpdate_Web_V2 = "api/v2/Tax/CreateOrEdit";
        public const string Tax_InsertOrUpdate_Web_V2 = "api/v2/Tax/CreateOrEdit/Web"; // Update 07092018, Api for web
        public const string DeleteTax_V2 = "api/v2/Tax/Delete";
        public const string Check_Product_Tax = "api/v2/Tax/Check/Product";

        public const string ImportTax = "api/v1/Tax/Import";
        public const string ExportTax = "api/v1/Tax/Export";
        //End Tax

        //PaymentMethod
        public const string GetPaymentMethod = "api/v1/PaymentMethod/Get";
        public const string GetPaymentMethodForWeb = "api/v1/PaymentMethod/GetForWeb";
        public const string CreateOrEditPaymentMethod = "api/v1/PaymentMethod/CreateOrEdit";
        public const string DeletePaymentMethod = "api/v1/PaymentMethod/Delete";
        public const string ImportPaymentMethod = "api/v1/PaymentMethod/Import";
        public const string ExportPaymentMethod = "api/v1/PaymentMethod/Export";

        //Currency
        public const string GetCurrency = "api/v1/Currency/Get";
        public const string CreateOrEditCurrency = "api/v1/Currency/CreateOrEdit";
        public const string DeleteCurrency = "api/v1/Currency/Delete";
        public const string ImportCurrency = "api/v1/Currency/Import";
        public const string ExportCurrency = "api/v1/Currency/Export";

        //GeneralSetting
        public const string GetGeneralSettings = "api/v1/Setting/GetGeneralSettings";
        public const string SaveGeneralSetting = "api/v1/Setting/SaveGeneralSetting";
        public const string GetServiceCharge = "api/v1/Setting/GetServiceCharge";
        public const string UpdateServiceCharge = "api/v1/Setting/UpdateServiceCharge";

        //MerchantSettings
        public const string GetMerchantSettings_WalletGet = "api/v1/Setting/MultiStoreOrderOnWallet/Get";
        public const string SaveMerchantSetting_WalletSave = "api/v1/Setting/MultiStoreOrderOnWallet/Save";
        public const string GetMerchantSettings_PaymentGet = "api/v1/Setting/StoreReceivesAllPayment/Get";
        public const string SaveMerchantSetting_PaymentSave = "api/v1/Setting/StoreReceivesAllPayment/Save";

        //GetFollowRegion
        public const string GetFollowRegion = "api/v1/Setting/GetFollowRegion";

        //Setup Delivery
        public const string SettingDeliveryCreateOrEdit = "api/v1/Setting/CreateOrEditDelivery";
        public const string SettingDeliveryGet = "api/v1/Setting/GetDelivery";

        //Invoice Setting
        public const string SettingGetOtherPrintSetting = "api/v1/Setting/OtherPrintSetting";
        public const string SettingOtherPrintSettingSave = "api/v1/Setting/OtherPrintSetting/Save";

        // Transaction Reason Settings
        public const string GetListTransactionReasonSettings = "api/v1/Reason/Get";
        public const string GetDetailTransactionReasonSettings = "api/v1/Reason/GetDetail/Web";
        public const string CreateOrUpdateTransactionReasonSettings = "api/v1/Reason/CreateOrEdit/Web";
        public const string DeleteTransactionReasonSettings = "api/v1/Reason/Delete";
        // Employee Add Company
        public const string EmployeeAddCompany = "api/v1/Employee/AddCompany";

        //++++SandBox
        //=================================================================
        //Categories
        //=================================================================
        public const string GetProductType = "api/v1/ProductType/Get";
        public const string GetCategoryReport = "api/v1/Category/GetFilterForWeb";
        //Zone
        //=================================================================
        public const string GetZones = "api/v1/Zone/Get";
        public const string CreateOrEditZones = "api/v1/Zone/CreateOrEdit";
        public const string DeleteZones = "api/v1/Zone/Delete";
        //Categories
        //=================================================================
        public const string GetCategories = "api/v1/Category/Get";
        public const string CreateOrEditCategories = "api/v1/Category/CreateOrEdit";
        public const string DeleteCategories = "api/v1/Category/Delete";
        //Discount
        //=================================================================
        public const string GetDiscount = "api/v1/Discount/Get";
        public const string CreateOrEditDiscount = "api/v1/Discount/CreateOrEdit";
        public const string DeleteDiscount = "api/v1/Discount/Delete";
        public const string ImportDiscount = "api/v1/Discount/Import";
        public const string ExportDiscount = "api/v1/Discount/Export";
        //Product - SetMenu/Modifier/Dish
        //=================================================================
        public const string GetProduct = "api/v1/Product/Get/Inventory";
        public const string GetProductDetail = "api/v1/Product/Get/ID";
        public const string CreateOrEditProduct = "api/v1/Product/CreateOrEdit";
        public const string DeleteProduct = "api/v1/Product/Delete";
        public const string ImportProduct = "api/v1/Product/Import";
        public const string ExportProduct = "api/v1/Product/Export";
        public const string GetSetMenuFilterForWeb = "api/v1/Product/GetSetMenuFilterForWeb";

        //Printer
        public const string GetPrinter = "api/v1/Printer/Get";

        //Promtion
        public const string GetListPromotion = "api/v1/Promotion/GetList";
        public const string GetPromotion = "api/v1/Promotion/Get";
        public const string CreateOrEditPromotion = "api/v1/Promotion/CreateOrEdit";
        public const string DeletePromotion = "api/v1/Promotion/Delete";
        public const string GetListCategoryPromotion = "api/v1/Promotion/GetListCategory";
        public const string CreateOrEditCategoryPromotion = "api/v1/Promotion/CreateOrEditCategory";
        public const string DeleteCategoryPromotion = "api/v1/Promotion/DeleteCategory";
        public const string ImportPromotion = "api/v1/Promotion/Import";
        public const string ExportPromotion = "api/v1/Promotion/Export";
        //++++End SandBox

        //===============SchedulerTack Report
        public const string Report_TimeClockSummary = "TimeClockSummary";
        public const string Report_ItemizeSalesAnalysis = "ItemizeSalesAnalysis";
        public const string Report_InventoryReport = "InventoryReport";
        public const string Report_HourlySales = "HourlySales";
        public const string Report_DailySale = "DailySale";
        public const string Report_ClosedReceipt = "ClosedReceipt";

        public const string BusinessDay_Get_History = "api/v1/BusinessDay/Get/History";

        public static string Version = string.Format("Version {0}", new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime);

        //RSVP Product Mapping
        //=================================================================
        public const string GetListRSVP = "api/v1/Product/GetListProductMapping";
        public const string CreateOrEditRSVP = "api/v1/Product/CreateOrEditProductMapping";
        public const string CloneRSVP = "api/v1/Product/CloneProductMapping";

        //=================Tax============================
        public const string Tax_Get = "api/v1/Tax/Get";
        public static List<string> ListStoreIds { get; set; }
        public static bool IsAction { get; set; }
        public static bool IsShowNotAuthorized { get; set; }
        public static string Controller { get; set; }
        public static string Action { get; set; }
        public static bool IsNotAuthorized { get; set; }


        #region===========API Integration For Project FJ
        //Employee
        //=================================================================
        public const string InteLogin = "api/v1/Employee/LogInWeb";
        public const string InteGetAllEmployee = "api/v1/Employee/Get/Web";
        public const string InteGetEmployee = "api/v1/Employee/Get";
        public const string InteCreateOrEditEmployee = "api/v1/Employee/CreateOrEdit/Web";
        public const string InteDeleteEmployee = "api/v1/Employee/Delete";
        public const string InteImportEmployee = "api/v1/Employee/Import";
        public const string InteExportEmployee = "api/v1/Employee/Export";
        //Categories
        //=================================================================
        public const string InteGetCategories = "api/v1/Category/Get/Web";
        public const string InteCreateOrEditCategories = "api/v1/Category/CreateOrEdit/Web";
        public const string InteDeleteCategories = "api/v1/Category/Delete/Web";
        public const string InteGetCategoryForProduct = "api/v1/Category/Get/Product";
        public const string InteImportCategories = "api/v1/Category/Import";
        public const string InteExportCategories = "api/v1/Category/Export";

        //Extend Product for Ver3 

        public const string ProductExtend = "api/v1/Product/Extend";
        public const string CategoryExtend = "api/v1/Category/Extend";

        //Product - SetMenu/Modifier/Dish
        //=================================================================
        public const string InteGetProduct = "api/v1/Product/Get/Web"; /*PonS*/
        public const string InteGetProductApplyStore = "api/v1/Product/Get/Apply"; /*PonS*/
        public const string InteGetProductDetail = "api/v1/Product/Get/ID/Web";
        public const string InteCreateOrEditProduct = "api/v1/Product/CreateOrEdit/Web";
        public const string InteDeleteProduct = "api/v1/Product/Delete/Web";
        public const string InteImportProduct = "api/v1/Product/Import";
        public const string InteExportProduct = "api/v1/Product/Export";
        public const string InteGetSetMenuFilterForWeb = "api/v1/Product/GetSetMenuFilterForWeb";

        //Merchant extend
        public const string GetStoresExtend = "api/v1/Store/Get/Extend";
        #endregion===========End API Integration For Project FJ

        //======================= chatting =============================
        public const string ChattingCreateOrEdit = "api/v1/ChattingTemplate/CreateOrEdit";
        public const string ChattingDelete = "api/v1/ChattingTemplate/Delete";
        public const string ChattingGet = "api/v1/ChattingTemplate/Get";
        public const string ChattingImport = "api/v1/ChattingTemplate/Import";
        public const string ChattingExport = "api/v1/ChattingTemplate/Export";

        //Register
        public const string RegisterAccount = "api/v1/Merchant/SpecCode";
        public const string GetRegisterAccount = "api/v1/Merchant/SpecCode/Get";

        //Screen Saver Mode 
        public const string KioskImageCreateOrEdit = "api/v1/Kiosk/Image/CreateOrEdit/TV";
        public const string KioskImageGetList = "api/v1/Kiosk/Image/Get";
        public const string KioskImageDelete = "api/v1/Kiosk/Image/Delete";

        #region Xero
        public static string XeroURL = ConfigurationManager.AppSettings["XeroIntergrationURL"];
        public static bool IsXeroIngredient = bool.Parse(ConfigurationManager.AppSettings["XeroIntegrated"] == null ? "false" : ConfigurationManager.AppSettings["XeroIntegrated"]);
        public static string XeroRegistrationAppId = ConfigurationManager.AppSettings["XeroRegistrationAppId"];
        public static string XeroAccessToken = ConfigurationManager.AppSettings["XeroAccessToken"];
        public static string XeroCompanyKey = ConfigurationManager.AppSettings["XeroCompanyKey"];
        public static string XeroStoreId = ConfigurationManager.AppSettings["XeroStoreId"];

        public const string AccountCode_Inventory = "630";
        public const string AccountCode_COGS = "310";
        public const string AccountCode_200 = "200";

        public const string XeroApi_InsertInventory = "api/v1/xeroprivateapp/insertinventoryitem";
        public const string XeroApi_GetInventory = "api/v1/xeroprivateapp/getinventoryitems";
        public const string XeroApi_UpdateInventory = "api/v1/xeroprivateapp/updateinventoryquantity";
        public const string XeroApi_GetTax = "api/v1/ns-xro/private/taxrates";
        public const string XeroApi_GetSetting = "api/v1/ns-xro/private/accounts";
        public const string XeroApi_Generate_Invoice = "api/v1/ns-xro/private/generate_invoice";
        public const string XeroApi_PO_Insert_Update = "api/v1/ns-xro/private/purchaseorder/insert_update";
        public const string XeroApi_InsertOrUpdateSupplier = "api/v1/ns-xro/private/contact/insert_update";
        public const string XeroApi_GetContacts = "api/v1/ns-xro/private/contacts";
        public const string XeroApi_Ingredient_Insert_Update = "api/v1/ns-xro/ingredient/insert_update";
        public const string XeroApi_Sales_Item = "api/v1/ns-xro/nuweb/sales_items";

        
        #endregion End xero
        public const string Language_Get = "api/v1/get";
        #region Languages

        #endregion End languages
        //Enum
        public enum EReasonCode
        {
            VoidItem_Order = 1,
            Refund = 2,
            NoSale = 3,
            Deposit = 4,
            Payout = 5
        }
        public enum EOrderStatus
        {
            Hold = 0,
            Occuppied = 2,
            CalledForBill = 3,
            Paid = 4
        }
        public enum ETableStatus
        {
            Hold = 0,
            Available = 1,
            Occuppied = 2,
            CalledForBill = 3,
            Paid = 4,
            CalledForSevice = 5,
            GuestCheck = 6
        }

        public enum EReceiptNoteStatus
        {
            Approve = 1,
            UnApprove = 0,

            Closed = 2,
            Return = 3
        }

        public enum EGender
        {
            Male = 1,
            Female = 0
        }

        public enum EStatus
        {
            Actived = 1,
            Deleted = 9,
            InActived = 3,
            Demo = 2,
            Refund = 4
        }

        public enum EProductType
        {
            SetMenu = 4,
            Modifier = 2,
            Dish = 1,
            Misc = 9,
            Discount = 5,
            SpecialModifier = 10,
            Promotion = 6
        }

        public enum ESendItem
        {
            NotSend = 0,
            IsSend = 1,
        }

        public enum EValueType
        {
            Percent = 0,
            Currency = 1,
        }

        public enum ETax
        {
            TaxExempt = 1,
            Inclusive = 2,
            AddOn = 3,
        }

        public enum ETaxType
        {
            Normal = 0,
            Xero = 1,
        }

        public enum EItemState
        {
            PendingStatus = 1,
            CompleteStatus = 2,
            ServedStatus = 3
        }

        public enum EModifierType
        {
            Product = 0,
            Forced = 1,
            Optional = 2,
            AdditionalModifier = 3,
            Special = 4,
            AdditionalDish = 5
        }

        public enum ERepeatType
        {
            EveryDay = 1,
            DayOfWeek = 2,
            DayOfMonth = 3
        }

        public enum ETableStyle
        {
            Circle = 0,
            Rectangle = 1,
            Square = 2,
            Other = 3
        }

        public enum ESetting
        {
            HomePage = 1,
            Signature = 2,
            OrderPrefix = 3,
            ReceiptPrefix = 4,
            TimeValidReservation = 5,
            TimeRemidCustomerForReservation = 6,
            TimeHoldReservedTable = 7,
            RoundingMethod = 8,
            AutoPrintGuestCheck = 9,
            SingleOrderChit = 10,
            TotalOrderChit = 11,
            AllowBookingOnline = 12,
            CloseDrawerPrintReport = 22, /*13*/
            PrintConsolidated = 14,
            ShowValueInCashDrawer = 15,
            ShowValueInStore = 16,
            TimeHoldingAfterPayment = 17,
            EndShiftPrintReport = 18,
            PushData = 19,
            PrintItemTotalChit = 20,
            PrintDirect = 21,
            PrintCloseDrawerReport = 22,
            PrintEndShiftReport = 23,
            Email = 24,
            Password = 25,
            StartNumber = 26,
            ShowDeleteConfirmSendItem = 27,
            ShowDeleteConfirmOrder = 28,
            ShowAllCategory = 29,
            MakeTableAvailable = 53, // Updated 08172017
            NoSale = 30,
            ShowValueInShift = 31,
            /** FTP - report **/
            FTPFolderReport = 36,
            FTPUser = 37,
            FTPPassword = 38,
            IsSubmitMallReport = 39,
            MallReportMailTo = 40,
            MallReportFileNameFormat = 41,
            MachineID = 42,
            IsNotifySubmitMallReport = 43,
            NumberOfLatestOrder = 44,

            ShowBarcode = 33,
            ShowPopUpToPrintReceipt = 49,
            IntegrationInclude = 48,

            AutoSendOrderTime = 45,
            NotificationReminder = 46,
            NotificationPushTime = 47,

            ManualPromotion = 58,
            BreakfastStart = 59,
            BreakfastEnd = 60,
            LunchStart = 61,
            LunchEnd = 62,
            DinnerStart = 63,
            DinnerEnd = 64,

            MinimumAmountForDeliveryOrder = 74,
            AfterStartingBusiness = 75,
            BeforeEndingBusiness = 76,
            NeedPincodeToConfirmPickupDeliveryNupos = 77,
            AllowToCreateOnlinePickupOrder = 78,
            AllowToCreateOnlineDeliveryOrder = 79,
            AllowToCancelDeliveryPickupOrderFromWallet = 80,
            ApplyTaxOnDeliveryFee = 81,

            PickupNew = 82,
            PickupAccepted = 83,
            PickupReady = 84,
            DeliveryNew = 85,
            DeliveryAccepted = 86,
            DeliveryReady = 87,
            DeliveryDelivering = 88,

            PrintReceipt = 94,

            PrintTaxCode = 95,
            PrintSummaryTax = 96,
            PrintCustomerClaimTax = 97,

            Region = 99,

            PrintRoundingAmount = 100,

            ItemBackgroundReady = 114,
            ItemBackgroundServed = 115,
            NumOfCellOrderMana = 116,
            PendingTime = 117,
            SoundPrepareScreen = 54,
            SoundServeScreen = 55,
            PrintDayCategorySale = 90,
            PrintDayMenuItemSale = 91,
            AddToTab = 101,
            PayToWaiter = 102,
            TimeSlot = 103,
            MaxNoOfPerson = 104,
            AddOnCoverNo = 105,
            DuplicateBooking = 106,
            NumberOfDuplicates = 107,
            LabelText = 108,
            LabelExpired = 109,
            LabelTimeNoti = 110,
            DeliveryAllowToAutoAccept = 112,
            DeliveryTimeSlot = 118,
            NumberofDaysAllowforRefund = 119,

            PrintEndDayReport = 52,
            PrintReceiptCompany = 120,
            IncludeShiftData = 121,
            IncludeDrawerData = 122,
            AddExtraPriceToDishSet = 127,
        }
        public enum EDeliverySettingType
        {
            PostalCode = 0,
            Cancel = 1,
            Fee = 2
        }

        public enum EMallFileName
        {
            NoUse = 0,
            MerchantID = 1,
            Date = 2,
            Hour = 3
        }
        public enum ESeperatorType
        {
            Normal = 0,
            Underscore = 1
        }
        public enum ProductPrinterType
        {
            Normal = 0,
            Label = 1
        }

        public enum EModuleCode
        {
            POS = 100,
            POS_Dishes = 101,
            POS_SetMenu = 102,
            POS_Discount = 103,
            POS_Promotions = 104,
            POS_Misc = 105,
            POS_GiftCard = 106,
            POS_Pay = 107,
            POS_Orders = 108,
            POS_KickDrawer = 109,
            POS_GuestCheck = 110,
            POS_Table_Pay = 111,
            POS_Split_Merge = 112,
            Reservation = 200,
            OrderManagement = 300,
            OrderMana_Prepare = 301,
            OrderMana_Serve = 302,
            Employees = 400,
            Customers = 500,
            Customers_Membership = 501,
            Customers_Non_Membership = 502,
            CashManagement = 600,
            CashMana_History = 601,
            CashMana_Shift = 602,
            CashMana_Store = 603,
            QueueManagement = 700,
            Activities = 800,
            TimeClock = 900,
            Inventory = 1000,
            Inventory_Dishes = 1001,
            Inventory_SetMenus = 1002,
            Inventory_Modifiers = 1003,
            Inventory_Discounts = 1004,
            Inventory_Promotions = 1005,
            Inventory_GiftCard = 1006,
            Settings = 1100,
            Settings_DetailInfo = 1101,
            Settings_SoftwareInfo = 1102,
            Settings_Themes = 1103,
            Settings_Ipad = 1104,
            Settings_General = 1105,
            Settings_Currency = 1106,
            Settings_PaymentMethod = 1107,
            Settings_Tips_And_ServiceChage = 1108,
            Settings_Timeslot = 1109,
            Settings_CashDrawer = 1110,
            Settings_Membership = 1111,
            Settings_RoleSetting = 1112,
            Settings_Printer = 1113,
            Settings_CancelOrderReason = 1114,
            Settings_TableList = 1115,
            Settings_RedeemRules = 1116,
            Settings_EarnRules = 1117,
            Settings_Taxes = 1118,
            Settings_Store = 1119,
            Settings_VoidItemReason = 1120,
            Settings_TableMap = 1121,
            PrinterManagement = 1200
        }

        public enum ESpendType  /* Type of spend Rule */
        {
            BuyItem = 1,
            SpendMoney = 2,
        }
        public enum ESpendOnType /* Spend on Type */
        {
            //AnyItem = 1,
            //SpecificItem = 2,
            //Category = 3,
            AnyItem = 3,
            SpecificItem = 4,
            Category = 5,
            TotalBill = 6,
        }
        public enum EEarnType /* Earn rule apply on type */
        {
            TotalBill = 1,
            SpentItem = 2,
            SpecificItem = 3,
            Category = 4,
        }

        public enum EMappingType
        {
            RSVP = 1
        }
        public enum EChatTemplate
        {
            Artiste = 1,
            Customer = 2,
        }
        public enum ESound
        {
            prepare = 0,
            served = 1,
            callBill = 2,
            callService = 3,
        }

        #region Ingredient
        public enum EPOStatus
        {
            Open = 1,
            Approved = 2,
            InProgress = 3,
            Closed = 4
        }
        public enum EStockCountStatus
        {
            Open = 1,
            Approved = 2,
        }

        public enum ETableZipCode
        {
            PurchaseOrder = 1,
            ReceiptNote = 2,
            ReturnNote = 3,
            StockTransfer = 4,
            DataEntry = 5,
            Allocation = 6,
            Sale = 7,
            StockCount = 8,
            ReceiptNoteSelfMade = 9,
            WorkOrder = 10,
        }
        public enum EStockStatus
        {
            Normal = 1,
            StockEmpty = 2,
            LowStock = 3
        }
        public enum EPaymentCode
        {
            Cash = 1,
            GiftCard = 4
        }

        public enum EMerchantSetting
        {
            AllowMultiStoreOrderOnWallet = 111,
            DeliveryAllowToAutoAccept = 112,
            StoreReceivesAllPayment = 113
        }
        public enum EThirdPartyType
        {
            Delivery = 0,
            Payment = 1,
            DeliveryTracking = 2,
            Xero = 3,
        }
        #endregion

        // For FJ Daily Sales Report of Stall #14
        public const string Stall14StoreCode = "ORP14";
        public static string Stall14StoreId;
        public static string Stall14StoreName;
        public const string Stall14GLAccountCode1 = "LIQUOR-SALES";
        public const string Stall14GLAccountCode2 = "RSVP-CARD";
        public const string TENANT_SALES = "TENANT-SALES";
        public static string ServerExportPath { get; set; }

        //enum for Filter report 2018-05-25
        #region Rule for report filter time 
        //BD1 start 17 9h am - 18/5 10h am,
        //th1: chọn date range 17-18, time range default (0-0): show full data của BD1
        //-> chart show data on column 17 (full data)
        //th2: chọn date range 17-17, time range default : show full data của BD1
        //-> chart show data on column 17 (full data)
        //th3: chọn date range 18-18, time range default : not show, chart not show
        //th4: chọn date range 17-18 hay 17-17, time range 9-10h: show data của BD1 gồm 9-10h 17/5 +  9-10h 18/5
        //time range 0-2h: show data của BD1 ngày 18/5 từ 0-2h
        //time range 9-8h: show data của BD1 từ 17/5 9h đến 18/5 8h

        //còn BD2 start 17 9h am - 19/5 10h am,
        //th1: chọn date range 17-17 hay 17-18 hay 17-19/5, time range default thì đều show full BD2
        //th2: chọn date range 17-17 hay 17-18 hay 17-19/5, time range 9-10h: show data của BD1 gồm 9-10h 17/5 +  9-10h 18/5 + 9-10h 19/5
        //time range 0-2h: show data của BD1 ngày 18/5 từ 0-2h + 19/5 0-2h
        //time range 9-8h: show data của BD1 từ(17/5 9h đến 18/5 8h) + (18/5 9h đến 18/5 8h)
        #endregion 
        /// <summary>
        ///  None = 0 (DateFrom min | DateTo max -> default), OnDay = 1, PassDay = 2
        /// </summary>
        public enum EFilterType
        {
            None = 0,
            OnDay = 1,
            Days = 2
        }

        //Enum for INVOICE
        public enum EInvoiceType
        {
            AccountsPayable = 0,
            AccountsReceivable = 1
        }
        //Enum for INVOICETYPE
        public enum EXEROInvoiceType
        {
            ACCREC = 0,
            ACCPAY = 1
        }
        public enum ELineAmountType
        {
            Exclusive = 0,
            Inclusive = 1,
            NoTax = 2
        }
        public enum EInvoiceStatus
        {
            Draft = 0,
            Submitted = 1,
            Deleted = 2,
            Authorised = 3,
            Paid = 4,
            Voided = 5
        }
        //End Enum for INVOICE

        public enum EPurchaseOrderStatuses
        {
            Draft = 0,
            Submitted = 1,
            Authorised = 2,
            Billed = 3,
            Deleted = 4
        }

        //enum for xero
        public enum EGeneralSetting
        {
            //business date setting
            CostOfGoodSold = 100,
            StockOnHand = 101,
            PostToVend = 102,
            Miscellaneous = 103,
            //inventory order and account sales
            SendBillAs = 104,
            //accounts for payments
            RoundingError = 105,
            DiscountAccount = 106,
            //Accounts for liability and expiry
            LoyaltyLiability = 107,
            Loyaltyexpense = 108,
            GCLiability = 109,
            //Accounts for cash management
            Deposit = 110,
            Payout = 111,
            TillPaymentDiscrepanceis = 112,
            CashFloat = 113,
            RefundByGC = 114,
            ReturnGCAsCash = 115

        }

        public static bool isIntegrateXero(string storeId)
        {
            if (System.Web.HttpContext.Current.Session["User"] != null)
                return ((UserSession)System.Web.HttpContext.Current.Session["User"]).listStore.Any(a => a.ThirdParty != null
                        && storeId == a.ThirdParty.StoreID && a.ThirdParty.IsIntegrate);
            return false;
        }

        public static bool isIntegrateXero(List<string> lstStoreIds)
        {
            if (System.Web.HttpContext.Current.Session["User"] != null)
                return ((UserSession)System.Web.HttpContext.Current.Session["User"]).listStore.Any(a => a.ThirdParty != null
                        && lstStoreIds.Contains(a.ThirdParty.StoreID) && a.ThirdParty.IsIntegrate);
            return false;
        }

        public static TerminalDTO GetIntegrateInfo(string storeId)
        {
            if (System.Web.HttpContext.Current.Session["User"] != null)
                return ((UserSession)System.Web.HttpContext.Current.Session["User"]).listStore
                    .Where(ww => ww.ID == storeId && ww.ThirdParty != null && ww.ThirdParty.IsIntegrate).Select(s => s.ThirdParty).FirstOrDefault();
            return null;
        }

        public static TerminalDTO GetIntegrateInfoWithComId(string companyId)
        {
            var _storeFactory = new StoreFactory();
            var listStoresInfoSession = _storeFactory.GetListStoresInfo();
            if (listStoresInfoSession != null && listStoresInfoSession.Any())
            {
                var lstStoreIds = listStoresInfoSession.Where(w => w.CompanyId == companyId).Select(s => s.Id).ToList();
                if (System.Web.HttpContext.Current.Session["User"] != null)
                    return ((UserSession)System.Web.HttpContext.Current.Session["User"]).listStore
                        .Where(ww => lstStoreIds.Contains(ww.ID) && ww.ThirdParty != null && ww.ThirdParty.IsIntegrate).Select(s => s.ThirdParty).FirstOrDefault();
            }
            return null;
        }

    }
}
