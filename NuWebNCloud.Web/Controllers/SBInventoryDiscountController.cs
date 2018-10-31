using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SBInventoryDiscountController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DiscountFactory _factory = null;

        public SBInventoryDiscountController()
        {
            _factory = new DiscountFactory();
            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                DiscountViewModels model = new DiscountViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Discount_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(DiscountViewModels model)
        {
            try
            {
                var datas = _factory.GetListDiscount(model.StoreID, null, CurrentUser.ListOrganizationId);
                foreach (var item in datas)
                {
                    item.BType = item.Type == (byte)Commons.EValueType.Currency ? true : false;
                }
                model.ListItem = datas;
            }
            catch (Exception e)
            {
                _logger.Error("Discount_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            DiscountModels model = new DiscountModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(DiscountModels model)
        {
            try
            {
                if (!model.BType)
                {
                    if (model.Value < 0 || model.Value > 100)
                    {
                        ModelState.AddModelError("Value", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Value must between 0 and 100"));
                    }
                }

                string msg = "";
                bool result = _factory.InsertOrUpdateDiscount(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //return RedirectToAction("Create");
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Discount_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public DiscountModels GetDetail(string id)
        {
            try
            {
                DiscountModels model = _factory.GetListDiscount(null, id)[0];
                model.BType = model.Type == (byte)Commons.EValueType.Currency ? true : false;
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Discount_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            DiscountModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            DiscountModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(DiscountModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose Store."));
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Name is required"));

                if (!model.BType)
                {
                    if (model.Value < 0 || model.Value > 100)
                    {
                        ModelState.AddModelError("Value", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Value must between 0 and 100"));
                    }
                }

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }

                //====================
                string msg = "";
                var result = _factory.InsertOrUpdateDiscount(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //return PartialView("_Edit", model);
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Discount_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            DiscountModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(DiscountModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteDiscount(model.ID, ref msg);
                if (!result)
                {
                    //ModelState.AddModelError("Name", "Have a error when you delete an Discount");
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Discount_Delete: " + ex);
                ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Discount"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult Import()
        {
            DiscountModels model = new DiscountModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(DiscountModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store."));
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
                    ModelState.AddModelError("", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(response.MsgError));
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
                    _logger.Error("Employee_Import: " + msg);
                    ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Discount_Import: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            DiscountModels model = new DiscountModels();
            return View(model);
        }


        [HttpPost]
        public ActionResult Export(DiscountModels model)
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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Discount").Replace(" ", "_")));

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
                _logger.Error("Discount_Export: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }
    }
}