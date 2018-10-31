using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Sandbox;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox;
using NuWebNCloud.Shared.Models.Sandbox.Import;
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
    public class SBCustomerController : HQController
    {
        // GET: SBCustomer
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private CustomerFactory _factory = null;
        List<string> listPropertyReject = null;

        public SBCustomerController()
        {
            _factory = new CustomerFactory();
            //================
            ViewBag.ListStore = GetListStore();
            listPropertyReject = new List<string>();
            listPropertyReject.Add("Marital");
        }

        public ActionResult Index()
        {
            try
            {
                CustomerViewModels model = new CustomerViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Customer_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(CustomerViewModels model)
        {
            try
            {
                var datas = _factory.GetListCustomer(model.StoreID, null, CurrentUser.ListOrganizationId);
                foreach (var item in datas)
                {
                    item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
                    if (!string.IsNullOrEmpty(item.IC) && item.IC.Length > 8)
                        item.IC = "****" + item.IC.Substring(item.IC.Length - 4, 4);
                    else
                        item.IC = "********";
                    if (!string.IsNullOrEmpty(item.Phone) && item.Phone.Length > 8)
                        item.Phone = "****" + item.Phone.Substring(item.Phone.Length - 4, 4);
                    else
                        item.Phone = "********";
                    if (!string.IsNullOrEmpty(item.Email) && item.Email.Length > 8)
                        item.Email = item.Email.Substring(0, 4) + "****";
                    else
                        item.Email = "********";
                    if (string.IsNullOrEmpty(item.Name))
                    {
                        item.Name = item.Email;
                    }
                }
                model.ListItem = datas;
            }
            catch (Exception e)
            {
                _logger.Error("Customer_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            CustomerModels model = new CustomerModels();
            return View(model);
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
        public ActionResult Create(CustomerModels model)
        {
            try
            {
                byte[] photoByte = null;

                if (string.IsNullOrEmpty(model.StoreID))
                {
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name field is required"));
                }
                if (!model.PrivacyPolicy)
                {
                    ModelState.AddModelError("PrivacyPolicy", CurrentUser.GetLanguageTextFromKey("Please confirm before save"));
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
                PropertyReject();
                List<MembershipDTO> listStore = new List<MembershipDTO>();
                listStore.Add(new MembershipDTO
                {
                    IsMembership = model.IsMembership,
                    StoreID = model.StoreID
                });
                model.ListStore = listStore;
                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View(model);
                }
                //====================
                string msg = "";
                bool result = _factory.InsertOrUpdateCustomer(model, ref msg);
                if (result)
                {
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
                    ModelState.AddModelError("Email", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Customer_Create: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);
                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey("Have a error"));
                return View(model);
            }
        }

        public CustomerModels GetDetail(string id, string StoreId)
        {
            try
            {
                CustomerModels model = new CustomerModels();
                var models = _factory.GetListCustomer(null, id);
                if (models != null && models.Any())
                {
                    model = models.FirstOrDefault();
                    model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                    model.StoreID = model.ListStore[0].StoreID;
                    model.JoinedDate = model.JoinedDate.ToLocalTime();
                    model.BirthDate = model.BirthDate.ToLocalTime();
                    model.Anniversary = model.Anniversary.ToLocalTime();
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Customer_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id, string StoreId)
        {
            CustomerModels model = GetDetail(id, StoreId);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id, string StoreId)
        {
            CustomerModels model = GetDetail(id, StoreId);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(CustomerModels model)
        {
            try
            {
                byte[] photoByte = null;

                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose Store."));

                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name is required"));
                PropertyReject();

                if (!model.PrivacyPolicy)
                {
                    ModelState.AddModelError("PrivacyPolicy", CurrentUser.GetLanguageTextFromKey("Please confirm before save"));
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
                List<MembershipDTO> listStore = new List<MembershipDTO>();
                listStore.Add(new MembershipDTO
                {
                    IsMembership = model.IsMembership,
                    StoreID = model.StoreID
                });
                model.ListStore = listStore;

                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }

                //====================
                string msg = "";
                var result = _factory.InsertOrUpdateCustomer(model, ref msg);
                if (result)
                {
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
                    //return PartialView("_Edit", model);
                    ModelState.AddModelError("name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Customer_Edit: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);
                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey("Have a error"));
                model = GetDetail(model.ID, model.StoreID);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id, string StoreId)
        {
            CustomerModels model = GetDetail(id, StoreId);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(CustomerModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteCustomer(model.ID, model.StoreID, ref msg);
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
                _logger.Error("Customer_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("An error has occurred when deleting customer"));
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
                    //return View(model);
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
                        //return View(model);
                    }
                }
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    //return View(model);
                }

                if (!ModelState.IsValid)
                {
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

                ImportModel importModel = new ImportModel();
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

                    importModel = _factory.Import(filePath, listFiles, model.ListStores, ref msg);
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
                    _logger.Error("Customer_Import: " + msg);
                    ModelState.AddModelError("ImageZipUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Customer_Import: " + e);
                ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            CustomerModels model = new CustomerModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(CustomerModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsSetMenu = wb.Worksheets.Add("Sheet1");
                StatusResponse response = _factory.Export(ref wsSetMenu, model.ListStores);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Customer").Replace(" ", "_")));

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
                _logger.Error("Customer_Export: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }
    }
}