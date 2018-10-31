using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Utilities;
//using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product
{
    public class ProductFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public ProductFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<ProductModels> GetListProduct(string StoreId = null, int TypeID = -1, List<string> ListOrganizationId = null)
        {
            List<ProductModels> listData = new List<ProductModels>();
            try
            {
                ProductApiModels paraBody = new ProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductTypeID = TypeID.ToString();
                paraBody.StoreID = StoreId;
                paraBody.IsShowInReservation = false;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetProduct, null, paraBody);
                dynamic data = result.Data;
                var lstData = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstData);
                listData = JsonConvert.DeserializeObject<List<ProductModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listData = listData.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();
                listData = listData.OrderBy(oo => oo.Name).ToList();
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Product_GetList: " + e);
                return listData;
            }
        }

        public ProductModels GetProductDetail(string ProductId)
        {
            ProductModels model = new ProductModels();
            try
            {
                ProductApiModels paraBody = new ProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ID = ProductId;
                paraBody.IsShowInReservation = false;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetProductDetail, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ProductDTO"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                model = JsonConvert.DeserializeObject<ProductModels>(lstContent);
                return model;
            }
            catch (Exception e)
            {
                _logger.Error("Product_GetList: " + e);
                return model;
            }
        }

        /**/
        public double GetExtraPrice(string ProductId, int ProductType)
        {
            //ProductModels model = new ProductModels();
            //try
            //{
            //    ProductApiModels paraBody = new ProductApiModels();
            //    paraBody.AppKey = Commons.AppKey;
            //    paraBody.AppSecret = Commons.AppSecret;
            //    paraBody.CreatedUser = Commons.CreateUser;
            //    paraBody.ID = ProductId;
            //    paraBody.IsShowInReservation = false;

            //    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetProductDetail, null, paraBody);
            //    dynamic data = result.Data;
            //    var lstC = data["ProductDTO"];
            //    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
            //    model = JsonConvert.DeserializeObject<ProductModels>(lstContent);

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

        public bool InsertOrUpdateProduct(ProductModels model, ref string msg)
        {
            try
            {
                ProductApiModels paraBody = new ProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                //=========Price
                model.DefaultPrice = Math.Round(model.ListPrices[0].Price, 2);
                model.SeasonPrice = Math.Round(model.ListPrices[1].Price, 2);
                model.SeasonPriceID = model.ListPrices[1].SeasonID;

                model.ColorCode = "#000000";
                model.PrintOutText = model.PrintOutText ?? string.Empty;
                model.Measure = String.IsNullOrEmpty(model.Measure) ? "$" : model.Measure;
                //===
                paraBody.ProductDTO = model;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditProduct, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                        _logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    msg = result.ToString();
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Product_InsertOrUpdate: " + e);
                msg = e.ToString();
                return false;
            }
        }

        public bool DeleteProduct(string ID, ref string msg)
        {
            try
            {
                ProductApiModels paraBody = new ProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.ID = ID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteProduct, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                        _logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Product_Delete: " + e);
                msg = e.ToString();
                return false;
            }
        }

        /*===== Export */
        public StatusResponse ExportSetMenu(ref IXLWorksheet wsSetMenu, ref IXLWorksheet wsTab, ref IXLWorksheet wsDish, List<string> lstStore)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<ProductModels> listData = new List<ProductModels>();

                ProductApiModels paraBody = new ProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;
                paraBody.ProductType = (byte)Commons.EProductType.SetMenu;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportProduct, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<ProductModels>>(lstContent);
                listData = listData.OrderBy(o => o.StoreName).ToList();

                int row = 1;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SetMenu Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Bar Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Check Stock"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow Discount"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seasonal Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cost"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Limit"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Open Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print On Check"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Expired Date"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC value"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show Reservation & Queue Module"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image Url"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax")
                };
                for (int i = 1; i <= listSetMenuHeader.Length; i++)
                    wsSetMenu.Cell(row, i).Value = listSetMenuHeader[i - 1];
                int cols = listSetMenuHeader.Length;
                //Item
                row = 2;
                int countIndex = 1;
                int countIndexTab = 1;
                int countIndexDish = 1;

                List<ExportItemTab> lstTab = new List<ExportItemTab>();
                List<ExportItemDish> lstDish = new List<ExportItemDish>();

                foreach (var item in listData)
                {
                    wsSetMenu.Cell("A" + row).Value = countIndex;
                    wsSetMenu.Cell("B" + row).Value = item.OrderByIndex;
                    wsSetMenu.Cell("C" + row).Value = item.Name;
                    wsSetMenu.Cell("D" + row).Value = item.ProductCode;
                    wsSetMenu.Cell("E" + row).Value = item.BarCode;

                    wsSetMenu.Cell("F" + row).Value = item.IsCheckedStock ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("G" + row).Value = item.IsAllowedDiscount ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("H" + row).Value = item.Description;
                    wsSetMenu.Cell("I" + row).Value = item.IsShowMessage ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("J" + row).Value = item.PrintOutText;
                    wsSetMenu.Cell("K" + row).Value = item.IsShowMessage ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("L" + row).Value = item.DefaultPrice;
                    wsSetMenu.Cell("M" + row).Value = item.SeasonPrice;
                    wsSetMenu.Cell("N" + row).Value = item.ProductSeason;
                    wsSetMenu.Cell("O" + row).Value = item.Cost;
                    wsSetMenu.Cell("P" + row).Value = item.Quantity;
                    wsSetMenu.Cell("Q" + row).Value = item.Limit;
                    wsSetMenu.Cell("R" + row).Value = item.CategoryName;
                    wsSetMenu.Cell("S" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("T" + row).Value = item.IsAllowedOpenPrice ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("U" + row).Value = item.IsPrintedOnCheck ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("V" + row).Value = item.ExpiredDate.Value.Date == Commons._ExpiredDate.Date
                        ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited")
                        : "'" + item.ExpiredDate.Value.ToString("dd/MM/yyyy");
                    wsSetMenu.Cell("W" + row).Value = item.HasServiceCharge ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("X" + row).Value = !item.HasServiceCharge ? 0 : item.ServiceCharge;
                    wsSetMenu.Cell("Y" + row).Value = item.IsShowInReservation ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("Z" + row).Value = item.StoreName;
                    wsSetMenu.Cell("AA" + row).Value = item.ImageURL;
                    wsSetMenu.Cell("AB" + row).Value = item.Tax;
                    //=============================
                    if (item.ListGroup != null)
                    {
                        for (int i = 0; i < item.ListGroup.Count; i++)
                        {

                            ExportItemTab eTab = new ExportItemTab()
                            {
                                DisplayMsg = item.ListGroup[i].Description,
                                Quantity = item.ListGroup[i].Maximum,
                                Seq = item.ListGroup[i].Sequence,
                                SetMenuIndex = countIndex,
                                SetMenuName = item.Name,
                                TabName = item.ListGroup[i].Name,
                                TabIndex = countIndexTab
                            };
                            if (item.ListGroup[i].ListProductOnGroup != null)
                            {
                                for (int j = 0; j < item.ListGroup[i].ListProductOnGroup.Count; j++)
                                {
                                    ExportItemDish eDish = new ExportItemDish()
                                    {
                                        DishIndex = countIndexDish,
                                        DishName = item.ListGroup[i].ListProductOnGroup[j].ProductName,
                                        ExtraPrice = item.ListGroup[i].ListProductOnGroup[j].ExtraPrice,
                                        Printer = "",
                                        Seq = item.ListGroup[i].ListProductOnGroup[j].Sequence,
                                        TabIndex = countIndexTab
                                    };
                                    countIndexDish++;
                                    lstDish.Add(eDish);
                                }
                            }
                            countIndexTab++;
                            lstTab.Add(eTab);
                        }
                    }
                    row++;
                    countIndex++;
                }
                FormatExcelExport(wsSetMenu, row, cols);
                //=========
                row = 1;
                string[] listTabHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Display Message"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity")
                };
                for (int i = 1; i <= listTabHeader.Length; i++)
                    wsTab.Cell(row, i).Value = listTabHeader[i - 1];
                cols = listTabHeader.Length;
                row++;
                foreach (var item in lstTab)
                {
                    wsTab.Cell("A" + row).Value = item.TabIndex;
                    wsTab.Cell("B" + row).Value = item.Seq;
                    wsTab.Cell("C" + row).Value = item.SetMenuIndex;
                    wsTab.Cell("D" + row).Value = item.SetMenuName;
                    wsTab.Cell("E" + row).Value = item.TabName;
                    wsTab.Cell("F" + row).Value = item.DisplayMsg;
                    wsTab.Cell("G" + row).Value = item.Quantity;
                    row++;
                }
                FormatExcelExport(wsTab, row, cols);
                //============
                row = 1;
                string[] listDishHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Extra Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Printer")
                };
                for (int i = 1; i <= listDishHeader.Length; i++)
                    wsDish.Cell(row, i).Value = listDishHeader[i - 1];
                cols = listDishHeader.Length;
                row++;
                foreach (var item in lstDish)
                {
                    wsDish.Cell("A" + row).Value = item.DishIndex;
                    wsDish.Cell("B" + row).Value = item.Seq;
                    wsDish.Cell("C" + row).Value = item.TabIndex;
                    wsDish.Cell("D" + row).Value = item.DishName;
                    wsDish.Cell("E" + row).Value = item.ExtraPrice;
                    wsDish.Cell("F" + row).Value = item.Printer;
                    row++;
                }
                //========
                FormatExcelExport(wsDish, row, cols);
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

        public StatusResponse ExportModifier(ref IXLWorksheet wsSetMenu, List<string> lstStore)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<ProductModels> listData = new List<ProductModels>();

                ProductApiModels paraBody = new ProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;
                paraBody.ProductType = (byte)Commons.EProductType.Modifier;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportProduct, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<ProductModels>>(lstContent);
                listData = listData.OrderBy(o => o.StoreName).ToList();

                int row = 1;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Bar Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Check Stock"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Limit"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print On Check"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow to Apply Discount/Promotion"),
                    /*"Service Charge","SC value", */
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image Url")
                };
                for (int i = 1; i <= listSetMenuHeader.Length; i++)
                    wsSetMenu.Cell(row, i).Value = listSetMenuHeader[i - 1];
                int cols = listSetMenuHeader.Length;
                //Item
                row = 2;
                int countIndex = 1;

                foreach (var item in listData)
                {
                    wsSetMenu.Cell("A" + row).Value = countIndex;
                    wsSetMenu.Cell("B" + row).Value = item.OrderByIndex;
                    wsSetMenu.Cell("C" + row).Value = item.Name;
                    wsSetMenu.Cell("D" + row).Value = item.ProductCode;
                    wsSetMenu.Cell("E" + row).Value = item.BarCode;

                    wsSetMenu.Cell("F" + row).Value = item.IsCheckedStock ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("G" + row).Value = item.Description;
                    wsSetMenu.Cell("H" + row).Value = item.IsShowMessage ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("I" + row).Value = item.PrintOutText;
                    wsSetMenu.Cell("J" + row).Value = item.IsShowMessage ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("K" + row).Value = item.Quantity;
                    wsSetMenu.Cell("L" + row).Value = item.DefaultPrice;
                    wsSetMenu.Cell("M" + row).Value = item.IsAllowedOpenPrice ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("N" + row).Value = item.CategoryName;
                    wsSetMenu.Cell("O" + row).Value = item.Limit;
                    wsSetMenu.Cell("P" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("Q" + row).Value = item.IsPrintedOnCheck ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("R" + row).Value = item.IsAllowedDiscount ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    //wsSetMenu.Cell("R" + row).Value = item.HasServiceCharge ? "Yes" : "No";
                    //wsSetMenu.Cell("R" + row).Value = !item.HasServiceCharge ? 0 : item.ServiceCharge;
                    wsSetMenu.Cell("S" + row).Value = item.StoreName;
                    wsSetMenu.Cell("T" + row).Value = item.ImageURL;
                    //=============================
                    row++;
                    countIndex++;
                }
                FormatExcelExport(wsSetMenu, row, cols);
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

        public StatusResponse ExportDish(ref IXLWorksheet wsSetMenu, ref IXLWorksheet wsTab, ref IXLWorksheet wsDish, List<string> lstStore)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<ProductModels> listData = new List<ProductModels>();

                ProductApiModels paraBody = new ProductApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;
                paraBody.ProductType = (byte)Commons.EProductType.Dish;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportProduct, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListProduct"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<ProductModels>>(lstContent);
                listData = listData.OrderBy(o => o.StoreName).ToList();

                int row = 1;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Bar Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow Discount"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Check Stock"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Kitchen Display Name Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print out Name Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seasonal Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cost"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit of Measurement"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Limit"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Open Price"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Print On Check"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Expired Date"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SC value"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Force Modifier"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Optional Modifier"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image Url"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Printer"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Coming Soon"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show Kiosk Message"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Information"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Additional Dishes"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Color"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("POS Availability"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Message")
                };
                for (int i = 1; i <= listSetMenuHeader.Length; i++)
                    wsSetMenu.Cell(row, i).Value = listSetMenuHeader[i - 1];
                int cols = listSetMenuHeader.Length;
                //Item
                row = 2;
                int countIndex = 1;
                int countIndexTab = 1;
                int countIndexDish = 1;

                List<ExportItemTab> lstTab = new List<ExportItemTab>();
                List<ExportItemDish> lstDish = new List<ExportItemDish>();

                foreach (var item in listData)
                {
                    wsSetMenu.Cell("A" + row).Value = countIndex;
                    wsSetMenu.Cell("B" + row).Value = item.OrderByIndex;
                    wsSetMenu.Cell("C" + row).Value = item.Name;
                    wsSetMenu.Cell("D" + row).Value = item.ProductCode;
                    wsSetMenu.Cell("E" + row).Value = item.BarCode;

                    wsSetMenu.Cell("F" + row).Value = item.IsAllowedDiscount ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("G" + row).Value = item.IsCheckedStock ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("H" + row).Value = item.Description;
                    wsSetMenu.Cell("I" + row).Value = item.IsShowMessage ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("J" + row).Value = item.PrintOutText;
                    wsSetMenu.Cell("K" + row).Value = item.IsShowMessage ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("L" + row).Value = item.SeasonPrice;
                    wsSetMenu.Cell("M" + row).Value = item.ProductSeason;

                    wsSetMenu.Cell("N" + row).Value = item.Cost;
                    wsSetMenu.Cell("O" + row).Value = item.Unit;
                    wsSetMenu.Cell("P" + row).Value = item.Measure;
                    wsSetMenu.Cell("Q" + row).Value = item.DefaultPrice;
                    wsSetMenu.Cell("R" + row).Value = item.Quantity;
                    wsSetMenu.Cell("S" + row).Value = item.Limit;
                    wsSetMenu.Cell("T" + row).Value = item.CategoryName;
                    wsSetMenu.Cell("U" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("V" + row).Value = item.IsAllowedOpenPrice ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("W" + row).Value = item.IsPrintedOnCheck ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("X" + row).Value = item.ExpiredDate.Value.Date == Commons._ExpiredDate.Date ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited")
                        : "'" + item.ExpiredDate.Value.ToString("dd/MM/yyyy");
                    wsSetMenu.Cell("Y" + row).Value = item.HasServiceCharge ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("Z" + row).Value = !item.HasServiceCharge ? 0 : item.ServiceCharge;
                    wsSetMenu.Cell("AA" + row).Value = item.IsForce ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("AB" + row).Value = item.IsOptional ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("AC" + row).Value = item.StoreName;
                    wsSetMenu.Cell("AD" + row).Value = item.ImageURL;

                    if (item.ListPrinter != null && item.ListPrinter.Count > 0)
                    {
                        wsSetMenu.Cell("AE" + row).Value = string.Join(";", item.ListPrinter.Select(ss=>ss.PrinterName).ToArray());
                    }
                    else
                        wsSetMenu.Cell("AE" + row).Value = "";

                    wsSetMenu.Cell("AF" + row).Value = item.DefaultState == (byte)Commons.EItemState.PendingStatus
                                                        ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Pending") :
                                                       item.DefaultState == (byte)Commons.EItemState.PendingStatus
                                                       ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ready") :
                                                       item.DefaultState == (byte)Commons.EItemState.ServedStatus
                                                       ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Served") : "";

                    wsSetMenu.Cell("AG" + row).Value = item.IsComingSoon ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("AH" + row).Value = item.IsShowMessage ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("AI" + row).Value = item.Info;
                    wsSetMenu.Cell("AJ" + row).Value = item.Tax;
                    wsSetMenu.Cell("AK" + row).Value = item.IsAddition ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    wsSetMenu.Cell("AL" + row).Value = item.ColorCode;
                    var ListProductSeasonPOS = item.ListProductSeason.Where(x => x.IsPOS).Select(x => x.SeasonName).ToList();
                    item.ProductSeasonPOS = (ListProductSeasonPOS != null && ListProductSeasonPOS.Count > 0) ? string.Join(";", ListProductSeasonPOS) : "All";
                    wsSetMenu.Cell("AM" + row).Value = item.ProductSeasonPOS;
                    wsSetMenu.Cell("AN" + row).Value = item.Message;
                    //=============================
                    if (item.ListGroup != null)
                    {
                        for (int i = 0; i < item.ListGroup.Count; i++)
                        {
                            ExportItemTab eTab = new ExportItemTab()
                            {
                                DisplayMsg = item.ListGroup[i].Description,
                                Quantity = item.ListGroup[i].Maximum,
                                Seq = item.ListGroup[i].Sequence,
                                SetMenuIndex = countIndex,
                                SetMenuName = item.Name,
                                TabName = item.ListGroup[i].Name,
                                TabIndex = countIndexTab,
                                Type = item.ListGroup[i].Type
                            };
                            if (item.ListGroup[i].ListProductOnGroup != null)
                            {
                                for (int j = 0; j < item.ListGroup[i].ListProductOnGroup.Count; j++)
                                {
                                    ExportItemDish eDish = new ExportItemDish()
                                    {
                                        DishIndex = countIndexDish,
                                        DishName = item.ListGroup[i].ListProductOnGroup[j].ProductName,
                                        ExtraPrice = item.ListGroup[i].ListProductOnGroup[j].ExtraPrice,
                                        Printer = "",
                                        Seq = item.ListGroup[i].ListProductOnGroup[j].Sequence,
                                        TabIndex = countIndexTab
                                    };
                                    countIndexDish++;
                                    lstDish.Add(eDish);
                                }
                            }
                            countIndexTab++;
                            lstTab.Add(eTab);
                        }
                    }
                    row++;
                    countIndex++;
                }
                FormatExcelExport(wsSetMenu, row, cols);
                //=========
                row = 1;
                string[] listTabHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Display Message"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type")
                };
                for (int i = 1; i <= listTabHeader.Length; i++)
                    wsTab.Cell(row, i).Value = listTabHeader[i - 1];
                cols = listTabHeader.Length;
                row++;
                foreach (var item in lstTab)
                {
                    string modifierType = "";
                    if (item.Type == (byte)Commons.EModifierType.Product)
                        modifierType = Commons.EModifierType.Product.ToString();
                    else if (item.Type == (byte)Commons.EModifierType.Forced)
                        modifierType = Commons.EModifierType.Forced.ToString();
                    else if (item.Type == (byte)Commons.EModifierType.Optional)
                        modifierType = Commons.EModifierType.Optional.ToString();
                    else if (item.Type == (byte)Commons.EModifierType.AdditionalModifier)
                        modifierType = Commons.EModifierType.AdditionalModifier.ToString();
                    else if (item.Type == (byte)Commons.EModifierType.Special)
                        modifierType = Commons.EModifierType.Special.ToString();
                    else if (item.Type == (byte)Commons.EModifierType.AdditionalDish)
                        modifierType = Commons.EModifierType.AdditionalDish.ToString();
                    //=========
                    wsTab.Cell("A" + row).Value = item.TabIndex;
                    wsTab.Cell("B" + row).Value = item.Seq;
                    wsTab.Cell("C" + row).Value = item.SetMenuIndex;
                    wsTab.Cell("D" + row).Value = item.SetMenuName;
                    wsTab.Cell("E" + row).Value = item.TabName;
                    wsTab.Cell("F" + row).Value = item.DisplayMsg;
                    wsTab.Cell("G" + row).Value = item.Quantity;
                    wsTab.Cell("H" + row).Value = modifierType;
                    row++;
                }
                FormatExcelExport(wsTab, row, cols);
                //============
                row = 1;
                string[] listDishHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Extra Price")
                };
                for (int i = 1; i <= listDishHeader.Length; i++)
                    wsDish.Cell(row, i).Value = listDishHeader[i - 1];
                cols = listDishHeader.Length;
                row++;
                foreach (var item in lstDish)
                {
                    wsDish.Cell("A" + row).Value = item.DishIndex;
                    wsDish.Cell("B" + row).Value = item.Seq;
                    wsDish.Cell("C" + row).Value = item.TabIndex;
                    wsDish.Cell("D" + row).Value = item.DishName;
                    wsDish.Cell("E" + row).Value = item.ExtraPrice;
                    //wsDish.Cell("F" + row).Value = item.Printer;
                    row++;
                }
                //========
                FormatExcelExport(wsDish, row, cols);
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
        public List<SetMenuImportResultItem> ImportSetMenu(string filePath, FileInfo[] listFileInfo, out int totalRowExel, List<string> storeIndexes, ref string msg)
        {
            totalRowExel = 0;
            List<SetMenuImportResultItem> importItems = new List<SetMenuImportResultItem>();

            DataTable dtSetMenu = ReadExcelFile(@filePath, "SetMenus");
            DataTable dtTab = ReadExcelFile(@filePath, "Tabs");
            DataTable dtDish = ReadExcelFile(@filePath, "Dishes");

            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBInventorySetMenu.xlsx";
            DataTable dtSetMenuTmp = ReadExcelFile(@tmpExcelPath, "SetMenus");
            DataTable dtTabTmp = ReadExcelFile(@tmpExcelPath, "Tabs");
            DataTable dtDishTmp = ReadExcelFile(@tmpExcelPath, "Dishes");

            if (dtSetMenu.Columns.Count != dtSetMenuTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }

            if (dtTab.Columns.Count != dtTabTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }
            if (dtDish.Columns.Count != dtDishTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }

            List<SetMenuImportItem> lstSetMenu = GetListObject<SetMenuImportItem>(dtSetMenu);
            List<TabImportItem> lstTab = GetListObject<TabImportItem>(dtTab);
            List<DishTabImportItem> lstDish = GetListObject<DishTabImportItem>(dtDish);
            // validate tab Set Menu, 
            ValidateRowSetMenu(ref lstSetMenu);
            ValidateRowTabSetMenu(ref lstTab);
            ValidateRowDishSetMenu(ref lstDish);

            List<ProductModels> listData = new List<ProductModels>();
            SetMenuImportResultItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            foreach (var store in storeIndexes)
            {
                foreach (DataRow item in dtSetMenu.Rows)
                {
                    flagInsert = true;
                    msgError = "";

                    if (item[0].ToString().Equals(""))
                        continue;
                    int index = int.Parse(item[0].ToString());

                    string ImageUrl = "";
                    if (!string.IsNullOrEmpty(item[24].ToString()))
                    {
                        FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[24].ToString().ToLower());
                        if (file != null)
                        {
                            if (file.Length > Commons._MaxSizeFileUploadImg)
                            {
                                flagInsert = false;
                                msgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgAllowedSizeImg) + "<br/>";
                            }
                            else
                            {
                                ImageUrl = Guid.NewGuid() + file.Extension;
                                byte[] photoByte = null;
                                photoByte = System.IO.File.ReadAllBytes(file.FullName);
                                //19/01/2018
                                //photoByte = file.ReadFully();
                                //Save Image on Server
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

                    List<GroupProductModels> lstGProduct = new List<GroupProductModels>();
                    DataRow[] GProducs = dtTab.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Index") + "] = " + index + "");
                    foreach (DataRow gProduct in GProducs)
                    {
                        int tabIndex = int.Parse(gProduct[0].ToString());

                        //========== Get List ProductOnGroupModels
                        List<ProductOnGroupModels> ListProOnGroup = new List<ProductOnGroupModels>();
                        DataRow[] GoProducs = dtDish.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index") + "] = " + tabIndex + "");
                        foreach (DataRow GroupOnProduct in GoProducs)
                        {
                            ProductOnGroupModels goPModel = new ProductOnGroupModels()
                            {
                                ProductName = GroupOnProduct[3].ToString(),
                                ExtraPrice = string.IsNullOrEmpty(GroupOnProduct[4].ToString()) ? 0 : Math.Round(double.Parse(GroupOnProduct[4].ToString()), 2)
                            };
                            if (string.IsNullOrEmpty(GroupOnProduct[1].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seq is required");
                            }
                            if (string.IsNullOrEmpty(GroupOnProduct[2].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index is required");
                            }
                            if (string.IsNullOrEmpty(GroupOnProduct[3].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name is required");
                            }
                            if (flagInsert)
                            {
                                ListProOnGroup.Add(goPModel);
                            }
                        }
                        //=======
                        GroupProductModels gPModel = new GroupProductModels()
                        {
                            Sequence = int.Parse(gProduct[1].ToString()),
                            Name = gProduct[4].ToString(),
                            Description = gProduct[5].ToString(),
                            Minimum = 0,
                            Maximum = string.IsNullOrEmpty(gProduct[6].ToString()) ? 0 : int.Parse(gProduct[6].ToString()),
                            ListProductOnGroup = ListProOnGroup,
                            Type = (byte)Commons.EModifierType.Forced
                        };
                        if (string.IsNullOrEmpty(gProduct[1].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seq is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[2].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Index is required");
                        }
                        if (string.IsNullOrEmpty(gPModel.Name))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Name is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[4].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name is required");
                        }
                        if (flagInsert)
                        {
                            lstGProduct.Add(gPModel);
                        }
                    }

                    string msgItem = "";
                    DateTime ExpiredDate = DateTime.Now;
                    string expiryDate = item[21].ToString();
                    if (!expiryDate.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("unlimited")) && !expiryDate.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("never")) && !string.IsNullOrEmpty(expiryDate.Trim()))
                    {
                        ExpiredDate = DateTimeHelper.GetDateImport(expiryDate, ref msgItem);
                        if (!msgItem.Equals(""))
                        {
                            flagInsert = false;
                            msgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msgItem);
                        }
                    }
                    else
                    {
                        ExpiredDate = Commons._ExpiredDate;
                    }

                    ProductModels model = new ProductModels
                    {
                        Index = index.ToString(),
                        OrderByIndex = string.IsNullOrEmpty(item[1].ToString()) ? 0 : int.Parse(item[1].ToString()),
                        Name = item[2].ToString(),
                        ProductCode = item[3].ToString(),
                        BarCode = item[4].ToString(),

                        IsCheckedStock = string.IsNullOrEmpty(item[5].ToString()) ? false
                        : bool.Parse(item[5].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        IsAllowedDiscount = string.IsNullOrEmpty(item[6].ToString()) ? false
                        : bool.Parse(item[6].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        Description = item[7].ToString(),
                        IsShowMessage = string.IsNullOrEmpty(item[8].ToString()) ? false
                        : bool.Parse(item[8].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        PrintOutText = item[9].ToString(),
                        DefaultPrice = item[11].ToString().Equals("") ? 0 : Math.Round(double.Parse(item[11].ToString()), 2),

                        SeasonPrice = string.IsNullOrEmpty(item[12].ToString()) ? 0 : double.Parse(item[12].ToString()),
                        ProductSeason = item[13].ToString(),
                        Cost = item[14].ToString().Equals("") ? 0 : double.Parse(item[14].ToString()),
                        Quantity = item[15].ToString().Equals("") ? 0 : double.Parse(item[15].ToString()),
                        Limit = item[16].ToString().Equals("") ? 0 : int.Parse(item[16].ToString()),
                        CategoryName = item[17].ToString(),
                        IsActive = string.IsNullOrEmpty(item[18].ToString()) ? true
                        : bool.Parse(item[18].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        IsAllowedOpenPrice = string.IsNullOrEmpty(item[19].ToString()) ? true : bool.Parse(item[19].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        IsPrintedOnCheck = string.IsNullOrEmpty(item[20].ToString()) ? true : bool.Parse(item[20].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        ExpiredDate = string.IsNullOrEmpty(item[21].ToString()) ? Commons._ExpiredDate : ExpiredDate,

                        HasServiceCharge = string.IsNullOrEmpty(item[22].ToString()) ? true : bool.Parse(item[22].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        ServiceCharge = item[23].ToString().Equals("") ? 0 : double.Parse(item[23].ToString()),
                        IsShowInReservation = string.IsNullOrEmpty(item[24].ToString()) ? false : bool.Parse(item[24].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        StoreID = store,

                        ImageURL = ImageUrl,
                        ListGroup = lstGProduct,

                        ColorCode = "#000000",
                        ProductTypeCode = (byte)Commons.EProductType.SetMenu,
                        Measure = "Set"
                    };
                    //============
                    if (string.IsNullOrEmpty(model.Name))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Name is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (string.IsNullOrEmpty(model.ProductCode))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Code is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.ServiceCharge < 0 || model.ServiceCharge > 100)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge must between 0 and 100");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.OrderByIndex < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Quantity < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Quantity larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.DefaultPrice < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Default Price larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Cost < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Cost larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Limit < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Limit larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    //if (model.Unit < 0)
                    //{
                    //    flagInsert = false;
                    //    msgItem = "Please enter a value Unit larger or equal to 0";
                    //    msgError = "<br/>" + msgItem;
                    //}
                    if (model.Cost > model.DefaultPrice)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price must larger than cost");
                        msgError += "<br/>" + msgItem;
                    }
                    if (string.IsNullOrEmpty(model.CategoryName))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please re-enter category for this set menu");
                        msgError += "<br/>" + msgItem;
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
            }

            //try
            //{
            ProductApiModels paraBody = new ProductApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListProduct = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportProduct, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                //=====
                SetMenuImportResultItem importItem = new SetMenuImportResultItem();
                importItem.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have been") + " [" + (listData.Count - listError.Count) + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("row(s) import Successful") + "<strong>";
                //importItem.ListSuccessStoreName.Add("Have been [" + (listData.Count - listError.Count) + "] row(s) import Successful");
                importItems.Insert(0, importItem);
                //=====End

                foreach (ImportResult itemError in listError)
                {
                    SetMenuErrorItem item = new SetMenuErrorItem();
                    item.GroupName = itemError.Index;
                    item.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index + "<br/>"
                        + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error);

                    importItem = new SetMenuImportResultItem();
                    importItem.Name = itemError.Property;
                    importItem.ListFailStoreName.Add(itemError.StoreName);
                    importItem.ErrorItems.Add(item);
                    importItems.Add(importItem);
                }
                if (importItems.Count == 0)
                {
                    importItem = new SetMenuImportResultItem();
                    importItem.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("SetMenus");
                    importItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Set Menu Successful"));
                    importItems.Add(importItem);
                }
            }

            return importItems;
        }

        public List<SetMenuImportResultItem> ImportModifier(string filePath, FileInfo[] listFileInfo, out int totalRowExel, List<string> storeIndexes, ref string msg)
        {
            totalRowExel = 0;
            List<SetMenuImportResultItem> importItems = new List<SetMenuImportResultItem>();

            DataTable dtModifier = ReadExcelFile(@filePath, "Modifiers");

            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBInventoryModifier.xlsx";
            DataTable dtSetMenuTmp = ReadExcelFile(@tmpExcelPath, "Modifiers");

            if (dtModifier.Columns.Count != dtSetMenuTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }

            List<SetMenuImportItem> lstSetMenu = GetListObject<SetMenuImportItem>(dtModifier);
            // validate tab Set Menu, 
            //ValidateRowSetMenu(ref lstSetMenu);

            List<ProductModels> listData = new List<ProductModels>();
            SetMenuImportResultItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            foreach (var store in storeIndexes)
            {
                foreach (DataRow item in dtModifier.Rows)
                {
                    flagInsert = true;
                    msgError = "";

                    if (item[0].ToString().Equals(""))
                        continue;
                    int index = int.Parse(item[0].ToString());

                    string ImageUrl = "";
                    if (!string.IsNullOrEmpty(item[19].ToString()))
                    {
                        FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[19].ToString().ToLower());
                        if (file != null)
                        {
                            if (file.Length > Commons._MaxSizeFileUploadImg)
                            {
                                flagInsert = false;
                                msgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgAllowedSizeImg) + "<br/>";
                            }
                            else
                            {
                                ImageUrl = Guid.NewGuid() + file.Extension;
                                byte[] photoByte = null;
                                photoByte = System.IO.File.ReadAllBytes(file.FullName);
                                //19/01/2018
                                //photoByte = file.ReadFully();
                                //Save Image on Server
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

                    ProductModels model = new ProductModels
                    {
                        Index = index.ToString(),
                        OrderByIndex = string.IsNullOrEmpty(item[1].ToString()) ? 0 : int.Parse(item[1].ToString()),
                        Name = item[2].ToString(),
                        ProductCode = item[3].ToString(),
                        BarCode = item[4].ToString(),

                        IsCheckedStock = string.IsNullOrEmpty(item[5].ToString()) ? false
                        : bool.Parse(item[5].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        Description = item[6].ToString(),

                        IsShowMessage = string.IsNullOrEmpty(item[7].ToString()) ? false
                        : bool.Parse(item[7].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        PrintOutText = item[8].ToString(),

                        Quantity = item[10].ToString().Equals("") ? 0 : double.Parse(item[10].ToString()),

                        DefaultPrice = item[11].ToString().Equals("") ? 0 : Math.Round(double.Parse(item[11].ToString()), 2),

                        IsAllowedOpenPrice = string.IsNullOrEmpty(item[12].ToString()) ? false : bool.Parse(item[12].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        CategoryName = item[13].ToString(),
                        Limit = item[14].ToString().Equals("") ? 0 : int.Parse(item[14].ToString()),
                        IsActive = string.IsNullOrEmpty(item[15].ToString()) ? true
                        : bool.Parse(item[15].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        IsPrintedOnCheck = string.IsNullOrEmpty(item[16].ToString()) ? true : bool.Parse(item[16].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        IsAllowedDiscount = string.IsNullOrEmpty(item[17].ToString()) ? true : bool.Parse(item[17].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        //HasServiceCharge = string.IsNullOrEmpty(item[16].ToString()) ? true : bool.Parse(item[17].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        //ServiceCharge = item[18].ToString().Equals("") ? 0 : double.Parse(item[18].ToString()),
                        StoreID = store,
                        ImageURL = ImageUrl,
                        ProductTypeCode = (byte)Commons.EProductType.Modifier,
                        ExpiredDate = new DateTime(9999, 12, 31),
                        ColorCode = "#000000",
                        Measure = "$"
                    };

                    //============
                    string msgItem = "";
                    if (string.IsNullOrEmpty(model.Name))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Name is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (string.IsNullOrEmpty(model.ProductCode))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Code is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.ServiceCharge < 0 || model.ServiceCharge > 100)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge must between 0 and 100");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.OrderByIndex < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Quantity < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Quantity larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.DefaultPrice < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Default Price larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Cost < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Cost larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Limit < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Limit larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (string.IsNullOrEmpty(model.CategoryName))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please re-enter Category for this modifier");
                        msgError += "<br/>" + msgItem;
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
            }

            ProductApiModels paraBody = new ProductApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListProduct = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportProduct, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                SetMenuImportResultItem importItemNew = new SetMenuImportResultItem();
                importItemNew.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have been") + " [" + (listData.Count - listError.Count) + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("row(s) import Successful") + "<strong>";
                importItems.Add(importItemNew);

                foreach (ImportResult itemError in listError)
                {
                    SetMenuErrorItem item = new SetMenuErrorItem();
                    item.GroupName = itemError.Index;
                    item.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index + "<br/>"
                        + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error);

                    SetMenuImportResultItem importItem = new SetMenuImportResultItem();
                    importItem.Name = itemError.Property;
                    importItem.ListFailStoreName.Add(itemError.StoreName);
                    importItem.ErrorItems.Add(item);
                    importItems.Add(importItem);
                }

                if (importItems.Count == 0)
                {
                    SetMenuImportResultItem importItem = new SetMenuImportResultItem();
                    importItem.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifiers");
                    importItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Modifier Successful"));
                    importItems.Add(importItem);
                }
            }
            return importItems;
        }

        public List<DishImportResultItem> ImportDish(string filePath, FileInfo[] listFileInfo, out int totalRowExel, List<string> storeIndexes, ref string msg)
        {
            NSLog.Logger.Info("Start import dish", storeIndexes);
            totalRowExel = 0;
            List<DishImportResultItem> importItems = new List<DishImportResultItem>();

            DataTable dtDish = ReadExcelFile(filePath, "Dishes");
            DataTable dtTab = ReadExcelFile(filePath, "Tabs");
            DataTable dtModifier = ReadExcelFile(filePath, "Modifiers");

            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBInventoryDish.xlsx";
            DataTable dtDishTmp = ReadExcelFile(tmpExcelPath, "Dishes");
            DataTable dtTabTmp = ReadExcelFile(tmpExcelPath, "Tabs");
            DataTable dtModifierTmp = ReadExcelFile(tmpExcelPath, "Modifiers");

            if (dtDish.Columns.Count != dtDishTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }
            if (dtTab.Columns.Count != dtTabTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }
            if (dtModifier.Columns.Count != dtModifierTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }

            List<DishImportItem> dishes = GetListObject<DishImportItem>(dtDish);
            List<DishGroupImportItem> groups = GetListObject<DishGroupImportItem>(dtTab);
            List<DishModifierImportItem> modifiers = GetListObject<DishModifierImportItem>(dtModifier);
            // validate tab Dish, 
            ValidateRowDish(ref dishes);
            ValidateRowGroupDish(ref groups);
            ValidateRowModifier(ref modifiers);

            List<ProductModels> listData = new List<ProductModels>();

            DishImportResultItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            foreach (var store in storeIndexes)
            {
                foreach (DataRow item in dtDish.Rows)
                {
                    flagInsert = true;
                    msgError = "";

                    if (item[0].ToString().Equals(""))
                        continue;
                    int index = int.Parse(item[0].ToString());

                    string ImageUrl = "";
                    if (!string.IsNullOrEmpty(item[28].ToString()))
                    {
                        FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[28].ToString().ToLower());
                        if (file != null)
                        {
                            if (file.Length > Commons._MaxSizeFileUploadImg)
                            {
                                flagInsert = false;
                                msgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgAllowedSizeImg) + "<br/>";
                            }
                            else
                            {
                                ImageUrl = Guid.NewGuid() + file.Extension;
                                byte[] photoByte = null;
                                photoByte = System.IO.File.ReadAllBytes(file.FullName);
                                //19/01/2018
                                //photoByte = file.ReadFully();
                                //Save Image on Server
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

                    List<GroupProductModels> lstGProduct = new List<GroupProductModels>();
                    //====
                    DataRow[] GProducs = dtTab.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Index") + "] = " + index + "");
                    int _sequen = 0;
                    foreach (DataRow gProduct in GProducs)
                    {
                        int tabIndex = int.Parse(gProduct[0].ToString());
                        //========== Get List ProductOnGroupModels
                        List<ProductOnGroupModels> ListProOnGroup = new List<ProductOnGroupModels>();
                        DataRow[] GoProducs = dtModifier.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index") + "] = " + tabIndex + "");
                        foreach (DataRow GroupOnProduct in GoProducs)
                        {

                            ProductOnGroupModels goPModel = new ProductOnGroupModels()
                            {
                                Sequence = string.IsNullOrEmpty(GroupOnProduct[1].ToString()) ? 0 : int.Parse(GroupOnProduct[1].ToString()),
                                ProductName = GroupOnProduct[3].ToString(),
                                ExtraPrice = string.IsNullOrEmpty(GroupOnProduct[4].ToString()) ? 0 : Math.Round(double.Parse(GroupOnProduct[4].ToString()), 2)
                            };
                            //Validation
                            if (string.IsNullOrEmpty(GroupOnProduct[1].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seq is required");
                            }
                            if (string.IsNullOrEmpty(GroupOnProduct[2].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index is required");
                            }
                            if (string.IsNullOrEmpty(goPModel.ProductName))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Name is required");
                            }
                            if (flagInsert)
                            {
                                ListProOnGroup.Add(goPModel);
                            }
                        }
                        //=======
                        //int modifierType = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(
                        //                                gProduct[6].ToString()).Trim().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(
                        //                                    Commons.ModifierForced).ToString())
                        //                                ? (byte)Commons.EModifierType.Forced : (byte)Commons.EModifierType.Optional;

                        int modifierType = GetTypeForTabDish(gProduct[7].ToString().Trim());
                        GroupProductModels gPModel = new GroupProductModels()
                        {
                            Sequence = int.Parse(gProduct[1].ToString()),
                            Name = gProduct[4].ToString(),
                            Description = gProduct[5].ToString(),
                            Minimum = 0,
                            Maximum = int.Parse(gProduct[6].ToString()),
                            ListProductOnGroup = ListProOnGroup,
                            Type = modifierType
                        };

                        //Validation
                        if (string.IsNullOrEmpty(gProduct[1].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Seq is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[2].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Index is required");
                        }
                        if (string.IsNullOrEmpty(gPModel.Name))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[4].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab name is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[6].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity is required");
                        }
                        if (string.IsNullOrEmpty(gProduct[7].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type is required");
                        }
                        if (flagInsert)
                        {
                            lstGProduct.Add(gPModel);
                        }
                    }

                    int defaulStatus = 1;
                    string dStatus = item[30].ToString();
                    if (dStatus.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EItemState.PendingStatus.ToString())))
                    {
                        defaulStatus = (int)Commons.EItemState.PendingStatus;
                    }
                    else if (dStatus.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EItemState.CompleteStatus.ToString())))
                    {
                        defaulStatus = (int)Commons.EItemState.CompleteStatus;
                    }
                    else if (dStatus.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EItemState.ServedStatus.ToString())))
                    {
                        defaulStatus = (int)Commons.EItemState.ServedStatus;
                    }

                    string msgItem = "";
                    DateTime ExpiredDate = DateTime.Now;
                    string expiryDate = item[23].ToString();
                    if (!expiryDate.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("unlimited")) && !expiryDate.ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("never")) && !string.IsNullOrEmpty(expiryDate.Trim()))
                    {
                        ExpiredDate = DateTimeHelper.GetDateImport(expiryDate, ref msgItem);
                        if (!msgItem.Equals(""))
                        {
                            flagInsert = false;
                            msgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msgItem);
                        }
                    }
                    else
                    {
                        ExpiredDate = Commons._ExpiredDate;
                    }
                    #region Old
                    //ProductModels model = new ProductModels
                    //{
                    //    Index = index.ToString(),
                    //    OrderByIndex = string.IsNullOrEmpty(item[1].ToString()) ? 0 : int.Parse(item[1].ToString()),
                    //    Name = item[2].ToString(),
                    //    ProductCode = item[3].ToString(),
                    //    BarCode = item[4].ToString(),
                    //    IsAllowedDiscount = string.IsNullOrEmpty(item[5].ToString()) ? true : bool.Parse(item[5].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    IsCheckedStock = string.IsNullOrEmpty(item[6].ToString()) ? false : bool.Parse(item[6].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    Description = item[7].ToString(),

                    //    PrintOutText = item[9].ToString(),
                    //    Cost = item[11].ToString().Equals("") ? 0 : double.Parse(item[11].ToString()),
                    //    Unit = item[12].ToString().Equals("") ? 0 : int.Parse(item[12].ToString()),

                    //    Measure = item[13].ToString().Equals("") ? "$" : item[13].ToString(),

                    //    DefaultPrice = item[14].ToString().Equals("") ? 0 : Math.Round(double.Parse(item[14].ToString()), 2),

                    //    Quantity = item[15].ToString().Equals("") ? 0 : double.Parse(item[15].ToString()),
                    //    Limit = item[16].ToString().Equals("") ? 0 : int.Parse(item[16].ToString()),

                    //    CategoryName = item[17].ToString(),
                    //    IsActive = string.IsNullOrEmpty(item[18].ToString()) ? true : bool.Parse(item[18].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    IsAllowedOpenPrice = string.IsNullOrEmpty(item[19].ToString()) ? false : bool.Parse(item[19].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    IsPrintedOnCheck = string.IsNullOrEmpty(item[20].ToString()) ? true : bool.Parse(item[20].ToString().ToLower().Equals("yes").ToString()) ? true : false,

                    //    ExpiredDate = string.IsNullOrEmpty(item[21].ToString()) ? Commons._ExpiredDate : item[21].ToString().ToLower().Equals("unlimited") ? Commons._ExpiredDate : ExpiredDate,

                    //    HasServiceCharge = string.IsNullOrEmpty(item[22].ToString()) ? true : bool.Parse(item[22].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    ServiceCharge = item[23].ToString().Equals("") ? 0 : double.Parse(item[23].ToString()),
                    //    IsForce = bool.Parse(item[24].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    StoreID = store,
                    //    ImageURL = ImageUrl,
                    //    Printer = item[27].ToString(),

                    //    DefaultState = string.IsNullOrEmpty(dStatus) ? (byte)Commons.EItemState.PendingStatus : (byte)defaulStatus,

                    //    IsComingSoon = string.IsNullOrEmpty(item[28].ToString()) ? false : bool.Parse(item[28].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    IsShowMessage = string.IsNullOrEmpty(item[29].ToString()) ? false : bool.Parse(item[29].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    Info = item[30].ToString(),
                    //    IsAddition = string.IsNullOrEmpty(item[32].ToString()) ? false : bool.Parse(item[32].ToString().ToLower().Equals("yes").ToString()) ? true : false,
                    //    Message = item[35].ToString(),
                    //    ListGroup = lstGProduct,

                    //    ColorCode = "#000000",
                    //    ProductTypeCode = (byte)Commons.EProductType.Dish,
                    //};
                    #endregion 
                    ProductModels model = new ProductModels();
                    _sequen = 0;
                    if (!string.IsNullOrEmpty(item[1].ToString()))
                        int.TryParse(item[1].ToString(), out _sequen);
                    model.Index = index.ToString();
                    model.OrderByIndex = _sequen;//string.IsNullOrEmpty(item[1].ToString()) ? 0 : int.Parse(item[1].ToString());
                    model.Name = item[2].ToString();
                    model.ProductCode = item[3].ToString();
                    model.BarCode = item[4].ToString();
                    model.IsAllowedDiscount = string.IsNullOrEmpty(item[5].ToString()) ? true : bool.Parse(item[5].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;

                    model.IsCheckedStock = string.IsNullOrEmpty(item[6].ToString()) ? false : bool.Parse(item[6].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;
                    model.Description = item[7].ToString();

                    model.PrintOutText = item[9].ToString();
                    model.SeasonPrice = item[11].ToString().Equals("") ? 0 : double.Parse(item[11].ToString());
                    model.ProductSeason = item[12].ToString();
                    model.Cost = item[13].ToString().Equals("") ? 0 : double.Parse(item[13].ToString());
                    model.Unit = item[14].ToString().Equals("") ? 0 : int.Parse(item[14].ToString());

                    model.Measure = item[15].ToString().Equals("") ? "Dish" : item[15].ToString();

                    model.DefaultPrice = item[16].ToString().Equals("") ? 0 : Math.Round(double.Parse(item[16].ToString()), 2);

                    model.Quantity = item[17].ToString().Equals("") ? 0 : double.Parse(item[17].ToString());
                    model.Limit = item[18].ToString().Equals("") ? 0 : int.Parse(item[18].ToString());

                    model.CategoryName = item[19].ToString();
                    model.IsActive = string.IsNullOrEmpty(item[20].ToString()) ? true
                        : bool.Parse(item[20].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;

                    model.IsAllowedOpenPrice = string.IsNullOrEmpty(item[21].ToString()) ? false : bool.Parse(item[21].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;

                    model.IsPrintedOnCheck = string.IsNullOrEmpty(item[22].ToString()) ? true : bool.Parse(item[22].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;


                    model.ExpiredDate = string.IsNullOrEmpty(item[23].ToString()) ? Commons._ExpiredDate : ExpiredDate;

                    model.HasServiceCharge = string.IsNullOrEmpty(item[24].ToString()) ? true : bool.Parse(item[24].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;

                    model.ServiceCharge = item[25].ToString().Equals("") ? 0 : double.Parse(item[25].ToString());

                    model.IsForce = bool.Parse(item[26].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;
                    model.IsOptional = bool.Parse(item[27].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;

                    model.StoreID = store;
                    model.ImageURL = ImageUrl;
                    model.Printer = item[30].ToString();

                    model.DefaultState = string.IsNullOrEmpty(dStatus) ? (byte)Commons.EItemState.PendingStatus : (byte)defaulStatus;

                    model.IsComingSoon = string.IsNullOrEmpty(item[31].ToString()) ? false : bool.Parse(item[31].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;

                    model.IsShowMessage = string.IsNullOrEmpty(item[32].ToString()) ? false : bool.Parse(item[30].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;

                    model.Info = item[33].ToString();
                    model.IsAddition = string.IsNullOrEmpty(item[35].ToString()) ? false
                        : bool.Parse(item[35].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false;

                    model.Message = item[39].ToString();
                    model.ListGroup = lstGProduct;

                    model.ColorCode = string.IsNullOrEmpty(item[37].ToString()) ? "#000000" : item[37].ToString();
                    model.ProductTypeCode = (byte)Commons.EProductType.Dish;

                    string sPOS = item[38].ToString();
                    if (!string.IsNullOrEmpty(sPOS))
                    {
                        foreach (var itemPOS in sPOS.Split(';'))
                        {
                            if (!itemPOS.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("All")))
                            {
                                model.ListProductSeason.Add(new ProductSeasonDTO
                                {
                                    SeasonName = itemPOS,
                                    IsPOS = true
                                });
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(item[30].ToString()))
                    {
                        foreach (var itemPrinter in item[30].ToString().Split(';'))
                        {
                            model.ListPrinter.Add(new PrinterOnProductModels
                            {
                                PrinterName = itemPrinter
                            });
                        }
                    }
                    //============

                    if (string.IsNullOrEmpty(model.Name))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (string.IsNullOrEmpty(model.ProductCode))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Code is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.ServiceCharge < 0 || model.ServiceCharge > 100)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Service Charge must between 0 and 100");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.OrderByIndex < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Quantity < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Quantity larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.DefaultPrice < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Default Price larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Cost < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Cost larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Limit < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Limit larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Unit < 0)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Unit larger or equal to 0");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.Cost > model.DefaultPrice)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price must larger than cost");
                        msgError += "<br/>" + msgItem;
                    }
                    if (string.IsNullOrEmpty(model.CategoryName))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please re-enter Category for this dish");
                        msgError += "<br/>" + msgItem;
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
                        itemerr.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + index + msgError;

                        itemErr = new DishImportResultItem();
                        itemErr.Name = model.Name;
                        itemErr.ListFailStoreName.Add("");
                        itemErr.ErrorItems.Add(itemerr);
                        importItems.Add(itemErr);
                    }
                }
            }

            //try
            //{
            ProductApiModels paraBody = new ProductApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListProduct = listData;

            NSLog.Logger.Info("import dish data", paraBody);
            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportProduct, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                //=====
                DishImportResultItem importItem = new DishImportResultItem();
                importItem.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have been") + " [" + (listData.Count - listError.Count) + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("row(s) import Successful") + "<strong>";
                importItems.Insert(0, importItem);
                //=====End

                foreach (ImportResult itemError in listError)
                {
                    DishErrorItem item = new DishErrorItem();
                    item.GroupName = itemError.Index;
                    item.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index + "<br/>"
                        + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error);

                    importItem = new DishImportResultItem();
                    importItem.Name = itemError.Property;
                    importItem.ListFailStoreName.Add(itemError.StoreName);
                    importItem.ErrorItems.Add(item);
                    importItems.Add(importItem);
                }
                if (importItems.Count == 0)
                {
                    importItem = new DishImportResultItem();
                    importItem.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dishes");
                    importItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Dish Successful"));
                    importItems.Add(importItem);
                }
            }
            return importItems;
        }
        /*=====End Import */

        private int GetTypeForTabDish(string type)
        {
            int result = 0;
            if (type == Commons.EModifierType.Forced.ToString())
            {
                result = (int)Commons.EModifierType.Forced;
            }
            if (type == Commons.EModifierType.AdditionalDish.ToString())
            {
                result = (int)Commons.EModifierType.AdditionalDish;
            }
            if (type == Commons.EModifierType.AdditionalModifier.ToString())
            {
                result = (int)Commons.EModifierType.AdditionalModifier;
            }
            if (type == Commons.EModifierType.Optional.ToString())
            {
                result = (int)Commons.EModifierType.Optional;
            }
            if (type == Commons.EModifierType.Product.ToString())
            {
                result = (int)Commons.EModifierType.Product;
            }
            if (type == Commons.EModifierType.Special.ToString())
            {
                result = (int)Commons.EModifierType.Special;
            }
            return result;
        }
        public void ValidateRowSetMenu(ref List<SetMenuImportItem> lstSetMenu)
        {
            foreach (SetMenuImportItem item in lstSetMenu)
            {
                bool valid = true;
                if (item.SetMenuIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index must be more than 0"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.Name.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Name can not be null"));
                    valid = false;
                }

                if (valid)
                    item.Infor.IsValidRow = true;
                else
                    item.Infor.IsValidRow = false;
            }
        }

        public void ValidateRowTabSetMenu(ref List<TabImportItem> lstTab)
        {
            foreach (TabImportItem item in lstTab)
            {
                bool valid = true;
                if (item.TabIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index must be more than 0"));
                    valid = false;
                }

                if (item.SetMenuIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set Menu Index must be more than 0"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.Name.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name can not be null"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.DisplayMessage.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Display Message can not be null"));
                    valid = false;
                }

                if (item.Quantity < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity must be more than 0"));
                    valid = false;
                }

                if (valid)
                    item.Infor.IsValidRow = true;
                else
                    item.Infor.IsValidRow = false;
            }
        }

        public void ValidateRowDishSetMenu(ref List<DishTabImportItem> lstDish)
        {
            foreach (DishTabImportItem item in lstDish)
            {
                bool valid = true;
                if (item.DishIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Index must be more than 0"));
                    valid = false;
                }

                if (item.TabIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index must be more than 0"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.Name.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name can not be null"));
                    valid = false;
                }

                if (valid)
                    item.Infor.IsValidRow = true;
                else
                    item.Infor.IsValidRow = false;
            }
        }

        /*=============Dish*/
        public void ValidateRowDish(ref List<DishImportItem> dishes)
        {
            foreach (DishImportItem item in dishes)
            {
                bool valid = true;
                if (item.DishIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Index must be more than 0"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.Name.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Name can not be null"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.DishCode.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Code can not be null"));
                    valid = false;
                }

                if (item.Unit < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unit must be more than 0"));
                    valid = false;
                }

                if (item.Cost < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Cost must be more than 0"));
                    valid = false;
                }

                if (item.DefaultPrice < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Price must be more than 0"));
                    valid = false;
                }

                if (item.Quantity < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity must be more than 0"));
                    valid = false;
                }

                if (item.Limit < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Limit must be more than 0"));
                    valid = false;
                }

                if (item.PercentServiceCharge < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent Service Charge must be more than 0"));
                    valid = false;
                }

                if (valid)
                    item.Infor.IsValidRow = true;
                else
                    item.Infor.IsValidRow = false;
            }
        }

        public void ValidateRowGroupDish(ref List<DishGroupImportItem> groups)
        {
            foreach (DishGroupImportItem item in groups)
            {
                bool valid = true;
                if (item.GroupIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index must be more than 0"));
                    valid = false;
                }

                if (item.DishIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish Index must be more than 0"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.Name.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Name can not be null"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.DisplayMessage.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Display Message can not be null"));
                    valid = false;
                }

                if (item.Quantity < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity must be more than 0"));
                    valid = false;
                }

                if (item.Seq < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence must be more than 0"));
                    valid = false;
                }

                if (valid)
                    item.Infor.IsValidRow = true;
                else
                    item.Infor.IsValidRow = false;
            }
        }

        public void ValidateRowModifier(ref List<DishModifierImportItem> modifiers)
        {
            foreach (DishModifierImportItem item in modifiers)
            {
                bool valid = true;
                if (item.ModifierIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Index must be more than 0"));
                    valid = false;
                }

                if (item.TabIndex < 1)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tab Index must be more than 0"));
                    valid = false;
                }

                if (string.IsNullOrEmpty(item.Name.Trim()))
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier Name can not be null"));
                    valid = false;
                }

                if (item.Sequence < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence must be more than 0"));
                    valid = false;
                }

                if (item.ExtraPrice < 0)
                {
                    item.Infor.Errors.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Extra Price must be more than 0"));
                    valid = false;
                }

                if (valid)
                    item.Infor.IsValidRow = true;
                else
                    item.Infor.IsValidRow = false;
            }
        }

        #region ForReport
        public List<RFilterCategoryModel> GetAllSetMenuForReport(CategoryApiRequestModel request)
        {
            var lstData = new List<RFilterCategoryModel>();
            try
            {
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetSetMenuFilterForWeb, null, request);
                if (result.Success)
                {
                    dynamic data = result.Data;
                    var ListCate = data["ListCategories"];
                    RFilterCategoryModel obj = null;
                    foreach (var item in ListCate)
                    {
                        obj = new RFilterCategoryModel();
                        obj.Id = item["Id"];
                        obj.Name = item["Name"];
                        obj.StoreName = item["StoreName"];
                        obj.StoreId = item["StoreId"];
                        obj.Seq = item["Seq"];
                        if (CommonHelper.IsPropertyExist(item, "CategoryID"))
                        {
                            obj.CategoryID= item["CategoryID"];
                        }
                        else
                        {
                            obj.CategoryID = "SetMenu";
                        }
                        if (CommonHelper.IsPropertyExist(item, "CategoryName"))
                        {
                            obj.CategoryName = item["CategoryName"];
                        }
                        else
                        {
                            obj.CategoryName = "SetMenu";
                        }
                        lstData.Add(obj);

                        //lstData.Add(new RFilterCategoryModel
                        //{
                        //    Id = item["Id"],
                        //    Name = item["Name"],
                        //    StoreName = item["StoreName"],
                        //    StoreId = item["StoreId"],
                        //    Seq = item["Seq"]
                        //});
                       
                    }
                }
                lstData = lstData.OrderBy(oo => oo.StoreName).OrderBy(ww => ww.Seq).ToList();
                return lstData;
            }
            catch (Exception e)
            {
                _logger.Error("GetCate Report: " + e);
                return lstData;
            }
        }

        public List<RFilterCategoryModel> GetAllSetMenuForReportForMerchantExtend(List<CategoryApiRequestModel> lstRequest)
        {
            var lstData = new List<RFilterCategoryModel>();
            try
            {
                foreach (var request in lstRequest)
                {
                    var result = (ResponseApiModels)ApiResponse.PostWithoutHostConfig<ResponseApiModels>(request.HostUrl +"/"+ Commons.GetSetMenuFilterForWeb, null, request);
                    if (result.Success)
                    {
                        dynamic data = result.Data;
                        var ListCate = data["ListCategories"];
                        RFilterCategoryModel obj = null;
                        foreach (var item in ListCate)
                        {
                            obj = new RFilterCategoryModel();
                            obj.Id = item["Id"];
                            obj.Name = item["Name"];
                            obj.StoreName = item["StoreName"];
                            obj.StoreId = item["StoreId"];
                            obj.Seq = item["Seq"];
                            if (CommonHelper.IsPropertyExist(item, "CategoryID"))
                            {
                                obj.CategoryID = item["CategoryID"];
                            }
                            else
                            {
                                obj.CategoryID = "SetMenu";
                            }
                            if (CommonHelper.IsPropertyExist(item, "CategoryName"))
                            {
                                obj.CategoryName = item["CategoryName"];
                            }
                            else
                            {
                                obj.CategoryName = "SetMenu";
                            }
                            lstData.Add(obj);

                            //lstData.Add(new RFilterCategoryModel
                            //{
                            //    Id = item["Id"],
                            //    Name = item["Name"],
                            //    StoreName = item["StoreName"],
                            //    StoreId = item["StoreId"],
                            //    Seq = item["Seq"]
                            //});
                        }
                    }
                }
                lstData = lstData.OrderBy(oo => oo.StoreName).OrderBy(ww => ww.Seq).ToList();
                return lstData;
            }
            catch (Exception e)
            {
                _logger.Error("GetCate Report: " + e);
                return lstData;
            }
        }

        #endregion
    }

    public class ExportItemTab
    {
        public int TabIndex { get; set; }
        public int Seq { get; set; }
        public int SetMenuIndex { get; set; }
        public string SetMenuName { get; set; }
        public string TabName { get; set; }
        public string DisplayMsg { get; set; }
        public int Quantity { get; set; }
        public int Type { get; set; }
    }

    public class ExportItemDish
    {
        public int DishIndex { get; set; }
        public int Seq { get; set; }
        public int TabIndex { get; set; }
        public string DishName { get; set; }
        public double ExtraPrice { get; set; }
        public string Printer { get; set; }
    }
}
