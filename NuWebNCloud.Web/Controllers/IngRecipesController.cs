using NLog;
using NuWebNCloud.Shared;
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
    public class IngRecipesController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        RecipeFactory _factory = null;
        UnitOfMeasureFactory _UOMFactory = null;
        IngredientFactory _IngredientFactory = null;

        List<SelectListItem> lstStore = new List<SelectListItem>();
        List<string> listStoreId = new List<string>();
        public IngRecipesController()
        {
            _factory = new RecipeFactory();
            _UOMFactory = new UnitOfMeasureFactory();
            _IngredientFactory = new IngredientFactory();

            ViewBag.ListStore = GetListStore();
            //==========
            lstStore = ViewBag.ListStore;
            listStoreId = lstStore.Select(x => x.Value).ToList();
            ViewBag.IsAction = Commons.IsAction;
        }

        /*Region Of Product:  SetMenu | Dish | Modifier*/
        public ActionResult Product()
        {
            try
            {
                RecipeProductViewModels model = new RecipeProductViewModels();
                model.StoreID = CurrentUser.StoreId;
                model.Type = (byte)Commons.EProductType.Dish;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Recipe_Product: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        public ActionResult Modifier()
        {
            try
            {
                RecipeProductViewModels model = new RecipeProductViewModels();
                model.StoreID = CurrentUser.StoreId;
                model.Type = (byte)Commons.EProductType.Modifier;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Recipe_Modifier: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(RecipeProductViewModels model)
        {
            try
            {
                var data = _factory.GetDataProduct(model.StoreID, model.Type, CurrentUser.ListOrganizationId, listStoreId);
                model.ListItem = data;
            }
            catch (Exception e)
            {
                _logger.Error("Recipe_Product_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult LoadIngredient(string ProductId, string StoreId, int productType)
        {
            IngredientFactory IngFactory = new IngredientFactory();
            var listProductRecipe = _factory.GetListRecipeProduct(ProductId, StoreId, productType, listStoreId);
            RecipeProductIngredientViewModels model = new RecipeProductIngredientViewModels();
            var m_CompanyIds = GetListCompany().Select(x=> x.Value).ToList();
            var listIng = new List<IngredientModel>();
            if (m_CompanyIds.Count > 0)
                listIng = IngFactory.GetIngredient("").Where(x => m_CompanyIds.Contains(x.CompanyId)).ToList();
            else
                listIng = IngFactory.GetIngredient("").ToList();
            listIng = listIng.Where(x => x.IsActive == true).ToList();

            foreach (var item in listIng)
            {

                var ProIngre = new ProductIngredient()
                {
                    BaseUOM = item.BaseUOMName,

                    IngredientId = item.Id,
                    IngredientName = item.Name,
                    Usage = listProductRecipe.Where(x => x.IngredientId.Equals(item.Id)).FirstOrDefault() == null
                                                    ? 0 : listProductRecipe.FirstOrDefault(x => x.IngredientId.Equals(item.Id)).Usage,

                    BaseUOMId = listProductRecipe.Where(x => x.IngredientId.Equals(item.Id)).FirstOrDefault() == null
                                                    ? item.BaseUOMId : listProductRecipe.FirstOrDefault(x => x.IngredientId.Equals(item.Id)).UOMId,

                    IsSelect = listProductRecipe.Any(x => x.IngredientId.Equals(item.Id))
                };
                var lstItem = _UOMFactory.GetDataUOMRecipe(item.Id).ToList();
                if (lstItem != null)
                {
                    foreach (UnitOfMeasureModel uom in lstItem)
                        ProIngre.ListUOM.Add(new SelectListItem
                        {
                            Text = uom.Name,
                            Value = uom.Id,
                            Selected = uom.Id.Equals(ProIngre.BaseUOMId) == true ? true : false
                        });
                }
                model.ListItem.Add(ProIngre);
            }
            model.ListItem = model.ListItem.OrderByDescending(x => x.IsSelect ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
            return PartialView("_TableChooseIngredient", model);
        }

        [HttpPost]
        public ActionResult AddIngredient(RecipeProductIngredientViewModels data)
        {
            List<string> listUpdate = new List<string>();
            List<string> listUpdateModifier = new List<string>();

            List<RecipeProductModels> listInfo = new List<RecipeProductModels>();
            int type = 0;
            data.ListItem = data.ListItem.Where(x => x.IsSelect).ToList();
            foreach (var item in data.ListItem)
            {
                RecipeProductModels model = new RecipeProductModels
                {
                    StoreId = data.StoreId,
                    ItemId = data.ProductId,
                    ItemName = data.ProductName,
                    ItemType = (byte)Commons.EProductType.Dish,

                    IngredientId = item.IngredientId,
                    UOMId = item.BaseUOM,
                    Usage = item.Usage,
                    BaseUsage = item.Usage
                };

                double BaseUsage = _IngredientFactory.GetUsageUOMForIngredient(model.IngredientId, model.UOMId, ref type);
                if (type != 0)
                {
                    model.BaseUsage = (BaseUsage * model.Usage);
                }
                var itemDb = _factory.CheckInsertProduct(model, data.Type, listStoreId);
                if (itemDb == null) //Insert
                {
                    if (item.IsSelect)
                    {
                        model.CreatedBy = CurrentUser.UserId;
                        model.CreatedDate = DateTime.Now;
                        model.UpdatedBy = CurrentUser.UserId;
                        model.UpdatedDate = DateTime.Now;

                        string Id = "";
                        _factory.InsertRecipeProduct(model, data.Type, ref Id);
                        if (data.Type == (byte)Commons.EProductType.Dish)
                        {
                            listUpdate.Add(Id);
                        }
                        else if (data.Type == (byte)Commons.EProductType.Modifier)
                        {
                            listUpdateModifier.Add(Id);
                        }
                    }
                }
                else //Update
                {
                    if (item.IsSelect)
                    {
                        model.UpdatedBy = CurrentUser.UserId;
                        model.UpdatedDate = DateTime.Now;
                        model.Id = itemDb.Id;
                        _factory.UpdateRecipeProduct(model, data.Type);
                        if (data.Type == (byte)Commons.EProductType.Dish)
                        {
                            listUpdate.Add(itemDb.Id);
                        }
                        else if (data.Type == (byte)Commons.EProductType.Modifier)
                        {
                            listUpdateModifier.Add(itemDb.Id);
                        }
                    }
                    //else
                    //{
                    //    model.UpdatedBy = CurrentUser.UserId;
                    //    model.UpdatedDate = DateTime.Now;
                    //    model.Id = itemDb.Id;
                    //    _factory.DeleteRecipeProduct(model, data.Type);
                    //}
                }
            }

            //Delete
            if (data.Type == (byte)Commons.EProductType.Dish)
            {
                _factory.DeleteListIdRecipeProduct(data.ProductId, data.StoreId, listUpdate, (byte)Commons.EProductType.Dish);
            }
            else if (data.Type == (byte)Commons.EProductType.Modifier)
            {
                _factory.DeleteListIdRecipeProduct(data.ProductId, data.StoreId, listUpdateModifier, (byte)Commons.EProductType.Modifier);
            }
            //Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        /*Region Of Ingredient*/
        public ActionResult Ingredient()
        {
            try
            {
                var lstCompanyIds = GetListCompany().Select(x => x.Value).ToList();
                RecipeIngredientViewModels model = new RecipeIngredientViewModels();
                var datas = _factory.GetListRecipeIngredient(lstCompanyIds);
                model.ListItem = datas;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Recipe_Ingredient: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult LoadIngredientIngredient(string Id)
        {
            IngredientFactory IngFactory = new IngredientFactory();
            var listProductRecipe = _factory.GetListRecipeIngredient(Id);

            RecipeProductIngredientViewModels model = new RecipeProductIngredientViewModels();
            var listIng = IngFactory.GetIngredientUnlessData(Id);
            //listIng = listIng.Where(x => x.IsActive == true).ToList();

            var m_CompanyIds = GetListCompany().Select(x => x.Value).ToList();
            if (m_CompanyIds.Count > 0)
                listIng = listIng.Where(x => m_CompanyIds.Contains(x.CompanyId)).ToList();

            foreach (var item in listIng)
            {
                var ProIngre = new ProductIngredient()
                {

                    BaseUOM = item.BaseUOMName,
                    IngredientId = item.Id,
                    IngredientName = item.Name,

                    Usage = listProductRecipe.Where(x => x.IngredientId.Equals(item.Id)).FirstOrDefault() == null
                                                    ? 0 : Math.Round(listProductRecipe.FirstOrDefault(x => x.IngredientId.Equals(item.Id)).Usage, 4),
                    BaseUsage = listProductRecipe.Where(x => x.IngredientId.Equals(item.Id)).FirstOrDefault() == null
                                                    ? 0 : Math.Round(listProductRecipe.FirstOrDefault(x => x.IngredientId.Equals(item.Id)).BaseUsage, 4),

                    BaseUOMId = listProductRecipe.Where(x => x.IngredientId.Equals(item.Id)).FirstOrDefault() == null
                                                    ? item.BaseUOMId : listProductRecipe.FirstOrDefault(x => x.IngredientId.Equals(item.Id)).UOMId,

                    IsSelect = listProductRecipe.Any(x => x.IngredientId.Equals(item.Id))
                };
                var lstItem = _UOMFactory.GetDataUOMRecipe(item.Id).ToList();
                if (lstItem != null)
                {
                    foreach (UnitOfMeasureModel uom in lstItem)
                        ProIngre.ListUOM.Add(new SelectListItem
                        {
                            Text = uom.Name,
                            Value = uom.Id,
                            Selected = uom.Id.Equals(ProIngre.BaseUOMId) == true ? true : false
                        });
                }

                model.ListItem.Add(ProIngre);
            }
            model.ListItem = model.ListItem.OrderByDescending(x => x.IsSelect ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
            return PartialView("_TableChooseIngredient", model);
        }

        [HttpPost]
        public ActionResult AddIngredientIngredient(RecipeIngredientIngredientViewModels data)
        {
            List<string> listUpdate = new List<string>();
            List<RecipeProductModels> listInfo = new List<RecipeProductModels>();
            data.ListItem = data.ListItem.Where(x => x.IsSelect).ToList();
            int type = 0;
            foreach (var item in data.ListItem)
            {
                RecipeIngredientModels model = new RecipeIngredientModels
                {
                    MixtureIngredientId = data.Id,
                    UOMId = item.BaseUOM,

                    IngredientId = item.IngredientId,
                    Usage = item.Usage,
                    BaseUsage = item.Usage
                };

                double BaseUsage = _IngredientFactory.GetUsageUOMForIngredient(model.IngredientId, model.UOMId, ref type);
                if (type != 0)
                {
                    model.BaseUsage = (BaseUsage * model.Usage);
                }

                var itemDb = _factory.CheckInsertIngredient(model);
                if (itemDb == null) //Insert
                {
                    if (item.IsSelect)
                    {
                        model.CreatedBy = CurrentUser.UserId;
                        model.CreatedDate = DateTime.Now;
                        model.UpdatedBy = CurrentUser.UserId;
                        model.UpdatedDate = DateTime.Now;
                        string Id = "";
                        _factory.InsertRecipeIngredient(model, ref Id);
                        listUpdate.Add(Id);
                    }
                }
                else //Update
                {
                    if (item.IsSelect)
                    {
                        model.UpdatedBy = CurrentUser.UserId;
                        model.UpdatedDate = DateTime.Now;
                        model.Id = itemDb.Id;
                        _factory.UpdateRecipeIngredient(model);
                        listUpdate.Add(itemDb.Id);
                    }
                    //else
                    //{
                    //    model.UpdatedBy = CurrentUser.UserId;
                    //    model.UpdatedDate = DateTime.Now;
                    //    model.Id = itemDb.Id;
                    //    _factory.DeleteRecipeIngredient(model);
                    //}
                }
            }
            //===========
            //Delete
            _factory.DeleteListIdRecipeIngredient(data.Id, listUpdate);
            //Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

    }
}