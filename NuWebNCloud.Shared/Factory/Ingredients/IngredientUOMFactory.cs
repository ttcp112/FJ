using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class IngredientUOMFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public IngredientUOMFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<IngredientUOMModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_Ingredient_UOM> listInsert = new List<I_Ingredient_UOM>();
                    I_Ingredient_UOM item = null;
                    foreach (var model in models)
                    {
                        item = new I_Ingredient_UOM();
                        item.Id = Guid.NewGuid().ToString();
                        item.IngredientId = model.IngredientId;
                        item.UOMId = model.UOMId;
                        item.BaseUOM = model.BaseUOM;
                        item.ReceivingQty = model.ReceivingQty;

                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.UpdatedBy = model.UpdatedBy;
                        item.UpdatedDate = model.UpdatedDate;
                        item.IsActived = model.IsActived;

                        listInsert.Add(item);
                    }
                    cxt.I_Ingredient_UOM.AddRange(listInsert);
                    cxt.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        public bool Update(IngredientUOMModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = (from tb in cxt.I_Ingredient_UOM
                                      where tb.Id == model.Id
                                      select tb).FirstOrDefault();

                    itemUpdate.IngredientId = model.IngredientId;
                    itemUpdate.UOMId = model.UOMId;
                    itemUpdate.BaseUOM = model.BaseUOM;
                    itemUpdate.ReceivingQty = model.ReceivingQty;

                    itemUpdate.CreatedBy = model.CreatedBy;
                    itemUpdate.CreatedDate = model.CreatedDate;
                    itemUpdate.UpdatedBy = model.UpdatedBy;
                    itemUpdate.UpdatedDate = model.UpdatedDate;
                    itemUpdate.IsActived = model.IsActived;

                    cxt.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        public bool Delete(string Id, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    I_Ingredient_UOM itemDelete = (from tb in cxt.I_Ingredient_UOM
                                                   where tb.Id == Id
                                                   select tb).FirstOrDefault();
                    //cxt.I_Ingredient_UOM.Remove(itemDelete);
                    cxt.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        public List<IngredientUOMModels> GetData(DataEntryViewModels model, List<string> listStoreId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Ingredient_UOM
                                     select new IngredientUOMModels()
                                     {
                                         Id = tb.Id,
                                         IngredientId = tb.IngredientId,
                                         UOMId = tb.UOMId,
                                         BaseUOM = tb.BaseUOM,
                                         ReceivingQty = tb.ReceivingQty,
                                         CreatedBy = tb.CreatedBy,
                                         CreatedDate = tb.CreatedDate,
                                         UpdatedBy = tb.UpdatedBy,
                                         UpdatedDate = tb.UpdatedDate,
                                         IsActived = tb.IsActived
                                     }).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public IngredientUOMModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Ingredient_UOM
                                 where tb.Id == ID
                                 select new IngredientUOMModels()
                                 {
                                     Id = tb.Id,
                                     IngredientId = tb.IngredientId,
                                     UOMId = tb.UOMId,
                                     BaseUOM = tb.BaseUOM,
                                     ReceivingQty = tb.ReceivingQty,
                                     CreatedBy = tb.CreatedBy,
                                     CreatedDate = tb.CreatedDate,
                                     UpdatedBy = tb.UpdatedBy,
                                     UpdatedDate = tb.UpdatedDate,
                                     IsActived = tb.IsActived

                                 }).FirstOrDefault();
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<IngredientUOMModels> GetDataForIngredient(string IngredientId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Ingredient_UOM
                                     from uom in cxt.I_UnitOfMeasure
                                     where tb.IngredientId.Equals(IngredientId) && tb.IsActived
                                            && uom.IsActive && uom.Id.Equals(tb.UOMId)
                                     select new IngredientUOMModels()
                                     {
                                         Id = tb.Id,
                                         UOMId = tb.UOMId,
                                         ReceivingQty = tb.ReceivingQty,
                                         IsActived = tb.IsActived,

                                         UOMName = uom.Name
                                     }).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }
    }
}
