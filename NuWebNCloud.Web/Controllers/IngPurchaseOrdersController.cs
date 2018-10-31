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
    public class IngPurchaseOrdersController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private PurchaseOrderFactory _factory = null;
        private SupplierFactory _Supplierfactory = null;
        private StoreFactory _Storefactory = null;
        private DefaultCurrencyFactory _defaultCurrencyFactory = null;
        private Shared.Factory.Settings.TaxFactory _factoryTax = null;
        List<StoreModels> lstStore = new List<StoreModels>();
        List<string> listStoreId = new List<string>();

        List<SelectListItem> lstCompany = new List<SelectListItem>();
        List<string> listCompanyId = new List<string>();

        List<SelectListItem> lstCur = new List<SelectListItem>();

        public IngPurchaseOrdersController()
        {
            _factory = new PurchaseOrderFactory();
            _Supplierfactory = new SupplierFactory();
            _Storefactory = new StoreFactory();
            _defaultCurrencyFactory = new DefaultCurrencyFactory();
            _factoryTax = new Shared.Factory.Settings.TaxFactory();
            //ViewBag.ListStore = GetListStore();
            //lstStore = ViewBag.ListStore;
            //listStoreId = lstStore.Select(x => x.Value).ToList();

            lstStore = (List<StoreModels>)ViewBag.StoreID.Items;
            listStoreId = lstStore.Select(x => x.Id).ToList();

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
                PurchaseOrderViewModels model = new PurchaseOrderViewModels();
                model.StoreID = CurrentUser.StoreId;
                model.ApplyFrom = DateTime.Now;
                model.ApplyTo = DateTime.Now;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("PurchaseOrderIndex: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(PurchaseOrderViewModels model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.StoreID) || model.ApplyFrom.HasValue || model.ApplyTo.HasValue)
                {
                    model.ListItem = _factory.GetData(model.StoreID, listCompanyId, model.ApplyFrom, model.ApplyTo);
                    model.ListItem.ForEach(x =>
                    {
                        var currency = lstCur.Where(z => z.Value.Equals(x.Store)).FirstOrDefault();
                        x.Symbol = currency == null ? "$" : currency.Text;
                        //x.StoreName = lstStore.Where(ww => ww.Value == x.StoreID).Select(ss => ss.Text).FirstOrDefault();
                        x.StoreName = lstStore.Where(ww => ww.Id == x.StoreID).Select(ss => ss.Name).FirstOrDefault();
                    });

                    CurrentUser.StoreId = model.StoreID;
                }

            }
            catch (Exception e)
            {
                _logger.Error("PurchaseOrderSearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult LoadIngredient(LoadIngredientModel input)
        {
            IngredientFactory IngFactory = new IngredientFactory();
            POIngredientViewModels model = new POIngredientViewModels();
            //var listIng = IngFactory.GetIngredient("").Where(x => x.IsActive).ToList();

            var listIng = IngFactory.GetIngredientBySupplier(input.SupplierId, input.StoreId);
            foreach (var item in listIng)
            {
                model.ListItemView.Add(new POIngredient
                {
                    BaseUOM = item.ReceivingUOMName,
                    IngredientId = item.Id,
                    IngredientName = item.Name,
                    PurchasePrice = item.PurchasePrice,

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
            PurchaseOrderModels model = new PurchaseOrderModels();
            model.ListItem = new List<POIngredient>();
            //--------update
            bool IsIntegrate = false;
            bool _IsShowTax = false;
            var objStore = CurrentUser.listStore.Where(x => x.ID.Equals(data.StoreId)).FirstOrDefault();
            if (objStore != null)
            {
                var objThirdParty = objStore.ThirdParty;
                if (objThirdParty != null && !string.IsNullOrEmpty(objThirdParty.ApiURL))
                {
                    IsIntegrate = objThirdParty.IsIntegrate;
                }
            }
            List<SelectListItem> listItemTax = new List<SelectListItem>();
            if (IsIntegrate)
            {
                _IsShowTax = true;
                var lstTaxV2 = _factoryTax.GetListTaxV2(data.StoreId, null, CurrentUser.ListOrganizationId);
                lstTaxV2 = lstTaxV2.Where(x => x.IsActive).ToList();
                if (lstTaxV2 != null)
                {
                    foreach (Shared.Models.Settings.TaxModels tax in lstTaxV2)
                    {
                        listItemTax.Add(new SelectListItem
                        {
                            Text = tax.Name,
                            Value = tax.ID,
                        });
                    }
                }
            }
            //--------updated
            foreach (var item in data.ListItemView)
            {
                model.ListItem.Add(new POIngredient
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

                    Delete = item.Delete,
                    //--updated
                    TaxId = item.TaxId,
                    TaxName = item.TaxName,
                    TaxType = item.TaxType,
                    TaxAmount = item.TaxAmount,
                    TaxPercent = item.TaxPercent,
                    ListTax = listItemTax,
                    IsShowTax = _IsShowTax
                });
            }

            return PartialView("_ListItemNew", model);
        }

        //===
        //public ActionResult GetTaxInfo(string id)
        //{
        //    var taxDetail = _factoryTax.GetListTaxV2(null, id);
        //    return Json(taxDetail, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Create()
        {
            PurchaseOrderModels model = new PurchaseOrderModels();
            //model.GetListSupplierFromCompnay(listCompanyId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(PurchaseOrderModels model)
        {
            try
            {
                if (!model.StoreIntegrate)
                {
                    if (model.TaxType == (int)Commons.ETax.AddOn)
                    {
                        if (model.TaxValue < 0 || model.TaxValue > 100)
                        {
                            ModelState.AddModelError("TaxValue", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0 and maximun 100%"));
                        }
                    }
                }
                if (model.DeliveryDate.Date < model.PODate.Date)
                {
                    ModelState.AddModelError("DeliveryDate", CurrentUser.GetLanguageTextFromKey("Delivery date cannot be sooner than PO date"));
                }
                if (!ModelState.IsValid)
                {
                    //model.GetListSupplierFromCompnay(listCompanyId);
                    var ListSupplierInfo = GetListSuppliers(model.StoreID);
                    var ListSupplier = new SelectList(ListSupplierInfo, "Id", "Name");
                    model.ListSupplier = ListSupplier.ToList();

                    return View(model);
                }
                //====================
                string msg = "";
                model.CreatedBy = CurrentUser.Email;
                model.ModifierBy = CurrentUser.Email;
                model.CreatedDate = DateTime.Now;
                model.ModifierDate = DateTime.Now;
                model.POStatus = (int)Commons.EPOStatus.Open;
                model.IsActived = true;
                //=====
                model.ListItem = model.ListItem.Where(x => x.Delete != (int)Commons.EStatus.Deleted).ToList();
                //---------
                //var objStore = lstStore.Where(ww => ww.Value == model.StoreID).FirstOrDefault();
                var objStore = lstStore.Where(ww => ww.Id == model.StoreID).FirstOrDefault();
                if (objStore != null)
                    //model.StoreName = objStore.Text;
                    model.StoreName = objStore.Name;
                //-----
                bool result = _factory.Insert(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //model.GetListSupplierFromCompnay(listCompanyId);
                    var ListSupplierInfo = GetListSuppliers(model.StoreID);
                    var ListSupplier = new SelectList(ListSupplierInfo, "Id", "Name");
                    model.ListSupplier = ListSupplier.ToList();

                    ModelState.AddModelError("StoreID", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("PurchaseOrderCreate: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public PurchaseOrderModels GetDetail(string id)
        {
            try
            {
                PurchaseOrderModels model = _factory.GetDetail(id, listCompanyId);
                //model.GetListSupplierFromCompnay(listCompanyId);
                var ListSupplierInfo = GetListSuppliers(model.StoreID);
                var ListSupplier = new SelectList(ListSupplierInfo, "Id", "Name");
                model.ListSupplier = ListSupplier.ToList();
                //--------------
                bool IsIntegrate = false;
                var objStore = CurrentUser.listStore.Where(x => x.ID.Equals(model.StoreID)).FirstOrDefault();
                if (objStore != null && !string.IsNullOrEmpty(objStore.ThirdParty.ApiURL))
                    IsIntegrate = objStore.ThirdParty.IsIntegrate;

                List<SelectListItem> listItemTax = new List<SelectListItem>();
                if (IsIntegrate)
                {
                    model.StoreIntegrate = true;
                    //---------------------
                    var lstTaxV2 = _factoryTax.GetListTaxV2(model.StoreID, null, CurrentUser.ListOrganizationId);
                    lstTaxV2 = lstTaxV2.Where(x => x.IsActive).ToList();
                    string Symbol = GetSymbol(model.StoreID);
                    List<POTaxModels> listItem = new List<POTaxModels>();
                    int OffSet = 0;
                    lstTaxV2.ForEach(x =>
                    {
                        var amount = model.ListItem.Where(z => z.TaxId.Equals(x.ID)).Sum(z => z.TaxAmount);
                        listItem.Add(new POTaxModels
                        {
                            ID = x.ID,
                            Name = x.Name,
                            Percent = x.Percent,
                            TaxType = x.TaxType,
                            OffSet = OffSet++,
                            Symbol = Symbol,
                            sTaxType = (x.TaxType == (int)Commons.ETax.Inclusive) ? "Inclusive" : "Exclusive",
                            Amount = Math.Round(amount, 2),
                            IsShow = amount > 0 ? true : false
                        });

                        //----for Dropdownlist
                        listItemTax.Add(new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.ID,
                        });
                    });
                    model.ListItemTax = listItem;
                    model.TaxAmount = Math.Round(model.ListItemTax.Sum(z => z.Amount), 2);
                }
                //--------
                model.ListItem.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.TaxId))
                    {
                        var objTax = model.ListItemTax.Where(z => z.ID.Equals(x.TaxId)).FirstOrDefault();
                        x.TaxName = objTax != null ? objTax.Name : string.Empty;
                        x.TaxType = objTax != null ? objTax.TaxType : 0;
                    }
                    //-----
                    x.ListTax = listItemTax;
                    x.IsShowTax = IsIntegrate;
                });

                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("PurchaseOrderDetail: " + ex);
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
            PurchaseOrderModels model = GetDetail(id);
            model.Supplier = new SupplierModels();
            model.Supplier = _Supplierfactory.GetDetail(model.SupplierId);
            model.Symbol = GetSymbol(model.StoreID);
            model.Store = new SStoreModels();
            model.Store = _Storefactory.GetListStores(null, model.StoreID)[0];
            if (model.TaxType == (int)Commons.ETax.Inclusive)
            {
                model.TaxAmountInclusive = string.Format("{0} {1:N2}", model.Symbol, (model.SubTotal - (model.SubTotal / (1 + (model.TaxValue / 100)))));
            }
            //============
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            PurchaseOrderModels model = GetDetail(id);
            model.Symbol = "";
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(PurchaseOrderModels model)
        {
            try
            {
                if (!model.StoreIntegrate)
                {
                    if (model.TaxType == (int)Commons.ETax.AddOn)
                    {
                        if (model.TaxValue < 0 || model.TaxValue > 100)
                        {
                            ModelState.AddModelError("TaxValue", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0 and maximun 100%"));
                        }
                    }
                }
                if (model.DeliveryDate.Date < model.PODate.Date)
                {
                    ModelState.AddModelError("DeliveryDate", CurrentUser.GetLanguageTextFromKey("Delivery date cannot be sooner than PO date"));
                }
                model.ModifierBy = CurrentUser.Email;
                model.ModifierDate = DateTime.Now;
                if (!ModelState.IsValid)
                {
                    //model = GetDetail(model.Id);
                    var ListSupplierInfo = GetListSuppliers(model.StoreID);
                    var ListSupplier = new SelectList(ListSupplierInfo, "Id", "Name");
                    model.ListSupplier = ListSupplier.ToList();
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
                    //model = GetDetail(model.Id);
                    var ListSupplierInfo = GetListSuppliers(model.StoreID);
                    var ListSupplier = new SelectList(ListSupplierInfo, "Id", "Name");
                    model.ListSupplier = ListSupplier.ToList();
                    ModelState.AddModelError("StoreID", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("PurchaseOrderEdit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            PurchaseOrderModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }
        [HttpPost]
        public ActionResult Delete(PurchaseOrderModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.Delete(model.Id, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("PONumber", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("PurchaseOrderDelete: " + ex);
                ModelState.AddModelError("PONumber", CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Purchase Order"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        //====ChangeStatus PO
        [HttpPost]
        public ActionResult ChangeStatus(List<string> lstId, int status)
        {
            var listStore = (SelectList)ViewBag.StoreID;
            List<SelectListItem> listStoreItm = listStore.ToList();
            var data = _factory.ChangeStatus(lstId, status, listStoreItm);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Export()
        {
            PurchaseOrderModels model = new PurchaseOrderModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(PurchaseOrderModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsPO = wb.Worksheets.Add("PO List");
                var wsPOD = wb.Worksheets.Add("Ingredient List");
                //var wsPOR = wb.Worksheets.Add("Purchase Order Receipt");

                List<SelectListItem> listStore = (List<SelectListItem>)ViewBag.ListStore;

                model.PODate = new DateTime(model.PODate.Year, model.PODate.Month, model.PODate.Day, 0, 0, 0);
                model.DeliveryDate = new DateTime(model.DeliveryDate.Year, model.DeliveryDate.Month, model.DeliveryDate.Day, 23, 59, 59);

                StatusResponse response = _factory.Export(ref wsPO, ref wsPOD, /*ref wsPOR,*/ model, listStore, listCompanyId);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("PurchaseOrder").Replace(" ", "_")));

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
                _logger.Error("PurchaseOrderExport: " + e);
                ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Export file have error"));
                return View(model);
            }
        }

        public List<SupplierModels> GetListSuppliers(string storeId)
        {
            List<SupplierModels> lstData = new List<SupplierModels>();
            if (!string.IsNullOrEmpty(storeId))
            {
                var companyId = lstStore.Where(ww => ww.Id == storeId).Select(ss => ss.CompanyId).FirstOrDefault();
                lstData = _Supplierfactory.GetData(companyId);

                if (lstData != null && lstData.Any())
                {
                    lstData = lstData.OrderBy(oo => oo.Name).ToList();
                }
            }
            return lstData;
        }

        public ActionResult LoadSuppliers(string storeId)
        {
            var lstData = GetListSuppliers(storeId);
            return Json(lstData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckStoreIntegrate(string storeId)
        {
            bool IsIntegrate = false;
            var objStore = CurrentUser.listStore.Where(x => x.ID.Equals(storeId)).FirstOrDefault();
            if (objStore != null && !string.IsNullOrEmpty(objStore.ThirdParty.ApiURL))
                IsIntegrate = objStore.ThirdParty.IsIntegrate;
            return Json(IsIntegrate, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListTaxStoreIntegrate(string storeId)
        {
            PurchaseOrderModels model = new PurchaseOrderModels();
            var lstTaxV2 = _factoryTax.GetListTaxV2(storeId, null, CurrentUser.ListOrganizationId);
            lstTaxV2 = lstTaxV2.Where(x => x.IsActive).ToList();
            string Symbol = GetSymbol(storeId);
            List<POTaxModels> listItem = new List<POTaxModels>();
            int OffSet = 0;
            lstTaxV2.ForEach(x =>
            {
                listItem.Add(new POTaxModels
                {
                    ID = x.ID,
                    Name = x.Name,
                    Percent = x.Percent,
                    TaxType = x.TaxType,
                    OffSet = OffSet++,
                    Symbol = Symbol,
                    sTaxType = (x.TaxType == (int)Commons.ETax.Inclusive) ? "Inclusive" : "Exclusive",
                    Amount = 0
                });
            });
            model.ListItemTax = listItem;
            return PartialView("_ListItemTax", model);
        }
    }
}