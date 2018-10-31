using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class StoreSettingFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public StoreSettingFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<StoreSettingModels> GetData_Old(List<string> listStoreId)
        {
            List<StoreSettingModels> listData = new List<StoreSettingModels>();

            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    if (listStoreId == null || listStoreId.Count == 0)
                    {
                        return listData;
                    }
                    else
                    {
                        listData = (from po in cxt.I_Purchase_Order
                                    from pod in cxt.I_Purchase_Order_Detail.Where(xx => po.Id == xx.PurchaseOrderId).DefaultIfEmpty()
                                    from ing in cxt.I_Ingredient.Where(xx => pod.IngredientId == xx.Id).DefaultIfEmpty()
                                    from uom in cxt.I_UnitOfMeasure.Where(xx => ing.ReceivingUOMId == xx.Id).DefaultIfEmpty()
                                    where listStoreId.Contains(po.StoreId)
                                            && uom.Status != (int)Commons.EStatus.Deleted
                                            && ing.Status != (int)Commons.EStatus.Deleted
                                    select new StoreSettingModels()
                                    {
                                        IngredientId = pod.IngredientId,
                                        IngredientCode = ing.Code,
                                        IngredientName = ing.Name,
                                        ReceivingUOM = uom.Code,
                                        StoreId = po.StoreId,
                                        ReorderingQty = ing.ReOrderQty.HasValue ? ing.ReOrderQty.Value : 0,
                                        MinAlert = ing.MinAlertQty.HasValue ? ing.MinAlertQty.Value : 0
                                    }).Distinct().ToList();
                        listData.ForEach(x =>
                        {
                            var ItemExist = (from st in cxt.I_StoreSetting
                                             where listStoreId.Contains(st.StoreId) && st.IngredientId.Equals(x.IngredientId)
                                                      && st.StoreId.Equals(x.StoreId)
                                             select st).FirstOrDefault();
                            if (ItemExist != null)
                            {
                                x.ReorderingQty = ItemExist.ReorderingQuantity;
                                x.MinAlert = ItemExist.MinAltert;
                            }
                        });
                        return listData;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }
        public List<StoreSettingModels> GetData(List<string> listStoreId, List<string> lstCompanyId)
        {
            List<StoreSettingModels> listData = new List<StoreSettingModels>();

            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    if (listStoreId == null || listStoreId.Count == 0)
                    {
                        listData = (
                                    from ing in cxt.I_Ingredient
                                    from uom in cxt.I_UnitOfMeasure.Where(xx => ing.ReceivingUOMId == xx.Id)
                                    where lstCompanyId.Contains(ing.CompanyId)
                                    && ing.Status == (int)Commons.EStatus.Actived
                                     && uom.Status == (int)Commons.EStatus.Actived
                                    select new StoreSettingModels()
                                    {
                                        IngredientId = ing.Id,
                                        IngredientCode = ing.Code,
                                        IngredientName = ing.Name,
                                        ReceivingUOM = uom.Code,
                                        ReorderingQty = ing.ReOrderQty.HasValue ? ing.ReOrderQty.Value : 0,
                                        MinAlert = ing.MinAlertQty.HasValue ? ing.MinAlertQty.Value : 0,
                                        CompanyId = ing.CompanyId,
                                    }).ToList();
                        if (listData != null)
                        {
                            listData.ForEach(x =>
                            {
                                var ItemExist = (from st in cxt.I_StoreSetting
                                                 where st.IngredientId.Equals(x.IngredientId)
                                                          && st.StoreId.Equals(x.StoreId)
                                                 select st).FirstOrDefault();
                                if (ItemExist != null)
                                {
                                    x.ReorderingQty = ItemExist.ReorderingQuantity;
                                    x.MinAlert = ItemExist.MinAltert;
                                }
                            });
                        }
                        return listData;
                    }
                    else
                    {
                        listData = (
                                     from ing in cxt.I_Ingredient
                                     from uom in cxt.I_UnitOfMeasure.Where(xx => ing.ReceivingUOMId == xx.Id)
                                     from sto in cxt.I_StoreSetting.Where(x => x.IngredientId.Equals(ing.Id))
                                     where listStoreId.Contains(sto.StoreId)
                                             && uom.Status == (int)Commons.EStatus.Actived
                                             && ing.Status == (int)Commons.EStatus.Actived
                                     select new StoreSettingModels()
                                     {
                                         IngredientId = ing.Id,
                                         IngredientCode = ing.Code,
                                         IngredientName = ing.Name,
                                         ReceivingUOM = uom.Code,
                                         StoreId = sto.StoreId,
                                         ReorderingQty = ing.ReOrderQty.HasValue ? ing.ReOrderQty.Value : 0,
                                         MinAlert = ing.MinAlertQty.HasValue ? ing.MinAlertQty.Value : 0
                                     }).Distinct().ToList();
                        listData.ForEach(x =>
                        {
                            var ItemExist = (from st in cxt.I_StoreSetting
                                             where listStoreId.Contains(st.StoreId) && st.IngredientId.Equals(x.IngredientId)
                                                      && st.StoreId.Equals(x.StoreId)
                                             select st).FirstOrDefault();
                            if (ItemExist != null)
                            {
                                x.ReorderingQty = ItemExist.ReorderingQuantity;
                                x.MinAlert = ItemExist.MinAltert;
                            }
                        });
                        return listData;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        //public List<StoreSettingModels> GetDataOld(List<string> listStoreId, List<string> lstCompanyId)
        //{
        //    List<StoreSettingModels> listData = new List<StoreSettingModels>();

        //    using (NuWebContext cxt = new NuWebContext())
        //    {
        //        try
        //        {
        //            if (listStoreId == null || listStoreId.Count == 0)
        //            {                        
        //                return listData;
        //            }
        //            else
        //            {
        //                listData = (from i in cxt.I_InventoryManagement
        //                            from ing in cxt.I_Ingredient.Where(xx => i.IngredientId == xx.Id).DefaultIfEmpty()
        //                            from uom in cxt.I_UnitOfMeasure.Where(xx => ing.ReceivingUOMId == xx.Id).DefaultIfEmpty()
        //                            where listStoreId.Contains(i.StoreId)
        //                                    && uom.Status != (int)Commons.EStatus.Deleted
        //                                    && ing.Status != (int)Commons.EStatus.Deleted
        //                            select new StoreSettingModels()
        //                            {
        //                                IngredientId = i.IngredientId,
        //                                IngredientCode = ing.Code,
        //                                IngredientName = ing.Name,
        //                                ReceivingUOM = uom.Code,
        //                                StoreId = i.StoreId,
        //                                ReorderingQty = ing.ReOrderQty.HasValue ? ing.ReOrderQty.Value : 0,
        //                                MinAlert = ing.MinAlertQty.HasValue ? ing.MinAlertQty.Value : 0
        //                            }).Distinct().ToList();
        //                listData.ForEach(x =>
        //                {
        //                    var ItemExist = (from st in cxt.I_StoreSetting
        //                                     where listStoreId.Contains(st.StoreId) && st.IngredientId.Equals(x.IngredientId)
        //                                              && st.StoreId.Equals(x.StoreId)
        //                                     select st).FirstOrDefault();
        //                    if (ItemExist != null)
        //                    {
        //                        x.ReorderingQty = ItemExist.ReorderingQuantity;
        //                        x.MinAlert = ItemExist.MinAltert;
        //                    }
        //                });
        //                return listData;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error(ex.Message);
        //            return null;
        //        }
        //    }
        //}
        public bool SaveData(List<StoreSettingModels> listData, string quantity, string minlert, string user, ref string msg)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_StoreSetting> listInsert = new List<I_StoreSetting>();
                    I_StoreSetting itemInsert = null;
                    foreach (var item in listData)
                    {
                        var ItemExist = (from sst in cxt.I_StoreSetting
                                         where sst.StoreId.Equals(item.StoreId) && sst.IngredientId.Equals(item.IngredientId)
                                         select sst).FirstOrDefault();
                        if (ItemExist == null)
                        {
                            itemInsert = new I_StoreSetting();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.IngredientId = item.IngredientId;
                            itemInsert.ReorderingQuantity = Convert.ToDouble(quantity);
                            itemInsert.MinAltert = Convert.ToDouble(minlert);
                            itemInsert.CreatedBy = user;
                            itemInsert.UpdatedBy = user;
                            itemInsert.CreatedDate = DateTime.Now;
                            itemInsert.UpdatedDate = DateTime.Now;
                            listInsert.Add(itemInsert);
                        }
                        else
                        {
                            ItemExist.ReorderingQuantity = Convert.ToDouble(quantity);
                            ItemExist.MinAltert = Convert.ToDouble(minlert);
                            ItemExist.UpdatedBy = user;
                            ItemExist.UpdatedDate = DateTime.Now;
                        }
                    }
                    if (listInsert.Count > 0)
                        cxt.I_StoreSetting.AddRange(listInsert);
                    cxt.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return false;
                }
            }
        }
    }
}
