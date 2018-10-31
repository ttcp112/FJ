using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings.DefaultCurency;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Linq;
namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SDefaultCurrencyController : HQController
    {
        private DefaultCurrencyFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SDefaultCurrencyController()
        {
            _factory = new DefaultCurrencyFactory();
            ViewBag.ListStore = GetListStore();
        }

        // GET: SDefaultCurrency
        public ActionResult Index()
        {
            try
            {
                DefautCurrencyViewModels model = new DefautCurrencyViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("DefaultCurrency_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(DefautCurrencyViewModels model)
        {
            try
            {
                var datas = _factory.GetListDefaultCurrency(model.StoreID, null, CurrentUser.ListOrganizationId);
                if (model.StoreID != null)
                    datas = datas.Where(x => x.StoreId == model.StoreID).ToList();
                model.List_DefaultCurrency = datas;
            }
            catch (Exception ex)
            {
                _logger.Error("DefaultCurrency_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

            return PartialView("_ListData", model);
        }

        public DefaultCurrencyModels GetDetail(string id)
        {
            try
            {
                DefaultCurrencyModels model = _factory.GetListDefaultCurrency(null, id)[0];
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("DefaultCurrency_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            DefaultCurrencyModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public ActionResult Create()
        {
            DefaultCurrencyModels model = new DefaultCurrencyModels();
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(DefaultCurrencyModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name field is required"));
                    return View(model);
                }
                string msg = "";
                bool result = _factory.InsertOrUpdateDefaultCurrency(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("DefaultCurrency_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            DefaultCurrencyModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(DefaultCurrencyModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteDefaultCurrency(model.Id, ref msg);
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
                _logger.Error("DefaultCurrency_Delete: " + ex);
                ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have a error when you delete an currency"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public PartialViewResult Edit(string id)
        {
            DefaultCurrencyModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(DefaultCurrencyModels model)
        {
            try
            {

                if (string.IsNullOrEmpty(model.StoreId))
                    ModelState.AddModelError("StoreID", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store"));
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Default Currency Name is required"));
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                string msg = "";
                //====================
                var result = _factory.InsertOrUpdateDefaultCurrency(model, ref msg);
                if (result)
                {

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", msg);
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("DefaultCurrency_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Import()
        {
            DefaultCurrencyModels model = new DefaultCurrencyModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(DefaultCurrencyModels model)
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
                    _logger.Error("Currency_Import: " + msg);
                    ModelState.AddModelError("ExcelUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency") + "_" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import") + ": " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            DefaultCurrencyModels model = new DefaultCurrencyModels();
            return View(model);
        }
        [HttpPost]
        public ActionResult Export(DefaultCurrencyModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsSetMenu = wb.Worksheets.Add("Sheet1");
                Shared.Models.StatusResponse response = _factory.Export(ref wsSetMenu, model.ListStores);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx",
                    CommonHelper.GetExportFileName("Currency").Replace(" ", "_")));

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
                _logger.Error("DefaultCurrency_Export: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }
    }
}