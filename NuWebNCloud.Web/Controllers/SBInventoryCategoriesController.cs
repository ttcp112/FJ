using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SBInventoryCategoriesController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private CategoriesFactory _factory = null;
        private ProductTypeFactory _productTypeFactory = null;
        public SBInventoryCategoriesController()
        {
            _factory = new CategoriesFactory();
            _productTypeFactory = new ProductTypeFactory();
            //================
            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                CategoriesViewModels model = new CategoriesViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Categories_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(CategoriesViewModels model)
        {
            try
            {
                var datas = _factory.GetListCategory(model.StoreID, null, null, CurrentUser.ListOrganizationId);
                datas = datas.Where(x => x.StoreID == model.StoreID).ToList();
                foreach (var item in datas)
                {
                    item.ProductTypeName = CurrentUser.GetLanguageTextFromKey(_productTypeFactory.GetProductTypeName(item.Type));
                    item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
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
            CategoriesModels model = new CategoriesModels();
            //model.GetListProductType();
            model.ProductTypeID = model.ListProductType.Where(w => w.Text.ToLower().Trim() == Commons.EProductType.Dish.ToString().ToLower().Trim()).Select(s => s.Value).FirstOrDefault();
            model.IsIncludeNetSale = true;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CategoriesModels model)
        {
            try
            {
                byte[] photoByte = null;
                //model.GetListProductType();
                //if (model.ListStoreId.Count < 0)
                //{
                //    ModelState.AddModelError("StoreID", " Please choose Store");
                //}
                if (model.ParentID != null)
                {
                    var parentName = model.ListProductType.Find(x => x.Value.Equals(model.Type.ToString())).Text.ToLower();
                    if (!CurrentUser.GetLanguageTextFromKey(parentName).Equals(CurrentUser.GetLanguageTextFromKey("dish")))
                    {
                        model.ParentID = null;
                    }
                }
                //if (string.IsNullOrEmpty(model.Name))
                //{
                //    //model.ListCategories = GetSelectListParentCategory(model.StoreID, model.ProductTypeID);
                //    ModelState.AddModelError("Name", "Name field is required");
                //}
                //if (model.Type.ToString() == Commons.EProductType.Dish.ToString("d"))
                //{
                //    if (string.IsNullOrEmpty(model.ParentID))
                //    {
                //        model.ListCategories = GetSelectListParentCategory(model.StoreID, model.Type.ToString());
                //        ModelState.AddModelError("ParentID", "Parent category field is required");
                //    }
                //}                

                string ProductTypeText = "";
                if (model.ListProductType != null && model.ListProductType.Count > 0)
                {
                    ProductTypeText = model.ListProductType.Where(w => w.Value.Trim() == model.Type.ToString().Trim()).Select(s => s.Text).FirstOrDefault();
                    if (ProductTypeText.Trim().ToLower() != Commons.EProductType.Dish.ToString().Trim().ToLower())
                    {
                        model.GLAccountCode = null;
                        model.IsGiftCard = false;
                        model.IsIncludeNetSale = false;
                        model.ParentName = "";
                        model.ParentID = null;
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
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    //model.ListCategories = GetSelectListParentCategory(model.StoreID, model.Type.ToString());
                    
                    // Updated 09082017 sort by parent 
                    model.lstCateGroup = GetSelectListParentCategorySort(model.StoreID, model.Type.ToString());
                    if (!string.IsNullOrEmpty(model.ParentID))
                    {
                        var cateChoose = model.lstCateGroup.Where(w => w.Id == model.ParentID).FirstOrDefault();
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

                    // Updated 09082017 sort by parent 
                    model.lstCateGroup = GetSelectListParentCategorySort(model.StoreID, model.Type.ToString());
                    if (!string.IsNullOrEmpty(model.ParentID))
                    {
                        var cateChoose = model.lstCateGroup.Where(w => w.Id == model.ParentID).FirstOrDefault();
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
                _logger.Error("Categories_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public CategoriesModels GetDetail(string id)
        {
            try
            {
                CategoriesModels model = _factory.GetListCategory(null, id)[0];
                model.ProductTypeName = _productTypeFactory.GetProductTypeName(model.Type);
                model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                if (!string.IsNullOrEmpty(model.ParentID) || CurrentUser.GetLanguageTextFromKey(model.ProductTypeName).Equals(CurrentUser.GetLanguageTextFromKey("Dish")))
                {
                    //model.ListCategories = GetSelectListParentCategory(model.StoreID, model.ProductTypeID, id);

                    // Updated 09082017 sort by parent
                    // No include list cate child of current cate
                    model.lstCateGroup = GetSelectListParentCategorySortForEditCate(model.StoreID, model.ProductTypeID, id, model.ParentID);
                }
                return model;
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
            CategoriesModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            CategoriesModels model = GetDetail(id);
            //model.GetListProductType();
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(CategoriesModels model)
        {
            try
            {
                byte[] photoByte = null;
                //if (string.IsNullOrEmpty(model.StoreID))
                //{
                //    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose Store."));
                //}
                //if (string.IsNullOrEmpty(model.Name))
                //{
                //    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name is required"));
                //}
                //if (model.Type.ToString() == Commons.EProductType.Dish.ToString("d"))
                //{
                //    if (string.IsNullOrEmpty(model.ParentID))
                //    {
                //        ModelState.AddModelError("ParentID", "Parent category field is required");
                //    }
                //}
                if (string.IsNullOrEmpty(model.ImageURL))
                {
                    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                }
                //if (model.ListStoreId.Count < 0)
                //{
                //    ModelState.AddModelError("StoreID", " Please choose Store");
                //}
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
                {
                    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                }

                //string ProductTypeText = "";
                //if (model.ListProductType != null && model.ListProductType.Count > 0)
                //{
                //    ProductTypeText = model.ListProductType.Where(w => w.Value.Trim() == model.Type.ToString().Trim()).Select(s => s.Text).FirstOrDefault();
                //    if (ProductTypeText.Trim().ToLower() != Commons.EProductType.Dish.ToString().Trim().ToLower())
                //    {
                //        model.GLAccountCode = null;
                //        model.IsGiftCard = false;
                //        model.IsIncludeNetSale = false;
                //        model.ParentName = "";
                //        model.ParentID = null;
                //    }
                //}

                string cateIdChoose = null;
                if (!string.IsNullOrEmpty(model.ParentID))
                {
                    cateIdChoose = model.ParentID;
                }

                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    //model.ListCategories = GetSelectListParentCategory(model.StoreID, model.Type.ToString(), model.ID);

                    // Updated 09082017
                    // No include list cate child of current cate
                    model.lstCateGroup = GetSelectListParentCategorySortForEditCate(model.StoreID, model.Type.ToString(), model.ID, model.ParentID);
                    //if (!string.IsNullOrEmpty(cateIdChoose))
                    //{
                    //    var cateChoose = model.lstCateGroup.Where(w => w.Id == cateIdChoose).FirstOrDefault();
                    //    if (cateChoose != null)
                    //    {
                    //        cateChoose.Selected = true;
                    //    }
                    //}

                    model.ProductTypeName = model.ListProductType.Where(w => w.Value == model.Type.ToString()).Select(s => s.Text).FirstOrDefault();
                    //model.ListCategories = GetSelectListParentCategory(model.StoreID, model.ProductTypeID);
                    //model = GetDetail(model.ID);
                    //model.GetListProductType();
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
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
                    //model.GetListProductType();

                    // Updated 09082017
                    // No include list cate child of current cate
                    model.lstCateGroup = GetSelectListParentCategorySortForEditCate(model.StoreID, model.Type.ToString(), model.ID, model.ParentID);
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
                //model.GetListProductType();

                // Updated 09082017
                // No include list cate child of current cate
                model.lstCateGroup = GetSelectListParentCategorySortForEditCate(model.StoreID, model.Type.ToString(), model.ID, model.ParentID);
                //if (!string.IsNullOrEmpty(model.ParentID))
                //{
                //    var currentCate = model.lstCateGroup.Where(w => w.Id == model.ParentID).FirstOrDefault();
                //    if (currentCate != null)
                //    {
                //        currentCate.Selected = true;
                //    }
                //}

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            CategoriesModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(CategoriesModels model)
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
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("An error has occurred when deleting"));// "Have an error when you delete a Categories");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        ///*Load Parent Category*/
        public List<SelectListItem> GetSelectListParentCategory(string StoreID, string ProductTypeID, string id = "")
        {
            List<CategoriesModels> lstData = _factory.GetListCategory(StoreID, null, ProductTypeID, CurrentUser.ListOrganizationId);
            List<SelectListItem> slcParentCate = new List<SelectListItem>();
            if (lstData != null)
            {
                var newCate = lstData.Where(x => x.ID == id).FirstOrDefault();
                string parentID = "";
                if (newCate.ListChild != null && newCate.ListChild.Count > 0)
                {
                    parentID = newCate.ListChild[0].ParentID;
                }
                else
                    parentID = newCate.ParentID;
                lstData = lstData.Where(x => x.ID != id && x.ID != parentID).ToList();
                foreach (CategoriesModels item in lstData)
                    slcParentCate.Add(new SelectListItem
                    {
                        Text = item.Name,// + " [" + item.StoreName + "]",
                        Value = item.ID
                    });
            }
            return slcParentCate;
        }

        /*Load Parent Category*/
        public List<SBInventoryBaseCateGroupViewModel> GetSelectListParentCategorySort(string StoreID, string ProductTypeID, string id = "")
        {
            List<SBInventoryBaseCateGroupViewModel> lstData = _factory.GetListCategorySortParent(StoreID, null, ProductTypeID, CurrentUser.ListOrganizationId, id);

            return lstData;
        }

        // No include list cate child of current cate
        public List<SBInventoryBaseCateGroupViewModel> GetSelectListParentCategorySortForEditCate(string StoreID, string ProductTypeID, string id, string parentId)
        {
            List<SBInventoryBaseCateGroupViewModel> lstData = _factory.GetListCategorySortParentForEditCate(StoreID, null, ProductTypeID, CurrentUser.ListOrganizationId, id, parentId);

            return lstData;
        }

        //public ActionResult LoadParentCategory(string StoreID, string ProductTypeID)
        //{
        //    List<CategoriesModels> lstData = _factory.GetListCategory(StoreID, null, ProductTypeID, CurrentUser.ListOrganizationId);
        //    SBInventoryBaseModel model = new SBInventoryBaseModel();
        //    if (lstData != null && lstData.Count > 0)
        //    {
        //        //lstData = lstData.Where(x => string.IsNullOrEmpty(x.ParentName)).ToList();
        //        foreach (CategoriesModels data in lstData)
        //        {
        //            SelectListItem item = new SelectListItem()
        //            {
        //                Value = data.ID,
        //                Text = data.Name // + " [" + data.StoreName + "]";
        //            };
        //            model.ListCategories.Add(item);
        //        }
        //    }
        //    return PartialView("_DDLParentCategory", model);
        //}

        public ActionResult LoadParentCategory(string StoreID, string ProductTypeID)
        {
            List<SBInventoryBaseCateGroupViewModel> lstData = _factory.GetListCategorySortParent(StoreID, null, ProductTypeID, CurrentUser.ListOrganizationId);
            SBInventoryBaseModel model = new SBInventoryBaseModel();
            model.lstCateGroup = lstData;
            return PartialView("_DDLParentCategory", model);
        }
        
    }
}