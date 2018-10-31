using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Models.Xero;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class StockUsageFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private XeroFactory _xeroFactory = null;
        private UsageManagementFactory _usageManagementFactory = null;
        public StockUsageFactory()
        {
            _baseFactory = new BaseFactory();
            _xeroFactory = new XeroFactory();
            _usageManagementFactory = new UsageManagementFactory();
        }
        public bool Insert(StockUsageRequestModel info)
        {
            bool result = true;
            try
            {
                using (NuWebContext cxt = new NuWebContext())
                {
                    //Check Exist
                    var usage = info.ListDetails.FirstOrDefault();
                    var obj = cxt.I_StockUsage.Where(ww => ww.StoreId == info.StoreId
                                    && ww.CreatedDate == usage.CreatedDate && ww.ItemId == usage.ItemId).FirstOrDefault();
                    if (obj != null)
                    {
                        NSLog.Logger.Info("Insert [Stock Usage] data exist");
                        return result;
                    }
                    using (var transaction = cxt.Database.BeginTransaction())
                    {
                        try
                        {
                            List<I_StockUsage> lstInsert = new List<I_StockUsage>();
                            I_StockUsage itemInsert = null;
                            foreach (var item in info.ListDetails)
                            {
                                itemInsert = new I_StockUsage();
                                itemInsert.Id = Guid.NewGuid().ToString();
                                itemInsert.StoreId = info.StoreId;
                                itemInsert.ItemId = item.ItemId;
                                itemInsert.ItemName = item.ItemName;
                                itemInsert.Quantity = item.Quantity;
                                itemInsert.CreatedDate = item.CreatedDate;
                                itemInsert.Mode = item.Mode;

                                lstInsert.Add(itemInsert);
                            }
                            cxt.I_StockUsage.AddRange(lstInsert);
                            cxt.SaveChanges();
                            transaction.Commit();
                            //Insert Usage managements
                            CalUsageManagementForInventory(info.CompanyId, info.ListDetails, info.BusinessId, info.StoreId, info.DateFrom, info.DateTo);

                            NSLog.Logger.Info("Insert [Stock Usage] data success", info);
                        }
                        catch (Exception ex)
                        {
                            NSLog.Logger.Error("Insert [Stock Usage] data fail", ex);
                            //_logger.Error(ex);
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
                //var jsonContent = JsonConvert.SerializeObject(info.ListDetails);
                //_baseFactory.InsertTrackingLog("I_StockUsage", jsonContent, info.StoreId.ToString(), result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return result;
        }

        private List<StockUsageModel> GetDataSale(UsageManagementRequest request)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = (from s in cxt.I_StockUsage
                               where s.StoreId == request.StoreId && s.CreatedDate >= request.DateFrom && s.CreatedDate <= request.DateTo
                               //group s by new{ s.ItemId, s.ItemName} into st
                               select new StockUsageModel
                               {
                                   ItemId = s.ItemId,
                                   ItemName = s.ItemName,
                                   Quantity = s.Quantity,
                                   CreatedDate = s.CreatedDate

                               }).ToList();

                return lstData;
            }
        }
        //For Old not use
        public List<UsageManagementModel> CalUsageManagementwithoutDetail(UsageManagementRequest request)
        {
            var result = new List<UsageManagementModel>();
            request.DateFrom = new DateTime(request.DateFrom.Year, request.DateFrom.Month, request.DateFrom.Day, 0, 0, 0);
            request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);
            //List Sale 
            var lstSale = GetDataSale(request);
            if (lstSale != null && lstSale.Count > 0)
            {
                var lstItemIdSale = lstSale.Select(ss => ss.ItemId).Distinct().ToList();

                var lstRecipeItems = GetRecipeItem(request.StoreId);
                var lstrecipeModifier = GetRecipeModifier(request.StoreId);
                lstRecipeItems.AddRange(lstrecipeModifier);

                List<ItemSaleForIngredientModel> _lstDetailTmp = new List<ItemSaleForIngredientModel>();

                if (lstRecipeItems != null && lstRecipeItems.Count > 0)
                {
                    lstRecipeItems = lstRecipeItems.Where(ww => lstItemIdSale.Contains(ww.ItemId)).ToList();

                    foreach (var item in lstRecipeItems)
                    {
                        var lstSaleItem = lstSale.Where(ww => ww.ItemId == item.ItemId).ToList();

                        foreach (var subItem in lstSaleItem)
                        {
                            item.Usage += item.BaseUsage * subItem.Quantity;
                        }
                    }
                    var lstUsageGroupIngredient = lstRecipeItems.GroupBy(gg => gg.IngredientId);

                    UsageManagementModel usageManagementModel = null;
                    int indexPage = 1;
                    foreach (var item in lstUsageGroupIngredient)
                    {
                        usageManagementModel = new UsageManagementModel();
                        usageManagementModel.Index = indexPage;
                        usageManagementModel.Id = item.Key;
                        usageManagementModel.Code = item.Select(ss => ss.IngredientCode).FirstOrDefault();
                        usageManagementModel.Name = item.Select(ss => ss.IngredientName).FirstOrDefault();
                        usageManagementModel.UOMName = item.Select(ss => ss.UOMName).FirstOrDefault();
                        usageManagementModel.Usage = item.Sum(ss => ss.Usage);

                        result.Add(usageManagementModel);
                        indexPage++;
                    }

                }
            }
            result = result.Where(ww => ww.Usage > 0).ToList();
            return result;
        }

        public async void CalUsageManagementForInventory(string companyId, List<StockUsageModel> lstSale, string businessDayId, string storeId, DateTime dFrom, DateTime dTo)
        {
            var result = new List<UsageManagementModel>();

            if (lstSale != null && lstSale.Count > 0)
            {
                var lstItemIdSale = lstSale.Select(ss => ss.ItemId).Distinct().ToList();

                var lstRecipeItems = GetRecipeItem(storeId);
                var lstrecipeModifier = GetRecipeModifier(storeId);
                lstRecipeItems.AddRange(lstrecipeModifier);

                List<ItemSaleForIngredientModel> _lstDetailTmp = new List<ItemSaleForIngredientModel>();

                if (lstRecipeItems != null && lstRecipeItems.Count > 0)
                {
                    lstRecipeItems = lstRecipeItems.Where(ww => lstItemIdSale.Contains(ww.ItemId)).ToList();

                    foreach (var item in lstRecipeItems)
                    {
                        var lstSaleItem = lstSale.Where(ww => ww.ItemId == item.ItemId).ToList();

                        foreach (var subItem in lstSaleItem)
                        {
                            item.Usage += item.BaseUsage * subItem.Quantity;

                        }
                    }
                    var lstUsageGroupIngredient = lstRecipeItems.GroupBy(gg => gg.IngredientId);

                    UsageManagementModel usageManagementModel = null;
                    BusinessDayDisplayModels busDay = new BusinessDayDisplayModels();
                    busDay.StoreId = storeId;
                    busDay.DateDisplay = dFrom.ToString("dd/MM/yyyy HH:mm") + " - " + dTo.ToString("dd/MM/yyyy HH:mm");
                    busDay.DateFrom = dFrom;
                    busDay.DateTo = dTo;

                    foreach (var item in lstUsageGroupIngredient)
                    {
                        usageManagementModel = new UsageManagementModel();
                        usageManagementModel.Id = item.Key;
                        usageManagementModel.Code = item.Select(ss => ss.IngredientCode).FirstOrDefault();
                        //usageManagementModel.Name = item.Select(ss => ss.IngredientName).FirstOrDefault();
                        //usageManagementModel.UOMName = item.Select(ss => ss.UOMName).FirstOrDefault();
                        usageManagementModel.Usage = item.Sum(ss => ss.Usage);
                        usageManagementModel.ListDetail = CalUsageManagementByIngredientForInventory(lstSale, lstRecipeItems, busDay, usageManagementModel.Id);

                        result.Add(usageManagementModel);
                    }

                }
            }
            result = result.Where(ww => ww.Usage > 0).ToList();
            _usageManagementFactory.SaveUsageManagement(companyId, storeId, businessDayId, dFrom, dTo, result);
            //return result;
        }
        public List<UsageManagementDetailModel> CalUsageManagementByIngredientForInventory(List<StockUsageModel> lstSale,
            List<RecipeItemModel> lstRecipeItems, BusinessDayDisplayModels busDay, string ingredientId)
        {
            List<UsageManagementDetailModel> result = new List<UsageManagementDetailModel>();
            var lstItemIdSale = lstSale.Select(ss => ss.ItemId).Distinct().ToList();
            List<ItemSaleForIngredientModel> _lstDetailTmp = new List<ItemSaleForIngredientModel>();

            lstRecipeItems = lstRecipeItems.Where(ww => ww.IngredientId == ingredientId).ToList();
            if (lstRecipeItems != null && lstRecipeItems.Count > 0)
            {
                lstRecipeItems = lstRecipeItems.Where(ww => lstItemIdSale.Contains(ww.ItemId)).ToList();
                var lstItemIdInrecipe = lstRecipeItems.Select(ss => ss.ItemId).Distinct().ToList();
                //Group item sale
                var lstItemSaleHaveRecipe = lstSale.Where(ww => lstItemIdInrecipe.Contains(ww.ItemId)).ToList();
                if (lstItemSaleHaveRecipe != null && lstItemSaleHaveRecipe.Count > 0)
                {
                    UsageManagementDetailModel detail = null;

                    var lstDish = lstItemSaleHaveRecipe.Where(ww => ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).ToList();
                    if (lstDish != null && lstDish.Count > 0)
                    {
                        var lstDishGroup = lstDish.GroupBy(gg => gg.ItemId);
                        foreach (var dish in lstDishGroup)
                        {
                            var recipe = lstRecipeItems.Where(ww => ww.ItemId == dish.Key).FirstOrDefault();
                            if (recipe != null)
                            {
                                detail = new UsageManagementDetailModel();
                                detail.BusinessDay = busDay.DateDisplay;
                                detail.ItemId = dish.Key;
                                detail.ItemName = dish.Select(ss => ss.ItemName).FirstOrDefault();
                                detail.Qty = dish.Sum(ss => ss.Quantity);
                                detail.Usage = detail.Qty * recipe.BaseUsage;

                                result.Add(detail);
                            }
                        }

                    }

                }

            }
            return result;
        }

        public List<UsageManagementModel> CalUsageManagement(UsageManagementRequest request)
        {
            var result = new List<UsageManagementModel>();

            //List Sale 
            var lstSale = GetDataSale(request);
            if (lstSale != null && lstSale.Count > 0)
            {
                //get all business day
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.StoreId, request.Mode);
                var lstItemIdSale = lstSale.Select(ss => ss.ItemId).Distinct().ToList();

                var lstRecipeItems = GetRecipeItem(request.StoreId);
                var lstrecipeModifier = GetRecipeModifier(request.StoreId);
                lstRecipeItems.AddRange(lstrecipeModifier);

                List<ItemSaleForIngredientModel> _lstDetailTmp = new List<ItemSaleForIngredientModel>();

                if (lstRecipeItems != null && lstRecipeItems.Count > 0)
                {
                    lstRecipeItems = lstRecipeItems.Where(ww => lstItemIdSale.Contains(ww.ItemId)).ToList();

                    foreach (var item in lstRecipeItems)
                    {
                        var lstSaleItem = lstSale.Where(ww => ww.ItemId == item.ItemId).ToList();
                        foreach (var subItem in lstSaleItem)
                        {
                            item.Usage += item.BaseUsage * subItem.Quantity;

                            _lstDetailTmp.Add(new ItemSaleForIngredientModel()
                            {
                                IngredientId = item.IngredientId,
                                ItemId = subItem.ItemId,
                                ItemName = subItem.ItemName,
                                BaseUsage = item.BaseUsage,
                                CreatedDate = subItem.CreatedDate,
                                Qty = subItem.Quantity
                            });
                        }
                    }
                    var lstUsageGroupIngredient = lstRecipeItems.GroupBy(gg => gg.IngredientId);

                    UsageManagementModel usageManagementModel = null;
                    UsageManagementDetailModel detail = null;
                    int indexPage = 1, indexChildPage = 1;
                    foreach (var item in lstUsageGroupIngredient)
                    {
                        usageManagementModel = new UsageManagementModel();
                        usageManagementModel.Index = indexPage;
                        usageManagementModel.Id = item.Key;
                        usageManagementModel.Code = item.Select(ss => ss.IngredientCode).FirstOrDefault();
                        usageManagementModel.Name = item.Select(ss => ss.IngredientName).FirstOrDefault();
                        usageManagementModel.UOMName = item.Select(ss => ss.UOMName).FirstOrDefault();
                        usageManagementModel.Usage = item.Sum(ss => ss.Usage);

                        indexChildPage = 1;
                        foreach (var busDay in _lstBusDayAllStore)
                        {
                            var lstDish = _lstDetailTmp.Where(ww => ww.IngredientId == usageManagementModel.Id
                             && ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).ToList();
                            if (lstDish != null && lstDish.Count > 0)
                            {
                                var lstDishGroup = lstDish.GroupBy(gg => gg.ItemId);
                                foreach (var dish in lstDishGroup)
                                {
                                    detail = new UsageManagementDetailModel();
                                    detail.BusinessDay = busDay.DateDisplay;
                                    detail.Index = indexChildPage;
                                    detail.ItemName = dish.Select(ss => ss.ItemName).FirstOrDefault();
                                    detail.Qty = dish.Sum(ss => ss.Qty);
                                    detail.Usage = detail.Qty * dish.Select(ss => ss.BaseUsage).FirstOrDefault();

                                    usageManagementModel.ListDetail.Add(detail);
                                    indexChildPage++;
                                }

                            }
                        }

                        result.Add(usageManagementModel);
                        indexPage++;
                    }

                }
            }



            return result;
        }

        //For Old
        public UsageManagementModel CalUsageManagementByIngredient(UsageManagementRequest request, string ingredientId)
        {
            request.DateFrom = new DateTime(request.DateFrom.Year, request.DateFrom.Month, request.DateFrom.Day, 0, 0, 0);
            request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);
            UsageManagementModel usageManagementModel = new UsageManagementModel();
            //List Sale 
            var lstSale = GetDataSale(request);
            if (lstSale != null && lstSale.Count > 0)
            {
                //get all business day
                var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.StoreId, request.Mode);
                var lstItemIdSale = lstSale.Select(ss => ss.ItemId).Distinct().ToList();

                var lstRecipeItems = GetRecipeItem(request.StoreId, ingredientId);
                var lstrecipeModifier = GetRecipeModifier(request.StoreId, ingredientId);
                lstRecipeItems.AddRange(lstrecipeModifier);

                List<ItemSaleForIngredientModel> _lstDetailTmp = new List<ItemSaleForIngredientModel>();

                if (lstRecipeItems != null && lstRecipeItems.Count > 0)
                {
                    lstRecipeItems = lstRecipeItems.Where(ww => lstItemIdSale.Contains(ww.ItemId)).ToList();
                    var lstItemIdInrecipe = lstRecipeItems.Select(ss => ss.ItemId).Distinct().ToList();
                    //Group item sale
                    var lstItemSaleHaveRecipe = lstSale.Where(ww => lstItemIdInrecipe.Contains(ww.ItemId)).ToList();
                    if (lstItemSaleHaveRecipe != null && lstItemSaleHaveRecipe.Count > 0)
                    {
                        UsageManagementDetailModel detail = null;
                        int indexChildPage = 1;
                        foreach (var busDay in _lstBusDayAllStore)
                        {
                            var lstDish = lstItemSaleHaveRecipe.Where(ww => ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).ToList();
                            if (lstDish != null && lstDish.Count > 0)
                            {
                                var lstDishGroup = lstDish.GroupBy(gg => gg.ItemId);
                                foreach (var dish in lstDishGroup)
                                {
                                    var recipe = lstRecipeItems.Where(ww => ww.ItemId == dish.Key).FirstOrDefault();
                                    if (recipe != null)
                                    {
                                        detail = new UsageManagementDetailModel();
                                        detail.BusinessDay = busDay.DateDisplay;
                                        detail.Index = indexChildPage;
                                        detail.ItemName = dish.Select(ss => ss.ItemName).FirstOrDefault();
                                        detail.Qty = dish.Sum(ss => ss.Quantity);
                                        detail.Usage = detail.Qty * recipe.BaseUsage;

                                        usageManagementModel.ListDetail.Add(detail);
                                        indexChildPage++;
                                    }
                                }

                            }
                        }
                    }

                }
            }
            return usageManagementModel;
        }

        private List<RecipeItemModel> GetRecipeItem(string storeId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResult = (from i in cxt.I_Recipe_Item
                                 join ing in cxt.I_Ingredient on i.IngredientId equals ing.Id
                                 where i.StoreId == storeId && i.Status != (int)Commons.EStatus.Deleted
                                  && ing.Status != (int)Commons.EStatus.Deleted
                                 select new RecipeItemModel()
                                 {
                                     IngredientId = ing.Id,
                                     ItemId = i.ItemId,
                                     IngredientCode = ing.Code,
                                     IngredientName = ing.Name,
                                     UOMName = ing.BaseUOMName,
                                     Usage = 0,
                                     BaseUsage = i.BaseUsage.HasValue? i.BaseUsage.Value: 0
                                 }).ToList();

                return lstResult;
            }
        }

        private List<RecipeItemModel> GetRecipeModifier(string storeId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResult = (from i in cxt.I_Recipe_Modifier
                                 join ing in cxt.I_Ingredient on i.IngredientId equals ing.Id
                                 where i.StoreId == storeId && i.Status != (int)Commons.EStatus.Deleted
                                   && ing.Status != (int)Commons.EStatus.Deleted
                                 select new RecipeItemModel()
                                 {
                                     IngredientId = ing.Id,
                                     ItemId = i.ModifierId,
                                     IngredientCode = ing.Code,
                                     IngredientName = ing.Name,
                                     UOMName = ing.BaseUOMName,
                                     Usage = 0,
                                     BaseUsage = i.BaseUsage.HasValue ? i.BaseUsage.Value : 0
                                 }).ToList();

                return lstResult;
            }
        }

        #region Details
        private List<RecipeItemModel> GetRecipeItem(string storeId, string ingredientId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResult = (from i in cxt.I_Recipe_Item
                                 join ing in cxt.I_Ingredient on i.IngredientId equals ing.Id
                                 where i.StoreId == storeId && i.IngredientId == ingredientId
                                 && i.Status != (int)Commons.EStatus.Deleted
                                 select new RecipeItemModel()
                                 {
                                     IngredientId = ing.Id,
                                     ItemId = i.ItemId,
                                     IngredientCode = ing.Code,
                                     IngredientName = ing.Name,
                                     UOMName = ing.BaseUOMName,
                                     Usage = 0,
                                     BaseUsage = i.Usage
                                 }).ToList();

                return lstResult;
            }
        }
        private List<RecipeItemModel> GetRecipeModifier(string storeId, string ingredientId)
        {
            using (var cxt = new NuWebContext())
            {
                var lstResult = (from i in cxt.I_Recipe_Modifier
                                 join ing in cxt.I_Ingredient on i.IngredientId equals ing.Id
                                 where i.StoreId == storeId && i.IngredientId == ingredientId
                                  && i.Status != (int)Commons.EStatus.Deleted
                                 select new RecipeItemModel()
                                 {
                                     IngredientId = ing.Id,
                                     ItemId = i.ModifierId,
                                     IngredientCode = ing.Code,
                                     IngredientName = ing.Name,
                                     UOMName = ing.BaseUOMName,
                                     Usage = 0,
                                     BaseUsage = i.Usage
                                 }).ToList();

                return lstResult;
            }
        }
        #endregion

        #region Export
        public ResultModels Export(ref IXLWorksheet wsexcel, UsageManagementRequest request)
        {
            var result = new ResultModels();
            try
            {
                using (var cxt = new NuWebContext())
                {
                    wsexcel.Cell("A" + 1).Value = "Usage Management";
                    wsexcel.Row(1).Style.Font.SetBold(true);
                    wsexcel.Row(1).Height = 25;
                    wsexcel.Range(1, 1, 1, 5).Merge();
                    //Date
                    string date = string.Format("Date: {0}", request.DateFrom.ToString("MM/dd/yyyy"));
                    if (request.DateFrom.Date != request.DateTo.Date)
                        date = string.Format("Date from {0} to {1}", request.DateFrom.ToString("MM/dd/yyyy"), request.DateTo.ToString("MM/dd/yyyy"));
                    wsexcel.Cell("A" + 2).Value = date;
                    wsexcel.Row(2).Style.Font.SetBold(true);
                    wsexcel.Row(2).Height = 16;
                    wsexcel.Range(2, 1, 2, 5).Merge();

                    string[] lstHeaders = new string[] {
                    "Index","Ingredient Code", "Ingredient Name","Base UOMs","Usage"};
                    int row = 3;
                    //Add header to excel
                    for (int i = 1; i <= lstHeaders.Length; i++)
                    {
                        wsexcel.Cell(row, i).Value = lstHeaders[i - 1];
                        wsexcel.Row(row).Style.Font.SetBold(true);
                    }
                    int cols = lstHeaders.Length;
                    row = 4;
                    //Get list data
                    var lstData = _usageManagementFactory.GetUsageManagement(request); //CalUsageManagementwithoutDetail(request);
                    if (lstData != null && lstData.Count > 0)
                    {
                        foreach (var item in lstData)
                        {
                            wsexcel.Cell("A" + row).Value = item.Index;
                            wsexcel.Cell("B" + row).Value = item.Code;
                            wsexcel.Cell("C" + row).Value = item.Name;
                            wsexcel.Cell("D" + row).Value = item.UOMName;
                            wsexcel.Cell("E" + row).Value = item.Usage;

                            row++;
                        }
                    }
                    wsexcel.Range("E2", "E" + row).Style.NumberFormat.Format = "#,##0.0000";

                    BaseFactory.FormatExcelExport(wsexcel, row, cols);
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
        #endregion

        #region Xero Old

        private bool IsPush(DateTime dTo, string storeId)
        {
            if (Commons.IsXeroIngredient)
            {
                using (var cxt = new NuWebContext())
                {
                    var obj = cxt.I_UsageManagementXeroTrackLog.Where(ww => DbFunctions.TruncateTime(ww.ToDate) == DbFunctions.TruncateTime(dTo)
                    && ww.StoreId == storeId).FirstOrDefault();
                    if (obj != null)
                    {
                        //have push
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public bool PushDataToXero(UsageManagementRequest request, List<UsageManagementModel> lstUsages)
        {
            bool result = true;
            //if (IsPush(request.DateTo, request.StoreId))
            //{
                //List<UsageManagementModel> lstCalResult = CalUsageManagementwithoutDetail(request);
                try
                {
                    IngredientSyncRequestDTO model = new IngredientSyncRequestDTO();
                    model.AppRegistrationId = Commons.XeroRegistrationAppId;
                    model.AccessToken = Commons.XeroAccessToken;
                    model.StoreId = request.StoreId;

                    var lstIngredient = XeroFactory.GetIngredientsFromXero(new Models.Xero.XeroBaseModel()
                    {
                        AppRegistrationId = Commons.XeroRegistrationAppId,
                        AccessToken = Commons.XeroAccessToken,
                        StoreId = request.StoreId
                    }).Result;

                    if (lstIngredient.Success)
                    {
                        foreach (var item in lstUsages)
                        {
                            var obj = lstIngredient.Data.Items.Where(ww => ww.Code.ToUpper().Equals(item.Code.ToUpper())).FirstOrDefault();
                            if (obj != null)
                            {
                                model.Items.Add(new IngredientUsageSyncItem()
                                {
                                    Id = obj.ID,
                                    Code = obj.Code,
                                    QuantityUsed = (decimal)item.Usage
                                });
                            }
                        }
                        if (model.Items != null && model.Items.Count > 0)
                        {
                        NSLog.Logger.Info("PushDataToXero Data", model.Items);
                            result = XeroFactory.SyncIngredientsUsageToXero(request.DateTo, request.StoreId, model).Result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    _logger.Error(ex);
                }
            //}
            return result;
        }
        #endregion
    }
}
