using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Integration.Factory.Sandbox.Product;
using NuWebNCloud.Shared.Models.Settings.RSVPProductMapping;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SRSVPProductMappingController : SBInventoryBaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private InteProductFactory _factory = null;
        private RSVPProductMappingFactory _factoryRSVP = null;

        public SRSVPProductMappingController()
        {
            _factory = new InteProductFactory();
            _factoryRSVP = new RSVPProductMappingFactory();

            ViewBag.ListStore = GetListStoreIntegration();
        }

        public ActionResult Index()
        {
            try
            {
                RSVPProductMappingViewModels model = new RSVPProductMappingViewModels();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("RSVPProductMapping_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(RSVPProductMappingViewModels model)
        {
            try
            {
                var result = (List<SelectListItem>)ViewBag.ListStore;
                List<string> ListStoreId = result.Select(x => x.Value).ToList();
                model.ListItem = _factoryRSVP.GetList(ListStoreId, CurrentUser.ListOrganizationId);
                model.ListItem.ForEach(x =>
                {
                    x.StoreName = result == null ? "" : result.Where(z => z.Value.Equals(x.StoreID)).FirstOrDefault().Text;
                });
            }
            catch (Exception e)
            {
                _logger.Error("RSVPProductMapping_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            RSVPProductMappingModels model = new RSVPProductMappingModels();
            var lstStore = (List<SelectListItem>)ViewBag.ListStore;
            model.ListStoreView = lstStore;
            //==============
            List<string> ListStoreId = lstStore.Select(x => x.Value).ToList();
            var data = _factoryRSVP.GetList(ListStoreId, CurrentUser.ListOrganizationId);
            data = data.Where(x => x.ListRSVPProductMapping.Count > 0).ToList();
            model.ListStoreView.ForEach(k =>
            {
                if (!k.Selected)
                    k.Selected = data.Select(x => x.StoreID).ToList().Any(x => x.Equals(k.Value));
            });
            if (data != null && data.Count > 0)
            {
                int OffSet = 0;
                data.ForEach(x =>
                {
                    x.OffSet = OffSet++;
                    int OffSetProduct = 0;
                    var storeInfo = model.ListStoreView.Where(o => o.Value.Equals(x.StoreID)).FirstOrDefault();
                    x.StoreName = storeInfo == null ? "" : storeInfo.Text;
                    x.ListRSVPProductMapping.ForEach(z =>
                    {
                        z.StoreOffSet = x.OffSet;
                        z.OffSet = OffSetProduct++;
                        z.ProductName = z.ProductCode;
                        z.StoreID = x.StoreID;
                    });
                });
            }
            model.ListRSVPStoreProductMapping = data;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(RSVPProductMappingModels model)
        {
            try
            {
                model.ListStoreView = model.ListStoreView.Where(x => x.Selected).ToList();
                if (model.ListStoreView.Count == 0)
                    ModelState.AddModelError("ListStoreView", (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store")));

                model.ListRSVPStoreProductMapping = model.ListRSVPStoreProductMapping.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                if (model.ListRSVPStoreProductMapping != null && model.ListRSVPStoreProductMapping.Count > 0)
                {
                    foreach (var itemProduct in model.ListRSVPStoreProductMapping)
                    {
                        if (itemProduct.ListRSVPProductMapping != null)
                        {
                            itemProduct.ListRSVPProductMapping = itemProduct.ListRSVPProductMapping.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                        }
                    }
                }
                if (!ModelState.IsValid)
                {
                    var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                    model.ListStoreView = lstStore;
                    return View(model);
                }
                //====================
                model.Type = (byte)Commons.EMappingType.RSVP;
                string msg = "";
                bool result = _factoryRSVP.InsertOrUpdate(model, CurrentUser.UserId, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", msg);
                    var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                    model.ListStoreView = lstStore;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("RSVPProductMapping_Create: " + ex);
                var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                model.ListStoreView = lstStore;
                ModelState.AddModelError("Name", (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have an error")));
                return View(model);
            }
        }

        public RSVPStoreProducMappingModels GetDetail(string id)
        {
            try
            {
                var result = (List<SelectListItem>)ViewBag.ListStore;
                List<string> ListStoreId = result.Select(x => x.Value).ToList();
                var data = _factoryRSVP.GetList(ListStoreId, CurrentUser.ListOrganizationId);
                RSVPStoreProducMappingModels model = new RSVPStoreProducMappingModels();
                if (data != null && data.Count > 0)
                {
                    model = data.Where(x => x.StoreID.Equals(id)).FirstOrDefault();
                    model.StoreName = result == null ? "" : result.Where(z => z.Value.Equals(id)).FirstOrDefault().Text;
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("RSVPProductMapping_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            RSVPStoreProducMappingModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        /*Add More Store Product Mapping*/
        public ActionResult AddMoreStoreProductMapping(int currentOffset, string StoreID, string StoreName)
        {
            RSVPStoreProducMappingModels model = new RSVPStoreProducMappingModels();
            model.OffSet = currentOffset;
            model.StoreName = StoreName;
            model.StoreID = StoreID;
            return PartialView("_StoreProductMapping", model);
        }
        /*End Add More Store Product Mapping*/

        /*Add Item RSVP*/
        public ActionResult AddItemRSVP(int currentOffset, string StoreID, int StoreOffSet)
        {
            ProductItemModels productItem = new ProductItemModels();
            productItem.OffSet = currentOffset;
            productItem.StoreID = StoreID;
            productItem.StoreOffSet = StoreOffSet;
            return PartialView("_ItemRSVP", productItem);
        }

        public ActionResult LoadItemProduct(string StoreID)
        {
            var listProductDish = _factory.GetProductApplyStore(StoreID, (byte)Commons.EProductType.Dish);
            var listProduct = listProductDish;
            var listProductSetMenu = _factory.GetProductApplyStore(StoreID, (byte)Commons.EProductType.SetMenu);
            listProduct.AddRange(listProductSetMenu);

            RSVPStoreProducMappingModels model = new RSVPStoreProducMappingModels();
            if (listProduct != null)
            {
                model.ListRSVPProductMapping = new List<ProductItemModels>();
                foreach (var item in listProduct)
                {
                    ProductItemModels product = new ProductItemModels()
                    {
                        ProductID = item.ID,
                        ProductName = item.Name,
                        ProductCode = item.ProductCode,
                        ProductType = item.ProductType
                    };
                    model.ListRSVPProductMapping.Add(product);
                }
            }
            model.ListRSVPProductMapping = model.ListRSVPProductMapping.OrderBy(x => x.ProductName).ToList();
            return PartialView("_ListItemProduct", model);
        }
        /*End Add Item RSVP*/

        /*For Clone*/
        public ActionResult AddItemRSVPClone(ProductItemModels model)
        {
            ProductItemModels productItem = new ProductItemModels();
            productItem.OffSet = model.OffSet;
            productItem.StoreID = model.StoreID;
            productItem.StoreOffSet = model.StoreOffSet;
            productItem.ItemCode = model.ItemCode;
            //=======
            var listProductDish = _factory.GetProductApplyStore(model.StoreID, (byte)Commons.EProductType.Dish);
            var listProduct = listProductDish;
            var listProductSetMenu = _factory.GetProductApplyStore(model.StoreID, (byte)Commons.EProductType.SetMenu);
            listProduct.AddRange(listProductSetMenu);
            var product = listProduct.Where(x => x.ProductCode.Equals(model.ProductName)).FirstOrDefault();

            productItem.ID = product == null ? "" : product.ID;
            productItem.ProductID = product == null ? "" : product.ID;
            productItem.ProductName = product == null ? "" : product.ProductCode;
            productItem.ProductCode = product == null ? "" : product.ProductCode;

            return PartialView("_ItemRSVP", productItem);
        }
        /*End For Clone*/
    }
}