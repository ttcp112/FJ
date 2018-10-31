using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class StockTransferDetailFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public StockTransferDetailFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<StockTransferDetailModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_Stock_Transfer_Detail> listInsert = new List<I_Stock_Transfer_Detail>();
                    I_Stock_Transfer_Detail item = null;
                    foreach (var model in models)
                    {
                        item = new I_Stock_Transfer_Detail();

                        item.Id = Guid.NewGuid().ToString();
                        item.StockTransferId = model.StockTransferId;
                        item.IngredientId = model.IngredientId;
                        item.RequestQty = model.RequestQty;
                        item.ReceiveQty = model.ReceiveQty;
                        item.IssueQty = model.IssueQty;
                        item.UOMId = model.UOMId;

                        listInsert.Add(item);
                    }
                  
                    cxt.I_Stock_Transfer_Detail.AddRange(listInsert);
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

        public bool Update(StockTransferDetailModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = (from tb in cxt.I_Stock_Transfer_Detail
                                      where tb.Id == model.Id
                                      select tb).FirstOrDefault();

                    itemUpdate.StockTransferId = model.StockTransferId;
                    itemUpdate.IngredientId = model.IngredientId;
                    itemUpdate.RequestQty = model.RequestQty;
                    itemUpdate.ReceiveQty = model.ReceiveQty;
                    itemUpdate.IssueQty = model.IssueQty;
                    itemUpdate.ReceiveQty = model.ReceiveQty;
                    itemUpdate.UOMId = model.UOMId;

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
                    I_Stock_Transfer_Detail itemDelete = (from tb in cxt.I_Stock_Transfer_Detail
                                                   where tb.Id == Id
                                                   select tb).FirstOrDefault();
                    //cxt.I_Stock_Transfer_Detail.Remove(itemDelete);
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

        public List<StockTransferDetailModels> GetData()
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Stock_Transfer_Detail
                                     select new StockTransferDetailModels()
                                     {
                                         Id = tb.Id,
                                         IngredientId = tb.IngredientId,
                                         ReceiveQty = tb.ReceiveQty,
                                         IssueQty = tb.IssueQty,
                                         RequestQty = tb.RequestQty,
                                         StockTransferId = tb.StockTransferId,
                                         UOMId = tb.UOMId
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

        public StockTransferDetailModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Stock_Transfer_Detail
                                 where tb.Id == ID
                                 select new StockTransferDetailModels()
                                 {
                                     Id = tb.Id,
                                     IngredientId = tb.IngredientId,
                                     ReceiveQty = tb.ReceiveQty,
                                     IssueQty = tb.IssueQty,
                                     RequestQty = tb.RequestQty,
                                     StockTransferId = tb.StockTransferId,
                                     UOMId = tb.UOMId
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
    }
}
