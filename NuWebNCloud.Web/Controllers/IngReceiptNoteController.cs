using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngReceiptNoteController : HQController
    {
        //private static Logger _logger = LogManager.GetCurrentClassLogger();

        private ReceiptNoteFactory _factory = null;
        private ReceiptNoteDetailFactory _Detailfactory = null;
        private PurchaseOrderFactory _POfactory = null;
        private PurchaseOrderDetailFactory _PODFactory = null;
        private ReturnNoteFactory _RTFactory = null;
        private InventoryFactory _InventoryFactory = null;
        private IngredientFactory _IngredientFactory = null;
        private UnitOfMeasureFactory _UOMFactory = null;

        List<SelectListItem> lstStore = new List<SelectListItem>();
        List<string> listStoreId = new List<string>();
        List<SelectListItem> lstCompany = new List<SelectListItem>();
        List<string> listCompanyId = new List<string>();

        public IngReceiptNoteController()
        {
            _factory = new ReceiptNoteFactory();
            _Detailfactory = new ReceiptNoteDetailFactory();
            _POfactory = new PurchaseOrderFactory();
            _PODFactory = new PurchaseOrderDetailFactory();
            _RTFactory = new ReturnNoteFactory();
            _InventoryFactory = new InventoryFactory();
            _UOMFactory = new UnitOfMeasureFactory();
            _IngredientFactory = new IngredientFactory();

            ViewBag.ListStore = GetListStore();
            lstStore = ViewBag.ListStore;
            //listStoreId = lstStore.Select(x => x.Value).ToList();
            //==========
            lstCompany = GetListCompany();
            listCompanyId = lstCompany.Select(x => x.Value).ToList();
        }

        public ActionResult Index()
        {
            try
            {
                ReceiptNoteViewModels model = new ReceiptNoteViewModels();
                model.StoreID = CurrentUser.StoreId;

                var ListSupplierInfo = GetListSuppliers(model.StoreID);
                var ListSupplier = new SelectList(ListSupplierInfo, "Id", "Name");
                model.ListSupplier = ListSupplier.ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ReceiptNote_Index Error: ", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(ReceiptNoteViewModels model)
        {
            try
            {
                listStoreId = new List<string>();
                List<ReceiptNoteModels> data = new List<ReceiptNoteModels>();
                if (!string.IsNullOrEmpty(model.StoreID))
                {
                    listStoreId.Add(model.StoreID);
                    data = _factory.GetData(model, listStoreId, listCompanyId);
                    data.ForEach(x =>
                    {
                        if (!x.IsPurchaseOrder)
                        {
                            x.SupplierName = "";
                        }
                    });
                }

                model.ListItem = data;
                CurrentUser.StoreId = model.StoreID;

            }
            catch (Exception e)
            {
                NSLog.Logger.Error("ReceiptNote_Search: " , e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult LoadDetail(string ReceiptNoteId)
        {
            var model = new ReceiptNoteModels();
            try
            {
                model = _factory.GetReceiptNoteById(ReceiptNoteId, listCompanyId);
                model.StoreName = lstStore.Where(x => x.Value.Equals(model.StoreId)).FirstOrDefault().Text;
                //Choose PO
                if (model.IsPurchaseOrder)
                {
                    foreach (var item in model.ListPurchaseOrder)
                    {
                        item.ListItem.ForEach(x => x.RemainingQty += x.QtyToleranceP);
                    }
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("LoadDetail in RN",ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView(model.IsPurchaseOrder ? "_PopUpDetail" : "_PopUpDetailItem", model);
        }

        public ActionResult LoadPurchaseOrder(string StoreId, string SupplierId, string PONo)
        {
            ReceiptNoteModels model = new ReceiptNoteModels();
            //List<string> ListPONo = new List<string>();
            //foreach (var PONumber in PONo.Split(','))
            //{
            //    if (!PONumber.Equals(""))
            //    {
            //        ListPONo.Add(PONumber);
            //    }
            //}
            model.ListPurchaseOrder = _POfactory.LoadPOForRN(StoreId, SupplierId, null, PONo);
            return PartialView("_ListPO", model);
        }

        public ActionResult AddReceiptNote(ReceiptNoteModels data)
        {
            ReceiptNoteModels model = new ReceiptNoteModels();
            model.ListPurchaseOrder = new List<PurchaseOrderModels>();
            foreach (var item in data.ListPurchaseOrder)
            {
                var ListItemDetail = _PODFactory.GetData(item.Id);
                model.ListPurchaseOrder.Add(new PurchaseOrderModels
                {
                    Id = item.Id,
                    PONumber = item.PONumber,
                    Delete = 0,
                    ListItem = ListItemDetail
                });
            }
            return PartialView("_ListPODetail", model);
        }

        public ActionResult LoadIngredient(ReceiptNoteModels data)
        {
            IngredientFactory IngFactory = new IngredientFactory();

            //Get Data from Client
            List<ReceiptNoteIngredient> listReceiptNoteIngre = listReceiptNoteIngre = data.ListItem;

            ReceiptNoteModels model = new ReceiptNoteModels();

            //var m_CompanyIds = GetListCompany().Select(x => x.Value).ToList();
            //var listIng = new List<IngredientModel>();
            //if (m_CompanyIds.Count > 0)
            //    listIng = IngFactory.GetIngredient("").Where(x => m_CompanyIds.Contains(x.CompanyId)).ToList();
            //else
            //    listIng = IngFactory.GetIngredient("").ToList();

            ////==== Only get list Ingredients of the company of Receipt Note Store
            var lstStoreInfo = (List<StoreModels>)ViewBag.StoreID.Items;
            var compId = lstStoreInfo.Where(w => w.Id == data.StoreId).Select(s => s.CompanyId).FirstOrDefault();

            var listIng = new List<IngredientModel>();
            if (!string.IsNullOrEmpty(compId))
            {
                listIng = _IngredientFactory.GetIngredient("").Where(x => x.CompanyId == compId).ToList();
                if (listIng != null && listIng.Any())
                {
                    listIng = listIng.Where(x => x.IsActive == true && !x.IsSelfMode).ToList();

                    foreach (var item in listIng)
                    {
                        var ProIngre = new ReceiptNoteIngredient()
                        {
                            BaseUOM = item.BaseUOMName,
                            IngredientId = item.Id,
                            IngredientCode = item.Code,
                            IngredientName = item.Name,
                            Description = item.Description,

                            //=======Set Qty
                            Qty = listReceiptNoteIngre.Where(x => x.IngredientId.Equals(item.Id)).FirstOrDefault() == null
                                                            ? 1 : listReceiptNoteIngre.FirstOrDefault(x => x.IngredientId.Equals(item.Id)).Qty,

                            //=====Set UOM: Priority| ReceivingUOM -> BaseUOM
                            BaseUOMId = listReceiptNoteIngre.Where(x => x.IngredientId.Equals(item.Id)).FirstOrDefault() != null
                                                            ? listReceiptNoteIngre.FirstOrDefault(x => x.IngredientId.Equals(item.Id)).BaseUOMId
                                                            : item.ReceivingUOMId,
                            //=======Set Ing Select
                            IsSelect = listReceiptNoteIngre.Any(x => x.IngredientId.Equals(item.Id))
                        };

                        var lstItem = _UOMFactory.GetDataUOMRecipe(item.Id).ToList();
                        if (lstItem != null)
                        {
                            foreach (UnitOfMeasureModel uom in lstItem)
                            {
                                ProIngre.ListUOM.Add(new SelectListItem
                                {
                                    Text = uom.Name,
                                    Value = uom.Id,
                                    //Selected = uom.Id.Equals(ProIngre.BaseUOMId) ? true :  false
                                });
                            }
                            //=====Reset UOM from BaseUOM If lstItemUOM not ReceivingUOM
                            var isExists = lstItem.Exists(z => z.Id.Equals(ProIngre.BaseUOMId));
                            if (!isExists)
                                ProIngre.BaseUOMId = item.BaseUOMId;
                        }
                        //======
                        model.ListItem.Add(ProIngre);
                    }
                    model.ListItem = model.ListItem.OrderByDescending(x => x.IsSelect ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
                }
            }
            
            return PartialView("_TableChooseIngredient", model);
        }

        [HttpPost]
        public ActionResult AddIngredient(ReceiptNoteModels data)
        {
            ReceiptNoteModels model = new ReceiptNoteModels();
            if (data.ListItem != null && data.ListItem.Count > 0)
            {
                foreach (var item in data.ListItem)
                {
                    var ProIngre = new ReceiptNoteIngredient()
                    {
                        IngredientId = item.IngredientId,
                        IngredientCode = item.IngredientCode,
                        IngredientName = item.IngredientName,
                        Description = item.Description,
                        Qty = (item.Qty == 0) ? 1 : item.Qty, // Receiving Quantity
                        BaseUOMId = item.BaseUOMId
                    };
                    //=====================
                    var lstItem = _UOMFactory.GetDataUOMRecipe(item.IngredientId).ToList();
                    if (lstItem != null)
                    {
                        foreach (UnitOfMeasureModel uom in lstItem)
                        {
                            ProIngre.ListUOM.Add(new SelectListItem
                            {
                                Text = uom.Name,
                                Value = uom.Id,
                                //Selected = uom.Id.Equals(ProIngre.BaseUOMId) ? true : false
                            });
                        }
                    }
                    //=========
                    model.ListItem.Add(ProIngre);
                }
            }
            return PartialView("_ListItem", model);
        }

        //public ActionResult ApproveReceiptNote(string ReceiptNoteId)
        //{
        //    InventoryFactory inventoryFactory = new InventoryFactory();
        //    var receiptNote = _factory.GetReceiptNoteById(ReceiptNoteId);
        //    var listDetail = _Detailfactory.GetReceiptNoteDetailData(ReceiptNoteId);
        //    List<InventoryModels> listInModel = new List<InventoryModels>();
        //    foreach (var item in listDetail)
        //    {
        //        InventoryModels inModel = new InventoryModels
        //        {
        //            IngredientId = item.IngredientId,
        //            Quantity = item.Quantity,
        //            StoreId = receiptNote.StoreId
        //        };
        //        listInModel.Add(inModel);
        //    }
        //    bool result = inventoryFactory.SaveInventory(listInModel, ReceiptNoteId);
        //    if (result)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.OK);
        //    }
        //    else
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //}

        public ActionResult Create()
        {
            ReceiptNoteModels model = new ReceiptNoteModels();
            //model.GetListSupplierFromCompnay(listCompanyId);
            //model.StoreId = CurrentUser.StoreId;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ReceiptNoteModels model)
        {
            try
            {
                model.CreatedBy = CurrentUser.UserName;
                model.UpdatedBy = CurrentUser.UserName;
                model.ReceiptBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;
                model.ReceiptDate = DateTime.Now;
                model.ListPurchaseOrder.ForEach(x =>
                {
                    x.ListItem = x.ListItem.Where(z => !z.IsVisible).ToList();
                });
                string msg = "";
                model.ListPurchaseOrder = model.ListPurchaseOrder.Where(x => x.ListItem.Count > 0 && x.Delete != (int)Commons.EStatus.Deleted).ToList();
                //===================
                if (!model.IsPurchaseOrder)
                {
                    model.ListItem = model.ListItem.Where(x => x.Delete != (int)Commons.EStatus.Deleted).ToList();
                    foreach (var itemIngredient in model.ListItem)
                    {
                        int type = 0;
                        double BaseUsage = _IngredientFactory.GetUsageUOMForIngredient(itemIngredient.IngredientId, itemIngredient.BaseUOMId, ref type);
                        itemIngredient.BaseReceivingQty = type != 0 ? (BaseUsage * itemIngredient.Qty) : (1 * itemIngredient.Qty);
                    }
                }

                List<string> lstStoreId = lstStore.Select(z => z.Value).ToList();
                bool result = _factory.Insert(model, lstStoreId, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //model.GetListSupplierFromCompnay(listCompanyId);
                    var ListSupplierInfo = GetListSuppliers(model.StoreId);
                    var ListSupplier = new SelectList(ListSupplierInfo, "Id", "Name");
                    model.ListSupplier = ListSupplier.ToList();

                    ModelState.AddModelError("StoreId", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ReceiptNote_Create: ", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        //public ReceiptNoteModels GetDetail(string id)
        //{
        //    try
        //    {
        //        ReceiptNoteModels model = _factory.GetReceiptNoteById(id);
        //        return model;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("ReceiptNote_Detail: " + ex);
        //        return null;
        //    }
        //}

        //[HttpGet]
        //public PartialViewResult View(string id)
        //{
        //    ReceiptNoteModels model = GetDetail(id);
        //    return PartialView("_View", model);
        //}

        //[HttpPost]
        //public ActionResult Delete(ReceiptNoteModels model)
        //{
        //    try
        //    {
        //        string msg = "";
        //        var result = _factory.GetReceiptNoteById(model.Id);
        //        //if (!result)
        //        //{
        //        //    //ModelState.AddModelError("Name", "Have a error when you delete a ReceiptNote");
        //        //    ModelState.AddModelError("Name", msg);
        //        //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        //    return PartialView("_Delete", model);
        //        //}
        //        return new HttpStatusCodeResult(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("ReceiptNote_Delete: " + ex);
        //        ModelState.AddModelError("Name", "Have an error when you delete a ReceiptNote");
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return PartialView("_Delete", model);
        //    }
        //}

        public ActionResult Export()
        {
            ReceiptNoteModels model = new ReceiptNoteModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(ReceiptNoteModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsRN = wb.Worksheets.Add("Receipt Note List");
                var wsRND = wb.Worksheets.Add("Item List");

                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);

                StatusResponse response = _factory.Export(ref wsRN, ref wsRND, /*ref wsPOR,*/ model, lstStore, listCompanyId);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("ReceiptNote").Replace(" ", "_")));

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
                NSLog.Logger.Error("ReceiptNoteExport Error: ", e);
                ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Export file have error."));
                return View(model);
            }
        }

        #region RETURN NOTE
        public ActionResult CreateReturnNote(string id)
        {
            ReceiptNoteModels model = new ReceiptNoteModels();
            if (!string.IsNullOrEmpty(id))
            {
                model = _factory.GetReceiptNoteReturnNote(id, listCompanyId);
                foreach (var item in model.ListPurchaseOrder)
                {
                    item.ListItem.ForEach(x => x.RemainingQty += x.QtyToleranceP);
                }
            }
            //return PartialView("_CreateReturnNote", model);
            return View("_CreateReturnNote", model);
        }

        [HttpPost]
        public ActionResult CreateReturnNote(ReceiptNoteModels model)
        {
            try
            {

                List<PurchaseOrderModels> _ListPurchaseOrder = new List<PurchaseOrderModels>();
                //_ListPurchaseOrder.InsertRange(0, model.ListPurchaseOrder);
                foreach (var item in model.ListPurchaseOrder)
                {
                    var obj = new PurchaseOrderModels();
                    obj.Additional = item.Additional;
                    obj.AdditionalReason = item.AdditionalReason;
                    obj.ColorAlert = item.ColorAlert;
                    obj.CreatedBy = item.CreatedBy;
                    obj.CreatedDate = item.CreatedDate;
                    obj.Delete = item.Delete;
                    obj.DeliveryDate = item.DeliveryDate;
                    obj.Id = item.Id;
                    obj.IsActived = item.IsActived;
                    obj.ListItem = item.ListItem;
                    obj.ListReceiptPO = item.ListReceiptPO;
                    obj.ListStores = item.ListStores;
                    obj.ListSupplier = item.ListSupplier;
                    obj.ListTaxType = item.ListTaxType;
                    obj.ModifierBy = item.ModifierBy;
                    obj.ModifierDate = item.ModifierDate;
                    obj.Note = item.Note;
                    obj.PODate = item.PODate;
                    obj.PONumber = item.PONumber;
                    obj.POStatus = item.POStatus;
                    obj.Store = item.Store;
                    obj.StoreID = item.StoreID;
                    obj.StoreName = item.StoreName;
                    obj.SubTotal = item.SubTotal;
                    obj.Supplier = item.Supplier;
                    obj.SupplierId = item.SupplierId;
                    obj.SupplierName = item.SupplierName;
                    obj.Symbol = item.Symbol;
                    obj.TaxAmount = item.TaxAmount;
                    obj.TaxType = item.TaxType;
                    obj.TaxValue = item.TaxValue;
                    obj.Total = item.Total;

                    _ListPurchaseOrder.Add(obj);

                }

                model.CreatedBy = CurrentUser.UserName;
                model.UpdatedBy = CurrentUser.UserName;
                model.ReceiptBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;
                model.ReceiptDate = DateTime.Now;

                model.ListPurchaseOrder.ForEach(x =>
                {
                    x.ListItem = x.ListItem.Where(z => z.ReturnQty > 0).ToList();
                });
                //==========
                ReturnNoteModels RTmodel = new ReturnNoteModels()
                {
                    ReceiptNoteId = model.Id,
                    CreatedBy = CurrentUser.UserName,
                    CreatedDate = DateTime.Now,
                    ModifierBy = CurrentUser.UserName,
                    ModifierDate = DateTime.Now,
                    IsActived = true
                };
                RTmodel.ListPurchaseOrder = model.ListPurchaseOrder.Where(x => x.ListItem.Count > 0).ToList();
                bool result = false;
                string msg = "";
                double qtyCurrentStock = 0;
                bool isCheck = true;
                string ingredientName = string.Empty;

                if (RTmodel.ListPurchaseOrder.Count > 0)
                {
                    foreach (var poDetail in RTmodel.ListPurchaseOrder)
                    {
                        foreach (var item in poDetail.ListItem)
                        {

                            if (item.ReturnQty < 0)
                            {
                                ModelState.AddModelError("error_msg" + item.Id, CurrentUser.GetLanguageTextFromKey("Please, can not input negative number"));
                                break;
                            }
                            if (item.ReturnQty > item.ReceivingQty)
                            {
                                ModelState.AddModelError("error_msg" + item.Id, CurrentUser.GetLanguageTextFromKey("Can not return more than receiving quantity"));
                                break;
                            }
                            ingredientName = string.Empty;
                            qtyCurrentStock = 0;

                            isCheck = _InventoryFactory.CheckStockForReturn(model.StoreId, item.IngredientId
                                , item.ReturnQty, item.BaseQty, ref qtyCurrentStock, ref ingredientName);

                            if (!isCheck)
                            {
                                ModelState.AddModelError("error_msg" + item.Id, string.Format("[{0}] " + CurrentUser.GetLanguageTextFromKey("is not enough stock to return!"), ingredientName));
                                break;
                            }
                        }
                    }
                    if (!ModelState.IsValid)
                    {
                        model.ListPurchaseOrder = _ListPurchaseOrder;
                        //return PartialView("_CreateReturnNote", model);
                        return View("_CreateReturnNote", model);
                    }
                    result = _RTFactory.Insert(RTmodel, model.StoreId, ref msg);
                }

                else
                    result = true;
                //===
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("StoreId", msg);
                    return View("_CreateReturnNote", model);
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ReturnNote_Create: ", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult ViewReturnNote(ReturnNoteReceiptView RTNRmodel)
        {
            ReceiptNoteModels model = new ReceiptNoteModels();
            try
            {
                model = _factory.GetViewReceiptNoteReturnNote(RTNRmodel);
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ViewReturnNote", model);
        }
        #endregion

        public List<SupplierModels> GetListSuppliers(string storeId)
        {
            List<SupplierModels> lstData = new List<SupplierModels>();
            if (!string.IsNullOrEmpty(storeId))
            {
                var lstStoreInfo = (List<StoreModels>)ViewBag.StoreID.Items;
                var companyId = lstStoreInfo.Where(ww => ww.Id == storeId).Select(ss => ss.CompanyId).FirstOrDefault();
                SupplierFactory supplierFactory = new SupplierFactory();
                lstData = supplierFactory.GetData(companyId);

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
    }
}