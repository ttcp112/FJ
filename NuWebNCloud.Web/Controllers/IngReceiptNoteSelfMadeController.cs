using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngReceiptNoteSelfMadeController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private ReceiptNoteSelfMadeFactory _factory = null;
        private IngredientFactory _ingredientFactory = null;
        private RecipeFactory _recipeFactory = null;
        private InventoryFactory _inventoryFactory = null;

        List<SelectListItem> lstStore = new List<SelectListItem>();
        List<string> listStoreId = new List<string>();

        public IngReceiptNoteSelfMadeController()
        {
            _factory = new ReceiptNoteSelfMadeFactory();
            _ingredientFactory = new IngredientFactory();
            _recipeFactory = new RecipeFactory();
            _inventoryFactory = new InventoryFactory();

            ViewBag.ListStore = GetListStore();
            lstStore = ViewBag.ListStore;
            //==========
        }

        // GET: IngReceiptNoteSelfMadeSelfMade
        public ActionResult Index()
        {
            try
            {
                ReceiptNoteSelfMadeViewModels model = new ReceiptNoteSelfMadeViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("ReceiptNoteSelfMade_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(ReceiptNoteSelfMadeViewModels model)
        {
            try
            {
                listStoreId = new List<string>();
                List<ReceiptNoteSelfMadeModels> data = new List<ReceiptNoteSelfMadeModels>();
                if (!string.IsNullOrEmpty(model.StoreID))
                {
                    listStoreId.Add(model.StoreID);
                    data = _factory.GetData(model, listStoreId);
                }

                model.ListItem = data;
                CurrentUser.StoreId = model.StoreID;
            }
            catch (Exception e)
            {
                _logger.Error("ReceiptNoteSelfMade_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult LoadDetail(string ReceiptNoteSelfMadeId)
        {
            var model = new ReceiptNoteSelfMadeModels();
            try
            {
                model = _factory.GetReceiptNoteSelfMadeById(ReceiptNoteSelfMadeId);
                model.StoreName = lstStore.Where(x => x.Value.Equals(model.StoreId)).FirstOrDefault().Text;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_PopUpDetail", model);
        }

        public ActionResult Create()
        {
            ReceiptNoteSelfMadeModels model = new ReceiptNoteSelfMadeModels();
            model.StoreId = CurrentUser.StoreId;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ReceiptNoteSelfMadeModels model)
        {
            try
            {
                model.ListItemForSelect = null;
                RecipeIngredientUsageModels _objIngredientDependent = null;
                model.CreatedBy = CurrentUser.UserName;
                model.UpdatedBy = CurrentUser.UserName;
                model.ReceiptBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;
                model.ReceiptDate = DateTime.Now;
                string msg = "";
                model.ListItem = model.ListItem.Where(x => x.Status != (int)Commons.EStatus.Deleted && x.Status != null).ToList();

                //Check stock before save
                //var lstIngredientSeftMadeIds = model.ListItem.Where(ww => ww.IsSelfMode && ww.IsStockAble).Select(ss => ss.IngredientId).ToList();
                var lstIngredientSeftMade = model.ListItem.Where(ww => ww.IsSelfMode && ww.IsStockAble).ToList();
                if (lstIngredientSeftMade != null && lstIngredientSeftMade.Any())
                {

                    double currentQty = 0;
                    string mess = string.Empty;
                    //List<string> lstIngredientName = new List<string>();
                    List<ErrorEnoughModels> lstIngredientName = new List<ErrorEnoughModels>();
                    _objIngredientDependent = _recipeFactory.GetRecipesByIngredientSeftMade(lstIngredientSeftMade);

                    bool resultCheck = _inventoryFactory.CheckStockBeforeRNSeftMade(model.StoreId, _objIngredientDependent.ListChilds
                        , ref currentQty, ref lstIngredientName);
                    if (!resultCheck)
                    {
                        if (lstIngredientName != null && lstIngredientName.Any())
                        {
                            foreach (var item in lstIngredientName)
                            {
                                if (item.ListIngredientNameNotEnough != null && item.ListIngredientNameNotEnough.Count > 1)
                                    mess = string.Format("Stock of [{0}] are not enough", string.Join(", ", item.ListIngredientNameNotEnough.ToArray()));
                                else
                                    mess = string.Format("Stock of [{0}] is not enough", string.Join(", ", item.ListIngredientNameNotEnough.ToArray()));

                                ModelState.AddModelError("error_msg" + item.MixIngredientId, mess);
                            }
                        }
                        //    if (lstIngredientName != null && lstIngredientName.Count > 1)
                        //{
                        //    mess = string.Format("Stock of [{0}] are not enough", string.Join(", ",lstIngredientName.ToArray()));
                        //}
                        //else
                        //    mess = string.Format("Stock of [{0}] is not enough", string.Join(", ", lstIngredientName.ToArray()));
                    }
                }
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                //check item for WO
                var lstWoId = model.ListItem.Where(ww => !string.IsNullOrEmpty(ww.WOId)).Select(ss => ss.WOId).ToList();
                if (lstWoId != null && lstWoId.Any() && model.ListWorkOrder != null && model.ListWorkOrder.Any())
                {
                    lstWoId = lstWoId.Distinct().ToList();
                    model.ListWorkOrder = model.ListWorkOrder.Where(ww => lstWoId.Contains(ww.Id)).ToList();
                }
                bool result = _factory.Insert(model, _objIngredientDependent, ref msg);
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
                _logger.Error("ReceiptNoteSelfMade_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }


        public ActionResult LoadIngredient()
        {
            var lstComId = GetListCompany().Select(ss => ss.Value).ToList();
            ReceiptNoteSelfMadeModels model = new ReceiptNoteSelfMadeModels();
            var listIng = _ingredientFactory.GetIngredientSelfMade(lstComId);
            foreach (var item in listIng)
            {
                var itemDetail = new ReceiptNoteSelfMadeDetailModels
                {
                    BaseUOM = item.ReceivingUOMName,
                    IngredientId = item.Id,
                    IngredientName = item.Name,
                    IngredientCode = item.Code,

                    //BaseReceivingQty = item.ReceivingQty,
                    BaseQty = item.ReceivingQty,
                    IsSelfMode = item.IsSelfMode,
                    IsStockAble = item.IsStockable,
                };
                model.ListItemForSelect.Add(itemDetail);
            }
            model.ListItemForSelect = model.ListItemForSelect.OrderByDescending(x => x.IsActived ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
            return PartialView("_TableChooseIngredient", model);
        }

        public ActionResult AddIngredient(ReceiptNoteSelfMadeModels data)
        {
            ReceiptNoteSelfMadeModels model = new ReceiptNoteSelfMadeModels();
            model.ListItem = new List<ReceiptNoteSelfMadeDetailModels>();
            int OffSet = 0;
            foreach (var item in data.ListItem)
            {
                var itemDetail = new ReceiptNoteSelfMadeDetailModels
                {
                    Id = item.Id,

                    WOId = item.WOId,
                    WONumber = item.WONumber,

                    IngredientId = item.IngredientId,
                    IngredientName = item.IngredientName,
                    IngredientCode = item.IngredientCode,

                    IsActived = item.IsActived,
                    BaseReceivingQty = item.BaseReceivingQty,
                    IsSelfMode = item.IsSelfMode,
                    IsStockAble = item.IsStockAble,

                    ReceivingQty = item.ReceivingQty,
                    BaseQty = item.ReceivingQty,

                    BaseUOM = item.BaseUOM,
                    Status = (byte)Commons.EStatus.Actived,
                    Qty = item.Qty,
                    RemainingQty = item.RemainingQty,
                    OffSet = OffSet++
                };
                model.ListItem.Add(itemDetail);
            }
            return PartialView("_ListItem", model);
        }

        public ActionResult LoadWorkOrder(/*string StoreId, string WONo, */ReceiptNoteSelfMadeModels data)
        {
            ReceiptNoteSelfMadeModels model = new ReceiptNoteSelfMadeModels();
            model.ListWorkOrder = _factory.LoadWOForRN(data.StoreId, null, data.WONo);
            model.ListItem = data.ListItem;
            return PartialView("_ListWO", model);
        }

        public ActionResult AddReceiptNoteSelfMade(ReceiptNoteSelfMadeModels data)
        {
            ReceiptNoteSelfMadeModels model = new ReceiptNoteSelfMadeModels();
            int OffSet = 0;
            model.ListItem = new List<ReceiptNoteSelfMadeDetailModels>();
            List<string> lstWOIds = new List<string>();
            if (data.ListItem != null && data.ListItem.Count > 0)
            {
                model.ListItem = data.ListItem;
                //OffSet = data.ListItem.Count;
                OffSet = OffSet++;
                lstWOIds = model.ListItem.Where(ww => !string.IsNullOrEmpty(ww.WOId)).Select(x => x.WOId).ToList();
            }
            if (lstWOIds == null)
                lstWOIds = new List<string>();
            foreach (var item in data.ListWorkOrder)
            {
                if (!lstWOIds.Contains(item.Id))
                {
                    var ListItemDetail = _factory.GetData(item.Id);
                    //if (IngredientIds.Count > 0)
                    //{
                    //    ListItemDetail = ListItemDetail.Where(x => !IngredientIds.Contains(x.IngredientId) && x.WOId == item.Id).ToList();
                    //}
                    //item.ListItemForRN= ListItemDetail; 
                    foreach (var ItemDetail in ListItemDetail)
                    {
                        model.ListItem.Add(new ReceiptNoteSelfMadeDetailModels
                        {
                            WOId = item.Id,
                            WONumber = item.WONumber,

                            Id = ItemDetail.Id,
                            IngredientId = ItemDetail.IngredientId,
                            IngredientName = ItemDetail.IngredientName,
                            IngredientCode = ItemDetail.IngredientCode,
                            Qty = ItemDetail.Qty,
                            RemainingQty = ItemDetail.RemainingQty,

                            //IsActived = ItemDetail.IsActived,
                            //BaseReceivingQty = ItemDetail.BaseReceivingQty,
                            IsSelfMode = ItemDetail.IsSelfMode,
                            IsStockAble = ItemDetail.IsStockAble,

                            ReceivingQty = ItemDetail.ReceivingQty,
                            BaseQty = ItemDetail.BaseQty,

                            BaseUOM = ItemDetail.BaseUOM,
                            Status = (byte)Commons.EStatus.Actived,
                            OffSet = OffSet++
                        });

                    }
                }
            }
            return PartialView("_ListItem", model);
        }

    }
}