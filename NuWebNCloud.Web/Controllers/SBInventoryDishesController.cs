using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models.Settings.Season;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SBInventoryDishesController : SBInventoryBaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private ProductFactory _factory = null;
        List<string> listPropertyReject = null;

        public SBInventoryDishesController()
        {
            _factory = new ProductFactory();
            ViewBag.ListStore = GetListStore();
            listPropertyReject = new List<string>();
            listPropertyReject.Add("ExpiredDate");
        }

        public ActionResult Index()
        {
            try
            {
                ProductViewModels model = new ProductViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Product_Dish_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(ProductViewModels model)
        {
            try
            {
                var datas = _factory.GetListProduct(model.StoreID, (byte)Commons.EProductType.Dish, CurrentUser.ListOrganizationId);
                foreach (var item in datas)
                {
                    item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
                }
                model.ListItem = datas;
            }
            catch (Exception e)
            {
                _logger.Error("Product_Dish_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            ProductModels model = new ProductModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ProductModels model)
        {
            try
            {
                byte[] photoByte = null;
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store."));

                if (string.IsNullOrEmpty(model.CategoryID))
                    ModelState.AddModelError("CategoryID", CurrentUser.GetLanguageTextFromKey("Please choose Category for store"));

                if (model.ListPrices[0].Price < 0)
                    ModelState.AddModelError("ListPrices[0].Price", CurrentUser.GetLanguageTextFromKey("Please input default price greater than 0"));
                else
                {
                    if (model.Cost > model.ListPrices[0].Price)
                    {
                        ModelState.AddModelError("ListPrices[0].Price", CurrentUser.GetLanguageTextFromKey("Default Price must be larger than cost"));
                    }
                }

                for (int i = 1; i < model.ListPrices.Count; i++)
                    if (model.ListPrices[i].Price > 0 && string.IsNullOrEmpty(model.ListPrices[i].SeasonID))
                        ModelState.AddModelError(string.Format("ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please select Season for Price"));

                for (int i = 1; i < model.ListPrices.Count; i++)
                    if (!string.IsNullOrEmpty(model.ListPrices[i].SeasonID) && model.ListPrices[i].Price < 0)
                        ModelState.AddModelError(string.Format("ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please input price greater than 0"));

                //============
                int countSeasonKiosk = model.ListSeason.Count;
                var lstChooseSeasonKiosk = model.ListSeason.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                int countChooseSeasonKiosk = lstChooseSeasonKiosk.Count;
                if (countSeasonKiosk != countChooseSeasonKiosk)
                {
                    foreach (var item in lstChooseSeasonKiosk)
                    {
                        model.ListProductSeason.Add(new ProductSeasonDTO
                        {
                            SeasonID = item.ID,
                            IsPOS = false
                        });
                    }
                }

                int countSeasonPOS = model.ListSeasonPOS.Count;
                var lstChooseSeasonPOS = model.ListSeasonPOS.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                int countChooseSeasonPOS = lstChooseSeasonPOS.Count;
                if (countSeasonPOS != countChooseSeasonPOS)
                {
                    foreach (var item in lstChooseSeasonPOS)
                    {
                        model.ListProductSeason.Add(new ProductSeasonDTO
                        {
                            SeasonID = item.ID,
                            IsPOS = true
                        });
                    }
                }

                //============
                if (model.IsForce)
                {
                    if (model.ListGroup == null)
                    {
                        ModelState.AddModelError("Dish", CurrentUser.GetLanguageTextFromKey("You have just enable Force modifier. Please define details of modifier list."));
                    }
                    else
                    {
                        //int countItem = model.ListGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList().Count;

                        // Updated 09112017
                        int countItem = model.ListGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted && x.Type == (byte)Commons.EModifierType.Forced).ToList().Count;
                        if (countItem == 0)
                        {
                            ModelState.AddModelError("Dish", CurrentUser.GetLanguageTextFromKey("You have just enable Force modifier. Please define details of modifier list."));
                        }
                    }
                }
                if (model.ListGroup != null)
                {
                    foreach (var item in model.ListGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList())
                    {
                        int qty = item.Maximum;
                        string tabName = item.Name;
                        int modifierType = item.Type;
                        if (item.Sequence < 0)
                        {
                            ModelState.AddModelError("Dish", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                        }
                        // Updated 09112017
                        if (string.IsNullOrEmpty(item.Name))
                        {
                            ModelState.AddModelError("ListGroup[" + item.OffSet + "].Name", CurrentUser.GetLanguageTextFromKey("Name field is required"));
                        }
                        if (modifierType == (byte)Commons.EModifierType.Forced)
                        {
                            if (item.ListProductOnGroup != null)
                            {
                                int listItem = item.ListProductOnGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList().Count;

                                if (listItem < qty)
                                {
                                    ModelState.AddModelError("Dish", "Number of modifiers of 'Tab Name [" + tabName
                                        + "]' must be more than or equal Quantity of Tab Name [" + tabName + "]' equal " + qty + "");
                                    break;
                                }
                                foreach (var ProductOnGroup in item.ListProductOnGroup)
                                {
                                    if (ProductOnGroup.ExtraPrice < 0)
                                    {
                                        ModelState.AddModelError("Dish",
                                            "Please enter a value Extra Price of [" + ProductOnGroup.ProductName + "] greater than or equal to 0");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                //============

                if (model.ExpiredDate == null)
                    model.ExpiredDate = Commons._ExpiredDate;
                else
                    //model.ExpiredDate = new DateTime(model.ExpiredDate.Value.Year, model.ExpiredDate.Value.Month, model.ExpiredDate.Value.Day, 12, 59, 59);
                    model.ExpiredDate = new DateTime(model.ExpiredDate.Value.Year, model.ExpiredDate.Value.Month, model.ExpiredDate.Value.Day, 12, 0, 0, DateTimeKind.Utc);

                PropertyReject();

                //Return Data
                model.ListPrices = GetPrices(model.ListPrices, model.StoreID);
                model.ListCategories = GetSelectListCategories(model.StoreID, Commons.EProductType.Dish.ToString("d"));

                //model.LstPrinter = GetSelectListPrinters(model.StoreID, model.ListPrinter);

                if (model.HasServiceCharge)
                {
                    if (string.IsNullOrEmpty(model.sServiceCharge))
                        model.sServiceCharge = "0";
                    model.ServiceCharge = double.Parse(model.sServiceCharge ?? "0");
                    if (model.ServiceCharge < 0)
                        ModelState.AddModelError("ServiceCharge", CurrentUser.GetLanguageTextFromKey("Please input default price greater than 0"));
                    else if (model.ServiceCharge > 100)
                        ModelState.AddModelError("ServiceCharge", CurrentUser.GetLanguageTextFromKey("Maximum service charge is 100%"));
                }
                else
                    model.ServiceCharge = -1;

                //Get List Printer Name
                var lstPrinter = model.LstPrinter.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                foreach (var item in lstPrinter)
                {
                    model.ListPrinter.Add(new PrinterOnProductModels
                    {
                        PrinterID = item.Id,
                        PrinterName = item.PrinterName,
                        IsActive = true
                    });
                }
                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                    photoByte = imgByte;
                }

                model.ProductTypeID = Commons.EProductType.Dish.ToString("d");
                model.ProductTypeCode = (byte)Commons.EProductType.Dish;
                //--------
                if (model.ListGroup != null)
                {
                    model.ListGroup = model.ListGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();

                    for (int i = 0; i < model.ListGroup.Count; i++)
                    {
                        if (model.ListGroup[i].ListProductOnGroup != null)
                            model.ListGroup[i].ListProductOnGroup = model.ListGroup[i].ListProductOnGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                    }
                }

                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";

                    // Updated 08282017
                    model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Dish.ToString("d"));
                    if (!string.IsNullOrEmpty(model.CategoryID))
                    {
                        var cateChoose = model.lstCateGroup.Where(w => w.Id == model.CategoryID).FirstOrDefault();
                        if (cateChoose != null)
                        {
                            cateChoose.Selected = true;
                        }
                    }

                    return View(model);
                }
                //====================
                string msg = "";
                var tmp = model.PictureByte;
                model.PictureByte = null;
                bool result = _factory.InsertOrUpdateProduct(model, ref msg);
                if (result)
                {
                    model.PictureByte = tmp;
                    //Save Image on Server
                    if (!string.IsNullOrEmpty(model.ImageURL) && model.PictureByte != null)
                    {
                        var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                        var path = string.Format("{0}{1}", originalDirectory, model.ImageURL);
                        MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                        ms.Write(photoByte, 0, photoByte.Length);
                        System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                        ImageHelper.Me.SaveCroppedImage(imageTmp, path, model.ImageURL, ref photoByte);
                        model.PictureByte = photoByte;

                        FTP.Upload(model.ImageURL, model.PictureByte);

                        ImageHelper.Me.TryDeleteImageUpdated(path);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("name", msg);

                    // Updated 08282017
                    model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Dish.ToString("d"));
                    if (!string.IsNullOrEmpty(model.CategoryID))
                    {
                        var cateChoose = model.lstCateGroup.Where(w => w.Id == model.CategoryID).FirstOrDefault();
                        if (cateChoose != null)
                        {
                            cateChoose.Selected = true;
                        }
                    }

                    return View(model);
                    //return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Product_Dish_Create: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);
                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey("Have an error"));

                // Updated 08282017
                model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Dish.ToString("d"));
                if (!string.IsNullOrEmpty(model.CategoryID))
                {
                    var cateChoose = model.lstCateGroup.Where(w => w.Id == model.CategoryID).FirstOrDefault();
                    if (cateChoose != null)
                    {
                        cateChoose.Selected = true;
                    }
                }
                return View(model);
            }
        }

        public ProductModels GetDetail(string id)
        {
            try
            {
                ProductModels model = _factory.GetProductDetail(id);
               
                if (model.ListGroup != null)
                {
                    List<GroupProductModels> groupP = model.ListGroup;
                    for (int i = 0; i < model.ListGroup.Count; i++)
                    {
                        model.ListGroup[i].OffSet = i;
                        model.CurrencySymbol = CurrentUser.CurrencySymbol;
                        if (model.ListGroup[i].ListProductOnGroup != null)
                        {
                            for (int j = 0; j < model.ListGroup[i].ListProductOnGroup.Count; j++)
                            {
                                model.ListGroup[i].ListProductOnGroup[j].OffSet = j;
                                //model.ListGroup[i].ListProductOnGroup[j].Seq = model.ListGroup[i].ListProductOnGroup[j].Sequence;
                            }
                        }
                    }
                }
                model.sServiceCharge = model.ServiceCharge == -1 ? "" : model.ServiceCharge.ToString();
                model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;

                //================Get Category
                CategoriesFactory categoryf = new CategoriesFactory();
                //List<CategoriesModels> lstCg = categoryf.GetListCategory(model.StoreID, null, Commons.EProductType.Dish.ToString("d"), CurrentUser.ListOrganizationId);
                //SelectListItem sslItemCag = new SelectListItem();
                //foreach (var category in lstCg)
                //{
                //    if (category.ListChild != null)
                //    {
                //        foreach (var item in category.ListChild)
                //        {
                //            sslItemCag = new SelectListItem();
                //            sslItemCag.Value = item.ID;
                //            sslItemCag.Text = item.Name;// + " [" + item.StoreName + "]";
                //            sslItemCag.Selected = item.ID.Equals(model.CategoryID);
                //            model.ListCategories.Add(sslItemCag);
                //        }
                //    }
                //    else
                //    {
                //        sslItemCag = new SelectListItem();
                //        sslItemCag.Value = category.ID;
                //        sslItemCag.Text = category.Name;// + " [" + category.StoreName + "]";
                //        sslItemCag.Selected = category.ID.Equals(model.CategoryID);
                //        model.ListCategories.Add(sslItemCag);
                //    }
                //}

                // Updated 08282017
                List<SBInventoryBaseCateGroupViewModel> lstCateGroup = categoryf.GetListCategorySortParent(model.StoreID, null, Commons.EProductType.Dish.ToString("d"), CurrentUser.ListOrganizationId, model.CategoryID);

                model.lstCateGroup = lstCateGroup;

                //======= TImeSlot Kiosk
                var ListProductSeasonKiosk = model.ListProductSeason.Where(x => x.IsPOS == false).ToList();
                model.ListSeason = GetListTimeSlot02(model.StoreID, ListProductSeasonKiosk);
                if (ListProductSeasonKiosk != null && ListProductSeasonKiosk.Count != 0)
                {
                    model.ProductSeason = string.Join(",", ListProductSeasonKiosk.Select(x => x.SeasonName).ToList());
                }
                else if (ListProductSeasonKiosk.Count == 0)
                {
                    model.ProductSeason = "All";
                }

                //======= TImeSlot POS
                var ListProductSeasonPOS = model.ListProductSeason.Where(x => x.IsPOS).ToList();
                model.ListSeasonPOS = GetListTimeSlot02(model.StoreID, ListProductSeasonPOS, true);
                if (ListProductSeasonPOS != null && ListProductSeasonPOS.Count != 0)
                {
                    model.ProductSeasonPOS = string.Join(",", ListProductSeasonPOS.Select(x => x.SeasonName).ToList());
                }
                else if (ListProductSeasonPOS.Count == 0)
                {
                    model.ProductSeasonPOS = "All";
                }
                //================Price
                SeasonFactory seasonf = new SeasonFactory();
                List<SeasonModels> lstSs = seasonf.GetListSeason(model.StoreID, null, CurrentUser.ListOrganizationId);
                List<SelectListItem> lstSllItemSeason = new List<SelectListItem>();
                SelectListItem sslItemSeason;
                foreach (var season in lstSs)
                {
                    sslItemSeason = new SelectListItem();
                    sslItemSeason.Value = season.ID;
                    sslItemSeason.Text = season.Name;// + " [" + season.StoreName + "]";
                    sslItemSeason.Selected = season.ID.Equals(model.SeasonPriceID);
                    lstSllItemSeason.Add(sslItemSeason);
                }
                foreach (PriceItem priceItem in model.ListPrices)
                    priceItem.ListSeasons = lstSllItemSeason;
                model.ListPrices[0].Price = model.DefaultPrice;
                model.ListPrices[1].Price = model.SeasonPrice == -1 ? 0 : model.SeasonPrice;
                model.ListPrices[1].SeasonID = model.SeasonPriceID;

                //===========Printer
                model.LstPrinter = GetSelectListPrinters(model.StoreID, model.ListPrinter);

                model.ListPrinter.ForEach(x =>
                {
                    x.PrinterName = x.PrinterName;// + "[" + model.StoreName + "]";
                });

                //if (model.ListPrinter != null && model.ListPrinter.Count != 0)
                //{
                //    model.Printer = string.Join(",", model.ListPrinter.Select(x => x.PrinterName).ToList());
                //}

                // Filter list printer is actived (selected)
                model.ListPrinter = model.ListPrinter.Where(w => w.IsActive).ToList();
                if (model.ListPrinter != null && model.ListPrinter.Any())
                {
                    model.Printer = string.Join(",", model.ListPrinter.Select(x => x.PrinterName).ToList());
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Product_Dish_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            ProductModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            ProductModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        public void PropertyReject()
        {
            foreach (var item in listPropertyReject)
            {
                if (ModelState.ContainsKey(item))
                    ModelState[item].Errors.Clear();
            }
        }

        [HttpPost]
        public ActionResult Edit(ProductModels model)
        {
            try
            {
                byte[] photoByte = null;
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Dish Name field is required"));
                if (string.IsNullOrEmpty(model.CategoryID))
                    ModelState.AddModelError("CategoryID", CurrentUser.GetLanguageTextFromKey("Please choose Category for store"));

                // Updated 08282017
                string cateIdChoose = null;
                if (!string.IsNullOrEmpty(model.CategoryID))
                {
                    cateIdChoose = model.CategoryID;
                }

                if (model.ListPrices[0].Price < 0)
                    ModelState.AddModelError("ListPrices[0].Price", CurrentUser.GetLanguageTextFromKey("Please input default price greater than 0"));
                else
                {
                    if (model.Cost > model.ListPrices[0].Price)
                    {
                        ModelState.AddModelError("ListPrices[0].Price", CurrentUser.GetLanguageTextFromKey("Default Price must be larger than cost"));
                    }
                }
                for (int i = 1; i < model.ListPrices.Count; i++)
                    if (model.ListPrices[i].Price > 0 && string.IsNullOrEmpty(model.ListPrices[i].SeasonID))
                        ModelState.AddModelError(string.Format("ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please select Season for Price"));

                for (int i = 1; i < model.ListPrices.Count; i++)
                    if (!string.IsNullOrEmpty(model.ListPrices[i].SeasonID) && model.ListPrices[i].Price < 0)
                        ModelState.AddModelError(string.Format("ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please input price greater than 0"));

                //============
                int countSeasonKiosk = model.ListSeason.Count;
                var lstChooseSeasonKiosk = model.ListSeason.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                int countChooseSeasonKiosk = lstChooseSeasonKiosk.Count;
                if (countSeasonKiosk != countChooseSeasonKiosk)
                {
                    foreach (var item in lstChooseSeasonKiosk)
                    {
                        model.ListProductSeason.Add(new ProductSeasonDTO
                        {
                            SeasonID = item.ID,
                            IsPOS = false
                        });
                    }
                }

                int countSeasonPOS = model.ListSeasonPOS.Count;
                var lstChooseSeasonPOS = model.ListSeasonPOS.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                int countChooseSeasonPOS = lstChooseSeasonPOS.Count;
                if (countSeasonPOS != countChooseSeasonPOS)
                {
                    foreach (var item in lstChooseSeasonPOS)
                    {
                        model.ListProductSeason.Add(new ProductSeasonDTO
                        {
                            SeasonID = item.ID,
                            IsPOS = true,
                        });
                    }
                }


                if (model.IsForce)
                {
                    if (model.ListGroup == null)
                    {
                        ModelState.AddModelError("Dish", CurrentUser.GetLanguageTextFromKey("You have just enable Force modifier. Please define details of modifier list."));
                    }
                    else
                    {
                        //int countItem = model.ListGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList().Count;
                        
                        // Updated 09112017
                        int countItem = model.ListGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted && x.Type == (byte)Commons.EModifierType.Forced).ToList().Count;
                        if (countItem == 0)
                        {
                            ModelState.AddModelError("Dish", CurrentUser.GetLanguageTextFromKey("You have just enable Force modifier. Please define details of modifier list."));
                        }
                    }
                }
                if (model.ListGroup != null)
                {
                    foreach (var item in model.ListGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList())
                    {
                        int qty = item.Maximum;
                        string tabName = item.Name;
                        int modifierType = item.Type;
                        if (item.Sequence < 0)
                        {
                            ModelState.AddModelError("Dish", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                        }
                        // Updated 09112017
                        if (string.IsNullOrEmpty(item.Name))
                        {
                            ModelState.AddModelError("ListGroup[" + item.OffSet + "].Name", CurrentUser.GetLanguageTextFromKey("Name field is required"));
                        }
                        if (modifierType == (byte)Commons.EModifierType.Forced)
                        {
                            if (item.ListProductOnGroup != null)
                            {
                                int listItem = item.ListProductOnGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList().Count;
                                if (listItem < qty)
                                {
                                    ModelState.AddModelError("Dish",
                                        "Number of modifiers of 'Tab Name [" + tabName
                                        + "]' must be larger than or equal Quantity of Tab Name [" + tabName + "]' equal " + qty + "");
                                    break;
                                }
                                foreach (var ProductOnGroup in item.ListProductOnGroup)
                                {
                                    if (ProductOnGroup.ExtraPrice < 0)
                                    {
                                        ModelState.AddModelError("Dish",
                                            "Please enter a value Extra Price of [" + ProductOnGroup.ProductName + "] greater than or equal to 0");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                //============
                if (model.ExpiredDate == null)
                    model.ExpiredDate = Commons._ExpiredDate;
                else
                    model.ExpiredDate = new DateTime(model.ExpiredDate.Value.Year, model.ExpiredDate.Value.Month, model.ExpiredDate.Value.Day, 12, 0, 0, DateTimeKind.Utc);
                //model.ExpiredDate = CommonHelper.ConvertToUTCTime(model.ExpiredDate.Value);
                if (model.HasServiceCharge)
                {
                    if (string.IsNullOrEmpty(model.sServiceCharge))
                        model.sServiceCharge = "0";
                    model.ServiceCharge = double.Parse(model.sServiceCharge ?? "0");
                    if (model.ServiceCharge < 0)
                        ModelState.AddModelError("ServiceCharge", CurrentUser.GetLanguageTextFromKey("Please input default price greater than 0"));
                    else if (model.ServiceCharge > 100)
                        ModelState.AddModelError("ServiceCharge", CurrentUser.GetLanguageTextFromKey("Maximum service charge is 100%"));
                }
                else
                    model.ServiceCharge = -1;

                PropertyReject();

                // Get List Printer Name
                // New Select or Delete old printer
                var lstPrinter = model.LstPrinter.Where(x => x.Status == (int)Commons.EStatus.Actived || x.IsMapProduct).ToList();
                foreach (var item in lstPrinter)
                {
                    model.ListPrinter.Add(new PrinterOnProductModels
                    {
                        PrinterID = item.Id,
                        PrinterName = item.PrinterName,
                        IsActive = (item.Status == (int)Commons.EStatus.Actived) ? true : false
                    });
                }
                if (model.ListPrinter != null && model.ListPrinter.Count != 0)
                {
                    model.Printer = string.Join(",", model.ListPrinter.Select(x => x.PrinterName).ToList());
                }
                //=====
                if (string.IsNullOrEmpty(model.ImageURL))
                {
                    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                }
                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                    photoByte = imgByte;
                }
                else
                    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                //====================
                model.ProductTypeID = Commons.EProductType.Dish.ToString("d");
                model.ProductTypeCode = (byte)Commons.EProductType.Dish;
                //--------
                if (model.ListGroup != null)
                {
                    model.ListGroup = model.ListGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();

                    for (int i = 0; i < model.ListGroup.Count; i++)
                    {
                        if (model.ListGroup[i].ListProductOnGroup != null)
                        {
                            model.ListGroup[i].ListProductOnGroup = model.ListGroup[i].ListProductOnGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    //Return Data
                    //model = GetDetail(model.ID);

                    // Updated 08282017
                    model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Dish.ToString("d"));
                    if (!string.IsNullOrEmpty(cateIdChoose))
                    {

                        var currentCate = model.lstCateGroup.Where(w => w.Id == model.CategoryID).FirstOrDefault();
                        if (currentCate != null)
                        {
                            currentCate.Selected = false;
                        }

                        var cateChoose = model.lstCateGroup.Where(w => w.Id == cateIdChoose).FirstOrDefault();
                        if (cateChoose != null)
                        {
                            cateChoose.Selected = true;
                        }
                    }

                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //=============
                string msg = "";
                var tmp = model.PictureByte;
                model.PictureByte = null;
                var result = _factory.InsertOrUpdateProduct(model, ref msg);
                if (result)
                {
                    model.PictureByte = tmp;
                    //Save Image on Server
                    if (!string.IsNullOrEmpty(model.ImageURL) && model.PictureByte != null)
                    {
                        var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                        var path = string.Format("{0}{1}", originalDirectory, model.ImageURL);
                        MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                        ms.Write(photoByte, 0, photoByte.Length);
                        System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                        ImageHelper.Me.SaveCroppedImage(imageTmp, path, model.ImageURL, ref photoByte);
                        model.PictureByte = photoByte;

                        FTP.Upload(model.ImageURL, model.PictureByte);

                        ImageHelper.Me.TryDeleteImageUpdated(path);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("name", msg);
                    model = GetDetail(model.ID);

                    // Updated 08282017
                    //model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Dish.ToString("d"));
                    if (!string.IsNullOrEmpty(cateIdChoose))
                    {
                        var currentCate = model.lstCateGroup.Where(w => w.Id == model.CategoryID).FirstOrDefault();
                        if (currentCate != null)
                        {
                            currentCate.Selected = false;
                        }

                        var cateChoose = model.lstCateGroup.Where(w => w.Id == cateIdChoose).FirstOrDefault();
                        if (cateChoose != null)
                        {
                            cateChoose.Selected = true;
                        }

                        
                    }

                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Product_Dish_Edit: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);

                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey("Have an error"));
                model = GetDetail(model.ID);

                // Updated 08282017
                //model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Dish.ToString("d"));
                if (!string.IsNullOrEmpty(model.CategoryID))
                {
                    var currentCate = model.lstCateGroup.Where(w => w.Id == model.CategoryID).FirstOrDefault();
                    if (currentCate != null)
                    {
                        currentCate.Selected = true;
                    }
                }

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            ProductModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(ProductModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteProduct(model.ID, ref msg);
                if (!result)
                {
                    //ModelState.AddModelError("Name", "Have a error when you delete an Set menu");
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Product_Dish_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("An error has occurred when deleting"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult Import()
        {
            SandboxImportModel model = new SandboxImportModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(SandboxImportModel model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ExcelUpload", CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    return View(model);
                }

                //if (model.ImageZipUpload == null || model.ImageZipUpload.ContentLength <= 0)
                //{
                //    ModelState.AddModelError("ImageZipUpload", "Image Folder (.zip) cannot be null");
                //    return View(model);
                //}
                if (model.ImageZipUpload != null)
                {
                    if (!Path.GetExtension(model.ImageZipUpload.FileName).ToLower().Equals(".zip"))
                    {
                        ModelState.AddModelError("ImageZipUpload", "");
                        return View(model);
                    }
                }

                FileInfo[] listFiles = new FileInfo[] { };
                string serverZipExtractPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads") + "/ExtractFolder";
                if (model.ImageZipUpload != null && model.ImageZipUpload.ContentLength > 0)
                {
                    bool isFolderEmpty;
                    string fileName = Path.GetFileName(model.ImageZipUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);

                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ImageZipUpload.SaveAs(filePath);

                    //extract file
                    CommonHelper.ExtractZipFile(filePath, serverZipExtractPath);

                    //delete zip file after extract
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    isFolderEmpty = CommonHelper.IsDirectoryEmpty(serverZipExtractPath);

                    if (!isFolderEmpty)
                    {
                        string[] extensions = new[] { ".jpg", ".png", ".jpeg" };
                        DirectoryInfo dInfo = new DirectoryInfo(serverZipExtractPath);
                        //Getting Text files
                        listFiles = dInfo.EnumerateFiles()
                                 .Where(f => extensions.Contains(f.Extension.ToLower()))
                                 .ToArray();
                    }
                }

                DishImportResultModels importModel = new DishImportResultModels();
                string msg = "";
                // read excel file
                if (model.ExcelUpload != null && model.ExcelUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ExcelUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);

                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ExcelUpload.SaveAs(filePath);

                    int totalRowExcel;
                    importModel.ListImport = _factory.ImportDish(filePath, listFiles, out totalRowExcel, model.ListStores, ref msg);
                    importModel.TotalRowExcel = totalRowExcel;

                    //delete folder extract after get file.
                    if (System.IO.Directory.Exists(serverZipExtractPath))
                        System.IO.Directory.Delete(serverZipExtractPath, true);
                    //delete file excel after insert to database
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                if (msg.Equals(""))
                {
                    return View("ImportDetail", importModel);
                }
                else
                {
                    _logger.Error("Product_Dish_Import: " + msg);
                    ModelState.AddModelError("ImageZipUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Product_Dish_Import: " + e);
                ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey(e.Message));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            ProductModels model = new ProductModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(ProductModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsSetMenu = wb.Worksheets.Add("Dishes");
                var wsTab = wb.Worksheets.Add("Tabs");
                var wsDish = wb.Worksheets.Add("Modifiers");

                StatusResponse response = _factory.ExportDish(ref wsSetMenu, ref wsTab, ref wsDish, model.ListStores);

                if (!response.Status)
                {
                    ModelState.AddModelError("", response.MsgError);
                    return View(model);
                }

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Dishes").Replace(" ", "_")));

                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                return RedirectToAction("Export");
            }
            catch (Exception e)
            {
                _logger.Error("Product_Dish_Export: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }

        #region /*Extension Function*/
        //public ActionResult AddTab(int currentOffset)
        //{
        //    GroupProductModels group = new GroupProductModels();
        //    group.OffSet = currentOffset;
        //    return PartialView("_TabContent", group);
        //}

        //public ActionResult AddModifiers(/*string[] dishIDs*/ GroupProductModels data /*, int currentGroupOffSet, int currentDishOffset*/)
        //{
        //    GroupProductModels model = new GroupProductModels();
        //    if (data.ListProductOnGroup != null && data.ListProductOnGroup.Count() > 0)
        //        model.ListProductOnGroup = new List<ProductOnGroupModels>();

        //    for (int i = 0; i < data.ListProductOnGroup.Count(); i++)
        //    {
        //        ProductOnGroupModels dish = new ProductOnGroupModels();
        //        dish.OffSet = data.currentOffset;

        //        dish.ProductID = data.ListProductOnGroup[i].ProductID;
        //        dish.ProductName = data.ListProductOnGroup[i].ProductName;
        //        dish.Sequence = data.ListProductOnGroup[i].Sequence;
        //        dish.ExtraPrice = data.ListProductOnGroup[i].ExtraPrice;

        //        model.ListProductOnGroup.Add(dish);

        //        data.currentOffset++;
        //    }
        //    model.OffSet = data.currentgroupOffSet; // trongntn
        //    return PartialView("_DishModal", model);
        //}

        ////public ActionResult LoadModifiers(bool isMultiChoice, string[] StoreID)
        ////{
        ////    List<ProductModels> lstModifiers = null;
        ////    StoreID = StoreID.Where(ww => !string.IsNullOrEmpty(ww)).ToArray();
        ////    if (StoreID.Length == 0)
        ////    {
        ////        lstModifiers = new List<ProductModels>();
        ////    }
        ////    else
        ////    {
        ////        lstModifiers = new List<ProductModels>();
        ////        foreach (var item in StoreID)
        ////        {
        ////            var lstModifier = _factory.GetListProduct(item, (byte)Commons.EProductType.Modifier, CurrentUser.ListOrganizationId);
        ////            lstModifiers.AddRange(lstModifier);
        ////        }
        ////    }

        ////    lstModifiers = lstModifiers.Where(x => x.IsActive).ToList();
        ////    GroupProductModels model = new GroupProductModels();
        ////    if (lstModifiers != null)
        ////    {
        ////        model.ListProductOnGroup = new List<ProductOnGroupModels>();
        ////        foreach (var item in lstModifiers)
        ////        {
        ////            ProductOnGroupModels dish = new ProductOnGroupModels()
        ////            {
        ////                ProductID = item.ID,
        ////                ProductName = item.Name,
        ////                Seq = item.OrderByIndex,
        ////                ExtraPrice = item.ExtraPrice
        ////            };
        ////            model.ListProductOnGroup.Add(dish);
        ////        }
        ////    }
        ////    if (isMultiChoice)
        ////        return PartialView("_TableChooseDishes", model);
        ////    return PartialView("_TableChooseDish", model);
        ////}

        //public ActionResult LoadModifiers(bool isMultiChoice, string StoreID)
        //{
        //    var lstModifier = _factory.GetListProduct(StoreID, (byte)Commons.EProductType.Modifier, CurrentUser.ListOrganizationId);
        //    lstModifier = lstModifier.Where(x => x.IsActive).ToList();
        //    GroupProductModels model = new GroupProductModels();
        //    if (lstModifier != null)
        //    {
        //        model.ListProductOnGroup = new List<ProductOnGroupModels>();
        //        foreach (var item in lstModifier)
        //        {
        //            ProductOnGroupModels dish = new ProductOnGroupModels()
        //            {
        //                ProductID = item.ID,
        //                ProductName = item.Name,
        //                Sequence = item.OrderByIndex,
        //                ExtraPrice = item.ExtraPrice
        //            };
        //            model.ListProductOnGroup.Add(dish);
        //        }
        //    }
        //    if (isMultiChoice)
        //        return PartialView("_TableChooseDishes", model);
        //    return PartialView("_TableChooseDish", model);
        //}

        //public ActionResult CheckModifier(ProductModels product)
        //{
        //    //List<string> lstStoreIndex = setMenuModel.ApplyStoreModel.GetSelectedStoreIndex();
        //    //if (setMenuModel != null && setMenuModel.ListGroup != null)
        //    //    foreach (MediateSetGroup group in setMenuModel.ListGroup)
        //    //        if (group.DishModel != null && group.DishModel.ListDish != null)
        //    //            foreach (MediateSetGroupDish dish in group.DishModel.ListDish)
        //    //            {
        //    //                string dishID = dish.ItemID;
        //    //                if (!StoreHaveDish(lstStoreIndex, dishID))
        //    //                    dish.Status = 9;
        //    //            }

        //    return PartialView("_BodyTableTabContent", product.ListGroup);
        //}


        // Updated 09072017
        public ActionResult AddTab(int currentOffset, Boolean isModifier)
        {
            GroupProductModels group = new GroupProductModels();
            group.OffSet = currentOffset;
            if (!isModifier)
            {
                return PartialView("_TabContentAdditionalDish", group);
            }
            return PartialView("_TabContent", group);
        }

        public ActionResult AddModifiers(GroupProductModels data, Boolean isModifier)
        {
            GroupProductModels model = new GroupProductModels();
            if (data.ListProductOnGroup != null && data.ListProductOnGroup.Count() > 0)
                model.ListProductOnGroup = new List<ProductOnGroupModels>();

            for (int i = 0; i < data.ListProductOnGroup.Count(); i++)
            {
                ProductOnGroupModels dish = new ProductOnGroupModels();
                dish.OffSet = data.currentOffset;

                dish.ProductID = data.ListProductOnGroup[i].ProductID;
                dish.ProductName = data.ListProductOnGroup[i].ProductName;
                dish.Sequence = data.ListProductOnGroup[i].Sequence;
                dish.ExtraPrice = data.ListProductOnGroup[i].ExtraPrice;

                model.ListProductOnGroup.Add(dish);

                data.currentOffset++;
            }
            model.OffSet = data.currentgroupOffSet;
            if (!isModifier)
            {
                return PartialView("_AdditionalDishModal", model);
            }
            return PartialView("_DishModal", model);
        }

        public ActionResult LoadModifiers(bool isMultiChoice, string StoreID, Boolean isModifier)
        {
            byte eProductType = (byte)Commons.EProductType.Modifier;
            if (!isModifier)
            {
                eProductType = (byte)Commons.EProductType.Dish;
            }
            var lstModifier = _factory.GetListProduct(StoreID, eProductType, CurrentUser.ListOrganizationId);
            lstModifier = lstModifier.Where(x => x.IsActive).OrderBy(oo => oo.OrderByIndex).ThenBy(oo => oo.Name).ToList();
            GroupProductModels model = new GroupProductModels();
            if (lstModifier != null)
            {
                model.ListProductOnGroup = new List<ProductOnGroupModels>();
                foreach (var item in lstModifier)
                {
                    ProductOnGroupModels dish = new ProductOnGroupModels()
                    {
                        ProductID = item.ID,
                        ProductName = item.Name,
                        Sequence = item.OrderByIndex,
                        ExtraPrice = item.ExtraPrice
                    };
                    model.ListProductOnGroup.Add(dish);
                }
            }
            if (!isModifier)
            {
                if (isMultiChoice)
                    return PartialView("_TableChooseAdditionalDishes", model);
                return PartialView("_TableChooseAdditionalDish", model);
            }
            if (isMultiChoice)
                return PartialView("_TableChooseDishes", model);
            return PartialView("_TableChooseDish", model);
        }

        public ActionResult CheckModifier(ProductModels product, Boolean isModifier)
        {
            if (!isModifier)
            {
                return PartialView("_BodyTableTabContentAdditionalDish", product.ListGroup);
            }
            return PartialView("_BodyTableTabContent", product.ListGroup);
        }


        #endregion
    }
}