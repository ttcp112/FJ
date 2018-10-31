using NLog;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models.Settings.MerchantSetting;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SMerchantSettingController : HQController
    {
        private MerchantSettingFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SMerchantSettingController()
        {
            _factory = new MerchantSettingFactory();
            ViewBag.ListStore = GetListStore();
        }

        // GET: SMerchantSetting
        public ActionResult Index()
        {
            try
            {
                MerchantSettingViewModels model = new MerchantSettingViewModels();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("MerchantSetting_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(MerchantSettingViewModels model)
        {
            try
            {
                //var data = _factory.GetListData(model.StoreID, CurrentUser.ListOrganizationId);
                var data = CurrentUser.ListOrganizations;
                model.ListItem = data;
            }
            catch (Exception ex)
            {
                _logger.Error("MerchantSetting_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

            return PartialView("_ListData", model);
        }

        public MerchantSettingModels GetDetail(string id)
        {
            try
            {
                MerchantSettingModels model = new MerchantSettingModels();
                //===================================
                var listStoreSetting = _factory.GetListDataPayment(id, CurrentUser.ListOrganizationId);
                if (listStoreSetting != null && listStoreSetting.Count > 0)
                {
                    var storeSetting = listStoreSetting.Where(x => x.Value.ToLower().Equals("true")).FirstOrDefault();
                    if (storeSetting != null)
                    {
                        model.StoreID = storeSetting.StoreId;
                        model.StoreName = storeSetting.StoreName;
                    }
                }
                //List<string> ListOrganizationId = new List<string>();
                //ListOrganizationId.Add(id);
                //var listStore = _factory.GetListStoreForMerchant(ListOrganizationId, CurrentUser.ListStoreID);
                //ViewBag.ListStore = listStore;
                //===============
                var listCompnay = _factory.GetListDataWallet(id, CurrentUser.ListOrganizationId);
                if (listCompnay != null && listCompnay.Count > 0)
                {
                    listCompnay = listCompnay.OrderBy(x => x.CompanyName).ToList();
                    int offset = 0;
                    listCompnay.ForEach(x =>
                    {
                        x.IsActive = (x.Value.ToLower().Equals("true")) ? true : false;
                        x.OffSet = offset++;
                    });
                    model.ListCompnayShow = listCompnay;
                }
                model.Id = id;
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("MerchantSetting_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            MerchantSettingModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            MerchantSettingModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(MerchantSettingModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store"));
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }

                model.StoreSetting = new StoreSettingModels();
                model.StoreSetting.StoreId = model.StoreID;
                model.StoreSetting.Value = "true";

                //==============
                //List<string> ListOrganizationId = new List<string>();
                //ListOrganizationId.Add(model.Id);
                //var listStore = _factory.GetListStoreForMerchant(ListOrganizationId, CurrentUser.ListStoreID);
                //ViewBag.ListStore = listStore;

                //==========
                string msg = "";
                //Payment
                var result = _factory.UpdatePayment(model, CurrentUser.ListOrganizationId, ref msg);
                if (result)
                {
                    //Wallet
                    if (model.ListCompnayShow != null && model.ListCompnayShow.Count > 0)
                    {
                        model.ListCompnayShow.ForEach(x =>
                        {
                            x.Value = x.IsActive.ToString();
                        });
                    }
                    model.ListCompany = model.ListCompnayShow;
                    //============
                    result = _factory.UpdateWallet(model, CurrentUser.ListOrganizationId, ref msg);
                    if (result)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("StoreID", msg);
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return PartialView("_Edit", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("StoreID", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("MerchantSetting_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}