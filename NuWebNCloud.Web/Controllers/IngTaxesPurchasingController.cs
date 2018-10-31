using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NuWebNCloud.Shared.Models.Ingredients.TaxPurchasing;
using NuWebNCloud.Shared.Utilities;
using System.Net;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models;
using ClosedXML.Excel;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngTaxesPurchasingController : HQController
    {
        private TaxFactory _factory = null;
        private ProductFactory _factoryProduct = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public IngTaxesPurchasingController()
        {
            _factory = new TaxFactory();
            _factoryProduct = new ProductFactory();
            ViewBag.ListStore = GetListStore();
            var xero = Commons.GetIntegrateInfo(CurrentUser.StoreId);
            //if (xero != null)
                ViewBag.ListTaxXero = new List<SelectListItem>();
        }
        // GET: TaxesPurchasing
        public ActionResult Index()
        {
            try
            {
                TaxPurchasingViewModels model = new TaxPurchasingViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("TaxPurchasing_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(TaxPurchasingViewModels model)
        {
            try
            {
                var datas = _factory.GetListTaxV2(model.StoreID, null, CurrentUser.ListOrganizationId, true);
                model.ListItem = datas.Where(o => o.Type == (int)Commons.ETaxType.Xero).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("TaxPurchasing_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            Shared.Models.Settings.TaxModels model = new Shared.Models.Settings.TaxModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(Shared.Models.Settings.TaxModels model)
        {
            try
            {

                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Name is required"));


                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                //=======
                string msg = "";
                model.Type = (int)Commons.ETaxType.Xero;
                bool result = _factory.InsertOrUpdateTax_V2(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //return RedirectToAction("Create");
                    ModelState.AddModelError("Name", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("TaxPurchasing_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public Shared.Models.Settings.TaxModels GetDetail(string id)
        {
            try
            {
                Shared.Models.Settings.TaxModels model = new Shared.Models.Settings.TaxModels();
                var models = _factory.GetListTaxV2(null, id, null, true);
                if (models != null && models.Any())
                {
                    model = models.FirstOrDefault();
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Tax_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            Shared.Models.Settings.TaxModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            Shared.Models.Settings.TaxModels model = GetDetail(id);
            var InfoXero = Commons.GetIntegrateInfo(model.StoreID);
            if (InfoXero != null)
            {
                if (!string.IsNullOrEmpty(InfoXero.ApiURL))
                {
                    ViewBag.ListTaxXero = ListTaxXero(InfoXero.IPAddress, InfoXero.ThirdPartyID, InfoXero.ApiURL);
                }
            }

            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(Shared.Models.Settings.TaxModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Name is required"));


                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //=========

                string msg = "";
                model.Type = (int)Commons.ETaxType.Xero;
                var result = _factory.InsertOrUpdateTax_V2(model, ref msg);
                if (result)
                    return RedirectToAction("Index");
                else
                {
                    //return PartialView("_Edit", model);
                    ModelState.AddModelError("Name", msg);
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("TaxPurchasing_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public PartialViewResult Delete(string id)
        {
            Shared.Models.Settings.TaxModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(Shared.Models.Settings.TaxModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteTax_V2(model.ID, ref msg);
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
                _logger.Error(" Taxes_for_Purchasing _Delete: " + ex);
                ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Categories"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult Import()
        {
            Shared.Models.Settings.TaxModels model = new Shared.Models.Settings.TaxModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(Shared.Models.Settings.TaxModels model)
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
                    _logger.Error("Tax_Import: " + msg);
                    ModelState.AddModelError("ExcelUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Tax_Import: " + e);
                ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            Shared.Models.Settings.TaxModels model = new Shared.Models.Settings.TaxModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(Shared.Models.Settings.TaxModels model)
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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Tax").Replace(" ", "_")));

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
                _logger.Error("Tax_Export: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult LoadItems(string StoreID, int ItemType)
        {
            Shared.Models.Settings.TaxModels model = new Shared.Models.Settings.TaxModels();
            dynamic lstData = _factoryProduct.GetListProduct(StoreID, ItemType, CurrentUser.ListOrganizationId);
            if (lstData != null)
            {
                model.ListProductOfTax = new List<ProductOfTaxDTO>();
                foreach (var item in lstData)
                {
                    ProductOfTaxDTO product = new ProductOfTaxDTO()
                    {
                        ProductID = item.ID,
                        ProductName = item.Name,
                        ProductCode = item.ProductCode,
                        ProductTypeCode = (byte)ItemType
                    };
                    model.ListProductOfTax.Add(product);
                }
                model.ListProductOfTax = model.ListProductOfTax.OrderBy(x => x.ProductName).ToList();
                model.StoreID = StoreID;
            }
            return PartialView("_TableChooseItems", model);
        }

        public ActionResult AddItems(Shared.Models.Settings.TaxModels data)
        {
            Shared.Models.Settings.TaxModels model = new Shared.Models.Settings.TaxModels();
            if (data.ListProductOfTax != null && data.ListProductOfTax.Count > 0)
                model.ListProductOfTax = new List<ProductOfTaxDTO>();
            for (int i = 0; i < data.ListProductOfTax.Count; i++)
            {
                ProductOfTaxDTO item = new ProductOfTaxDTO();
                item.OffSet = data.currentItemOffset;
                item.ProductID = data.ListProductOfTax[i].ProductID;
                item.ProductCode = data.ListProductOfTax[i].ProductCode;
                item.ProductName = data.ListProductOfTax[i].ProductName;
                item.ProductTypeCode = data.ListProductOfTax[i].ProductTypeCode;
                model.ListProductOfTaxSel.Add(item);
                data.currentItemOffset++;
            }
            return PartialView("_ItemModal", model);
        }

        public ActionResult CheckProductOnTax(Shared.Models.Settings.TaxModels data)
        {
            Shared.Models.Settings.TaxModels model = new Shared.Models.Settings.TaxModels();
            if (data.ListProductOfTax != null && data.ListProductOfTax.Count > 0)
                model.ListProductOfTax = new List<ProductOfTaxDTO>();
            //===============
            List<string> ListProductID = data.ListProductOfTax.Select(x => x.ProductID).ToList();
            var listData = _factory.CheckProductOnTax(data.StoreID, null, ListProductID, (byte)Commons.ETaxType.Xero);
            if (!string.IsNullOrEmpty(data.ID))
                listData = listData.Where(x => x.TaxID != data.ID).ToList();
            return Json(listData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTaxXeroByStore(string StoreId)
        {
            try
            {
                NSLog.Logger.Info("GetTaxXeroByStore_Request : ", StoreId);

                var InfoXero = Commons.GetIntegrateInfo(StoreId);
                     if (InfoXero != null)
                    {
                        if ( !string.IsNullOrEmpty(InfoXero.ApiURL))
                        {
                            var data = ListTaxXero(InfoXero.IPAddress, InfoXero.ThirdPartyID, InfoXero.ApiURL);
                            if (data != null)
                            {
                                var dataJson = data.Select(o => new
                                {
                                    id = o.Value,
                                    text = o.Text
                                }).ToList();
                                return Json(dataJson, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }


            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetTaxXeroByStore : ", ex);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }
}