using NLog;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models.Settings.Zone;
using NuWebNCloud.Web.App_Start;
using System;
using System.Net;
using System.Web.Mvc;
using System.Linq;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SZoneController : HQController
    {
        private ZoneFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SZoneController()
        {
            _factory = new ZoneFactory();
            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                ZonesModelsView model = new ZonesModelsView();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Zone_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(ZonesModelsView model)
        {
            try
            {
                var datas = _factory.GetListZone(model.StoreID,null, CurrentUser.ListOrganizationId);               
                datas = datas.Where(o => o.StoreID == model.StoreID).ToList();
                model.List_Zones = datas;
            }
            catch (Exception ex)
            {
                _logger.Error("Zone_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

            return PartialView("_ListData", model);
        }

        public ZoneModels GetDetail(string id)
        {
            try
            {
                ZoneModels model = _factory.GetListZone(null, id)[0];
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Zones_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            
            ZoneModels model = GetDetail(id);           
            return PartialView("_View", model);
        }

        public ActionResult Create()
        {
            ZoneModels model = new ZoneModels();
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(ZoneModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name field is required"));
                    return View(model);
                }
                string msg = "";
                bool result = _factory.InsertOrUpdateZones(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //return RedirectToAction("Create");
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Zone_Create : " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            ZoneModels model = GetDetail(id);             
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(ZoneModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteZones(model.ID, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Zones_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Zone"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public PartialViewResult Edit(string id)
        {
            ZoneModels model = GetDetail(id);           
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(ZoneModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose Store"));
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Zone Name is required"));
                
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }

                //====================
                
                string msg = "";
                var result = _factory.InsertOrUpdateZones(model, ref msg);
               
                if (result)
                {

                    return RedirectToAction("Index");
                }
                else
                {
                   
                    ModelState.AddModelError("Height", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Zone_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}