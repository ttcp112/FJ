using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngStockCountController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private StockCountFactory _factory = null;
        private InventoryFactory _InventoryFactory = null;
        private BusinessDayFactory _BusinessDayFactory = null;

        List<SelectListItem> lstStore = new List<SelectListItem>();
        List<string> listStoreId = new List<string>();

        public IngStockCountController()
        {
            _factory = new StockCountFactory();
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
            StockCountViewModels model = new StockCountViewModels();
            model.StoreId = CurrentUser.StoreId;
            return View(model);
        }

        public ActionResult Search(StockCountViewModels model)
        {
            try
            {
                if (model.StoreId != null)
                {
                    var data = _factory.GetData(model, listStoreId);
                    if (data == null)
                    {
                        data = new List<StockCountModels>();
                    }
                    var lastedItem = data.FirstOrDefault();
                    if(lastedItem != null)
                    {
                        if (!lastedItem.IsAutoCreated)
                        {
                            if (lastedItem.Status == (int)Commons.EStockCountStatus.Open)
                                lastedItem.IsVisible = true;
                        }
                    }
                    //var ListMax = data.GroupBy(x => new { x.StoreId })
                    //        .Select(g => new StockCountModels()
                    //        {
                    //            StockCountDate = g.Max(x => x.StockCountDate),
                    //            Id = g.Select(x => x.Id).FirstOrDefault(),
                    //            IsAutoCreated = g.Select(x=>x.IsAutoCreated).FirstOrDefault()
                    //        }).ToList();
                    data.ForEach(x =>
                    {
                        x.StoreName = lstStore.Where(z => z.Value.Equals(x.StoreId)).FirstOrDefault().Text;
                       
                    });
                    model.ListItem = data;
                    CurrentUser.StoreId = model.StoreId;
                }
            }
            catch (Exception e)
            {
                _logger.Error("StockCountSearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult LoadIngredient(string StoreId, string businessId)
        {
            StockCountModels model = new StockCountModels();
            var listIng = _InventoryFactory.LoadIngredientForStockCount(StoreId, businessId);
            var lstStockCountDetail = _factory.GetIngredientFromStockCount(StoreId, businessId);
            foreach (var item in listIng)
            {
                item.Id = lstStockCountDetail.Where(ww => ww.IngredientId == item.IngredientId).Select(ss => ss.Id).FirstOrDefault();
                item.CloseBal = lstStockCountDetail.Where(ww => ww.IngredientId == item.IngredientId).Select(ss => ss.CloseBal).FirstOrDefault();
                //model.ListItem.Add(new StockCountDetailModels
                //{
                //    IngredientId = item.Id,
                //    IngredientName = item.Name,
                //    IngredientCode = item.Code,
                //    CloseBal = 0
                //});
            }
            model.ListItem = listIng;
            return PartialView("_ListItemNew", model);
        }

        public ActionResult Create()
        {
            StockCountModels model = new StockCountModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(StockCountModels model)
        {
            try
            {
                model.CreatedBy = CurrentUser.UserName;
                model.ModifierBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.ModifierDate = DateTime.Now;
                model.StockCountDate = model.CreatedDate;
                model.BusinessId = model.BusinessId;
                model.IsActived = true;
                model.Status = (int)Commons.EStockCountStatus.Open;

                string msg = "";
                bool result = _factory.InsertManual(model, false, ref msg);
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
                _logger.Error("StockCount_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public StockCountModels GetDetail(string id)
        {
            try
            {
                StockCountModels model = _factory.GetStockCountById(id);
                model.StoreName = lstStore.Where(x => x.Value.Equals(model.StoreId)).FirstOrDefault().Text;
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("StockCount_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            StockCountModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            StockCountModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(StockCountModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreId))
                    ModelState.AddModelError("Store", CurrentUser.GetLanguageTextFromKey("Store field is required"));
                if (string.IsNullOrEmpty(model.BusinessId))
                    ModelState.AddModelError("Apply Date", CurrentUser.GetLanguageTextFromKey("Apply Date field is required"));
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
                _logger.Error("Edit StokCount: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        /*Delete*/
        public PartialViewResult Delete(string id)
        {
            StockCountModels model = _factory.GetStockCountWithoutDetailById(id);
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
                _logger.Error("StockCount_Delete: " + ex);
                ModelState.AddModelError("StoreId", CurrentUser.GetLanguageTextFromKey("Have an error when you confrim a Stock Count"));
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
                var firstBusinessDay = lstData.FirstOrDefault();
                if (firstBusinessDay != null && firstBusinessDay.ClosedOn.HasValue 
                    && firstBusinessDay.ClosedOn.Value != Commons.MinDate &&  lstData.Count == 2)
                    lstData.RemoveAt(1);

                var lst = lstData.Select(ss => ss.ID).ToList();
                var lstBusinessId = _factory.CheckShowBusiness(StoreID, lst);

                lstData = lstData.Where(ww => lstBusinessId.Contains(ww.ID)).ToList();
            }
           
            return Json(lstData, JsonRequestBehavior.AllowGet);
        }
        
        public PartialViewResult Confirm(string id)
        {
            StockCountModels model = _factory.GetStockCountWithoutDetailById(id);
            return PartialView("_Confirm", model);
        }
        [HttpPost]
        public ActionResult Confirm(StockCountModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.Confirm(model.Id, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Confirm", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("StockCount_Confirm: " + ex);
                ModelState.AddModelError("StoreId", CurrentUser.GetLanguageTextFromKey("Have an error when you confrim a Stock Count"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Confirm", model);
            }
        }
    }
}