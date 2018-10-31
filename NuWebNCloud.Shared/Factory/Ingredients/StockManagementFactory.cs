using ClosedXML.Excel;
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
    public class StockManagementFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public StockManagementFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<StockManagementModels> GetData(List<string> ListStored)
        {
            List<StockManagementModels> listData = new List<StockManagementModels>();
            try
            {
                using (var cxt = new NuWebContext())
                {
                    var results = (from sm in cxt.I_InventoryManagement
                                   from i in cxt.I_Ingredient.Where(ww => ww.Id == sm.IngredientId).DefaultIfEmpty()
                                   from uom in cxt.I_UnitOfMeasure.Where(ww => ww.Id == i.BaseUOMId).DefaultIfEmpty()
                                   from uom2 in cxt.I_UnitOfMeasure.Where(ww => ww.Id == i.ReceivingUOMId).DefaultIfEmpty()
                                   from s in cxt.I_StoreSetting.Where(ww => ww.IngredientId == sm.IngredientId && ww.StoreId == sm.StoreId).DefaultIfEmpty()
                                   where ListStored.Contains(sm.StoreId) && ((i.IsSelfMode && i.StockAble.Value) || i.IsPurchase)
                                   orderby sm.StoreId 
                                   select new { sm, i, uom, uom2, s });

                    listData = results.Select(ss => new StockManagementModels
                    {
                        Id = ss.sm.Id,
                        IngredientCode = ss.i.Code,
                        IngredientId = ss.sm.IngredientId,
                        IngredientName = ss.i.Name,
                        Price = ss.sm.Price,
                        Quantity = ss.sm.Quantity,
                        StoreId = ss.sm.StoreId,
                        StoreName = ss.sm.StoreId,
                        BaseUOM = ss.uom != null ? ss.uom.Name : "",
                        ReceivingUOM = ss.uom2 != null ? ss.uom2.Name : "",
                        Type = ss.i != null ? (ss.i.IsPurchase ? "Purchase" : "Self-made") : "",
                        Rate = ss.i.ReceivingQty,
                        POQty = Math.Round(((ss.sm.POQty.HasValue ? ss.sm.POQty.Value : 0) / (ss.i.ReceivingQty ==0? 1: ss.i.ReceivingQty)),4),
                        Status = ss.sm.Quantity <= 0 ? (int)Commons.EStockStatus.StockEmpty :
                                            ((ss.s != null && ss.sm.Quantity < (ss.s.MinAltert * ss.i.ReceivingQty)) ? (int)Commons.EStockStatus.LowStock : (int)Commons.EStockStatus.Normal)
                    }).ToList();

                    //if (ListStored!= null)
                    //{
                    //    listData = listData.Where(x => ListStored.Contains(x.StoreId)).ToList();
                    //}
                }
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("StockManagement_GetList: " + e);
                return listData;
            }
        }

        public StatusResponse Export(ref IXLWorksheet ws, List<string> lstStore, List<SelectListItem> vbStore)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<StockManagementModels> listData = this.GetData(lstStore);
                listData = listData.OrderBy(x => x.StoreName).ThenBy(x => x.IngredientName).ToList();
                int cols = 8;
                //Header
                int row = 1;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index") /*"Index"*/;
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code")/*"Ingredient Code"*/;
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Name")/*"Ingredient Name"*/;
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unrestricted")/*"Unrestricted"*/;
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Re-Order")/*"Re-Order"*/;
                ws.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Base UOM")/*"Base UOM"*/;
                ws.Cell("G" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receiving UOM")/* "Receiving UOM"*/;
                ws.Cell("H" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store")/*"Store"*/;
                //Item
                row = 2;
                int countIndex = 1;
                foreach (var item in listData)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.IngredientCode;
                    ws.Cell("C" + row).Value = item.IngredientName;
                    ws.Cell("D" + row).Value = item.Quantity;
                    ws.Cell("E" + row).Value = item.POQty;
                    ws.Cell("F" + row).Value = item.BaseUOM;
                    ws.Cell("G" + row).Value = item.ReceivingUOM;
                    ws.Cell("H" + row).Value = vbStore.Where(x => x.Value.Equals(item.StoreId)).FirstOrDefault().Text;
                    row++;
                    countIndex++;
                }
                FormatExcelExport(ws, row, cols);
                Response.Status = true;
            }
            catch (Exception e)
            {
                Response.Status = false;
                Response.MsgError = e.Message;
            }
            finally
            {

            }
            return Response;
        }

    }
}
