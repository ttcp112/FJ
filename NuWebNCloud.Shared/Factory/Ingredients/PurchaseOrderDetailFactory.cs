using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class PurchaseOrderDetailFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public PurchaseOrderDetailFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<PurchaseOrderDetailModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_Purchase_Order_Detail> listInsert = new List<I_Purchase_Order_Detail>();
                    I_Purchase_Order_Detail item = null;
                    foreach (var model in models)
                    {
                        item = new I_Purchase_Order_Detail();

                        item.Id = Guid.NewGuid().ToString();
                        item.PurchaseOrderId = model.PurchaseOrderId;
                        item.IngredientId = model.IngredientId;
                        item.Qty = model.Qty;
                        item.UnitPrice = model.UnitPrice;
                        item.Amount = model.Amount;
                        item.ReceiptNoteQty = model.ReceiptNoteQty;
                        item.ReturnReceiptNoteQty = model.ReturnReceiptNoteQty;
                        listInsert.Add(item);
                    }
                    cxt.I_Purchase_Order_Detail.AddRange(listInsert);
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

        public bool _Update(List<PurchaseOrderDetailModels> models, List<string> listIdDelete, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    //Delete Item
                    Delete(listIdDelete, ref msg);
                    //=========
                    List<I_Purchase_Order_Detail> listUpdate = new List<I_Purchase_Order_Detail>();
                    foreach (var model in models)
                    {
                        var itemUpdate = (from tb in cxt.I_Purchase_Order_Detail
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();
                        if (itemUpdate != null)
                        {
                            itemUpdate.Qty = model.Qty;
                            itemUpdate.UnitPrice = model.UnitPrice;
                            itemUpdate.Amount = model.Amount;
                            cxt.SaveChanges();
                        }
                    }
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

        public bool Delete(List<string> lstId, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_Purchase_Order_Detail> listDelete = (from tb in cxt.I_Purchase_Order_Detail
                                                                where lstId.Contains(tb.Id)
                                                                select tb).ToList();
                    if (listDelete != null && listDelete.Count > 0)
                    {
                        cxt.I_Purchase_Order_Detail.RemoveRange(listDelete);
                        cxt.SaveChanges();
                    }
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

        public List<POIngredient> GetData(string PurchaseOrderId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Purchase_Order_Detail
                                     from i in cxt.I_Ingredient
                                     from uom in cxt.I_UnitOfMeasure
                                     where tb.PurchaseOrderId.Equals(PurchaseOrderId) && tb.IngredientId.Equals(i.Id)
                                            && i.ReceivingUOMId.Equals(uom.Id)
                                            && uom.Status != (int)Commons.EStatus.Deleted
                                            && i.Status != (int)Commons.EStatus.Deleted

                                     select new POIngredient()
                                     {
                                         Id = tb.Id,

                                         IngredientId = tb.IngredientId,
                                         BaseUOM = uom.Name,
                                         IngredientCode = i.Code,
                                         IngredientName = i.Name,
                                         IngReceivingQty = i.ReceivingQty,

                                         BaseQty = tb.BaseQty ?? 0,

                                         QtyTolerance = i.QtyTolerance,
                                         QtyToleranceS = ((i.QtyTolerance / 100) * tb.Qty),
                                         QtyToleranceP = (((i.QtyTolerance / 100) * tb.Qty)),

                                         Qty = tb.Qty, //=>(1)
                                         UnitPrice = tb.UnitPrice,
                                         Amount = tb.Amount,

                                         ReceiptNoteQty = tb.ReceiptNoteQty.Value, //=>(2)
                                         ReceivedQty = tb.ReceiptNoteQty.Value,
                                         ReturnReceiptNoteQty = tb.ReturnReceiptNoteQty.Value,//=>(3)

                                         // = (1) - (2) + (3)
                                         RemainingQty = (tb.Qty - tb.ReceiptNoteQty.Value + tb.ReturnReceiptNoteQty.Value)
                                     }).ToList();
                    lstResult.ForEach(x =>
                    {
                        if (x.RemainingQty <= 0) // Full
                        {
                            x.RemainingQty = 0;
                            x.IsVisible = true;
                        }
                        x.ReceivingQty = x.RemainingQty;
                    });
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<POIngredient> GetDataView(string PurchaseOrderId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Purchase_Order_Detail
                                     from rnd in cxt.I_ReceiptNoteDetail
                                     from rtd in cxt.I_Return_Note_Detail.Where(w => rnd.Id.Equals(w.ReceiptNoteDetailId)).DefaultIfEmpty()
                                     from i in cxt.I_Ingredient
                                     from uom in cxt.I_UnitOfMeasure

                                     where tb.PurchaseOrderId.Equals(PurchaseOrderId) && tb.IngredientId.Equals(i.Id)
                                            && i.BaseUOMId.Equals(uom.Id)
                                            && uom.IsActive && uom.Status != (int)Commons.EStatus.Deleted
                                            && i.IsActive && i.Status != (int)Commons.EStatus.Deleted

                                            && rnd.PurchaseOrderDetailId.Equals(tb.Id)

                                     select new POIngredient()
                                     {
                                         Id = tb.Id,

                                         IngredientId = tb.IngredientId,
                                         BaseUOM = uom.Name,
                                         IngredientCode = i.Code,
                                         IngredientName = i.Name,

                                         Qty = tb.Qty,
                                         UnitPrice = tb.UnitPrice,
                                         Amount = tb.Amount,

                                         ReceivingQty = rnd.ReceivingQty,
                                         RemainingQty = rnd.RemainingQty,

                                         ReturnQty = rtd == null ? 0 : rtd.ReturnQty

                                     }).ToList();
                    lstResult.ForEach(x =>
                    {
                        if (x.RemainingQty < 0) // Full
                        {
                            x.RemainingQty = 0;
                        }
                    });
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public PurchaseOrderDetailModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Purchase_Order_Detail
                                 where tb.Id == ID
                                 select new PurchaseOrderDetailModels()
                                 {
                                     Id = tb.Id,
                                     PurchaseOrderId = tb.PurchaseOrderId,
                                     IngredientId = tb.IngredientId,
                                     Qty = tb.Qty,
                                     UnitPrice = tb.UnitPrice,
                                     Amount = tb.Amount,
                                     ReceiptNoteQty = tb.ReceiptNoteQty,
                                     ReturnReceiptNoteQty = tb.ReturnReceiptNoteQty,

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


        public bool UpdateReceiptNoteQty(List<PurchaseOrderDetailModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_Purchase_Order_Detail> listUpdate = new List<I_Purchase_Order_Detail>();
                    foreach (var model in models)
                    {
                        var itemUpdate = (from tb in cxt.I_Purchase_Order_Detail
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();
                        if (itemUpdate != null)
                        {
                            itemUpdate.ReceiptNoteQty += model.ReceiptNoteQty;
                        }
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

        public bool UpdateReturnReceiptNoteQty(List<PurchaseOrderDetailModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_Purchase_Order_Detail> listUpdate = new List<I_Purchase_Order_Detail>();
                    foreach (var model in models)
                    {
                        var itemUpdate = (from tb in cxt.I_Purchase_Order_Detail
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();
                        if (itemUpdate != null)
                        {
                            itemUpdate.ReturnReceiptNoteQty += model.ReturnReceiptNoteQty;
                        }
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
    }
}
