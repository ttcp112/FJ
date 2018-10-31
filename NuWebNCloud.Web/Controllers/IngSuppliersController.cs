using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

// Updated 08292017
using NuWebNCloud.Shared.Models.Api;


namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngSuppliersController : HQController
    {
        private CompanyFactory _companyFactory = new CompanyFactory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private SupplierFactory _factory = null;
        private IngredientSupplierFactory _IngredientSupplierFactory = null;
        IngredientFactory _ingredientFactory = null;

        List<SelectListItem> lstCompany = new List<SelectListItem>();
        List<string> listCompanyId = new List<string>();

        public IngSuppliersController()
        {
            _factory = new SupplierFactory();
            _IngredientSupplierFactory = new IngredientSupplierFactory();
            _ingredientFactory = new IngredientFactory();

            ViewBag.ListCompany = GetListCompany();
            //==========
            lstCompany = ViewBag.ListCompany;
            listCompanyId = lstCompany.Select(x => x.Value).ToList();
        }

        public ActionResult Index()
        {
            try
            {
                SupplierViewModels model = new SupplierViewModels();
                if (lstCompany != null && lstCompany.Count > 0)
                {
                    model.CompanyId = lstCompany[0].Value;
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("SupplierIndex: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Search(SupplierViewModels model)
        {
            try
            {
                if (lstCompany != null && lstCompany.Count > 0)
                {
                    model.CompanyId = lstCompany[0].Value;
                }
                if (model.CompanyId != null)
                {                    
                    var data = _factory.GetData(model.CompanyId, CurrentUser.ListOrganizationId);
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
                _logger.Error("SupplierSearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            SupplierModels model = new SupplierModels();

            // Get list Countries
            // Updated 08292017
            List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
            foreach (var country in lstCountries)
            {
                model.ListCountries.Add(new SelectListItem
                {
                    Value = country.Name,
                    Text = country.Name,
                    Selected = country.Name.Equals(model.Country) ? true : false
                });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(SupplierModels model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Get list Countries
                    // Updated 08292017
                    List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
                    foreach (var country in lstCountries)
                    {
                        model.ListCountries.Add(new SelectListItem
                        {
                            Value = country.Name,
                            Text = country.Name,
                            Selected = country.Name.Equals(model.Country) ? true : false
                        });
                    }

                    return View(model);
                }

                model.CreatedBy = CurrentUser.UserName;
                model.ModifierBy = CurrentUser.UserName;
                model.CreatedDate = DateTime.Now;
                model.ModifierDate = DateTime.Now;
                model.ListSupIng = model.ListSupIng.Where(x => x.IsActived).ToList();

                string msg = "";
                bool result = _factory.Insert(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", msg);

                    // Get list Countries
                    // Updated 08292017
                    List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
                    foreach (var country in lstCountries)
                    {
                        model.ListCountries.Add(new SelectListItem
                        {
                            Value = country.Name,
                            Text = country.Name,
                            Selected = country.Name.Equals(model.Country) ? true : false
                        });
                    }

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("SupplierCreate: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public SupplierModels GetDetail(string id)
        {
            try
            {
                SupplierModels model = _factory.GetDetail(id);
                if (!string.IsNullOrEmpty(model.CompanyId))
                    model.CompanyName = lstCompany.Where(z => z.Value.Equals(model.CompanyId)).FirstOrDefault().Text;
                //=============
                var ListSupIng = _IngredientSupplierFactory.GetDataForSupplier(model.Id, model.CompanyId);
                var ListIng = _ingredientFactory.GetIngredient(null).Where(x => x.IsActive).ToList();
                foreach (var item in ListIng)
                {
                    model.ListSupIng.Add(new Ingredients_SupplierModel
                    {
                        IngredientId = item.Id,
                        IngredientName = item.Name,
                        IngredientCode = item.Code,
                        //IsCheck = ListSupIng.Any(z => z.Equals(item.Id)),
                        IsActived = ListSupIng.Any(x => x.Equals(item.Id)),
                    });
                }
                //model.ListSupIng.ForEach(x =>
                //{
                //    x.IsCheck = ListSupIng.Any(z => z.Equals(x.IngredientId));
                //});

                model.ListSupIng = model.ListSupIng.Where(x => x.IsActived).OrderByDescending(x => x.IsActived ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
                //model.ListSupIng = model.ListSupIng.OrderByDescending(x => x.IsActived ? 1 : 0).ThenBy(x => x.IngredientName).ToList();
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("SupplierDetail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            SupplierModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            SupplierModels model = GetDetail(id);

            // Get list Countries
            // Updated 08292017
            List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
            foreach (var country in lstCountries)
            {
                model.ListCountries.Add(new SelectListItem
                {
                    Value = country.Name,
                    Text = country.Name,
                    Selected = country.Name.Equals(model.Country) ? true : false
                });
            }

            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(SupplierModels model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Get list Countries
                    // Updated 08292017
                    List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
                    foreach (var country in lstCountries)
                    {
                        model.ListCountries.Add(new SelectListItem
                        {
                            Value = country.Name,
                            Text = country.Name,
                            Selected = country.Name.Equals(model.Country) ? true : false
                        });
                    }
                    return PartialView("_Edit", model);
                }
                model.ModifierDate = DateTime.Now;
                model.ModifierBy = CurrentUser.UserId;
                model.ListSupIng = model.ListSupIng.Where(x => x.IsActived).ToList();
                model.ListSupIngUnSelected = model.ListSupIngUnSelected.Where(x => x.IsActived).ToList();
                model.ListSupIng.AddRange(model.ListSupIngUnSelected);
                //====================
                string msg = "";
                var result = _factory.Update(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Get list Countries
                    // Updated 08292017
                    List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
                    foreach (var country in lstCountries)
                    {
                        model.ListCountries.Add(new SelectListItem
                        {
                            Value = country.Name,
                            Text = country.Name,
                            Selected = country.Name.Equals(model.Country) ? true : false
                        });
                    }

                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("SupplierEdit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult LoadIngredient(string Id, string CompanyId)
        {
            SupplierModels model = new SupplierModels();
            try
            {
                var ListSupIng = _IngredientSupplierFactory.GetDataForSupplier(Id, CompanyId);
                var ListIng = _ingredientFactory.GetIngredient(null).Where(x => x.IsActive && x.CompanyId.Equals(CompanyId)).ToList();
                foreach (var item in ListIng)
                {
                    model.ListSupIng.Add(new Ingredients_SupplierModel
                    {
                        IngredientId = item.Id,
                        IngredientName = item.Name,
                        IngredientCode = item.Code,
                        IsCheck = false,

                        CompanyId = item.CompanyId
                    });
                }
                model.ListSupIng = model.ListSupIng.OrderBy(oo => oo.IngredientCode).ThenBy(x => x.IngredientName).ToList();
            }
            catch (Exception e)
            {
                _logger.Error("SupplierLoadIngredient: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListIngredient", model);
        }

        public ActionResult LoadIngredientUnSelect(string Id, string CompanyId)
        {
            SupplierModels model = new SupplierModels();
            try
            {
                var ListSupIng = _IngredientSupplierFactory.GetDataForSupplier(Id, CompanyId);
                var ListIng = _ingredientFactory.GetIngredient(null).Where(x => x.IsActive && x.CompanyId.Equals(CompanyId)).ToList();
                foreach (var item in ListIng)
                {
                    model.ListSupIngUnSelected.Add(new Ingredients_SupplierModel
                    {
                        IngredientId = item.Id,
                        IngredientName = item.Name,
                        IngredientCode = item.Code,
                        IsCheck = false,
                        CompanyId = item.CompanyId
                    });
                }
                model.ListSupIngUnSelected = model.ListSupIngUnSelected.OrderBy(oo => oo.IngredientCode).ThenBy(x => x.IngredientName).ToList();
                model.ListSupIngUnSelected = model.ListSupIngUnSelected.Where(Ing => !(ListSupIng).Any(a1 => a1 == Ing.IngredientId)).ToList();
            }
            catch (Exception e)
            {
                _logger.Error("SupplierLoadIngredientUn: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListIngredientUnSelected", model);
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            SupplierModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(SupplierModels model)
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
                _logger.Error("SupplierDelete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Have an error when you delete an Supplier"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult Import()
        {
            SupplierModels model = new SupplierModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(SupplierModels model)
        {
            try
            {
                if (model.ListCompanys == null || model.ListCompanys.Count == 0)
                {
                    ModelState.AddModelError("ListCompanys", CurrentUser.GetLanguageTextFromKey("Please choose company."));
                    return View(model);
                }

                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ExcelUpload", CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    return View(model);
                }

                ImportModel importModel = new ImportModel();
                string msg = "";
                int totalRowExcel;
                if (model.ExcelUpload != null && model.ExcelUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ExcelUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);
                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ExcelUpload.SaveAs(filePath);

                    StatusResponse response = _factory.Import(filePath, CurrentUser.UserName, out totalRowExcel, model.ListCompanys, ref importModel, ref msg);

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
                    _logger.Error("Supplier_Import: " + msg);
                    ModelState.AddModelError("ExcelUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Supplier: " + e);
                ModelState.AddModelError("ExcelUpload", CurrentUser.GetLanguageTextFromKey("Import file have error"));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            SupplierModels model = new SupplierModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(SupplierModels model)
        {
            try
            {
                // set name for sheet in file excel
                XLWorkbook wb = new XLWorkbook();
                var wsSupplier = wb.Worksheets.Add("Supplier");
                var wsSupplierIngerdient = wb.Worksheets.Add("Ingredients_Supplier");

                var response = _factory.Export(ref wsSupplier, ref wsSupplierIngerdient, model.ListCompanys, lstCompany);


                if (!response.IsOk)
                {
                    ModelState.AddModelError("Supplier", response.Message);
                    return View(response);
                }

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Supplier").Replace(" ", "_")));

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
                _logger.Error("SupplierExport: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("List", CurrentUser.GetLanguageTextFromKey("Export file have error"));
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