using NLog;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class STipServiceChargeController : HQController
    {
        private TipServiceChargeFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public STipServiceChargeController()
        {
            _factory = new TipServiceChargeFactory();
            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                TipServiceChargeViewModels model = new TipServiceChargeViewModels();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("TipServiceCharge_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(TipServiceChargeViewModels model)
        {
            try
            {
                var datas = _factory.GetListTipServiceCharge(model.StoreID);
                model.ListItem = datas;
            }
            catch (Exception ex)
            {
                _logger.Error("TipServiceCharge_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }

        public TipServiceChargeModels GetDetail(string id, string StoreID)
        {
            try
            {
                TipServiceChargeModels model = _factory.GetListTipServiceCharge(StoreID, id)[0];
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("TipServiceCharge_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string StoreID, string id)
        {
            TipServiceChargeModels model = GetDetail(id, StoreID);
            return PartialView("_View", model);
        }

        public ActionResult Create()
        {
            TipServiceChargeModels model = new TipServiceChargeModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(TipServiceChargeModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Value.ToString()))
                {
                    ModelState.AddModelError("Value", "Value field is required");
                }
                if (!model.IsCurrency)
                {
                    if (model.Value < 0 || model.Value > 100)
                    {
                        ModelState.AddModelError("Value", "Please input default price greater than 0 and Maximum service charge is 100%");
                    }
                }
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                string msg = "";
                bool result = _factory.InsertOrUpdateTipServiceCharge(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("TipServiceCharge_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public PartialViewResult Edit(string StoreID, string id)
        {
            TipServiceChargeModels model = GetDetail(id, StoreID);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(TipServiceChargeModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose Store"));
                if (!model.IsCurrency)
                {
                    if (model.Value < 0 || model.Value > 100)
                    {
                        ModelState.AddModelError("Value", CurrentUser.GetLanguageTextFromKey("Please input default price greater than 0 and Maximum service charge is 100%"));
                    }
                }
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }


                //====================
                string msg = "";
                var result = _factory.InsertOrUpdateTipServiceCharge(model, ref msg);
                if (result)
                {

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", msg);
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("TipServiceCharge_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}