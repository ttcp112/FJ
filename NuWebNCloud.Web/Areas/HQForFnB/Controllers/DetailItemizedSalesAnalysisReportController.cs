using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using NuWebNCloud.Web.Controllers;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class DetailItemizedSalesAnalysisReportController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        // GET: DetailItemizedSalesAnalysisReport
        public ActionResult Index()
        {
            BaseReportModel model = new BaseReportModel();
            return View(model);
        }

        public ActionResult Report(BaseReportModel reportModel)
        {
            try
            {
                if (reportModel.FromDate > reportModel.ToDate)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                else if (reportModel.Type == Commons.TypeCompanySelected) //Company
                {
                    if (reportModel.ListCompanys == null)
                        ModelState.AddModelError("ListCompanys", CurrentUser.GetLanguageTextFromKey("Please choose company."));
                }
                else //Store
                {
                    if (reportModel.ListStores == null)
                        ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                }

                if (!ModelState.IsValid)
                    return View("Index", reportModel);
                //Get Selected Store
                List<StoreModels> lstStore = new List<StoreModels>();
                ////if (reportModel.Type == Commons.TypeCompanySelected) //Company
                ////{
                ////    lstStore = reportModel.GetSelectedStoreCompany();
                ////}
                ////else //Store
                ////{
                ////    List<SelectListItem> vbStore = ViewBag.Stores;
                ////    lstStore = reportModel.GetSelectedStore(vbStore);
                ////}
                ///////======= Updated 072018
                if (reportModel.Type == Commons.TypeCompanySelected) //Company
                {
                    lstStore = listStoresInfoSession.Where(ww => reportModel.ListCompanys.Contains(ww.CompanyId)).ToList();
                    reportModel.ListStores = lstStore.Select(ss => ss.Id).ToList();
                }
                else //Store
                {
                    lstStore = listStoresInfoSession.Where(ww => reportModel.ListStores.Contains(ww.Id)).ToList();
                }
                //End Get Selected Store

                DetailItemizedSalesAnalysisReportHeaderFactory factory = new DetailItemizedSalesAnalysisReportHeaderFactory();

                DateTimeHelper.GetDateTime(ref reportModel);
                var categorys = factory.GetListCategory(reportModel);
                List<string> listCategoryIds = (from c in categorys select c.CategoryId).ToList<string>();

                var data = factory.GetData(reportModel, listCategoryIds);
                List<DateTime> listDates = new List<DateTime>();
                foreach (var item in data)
                {
                    listDates.Add(item.CreatedDate);
                }
                //List<DetailItemizedModel> listStores = new List<DetailItemizedModel>();
                //#region add dish to listStore
                //foreach (var item in listDishTemp)
                //{
                //    listDates.Add(item.DateCreated.Value);
                //    DetailItemizedModel valueCompare = new DetailItemizedModel()
                //    {
                //        DateCreated = item.DateCreated.Value,
                //        StoreIndex = item.StoreIndex,
                //        StoreName = item.StoreName,
                //        CateID = item.CategoryID,
                //        CateName = item.CatName,
                //        ItemID = item.DishID,
                //        ItemName = item.DishName,
                //        TypeID = 1,
                //        ItemQuantity = (item.DishQuantity == null) ? 0 : item.DishQuantity.Value,
                //        ItemPrice = item.DishPrice
                //    };
                //    int indexStore = IsExistItem(listStores, valueCompare, "store");
                //    if (indexStore == -1)
                //    {
                //        #region add new store
                //        DetailItemizedModel storeItem = new DetailItemizedModel()
                //        {
                //            StoreIndex = item.StoreIndex,
                //            StoreName = item.StoreName,
                //            ListChilds = new List<DetailItemizedModel>()
                //        };

                //        //add new category
                //        DetailItemizedModel cateItem = new DetailItemizedModel()
                //        {
                //            CateID = item.CategoryID,
                //            CateName = item.CatName,
                //            TypeID = 1,
                //            ListChilds = new List<DetailItemizedModel>()
                //        };

                //        //add new dish or setmenu
                //        DetailItemizedModel dishItem = new DetailItemizedModel()
                //        {
                //            ItemID = item.DishID,
                //            ItemName = item.DishName,
                //            ItemPrice = item.DishPrice,
                //            TypeID = valueCompare.TypeID,
                //            ListChilds = new List<DetailItemizedModel>(),
                //            ListSetChilds = new List<DetailItemizedModel>()
                //        };
                //        //add new date - quantity
                //        dishItem.ListChilds.Add(new DetailItemizedModel()
                //        {
                //            DateCreated = item.DateCreated.Value,
                //            ItemQuantity = item.DishQuantity.Value
                //        });

                //        cateItem.ListChilds.Add(dishItem);
                //        storeItem.ListChilds.Add(cateItem);
                //        listStores.Add(storeItem);
                //        #endregion add new store
                //    }
                //    else
                //    {
                //        int indexCate = IsExistItem(listStores[indexStore].ListChilds, valueCompare, "category");

                //        if (indexCate == -1)
                //        {
                //            #region add new category.
                //            //add new category
                //            DetailItemizedModel cateItem = new DetailItemizedModel()
                //            {
                //                CateID = item.CategoryID,
                //                CateName = item.CatName,
                //                TypeID = 1,
                //                ListChilds = new List<DetailItemizedModel>()
                //            };
                //            //add new dish or setmenu
                //            DetailItemizedModel dishItem = new DetailItemizedModel()
                //            {
                //                ItemID = item.DishID,
                //                ItemName = item.DishName,
                //                ItemPrice = item.DishPrice,
                //                TypeID = valueCompare.TypeID,
                //                ListChilds = new List<DetailItemizedModel>(),
                //                ListSetChilds = new List<DetailItemizedModel>()
                //            };
                //            //add new date - quantity
                //            dishItem.ListChilds.Add(new DetailItemizedModel()
                //            {
                //                DateCreated = item.DateCreated.Value,
                //                ItemQuantity = item.DishQuantity.Value
                //            });

                //            cateItem.ListChilds.Add(dishItem);
                //            listStores[indexStore].ListChilds.Add(cateItem);
                //            #endregion
                //        }
                //        else
                //        {
                //            int indexDishMenu = IsExistItem(listStores[indexStore].ListChilds[indexCate].ListChilds, valueCompare, "dishsetmenu");

                //            if (indexDishMenu == -1)
                //            {
                //                #region add new dish or setmenu.
                //                DetailItemizedModel dishItem = new DetailItemizedModel()
                //                {
                //                    ItemID = item.DishID,
                //                    ItemName = item.DishName,
                //                    ItemPrice = item.DishPrice,
                //                    TypeID = valueCompare.TypeID,
                //                    ListChilds = new List<DetailItemizedModel>(),
                //                    ListSetChilds = new List<DetailItemizedModel>()
                //                };
                //                //add new date - quantity
                //                dishItem.ListChilds.Add(new DetailItemizedModel()
                //                {
                //                    DateCreated = item.DateCreated.Value,
                //                    ItemQuantity = item.DishQuantity.Value
                //                });

                //                listStores[indexStore].ListChilds[indexCate].ListChilds.Add(dishItem);
                //                #endregion
                //            }
                //            else
                //            {
                //                //add new date - quantity
                //                listStores[indexStore].ListChilds[indexCate].ListChilds[indexDishMenu].ListChilds.Add(new DetailItemizedModel()
                //                {
                //                    DateCreated = item.DateCreated.Value,
                //                    ItemQuantity = item.DishQuantity.Value
                //                });
                //            }
                //        }
                //    }
                //}
                //#endregion

                //#region add setmenu to listStore
                //foreach (var item in querySetMenus)
                //{
                //    listDates.Add(item.DateCreated.Value);
                //    DetailItemizedModel valueCompare = new DetailItemizedModel()
                //    {
                //        DateCreated = item.DateCreated.Value,
                //        StoreIndex = item.StoreIndex,
                //        StoreName = item.StoreName,
                //        CateID = "SetMenu",
                //        CateName = "SetMenu",
                //        ItemID = item.SetID,
                //        ItemName = item.SetName,
                //        TypeID = 4,
                //        ItemQuantity = item.SetQuantity.Value,
                //        ItemPrice = item.SetPrice.Value
                //    };

                //    int indexStore = IsExistItem(listStores, valueCompare, "store");
                //    //if store is not exist => add new store
                //    if (indexStore == -1)
                //    {
                //        #region add new store
                //        DetailItemizedModel storeItem = new DetailItemizedModel()
                //        {
                //            StoreIndex = item.StoreIndex,
                //            StoreName = item.StoreName,
                //            ListChilds = new List<DetailItemizedModel>()
                //        };

                //        //add new category
                //        DetailItemizedModel cateItem = new DetailItemizedModel()
                //        {
                //            CateID = "SetMenu",
                //            CateName = "SetMenu",
                //            TypeID = 4,
                //            ListChilds = new List<DetailItemizedModel>()
                //        };

                //        //add new dish or setmenu
                //        DetailItemizedModel setmenuItem = new DetailItemizedModel()
                //        {
                //            ItemID = item.SetID,
                //            ItemName = item.SetName,
                //            ItemPrice = item.SetPrice.Value,
                //            TypeID = valueCompare.TypeID,
                //            ListChilds = new List<DetailItemizedModel>(),
                //            ListSetChilds = new List<DetailItemizedModel>()
                //        };
                //        //add new date - quantity
                //        DetailItemizedModel dishItem = new DetailItemizedModel()
                //        {
                //            DateCreated = item.DateCreated.Value,
                //            ItemQuantity = item.SetQuantity.Value
                //        };
                //        dishItem.ReceiptDetailIDs.Add(item.ReceiptDetailID);
                //        setmenuItem.ListChilds.Add(dishItem);
                //        cateItem.ListChilds.Add(setmenuItem);
                //        storeItem.ListChilds.Add(cateItem);
                //        listStores.Add(storeItem);
                //        #endregion add new store
                //    }
                //    else
                //    {
                //        int indexCate = IsExistItem(listStores[indexStore].ListChilds, valueCompare, "category");
                //        //if category is not exist => add new category
                //        if (indexCate == -1)
                //        {
                //            #region add new category
                //            //add new category
                //            DetailItemizedModel cateItem = new DetailItemizedModel()
                //            {
                //                CateID = "SetMenu",
                //                CateName = "SetMenu",
                //                TypeID = 4,
                //                ListChilds = new List<DetailItemizedModel>()
                //            };

                //            //add new dish or setmenu
                //            DetailItemizedModel setmenuItem = new DetailItemizedModel()
                //            {
                //                ItemID = item.SetID,
                //                ItemName = item.SetName,
                //                ItemPrice = item.SetPrice.Value,
                //                TypeID = valueCompare.TypeID,
                //                ListChilds = new List<DetailItemizedModel>(),
                //                ListSetChilds = new List<DetailItemizedModel>()
                //            };
                //            //add new date - quantity
                //            DetailItemizedModel dishItem = new DetailItemizedModel()
                //            {
                //                DateCreated = item.DateCreated.Value,
                //                ItemQuantity = item.SetQuantity.Value
                //            };
                //            dishItem.ReceiptDetailIDs.Add(item.ReceiptDetailID);
                //            setmenuItem.ListChilds.Add(dishItem);
                //            cateItem.ListChilds.Add(setmenuItem);
                //            listStores[indexStore].ListChilds.Add(cateItem);
                //            #endregion
                //        }
                //        else
                //        {
                //            int indexSetMenu = IsExistItem(listStores[indexStore].ListChilds[indexCate].ListChilds, valueCompare, "dishsetmenu");

                //            if (indexSetMenu == -1)
                //            {
                //                #region add new dish or setmenu.
                //                DetailItemizedModel setmenuItem = new DetailItemizedModel()
                //                {
                //                    ItemID = item.SetID,
                //                    ItemName = item.SetName,
                //                    ItemPrice = item.SetPrice.Value,
                //                    TypeID = valueCompare.TypeID,
                //                    ListChilds = new List<DetailItemizedModel>(),
                //                    ListSetChilds = new List<DetailItemizedModel>()
                //                };
                //                //add new date - quantity
                //                DetailItemizedModel dishItem = new DetailItemizedModel()
                //                {
                //                    DateCreated = item.DateCreated.Value,
                //                    ItemQuantity = item.SetQuantity.Value
                //                };
                //                dishItem.ReceiptDetailIDs.Add(item.ReceiptDetailID);
                //                setmenuItem.ListChilds.Add(dishItem);
                //                listStores[indexStore].ListChilds[indexCate].ListChilds.Add(setmenuItem);
                //                #endregion
                //            }
                //            else
                //            {
                //                int indexDate = IsExistItem(listStores[indexStore].ListChilds[indexCate].ListChilds[indexSetMenu].ListChilds, valueCompare, "date");
                //                if (indexDate == -1)
                //                {
                //                    //add new date - quantity
                //                    DetailItemizedModel dishItem = new DetailItemizedModel()
                //                    {
                //                        DateCreated = item.DateCreated.Value,
                //                        ItemQuantity = item.SetQuantity.Value
                //                    };
                //                    dishItem.ReceiptDetailIDs.Add(item.ReceiptDetailID);
                //                    listStores[indexStore].ListChilds[indexCate].ListChilds[indexSetMenu].ListChilds.Add(dishItem);
                //                }
                //                else
                //                {
                //                    if (listStores[indexStore].ListChilds[indexCate].ListChilds[indexSetMenu].ListChilds[indexDate].ReceiptDetailIDs.IndexOf(item.ReceiptDetailID) < 0)
                //                    {
                //                        listStores[indexStore].ListChilds[indexCate].ListChilds[indexSetMenu].ListChilds[indexDate].ItemQuantity += item.SetQuantity.Value;
                //                        listStores[indexStore].ListChilds[indexCate].ListChilds[indexSetMenu].ListChilds[indexDate].ReceiptDetailIDs.Add(item.ReceiptDetailID);
                //                    }
                //                }

                //            }
                //        }
                //    }
                //}
                //#endregion

                //#region add dish to setmenu of listStore
                //foreach (var item in querySetMenus)
                //{
                //    DetailItemizedModel valueCompare = new DetailItemizedModel()
                //    {
                //        DateCreated = item.DateCreated.HasValue ? item.DateCreated.Value : DateTime.MinValue,
                //        StoreIndex = item.StoreIndex,
                //        StoreName = item.StoreName,
                //        CateID = "SetMenu",
                //        CateName = "SetMenu",
                //        ItemID = item.SetID,
                //        ItemName = item.SetName,
                //        TypeID = 4,
                //        ItemQuantity = item.DishQuantity.HasValue ? item.DishQuantity.Value : 0,
                //        ItemPrice = item.SetPrice.HasValue ? item.SetPrice.Value : 0
                //    };
                //    DetailItemizedModel valueCompareDish = new DetailItemizedModel()
                //    {
                //        DateCreated = item.DateCreated.HasValue ? item.DateCreated.Value : DateTime.MinValue,
                //        StoreIndex = item.StoreIndex,
                //        StoreName = item.StoreName,
                //        ItemID = item.DishID,
                //        ItemName = item.DishName,
                //        TypeID = 41,
                //        ItemQuantity = item.DishQuantity.HasValue ? item.DishQuantity.Value : 0,
                //        ItemPrice = item.DishPrice
                //    };

                //    int indexStore = IsExistItem(listStores, valueCompare, "store");
                //    int indexCate = IsExistItem(listStores[indexStore].ListChilds, valueCompare, "category");
                //    int indexDishMenu = IsExistItem(listStores[indexStore].ListChilds[indexCate].ListChilds, valueCompare, "dishsetmenu");
                //    int indexSetChild = IsExistItem(listStores[indexStore].ListChilds[indexCate].ListChilds[indexDishMenu].ListSetChilds, valueCompareDish, "setchild");
                //    if (indexSetChild == -1)
                //    {
                //        //add new setmenu child
                //        listStores[indexStore].ListChilds[indexCate].ListChilds[indexDishMenu].ListSetChilds.Add(new DetailItemizedModel()
                //        {
                //            CateID = item.CatID,//
                //            CateName = item.CatName,//
                //            ItemID = item.DishID,
                //            ItemName = item.DishName,
                //            ItemPrice = item.DishPrice,
                //            TypeID = valueCompareDish.TypeID,
                //            ListChilds = new List<DetailItemizedModel>()
                //        });
                //        //add new date - quantity
                //        listStores[indexStore].ListChilds[indexCate].ListChilds[indexDishMenu].ListSetChilds.Last().ListChilds.Add(new DetailItemizedModel()
                //        {
                //            DateCreated = item.DateCreated.HasValue ? item.DateCreated.Value : DateTime.MinValue,
                //            ItemQuantity = item.DishQuantity.HasValue ? item.DishQuantity.Value : 0
                //        });
                //    }
                //    else
                //    {
                //        //add new date - quantity
                //        DetailItemizedModel child = listStores[indexStore].ListChilds[indexCate].ListChilds[indexDishMenu].ListSetChilds[indexSetChild].ListChilds.Where(c => c.DateCreated == item.DateCreated).FirstOrDefault();
                //        if (child != null)
                //            child.ItemQuantity += item.DishQuantity.HasValue ? item.DishQuantity.Value : 0;
                //        else
                //            listStores[indexStore].ListChilds[indexCate].ListChilds[indexDishMenu].ListSetChilds[indexSetChild].ListChilds.Add(new DetailItemizedModel()
                //            {
                //                DateCreated = item.DateCreated.HasValue ? item.DateCreated.Value : DateTime.MinValue,
                //                ItemQuantity = item.DishQuantity.HasValue ? item.DishQuantity.Value : 0
                //            });
                //    }
                //}
                //#endregion

                listDates = listDates.Distinct().Select(item => item).ToList();
                listDates = listDates.OrderBy(item => item).ToList();
                XLWorkbook wb = factory.ExportExcel(reportModel, listDates, data, lstStore);

                string sheetName = string.Format("Report_Detail_Itemized_Sales_Analysis_{0}", DateTime.Now.ToString("MMddyyyy")).Replace(" ", "_");
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                //HttpContext.Response.Clear();
                Response.Charset = System.Text.Encoding.UTF8.WebName;
                Response.ContentEncoding = System.Text.Encoding.UTF8;

                if (reportModel.FormatExport.Equals(Commons.Html))
                {
                    Response.AddHeader("content-disposition", string.Format(@"attachment;filename={0}.html", sheetName));
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", string.Format(@"attachment;filename={0}.xlsx", sheetName));
                }
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    if (reportModel.FormatExport.Equals(Commons.Html))
                    {
                        Workbook workbook = new Workbook();
                        workbook.LoadFromStream(memoryStream);
                        //convert Excel to HTML
                        Worksheet sheet = workbook.Worksheets[0];
                        using (var ms = new MemoryStream())
                        {
                            sheet.SaveToHtml(ms);
                            ms.WriteTo(HttpContext.Response.OutputStream);
                            ms.Close();
                        }
                    }
                    else
                    {
                        memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    }
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error("Detail Itemized Sales Analysis Report Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        

        //public int IsExistItem(List<DetailItemizedModel> listItems, DetailItemizedModel valueCompare, string action)
        //{
        //    for (int i = 0; i < listItems.Count; i++)
        //    {
        //        switch (action)
        //        {
        //            case "store":
        //                if (listItems[i].StoreIndex == valueCompare.StoreIndex)
        //                    return i;
        //                break;
        //            case "category":
        //                if (listItems[i].CateID == valueCompare.CateID)
        //                    return i;
        //                break;
        //            case "dishsetmenu":
        //                if (listItems[i].TypeID == valueCompare.TypeID && listItems[i].ItemID == valueCompare.ItemID && listItems[i].ItemName == valueCompare.ItemName)
        //                    return i;
        //                break;
        //            case "date":
        //                if (listItems[i].DateCreated == valueCompare.DateCreated)
        //                    return i;
        //                break;
        //            case "setchild":
        //                if (listItems[i].TypeID == valueCompare.TypeID && listItems[i].ItemID == valueCompare.ItemID && listItems[i].ItemName == valueCompare.ItemName)
        //                    return i;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    return -1;
        //}

        //private void GetListSetMenus(List<DateTime> listDates, List<DetailItemizedModel> listStores, List<sp_Report_DetailItemizedSalesAnalysis_SetMenuResult> listSetMenuTemp)
        //{
        //    foreach (var item in listSetMenuTemp)
        //    {
        //        listDates.Add(item.DateCreated.Value);
        //        DetailItemizedModel valueCompare = new DetailItemizedModel()
        //        {
        //            DateCreated = item.DateCreated.Value,
        //            StoreIndex = item.StoreIndex,
        //            StoreName = item.StoreName,
        //            ItemID = item.SetID,
        //            ItemName = item.SetName,
        //            TypeID = 4,
        //            ItemQuantity = item.SetQuantity.Value,
        //            ItemPrice = item.SetPrice.Value
        //        };
        //        int indexStore = IsExistItem(listStores, valueCompare, "store");
        //        if (indexStore == -1)
        //        {
        //            #region add new store
        //            DetailItemizedModel storeItem = new DetailItemizedModel()
        //            {
        //                StoreIndex = item.StoreIndex,
        //                StoreName = item.StoreName,
        //                ListChilds = new List<DetailItemizedModel>()
        //            };
        //            //add new dish or setmenu
        //            DetailItemizedModel setmenuItem = new DetailItemizedModel()
        //            {
        //                ItemID = item.SetID,
        //                ItemName = item.SetName,
        //                ItemPrice = item.SetPrice.Value,
        //                TypeID = valueCompare.TypeID,
        //                ListChilds = new List<DetailItemizedModel>(),
        //                ListSetChilds = new List<DetailItemizedModel>()
        //            };
        //            //add new date - quantity
        //            setmenuItem.ListChilds.Add(new DetailItemizedModel()
        //            {
        //                DateCreated = item.DateCreated.Value,
        //                ItemQuantity = item.SetQuantity.Value
        //            });
        //            storeItem.ListChilds.Add(setmenuItem);
        //            #endregion add new store
        //        }
        //        else
        //        {
        //            int indexDishMenu = IsExistItem(listStores[indexStore].ListChilds, valueCompare, "dishsetmenu");
        //            if (indexDishMenu == -1)
        //            {
        //                #region add new dish or setmenu.
        //                DetailItemizedModel setmenuItem = new DetailItemizedModel()
        //                {
        //                    ItemID = item.SetID,
        //                    ItemName = item.SetName,
        //                    ItemPrice = item.SetPrice.Value,
        //                    TypeID = valueCompare.TypeID,
        //                    ListChilds = new List<DetailItemizedModel>(),
        //                    ListSetChilds = new List<DetailItemizedModel>()
        //                };
        //                //add new date - quantity
        //                setmenuItem.ListChilds.Add(new DetailItemizedModel()
        //                {
        //                    DateCreated = item.DateCreated.Value,
        //                    ItemQuantity = item.SetQuantity.Value
        //                });
        //                listStores[indexStore].ListChilds.Add(setmenuItem);
        //                #endregion
        //            }
        //            else
        //            {
        //                int indexDate = IsExistItem(listStores[indexStore].ListChilds[indexDishMenu].ListChilds, valueCompare, "date");
        //                if (indexDate == -1)
        //                {
        //                    //add new date - quantity
        //                    listStores[indexStore].ListChilds[indexDishMenu].ListChilds.Add(new DetailItemizedModel()
        //                    {
        //                        DateCreated = item.DateCreated.Value,
        //                        ItemQuantity = item.SetQuantity.Value
        //                    });
        //                }
        //                else
        //                {
        //                    listStores[indexStore].ListChilds[indexDishMenu].ListChilds[indexDate].ItemQuantity += item.SetQuantity.Value;
        //                }
        //            }
        //        }
        //    }
        //}

    }
}