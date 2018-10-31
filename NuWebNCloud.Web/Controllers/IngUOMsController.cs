using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using System.IO;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngUOMsController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private UnitOfMeasureFactory _factory = null;

        public IngUOMsController()
        {
            _factory = new UnitOfMeasureFactory();
            //================
            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                UnitOfMeasureViewModel model = new UnitOfMeasureViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("UOMIndex: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(UnitOfMeasureViewModel model)
        {
            try
            {
                model.ListItem = _factory.GetData(CurrentUser.ListOrganizationId);
            }
            catch (Exception e)
            {
                _logger.Error("UOMSearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            UnitOfMeasureModel model = new UnitOfMeasureModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(UnitOfMeasureModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                string msg = "";
                model.CreatedBy = CurrentUser.UserName;
                model.UpdatedBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;
                model.OrganizationId = CurrentUser.ListOrganizationId[0];

                bool result = _factory.Insert(model, ref msg);
                string msg1 = msg;
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Code", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("UOMCreate: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public UnitOfMeasureModel GetDetail(string id)
        {
            try
            {
                UnitOfMeasureModel model = _factory.GetDetail(id);
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("UOMDetail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            UnitOfMeasureModel model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            UnitOfMeasureModel model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(UnitOfMeasureModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model = GetDetail(model.Id);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                model.UpdatedDate = DateTime.Now;
                model.UpdatedBy = CurrentUser.UserName;
                model.OrganizationId = CurrentUser.ListOrganizationId[0];
                //====================
                string msg = "";
                var result = _factory.Update(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Code", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("UOMEdit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            UnitOfMeasureModel model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(UnitOfMeasureModel model)
        {
            try
            {
                string msg = "";
                var result = _factory.Delete(model.Id, ref msg);
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
                _logger.Error("UOMDelete: " + ex);
                ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have an error when you delete an UOM") );
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult Import()
        {
            UnitOfMeasureModel model = new UnitOfMeasureModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(UnitOfMeasureModel model)
        {
            //try
            //{
            //    //if (model.ListStores == null)
            //    //{
            //    //    ModelState.AddModelError("ListStores", "Please choose store.");
            //    //    return View(model);
            //    //}
            //    if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
            //    {
            //        ModelState.AddModelError("ExcelUpload", "File excel cannot be null");
            //        return View(model);
            //    }

            //    ImportModel importModel = new ImportModel();
            //    string msg = "";
            //    string msg1 = "";

            //    StatusResponse response = _factory.Import(model.ExcelUpload, ref importModel, ref msg, ref msg1, CurrentUser);

            //    if (!response.Status)
            //    {
            //        ModelState.AddModelError("", response.MsgError);
            //        return View(model);
            //    }
            //    // Delete File Excel and File Zip Image
            //    CommonHelper.DeleteFileFromServer(CommonHelper.GetFilePath(model.ExcelUpload));
            //    //if (!ModelState.IsValid)
            //    //    return View(model);
            //    if (msg.Equals(""))
            //    {
            //        return View("ImportDetail", importModel);
            //    }
            //    else
            //    {
            //        _logger.Error("UOM_Import: " + msg);
            //        ModelState.AddModelError("ExcelUpload", msg);
            //        return View(model);
            //    }
            //}
            //catch (Exception e)
            //{
            //    _logger.Error("UOMImport: " + e);
            //    //return new HttpStatusCodeResult(400, e.Message);
            //    ModelState.AddModelError("ExcelUpload", "Import file have error.");
            //    return View(model);
            //}

            try
            {
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    return View(model);
                }

                ImportModel importModel = new ImportModel();
                string msg = "";
                if (model.ExcelUpload != null && model.ExcelUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ExcelUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);
                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ExcelUpload.SaveAs(filePath);

                    StatusResponse response = _factory.Import(filePath, ref importModel, ref msg, CurrentUser);
                    if (!response.Status)
                    {
                        ModelState.AddModelError("", response.MsgError);
                        return View(model);
                    }
                }
                if (msg.Equals(""))
                {
                    return View("ImportDetail", importModel);
                }
                else
                {
                    _logger.Error("UOMImport: " + msg);
                    ModelState.AddModelError("ExcelUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("UOM: " + e);
                ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(e.Message) );
                return View(model);
            }
        }

        public ActionResult Export()
        {
            UnitOfMeasureModel model = new UnitOfMeasureModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(UnitOfMeasureModel model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store")+"." );
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Sheet1");

                StatusResponse response = _factory.Export(ref ws, model.ListStores, CurrentUser.ListOrganizationId);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("UOM").Replace(" ", "_")));

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
                _logger.Error("UOMExport: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Export file have error") + ".");
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EnableActive(List<string> lstId, bool active)
        {
            var data = _factory.EnableActive(lstId, active);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}