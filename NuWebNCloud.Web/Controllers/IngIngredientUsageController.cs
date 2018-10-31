using NLog;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Ingredients;
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
    public class IngIngredientUsageController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private IngredientsUsageFactory _factory = null;
        List<SelectListItem> lstStore = new List<SelectListItem>();
        List<string> listStoreId = new List<string>();

        public IngIngredientUsageController()
        {
            _factory = new IngredientsUsageFactory();

            ViewBag.ListStore = GetListStore();
            //==========
            lstStore = ViewBag.ListStore;
            listStoreId = lstStore.Select(x => x.Value).ToList();
        }

        public ActionResult Index(string searchstring)
        {
            IngredientsUsageRequestViewModels model = new IngredientsUsageRequestViewModels();
            //model.StoreId = CurrentUser.StoreId;
            return View(model);
        }

        public ActionResult Search(IngredientsUsageRequestViewModels model)
        {
            var dataReturn = new IngredientsUsageRequestViewModels();
            try
            {
                if (!string.IsNullOrEmpty(model.StoreId))
                {
                    var data = _factory.GetListIngredientUsage(model);

                    dataReturn.ListItem = data;
                    //CurrentUser.StoreId = model.StoreId;
                }
            }
            catch (Exception e)
            {
                _logger.Error("IngredientsUsageSearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", dataReturn);
        }

        public ActionResult CreateAllocate(List<string> lstId, string storeId, string dFrom, string dTo)
        {
            //List<IngredientsUsageRequestViewModels> lstInput = new List<IngredientsUsageModels>();
            IngredientsUsageRequestViewModels model = new IngredientsUsageRequestViewModels();
            model.StoreId = storeId;
            model.ApplyFrom = new DateTime(int.Parse(dFrom.Substring(6, 4)), int.Parse(dFrom.Substring(0, 2)), int.Parse(dFrom.Substring(3, 2)));
            model.ApplyTo = new DateTime(int.Parse(dTo.Substring(6, 4)), int.Parse(dTo.Substring(0, 2)), int.Parse(dTo.Substring(3, 2)));
            var data = _factory.GetListIngredientUsage(model);

            model.ListItem = data.Where(ww => lstId.Contains(ww.Id)).ToList();
            return PartialView("_ListAllocation", model);
        }

        [HttpPost]
        public ActionResult SaveAllocateVariance(IngredientsUsageRequestViewModels input)
        {
            try
            {
                foreach (var item in input.ListItem)
                {
                    item.Date = new DateTime(int.Parse(item.DateDisplay.Substring(6, 4)), int.Parse(item.DateDisplay.Substring(0, 2))
                        , int.Parse(item.DateDisplay.Substring(3, 2)));
                    //if(item.Damage <=0 && item.Wast <=0 && item.Others <= 0)
                    //{
                    //    ModelState.AddModelError("Allocation", "Please enter value for at least 1 field before saving...");
                    //}
                }
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_ListAllocation", input);
                }
                string msg = "";
                bool result = _factory.InsertAlocation(input.ListItem, CurrentUser.UserId, ref msg);

                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("StoreId", msg);
                    return PartialView("_ListAllocation", input);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("IngredientsUsageCreate: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        //[HttpPost]
        //public ActionResult AllocateVariance(IngredientsUsageModels model)
        //{
        //    try
        //    {
        //        string msg = "";
        //        bool result = _factory.Insert(model, ref msg);
        //        if (result)
        //        {
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("StoreId", msg);
        //            return PartialView("_ListAllocation");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("ReturnNote_Create: " + ex);
        //        return new HttpStatusCodeResult(400, ex.Message);
        //    }
        //}

        //public ActionResult LoadAllocation(List<string> lstId)
        //{
        //    IngredientsUsageModels model = new IngredientsUsageModels();
        //    try
        //    {
        //        IngredientFactory IngFactory = new IngredientFactory();
        //        var listIng = IngFactory.GetIngredient("");
        //        foreach (var item in listIng)
        //        {
        //            model.ListItem.Add(new IngredientsUsageModels
        //            {
        //                IngredientId = item.Id,
        //                IngredientName = item.Name,
        //                IngredientCode = item.Code,
        //                CloseBal = 0
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex);
        //        return new HttpStatusCodeResult(400, ex.Message);
        //    }
        //    return PartialView("_ListAllocation", model);
        //}
    }
}