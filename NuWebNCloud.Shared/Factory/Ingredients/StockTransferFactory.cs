using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class StockTransferFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private StockTransferDetailFactory _StockTransferDetailFactory = null;
        private InventoryFactory _inventoryFactory = null;

        public StockTransferFactory()
        {
            _baseFactory = new BaseFactory();
            _StockTransferDetailFactory = new StockTransferDetailFactory();
            _inventoryFactory = new InventoryFactory();
        }

        public bool Insert(StockTransferModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        ResultModels resultModels = new ResultModels();

                        I_Stock_Transfer item = new I_Stock_Transfer();
                        string StockTransferId = Guid.NewGuid().ToString();

                        item.Id = StockTransferId;
                        item.StockTransferNo = CommonHelper.GetGenNo(Commons.ETableZipCode.StockTransfer, model.IssueStoreId);

                        item.IssueStoreId = model.IssueStoreId;
                        item.ReceiveStoreId = model.ReceiveStoreId;

                        item.RequestBy = model.RequestBy;
                        item.RequestDate = model.RequestDate;

                        item.IssueBy = model.IssueBy;
                        item.IssueDate = model.IssueDate;

                        item.ReceiveBy = model.ReceiveBy;
                        item.ReceiveDate = model.ReceiveDate;

                        item.IsActive = model.IsActive;

                        List<I_Stock_Transfer_Detail> listInsert = new List<I_Stock_Transfer_Detail>();
                        I_Stock_Transfer_Detail itemDetail = null;
                        //for stock
                        List<InventoryTransferModels> lstInventory = new List<InventoryTransferModels>();
                        InventoryTransferModels inventory = null;
                        foreach (var STDetailItem in model.ListItem)
                        {
                            itemDetail = new I_Stock_Transfer_Detail();

                            itemDetail.Id = Guid.NewGuid().ToString();
                            itemDetail.StockTransferId = StockTransferId;
                            itemDetail.IngredientId = STDetailItem.IngredientId;
                            itemDetail.RequestQty = STDetailItem.RequestQty;
                            itemDetail.ReceiveQty = STDetailItem.ReceiveQty;
                            itemDetail.IssueQty = STDetailItem.IssueQty;
                            itemDetail.UOMId = STDetailItem.UOMId;
                            itemDetail.ReceiveBaseQty = (STDetailItem.ReceiveQty * STDetailItem.Rate);
                            itemDetail.IssueBaseQty = (STDetailItem.IssueQty * STDetailItem.Rate);

                            listInsert.Add(itemDetail);

                            inventory = new InventoryTransferModels();
                            inventory.IssueStoreId = model.IssueStoreId;
                            inventory.ReceiveStoreId = model.ReceiveStoreId;
                            inventory.IngredientId = STDetailItem.IngredientId;
                            inventory.IssueQty = (STDetailItem.IssueQty * STDetailItem.Rate);
                            inventory.ReceiveQty= (STDetailItem.ReceiveQty * STDetailItem.Rate);

                            inventory.Price = 0;

                            lstInventory.Add(inventory);
                        }

                        cxt.I_Stock_Transfer.Add(item);
                        cxt.I_Stock_Transfer_Detail.AddRange(listInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        //Update inventory
                        _inventoryFactory.UpdateInventoryForTransfer(lstInventory, StockTransferId, ref resultModels);
                        _logger.Info(string.Format("UpdateInventoryForTransfer: [{0}] - [{1}] - [{2}] ", resultModels.IsOk, StockTransferId, resultModels.Message));
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
                    I_Stock_Transfer itemDelete = (from tb in cxt.I_Stock_Transfer
                                                   where tb.Id == Id
                                                   select tb).FirstOrDefault();
                    //cxt.I_Stock_Transfer.Remove(itemDelete);
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

        public List<StockTransferModels> GetData(StockTransferViewModels model, List<string> listStoreIssueId, List<string> listStoreReceiveId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Stock_Transfer
                                     let ItemsTotal = cxt.I_Stock_Transfer_Detail.Where(z => z.StockTransferId.Equals(tb.Id)).Count()
                                     where (listStoreIssueId.Contains(tb.IssueStoreId) && listStoreReceiveId.Contains(tb.ReceiveStoreId))
                                     select new StockTransferModels()
                                     {
                                         Id = tb.Id,
                                         IssueStoreId = tb.IssueStoreId,
                                         ReceiveStoreId = tb.ReceiveStoreId,
                                         RequestBy = tb.RequestBy,
                                         RequestDate = tb.RequestDate,
                                         IssueBy = tb.IssueBy,
                                         IssueDate = tb.IssueDate,
                                         ReceiveBy = tb.ReceiveBy,
                                         ReceiveDate = tb.ReceiveDate,
                                         IsActive = tb.IsActive,
                                         StockTransferNo = tb.StockTransferNo,

                                         ItemsTotal = ItemsTotal
                                     }).ToList();

                    if (model.StockTransferNo != null)
                    {
                        lstResult = lstResult.Where(x => x.StockTransferNo.Contains(model.StockTransferNo)).ToList();
                    }
                    if (model.IssuingStoreId != null)
                    {
                        lstResult = lstResult.Where(x => x.IssueStoreId.Equals(model.IssuingStoreId)).ToList();
                    }
                    if (model.ReceivingStoreId != null)
                    {
                        lstResult = lstResult.Where(x => x.ReceiveStoreId.Equals(model.ReceivingStoreId)).ToList();
                    }
                    if (model.IssueDate)
                    {
                        if (model.ApplyFrom != null)
                        {
                            DateTime fromDate = new DateTime(model.ApplyFrom.Value.Year, model.ApplyFrom.Value.Month, model.ApplyFrom.Value.Day, 0, 0, 0);
                            DateTime ToDate = new DateTime(model.ApplyTo.Value.Year, model.ApplyTo.Value.Month, model.ApplyTo.Value.Day, 23, 59, 59);
                            lstResult = lstResult.Where(ww => ww.IssueDate >= fromDate && ww.IssueDate <= ToDate).ToList();
                        }
                    }
                    if (model.ReveieDate)
                    {
                        if (model.ApplyFrom != null)
                        {
                            DateTime fromDate = new DateTime(model.ApplyFrom.Value.Year, model.ApplyFrom.Value.Month, model.ApplyFrom.Value.Day, 0, 0, 0);
                            DateTime ToDate = new DateTime(model.ApplyTo.Value.Year, model.ApplyTo.Value.Month, model.ApplyTo.Value.Day, 23, 59, 59);
                            lstResult = lstResult.Where(ww => ww.ReceiveDate >= fromDate && ww.ReceiveDate <= ToDate).ToList();
                        }
                    }
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return new List<StockTransferModels>();
                }
            }
        }

        public StockTransferModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Stock_Transfer
                                 where tb.Id == ID
                                 select new StockTransferModels()
                                 {
                                     Id = tb.Id,
                                     IssueStoreId = tb.IssueStoreId,
                                     ReceiveStoreId = tb.ReceiveStoreId,
                                     RequestBy = tb.RequestBy,
                                     RequestDate = tb.RequestDate,
                                     IssueBy = tb.IssueBy,
                                     IssueDate = tb.IssueDate,
                                     ReceiveBy = tb.ReceiveBy,
                                     ReceiveDate = tb.ReceiveDate,
                                     IsActive = tb.IsActive,
                                     StockTransferNo = tb.StockTransferNo,

                                 }).FirstOrDefault();

                    model.ListItem = (from tb in cxt.I_Stock_Transfer_Detail
                                      from i in cxt.I_Ingredient
                                      from uom in cxt.I_UnitOfMeasure
                                      where tb.StockTransferId == ID
                                            && tb.IngredientId.Equals(i.Id) && i.Status != (int)Commons.EStatus.Deleted
                                            && tb.UOMId.Equals(uom.Id) && uom.Status != (int)Commons.EStatus.Deleted
                                      select new StockTransferDetailModels()
                                      {
                                          Id = tb.Id,
                                          IngredientCode = i.Code,
                                          IngredientName = i.Name,
                                          RequestQty = tb.RequestQty,
                                          IssueQty = tb.IssueQty,
                                          ReceiveQty = tb.ReceiveQty,
                                          BaseUOM = uom.Name
                                      }).ToList();
                    return model;
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
