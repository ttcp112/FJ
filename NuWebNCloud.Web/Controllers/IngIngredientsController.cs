using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Ingredients;
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
    public class IngIngredientsController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private IngredientFactory _factory = null;
        private IngredientUOMFactory _IngredientUOMFactory = null;
        private IngredientSupplierFactory _IngredientSupplierFactory = null;
        private SupplierFactory _SupplierFactory = null;

        List<SelectListItem> lstCompany = new List<SelectListItem>();
        List<string> listCompanyId = new List<string>();

        public IngIngredientsController()
        {
            _factory = new IngredientFactory();
            _IngredientUOMFactory = new IngredientUOMFactory();
            _IngredientSupplierFactory = new IngredientSupplierFactory();
            _SupplierFactory = new SupplierFactory();
            ViewBag.ListCompany = GetListCompany();
            //==========
            lstCompany = ViewBag.ListCompany;
            listCompanyId = lstCompany.Select(x => x.Value).ToList();
        }

        // GET: IngUOMs
        public ActionResult Index()
        {
            try
            {
                IngredientViewModel model = new IngredientViewModel();
                model.CompanyId = listCompanyId[0];
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("IngredientIndex: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(IngredientViewModel model)
        {
            try
            {
                model.CompanyId = listCompanyId[0];
                if (model.CompanyId != null)
                {
                    var data = _factory.GetData(model);
                    data.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x.CompanyId))
                        {
                            x.CompanyName = lstCompany.Where(z => z.Value.Equals(x.CompanyId)).FirstOrDefault().Text;
                        }
                    });
                    model.ListItem = data;
                }
            }
            catch (Exception e)
            {
                _logger.Error("IngredientSearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            IngredientModel model = new IngredientModel();
            model.ReceivingQty = 1;
            model.GetFillData(CurrentUser.ListOrganizationId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(IngredientModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.GetFillData(CurrentUser.ListOrganizationId);
                    return View(model);
                }

                model.CreatedBy = CurrentUser.UserName;
                model.UpdatedBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;
                model.ListIngUOM = model.ListIngUOM.Where(x => x.Status != (int)Commons.EStatus.Deleted).ToList();
                model.ListIngSupplier = model.ListIngSupplier.Where(x => x.IsActived).ToList();

                //For Xero Ingredient
                model.ListStoreId = CurrentUser.ListStoreID;
                //==============
                string msg = "";
                bool result = _factory.Insert(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    model.GetFillData(CurrentUser.ListOrganizationId);
                    ModelState.AddModelError("CompanyId", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public IngredientModel GetDetail(string id)
        {
            try
            {
                IngredientModel model = _factory.GetIngredientById(id);
                if (!string.IsNullOrEmpty(model.CompanyId))
                    model.CompanyName = lstCompany.Where(z => z.Value.Equals(model.CompanyId)).FirstOrDefault().Text;
                else
                    model.CompanyName = "None";

                model.GetFillData(CurrentUser.ListOrganizationId);
                //=============
                var ListIngUOM = _IngredientUOMFactory.GetDataForIngredient(model.Id);
                for (int i = 0; i < ListIngUOM.Count; i++)
                {
                    var IngUOM = ListIngUOM[i];
                    model.ListIngUOM.Add(new IngredientUOMModels
                    {
                        Id = IngUOM.Id,
                        OffSet = i,
                        UOMId = IngUOM.UOMId,
                        ReceivingQty = IngUOM.ReceivingQty,
                        UOMName = IngUOM.UOMName
                    });
                }
                var ListIngSup = _IngredientSupplierFactory.GetDataForIngredient(model.Id, model.CompanyId);
                var lstSupplierItem = _SupplierFactory.GetData();
                lstSupplierItem = lstSupplierItem.Where(x => x.CompanyId.Equals(model.CompanyId)).ToList();
                if (lstSupplierItem != null)
                {
                    foreach (SupplierModels supplier in lstSupplierItem)
                        model.ListIngSupplier.Add(new Ingredients_SupplierModel
                        {
                            Id = supplier.Id,
                            SupplierName = supplier.Name,
                            SupplierAddress = supplier.Address,
                            SupplierPhone = supplier.Phone1 + " - " + supplier.Phone2,
                            //IsCheck = false,
                            IsActived = ListIngSup.Any(x => x.Equals(supplier.Id)),
                            CompanyId = supplier.CompanyId
                        });
                    model.ListIngSupplier = model.ListIngSupplier.OrderBy(oo => oo.SupplierName).ToList();
                }
                //model.ListIngSupplier.ForEach(x =>
                //{
                //    x.IsCheck = ListIngSup.Any(z => z.Equals(x.Id));
                //});
                model.ListIngSupplier = model.ListIngSupplier.Where(x => x.IsActived).OrderByDescending(x => x.IsActived ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("IngredienDetail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            IngredientModel model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            IngredientModel model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(IngredientModel model)
        {
            try
            {
                //======
                if (!ModelState.IsValid)
                {
                    model.GetFillData(CurrentUser.ListOrganizationId);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                model.UpdatedBy = CurrentUser.UserName;
                model.UpdatedDate = DateTime.Now;
                //======
                model.ListIngSupplier = model.ListIngSupplier.Where(x => x.IsActived).ToList();
                model.ListIngSupplierUnSelected = model.ListIngSupplierUnSelected.Where(x => x.IsActived).ToList();
                model.ListIngSupplier.AddRange(model.ListIngSupplierUnSelected);
                //===============
                model.ListIngUOM = model.ListIngUOM.Where(x => x.Status != (int)Commons.EStatus.Deleted).ToList();
                List<string> listItemUOMOnData = _IngredientUOMFactory.GetDataForIngredient(model.Id).Select(x => x.Id).ToList();
                var listIngUOMIdDelete = listItemUOMOnData.Where(a => !(model.ListIngUOM.Select(x => x.Id).ToList()).Any(a1 => a1 == a)).ToList();

                //For Xero Ingredient
                model.ListStoreId = CurrentUser.ListStoreID;
                //==============
                string msg = "";
                bool result = _factory.Update(model, listIngUOMIdDelete, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    model.GetFillData(CurrentUser.ListOrganizationId);
                    ModelState.AddModelError("CompanyId", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                model.GetFillData(CurrentUser.ListOrganizationId);
                _logger.Error(ex.Message);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult LoadSupplier(string CompanyId)
        {
            IngredientModel model = new IngredientModel();
            try
            {
                var lstSupplierItem = _SupplierFactory.GetData();
                lstSupplierItem = lstSupplierItem.Where(x => x.IsActived && x.CompanyId.Equals(CompanyId)).ToList();
                if (lstSupplierItem != null)
                {
                    foreach (SupplierModels supplier in lstSupplierItem)
                        model.ListIngSupplier.Add(new Ingredients_SupplierModel
                        {
                            Id = supplier.Id,
                            SupplierName = supplier.Name,
                            SupplierAddress = supplier.Address,
                            SupplierPhone = supplier.Phone1 + " - " + supplier.Phone2,
                            IsCheck = false,

                            CompanyId = supplier.CompanyId
                        });
                }
                if (model.ListIngSupplier != null && model.ListIngSupplier.Count > 0)
                {
                    model.ListIngSupplier = model.ListIngSupplier.OrderBy(oo => oo.SupplierName).ToList();
                }
            }
            catch (Exception e)
            {
                _logger.Error("IngredientLoadSupplier: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListSupplier", model);
        }

        public ActionResult LoadSupplierUnSelected(string Id, string CompanyId)
        {
            IngredientModel model = new IngredientModel();
            try
            {
                var ListIngSup = _IngredientSupplierFactory.GetDataForIngredient(Id, CompanyId);
                var lstSupplierItem = _SupplierFactory.GetData();
                lstSupplierItem = lstSupplierItem.Where(x => x.IsActived && x.CompanyId.Equals(CompanyId)).ToList();
                if (lstSupplierItem != null)
                {
                    foreach (SupplierModels supplier in lstSupplierItem)
                        model.ListIngSupplierUnSelected.Add(new Ingredients_SupplierModel
                        {
                            Id = supplier.Id,
                            SupplierName = supplier.Name,
                            SupplierAddress = supplier.Address,
                            SupplierPhone = supplier.Phone1 + " - " + supplier.Phone2,
                            IsCheck = false,

                            CompanyId = supplier.CompanyId
                        });
                }
                if (model.ListIngSupplierUnSelected != null && model.ListIngSupplierUnSelected.Count > 0)
                {
                    model.ListIngSupplierUnSelected = model.ListIngSupplierUnSelected.OrderBy(oo => oo.SupplierName).ToList();
                    model.ListIngSupplierUnSelected = model.ListIngSupplierUnSelected.Where(Ing => !(ListIngSup).Any(a1 => a1 == Ing.Id)).ToList();
                }
            }
            catch (Exception e)
            {
                _logger.Error("IngredientLoadUnSelectedSupplier: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListSupplierUnSelected", model);
        }

        public PartialViewResult Delete(string id)
        {
            IngredientModel model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(IngredientModel model)
        {
            try
            {
                string msg = string.Empty;

                _factory.Delte(model, ref msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return new HttpStatusCodeResult(400, ex.Message);
            }

        }

        [HttpPost]
        public ActionResult InsertIngredients(IngredientModel model)
        {
            //Check 
            var result = _factory.CheckInsert(model);
            if (!result.IsOk)
            {
                ModelState.AddModelError("Exist", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(result.Message));
                return View("Index", model);
            }
            return View("Index", model);
        }

        [HttpPost]
        public ActionResult EnableActive(List<string> lstId, bool active)
        {
            var data = _factory.EnableActive(lstId, active);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult AddItemUOM(int currentOffset)
        {
            IngredientUOMModels itemUOM = new IngredientUOMModels();
            itemUOM.OffSet = currentOffset;
            return PartialView("_ItemUOM", itemUOM);
        }

        #region Import
        public ActionResult Import()
        {
            IngredientModel model = new IngredientModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(IngredientModel model)
        {
            try
            {
                if (model.ListCompany.Count == 0)
                {
                    ModelState.AddModelError("ListCompany", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose company."));
                    return View(model);
                }
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    return View(model);
                }
                string fileName = Path.GetFileName(model.ExcelUpload.FileName);
                string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);

                IngredientImportResultModels importModel = new IngredientImportResultModels();
                string msg = "";

                //upload file to server
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                model.ExcelUpload.SaveAs(filePath);
                int totalRowExcel;
                importModel.ListImport = _factory.Import(filePath, CurrentUser.UserName, CurrentUser.ListOrganizationId, out totalRowExcel, model.ListCompany, CurrentUser.ListStoreID, ref msg);
                importModel.TotalRowExcel = totalRowExcel;

                //delete file excel after insert to database
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                //return View("DetailStatus", result);
                if (msg.Equals(""))
                {
                    return View("ImportDetail", importModel);
                }
                else
                {
                    _logger.Error("IngredientImport: " + msg);
                    ModelState.AddModelError("ListCompany", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex);
                //return new HttpStatusCodeResult(400, ex.Message);
                _logger.Error("Ingredient_Import: " + ex);
                ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(ex.Message));
                return View(model);
            }
        }
        #endregion

        #region export
        public ActionResult Export()
        {
            IngredientModel model = new IngredientModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(IngredientModel model, bool isExport = true)
        {
            try
            {
                //IngredientModel model = new IngredientModel();
                XLWorkbook wb = new XLWorkbook();
                var wsIngredient = wb.Worksheets.Add("Ingredients");
                var wsIngredientUOM = wb.Worksheets.Add("IngredientUOM");
                var wsIngredientSupplier = wb.Worksheets.Add("IngredientSupplier");

                var data = _factory.Export(ref wsIngredient, ref wsIngredientUOM, ref wsIngredientSupplier, model.ListCompany, lstCompany);

                if (!data.IsOk)
                {
                    ModelState.AddModelError("ingredient", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(data.Message));
                    return View(data);
                }
                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = CommonHelper.GetExportFileName("Ingredients").Replace(" ", "_");
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", fileName));

                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                ViewBag.IsSuccess = true;
                return RedirectToAction("Export");
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }
        #endregion
    }
}