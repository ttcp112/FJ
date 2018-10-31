using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class ReceiptPurchaseOrderFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public ReceiptPurchaseOrderFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<ReceiptPurchaseOrderModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_Receipt_Purchase_Order> listInsert = new List<I_Receipt_Purchase_Order>();
                    I_Receipt_Purchase_Order item = null;
                    foreach (var model in models)
                    {
                        item = new I_Receipt_Purchase_Order();
                        item.Id = Guid.NewGuid().ToString();

                        item.ReceiptNoteId = model.ReceiptNoteId;
                        item.PurchaseOrderId = model.PurchaseOrderId;

                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.ModifierBy = model.ModifierBy;
                        item.ModifierDate = model.ModifierDate;
                        item.IsActived = model.IsActived;

                        listInsert.Add(item);
                    }
                    cxt.I_Receipt_Purchase_Order.AddRange(listInsert);
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

        public bool Update(ReceiptPurchaseOrderModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = (from tb in cxt.I_Receipt_Purchase_Order
                                      where tb.Id == model.Id
                                      select tb).FirstOrDefault();

                    itemUpdate.ReceiptNoteId = model.ReceiptNoteId;
                    itemUpdate.PurchaseOrderId = model.PurchaseOrderId;
                    itemUpdate.CreatedBy = model.CreatedBy;
                    itemUpdate.CreatedDate = model.CreatedDate;
                    itemUpdate.ModifierBy = model.ModifierBy;
                    itemUpdate.ModifierDate = model.ModifierDate;
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
                    I_Receipt_Purchase_Order itemDelete = (from tb in cxt.I_Receipt_Purchase_Order
                                                          where tb.Id == Id
                                                          select tb).FirstOrDefault();
                    //cxt.I_Receipt_Purchase_Order.Remove(itemDelete);
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

        public List<ReceiptPurchaseOrderModels> GetData()
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Receipt_Purchase_Order
                                     select new ReceiptPurchaseOrderModels()
                                     {
                                         Id = tb.Id,
                                         ReceiptNoteId = tb.ReceiptNoteId,
                                         PurchaseOrderId = tb.PurchaseOrderId,

                                         CreatedBy = tb.CreatedBy,
                                         CreatedDate = tb.CreatedDate,
                                         ModifierBy = tb.ModifierBy,
                                         ModifierDate = tb.ModifierDate,
                                         IsActived = tb.IsActived,
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

        public ReceiptPurchaseOrderModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Receipt_Purchase_Order
                                 where tb.Id == ID
                                 select new ReceiptPurchaseOrderModels()
                                 {
                                     Id = tb.Id,
                                     ReceiptNoteId = tb.ReceiptNoteId,
                                     PurchaseOrderId = tb.PurchaseOrderId,

                                     CreatedBy = tb.CreatedBy,
                                     CreatedDate = tb.CreatedDate,
                                     ModifierBy = tb.ModifierBy,
                                     ModifierDate = tb.ModifierDate,
                                     IsActived = tb.IsActived,

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
