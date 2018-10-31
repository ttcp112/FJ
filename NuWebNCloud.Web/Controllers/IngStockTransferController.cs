using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Factory.Sandbox;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Sandbox;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngStockTransferController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private StockTransferFactory _factory = null;
        private InventoryFactory _InventoryFactory = null;
        UnitOfMeasureFactory _UOMFactory = null;
        EmployeeFactory _EmployeeFactory = null;

        //List<SelectListItem> lstStoreIssue = new List<SelectListItem>();
        List<StoreModels> lstStoreReceiving = new List<StoreModels>();
        List<string> listStoreIssueId = new List<string>();
        List<string> listStoreReceiveId = new List<string>();

        public IngStockTransferController()
        {
            _factory = new StockTransferFactory();
            _InventoryFactory = new InventoryFactory();
            _UOMFactory = new UnitOfMeasureFactory();
            _EmployeeFactory = new EmployeeFactory();

            //lstStoreReceiving = GetListStoreForTransfer();
            //lstStoreIssue.AddRange(lstStoreReceiving);

            //if (base.CurrentUser != null)
            //{
            //    lstStoreIssue = lstStoreIssue.Where(ww => CurrentUser.ListStoreID.Contains(ww.Value)).ToList();
            //}
            //ViewBag.ListStore = lstStoreIssue;
            //ViewBag.ListStoreReceived = lstStoreReceiving;

            lstStoreReceiving = GetListStoreForTransfer();
            ViewBag.ListStoreReceived = new SelectList(lstStoreReceiving, "Id", "Name", "CompanyName", 1);
            //==========
            //lstStore = ViewBag.ListStore;
            if (lstStoreReceiving != null && lstStoreReceiving.Any())
            {
                listStoreReceiveId = lstStoreReceiving.Select(x => x.Id).ToList();
                listStoreIssueId.AddRange(listStoreReceiveId);
                if (CurrentUser != null)
                {
                    listStoreIssueId = listStoreIssueId.Where(ww => CurrentUser.ListStoreID.Contains(ww)).ToList();
                }
            }
        }

        // GET: IngStackTransfer
        public ActionResult Index()
        {
            try
            {
                StockTransferViewModels model = new StockTransferViewModels();
                model.IssuingStoreId = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("StockTransfer_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(StockTransferViewModels model)
        {
            try
            {
                if (model.IssuingStoreId != null
                    || model.ReceivingStoreId != null || !string.IsNullOrEmpty(model.StockTransferNo)
                        || model.ApplyFrom != null || model.ApplyTo != null)
                {
                    var data = _factory.GetData(model, listStoreIssueId, listStoreReceiveId);
                    data.ForEach(x =>
                    {
                        var issueStore = lstStoreReceiving.Where(z => z.Id.Equals(x.IssueStoreId)).FirstOrDefault();
                        if (issueStore != null)
                        {
                            x.IssueStoreName = issueStore.Name;
                        }
                        var receiveStore = lstStoreReceiving.Where(z => z.Id.Equals(x.ReceiveStoreId)).FirstOrDefault();
                        if (receiveStore != null)
                        {
                            x.ReceiveStoreName = receiveStore.Name;
                        }
                    });
                    model.ListItem = data;
                    CurrentUser.StoreId = model.IssuingStoreId;
                }
            }
            catch (Exception e)
            {
                _logger.Error("StockTransfer_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }


        public ActionResult LoadIngredient(List<string> ListItemNew, string IssuingStoreId)
        {

            STIngredientViewModels model = new STIngredientViewModels();
            var listIng = _InventoryFactory.LoadIngredient(IssuingStoreId);
            foreach (var item in listIng)
            {
                var itemDetail = new StockTransferDetailModels
                {
                    UOMId = item.BaseUOMId,
                    BaseUOM = item.BaseUOMName,
                    IngredientId = item.Id,
                    IngredientName = item.Name,
                    IngredientCode = item.Code
                };
                model.ListItemView.Add(itemDetail);
            }
            if (ListItemNew != null)
            {
                model.ListItemView = model.ListItemView.Where(x => !ListItemNew.Contains(x.IngredientId)).ToList();
            }
            model.ListItemView = model.ListItemView.OrderByDescending(x => x.IsSelect ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
            return PartialView("_TableChooseIngredient", model);
        }

        public ActionResult AddIngredient(STIngredientViewModels data)
        {
            StockTransferModels model = new StockTransferModels();
            model.ListItem = new List<StockTransferDetailModels>();
            foreach (var item in data.ListItemView)
            {
                var itemDetail = new StockTransferDetailModels
                {
                    Id = "", // Add New

                    IngredientId = item.IngredientId,
                    IngredientName = item.IngredientName,
                    IngredientCode = item.IngredientCode,

                    IsSelect = item.IsSelect,
                    RequestQty = item.RequestQty,
                    IssueQty = item.IssueQty,
                    ReceiveQty = item.ReceiveQty,

                    UOMId = item.UOMId,
                    BaseUOM = item.BaseUOM,
                };
                var lstItem = _UOMFactory.GetDataUOMRecipe(item.IngredientId).ToList();
                if (lstItem != null)
                {
                    foreach (UnitOfMeasureModel uom in lstItem)
                        itemDetail.ListUOM.Add(new SelectListItem
                        {
                            Text = uom.Name,
                            Value = uom.Id
                        });
                }
                model.ListItem.Add(itemDetail);
            }
            return PartialView("_ListIngredients", model);
        }

        public StockTransferModels GetDetail(string id)
        {
            try
            {
                StockTransferModels model = _factory.GetDetail(id);
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("StockTransfer_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            StockTransferModels model = GetDetail(id);
            var issueStore = lstStoreReceiving.Where(z => z.Id.Equals(model.IssueStoreId)).FirstOrDefault();
            if (issueStore != null)
            {
                model.IssueStoreName = issueStore.Name;
            }
            var receiveStore = lstStoreReceiving.Where(z => z.Id.Equals(model.ReceiveStoreId)).FirstOrDefault();
            if (receiveStore != null)
            {
                model.ReceiveStoreName = receiveStore.Name;
            }
            //==========
            List<EmployeeModels> listEmp = _EmployeeFactory.GetListEmployee(model.IssueStoreId, null, CurrentUser.ListOrganizationId);
            var Emp = listEmp.Where(x => x.ID.Equals(model.IssueBy)).FirstOrDefault();
            model.IssueBy = Emp == null ? "" : Emp.Name;

            listEmp = _EmployeeFactory.GetListEmployee(model.ReceiveStoreId, null, CurrentUser.ListOrganizationId);
            var EmpRequestBy = listEmp.Where(x => x.ID.Equals(model.RequestBy)).FirstOrDefault();
            model.RequestBy = EmpRequestBy == null ? "" : EmpRequestBy.Name;

            var EmpReceiveBy = listEmp.Where(x => x.ID.Equals(model.ReceiveBy)).FirstOrDefault();
            model.ReceiveBy = EmpReceiveBy == null ? "" : EmpReceiveBy.Name;

            return PartialView("_View", model);
        }

        public ActionResult Create()
        {
            StockTransferModels model = new StockTransferModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(StockTransferModels model)
        {
            try
            {

                if (string.IsNullOrEmpty(model.RequestBy))
                {
                    ModelState.AddModelError("RequestBy", CurrentUser.GetLanguageTextFromKey("Request By field is required"));
                }
                if (string.IsNullOrEmpty(model.IssueBy))
                {
                    ModelState.AddModelError("IssueBy", CurrentUser.GetLanguageTextFromKey("Issue By field is required"));
                }
                if (string.IsNullOrEmpty(model.ReceiveBy))
                {
                    ModelState.AddModelError("ReceiveBy", CurrentUser.GetLanguageTextFromKey("Receive By field is required"));
                }
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                double qtyCurrentStock = 0, rate = 1;
                bool isCheck = true;
                string ingredientName = string.Empty;
                model.ListItem = model.ListItem.Where(ww => ww.Delete != (int)Commons.EStatus.Deleted).ToList();
                //Check Stock before send
                foreach (var item in model.ListItem)
                {
                    ingredientName = string.Empty;
                    qtyCurrentStock = 0;
                    rate = 1;

                    isCheck = _InventoryFactory.CheckStockBeforeTransfer(model.IssueStoreId, item.IngredientId
                        , item.UOMId, item.IssueQty, ref qtyCurrentStock, ref ingredientName, ref rate);

                    item.Rate = rate;
                    if (!isCheck)
                    {
                        ModelState.AddModelError("error_msg", string.Format("[{0}]" + CurrentUser.GetLanguageTextFromKey("not enough stock transfer") + "!", ingredientName));
                        break;
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                model.IsActive = true;
                string msg = "";
                bool result = _factory.Insert(model, ref msg);
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
                _logger.Error("StockTransfer_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        //public PartialViewResult Edit(string id)
        //{
        //    StockTransferModels model = GetDetail(id);
        //    return PartialView("_Edit", model);
        //}

        //[HttpGet]
        //public PartialViewResult Delete(string id)
        //{
        //    StockTransferModels model = GetDetail(id);
        //    return PartialView("_Delete", model);
        //}


        //Issuing Warehouse
        public ActionResult LoadEmployee(string StoreID, string empId = null)
        {
            List<EmployeeModels> lstData = new List<EmployeeModels>();
            if (!string.IsNullOrEmpty(StoreID))
            {
                lstData  = _EmployeeFactory.GetListEmployee(StoreID, null, CurrentUser.ListOrganizationId);
            }

            StockTransferModels model = new StockTransferModels();
            if (!string.IsNullOrEmpty(empId))
            {
                model.IssueBy = empId;
            }
            if (lstData != null && lstData.Any())
            {
                lstData = lstData.OrderBy(oo => oo.Name).ToList();
                foreach (EmployeeModels emp in lstData)
                {
                    model.ListEmployee.Add(new SelectListItem
                    {
                        Value = emp.ID,
                        Text = emp.Name,
                        Selected = false
                    });
                }
            }
                
            return PartialView("_DDLEmployee", model);
        }

        //Receive Warehouse
        public ActionResult LoadEmployee02(string StoreID)
        {
            List<EmployeeModels> lstData = new List<EmployeeModels>();
            if (!string.IsNullOrEmpty(StoreID))
            {
                lstData = _EmployeeFactory.GetListEmployee(StoreID, null, CurrentUser.ListOrganizationId);
            }
            return Json(lstData, JsonRequestBehavior.AllowGet);
        }
    }
}