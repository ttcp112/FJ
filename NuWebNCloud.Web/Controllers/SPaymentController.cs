using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings;
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
    public class SPaymentController : HQController
    {
        private PaymentMethodFactory _factory = null;
        private XeroFactory _facXero = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SPaymentController()
        {
            _factory = new PaymentMethodFactory();
            _facXero = new XeroFactory();
            ViewBag.ListStore = GetListStore();
            var ListAcc = new List<XeroDTO>();
            ViewBag.ListAccountXero = ListAcc;
           // ViewBag.ListAccountXero = new SelectList(ListAcc, "Code", "NameDisplayCombobox", "ReportingCodeName", 1);
        }

        public ActionResult Index()
        {
            try
            {
                PaymentMethodViewModels model = new PaymentMethodViewModels();
                model.StoreID = CurrentUser.StoreId;
                ///GetListStoreGroup();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("PaymentMethod_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(PaymentMethodViewModels model)
        {
            try
            {
                var data = _factory.GetListPaymentMethod(model.StoreID, null, CurrentUser.ListOrganizationId);
                model.ListItem = data;
            }
            catch (Exception ex)
            {
                _logger.Error("PaymentMethod_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

            return PartialView("_ListData", model);
        }

        public PaymentMethodModels GetDetail(string id, string StoreID)
        {
            try
            {
                PaymentMethodModels model = _factory.GetListPaymentMethod(StoreID, id)[0];
                if (model.ListChild != null)
                {
                    for (int i = 0; i < model.ListChild.Count; i++)
                    {
                        model.ListChild[i].OffSet = i;
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("PaymentMethod_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id, string StoreID)
        {
            PaymentMethodModels model = GetDetail(id, StoreID);
            return PartialView("_View", model);
        }

        public ActionResult Create()
        {
            PaymentMethodModels model = new PaymentMethodModels();
            return PartialView("Create", model);
            //return View("Create", model);
        }

        [HttpPost]
        public ActionResult Create(PaymentMethodModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                {
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store"));

                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Payment method name is required"));

                }
                if (model.ListChild != null)
                {
                    model.ListChild = model.ListChild.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                    model.ListChild.ForEach(x =>
                    {
                        if (string.IsNullOrEmpty(x.Name))
                        {
                            ModelState.AddModelError("ListChild[" + x.OffSet + "].Name", CurrentUser.GetLanguageTextFromKey("Sub payment method name is required"));
                        }
                        if (x.PictureUpload != null && x.PictureUpload.ContentLength > 0)
                        {
                            Byte[] imgByte = new Byte[x.PictureUpload.ContentLength];
                            x.PictureUpload.InputStream.Read(imgByte, 0, x.PictureUpload.ContentLength);
                            x.PictureByte = imgByte;
                            x.ImageURL = Guid.NewGuid() + Path.GetExtension(x.PictureUpload.FileName);
                            x.PictureUpload = null;
                            x.photoByte = imgByte;
                        }
                    });
                }
                if (!ModelState.IsValid)
                {
                    if (model.ListChild != null)
                    {
                        model.ListChild.ForEach(x =>
                        {
                            if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                                x.ImageURL = "";
                        });
                    }
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    if (string.IsNullOrEmpty(model.Name))
                        model.Name = "";
                    return PartialView("Create", model);
                    //return View("Create", model);
                }
                //==========
                string msg = "";
                bool result = _factory.InsertOrUpdatePaymentMethod(model, ref msg);
                if (result)
                {
                    if (model.ListChild != null)
                    {
                        byte[] photoByte = null;
                        //Save Image on Server
                        model.ListChild.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x.ImageURL) && x.PictureByte != null)
                            {
                                var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                                var path = string.Format("{0}{1}", originalDirectory, x.ImageURL);
                                MemoryStream ms = new MemoryStream(x.photoByte, 0, x.photoByte.Length);
                                ms.Write(x.photoByte, 0, x.photoByte.Length);
                                System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);
                                photoByte = x.photoByte;
                                ImageHelper.Me.SaveCroppedImage(imageTmp, path, x.ImageURL, ref photoByte);
                                x.PictureByte = x.photoByte;
                                FTP.Upload(x.ImageURL, x.PictureByte);

                                ImageHelper.Me.TryDeleteImageUpdated(path);
                            }
                        });
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    //return RedirectToAction("Create");                   
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey(msg));
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("Create", model);
                    //return View("Create", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Table_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public PartialViewResult Edit(string id, string StoreID)
        {
            PaymentMethodModels model = GetDetail(id, StoreID);
            if (!string.IsNullOrEmpty(model.StoreID))
            {
                var InfoStore = CurrentUser.listStore.Where(o => o.ID.Equals(model.StoreID)).FirstOrDefault();
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
                                data = data.Select(o => new XeroDTO
                                {
                                    AccountID = o.AccountID,
                                    Name = o.Name,
                                    Code = o.Code,
                                    ReportingCodeName = o.ReportingCodeName,
                                    NameDisplayCombobox = o.Code + "-" + o.Name,
                                }).OrderBy(o => o.ReportingCodeName).ToList();
                                //if ((model.Name.ToLower().Equals("cash") || model.Code == (byte)Commons.EPaymentCode.Cash || model.Name.ToLower().Equals("gift card") || model.Code == (byte)Commons.EPaymentCode.GiftCard))
                                //{
                                //}
                                //else
                                //{
                                //    data.ForEach(o =>
                                //    {
                                //        o.ReportingCodeName = !string.IsNullOrEmpty(o.ReportingCodeName) ? (o.ReportingCodeName.Length < 11 ? o.ReportingCodeName : o.ReportingCodeName.Substring(0, 11) + "...") : "";
                                //        o.NameDisplayCombobox = !string.IsNullOrEmpty(o.NameDisplayCombobox) ? (o.NameDisplayCombobox.Length < 11 ? o.NameDisplayCombobox : o.NameDisplayCombobox.Substring(0, 11) + "...") : "";
                                //    });
                                //}

                                
                                data = data.OrderBy(o => o.ReportingCodeName).ToList();
                                ViewBag.ListAccountXero = data;
                                //ViewBag.ListAccountXero = new SelectList(data, "Code", "NameDisplayCombobox", "ReportingCodeName", 1);

                            }
                        }
                    }
                }
            }
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(PaymentMethodModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Payment method name is required"));
                }
                if (model.ListChild != null && model.ListChild.Count > 0)
                {
                    model.ListChild.ForEach(x =>
                {
                    if (string.IsNullOrEmpty(x.Name))
                    {
                        ModelState.AddModelError("ListChild[" + x.OffSet + "].Name", CurrentUser.GetLanguageTextFromKey("Sub payment method name is required"));
                    }
                });
                
                    model.ListChild = model.ListChild.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                    foreach (var item in model.ListChild)
                    {
                        if (!string.IsNullOrEmpty(item.ImageURL))
                        {
                            item.ImageURL = item.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                        }
                        if (item.PictureUpload != null && item.PictureUpload.ContentLength > 0)
                        {
                            Byte[] imgByte = new Byte[item.PictureUpload.ContentLength];
                            item.PictureUpload.InputStream.Read(imgByte, 0, item.PictureUpload.ContentLength);
                            item.PictureByte = imgByte;
                            item.ImageURL = Guid.NewGuid() + Path.GetExtension(item.PictureUpload.FileName);
                            item.PictureUpload = null;
                            item.photoByte = imgByte;
                        }
                    }
                }
                if (!ModelState.IsValid)
                {
                    if (model.ListChild != null)
                    {
                        model.ListChild.ForEach(x =>
                        {
                            if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                                x.ImageURL = "";
                        });
                    }
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    if (string.IsNullOrEmpty(model.Name))
                        model.Name = "";
                    return PartialView("_Edit", model);
                }

                //====================
                string msg = "";
                var result = _factory.InsertOrUpdatePaymentMethod(model, ref msg);
                if (result)
                {
                    if (model.ListChild != null)
                    {
                        byte[] photoByte = null;
                        //Save Image on Server
                        model.ListChild.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x.ImageURL) && x.PictureByte != null)
                            {
                                var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                                var path = string.Format("{0}{1}", originalDirectory, x.ImageURL);
                                MemoryStream ms = new MemoryStream(x.photoByte, 0, x.photoByte.Length);
                                ms.Write(x.photoByte, 0, x.photoByte.Length);
                                System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);
                                photoByte = x.photoByte;
                                ImageHelper.Me.SaveCroppedImage(imageTmp, path, x.ImageURL, ref photoByte);
                                x.PictureByte = x.photoByte;
                                FTP.Upload(x.ImageURL, x.PictureByte);

                                ImageHelper.Me.TryDeleteImageUpdated(path);
                            }
                        });
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
                _logger.Error("PaymentMethod_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id, string StoreID)
        {
            PaymentMethodModels model = GetDetail(id, StoreID);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(PaymentMethodModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeletePaymentMethod(model.ID, model.StoreID, ref msg);
                if (!result)
                {
                    //ModelState.AddModelError("Name", "Have a error when you delete a Category");
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("PaymentMethod_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Payment Method"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        //public ActionResult AddSubPayment(int currentOffset)
        public ActionResult AddSubPayment(PaymentMethodModels PaymentMethodDTO)
        {
            PaymentMethodModels group = new PaymentMethodModels();
            group.OffSet = PaymentMethodDTO.OffSet;
            group.Name = PaymentMethodDTO.Name;
            group.NumberOfCopies = PaymentMethodDTO.NumberOfCopies;
            group.FixedAmount = PaymentMethodDTO.FixedAmount;
            group.GLAccountCode = PaymentMethodDTO.GLAccountCode;
            group.Sequence = PaymentMethodDTO.Sequence;
            group.IsGiveBackChange = PaymentMethodDTO.IsGiveBackChange;
            group.IsAllowKickDrawer = PaymentMethodDTO.IsAllowKickDrawer;
            group.IsIncludeOnSale = PaymentMethodDTO.IsIncludeOnSale;
            group.IsShowOnPos = PaymentMethodDTO.IsShowOnPos;
            if(!string.IsNullOrEmpty(PaymentMethodDTO.StoreID))
            {
                var InfoStore = CurrentUser.listStore.Where(o => o.ID.Equals(PaymentMethodDTO.StoreID)).FirstOrDefault();
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
                                data = data.Select(o => new XeroDTO
                                {
                                    AccountID = o.AccountID,
                                    Name = o.Name,
                                    Code = o.Code,
                                    ReportingCodeName = o.ReportingCodeName,
                                    NameDisplayCombobox = o.Code + "-" + o.Name,
                                }).OrderBy(o => o.ReportingCodeName).ToList();

                                //data.ForEach(o =>
                                //{
                                //    o.ReportingCodeName = !string.IsNullOrEmpty(o.ReportingCodeName) ? (o.ReportingCodeName.Length < 11 ? o.ReportingCodeName : o.ReportingCodeName.Substring(0, 11) + "...") : "";
                                //    o.NameDisplayCombobox = !string.IsNullOrEmpty(o.NameDisplayCombobox) ? (o.NameDisplayCombobox.Length < 11 ? o.NameDisplayCombobox : o.NameDisplayCombobox.Substring(0, 11) + "...") : "";
                                //});
                                data = data.OrderBy(o => o.ReportingCodeName).ToList();
                                ViewBag.ListAccountXero = data;
                                //ViewBag.ListAccountXero = new SelectList(data, "Code", "NameDisplayCombobox", "ReportingCodeName", 1);
                            }
                        }
                    }
                }
            }
            return PartialView("_SubPayment", group);
        }

        public ActionResult Import()
        {
            PaymentMethodModels model = new PaymentMethodModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(PaymentMethodModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                    return View(model);
                }
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ExcelUpload", CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    return View(model);
                }

                ImportModel importModel = new ImportModel();
                string msg = "";
                StatusResponse response = _factory.Import(model.ExcelUpload, model.ListStores, ref importModel, ref msg);
                if (!response.Status)
                {
                    ModelState.AddModelError("", response.MsgError);
                    return View(model);
                }

                // Delete File Excel and File Zip Image
                CommonHelper.DeleteFileFromServer(CommonHelper.GetFilePath(model.ExcelUpload));

                //if (!ModelState.IsValid)
                //    return View(model);

                if (msg.Equals(""))
                {
                    return View("ImportDetail", importModel);
                }
                else
                {
                    _logger.Error("PaymentMethod_Import: " + msg);
                    ModelState.AddModelError("ExcelUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("PaymentMethod_Import: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ExcelUpload", CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            PaymentMethodModels model = new PaymentMethodModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(PaymentMethodModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Sheet1");
                StatusResponse response = _factory.Export(ref ws, model.ListStores);
                if (!response.Status)
                {
                    ModelState.AddModelError("", response.MsgError);
                    return View(model);
                }

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("PaymentMethod").Replace(" ", "_")));

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
                _logger.Error("PaymentMethod_Export: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public JsonResult GetAccountXeroByStore(string StoreId)
        {
            try
            {
                NSLog.Logger.Info("GetAccountXeroByStore_Request : ", StoreId);
                var InfoStore = CurrentUser.listStore.Where(o => o.ID.Equals(StoreId)).FirstOrDefault();
                if (InfoStore != null)
                {
                    var InfoXero = InfoStore.ThirdParty;
                    if (InfoXero != null)
                    {
                        if (!string.IsNullOrEmpty(InfoXero.ApiURL))
                        {
                            var data = _facXero.GetListXeroSetting(InfoXero.IPAddress, InfoXero.ThirdPartyID, InfoXero.ApiURL,null);
                            if (data != null)
                            {
                                var LstCombobox = new List<AccountComboboxModel>();
                                var Parrents = data.GroupBy(o => o.ReportingCodeName).ToList();
                                if (Parrents != null && Parrents.Any())
                                {
                                    Parrents.ForEach(o =>
                                    {
                                        var _children = data.Where(x => x.ReportingCodeName.Equals(o.Key)).Select(y => new AccountChildrenModel
                                        {
                                            id = y.Code,
                                            text = y.Code + "-" + y.Name
                                            //  text =!string.IsNullOrEmpty(y.Code + "-"+y.Name) ? ((y.Code + "-" + y.Name).Length < 11 ? (y.Code + "-" + y.Name) : (y.Code + "-" + y.Name).Substring(0,11) + "..."): "",
                                        }).ToList();
                                        LstCombobox.Add(new AccountComboboxModel
                                        {
                                            text = o.Key,
                                           // text =!string.IsNullOrEmpty(o.Key) ? (o.Key.Length < 11 ? o.Key : o.Key.Substring(0,11) + "..."): "",
                                            children = _children
                                        });
                                    });
                                }
                                LstCombobox = LstCombobox.OrderBy(o => o.text).ToList();

                                //var dataJson = data.Select(o => new 
                                //{
                                //    id = o.AccountID,
                                //    text = o.Name
                                //}).OrderBy(o => o.text).ToList();
                                return Json(LstCombobox, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
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