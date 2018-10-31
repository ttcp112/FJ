using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class DataEntryFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private DataEntryDetailFactory _DataEntryDetailFactory = null;
        private InventoryFactory _InventoryFactory = null;
        public DataEntryFactory()
        {
            _baseFactory = new BaseFactory();
            _DataEntryDetailFactory = new DataEntryDetailFactory();
            _InventoryFactory = new InventoryFactory();
        }

        public bool Insert(DataEntryModels model, bool isAutoCreate, ref string msg)
        {
            bool result = true;
            ResultModels resultModels = new ResultModels();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        string dataEntryId = string.Empty;

                        I_DataEntry item = new I_DataEntry();

                        item.Id = Guid.NewGuid().ToString();
                        dataEntryId = item.Id;
                        item.EntryCode = CommonHelper.GetGenNo(Commons.ETableZipCode.DataEntry, model.StoreId);
                        item.EntryDate = model.EntryDate;
                        item.StoreId = model.StoreId;
                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.ModifierBy = model.ModifierBy;
                        item.ModifierDate = model.ModifierDate;
                        item.IsActived = model.IsActived;
                        
                        //Detail
                        List<I_DataEntryDetail> listDetailInsert = new List<I_DataEntryDetail>();
                        I_DataEntryDetail itemDetail = null;

                        foreach (var detail in model.ListItem)
                        {
                            itemDetail = new I_DataEntryDetail();
                            itemDetail.Id = Guid.NewGuid().ToString();
                            itemDetail.DataEntryId = item.Id;
                            itemDetail.IngredientId = detail.IngredientId;
                            itemDetail.Damage = detail.Damage;
                            itemDetail.Wastage = detail.Wast;
                            itemDetail.OrderQty = detail.OrderQty;
                            itemDetail.Reasons = detail.Reasons;

                            listDetailInsert.Add(itemDetail);

                        }

                        cxt.I_DataEntry.Add(item);
                        cxt.I_DataEntryDetail.AddRange(listDetailInsert);
                        cxt.SaveChanges();
                        transaction.Commit();
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            return result;
        }

        public bool Update(DataEntryModels model, ref string msg)
        {
            bool result = true;
            ResultModels resultModels = new ResultModels();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {

                        var itemUpdate = (from tb in cxt.I_DataEntry
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();

                        itemUpdate.ModifierBy = model.ModifierBy;
                        itemUpdate.ModifierDate = model.ModifierDate;
                        itemUpdate.IsActived = model.IsActived;
                        
                        //Detail
                        //Check item insert
                        var lstDetailId = model.ListItem.Select(ss => ss.Id).ToList();
                        if (lstDetailId != null && lstDetailId.Count > 0)
                        {
                            var lstDetailUpdate = cxt.I_DataEntryDetail.Where(ww => lstDetailId.Contains(ww.Id)).ToList();
                            foreach (var item in model.ListItem)
                            {
                                var obj = lstDetailUpdate.Where(ww => ww.Id == item.Id).FirstOrDefault();
                                if (obj != null)
                                {
                                    obj.Damage = item.Damage;
                                    obj.Wastage = item.Wast;
                                    obj.OrderQty = item.OrderQty;
                                    obj.Reasons = item.Reasons;

                                }
                            }
                        }

                        cxt.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
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
                    I_DataEntry itemDelete = (from tb in cxt.I_DataEntry
                                              where tb.Id == Id
                                              select tb).FirstOrDefault();
                    if (itemDelete != null)
                    {
                        itemDelete.IsActived = false;
                    }
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

        public List<DataEntryModels> GetData(DataEntryViewModels model, List<string> listStoreId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {

                    var query = (from tb in cxt.I_DataEntry where tb.IsActived select tb);

                    //var lstResult = (from tb in cxt.I_DataEntry
                    //                 where tb.IsActived
                    //                 select new DataEntryModels()
                    //                 {
                    //                     Id = tb.Id,
                    //                     EntryCode = tb.EntryCode,
                    //                     BusinessId = tb.BusinessId,
                    //                     EntryDate = tb.EntryDate,
                    //                     StoreId = tb.StoreId
                    //                 }).ToList();

                    if (model.StoreId != null)
                    {
                        query = query.Where(x => x.StoreId.Equals(model.StoreId));
                    }
                    var lstResult = query.Select(ss=> new DataEntryModels()
                                     {
                                         Id = ss.Id,
                                         EntryCode = ss.EntryCode,
                                         BusinessId = ss.BusinessId,
                                         EntryDate = ss.EntryDate,
                                         StoreId = ss.StoreId,
                                         StartedOn = ss.StartedOn,
                                         ClosedOn = ss.ClosedOn
                                     }).ToList();

                    if (lstResult != null)
                    {
                        lstResult = lstResult.OrderByDescending(oo => oo.EntryDate).ToList();
                        var lstNotHaveBusinessId = lstResult.Where(ww => ww.BusinessId == null || ww.BusinessId == string.Empty).ToList();
                        if (lstNotHaveBusinessId != null && lstNotHaveBusinessId.Any())
                        {
                            foreach (var item in lstNotHaveBusinessId)
                            {
                                item.IsVisible = false;
                            }
                        }
                    }
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public DataEntryModels GetDataEntryById(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_DataEntry
                                 from dt in cxt.I_DataEntryDetail.Where(ww => tb.Id == ww.DataEntryId).DefaultIfEmpty()
                                 from i in cxt.I_Ingredient.Where(ww => ww.Id == dt.IngredientId)
                                 where tb.Id == ID
                                 select new DataEntryModels()
                                 {
                                     Id = tb.Id,
                                     EntryCode = tb.EntryCode,
                                     BusinessId = tb.BusinessId,
                                     EntryDate = tb.EntryDate,
                                     StoreId = tb.StoreId
                                 }).FirstOrDefault();

                    if (model != null)
                    {
                        var detailQuery = (from dt in cxt.I_DataEntryDetail
                                           from i in cxt.I_Ingredient.Where(ww => ww.Id == dt.IngredientId)
                                           where dt.DataEntryId == model.Id
                                           select new DataEntryDetailModels()
                                           {
                                               Id = dt.Id,
                                               IngredientId = dt.IngredientId,
                                               IngredientCode = i.Code,
                                               IngredientName = i.Name,
                                               Damage = dt.Damage.HasValue? dt.Damage.Value:0,
                                               Wast = dt.Wastage.HasValue ? dt.Wastage.Value : 0,
                                               OrderQty = dt.OrderQty.HasValue ? dt.OrderQty.Value : 0,
                                               Reasons = dt.Reasons
                                           }).ToList();
                        if (detailQuery != null && detailQuery.Any())
                        {
                            detailQuery = detailQuery.OrderBy(oo => oo.IngredientName).ToList();
                        }
                        model.ListItem = detailQuery;

                        
                    }
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public DataEntryModels GetDataEntryWithoutDetailById(string Id)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var obj = cxt.I_DataEntry.Where(ww => ww.Id == Id).Select(ss => new DataEntryModels()
                {
                    Id = ss.Id,
                    EntryCode = ss.EntryCode
                }).FirstOrDefault();
                return obj;
            }
        }

        private List<DataEntryDetailModels> GetDataEnTryBefore(string currentEntryId, string storeId)
        {
            List<DataEntryDetailModels> lstEntryDetail = new List<DataEntryDetailModels>();
            try
            {
                using (var cxt = new NuWebContext())
                {
                    var query = (from h in cxt.I_DataEntry where h.StoreId == storeId && h.IsActived select h);
                    if (!string.IsNullOrEmpty(currentEntryId))
                    {
                        query = query.Where(ww => ww.Id != currentEntryId);
                    }
                    var entryId = query.OrderByDescending(oo => oo.EntryDate).Select(ss => ss.Id).FirstOrDefault();

                    lstEntryDetail = cxt.I_DataEntryDetail.Where(ww => ww.DataEntryId == entryId).Select(ss => new DataEntryDetailModels()
                    {
                        IngredientId = ss.IngredientId,
                        //CloseBal = ss.CloseBal

                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            if (lstEntryDetail == null)
                lstEntryDetail = new List<DataEntryDetailModels>();
            return lstEntryDetail;
        }

        public List<string> CheckShowBusiness(string storeId, List<string> lstBusinessIds)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var lstInfo = cxt.I_StockCount.Where(ww => ww.StoreId == storeId && lstBusinessIds.Contains(ww.BusinessId) && ww.IsActived
                && (!ww.IsAutoCreated.HasValue || (ww.IsAutoCreated.HasValue && !ww.IsAutoCreated.Value))).Select(ss => ss.BusinessId).ToList();
                if (lstInfo != null && lstInfo.Any())
                {
                    lstBusinessIds = lstBusinessIds.Where(ww => !lstInfo.Contains(ww)).ToList();
                }
            }
            return lstBusinessIds;
        }
    }
}
