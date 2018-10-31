using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Models.Settings.Zone;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class STableController : HQController
    {
        private TableFactory _factory = null;
        private ZoneFactory _Zonefactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public STableController()
        {
            _factory = new TableFactory();
            _Zonefactory = new ZoneFactory();

            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                TableViewModels model = new TableViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Table_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(TableViewModels model)
        {
            try
            {
                var datas = _factory.GetListTable(model.StoreID,null, CurrentUser.ListOrganizationId);
                datas = datas.Where(x => x.StoreID == model.StoreID).ToList();
              
                model.ListItem = datas;
            }
            catch (Exception ex)
            {
                _logger.Error("Table_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

            return PartialView("_ListData", model);
        }

        public TableModels GetDetail(string id)
        {
            try
            {
                TableModels model = new TableModels();
                var listTable = _factory.GetListTable(null, id);
                if (listTable != null && listTable.Any())
                {
                    model = listTable.FirstOrDefault();
                    model.ListZone = GetSelectListZone(model.StoreID);
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Tables_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            TableModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public ActionResult Create()
        {
            TableModels model = new TableModels();
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(TableModels model)
        {
            try
            {
                if (model.Cover < 1)
                {
                    ModelState.AddModelError("Cover", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 1"));
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name field is required"));
                }

                if (!ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(model.StoreID))
                    {
                        model.ListZone = GetSelectListZone(model.StoreID);

                    }
                    return View(model);
                }

                string msg = "";
                bool result = _factory.InsertOrUpdateTables(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //return RedirectToAction("Create");
                    ModelState.AddModelError("Name", msg);
                    if (!string.IsNullOrEmpty(model.StoreID))
                    {
                        model.ListZone = GetSelectListZone(model.StoreID);

                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Table_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            TableModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(TableModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteTables(model.ID, ref msg);
                if (!result)
                {
                    //ModelState.AddModelError("Name", "Have a error when you delete an Category");
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Tables_Delete: " + ex);
                ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Table"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public PartialViewResult Edit(string id)
        {
            TableModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(TableModels model)
        {
            try
            {
                if(model.Cover < 1)
                {
                    ModelState.AddModelError("Cover", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 1"));
                }

                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Table Name is required"));

                if (!ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(model.StoreID))
                    {
                        model.ListZone = GetSelectListZone(model.StoreID);

                    }
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //====================
                string msg = "";
                var result = _factory.InsertOrUpdateTables(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //return PartialView("_Edit", model);
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    if (!string.IsNullOrEmpty(model.StoreID))
                    {
                        model.ListZone = GetSelectListZone(model.StoreID);

                    }
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Table_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        /*Printer*/
        public List<SelectListItem> GetSelectListZone(string StoreID)
        {
            List<ZoneModels> lstData = _Zonefactory.GetListZone(StoreID,null, CurrentUser.ListOrganizationId);
            List<SelectListItem> slcZone = new List<SelectListItem>();
            if (lstData != null)
            {
                foreach (ZoneModels item in lstData)
                    slcZone.Add(new SelectListItem
                    {
                        Text = item.Name + " [" + item.StoreName + "]",
                        Value = item.ID
                    });
            }
            return slcZone;
        }

        public ActionResult LoadZone(string StoreID)
        {
            List<ZoneModels> lstData = _Zonefactory.GetListZone(StoreID, null, CurrentUser.ListOrganizationId);
            TableModels model = new TableModels();
            if (lstData != null && lstData.Count > 0)
                foreach (ZoneModels data in lstData)
                    model.ListZone.Add(new SelectListItem
                    {
                        Value = data.ID,
                        Text = data.Name + " [" + data.StoreName + "]"
                    });
            return PartialView("_DropDownListZone", model);
        }


        public ActionResult Import()
        {
            TableModels model = new TableModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(TableModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store"));
                    return View(model);
                }
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
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
                    _logger.Error("Table_Import: " + msg);
                    ModelState.AddModelError("ImageZipUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Table_Import : " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error"));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            TableModels model = new TableModels();
            return View(model);
        }


        [HttpPost]
        public ActionResult Export(TableModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store."));
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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Table List").Replace(" ", "_")));

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
                _logger.Error("Table_Export: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error"));
                return View(model);
            }
        }

    }
}