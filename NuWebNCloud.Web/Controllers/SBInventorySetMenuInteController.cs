using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Integration.Factory.Sandbox.Product;
using NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
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
    public class SBInventorySetMenuInteController : SBInventoryBaseController
    {
        // GET: SBInventorySetMenuInte
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private InteProductFactory _factory = null;
        SeasonFactory _seasonFactory = null;
        CategoriesFactory _categoryFactory = null;
        TipServiceChargeFactory _tipServiceChargeFactory = null;
        TaxFactory _taxFactory = null;

        public SBInventorySetMenuInteController()
        {
            _factory = new InteProductFactory();

            _seasonFactory = new SeasonFactory();
            _categoryFactory = new CategoriesFactory();
            _tipServiceChargeFactory = new TipServiceChargeFactory();
            _taxFactory = new TaxFactory();
            //ViewBag.ListStore = GetListStoreIntegration();
        }

        public ActionResult Index()
        {
            try
            {
                InteProductViewModels model = new InteProductViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Product_SetMenu_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(InteProductViewModels model)
        {
            try
            {
                var result = _factory.GetListProduct((byte)Commons.EProductType.SetMenu, CurrentUser.ListOrganizationId);
                List<InteProductLiteModels> listsetmenuinte = new List<InteProductLiteModels>();
                List<InteProductLiteModels> list = new List<InteProductLiteModels>();
                if (model.ListStoreID != null)
                {

                    result = result.Where(o => o.ListStore.Select(s => s.ID).ToList().Any(sID => model.ListStoreID.Contains(sID))).ToList();

                }

                result.ForEach(x =>
                {
                    x.ImageURL = string.IsNullOrEmpty(x.ImageURL) ? Commons.Image100_100 : x.ImageURL;
                    x.ListStoreName = x.ListStore.Select(z => z.Name).ToList();
                    x.ListStoreName = x.ListStoreName.OrderBy(s => s).ToList();

                    x.StoreName = string.Join(", ", x.ListStoreName);
                });
                model.ListItem = result;
            }
            catch (Exception e)
            {
                _logger.Error("Product_SetMenu_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            InteProductModels model = new InteProductModels();
            //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
            //model.ListStoreView = lstStore;
            var lstStore = (SelectList)ViewBag.StoreID;
            model.ListStoreView = lstStore.ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(InteProductModels model)
        {
            try
            {
                model.ListStoreView = model.ListStoreView.Where(x => x.Selected).ToList();
                if (model.ListStoreView.Count == 0)
                {
                    ModelState.AddModelError("ListStoreView", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                }
                model.ListProductOnStore = model.ListProductOnStore.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                if (model.ListProductOnStore != null && model.ListProductOnStore.Count > 0)
                {
                    foreach (var itemProduct in model.ListProductOnStore)
                    {
                        if (itemProduct.ListPrices[0].Price < 0)
                            ModelState.AddModelError("ListProductOnStore[" + itemProduct.OffSet + "].ListPrices[0].Price", CurrentUser.GetLanguageTextFromKey("Please input default price greater than 0"));
                        else
                        {
                            if (itemProduct.Cost > itemProduct.ListPrices[0].Price)
                            {
                                ModelState.AddModelError("ListProductOnStore[" + itemProduct.OffSet + "].ListPrices[0].Price", CurrentUser.GetLanguageTextFromKey("Default Price must be larger than cost"));
                            }
                        }

                        for (int i = 1; i < itemProduct.ListPrices.Count; i++)
                        {
                            if (itemProduct.ListPrices[i].Price > 0 && string.IsNullOrEmpty(itemProduct.ListPrices[i].SeasonID))
                                ModelState.AddModelError(string.Format("ListProductOnStore[" + itemProduct.OffSet + "].ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please select Season for Price"));
                            if (itemProduct.ListPrices[i].Price == 0)
                            {
                                itemProduct.ListPrices[i].SeasonID = null;
                            }
                        }

                        for (int i = 1; i < itemProduct.ListPrices.Count; i++)
                            if (!string.IsNullOrEmpty(itemProduct.ListPrices[i].SeasonID) && itemProduct.ListPrices[i].Price < 0)
                                ModelState.AddModelError(string.Format("ListProductOnStore[" + itemProduct.OffSet + "].ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please input price greater than 0"));

                        if (itemProduct.ListProductGroup == null)
                            ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Set Menu must have as least one Tab"));
                        else
                            foreach (InteGroupProductModels group in itemProduct.ListProductGroup)
                                if (group.ListProductOnGroup == null)
                                    ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Tab must have as least one Dish"));

                        //============
                        //L 2018/05/21
                        //int countSeasonKiosk = itemProduct.ListSeasonKiosk.Count;
                        var lstChooseSeasonKiosk = itemProduct.ListSeasonKiosk.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                        //int countChooseSeasonKiosk = lstChooseSeasonKiosk.Count;
                        //if (countSeasonKiosk != countChooseSeasonKiosk)
                        //{
                        foreach (var item in lstChooseSeasonKiosk)
                        {
                            itemProduct.ListProductSeason.Add(new ProductSeasonDTO
                            {
                                SeasonID = item.ID,
                                IsPOS = item.IsPOS,
                            });
                        }
                        //}
                        //==========Tax
                        if (itemProduct.IsTaxRequired)
                        {
                            var lstChooseTax = itemProduct.ListTax.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                            if (lstChooseTax.Count == 0)
                                ModelState.AddModelError(string.Format("ListProductOnStore[" + itemProduct.OffSet + "].TaxName"), CurrentUser.GetLanguageTextFromKey("Please select tax for product"));
                            else
                            {
                                var TaxSelected = lstChooseTax.FirstOrDefault();
                                if (TaxSelected != null)
                                    itemProduct.TaxID = TaxSelected.ID;
                            }
                        }
                        //============
                        if (itemProduct.ListProductGroup != null)
                        {
                            foreach (var item in itemProduct.ListProductGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList())
                            {
                                int qty = item.Maximum;
                                string tabName = item.Name;
                                if (string.IsNullOrEmpty(tabName))
                                {
                                    ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Tab name") + " [" + item.OffSet + "]" + CurrentUser.GetLanguageTextFromKey("is required."));
                                }
                                if (item.Sequence < 0)
                                {
                                    ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                                }
                                if (item.ListProductOnGroup != null)
                                {
                                    int listItem = item.ListProductOnGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList().Count;
                                    //if (listItem < qty)
                                    //{
                                    //    ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Number of dishes of 'Tab Name") + " [" + tabName
                                    //        + "]" + CurrentUser.GetLanguageTextFromKey("must be more than or equal Quantity of Tab Name") + " ["
                                    //        + tabName + "]" + CurrentUser.GetLanguageTextFromKey("equal") + qty + "");
                                    //    break;
                                    //}

                                    if (listItem == 0)
                                    {
                                        ModelState.AddModelError("MsgDish_" + itemProduct.StoreID,
                                            CurrentUser.GetLanguageTextFromKey("Please choose dishes of 'Tab Name'") + " [" + tabName + "]");
                                        break;
                                    }

                                    foreach (var ProductOnGroup in item.ListProductOnGroup)
                                    {
                                        if (ProductOnGroup.Sequence < 0)
                                        {
                                            ModelState.AddModelError("MsgDish_" + itemProduct.StoreID,
                                                CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence of") + " [" + ProductOnGroup.ProductName + "] " + CurrentUser.GetLanguageTextFromKey("greater than or equal to 0"));
                                            break;
                                        }
                                        if (ProductOnGroup.ExtraPrice < 0)
                                        {
                                            ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Please enter a value Extra Price of") + " ["
                                                + ProductOnGroup.ProductName + "]" + CurrentUser.GetLanguageTextFromKey("greater than or equal to 0"));
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        //--------
                        if (itemProduct.ListProductGroup != null)
                        {
                            itemProduct.ListProductGroup = itemProduct.ListProductGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();

                            for (int i = 0; i < itemProduct.ListProductGroup.Count; i++)
                            {
                                if (itemProduct.ListProductGroup[i].ListProductOnGroup != null)
                                    itemProduct.ListProductGroup[i].ListProductOnGroup = itemProduct.ListProductGroup[i].ListProductOnGroup.Where(x => x.Status
                                            != (byte)Commons.EStatus.Deleted).ToList();
                            }
                        }

                        // ============
                        if (itemProduct.ExpiredDate == null)
                            itemProduct.ExpiredDate = Commons._ExpiredDate;
                        else
                            itemProduct.ExpiredDate = new DateTime(itemProduct.ExpiredDate.Value.Year,
                                itemProduct.ExpiredDate.Value.Month, itemProduct.ExpiredDate.Value.Day, 12, 59, 59);

                        if (itemProduct.HasServiceCharge)
                        {
                            if (string.IsNullOrEmpty(itemProduct.sServiceCharge))
                                itemProduct.sServiceCharge = "0";
                            itemProduct.ServiceChargeValue = double.Parse(itemProduct.sServiceCharge ?? "0");
                            if (itemProduct.ServiceChargeValue < 0)
                                ModelState.AddModelError("sServiceCharge", CurrentUser.GetLanguageTextFromKey("Please input service charge greater than 0"));
                            else if (itemProduct.ServiceChargeValue > 100)
                                ModelState.AddModelError("sServiceCharge", CurrentUser.GetLanguageTextFromKey("Maximum service charge is 100%"));
                        }
                        else
                            itemProduct.ServiceChargeValue = -1;

                        if (string.IsNullOrEmpty(itemProduct.CategoryID))
                        {
                            ModelState.AddModelError("ListProductOnStore[" + itemProduct.OffSet + "].CategoryID", CurrentUser.GetLanguageTextFromKey("Please choose category"));
                        }

                        // Updated 08282017
                        string cateIdChoose = null;
                        if (!string.IsNullOrEmpty(itemProduct.CategoryID))
                        {
                            cateIdChoose = itemProduct.CategoryID;
                        }

                        itemProduct.IsOptional = true;
                        itemProduct.IsForce = true;

                        InteProductPriceModels proPrice = new InteProductPriceModels()
                        {
                            DefaultPrice = Math.Round(itemProduct.ListPrices[0].Price, 2),
                            SeasonPrice = Math.Round(itemProduct.ListPrices[1].Price, 2),
                            SeasonPriceID = itemProduct.ListPrices[1].SeasonID
                        };
                        itemProduct.PriceOnStore = proPrice;
                        itemProduct.ColorCode = "#000000";
                        itemProduct.PrintOutName = itemProduct.PrintOutName;
                        itemProduct.Measure = String.IsNullOrEmpty(itemProduct.Measure) ? "$" : itemProduct.Measure;
                        //=================
                        //Return Data
                        itemProduct.ListPrices = GetPrices(itemProduct.ListPrices, itemProduct.StoreID);
                        itemProduct.ListCategories = GetSelectListCategoriesInte(itemProduct.StoreID, Commons.EProductType.SetMenu.ToString("d"));

                        // Updated 08292017
                        itemProduct.lstCateGroup = GetSelectListCategoriesInteSortParent(itemProduct.StoreID, Commons.EProductType.SetMenu.ToString("d"));
                        if (!string.IsNullOrEmpty(cateIdChoose))
                        {
                            var cateChoose = itemProduct.lstCateGroup.Where(w => w.Id == cateIdChoose).FirstOrDefault();
                            if (cateChoose != null)
                            {
                                cateChoose.Selected = true;
                            }
                        }
                    }
                }
                byte[] photoByte = null;
                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                    photoByte = imgByte;
                }
                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                    //model.ListStoreView = lstStore;
                    var lstStore = (SelectList)ViewBag.StoreID;
                    model.ListStoreView = lstStore.ToList();
                    return View(model);
                }
                model.ProductType = (byte)Commons.EProductType.SetMenu;
                //====================
                string msg = "";
                var tmp = model.PictureByte;
                model.PictureByte = null;
                bool result = _factory.InsertOrUpdateProduct(model, CurrentUser.UserId, ref msg);
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
                    ModelState.AddModelError("Name", msg);
                    //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                    //model.ListStoreView = lstStore;
                    var lstStore = (SelectList)ViewBag.StoreID;
                    model.ListStoreView = lstStore.ToList();
                    return View(model);
                    //return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Product_SetMenu_Create: " + ex);
                //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                //model.ListStoreView = lstStore;
                var lstStore = (SelectList)ViewBag.StoreID;
                model.ListStoreView = lstStore.ToList();
                //return new HttpStatusCodeResult(400, ex.Message);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Have a error"));
                return View(model);
            }
        }

        public InteProductModels GetDetail(string id)
        {
            try
            {
                InteProductModels model = _factory.GetProductDetail(id);
                model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                model.CurrencySymbol = CurrentUser.CurrencySymbol;
                //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                //model.ListStoreView = lstStore;
                var lstStore = (SelectList)ViewBag.StoreID;
                model.ListStoreView = lstStore.ToList();
                model.ListStoreView = model.ListStoreView.OrderBy(o => o.Text).ToList();
                int OffSet = 0;
                if (model.ListProductOnStore != null && model.ListProductOnStore.Count > 0)
                {
                    model.ListProductOnStore = model.ListProductOnStore.OrderBy(oo => oo.StoreName).ToList();
                    foreach (var item in model.ListProductOnStore)
                    {
                        item.CurrencySymbol = model.CurrencySymbol;
                        item.OffSet = OffSet++;
                        item.IsDeleteTemplate = true;
                        item.ExpiredDate = item.ExpiredDate.Value.ToLocalTime();
                        if (item.ExpiredDate.Value.Date == Commons._ExpiredDate.Date)
                        {
                            item.ExpiredDate = null;
                        }
                        model.ListStoreView.ForEach(x =>
                        {
                            if (!x.Selected)
                            {
                                x.Selected = x.Value.Equals(item.StoreID) ? true : false;
                                x.Disabled = x.Selected;
                            }
                        });

                        //===========
                        InteProductPriceModels PriceOnStore = item.PriceOnStore == null ? new InteProductPriceModels() : item.PriceOnStore;
                        if (item.ListProductGroup != null)
                        {
                            List<InteGroupProductModels> groupP = item.ListProductGroup;
                            for (int i = 0; i < item.ListProductGroup.Count; i++)
                            {
                                item.ListProductGroup[i].OffSet = i;
                                item.ListProductGroup[i].StoreOffSet = item.OffSet;
                                item.ListProductGroup[i].StoreID = item.StoreID;
                                if (item.ListProductGroup[i].ListProductOnGroup != null)
                                {
                                    item.ListProductGroup[i].ListProductOnGroup = item.ListProductGroup[i].ListProductOnGroup.OrderByDescending(z => z.Sequence).ToList();
                                    for (int j = 0; j < item.ListProductGroup[i].ListProductOnGroup.Count; j++)
                                    {
                                        item.ListProductGroup[i].ListProductOnGroup[j].OffSet = j;
                                    }
                                }
                            }
                        }
                        item.sServiceCharge = item.ServiceChargeValue == -1 ? "" : item.ServiceChargeValue.ToString();
                        //================ Get Category
                        item.ListCategories = this.GetSelectListCategoriesInte(item.StoreID, Commons.EProductType.SetMenu.ToString("d"));

                        // Updated 08282017
                        item.lstCateGroup = this.GetSelectListCategoriesInteSortParent(item.StoreID, Commons.EProductType.SetMenu.ToString("d"));
                        if (!string.IsNullOrEmpty(item.CategoryID))
                        {
                            var cateChoose = item.lstCateGroup.Where(w => w.Id == item.CategoryID).FirstOrDefault();
                            if (cateChoose != null)
                            {
                                cateChoose.Selected = true;
                            }
                        }
                        //======= TImeSlot Kiosk
                        var ListProductSeasonKiosk = item.ListProductSeason.Where(x => x.IsPOS == false).ToList();
                        item.ListSeasonKiosk = GetListTimeSlot02(item.StoreID, ListProductSeasonKiosk);
                        if (ListProductSeasonKiosk != null && ListProductSeasonKiosk.Count != 0)
                        {
                            item.ProductSeasonKiosk = string.Join(", ", ListProductSeasonKiosk.Select(x => x.SeasonName).ToList());
                        }
                        else if (ListProductSeasonKiosk.Count == 0)
                        {
                            item.ProductSeasonKiosk = CurrentUser.GetLanguageTextFromKey("All");
                        }
                        //============Tax
                        var listTax = _taxFactory.GetListTax(item.StoreID);
                        listTax = listTax.Where(x => x.IsActive).ToList();
                        if (listTax != null && listTax.Count > 0)
                            item.IsTaxRequired = true;
                        item.ListTax = listTax;
                        item.ListTax.ForEach(x => x.Status = (x.ID == item.TaxID ? (int)Commons.EStatus.Actived : (int)Commons.EStatus.Deleted));

                        //================Price
                        SeasonFactory seasonf = new SeasonFactory();
                        List<SeasonModels> lstSs = seasonf.GetListSeason(item.StoreID, null, CurrentUser.ListOrganizationId);
                        List<SelectListItem> lstSllItemSeason = new List<SelectListItem>();
                        SelectListItem sslItemSeason;
                        foreach (var season in lstSs)
                        {
                            sslItemSeason = new SelectListItem();
                            sslItemSeason.Value = season.ID;
                            sslItemSeason.Text = season.Name;
                            sslItemSeason.Selected = season.ID.Equals(PriceOnStore.SeasonPriceID);
                            lstSllItemSeason.Add(sslItemSeason);
                        }
                        foreach (PriceItem priceItem in item.ListPrices)
                            priceItem.ListSeasons = lstSllItemSeason;
                        item.ListPrices[0].Price = PriceOnStore.DefaultPrice;
                        item.ListPrices[1].Price = PriceOnStore.SeasonPrice == -1 ? 0 : PriceOnStore.SeasonPrice;
                        item.ListPrices[1].SeasonID = PriceOnStore.SeasonPriceID;
                        item.ListPrices[1].PriceTag = PriceOnStore.SeasonPriceName;
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Product_SetMenu_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            InteProductModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            InteProductModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(InteProductModels model)
        {
            try
            {
                //model.ListStoreView = model.ListStoreView.Where(x => x.Selected).ToList();
                var lListStoreSel = model.ListStoreView.Where(x => x.Selected).ToList();
                if (lListStoreSel.Count == 0)
                {
                    ModelState.AddModelError("ListStoreView", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                }

                model.ListProductOnStore = model.ListProductOnStore.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                if (model.ListProductOnStore != null && model.ListProductOnStore.Count > 0)
                {
                    model.ListStoreView.ForEach(z =>
                    {
                        var isCheck = model.ListProductOnStore.Exists(x => x.StoreID.Equals(z.Value) && x.IsDeleteTemplate);
                        z.Selected = isCheck;
                    });

                    foreach (var itemProduct in model.ListProductOnStore)
                    {
                        if (itemProduct.ListPrices[0].Price < 0)
                            ModelState.AddModelError("ListProductOnStore[" + itemProduct.OffSet + "].ListPrices[{0}].Price",
                                CurrentUser.GetLanguageTextFromKey("Please input default price greater than 0"));
                        else
                        {
                            if (itemProduct.Cost > itemProduct.ListPrices[0].Price)
                            {
                                ModelState.AddModelError("ListProductOnStore[" + itemProduct.OffSet + "].ListPrices[{0}].Price",
                                    CurrentUser.GetLanguageTextFromKey("Default Price must be larger than cost"));
                            }
                        }

                        for (int i = 1; i < itemProduct.ListPrices.Count; i++)
                        {
                            if (itemProduct.ListPrices[i].Price > 0 && string.IsNullOrEmpty(itemProduct.ListPrices[i].SeasonID))
                                ModelState.AddModelError(string.Format("ListProductOnStore[" + itemProduct.OffSet + "].ListPrices[{0}].Price", i),
                                    CurrentUser.GetLanguageTextFromKey("Please select Season for Price"));

                            if (itemProduct.ListPrices[i].Price == 0)
                            {
                                itemProduct.ListPrices[i].SeasonID = null;
                            }
                        }


                        for (int i = 1; i < itemProduct.ListPrices.Count; i++)
                            if (!string.IsNullOrEmpty(itemProduct.ListPrices[i].SeasonID) && itemProduct.ListPrices[i].Price < 0)
                                ModelState.AddModelError(string.Format("ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please input price greater than 0"));

                        if (itemProduct.ListProductGroup == null)
                            ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Set Menu must have as least one Tab"));
                        else
                            foreach (InteGroupProductModels group in itemProduct.ListProductGroup)
                                if (group.ListProductOnGroup == null)
                                    ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Tab must have as least one Dish"));


                        //============
                        //L 2018/05/21
                        //int countSeasonKiosk = itemProduct.ListSeasonKiosk.Count;
                        var lstChooseSeasonKiosk = itemProduct.ListSeasonKiosk.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                        //int countChooseSeasonKiosk = lstChooseSeasonKiosk.Count;
                        //if (countSeasonKiosk != countChooseSeasonKiosk)
                        //{
                        foreach (var item in lstChooseSeasonKiosk)
                        {
                            itemProduct.ListProductSeason.Add(new ProductSeasonDTO
                            {
                                SeasonID = item.ID,
                                IsPOS = item.IsPOS,
                            });
                        }
                        //}
                        //==========Tax
                        if (itemProduct.IsTaxRequired)
                        {
                            var lstChooseTax = itemProduct.ListTax.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                            if (lstChooseTax.Count == 0)
                                ModelState.AddModelError(string.Format("ListProductOnStore[" + itemProduct.OffSet + "].TaxName"), CurrentUser.GetLanguageTextFromKey("Please select tax for product"));
                            else
                            {
                                var TaxSelected = lstChooseTax.FirstOrDefault();
                                if (TaxSelected != null)
                                    itemProduct.TaxID = TaxSelected.ID;
                            }
                        }
                        //========================
                        if (itemProduct.ListProductGroup != null)
                        {
                            foreach (var item in itemProduct.ListProductGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList())
                            {
                                int qty = item.Maximum;
                                string tabName = item.Name;
                                if (string.IsNullOrEmpty(tabName))
                                {
                                    ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Tab name") + " [" + item.OffSet + "]" + CurrentUser.GetLanguageTextFromKey("is required."));
                                }
                                if (item.Sequence < 0)
                                {
                                    ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                                }
                                if (item.ListProductOnGroup != null)
                                {
                                    int listItem = item.ListProductOnGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList().Count;
                                    //if (listItem < qty)
                                    //{
                                    //    ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Number of dishes of 'Tab Name") + " [" + tabName
                                    //        + "]" + CurrentUser.GetLanguageTextFromKey("must be more than or equal Quantity of Tab Name") + " ["
                                    //        + tabName + "]" + CurrentUser.GetLanguageTextFromKey("equal") + qty + "");
                                    //    break;
                                    //}
                                    if (listItem == 0)
                                    {
                                        ModelState.AddModelError("MsgDish_" + itemProduct.StoreID,
                                            CurrentUser.GetLanguageTextFromKey("Please choose dishes of 'Tab Name'") + " [" + tabName + "]");
                                        break;
                                    }

                                    foreach (var ProductOnGroup in item.ListProductOnGroup)
                                    {
                                        if (ProductOnGroup.Sequence < 0)
                                        {
                                            ModelState.AddModelError("MsgDish_" + itemProduct.StoreID,
                                                CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence of") + " [" + ProductOnGroup.ProductName + "]" + CurrentUser.GetLanguageTextFromKey("greater than or equal to 0"));
                                            break;
                                        }
                                        if (ProductOnGroup.ExtraPrice < 0)
                                        {
                                            ModelState.AddModelError("MsgDish_" + itemProduct.StoreID, CurrentUser.GetLanguageTextFromKey("Please enter a value Extra Price of") + " ["
                                                + ProductOnGroup.ProductName + "]" + CurrentUser.GetLanguageTextFromKey("greater than or equal to 0"));
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // ============
                        if (itemProduct.ExpiredDate == null)
                            itemProduct.ExpiredDate = Commons._ExpiredDate;
                        else
                            itemProduct.ExpiredDate = new DateTime(itemProduct.ExpiredDate.Value.Year,
                                itemProduct.ExpiredDate.Value.Month, itemProduct.ExpiredDate.Value.Day, 12, 59, 59);

                        if (itemProduct.HasServiceCharge)
                        {
                            if (string.IsNullOrEmpty(itemProduct.sServiceCharge))
                                itemProduct.sServiceCharge = "0";
                            itemProduct.ServiceChargeValue = double.Parse(itemProduct.sServiceCharge ?? "0");
                            if (itemProduct.ServiceChargeValue < 0)
                                ModelState.AddModelError("sServiceCharge", CurrentUser.GetLanguageTextFromKey("Please input service charge greater than 0"));
                            else if (itemProduct.ServiceChargeValue > 100)
                                ModelState.AddModelError("sServiceCharge", CurrentUser.GetLanguageTextFromKey("Maximum service charge is 100%"));
                        }
                        else
                            itemProduct.ServiceChargeValue = -1;

                        if (string.IsNullOrEmpty(itemProduct.CategoryID))
                        {
                            ModelState.AddModelError("ListProductOnStore[" + itemProduct.OffSet + "].CategoryID",
                                CurrentUser.GetLanguageTextFromKey("Please choose category"));
                        }

                        // Updated 08282017
                        string cateIdChoose = null;
                        if (!string.IsNullOrEmpty(itemProduct.CategoryID))
                        {
                            cateIdChoose = itemProduct.CategoryID;
                        }

                        itemProduct.IsOptional = true;
                        itemProduct.IsForce = true;

                        //--------
                        if (itemProduct.ListProductGroup != null)
                        {
                            itemProduct.ListProductGroup = itemProduct.ListProductGroup.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();

                            for (int i = 0; i < itemProduct.ListProductGroup.Count; i++)
                            {
                                if (itemProduct.ListProductGroup[i].ListProductOnGroup != null)
                                    itemProduct.ListProductGroup[i].ListProductOnGroup = itemProduct.ListProductGroup[i].ListProductOnGroup.Where(x => x.Status
                                            != (byte)Commons.EStatus.Deleted).ToList();
                            }
                        }
                        InteProductPriceModels proPrice = new InteProductPriceModels()
                        {
                            DefaultPrice = Math.Round(itemProduct.ListPrices[0].Price, 2),
                            SeasonPrice = Math.Round(itemProduct.ListPrices[1].Price, 2),
                            SeasonPriceID = itemProduct.ListPrices[1].SeasonID
                        };
                        itemProduct.PriceOnStore = proPrice;
                        itemProduct.ColorCode = "#000000";
                        itemProduct.PrintOutName = itemProduct.PrintOutName;
                        itemProduct.Measure = String.IsNullOrEmpty(itemProduct.Measure) ? "$" : itemProduct.Measure;
                        //=================
                        //Return Data
                        itemProduct.ListPrices = GetPrices(itemProduct.ListPrices, itemProduct.StoreID);
                        itemProduct.ListCategories = GetSelectListCategoriesInte(itemProduct.StoreID, Commons.EProductType.SetMenu.ToString("d"));

                        // Updated 08282017
                        itemProduct.lstCateGroup = GetSelectListCategoriesInteSortParent(itemProduct.StoreID, Commons.EProductType.SetMenu.ToString("d"));
                        if (!string.IsNullOrEmpty(cateIdChoose))
                        {
                            var cateChoose = itemProduct.lstCateGroup.Where(w => w.Id == cateIdChoose).FirstOrDefault();
                            if (cateChoose != null)
                            {
                                cateChoose.Selected = true;
                            }
                        }
                    }
                }


                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    //Return Data
                    //model = GetDetail(model.ID);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }

                //=====
                byte[] photoByte = null;
                if (!string.IsNullOrEmpty(model.ImageURL))
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

                model.ProductType = (byte)Commons.EProductType.SetMenu;
                //=============
                string msg = "";
                var tmp = model.PictureByte;
                model.PictureByte = null;
                var result = _factory.InsertOrUpdateProduct(model, CurrentUser.UserId, ref msg);
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
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Product_SetMenu_Edit: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Have a error"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            InteProductModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(InteProductModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteProduct(model.ID, CurrentUser.UserId, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Product_SetMenu_Delete: " + ex);
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
                if (model.ImageZipUpload != null)
                {
                    if (!Path.GetExtension(model.ImageZipUpload.FileName).ToLower().Equals(".zip"))
                    {
                        ModelState.AddModelError("ImageZipUpload", "");
                        return View(model);
                    }
                }

                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    return View(model);
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

                SetMenuImportResultModels importModel = new SetMenuImportResultModels();
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
                    importModel.ListImport = _factory.ImportSetMenu(filePath, listFiles, out totalRowExcel, CurrentUser.ListOrganizationId, ref msg);
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
                    _logger.Error("Product_SetMenu_Import: " + msg);
                    ModelState.AddModelError("ImageZipUpload", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Product_SetMenu_Import: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);
                ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey(ex.Message));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            InteProductModels model = new InteProductModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(InteProductModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsSetMerchant = wb.Worksheets.Add("Set Merchant");
                var wsSetStore = wb.Worksheets.Add("Set Store");
                var wsTab = wb.Worksheets.Add("Tabs");
                var wsDishes = wb.Worksheets.Add("Dishes");
                StatusResponse response = _factory.ExportSetMenu(ref wsSetMerchant, ref wsSetStore, ref wsTab, ref wsDishes, CurrentUser.ListOrganizationId, model.ListStores);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("SetMenus").Replace(" ", "_")));

                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                return RedirectToAction("Export");
            }
            catch (Exception ex)
            {
                _logger.Error("Product_SetMenu_Export: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        #region /*Extension Function*/
        public ActionResult AddTab(int currentOffset, string StoreID, int StoreOffSet)
        {
            InteGroupProductModels groupProduct = new InteGroupProductModels();
            groupProduct.OffSet = currentOffset;
            groupProduct.StoreID = StoreID;
            groupProduct.StoreOffSet = StoreOffSet;
            return PartialView("_TabContent", groupProduct);
        }

        public ActionResult AddDishes(/*string[] dishIDs*/InteGroupProductModels data /*, int currentGroupOffSet, int currentDishOffset*/)
        {
            InteGroupProductModels model = new InteGroupProductModels();
            if (data.ListProductOnGroup != null && data.ListProductOnGroup.Count() > 0)
                model.ListProductOnGroup = new List<InteProductOnGroupModels>();

            for (int i = 0; i < data.ListProductOnGroup.Count(); i++)
            {
                InteProductOnGroupModels dish = new InteProductOnGroupModels();
                dish.OffSet = data.currentOffset;

                dish.ProductID = data.ListProductOnGroup[i].ProductID;
                dish.ProductName = data.ListProductOnGroup[i].ProductName;
                dish.Seq = data.ListProductOnGroup[i].Seq;
                dish.ExtraPrice = data.ListProductOnGroup[i].ExtraPrice;
                dish.Sequence = data.ListProductOnGroup[i].Sequence;
                model.ListProductOnGroup.Add(dish);

                data.currentOffset++;
            }
            model.OffSet = data.currentgroupOffSet; // trongntn
            model.StoreID = data.StoreID;
            model.StoreOffSet = data.StoreOffSet;
            return PartialView("_DishModal", model);
        }

        public ActionResult LoadDishes(bool isMultiChoice, string StoreID)
        {
            var lstDish = _factory.GetProductApplyStore(StoreID, (byte)Commons.EProductType.Dish);
            InteGroupProductModels model = new InteGroupProductModels();
            if (lstDish != null)
            {
                model.ListProductOnGroup = new List<InteProductOnGroupModels>();
                foreach (var item in lstDish)
                {
                    InteProductOnGroupModels dish = new InteProductOnGroupModels()
                    {
                        ProductID = item.ID,
                        ProductName = item.Name,
                        //Seq = item.OrderByIndex,
                        //ExtraPrice = item.ExtraPrice
                    };
                    model.ListProductOnGroup.Add(dish);
                }
            }
            model.ListProductOnGroup = model.ListProductOnGroup.OrderBy(x => x.ProductName).ToList();
            if (isMultiChoice)
                return PartialView("_TableChooseDishes", model);
            return PartialView("_TableChooseDish", model);
        }

        public ActionResult CheckDish(InteProductModels setMenuModel)
        {
            //List<string> lstStoreIndex = setMenuModel.ApplyStoreModel.GetSelectedStoreIndex();
            //if (setMenuModel != null && setMenuModel.ListGroup != null)
            //    foreach (MediateSetGroup group in setMenuModel.ListGroup)
            //        if (group.DishModel != null && group.DishModel.ListDish != null)
            //            foreach (MediateSetGroupDish dish in group.DishModel.ListDish)
            //            {
            //                string dishID = dish.ItemID;
            //                if (!StoreHaveDish(lstStoreIndex, dishID))
            //                    dish.Status = 9;
            //            }

            return null;// PartialView("_BodyTableTabContent", setMenuModel.ListGroup);
        }
        /*==========*/
        public ActionResult AddTabClone(InteGroupProductModels model)
        {
            InteGroupProductModels groupProduct = new InteGroupProductModels();
            groupProduct.OffSet = model.OffSet;
            groupProduct.StoreID = model.StoreID;
            groupProduct.StoreOffSet = model.StoreOffSet;
            groupProduct.Sequence = model.Sequence;
            groupProduct.Name = model.Name;
            groupProduct.Description = model.Description;
            groupProduct.Maximum = model.Maximum;
            return PartialView("_TabContent", groupProduct);
        }

        public ActionResult AddDishesClone(InteGroupProductModels data)
        {
            var lstDish = _factory.GetProductApplyStore(data.StoreID, (byte)Commons.EProductType.Dish).Select(x => x.ID).ToList();

            InteGroupProductModels model = new InteGroupProductModels();
            if (data.ListProductOnGroup != null && data.ListProductOnGroup.Count() > 0)
                model.ListProductOnGroup = new List<InteProductOnGroupModels>();

            data.ListProductOnGroup = data.ListProductOnGroup.Where(x => lstDish.Contains(x.ProductID)).ToList();
            for (int i = 0; i < data.ListProductOnGroup.Count(); i++)
            {
                InteProductOnGroupModels dish = new InteProductOnGroupModels();
                dish.OffSet = data.currentOffset;

                dish.ProductID = data.ListProductOnGroup[i].ProductID;
                dish.ProductName = data.ListProductOnGroup[i].ProductName;
                dish.Seq = data.ListProductOnGroup[i].Seq;
                dish.ExtraPrice = data.ListProductOnGroup[i].ExtraPrice;
                dish.Sequence = data.ListProductOnGroup[i].Sequence;
                model.ListProductOnGroup.Add(dish);

                data.currentOffset++;
            }
            model.OffSet = data.currentgroupOffSet; // trongntn
            model.StoreID = data.StoreID;
            model.StoreOffSet = data.StoreOffSet;
            return PartialView("_DishModal", model);
        }

        #endregion

        /*POnS*/
        public ActionResult AddMoreProductItemOnStore(int currentOffset, string StoreID, string StoreName)
        {
            InteProductItemOnStore model = new InteProductItemOnStore();
            InteProductPriceModels PriceOnStore = new InteProductPriceModels();
            model.CurrencySymbol = CurrentUser.CurrencySymbol;
            model.OffSet = currentOffset;
            model.StoreName = StoreName;
            model.StoreID = StoreID;
            model.HasServiceCharge = true;
            //================Price
            SeasonFactory seasonf = new SeasonFactory();
            List<SeasonModels> lstSs = seasonf.GetListSeason(model.StoreID, null, CurrentUser.ListOrganizationId);
            List<SelectListItem> lstSllItemSeason = new List<SelectListItem>();
            SelectListItem sslItemSeason;

            //if (lstSs != null && lstSs.Count > 0)
            //    PriceOnStore.SeasonPriceID = lstSs[0].ID;
            foreach (var season in lstSs)
            {
                sslItemSeason = new SelectListItem();
                sslItemSeason.Value = season.ID;
                sslItemSeason.Text = season.Name;
                sslItemSeason.Selected = season.ID.Equals(PriceOnStore.SeasonPriceID);
                lstSllItemSeason.Add(sslItemSeason);
            }
            foreach (PriceItem priceItem in model.ListPrices)
                priceItem.ListSeasons = lstSllItemSeason;
            if (model.ListPrices != null && model.ListPrices.Count > 0)
            {
                model.ListPrices[0].Price = PriceOnStore.DefaultPrice;
                model.ListPrices[1].Price = PriceOnStore.SeasonPrice == -1 ? 0 : PriceOnStore.SeasonPrice;
                model.ListPrices[1].SeasonID = null;// PriceOnStore.SeasonPriceID;

                model.ListPrices[0].StoreID = model.StoreID;
                model.ListPrices[1].StoreID = model.StoreID;
            }

            //======Category 
            model.ListCategories = this.GetSelectListCategoriesInte(StoreID, Commons.EProductType.SetMenu.ToString("d"));

            // Updated 08282017
            model.lstCateGroup = this.GetSelectListCategoriesInteSortParent(StoreID, Commons.EProductType.SetMenu.ToString("d"));
            if (!string.IsNullOrEmpty(model.CategoryID))
            {
                var cateChoose = model.lstCateGroup.Where(w => w.Id == model.CategoryID).FirstOrDefault();
                if (cateChoose != null)
                {
                    cateChoose.Selected = true;
                }
            }

            //Kiosk | POS Availability
            var listKiosk = GetListTimeSlot02(model.StoreID, null);             //Kiosk
            model.ListSeasonKiosk = listKiosk;

            //============ LoadServiceCharge
            var objServiceCharge = _tipServiceChargeFactory.GetListTipServiceCharge(model.StoreID);
            if (objServiceCharge != null && objServiceCharge.Count > 0)
            {
                var value = objServiceCharge[0].Value;
                var IsCurrency = objServiceCharge[0].IsCurrency;
                var IsIncludedOnBill = objServiceCharge[0].IsIncludedOnBill;
                model.sServiceCharge = value.ToString();
                if (!IsCurrency)
                {
                    //$('#chbServiceCharge').attr('disabled', false);
                    model.ServiceChargeDisabled = false;
                }
                else if (IsCurrency || IsIncludedOnBill)
                {
                    //$('#chbServiceCharge').attr('disabled', true);
                    model.ServiceChargeDisabled = true;
                }
            }

            //=======Tax
            var listTax = _taxFactory.GetListTax(model.StoreID);
            listTax = listTax.Where(x => x.IsActive).ToList();
            if (listTax != null && listTax.Count > 0)
                model.IsTaxRequired = true;
            model.ListTax = listTax;
            model.ListTax.ForEach(x => x.Status = (int)Commons.EStatus.Deleted);
            //==============
            return PartialView("_ProductItemOnStore", model);
        }

        public ActionResult ExtendAll()
        {
            var model = new InteProductViewModels();
            //ViewBag.ListStoreTo = ViewBag.ListStore;
            model.ListStoreTo = (SelectList)ViewBag.StoreID;
            return View(model);
        }

        [HttpPost]
        public ActionResult ExtendAll(InteProductViewModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (model.ListStoreID == null || model.ListStoreID.Count == 0)
                    ModelState.AddModelError("ListStoreID", CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (model.ListItem == null || model.ListItem.Count(x => x.IsSelected) == 0)
                    ModelState.AddModelError("ListItem", CurrentUser.GetLanguageTextFromKey("Please choose item"));
                else
                {
                    model.IsCheckAll = model.ListItem.Count == model.ListItem.Count(x => x.IsSelected);
                }
                //var temp = ViewBag.ListStore as List<SelectListItem>;
                //var temps = temp.Where(x => x.Value != model.StoreID).Select(x => new SelectListItem { Value = x.Value, Text = x.Text }).ToList();
                //ViewBag.ListStoreTo = temps;

                // Return new ListStoreTo 
                var lstStoreView = (List<StoreModels>)ViewBag.StoreID.Items;
                var temps = lstStoreView.Where(ww => ww.Id != model.StoreID).ToList();
                model.ListStoreTo = new SelectList(temps, "Id", "Name", "CompanyName", 1);


                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                string msg = "";
                DishImportResultModels importModel = new DishImportResultModels();
                importModel.ListImport = _factory.ExtendProduct(model, ref msg);
                if (string.IsNullOrEmpty(msg))
                {
                    return View("ExtendDetail", importModel);
                }
                else
                {
                    ModelState.AddModelError("StoreID", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ExtendAll :", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

        }

        public ActionResult LoadProductByStore(string StoreId)
        {
            var model = new InteProductViewModels();
            try
            {
                var result = _factory.GetListProduct((byte)Commons.EProductType.SetMenu, CurrentUser.ListOrganizationId);
                if (!string.IsNullOrEmpty(StoreId))
                {
                    result = result.Where(o => o.ListStore.Select(s => s.ID).ToList().Any(sID => sID.Equals(StoreId))).ToList();
                }
                result.ForEach(x =>
                {
                    x.ImageURL = string.IsNullOrEmpty(x.ImageURL) ? Commons.Image100_100 : x.ImageURL;
                    x.ListStoreName = x.ListStore.Select(z => z.Name).ToList();
                    x.ListStoreName = x.ListStoreName.OrderBy(s => s).ToList();
                    x.StoreName = string.Join(", ", x.ListStoreName);
                    x.CategoryName = x.ListCategoryName != null ? x.ListCategoryName[0] : "";
                });
                model.ListItem = result;
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("LoadProductByStore", ex);
            }
            return PartialView("_ItemProductExtend", model);
        }
    }
}