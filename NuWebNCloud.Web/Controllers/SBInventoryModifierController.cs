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
    public class SBInventoryModifierController : SBInventoryBaseController
    {
        // GET: SBInventoryModifier
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private ProductFactory _factory = null;

        //List<string> listPropertyReject = null;

        public SBInventoryModifierController()
        {
            _factory = new ProductFactory();
            ViewBag.ListStore = GetListStore();
            //================
            //listPropertyReject = new List<string>();
            //listPropertyReject.Add("Measure");
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
                _logger.Error("Product_Modifier_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(ProductViewModels model)
        {
            try
            {
                var datas = _factory.GetListProduct(model.StoreID, (byte)Commons.EProductType.Modifier, CurrentUser.ListOrganizationId);
                foreach (var item in datas)
                {
                    item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
                }
                model.ListItem = datas;
            }
            catch (Exception e)
            {
                _logger.Error("Product_Modifier_Search: " + e);
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

                // Updated 08302017
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

                //Return Data
                model.ListPrices = GetPrices(model.ListPrices, model.StoreID);
                model.ListCategories = GetSelectListCategories(model.StoreID, Commons.EProductType.Modifier.ToString("d"));

                if (model.ExpiredDate == null)
                    model.ExpiredDate = Commons._ExpiredDate;
                else
                    model.ExpiredDate = new DateTime(model.ExpiredDate.Value.Year, model.ExpiredDate.Value.Month, model.ExpiredDate.Value.Day, 12, 59, 59);
                //PropertyReject();

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

                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    //imageLength = model.PictureUpload.ContentLength;
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                    photoByte = imgByte;
                }

                //====================
                model.ProductTypeID = Commons.EProductType.Modifier.ToString("d");
                model.ProductTypeCode = (byte)Commons.EProductType.Modifier;

                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";

                    // Updated 08282017
                    model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Modifier.ToString("d"));
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
                    //return RedirectToAction("Create");
                    ModelState.AddModelError("name", msg);

                    // Updated 08282017
                    model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Modifier.ToString("d"));
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
            catch (Exception ex)
            {
                _logger.Error("Product_Modifier_Create: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);
                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey("Have an error"));

                // Updated 08282017
                model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Modifier.ToString("d"));
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
                model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                model.sServiceCharge = model.ServiceCharge == -1 ? "" : model.ServiceCharge.ToString();
                //================Get Category
                CategoriesFactory categoryf = new CategoriesFactory();
                //List<CategoriesModels> lstCg = categoryf.GetListCategory(model.StoreID, null, Commons.EProductType.Modifier.ToString("d"), CurrentUser.ListOrganizationId);
                //SelectListItem sslItemCag;
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
                List<SBInventoryBaseCateGroupViewModel> lstCateGroup = categoryf.GetListCategorySortParent(model.StoreID, null, Commons.EProductType.Modifier.ToString("d"), CurrentUser.ListOrganizationId, model.CategoryID);

                model.lstCateGroup = lstCateGroup;

                //================Price/Season
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

                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Product_Modifier_Detail: " + ex);
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

        //public void PropertyReject()
        //{
        //    foreach (var item in listPropertyReject)
        //    {
        //        if (ModelState.ContainsKey(item))
        //            ModelState[item].Errors.Clear();
        //    }
        //}

        [HttpPost]
        public ActionResult Edit(ProductModels model)
        {
            try
            {
                byte[] photoByte = null;
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store."));

                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Modifier Name is required"));

                // Updated 08302017
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

                if (model.ListPrices[1].Price < 0)
                    ModelState.AddModelError("ListPrices[1].Price", CurrentUser.GetLanguageTextFromKey("Please input default price greater than 0"));

                for (int i = 1; i < model.ListPrices.Count; i++)
                    if (model.ListPrices[i].Price > 0 && string.IsNullOrEmpty(model.ListPrices[i].SeasonID))
                        ModelState.AddModelError(string.Format("ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please select Season for Price"));

                for (int i = 1; i < model.ListPrices.Count; i++)
                    if (!string.IsNullOrEmpty(model.ListPrices[i].SeasonID) && model.ListPrices[i].Price < 0)
                        ModelState.AddModelError(string.Format("ListPrices[{0}].Price", i), CurrentUser.GetLanguageTextFromKey("Please input price greater than 0"));

                //PropertyReject();
                if (model.ExpiredDate == null)
                    model.ExpiredDate = Commons._ExpiredDate;
                else
                    model.ExpiredDate = new DateTime(model.ExpiredDate.Value.Year, model.ExpiredDate.Value.Month, model.ExpiredDate.Value.Day, 12, 59, 59);

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
                //===============
                model.ProductTypeID = Commons.EProductType.Modifier.ToString("d");
                model.ProductTypeCode = (byte)Commons.EProductType.Modifier;
                //===============
                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";

                    //Return Data
                    model = GetDetail(model.ID);

                    // Updated 08282017
                   // model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Modifier.ToString("d"));
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
                //===============
                string msg = "";
                var tmp = model.PictureByte;
                model.PictureByte = null;
                var result = _factory.InsertOrUpdateProduct(model, ref msg);
                if (result)
                {
                    model.PictureByte = tmp;
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
                   // model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Modifier.ToString("d"));
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
                _logger.Error("Product_Modifier_Edit: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);
                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey("Have a error"));
                model = GetDetail(model.ID);

                // Updated 08282017
                //model.lstCateGroup = GetSelectListCategoriesSortParent(model.StoreID, Commons.EProductType.Modifier.ToString("d"));
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
                    //ModelState.AddModelError("Name", "Have an error when you delete a Set menu");
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Product_Modifier_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Have an error when you delete a modifier"));
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
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store"));
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
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("", CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
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
                    //Directory.CreateDirectory(filePath);
                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ExcelUpload.SaveAs(filePath);

                    int totalRowExcel;
                    importModel.ListImport = _factory.ImportModifier(filePath, listFiles, out totalRowExcel, model.ListStores, ref msg);
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
                    _logger.Error("Product_Modifier_Import: " + msg);
                    ModelState.AddModelError("ImageZipUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Product_Modifier_Import: " + e);
                ModelState.AddModelError("ImageZipUpload", string.Format(CurrentUser.GetLanguageTextFromKey("Import file have error").ToString() +": {0}", e.Message));
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
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsSetMenu = wb.Worksheets.Add("Modifiers");

                StatusResponse response = _factory.ExportModifier(ref wsSetMenu, model.ListStores);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Modifiers").Replace(" ", "_")));

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
                _logger.Error("Product_Modifier_Import: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }
    }
}