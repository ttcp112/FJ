using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Integration.Factory.Sandbox.Categories;
using NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models.Settings;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SBInventoryCategoriesInteController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private InteCategoriesFactory _factory = null;
        private ProductTypeFactory _productTypeFactory = null;
        private XeroFactory _facXero = null;
        public SBInventoryCategoriesInteController()
        {
            _factory = new InteCategoriesFactory();
            _productTypeFactory = new ProductTypeFactory();
            _facXero = new XeroFactory();
            //================
            //ViewBag.ListStore = GetListStore();
            //if (Commons.isIntegrateXero)
            var ListAcc = new List<XeroDTO>();
            ViewBag.ListAccountXero = new SelectList(ListAcc, "Code", "NameDisplayCombobox", "ReportingCodeName", 1);
        }
        // GET: SBInventoryCategoriesFnB
        public ActionResult Index()
        {
            try
            {
                InteCategoriesViewModels model = new InteCategoriesViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Categories_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(InteCategoriesViewModels model)
        {
            try
            {
                var datas = _factory.GetListCategorOfCate(model.ListStoreID, null, null, CurrentUser.ListOrganizationId, CurrentUser.ListStoreID);
                foreach (var item in datas)
                {
                    item.ProductTypeName = _productTypeFactory.GetProductTypeName(item.Type);
                    item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
                    item.ListCategoryOnStore = item.ListCategoryOnStore.OrderBy(s => s.StoreName).ToList();
                    item.StoreName = string.Join(", ", item.ListCategoryOnStore.Select(z => z.StoreName).ToList());
                }
                model.ListItem = datas;
            }
            catch (Exception e)
            {
                _logger.Error("Categories_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            InteCategoriesModels model = new InteCategoriesModels();
            model.ProductTypeID = model.ListProductType.Where(w => w.Text.ToLower().Trim() == Commons.EProductType.Dish.ToString().ToLower().Trim()).Select(s => s.Value).FirstOrDefault();
            model.IsIncludeNetSale = true;
            //model.GetListProductType();
            //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
            //model.ListStore = lstStore;
            var lstStore = (SelectList)ViewBag.StoreID;
            model.ListStore = lstStore.ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(InteCategoriesModels model)
        {
            try
            {
                byte[] photoByte = null;
                var countStoreFalse = model.ListStore.Where(ww => ww.Selected == true).Count();
                if (countStoreFalse == 0)
                {
                    ModelState.AddModelError("ListStore", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                }

                if (countStoreFalse > 0)
                {
                    List<string> LstStore = new List<string>();
                    if (model.ProductTypeID == Commons.EProductType.Dish.ToString("d"))
                    {
                        LstStore = model.ListStore.Where(w => w.Selected == true).Select(s => s.Value).ToList();
                        //model.ListCategories = GetSelectListParentCategory(LstStore, model.ProductTypeID);
                    }
                }

                if (model.ParentID != null)
                {
                    var parentName = model.ListProductType.Find(x => x.Value.Equals(model.ProductTypeID)).Text.ToLower();
                    if (!parentName.Equals("dish"))
                    {
                        model.ParentID = null;
                    }
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name field is required"));
                }

                if (model.ListItemOnStores != null && model.ListItemOnStores.Count > 0)
                {
                    model.ListItemOnStores = model.ListItemOnStores.Where(o => o.State == 0).ToList();
                    if (model.ListItemOnStores != null && model.ListItemOnStores.Count > 0)
                    {
                        for (int i = 0; i < model.ListItemOnStores.Count; i++)
                        {
                            if (Convert.ToInt32(model.ListItemOnStores[i].Sequence) < 0)
                            {
                                ModelState.AddModelError(string.Format("ListItemOnStores[{0}].Sequence", model.ListItemOnStores[i].OffSet), CurrentUser.GetLanguageTextFromKey("Limit must be more than 0"));
                            }
                        }
                    }
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

                if (!ModelState.IsValid)
                {
                    model.ListItemOnStores = model.ListItemOnStores.Where(o => o.State == 0).ToList();
                    List<string> LstStore = new List<string>();
                    LstStore = model.ListItemOnStores.Select(o => o.StoreID).ToList();
                    if (model.ListItemOnStores.Count > 0)
                    {
                        model.ListCategories = GetSelectListParentCategory(LstStore, model.ProductTypeID);

                        // Updated 09222017
                        model.lstCateGroup = GetSelectListParentCategorySort(LstStore, model.ProductTypeID, model.ParentID);
                    }
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";

                    var lstStore = (SelectList)ViewBag.StoreID;
                    model.ListStore = lstStore.ToList();

                    return View(model);
                }
                if (model.ListItemOnStores != null && model.ListItemOnStores.Count > 0)
                {
                    model.ListItemOnStores = model.ListItemOnStores.Where(o => o.State == 0).ToList();
                    foreach (var item in model.ListItemOnStores)
                    {
                        CategoryOnStoreWebDTO newItemOnStore = new CategoryOnStoreWebDTO();
                        newItemOnStore.StoreID = item.StoreID;
                        newItemOnStore.StoreName = item.StoreName;
                        newItemOnStore.IsShowInKiosk = item.IsShowInKiosk;
                        newItemOnStore.IsShowInReservation = item.IsShowInReservation;
                        newItemOnStore.Sequence = item.Sequence;
                        model.ListCategoryOnStore.Add(newItemOnStore);
                    }
                }

                //====================
                string msg = "";
                var tmp = model.PictureByte;
                model.PictureByte = null;
                bool result = _factory.InsertOrUpdateCategories(model, ref msg);
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

                    // Updated 09222017
                    List<string> LstStore = model.ListItemOnStores.Select(o => o.StoreID).ToList();
                    model.lstCateGroup = GetSelectListParentCategorySort(LstStore, model.ProductTypeID, model.ParentID);

                    var lstStore = (SelectList)ViewBag.StoreID;
                    model.ListStore = lstStore.ToList();

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Categories_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public InteCategoriesModels GetDetail(string id, int type = 0)
        {
            string Type = type.ToString();
            try
            {
                InteCategoriesModels model = _factory.GetListCategory(null, id, null, CurrentUser.ListOrganizationId).FirstOrDefault();
                if (model != null)
                {
                    model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                    model.ProductTypeName = _productTypeFactory.GetProductTypeName(model.Type);
                    if (model.ListCategoryOnStore != null && model.ListCategoryOnStore.Any())
                    {
                        model.ListCategoryOnStore = model.ListCategoryOnStore.OrderBy(oo => oo.StoreName).ToList();
                    }
                    if (!string.IsNullOrEmpty(model.ParentID) || model.ProductTypeName.ToLower().Equals("dish"))
                    {
                        List<string> LstStore = new List<string>();
                        LstStore.AddRange(model.ListCategoryOnStore.Select(s => s.StoreID).ToList());
                        model.ListCategories = GetSelectListParentCategory(LstStore, Type);

                        // Updated 09222017
                        // No include list cate child of current cate
                        model.lstCateGroup = GetSelectListParentCategorySortForEditCate(LstStore, Type, id, model.ParentID);

                    }
                    model.StoreName = string.Join(", ", model.ListCategoryOnStore.Select(z => z.StoreName).ToList());
                    foreach (var item in model.ListCategoryOnStore)
                    {
                        SelectListItem newItem = new SelectListItem();
                        newItem.Value = item.StoreID;
                        newItem.Text = item.StoreName;
                        newItem.Selected = true;
                        model.ListStore.Add(newItem);
                    }
                    return model;
                }
                else
                {
                    model = new InteCategoriesModels();
                    return model;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Categories_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            InteCategoriesModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id, int type)
        {
            InteCategoriesModels model = GetDetail(id, type);
            // var ListAcc = new List<SelectListItem>();
            var ListAcc = new List<XeroDTO>();
            if (model.ListCategories != null && model.ListCategories.Count > 0)
            {
                model.ListCategories = model.ListCategories.Where(o => o.Text != model.Name).ToList();
            }
            model.ProductTypeID = model.Type.ToString();
            if (model.ListCategoryOnStore != null && model.ListCategoryOnStore.Count > 0)
            {
                for (int i = 0; i < model.ListCategoryOnStore.Count; i++)
                {
                    model.ListItemOnStores.Add(new ItemOnStore()
                    {
                        StoreID = model.ListCategoryOnStore[i].StoreID,
                        StoreName = model.ListCategoryOnStore[i].StoreName,
                        IsShowInKiosk = model.ListCategoryOnStore[i].IsShowInKiosk,
                        IsShowInReservation = model.ListCategoryOnStore[i].IsShowInReservation,
                        Sequence = model.ListCategoryOnStore[i].Sequence,
                        OffSet = i,
                        State = 0,
                        IsCheckDisabled = 9
                    });
                }
                //model.ListStore = ViewBag.ListStore;
                var lstStore = (SelectList)ViewBag.StoreID;
                model.ListStore = lstStore.ToList();

                for (int i = 0; i < model.ListStore.Count; i++)
                {
                    var IsCheck = model.ListItemOnStores.Where(o => o.StoreID == model.ListStore[i].Value).FirstOrDefault();
                    if (IsCheck != null)
                    {
                        model.ListStore[i].Selected = true;
                        model.ListStore[i].Disabled = true;
                    }

                    /* get list account xero */
                    //if (Commons.isIntegrateXero)
                    {
                        var InfoStore = CurrentUser.listStore.Where(o => o.ID.Equals(model.ListStore[i].Value)).FirstOrDefault();
                        if (InfoStore != null)
                        {
                            var InfoXero = InfoStore.ThirdParty;
                            if (InfoXero != null)
                            {
                                if (!string.IsNullOrEmpty(InfoXero.ApiURL))
                                {
                                    var data = _facXero.GetListXeroSetting(InfoXero.IPAddress, InfoXero.ThirdPartyID, InfoXero.ApiURL, null);
                                    if (data != null)
                                    {
                                        //var dataJson = data.Select(o => new SelectListItem
                                        //{
                                        //    Value = o.AccountID,
                                        //    Text = o.Name
                                        //}).ToList();
                                        //ListAcc.AddRange(dataJson);
                                        ListAcc.AddRange(data);
                                    }
                                }
                            }
                        }
                        ListAcc = ListAcc.GroupBy(o => o.AccountID)
                                          .Select(o => new XeroDTO
                                          {
                                              AccountID = o != null ? o.FirstOrDefault().AccountID : "",
                                              Name = o != null ? o.FirstOrDefault().Name : "",
                                              Code = o != null ? o.FirstOrDefault().Code : "",
                                              ReportingCodeName = o != null ? o.FirstOrDefault().ReportingCodeName : "",
                                              NameDisplayCombobox = o != null ? o.FirstOrDefault().Code + "-" + o.FirstOrDefault().Name : "",
                                          }).OrderBy(o => o.ReportingCodeName).ToList();

                        //ListAcc = ListAcc.GroupBy(o => new { Value = o.Value, Text = o.Text }).Select(o => new SelectListItem
                        //{
                        //    Value = o.Key.Value,
                        //    Text = o.Key.Text
                        //}).ToList();
                    }
                }
            }
            else
            {
                //model.ListStore = ViewBag.ListStore;
                var lstStore = (SelectList)ViewBag.StoreID;
                model.ListStore = lstStore.ToList();
            }
            ListAcc = ListAcc.OrderBy(o => o.ReportingCodeName).ToList();
            ViewBag.ListAccountXero = new SelectList(ListAcc, "Code", "NameDisplayCombobox", "ReportingCodeName", 1);
            //model.GetListProductType();
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(InteCategoriesModels model)
        {
            model.ProductTypeID = model.Type.ToString();
            try
            {
                byte[] photoByte = null;
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name field is required"));
                }

                if (model.ListItemOnStores != null && model.ListItemOnStores.Count > 0)
                {
                    model.ListItemOnStores = model.ListItemOnStores.Where(o => o.State == 0).ToList();
                    if (model.ListItemOnStores != null && model.ListItemOnStores.Count > 0)
                    {
                        for (int i = 0; i < model.ListItemOnStores.Count; i++)
                        {
                            if (Convert.ToInt32(model.ListItemOnStores[i].Sequence) < 0)
                            {
                                ModelState.AddModelError(string.Format("ListItemOnStores[{0}].Sequence", model.ListItemOnStores[i].OffSet), CurrentUser.GetLanguageTextFromKey("Limit must be more than 0"));
                            }
                        }
                    }
                }

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

                // Updated 09222017
                string cateIdChoose = null;
                if (!string.IsNullOrEmpty(model.ParentID))
                {
                    cateIdChoose = model.ParentID;
                }

                if (!ModelState.IsValid)
                {
                    List<string> LstStore = new List<string>();
                    LstStore = model.ListItemOnStores.Select(o => o.StoreID).ToList();
                    List<string> LstStore1 = new List<string>();
                    LstStore1 = model.ListItemOnStores.Where(o => o.IsCheckDisabled == 0).Select(o => o.StoreID).ToList();
                    if (LstStore1.Count > 0 && LstStore1 != null)
                    {
                        for (int i = 0; i < LstStore1.Count; i++)
                        {
                            var IsCheck = model.ListStore.Where(o => o.Value == LstStore1[i]).FirstOrDefault();
                            if (IsCheck != null)
                            {
                                for (int j = 0; j < model.ListStore.Count; j++)
                                {
                                    if (model.ListStore[j].Value == LstStore1[i])
                                    {
                                        model.ListStore[j].Selected = true;
                                    }
                                }
                            }
                        }
                    }
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    model.ListCategories = GetSelectListParentCategory(LstStore, model.ProductTypeID);

                    // Updated 09222017
                    // No include list cate child of current cate
                    model.lstCateGroup = GetSelectListParentCategorySortForEditCate(LstStore, model.ProductTypeID.ToString(), model.ID, model.ParentID);

                    //model = GetDetail(model.ID);
                    //model.GetListProductType();
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                if (model.ListItemOnStores != null && model.ListItemOnStores.Count > 0)
                {
                    model.ListItemOnStores = model.ListItemOnStores.Where(o => o.State == 0).ToList();
                    foreach (var item in model.ListItemOnStores)
                    {
                        CategoryOnStoreWebDTO newItemOnStore = new CategoryOnStoreWebDTO();
                        newItemOnStore.StoreID = item.StoreID;
                        newItemOnStore.StoreName = item.StoreName;
                        newItemOnStore.IsShowInKiosk = item.IsShowInKiosk;
                        newItemOnStore.IsShowInReservation = item.IsShowInReservation;
                        newItemOnStore.Sequence = item.Sequence;
                        model.ListCategoryOnStore.Add(newItemOnStore);
                    }
                }
                //====================
                string msg = "";
                var tmp = model.PictureByte;
                model.PictureByte = null;
                var result = _factory.InsertOrUpdateCategories(model, ref msg);
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

                    model = GetDetail(model.ID);
                    model.GetListProductType();

                    // Updated 09222017
                    List<string> LstStore = model.ListItemOnStores.Select(o => o.StoreID).ToList();
                    model.lstCateGroup = GetSelectListParentCategorySortForEditCate(LstStore, model.ProductTypeID.ToString(), model.ID, model.ParentID);
                    if (!string.IsNullOrEmpty(cateIdChoose))
                    {
                        var cateChoose = model.lstCateGroup.Where(w => w.Id == cateIdChoose).FirstOrDefault();
                        if (cateChoose != null)
                        {
                            cateChoose.Selected = true;
                        }

                        var currentCate = model.lstCateGroup.Where(w => w.Id == model.ParentID).FirstOrDefault();
                        if (currentCate != null)
                        {
                            currentCate.Selected = false;
                        }

                    }

                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Categories_Edit: " + ex);
                model = GetDetail(model.ID);
                model.GetListProductType();

                // Updated 09222017
                // No include list cate child of current cate
                List<string> LstStore = model.ListItemOnStores.Select(o => o.StoreID).ToList();
                model.lstCateGroup = GetSelectListParentCategorySortForEditCate(LstStore, model.ProductTypeID.ToString(), model.ID, model.ParentID);

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            InteCategoriesModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(InteCategoriesModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteCategories(model.ID, ref msg);
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
                _logger.Error("Categories_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Categories"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        /*Load Parent Category*/
        public List<SelectListItem> GetSelectListParentCategory(List<string> LstStoreID, string ProductTypeID)
        {
            List<InteCategoriesModels> lstData = new List<InteCategoriesModels>();
            var result = _factory.GetListCategory(LstStoreID, null, ProductTypeID, CurrentUser.ListOrganizationId);
            lstData.AddRange(result);
            List<SelectListItem> slcParentCate = new List<SelectListItem>();
            if (lstData != null)
            {
                lstData = lstData.Where(x => string.IsNullOrEmpty(x.ParentName)).ToList();
                lstData = lstData.Where(o => o.Type == Convert.ToInt32(ProductTypeID)).ToList();
                foreach (InteCategoriesModels item in lstData)
                    slcParentCate.Add(new SelectListItem
                    {
                        Text = item.Name/* + " [" + item.StoreName + "]"*/,
                        Value = item.ID
                    });
            }
            return slcParentCate;
        }

        //public PartialViewResult LoadParentCategory(string[] StoreID, string ProductTypeID)
        //{
        //    if (StoreID == null)
        //    {
        //        StoreID = new string[] { };
        //    }
        //    StoreID = StoreID.Where(ww => !string.IsNullOrEmpty(ww)).ToArray();
        //    List<InteCategoriesModels> lstData = new List<InteCategoriesModels>();
        //    if (StoreID.Length > 0)
        //    {
        //        List<string> LstSt = StoreID.ToList();
        //        var result = _factory.GetListCategory(LstSt, null, ProductTypeID, CurrentUser.ListOrganizationId);
        //        lstData.AddRange(result);
        //        lstData = lstData.Where(o => o.Type == Convert.ToInt32(ProductTypeID)).ToList();
        //    }

        //    InteCategoriesModels model = new InteCategoriesModels();
        //    if (lstData != null && lstData.Count > 0)
        //    {
        //        lstData = lstData.Where(x => string.IsNullOrEmpty(x.ParentName)).ToList();
        //        foreach (InteCategoriesModels data in lstData)
        //        {
        //            SelectListItem item = new SelectListItem();
        //            item.Value = data.ID;
        //            item.Text = data.Name /*+ " [" + data.StoreName + "]"*/;
        //            model.ListCategories.Add(item);
        //        }
        //    }
        //    var lstStore = (List<SelectListItem>)ViewBag.ListStore;
        //    return PartialView("_DDLParentCategory", model);
        //}

        public ActionResult LoadItemOnStore(int currentOffset, string StoreID, string ProductTypeID, string StoreName)
        {
            ItemOnStore model = new ItemOnStore();
            //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
            var lstStoreInfo = (SelectList)ViewBag.StoreID;
            var lstStore = lstStoreInfo.ToList();
            StoreName = lstStore.Where(ww => ww.Value == StoreID).Select(ss => ss.Text).FirstOrDefault();
            model.OffSet = currentOffset;
            model.StoreID = StoreID;
            model.StoreName = StoreName;
            return PartialView("_ListItemOnStore", model);
        }

        //public ActionResult LoadParentCategory(int currentOffset, string[] StoreID, string ProductTypeID, string StoreName)
        //{
        //    List<CategoriesModels> lstData = _factory.GetListCategory(StoreID[0], null, ProductTypeID, CurrentUser.ListOrganizationId);
        //    CategoryChild model = new CategoryChild();
        //    if (lstData != null && lstData.Count > 0)
        //    {
        //        lstData = lstData.Where(x => string.IsNullOrEmpty(x.ParentName)).ToList();

        //        foreach (CategoriesModels data in lstData)
        //        {
        //            SelectListItem item = new SelectListItem();
        //            item.Value = data.ID;
        //            item.Text = data.Name + " [" + data.StoreName + "]";
        //            model.ListCategories.Add(item);
        //        }
        //    }
        //    model.OffSet = currentOffset;
        //    model.StoreID = StoreID[0];
        //    var lstStore = (List<SelectListItem>)ViewBag.ListStore;
        //    if ((StoreID != null) && (StoreName == ""))
        //    {
        //        StoreName = lstStore.Where(ww => ww.Value == StoreID[0]).Select(ss => ss.Text).FirstOrDefault();
        //    }
        //    model.StoreName = StoreName;

        //    return PartialView("_DDLParentCategory", model);
        //}

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
                    ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey("File excel cannot be null"));
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
                    importModel.ListImport = _factory.Import(filePath, listFiles, out totalRowExcel, CurrentUser.ListOrganizationId, ref msg);
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
                    NSLog.Logger.Info("Categories_Import: " , msg);
                    ModelState.AddModelError("ImageZipUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("Categories_Import: ", e);
                ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey(e.Message));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            InteCategoriesModels model = new InteCategoriesModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(InteCategoriesModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsCategoryMerchant = wb.Worksheets.Add("Category Merchant");
                var wsCategoryStore = wb.Worksheets.Add("Category Store");

                StatusResponse response = _factory.Export(ref wsCategoryMerchant, ref wsCategoryStore, model.ListStores, CurrentUser.ListOrganizationId);
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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Categories").Replace(" ", "_")));

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
                _logger.Error("Categories_Import: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }



        // Updated 09222017
        // Display category by parent & child, sort by seq & name
        public List<SBInventoryBaseCateGroupViewModel> GetSelectListParentCategorySort(List<string> LstStoreID, string ProductTypeID, string CurCateId = null)
        {
            List<SBInventoryBaseCateGroupViewModel> lstData = _factory.GetListCategorySortParent(LstStoreID, null, ProductTypeID, CurrentUser.ListOrganizationId, CurCateId);
            return lstData;
        }

        // No include list cate child of current cate
        public List<SBInventoryBaseCateGroupViewModel> GetSelectListParentCategorySortForEditCate(List<string> LstStoreID, string ProductTypeID, string CurCateId, string CurParentId)
        {
            List<SBInventoryBaseCateGroupViewModel> lstData = _factory.GetListCategorySortParentForEditCate(LstStoreID, null, ProductTypeID, CurrentUser.ListOrganizationId, CurCateId, CurParentId);

            return lstData;
        }

        public PartialViewResult LoadParentCategory(string[] StoreID, string ProductTypeID)
        {
            List<SBInventoryBaseCateGroupViewModel> lstData = new List<SBInventoryBaseCateGroupViewModel>();
            if (StoreID == null)
            {
                StoreID = new string[] { };
            }
            List<string> lstStoreId = StoreID.Where(w => !string.IsNullOrEmpty(w)).ToList();
            if(lstStoreId != null && lstStoreId.Any())
            {
                lstData = _factory.GetListCategorySortParent(lstStoreId, null, ProductTypeID, CurrentUser.ListOrganizationId);
            }
            SBInventoryBaseModel model = new SBInventoryBaseModel();
            model.lstCateGroup = lstData;

            return PartialView("_DDLParentCategory", model);
        }

        public ActionResult Extend()
        {
            InteCategoriesViewModels model = new InteCategoriesViewModels();
            model.ListStoreTo = (SelectList)ViewBag.StoreID;
            return View(model);
        }

        [HttpPost]
        public ActionResult Extend(InteCategoriesViewModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreExtendFrom))
                {
                    ModelState.AddModelError("StoreExtendFrom", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                } 
                if (model.StoreExtendTo == null || model.StoreExtendTo.Count == 0)
                {
                    ModelState.AddModelError("StoreExtendTo", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                }
                   
                if (model.ListItem == null || model.ListItem.Count(x => x.IsSelected) == 0)
                {
                    ModelState.AddModelError("ListItem", CurrentUser.GetLanguageTextFromKey("Please choose item."));
                }

                // Return new ListStoreTo 
                var lstStoreView = (List<StoreModels>)ViewBag.StoreID.Items;
                var temps = lstStoreView.Where(ww => ww.Id != model.StoreExtendFrom).ToList();
                model.ListStoreTo = new SelectList(temps, "Id", "Name", "CompanyName", 1);

                if (!ModelState.IsValid)
                {  
                    return View(model);
                }
                //====================
                SetMenuImportResultModels importModel = new SetMenuImportResultModels();
                string msg = "";
                importModel = _factory.ExtendCategories(model, ref msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    return View("ExtendDetail", importModel);
                }
                else
                {
                    ModelState.AddModelError("extend", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Categories_Extend: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        public ActionResult LoadCateExtend(string storeID)
        {
            List<string> ListStoreID = new List<string>();
            ListStoreID.Add(storeID);
            InteCategoriesViewModels model = new InteCategoriesViewModels();
            var datas = _factory.GetListCategorOfCate(ListStoreID, null, null, CurrentUser.ListOrganizationId, CurrentUser.ListStoreID);
            foreach (var item in datas)
            {
                item.ProductTypeName = _productTypeFactory.GetProductTypeName(item.Type);
                item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
                item.ListCategoryOnStore = item.ListCategoryOnStore.OrderBy(s => s.StoreName).ToList();
                item.StoreName = string.Join(", ", item.ListCategoryOnStore.Select(z => z.StoreName).ToList());
            }
            model.ListItem = datas;
            return PartialView("_ListCateExtend", model);
        }

        // Updated 04192018, for list stores group by company
        public ActionResult LoadListStoreExtendToOfCate(string StoreId)
        {
            // Return for new list stores extend
            InteCategoriesViewModels model = new InteCategoriesViewModels();
            var lstStoreView = (List<StoreModels>)ViewBag.StoreID.Items;
            var temps = lstStoreView.Where(ww => ww.Id != StoreId).ToList();
            model.ListStoreTo = new SelectList(temps, "Id", "Name", "CompanyName", 1);
            return PartialView("_LoadListStoreExtendToOfCategory", model);
        }

        public JsonResult GetAccountXeroByStore(string[] StoreID)
        {
            try
            {
                NSLog.Logger.Info("GetAccountXeroByStore_Request : ", StoreID);
                /* get list account by store */
                if (StoreID != null && StoreID.Any())
                {
                    //var ListAcc = new List<SelectListItem>();
                    var ListAcc = new List<XeroDTO>();
                    foreach (var item in StoreID)
                    {
                        var InfoStore = CurrentUser.listStore.Where(o => o.ID.Equals(item)).FirstOrDefault();
                        if (InfoStore != null)
                        {
                            var InfoXero = InfoStore.ThirdParty;
                            if (InfoXero != null)
                            {
                                if (!string.IsNullOrEmpty(InfoXero.ApiURL))
                                {
                                    var data = _facXero.GetListXeroSetting(InfoXero.IPAddress, InfoXero.ThirdPartyID, InfoXero.ApiURL, null);
                                    if (data != null)
                                    {
                                        //var dataJson = data.Select(o => new SelectListItem
                                        //{
                                        //    Value = o.AccountID,
                                        //    Text = o.Name
                                        //}).ToList();
                                        // ListAcc.AddRange(dataJson);
                                        ListAcc.AddRange(data);
                                    }
                                }
                            }
                        }
                    }

                    //var _data = ListAcc.GroupBy(o => new { Value = o.Value , Text = o.Text }).Select(o => new
                    //{
                    //    id = o.Key.Value,
                    //    text = o.Key.Text
                    //}).OrderBy(o => o.text).ToList();
                    var LstCombobox = new List<AccountComboboxModel>();
                    ListAcc = ListAcc.GroupBy(o => o.AccountID)
                                         .Select(o => new XeroDTO
                                         {
                                             AccountID = o != null ? o.FirstOrDefault().AccountID : "",
                                             Name = o != null ? o.FirstOrDefault().Name : "",
                                             Code = o != null ? o.FirstOrDefault().Code : "",
                                             ReportingCodeName = o != null ? o.FirstOrDefault().ReportingCodeName : "",
                                             NameDisplayCombobox = o != null ? o.FirstOrDefault().Code + "-"+ o.FirstOrDefault().Name : "",
                                         }).ToList();
                    var Parrents = ListAcc.GroupBy(o => o.ReportingCodeName).ToList();
                    if(Parrents != null && Parrents.Any())
                    {
                        Parrents.ForEach(o =>
                        {
                            var _children = ListAcc.Where(x => x.ReportingCodeName.Equals(o.Key)).Select(y => new AccountChildrenModel {
                                id = y.Code,
                                text = y.NameDisplayCombobox,
                            }).ToList();
                            LstCombobox.Add(new AccountComboboxModel
                            {
                                text = o.Key,
                                children = _children
                            });
                        });
                    }
                    LstCombobox = LstCombobox.OrderBy(o => o.text).ToList();
                    return Json(LstCombobox, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetAccountXeroByStore : ", ex);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
    }
}
