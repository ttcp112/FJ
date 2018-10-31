using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class InventoryFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        //private BaseFactory _baseFactory = null;
        private static Semaphore m_Semaphore = new Semaphore(1, 1);

        public InventoryFactory()
        {
            //_baseFactory = new BaseFactory();

        }

        public bool SaveInventory(List<InventoryModels> lstInput, string receiptId)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        //Insert
                        List<I_InventoryManagement> lstInsert = new List<I_InventoryManagement>();

                        foreach (var item in lstInput)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.StoreId && ww.IngredientId == item.IngredientId).FirstOrDefault();
                            if (obj == null)
                            {
                                obj = new I_InventoryManagement();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.IngredientId = item.IngredientId;
                                obj.StoreId = item.StoreId;
                                obj.Quantity = item.Quantity;
                                obj.Price = 0;

                                lstInsert.Add(obj);
                            }
                            else
                            {
                                double a = 0, b = 0;
                                var invenQt = double.TryParse(obj.Quantity.ToString(), out a);
                                var inputQty = double.TryParse(item.Quantity.ToString(), out b);

                                obj.Quantity = a + b;
                            }
                        }
                        if (lstInsert.Count > 0)
                        {
                            cxt.I_InventoryManagement.AddRange(lstInsert);
                        }

                        var receiptDb = cxt.I_ReceiptNote.Where(ww => ww.Id == receiptId).FirstOrDefault();
                        if (receiptDb != null)
                        {
                            receiptDb.Status = (int)Commons.EReceiptNoteStatus.Approve;
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
        }//end save inventory


        //Update qty after sale
        public bool UpdateInventoryWhenSale(string companyId, List<UsageManagementModel> lstUsage, string storeId, string usageId, string businessId, DateTime dFrom, DateTime dTo)
        {
            NSLog.Logger.Info(string.Format("UpdateInventoryWhenSale Company {0} | Store {1}", companyId, storeId), lstUsage);
            var result = true;
            RecipeFactory _recipeFactory = new RecipeFactory();
            List<UsageManagementModel> lstIngredientSMNotStockAble = new List<UsageManagementModel>();
            RecipeIngredientUsageModels _objIngredientDependent = null;
            using (NuWebContext cxt = new NuWebContext())
            {
                //check ingredientseftmade
                var listIngredientId = lstUsage.Select(ss => ss.Id).ToList();
                var lstlstIngredientSeftMade = cxt.I_Ingredient.Where(ww => listIngredientId.Contains(ww.Id) && ww.IsSelfMode
                && (ww.StockAble == null || !ww.StockAble.Value)).ToList();

                if (lstlstIngredientSeftMade != null && lstlstIngredientSeftMade.Any())
                {
                    var lstTmpId = lstlstIngredientSeftMade.Select(ss => ss.Id).ToList();
                    lstIngredientSMNotStockAble = lstUsage.Where(ww => lstTmpId.Contains(ww.Id)).ToList();
                    lstUsage = lstUsage.Where(ww => !lstTmpId.Contains(ww.Id)).ToList();

                    _objIngredientDependent = _recipeFactory.GetRecipesByIngredientSeftMade(lstTmpId);

                    NSLog.Logger.Info("UpdateInventoryForStockCount Seft-Made (Stock-Able: OFF)", lstIngredientSMNotStockAble);
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<InventoryTrackLogModel> lstInventoryTracLogFinal = new List<InventoryTrackLogModel>();
                        List<InventoryTrackLogModel> lstInventoryTracLog = new List<InventoryTrackLogModel>();
                        InventoryTrackLogModel inventoryTracLog = null;

                        List<I_InventoryManagement> lstInsert = new List<I_InventoryManagement>();
                        foreach (var item in lstUsage)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == storeId && ww.IngredientId == item.Id).FirstOrDefault();
                            if (obj != null)
                            {
                                {
                                    decimal a = 0, b = 0;
                                    var invenQt = decimal.TryParse(obj.Quantity.ToString(), out a);
                                    var inputQty = decimal.TryParse(item.Usage.ToString(), out b);

                                    inventoryTracLog = new InventoryTrackLogModel();
                                    inventoryTracLog.IngredientId = obj.IngredientId;
                                    inventoryTracLog.StoreId = storeId;
                                    inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.Sale;
                                    inventoryTracLog.TypeCodeId = usageId;
                                    inventoryTracLog.CurrentQty = (double)a;
                                    inventoryTracLog.NewQty = (double)b;
                                    lstInventoryTracLog.Add(inventoryTracLog);


                                    obj.Quantity = (double)(a - b);
                                }
                            }
                            //obj.Quantity -= item.Usage;
                            else
                            {
                                obj = new I_InventoryManagement();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.IngredientId = item.Id;
                                obj.StoreId = storeId;
                                obj.Quantity = -item.Usage;
                                obj.Price = 0;

                                lstInsert.Add(obj);

                                inventoryTracLog = new InventoryTrackLogModel();
                                inventoryTracLog.IngredientId = item.Id;
                                inventoryTracLog.StoreId = storeId;
                                inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.Sale;
                                inventoryTracLog.TypeCodeId = usageId;
                                inventoryTracLog.CurrentQty = 0;
                                inventoryTracLog.NewQty = item.Usage;
                                lstInventoryTracLog.Add(inventoryTracLog);

                            }
                        }

                        var usageManagement = cxt.I_UsageManagement.FirstOrDefault(ss => ss.Id == usageId);
                        if (usageManagement != null)
                            usageManagement.IsStockInventory = true;

                        if (_objIngredientDependent != null && (lstIngredientSMNotStockAble != null && lstIngredientSMNotStockAble.Any()))
                        {
                            foreach (var item in lstIngredientSMNotStockAble)
                            {
                                var obj = _objIngredientDependent.ListChilds.Where(ww => ww.MixtureIngredientId == item.Id).ToList();
                                obj.ForEach(ss => ss.BaseUsage = ss.BaseUsage * item.Usage);
                            }
                            var lstGroup = _objIngredientDependent.ListChilds.GroupBy(gg => gg.Id);
                            decimal a = 0, b = 0;
                            foreach (var item in lstGroup)
                            {
                                var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == storeId
                                && ww.IngredientId == item.Key).FirstOrDefault();

                                a = 0; b = 0;
                                decimal.TryParse(obj.Quantity.ToString(), out a);
                                decimal.TryParse(item.Sum(ss => ss.BaseUsage).ToString(), out b);

                                if (obj != null)
                                {
                                    obj.Quantity = (double)(a - b);

                                    inventoryTracLog = new InventoryTrackLogModel();
                                    inventoryTracLog.IngredientId = obj.IngredientId;
                                    inventoryTracLog.StoreId = storeId;
                                    inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.Sale;
                                    inventoryTracLog.TypeCodeId = usageId;
                                    inventoryTracLog.CurrentQty = (double)a;
                                    inventoryTracLog.NewQty = (double)b;

                                    lstInventoryTracLog.Add(inventoryTracLog);

                                }
                                else
                                {
                                    obj = new I_InventoryManagement();
                                    obj.Id = Guid.NewGuid().ToString();
                                    obj.IngredientId = item.Key;
                                    obj.StoreId = storeId;
                                    obj.Quantity = -item.Sum(ss => ss.BaseUsage);
                                    obj.Price = 0;

                                    lstInsert.Add(obj);

                                    inventoryTracLog = new InventoryTrackLogModel();
                                    inventoryTracLog.IngredientId = item.Key;
                                    inventoryTracLog.StoreId = storeId;
                                    inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.Sale;
                                    inventoryTracLog.TypeCodeId = usageId;
                                    inventoryTracLog.CurrentQty = 0;
                                    inventoryTracLog.NewQty = item.Sum(ss => ss.BaseUsage);
                                    lstInventoryTracLog.Add(inventoryTracLog);


                                }
                                NSLog.Logger.Info("_objIngredientDependent In UpdateInventoryWhenSale", obj);
                            }
                        }
                        if (lstInsert.Count > 0)
                        {
                            cxt.I_InventoryManagement.AddRange(lstInsert);
                        }

                        cxt.SaveChanges();
                        transaction.Commit();

                        Task.Run(() => InsertInventoryTrackLog(lstInventoryTracLog));
                        NSLog.Logger.Info(string.Format("UpdateInventoryWhenSale: StoreId [{0}] - [{1}]", storeId, usageId), lstUsage);

                        //AutoCreate DataEntry
                        StockCountFactory _stockCountFactory = new StockCountFactory();
                        var tmp = lstInventoryTracLog.GroupBy(gg => gg.IngredientId);
                        foreach (var item in tmp)
                        {
                            lstInventoryTracLogFinal.Add(new InventoryTrackLogModel()
                            {
                                IngredientId = item.Key,
                                StoreId = item.Select(ss => ss.StoreId).FirstOrDefault(),
                                TypeCode = item.Select(ss => ss.TypeCode).FirstOrDefault(),
                                TypeCodeId = item.Select(ss => ss.TypeCodeId).FirstOrDefault(),
                                NewQty = item.Sum(ss => ss.NewQty),
                            });
                        }
                        Task.Run(() => _stockCountFactory.AutoCreatedStockCount(companyId, storeId, businessId, dFrom, dTo, lstInventoryTracLogFinal));
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

        public List<InventoryInputModel> GetInventoryInput(InventoryRequest request)
        {
            //var result = new List<InventoryInputModel>();
            //using (var cxt = new NuWebContext())
            //{
            //    var query = (from r in cxt.I_ReceiptNote
            //                 join rd in cxt.I_ReceiptNoteDetail on r.Id equals rd.ReceiptNoteId
            //                 join i in cxt.I_Ingredient on rd.IngredientId equals i.Id
            //                 where r.CreatedDate >= request.DateFrom && r.CreatedDate <= request.DateTo && r.Status == (int)Commons.EReceiptNoteStatus.Approve
            //                 select new { rd, i });
            //    if (query != null && query.Any())
            //    {
            //        var tmp = query.Select(ss => new InventoryInputModel()
            //        {
            //            Code = ss.i.Code,
            //            Name = ss.i.Name,
            //            QtyInput = ss.rd.Quantity,
            //            UOMName = ss.i.BaseUOMName,
            //            ListInventoryDetailId = ss.rd.Id
            //        }).ToList();
            //        var tmpGroupCode = tmp.GroupBy(gg => gg.Code);
            //        int index = 1;
            //        InventoryInputModel obj = null;
            //        foreach (var item in tmpGroupCode)
            //        {
            //            obj = new InventoryInputModel();
            //            obj.Index = index;
            //            obj.Code = item.Key;
            //            obj.Name = item.Select(ss => ss.Name).FirstOrDefault();
            //            obj.QtyInput = item.Sum(ss => ss.QtyInput);
            //            obj.UOMName = item.Select(ss => ss.UOMName).FirstOrDefault();
            //            obj.ListInventoryDetailId = string.Join("|", item.Select(ss => ss.ListInventoryDetailId));

            //            result.Add(obj);
            //            index++;
            //        }

            //    }
            //}
            //return result;
            return new List<InventoryInputModel>();
        }
        public List<InventoryInputDetailModel> GetInventoryInputDetail(List<string> ListInventoryDetailIds)
        {
            //var result = new List<InventoryInputDetailModel>();
            //using (var cxt = new NuWebContext())
            //{
            //    var query = (from rd in cxt.I_ReceiptNoteDetail
            //                 join r in cxt.I_ReceiptNote on rd.ReceiptNoteId equals r.Id
            //                 where ListInventoryDetailIds.Contains(rd.Id)
            //                 select new { r, rd });
            //    if (query != null && query.Any())
            //    {
            //        result = query.Select(ss => new InventoryInputDetailModel()
            //        {
            //            ReceiptNo = ss.r.ReceiptNo,
            //            CreatedDate = ss.r.CreatedDate,
            //            Qty = ss.rd.Quantity
            //        }).ToList();
            //    }
            //}
            //return result;
            return new List<InventoryInputDetailModel>();
        }

        #region Ingredient2
        public bool UpdateInventoryForPO(List<InventoryModels> lstIngredientInput)
        {
            NSLog.Logger.Info("UpdateInventoryForPO", lstIngredientInput);
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        m_Semaphore.WaitOne();
                        List<I_InventoryManagement> lstInsert = new List<I_InventoryManagement>();
                        var lstGroupStore = lstIngredientInput.GroupBy(gg => new { gg.StoreId, gg.IngredientId });
                        foreach (var item in lstGroupStore)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.Key.StoreId && ww.IngredientId == item.Key.IngredientId).FirstOrDefault();
                            NSLog.Logger.Info("Inventory Ingredient Current (PO)", obj);
                            if (obj != null)
                            {
                                if (!obj.POQty.HasValue)
                                {
                                    obj.POQty = item.Sum(ss => ss.Quantity);
                                }
                                else
                                    obj.POQty += item.Sum(ss => ss.Quantity);
                            }
                            else
                            {
                                obj = new I_InventoryManagement();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.IngredientId = item.Key.IngredientId;
                                obj.StoreId = item.Key.StoreId;
                                obj.Quantity = 0;
                                obj.POQty = item.Sum(ss => ss.Quantity);
                                obj.Price = item.Select(ss => ss.Price).FirstOrDefault();

                                lstInsert.Add(obj);
                            }
                        }
                        if (lstInsert.Count > 0)
                        {
                            cxt.I_InventoryManagement.AddRange(lstInsert);
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
                        m_Semaphore.Release();
                    }
                }
            }
            return result;
        }

        public bool UpdateInventoryForWO(List<InventoryModels> lstIngredientInput)
        {
            NSLog.Logger.Info("UpdateInventoryForWO", lstIngredientInput);
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<I_InventoryManagement> lstInsert = new List<I_InventoryManagement>();
                        var lstGroupStore = lstIngredientInput.GroupBy(gg => new { gg.StoreId, gg.IngredientId });
                        foreach (var item in lstGroupStore)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.Key.StoreId && ww.IngredientId == item.Key.IngredientId).FirstOrDefault();
                            NSLog.Logger.Info("Inventory Ingredient Current (WO)", obj);
                            if (obj != null)
                            {
                                if (!obj.POQty.HasValue)
                                {
                                    obj.POQty = item.Sum(ss => ss.Quantity);
                                }
                                else
                                    obj.POQty += item.Sum(ss => ss.Quantity);
                            }
                            else
                            {
                                obj = new I_InventoryManagement();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.IngredientId = item.Key.IngredientId;
                                obj.StoreId = item.Key.StoreId;
                                obj.Quantity = 0;
                                obj.POQty = item.Sum(ss => ss.Quantity);
                                obj.Price = item.Select(ss => ss.Price).FirstOrDefault();

                                lstInsert.Add(obj);
                            }
                        }
                        if (lstInsert.Count > 0)
                        {
                            cxt.I_InventoryManagement.AddRange(lstInsert);
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
        public bool UpdateInventoryForPOForCloseManual(List<InventoryModels> lstIngredientInput)
        {
            NSLog.Logger.Info("UpdateInventoryForPOForCloseManual", lstIngredientInput);
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<I_InventoryManagement> lstInsert = new List<I_InventoryManagement>();
                        var lstGroupStore = lstIngredientInput.GroupBy(gg => new { gg.StoreId, gg.IngredientId });
                        double qty = 0;
                        foreach (var item in lstGroupStore)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.Key.StoreId && ww.IngredientId == item.Key.IngredientId).FirstOrDefault();
                            NSLog.Logger.Info("UpdateInventoryForPOForCloseManual inventory current", obj);
                            if (obj != null)
                            {
                                qty = (item.Sum(ss => ss.Quantity) / item.Sum(ss => ss.POQty)) * (item.Sum(ss => ss.POQty) - item.Sum(ss => ss.ReceiptQty) + item.Sum(ss => ss.ReturnQty));
                                if (!obj.POQty.HasValue)
                                {
                                    obj.POQty = 0;
                                }
                                else
                                {
                                    obj.POQty -= qty;
                                    if (obj.POQty < 0)
                                    {
                                        obj.POQty = 0;
                                    }
                                }

                            }
                            else
                            {
                                obj = new I_InventoryManagement();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.IngredientId = item.Key.IngredientId;
                                obj.StoreId = item.Key.StoreId;
                                obj.Quantity = 0;
                                obj.POQty = 0;
                                obj.Price = item.Select(ss => ss.Price).FirstOrDefault();

                                lstInsert.Add(obj);
                            }
                        }
                        if (lstInsert.Count > 0)
                        {
                            cxt.I_InventoryManagement.AddRange(lstInsert);
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

        public bool UpdateInventoryForWOForCloseManual(List<InventoryModels> lstIngredientInput)
        {
            NSLog.Logger.Info("UpdateInventoryForWOForCloseManual", lstIngredientInput);
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<I_InventoryManagement> lstInsert = new List<I_InventoryManagement>();
                        var lstGroupStore = lstIngredientInput.GroupBy(gg => new { gg.StoreId, gg.IngredientId });
                        double qty = 0;
                        foreach (var item in lstGroupStore)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.Key.StoreId && ww.IngredientId == item.Key.IngredientId).FirstOrDefault();
                            NSLog.Logger.Info("UpdateInventoryForWOForCloseManual inventory current", obj);
                            if (obj != null)
                            {
                                qty = (item.Sum(ss => ss.Quantity) / item.Sum(ss => ss.POQty)) * (item.Sum(ss => ss.POQty) - item.Sum(ss => ss.ReceiptQty) + item.Sum(ss => ss.ReturnQty));
                                if (!obj.POQty.HasValue)
                                {
                                    obj.POQty = 0;
                                }
                                else
                                {
                                    obj.POQty -= qty;
                                    if (obj.POQty < 0)
                                    {
                                        obj.POQty = 0;
                                    }
                                }

                            }
                            else
                            {
                                obj = new I_InventoryManagement();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.IngredientId = item.Key.IngredientId;
                                obj.StoreId = item.Key.StoreId;
                                obj.Quantity = 0;
                                obj.POQty = 0;
                                obj.Price = item.Select(ss => ss.Price).FirstOrDefault();

                                lstInsert.Add(obj);
                            }
                        }
                        if (lstInsert.Count > 0)
                        {
                            cxt.I_InventoryManagement.AddRange(lstInsert);
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

        public void UpdateInventoryForReceiptNote(List<InventoryModels> lstIngredientInput, string receiptNoteId, ref ResultModels result)
        {
            NSLog.Logger.Info("UpdateInventoryForReceiptNote Start", lstIngredientInput);
            result.IsOk = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        m_Semaphore.WaitOne();
                        List<InventoryTrackLogModel> lstInventoryTracLog = new List<InventoryTrackLogModel>();
                        InventoryTrackLogModel inventoryTracLog = null;

                        List<I_InventoryManagement> lstInsert = new List<I_InventoryManagement>();

                        foreach (var item in lstIngredientInput)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.StoreId && ww.IngredientId == item.IngredientId).FirstOrDefault();
                            NSLog.Logger.Info("Inventory Ingredient Current", obj);
                            if (obj != null)
                            {
                                inventoryTracLog = new InventoryTrackLogModel();
                                inventoryTracLog.IngredientId = obj.IngredientId;
                                inventoryTracLog.StoreId = item.StoreId;
                                inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.ReceiptNote;
                                inventoryTracLog.TypeCodeId = receiptNoteId;
                                inventoryTracLog.CurrentQty = obj.Quantity;
                                inventoryTracLog.NewQty = item.Quantity;
                                lstInventoryTracLog.Add(inventoryTracLog);

                                obj.Quantity += item.Quantity;
                                try
                                {
                                    if (!obj.POQty.HasValue)
                                        obj.POQty = 0;
                                    else
                                    {
                                        obj.POQty -= item.Quantity;
                                        if (obj.POQty.HasValue && obj.POQty.Value < 0)
                                            obj.POQty = 0;
                                    }

                                }
                                catch
                                {
                                }

                            }
                            else
                            {
                                //string ingredientName = string.Empty;
                                //var ingredient = cxt.I_Ingredient.Where(ww => ww.Id == item.IngredientId).FirstOrDefault();
                                //if (ingredient != null)
                                //    ingredientName = ingredient.Name;

                                //result.IsOk = false;
                                //result.Message = string.Format("Please input PO for [{0}] first!", ingredientName);
                                //return;

                                obj = new I_InventoryManagement();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.IngredientId = item.IngredientId;
                                obj.StoreId = item.StoreId;
                                obj.Quantity = item.Quantity;
                                obj.POQty = 0;
                                obj.Price = 0;

                                lstInsert.Add(obj);
                            }
                        }
                        if (lstInsert != null && lstInsert.Any())
                        {
                            NSLog.Logger.Info("UpdateInventoryForReceiptNote Insert New: ", lstInsert);
                            cxt.I_InventoryManagement.AddRange(lstInsert);
                        }

                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("UpdateInventoryForReceiptNote Success RN", receiptNoteId);

                        Task.Run(() => InsertInventoryTrackLog(lstInventoryTracLog));
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("UpdateInventoryForReceiptNote Error:", ex);
                        result.IsOk = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                        m_Semaphore.Release();
                    }
                }
            }
        }
        public void UpdateInventoryForTransfer(List<InventoryTransferModels> lstIngredientInput, string transferId, ref ResultModels result)
        {
            NSLog.Logger.Info("UpdateInventoryForTransfer Start", lstIngredientInput);
            result.IsOk = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<InventoryTrackLogModel> lstInventoryTracLog = new List<InventoryTrackLogModel>();
                        InventoryTrackLogModel inventoryTracLog = null;

                        List<I_InventoryManagement> lstInsert = new List<I_InventoryManagement>();
                        foreach (var item in lstIngredientInput)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.IssueStoreId && ww.IngredientId == item.IngredientId).FirstOrDefault();
                            if (obj != null)
                            {
                                //check stock in receive store
                                var stockReceive = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.ReceiveStoreId && ww.IngredientId == item.IngredientId).FirstOrDefault();
                                if (stockReceive != null)
                                {
                                    inventoryTracLog = new InventoryTrackLogModel();
                                    inventoryTracLog.IngredientId = obj.IngredientId;
                                    inventoryTracLog.StoreId = item.ReceiveStoreId;
                                    inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.StockTransfer;
                                    inventoryTracLog.TypeCodeId = transferId;
                                    inventoryTracLog.CurrentQty = stockReceive.Quantity;
                                    inventoryTracLog.NewQty = item.ReceiveQty;
                                    lstInventoryTracLog.Add(inventoryTracLog);


                                    stockReceive.Quantity += item.ReceiveQty;
                                }
                                else
                                {
                                    stockReceive = new I_InventoryManagement();
                                    stockReceive.Id = Guid.NewGuid().ToString();
                                    stockReceive.IngredientId = item.IngredientId;
                                    stockReceive.StoreId = item.ReceiveStoreId;
                                    stockReceive.Quantity = item.ReceiveQty;
                                    stockReceive.POQty = 0;
                                    stockReceive.Price = item.Price;
                                    lstInsert.Add(stockReceive);

                                    //Track
                                    inventoryTracLog = new InventoryTrackLogModel();
                                    inventoryTracLog.IngredientId = obj.IngredientId;
                                    inventoryTracLog.StoreId = item.ReceiveStoreId;
                                    inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.StockTransfer;
                                    inventoryTracLog.TypeCodeId = transferId;
                                    inventoryTracLog.CurrentQty = 0;
                                    inventoryTracLog.NewQty = item.ReceiveQty;
                                    lstInventoryTracLog.Add(inventoryTracLog);
                                }
                                //track
                                inventoryTracLog = new InventoryTrackLogModel();
                                inventoryTracLog.IngredientId = obj.IngredientId;
                                inventoryTracLog.StoreId = item.IssueStoreId;
                                inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.StockTransfer;
                                inventoryTracLog.TypeCodeId = transferId;
                                inventoryTracLog.CurrentQty = obj.Quantity;
                                inventoryTracLog.NewQty = item.IssueQty;
                                lstInventoryTracLog.Add(inventoryTracLog);

                                obj.Quantity -= item.IssueQty;

                            }
                            else
                            {
                                string ingredientName = string.Empty;
                                var ingredient = cxt.I_Ingredient.Where(ww => ww.Id == item.IngredientId).FirstOrDefault();
                                if (ingredient != null)
                                    ingredientName = ingredient.Name;

                                result.IsOk = false;
                                result.Message = string.Format("Not found [{0}] in Issue store", ingredientName);
                                return;
                            }
                        }
                        if (lstInsert.Count > 0)
                        {
                            cxt.I_InventoryManagement.AddRange(lstInsert);
                        }
                        cxt.SaveChanges();
                        transaction.Commit();

                        Task.Run(() => InsertInventoryTrackLog(lstInventoryTracLog));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result.IsOk = false;
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

        public bool CheckStockBeforeTransfer(string storeId, string ingredientId, string uomSelectedId, double issueQty
            , ref double QtyCurrentStock, ref string ingredientName, ref double rateReturn)
        {
            bool result = true;
            double rate = 1;
            using (var cxt = new NuWebContext())
            {
                //check UOM in ingerdient
                var obj = cxt.I_Ingredient.Where(ww => ww.BaseUOMId == uomSelectedId && ww.Id == ingredientId)
                    .Select(ss => new IngredientReceivingModels()
                    {
                        StoreId = storeId,
                        IngredientId = ss.Id,
                        ReceivingQty = 1,
                        IngredientName = ss.Name
                    }).FirstOrDefault();
                if (obj == null)
                {
                    obj = cxt.I_Ingredient.Where(ww => ww.ReceivingUOMId == uomSelectedId && ww.Id == ingredientId)
                        .Select(ss => new IngredientReceivingModels()
                        {
                            StoreId = storeId,
                            IngredientId = ss.Id,
                            ReceivingQty = ss.ReceivingQty,
                            IngredientName = ss.Name
                        }).FirstOrDefault();
                    if (obj == null)
                    {
                        obj = (from iu in cxt.I_Ingredient_UOM
                               from i in cxt.I_Ingredient.Where(ww => ww.Id == iu.IngredientId && ww.IsActive).DefaultIfEmpty()
                               where iu.UOMId == uomSelectedId && iu.IngredientId == ingredientId
                               select new IngredientReceivingModels()
                               {
                                   StoreId = storeId,
                                   IngredientId = iu.IngredientId,
                                   ReceivingQty = iu.ReceivingQty,
                                   IngredientName = i.Name
                               }).FirstOrDefault();
                    }
                }
                if (obj != null)
                {
                    rate = obj.ReceivingQty;
                    ingredientName = obj.IngredientName;
                }


                //Check current stock 
                var qty = cxt.I_InventoryManagement.Where(ww => ww.StoreId == storeId && ww.IngredientId == ingredientId).Select(ss => ss.Quantity).FirstOrDefault();
                QtyCurrentStock = qty;
                if ((rate * issueQty) > qty)//not enough
                {
                    result = false;
                }
            }
            rateReturn = rate;
            return result;
        }

        public bool CheckStockForReturn(string storeId, string ingredientId, double qtyReturn, double rate
            , ref double QtyCurrentStock, ref string ingredientName)
        {
            bool result = true;
            using (var cxt = new NuWebContext())
            {
                //Check current stock 
                var qty = cxt.I_InventoryManagement.Where(ww => ww.StoreId == storeId && ww.IngredientId == ingredientId).Select(ss => ss.Quantity).FirstOrDefault();
                QtyCurrentStock = qty;
                if ((rate * qtyReturn) > qty)//not enough
                {
                    var ingr = cxt.I_Ingredient.Where(ww => ww.Id == ingredientId).FirstOrDefault();
                    if (ingr != null)
                        ingredientName = ingr.Name;
                    result = false;
                }
            }

            return result;
        }
        public void UpdateInventoryForReturnNote(List<InventoryModels> lstIngredientInput, string returnNoteId, ref ResultModels result)
        {
            result.IsOk = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<InventoryTrackLogModel> lstInventoryTracLog = new List<InventoryTrackLogModel>();
                        InventoryTrackLogModel inventoryTracLog = null;

                        foreach (var item in lstIngredientInput)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.StoreId && ww.IngredientId == item.IngredientId).FirstOrDefault();
                            if (obj != null)
                            {
                                inventoryTracLog = new InventoryTrackLogModel();
                                inventoryTracLog.IngredientId = obj.IngredientId;
                                inventoryTracLog.StoreId = item.StoreId;
                                inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.ReturnNote;
                                inventoryTracLog.TypeCodeId = returnNoteId;
                                inventoryTracLog.CurrentQty = obj.Quantity;
                                inventoryTracLog.NewQty = item.Quantity;
                                lstInventoryTracLog.Add(inventoryTracLog);

                                obj.Quantity -= item.Quantity;
                            }
                            else
                            {
                                string ingredientName = string.Empty;
                                var ingredient = cxt.I_Ingredient.Where(ww => ww.Id == item.IngredientId).FirstOrDefault();
                                if (ingredient != null)
                                    ingredientName = ingredient.Name;

                                result.IsOk = false;
                                result.Message = string.Format("Not found [{0}] in inventory!", ingredientName);
                                return;
                            }
                        }

                        cxt.SaveChanges();
                        transaction.Commit();

                        Task.Run(() => InsertInventoryTrackLog(lstInventoryTracLog));

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result.IsOk = false;
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
        /*Editor by Trongntn 23-02-2017*/
        public List<IngredientModel> LoadIngredient(string StoreId, string companyId)
        {
            List<IngredientModel> lstResults = new List<IngredientModel>();

            using (var cxt = new NuWebContext())
            {
                var query = (from IM in cxt.I_InventoryManagement
                             from i in cxt.I_Ingredient
                             from uom in cxt.I_UnitOfMeasure
                             where i.Status != (int)Commons.EStatus.Deleted && IM.IngredientId.Equals(i.Id)
                                    && i.BaseUOMId.Equals(uom.Id) && uom.IsActive && IM.StoreId.Equals(StoreId)
                             select new { i, uom, IM });
                lstResults = query.Select(item => new IngredientModel()
                {
                    Id = item.i.Id,
                    Code = item.i.Code,
                    Name = item.i.Name,
                    BaseUOMName = item.uom.Name,
                    Qty = item.IM.Quantity,
                    IsSelfMode = item.i.IsSelfMode,
                    IsStockable = item.i.StockAble.HasValue ? item.i.StockAble.Value : false
                }).ToList();

                var queryExtend = (from i in cxt.I_Ingredient
                                   join uom in cxt.I_UnitOfMeasure on i.BaseUOMId equals uom.Id
                                   where i.Status != (int)Commons.EStatus.Deleted
                                         && uom.IsActive && i.CompanyId.Equals(companyId) && i.IsSelfMode
                                         && ((i.StockAble.HasValue && !i.StockAble.Value) || !i.StockAble.HasValue)
                                   select new IngredientModel()
                                   {
                                       Id = i.Id,
                                       Code = i.Code,
                                       Name = i.Name,
                                       BaseUOMName = uom.Name,
                                       Qty = 0,
                                       IsSelfMode = i.IsSelfMode,
                                       IsStockable = i.StockAble.HasValue ? i.StockAble.Value : false
                                   }).ToList();
                if (lstResults == null)
                    lstResults = new List<IngredientModel>();

                lstResults.AddRange(queryExtend);

                lstResults = lstResults.OrderBy(oo => oo.Name).ToList();
            }

            return lstResults;
        }
        public List<StockCountDetailModels> LoadIngredientForStockCount(string StoreId, string businessId)
        {
            List<StockCountDetailModels> lstResults = new List<StockCountDetailModels>();
            //if (!CheckBusinessDayExist(StoreId, businessId))
            //{
            using (var cxt = new NuWebContext())
            {
                var query = (from IM in cxt.I_InventoryManagement
                             from i in cxt.I_Ingredient
                             from uom in cxt.I_UnitOfMeasure
                             where i.Status != (int)Commons.EStatus.Deleted && IM.IngredientId.Equals(i.Id)
                                    && i.BaseUOMId.Equals(uom.Id) && uom.IsActive && IM.StoreId.Equals(StoreId)
                             select new { i, uom, IM });
                lstResults = query.Select(item => new StockCountDetailModels()
                {
                    IngredientId = item.i.Id,
                    IngredientCode = item.i.Code,
                    IngredientName = item.i.Name,
                    BaseUOM = item.uom.Name,
                    CloseBal = 0
                }).ToList();
                if (lstResults != null)
                    lstResults = lstResults.OrderBy(oo => oo.IngredientName).ToList();
            }
            return lstResults;
        }
        public List<IngredientModel> LoadIngredientForDataEntry(string storeId)
        {
            List<IngredientModel> lstResults = new List<IngredientModel>();
            //if (!CheckBusinessDayExist(StoreId, businessId))
            //{
            using (var cxt = new NuWebContext())
            {
                var query = (from IM in cxt.I_InventoryManagement
                             from i in cxt.I_Ingredient.Select(ss => new { ss.Id, ss.Code, ss.Name, ss.Status, ss.BaseUOMId })
                             from uom in cxt.I_UnitOfMeasure
                             where i.Status != (int)Commons.EStatus.Deleted && IM.IngredientId.Equals(i.Id)
                                    && i.BaseUOMId.Equals(uom.Id) && uom.IsActive && IM.StoreId.Equals(storeId)
                                    && IM.Quantity > 0
                             select new { i, uom, IM });
                lstResults = query.Select(item => new IngredientModel()
                {
                    Id = item.i.Id,
                    Code = item.i.Code,
                    Name = item.i.Name,
                    BaseUOMName = item.uom.Name,
                    Qty = item.IM.Quantity
                }).ToList();
                if (lstResults != null)
                    lstResults = lstResults.OrderBy(oo => oo.Name).ToList();
            }
            //}
            return lstResults;
        }
        public List<IngredientModel> LoadIngredient(string StoreId)
        {
            List<IngredientModel> lstResults = new List<IngredientModel>();
            using (var cxt = new NuWebContext())
            {
                var query = (from IM in cxt.I_InventoryManagement
                             from i in cxt.I_Ingredient.Where(ww => ww.Id == IM.IngredientId).DefaultIfEmpty()
                             from uom in cxt.I_UnitOfMeasure.Where(ww => ww.Id == i.ReceivingUOMId).DefaultIfEmpty()
                             where i.Status != (int)Commons.EStatus.Deleted && IM.StoreId.Equals(StoreId) && IM.Quantity > 0
                             select new { i, uom });
                lstResults = query.Select(item => new IngredientModel()
                {
                    Id = item.i.Id,
                    Code = item.i.Code,
                    Name = item.i.Name,
                    BaseUOMName = item.uom.Name,
                    BaseUOMId = item.i.ReceivingUOMId
                }).ToList();
            }

            return lstResults;
        }

        /// <summary>
        /// True: is exist
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessId"></param>
        /// <returns></returns>
        public bool CheckBusinessDayExist(StockCountModels model, bool isAutoCreate, DateTime? dTo)
        {
            bool result = false;
            using (var cxt = new NuWebContext())
            {
                I_StockCount obj = new I_StockCount();
                if (isAutoCreate)
                {
                    List<I_StockCountDetail> listDetailInsert = new List<I_StockCountDetail>();
                    I_StockCountDetail itemDetail = null;
                    obj = cxt.I_StockCount.Where(ww => ww.StoreId == model.StoreId && ww.BusinessId == model.BusinessId && ww.IsActived).FirstOrDefault();
                    if (obj != null && dTo.HasValue)
                    {
                        obj.ClosedOn = dTo;

                        //lst stock detail
                        var lstStockDetail = cxt.I_StockCountDetail.Where(ww => ww.StockCountId == obj.Id).ToList();
                        if (obj.Status == (int)Commons.EStockCountStatus.Open)
                        {
                            //obj.IsAutoCreated = true;
                            //obj.Status = (int)Commons.EStockCountStatus.Approved;
                            obj.ModifierBy = "Auto";
                            obj.ModifierDate = DateTime.Now;
                            foreach (var item in lstStockDetail)
                            {
                                item.Damage = model.ListItem.Where(ww => ww.IngredientId == item.IngredientId).Sum(ss => ss.Damage);
                                item.OtherQty = model.ListItem.Where(ww => ww.IngredientId == item.IngredientId).Sum(ss => ss.OtherQty);
                                item.Wastage = model.ListItem.Where(ww => ww.IngredientId == item.IngredientId).Sum(ss => ss.Wastage);
                                item.CloseBal = model.ListItem.Where(ww => ww.IngredientId == item.IngredientId).Sum(ss => ss.CloseBal);
                            }
                            var lstIngredientIdDbs = lstStockDetail.Select(ww => ww.IngredientId).ToList();
                            var lstNew = model.ListItem.Where(ww => !lstIngredientIdDbs.Contains(ww.IngredientId)).ToList();
                            if (lstNew != null && lstNew.Any())
                            {
                                foreach (var item in lstNew)
                                {
                                    itemDetail = new I_StockCountDetail();
                                    itemDetail.Id = Guid.NewGuid().ToString();
                                    itemDetail.StockCountId = obj.Id;
                                    itemDetail.IngredientId = item.IngredientId;
                                    itemDetail.CloseBal = item.CloseBal;
                                    itemDetail.Damage = item.Damage;
                                    itemDetail.Wastage = item.Wastage;
                                    itemDetail.OtherQty = item.OtherQty;
                                    itemDetail.Reasons = item.Reasons;
                                    itemDetail.OpenBal = item.OpenBal;
                                    itemDetail.AutoCloseBal = item.CloseBal;

                                    listDetailInsert.Add(itemDetail);
                                }
                            }

                        }
                        else//close
                        {
                            foreach (var item in lstStockDetail)
                            {
                                item.Damage = model.ListItem.Where(ww => ww.IngredientId == item.IngredientId).Sum(ss => ss.Damage);
                                item.OtherQty = model.ListItem.Where(ww => ww.IngredientId == item.IngredientId).Sum(ss => ss.OtherQty);
                                item.Wastage = model.ListItem.Where(ww => ww.IngredientId == item.IngredientId).Sum(ss => ss.Wastage);
                                item.AutoCloseBal = model.ListItem.Where(ww => ww.IngredientId == item.IngredientId).Sum(ss => ss.CloseBal);
                            }
                            UpdateInventoryWhenSaleForStockCount(lstStockDetail, model.StoreId);

                        }
                        if (listDetailInsert != null && listDetailInsert.Any())
                            cxt.I_StockCountDetail.AddRange(listDetailInsert);
                        cxt.SaveChanges();
                        _logger.Info("CheckBusinessDayExist For AutoCreate");
                    }
                }
                else //manual create
                {
                    obj = cxt.I_StockCount.Where(ww => ww.StoreId == model.StoreId && ww.BusinessId == model.BusinessId && ww.IsActived
                    && (!ww.IsAutoCreated.HasValue || (ww.IsAutoCreated.HasValue && !ww.IsAutoCreated.Value))).FirstOrDefault();
                }
                if (obj != null)
                    result = true;
            }
            return result;
        }

        public bool UpdateInventoryWhenSaleForStockCount(List<I_StockCountDetail> lstStockDetail, string storeId)
        {
            var result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var lstIngredientIds = lstStockDetail.Select(ss => ss.IngredientId).ToList();
                        var lstInventorys = cxt.I_InventoryManagement.Where(ww => ww.StoreId == storeId && lstIngredientIds.Contains(ww.IngredientId)).ToList();
                        foreach (var item in lstStockDetail)
                        {
                            var obj = lstInventorys.Where(ww => ww.IngredientId == item.IngredientId).FirstOrDefault();
                            if (obj != null)
                            {
                                {
                                    //decimal a = 0, b = 0;
                                    //var invenQt = decimal.TryParse(obj.Quantity.ToString(), out a);
                                    //var inputQty = decimal.TryParse(item.Sale.ToString(), out b);

                                    obj.Quantity = item.CloseBal;//(double)(a + b);
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

        public void ClosePOAuto(List<string> lstPOId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstPOIdUpdateStatus = new List<string>();
                    double qtyReceipt = 0, qty2 = 0;
                    for (int i = 0; i < lstPOId.Count; i++)
                    {
                        bool isClose = true;
                        string POId = lstPOId[i];
                        var lstDetail = cxt.I_Purchase_Order_Detail.Where(ww => ww.PurchaseOrderId == POId).ToList();
                        var lstIngredientId = lstDetail.Select(ss => ss.IngredientId).Distinct().ToList();
                        var lstIngredients = cxt.I_Ingredient.Where(ww => lstIngredientId.Contains(ww.Id)).ToList();
                        foreach (var item in lstDetail)
                        {
                            var ingre = lstIngredients.Where(ww => ww.Id == item.IngredientId).FirstOrDefault();
                            //(quantity received * (1 - tolerance) <= quantity ordered <= quantity received * (1 + tolerance))

                            //qty1 = ((item.ReceiptNoteQty.HasValue ? item.ReceiptNoteQty.Value : 0) * (1 - (ingre != null ? ingre.QtyTolerance/100 : 0))) - (item.ReturnReceiptNoteQty.HasValue ? item.ReturnReceiptNoteQty.Value : 0);
                            //qty2 = ((item.ReceiptNoteQty.HasValue ? item.ReceiptNoteQty.Value : 0) * (1 + (ingre != null ? ingre.QtyTolerance/100 : 0))) - (item.ReturnReceiptNoteQty.HasValue ? item.ReturnReceiptNoteQty.Value : 0);

                            //qty1 = ((item.Qty) * (1 - (ingre != null ? ingre.QtyTolerance / 100 : 0))) - (item.ReturnReceiptNoteQty.HasValue ? item.ReturnReceiptNoteQty.Value : 0);
                            qty2 = ((item.Qty) * (1 + (ingre != null ? ingre.QtyTolerance / 100 : 0))); //- (item.ReturnReceiptNoteQty.HasValue ? item.ReturnReceiptNoteQty.Value : 0);
                            qtyReceipt = ((item.ReceiptNoteQty.HasValue ? item.ReceiptNoteQty.Value : 0) - (item.ReturnReceiptNoteQty.HasValue ? item.ReturnReceiptNoteQty.Value : 0));
                            //if (qty1 <= item.Qty && item.Qty <= qty2)
                            //{
                            //    //do notthing
                            //}
                            if (qtyReceipt >= item.Qty && qtyReceipt <= qty2)
                            {
                                //do notthing
                            }
                            else
                            {
                                isClose = false;
                                break;
                            }
                        }
                        if (isClose)
                        {
                            lstPOIdUpdateStatus.Add(POId);
                        }
                    }

                    if (lstPOIdUpdateStatus != null && lstPOIdUpdateStatus.Count > 0)
                    {
                        var lstPO = cxt.I_Purchase_Order.Where(ww => lstPOIdUpdateStatus.Contains(ww.Id)).ToList();
                        foreach (var item in lstPO)
                        {
                            if (item.Status != (int)Commons.EPOStatus.Closed)
                            {
                                item.Status = (int)Commons.EPOStatus.Closed;
                            }
                        }

                        NSLog.Logger.Info("ClosePOAuto", lstPO);
                        cxt.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public void CloseWOAuto(List<string> lstWOId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstWOIdUpdateStatus = new List<string>();
                    double qtyReceipt = 0, qty2 = 0;
                    for (int i = 0; i < lstWOId.Count; i++)
                    {
                        bool isClose = true;
                        string WOId = lstWOId[i];
                        var lstDetail = cxt.I_Work_Order_Detail.Where(ww => ww.WorkOrderId == WOId).ToList();
                        var lstIngredientId = lstDetail.Select(ss => ss.IngredientId).Distinct().ToList();
                        var lstIngredients = cxt.I_Ingredient.Where(ww => lstIngredientId.Contains(ww.Id)).ToList();
                        foreach (var item in lstDetail)
                        {
                            var ingre = lstIngredients.Where(ww => ww.Id == item.IngredientId).FirstOrDefault();
                            qty2 = ((item.Qty) * (1 + (ingre != null ? ingre.QtyTolerance / 100 : 0))); //- (item.ReturnReceiptNoteQty.HasValue ? item.ReturnReceiptNoteQty.Value : 0);
                            qtyReceipt = ((item.ReceiptNoteQty.HasValue ? item.ReceiptNoteQty.Value : 0) - (item.ReturnReceiptNoteQty.HasValue ? item.ReturnReceiptNoteQty.Value : 0));
                            //if (qty1 <= item.Qty && item.Qty <= qty2)
                            //{
                            //    //do notthing
                            //}
                            if (qtyReceipt >= item.Qty && qtyReceipt <= qty2)
                            {
                                //do notthing
                            }
                            else
                            {
                                isClose = false;
                                break;
                            }
                        }
                        if (isClose)
                        {
                            lstWOIdUpdateStatus.Add(WOId);
                        }
                    }

                    if (lstWOIdUpdateStatus != null && lstWOIdUpdateStatus.Count > 0)
                    {
                        var lstWO = cxt.I_Work_Order.Where(ww => lstWOIdUpdateStatus.Contains(ww.Id)).ToList();
                        foreach (var item in lstWO)
                        {
                            if (item.Status != (int)Commons.EPOStatus.Closed)
                            {
                                item.Status = (int)Commons.EPOStatus.Closed;
                            }
                        }
                        NSLog.Logger.Info("CloseWOAuto", lstWO);
                        cxt.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public void UpdateInventoryForStockCount(List<InventoryModels> lstIngredientInput, string stockCountId, ref ResultModels result)
        {
            result.IsOk = true;
            //RecipeFactory _recipeFactory = new RecipeFactory();
            //List<InventoryModels> lstIngredientSMNotStockAble = new List<InventoryModels>();
            //RecipeIngredientUsageModels _objIngredientDependent = null;
            NSLog.Logger.Info("UpdateInventoryForStockCount", lstIngredientInput);

            using (NuWebContext cxt = new NuWebContext())
            {
                //check ingredientseftmade
                //var listIngredientId = lstIngredientInput.Select(ss => ss.IngredientId).ToList();
                //var lstlstIngredientSeftMade = cxt.I_Ingredient.Where(ww => listIngredientId.Contains(ww.Id) && ww.IsSelfMode
                //&& (ww.StockAble == null || !ww.StockAble.Value)).ToList();

                //if (lstlstIngredientSeftMade != null && lstlstIngredientSeftMade.Any())
                //{
                //    var lstTmpId = lstlstIngredientSeftMade.Select(ss => ss.Id).ToList();
                //    lstIngredientSMNotStockAble = lstIngredientInput.Where(ww => lstTmpId.Contains(ww.IngredientId)).ToList();
                //    lstIngredientInput = lstIngredientInput.Where(ww => !lstTmpId.Contains(ww.IngredientId)).ToList();

                //    _objIngredientDependent = _recipeFactory.GetRecipesByIngredientSeftMade(lstTmpId);

                //    NSLog.Logger.Info("UpdateInventoryForStockCount Seft-Made (Stock-Able: OFF)", lstIngredientSMNotStockAble);
                //}
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<InventoryTrackLogModel> lstInventoryTracLog = new List<InventoryTrackLogModel>();
                        InventoryTrackLogModel inventoryTracLog = null;
                        foreach (var item in lstIngredientInput)
                        {
                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.StoreId && ww.IngredientId == item.IngredientId).FirstOrDefault();
                            if (obj != null)
                            {
                                inventoryTracLog = new InventoryTrackLogModel();
                                inventoryTracLog.IngredientId = obj.IngredientId;
                                inventoryTracLog.StoreId = item.StoreId;
                                inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.StockCount;
                                inventoryTracLog.TypeCodeId = stockCountId;
                                inventoryTracLog.CurrentQty = obj.Quantity;
                                inventoryTracLog.NewQty = item.Quantity;
                                lstInventoryTracLog.Add(inventoryTracLog);

                                obj.Quantity = item.Quantity;
                            }
                            else
                            {
                                string ingredientName = string.Empty;
                                var ingredient = cxt.I_Ingredient.Where(ww => ww.Id == item.IngredientId).FirstOrDefault();
                                if (ingredient != null)
                                    ingredientName = ingredient.Name;

                                result.IsOk = false;
                                result.Message = string.Format("Not found [{0}] in store!", ingredientName);
                            }
                        }
                        //if (_objIngredientDependent != null && (lstIngredientSMNotStockAble != null && lstIngredientSMNotStockAble.Any()))
                        //{
                        //    string storeId = lstIngredientSMNotStockAble.Select(ss => ss.StoreId).FirstOrDefault();
                        //    foreach (var item in lstIngredientSMNotStockAble)
                        //    {
                        //        var obj = _objIngredientDependent.ListChilds.Where(ww => ww.MixtureIngredientId == item.IngredientId).ToList();
                        //        obj.ForEach(ss => ss.BaseUsage = ss.BaseUsage * item.Quantity);
                        //    }
                        //    var lstGroup = _objIngredientDependent.ListChilds.GroupBy(gg => gg.Id);
                        //    //decimal a = 0, b = 0;
                        //    foreach (var item in lstGroup)
                        //    {
                        //        var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == storeId
                        //        && ww.IngredientId == item.Key).FirstOrDefault();

                        //        //a = 0; b = 0;
                        //        //decimal.TryParse(obj.Quantity.ToString(), out a);
                        //        //decimal.TryParse(item.Sum(ss => ss.BaseUsage).ToString(), out b);

                        //        if (obj != null)
                        //        {
                        //            obj.Quantity = item.Sum(ss => ss.BaseUsage);
                        //        }
                        //    }
                        //}

                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("UpdateInventoryForStockCount Success", lstIngredientInput);
                        //Task.Run(() => InsertInventoryTrackLog(lstInventoryTracLog));
                    }
                    catch (Exception ex)
                    {
                        //_logger.Error(ex);
                        NSLog.Logger.Error("UpdateInventoryForStockCount", ex);
                        result.IsOk = false;
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

        private async void InsertInventoryTrackLog(List<InventoryTrackLogModel> lstInventoryTracLog)
        {
            try
            {
                using (NuWebContext cxt = new NuWebContext())
                {
                    var lstInsert = new List<I_InventoryManagementTrackLog>();
                    I_InventoryManagementTrackLog obj = null;
                    foreach (var item in lstInventoryTracLog)
                    {
                        obj = new I_InventoryManagementTrackLog();

                        obj.Id = Guid.NewGuid().ToString();
                        obj.IngredientId = item.IngredientId;
                        obj.TypeCode = item.TypeCode;
                        obj.TypeCodeId = item.TypeCodeId;
                        obj.CurrentQty = item.CurrentQty;
                        obj.NewQty = item.NewQty;
                        obj.StoreId = item.StoreId;
                        obj.CreatedBy = "System";
                        obj.CreatedDate = DateTime.Now;

                        lstInsert.Add(obj);
                    }
                    cxt.I_InventoryManagementTrackLog.AddRange(lstInsert);
                    cxt.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        //==========================================================
        // For seft-made
        //==========================================================
        public void UpdateInventoryForSeftMadeReceiptNoteWhenStockAbleOn(List<InventoryModels> lstIngredientInput, RecipeIngredientUsageModels _objIngredientDependent,
            string receiptNoteId, ref ResultModels result)
        {
            NSLog.Logger.Info("UpdateInventoryForSeftMadeReceiptNoteWhenStockAbleOn Start", lstIngredientInput);
            result.IsOk = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        string storeId = lstIngredientInput.Select(ss => ss.StoreId).FirstOrDefault();
                        List<InventoryTrackLogModel> lstInventoryTracLog = new List<InventoryTrackLogModel>();
                        InventoryTrackLogModel inventoryTracLog = null;
                        List<I_InventoryManagement> lstInventorys = new List<I_InventoryManagement>();
                        //RecipeIngredientUsageModels _objDepentNew = new RecipeIngredientUsageModels();
                        foreach (var item in lstIngredientInput)
                        {
                            //var lst = _objIngredientDependent.ListChilds.Where(ww => ww.MixtureIngredientId == item.IngredientId).ToList();
                            //lst.ForEach(gg => gg.BaseUsage = gg.BaseUsage * item.Quantity);
                            //_objDepentNew.ListChilds.AddRange(lst);

                            var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == item.StoreId && ww.IngredientId == item.IngredientId).FirstOrDefault();
                            if (obj != null)
                            {
                                inventoryTracLog = new InventoryTrackLogModel();
                                inventoryTracLog.IngredientId = obj.IngredientId;
                                inventoryTracLog.StoreId = item.StoreId;
                                inventoryTracLog.TypeCode = (int)Commons.ETableZipCode.ReceiptNoteSelfMade;
                                inventoryTracLog.TypeCodeId = receiptNoteId;
                                inventoryTracLog.CurrentQty = obj.Quantity;
                                inventoryTracLog.NewQty = item.Quantity;
                                lstInventoryTracLog.Add(inventoryTracLog);

                                obj.Quantity += item.Quantity;
                                try
                                {
                                    if (!obj.POQty.HasValue)
                                        obj.POQty = 0;
                                    else
                                    {
                                        obj.POQty -= item.Quantity;
                                        if (obj.POQty.HasValue && obj.POQty.Value < 0)
                                            obj.POQty = 0;
                                    }

                                }
                                catch
                                {
                                }

                            }
                            else
                            {
                                //create new inventory
                                obj = new I_InventoryManagement();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.StoreId = storeId;
                                obj.IngredientId = item.IngredientId;
                                obj.Quantity = item.Quantity;
                                obj.Price = 0;
                                obj.POQty = 0;
                                lstInventorys.Add(obj);

                                //return;
                            }
                        }
                        if (lstInventorys != null && lstInventorys.Any())
                        {
                            NSLog.Logger.Info("List ingredient seft-made new", lstInventorys);
                            cxt.I_InventoryManagement.AddRange(lstInventorys);
                        }

                        ///ingredient dependent
                        //NSLog.Logger.Info()
                        if (_objIngredientDependent != null && _objIngredientDependent.ListChilds != null && _objIngredientDependent.ListChilds.Any())
                        {
                            var lstGroup = _objIngredientDependent.ListChilds.GroupBy(gg => gg.Id);
                            NSLog.Logger.Info("List ingredient dependence", _objIngredientDependent.ListChilds);
                            decimal a = 0, b = 0;
                            foreach (var item in lstGroup)
                            {
                                var obj = cxt.I_InventoryManagement.Where(ww => ww.StoreId == storeId && ww.IngredientId == item.Key).FirstOrDefault();

                                a = 0; b = 0;
                                decimal.TryParse(obj.Quantity.ToString(), out a);
                                decimal.TryParse(item.Sum(ss => ss.TotalUsage).ToString(), out b);

                                obj.Quantity = (double)(a - b);

                                NSLog.Logger.Info(string.Format("UpdateStock CurrentQty: {0} | SaleQty: {1} | NewQty: {2}", a, b, obj.Quantity), obj);
                            }
                        }
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("UpdateInventoryForSeftMadeReceiptNoteWhenStockAbleOn Success");
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("UpdateInventoryForSeftMadeReceiptNoteWhenStockAbleOn", ex);
                        _logger.Error(ex);
                        result.IsOk = false;
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

        public bool CheckStockBeforeRNSeftMade(string storeId, List<RecipeIngredientUsageModels> lstIngredientUsages
           , ref double QtyCurrentStock, ref List<ErrorEnoughModels> lstIngredientNames)
        {
            var lstIngredientUsagesGroup = lstIngredientUsages.GroupBy(gg => gg.Id).ToList();
            bool result = true;
            string ingredientName = string.Empty;
            ErrorEnoughModels objError = null;
            using (var cxt = new NuWebContext())
            {
                var lstIngredientIds = lstIngredientUsages.Select(ss => ss.Id).Distinct().ToList();
                var lstCurrentStocks = cxt.I_InventoryManagement.Where(ww => ww.StoreId == storeId && lstIngredientIds.Contains(ww.IngredientId)).ToList();
                I_InventoryManagement obj = null;
                for (int i = 0; i < lstIngredientIds.Count; i++)
                {
                    var input = lstIngredientUsagesGroup.Where(ww => ww.Key == lstIngredientIds[i]).FirstOrDefault();
                    obj = lstCurrentStocks.Where(ww => ww.IngredientId == lstIngredientIds[i]).FirstOrDefault();
                    if (obj == null || obj.Quantity < input.Sum(ss => ss.TotalUsage))// not enough stock
                    {
                        if (obj != null)
                        {
                            QtyCurrentStock = obj.Quantity;
                        }
                        objError = lstIngredientNames.Where(ww => ww.MixIngredientId == input.Select(ss => ss.MixtureIngredientId).FirstOrDefault()).FirstOrDefault();
                        if (objError == null)
                        {
                            objError = new ErrorEnoughModels();
                            objError.MixIngredientId = lstIngredientUsages.Where(ww => ww.Id == lstIngredientIds[i]).Select(ss => ss.MixtureIngredientId).FirstOrDefault();
                            lstIngredientNames.Add(objError);
                        }
                        ingredientName = cxt.I_Ingredient.Where(ww => ww.Id == input.Key).Select(ss => ss.Name).FirstOrDefault();
                        objError.ListIngredientNameNotEnough.Add(ingredientName);

                        //lstIngredientNames.Add(ingredientName);
                        result = false;
                    }
                }
            }
            return result;
        }
        #endregion End- Ingredient2

        public void UpdateBusiness(string storeId, string businessId, DateTime dFrom, DateTime dTo,
            List<string> _lstReceiptNoteId, List<string> _lstTransOutlId
                        , List<string> _lstTransInId, List<string> _lstReturnId, List<string> _lstDataEntryId, List<string> _lstRNSeftMadeId)
        {
            using (var cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        //RN
                        var lstReceipteds = cxt.I_ReceiptNote.Where(ww => (ww.BusinessId == null || ww.BusinessId == string.Empty)
                        && ww.StoreId == storeId && _lstReceiptNoteId.Contains(ww.Id)).ToList();

                        lstReceipteds.ForEach(ss => ss.BusinessId = businessId);
                        _logger.Info(string.Format("The start get RN with Business: [{0}]", businessId));

                        //RN seft-made
                        var lstReceiptedSeftMades = cxt.I_ReceiptNoteForSeftMade.Where(ww => (ww.BusinessId == null || ww.BusinessId == string.Empty)
                        && ww.StoreId == storeId && _lstRNSeftMadeId.Contains(ww.Id)).ToList();

                        lstReceiptedSeftMades.ForEach(ss => ss.BusinessId = businessId);
                        _logger.Info(string.Format("The start get RN-SeftMade with Business: [{0}]", businessId));

                        //return
                        var lstReturns = (from rn in cxt.I_Return_Note
                                          join h in cxt.I_ReceiptNote on rn.ReceiptNoteId equals h.Id
                                          where (rn.BusinessId == null || rn.BusinessId == string.Empty) && h.StoreId == storeId && _lstReturnId.Contains(rn.Id)
                                          select rn).ToList();

                        lstReturns.ForEach(ss => ss.BusinessId = businessId);
                        _logger.Info(string.Format("The start get Return note with Business: [{0}]", businessId));

                        //transfer Out
                        var lstTransfers = cxt.I_Stock_Transfer.Where(ww => (ww.BusinessId == null || ww.BusinessId == string.Empty)
                        && (ww.IssueStoreId == storeId) && _lstTransOutlId.Contains(ww.Id)).ToList();
                        lstTransfers.ForEach(ss => ss.BusinessId = businessId);
                        _logger.Info(string.Format("The start get Transfer with Business: [{0}]", businessId));

                        //transfer in
                        var lstTransfersIn = cxt.I_Stock_Transfer.Where(ww => (ww.BusinessReceiveId == null || ww.BusinessReceiveId == string.Empty)
                        && (ww.ReceiveStoreId == storeId) && _lstTransInId.Contains(ww.Id)).ToList();

                        lstTransfersIn.ForEach(ss => ss.BusinessReceiveId = businessId);
                        _logger.Info(string.Format("The start get Transfer in with Business: [{0}]", businessId));

                        //DataEntry

                        var lstDataEntry = cxt.I_DataEntry.Where(ww => (ww.BusinessId == null || ww.BusinessId == string.Empty)
                        && ww.StoreId == storeId && _lstDataEntryId.Contains(ww.Id)).ToList();

                        //update damage & wast & orderQty to stockCount
                        if (lstDataEntry != null && lstDataEntry.Any())
                        {
                            _logger.Info(string.Format("The start get DataEntry with Business: [{0}]", businessId));
                            foreach (var item in lstDataEntry)
                            {
                                item.BusinessId = businessId;
                                item.StartedOn = dFrom;
                                item.ClosedOn = dTo;
                            }
                            _logger.Info(string.Format("End set Business for DataEntry: [{0}]", businessId));


                        }

                        cxt.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }//end Transaction
            }

        }
    }
}
