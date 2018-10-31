using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Xero;
using NuWebNCloud.Shared.Models.Xero.Ingredient;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class IngredientFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private ProductFactory _productFactory = null;
        private IngredientUOMFactory _IngredientUOMFactory = null;
        private XeroFactory _xeroFactory = null;

        public IngredientFactory()
        {
            _baseFactory = new BaseFactory();
            _productFactory = new ProductFactory();
            _IngredientUOMFactory = new IngredientUOMFactory();
            _xeroFactory = new XeroFactory();
        }

        public bool Insert(IngredientModel model, ref string msg)
        {
            using (var cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var itemExsit = cxt.I_Ingredient.Any(x => x.Code.ToLower().Equals(model.Code.ToLower()) && x.Status != (int)Commons.EStatus.Deleted && x.CompanyId == model.CompanyId);
                        if (itemExsit)
                        {
                            msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code") + " [" + model.Code + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("is duplicated");
                            return false;
                        }
                        var item = new I_Ingredient();
                        string IngredientId = Guid.NewGuid().ToString();
                        item.Id = IngredientId;
                        item.Code = model.Code.Trim();
                        item.Name = model.Name.Trim();
                        item.Description = model.Description == null ? "" : model.Description;
                        item.BaseUOMName = "";
                        item.IsActive = model.IsActive;
                        item.Status = (int)Commons.EStatus.Actived;
                        item.PurchasePrice = model.PurchasePrice;
                        item.SalePrice = model.SalePrice;
                        item.ReOrderQty = model.ReOrderQty;
                        item.MinAlertQty = model.MinAlertQty;

                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.UpdatedBy = model.UpdatedBy;
                        item.UpdatedDate = model.UpdatedDate;

                        item.XeroId = model.XeroId;
                        item.CompanyId = model.CompanyId;
                        item.IsPurchase = model.IsPurchase;
                        item.IsCheckStock = model.IsCheckStock;
                        item.IsSelfMode = model.IsSelfMode;
                        item.StockAble = model.IsStockable;
                        item.BaseUOMId = model.BaseUOMId;
                        item.ReceivingUOMId = model.ReceivingUOMId;
                        item.ReceivingQty = model.ReceivingQty;
                        item.QtyTolerance = model.QtyTolerance;

                        cxt.I_Ingredient.Add(item);

                        /*Detail*/
                        //Insert Ingredient Supplier
                        List<I_Ingredient_Supplier> listIngSupplier = new List<I_Ingredient_Supplier>();
                        foreach (var supllier in model.ListIngSupplier)
                        {
                            listIngSupplier.Add(new I_Ingredient_Supplier
                            {
                                Id = Guid.NewGuid().ToString(),

                                IngredientId = IngredientId,
                                SupplierId = supllier.Id,

                                CreatedBy = model.CreatedBy,
                                CreatedDate = model.CreatedDate,
                                ModifierBy = model.UpdatedBy,
                                ModifierDate = model.UpdatedDate,
                                IsActived = true
                            });
                        }
                        if (listIngSupplier.Count > 0)
                        {
                            cxt.I_Ingredient_Supplier.AddRange(listIngSupplier);
                        }
                        //Insert Ingredient Usage UOM
                        List<I_Ingredient_UOM> listIngUOM = new List<I_Ingredient_UOM>();
                        foreach (var UsageUOMModel in model.ListIngUOM)
                        {
                            listIngUOM.Add(new I_Ingredient_UOM
                            {
                                Id = Guid.NewGuid().ToString(),
                                IngredientId = IngredientId,
                                UOMId = UsageUOMModel.UOMId,
                                BaseUOM = 0,
                                ReceivingQty = UsageUOMModel.ReceivingQty,
                                CreatedBy = model.CreatedBy,
                                CreatedDate = model.CreatedDate,
                                UpdatedBy = model.UpdatedBy,
                                UpdatedDate = model.UpdatedDate,
                                IsActived = true
                            });
                        }
                        if (listIngUOM.Count > 0)
                        {
                            cxt.I_Ingredient_UOM.AddRange(listIngUOM);
                        }
                        /*End Detail*/
                        cxt.SaveChanges();
                        //push to xero
                        //if (Commons.IsXeroIngredient)
                        //{
                        //    XeroFactory.PushIngredientsToXero(new XeroIngredientModel()
                        //    {
                        //        Id = item.Id,
                        //        Code = model.Code,
                        //        Name = model.Name,
                        //        PurchaseUnitPrice = model.PurchasePrice,
                        //        SaleUnitPrice = model.SalePrice,
                        //        InventoryAssetAccountCode = Commons.AcoountCode_Inventory,
                        //        AppRegistrationId = Commons.XeroRegistrationAppId,
                        //        AccessToken = Commons.XeroAccessToken,
                        //        StoreId = "Newstead",
                        //        Description = model.Description,
                        //        QtyOH = 1200,
                        //        IsTrackedAsInventory = true
                        //    });
                        //}

                        /* NS Xero INGREDIENT  */
                        List<IngredientModel> listItem = new List<IngredientModel>();
                        listItem.Add(model);
                        NSXeroIngredientResponseModels dataResponse = new NSXeroIngredientResponseModels();
                        XeroIngredientInsertOrUpdate(listItem, model.ListStoreId, "insert", ref msg, ref dataResponse);
                        //======
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        transaction.Rollback();
                        return false;
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
        }

        /* NS Xero INGREDIENT  */
        /// <summary>
        /// Function [XeroIngredientInsertOrUpdate]
        /// </summary>
        /// <param name="listItem">List Ingredient</param>
        /// <param name="Action">Action [Insert/Import => ItemId = "" | Update => ItemId != ""]</param>
        /// <param name="msg">Msg return from Function [IngredientInsertUpdate]</param>
        /// <param name="dataResponse">dataResponse return from Function [IngredientInsertUpdate]</param>
        /// <returns></returns>
        private bool XeroIngredientInsertOrUpdate(List<IngredientModel> listItem, List<string> ListStoreId, string Action, ref string msg, ref NSXeroIngredientResponseModels dataResponse)
        {
            bool result = false;
            try
            {
                if (Commons.isIntegrateXero(ListStoreId))
                {
                    string COGSAccountCode = Commons.NSXeroIngrePDAcoountCode;
                    string saleAccountCode = Commons.NSXeroIngreSDAcoountCode;
                    using (var cxt = new NuWebContext())
                    {
                        var CostOfGoodSold = (from _store in cxt.G_SettingOnStore
                                              from _setting in cxt.G_GeneralSetting
                                              where _store.SettingId == _setting.Id && ListStoreId.Contains(_store.StoreId)
                                                    && _store.Status && _setting.Status
                                                    && _setting.Code.Equals((byte)Commons.EGeneralSetting.CostOfGoodSold)
                                              select _store).FirstOrDefault();
                        if (CostOfGoodSold != null)
                            COGSAccountCode = CostOfGoodSold.Value;
                    }
                    //====================
                    List<NSXeroIngredientModels> ListIng = new List<NSXeroIngredientModels>();
                    foreach (var item in listItem)
                    {
                        PurchaseDetailsModels PDModel = new PurchaseDetailsModels
                        {
                            COGSAccountCode = COGSAccountCode,
                            UnitPrice = item.PurchasePrice,
                            AccountCode = "",
                            TaxType = "INPUT"
                        };
                        SalesDetailsModels SDModel = new SalesDetailsModels
                        {
                            UnitPrice = item.SalePrice,
                            AccountCode = saleAccountCode,
                            TaxType = "OUTPUT"
                        };
                        ListIng.Add(new NSXeroIngredientModels
                        {
                            ItemID = Action.Equals("update") ? item.Id : "",
                            Name = item.Name,
                            Code = item.Code,
                            Description = item.Description,
                            PurchaseDetails = PDModel,
                            SalesDetails = SDModel,
                            InventoryAssetAccountCode = Commons.NSXeroIngreInventoryAssetAccountCode,
                            QuantityOnHand = 0,
                            IsSold = true,
                            IsPurchased = item.IsPurchase,
                            PurchaseDescription = "",
                            IsTrackedAsInventory = true,
                            TotalCostPool = 0
                        });
                    }
                    var lstStore = _AttributeForLanguage.CurrentUser.listStore;
                    var StoreThirdParty = lstStore.Where(x => x.ThirdParty != null && x.ThirdParty.IPAddress != null && x.ThirdParty.IsIntegrate).FirstOrDefault();
                    if(!string.IsNullOrEmpty(StoreThirdParty.ThirdParty.IPAddress))
                    {
                        string AppRegistrationId = StoreThirdParty.ThirdParty.ThirdPartyID;// Commons.NSXeroAppRegistrationId;
                        string StoreId = StoreThirdParty.ThirdParty.IPAddress;//Commons.NSXeroStoreId;
                        string _hostXeroApi =  StoreThirdParty.ThirdParty.ApiURL;//

                        result = _xeroFactory.IngredientInsertUpdate(_hostXeroApi, ListIng, AppRegistrationId, StoreId, ref msg, ref dataResponse);
                    }
                   
                   
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("XeroIngredientInsertOrUpdate|Error: ", ex);
            }
            return result;
        }

        public bool Update(IngredientModel model, List<string> listIngUOMIdDelete, ref string msg)
        {
            using (var cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var itemExsit = cxt.I_Ingredient.Where(x => x.Code.ToLower().Equals(model.Code.ToLower()) && x.Status != (int)Commons.EStatus.Deleted && x.CompanyId == model.CompanyId).FirstOrDefault();
                        if (itemExsit != null)
                        {
                            if (!itemExsit.Id.Equals(model.Id))
                            {
                                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code") + " [" + model.Code + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("is duplicated");
                                return false;
                            }
                        }

                        var item = cxt.I_Ingredient.Where(ww => ww.Id == model.Id).FirstOrDefault();
                        if (item != null)
                        {
                            item.Code = model.Code.Trim();
                            item.Name = model.Name.Trim();
                            item.Description = model.Description == null ? "" : model.Description;
                            item.IsActive = model.IsActive;

                            item.PurchasePrice = model.PurchasePrice;
                            item.SalePrice = model.SalePrice;
                            item.ReOrderQty = model.ReOrderQty;
                            item.MinAlertQty = model.MinAlertQty;

                            item.UpdatedBy = model.UpdatedBy;
                            item.UpdatedDate = model.UpdatedDate;

                            item.IsPurchase = model.IsPurchase;
                            item.IsCheckStock = model.IsCheckStock;
                            item.IsSelfMode = model.IsSelfMode;
                            item.StockAble = model.IsStockable;
                            item.BaseUOMId = model.BaseUOMId;
                            item.ReceivingUOMId = model.ReceivingUOMId;
                            item.ReceivingQty = model.ReceivingQty;
                            item.QtyTolerance = model.QtyTolerance;

                            /*Detail*/
                            var ListDelIngSup = (from IS in cxt.I_Ingredient_Supplier
                                                 where IS.IngredientId.Equals(model.Id)
                                                 select IS).ToList();
                            if (ListDelIngSup.Count > 0)
                            {
                                ListDelIngSup.ForEach(x => x.IsActived = false);
                            }
                            List<I_Ingredient_Supplier> listIngSupplier = new List<I_Ingredient_Supplier>();
                            foreach (var supllier in model.ListIngSupplier)
                            {
                                listIngSupplier.Add(new I_Ingredient_Supplier
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CreatedBy = model.UpdatedBy,
                                    CreatedDate = model.UpdatedDate,
                                    IngredientId = model.Id,
                                    IsActived = true,
                                    ModifierBy = model.UpdatedBy,
                                    ModifierDate = model.UpdatedDate,
                                    SupplierId = supllier.Id
                                });
                            }
                            cxt.I_Ingredient_Supplier.AddRange(listIngSupplier);

                            /*Ingredient UOM*/
                            //Delete [Update IsActive = False] Ingredient Usage UOM
                            var lstDel = (from tb in cxt.I_Ingredient_UOM
                                          where listIngUOMIdDelete.Contains(tb.Id)
                                          select tb).ToList();
                            if (lstDel != null && lstDel.Count > 0)
                            {
                                lstDel.ForEach(ss => ss.IsActived = false);
                            }
                            //Update Ingredient Usage UOM
                            List<I_Ingredient_UOM> listIngUOM = new List<I_Ingredient_UOM>();
                            foreach (var UsageUOMModel in model.ListIngUOM)
                            {
                                var itemUpdate = (from tb in cxt.I_Ingredient_UOM
                                                  where tb.Id == UsageUOMModel.Id
                                                  select tb).FirstOrDefault();
                                if (itemUpdate != null) //Update
                                {
                                    itemUpdate.UOMId = UsageUOMModel.UOMId;
                                    itemUpdate.ReceivingQty = UsageUOMModel.ReceivingQty;
                                }
                                else //Insert
                                {
                                    listIngUOM.Add(new I_Ingredient_UOM
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IngredientId = model.Id,
                                        UOMId = UsageUOMModel.UOMId,
                                        BaseUOM = 0,
                                        ReceivingQty = UsageUOMModel.ReceivingQty,
                                        CreatedBy = model.UpdatedBy,
                                        CreatedDate = model.UpdatedDate,
                                        UpdatedBy = model.UpdatedBy,
                                        UpdatedDate = model.UpdatedDate,
                                        IsActived = true
                                    });
                                }

                            }
                            if (listIngUOM.Count > 0)
                            {
                                cxt.I_Ingredient_UOM.AddRange(listIngUOM);
                            }
                            /*End Detail*/

                            cxt.SaveChanges();

                            /* NS Xero INGREDIENT  */
                            List<IngredientModel> listItem = new List<IngredientModel>();
                            listItem.Add(model);
                            NSXeroIngredientResponseModels dataResponse = new NSXeroIngredientResponseModels();
                            XeroIngredientInsertOrUpdate(listItem, model.ListStoreId, "update", ref msg, ref dataResponse);
                            //==========
                            transaction.Commit();
                            return true;
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        transaction.Rollback();
                        return false;
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
        }

        public async void UpdateIngredient(string idOld, string idNew)
        {
            using (var cxt = new NuWebContext())
            {
                var item = cxt.I_Ingredient.Where(ww => ww.Id == idOld).FirstOrDefault();
                if (item != null)
                {
                    item.XeroId = idNew;
                    cxt.SaveChanges();
                }
            }
        }

        public void Delte(IngredientModel model, ref string msg)
        {
            using (var cxt = new NuWebContext())
            {
                if (IsCanDelete(model.Id))
                {
                    var item = cxt.I_Ingredient.Where(ww => ww.Id == model.Id).FirstOrDefault();
                    if (item != null)
                    {
                        item.Status = (int)Commons.EStatus.Deleted;
                        cxt.SaveChanges();
                    }
                }
                else
                    msg = "This Ingredient has been in used. Please deactivate it only.";
            }
        }

        public ResultModels CheckInsert(IngredientModel model)
        {
            var result = new ResultModels();
            result.IsOk = true;
            using (var cxt = new NuWebContext())
            {
                //Check code
                //var itemDb = cxt.I_Ingredient.Where(ww => (ww.Code == model.Code.Trim() || ww.Name.ToUpper() == model.Name.Trim().ToUpper()) && ww.CompanyId == model.CompanyId).FirstOrDefault();
                var itemDb = cxt.I_Ingredient.Where(ww => ww.Code.ToLower().Equals(model.Code.ToLower()) && ww.Status != (int)Commons.EStatus.Deleted && ww.CompanyId == model.CompanyId).FirstOrDefault();
                if (itemDb != null)
                {
                    result.IsOk = false;
                    //result.Message = "Ingredient code/ Name is exist in data!";
                    result.Message = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code") + " [" + model.Code + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("is duplicated");
                }

            }
            return result;
        }

        public List<IngredientModel> GetData(IngredientViewModel model)
        {
            List<IngredientModel> lstResults = new List<IngredientModel>();
            using (var cxt = new NuWebContext())
            {
                //var listUOM = cxt.I_UnitOfMeasure.ToList();

                var query = (from i in cxt.I_Ingredient
                             from baseUOM in cxt.I_UnitOfMeasure.Where(ww => ww.Id == i.BaseUOMId).DefaultIfEmpty()
                             from reUOM in cxt.I_UnitOfMeasure.Where(ww => ww.Id == i.ReceivingUOMId).DefaultIfEmpty()
                             where i.Status != (int)Commons.EStatus.Deleted && i.BaseUOMId != "" && i.ReceivingUOMId != ""
                             select new { i, baseUOM, reUOM });
                if (query != null && query.Any())
                {
                    if (model.CompanyId != null)
                        query = query.Where(ww => ww.i.CompanyId.Equals(model.CompanyId));
                    lstResults = query.Select(ss => new IngredientModel()
                    {
                        Id = ss.i.Id,
                        CompanyId = ss.i.CompanyId,
                        Code = ss.i.Code,
                        Name = ss.i.Name,
                        Description = ss.i.Description,
                        IsActive = ss.i.IsActive,
                        ReOrderQty = ss.i.ReOrderQty.HasValue ? ss.i.ReOrderQty.Value : 0,
                        MinAlertQty = ss.i.MinAlertQty.HasValue ? ss.i.MinAlertQty.Value : 0,
                        Status = ss.i.Status,

                        XeroId = ss.i.XeroId,

                        BaseUOMId = ss.i.BaseUOMId,
                        ReceivingUOMId = ss.i.ReceivingUOMId,
                        ReceivingQty = ss.i.ReceivingQty,
                        BaseUOMName = ss.baseUOM.Name,
                        ReceivingUOMName = ss.reUOM.Name

                    }).ToList();

                    //lstResults.ForEach(x =>
                    //{
                    //    var BaseUOM = listUOM.Where(z => z.Id.Equals(x.BaseUOMId)).FirstOrDefault();
                    //    var ReceivingUOM = listUOM.Where(z => z.Id.Equals(x.ReceivingUOMId)).FirstOrDefault();

                    //    x.BaseUOMName = BaseUOM == null ? "" : BaseUOM.Name;
                    //    x.ReceivingUOMName = ReceivingUOM == null ? "" : ReceivingUOM.Name;
                    //});
                }
                return lstResults;
            }
        }

        public List<IngredientModel> GetIngredient(string strSearch)
        {
            List<IngredientModel> lstResults = new List<IngredientModel>();
            using (var cxt = new NuWebContext())
            {
                var listUOM = cxt.I_UnitOfMeasure.ToList();

                var query = (from i in cxt.I_Ingredient
                             from uom in cxt.I_UnitOfMeasure
                             where i.Status != (int)Commons.EStatus.Deleted
                                    //&& i.IsActive
                                    && i.BaseUOMId.Equals(uom.Id)
                                    //&& uom.IsActive 
                                    && uom.Status != (int)Commons.EStatus.Deleted
                             select new { i, uom });
                if (query != null && query.Any())
                {
                    if (!string.IsNullOrEmpty(strSearch))
                    {
                        query = query.Where(ww => ww.i.Code.ToLower().Contains(strSearch.ToLower())
                        || ww.i.Name.ToLower().Contains(strSearch.ToLower()));
                    }
                    lstResults = query.Select(item => new IngredientModel()
                    {
                        Id = item.i.Id,
                        Code = item.i.Code,
                        Name = item.i.Name,
                        Description = item.i.Description,
                        BaseUOMName = item.uom.Name,
                        ReceivingUOMId = item.i.ReceivingUOMId,
                        ReceivingQty = item.i.ReceivingQty,
                        IsActive = item.i.IsActive,
                        CompanyId = item.i.CompanyId,
                        BaseUOMId = item.i.BaseUOMId
                    }).ToList();
                }

                lstResults.ForEach(x =>
                {
                    var ReceivingUOM = listUOM.Where(z => z.Id.Equals(x.ReceivingUOMId)).FirstOrDefault();
                    x.ReceivingUOMName = ReceivingUOM.Name;
                });

                return lstResults;
            }
        }

        public List<IngredientModel> GetIngredientBySupplier(string supplierId, string storeId)
        {
            List<IngredientModel> lstResults = new List<IngredientModel>();
            using (var cxt = new NuWebContext())
            {
                var listUOM = cxt.I_UnitOfMeasure.ToList();

                var query = (from ip in cxt.I_Ingredient_Supplier
                             from i in cxt.I_Ingredient.Where(ww => ww.Id == ip.IngredientId).DefaultIfEmpty()
                             from st in cxt.I_StoreSetting.Where(ww => ww.IngredientId == ip.IngredientId && ww.StoreId == storeId).Select(ss => new { ss.ReorderingQuantity }).DefaultIfEmpty()
                             from uom in cxt.I_UnitOfMeasure
                             where i.Status != (int)Commons.EStatus.Deleted
                                    && ip.IsActived
                                    && i.BaseUOMId.Equals(uom.Id)
                                    && i.IsActive
                                    && uom.Status != (int)Commons.EStatus.Deleted
                                    && !i.IsSelfMode
                             select new { ip, i, uom, st });
                if (query != null && query.Any())
                {
                    if (!string.IsNullOrEmpty(supplierId))
                    {
                        query = query.Where(ww => ww.ip.SupplierId == supplierId);
                    }
                    lstResults = query.Select(item => new IngredientModel()
                    {
                        Id = item.i.Id,
                        Code = item.i.Code,
                        Name = item.i.Name,
                        Description = item.i.Description,
                        PurchasePrice = item.i.PurchasePrice,

                        BaseUOMName = item.uom.Name,
                        ReceivingUOMId = item.i.ReceivingUOMId,
                        ReceivingQty = item.i.ReceivingQty,
                        IsActive = item.i.IsActive,
                        CompanyId = item.i.CompanyId,
                        BaseUOMId = item.i.BaseUOMId,
                        ReOrderQty = (item.st != null && item.st.ReorderingQuantity > 0) ? item.st.ReorderingQuantity : (item.i.ReOrderQty.HasValue ? item.i.ReOrderQty.Value : 0)
                    }).ToList();
                }

                lstResults.ForEach(x =>
                {
                    var ReceivingUOM = listUOM.Where(z => z.Id.Equals(x.ReceivingUOMId)).FirstOrDefault();
                    x.ReceivingUOMName = ReceivingUOM.Name;
                });

                return lstResults;
            }
        }

        public List<IngredientModel> GetIngredientUnlessData(string id)
        {
            List<IngredientModel> lstResults = new List<IngredientModel>();
            using (var cxt = new NuWebContext())
            {
                var query = (from i in cxt.I_Ingredient
                             where i.Status != (int)Commons.EStatus.Deleted && i.Id != id
                             && i.IsActive && !i.IsSelfMode
                             select i);
                if (query != null && query.Any())
                {
                    lstResults = query.Select(i => new IngredientModel()
                    {
                        Id = i.Id,
                        Code = i.Code,
                        Name = i.Name,
                        Description = i.Description,
                        BaseUOMName = i.BaseUOMName,
                        IsActive = i.IsActive,
                        Status = i.Status,
                        CreatedBy = i.CreatedBy,
                        CreatedDate = i.CreatedDate,
                        UpdatedBy = i.UpdatedBy,
                        UpdatedDate = i.UpdatedDate,
                        BaseUOMId = i.BaseUOMId,
                        CompanyId = i.CompanyId
                    }).ToList();
                }
                return lstResults;
            }
        }

        public ResultModels EnableActive(List<string> lstId, bool active)
        {
            //if (!active)
            //{
            //    lstId = ListIngInActive(lstId);
            //}
            ResultModels data = new ResultModels();
            using (var cxt = new NuWebContext())
            {
                var lstObj = cxt.I_Ingredient.Where(ww => lstId.Contains(ww.Id)).ToList();
                if (lstObj != null && lstObj.Count > 0)
                {
                    lstObj.ForEach(ss => ss.IsActive = active);
                    cxt.SaveChanges();
                    data.IsOk = true;
                }
                else
                {
                    data.IsOk = false;
                    data.Message = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Not found") + "!";
                }
            }
            return data;
        }

        public IngredientModel GetIngredientById(string id)
        {
            using (var cxt = new NuWebContext())
            {
                var model = cxt.I_Ingredient.Where(ww => ww.Id == id)
                    .Select(ss => new IngredientModel()
                    {
                        Id = ss.Id,
                        Code = ss.Code,
                        Name = ss.Name,
                        Description = ss.Description,
                        BaseUOMName = ss.BaseUOMName,
                        IsActive = ss.IsActive,
                        ReOrderQty = ss.ReOrderQty.HasValue ? ss.ReOrderQty.Value : 0,
                        MinAlertQty = ss.MinAlertQty.HasValue ? ss.MinAlertQty.Value : 0,
                        PurchasePrice = ss.PurchasePrice,
                        SalePrice = ss.SalePrice,

                        IsPurchase = ss.IsPurchase,
                        IsCheckStock = ss.IsCheckStock,
                        IsSelfMode = ss.IsSelfMode,
                        IsStockable = ss.StockAble.HasValue ? ss.StockAble.Value : false,

                        BaseUOMId = ss.BaseUOMId,
                        ReceivingUOMId = ss.ReceivingUOMId,
                        ReceivingQty = ss.ReceivingQty,

                        QtyTolerance = ss.QtyTolerance,
                        CompanyId = ss.CompanyId
                    })
                    .FirstOrDefault();
                //===========
                var listUOM = cxt.I_UnitOfMeasure.ToList();
                var BaseUOM = listUOM.Where(z => z.Id.Equals(model.BaseUOMId)).FirstOrDefault();
                var ReceivingUOM = listUOM.Where(z => z.Id.Equals(model.ReceivingUOMId)).FirstOrDefault();
                model.BaseUOMName = BaseUOM == null ? "" : BaseUOM.Name;
                model.ReceivingUOMName = ReceivingUOM == null ? "" : ReceivingUOM.Name;

                return model;
            }
        }

        public List<IngredientImportResultItem> Import(string filePath, string userName, List<string> lstMerchantId, out int totalRowExcel, List<string> ListCompanyId, List<string> ListStoreId, ref string msg)
        {
            totalRowExcel = 0;
            List<IngredientImportResultItem> importItems = new List<IngredientImportResultItem>();
            using (var cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {

                    DataTable dtIngredient = _productFactory.ReadExcelFile(@filePath, "Ingredients");
                    DataTable dtIngredientUOM = _productFactory.ReadExcelFile(filePath, "IngredientUOM");
                    DataTable dtIngredientSupplier = _productFactory.ReadExcelFile(filePath, "IngredientSupplier");

                    string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/IngredientImportTemplate.xlsx";
                    DataTable dtTmpIngredient = _productFactory.ReadExcelFile(@tmpExcelPath, "Ingredients");
                    DataTable dtTmpIngredientUOM = _productFactory.ReadExcelFile(@tmpExcelPath, "IngredientUOM");
                    DataTable dtTmpIngredientSupplier = _productFactory.ReadExcelFile(@tmpExcelPath, "IngredientSupplier");

                    if (dtIngredient.Columns.Count != dtTmpIngredient.Columns.Count)
                    {
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel); ;// "Wrong template. Please update lastest template!";
                        return importItems;
                    }
                    if (dtIngredientUOM.Columns.Count != dtTmpIngredientUOM.Columns.Count)
                    {
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                        return importItems;
                    }
                    if (dtIngredientSupplier.Columns.Count != dtTmpIngredientSupplier.Columns.Count)
                    {
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                        return importItems;
                    }
                    //==============
                    var listUOM = cxt.I_UnitOfMeasure.Where(x => x.IsActive && lstMerchantId.Contains(x.OrganizationId) && x.Status == (int)Commons.EStatus.Actived).ToList();
                    var listSupplier = cxt.I_Supplier.Where(x => x.IsActived && ListCompanyId.Contains(x.CompanyId) && x.Status == (int)Commons.EStatus.Actived).ToList();

                    IngredientImportResultItem itemErr = null;
                    bool flagInsert = true;
                    string msgError = "";

                    List<IngredientModel> Models = new List<IngredientModel>();
                    IngredientModel IngModel = null;
                    double purchasePrice = 0, salePrice = 0, ReceivingQty = 0, QtyTolerance = 0;

                    foreach (var CompanyId in ListCompanyId)
                    {
                        foreach (DataRow item in dtIngredient.Rows)
                        {
                            flagInsert = true;
                            msgError = "";

                            purchasePrice = 0;
                            salePrice = 0;

                            if (string.IsNullOrEmpty(item[1].ToString()))
                                continue;

                            IngModel = new IngredientModel();
                            string IngredientId = Guid.NewGuid().ToString();
                            IngModel.Id = IngredientId;
                            IngModel.Name = item[1].ToString();
                            IngModel.Code = item[2].ToString().Trim();
                            IngModel.Description = item[3].ToString();

                            double ReOrderQty = 0;
                            double.TryParse(item[4].ToString(), out ReOrderQty);
                            IngModel.ReOrderQty = ReOrderQty;

                            double MinAlertQty = 0;
                            double.TryParse(item[5].ToString(), out MinAlertQty);
                            IngModel.MinAlertQty = MinAlertQty;

                            IngModel.IsActive = string.IsNullOrEmpty(item[6].ToString()) ? true : item[6].ToString().Trim().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active")) ? true : false;

                            //BaseUOMId
                            IngModel.BaseUOMId = item[7].ToString();

                            double.TryParse(item[8].ToString(), out purchasePrice);
                            IngModel.PurchasePrice = Math.Round(purchasePrice, 2);
                            double.TryParse(item[9].ToString(), out salePrice);
                            IngModel.SalePrice = Math.Round(salePrice, 2);
                            IngModel.Status = (int)Commons.EStatus.Actived;
                            IngModel.CreatedBy = userName;
                            IngModel.CreatedDate = DateTime.Now;
                            IngModel.UpdatedBy = userName;
                            IngModel.UpdatedDate = DateTime.Now;

                            IngModel.IsPurchase = string.IsNullOrEmpty(item[10].ToString()) ? false : item[10].ToString().Trim().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TRUE").ToLower()) ? true : false;
                            IngModel.IsCheckStock = string.IsNullOrEmpty(item[11].ToString()) ? false : item[11].ToString().ToLower().Trim().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TRUE").ToLower()) ? true : false;
                            IngModel.IsSelfMode = string.IsNullOrEmpty(item[12].ToString()) ? true : item[12].ToString().Trim().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TRUE").ToLower()) ? true : false;

                            //ReceivingUOMId
                            IngModel.ReceivingUOMId = item[13].ToString();

                            double.TryParse(item[14].ToString(), out ReceivingQty);
                            IngModel.ReceivingQty = Math.Round(ReceivingQty, 2);
                            double.TryParse(item[15].ToString(), out QtyTolerance);
                            IngModel.QtyTolerance = Math.Round(QtyTolerance, 2);

                            IngModel.CompanyId = CompanyId;
                            //==============
                            int index = int.Parse(item[0].ToString());
                            IngModel.Index = index.ToString();

                            /*List Ingredient UOM*/
                            List<IngredientUOMModels> ListIngredientUOM = new List<IngredientUOMModels>();
                            DataRow[] IngredientUOMs = dtIngredientUOM.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Index") + "] = " + index + "");
                            foreach (DataRow itemUOM in IngredientUOMs)
                            {
                                var UOM = listUOM.Where(x => x.Name.ToLower().Equals(itemUOM[3].ToString().ToLower())).FirstOrDefault();
                                double ConversionRate = 0;
                                double.TryParse(itemUOM[4].ToString(), out ConversionRate);

                                if (string.IsNullOrEmpty(itemUOM[1].ToString()))
                                {
                                    flagInsert = false;
                                    msgError += "<br/>[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient UOM") + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Index is required") + "";
                                }
                                //====
                                if (string.IsNullOrEmpty(itemUOM[3].ToString()))
                                {
                                    flagInsert = false;
                                    msgError += "<br/>[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient UOM") + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Usage UOM is required") + "";
                                }
                                else if (UOM == null)
                                {
                                    flagInsert = false;
                                    msgError += "<br/>[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient UOM") + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Usage UOM does not exist") + "";
                                }

                                if (string.IsNullOrEmpty(itemUOM[4].ToString()))
                                {
                                    flagInsert = false;
                                    msgError += "<br/>[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient UOM") + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Conversion Rate is required") + "";
                                }
                                if (ConversionRate < 0)
                                {
                                    flagInsert = false;
                                    msgError += "<br/>[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient UOM") + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Conversion Rate rate larger or equal to 0") + "";
                                }

                                if (flagInsert)
                                {
                                    IngredientUOMModels IngredientUOM = new IngredientUOMModels()
                                    {
                                        ReceivingQty = ConversionRate,
                                        IsActived = string.IsNullOrEmpty(item[5].ToString()) ? false : bool.Parse(item[5].ToString().ToLower().Equals("true").ToString()) ? true : false,
                                        UOMId = UOM.Id
                                    };
                                    ListIngredientUOM.Add(IngredientUOM);
                                }
                            }
                            IngModel.ListIngUOM = ListIngredientUOM;

                            /*List Ingredient Supplier*/
                            List<Ingredients_SupplierModel> ListIngredientSupplier = new List<Ingredients_SupplierModel>();
                            DataRow[] IngredientSuppliers = dtIngredientSupplier.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Index") + "] = " + index + "");
                            foreach (DataRow itemSupplier in IngredientSuppliers)
                            {
                                if (string.IsNullOrEmpty(itemSupplier[1].ToString()))
                                {
                                    flagInsert = false;
                                    msgError += "<br/>[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Supplier") + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Index is required") + "";
                                }
                                //====
                                var Supplier = listSupplier.Where(x => x.Name.ToLower().Equals(itemSupplier[3].ToString().ToLower())).FirstOrDefault();
                                if (string.IsNullOrEmpty(itemSupplier[3].ToString()))
                                {
                                    flagInsert = false;
                                    msgError += "<br/>[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Supplier") + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier is required") + "";
                                }
                                else if (Supplier == null)
                                {
                                    flagInsert = false;
                                    msgError += "<br/>[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Supplier") + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier does not exist") + "";
                                }
                                if (flagInsert)
                                {
                                    Ingredients_SupplierModel IngredientSupplier = new Ingredients_SupplierModel()
                                    {
                                        SupplierName = itemSupplier[3].ToString(),
                                        SupplierAddress = itemSupplier[4].ToString(),
                                        SupplierPhone = itemSupplier[5].ToString(),
                                        SupplierId = Supplier.Id
                                    };
                                    ListIngredientSupplier.Add(IngredientSupplier);
                                }
                            }

                            IngModel.ListIngSupplier = ListIngredientSupplier;

                            //=======
                            if (string.IsNullOrEmpty(IngModel.Name))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Name is required");
                            }
                            if (string.IsNullOrEmpty(IngModel.Code))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code is required");
                            }
                            //=====BaseUOM
                            var BaseUOM = listUOM.Where(x => x.Name.ToLower().Equals(IngModel.BaseUOMId.ToLower())).FirstOrDefault();
                            if (string.IsNullOrEmpty(IngModel.BaseUOMId))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Base UOM is required");
                            }
                            else if (BaseUOM == null)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Base UOM does not exist");
                            }
                            else if (BaseUOM != null)
                            {
                                IngModel.BaseUOMId = BaseUOM.Id;
                            }
                            //=====ReceivingUOM
                            var ReceivingUOM = listUOM.Where(x => x.Name.ToLower().Equals(IngModel.ReceivingUOMId.ToLower())).FirstOrDefault();
                            if (string.IsNullOrEmpty(IngModel.ReceivingUOMId))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receiving UOM is required");
                            }
                            else if (ReceivingUOM == null)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receiving UOM does not exist");
                            }
                            else if (ReceivingUOM != null)
                            {
                                IngModel.ReceivingUOMId = ReceivingUOM.Id;
                            }
                            //===
                            if (IngModel.ReceivingQty < 0)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Conversion rate larger or equal to 0");
                            }
                            if (IngModel.QtyTolerance < 0 || IngModel.QtyTolerance > 100)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity Tolerance must between 0 and 100");
                            }
                            //=====
                            if (flagInsert)// Allow Pass
                            {
                                Models.Add(IngModel);
                            }
                            else
                            {
                                IngredientErrorItem itemerr = new IngredientErrorItem();
                                itemerr.GroupName = IngModel.Index;
                                itemerr.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + index + msgError;

                                itemErr = new IngredientImportResultItem();
                                itemErr.Name = IngModel.Name;
                                itemErr.ListFailCompanyName.Add("");
                                itemErr.ErrorItems.Add(itemerr);
                                importItems.Add(itemErr);
                            }
                        }
                    }

                    try
                    {

                        var lstCodes = Models.Select(ss => ss.Code.ToLower()).ToList();
                        var lstExists = cxt.I_Ingredient.Where(ww => lstCodes.Contains(ww.Code.ToLower())
                        && ListCompanyId.Contains(ww.CompanyId)
                        && ww.Status != (int)Commons.EStatus.Deleted).ToList();
                        lstCodes = new List<string>();
                        if (lstExists != null && lstExists.Count > 0)
                        {
                            //lstCodes = new List<string>();
                            //result.IsOk = false;
                            //result.Message = string.Format("Code [{0}] is exists!", lstExists[0].Code);
                            foreach (var item in lstExists)
                            {
                                msgError = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code") + " [{0}] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("is exist") + "!", item.Code);
                                IngredientErrorItem itemerr = new IngredientErrorItem();
                                itemerr.GroupName = "-";
                                itemerr.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":-<br/>" + msgError;

                                itemErr = new IngredientImportResultItem();
                                itemErr.Name = item.Name;
                                itemErr.ListFailCompanyName.Add("");
                                itemErr.ErrorItems.Add(itemerr);
                                importItems.Add(itemErr);

                                lstCodes.Add(item.Code);
                            }
                        }

                        //Models = Models.Where(x => !lstCodes.Contains(x.Code)).ToList();
                        if (importItems.Count() == 0)
                        {
                            //========
                            List<I_Ingredient> lstSave = new List<I_Ingredient>();
                            I_Ingredient item = null;
                            foreach (var model in Models)
                            {
                                item = new I_Ingredient();
                                item.Id = model.Id;
                                item.Code = model.Code.Trim();
                                item.Name = model.Name.Trim();
                                item.Description = model.Description == null ? model.Name : model.Description;
                                item.BaseUOMName = "";
                                item.IsActive = model.IsActive;
                                item.Status = (int)Commons.EStatus.Actived;
                                item.PurchasePrice = Math.Round(model.PurchasePrice, 2);
                                item.SalePrice = Math.Round(model.SalePrice, 2);

                                item.CreatedBy = model.CreatedBy;
                                item.CreatedDate = model.CreatedDate;
                                item.UpdatedBy = model.UpdatedBy;
                                item.UpdatedDate = model.UpdatedDate;

                                item.XeroId = model.XeroId;
                                item.CompanyId = model.CompanyId;
                                item.IsPurchase = model.IsPurchase;
                                item.IsCheckStock = model.IsCheckStock;
                                item.IsSelfMode = model.IsSelfMode;
                                item.BaseUOMId = model.BaseUOMId;
                                item.ReceivingUOMId = model.ReceivingUOMId;
                                item.ReceivingQty = model.ReceivingQty;
                                item.QtyTolerance = model.QtyTolerance;

                                item.ReOrderQty = model.ReOrderQty;
                                item.MinAlertQty = model.MinAlertQty;

                                lstSave.Add(item);
                            }
                            cxt.I_Ingredient.AddRange(lstSave);
                            /*======>*/
                            foreach (var model in Models)
                            {
                                /*Detail*/
                                //Insert Ingredient Supplier
                                List<I_Ingredient_Supplier> listIngSupplier = new List<I_Ingredient_Supplier>();
                                foreach (var supllier in model.ListIngSupplier)
                                {
                                    listIngSupplier.Add(new I_Ingredient_Supplier
                                    {
                                        Id = Guid.NewGuid().ToString(),

                                        IngredientId = model.Id,
                                        SupplierId = supllier.SupplierId,

                                        CreatedBy = model.CreatedBy,
                                        CreatedDate = model.CreatedDate,
                                        ModifierBy = model.UpdatedBy,
                                        ModifierDate = model.UpdatedDate,
                                        IsActived = model.IsActive
                                    });
                                }
                                if (listIngSupplier.Count > 0)
                                {
                                    cxt.I_Ingredient_Supplier.AddRange(listIngSupplier);
                                }

                                //Insert Ingredient Usage UOM
                                List<I_Ingredient_UOM> listIngUOM = new List<I_Ingredient_UOM>();
                                foreach (var UsageUOMModel in model.ListIngUOM)
                                {
                                    listIngUOM.Add(new I_Ingredient_UOM
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IngredientId = model.Id,
                                        UOMId = UsageUOMModel.UOMId,
                                        BaseUOM = 0,
                                        ReceivingQty = UsageUOMModel.ReceivingQty,
                                        CreatedBy = model.CreatedBy,
                                        CreatedDate = model.CreatedDate,
                                        UpdatedBy = model.UpdatedBy,
                                        UpdatedDate = model.UpdatedDate,
                                        IsActived = model.IsActive
                                    });
                                }
                                if (listIngUOM.Count > 0)
                                {
                                    cxt.I_Ingredient_UOM.AddRange(listIngUOM);
                                }
                                /*End Detail*/
                            }

                            cxt.SaveChanges();
                            /* NS Xero INGREDIENT  */
                            //List<IngredientModel> listItem = new List<IngredientModel>();
                            //listItem.Add(model);
                            NSXeroIngredientResponseModels dataResponse = new NSXeroIngredientResponseModels();
                            XeroIngredientInsertOrUpdate(Models, ListStoreId, "import", ref msg, ref dataResponse);
                            //==================
                            transaction.Commit();
                            //result.IsOk = true;
                            //if (importItems.Count == 0)
                            //{
                            IngredientImportResultItem importItem = new IngredientImportResultItem();
                            importItem.Name = "Import Ingredient Successful";
                            importItem.ListSuccessCompanyName.Add("Import Ingredient Successful");
                            importItems.Add(importItem);
                            //}
                            NSLog.Logger.Info("Import Ingredient Successful", lstSave);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        //_logger.Error(ex);
                        NSLog.Logger.Error("Import Ingredient error", ex);
                        //result.IsOk = false;
                        //result.Message = ex.Message;
                    }
                }
            }
            return importItems;
        }

        public ResultModels Export(ref IXLWorksheet wsIngredient, ref IXLWorksheet wsIngredientUOM, ref IXLWorksheet wsIngredientSupplier,
                    List<string> ListCompanyId, List<SelectListItem> lstCompany)
        {
            var result = new ResultModels();
            try
            {
                using (var cxt = new NuWebContext())
                {
                    //string[] lstHeaders = new string[] {
                    //"Index", "Ingredient Name", "Code","Description", "ReOrdering Qty","Min Alert Qty",
                    //    "Status","Base UOM","Purchase Price","Sale Price",
                    //    "Purchase", "Check Stock", "Self Mode", "Receiving UOM",
                    //    "Conversion rate", "Quantity Tolerance (%)", "Company" };

                    string[] lstHeaders = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Name"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Code"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Description"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("ReOrdering Qty"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Min Alert Qty"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Base UOM"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Purchase Price"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sale Price"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Purchase"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Check Stock"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Self Mode"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receiving UOM"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Conversion rate"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity Tolerance (%)"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Company")
                    };
                    var lstData = cxt.I_Ingredient
                                    .Where(ww => ww.Status != (int)Commons.EStatus.Deleted
                                            && ListCompanyId.Contains(ww.CompanyId)
                                            && ww.BaseUOMId != "" && ww.ReceivingUOMId != "")
                                    .Select(ss => new IngredientModel()
                                    {
                                        Id = ss.Id,
                                        Code = ss.Code,
                                        Name = ss.Name,
                                        Description = ss.Description,
                                        BaseUOMName = ss.BaseUOMName,
                                        IsActive = ss.IsActive,
                                        PurchasePrice = ss.PurchasePrice,
                                        SalePrice = ss.SalePrice,

                                        IsPurchase = ss.IsPurchase,
                                        IsCheckStock = ss.IsCheckStock,
                                        IsSelfMode = ss.IsSelfMode,

                                        BaseUOMId = ss.BaseUOMId,
                                        ReceivingUOMId = ss.ReceivingUOMId,
                                        ReceivingQty = ss.ReceivingQty,

                                        QtyTolerance = ss.QtyTolerance,

                                        CompanyId = ss.CompanyId,

                                        ReOrderQty = ss.ReOrderQty,
                                        MinAlertQty = ss.MinAlertQty
                                    }).ToList();

                    int row = 1;
                    //add header to excel file
                    for (int i = 1; i <= lstHeaders.Length; i++)
                        wsIngredient.Cell(row, i).Value = lstHeaders[i - 1];
                    int cols = lstHeaders.Length;
                    row = 2;

                    int countIndex = 1;
                    int countIndexIngredientUOM = 1;
                    int countIndexIngredientSupplier = 1;

                    List<ExportIngUOM> lstIngredientUOM = new List<ExportIngUOM>();
                    List<ExportIngSupplier> lstIngredientSupplier = new List<ExportIngSupplier>();

                    var listUOM = cxt.I_UnitOfMeasure.ToList();
                    lstData = lstData.OrderBy(x => x.CompanyName).OrderBy(x => x.Name).ToList();
                    if (lstData != null && lstData.Count > 0)
                    {
                        foreach (var item in lstData)
                        {
                            var Company = lstCompany.Where(x => x.Value.Equals(item.CompanyId)).FirstOrDefault();
                            var BaseUOM = listUOM.Where(x => x.Id.Equals(item.BaseUOMId)).FirstOrDefault();
                            var ReceivingUOM = listUOM.Where(x => x.Id.Equals(item.ReceivingUOMId)).FirstOrDefault();

                            wsIngredient.Cell("A" + row).Value = countIndex;
                            wsIngredient.Cell("B" + row).Value = item.Name;
                            wsIngredient.Cell("C" + row).Value = item.Code;
                            wsIngredient.Cell("D" + row).Value = item.Description == null ? item.Name : item.Description;

                            wsIngredient.Cell("E" + row).Value = item.ReOrderQty;
                            wsIngredient.Cell("F" + row).Value = item.MinAlertQty;

                            wsIngredient.Cell("G" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active")
                                : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");
                            wsIngredient.Cell("H" + row).Value = BaseUOM == null ? "" : BaseUOM.Name;
                            wsIngredient.Cell("I" + row).Value = item.PurchasePrice;
                            wsIngredient.Cell("J" + row).Value = item.SalePrice;

                            wsIngredient.Cell("K" + row).Value = item.IsPurchase ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TRUE")
                                : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FALSE");

                            wsIngredient.Cell("L" + row).Value = item.IsCheckStock ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TRUE")
                                : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FALSE");

                            wsIngredient.Cell("M" + row).Value = item.IsSelfMode ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TRUE")
                                : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FALSE");

                            wsIngredient.Cell("N" + row).Value = ReceivingUOM == null ? "" : ReceivingUOM.Name;
                            wsIngredient.Cell("O" + row).Value = item.ReceivingQty;
                            wsIngredient.Cell("P" + row).Value = item.QtyTolerance;
                            wsIngredient.Cell("Q" + row).Value = Company == null ? "" : Company.Text;

                            /*Get List Ingredient Usage UOM*/
                            var listIngUOM = (from IU in cxt.I_Ingredient_UOM
                                              where IU.IngredientId.Equals(item.Id)
                                              select new IngredientUOMModels
                                              {
                                                  UOMId = IU.UOMId,
                                                  ReceivingQty = IU.ReceivingQty,
                                                  IsActived = IU.IsActived
                                              }).ToList();
                            foreach (var IngUOM in listIngUOM)
                            {
                                var IngUOM_ReceivingUOM = listUOM.Where(x => x.Id.Equals(IngUOM.UOMId)).FirstOrDefault();
                                ExportIngUOM etIngUOM = new ExportIngUOM()
                                {
                                    Index = countIndexIngredientUOM,
                                    IngredientIndex = countIndex,
                                    IngredientName = item.Name,
                                    UsageUOM = IngUOM_ReceivingUOM == null ? "" : IngUOM_ReceivingUOM.Name,
                                    Quantity = IngUOM.ReceivingQty,
                                    Active = IngUOM.IsActived.ToString()
                                };
                                lstIngredientUOM.Add(etIngUOM);
                                countIndexIngredientUOM++;
                            }

                            /*Get List Ingredient Supplier*/
                            var listIngSupplier = (from IS in cxt.I_Ingredient_Supplier
                                                   from S in cxt.I_Supplier
                                                   where IS.IngredientId.Equals(item.Id) && IS.SupplierId.Equals(S.Id)
                                                            && IS.IsActived && S.Status != (int)Commons.EStatus.Deleted
                                                   select new ExportIngSupplier
                                                   {
                                                       Address = S.Address,
                                                       Name = S.Name,
                                                       Phone = S.Phone1 + " - " + S.Phone2
                                                   }).ToList();

                            foreach (var IngSupplier in listIngSupplier)
                            {
                                ExportIngSupplier etIngSupplier = new ExportIngSupplier()
                                {
                                    Index = countIndexIngredientSupplier,
                                    IngredientIndex = countIndex,
                                    IngredientName = item.Name,
                                    Name = IngSupplier.Name,
                                    Address = IngSupplier.Address,
                                    Phone = IngSupplier.Phone
                                };
                                lstIngredientSupplier.Add(etIngSupplier);
                                countIndexIngredientSupplier++;
                            }
                            //=====
                            row++;
                            countIndex++;
                        }
                    }
                    BaseFactory.FormatExcelExport(wsIngredient, row, cols);
                    /*Format For Excel*/
                    wsIngredient.Range("I" + 1 + ":J" + (row - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    wsIngredient.Range("I" + 1 + ":J" + (row - 1)).Style.NumberFormat.Format = "#,##0.00";
                    wsIngredient.Range("O" + 1 + ":P" + (row - 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    wsIngredient.Range("O" + 1 + ":P" + (row - 1)).Style.NumberFormat.Format = "#,##0.00";
                    //=========
                    row = 1;
                    string[] listIngUOMHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index") /*"Index"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Index") /*"Ingredient Index"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Name") /*"Ingredient Name"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Usage UOM") /*"Usage UOM"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Conversion rate") /*"Conversion rate"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") /*"Active"*/
                    };

                    for (int i = 1; i <= listIngUOMHeader.Length; i++)
                        wsIngredientUOM.Cell(row, i).Value = listIngUOMHeader[i - 1];
                    cols = listIngUOMHeader.Length;
                    row++;
                    foreach (var item in lstIngredientUOM)
                    {
                        //=========
                        wsIngredientUOM.Cell("A" + row).Value = item.Index;
                        wsIngredientUOM.Cell("B" + row).Value = item.IngredientIndex;
                        wsIngredientUOM.Cell("C" + row).Value = item.IngredientName;
                        wsIngredientUOM.Cell("D" + row).Value = item.UsageUOM;
                        wsIngredientUOM.Cell("E" + row).Value = item.Quantity;
                        wsIngredientUOM.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item.Active);
                        row++;
                    }
                    BaseFactory.FormatExcelExport(wsIngredientUOM, row, cols);
                    ////============
                    row = 1;
                    string[] listIngSupplierHeader = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index") /*"Index"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Index") /*"Ingredient Index"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Name") /*"Ingredient Name"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier Name") /*"Supplier Name"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Address") /*"Address"*/,
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Phone") /*"Phone"*/
                    };

                    for (int i = 1; i <= listIngSupplierHeader.Length; i++)
                        wsIngredientSupplier.Cell(row, i).Value = listIngSupplierHeader[i - 1];
                    cols = listIngSupplierHeader.Length;
                    row++;
                    foreach (var item in lstIngredientSupplier)
                    {
                        //=========
                        wsIngredientSupplier.Cell("A" + row).Value = item.Index;
                        wsIngredientSupplier.Cell("B" + row).Value = item.IngredientIndex;
                        wsIngredientSupplier.Cell("C" + row).Value = item.IngredientName;
                        wsIngredientSupplier.Cell("D" + row).Value = item.Name;
                        wsIngredientSupplier.Cell("E" + row).Value = item.Address;
                        wsIngredientSupplier.Cell("F" + row).Value = item.Phone;
                        row++;
                    }
                    BaseFactory.FormatExcelExport(wsIngredientSupplier, row, cols);
                    //============
                    result.IsOk = true;
                }
            }
            catch (Exception ex)
            {
                result.IsOk = false;
                result.Message = ex.Message;
                _logger.Error(ex);
            }

            return result;
        }

        private bool IsCanDelete(string id)
        {
            using (var cxt = new NuWebContext())
            {
                var isExists = cxt.I_Recipe_Item.Any(aa => aa.IngredientId == id && aa.Status != (int)Commons.EStatus.Deleted);
                if (!isExists)
                {
                    isExists = cxt.I_Recipe_Modifier.Any(aa => aa.IngredientId == id && aa.Status != (int)Commons.EStatus.Deleted);
                    if (!isExists)
                    {
                        isExists = cxt.I_Recipe_Ingredient.Any(aa => (aa.IngredientId == id || aa.MixtureIngredientId == id) && aa.Status != (int)Commons.EStatus.Deleted);
                        if (!isExists)
                        {
                            isExists = cxt.I_Ingredient_UOM.Any(aa => (aa.IngredientId == id) && aa.IsActived == true);
                            if (!isExists)
                            {
                                isExists = cxt.I_Purchase_Order_Detail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
                                if (!isExists)
                                {
                                    isExists = cxt.I_StoreSetting.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
                                    if (!isExists)
                                    {
                                        isExists = cxt.I_AllocationDetail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
                                        if (!isExists)
                                        {
                                            isExists = cxt.I_DataEntryDetail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
                                            if (!isExists)
                                            {
                                                isExists = cxt.I_Ingredient_Supplier.Any(aa => (aa.IngredientId == id) && aa.IsActived == true);
                                                if (!isExists)
                                                {
                                                    isExists = cxt.I_InventoryManagement.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
                                                    if (!isExists)
                                                    {
                                                        isExists = cxt.I_Stock_Transfer_Detail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
                                                        if (!isExists)
                                                        {
                                                            isExists = cxt.I_UsageManagementDetail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return !isExists;
            }
        }

        //private bool OldIsCanDelete(string id)
        //{
        //    using (var cxt = new NuWebContext())
        //    {
        //        var isExists = cxt.I_Recipe_Item.Any(aa => aa.IngredientId == id && aa.Status != (int)Commons.EStatus.Deleted);
        //        if (!isExists)
        //        {
        //            isExists = cxt.I_Recipe_Modifier.Any(aa => aa.IngredientId == id && aa.Status != (int)Commons.EStatus.Deleted);
        //            if (!isExists)
        //            {
        //                isExists = cxt.I_Recipe_Ingredient.Any(aa => (aa.IngredientId == id || aa.MixtureIngredientId == id) && aa.Status != (int)Commons.EStatus.Deleted);
        //                if (!isExists)
        //                {
        //                    isExists = cxt.I_Ingredient_UOM.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                    if (!isExists)
        //                    {
        //                        isExists = cxt.I_Purchase_Order_Detail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                        if (!isExists)
        //                        {
        //                            isExists = cxt.I_StoreSetting.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                            if (!isExists)
        //                            {
        //                                isExists = cxt.I_AllocationDetail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                                if (!isExists)
        //                                {
        //                                    isExists = cxt.I_DataEntryDetail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                                    if (!isExists)
        //                                    {
        //                                        isExists = cxt.I_Ingredient_Supplier.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                                        if (!isExists)
        //                                        {
        //                                            isExists = cxt.I_InventoryManagement.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                                            if (!isExists)
        //                                            {
        //                                                isExists = cxt.I_Stock_Transfer_Detail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                                                if (!isExists)
        //                                                {
        //                                                    isExists = cxt.I_UsageManagementDetail.Any(aa => (aa.IngredientId == id) /*&& aa.Status != (int)Commons.EStatus.Deleted*/);
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        return !isExists;
        //    }
        //}

        private List<string> ListIngInActive(List<string> lstId)
        {
            List<string> lstIdInActive = lstId;
            using (var cxt = new NuWebContext())
            {
                var listIng = cxt.I_Recipe_Item.Where(aa => lstIdInActive.Contains(aa.IngredientId) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.IngredientId).ToList();
                listIng.AddRange(cxt.I_Recipe_Modifier.Where(aa => lstIdInActive.Contains(aa.IngredientId) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_Recipe_Ingredient.Where(aa => (lstIdInActive.Contains(aa.IngredientId)) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_Recipe_Ingredient.Where(aa => lstIdInActive.Contains(aa.MixtureIngredientId) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.MixtureIngredientId).ToList());
                listIng.AddRange(cxt.I_Ingredient_UOM.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_Purchase_Order_Detail.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_StoreSetting.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_AllocationDetail.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_DataEntryDetail.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_Ingredient_Supplier.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_InventoryManagement.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_Stock_Transfer_Detail.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());
                listIng.AddRange(cxt.I_UsageManagementDetail.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.IngredientId).ToList());

                //Remove
                lstIdInActive.RemoveAll(i => listIng.Contains(i));
                return lstIdInActive;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IngredientId"></param>
        /// <param name="UOMid"></param>
        /// <param name="Type">0: BaseUOM | !0: Receiving-IngredientUOM</param>
        /// <returns></returns>
        /*Get Usage UOM For Ingredient*/
        public double GetUsageUOMForIngredient(string IngredientId, string UOMid, ref int Type)
        {
            double baseUsage = 1;
            using (var cxt = new NuWebContext())
            {
                //Base
                var Ing = cxt.I_Ingredient.Where(x => x.BaseUOMId.Equals(UOMid) && x.Id.Equals(IngredientId)
                                                && x.Status != (int)Commons.EStatus.Deleted).FirstOrDefault();
                if (Ing != null)
                {
                    Type = 0;
                }
                else
                {
                    Type = 1;
                    //Receiving
                    Ing = cxt.I_Ingredient.Where(x => x.ReceivingUOMId.Equals(UOMid) && x.Id.Equals(IngredientId)
                                                && x.Status != (int)Commons.EStatus.Deleted).FirstOrDefault();
                    if (Ing != null)
                    {
                        baseUsage = Ing.ReceivingQty == 0 ? baseUsage : Ing.ReceivingQty;
                    }
                    else
                    {
                        //Ingredient Usage
                        var IngUOM = cxt.I_Ingredient_UOM.Where(x => x.UOMId.Equals(UOMid) && x.IngredientId.Equals(IngredientId) && x.IsActived).FirstOrDefault();
                        if (IngUOM != null)
                        {
                            baseUsage = IngUOM.ReceivingQty == 0 ? baseUsage : IngUOM.ReceivingQty;
                        }
                    }
                }
            }
            return baseUsage;
        }

        public List<IngredientModel> GetIngredientSelfMade(List<string> lstCompany)
        {
            List<IngredientModel> lstResults = new List<IngredientModel>();
            using (var cxt = new NuWebContext())
            {
                var listUOM = cxt.I_UnitOfMeasure.ToList();
                var query = (from i in cxt.I_Ingredient
                             from uom in cxt.I_UnitOfMeasure
                             where i.Status != (int)Commons.EStatus.Deleted
                                    && i.BaseUOMId.Equals(uom.Id)
                                    && uom.Status != (int)Commons.EStatus.Deleted
                                    && i.IsSelfMode
                                    && i.StockAble.HasValue && i.StockAble.Value
                             select new { i, uom });
                if (lstCompany != null && lstCompany.Any())
                {
                    query = query.Where(ww => lstCompany.Contains(ww.i.CompanyId));
                }
                if (query != null && query.Any())
                {
                    lstResults = query.Select(item => new IngredientModel()
                    {
                        Id = item.i.Id,
                        Code = item.i.Code,
                        Name = item.i.Name,
                        Description = item.i.Description,
                        BaseUOMName = item.uom.Name,
                        ReceivingQty = item.i.ReceivingQty,
                        ReceivingUOMId = item.i.ReceivingUOMId,

                        IsSelfMode = item.i.IsSelfMode,
                        IsStockable = item.i.StockAble ?? false
                    }).ToList();
                }
                lstResults.ForEach(x =>
                {
                    var ReceivingUOM = listUOM.Where(z => z.Id.Equals(x.ReceivingUOMId)).FirstOrDefault();
                    x.ReceivingUOMName = ReceivingUOM.Name;
                });

                return lstResults;
            }
        }

    }

    public class ExportIngUOM
    {
        public int Index { get; set; }
        public int IngredientIndex { get; set; }
        public string IngredientName { get; set; }
        public string UsageUOM { get; set; }
        public double Quantity { get; set; }
        public string Active { get; set; }
    }

    public class ExportIngSupplier
    {
        public int Index { get; set; }
        public int IngredientIndex { get; set; }
        public string IngredientName { get; set; }
        public string IngredientCode { get; set; }

        public int SupplierIndex { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}
