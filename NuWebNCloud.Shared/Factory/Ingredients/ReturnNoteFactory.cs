using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class ReturnNoteFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        private ReceiptNoteFactory _RNFactory = null;
        private ReturnNoteDetailFactory _RNDFactory = null;
        private PurchaseOrderDetailFactory _PODetailFactory = null;
        InventoryFactory _inventoryFactory = null;

        public ReturnNoteFactory()
        {
            _baseFactory = new BaseFactory();
            //=====
            _RNFactory = new ReceiptNoteFactory();
            _RNDFactory = new ReturnNoteDetailFactory();
            _PODetailFactory = new PurchaseOrderDetailFactory();
            _inventoryFactory = new InventoryFactory();
        }

        public bool Insert(ReturnNoteModels model, string storeId, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        ResultModels resultModels = new ResultModels();
                        List<InventoryModels> lstInventory = new List<InventoryModels>();

                        I_Return_Note item = new I_Return_Note();
                        string ReturnNoteId = Guid.NewGuid().ToString();
                        item.Id = ReturnNoteId;

                        item.ReceiptNoteId = model.ReceiptNoteId;
                        item.ReturnNoteNo = CommonHelper.GetGenNo(Commons.ETableZipCode.ReturnNote, storeId);

                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.ModifierBy = model.ModifierBy;
                        item.ModifierDate = model.ModifierDate;
                        item.IsActived = model.IsActived;

                        cxt.I_Return_Note.Add(item);


                        //========Change Status Return For RN
                        //List<string> lstId = new List<string>();
                        //lstId.Add(model.ReceiptNoteId);
                        //_RNFactory.ChangeStatus(lstId, (int)Commons.EReceiptNoteStatus.Return);
                        var RNote = cxt.I_ReceiptNote.Where(x => x.Id.Equals(model.ReceiptNoteId)).FirstOrDefault();
                        if (RNote != null)
                        {
                            RNote.Status = (int)Commons.EReceiptNoteStatus.Return;
                        }
                        List<PurchaseOrderDetailModels> PODetailModels = new List<PurchaseOrderDetailModels>();
                        foreach (var RNDetailParent in model.ListPurchaseOrder)
                        {
                            foreach (var RNDetailChild in RNDetailParent.ListItem)
                            {

                                var itemUpdate = (from tb in cxt.I_Purchase_Order_Detail
                                                  where tb.Id == RNDetailChild.Id
                                                  select tb).FirstOrDefault();
                                if (itemUpdate != null)
                                {
                                    itemUpdate.ReturnReceiptNoteQty += RNDetailChild.ReturnQty;
                                }
                            }
                        }

                        List<I_Return_Note_Detail> ListInsertReturnNoteDetail = new List<I_Return_Note_Detail>();
                        foreach (var RNDetailParent in model.ListPurchaseOrder)
                        {
                            foreach (var RNDetailChild in RNDetailParent.ListItem)
                            {
                                //var sumReturnNoteDetail = cxt.I_Return_Note_Detail.Where(x => x.ReceiptNoteDetailId.Equals(RNDetailChild.ReceiptNoteDetailId)).Sum(x => (double?)x.ReturnQty) ?? 0;
                                ListInsertReturnNoteDetail.Add(new I_Return_Note_Detail
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ReturnNoteId = ReturnNoteId,
                                    ReceiptNoteDetailId = RNDetailChild.ReceiptNoteDetailId,
                                    //ReceivedQty = RNDetailChild.ReceivingQty - sumReturnNoteDetail,
                                    ReceivedQty = RNDetailChild.ReceivingQty,
                                    ReturnQty = RNDetailChild.ReturnQty,
                                    ReturnBaseQty = RNDetailChild.ReturnQty * RNDetailChild.BaseQty,
                                    IsActived = true,
                                });

                                lstInventory.Add(new InventoryModels()
                                {
                                    StoreId = storeId,
                                    IngredientId = RNDetailChild.IngredientId,
                                    Price = RNDetailChild.UnitPrice,
                                    Quantity = RNDetailChild.ReturnQty * RNDetailChild.BaseQty
                                });

                            }
                        }
                        cxt.I_Return_Note_Detail.AddRange(ListInsertReturnNoteDetail);

                        cxt.SaveChanges();
                        transaction.Commit();

                        //update inventory
                        _inventoryFactory.UpdateInventoryForReturnNote(lstInventory, ReturnNoteId, ref resultModels);
                        _logger.Info(string.Format("UpdateInventoryForReturnNote: [{0}] - [{1}]- [{2}]", resultModels.IsOk, ReturnNoteId, resultModels.Message));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        transaction.Rollback();
                        result = false;
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

        public bool Update(ReturnNoteModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = (from tb in cxt.I_Return_Note
                                      where tb.Id == model.Id
                                      select tb).FirstOrDefault();

                    itemUpdate.ReceiptNoteId = model.ReceiptNoteId;
                    itemUpdate.ReturnNoteNo = model.ReturnNoteNo;

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
                    I_Return_Note itemDelete = (from tb in cxt.I_Return_Note
                                                where tb.Id == Id
                                                select tb).FirstOrDefault();
                    //cxt.I_Return_Note.Remove(itemDelete);
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

        public List<ReturnNoteModels> GetData()
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Return_Note
                                     select new ReturnNoteModels()
                                     {
                                         Id = tb.Id,
                                         ReceiptNoteId = tb.ReceiptNoteId,
                                         ReturnNoteNo = tb.ReturnNoteNo,

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

        public ReturnNoteModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Return_Note
                                 where tb.Id == ID
                                 select new ReturnNoteModels()
                                 {
                                     Id = tb.Id,
                                     ReceiptNoteId = tb.ReceiptNoteId,
                                     ReturnNoteNo = tb.ReturnNoteNo,

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
