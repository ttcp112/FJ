using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SXeroController : HQController
    {
        private XeroFactory _factory = null;

        public SXeroController()
        {
            _factory = new XeroFactory();
        }

        // GET: SXeroController
        public ActionResult Index()
        {
            try
            {
                XeroSettingViewModels model = new XeroSettingViewModels();
                model.StoreID = CurrentUser.StoreId;
                ////========Test GEN INVOICE
                //if (Commons.isIntegrateXero)
                //{
                //    string msg = "";
                //    GenerateInvoiceModels GenerateInvoice = new GenerateInvoiceModels();
                //    var data = _factory.GenerateInvoice(GenerateInvoice, ref msg);
                //}
                return View(model);
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("XeroSetting_Index: ", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(XeroSettingViewModels model)
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
                NSLog.Logger.Error("XeroSetting_Search: ", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }

        public List<XeroDTO> GetListXero(string StoreID)
        {
            var ThirdParty = CurrentUser.listStore.Where(w => w.ID.Equals(StoreID)).FirstOrDefault();
            string ThirdPartyID = "";
            string Url = "";
            if (ThirdParty != null && ThirdParty.ThirdParty != null)
            {
                StoreID = ThirdParty.ThirdParty.IPAddress;
                ThirdPartyID = ThirdParty.ThirdParty.ThirdPartyID;
                Url = ThirdParty.ThirdParty.ApiURL;
            }
            List<XeroDTO> data = new List<XeroDTO>();
            if (StoreID != null && ThirdPartyID != null && Url != null)
            {
                data = _factory.GetListXeroSetting(StoreID, ThirdPartyID, Url);
            }            
            return data;
        }

        public XeroSettingModels GetDetail(string StoreID)
        {
            List<XeroDTO> LstXero = GetListXero(StoreID);
            XeroSettingModels model = new XeroSettingModels();
            if (LstXero != null)
                model.LisXeroDTO.AddRange(LstXero);
            var listSettings = _factory.GetListSetting(StoreID);
            if (listSettings.Any())
            {
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.CostOfGoodSold).FirstOrDefault() != null)
                {
                    model.CostOfGoodSold = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.CostOfGoodSold).Select(o => o.Value).FirstOrDefault();
                    model.DisplayCostOfGoodSold = LstXero.Where(x => x.AccountID.Equals(model.CostOfGoodSold)).Select(o => o.Name).FirstOrDefault();
                }

                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.StockOnHand).FirstOrDefault() != null)
                {
                    model.StockOnHand = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.StockOnHand).Select(o => o.Value).FirstOrDefault();
                    model.DisplayStockOnHand = LstXero.Where(x => x.AccountID.Equals(model.StockOnHand)).Select(o => o.Name).FirstOrDefault();
                }

                var IsPostToVend = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.PostToVend).Select(o=>o.Value).FirstOrDefault();
                if (IsPostToVend.ToLower() == "true")
                    model.IsPostToVend = true;
                else
                    model.IsPostToVend = false;

                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.Miscellaneous).FirstOrDefault() != null)
                {
                    model.Miscellaneous = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.Miscellaneous).Select(o => o.Value).FirstOrDefault();
                    model.DisplayMiscellaneous = LstXero.Where(x => x.AccountID.Equals(model.Miscellaneous)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.SendBillAs).FirstOrDefault() != null)
                {
                    model.SendBillAs = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.SendBillAs).Select(o => o.Value).FirstOrDefault();
                    model.DisplaySendBillAs = model.ListInvoice.Where(x => x.Value.Equals(model.SendBillAs)).Select(o => o.Text).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.RoundingError).FirstOrDefault() != null)
                {
                    model.RoundingError = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.RoundingError).Select(o => o.Value).FirstOrDefault();
                    model.DisplayRoundingError = LstXero.Where(x => x.AccountID.Equals(model.RoundingError)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.DiscountAccount).FirstOrDefault() != null)
                {
                    model.DiscountAccount = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.DiscountAccount).Select(o => o.Value).FirstOrDefault();
                    model.DisplayDiscountAccount = LstXero.Where(x => x.AccountID.Equals(model.DiscountAccount)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.LoyaltyLiability).FirstOrDefault() != null)
                {
                    model.LoyaltyLiability = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.LoyaltyLiability).Select(o => o.Value).FirstOrDefault();
                    model.DisplayLoyaltyLiability = LstXero.Where(x => x.AccountID.Equals(model.LoyaltyLiability)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.Loyaltyexpense).FirstOrDefault() != null)
                {
                    model.Loyaltyexpense = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.Loyaltyexpense).Select(o => o.Value).FirstOrDefault();
                    model.DisplayLoyaltyexpense = LstXero.Where(x => x.AccountID.Equals(model.Loyaltyexpense)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.GCLiability).FirstOrDefault() != null)
                {
                    model.GCLiability = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.GCLiability).Select(o => o.Value).FirstOrDefault();
                    model.DisplayGCLiability = LstXero.Where(x => x.AccountID.Equals(model.GCLiability)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.Deposit).FirstOrDefault() != null)
                {
                    model.Deposit = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.Deposit).Select(o => o.Value).FirstOrDefault();
                    model.DisplayDeposit = LstXero.Where(x => x.AccountID.Equals(model.Deposit)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.Payout).FirstOrDefault() != null)
                {
                    model.Payout = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.Payout).Select(o => o.Value).FirstOrDefault();
                    model.DisplayPayout = LstXero.Where(x => x.AccountID.Equals(model.Payout)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.TillPaymentDiscrepanceis).FirstOrDefault() != null)
                {
                    model.TillPaymentDiscrepanceis = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.TillPaymentDiscrepanceis).Select(o => o.Value).FirstOrDefault();
                    model.DisplayTillPaymentDiscrepanceis = LstXero.Where(x => x.AccountID.Equals(model.TillPaymentDiscrepanceis)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.CashFloat).FirstOrDefault() != null)
                {
                    model.CashFloat = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.CashFloat).Select(o => o.Value).FirstOrDefault();
                    model.DisplayCashFloat = LstXero.Where(x => x.AccountID.Equals(model.CashFloat)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.RefundByGC).FirstOrDefault() != null)
                {
                    model.RefundByGC = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.RefundByGC).Select(o => o.Value).FirstOrDefault();
                    model.DisplayRefundByGC = LstXero.Where(x => x.AccountID.Equals(model.RefundByGC)).Select(o => o.Name).FirstOrDefault();
                }
                if (listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.ReturnGCAsCash).FirstOrDefault() != null)
                {
                    model.ReturnGCAsCash = listSettings.Where(x => x.Code == (byte)Commons.EGeneralSetting.ReturnGCAsCash).Select(o => o.Value).FirstOrDefault();
                    model.DisplayReturnGCAsCash = LstXero.Where(x => x.AccountID.Equals(model.ReturnGCAsCash)).Select(o => o.Name).FirstOrDefault();
                }
            }
            return model;
        }

        [HttpGet]
        public new PartialViewResult View(string StoreID, string StoreName)
        {
            XeroSettingModels model = GetDetail(StoreID);
            model.StoreName = StoreName;
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string StoreID, string StoreName)
        {
            XeroSettingModels model = GetDetail(StoreID);
            if (model.LisXeroDTO != null && model.LisXeroDTO.Any())
            {
                model.LisXeroDTO = model.LisXeroDTO.OrderBy(o => o.ReportingCodeName).ToList();
                //model.LisXeroDTO.ForEach(o =>
                //{
                //    if (!string.IsNullOrEmpty(o.ReportingCodeName))
                //    {
                //        if (o.ReportingCodeName.Length < 25)
                //            o.ReportingCodeName = o.ReportingCodeName;
                //        else
                //            o.ReportingCodeName = o.ReportingCodeName.Substring(0, 25) + "...";
                //    }
                //    if (!string.IsNullOrEmpty(o.Name))
                //    {
                //        if (o.Name.Length < 25)
                //            o.Name = o.Code + "-" + o.Name;
                //        else
                //            o.Name = o.Code + "-" + o.Name.Substring(0, 25) + "...";
                //    }
                //});
            }
            ViewBag.ListAccount = new SelectList(model.LisXeroDTO, "Code", "Name", "ReportingCodeName", 1);
            model.StoreID = StoreID;
            model.StoreName = StoreName;
            model.ListInvoice = model.ListInvoice.OrderBy(o => o.Text).ToList();            
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(XeroSettingModels model)
        {
            try
            {
                //if (string.IsNullOrEmpty(model.StoreID))
                //    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                List<SettingXeroDTO> ListSettings = new List<SettingXeroDTO>();

                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.CostOfGoodSold.ToString("d")),
                    Value = string.IsNullOrEmpty(model.CostOfGoodSold) ? "" : model.CostOfGoodSold,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.StockOnHand.ToString("d")),
                    Value = string.IsNullOrEmpty(model.StockOnHand) ? "" : model.StockOnHand,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.PostToVend.ToString("d")),
                    Value = model.IsPostToVend.ToString(),
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.Miscellaneous.ToString("d")),
                    Value = string.IsNullOrEmpty(model.Miscellaneous) ? "" : model.Miscellaneous,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.SendBillAs.ToString("d")),
                    Value = string.IsNullOrEmpty(model.SendBillAs) ? "" : model.SendBillAs,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.RoundingError.ToString("d")),
                    Value = string.IsNullOrEmpty(model.RoundingError) ? "" : model.RoundingError,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.DiscountAccount.ToString("d")),
                    Value = string.IsNullOrEmpty(model.DiscountAccount) ? "" : model.DiscountAccount,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.LoyaltyLiability.ToString("d")),
                    Value = string.IsNullOrEmpty(model.LoyaltyLiability) ? "" : model.LoyaltyLiability,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.Loyaltyexpense.ToString("d")),
                    Value = string.IsNullOrEmpty(model.Loyaltyexpense) ? "" : model.Loyaltyexpense,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.GCLiability.ToString("d")),
                    Value = string.IsNullOrEmpty(model.GCLiability) ? "" : model.GCLiability,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.Deposit.ToString("d")),
                    Value = string.IsNullOrEmpty(model.Deposit) ? "" : model.Deposit,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.Payout.ToString("d")),
                    Value = string.IsNullOrEmpty(model.Payout) ? "" : model.Payout,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.TillPaymentDiscrepanceis.ToString("d")),
                    Value = string.IsNullOrEmpty(model.TillPaymentDiscrepanceis) ? "" : model.TillPaymentDiscrepanceis,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.CashFloat.ToString("d")),
                    Value = string.IsNullOrEmpty(model.CashFloat) ? "" : model.CashFloat,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.RefundByGC.ToString("d")),
                    Value = string.IsNullOrEmpty(model.RefundByGC) ? "" : model.RefundByGC,
                });
                ListSettings.Add(new SettingXeroDTO
                {
                    Code = int.Parse(Commons.EGeneralSetting.ReturnGCAsCash.ToString("d")),
                    Value = string.IsNullOrEmpty(model.ReturnGCAsCash) ? "" : model.ReturnGCAsCash,
                });
                model.ListSettingDTO.AddRange(ListSettings);
                //====================
                string msg = "";
                var result = _factory.CreateOrUpdateGeneralSetting(model, CurrentUser.UserId, ref msg);
                if (result)
                    return RedirectToAction("Index");
                else
                    return PartialView("_Edit", model);
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("XeroSetting_Edit Error: ", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}