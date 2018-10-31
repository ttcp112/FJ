using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Settings.ScreenSaverMode;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SScreenSaverModeController : HQController
    {
        private ScreenSaverModeFactory _factory;
        public SScreenSaverModeController()
        {
            _factory = new ScreenSaverModeFactory();
            ViewBag.ListStore = GetListStore();
        }
        // GET: SScreenSaverMode
        public ActionResult Index()
        {
            ScreenSaverModeViewModels model = new ScreenSaverModeViewModels();
            model.StoreID = CurrentUser.StoreId;
            return View(model);
        }

        public PartialViewResult Edit(string StoreID)
        {
            var model = new ScreenSaverModeModels();
            model.ListProduct = GetDetail(null, StoreID);
            var _OffSet = 0;
            if (model.ListProduct != null && model.ListProduct.Any())
            {
                model.ListProduct.ForEach(x =>
                {
                    x.OffSet = _OffSet;
                    _OffSet = _OffSet + 1;
                    x.ImageName = " ";
                    x.StoreID = StoreID;
                });
            }
            model.PictureUpload = null;
            model.StoreID = StoreID;
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(ScreenSaverModeModels model)
        {
            try
            {
                Dictionary<int, byte[]> lstImgByte = new Dictionary<int, byte[]>();
                var data = new List<ScreenSaverModeModels>();
                byte[] photoByte = null;
                if (model.PictureUpload.Length > 0 && model.PictureUpload.Any() && model.PictureUpload[0] != null)
                {
                    model.PictureUpload = model.PictureUpload.Where(ww => ww != null).ToArray();
                    foreach (HttpPostedFileBase File in model.PictureUpload)
                    {
                        if (model.ListProduct != null && model.ListProduct.Any())
                        {
                            var _temp = model.ListProduct.Where(x => x.ImageName.Equals(File.FileName) && !x.IsDelete).FirstOrDefault();
                            if (_temp != null)
                            {
                                if (File != null && File.ContentLength > 0)
                                {
                                    Byte[] imgByte = new Byte[File.ContentLength];
                                    File.InputStream.Read(imgByte, 0, File.ContentLength);
                                    _temp.PictureByte = imgByte;
                                    _temp.ImageURL = Guid.NewGuid() + Path.GetExtension(File.FileName);
                                    _temp.PictureUpload = null;
                                    lstImgByte.Add(_temp.OffSet, imgByte);
                                }
                                data.Add(new ScreenSaverModeModels
                                {
                                    ImageURL = _temp.ImageURL,
                                    IsActive = _temp.IsActive,
                                    PictureByte = _temp.PictureByte,
                                    OffSet = _temp.OffSet
                                });
                            }
                        }
                    }
                }

                if (model.ListProduct != null && model.ListProduct.Any())
                {
                    model.ListProduct.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x.ImageURL) && x.PictureByte == null)
                        {
                            x.ImageURL = x.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                            data.Add(new ScreenSaverModeModels
                            {
                                ImageURL = x.ImageURL,
                                IsActive = x.IsActive,
                                PictureByte = x.PictureByte,
                                OffSet = x.OffSet,
                                ID = x.ID
                            });
                        }
                    });
                }

                string msg = "";
                var dataapi = data.Select(x => new ScreenSaverModeApiModel
                {
                    ID = x.ID,
                    ImageURL = x.ImageURL,
                    IsActive = x.IsActive,
                    StoreID = x.StoreID
                }).ToList();
                var result = _factory.InsertOrUpdate(dataapi, model.StoreID, ref msg);
                if (result)
                {
                    //Save Image on Server
                    foreach (var item in data)
                    {
                        if (!string.IsNullOrEmpty(item.ImageURL) && item.PictureByte != null)
                        {
                            var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                            var path = string.Format("{0}{1}", originalDirectory, item.ImageURL);
                            MemoryStream ms = new MemoryStream(lstImgByte[item.OffSet], 0, lstImgByte[item.OffSet].Length);
                            ms.Write(lstImgByte[item.OffSet], 0, lstImgByte[item.OffSet].Length);
                            System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                            ImageHelper.Me.SaveCroppedImage(imageTmp, path, item.ImageURL, ref photoByte);
                            model.PictureByte = photoByte;

                            FTP.Upload(item.ImageURL, item.PictureByte);
                            ImageHelper.Me.TryDeleteImageUpdated(path);
                        }
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("Create Screen Saver Mode", e);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
        }

        public ActionResult Search()
        {
            var model = new ScreenSaverModeViewModels();
            try
            {
                var listStore = ViewBag.ListStore;
                foreach (var item in listStore)
                {
                    model.ListItem.Add(new StoreModels
                    {
                        Name = item.Text,
                        Id = item.Value
                    });
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetListScreenSaverMode_error", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            ScreenSaverModeModels model = new ScreenSaverModeModels();
            return View(model);
        }

        public List<ScreenSaverModeModels> GetDetail(string id = null, string StoreID = null)
        {
            try
            {
                var model = _factory.GetListScreenSaverMode(StoreID, id);
                return model;
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetDetail_Screen_Saver_Mode_Error", ex);
            }
            return null;
        }

        [HttpPost]
        public ActionResult Create(ScreenSaverModeModels model)
        {
            try
            {
                Dictionary<int, byte[]> lstImgByte = new Dictionary<int, byte[]>();
                var data = new List<ScreenSaverModeModels>();
                byte[] photoByte = null;
                if (model.PictureUpload.Length > 0)
                {
                    foreach (HttpPostedFileBase File in model.PictureUpload)
                    {
                        if (model.ListProduct != null && model.ListProduct.Any())
                        {
                            var _temp = model.ListProduct.Where(x => x.ImageName.Equals(File.FileName)).FirstOrDefault();
                            if (_temp != null)
                            {
                                if (File != null && File.ContentLength > 0)
                                {
                                    Byte[] imgByte = new Byte[File.ContentLength];
                                    File.InputStream.Read(imgByte, 0, File.ContentLength);
                                    _temp.PictureByte = imgByte;
                                    _temp.ImageURL = Guid.NewGuid() + Path.GetExtension(File.FileName);
                                    _temp.PictureUpload = null;
                                    lstImgByte.Add(_temp.OffSet, imgByte);
                                }
                                data.Add(new ScreenSaverModeModels
                                {
                                    ImageURL = _temp.ImageURL,
                                    IsActive = _temp.IsActive,
                                    PictureByte = _temp.PictureByte,
                                    OffSet = _temp.OffSet
                                });
                            }
                        }
                    }
                }

                if (model.ListProduct != null && model.ListProduct.Any())
                {
                    model.ListProduct.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x.ImageURL) && x.PictureByte == null)
                        {
                            x.ImageURL = x.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                            data.Add(new ScreenSaverModeModels
                            {
                                ImageURL = x.ImageURL,
                                IsActive = x.IsActive,
                                PictureByte = x.PictureByte,
                                OffSet = x.OffSet
                            });
                        }
                    });
                }

                string msg = "";
                var dataapi = data.Select(x => new ScreenSaverModeApiModel
                {
                    ID = x.ID,
                    ImageURL = x.ImageURL,
                    IsActive = x.IsActive,
                    StoreID = x.StoreID
                }).ToList();
                var result = _factory.InsertOrUpdate(dataapi, model.StoreID, ref msg);
                if (result)
                {
                    //Save Image on Server
                    foreach (var item in data)
                    {
                        if (!string.IsNullOrEmpty(item.ImageURL) && item.PictureByte != null)
                        {
                            var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                            var path = string.Format("{0}{1}", originalDirectory, item.ImageURL);
                            MemoryStream ms = new MemoryStream(lstImgByte[item.OffSet], 0, lstImgByte[item.OffSet].Length);
                            ms.Write(lstImgByte[item.OffSet], 0, lstImgByte[item.OffSet].Length);
                            System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                            ImageHelper.Me.SaveCroppedImage(imageTmp, path, item.ImageURL, ref photoByte);
                            model.PictureByte = photoByte;

                            FTP.Upload(item.ImageURL, item.PictureByte);
                            ImageHelper.Me.TryDeleteImageUpdated(path);
                        }
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("Create Screen Saver Mode", e);
                return View(model);
            }
        }

        [HttpPost]
        public PartialViewResult AddImageItem(int OffSet, int Length)
        {

            List<ScreenSaverModeModels> model = new List<ScreenSaverModeModels>();
            var _OffSet = OffSet;
            for (int i = 0; i < Length; i++)
            {
                model.Add(new ScreenSaverModeModels
                {
                    OffSet = _OffSet,
                    IsActive = true,
                });
                _OffSet = _OffSet + 1;
            }
            return PartialView("_ListItem", model);
        }

        [HttpGet]
        public PartialViewResult Delete(string id, string StoreID)
        {
            ScreenSaverModeModels model = GetDetail(id, StoreID).FirstOrDefault();
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult DeleteImage(string Id, string StoreID)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteScreenSaverMode(Id, StoreID, ref msg);
                if (!result)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return new HttpStatusCodeResult(400, "Have an error when you delete a Payment Method");
                }
                //return new HttpStatusCodeResult(HttpStatusCode.OK);
                var model = new ScreenSaverModeModels();
                model.ListProduct = GetDetail(null, StoreID);
                var _OffSet = 0;
                if (model.ListProduct != null && model.ListProduct.Any())
                {
                    model.ListProduct.ForEach(x =>
                    {
                        x.OffSet = _OffSet;
                        _OffSet = _OffSet + 1;
                        x.ImageName = " ";
                        x.StoreID = StoreID;
                    });
                }
                return PartialView("_ListItem", model.ListProduct);
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("DeleteScreenSaverMode_Error: ", ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}