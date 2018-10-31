using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Settings;
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
    public class IngWorkOrdersController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private WorkOrderFactory _factory = null;
        private StoreFactory _Storefactory = null;
        private DefaultCurrencyFactory _defaultCurrencyFactory = null;

        List<SelectListItem> lstStore = new List<SelectListItem>();
        List<string> listStoreId = new List<string>();

        List<SelectListItem> lstCompany = new List<SelectListItem>();
        List<string> listCompanyId = new List<string>();

        List<SelectListItem> lstCur = new List<SelectListItem>();

        public IngWorkOrdersController()
        {
            _factory = new WorkOrderFactory();
            _Storefactory = new StoreFactory();
            _defaultCurrencyFactory = new DefaultCurrencyFactory();

            ViewBag.ListStore = GetListStore();
            lstStore = ViewBag.ListStore;
            listStoreId = lstStore.Select(x => x.Value).ToList();
            //==========
            lstCompany = GetListCompany();
            listCompanyId = lstCompany.Select(x => x.Value).ToList();

            var lstCurData = _defaultCurrencyFactory.GetListDefaultCurrency();
            foreach (var item in lstCurData)
            {
                lstCur.Add(new SelectListItem
                {
                    Value = item.Id,
                    Text = item.Symbol
                });
            }
        }

        public ActionResult Index()
        {
            try
            {
                WorkOrderModelsViewModels model = new WorkOrderModelsViewModels();
                model.StoreID = CurrentUser.StoreId;
                model.ApplyFrom = DateTime.Now;
                model.ApplyTo = DateTime.Now;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("WorkOrderIndex: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(WorkOrderModelsViewModels model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.StoreID) || model.ApplyFrom.HasValue || model.ApplyTo.HasValue)
                {
                    model.ListItem = _factory.GetData(model.StoreID, CurrentUser.ListStoreID, model.ApplyFrom, model.ApplyTo);
                    model.ListItem.ForEach(x =>
                    {
                        //var currency = lstCur.Where(z => z.Value.Equals(x.Store)).FirstOrDefault();
                        //x.Symbol = currency == null ? "$" : currency.Text;
                        x.StoreName = lstStore.Where(ww => ww.Value == x.StoreId).Select(ss => ss.Text).FirstOrDefault();
                    });
                    CurrentUser.StoreId = model.StoreID;
                }

            }
            catch (Exception e)
            {
                _logger.Error("WorkOrderSearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult LoadIngredient(LoadIngredientModel input)
        {
            IngredientFactory IngFactory = new IngredientFactory();
            WOIngredientViewModels model = new WOIngredientViewModels();
            var lstComId = GetListCompany().Select(ss => ss.Value).ToList();
            var listIng = IngFactory.GetIngredientSelfMade(lstComId);
            foreach (var item in listIng)
            {
                model.ListItemView.Add(new WOIngredient
                {
                    BaseUOM = item.ReceivingUOMName,
                    IngredientId = item.Id,
                    IngredientName = item.Name,
                    WorkPrice = item.PurchasePrice,

                    Description = item.Description,
                    IngredientCode = item.Code,
                    IngReceivingQty = item.ReceivingQty,
                    Qty = item.ReOrderQty.HasValue ? item.ReOrderQty.Value : 0
                });
            }
           
            if (input.ListItemNew != null)
            {
                model.ListItemView = model.ListItemView.Where(x => !input.ListItemNew.Contains(x.IngredientId)).ToList();
            }
            model.ListItemView = model.ListItemView.OrderByDescending(x => x.IsSelect ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
            return PartialView("_TableChooseIngredient", model);
        }

        public ActionResult AddIngredient(POIngredientViewModels data)
        {
            WorkOrderModels model = new WorkOrderModels();
            model.ListItem = new List<WOIngredient>();
            foreach (var item in data.ListItemView)
            {
                model.ListItem.Add(new WOIngredient
                {
                    Id = item.Id, // Add New

                    IngredientId = item.IngredientId,
                    IngredientName = item.IngredientName,
                    IngredientCode = item.IngredientCode,
                    Description = item.Description,

                    IsSelect = item.IsSelect,
                    Qty = item.Qty,
                    UnitPrice = item.UnitPrice,
                    Amount = item.Amount,

                    BaseUOM = item.BaseUOM,

                    IngReceivingQty = item.IngReceivingQty,

                    Delete = item.Delete
                });
            }
            return PartialView("_ListItemNew", model);
        }

        public ActionResult Create()
        {
            WorkOrderModels model = new WorkOrderModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(WorkOrderModels model)
        {
            try
            {
                if (model.DateCompleted.Date < model.WODate.Date)
                {
                    ModelState.AddModelError("DateCompleted",_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Completed date cannot be sooner than WO date"));
                }
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                //====================
                string msg = "";
                model.CreatedBy = CurrentUser.UserName;
                model.ModifierBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.ModifierDate = DateTime.Now;
                model.Status = (int)Commons.EPOStatus.Open;
                model.Total = 0;
                model.IsActived = true;
                //=====
                model.ListItem = model.ListItem.Where(x => x.Delete != (int)Commons.EStatus.Deleted).ToList();
                bool result = _factory.Insert(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("StoreId", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("WorkOrderCreate: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public WorkOrderModels GetDetail(string id)
        {
            try
            {
                WorkOrderModels model = _factory.GetDetail(id);
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("WorkOrderDetail: " + ex);
                return null;
            }
        }
        private string GetSymbol(string storeId)
        {
            var result = "$";
            try
            {
                var lstObj = _defaultCurrencyFactory.GetListDefaultCurrency(storeId);
                if (lstObj != null && lstObj.Count > 0)
                {
                    result = lstObj.Where(ww => ww.IsSelected).Select(ss => ss.Symbol).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return result;
        }

        public JsonResult GetSymbolCurrency(string storeId)
        {
            var result = "$";
            try
            {
                var lstObj = _defaultCurrencyFactory.GetListDefaultCurrency(storeId);
                if (lstObj != null && lstObj.Count > 0)
                {
                    result = lstObj.Where(ww => ww.IsSelected).Select(ss => ss.Symbol).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            WorkOrderModels model = GetDetail(id);
            model.Symbol = GetSymbol(model.StoreId);
            model.Store = new SStoreModels();
            model.Store = _Storefactory.GetListStores(null, model.StoreId)[0];
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            WorkOrderModels model = GetDetail(id);
            model.Symbol = "";
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(WorkOrderModels model)
        {
            try
            {
                if (model.DateCompleted.Date < model.WODate.Date)
                {
                    ModelState.AddModelError("DateCompleted", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Completed date cannot be sooner than PO date"));
                }
                model.ModifierBy = CurrentUser.UserName;
                model.ModifierDate = DateTime.Now;
                model.Total = 0;
                if (!ModelState.IsValid)
                {
                    model = GetDetail(model.Id);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //====================
                model.ListItem = model.ListItem.Where(x => x.Delete != (int)Commons.EStatus.Deleted).ToList();
                List<string> listItemOnData = _factory.listItemOnData(model.Id);
                var listIdDelete = listItemOnData.Where(a => !(model.ListItem.Select(x => x.Id).ToList()).Any(a1 => a1 == a)).ToList();

                string msg = "";
                var result = _factory.Update(model, listIdDelete, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    model = GetDetail(model.Id);
                    ModelState.AddModelError("StoreId", msg);
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("WorkOrderEdit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public PartialViewResult Delete(string id)
        {
            WorkOrderModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(WorkOrderModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.Delete(model.Id, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("WONumber", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("WorkOrderDelete: " + ex);
                ModelState.AddModelError("WONumber", "Have an error when you delete a Work Order");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        //====ChangeStatus PO
        [HttpPost]
        public ActionResult ChangeStatus(List<string> lstId, int status)
        {
            var data = _factory.ChangeStatus(lstId, status);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Export()
        {
            WorkOrderModels model = new WorkOrderModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(WorkOrderModels model)
        {
            try
            {
                if (model.ListStores == null || model.ListStores.Count == 0)
                {
                    ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsWO = wb.Worksheets.Add("WO List");
                var wsWOD = wb.Worksheets.Add("Ingredient List");

                List<SelectListItem> listStore = (List<SelectListItem>)ViewBag.ListStore;
                model.WODate = new DateTime(model.WODate.Year, model.WODate.Month, model.WODate.Day, 0, 0, 0);
                model.DateCompleted = new DateTime(model.DateCompleted.Year, model.DateCompleted.Month, model.DateCompleted.Day, 23, 59, 59);
                StatusResponse response = _factory.Export(ref wsWO, ref wsWOD, model, listStore, listCompanyId);
                if (!response.Status)
                {
                    ModelState.AddModelError("", response.MsgError);
                    return View(model);
                }

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Work_Orders").Replace(" ", "_")));

                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                return RedirectToAction("Export");
            }
            catch (Exception e)
            {
                _logger.Error("WorkOrderExport: " + e);
                ModelState.AddModelError("ListStores", "Export file have error.");
                return View(model);
            }
        }
    }
}