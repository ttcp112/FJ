using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Utilities;
//using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace NuWebNCloud.Shared.Integration.Factory.Sandbox.Product
{
    public class InteProductFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public InteProductFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<InteProductLiteModels> GetListProduct(int TypeID = -1, List<string> ListOrganizationId = null)
        {
            List<InteProductLiteModels> listData = new List<InteProductLiteModels>();
            try
            {
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.Type = TypeID;
                paraBody.ListOrgID = ListOrganizationId;
                NSLog.Logger.Info("GetListProduct request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteGetProduct, null, paraBody);
                NSLog.Logger.Info("GetListProduct result", result);

                dynamic data = result.Data;
                var lstData = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstData);
                listData = JsonConvert.DeserializeObject<List<InteProductLiteModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetListProduct error", e);
                //_logger.Error("Product_GetList: " + e);
                return listData;
            }
        }

        public InteProductModels GetProductDetail(string ProductId)
        {
            InteProductModels model = new InteProductModels();
            try
            {
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.ID = ProductId;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteGetProductDetail, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ProductDetail"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                model = JsonConvert.DeserializeObject<InteProductModels>(lstContent);
                return model;
            }
            catch (Exception e)
            {
                _logger.Error("Product_GetList: " + e);
                return model;
            }
        }

        public List<InteProductApplyItemModels> GetProductApplyStore(string StoreId = null, int TypeID = -1, List<string> ListStoreID = null)
        {
            List<InteProductApplyItemModels> listData = new List<InteProductApplyItemModels>();
            try
            {
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.StoreID = StoreId;
                paraBody.Type = TypeID;
                paraBody.ListStoreID = ListStoreID;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteGetProductApplyStore, null, paraBody);
                dynamic data = result.Data;
                var lstData = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstData);
                listData = JsonConvert.DeserializeObject<List<InteProductApplyItemModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Product_GetList: " + e);
                return listData;
            }
        }

        /**/
        public double GetExtraPrice(string ProductId, int ProductType)
        {
            //InteProductModels model = new InteProductModels();
            //try
            //{
            //    InteProductApiModels paraBody = new InteProductApiModels();
            //    paraBody.AppKey = Commons.AppKey;
            //    paraBody.AppSecret = Commons.AppSecret;
            //    paraBody.CreatedUser = Commons.CreateUser;
            //    paraBody.ID = ProductId;
            //    paraBody.IsShowInReservation = false;

            //    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetProductDetail, null, paraBody);
            //    dynamic data = result.Data;
            //    var lstC = data["ProductDTO"];
            //    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
            //    model = JsonConvert.DeserializeObject<InteProductModels>(lstContent);

            //    double extraPrice = 0;
            //    if (model.ListGroup != null)
            //    {
            //        int index = 0;
            //        model.ListGroup.ForEach(w =>
            //        {
            //            if (model.ListGroup[index] != null)
            //            {
            //                model.ListGroup[index].ListProductOnGroup.ForEach(x =>
            //                {
            //                    if (ProductType== (byte)Commons.EProductType.Dish)//Dish
            //                    {
            //                        extraPrice += x.ExtraPrice;
            //                    }
            //                    else if (ProductType == (byte)Commons.EProductType.SetMenu) //SetMenu
            //                    {
            //                        extraPrice = GetExtraPrice(x.ProductID, (byte)Commons.EProductType.Dish);
            //                    }
            //                });
            //            }
            //            index++;
            //        });
            //    }
            //    return extraPrice;
            //}
            //catch (Exception e)
            //{
            //    _logger.Error("Product_ExtraPrice: " + e);
            //    return 0;
            //}
            return 0;
        }

        public bool InsertOrUpdateProduct(InteProductModels model, string CreatedUser, ref string msg)
        {
            try
            {
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.ProductDTO = model;
                paraBody.CreatedUser = CreatedUser;

                NSLog.Logger.Info("InsertOrUpdateProduct request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteCreateOrEditProduct, null, paraBody);
                NSLog.Logger.Info("InsertOrUpdateProduct result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
  
                        return false;
                    }
                }
                else
                {
                    msg = result.ToString();
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("InsertOrUpdateProduct error: " ,e);
                msg = e.Message;
                return false;
            }
        }

        public bool DeleteProduct(string ID, string CreatedUser, ref string msg)
        {
            try
            {
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.ID = ID;
                paraBody.CreatedUser = CreatedUser;
                NSLog.Logger.Info("DeleteProduct request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteDeleteProduct, null, paraBody);
                NSLog.Logger.Info("DeleteProduct result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("DeleteProduct Error: ", e);
                msg = e.Message.ToString();
                return false;
            }
        }

        /*===== Export */
        public StatusResponse ExportSetMenu(ref IXLWorksheet wsSetMerchant, ref IXLWorksheet wsSetStore, ref IXLWorksheet wsTab
            , ref IXLWorksheet wsDishes, List<string> ListOrgID, List<string> lstStoreIds)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<InteProductModels> listData = new List<InteProductModels>();
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.ListOrgID = ListOrgID;
                paraBody.ProductType = (byte)Commons.EProductType.SetMenu;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteExportProduct, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<InteProductModels>>(lstContent);
                if (listData != null && listData.Any())
                {
                    listData = listData.Where(o => o.ListProductOnStore.Select(s => s.StoreID).ToList().Any(sID => lstStoreIds.Contains(sID))).ToList();
                }

                int row = 1;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Merchant Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Barcode"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image Url") };
                for (int i = 1; i <= listSetMenuHeader.Length; i++)
                    wsSetMerchant.Cell(row, i).Value = listSetMenuHeader[i - 1];
                int cols = listSetMenuHeader.Length;
                //Item
                row = 2;
                int countMerchantIndex = 1;
                int countStoreIndex = 1;
                int countIndexTab = 1;

                List<ExportSetStore> lstSetStore = new List<ExportSetStore>();
                List<ExportSetTabDish> lstSetTab = new List<ExportSetTabDish>();
                List<ExportSetDish> lstDish = new List<ExportSetDish>();

                foreach (var item in listData)
                {
                    wsSetMerchant.Cell("A" + row).Value = countMerchantIndex;
                    wsSetMerchant.Cell("B" + row).Value = item.Name;
                    wsSetMerchant.Cell("C" + row).Value = item.ProductCode;
                    wsSetMerchant.Cell("D" + row).Value = item.BarCode;
                    wsSetMerchant.Cell("E" + row).Value = item.ImageURL;
                    //=============================
                    if (item.ListProductOnStore != null)
                    {
                        foreach (var itemProStore in item.ListProductOnStore)
                        {
                            if (lstStoreIds.Contains(itemProStore.StoreID))
                            {
                                var PriceOnStore = itemProStore.PriceOnStore;
                                ExportSetStore eProStore = new ExportSetStore()
                                {
                                    SetMerchantIndex = countMerchantIndex,
                                    SetStoreIndex = countStoreIndex,
                                    StoreName = itemProStore.StoreName,
                                    Sequence = itemProStore.Sequence,
                                    IsActive = itemProStore.IsActive,
                                    KitchenDisplayName = itemProStore.KitchenDisplayName,
                                    PrintOutName = itemProStore.PrintOutName,
                                    Price = PriceOnStore.DefaultPrice,
                                    SeasonalPrice = PriceOnStore.SeasonPrice,
                                    Season = PriceOnStore.SeasonPriceName,
                                    Cost = itemProStore.Cost,
                                    Quantity = itemProStore.Quantity,
                                    IsCheckStock = itemProStore.IsCheckStock,
                                    Limit = itemProStore.Limit,
                                    Category = itemProStore.CategoryName,
                                    IsOpenPrice = itemProStore.IsAllowOpenPrice,
                                    ExpiredDate = itemProStore.ExpiredDate.Value.ToLocalTime(),
                                    IsPrintOnCheck = itemProStore.IsPrintOnCheck,
                                    SVC = itemProStore.ServiceChargeValue,
                                    IsAllowDiscount = itemProStore.IsAllowDiscount,
                                    IsComingSoon = itemProStore.IsComingSoon,
                                    SetInformation = itemProStore.Info,
                                    IsShowKioskMessage = itemProStore.IsShowMessage,
                                    TaxName = itemProStore.TaxName,
                                    IsPromotion = itemProStore.IsPromo
                                };

                                var ListProductSeasonKiosk = itemProStore.ListProductSeason.Where(x => !x.IsPOS).Select(x => x.SeasonName).ToList();
                                eProStore.KioskAvailability = (ListProductSeasonKiosk != null && ListProductSeasonKiosk.Count > 0) ? string.Join(";", ListProductSeasonKiosk) : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("All");
                                //======Tab
                                if (itemProStore.ListProductGroup != null && itemProStore.ListProductGroup.Count > 0)
                                {
                                    foreach (var ProOnGroup in itemProStore.ListProductGroup)
                                    {
                                        ExportSetTabDish eDishTab = new ExportSetTabDish()
                                        {
                                            TabIndex = countIndexTab,
                                            SetStoreIndex = countStoreIndex,
                                            Seq = ProOnGroup.Sequence,
                                            Quantity = ProOnGroup.Maximum,
                                            TabName = ProOnGroup.Name,
                                            DisplayMessage = ProOnGroup.Description,
                                        };

                                        //======Dish
                                        if (ProOnGroup.ListProductOnGroup != null && ProOnGroup.ListProductOnGroup.Count > 0)
                                        {
                                            foreach (var itemModifier in ProOnGroup.ListProductOnGroup)
                                            {
                                                ExportSetDish eModifier = new ExportSetDish()
                                                {
                                                    TabIndex = countIndexTab,
                                                    DishName = itemModifier.ProductName,
                                                    ExtraPrice = itemModifier.ExtraPrice,
                                                    Sequence = itemModifier.Sequence
                                                };
                                                lstDish.Add(eModifier);
                                            }
                                        }
                                        countIndexTab++;
                                        lstSetTab.Add(eDishTab);
                                    }
                                }
                                countStoreIndex++;
                                lstSetStore.Add(eProStore);
                            }
                        }
                    }
                    row++;
                    countMerchantIndex++;
                }
                FormatExcelExport(wsSetMerchant, row, cols);
                //========= STORE
                row = 1;
                string[] listDisStoreHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Merchant Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Store Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seasonal Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cost"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Check Stock"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Limit"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Open Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Expired Date"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print On Check"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SVC"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow Discount"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kiosk Availability"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Information"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show Kiosk Message"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show in Reservation & Queue Management module"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Is Promotion")
                };
                for (int i = 1; i <= listDisStoreHeader.Length; i++)
                    wsSetStore.Cell(row, i).Value = listDisStoreHeader[i - 1];
                cols = listDisStoreHeader.Length;
                row++;
                foreach (var item in lstSetStore)
                {
                    wsSetStore.Cell("A" + row).Value = item.SetMerchantIndex;
                    wsSetStore.Cell("B" + row).Value = item.SetStoreIndex;
                    wsSetStore.Cell("C" + row).Value = item.StoreName;
                    wsSetStore.Cell("D" + row).Value = item.Sequence;
                    wsSetStore.Cell("E" + row).Value = item.IsActive ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetStore.Cell("F" + row).Value = item.KitchenDisplayName;
                    wsSetStore.Cell("G" + row).Value = item.PrintOutName;
                    wsSetStore.Cell("H" + row).Value = item.Price;
                    wsSetStore.Cell("I" + row).Value = item.SeasonalPrice;
                    wsSetStore.Cell("J" + row).Value = item.Season;
                    wsSetStore.Cell("K" + row).Value = item.Cost;
                    wsSetStore.Cell("L" + row).Value = item.Quantity;
                    wsSetStore.Cell("M" + row).Value = item.IsCheckStock ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetStore.Cell("N" + row).Value = item.Limit;
                    wsSetStore.Cell("O" + row).Value = item.Category;
                    wsSetStore.Cell("P" + row).Value = item.IsOpenPrice ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetStore.Cell("Q" + row).Value = item.ExpiredDate.Date == Commons._ExpiredDate.Date ? "" : "'" + item.ExpiredDate.ToString("dd/MM/yyyy");
                    wsSetStore.Cell("R" + row).Value = item.IsPrintOnCheck ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetStore.Cell("S" + row).Value = item.SVC;
                    wsSetStore.Cell("T" + row).Value = item.IsAllowDiscount ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetStore.Cell("U" + row).Value = item.KioskAvailability;
                    wsSetStore.Cell("V" + row).Value = item.SetInformation;
                    wsSetStore.Cell("W" + row).Value = item.IsShowKioskMessage ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetStore.Cell("X" + row).Value = item.IsShowInReservation ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetStore.Cell("Y" + row).Value = item.TaxName;
                    wsSetStore.Cell("Z" + row).Value = item.IsPromotion ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");

                    row++;
                }
                FormatExcelExport(wsSetStore, row, cols);
                //========= TAB
                row = 1;
                string[] listTabHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Store Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Display Message"),
                };
                for (int i = 1; i <= listTabHeader.Length; i++)
                    wsTab.Cell(row, i).Value = listTabHeader[i - 1];
                cols = listTabHeader.Length;
                row++;
                foreach (var item in lstSetTab)
                {
                    wsTab.Cell("A" + row).Value = item.TabIndex;
                    wsTab.Cell("B" + row).Value = item.SetStoreIndex;
                    wsTab.Cell("C" + row).Value = item.Seq;
                    wsTab.Cell("D" + row).Value = item.Quantity;
                    wsTab.Cell("E" + row).Value = item.TabName;
                    wsTab.Cell("F" + row).Value = item.DisplayMessage;
                    row++;
                }
                FormatExcelExport(wsTab, row, cols);
                //============ DISHES
                row = 1;
                string[] listModifierHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Extra Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence")
                };
                for (int i = 1; i <= listModifierHeader.Length; i++)
                    wsDishes.Cell(row, i).Value = listModifierHeader[i - 1];
                cols = listModifierHeader.Length;
                row++;
                foreach (var item in lstDish)
                {
                    wsDishes.Cell("A" + row).Value = item.TabIndex;
                    wsDishes.Cell("B" + row).Value = item.DishName;
                    wsDishes.Cell("C" + row).Value = item.ExtraPrice;
                    wsDishes.Cell("D" + row).Value = item.Sequence;
                    row++;
                }
                //========
                FormatExcelExport(wsDishes, row, cols);
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

        public StatusResponse ExportDish(ref IXLWorksheet wsDishMerchant
            , ref IXLWorksheet wsDishStore, ref IXLWorksheet wsTab, ref IXLWorksheet wsModifier, List<string> ListOrgID, List<string> lstStoreIds)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<InteProductModels> listData = new List<InteProductModels>();
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.ListOrgID = ListOrgID;
                paraBody.ProductType = (byte)Commons.EProductType.Dish;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteExportProduct, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<InteProductModels>>(lstContent);
                if (listData != null && listData.Any())
                {
                    //var a = listData.Select(o => o.ListProductOnStore.Select(s => s.StoreID)).Distinct().ToList();
                    listData = listData.Where(o => o.ListProductOnStore.Select(s => s.StoreID).ToList().Any(sID => lstStoreIds.Contains(sID))).ToList();
                }


                int row = 1;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Merchant Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("BarCode"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image Url"),};
                for (int i = 1; i <= listSetMenuHeader.Length; i++)
                    wsDishMerchant.Cell(row, i).Value = listSetMenuHeader[i - 1];
                int cols = listSetMenuHeader.Length;
                //Item
                row = 2;
                int countMerchantIndex = 1;
                int countStoreIndex = 1;
                int countIndexTab = 1;

                List<ExportDishStore> lstDishStore = new List<ExportDishStore>();
                List<ExportDishTabModifier> lstDishTab = new List<ExportDishTabModifier>();
                List<ExportDishModifier> lstModifier = new List<ExportDishModifier>();

                foreach (var item in listData)
                {
                    wsDishMerchant.Cell("A" + row).Value = countMerchantIndex;
                    wsDishMerchant.Cell("B" + row).Value = item.Name;
                    wsDishMerchant.Cell("C" + row).Value = item.ProductCode;
                    wsDishMerchant.Cell("D" + row).Value = item.BarCode;
                    wsDishMerchant.Cell("E" + row).Value = item.ImageURL;
                    //=============================
                    if (item.ListProductOnStore != null)
                    {
                        foreach (var itemProStore in item.ListProductOnStore)
                        {
                            if (lstStoreIds.Contains(itemProStore.StoreID))
                            {
                                var PriceOnStore = itemProStore.PriceOnStore;
                                ExportDishStore eProStore = new ExportDishStore()
                                {
                                    DishMerchantIndex = countMerchantIndex,
                                    DishStoreIndex = countStoreIndex,
                                    StoreName = itemProStore.StoreName,
                                    Sequence = itemProStore.Sequence,
                                    IsActive = itemProStore.IsActive,
                                    Color = itemProStore.ColorCode,
                                    KitchenDisplayName = itemProStore.KitchenDisplayName,
                                    PrintOutName = itemProStore.PrintOutName,
                                    Price = PriceOnStore.DefaultPrice,
                                    SeasonalPrice = PriceOnStore.SeasonPrice,
                                    Season = PriceOnStore.SeasonPriceName,
                                    Cost = itemProStore.Cost,
                                    Unit = itemProStore.Unit,
                                    Quantity = itemProStore.Quantity,
                                    IsCheckStock = itemProStore.IsCheckStock,
                                    Limit = itemProStore.Limit,
                                    Category = itemProStore.CategoryName,
                                    IsOpenPrice = itemProStore.IsAllowOpenPrice,
                                    ExpiredDate = itemProStore.ExpiredDate.Value.ToLocalTime(),
                                    UnitofMeasurement = itemProStore.Measure,
                                    IsPrintOnCheck = itemProStore.IsPrintOnCheck,
                                    SVC = itemProStore.ServiceChargeValue,
                                    IsAllowDiscount = itemProStore.IsAllowDiscount,
                                    IsForceModifierPopup = itemProStore.IsForce,
                                    IsOptionalModifierPopup = itemProStore.IsOptional,

                                    DefaultStatus = itemProStore.DefaultState,
                                    IsComingSoon = itemProStore.IsComingSoon,
                                    DishInformation = itemProStore.Info,

                                    IsShowKioskMessage = itemProStore.IsShowMessage,
                                    TaxName = itemProStore.TaxName
                                };

                                var ListProductSeasonKiosk = itemProStore.ListProductSeason.Where(x => !x.IsPOS).Select(x => x.SeasonName).ToList();
                                eProStore.KioskAvailability = (ListProductSeasonKiosk != null && ListProductSeasonKiosk.Count > 0) ? string.Join(";", ListProductSeasonKiosk) : "All";

                                var ListProductSeasonPOS = itemProStore.ListProductSeason.Where(x => x.IsPOS).Select(x => x.SeasonName).ToList();
                                eProStore.POSAvailability = (ListProductSeasonPOS != null && ListProductSeasonPOS.Count > 0) ? string.Join(";", ListProductSeasonPOS) : "All";

                                var ListPrinters = itemProStore.ListProductPrinter.Where(x => x.Type == (byte)(Commons.ProductPrinterType.Normal)).ToList().Select(x => x.PrinterName).ToList();
                                eProStore.PrInte = (ListPrinters != null && ListPrinters.Count > 0) ? string.Join(";", ListPrinters) : "";
                                var ListLabelPrinters = itemProStore.ListProductPrinter.Where(x => x.Type == (byte)(Commons.ProductPrinterType.Label)).ToList().Select(x => x.PrinterName).ToList();
                                eProStore.LabelPrinter = (ListLabelPrinters != null && ListLabelPrinters.Count > 0) ? string.Join(";", ListLabelPrinters) : "";
                                //======Tab
                                if (itemProStore.ListProductGroup != null && itemProStore.ListProductGroup.Count > 0)
                                {
                                    foreach (var ProOnGroup in itemProStore.ListProductGroup)
                                    {
                                        ExportDishTabModifier eDishTab = new ExportDishTabModifier()
                                        {
                                            TabIndex = countIndexTab,
                                            DishStoreIndex = countStoreIndex,
                                            Seq = ProOnGroup.Sequence,
                                            Quantity = ProOnGroup.Maximum,
                                            TabName = ProOnGroup.Name,
                                            DisplayMessage = ProOnGroup.Description,
                                            GroupType = ProOnGroup.GroupType
                                        };

                                        //======Modifier
                                        if (ProOnGroup.ListProductOnGroup != null && ProOnGroup.ListProductOnGroup.Count > 0)
                                        {
                                            foreach (var itemModifier in ProOnGroup.ListProductOnGroup)
                                            {
                                                ExportDishModifier eModifier = new ExportDishModifier()
                                                {
                                                    TabIndex = countIndexTab,
                                                    ModifierName = itemModifier.ProductName,
                                                    ExtraPrice = itemModifier.ExtraPrice,
                                                    Sequence = itemModifier.Sequence
                                                };
                                                lstModifier.Add(eModifier);
                                            }
                                        }
                                        countIndexTab++;
                                        lstDishTab.Add(eDishTab);
                                    }
                                }
                                countStoreIndex++;
                                lstDishStore.Add(eProStore);
                            }
                        }// end  foreach (var itemProStore in item.ListProductOnStore)
                    }
                    row++;
                    countMerchantIndex++;
                }
                FormatExcelExport(wsDishMerchant, row, cols);
                //========= STORE
                row = 1;
                string[] listDisStoreHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Merchant Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Store Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Color"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seasonal Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cost"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Check Stock"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Limit"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Open Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Expired Date"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit of Measurement"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print On Check"),"SVC",
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow Discount"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Force Modifier Popup"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Optional Modifier Popup"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kiosk Availability"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("POS Availability"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Coming Soon"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Information"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show Kiosk Message"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Printer"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Label Printer"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax")
                };
                for (int i = 1; i <= listDisStoreHeader.Length; i++)
                    wsDishStore.Cell(row, i).Value = listDisStoreHeader[i - 1];
                cols = listDisStoreHeader.Length;
                row++;
                foreach (var item in lstDishStore)
                {
                    wsDishStore.Cell("A" + row).Value = item.DishMerchantIndex;
                    wsDishStore.Cell("B" + row).Value = item.DishStoreIndex;
                    wsDishStore.Cell("C" + row).Value = item.StoreName;
                    wsDishStore.Cell("D" + row).Value = item.Sequence;
                    wsDishStore.Cell("E" + row).Value = item.IsActive ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("F" + row).Value = item.Color;
                    wsDishStore.Cell("G" + row).Value = item.KitchenDisplayName;
                    wsDishStore.Cell("H" + row).Value = item.PrintOutName;
                    wsDishStore.Cell("I" + row).Value = item.Price;
                    wsDishStore.Cell("J" + row).Value = item.SeasonalPrice;
                    wsDishStore.Cell("K" + row).Value = item.Season;
                    wsDishStore.Cell("L" + row).Value = item.Cost;
                    wsDishStore.Cell("M" + row).Value = item.Unit;
                    wsDishStore.Cell("N" + row).Value = item.Quantity;
                    wsDishStore.Cell("O" + row).Value = item.IsCheckStock ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("P" + row).Value = item.Limit;
                    wsDishStore.Cell("Q" + row).Value = item.Category;
                    wsDishStore.Cell("R" + row).Value = item.IsOpenPrice ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("S" + row).Value = item.ExpiredDate.Date == Commons._ExpiredDate.Date ? "" : "'" + item.ExpiredDate.ToString("dd/MM/yyyy");
                    wsDishStore.Cell("T" + row).Value = item.UnitofMeasurement;
                    wsDishStore.Cell("U" + row).Value = item.IsPrintOnCheck ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("V" + row).Value = item.SVC == -1 ? "" : item.SVC.ToString();
                    wsDishStore.Cell("W" + row).Value = item.IsAllowDiscount ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("X" + row).Value = item.IsForceModifierPopup ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("Y" + row).Value = item.IsOptionalModifierPopup ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("Z" + row).Value = item.KioskAvailability;

                    wsDishStore.Cell("AA" + row).Value = item.POSAvailability;
                    wsDishStore.Cell("AB" + row).Value = item.DefaultStatus == (byte)Commons.EItemState.PendingStatus ? Commons.PendingStatus :
                                                       item.DefaultStatus == (byte)Commons.EItemState.CompleteStatus ? Commons.ReadyStatus :
                                                       item.DefaultStatus == (byte)Commons.EItemState.ServedStatus ? Commons.ServedStatus : "";

                    wsDishStore.Cell("AC" + row).Value = item.IsComingSoon ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("AD" + row).Value = item.DishInformation;
                    wsDishStore.Cell("AE" + row).Value = item.IsShowKioskMessage ?
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") :
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsDishStore.Cell("AF" + row).Value = item.PrInte;
                    wsDishStore.Cell("AG" + row).Value = item.LabelPrinter;
                    wsDishStore.Cell("AH" + row).Value = item.TaxName;
                    row++;
                }
                FormatExcelExport(wsDishStore, row, cols);
                //========= TAB
                row = 1;
                string[] listTabHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Store Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Display Message"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type")
                };
                for (int i = 1; i <= listTabHeader.Length; i++)
                    wsTab.Cell(row, i).Value = listTabHeader[i - 1];
                cols = listTabHeader.Length;
                row++;
                foreach (var item in lstDishTab)
                {
                    string modifierType = "";
                    if (item.GroupType == (byte)Commons.EModifierType.Forced)
                        modifierType = Commons.EModifierType.Forced.ToString();
                    else if (item.GroupType == (byte)Commons.EModifierType.Optional)
                        modifierType = Commons.EModifierType.Optional.ToString();
                    else
                        modifierType = "Additional Dish";
                    //=========
                    wsTab.Cell("A" + row).Value = item.TabIndex;
                    wsTab.Cell("B" + row).Value = item.DishStoreIndex;
                    wsTab.Cell("C" + row).Value = item.Seq;
                    wsTab.Cell("D" + row).Value = item.Quantity;
                    wsTab.Cell("E" + row).Value = item.TabName;
                    wsTab.Cell("F" + row).Value = item.DisplayMessage;
                    wsTab.Cell("G" + row).Value = modifierType;
                    row++;
                }
                FormatExcelExport(wsTab, row, cols);
                //============ MODIFIER
                row = 1;
                string[] listModifierHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Extra Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence")
                };
                for (int i = 1; i <= listModifierHeader.Length; i++)
                    wsModifier.Cell(row, i).Value = listModifierHeader[i - 1];
                cols = listModifierHeader.Length;
                row++;
                foreach (var item in lstModifier)
                {
                    wsModifier.Cell("A" + row).Value = item.TabIndex;
                    wsModifier.Cell("B" + row).Value = item.ModifierName;
                    wsModifier.Cell("C" + row).Value = item.ExtraPrice;
                    wsModifier.Cell("D" + row).Value = item.Sequence;
                    row++;
                }
                //========
                FormatExcelExport(wsModifier, row, cols);
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

        public StatusResponse ExportModifier(ref IXLWorksheet wsModifierMerchant, ref IXLWorksheet wsModifierStore
            , List<string> ListOrgID, List<string> lstStoreIds)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<InteProductModels> listData = new List<InteProductModels>();
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.ListOrgID = ListOrgID;
                paraBody.ProductType = (byte)Commons.EProductType.Modifier;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteExportProduct, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<InteProductModels>>(lstContent);
                if (listData != null && listData.Any())
                {
                    listData = listData.Where(o => o.ListProductOnStore.Select(s => s.StoreID).ToList().Any(sID => lstStoreIds.Contains(sID))).ToList();
                }
                List<ExportModifierStore> lstModifierStore = new List<ExportModifierStore>();
                int row = 1;
                string[] listModifierHeader = new string[] { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Merchant Index"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Name"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Code"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Bar Code"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image URL") };
                for (int i = 1; i <= listModifierHeader.Length; i++)
                    wsModifierMerchant.Cell(row, i).Value = listModifierHeader[i - 1];
                int cols = listModifierHeader.Length;
                //Item
                row = 2;
                int countIndex = 1;
                int countIndexModStore = 1;
                foreach (var item in listData)
                {
                    wsModifierMerchant.Cell("A" + row).Value = countIndex;
                    wsModifierMerchant.Cell("B" + row).Value = item.Name;
                    wsModifierMerchant.Cell("C" + row).Value = item.ProductCode;
                    wsModifierMerchant.Cell("D" + row).Value = item.BarCode;
                    wsModifierMerchant.Cell("E" + row).Value = item.ImageURL;// string.IsNullOrEmpty(item.ImageURL) ? "" : item.ImageURL.Replace(Commons._PublicImages, "");
                    //=============================
                    if (item.ListProductOnStore != null)
                    {
                        for (int i = 0; i < item.ListProductOnStore.Count; i++)
                        {
                            if (lstStoreIds.Contains(item.ListProductOnStore[i].StoreID))
                            {
                                var itemModStore = item.ListProductOnStore[i];
                                var PriceOnStore = itemModStore.PriceOnStore;
                                ExportModifierStore eModStore = new ExportModifierStore()
                                {
                                    ModifierMerchantIndex = countIndex,
                                    ModifierStoreIndex = countIndexModStore,
                                    StoreName = itemModStore.StoreName,
                                    Sequence = itemModStore.Sequence,
                                    IsActive = itemModStore.IsActive,
                                    KitchenDisplayName = itemModStore.KitchenDisplayName,
                                    PrintOutName = itemModStore.PrintOutName,
                                    Price = PriceOnStore.DefaultPrice,
                                    SeasonalPrice = PriceOnStore.SeasonPrice,
                                    Season = PriceOnStore.SeasonPriceName,
                                    Quantity = itemModStore.Quantity,
                                    IsCheckStock = itemModStore.IsCheckStock,
                                    Limit = itemModStore.Limit,
                                    Category = itemModStore.CategoryName,
                                    IsOpenPrice = itemModStore.IsAllowOpenPrice,
                                    IsPrintOnCheck = itemModStore.IsPrintOnCheck,
                                    IsAllowDiscount = itemModStore.IsAllowDiscount
                                };
                                countIndexModStore++;
                                lstModifierStore.Add(eModStore);
                            }
                        }
                    }
                    row++;
                    countIndex++;
                }
                FormatExcelExport(wsModifierMerchant, row, cols);
                //=========
                row = 1;
                string[] listModifierStoreHeader = new string[] {
                            _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Merchant Index"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Store Index"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name"),
                            _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seasonal Price"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Check Stock"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Limit"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show price (open price)"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print On Check"),
                            _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow to Apply Discount/Promotion")
                            };
                for (int i = 1; i <= listModifierStoreHeader.Length; i++)
                    wsModifierStore.Cell(row, i).Value = listModifierStoreHeader[i - 1];
                cols = listModifierStoreHeader.Length;
                row++;
                foreach (var item in lstModifierStore)
                {
                    wsModifierStore.Cell("A" + row).Value = item.ModifierMerchantIndex;
                    wsModifierStore.Cell("B" + row).Value = item.ModifierStoreIndex;
                    wsModifierStore.Cell("C" + row).Value = item.StoreName;
                    wsModifierStore.Cell("D" + row).Value = item.Sequence;
                    wsModifierStore.Cell("E" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsModifierStore.Cell("F" + row).Value = item.KitchenDisplayName;
                    wsModifierStore.Cell("G" + row).Value = item.PrintOutName;
                    wsModifierStore.Cell("H" + row).Value = item.Price;
                    wsModifierStore.Cell("I" + row).Value = item.SeasonalPrice;
                    wsModifierStore.Cell("J" + row).Value = item.Season;
                    wsModifierStore.Cell("K" + row).Value = item.Quantity;
                    wsModifierStore.Cell("L" + row).Value = item.IsCheckStock ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsModifierStore.Cell("M" + row).Value = item.Limit;
                    wsModifierStore.Cell("N" + row).Value = item.Category;
                    wsModifierStore.Cell("O" + row).Value = item.IsOpenPrice ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsModifierStore.Cell("P" + row).Value = item.IsPrintOnCheck ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsModifierStore.Cell("Q" + row).Value = item.IsAllowDiscount ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    row++;
                }
                FormatExcelExport(wsModifierStore, row, cols);

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

        /*=====End Export */

        /*===== Import */
        public List<SetMenuImportResultItem> ImportSetMenu(string filePath, FileInfo[] listFileInfo, out int totalRowExel, List<string> ListOrgID, ref string msg)
        {
            totalRowExel = 0;
            List<SetMenuImportResultItem> importItems = new List<SetMenuImportResultItem>();

            DataTable dtSetMerchant = ReadExcelFile(@filePath, "Set Merchant");
            DataTable dtSetStore = ReadExcelFile(@filePath, "Set Store");
            DataTable dtTab = ReadExcelFile(@filePath, "Tabs");
            DataTable dtDish = ReadExcelFile(@filePath, "Dishes");

            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBInventorySetMenuInte.xlsx";
            DataTable dtSetMerchantTmp = ReadExcelFile(@tmpExcelPath, "Set Merchant");
            DataTable dtSetStoreTmp = ReadExcelFile(@tmpExcelPath, "Set Store");
            DataTable dtTabTmp = ReadExcelFile(@tmpExcelPath, "Tabs");
            DataTable dtDishTmp = ReadExcelFile(@tmpExcelPath, "Dishes");

            if (dtSetMerchant.Columns.Count != dtSetMerchant.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            if (dtSetStore.Columns.Count != dtSetStoreTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            if (dtTab.Columns.Count != dtTabTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            if (dtDish.Columns.Count != dtDishTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }

            //List<SetMenuImportItem> lstSetMenu = GetListObject<SetMenuImportItem>(dtSetMenu);
            //List<TabImportItem> lstTab = GetListObject<TabImportItem>(dtTab);
            //List<DishTabImportItem> lstDish = GetListObject<DishTabImportItem>(dtDish);
            //// validate tab Set Menu, 
            //ValidateRowSetMenu(ref lstSetMenu);
            //ValidateRowTabSetMenu(ref lstTab);
            //ValidateRowDishSetMenu(ref lstDish);

            List<InteProductModels> listData = new List<InteProductModels>();
            SetMenuImportResultItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            foreach (DataRow item in dtSetMerchant.Rows)
            {
                flagInsert = true;
                msgError = "";

                if (item[0].ToString().Equals(""))
                    continue;
                int index = int.Parse(item[0].ToString());

                string ImageUrl = "";
                if (!string.IsNullOrEmpty(item[4].ToString()))
                {
                    FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[4].ToString().ToLower());
                    if (file != null)
                    {
                        if (file.Length > Commons._MaxSizeFileUploadImg)
                        {
                            flagInsert = false;
                            msgError = Commons._MsgAllowedSizeImg + "<br/>";
                        }
                        else
                        {
                            ImageUrl = Guid.NewGuid() + file.Extension;
                            byte[] photoByte = null;
                            photoByte = System.IO.File.ReadAllBytes(file.FullName);
                            //19/01/2018
                            //photoByte = file.ReadFully();
                            if (!string.IsNullOrEmpty(ImageUrl) && photoByte != null)
                            {
                                var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", System.Web.HttpContext.Current.Server.MapPath(@"\")));
                                var path = string.Format("{0}{1}", originalDirectory, ImageUrl);
                                MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                                ms.Write(photoByte, 0, photoByte.Length);
                                System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);
                                ImageHelper.Me.SaveCroppedImage(imageTmp, path, ImageUrl, ref photoByte);
                                FTP.Upload(ImageUrl, photoByte);
                                ImageHelper.Me.TryDeleteImageUpdated(path);
                            }
                        }
                    }
                }

                List<InteProductItemOnStore> ListProductOnStore = new List<InteProductItemOnStore>();
                DataRow[] ProductOnStoreRow = dtSetStore.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Merchant Index") + "] = " + index + "");
                foreach (var gProductOnStoreRow in ProductOnStoreRow)
                {
                    if (gProductOnStoreRow[0].ToString().Equals(""))
                        continue;

                    int DishStoreIndex = int.Parse(gProductOnStoreRow[1].ToString());
                    var PriceOnStore = new InteProductPriceModels();
                    PriceOnStore.DefaultPrice = string.IsNullOrEmpty(gProductOnStoreRow[7].ToString()) ? 0 : double.Parse(gProductOnStoreRow[7].ToString());
                    PriceOnStore.SeasonPrice = string.IsNullOrEmpty(gProductOnStoreRow[8].ToString()) ? 0 : double.Parse(gProductOnStoreRow[8].ToString());
                    PriceOnStore.SeasonPriceName = gProductOnStoreRow[9].ToString();

                    string msgItem = "";
                    DateTime ExpiredDate = DateTime.Now;
                    string sExpiredDate = gProductOnStoreRow[16].ToString();
                    if (!sExpiredDate.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("unlimited")) && !sExpiredDate.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("never")) && !string.IsNullOrEmpty(sExpiredDate.Trim()))
                    {
                        ExpiredDate = DateTimeHelper.GetDateImport(sExpiredDate, ref msgItem);
                        if (!msgItem.Equals(""))
                        {
                            flagInsert = false;
                            msgError = msgItem;
                        }
                    }
                    else
                        ExpiredDate = Commons._ExpiredDate;

                    InteProductItemOnStore itemProOnStore = new InteProductItemOnStore
                    {
                        OffSet = int.Parse(gProductOnStoreRow[1].ToString()),
                        StoreName = gProductOnStoreRow[2].ToString(),
                        Sequence = string.IsNullOrEmpty(gProductOnStoreRow[3].ToString()) ? 1 : int.Parse(gProductOnStoreRow[3].ToString()),
                        IsActive = string.IsNullOrEmpty(gProductOnStoreRow[4].ToString()) ? true
                            : bool.Parse(gProductOnStoreRow[4].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("yes")).ToString()) ? true : false,

                        KitchenDisplayName = gProductOnStoreRow[5].ToString(),
                        PrintOutName = gProductOnStoreRow[6].ToString(),

                        PriceOnStore = PriceOnStore,
                        Cost = string.IsNullOrEmpty(gProductOnStoreRow[10].ToString()) ? 0 : double.Parse(gProductOnStoreRow[10].ToString()),
                        Quantity = string.IsNullOrEmpty(gProductOnStoreRow[11].ToString()) ? 0 : double.Parse(gProductOnStoreRow[11].ToString()),
                        IsCheckStock = string.IsNullOrEmpty(gProductOnStoreRow[12].ToString()) ? false :
                            bool.Parse(gProductOnStoreRow[12].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("yes")).ToString()) ? true : false,

                        Limit = string.IsNullOrEmpty(gProductOnStoreRow[13].ToString()) ? 0 : int.Parse(gProductOnStoreRow[13].ToString()),
                        CategoryName = gProductOnStoreRow[14].ToString(),

                        IsAllowOpenPrice = string.IsNullOrEmpty(gProductOnStoreRow[15].ToString()) ? false :
                            bool.Parse(gProductOnStoreRow[15].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("yes")).ToString()) ? true : false,

                        ExpiredDate = ExpiredDate,

                        IsPrintOnCheck = string.IsNullOrEmpty(gProductOnStoreRow[17].ToString()) ? true :
                            bool.Parse(gProductOnStoreRow[17].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("yes")).ToString()) ? true : false,

                        ServiceChargeValue = string.IsNullOrEmpty(gProductOnStoreRow[18].ToString()) ? 0 : double.Parse(gProductOnStoreRow[18].ToString()),

                        IsAllowDiscount = string.IsNullOrEmpty(gProductOnStoreRow[19].ToString()) ? true :
                            bool.Parse(gProductOnStoreRow[22].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("yes")).ToString()) ? true : false,

                        Info = gProductOnStoreRow[21].ToString(),

                        IsShowMessage = string.IsNullOrEmpty(gProductOnStoreRow[22].ToString()) ? true :
                            bool.Parse(gProductOnStoreRow[22].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("yes")).ToString()) ? true : false,

                        IsShowInReservation = string.IsNullOrEmpty(gProductOnStoreRow[23].ToString()) ? true :
                            bool.Parse(gProductOnStoreRow[23].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("yes")).ToString()) ? true : false,

                        TaxName = gProductOnStoreRow[24].ToString(),
                        IsPromo = string.IsNullOrEmpty(gProductOnStoreRow[25].ToString()) ? true
                            : bool.Parse(gProductOnStoreRow[25].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("yes")).ToString()) ? true : false,
                        ColorCode = "#ffffff",
                        IsForce = true,
                        IsOptional = true,
                        Measure = "Set"
                    };

                    ///Kiosk Availability
                    string sKiosk = gProductOnStoreRow[20].ToString();
                    if (!string.IsNullOrEmpty(sKiosk))
                    {
                        foreach (var itemKiosk in sKiosk.Split(';'))
                        {
                            if (!itemKiosk.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("all")))
                            {
                                itemProOnStore.ListProductSeason.Add(new ProductSeasonDTO
                                {
                                    SeasonName = itemKiosk,
                                    IsPOS = false
                                });
                            }
                        }
                    }

                    //============ Get List Product Item On Store
                    List<InteGroupProductModels> lstGProduct = new List<InteGroupProductModels>();
                    DataRow[] GProducs = dtTab.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Store Index") + "] = " 
                        + DishStoreIndex + "");
                    foreach (DataRow gProduct in GProducs)
                    {
                        if (gProduct[0].ToString().Equals(""))
                            continue;

                        int tabIndex = int.Parse(gProduct[0].ToString());
                        //========Dish
                        List<InteProductOnGroupModels> ListProOnGroup = new List<InteProductOnGroupModels>();
                        DataRow[] Dishes = dtDish.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index") + "] = " + tabIndex + "");
                        foreach (DataRow RDish in Dishes)
                        {
                            if (gProduct[0].ToString().Equals(""))
                                continue;
                            InteProductOnGroupModels dish = new InteProductOnGroupModels()
                            {
                                ProductName = RDish[1].ToString(),
                                ExtraPrice = string.IsNullOrEmpty(RDish[2].ToString()) ? 0 : Math.Round(double.Parse(RDish[2].ToString()), 2),
                                Sequence = string.IsNullOrEmpty(RDish[3].ToString()) ? 1 : int.Parse(RDish[3].ToString()),
                            };
                            //Validation
                            if (string.IsNullOrEmpty(RDish[0].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index is required");
                            }
                            if (string.IsNullOrEmpty(dish.ProductName))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name is required");
                            }
                            if (dish.ExtraPrice < 0)
                            {
                                flagInsert = false;
                                msgError += "<br/> " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Extra Price larger or equal to 0");
                            }
                            if (dish.Sequence < 0)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence larger or equal to 0");
                            }

                            if (flagInsert)
                            {
                                ListProOnGroup.Add(dish);
                            }
                        }
                        //=======
                        InteGroupProductModels gPModel = new InteGroupProductModels()
                        {
                            Sequence = string.IsNullOrEmpty(gProduct[2].ToString()) ? 1 : int.Parse(gProduct[2].ToString()),
                            Minimum = 0,
                            Maximum = string.IsNullOrEmpty(gProduct[3].ToString()) ? 1 : int.Parse(gProduct[3].ToString()),
                            Name = gProduct[4].ToString(),
                            Description = gProduct[5].ToString(),
                            ListProductOnGroup = ListProOnGroup,
                        };

                        //Validation
                        if (string.IsNullOrEmpty(gProduct[0].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[1].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Store Index is required");
                        }
                        if (string.IsNullOrEmpty(gPModel.Name))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name is required");
                        }
                        if (gPModel.Sequence < 0)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence larger or equal to 0");
                        }
                        if (gPModel.Maximum < 0)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Quantity larger or equal to 0");
                        }
                        if (flagInsert)
                        {
                            lstGProduct.Add(gPModel);
                        }
                    }
                    itemProOnStore.ListProductGroup = lstGProduct;

                    //Validation
                    if (string.IsNullOrEmpty(gProductOnStoreRow[0].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Merchant Index is required");
                    }
                    if (string.IsNullOrEmpty(gProductOnStoreRow[1].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Store Index is required");
                    }
                    if (string.IsNullOrEmpty(itemProOnStore.StoreName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name is required");
                    }
                    if (itemProOnStore.Sequence < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence larger or equal to 0");
                    }
                    if (string.IsNullOrEmpty(gProductOnStoreRow[8].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price is required");
                    }
                    if (PriceOnStore.DefaultPrice < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Default Price larger or equal to 0");
                    }
                    if (PriceOnStore.SeasonPrice < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Season Price larger or equal to 0");
                    }
                    if (PriceOnStore.SeasonPrice > 0)
                    {
                        if (string.IsNullOrEmpty(PriceOnStore.SeasonPriceName))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season name is required");
                        }
                    }
                    if (itemProOnStore.Cost < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Cost larger or equal to 0");
                    }
                    if (itemProOnStore.Quantity < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Quantity larger or equal to 0");
                    }
                    if (itemProOnStore.Limit < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Limit larger or equal to 0");
                    }
                    if (string.IsNullOrEmpty(itemProOnStore.CategoryName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category is required");
                    }
                    if (itemProOnStore.ServiceChargeValue < -1 || itemProOnStore.ServiceChargeValue > 100)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge must between 0 and 100");
                    }
                    //=============
                    if (string.IsNullOrEmpty(itemProOnStore.TaxName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax is required");
                    }
                    //=============
                    if (itemProOnStore.ListProductGroup == null)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu must have as least one Tab");
                    }
                    else
                    {
                        foreach (InteGroupProductModels group in itemProOnStore.ListProductGroup)
                        {
                            if (group.ListProductOnGroup == null)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab must have as least one Dish");
                                break;
                            }
                        }
                    }
                    if (itemProOnStore.ListProductGroup != null)
                    {
                        foreach (var itemPro in itemProOnStore.ListProductGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList())
                        {
                            int qty = itemPro.Maximum;
                            string tabName = itemPro.Name;
                            if (itemPro.ListProductOnGroup != null)
                            {
                                int listItem = itemPro.ListProductOnGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList().Count;
                                if (listItem < qty)
                                {
                                    flagInsert = false;
                                    //msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of dishes of 'Tab Name") + " [" + tabName + "]" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("must be more than or equal Quantity of Tab Name") + " [" + tabName + "]" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("equal") + qty + "";

                                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of dishes of Tab Name") + " [" + tabName + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("must be more than or equal to the Quantity") + " [" + qty + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("of Tab Name") + " [" + tabName + "] ";
                                    break;
                                }
                            }
                        }
                    }
                    //==== trongntn 01-06-2016
                    if (ListProductOnStore.Count > 0)
                    {
                        var IsExist = ListProductOnStore.Exists(x => x.OffSet.Equals(itemProOnStore.OffSet));
                        if (IsExist)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Store Index already exist");
                        }
                    }

                    if (flagInsert)
                    {
                        ListProductOnStore.Add(itemProOnStore);
                    }
                }

                InteProductModels model = new InteProductModels
                {
                    Index = index.ToString(),
                    Name = item[1].ToString(),
                    ProductCode = item[2].ToString(),
                    BarCode = item[3].ToString(),
                    ImageURL = ImageUrl,
                    ProductType = (byte)Commons.EProductType.SetMenu,
                    ListProductOnStore = ListProductOnStore
                };

                //Validation
                if (string.IsNullOrEmpty(item[0].ToString()))
                {
                    flagInsert = false;
                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Merchant Index is required");
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    flagInsert = false;
                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Name is required");
                }
                if (string.IsNullOrEmpty(model.ProductCode))
                {
                    flagInsert = false;
                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Code is required");
                }
                //==== trongntn 01-06-2016
                if (listData.Count > 0)
                {
                    var IsExist = listData.Exists(x => x.Index.Equals(model.Index));
                    if (IsExist)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Merchant Index already exist");
                    }
                    if (string.IsNullOrEmpty(model.ProductCode))
                    {
                        IsExist = listData.Exists(x => x.ProductCode.Equals(model.ProductCode));
                        if (IsExist)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Code already exist");
                        }
                    }
                    //if (string.IsNullOrEmpty(model.BarCode))
                    //{
                    //    IsExist = listData.Exists(x => x.BarCode.Equals(model.BarCode));
                    //    if (IsExist)
                    //    {
                    //        flagInsert = false;
                    //        msgError += "<br/>Set BarCode is exist";
                    //    }
                    //}
                }
                //==========
                if (flagInsert)
                {
                    listData.Add(model);
                }
                else
                {
                    SetMenuErrorItem itemerr = new SetMenuErrorItem();
                    itemerr.GroupName = model.Index;
                    itemerr.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + index + msgError;

                    itemErr = new SetMenuImportResultItem();
                    itemErr.Name = model.Name;
                    itemErr.ListFailStoreName.Add("");
                    itemErr.ErrorItems.Add(itemerr);
                    importItems.Add(itemErr);
                }
            }

            //try
            //{
            InteProductApiModels paraBody = new InteProductApiModels();
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.ListOrgID = ListOrgID;
            paraBody.ListProduct = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteImportProduct, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                //=====
                SetMenuImportResultItem importItem = new SetMenuImportResultItem();
                //importItem.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have been") + " [" + (listData.Count - listError.Count) + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("row(s) import Successful") + "<strong>";
                importItem.Name = "<strong style=\"color: #d9534f;\">" + "[" + (listData.Count - listError.Count) + "]" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(" row(s) have been imported successful") + "<strong>";
                importItems.Insert(0, importItem);
                foreach (ImportResult itemError in listError)
                {
                    SetMenuErrorItem item = new SetMenuErrorItem();
                    item.GroupName = itemError.Index;
                    item.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index + "<br/>" + itemError.Error;

                    importItem = new SetMenuImportResultItem();
                    importItem.Name = itemError.Property;
                    importItem.ListFailStoreName.Add(itemError.StoreName);
                    importItem.ErrorItems.Add(item);
                    importItems.Add(importItem);
                }
                if (importItems.Count == 0)
                {
                    importItem = new SetMenuImportResultItem();
                    importItem.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SetMenu");
                    importItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Set Menu Successful"));
                    importItems.Add(importItem);
                }
                //=====
                //importItem = new SetMenuImportResultItem();
                ////importItem.Name = "<strong style=\"color: #d9534f;\">Have been [" + (listData.Count - listError.Count) + "] row(s) import Successful<strong>";
                ////importItem.ListSuccessStoreName.Add("Have been [" + (listData.Count - listError.Count) + "] row(s) import Successful");
                //importItem.Name = "<strong style=\"color: #d9534f;\">[" + (importItems.Count) + "]" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("rows import successfully") + "<strong>";
                //importItems.Insert(0, importItem);
                //=====End
            }
            return importItems;
        }

        public List<DishImportResultItem> ImportDish(string filePath, FileInfo[] listFileInfo, out int totalRowExel, List<string> ListOrgID, ref string msg)
        {
            totalRowExel = 0;
            List<DishImportResultItem> importItems = new List<DishImportResultItem>();

            DataTable dtDishMerchant = ReadExcelFile(filePath, "Dish Merchant");
            DataTable dtDishStore = ReadExcelFile(filePath, "Dish Store");
            DataTable dtTab = ReadExcelFile(filePath, "Tabs");
            DataTable dtModifier = ReadExcelFile(filePath, "Modifier");

            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBInventoryDishInte.xlsx";
            DataTable dtDishMerchantTmp = ReadExcelFile(tmpExcelPath, "Dish Merchant");
            DataTable dtDishStoreTmp = ReadExcelFile(tmpExcelPath, "Dish Store");
            DataTable dtTabTmp = ReadExcelFile(tmpExcelPath, "Tabs");
            DataTable dtModifierTmp = ReadExcelFile(tmpExcelPath, "Modifier");

            if (dtDishMerchant.Columns.Count != dtDishMerchantTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            if (dtDishStore.Columns.Count != dtDishStoreTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            if (dtTab.Columns.Count != dtTabTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            if (dtModifier.Columns.Count != dtModifierTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }

            //List<DishImportItem> dishes = GetListObject<DishImportItem>(dtDishMerchant);
            //List<DishGroupImportItem> groups = GetListObject<DishGroupImportItem>(dtTab);
            //List<DishModifierImportItem> modifiers = GetListObject<DishModifierImportItem>(dtModifier);
            //// validate tab Dish, 
            //ValidateRowDish(ref dishes);
            //ValidateRowGroupDish(ref groups);
            //ValidateRowModifier(ref modifiers);

            List<InteProductModels> listData = new List<InteProductModels>();

            DishImportResultItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            foreach (DataRow item in dtDishMerchant.Rows)
            {
                flagInsert = true;
                msgError = "";

                if (item[0].ToString().Equals(""))
                    continue;
                int index = int.Parse(item[0].ToString());

                string ImageUrl = "";
                if (!string.IsNullOrEmpty(item[4].ToString()))
                {
                    FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[4].ToString().ToLower());
                    if (file != null)
                    {
                        if (file.Length > Commons._MaxSizeFileUploadImg)
                        {
                            flagInsert = false;
                            msgError = Commons._MsgAllowedSizeImg + "<br/>";
                        }
                        else
                        {
                            ImageUrl = Guid.NewGuid() + file.Extension;
                            byte[] photoByte = null;
                            photoByte = System.IO.File.ReadAllBytes(file.FullName);
                            //19/01/2018
                            //photoByte = file.ReadFully();
                            if (!string.IsNullOrEmpty(ImageUrl) && photoByte != null)
                            {
                                var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", System.Web.HttpContext.Current.Server.MapPath(@"\")));
                                var path = string.Format("{0}{1}", originalDirectory, ImageUrl);
                                MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                                ms.Write(photoByte, 0, photoByte.Length);
                                System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);
                                ImageHelper.Me.SaveCroppedImage(imageTmp, path, ImageUrl, ref photoByte);
                                FTP.Upload(ImageUrl, photoByte);
                                ImageHelper.Me.TryDeleteImageUpdated(path);
                            }
                        }
                    }
                }

                List<InteProductItemOnStore> ListProductOnStore = new List<InteProductItemOnStore>();
                DataRow[] ProductOnStoreRow = dtDishStore.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Merchant Index") + "] = " + index + "");
                foreach (var gProductOnStoreRow in ProductOnStoreRow)
                {
                    if (gProductOnStoreRow[0].ToString().Equals(""))
                        continue;

                    int DishStoreIndex = int.Parse(gProductOnStoreRow[1].ToString());
                    var PriceOnStore = new InteProductPriceModels();
                    PriceOnStore.DefaultPrice = string.IsNullOrEmpty(gProductOnStoreRow[8].ToString()) ? 0 : double.Parse(gProductOnStoreRow[8].ToString());
                    PriceOnStore.SeasonPrice = string.IsNullOrEmpty(gProductOnStoreRow[9].ToString()) ? 0 : double.Parse(gProductOnStoreRow[9].ToString());
                    PriceOnStore.SeasonPriceName = gProductOnStoreRow[10].ToString();

                    string msgItem = "";
                    DateTime ExpiredDate = DateTime.Now;
                    string sExpiredDate = gProductOnStoreRow[18].ToString();
                    if (!sExpiredDate.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("unlimited")) && !sExpiredDate.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("never")) && !string.IsNullOrEmpty(sExpiredDate.Trim()))
                    {
                        ExpiredDate = DateTimeHelper.GetDateImport(sExpiredDate, ref msgItem);
                        if (!msgItem.Equals(""))
                        {
                            flagInsert = false;
                            msgError = msgItem;
                        }
                    }
                    else
                        ExpiredDate = Commons._ExpiredDate;

                    InteProductItemOnStore itemProOnStore = new InteProductItemOnStore
                    {
                        OffSet = int.Parse(gProductOnStoreRow[1].ToString()),
                        StoreName = gProductOnStoreRow[2].ToString(),
                        Sequence = string.IsNullOrEmpty(gProductOnStoreRow[3].ToString()) ? 1 : int.Parse(gProductOnStoreRow[3].ToString()),
                        IsActive = string.IsNullOrEmpty(gProductOnStoreRow[4].ToString()) ? true : bool.Parse(gProductOnStoreRow[4].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,
                        ColorCode = gProductOnStoreRow[5].ToString(),
                        KitchenDisplayName = gProductOnStoreRow[6].ToString(),
                        PrintOutName = gProductOnStoreRow[7].ToString(),
                        PriceOnStore = PriceOnStore,
                        Cost = string.IsNullOrEmpty(gProductOnStoreRow[11].ToString()) ? 0 : double.Parse(gProductOnStoreRow[11].ToString()),
                        Unit = string.IsNullOrEmpty(gProductOnStoreRow[12].ToString()) ? 0 : int.Parse(gProductOnStoreRow[12].ToString()),
                        Quantity = string.IsNullOrEmpty(gProductOnStoreRow[13].ToString()) ? 0 : double.Parse(gProductOnStoreRow[13].ToString()),

                        IsCheckStock = string.IsNullOrEmpty(gProductOnStoreRow[14].ToString()) ? false :
                            bool.Parse(gProductOnStoreRow[14].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        Limit = string.IsNullOrEmpty(gProductOnStoreRow[15].ToString()) ? 0 : int.Parse(gProductOnStoreRow[15].ToString()),
                        CategoryName = gProductOnStoreRow[16].ToString(),

                        IsAllowOpenPrice = string.IsNullOrEmpty(gProductOnStoreRow[17].ToString()) ? false :
                            bool.Parse(gProductOnStoreRow[17].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        ExpiredDate = ExpiredDate,

                        Measure = string.IsNullOrEmpty(gProductOnStoreRow[19].ToString()) ? "Dish" : gProductOnStoreRow[19].ToString(),

                        IsPrintOnCheck = string.IsNullOrEmpty(gProductOnStoreRow[20].ToString()) ? true :
                            bool.Parse(gProductOnStoreRow[20].ToString().ToLower().Equals("yes").ToString()) ? true : false,

                        HasServiceCharge = true,
                        ServiceChargeValue = string.IsNullOrEmpty(gProductOnStoreRow[21].ToString()) ? -1 : double.Parse(gProductOnStoreRow[21].ToString()),

                        IsAllowDiscount = string.IsNullOrEmpty(gProductOnStoreRow[22].ToString()) ? true :
                            bool.Parse(gProductOnStoreRow[22].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        IsForce = string.IsNullOrEmpty(gProductOnStoreRow[23].ToString()) ? false :
                            bool.Parse(gProductOnStoreRow[23].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        IsOptional = string.IsNullOrEmpty(gProductOnStoreRow[24].ToString()) ? false :
                            bool.Parse(gProductOnStoreRow[24].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        DefaultState = string.IsNullOrEmpty(gProductOnStoreRow[27].ToString()) ? (byte)Commons.EItemState.PendingStatus :
                                     gProductOnStoreRow[27].ToString().ToLower().Equals("pending") ? (byte)Commons.EItemState.PendingStatus :
                                     gProductOnStoreRow[27].ToString().ToLower().Equals("ready") ? (byte)Commons.EItemState.CompleteStatus :
                                     (byte)Commons.EItemState.ServedStatus,

                        IsComingSoon = string.IsNullOrEmpty(gProductOnStoreRow[28].ToString()) ? false :
                            bool.Parse(gProductOnStoreRow[28].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        Info = gProductOnStoreRow[29].ToString(),

                        IsShowMessage = string.IsNullOrEmpty(gProductOnStoreRow[30].ToString()) ? true :
                            bool.Parse(gProductOnStoreRow[30].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,
                        TaxName = gProductOnStoreRow[33].ToString(),
                    };

                    ///Kiosk | POS Availability
                    string sKiosk = gProductOnStoreRow[25].ToString();
                    if (!string.IsNullOrEmpty(sKiosk))
                    {
                        foreach (var itemKiosk in sKiosk.Split(';'))
                        {
                            if (!itemKiosk.ToLower().Equals("all"))
                            {
                                itemProOnStore.ListProductSeason.Add(new ProductSeasonDTO
                                {
                                    SeasonName = itemKiosk,
                                    IsPOS = false
                                });
                            }
                        }
                    }
                    string sPOS = gProductOnStoreRow[26].ToString();
                    if (!string.IsNullOrEmpty(sPOS))
                    {
                        foreach (var itemPOS in sPOS.Split(';'))
                        {
                            if (!itemPOS.ToLower().Equals("all"))
                            {
                                itemProOnStore.ListProductSeason.Add(new ProductSeasonDTO
                                {
                                    SeasonName = itemPOS,
                                    IsPOS = true
                                });
                            }
                        }
                    }

                    ///PrInte
                    if (!string.IsNullOrEmpty(gProductOnStoreRow[31].ToString()))
                    {
                        foreach (var itemPrInte in gProductOnStoreRow[31].ToString().Split(';'))
                        {
                            itemProOnStore.ListProductPrinter.Add(new PrinterOnProductModels
                            {
                                PrinterName = itemPrInte,
                                Type = (byte)Commons.ProductPrinterType.Normal
                            });
                        }
                    }
                    //Label Printer
                    if (!string.IsNullOrEmpty(gProductOnStoreRow[32].ToString()))
                    {
                        foreach (var itemLabelPrInte in gProductOnStoreRow[32].ToString().Split(';'))
                        {
                            itemProOnStore.ListProductPrinter.Add(new PrinterOnProductModels
                            {
                                PrinterName = itemLabelPrInte,
                                Type = (byte)Commons.ProductPrinterType.Label
                            });
                        }
                    }
                    //============ Get List Product Item On Store
                    List<InteGroupProductModels> lstGProduct = new List<InteGroupProductModels>();
                    DataRow[] GProducs = dtTab.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Store Index") + "] = " + DishStoreIndex + "");
                    foreach (DataRow gProduct in GProducs)
                    {
                        if (string.IsNullOrEmpty(gProduct[0].ToString()))
                            continue;

                        int tabIndex = int.Parse(gProduct[0].ToString());
                        //========Modifier
                        List<InteProductOnGroupModels> ListProOnGroup = new List<InteProductOnGroupModels>();
                        DataRow[] Modifiers = dtModifier.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index") + "] = " + tabIndex + "");
                        foreach (DataRow RModifier in Modifiers)
                        {
                            if (string.IsNullOrEmpty(RModifier[0].ToString()))
                                continue;

                            InteProductOnGroupModels modifier = new InteProductOnGroupModels()
                            {
                                ProductName = RModifier[1].ToString(),
                                ExtraPrice = string.IsNullOrEmpty(RModifier[2].ToString()) ? 0 : Math.Round(double.Parse(RModifier[2].ToString()), 2),
                                Sequence = string.IsNullOrEmpty(RModifier[3].ToString()) ? 1 : int.Parse(RModifier[3].ToString()),
                            };
                            //Validation
                            if (string.IsNullOrEmpty(RModifier[0].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/> " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index is required");
                            }
                            if (string.IsNullOrEmpty(modifier.ProductName))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Name is required");
                            }
                            if (modifier.ExtraPrice < 0)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Extra Price of") + " [" + modifier.ProductName + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("greater than or equal to 0");
                            }
                            if (modifier.Sequence < 0)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence of") + " [" + modifier.ProductName + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("greater than or equal to 0") + "";
                            }

                            if (flagInsert)
                            {
                                ListProOnGroup.Add(modifier);
                            }
                        }
                        //=======
                        //int modifierType = gProduct[6].ToString().ToLower().Trim().Equals(Commons.ModifierForced.ToLower().ToString())
                        //                        ? (byte)Commons.EModifierType.Forced : (byte)Commons.EModifierType.Optional;
                        int modifierType = (int)Commons.EModifierType.Optional;
                        if (gProduct[6].ToString().ToLower().Trim() == Commons.ModifierForced.ToLower().ToString())
                            modifierType = (int)Commons.EModifierType.Forced;
                        else if (gProduct[6].ToString().ToLower().Trim() == "Additional Dish".ToLower().ToString())
                            modifierType = (int)Commons.EModifierType.AdditionalDish;

                        InteGroupProductModels gPModel = new InteGroupProductModels()
                        {
                            Sequence = string.IsNullOrEmpty(gProduct[2].ToString()) ? 1 : int.Parse(gProduct[2].ToString()),
                            Minimum = 0,
                            Maximum = string.IsNullOrEmpty(gProduct[3].ToString()) ? 1 : int.Parse(gProduct[3].ToString()),
                            Name = gProduct[4].ToString(),
                            Description = gProduct[5].ToString(),
                            GroupType = modifierType,
                            ListProductOnGroup = ListProOnGroup,
                        };

                        //Validation
                        if (string.IsNullOrEmpty(gProduct[0].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[1].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Store Index is required");
                        }
                        if (string.IsNullOrEmpty(gPModel.Name))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[6].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type is required, Force or Optional");
                        }
                        if (gPModel.Sequence < 0)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence larger or equal to 0");
                        }
                        if (gPModel.Maximum < 0)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Quantity larger or equal to 0");
                        }

                        if (flagInsert)
                        {
                            lstGProduct.Add(gPModel);
                        }
                    }

                    itemProOnStore.ListProductGroup = lstGProduct;

                    //Validation
                    if (string.IsNullOrEmpty(gProductOnStoreRow[0].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Merchant Index is required");
                    }
                    if (string.IsNullOrEmpty(gProductOnStoreRow[1].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Store Index is required");
                    }
                    if (string.IsNullOrEmpty(itemProOnStore.StoreName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name is required");
                    }
                    //========
                    if (string.IsNullOrEmpty(gProductOnStoreRow[8].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price is required");
                    }
                    if (PriceOnStore.DefaultPrice < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Default Price  larger or equal to 0");
                    }
                    if (PriceOnStore.SeasonPrice < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Season Price  larger or equal to 0");
                    }
                    if (PriceOnStore.SeasonPrice > 0)
                    {
                        if (string.IsNullOrEmpty(PriceOnStore.SeasonPriceName))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season name is required");
                        }
                    }
                    if (itemProOnStore.Cost < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Cost larger or equal to 0");
                    }
                    if (itemProOnStore.Unit < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Unit larger or equal to 0");
                    }
                    if (itemProOnStore.Quantity < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Quantity larger or equal to 0");
                    }
                    if (itemProOnStore.Limit < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Limit larger or equal to 0");
                    }
                    if (string.IsNullOrEmpty(itemProOnStore.CategoryName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category is required");
                    }
                    if (itemProOnStore.ServiceChargeValue < -1 || itemProOnStore.ServiceChargeValue > 100)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge must between 0 and 100");
                    }
                    if (string.IsNullOrEmpty(gProductOnStoreRow[31].ToString()))
                    {
                        flagInsert = false;
                        //                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Must choose at least 1 printer of store") + " [" + itemProOnStore.StoreName + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("for dish") + " [" + item[1].ToString() + "]";
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Must choose at least 1 printer for dish") + " [" + item[1].ToString() + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("on store") + " [" + itemProOnStore.StoreName + "]";
                    }
                    //=============
                    if (string.IsNullOrEmpty(itemProOnStore.TaxName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax is required");
                    }
                    //==== trongntn 01-06-2016
                    if (ListProductOnStore.Count > 0)
                    {
                        var IsExist = ListProductOnStore.Exists(x => x.OffSet.Equals(itemProOnStore.OffSet));
                        if (IsExist)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Store Index already exist");
                        }
                    }

                    if (flagInsert)
                    {
                        ListProductOnStore.Add(itemProOnStore);
                    }
                }

                InteProductModels model = new InteProductModels
                {
                    Index = index.ToString(),
                    Name = item[1].ToString(),
                    ProductCode = item[2].ToString(),
                    BarCode = item[3].ToString(),
                    ImageURL = ImageUrl,
                    ProductType = (byte)Commons.EProductType.Dish,
                    ListProductOnStore = ListProductOnStore
                };

                if (string.IsNullOrEmpty(item[0].ToString()))
                {
                    flagInsert = false;
                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Merchant Index is required");
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    flagInsert = false;
                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name is required");
                }
                if (string.IsNullOrEmpty(model.ProductCode))
                {
                    flagInsert = false;
                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Code is required");
                }
                //==== trongntn 01-06-2016
                if (listData.Count > 0)
                {
                    var IsExist = listData.Exists(x => x.Index.Equals(model.Index));
                    if (IsExist)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Merchant Index already exist");
                    }
                    if (string.IsNullOrEmpty(model.ProductCode))
                    {
                        IsExist = listData.Exists(x => x.ProductCode.Equals(model.ProductCode));
                        if (IsExist)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Code already exist");
                        }
                    }
                    //if (string.IsNullOrEmpty(model.BarCode))
                    //{
                    //    IsExist = listData.Exists(x => x.BarCode.Equals(model.BarCode));
                    //    if (IsExist)
                    //    {
                    //        flagInsert = false;
                    //        msgError += "<br/>Dish BarCode is exist";
                    //    }
                    //}
                }
                //==========
                if (flagInsert)
                {
                    listData.Add(model);
                }
                else
                {
                    DishErrorItem itemerr = new DishErrorItem();
                    itemerr.GroupName = model.Index;
                    itemerr.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Store Index") + ":" + index + msgError;

                    itemErr = new DishImportResultItem();
                    itemErr.Name = model.Name;
                    itemErr.ListFailStoreName.Add("");
                    itemErr.ErrorItems.Add(itemerr);
                    importItems.Add(itemErr);
                }
            }

            //try
            //{
            InteProductApiModels paraBody = new InteProductApiModels();
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.ListOrgID = ListOrgID;
            paraBody.ListProduct = listData;
            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteImportProduct, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                DishImportResultItem importItem = new DishImportResultItem();
                //importItem.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have been") + " [" + (listData.Count - listError.Count) + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("row(s) import Successful") + "<strong>";
                importItem.Name = "<strong style=\"color: #d9534f;\">" + "[" + (listData.Count - listError.Count) + "]" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(" row(s) have been imported successful") + "<strong>";
                importItems.Insert(0, importItem);
                foreach (ImportResult itemError in listError)
                {
                    DishErrorItem item = new DishErrorItem();
                    item.GroupName = itemError.Index;
                    item.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Store Index") + " : " + itemError.Index + "<br/>" + itemError.Error;

                    importItem = new DishImportResultItem();
                    importItem.Name = itemError.Property;
                    importItem.ListFailStoreName.Add(itemError.StoreName);
                    importItem.ErrorItems.Add(item);
                    importItems.Add(importItem);
                }
                if (importItems.Count == 0)
                {
                    importItem = new DishImportResultItem();
                    importItem.Name = "Dish";
                    importItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Dish Successful"));
                    importItems.Add(importItem);
                }
                //=====
                //importItem = new DishImportResultItem();
                ////importItem.Name = "<strong style=\"color: #d9534f;\">Have been [" + (listData.Count - listError.Count) + "] row(s) import Successful<strong>";
                ////importItem.ListSuccessStoreName.Add("Have been [" + (listData.Count - listError.Count) + "] row(s) import Successful");
                //importItem.Name = "<strong style=\"color: #d9534f;\">[" + (importItems.Count) + "] "+_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("rows import successfully") +"<strong>";
                //importItems.Insert(0, importItem);
                //=====End
            }
            return importItems;
        }

        public List<SetMenuImportResultItem> ImportModifier(string filePath, FileInfo[] listFileInfo, out int totalRowExel, List<string> ListOrgID, ref string msg)
        {
            totalRowExel = 0;
            List<SetMenuImportResultItem> importItems = new List<SetMenuImportResultItem>();

            DataTable dtModifierMerchant = ReadExcelFile(@filePath, "Modifier Merchant");
            DataTable dtModifierStore = ReadExcelFile(@filePath, "Modifier Store");

            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBInventoryModifierInte.xlsx";
            DataTable dtModifierMerchantTmp = ReadExcelFile(@tmpExcelPath, "Modifier Merchant");
            DataTable dtModifierStoreTmp = ReadExcelFile(@tmpExcelPath, "Modifier Store");

            if (dtModifierMerchant.Columns.Count != dtModifierMerchantTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            if (dtModifierStore.Columns.Count != dtModifierStoreTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }

            //List<SetMenuImportItem> lstSetMenu = GetListObject<SetMenuImportItem>(dtModifierMerchant);
            // validate tab Set Menu, 
            //ValidateRowSetMenu(ref lstSetMenu);

            List<InteProductModels> listData = new List<InteProductModels>();
            SetMenuImportResultItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";


            foreach (DataRow item in dtModifierMerchant.Rows)
            {
                flagInsert = true;
                msgError = "";

                if (item[0].ToString().Equals(""))
                    continue;
                int index = int.Parse(item[0].ToString());
                string ImageUrl = "";
                if (!string.IsNullOrEmpty(item[4].ToString()))
                {
                    FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[4].ToString().ToLower());
                    if (file != null)
                    {
                        if (file.Length > Commons._MaxSizeFileUploadImg)
                        {
                            flagInsert = false;
                            msgError = Commons._MsgAllowedSizeImg + "<br/>";
                        }
                        else
                        {
                            ImageUrl = Guid.NewGuid() + file.Extension;
                            byte[] photoByte = null;
                            photoByte = System.IO.File.ReadAllBytes(file.FullName);
                            //19/01/2018
                            //photoByte = file.ReadFully();
                            if (!string.IsNullOrEmpty(ImageUrl) && photoByte != null)
                            {
                                var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", System.Web.HttpContext.Current.Server.MapPath(@"\")));
                                var path = string.Format("{0}{1}", originalDirectory, ImageUrl);
                                MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                                ms.Write(photoByte, 0, photoByte.Length);
                                System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);
                                ImageHelper.Me.SaveCroppedImage(imageTmp, path, ImageUrl, ref photoByte);
                                FTP.Upload(ImageUrl, photoByte);
                                ImageHelper.Me.TryDeleteImageUpdated(path);
                            }
                        }
                    }
                }

                List<InteProductItemOnStore> ListProductOnStore = new List<InteProductItemOnStore>();
                DataRow[] ProductOnStoreRow = dtModifierStore.Select("[Modifier Merchant Index] = " + index + "");
                foreach (DataRow gProductOnStoreRow in ProductOnStoreRow)
                {
                    if (gProductOnStoreRow[0].ToString().Equals(""))
                        continue;

                    int ModifierMerchantIndex = int.Parse(gProductOnStoreRow[0].ToString());

                    var PriceOnStore = new InteProductPriceModels();
                    PriceOnStore.DefaultPrice = string.IsNullOrEmpty(gProductOnStoreRow[7].ToString()) ? 0 : double.Parse(gProductOnStoreRow[7].ToString());
                    PriceOnStore.SeasonPrice = string.IsNullOrEmpty(gProductOnStoreRow[8].ToString()) ? 0 : double.Parse(gProductOnStoreRow[8].ToString());
                    PriceOnStore.SeasonPriceName = gProductOnStoreRow[9].ToString();

                    InteProductItemOnStore itemProOnStore = new InteProductItemOnStore()
                    {
                        OffSet = int.Parse(gProductOnStoreRow[1].ToString()),
                        StoreName = gProductOnStoreRow[2].ToString(),
                        Sequence = string.IsNullOrEmpty(gProductOnStoreRow[3].ToString()) ? 1 : int.Parse(gProductOnStoreRow[3].ToString()),
                        IsActive = string.IsNullOrEmpty(gProductOnStoreRow[4].ToString()) ? true
                        : bool.Parse(gProductOnStoreRow[4].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes").ToLower()).ToString()) ? true : false,
                        KitchenDisplayName = gProductOnStoreRow[5].ToString(),
                        PrintOutName = gProductOnStoreRow[6].ToString(),
                        PriceOnStore = PriceOnStore,
                        Quantity = string.IsNullOrEmpty(gProductOnStoreRow[10].ToString()) ? 0 : double.Parse(gProductOnStoreRow[10].ToString()),

                        IsCheckStock = string.IsNullOrEmpty(gProductOnStoreRow[11].ToString()) ? false :
                        bool.Parse(gProductOnStoreRow[11].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes").ToLower()).ToString()) ? true : false,

                        Limit = string.IsNullOrEmpty(gProductOnStoreRow[12].ToString()) ? 0 : int.Parse(gProductOnStoreRow[12].ToString()),
                        CategoryName = gProductOnStoreRow[13].ToString(),

                        IsAllowOpenPrice = string.IsNullOrEmpty(gProductOnStoreRow[14].ToString()) ? false :
                        bool.Parse(gProductOnStoreRow[14].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes").ToLower()).ToString()) ? true : false,

                        IsPrintOnCheck = string.IsNullOrEmpty(gProductOnStoreRow[15].ToString()) ? true :
                        bool.Parse(gProductOnStoreRow[15].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes").ToLower()).ToString()) ? true : false,

                        IsAllowDiscount = string.IsNullOrEmpty(gProductOnStoreRow[16].ToString()) ? true :
                        bool.Parse(gProductOnStoreRow[16].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes").ToLower()).ToString()) ? true : false,

                        HasServiceCharge = true,
                        ServiceChargeValue = 0,

                        ExpiredDate = Commons._ExpiredDate,
                        IsForce = true,
                        IsOptional = true,
                        ColorCode = "#ffffff",
                        Measure = "$",

                        ListProductPrinter = new List<PrinterOnProductModels>(),
                        ListProductSeason = new List<ProductSeasonDTO>()
                    };

                    //Validation
                    if (string.IsNullOrEmpty(gProductOnStoreRow[0].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Merchant Index is required"));
                    }
                    if (string.IsNullOrEmpty(gProductOnStoreRow[1].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Store Index is required"));
                    }
                    if (string.IsNullOrEmpty(itemProOnStore.StoreName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name is required"));
                    }
                    if (string.IsNullOrEmpty(gProductOnStoreRow[7].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Price is required"));
                    }
                    if (PriceOnStore.DefaultPrice < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value default price larger or equal to 0"));
                    }
                    if (PriceOnStore.SeasonPrice < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value season price larger or equal to 0"));
                    }
                    if (PriceOnStore.SeasonPrice > 0)
                    {
                        if (string.IsNullOrEmpty(PriceOnStore.SeasonPriceName))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season name is required"));
                        }
                    }
                    if (itemProOnStore.Quantity < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value quantity larger or equal to 0"));
                    }
                    if (itemProOnStore.Limit < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value limit larger or equal to 0"));
                    }
                    if (string.IsNullOrEmpty(itemProOnStore.CategoryName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("please re-enter Category for this modifier"));
                    }

                    //==== trongntn 01-06-2016
                    if (ListProductOnStore.Count > 0)
                    {
                        var IsExist = ListProductOnStore.Exists(x => x.OffSet.Equals(itemProOnStore.OffSet));
                        if (IsExist)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Store Index already exist"));
                        }
                    }

                    if (flagInsert)
                    {
                        ListProductOnStore.Add(itemProOnStore);
                    }
                }
                //========
                InteProductModels model = new InteProductModels
                {
                    Index = index.ToString(),
                    Name = item[1].ToString(),
                    ProductCode = item[2].ToString(),
                    BarCode = item[3].ToString(),
                    ImageURL = ImageUrl,
                    ProductType = (int)Commons.EProductType.Modifier,
                    ListProductOnStore = ListProductOnStore
                };
                //============
                if (string.IsNullOrEmpty(model.Index))
                {
                    flagInsert = false;
                    msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Merchant Index is required"));
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    flagInsert = false;
                    msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Name is required"));
                }
                if (string.IsNullOrEmpty(model.ProductCode))
                {
                    flagInsert = false;
                    msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Code is required"));
                }
                //==== trongntn 01-06-2016
                if (listData.Count > 0)
                {
                    var IsExist = listData.Exists(x => x.Index.Equals(model.Index));
                    if (IsExist)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Merchant Index already exist"));
                    }
                    if (string.IsNullOrEmpty(model.ProductCode))
                    {
                        IsExist = listData.Exists(x => x.ProductCode.Equals(model.ProductCode));
                        if (IsExist)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Code already exist"));
                        }
                    }
                    //if (string.IsNullOrEmpty(model.BarCode))
                    //{
                    //    IsExist = listData.Exists(x => x.BarCode.Equals(model.BarCode));
                    //    if (IsExist)
                    //    {
                    //        flagInsert = false;
                    //        msgError += "<br/>Modifier BarCode is exist";
                    //    }
                    //}
                }
                //==========
                if (flagInsert)
                {
                    listData.Add(model);
                }
                else
                {
                    SetMenuErrorItem itemerr = new SetMenuErrorItem();
                    itemerr.GroupName = model.Index;
                    itemerr.ErrorMessage = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row")) + ": " + index + msgError;
                    itemErr = new SetMenuImportResultItem();
                    itemErr.Name = model.Name;
                    itemErr.ListFailStoreName.Add("");
                    itemErr.ErrorItems.Add(itemerr);
                    importItems.Add(itemErr);
                }
            }


            InteProductApiModels paraBody = new InteProductApiModels();
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.ListOrgID = ListOrgID;
            paraBody.ListProduct = listData;
            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteImportProduct, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                SetMenuImportResultItem importItem = new SetMenuImportResultItem();
                //importItem.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have been") + " [" + (listData.Count - listError.Count) + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("row(s) import Successful") + "<strong>";
                importItem.Name = "<strong style=\"color: #d9534f;\">" + "[" + (listData.Count - listError.Count) + "]" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(" row(s) have been imported successful") + "<strong>";
                importItems.Insert(0, importItem);

                foreach (ImportResult itemError in listError)
                {
                    SetMenuErrorItem item = new SetMenuErrorItem();
                    item.GroupName = itemError.Index;
                    item.ErrorMessage = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row")) + ": " + itemError.Index + "<br/>" + itemError.Error;

                    importItem = new SetMenuImportResultItem();
                    importItem.Name = itemError.Property;
                    importItem.ListFailStoreName.Add(itemError.StoreName);
                    importItem.ErrorItems.Add(item);
                    importItems.Add(importItem);
                }

                if (importItems.Count == 0)
                {
                    importItem = new SetMenuImportResultItem();
                    importItem.Name = "Modifier";
                    importItem.ListSuccessStoreName.Add((_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Modifier Successful")));
                    importItems.Add(importItem);
                }

                //=====
                //importItem = new SetMenuImportResultItem();
                ////importItem.Name = "<strong style=\"color: #d9534f;\">Have been [" + (listData.Count - listError.Count) + "] row(s) import Successful<strong>";
                ////importItem.ListSuccessStoreName.Add("Have been [" + (listData.Count - listError.Count) + "] row(s) import Successful");
                //importItem.Name = "<strong style=\"color: #d9534f;\">[" + (importItems.Count) + "] " + (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("rows import successfully")) + " <strong>";
                //importItems.Insert(0, importItem);
                //=====End
            }
            return importItems;
        }

        /*=====End Import */

        //public void ValidateRowSetMenu(ref List<SetMenuImportItem> lstSetMenu)
        //{
        //    foreach (SetMenuImportItem item in lstSetMenu)
        //    {
        //        bool valid = true;
        //        if (item.SetMenuIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Tab Index must be more than 0");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.Name.Trim()))
        //        {
        //            item.Infor.Errors.Add("Set Menu Name can not be null");
        //            valid = false;
        //        }
        //        if (valid)
        //            item.Infor.IsValidRow = true;
        //        else
        //            item.Infor.IsValidRow = false;
        //    }
        //}
        //public void ValidateRowTabSetMenu(ref List<TabImportItem> lstTab)
        //{
        //    foreach (TabImportItem item in lstTab)
        //    {
        //        bool valid = true;
        //        if (item.TabIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Tab Index must be more than 0");
        //            valid = false;
        //        }
        //        if (item.SetMenuIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Set Menu Index must be more than 0");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.Name.Trim()))
        //        {
        //            item.Infor.Errors.Add("Tab Name can not be null");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.DisplayMessage.Trim()))
        //        {
        //            item.Infor.Errors.Add("Display Message can not be null");
        //            valid = false;
        //        }
        //        if (item.Quantity < 1)
        //        {
        //            item.Infor.Errors.Add("Quantity must be more than 0");
        //            valid = false;
        //        }
        //        if (valid)
        //            item.Infor.IsValidRow = true;
        //        else
        //            item.Infor.IsValidRow = false;
        //    }
        //}
        //public void ValidateRowDishSetMenu(ref List<DishTabImportItem> lstDish)
        //{
        //    foreach (DishTabImportItem item in lstDish)
        //    {
        //        bool valid = true;
        //        if (item.DishIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Dish Index must be more than 0");
        //            valid = false;
        //        }
        //        if (item.TabIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Tab Index must be more than 0");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.Name.Trim()))
        //        {
        //            item.Infor.Errors.Add("Dish Name can not be null");
        //            valid = false;
        //        }
        //        if (valid)
        //            item.Infor.IsValidRow = true;
        //        else
        //            item.Infor.IsValidRow = false;
        //    }
        //}
        //public void ValidateRowDish(ref List<DishImportItem> dishes)
        //{
        //    foreach (DishImportItem item in dishes)
        //    {
        //        bool valid = true;
        //        if (item.DishIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Dish Index must be more than 0");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.Name.Trim()))
        //        {
        //            item.Infor.Errors.Add("Dish Name can not be null");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.DishCode.Trim()))
        //        {
        //            item.Infor.Errors.Add("Dish Code can not be null");
        //            valid = false;
        //        }
        //        if (item.Unit < 0)
        //        {
        //            item.Infor.Errors.Add("Unit must be more than 0");
        //            valid = false;
        //        }
        //        if (item.Cost < 0)
        //        {
        //            item.Infor.Errors.Add("Cost must be more than 0");
        //            valid = false;
        //        }
        //        if (item.DefaultPrice < 0)
        //        {
        //            item.Infor.Errors.Add("Default Price must be more than 0");
        //            valid = false;
        //        }
        //        if (item.Quantity < 0)
        //        {
        //            item.Infor.Errors.Add("Quantity must be more than 0");
        //            valid = false;
        //        }
        //        if (item.Limit < 0)
        //        {
        //            item.Infor.Errors.Add("Limit must be more than 0");
        //            valid = false;
        //        }
        //        if (item.PercentServiceCharge < 0)
        //        {
        //            item.Infor.Errors.Add("Percent Service Charge must be more than 0");
        //            valid = false;
        //        }
        //        if (valid)
        //            item.Infor.IsValidRow = true;
        //        else
        //            item.Infor.IsValidRow = false;
        //    }
        //}
        //public void ValidateRowGroupDish(ref List<DishGroupImportItem> groups)
        //{
        //    foreach (DishGroupImportItem item in groups)
        //    {
        //        bool valid = true;
        //        if (item.GroupIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Tab Index must be more than 0");
        //            valid = false;
        //        }
        //        if (item.DishIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Dish Index must be more than 0");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.Name.Trim()))
        //        {
        //            item.Infor.Errors.Add("Tab Name can not be null");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.DisplayMessage.Trim()))
        //        {
        //            item.Infor.Errors.Add("Display Message can not be null");
        //            valid = false;
        //        }
        //        if (item.Quantity < 1)
        //        {
        //            item.Infor.Errors.Add("Quantity must be more than 0");
        //            valid = false;
        //        }
        //        if (item.Seq < 0)
        //        {
        //            item.Infor.Errors.Add("Sequence must be more than 0");
        //            valid = false;
        //        }
        //        if (valid)
        //            item.Infor.IsValidRow = true;
        //        else
        //            item.Infor.IsValidRow = false;
        //    }
        //}
        //public void ValidateRowModifier(ref List<DishModifierImportItem> modifiers)
        //{
        //    foreach (DishModifierImportItem item in modifiers)
        //    {
        //        bool valid = true;
        //        if (item.ModifierIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Modifier Index must be more than 0");
        //            valid = false;
        //        }
        //        if (item.TabIndex < 1)
        //        {
        //            item.Infor.Errors.Add("Tab Index must be more than 0");
        //            valid = false;
        //        }
        //        if (string.IsNullOrEmpty(item.Name.Trim()))
        //        {
        //            item.Infor.Errors.Add("Modifier Name can not be null");
        //            valid = false;
        //        }
        //        if (item.Seq < 0)
        //        {
        //            item.Infor.Errors.Add("Sequence must be more than 0");
        //            valid = false;
        //        }
        //        if (item.ExtraPrice < 0)
        //        {
        //            item.Infor.Errors.Add("Extra Price must be more than 0");
        //            valid = false;
        //        }
        //        if (valid)
        //            item.Infor.IsValidRow = true;
        //        else
        //            item.Infor.IsValidRow = false;
        //    }
        //}

        #region ForReport
        //public List<RFilterCategoryModel> GetAllSetMenuForReport(CategoryApiRequestModel request)
        //{
        //    var lstData = new List<RFilterCategoryModel>();
        //    try
        //    {
        //        var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteGetSetMenuFilterForWeb, null, request);
        //        if (result.Success)
        //        {
        //            dynamic data = result.Data;
        //            var ListCate = data["ListCategories"];
        //            foreach (var item in ListCate)
        //            {
        //                lstData.Add(new RFilterCategoryModel
        //                {
        //                    Id = item["Id"],
        //                    Name = item["Name"],
        //                    StoreName = item["StoreName"],

        //                });
        //            }
        //        }
        //        lstData = lstData.OrderBy(oo => oo.StoreName).ToList();
        //        return lstData;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error("GetCate Report: " + e);
        //        return lstData;
        //    }
        //}
        #endregion


        public List<DishImportResultItem> ExtendProduct(InteProductViewModels model, ref string msg)
        {
            List<DishImportResultItem> importItems = new List<DishImportResultItem>();
            try
            {
                InteProductApiModels paraBody = new InteProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.ListProductID = model.ListItem.Where(o => o.IsSelected).Select(s => s.ID).ToList();
                paraBody.ListStoreIDExtendTo = model.ListStoreID;
                paraBody.StoreID = model.StoreID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ProductExtend, null, paraBody);

                if (result != null)
                {
                    dynamic data = result.Data;
                    var lstC = data["ListProperty"];
                    var lstContent = JsonConvert.SerializeObject(lstC);
                    var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);
                    msg = result.Message;
                    DishImportResultItem importItem = new DishImportResultItem();
                    //importItem.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have been") + " [" + (listData.Count - listError.Count) + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("row(s) import Successful") + "<strong>";
                    if (result.Success)
                    {
                        importItem.Name = "<strong style=\"color: #d9534f;\">" + "[" + (model.ListItem.Count - listError.Count) + "]" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(" row(s) have been extended successfully") + "<strong>";
                        importItems.Insert(0, importItem);
                    }

                    foreach (ImportResult itemError in listError)
                    {
                        DishErrorItem item = new DishErrorItem();
                        item.GroupName = itemError.Index;
                        item.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Index") + " : " + itemError.Index + "<br/>" + itemError.Error;

                        importItem = new DishImportResultItem();
                        importItem.Name = itemError.Property;
                        importItem.ListFailStoreName.Add(itemError.StoreName);
                        importItem.ErrorItems.Add(item);
                        importItems.Add(importItem);
                    }
                    if (importItems.Count == 0)
                    {
                        importItem = new DishImportResultItem();
                        importItem.Name = "Product";
                        importItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Extend Product Successful"));
                        importItems.Add(importItem);
                    }
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("ExtendProduct :", e);
            }
            return importItems;
        }
    }

    /*Set*/
    public class ExportSetStore
    {
        public int SetMerchantIndex { get; set; }
        public int SetStoreIndex { get; set; }
        public string StoreName { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public string KitchenDisplayName { get; set; }
        public string PrintOutName { get; set; }
        public double Price { get; set; }
        public double SeasonalPrice { get; set; }
        public string Season { get; set; }
        public double Cost { get; set; }
        public double Quantity { get; set; }
        public bool IsCheckStock { get; set; }
        public double Limit { get; set; }
        public string Category { get; set; }
        public bool IsOpenPrice { get; set; }
        public DateTime ExpiredDate { get; set; }
        public bool IsPrintOnCheck { get; set; }
        public double SVC { get; set; }
        public bool IsAllowDiscount { get; set; }
        public string KioskAvailability { get; set; }
        public bool IsComingSoon { get; set; }
        public string SetInformation { get; set; }
        public bool IsShowKioskMessage { get; set; }
        public bool IsShowInReservation { get; set; }
        public string TaxName { get; set; }
        public bool IsPromotion { get; set; }
    }
    public class ExportSetTabDish
    {
        public int TabIndex { get; set; }
        public int SetStoreIndex { get; set; }
        public int Seq { get; set; }
        public double Quantity { get; set; }
        public string TabName { get; set; }
        public string DisplayMessage { get; set; }
    }
    public class ExportSetDish
    {
        public int TabIndex { get; set; }
        public string DishName { get; set; }
        public double ExtraPrice { get; set; }
        public int Sequence { get; set; }
    }

    /*Dish*/
    public class ExportDishStore
    {
        public int DishMerchantIndex { get; set; }
        public int DishStoreIndex { get; set; }
        public string StoreName { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public string Color { get; set; }
        public string KitchenDisplayName { get; set; }
        public string PrintOutName { get; set; }
        public double Price { get; set; }
        public double SeasonalPrice { get; set; }
        public string Season { get; set; }
        public double Cost { get; set; }
        public double Unit { get; set; }
        public double Quantity { get; set; }
        public bool IsCheckStock { get; set; }
        public double Limit { get; set; }
        public string Category { get; set; }
        public bool IsOpenPrice { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string UnitofMeasurement { get; set; }
        public bool IsPrintOnCheck { get; set; }
        public double SVC { get; set; }
        public bool IsAllowDiscount { get; set; }
        public bool IsForceModifierPopup { get; set; }
        public bool IsOptionalModifierPopup { get; set; }
        public string KioskAvailability { get; set; }
        public string POSAvailability { get; set; }
        public int DefaultStatus { get; set; }
        public bool IsComingSoon { get; set; }
        public string DishInformation { get; set; }
        public bool IsShowKioskMessage { get; set; }
        public string PrInte { get; set; }
        public string TaxName { get; set; }
        public string LabelPrinter { get; set; }
    }
    public class ExportDishTabModifier
    {
        public int TabIndex { get; set; }
        public int DishStoreIndex { get; set; }
        public int Seq { get; set; }
        public double Quantity { get; set; }
        public string TabName { get; set; }
        public string DisplayMessage { get; set; }
        public int GroupType { get; set; }
    }
    public class ExportDishModifier
    {
        public int TabIndex { get; set; }
        public string ModifierName { get; set; }
        public double ExtraPrice { get; set; }
        public int Sequence { get; set; }
    }

    /*Modifier*/
    public class ExportModifierStore
    {
        public int ModifierMerchantIndex { get; set; }
        public int ModifierStoreIndex { get; set; }
        public string StoreName { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public string KitchenDisplayName { get; set; }
        public string PrintOutName { get; set; }
        public double Price { get; set; }
        public double SeasonalPrice { get; set; }
        public string Season { get; set; }
        public double Quantity { get; set; }
        public bool IsCheckStock { get; set; }
        public double Limit { get; set; }
        public string Category { get; set; }
        public bool IsOpenPrice { get; set; }
        public bool IsPrintOnCheck { get; set; }
        public bool IsAllowDiscount { get; set; }
    }
}
