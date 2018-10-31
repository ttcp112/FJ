using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SGeneralSettingController : HQController
    {
        private GeneralSettingFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SGeneralSettingController()
        {
            _factory = new GeneralSettingFactory();
            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                GeneralSettingViewModels model = new GeneralSettingViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("GeneralSetting_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(GeneralSettingViewModels model)
        {
            try
            {
                var listStore = GetListStores();
                listStore = listStore.OrderBy(x => x.CompanyName).ToList();
                foreach (var item in listStore)
                {
                    model.ListItem.Add(new StoreModels
                    {
                        Name = item.Name,
                        Id = item.Id,
                        CompanyName = item.CompanyName,
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("GeneralSetting_Search : " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }

        public GeneralSettingModels GetDetail(string StoreID)
        {
            try
             {
                GeneralSettingModels model = new GeneralSettingModels();
                model.StoreID = StoreID;

                #region Setup Delivery for Ver3
                if (CurrentUser.POSInstanceVersion.HasValue)
                {
                    var ListDeliverySetting = _factory.GetListDeliverySetting(StoreID, null);
                    if (ListDeliverySetting.Count != 0)
                    {
                        model.ListPostalCode = ListDeliverySetting.Where(o => o.Type == (byte)Commons.EDeliverySettingType.PostalCode).ToList();
                        if (model.ListPostalCode.Count != 0)
                        {
                            int index = 0;
                            model.ListPostalCode.ForEach(o =>
                            {
                                o.OffSet = index++;
                            });
                        }
                        model.ListDeliveryFee = ListDeliverySetting.Where(o => o.Type == (byte)Commons.EDeliverySettingType.Fee).ToList();
                        if (model.ListDeliveryFee.Count != 0)
                        {
                            int index = 0;
                            model.ListDeliveryFee.ForEach(o =>
                            {
                                o.OffSet = index++;
                            });
                        }
                    }
                }
                #endregion

                #region For Invoice Setting
                var InvoiceSetting = _factory.GetInvoicePrintSetting(StoreID, null);
                if (InvoiceSetting != null)
                {                    
                    model.ListInvoice.AddRange(InvoiceSetting.ListInvoice);
                    model.InvoiceID = model.ListInvoice.Where(o => o.IsSelected).Select(s => s.ID).FirstOrDefault();
                    var temp = model.ListInvoice.Where(w => w.IsSelected).FirstOrDefault();
                    string url = "";
                    if (temp != null && temp.Setting != null &&  temp.Setting.Logo != null)
                    {
                        url = temp.Setting.Logo;
                    }
                    for (int i = 0; i < model.ListInvoice.Count; i++)
                    {
                        if (i == 0 && temp != null)
                        {
                            model.ListInvoice[i].Setting.ImageURL = model.ListInvoice[i].Setting.PublicImages + url;
                        }
                        if (model.ListInvoice[i].Setting != null && model.ListInvoice[i].Setting.Logo != null)
                        {                                
                            model.ListInvoice[i].Setting.ImageURL = model.ListInvoice[i].Setting.PublicImages + model.ListInvoice[i].Setting.Logo;
                        }
                    } 
                }
                #endregion

                string StoreName = "";
                var listSettings = _factory.GetListGeneralSetting(ref StoreName, StoreID, null);
                model.StoreName = StoreName;
                if (listSettings.Count != 0)
                {
                    string[] listVal = new string[] { };
                    //Home Page Background
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.HomePage).FirstOrDefault() != null)
                    {
                        model.ImgBackgroundURL = listSettings.Where(x => x.Code == (byte)Commons.ESetting.HomePage).FirstOrDefault().Value;
                    }                    

                    //Signature
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.Signature).FirstOrDefault() != null)
                    {
                        listVal = listSettings.Where(x => x.Code == (byte)Commons.ESetting.Signature).FirstOrDefault().Value.Split('-');
                        if (listVal.Length >= 3)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (i == 0)
                                    model.SigInserText = listVal[i].Equals("1") ? true : false;
                                else if (i == 1)
                                    model.IsPositionSigImg = listVal[i].Equals("0") ? true : false;
                                else if (i == 2)
                                    model.SigMessage = listVal[i];
                                else if (i >= 3)
                                {
                                    if (listVal.Length == 8)
                                    {
                                        string ImageName = listVal[3] + "-" + listVal[4] + "-" + listVal[5] + "-" + listVal[6] + "-" + listVal[7];
                                        model.SigImgURL = /*Commons._PublicImages +*/ ImageName;
                                    }
                                }
                            }
                        }
                    }                                     

                    //Check Prefix
                    var ReceiptPrefix = listSettings.Where(x => x.Code == (byte)Commons.ESetting.ReceiptPrefix).FirstOrDefault();
                    if (ReceiptPrefix != null)
                    {
                        listVal = ReceiptPrefix.Value.Split('-');
                        for (int i = 0; i < listVal.Length; i++)
                        {
                            if (i == 0)
                                model.ReceiptPrefix = listVal[i];
                            else if (i == 1)
                                model.ResetDailyForReceipt = GetBoolean(listVal[i]);
                        }
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.OrderPrefix).FirstOrDefault() != null)
                    {
                        listVal = listSettings.Where(x => x.Code == (byte)Commons.ESetting.OrderPrefix).FirstOrDefault().Value.Split('-');
                        for (int i = 0; i < listVal.Length; i++)
                        {
                            if (i == 0)
                                model.OrderPrefix = listVal[i];
                            else if (i == 1)
                                model.ResetDailyForOrder = GetBoolean(listVal[i]);
                        }
                    }
                    
                    //Start Number
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.StartNumber).FirstOrDefault() != null)
                    {
                        model.StartNumber = double.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.StartNumber).FirstOrDefault().Value);
                    }                    
                    
                    //Reservation
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeHoldReservedTable).FirstOrDefault() != null)
                    {
                        listVal = listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeHoldReservedTable).FirstOrDefault().Value.Split('-');
                        for (int i = 0; i < listVal.Length; i++)
                        {
                            if (i == 0)
                                model.ReBefore = listVal[i];
                            else if (i == 1)
                                model.ReHAfter = listVal[i];
                        }
                    }                    
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeRemidCustomerForReservation).FirstOrDefault() != null)
                    {
                        model.ReAdvanceToRemind = listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeRemidCustomerForReservation).FirstOrDefault().Value;
                    }                    
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeValidReservation).FirstOrDefault() != null)
                    {
                        model.ReAdvanceForValid = listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeValidReservation).FirstOrDefault().Value;
                    }                    
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeHoldingAfterPayment).FirstOrDefault() != null)
                    {
                        model.TimeHoldingAfterPayment = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeHoldingAfterPayment).FirstOrDefault().Value);
                    }

                    //Update 07062017                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AutoSendOrderTime).FirstOrDefault() != null)
                    {
                        model.AutoSendOrderTime = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AutoSendOrderTime).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NotificationReminder).FirstOrDefault() != null)
                    {
                        model.NotificationReminder = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.NotificationReminder).FirstOrDefault().Value);
                    }
                   
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NotificationPushTime).FirstOrDefault() != null)
                    {
                        model.NotificationPushTime = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.NotificationPushTime).FirstOrDefault().Value);
                    }                   

                    //Update 03122018-Tu
                    if (CurrentUser.POSInstanceVersion.HasValue)
                    {
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.MinimumAmountForDeliveryOrder).FirstOrDefault() != null)
                        {
                            model.MinimumAmountForDeliveryOrder = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.MinimumAmountForDeliveryOrder).FirstOrDefault().Value);
                        }
                        
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AfterStartingBusiness).FirstOrDefault() != null)
                        {
                            model.AfterStartingBusiness = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AfterStartingBusiness).FirstOrDefault().Value);
                        }
                        
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.BeforeEndingBusiness).FirstOrDefault() != null)
                        {
                            model.BeforeEndingBusiness = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.BeforeEndingBusiness).FirstOrDefault().Value);
                        }
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeSlot).FirstOrDefault() != null)
                        {
                            model.DeliveryTimeSlot = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryTimeSlot).FirstOrDefault().Value);
                        }
                        //-------------------------
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NeedPincodeToConfirmPickupDeliveryNupos).FirstOrDefault() != null)
                        {
                            model.NeedPincodeToConfirmPickupDeliveryNupos = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.NeedPincodeToConfirmPickupDeliveryNupos).FirstOrDefault().Value);
                        }

                        if (listSettings.Where(x => x.Code == (byte)
                        Commons.ESetting.DeliveryAllowToAutoAccept).FirstOrDefault() != null)
                        {
                            model.DeliveryAllowToAutoAccept = GetBoolean(listSettings.Where(x => x.Code == (byte)
                        Commons.ESetting.DeliveryAllowToAutoAccept).FirstOrDefault().Value);
                        }

                        if (listSettings.Where(x => x.Code == (byte)
                        Commons.ESetting.AllowToCreateOnlinePickupOrder).FirstOrDefault() != null)
                        {
                            model.AllowToCreateOnlinePickupOrder = GetBoolean(listSettings.Where(x => x.Code == (byte)
                        Commons.ESetting.AllowToCreateOnlinePickupOrder).FirstOrDefault().Value);
                        }
                        
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AllowToCreateOnlineDeliveryOrder).FirstOrDefault() != null)
                        {
                            model.AllowToCreateOnlineDeliveryOrder = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AllowToCreateOnlineDeliveryOrder).FirstOrDefault().Value);
                        }
                        
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ApplyTaxOnDeliveryFee).FirstOrDefault() != null)
                        {
                            model.ApplyTaxOnDeliveryFee = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ApplyTaxOnDeliveryFee).FirstOrDefault().Value);
                        }
                        
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AllowToCancelDeliveryPickupOrderFromWallet).FirstOrDefault() != null)
                        {
                            model.AllowToCancelDeliveryPickupOrderFromWallet = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AllowToCancelDeliveryPickupOrderFromWallet).FirstOrDefault().Value);
                        }
                        
                        if (model.AllowToCreateOnlineDeliveryOrder == true)
                        {
                            //for Pickup Order
                            if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PickupNew).FirstOrDefault() != null)
                            {
                                model.PickupNew = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PickupNew).FirstOrDefault().Value);
                            }
                            
                            if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PickupAccepted).FirstOrDefault() != null)
                            {
                                model.PickupAccepted = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PickupAccepted).FirstOrDefault().Value);
                            }
                            
                            if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PickupReady).FirstOrDefault() != null)
                            {
                                model.PickupReady = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PickupReady).FirstOrDefault().Value);
                            }

                            //for Delivery Order
                            if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryNew).FirstOrDefault() != null)
                            {
                                model.DeliveryNew = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryNew).FirstOrDefault().Value);
                            }
                            
                            if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryAccepted).FirstOrDefault() != null)
                            {
                                model.DeliveryAccepted = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryAccepted).FirstOrDefault().Value);
                            }
                            
                            if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryReady).FirstOrDefault() != null)
                            {
                                model.DeliveryReady = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryReady).FirstOrDefault().Value);
                            }
                            
                            if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryDelivering).FirstOrDefault() != null)
                            {
                                model.DeliveryDelivering = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.DeliveryDelivering).FirstOrDefault().Value);
                            }                            
                        }
                    }
                    //=========================
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AutoPrintGuestCheck).FirstOrDefault() != null)
                    {
                        model.AutoPrintGuestCheck = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AutoPrintGuestCheck).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.SingleOrderChit).FirstOrDefault() != null)
                    {
                        model.SingleOrderChit = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.SingleOrderChit).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.TotalOrderChit).FirstOrDefault() != null)
                    {
                        model.TotalOrderChit = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.TotalOrderChit).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AllowBookingOnline).FirstOrDefault() != null)
                    {
                        model.AllowBookingOnline = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AllowBookingOnline).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.CloseDrawerPrintReport).FirstOrDefault() != null)
                    {
                        model.PrintReportWhenCloseDrawer = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.CloseDrawerPrintReport).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintConsolidated).FirstOrDefault() != null)
                    {
                        model.PrintConsolidated = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintConsolidated).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintItemTotalChit).FirstOrDefault() != null)
                    {
                        model.PrintAddOnItemsForTotalChit = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintItemTotalChit).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowValueInCashDrawer).FirstOrDefault() != null)
                    {
                        model.ShowValueInCashDrawer = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowValueInCashDrawer).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowValueInShift).FirstOrDefault() != null)
                    {
                        model.ShowValueInShift = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowValueInShift).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowValueInStore).FirstOrDefault() != null)
                    {
                        model.ShowValueInStore = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowValueInStore).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowDeleteConfirmSendItem).FirstOrDefault() != null)
                    {
                        model.ShowDeletionConfirmationForSentItem = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowDeleteConfirmSendItem).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowDeleteConfirmOrder).FirstOrDefault() != null)
                    {
                        model.ShowDeletionConfirmationForWholeOrder = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowDeleteConfirmOrder).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowAllCategory).FirstOrDefault() != null)
                    {
                        model.ShowAllCategories = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowAllCategory).FirstOrDefault().Value);
                    }

                    // Updated 08172017
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.MakeTableAvailable).FirstOrDefault() != null)
                    {
                        model.MakeTableAvailable = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.MakeTableAvailable).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NoSale).FirstOrDefault() != null)
                    {
                        model.NoSaleSetting = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.NoSale).FirstOrDefault().Value);
                    }

                    //===Update 07062017
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowBarcode).FirstOrDefault() != null)
                    {
                        model.ShowBarcode = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowBarcode).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowPopUpToPrintReceipt).FirstOrDefault() != null)
                    {
                        model.ShowPopUpToPrintReceipt = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ShowPopUpToPrintReceipt).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintReceipt).FirstOrDefault() != null)
                    {
                        model.PrintReceipt = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintReceipt).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintReceiptCompany).FirstOrDefault() != null)
                    {
                        model.PrintReceiptCompany = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintReceiptCompany).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintDayMenuItemSale).FirstOrDefault() != null)
                    {
                        model.PrintDayMenuItemSale = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintDayMenuItemSale).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintDayCategorySale).FirstOrDefault() != null)
                    {
                        model.PrintDayCategorySale = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintDayCategorySale).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.IncludeShiftData).FirstOrDefault() != null)
                    {
                        model.IncludeShiftData = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.IncludeShiftData).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.IncludeDrawerData).FirstOrDefault() != null)
                    {
                        model.IncludeDrawerData = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.IncludeDrawerData).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintEndShiftReport).FirstOrDefault() != null)
                    {
                        model.PrintEndShiftReport = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintEndShiftReport).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintEndDayReport).FirstOrDefault() != null)
                    {
                        model.PrintEndDayReport = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PrintEndDayReport).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.IntegrationInclude).FirstOrDefault() != null)
                    {
                        model.IntegrationInclude = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.IntegrationInclude).FirstOrDefault().Value);
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ManualPromotion).FirstOrDefault() != null)
                    {
                        model.ManualPromotion = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.ManualPromotion).FirstOrDefault().Value);
                    }
                    
                    //-------------------------------------------------
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.FTPFolderReport).FirstOrDefault() != null)
                    {
                        model.FTPFolderReport = listSettings.Where(x => x.Code == (byte)Commons.ESetting.FTPFolderReport).FirstOrDefault().Value;
                    }                   
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.FTPUser).FirstOrDefault() != null)
                    {
                        model.FTPUser = listSettings.Where(x => x.Code == (byte)Commons.ESetting.FTPUser).FirstOrDefault().Value;
                    }                    
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.FTPPassword).FirstOrDefault() != null)
                    {
                        model.FTPPassword = listSettings.Where(x => x.Code == (byte)Commons.ESetting.FTPPassword).FirstOrDefault().Value;
                    }                    
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumberOfLatestOrder).FirstOrDefault() != null)
                    {
                        model.NumberOfLatestOrder = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumberOfLatestOrder).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumberofDaysAllowforRefund).FirstOrDefault() != null)
                    {
                        model.NumberofDaysAllowforRefund = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumberofDaysAllowforRefund).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.IsSubmitMallReport).FirstOrDefault() != null)
                    {
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.IsSubmitMallReport).FirstOrDefault().Value.ToLower() == "true")
                        {
                            model.IsSubmitMallReport = true;
                        }
                        else model.IsSubmitMallReport = false;
                    }                    
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.IsNotifySubmitMallReport).FirstOrDefault() != null)
                    {
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.IsNotifySubmitMallReport).FirstOrDefault().Value.ToLower() == "true")
                        {
                            model.IsNotifySubmitMallReport = true;
                        }
                        else model.IsNotifySubmitMallReport = false;
                    }                    
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.MallReportMailTo).FirstOrDefault() != null)
                    {
                        string[] ArrayMallReportMailTo = listSettings.Where(x => x.Code == (byte)Commons.ESetting.MallReportMailTo).FirstOrDefault().Value.Split(',');
                        string resultKQ = "";

                        if (ArrayMallReportMailTo.Count() > 1)
                        {
                            for (int i = 0; i < ArrayMallReportMailTo.Length; i++)
                            {
                                ArrayMallReportMailTo[i] = "\"" + ArrayMallReportMailTo[i] + "\"";
                                resultKQ = resultKQ + ArrayMallReportMailTo[i] + ",";
                            }
                            resultKQ = resultKQ.Substring(0, resultKQ.Length - 1);
                            resultKQ = "[" + resultKQ + "]";
                            model.MallReportMailTo = resultKQ.ToString();
                        }
                        else
                        {
                            if (ArrayMallReportMailTo.Count() == 1)
                            {
                                if (ArrayMallReportMailTo[0] == "[")
                                {
                                    model.MallReportMailTo = "";
                                }
                                else
                                {
                                    if (ArrayMallReportMailTo[0] == " ")
                                    {
                                        model.MallReportMailTo = null;
                                    }
                                    if (ArrayMallReportMailTo[0] == "")
                                    {
                                        model.MallReportMailTo = null;
                                    }
                                    else
                                    {
                                        model.MallReportMailTo = ArrayMallReportMailTo[0];
                                        model.MallReportMailTo = "[\"" + model.MallReportMailTo + "\"]";
                                    }
                                }
                            }
                        }
                    }
                    
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.MallReportFileNameFormat).FirstOrDefault() != null)
                    {
                        string[] ArrayMallReportFileNameFormat = listSettings.Where(x => x.Code == (byte)Commons.ESetting.MallReportFileNameFormat).FirstOrDefault().Value.Split('_');
                        int kq0 = Convert.ToInt32(ArrayMallReportFileNameFormat[0]);
                        switch (kq0)
                        {
                            case 0:
                                model.IDMachineID = "0";
                                model.IDMachineIDString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NoUse");
                                break;
                            case 1:
                                model.IDMachineID = "1";
                                model.IDMachineIDString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("MachineID");
                                break;
                            case 2:
                                model.IDMachineID = "2";
                                model.IDMachineIDString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date(yyyymmdd)");
                                break;
                            case 3:
                                model.IDMachineID = "3";
                                model.IDMachineIDString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hour(hhmmss)");
                                break;
                        }
                        int kq1 = Convert.ToInt32(ArrayMallReportFileNameFormat[1]);
                        switch (kq1)
                        {
                            case 0:
                                model.IDDate = "0";
                                model.IDDateString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NoUse");
                                break;
                            case 1:
                                model.IDDate = "1";
                                model.IDDateString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("MachineID");
                                break;
                            case 2:
                                model.IDDate = "2";
                                model.IDDateString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date(yyyymmdd)");
                                break;
                            case 3:
                                model.IDDate = "3";
                                model.IDDateString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hour(hhmmss)");
                                break;
                        }
                        int kq2 = Convert.ToInt32(ArrayMallReportFileNameFormat[2]);
                        switch (kq2)
                        {
                            case 0:
                                model.IDHour = "0";
                                model.IDHourString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("NoUse");
                                break;
                            case 1:
                                model.IDHour = "1";
                                model.IDHourString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("MachineID");
                                break;
                            case 2:
                                model.IDHour = "2";
                                model.IDHourString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date(yyyymmdd)");
                                break;
                            case 3:
                                model.IDHour = "3";
                                model.IDHourString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hour(hhmmss)");
                                break;
                        }
                        int kq3 = Convert.ToInt32(ArrayMallReportFileNameFormat[3]);
                        switch (kq3)
                        {
                            case 0:
                                model.IDSeparator = "0";
                                model.IDSeparatorString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Separator(-)");
                                break;
                            case 1:
                                model.IDSeparator = "1";
                                model.IDSeparatorString = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Separator(_)");
                                break;
                        }
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.MachineID).FirstOrDefault() != null)
                    {
                        model.MachineID = listSettings.Where(x => x.Code == (byte)Commons.ESetting.MachineID).FirstOrDefault().Value;
                    }

                    //==Order Management Setting*
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumberOfLatestOrder).FirstOrDefault() != null)
                    {
                        model.PendingTime = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumberOfLatestOrder).FirstOrDefault().Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.SoundPrepareScreen).FirstOrDefault() != null)
                    {
                        model.SoundPrepareScreen = listSettings.Where(x => x.Code == (byte)Commons.ESetting.SoundPrepareScreen).FirstOrDefault().Value;
                        int val = Convert.ToInt32(model.SoundPrepareScreen);
                        switch (val)
                        {
                            case (byte)Commons.ESound.prepare:
                                model.SoundPrepareName = Commons.ESound.prepare.ToString();
                                break;
                            case (byte)Commons.ESound.served:
                                model.SoundPrepareName = Commons.ESound.served.ToString();
                                break;
                            case (byte)Commons.ESound.callBill:
                                model.SoundPrepareName = Commons.ESound.callBill.ToString();
                                break;
                            case (byte)Commons.ESound.callService:
                                model.SoundPrepareName = Commons.ESound.callService.ToString();
                                break;
                        }
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.SoundServeScreen).FirstOrDefault() != null)
                    {
                        model.SoundServeScreen = listSettings.Where(x => x.Code == (byte)Commons.ESetting.SoundServeScreen).FirstOrDefault().Value;
                        int val = Convert.ToInt32(model.SoundServeScreen);
                        switch (val)
                        {
                            case (byte)Commons.ESound.prepare:
                                model.SoundServeName = Commons.ESound.prepare.ToString();
                                break;
                            case (byte)Commons.ESound.served:
                                model.SoundServeName = Commons.ESound.served.ToString();
                                break;
                            case (byte)Commons.ESound.callBill:
                                model.SoundServeName = Commons.ESound.callBill.ToString();
                                break;
                            case (byte)Commons.ESound.callService:
                                model.SoundServeName = Commons.ESound.callService.ToString();
                                break;
                        }
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ItemBackgroundReady).FirstOrDefault() != null)
                    {
                        model.ItemBackgroundReady = listSettings.Where(x => x.Code == (byte)Commons.ESetting.ItemBackgroundReady).FirstOrDefault().Value;
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.ItemBackgroundServed).FirstOrDefault() != null)
                    {
                        model.ItemBackgroundServed = listSettings.Where(x => x.Code == (byte)Commons.ESetting.ItemBackgroundServed).FirstOrDefault().Value;
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumOfCellOrderMana).FirstOrDefault() != null)
                    {
                        if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumOfCellOrderMana).FirstOrDefault().Value.ToLower() == "3")
                        {
                            model.IsNumOfCellOrderMana = true;
                        }
                        else model.IsNumOfCellOrderMana = false;
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PendingTime).FirstOrDefault() != null)
                    {
                        model.PendingTime = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PendingTime).FirstOrDefault().Value);
                    }
                    //==Time Configuration in Reports*
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.BreakfastStart).FirstOrDefault() != null && !string.IsNullOrEmpty(listSettings.Where(x => x.Code == (byte)Commons.ESetting.BreakfastStart).FirstOrDefault().Value))
                    {
                        model.BreakfastStart = _factory.ConvertStringToTimeSpan(listSettings.Where(x => x.Code == (byte)Commons.ESetting.BreakfastStart).FirstOrDefault().Value);
                    }

                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.BreakfastEnd).FirstOrDefault() != null)
                    {
                        model.BreakfastEnd = _factory.ConvertStringToTimeSpan(listSettings.Where(x => x.Code == (byte)Commons.ESetting.BreakfastEnd).FirstOrDefault().Value);//TimeSpan.Parse(BreakfastEnd.Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.LunchStart).FirstOrDefault() != null)
                    {
                        model.LunchStart = _factory.ConvertStringToTimeSpan(listSettings.Where(x => x.Code == (byte)Commons.ESetting.LunchStart).FirstOrDefault().Value);//TimeSpan.Parse(LunchStart.Value);
                    }

                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.LunchEnd).FirstOrDefault() != null)
                    {
                        model.LunchEnd = _factory.ConvertStringToTimeSpan(listSettings.Where(x => x.Code == (byte)Commons.ESetting.LunchEnd).FirstOrDefault().Value); ;//TimeSpan.Parse(LunchEnd.Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.DinnerStart).FirstOrDefault() != null)
                    {
                        model.DinnerStart = _factory.ConvertStringToTimeSpan(listSettings.Where(x => x.Code == (byte)Commons.ESetting.DinnerStart).FirstOrDefault().Value); ;//TimeSpan.Parse(DinnerStart.Value);
                    }
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.DinnerEnd).FirstOrDefault() != null)
                    {
                        model.DinnerEnd = _factory.ConvertStringToTimeSpan(listSettings.Where(x => x.Code == (byte)Commons.ESetting.DinnerEnd).FirstOrDefault().Value); //TimeSpan.Parse(DinnerEnd.Value);
                    }
                    // At Reservation group
                    //Timeslot period
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeSlot).FirstOrDefault() != null)
                    {
                        model.TimeslotPeriod = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.TimeSlot).FirstOrDefault().Value);
                    }
                    //Maximum Of Person(s) come
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.MaxNoOfPerson).FirstOrDefault() != null)
                    {
                        model.MaximunOfPersonCome = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.MaxNoOfPerson).FirstOrDefault().Value);
                    }
                    //Add on [] of persons for reservation/booking
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AddOnCoverNo).FirstOrDefault() != null)
                    {
                        model.AddOnOfPersonForReservationOrBooking = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AddOnCoverNo).FirstOrDefault().Value);
                    }
                    //Allow Duplicate Booking
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.DuplicateBooking).FirstOrDefault() != null)
                    {
                        model.AllowDuplicateBooking = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.DuplicateBooking).FirstOrDefault().Value);
                    }
                    //Number of Duplicates
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumberOfDuplicates).FirstOrDefault() != null)
                    {
                        model.NumberofDuplicates = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.NumberOfDuplicates).FirstOrDefault().Value);
                    }
                    //Bottle Service
                    //Text to Show at Label Format
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.LabelText).FirstOrDefault() != null)
                    {
                        model.FormatText = listSettings.Where(x => x.Code == (byte)Commons.ESetting.LabelText).FirstOrDefault().Value;
                    }
                    //Expiry Date
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.LabelExpired).FirstOrDefault() != null)
                    {
                        model.ExpiryDate = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.LabelExpired).FirstOrDefault().Value);
                    }
                    //Time for Notification Reminder
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.LabelTimeNoti).FirstOrDefault() != null)
                    {
                        model.TimeforNotificationReminder = int.Parse(listSettings.Where(x => x.Code == (byte)Commons.ESetting.LabelTimeNoti).FirstOrDefault().Value);
                    }
                    //============
                    // Set up Payment options when making order 
                    // Add to Tab
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AddToTab).FirstOrDefault() != null)
                    {
                        model.AddToTab = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AddToTab).FirstOrDefault().Value);
                    }
                    // Pay to Waiter
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.PayToWaiter).FirstOrDefault() != null)
                    {
                        model.PayToWaiter = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.PayToWaiter).FirstOrDefault().Value);
                    }

                    // Add Extra Price to Dish/Set
                    if (listSettings.Where(x => x.Code == (byte)Commons.ESetting.AddExtraPriceToDishSet).FirstOrDefault() != null)
                    {
                        model.AddExtraPriceToDishSet = GetBoolean(listSettings.Where(x => x.Code == (byte)Commons.ESetting.AddExtraPriceToDishSet).FirstOrDefault().Value);
                    }
                    //=========
                }
                return model;
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GeneralSetting_Detail Error : " , ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string StoreID)
        {
            GeneralSettingModels model = GetDetail(StoreID);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string StoreID)
        {
            GeneralSettingModels model = GetDetail(StoreID);
            return PartialView("_Edit", model);
        }

        [HttpPost] 
        public ActionResult Edit(GeneralSettingModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (int.Parse(string.IsNullOrEmpty(model.ReAdvanceForValid) ? "0" : model.ReAdvanceForValid.ToString()) < 0)
                {
                    ModelState.AddModelError("ReAdvanceForValid", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                }
                if (int.Parse(string.IsNullOrEmpty(model.ReAdvanceToRemind) ? "0" : model.ReAdvanceToRemind.ToString()) < 0)
                {
                    ModelState.AddModelError("ReAdvanceToRemind", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                }
                if (int.Parse(string.IsNullOrEmpty(model.ReBefore) ? "0" : model.ReBefore.ToString()) < 0)
                {
                    ModelState.AddModelError("ReBefore", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                }
                if (int.Parse(string.IsNullOrEmpty(model.ReHAfter) ? "0" : model.ReHAfter.ToString()) < 0)
                {
                    ModelState.AddModelError("ReHAfter", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                }
                //if (int.Parse(model.TimeHoldingAfterPayment.ToString()) < 0)
                //{
                //    ModelState.AddModelError("TimeHoldingAfterPayment", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                //}

                //==========
                List<SettingDTO> ListSettings = new List<SettingDTO>();

                if (!string.IsNullOrEmpty(model.ImgBackgroundURL))
                {
                    model.ImgBackgroundURL = model.ImgBackgroundURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                }
                // Backgrounp Image
                if (model.ImgBackgroundUpload != null && model.ImgBackgroundUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.ImgBackgroundUpload.ContentLength];
                    model.ImgBackgroundUpload.InputStream.Read(imgByte, 0, model.ImgBackgroundUpload.ContentLength);
                    model.ImgBackgroundByte = imgByte;
                    model.ImgBackgroundURL = Guid.NewGuid() + Path.GetExtension(model.ImgBackgroundUpload.FileName);
                    model.ImgBackgroundUpload = null;
                }
                string HomePage = model.ImgBackgroundURL;
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.HomePage.ToString("d"),
                    Value = HomePage
                });

                //====Signature
                //Sign Image
                if (!string.IsNullOrEmpty(model.SigImgURL))
                {
                    model.SigImgURL = model.SigImgURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                }
                if (model.SigImgUpload != null && model.SigImgUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.SigImgUpload.ContentLength];
                    model.SigImgUpload.InputStream.Read(imgByte, 0, model.SigImgUpload.ContentLength);
                    model.SigImgByte = imgByte;
                    model.SigImgURL = Guid.NewGuid() + Path.GetExtension(model.SigImgUpload.FileName);
                    model.SigImgUpload = null;
                }
                string SigInserText = model.SigInserText ? "1" : "0";
                string SigImgPosition = model.IsPositionSigImg ? "0" : "1";
                string SigMessage = model.SigMessage;
                string SigImgURL = model.SigImgURL;
                string SignatureValue = SigInserText + "-" + SigImgPosition + "-" + SigMessage + "-" + SigImgURL;
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.Signature.ToString("d"),
                    Value = SignatureValue
                });

                //=======Check Prefix
                string ReceiptPre = model.ReceiptPrefix;
                string ResetDailyForReceipt = model.ResetDailyForReceipt ? "1" : "0";
                string ReceiptPrefix = ReceiptPre + "-" + ResetDailyForReceipt;
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ReceiptPrefix.ToString("d"),
                    Value = ReceiptPrefix
                });
                string OrderPre = model.OrderPrefix;
                string ResetDailyForOrder = model.ResetDailyForOrder ? "1" : "0";
                string OrderPrefix = OrderPre + "-" + ResetDailyForOrder;
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.OrderPrefix.ToString("d"),
                    Value = OrderPrefix
                });

                //=======Start Number
                double StartNumber = model.StartNumber;
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.StartNumber.ToString("d"),
                    Value = StartNumber.ToString()
                });

                //=========Reservation
                string ReBefore = model.ReBefore;
                string ReHAfter = model.ReHAfter;
                string TimeHoldReservedTable = ReBefore + "-" + ReHAfter;
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.TimeHoldReservedTable.ToString("d"),
                    Value = TimeHoldReservedTable
                });

                string ReAdvanceToRemind = model.ReAdvanceToRemind;
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.TimeRemidCustomerForReservation.ToString("d"),
                    Value = ReAdvanceToRemind
                });

                string ReAdvanceForValid = model.ReAdvanceForValid;
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.TimeValidReservation.ToString("d"),
                    Value = ReAdvanceForValid
                });

                string TimeHoldingAfterPayment = model.TimeHoldingAfterPayment.ToString();
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.TimeHoldingAfterPayment.ToString("d"),
                    Value = TimeHoldingAfterPayment
                });

                string AutoSendOrderTime = model.AutoSendOrderTime.ToString();
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.AutoSendOrderTime.ToString("d"),
                    Value = AutoSendOrderTime
                });

                string NotificationReminder = model.NotificationReminder.ToString();
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.NotificationReminder.ToString("d"),
                    Value = NotificationReminder
                });

                string NotificationPushTime = model.NotificationPushTime.ToString();
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.NotificationPushTime.ToString("d"),
                    Value = NotificationPushTime
                });

                if (CurrentUser.POSInstanceVersion.HasValue)
                {
                    string MinimumAmountForDeliveryOrder = model.MinimumAmountForDeliveryOrder.ToString();
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.MinimumAmountForDeliveryOrder.ToString("d"),
                        Value = MinimumAmountForDeliveryOrder
                    });

                    string AfterStartingBusiness = model.AfterStartingBusiness.ToString();
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.AfterStartingBusiness.ToString("d"),
                        Value = AfterStartingBusiness
                    });

                    string BeforeEndingBusiness = model.BeforeEndingBusiness.ToString();
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.BeforeEndingBusiness.ToString("d"),
                        Value = BeforeEndingBusiness
                    });

                    string DeliveryTimeSlot = model.DeliveryTimeSlot.ToString();
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.DeliveryTimeSlot.ToString("d"),
                        Value = DeliveryTimeSlot
                    });

                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.NeedPincodeToConfirmPickupDeliveryNupos.ToString("d"),
                        Value = model.NeedPincodeToConfirmPickupDeliveryNupos.ToString().ToLower()
                    });

                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.DeliveryAllowToAutoAccept.ToString("d"),
                        Value = model.DeliveryAllowToAutoAccept.ToString().ToLower()
                    });

                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.AllowToCreateOnlinePickupOrder.ToString("d"),
                        Value = model.AllowToCreateOnlinePickupOrder.ToString().ToLower()
                    });

                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.AllowToCreateOnlineDeliveryOrder.ToString("d"),
                        Value = model.AllowToCreateOnlineDeliveryOrder.ToString().ToLower()
                    });

                    //if (model.AllowToCreateOnlineDeliveryOrder)
                    //{
                        if (model.ListPostalCode != null && model.ListPostalCode.Any())
                        {
                            //model.ListPostalCode = model.ListPostalCode.Where(o => o.Status != 9).ToList();

                            if (model.ListPostalCode.Count > 0)
                            {
                                model.ListPostalCode.ForEach(o =>
                                {
                                    o.Type = (byte)Commons.EDeliverySettingType.PostalCode;
                                    o.StoreID = model.StoreID;
                                });
                                model.ListDeliverySetting.AddRange(model.ListPostalCode);
                            }
                        }
                    //}

                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.ApplyTaxOnDeliveryFee.ToString("d"),
                        Value = model.ApplyTaxOnDeliveryFee.ToString().ToLower()
                    });

                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.AllowToCancelDeliveryPickupOrderFromWallet.ToString("d"),
                        Value = model.AllowToCancelDeliveryPickupOrderFromWallet.ToString().ToLower()
                    });

                    //if (model.AllowToCancelDeliveryPickupOrderFromWallet)
                    //{
                        //For Deivery Pickup Order
                        ListSettings.Add(new SettingDTO
                        {
                            SettingId = Commons.ESetting.PickupNew.ToString("d"),
                            Value = model.PickupNew.ToString().ToLower()
                        });
                        ListSettings.Add(new SettingDTO
                        {
                            SettingId = Commons.ESetting.PickupAccepted.ToString("d"),
                            Value = model.PickupAccepted.ToString().ToLower()
                        });
                        ListSettings.Add(new SettingDTO
                        {
                            SettingId = Commons.ESetting.PickupReady.ToString("d"),
                            Value = model.PickupReady.ToString().ToLower()
                        });
                        //For Delivery Order
                        ListSettings.Add(new SettingDTO
                        {
                            SettingId = Commons.ESetting.DeliveryNew.ToString("d"),
                            Value = model.DeliveryNew.ToString().ToLower()
                        });
                        ListSettings.Add(new SettingDTO
                        {
                            SettingId = Commons.ESetting.DeliveryAccepted.ToString("d"),
                            Value = model.DeliveryAccepted.ToString().ToLower()
                        });
                        ListSettings.Add(new SettingDTO
                        {
                            SettingId = Commons.ESetting.DeliveryReady.ToString("d"),
                            Value = model.DeliveryReady.ToString().ToLower()
                        });
                        ListSettings.Add(new SettingDTO
                        {
                            SettingId = Commons.ESetting.DeliveryDelivering.ToString("d"),
                            Value = model.DeliveryDelivering.ToString().ToLower()
                        });
                    //}

                    if (model.ListDeliveryFee != null && model.ListDeliveryFee.Any())
                    {
                        //model.ListDeliveryFee = model.ListDeliveryFee.Where(o => o.Status != 9).ToList();

                        if (model.ListDeliveryFee.Count > 0)
                        {
                            model.ListDeliveryFee.ForEach(o =>
                            {
                                o.Type = (byte)Commons.EDeliverySettingType.Fee;
                                o.StoreID = model.StoreID;
                            });
                            model.ListDeliverySetting.AddRange(model.ListDeliveryFee);
                        }
                    }
                }                             

                //======Bool
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.AutoPrintGuestCheck.ToString("d"),
                    Value = model.AutoPrintGuestCheck.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.SingleOrderChit.ToString("d"),
                    Value = model.SingleOrderChit.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.TotalOrderChit.ToString("d"),
                    Value = model.TotalOrderChit.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.AllowBookingOnline.ToString("d"),
                    Value = model.AllowBookingOnline.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.CloseDrawerPrintReport.ToString("d"),
                    Value = model.PrintReportWhenCloseDrawer.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PrintConsolidated.ToString("d"),
                    Value = model.PrintConsolidated.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PrintItemTotalChit.ToString("d"),
                    Value = model.PrintAddOnItemsForTotalChit.ToString().ToLower()// ? "1" : "0"
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ShowValueInCashDrawer.ToString("d"),
                    Value = model.ShowValueInCashDrawer.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ShowValueInShift.ToString("d"),
                    Value = model.ShowValueInShift.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ShowValueInStore.ToString("d"),
                    Value = model.ShowValueInStore.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ShowDeleteConfirmSendItem.ToString("d"),
                    Value = model.ShowDeletionConfirmationForSentItem.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ShowDeleteConfirmOrder.ToString("d"),
                    Value = model.ShowDeletionConfirmationForWholeOrder.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ShowAllCategory.ToString("d"),
                    Value = model.ShowAllCategories.ToString().ToLower()
                });
                // Updated 08172017
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.MakeTableAvailable.ToString("d"),
                    Value = model.MakeTableAvailable.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.NoSale.ToString("d"),
                    Value = model.NoSaleSetting.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ShowBarcode.ToString("d"),
                    Value = model.ShowBarcode.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ShowPopUpToPrintReceipt.ToString("d"),
                    Value = model.ShowPopUpToPrintReceipt.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PrintReceipt.ToString("d"),
                    Value = model.PrintReceipt.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PrintReceiptCompany.ToString("d"),
                    Value = model.PrintReceiptCompany.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PrintDayMenuItemSale.ToString("d"),
                    Value = model.PrintDayMenuItemSale.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PrintDayCategorySale.ToString("d"),
                    Value = model.PrintDayCategorySale.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.IncludeShiftData.ToString("d"),
                    Value = model.IncludeShiftData.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.IncludeDrawerData.ToString("d"),
                    Value = model.IncludeDrawerData.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PrintEndShiftReport.ToString("d"),
                    Value = model.PrintEndShiftReport.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PrintEndDayReport.ToString("d"),
                    Value = model.PrintEndDayReport.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.IntegrationInclude.ToString("d"),
                    Value = model.IntegrationInclude.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ManualPromotion.ToString("d"),
                    Value = model.ManualPromotion.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.FTPFolderReport.ToString("d"),
                    Value = string.IsNullOrWhiteSpace(model.FTPFolderReport) ? "" : model.FTPFolderReport.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.FTPUser.ToString("d"),
                    Value = string.IsNullOrWhiteSpace(model.FTPUser) ? "" : model.FTPUser.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.FTPPassword.ToString("d"),
                    Value = string.IsNullOrWhiteSpace(model.FTPPassword) ? "" : model.FTPPassword.ToString().ToLower()
                });                
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.IsSubmitMallReport.ToString("d"),
                    Value = model.IsSubmitMallReport.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.IsNotifySubmitMallReport.ToString("d"),
                    Value = model.IsNotifySubmitMallReport.ToString().ToLower()
                });
                if (string.IsNullOrEmpty(model.MallReportMailTo))
                {
                    model.MallReportMailTo = "";
                }
                string[] ArrayMallReportMailTo = model.MallReportMailTo.Split(',');
                string resultKQ = "";

                for (int i = 0; i < ArrayMallReportMailTo.Length; i++)
                {
                    string item = ArrayMallReportMailTo[i].Replace("[\"", "");
                    item = item.Replace("\"", "");
                    item = item.Replace("]", "");
                    resultKQ = resultKQ + item + ",";
                }
                resultKQ = resultKQ.Substring(0, (resultKQ.Length - 1));
                //string item2 = ArrayMallReportMailTo[dem].Replace("[\"", "");
                //item2 = item2.Replace("\"", "");
                //item2 = item2.Replace("]", "");

                //resultKQ = resultKQ + item2;

                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.MallReportMailTo.ToString("d"),
                    Value = resultKQ.ToString()
                });
                if (model.IDMachineID == null)
                {
                    model.IDMachineID = "0";
                }
                if (model.IDDate == null)
                {
                    model.IDDate = "0";
                }
                if (model.IDHour == null)
                {
                    model.IDHour = "0";
                }
                if (model.IDSeparator == null)
                {
                    model.IDSeparator = "0";
                }
                string StringMallReportFileNameFormat = "";
                StringMallReportFileNameFormat = StringMallReportFileNameFormat + model.IDMachineID + "_" + model.IDDate + "_" + model.IDHour + "_" + model.IDSeparator;

                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.MallReportFileNameFormat.ToString("d"),
                    Value = StringMallReportFileNameFormat
                });

                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.MachineID.ToString("d"),
                    Value = string.IsNullOrWhiteSpace(model.MachineID) ? "" : model.MachineID.ToString().ToLower()
                });

                //For Order Management Setting
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.NumberOfLatestOrder.ToString("d"),
                    Value = model.NumberOfLatestOrder.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.NumberofDaysAllowforRefund.ToString("d"),
                    Value = model.NumberofDaysAllowforRefund.ToString().ToLower()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.SoundPrepareScreen.ToString("d"),
                    Value = string.IsNullOrWhiteSpace(model.SoundPrepareScreen) ? "" : model.SoundPrepareScreen.ToString()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.SoundServeScreen.ToString("d"),
                    Value = string.IsNullOrWhiteSpace(model.SoundServeScreen) ? "" : model.SoundServeScreen.ToString()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ItemBackgroundReady.ToString("d"),
                    Value = string.IsNullOrWhiteSpace(model.ItemBackgroundReady) ? "" : model.ItemBackgroundReady.ToString()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.ItemBackgroundServed.ToString("d"),
                    Value = string.IsNullOrWhiteSpace(model.ItemBackgroundServed) ? "" : model.ItemBackgroundServed.ToString()
                });
                if (model.IsNumOfCellOrderMana)
                {
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.NumOfCellOrderMana.ToString("d"),
                        Value = "3"
                    });
                }
                else
                {
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.NumOfCellOrderMana.ToString("d"),
                        Value = "6"
                    });
                }
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.PendingTime.ToString("d"),
                    Value = model.PendingTime.ToString().ToLower()
                });

                //For Time Configuration in Reports
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.BreakfastStart.ToString("d"),
                    Value = model.BreakfastStart.ToString()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.BreakfastEnd.ToString("d"),
                    Value = model.BreakfastEnd.ToString()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.LunchStart.ToString("d"),
                    Value = model.LunchStart.ToString()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.LunchEnd.ToString("d"),
                    Value = model.LunchEnd.ToString()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.DinnerStart.ToString("d"),
                    
                    Value = model.DinnerStart.ToString()
                });
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.DinnerEnd.ToString("d"),
                    Value = model.DinnerEnd.ToString()
                });
                
                if (CurrentUser.POSInstanceVersion.HasValue)
                {
                    // Set up Payment options when making order 
                    // Add to Tab
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.AddToTab.ToString("d"),
                        Value = model.AddToTab.ToString()
                    });
                    // Pay to Waiter
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.PayToWaiter.ToString("d"),
                        Value = model.PayToWaiter.ToString()
                    });
                    //At Reservation group
                    //Timeslot period
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.TimeSlot.ToString("d"),
                        Value = model.TimeslotPeriod.ToString()
                    });

                    //Maximum Of Person(s) come
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.MaxNoOfPerson.ToString("d"),
                        Value = model.MaximunOfPersonCome.ToString()
                    });
                    //Add on [] of persons for reservation/booking
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.AddOnCoverNo.ToString("d"),
                        Value = model.AddOnOfPersonForReservationOrBooking.ToString()
                    });
                    //Allow Duplicate Booking
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.DuplicateBooking.ToString("d"),
                        Value = model.AllowDuplicateBooking.ToString()
                    });

                    //Number of Duplicates
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.NumberOfDuplicates.ToString("d"),
                        Value = model.NumberofDuplicates.ToString()
                    });
                    //Bottle Service
                    string TxtFormat = "";
                    TxtFormat = !string.IsNullOrEmpty(model.FormatText) ? model.FormatText.ToString() : "";
                    //Text to Show at Label Format
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.LabelText.ToString("d"),
                        Value = TxtFormat
                    });
                    // Expiry Date ( days)
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.LabelExpired.ToString("d"),
                        Value = model.ExpiryDate.ToString()
                    });
                    // Time for Notification Reminder ( days)
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.LabelTimeNoti.ToString("d"),
                        Value = model.TimeforNotificationReminder.ToString()
                    });
                }
                // Add Extra Price to Dish/Set
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.AddExtraPriceToDishSet.ToString("d"),
                    Value = model.AddExtraPriceToDishSet.ToString().ToLower(),
                });

                //For Invoice Setting
                byte[] photoLogoInvoiceByte = null;
                if (model.ListInvoice != null && model.ListInvoice.Any() && model.ListInvoice[0].Setting != null)
                {
                    if (!string.IsNullOrEmpty(model.ListInvoice[0].Setting.ImageURL) && model.ListInvoice[0].Setting.ImageURL.Length > 0)
                    {
                        model.ListInvoice[0].Setting.Logo = model.ListInvoice[0].Setting.ImageURL.Replace(model.ListInvoice[0].Setting.PublicImages, "").Replace(Commons.Image100_100, "").ToString();
                    }
                    // Backgrounp Image
                    if (model.ListInvoice[0].Setting.ImgLogoInvoiceUpload != null && model.ListInvoice[0].Setting.ImgLogoInvoiceUpload.ContentLength > 0)
                    {
                        Byte[] imgByte = new Byte[model.ListInvoice[0].Setting.ImgLogoInvoiceUpload.ContentLength];
                        model.ListInvoice[0].Setting.ImgLogoInvoiceUpload.InputStream.Read(imgByte, 0, model.ListInvoice[0].Setting.ImgLogoInvoiceUpload.ContentLength);
                        model.ListInvoice[0].Setting.ImgLogoInvoiceByte = imgByte;
                        model.ListInvoice[0].Setting.Logo = Guid.NewGuid() + Path.GetExtension(model.ListInvoice[0].Setting.ImgLogoInvoiceUpload.FileName);
                        model.ListInvoice[0].Setting.ImgLogoInvoiceUpload = null;
                        photoLogoInvoiceByte = imgByte;
                    }
                }

                //==============
                model.ListSettings = ListSettings;
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
               
                //====================
                var result = _factory.InsertOrUpdateGeneralSetting(model);
                if (result)
                {
                    FTP.Upload(model.ImgBackgroundURL, model.ImgBackgroundByte);
                    FTP.Upload(model.SigImgURL, model.SigImgByte);                                       
                }
               
                if (model.ListInvoice != null && model.ListInvoice.Any())
                {
                    result = _factory.InsertOrUpdateGeneraInvoice(model);
                    if (result)
                    {
                        if (model.ListInvoice[0].Setting != null && !string.IsNullOrEmpty(model.ListInvoice[0].Setting.Logo) && photoLogoInvoiceByte != null)
                        {
                            model.ListInvoice[0].Setting.ImgLogoInvoiceByte = photoLogoInvoiceByte;
                            FTP.Upload(model.ListInvoice[0].Setting.Logo, model.ListInvoice[0].Setting.ImgLogoInvoiceByte);
                        }                        
                    }
                   
                }
                if (CurrentUser.POSInstanceVersion.HasValue && model.ListDeliverySetting != null && model.ListDeliverySetting.Any())
                {
                    result = _factory.InsertOrUpdateGeneraDeliverylSetting(model);
                    
                }
                if (result)
                    return RedirectToAction("Index");
                else
                    return PartialView("_Edit", model);
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GeneralSetting_Edit Error: " , ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        private bool GetBoolean(string value)
        {
            switch (value.ToLower())
            {
                case "1":
                case "true":
                    return true;
                default:
                    return false;
            }
        }

        public ActionResult AddDeliveryFee(int currentOffset)
        {
            DeliverySettingDTO group = new DeliverySettingDTO();
            group.OffSet = currentOffset;
            return PartialView("_TabDeliveryFee", group);
        }
        public ActionResult AddPostalCode(int currentOffset)
        {
            DeliverySettingDTO group = new DeliverySettingDTO();
            group.OffSet = currentOffset;
            return PartialView("_TabPostalCode", group);
        }

        public ActionResult AddTabInvoiceSetting(string invoiceID, string storeID)
        {
            InvoiceDTO model = new InvoiceDTO();
            var InvoiceSetting = _factory.GetInvoicePrintSetting(storeID, null);
            if (InvoiceSetting.ListInvoice != null)
            {
                model = InvoiceSetting.ListInvoice.Where(o => o.ID.Equals(invoiceID)).FirstOrDefault();
                model.IsSelected = true;
            }
            if (model.Setting.Logo != null)
            {
                model.Setting.ImageURL = model.Setting.PublicImages + model.Setting.Logo;
            }
            
            return PartialView("_TabInvoiceSetting", model);
        }
    }
}