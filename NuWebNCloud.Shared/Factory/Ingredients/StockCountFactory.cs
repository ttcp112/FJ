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
    public class StockCountFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private InventoryFactory _InventoryFactory = null;
        private RecipeFactory _RecipeFactory = null;
        public StockCountFactory()
        {
            _baseFactory = new BaseFactory();
            _InventoryFactory = new InventoryFactory();
            _RecipeFactory = new RecipeFactory();
        }
        public bool InsertManual(StockCountModels model, bool isAutoCreate, ref string msg)
        {
            bool result = true;
            I_StockCount obj = null;
            //Get StockCount Autogen before
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    obj = cxt.I_StockCount.Where(ww => ww.StoreId == model.StoreId && ww.BusinessId == model.BusinessId && ww.IsActived).FirstOrDefault();
                    if (obj != null)//update
                    {
                        model.Id = obj.Id;
                        Update(model, ref msg);
                    }
                    else//insert
                    {
                        Insert(model, isAutoCreate, ref msg);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("InsertManual: ", ex);
                    result = false;
                }

            }
            return result;
        }
        public bool Insert(StockCountModels model, bool isAutoCreate, ref string msg)
        {
            bool result = true;
            ResultModels resultModels = new ResultModels();
            if (isAutoCreate)
            {
                bool isExist = _InventoryFactory.CheckBusinessDayExist(model, isAutoCreate, model.ClosedOn);
                if (isExist)
                {
                    msg = string.Format("StockCount [Store: {0} - Business: {1} is exist", model.StoreId, model.BusinessId);
                    return true;
                }
            }
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<DataEntryDetailModels> _lstDataEntryDetails = new List<DataEntryDetailModels>();
                        List<InventoryModels> lstInventory = new List<InventoryModels>();
                        if (!isAutoCreate)
                        {
                            BusinessDayFactory _BusinessDayFactory = new BusinessDayFactory();
                            var lstData = _BusinessDayFactory.GetDataForStoreOnApi(model.StoreId);
                            if (lstData != null && lstData.Any())
                            {
                                var obj = lstData.Where(ww => ww.ID == model.BusinessId).FirstOrDefault();
                                if (obj != null)
                                {
                                    model.StartedOn = obj.StartedOn;
                                    model.ClosedOn = obj.ClosedOn;
                                }
                            }
                            _lstDataEntryDetails = GetDataEntryDetails(model.BusinessId, model.StoreId);
                        }
                        string dataEntryId = string.Empty;

                        I_StockCount item = new I_StockCount();
                        item.Id = Guid.NewGuid().ToString();
                        dataEntryId = item.Id;
                        item.Code = CommonHelper.GetGenNo(Commons.ETableZipCode.StockCount, model.StoreId);
                        item.StockCountDate = model.StockCountDate;
                        item.StoreId = model.StoreId;
                        item.BusinessId = model.BusinessId;
                        item.Status = model.Status;
                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.ModifierBy = model.ModifierBy;
                        item.ModifierDate = model.ModifierDate;
                        item.IsActived = model.IsActived;
                        item.StartedOn = model.StartedOn;
                        item.ClosedOn = model.ClosedOn;
                        item.IsAutoCreated = model.IsAutoCreated;

                        List<StockCountDetailModels> lstStockDetailBefore = new List<StockCountDetailModels>();
                        if (!isAutoCreate)
                        {
                            lstStockDetailBefore = GetStockCountBefore(string.Empty, model.StoreId);
                        }
                        StockCountDetailModels stockDetailOld = null;
                        //Detail
                        List<I_StockCountDetail> listDetailInsert = new List<I_StockCountDetail>();
                        I_StockCountDetail itemDetail = null;
                        InventoryModels inventoryObj = null;

                        if (_lstDataEntryDetails == null)
                            _lstDataEntryDetails = new List<DataEntryDetailModels>();
                        foreach (var detail in model.ListItem)
                        {
                            itemDetail = new I_StockCountDetail();
                            itemDetail.Id = Guid.NewGuid().ToString();
                            itemDetail.StockCountId = item.Id;
                            itemDetail.IngredientId = detail.IngredientId;
                            itemDetail.CloseBal = detail.CloseBal;
                            itemDetail.Damage = detail.Damage;
                            itemDetail.Wastage = detail.Wastage;
                            itemDetail.OtherQty = detail.OtherQty;
                            itemDetail.Reasons = detail.Reasons;
                            itemDetail.OpenBal = detail.OpenBal;
                            itemDetail.AutoCloseBal = detail.CloseBal;

                            if (!isAutoCreate)
                            {
                                stockDetailOld = lstStockDetailBefore.Where(ww => ww.IngredientId == detail.IngredientId).FirstOrDefault();
                                if (stockDetailOld != null)
                                    itemDetail.OpenBal = stockDetailOld.CloseBal;
                                else
                                    itemDetail.OpenBal = 0;


                                itemDetail.Damage = _lstDataEntryDetails.Where(ww => ww.IngredientId == detail.IngredientId).Sum(ss => ss.Damage);
                                itemDetail.Wastage = _lstDataEntryDetails.Where(ww => ww.IngredientId == detail.IngredientId).Sum(ss => ss.Wast);
                                itemDetail.OtherQty = _lstDataEntryDetails.Where(ww => ww.IngredientId == detail.IngredientId).Sum(ss => ss.OrderQty);
                                var reasons = _lstDataEntryDetails.Where(ww => ww.IngredientId == detail.IngredientId 
                                && ww.Reasons != null && ww.Reasons != string.Empty).Select(ss=>ss.Reasons).ToList();
                                itemDetail.Reasons = String.Join(", ", reasons);
                            }

                            listDetailInsert.Add(itemDetail);
                            //for inventory
                            if (isAutoCreate)
                            {
                                inventoryObj = new InventoryModels();
                                inventoryObj.StoreId = model.StoreId;
                                inventoryObj.IngredientId = detail.IngredientId;
                                inventoryObj.Price = 0;
                                inventoryObj.Quantity = detail.CloseBal;// - (detail.Damage + detail.Wastage + detail.OtherQty));
                                lstInventory.Add(inventoryObj);
                            }
                        }

                        cxt.I_StockCount.Add(item);
                        cxt.I_StockCountDetail.AddRange(listDetailInsert);
                        cxt.SaveChanges();
                        transaction.Commit();
                        NSLog.Logger.Info("InsertStockCount Success", model);
                        //uppdate inventory
                        if (lstInventory != null && lstInventory.Any())
                        {
                            NSLog.Logger.Info("UpdateInventoryForStockCount", lstInventory);
                            _InventoryFactory.UpdateInventoryForStockCount(lstInventory, dataEntryId, ref resultModels);
                            _logger.Info(string.Format("UpdateInventoryForStockCount: [{0}] - [{1}]- [{2}]", resultModels.IsOk, dataEntryId, resultModels.Message));
                        }

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

        public List<DataEntryDetailModels> GetDataEntryDetails(string businessId, string storeId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var lstResults = (from d in cxt.I_DataEntryDetail
                                  join h in cxt.I_DataEntry on d.DataEntryId equals h.Id
                                  where h.BusinessId == businessId && h.StoreId == storeId
                                  select new DataEntryDetailModels()
                                  {
                                      IngredientId = d.IngredientId,
                                      Damage = d.Damage.HasValue ? d.Damage.Value : 0,
                                      Wast = d.Wastage.HasValue ? d.Wastage.Value : 0,
                                      OrderQty = d.OrderQty.HasValue ? d.OrderQty.Value : 0,
                                      Reasons = d.Reasons
                                  }).ToList();


                return lstResults;
            }
        }
        public bool Update(StockCountModels model, ref string msg)
        {
            bool result = true;
            ResultModels resultModels = new ResultModels();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        //List<InventoryModels> lstInventory = new List<InventoryModels>();
                        string dataEntryId = string.Empty;


                        var itemUpdate = (from tb in cxt.I_StockCount
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();

                        itemUpdate.ModifierBy = model.ModifierBy;
                        itemUpdate.ModifierDate = model.ModifierDate;
                        itemUpdate.IsActived = model.IsActived;
                        itemUpdate.IsAutoCreated = false;
                        itemUpdate.Status = (int)Commons.EStockCountStatus.Open;

                        var lstStockDetailBefore = GetStockCountBefore(model.Id, model.StoreId);
                        StockCountDetailModels stockCountlOld = null;

                        List<I_StockCountDetail> _lstDetailNew = new List<I_StockCountDetail>();
                        I_StockCountDetail objDetail = null;
                        //Detail
                        //Check item insert
                        //var lstDetailId = model.ListItem.Select(ss => ss.Id).ToList();
                        //if (lstDetailId != null && lstDetailId.Count > 0)
                        //{
                        var lstDetailUpdate = cxt.I_StockCountDetail.Where(ww => ww.StockCountId == model.Id).ToList();
                        foreach (var item in model.ListItem)
                        {
                            if (!string.IsNullOrEmpty(item.Id))
                            {
                                var obj = lstDetailUpdate.Where(ww => ww.Id == item.Id).FirstOrDefault();
                                if (obj != null)
                                {
                                    obj.CloseBal = item.CloseBal;

                                    stockCountlOld = lstStockDetailBefore.Where(ww => ww.IngredientId == obj.IngredientId).FirstOrDefault();
                                    if (stockCountlOld != null)
                                        obj.OpenBal = stockCountlOld.CloseBal;
                                    else
                                        obj.OpenBal = 0;
                                }
                            }
                            else
                            {
                                objDetail = new I_StockCountDetail();
                                objDetail.Id = Guid.NewGuid().ToString();
                                objDetail.StockCountId = model.Id;
                                objDetail.IngredientId = item.IngredientId;
                                objDetail.OpenBal = 0;
                                objDetail.CloseBal = item.CloseBal;
                                objDetail.Damage = 0;
                                objDetail.Wastage = 0;
                                objDetail.OtherQty = 0;
                                objDetail.Reasons = string.Empty;

                                _lstDetailNew.Add(objDetail);
                            }
                        }
                        //}
                        if (_lstDetailNew.Any())
                        {
                            cxt.I_StockCountDetail.AddRange(_lstDetailNew);
                        }
                        cxt.SaveChanges();
                        transaction.Commit();
                        //uppdate inventory
                        //_InventoryFactory.UpdateInventoryForStockCount(lstInventory, dataEntryId, ref resultModels);
                        //_logger.Info(string.Format("UpdateInventoryForStockCount (Update): [{0}] - [{1}]- [{2}]", resultModels.IsOk, dataEntryId, resultModels.Message));
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
                    I_StockCount itemDelete = (from tb in cxt.I_StockCount
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

        public bool Confirm(string Id, ref string msg)
        {
            bool result = true;
            ResultModels resultModels = new ResultModels();
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    I_StockCount itemDb = (from tb in cxt.I_StockCount
                                           where tb.Id == Id
                                           select tb).FirstOrDefault();
                    if (itemDb != null)
                    {
                        itemDb.Status = (int)Commons.EStockCountStatus.Approved;

                        List<InventoryModels> lstInventory = new List<InventoryModels>();
                        var lstDetails = cxt.I_StockCountDetail.Where(ww => ww.StockCountId == itemDb.Id).ToList();
                        //update cal dataentry
                        var _lstEntryDetails = (from d in cxt.I_DataEntryDetail
                                            join h in cxt.I_DataEntry on d.DataEntryId equals h.Id
                                            where (h.BusinessId == itemDb.BusinessId || h.BusinessId == null || h.BusinessId == "") && h.StoreId == itemDb.StoreId
                                            && h.IsActived
                                            select new DataEntryDetailModels()
                                            {
                                                IngredientId = d.IngredientId,
                                                Damage = d.Damage.HasValue ? d.Damage.Value : 0,
                                                Wast = d.Wastage.HasValue ? d.Wastage.Value : 0,
                                                OrderQty = d.OrderQty.HasValue ? d.OrderQty.Value : 0,
                                                Reasons = d.Reasons
                                            }).ToList();

                        foreach (var detail in lstDetails)
                        {
                            detail.Damage = _lstEntryDetails.Where(ww => ww.IngredientId == detail.IngredientId).Sum(ss => ss.Damage);
                            detail.Wastage = _lstEntryDetails.Where(ww => ww.IngredientId == detail.IngredientId).Sum(ss => ss.Wast);
                            detail.OtherQty = _lstEntryDetails.Where(ww => ww.IngredientId == detail.IngredientId).Sum(ss => ss.OrderQty);
                            var reasons = _lstEntryDetails.Where(ww => ww.IngredientId == detail.IngredientId
                            && ww.Reasons != null && ww.Reasons != string.Empty).Select(ss=>ss.Reasons).ToList();
                            detail.Reasons = String.Join(", ", reasons);

                            lstInventory.Add(new InventoryModels()
                            {
                                StoreId = itemDb.StoreId,
                                IngredientId = detail.IngredientId,
                                Price = 0,
                                Quantity = detail.CloseBal
                            });
                        }
                        //uppdate inventory
                        if (lstInventory != null && lstInventory.Any())
                        {
                            NSLog.Logger.Info("UpdateInventoryForStockCount", lstInventory);
                            _InventoryFactory.UpdateInventoryForStockCount(lstInventory, itemDb.Id, ref resultModels);
                            NSLog.Logger.Info("UpdateInventoryForStockCount Result", resultModels);
                            _logger.Info(string.Format("UpdateInventoryForStockCount: [{0}] - [{1}]- [{2}]", resultModels.IsOk, itemDb.Id, resultModels.Message));
                        }
                    }
                    else
                        result = false;
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
        public List<StockCountModels> GetData(StockCountViewModels model, List<string> listStoreId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var query = (from tb in cxt.I_StockCount
                                 where tb.IsActived
                                 select tb);

                    if (model.StoreId != null)
                    {
                        query = query.Where(x => x.StoreId.Equals(model.StoreId));

                    }
                    var lstResult = query.Select(ss => new StockCountModels()
                    {
                        Id = ss.Id,
                        Code = ss.Code,
                        BusinessId = ss.BusinessId,
                        StockCountDate = ss.StockCountDate,
                        StoreId = ss.StoreId,
                        IsAutoCreated = ss.IsAutoCreated.HasValue ? ss.IsAutoCreated.Value : false,
                        StartedOn = ss.StartedOn,
                        ClosedOn = ss.ClosedOn,
                        Status = ss.Status
                    }).ToList();

                    if (lstResult != null)
                    {
                        lstResult = lstResult.OrderByDescending(oo => oo.StockCountDate).ToList();
                    }
                    var lstId = lstResult.Select(ss => ss.Id).ToList();
                    if (lstId != null && lstId.Any())
                    {
                        var lstDetail = cxt.I_StockCountDetail.Where(ww => lstId.Contains(ww.StockCountId)).ToList();
                        foreach (var item in lstResult)
                        {
                            item.BusinessValue = CommonHelper.BusinessDayDisplay(item.StartedOn, item.ClosedOn);
                            item.Damage = lstDetail.Where(ww => ww.StockCountId == item.Id && ww.Damage.HasValue).Sum(ss => ss.Damage.Value);
                            item.Wast = lstDetail.Where(ww => ww.StockCountId == item.Id && ww.Wastage.HasValue).Sum(ss => ss.Wastage.Value);
                            item.OtherQty = lstDetail.Where(ww => ww.StockCountId == item.Id && ww.OtherQty.HasValue).Sum(ss => ss.OtherQty.Value);
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

        public StockCountModels GetStockCountById(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_StockCount
                                 //from dt in cxt.I_StockCountDetail.Where(ww => tb.Id == ww.StockCountId).DefaultIfEmpty()
                                 //from i in cxt.I_Ingredient.Where(ww => ww.Id == dt.IngredientId)
                                 where tb.Id == ID
                                 select new StockCountModels()
                                 {
                                     Id = tb.Id,
                                     Code = tb.Code,
                                     BusinessId = tb.BusinessId,
                                     StockCountDate = tb.StockCountDate,
                                     StoreId = tb.StoreId,
                                     IsAutoCreated = tb.IsAutoCreated.HasValue ? tb.IsAutoCreated.Value : false
                                 }).FirstOrDefault();

                    if (model != null)
                    {
                        var detailQuery = (from dt in cxt.I_StockCountDetail
                                           from i in cxt.I_Ingredient.Where(ww => ww.Id == dt.IngredientId)
                                           where dt.StockCountId == model.Id
                                           select new StockCountDetailModels()
                                           {
                                               Id = dt.Id,
                                               IngredientId = dt.IngredientId,
                                               IngredientCode = i.Code,
                                               IngredientName = i.Name,
                                               CloseBal = dt.CloseBal,
                                               Damage = dt.Damage.HasValue ? dt.Damage.Value : 0,
                                               Wastage = dt.Wastage.HasValue ? dt.Wastage.Value : 0,
                                               OtherQty = dt.OtherQty.HasValue ? dt.OtherQty.Value : 0,
                                               Reasons = dt.Reasons,
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

        public StockCountModels GetStockCountWithoutDetailById(string Id)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var obj = cxt.I_StockCount.Where(ww => ww.Id == Id).Select(ss => new StockCountModels()
                {
                    Id = ss.Id,
                    Code = ss.Code
                }).FirstOrDefault();
                return obj;
            }
        }

        private List<StockCountDetailModels> GetStockCountBefore(string currentId, string storeId, string businessId = null)
        {
            List<StockCountDetailModels> lstStockCountDetail = new List<StockCountDetailModels>();
            try
            {
                int approved = (int)Commons.EStockCountStatus.Approved;
                using (var cxt = new NuWebContext())
                {
                    var query = (from h in cxt.I_StockCount where h.StoreId == storeId && h.IsActived && h.Status == approved select h);
                    if (!string.IsNullOrEmpty(currentId))
                    {
                        query = query.Where(ww => ww.Id != currentId);
                    }
                    if (!string.IsNullOrEmpty(businessId))
                    {
                        query = query.Where(ww => ww.BusinessId != businessId);
                    }
                    var objId = query.OrderByDescending(oo => oo.StockCountDate).Select(ss => ss.Id).FirstOrDefault();

                    lstStockCountDetail = cxt.I_StockCountDetail.Where(ww => ww.StockCountId == objId).Select(ss => new StockCountDetailModels()
                    {
                        IngredientId = ss.IngredientId,
                        CloseBal = ss.CloseBal

                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            if (lstStockCountDetail == null)
                lstStockCountDetail = new List<StockCountDetailModels>();
            return lstStockCountDetail;
        }

        public async void AutoCreatedStockCount(string companyId, string StoreId, string businessId, DateTime dFrom, DateTime dTo, List<InventoryTrackLogModel> lstSale)
        {
            //bool isLstSaleEmpty = (lstSale == null ? true : false);
            NSLog.Logger.Info(string.Format("AutoCreatedStockCount Start with company {0} | Store {1} | Business {2}", companyId, StoreId, businessId), lstSale);
            string msg = string.Empty;
            StockCountModels model = new StockCountModels();
            model.CreatedBy = "Auto";
            model.ModifierBy = "Auto";
            model.CreatedDate = DateTime.Now;
            model.ModifierDate = DateTime.Now;
            model.StockCountDate = model.CreatedDate;
            model.BusinessId = businessId;
            model.StoreId = StoreId;
            model.IsActived = true;
            model.IsAutoCreated = true;
            model.StartedOn = dFrom;
            model.ClosedOn = dTo;

            model.Status = (int)Commons.EStockCountStatus.Approved;
            try
            {
                List<string> _lstDataEntryId = new List<string>();
                List<string> _lstReceiptNoteId = new List<string>();
                List<string> _lstTransOutlId = new List<string>();
                List<string> _lstTransInId = new List<string>();
                List<string> _lstReturnId = new List<string>();
                List<string> _lstRNSeftMadeId = new List<string>();

                if (lstSale == null)
                    lstSale = new List<Models.Ingredients.InventoryTrackLogModel>();
                var listIng = _InventoryFactory.LoadIngredient(StoreId, companyId);
                List<DataEntryDetailModels> _lstEntryDetails = new List<DataEntryDetailModels>();
                List<IngredientsUsageModels> _lstStockOutIn = null;
                using (NuWebContext cxt = new NuWebContext())
                {
                    _lstEntryDetails = (from d in cxt.I_DataEntryDetail
                                        join h in cxt.I_DataEntry on d.DataEntryId equals h.Id
                                        where (h.BusinessId == businessId || h.BusinessId == null || h.BusinessId == "") && h.StoreId == StoreId
                                        && h.IsActived
                                        select new DataEntryDetailModels()
                                        {
                                            Id = h.Id,
                                            IngredientId = d.IngredientId,
                                            Damage = d.Damage.HasValue ? d.Damage.Value : 0,
                                            Wast = d.Wastage.HasValue ? d.Wastage.Value : 0,
                                            OrderQty = d.OrderQty.HasValue ? d.OrderQty.Value : 0,
                                            Reasons = d.Reasons
                                        }).ToList();


                    _lstStockOutIn = GetStockInOut(StoreId, businessId, ref _lstReceiptNoteId, ref _lstTransOutlId
                        , ref _lstTransInId, ref _lstReturnId, ref _lstRNSeftMadeId);
                }
                decimal openBal = 0; decimal stockIn = 0; //decimal closeBal = 0;
                decimal stockOut = 0;
                double stockInTmp = 0;
                double stockOutTmp = 0;
                //double openBalTmp = 0;
                decimal saleTmp = 0;
                var lstStockDetailBefore = GetStockCountBefore(string.Empty, model.StoreId, businessId);
                var obj = new StockCountDetailModels();
                List<StockCountDetailModels> _lstStockCountStockAbleOff = new List<StockCountDetailModels>();
                StockCountDetailModels stockDetailOld = null;
                _lstDataEntryId = _lstEntryDetails.Select(ss => ss.Id).ToList();
                if (_lstDataEntryId != null && _lstDataEntryId.Any())
                    _lstDataEntryId = _lstDataEntryId.Distinct().ToList();
                if (_lstDataEntryId == null)
                    _lstDataEntryId = new List<string>();

                foreach (var item in listIng)
                {
                    obj = new StockCountDetailModels();

                    obj.IngredientId = item.Id;
                    obj.IngredientName = item.Name;
                    obj.IngredientCode = item.Code;
                    obj.IsSeftMade = item.IsSelfMode;
                    obj.IsStockAble = item.IsStockable;

                    obj.Damage = _lstEntryDetails.Where(ww => ww.IngredientId == item.Id).Sum(ss => ss.Damage);
                    obj.Wastage = _lstEntryDetails.Where(ww => ww.IngredientId == item.Id).Sum(ss => ss.Wast);
                    obj.OtherQty = _lstEntryDetails.Where(ww => ww.IngredientId == item.Id).Sum(ss => ss.OrderQty);
                    var reasons = _lstEntryDetails.Where(ww => ww.IngredientId == item.Id && ww.Reasons != null && ww.Reasons != string.Empty).Select(ss=>ss.Reasons).ToList();
                    obj.Reasons = String.Join(", ", reasons);

                    var lstObj = _lstStockOutIn.Where(ww => ww.IngredientId == obj.IngredientId
                           && ww.StoreId == StoreId).ToList();
                    stockInTmp = 0;
                    stockOutTmp = 0;
                    openBal = 0;
                    stockIn = 0;
                    saleTmp = 0;
                    if (lstObj != null && lstObj.Any())
                    {
                        stockInTmp = lstObj.Sum(ss => ss.StockIn);
                        stockOutTmp = lstObj.Sum(ss => ss.StockOut);
                    }
                    stockDetailOld = lstStockDetailBefore.Where(ww => ww.IngredientId == obj.IngredientId).FirstOrDefault();
                    if (stockDetailOld != null)
                        obj.OpenBal = stockDetailOld.CloseBal;
                    else
                        obj.OpenBal = 0;

                    var sales = lstSale.Where(ww => ww.IngredientId == obj.IngredientId && ww.StoreId == StoreId).FirstOrDefault();
                    if (sales != null)
                    {
                        saleTmp = (decimal)Math.Abs(sales.NewQty);
                        obj.Sale = Math.Abs(sales.NewQty);
                    }
                    decimal.TryParse(obj.OpenBal.ToString(), out openBal);
                    decimal.TryParse(stockInTmp.ToString(), out stockIn);
                    decimal.TryParse(stockOutTmp.ToString(), out stockOut);
                    //decimal.TryParse(item.StockOut.ToString(), out stockOut);

                    //Close balance = (open balance + received + transfer in) -(return +transfer out +data entry + sales)
                    //obj.CloseBal = (item.Qty - (obj.Damage + obj.Wastage + obj.OtherQty));
                    obj.CloseBal = (double)((openBal + stockIn) - (stockOut + saleTmp) - (decimal)(obj.Damage + obj.Wastage + obj.OtherQty));

                    NSLog.Logger.Info(string.Format("Auto Stock Ingredient: {0} | Business: {1} | CloseBal: {2} | OpenBal: {3} | StockIn: {4} | StockOut: {5}  | Sale: {6} | DataEntry: {7}"
                     , item.Id, businessId, obj.CloseBal, openBal, stockIn, stockOut, saleTmp, (obj.Damage + obj.Wastage + obj.OtherQty)), obj);

                    _logger.Info(string.Format("Auto Stock Ingredient: {0} | Business: {1} | CloseBal: {2} | OpenBal: {3} | StockIn: {4} | StockOut: {5}  | Sale: {6} | DataEntry: {7}"
                        , item.Id, businessId, obj.CloseBal, openBal, stockIn, stockOut, saleTmp, (obj.Damage + obj.Wastage + obj.OtherQty)));
                    
                    if (obj.IsSeftMade && !obj.IsStockAble)
                        _lstStockCountStockAbleOff.Add(obj);
                    else
                        model.ListItem.Add(obj);

                }
                //Get list stock count for seft-made
                //if(_lstStockCountStockAbleOff != null && _lstStockCountStockAbleOff.Any())
                //{

                //    NSLog.Logger.Info("AutoCreatedStockCount ListStockAbleOff", _lstStockCountStockAbleOff);

                //    var lstIngredientIds = _lstStockCountStockAbleOff.Select(ss => ss.IngredientId).ToList();
                //   var objSeftMade = _RecipeFactory.GetRecipesByIngredientSeftMade(lstIngredientIds);
                //    if(objSeftMade != null && objSeftMade.ListChilds != null)
                //    {
                //        decimal a = 0, b = 0;
                //        foreach (var item in objSeftMade.ListChilds)
                //        {
                //            var objUpdateSale = model.ListItem.Where(ww => ww.IngredientId == item.MixtureIngredientId).FirstOrDefault();
                //            if(objUpdateSale != null)
                //            {
                //                a = 0; b = 0;
                //                decimal.TryParse(objUpdateSale.Sale.ToString(), out a);
                //                decimal.TryParse(objUpdateSale.CloseBal.ToString(), out b);
                //                objUpdateSale.CloseBal = (double)(b -a);
                //            }
                //        }
                //    }
                //}

                NSLog.Logger.Info("AutoCreatedStockCount Info", model);

                bool result = Insert(model, true, ref msg);
                _logger.Info(string.Format("AutoCreatedStockCount Result: {0} - Meassage: {1}", result, msg));
                NSLog.Logger.Info(string.Format("AutoCreatedStockCount Result: {0} - Meassage: {1}", result, msg));
                //Update BusinessID
                _logger.Info("Start update business Id");
                _InventoryFactory.UpdateBusiness(StoreId, businessId, dFrom, dTo, _lstReceiptNoteId, _lstTransOutlId
                        , _lstTransInId, _lstReturnId, _lstDataEntryId, _lstRNSeftMadeId);
                _logger.Info("End update business Id");
                NSLog.Logger.Info(string.Format("End update business Id: {0} | Store: {1} In AutoCreatedStockCount", businessId, StoreId));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            //return model;
        }

        private List<IngredientsUsageModels> GetStockInOut(string storeId, string businessId
            , ref List<string> lstReceiptNoteId, ref List<string> lstTransOutlId
            , ref List<string> lstTransInId, ref List<string> lstReturnId, ref List<string> lstRNSefMade)
        {
            var lstResults = new List<IngredientsUsageModels>();
            //dFrom = new DateTime(dFrom.Year, dFrom.Month, dFrom.Day, 0, 0, 0);
            //dTo = new DateTime(dTo.Year, dTo.Month, dTo.Day, 23, 59, 59);
            using (NuWebContext cxt = new NuWebContext())
            {
                //Receive Note
                var query = (from d in cxt.I_ReceiptNoteDetail
                             from h in cxt.I_ReceiptNote.Where(ww => ww.Id == d.ReceiptNoteId).DefaultIfEmpty()
                             from p in cxt.I_Purchase_Order_Detail.Where(ww => ww.Id == d.PurchaseOrderDetailId).DefaultIfEmpty()
                             where (h.BusinessId == businessId || h.BusinessId == null || h.BusinessId == "") && h.StoreId == storeId
                             select new IngredientsUsageModels()
                             {
                                 Id = h.Id,
                                 BusinessId = h.BusinessId,
                                 IngredientId = p.IngredientId,
                                 StockIn = d.BaseReceivingQty.HasValue ? d.BaseReceivingQty.Value : 0,
                                 StoreId = h.StoreId,
                             }).ToList();

                //Transfer out
                var queryTransfer = (from d in cxt.I_Stock_Transfer_Detail
                                     from h in cxt.I_Stock_Transfer.Where(ww => ww.Id == d.StockTransferId).DefaultIfEmpty()
                                     where (h.BusinessId == businessId || h.BusinessId == null || h.BusinessId == "") && h.IssueStoreId == storeId
                                     select new IngredientsUsageModels()
                                     {
                                         Id = h.Id,
                                         BusinessId = h.BusinessId,
                                         IngredientId = d.IngredientId,
                                         StockOut = d.IssueBaseQty.HasValue ? d.IssueBaseQty.Value : 0,
                                         StoreId = h.IssueStoreId,
                                     }).ToList();


                //Transfer in
                var queryTransferIn = (from d in cxt.I_Stock_Transfer_Detail
                                       from h in cxt.I_Stock_Transfer.Where(ww => ww.Id == d.StockTransferId).DefaultIfEmpty()
                                           //where h.ReceiveStoreId == storeId && lstBusinessIds.Contains(h.BusinessId)  //&& h.ReceiveDate >= dFrom && h.ReceiveDate <= dTo
                                       where (h.BusinessReceiveId == businessId || h.BusinessReceiveId == null || h.BusinessReceiveId == "") 
                                       && h.ReceiveStoreId == storeId
                                       select new IngredientsUsageModels()
                                       {
                                           Id = h.Id,
                                           BusinessId = h.BusinessId,
                                           IngredientId = d.IngredientId,
                                           StockIn = d.ReceiveBaseQty.HasValue ? d.ReceiveBaseQty.Value : 0,
                                           StoreId = h.ReceiveStoreId,
                                       }).ToList();

             

                //return
                var queryReturn = (from d in cxt.I_Return_Note_Detail
                                   join h in cxt.I_Return_Note on d.ReturnNoteId equals h.Id
                                   join rd in cxt.I_ReceiptNoteDetail on d.ReceiptNoteDetailId equals rd.Id
                                   join pd in cxt.I_Purchase_Order_Detail on rd.PurchaseOrderDetailId equals pd.Id
                                   join rh in cxt.I_ReceiptNote on h.ReceiptNoteId equals rh.Id
                                   //where rh.StoreId == storeId && lstBusinessIds.Contains(rh.BusinessId)

                                   where (h.BusinessId == businessId || h.BusinessId == null || h.BusinessId == "") && rh.StoreId == storeId
                                   select new IngredientsUsageModels()
                                   {
                                       Id = h.Id,
                                       IngredientId = pd.IngredientId,
                                       StockOut = d.ReturnBaseQty.HasValue ? d.ReturnBaseQty.Value : 0,
                                       BusinessId = h.BusinessId,
                                       StoreId = rh.StoreId
                                   }).ToList();

                //Receive Note Safe-Made
                var queryRNSeftMade = (from d in cxt.I_ReceiptNoteForSeftMadeDetail
                             join h in cxt.I_ReceiptNoteForSeftMade on d.ReceiptNoteId equals h.Id
                             join i in cxt.I_Ingredient  on d.IngredientId equals i.Id
                             where (h.BusinessId == businessId || h.BusinessId == null || h.BusinessId == "") && h.StoreId == storeId
                             select new IngredientsUsageModels()
                             {
                                 Id = h.Id,
                                 BusinessId = h.BusinessId,
                                 IngredientId = d.IngredientId,
                                 StockIn = d.BaseReceivingQty.HasValue ? d.BaseReceivingQty.Value : 0,
                                 StoreId = h.StoreId,
                                 IsSeftMade = i.IsSelfMode,
                                 IsStockAble = i.StockAble.HasValue? i.StockAble.Value: false
                             }).ToList();
                //Get stock out for  Receive Note Safe-Made dependent
                var queryRNSeftMadeDepens = (from dd in cxt.I_ReceiptNoteForSeftMadeDependentDetail
                                       join d in cxt.I_ReceiptNoteForSeftMadeDetail on dd.RNSelfMadeDetailId equals d.Id
                                       join h in cxt.I_ReceiptNoteForSeftMade on d.ReceiptNoteId equals h.Id
                                       join i in cxt.I_Ingredient on d.IngredientId equals i.Id
                                       where (h.BusinessId == businessId || h.BusinessId == null || h.BusinessId == "") && h.StoreId == storeId
                                       select new IngredientsUsageModels()
                                       {
                                           Id = h.Id,
                                           BusinessId = h.BusinessId,
                                           IngredientId = dd.IngredientId,
                                           StockOut = dd.StockOutQty,
                                           StoreId = h.StoreId
                                          
                                       }).ToList();


                if (query != null && query.Any())
                    lstReceiptNoteId = query.Select(ss => ss.Id).Distinct().ToList();
                if (queryTransfer != null && queryTransfer.Any())
                    lstTransOutlId = queryTransfer.Select(ss => ss.Id).Distinct().ToList();
                if (queryTransferIn != null && queryTransferIn.Any())
                    lstTransInId = queryTransferIn.Select(ss => ss.Id).Distinct().ToList();
                if (queryReturn != null && queryReturn.Any())
                    lstReturnId = queryReturn.Select(ss => ss.Id).Distinct().ToList();

                if (queryRNSeftMade != null && queryRNSeftMade.Any())
                    lstRNSefMade = queryRNSeftMade.Select(ss => ss.Id).Distinct().ToList();

                lstResults.AddRange(query);
                lstResults.AddRange(queryTransfer);
                lstResults.AddRange(queryTransferIn);
                lstResults.AddRange(queryReturn);
                lstResults.AddRange(queryRNSeftMade);

                lstResults.AddRange(queryRNSeftMadeDepens);
            }
            return lstResults;
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

        public void UpdateStockCountWhenInsertAllocation(List<StockCountDetailModels> lstStockCount, ref ResultModels resultModels)
        {
            resultModels.IsOk = true;
            if (lstStockCount != null && lstStockCount.Any())
            {
                using (NuWebContext cxt = new NuWebContext())
                {
                    using (var transaction = cxt.Database.BeginTransaction())
                    {
                        try
                        {
                            var lstIngredientId = lstStockCount.Select(ss => ss.IngredientId).Distinct().ToList();
                            var lstBusinessId = lstStockCount.Select(ss => ss.BusinessId).Distinct().ToList();
                            foreach (var item in lstBusinessId)
                            {
                                var lstStocks = (from d in cxt.I_StockCountDetail
                                                 join h in cxt.I_StockCount on d.StockCountId equals h.Id
                                                 where h.IsActived && h.BusinessId == item && lstIngredientId.Contains(d.IngredientId)
                                                 select d).ToList();
                                foreach (var subItem in lstStocks)
                                {
                                    if (!subItem.Damage.HasValue)
                                        subItem.Damage = 0;
                                    if (!subItem.Wastage.HasValue)
                                        subItem.Wastage = 0;
                                    if (!subItem.OtherQty.HasValue)
                                        subItem.OtherQty = 0;
                                    if (subItem.Reasons == null)
                                        subItem.Reasons = string.Empty;

                                    subItem.Damage += lstStockCount.Where(ww => ww.IngredientId == subItem.IngredientId && item == ww.BusinessId).Sum(ss => ss.Damage);
                                    subItem.Wastage += lstStockCount.Where(ww => ww.IngredientId == subItem.IngredientId && item == ww.BusinessId).Sum(ss => ss.Wastage);
                                    subItem.OtherQty += lstStockCount.Where(ww => ww.IngredientId == subItem.IngredientId && item == ww.BusinessId).Sum(ss => ss.OtherQty);

                                    var reasons = lstStockCount.Where(ww => ww.IngredientId == subItem.IngredientId && item == ww.BusinessId && ww.Reasons != null && ww.Reasons != string.Empty).Select(s => s.Reasons).ToList();

                                    if (reasons != null && reasons.Any())
                                    {
                                        //subItem.Reasons += string.Join(", ", reasons);

                                        if (!string.IsNullOrEmpty(subItem.Reasons))
                                        {
                                            reasons.Insert(0, subItem.Reasons);
                                        }
                                        subItem.Reasons = string.Join(", ", reasons);

                                    }
                                }
                            }

                            cxt.SaveChanges();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            resultModels.IsOk = false;
                            resultModels.Message = ex.Message;
                            _logger.Error(ex);
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
        }

        public List<StockCountDetailModels> GetIngredientFromStockCount(string storeId, string businessId)
        {
            List<StockCountDetailModels> lstStockCountDetail = new List<StockCountDetailModels>();
            try
            {
                using (var cxt = new NuWebContext())
                {
                    var obj = cxt.I_StockCount.Where(ww => ww.StoreId == storeId && ww.BusinessId == businessId && ww.IsActived).FirstOrDefault();
                    if (obj != null)//update
                    {
                        lstStockCountDetail = cxt.I_StockCountDetail.Where(ww => ww.StockCountId == obj.Id).Select(ss => new StockCountDetailModels()
                        {
                            Id = ss.Id,
                            IngredientId = ss.IngredientId,
                            CloseBal = ss.CloseBal

                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            if (lstStockCountDetail == null)
                lstStockCountDetail = new List<StockCountDetailModels>();
            return lstStockCountDetail;
        }
    }
}
