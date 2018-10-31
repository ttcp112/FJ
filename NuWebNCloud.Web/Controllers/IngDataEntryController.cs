using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngDataEntryController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DataEntryFactory _factory = null;
        private InventoryFactory _InventoryFactory = null;
        private BusinessDayFactory _BusinessDayFactory = null;

        List<SelectListItem> lstStore = new List<SelectListItem>();
        List<string> listStoreId = new List<string>();

        public IngDataEntryController()
        {
            _factory = new DataEntryFactory();
            _InventoryFactory = new InventoryFactory();
            _BusinessDayFactory = new BusinessDayFactory();

            ViewBag.ListStore = GetListStore();
            //==========
            lstStore = ViewBag.ListStore;
            listStoreId = lstStore.Select(x => x.Value).ToList();
        }
        // GET: IngDataEntry
        public ActionResult Index()
        {
            DataEntryViewModels model = new DataEntryViewModels();
            model.StoreId = CurrentUser.StoreId;
            return View(model);
        }

        public ActionResult Search(DataEntryViewModels model)
        {
            try
            {
                if (model.StoreId != null)
                {
                    var data = _factory.GetData(model, listStoreId);

                    foreach (var item in data)
                    {
                        item.StoreName = lstStore.Where(z => z.Value.Equals(item.StoreId)).Select(ss => ss.Text).FirstOrDefault();
                        item.BusinessValue = CommonHelper.BusinessDayDisplay(item.StartedOn, item.ClosedOn);
                    }
                    //data.ForEach(x =>
                    //{
                    //    x.StoreName = lstStore.Where(z => z.Value.Equals(x.StoreId)).Select(ss => ss.Text).FirstOrDefault();
                       
                    //});
                    model.ListItem = data;
                    CurrentUser.StoreId = model.StoreId;
                }
            }
            catch (Exception e)
            {
                _logger.Error("DataEntrySearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult LoadIngredient(string storeId)
        {
            DataEntryModels model = new DataEntryModels();
            var listIng = _InventoryFactory.LoadIngredientForDataEntry(storeId);
            foreach (var item in listIng)
            {
                model.ListItem.Add(new DataEntryDetailModels
                {
                    IngredientId = item.Id,
                    IngredientName = item.Name,
                    IngredientCode = item.Code,
                    Damage = 0,
                    Wast = 0,
                    OrderQty =0,
                    Reasons ="",
                });
            }
            return PartialView("_ListItemNew", model);
        }

        public ActionResult Create()
        {
            DataEntryModels model = new DataEntryModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(DataEntryModels model)
        {
            try
            {
                model.CreatedBy = CurrentUser.UserName;
                model.ModifierBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.ModifierDate = DateTime.Now;
                model.EntryDate = model.CreatedDate;
                //model.BusinessId = model.BusinessId;
                model.IsActived = true;
                //var lst
                model.ListItem = model.ListItem.Where(ww => ww.Damage > 0 || ww.Wast > 0 || ww.OrderQty > 0).ToList();
                string msg = "";
                bool result = _factory.Insert(model, false, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("StoreId", msg);
                    return View("Create");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("DataEntry_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public DataEntryModels GetDetail(string id)
        {
            try
            {
                DataEntryModels model = _factory.GetDataEntryById(id);
                model.StoreName = lstStore.Where(x => x.Value.Equals(model.StoreId)).FirstOrDefault().Text;
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("DataEntry_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            DataEntryModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            DataEntryModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(DataEntryModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreId))
                    ModelState.AddModelError("Store", CurrentUser.GetLanguageTextFromKey("Store field is required"));
                //if (string.IsNullOrEmpty(model.BusinessId))
                //    ModelState.AddModelError("Apply Date", "Apply Date field is required");
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }

                model.IsActived = true;
                model.ModifierBy = CurrentUser.UserName;
                model.ModifierDate = DateTime.Now;
                string msg = "";
                bool result = _factory.Update(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("StoreId", msg);
                    //return View("Create");
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Edit DataEntry: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        /*Delete*/
        public PartialViewResult Delete(string id)
        {
            DataEntryModels model = _factory.GetDataEntryWithoutDetailById(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(StockCountModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.Delete(model.Id, ref msg);
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
                _logger.Error("DataEntry_Delete: " + ex);
                ModelState.AddModelError("StoreId", CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Data Entry"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult LoadBusinessDate(string StoreID)
        {
            var lstData = _BusinessDayFactory.GetDataForStoreOnApi(StoreID);
            if (lstData != null && lstData.Any())
            {
                lstData = lstData.Where(ww => ww.StartedOn != Commons.MinDate).ToList();
                var lst = lstData.Select(ss => ss.ID).ToList();
                var lstBusinessId = _factory.CheckShowBusiness(StoreID, lst);

                lstData = lstData.Where(ww => lstBusinessId.Contains(ww.ID)).ToList();
            }
           
            return Json(lstData, JsonRequestBehavior.AllowGet);
        }
    }
}