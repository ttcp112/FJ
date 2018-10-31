using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class RecipeFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public RecipeFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<RecipeProductModels> GetDataProduct(string StoreId = null, int TypeID = -1, List<string> ListOrganizationId = null, List<string> ListStoreId = null)
        {
            List<RecipeProductModels> listData = new List<RecipeProductModels>();
            try
            {
                ProductApiModels paraBody = new ProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductTypeID = TypeID.ToString();
                paraBody.StoreID = StoreId;
                paraBody.IsShowInReservation = false;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetProduct, null, paraBody);
                dynamic data = result.Data;
                var lstData = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstData);

                List<ProductModels> listProduct = JsonConvert.DeserializeObject<List<ProductModels>>(lstContent);

                using (var cxt = new NuWebContext())
                {
                    List<RecipeProductModels> listRecipeProduct = new List<RecipeProductModels>();
                    if (TypeID == (byte)Commons.EProductType.Dish)
                    {
                        listRecipeProduct = cxt.I_Recipe_Item
                                       .Where(s => s.Status == (byte)Commons.EStatus.Actived && ListStoreId.Contains(s.StoreId))
                                       .Select(ss => new RecipeProductModels()
                                       {
                                           ItemId = ss.ItemId,
                                           StoreId = ss.StoreId,
                                           Status = ss.Status
                                       }).ToList();
                    }
                    else if (TypeID == (byte)Commons.EProductType.Modifier)
                    {
                        listRecipeProduct = cxt.I_Recipe_Modifier
                            .Where(s => s.Status == (byte)Commons.EStatus.Actived && ListStoreId.Contains(s.StoreId))
                           .Select(ss => new RecipeProductModels()
                           {
                               Id = ss.Id,
                               ItemId = ss.ModifierId,
                               StoreId = ss.StoreId,
                               Status = ss.Status
                           }).ToList();
                    }
                    listData = (from d in listProduct
                                let _Ingredient = (
                                                   from r in listRecipeProduct
                                                   where r.StoreId.Equals(d.StoreID) && r.ItemId.Equals(d.ID)
                                                        && r.Status == (byte)Commons.EStatus.Actived
                                                   select r.Id).Count()
                                where d.IsActive == true && d.Status != (byte)Commons.EStatus.Deleted && ListStoreId.Contains(d.StoreID)
                                select new RecipeProductModels
                                {
                                    ItemId = d.ID,
                                    StoreId = d.StoreID,

                                    ItemCode = d.ProductCode,
                                    ItemName = d.Name,
                                    StoreName = d.StoreName,

                                    Ingredient = _Ingredient
                                }).ToList();
                    listData = listData.OrderByDescending(x => x.Ingredient).ToList();
                }

                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("RecipeProduct_GetList: " + e);
                return listData;
            }
        }

        public List<RecipeProductModels> GetListRecipeProduct(string ProductId, string StoreId, int ProductType, List<string> ListStoreId)
        {
            List<RecipeProductModels> listResults = new List<RecipeProductModels>();
            using (NuWebContext cxt = new NuWebContext())
            {
                if (ProductType == (byte)Commons.EProductType.Dish)
                {
                    var query = (from p in cxt.I_Recipe_Item
                                 where p.ItemId.Equals(ProductId) && p.StoreId.Equals(StoreId)
                                        && p.Status == (byte)Commons.EStatus.Actived
                                        && ListStoreId.Contains(p.StoreId)
                                 select new RecipeProductModels
                                 {
                                     IngredientId = p.IngredientId,
                                     Usage = p.Usage,
                                     UOMId = p.UOMId,
                                     BaseUsage = p.BaseUsage ?? 0
                                 }).ToList().AsQueryable();
                    listResults = query.ToList();
                }
                else if (ProductType == (byte)Commons.EProductType.Modifier)
                {
                    var query = (from p in cxt.I_Recipe_Modifier
                                 where p.ModifierId.Equals(ProductId) && p.StoreId.Equals(StoreId)
                                        && p.Status == (byte)Commons.EStatus.Actived
                                        && ListStoreId.Contains(p.StoreId)
                                 select new RecipeProductModels
                                 {
                                     IngredientId = p.IngredientId,
                                     Usage = p.Usage,
                                     UOMId = p.UOMId,
                                     BaseUsage = p.BaseUsage ?? 0
                                 }).ToList().AsQueryable();
                    listResults = query.ToList();
                }
            }
            return listResults;
        }

        public void InsertRecipeProduct(RecipeProductModels model, int ProductType, ref string Id)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    if (ProductType == (byte)Commons.EProductType.Dish)
                    {
                        I_Recipe_Item itemInsert = new I_Recipe_Item();
                        itemInsert.Id = Guid.NewGuid().ToString();
                        itemInsert.StoreId = model.StoreId;

                        itemInsert.IngredientId = model.IngredientId;
                        itemInsert.ItemId = model.ItemId;
                        itemInsert.ItemName = model.ItemName;
                        itemInsert.ItemType = model.ItemType;
                        itemInsert.UOMId = model.UOMId;
                        itemInsert.Usage = model.Usage;
                        itemInsert.BaseUsage = model.BaseUsage;

                        itemInsert.Status = (byte)Commons.EStatus.Actived;

                        itemInsert.CreatedBy = model.CreatedBy;
                        itemInsert.CreatedDate = model.CreatedDate;
                        itemInsert.UpdatedBy = model.UpdatedBy;
                        itemInsert.UpdatedDate = model.UpdatedDate;
                        cxt.I_Recipe_Item.Add(itemInsert);

                        Id = itemInsert.Id;
                    }
                    else if (ProductType == (byte)Commons.EProductType.Modifier)
                    {
                        I_Recipe_Modifier itemInsert = new I_Recipe_Modifier();
                        itemInsert.Id = Guid.NewGuid().ToString();
                        itemInsert.StoreId = model.StoreId;

                        itemInsert.IngredientId = model.IngredientId;
                        itemInsert.ModifierId = model.ItemId;
                        itemInsert.ModifierName = model.ItemName;
                        itemInsert.UOMId = model.UOMId;
                        itemInsert.Usage = model.Usage;
                        itemInsert.BaseUsage = model.BaseUsage;

                        itemInsert.Status = (byte)Commons.EStatus.Actived;

                        itemInsert.CreatedBy = model.CreatedBy;
                        itemInsert.CreatedDate = model.CreatedDate;
                        itemInsert.UpdatedBy = model.UpdatedBy;
                        itemInsert.UpdatedDate = model.UpdatedDate;
                        cxt.I_Recipe_Modifier.Add(itemInsert);

                        Id = itemInsert.Id;
                    }
                    cxt.SaveChanges();
                }
                catch (Exception e)
                {
                    _logger.Error("RecipeProduct_Insert: " + e);
                }

            }
        }

        public void UpdateRecipeProduct(RecipeProductModels model, int ProductType)
        {
            using (var cxt = new NuWebContext())
            {
                try
                {
                    if (ProductType == (byte)Commons.EProductType.Dish)
                    {
                        var item = cxt.I_Recipe_Item.Where(s => s.Id == model.Id).FirstOrDefault();
                        if (item != null)
                        {
                            item.UOMId = model.UOMId;
                            item.Usage = model.Usage;
                            item.BaseUsage = model.BaseUsage;

                            item.UpdatedBy = model.UpdatedBy;
                            item.UpdatedDate = DateTime.Now;

                            cxt.SaveChanges();
                        }
                    }
                    else if (ProductType == (byte)Commons.EProductType.Modifier)
                    {
                        var item = cxt.I_Recipe_Modifier.Where(s => s.Id == model.Id).FirstOrDefault();
                        if (item != null)
                        {
                            item.UOMId = model.UOMId;
                            item.Usage = model.Usage;
                            item.BaseUsage = model.BaseUsage;

                            item.UpdatedBy = model.UpdatedBy;
                            item.UpdatedDate = DateTime.Now;

                            cxt.SaveChanges();
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("RecipeProduct_Update: " + e);
                }
            }
        }

        public bool DeleteListIdRecipeProduct(string ItemId, string storeId, List<string> listUpdate, int ProductType)
        {
            bool result = false;
            using (var cxt = new NuWebContext())
            {
                try
                {
                    if (ProductType == (byte)Commons.EProductType.Dish)
                    {
                        var listAllId = (from RI in cxt.I_Recipe_Item
                                         where RI.ItemId.Equals(ItemId) && RI.StoreId.Equals(storeId)
                                                && RI.Status != (int)Commons.EStatus.Deleted
                                         select RI.Id).ToList();
                        var listIdDelete = listAllId.Where(a => !(listUpdate.Select(x => x).ToList()).Any(a1 => a1 == a)).ToList();

                        var listDelete = (from RI in cxt.I_Recipe_Item
                                          where listIdDelete.Contains(RI.Id)
                                          select RI).ToList();

                        listDelete.ForEach(x => x.Status = (int)Commons.EStatus.Deleted);
                        cxt.SaveChanges();
                    }
                    else if (ProductType == (byte)Commons.EProductType.Modifier)
                    {
                        var listAllId = (from RM in cxt.I_Recipe_Modifier
                                         where RM.ModifierId.Equals(ItemId) && RM.StoreId.Equals(storeId)
                                                && RM.Status != (int)Commons.EStatus.Deleted
                                         select RM.Id).ToList();
                        var listIdDelete = listAllId.Where(a => !(listUpdate.Select(x => x).ToList()).Any(a1 => a1 == a)).ToList();

                        var listDelete = (from RI in cxt.I_Recipe_Modifier
                                          where listIdDelete.Contains(RI.Id)
                                          select RI).ToList();

                        listDelete.ForEach(x => x.Status = (int)Commons.EStatus.Deleted);
                        cxt.SaveChanges();
                    }
                    result = true;
                }
                catch (Exception e)
                {
                    _logger.Error("GetListIdRecipeProduct: " + e);
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        public void DeleteRecipeProduct(RecipeProductModels model, int ProductType)
        {
            using (var cxt = new NuWebContext())
            {
                if (ProductType == (byte)Commons.EProductType.Dish)
                {
                    var item = cxt.I_Recipe_Item.Where(s => s.Id == model.Id).FirstOrDefault();
                    if (item != null)
                    {
                        item.Status = (byte)Commons.EStatus.Deleted;
                        item.UpdatedBy = model.UpdatedBy;
                        item.UpdatedDate = DateTime.Now;

                        cxt.SaveChanges();
                    }
                }
                else if (ProductType == (byte)Commons.EProductType.Modifier)
                {
                    var item = cxt.I_Recipe_Modifier.Where(s => s.Id == model.Id).FirstOrDefault();
                    if (item != null)
                    {
                        item.Status = (byte)Commons.EStatus.Deleted;
                        item.UpdatedBy = model.UpdatedBy;
                        item.UpdatedDate = DateTime.Now;

                        cxt.SaveChanges();
                    }
                }
            }
        }

        public RecipeProductModels CheckInsertProduct(RecipeProductModels model, int ProductType, List<string> ListStoreId)
        {
            var item = new RecipeProductModels();
            using (var cxt = new NuWebContext())
            {
                RecipeProductModels itemDb = null;
                //Check StoreId | ProductId | IngredientId
                if (ProductType == (byte)Commons.EProductType.Dish)
                {
                    itemDb = cxt.I_Recipe_Item.Where(
                                            s => s.StoreId.Equals(model.StoreId)
                                            && s.ItemId.Equals(model.ItemId)
                                            && s.IngredientId.Equals(model.IngredientId)
                                            && s.Status == (byte)Commons.EStatus.Actived
                                            && ListStoreId.Contains(s.StoreId)
                                ).Select(x => new RecipeProductModels
                                {
                                    Id = x.Id
                                }).FirstOrDefault();
                }
                else if (ProductType == (byte)Commons.EProductType.Modifier)
                {
                    itemDb = cxt.I_Recipe_Modifier.Where(
                                           s => s.StoreId.Equals(model.StoreId)
                                           && s.ModifierId.Equals(model.ItemId)
                                           && s.IngredientId.Equals(model.IngredientId)
                                           && s.Status == (byte)Commons.EStatus.Actived
                                           && ListStoreId.Contains(s.StoreId)
                               ).Select(x => new RecipeProductModels
                               {
                                   Id = x.Id
                               }).FirstOrDefault();
                }

                if (itemDb != null)
                    item = itemDb;
                return itemDb;
            }
            //return null;
        }

        #region /*Region Of Ingredient*/
        public List<RecipeIngredientModels> GetListRecipeIngredient(List<string> lstCompanyIds)
        {
            List<RecipeIngredientModels> listData = new List<RecipeIngredientModels>();
            if (lstCompanyIds == null)
                lstCompanyIds = new List<string>();
            using (NuWebContext cxt = new NuWebContext())
            {
                var query = (from i in cxt.I_Ingredient
                             where i.Status == (byte)Commons.EStatus.Actived && i.IsActive 
                             && i.IsSelfMode && lstCompanyIds.Contains(i.CompanyId)
                             select new RecipeIngredientModels
                             {
                                 MixtureIngredientId = i.Id,
                                 Id = i.Id,
                                 IngredientCode = i.Code,
                                 IngredientName = i.Name,
                                 Status = (byte)i.Status
                             }).ToList().AsQueryable();
                var listResults = query.ToList();

                var listRecipeIngredient = cxt.I_Recipe_Ingredient
                      .Select(ss => new RecipeIngredientModels()
                      {
                          Id = ss.Id,
                          MixtureIngredientId = ss.MixtureIngredientId,
                          Status = ss.Status
                      }).ToList();

                listData = (from d in listResults
                            let _Ingredient = (
                                               from r in listRecipeIngredient
                                               where r.MixtureIngredientId.Equals(d.Id)
                                                        && r.Status == (byte)Commons.EStatus.Actived
                                               select r.Id).Count()
                            where d.Status == (byte)Commons.EStatus.Actived
                            select new RecipeIngredientModels
                            {
                                MixtureIngredientId = d.MixtureIngredientId,
                                IngredientCode = d.IngredientCode,
                                IngredientName = d.IngredientName,
                                Ingredient = _Ingredient
                            }).ToList();
                listData = listData.OrderByDescending(x => x.Ingredient).ToList();
            }
            return listData;
        }

        public List<RecipeIngredientModels> GetListRecipeIngredient(string Id)
        {
            List<RecipeIngredientModels> listResults = new List<RecipeIngredientModels>();
            using (NuWebContext cxt = new NuWebContext())
            {
                var query = (from p in cxt.I_Recipe_Ingredient
                             where p.MixtureIngredientId.Equals(Id) && p.Status == (byte)Commons.EStatus.Actived
                             select new RecipeIngredientModels
                             {
                                 IngredientId = p.IngredientId,
                                 Usage = p.Usage,
                                 UOMId = p.UOMId,
                                 BaseUsage = p.BaseUsage ?? 0
                             }).ToList().AsQueryable();
                listResults = query.ToList();
            }
            return listResults;
        }


        public bool DeleteListIdRecipeIngredient(string MixtureIngredientId, List<string> listUpdate)
        {
            bool result = false;
            using (var cxt = new NuWebContext())
            {
                try
                {
                    var listAllId = (from RI in cxt.I_Recipe_Ingredient
                                     where RI.MixtureIngredientId.Equals(MixtureIngredientId) && RI.Status != (int)Commons.EStatus.Deleted
                                     select RI.Id).ToList();
                    var listIdDelete = listAllId.Where(a => !(listUpdate.Select(x => x).ToList()).Any(a1 => a1 == a)).ToList();

                    var listDelete = (from RI in cxt.I_Recipe_Ingredient
                                      where listIdDelete.Contains(RI.Id)
                                      select RI).ToList();

                    listDelete.ForEach(x => x.Status = (int)Commons.EStatus.Deleted);
                    cxt.SaveChanges();
                    result = true;
                }
                catch (Exception e)
                {
                    _logger.Error("GetListIdRecipeIngredient: " + e);
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        public void InsertRecipeIngredient(RecipeIngredientModels model, ref string Id)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        I_Recipe_Ingredient itemInsert = new I_Recipe_Ingredient();
                        itemInsert.Id = Guid.NewGuid().ToString();

                        itemInsert.IngredientId = model.IngredientId;
                        itemInsert.MixtureIngredientId = model.MixtureIngredientId;
                        itemInsert.UOMId = model.UOMId;
                        itemInsert.Usage = model.Usage;
                        itemInsert.Status = (byte)Commons.EStatus.Actived;
                        itemInsert.BaseUsage = model.BaseUsage;

                        itemInsert.CreatedBy = model.CreatedBy;
                        itemInsert.CreatedDate = model.CreatedDate;
                        itemInsert.UpdatedBy = model.UpdatedBy;
                        itemInsert.UpdatedDate = model.UpdatedDate;
                        cxt.I_Recipe_Ingredient.Add(itemInsert);

                        cxt.SaveChanges();
                        transaction.Commit();

                        Id = itemInsert.Id;
                    }
                    catch (Exception e)
                    {
                        _logger.Error("RecipeProduct_Insert: " + e);
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
        }

        public void UpdateRecipeIngredient(RecipeIngredientModels model)
        {
            using (var cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var item = cxt.I_Recipe_Ingredient.Where(s => s.Id == model.Id).FirstOrDefault();
                        if (item != null)
                        {
                            item.UOMId = model.UOMId;
                            item.Usage = model.Usage;
                            item.BaseUsage = model.BaseUsage;
                            item.UpdatedBy = model.UpdatedBy;
                            item.UpdatedDate = DateTime.Now;
                            cxt.SaveChanges();
                            transaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error("RecipeProduct_Update: " + e);
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
        }

        public void DeleteRecipeIngredient(RecipeIngredientModels model)
        {
            using (var cxt = new NuWebContext())
            {
                var item = cxt.I_Recipe_Ingredient.Where(s => s.Id == model.Id).FirstOrDefault();
                if (item != null)
                {
                    item.Status = (byte)Commons.EStatus.Deleted;
                    item.UpdatedBy = model.UpdatedBy;
                    item.UpdatedDate = DateTime.Now;
                    cxt.SaveChanges();
                }
            }
        }

        public RecipeIngredientModels CheckInsertIngredient(RecipeIngredientModels model)
        {
            var item = new RecipeIngredientModels();
            using (var cxt = new NuWebContext())
            {
                RecipeIngredientModels itemDb = null;
                //Check MixtureIngredientId | IngredientId
                itemDb = cxt.I_Recipe_Ingredient.Where(s => s.MixtureIngredientId.Equals(model.MixtureIngredientId)
                                       && s.IngredientId.Equals(model.IngredientId)
                                       && s.Status == (byte)Commons.EStatus.Actived)
                                       .Select(x => new RecipeIngredientModels
                                       {
                                           Id = x.Id
                                       }).FirstOrDefault();
                if (itemDb != null)
                    item = itemDb;
                return itemDb;
            }
        }
        public RecipeIngredientUsageModels GetRecipesByIngredientSeftMade(string ingredientId)
        {
            RecipeIngredientUsageModels objReturn = new RecipeIngredientUsageModels();
            using (var cxt = new NuWebContext())
            {
                objReturn.ListChilds = cxt.I_Recipe_Ingredient.Where(s => s.MixtureIngredientId.Equals(ingredientId)
                                       && s.Status == (byte)Commons.EStatus.Actived)
                                       .Select(x => new RecipeIngredientUsageModels
                                       {
                                           Id = x.IngredientId,
                                           BaseUsage = x.BaseUsage.HasValue? x.BaseUsage.Value : 1 * x.Usage
                                       }).ToList();
            }
            return objReturn;
        }
        public RecipeIngredientUsageModels GetRecipesByIngredientSeftMade(List<string> lstIngredientIds)
        {
            RecipeIngredientUsageModels objReturn = new RecipeIngredientUsageModels();
            using (var cxt = new NuWebContext())
            {
                objReturn.ListChilds = cxt.I_Recipe_Ingredient.Where(s => lstIngredientIds.Contains(s.MixtureIngredientId)
                                       && s.Status == (byte)Commons.EStatus.Actived)
                                       .Select(x => new RecipeIngredientUsageModels
                                       {
                                           Id = x.IngredientId,
                                           MixtureIngredientId = x.MixtureIngredientId,
                                           BaseUsage = x.BaseUsage.HasValue ? x.BaseUsage.Value : 1 * x.Usage
                                       }).ToList();
            }
            return objReturn;
        }
        public RecipeIngredientUsageModels GetRecipesByIngredientSeftMade(List<ReceiptNoteSelfMadeDetailModels> lstInput)
        {
            RecipeIngredientUsageModels objReturn = new RecipeIngredientUsageModels();
            var lstIngredientIds = lstInput.Select(ss => ss.IngredientId).ToList();
            using (var cxt = new NuWebContext())
            {
                objReturn.ListChilds = cxt.I_Recipe_Ingredient.Where(s => lstIngredientIds.Contains(s.MixtureIngredientId)
                                       && s.Status == (byte)Commons.EStatus.Actived)
                                       .Select(x => new RecipeIngredientUsageModels
                                       {
                                           Id = x.IngredientId,
                                           MixtureIngredientId = x.MixtureIngredientId,
                                           BaseUsage = x.BaseUsage.HasValue ? x.BaseUsage.Value : 1 * x.Usage
                                       }).ToList();
                if(objReturn.ListChilds != null && objReturn.ListChilds.Any())
                {
                    foreach (var item in objReturn.ListChilds)
                    {
                        var obj = lstInput.Where(ww => ww.IngredientId == item.MixtureIngredientId).FirstOrDefault();
                        if (obj != null)
                            item.TotalUsage = (double)((decimal)item.BaseUsage * (decimal)obj.BaseQty * (decimal)lstInput.Where(ww => ww.IngredientId == item.MixtureIngredientId).Sum(ss => ss.ReceivingQty));
                    }
                }
            }
            return objReturn;
        }
        #endregion
    }
}
