using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Integration.Factory.Sandbox;
using NuWebNCloud.Shared.Integration.Models.Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    public class SEmployeeAddCompanyInteController : HQController
    {
        private InteEmployeeFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private EmployeeAddCompanyFactory _EmpAddCompFactory = null;
        public SEmployeeAddCompanyInteController ()
        {
            _factory = new InteEmployeeFactory();
            _EmpAddCompFactory = new EmployeeAddCompanyFactory();
            ViewBag.ListCompany = GetListCompany();
            ViewBag.ListStore = GetListStoreIntegration();
        }
        // GET: SEmployeeAddCompany
        public ActionResult Index()
        {
            try
            {
                InteEmployeeViewModels model = new InteEmployeeViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Employee Add Company_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(InteEmployeeViewModels model)
        {
            try
            {
                var data = _factory.GetListEmployee(model.StoreID, null, CurrentUser.ListOrganizationId);
                data.ForEach(x => x.ImageURL = string.IsNullOrEmpty(x.ImageURL) ? Commons.Image100_100 : x.ImageURL);
                model.ListItem = data;

            }
            catch (Exception e)
            {
                _logger.Error("Employee Add Company Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }
        public InteEmployeeModels GetDetail(string id, string StoreId)
        {
            try
            {
                InteEmployeeModels model = _factory.GetListEmployee(null, id)[0];                 
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Employee Add Company_Detail: " + ex);
                return null;
            }
        }
        public PartialViewResult EmpAddComp(string id, string StoreId)
        {

            InteEmployeeModels model = new InteEmployeeModels();            
            model = GetDetail(id, StoreId);
            model.LstCompany = ViewBag.ListCompany;
            if (model.ListCompany.Count > 0)
            {
                foreach (var item in model.ListCompany)
                {
                    var compChoose = model.LstCompany.Where(w => w.Value == item.Id).FirstOrDefault();
                    if (compChoose != null)
                    {
                        compChoose.Selected = true;
                    }
                }
            }
            return PartialView("_Edit", model);
        }
        [HttpPost]
        public ActionResult EmpAddComp(InteEmployeeModels model)
        {
            try
            {
                var name = model.Name;
                model.LstCompany = model.LstCompany.Where(x => x.Selected == true).ToList();
                if(model.LstCompany.Count == 0)
                {
                    ModelState.AddModelError("LstCompany", CurrentUser.GetLanguageTextFromKey("Please choose Company"));
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    model.Name = name;
                    model.LstCompany = ViewBag.ListCompany;
                    return PartialView("_Edit", model);

                }
                
                string msg = "";
                var result = _EmpAddCompFactory.EmployeeAddCompany(model, ref msg);
                if (result)
                {                   
                    return RedirectToAction("Index");
                }
                else
                {                 
                   
                    model = GetDetail(model.ID, model.StoreID);
                    model.LstCompany = ViewBag.ListCompany;
                    if (model.ListCompany.Count > 0)
                    {
                        foreach (var item in model.ListCompany)
                        {
                            var compChoose = model.LstCompany.Where(w => w.Value == item.Id).FirstOrDefault();
                            if (compChoose != null)
                            {
                                compChoose.Selected = true;
                            }
                        }
                    }                    
                    return PartialView("_Edit", model);
                }
            }
            catch (FormatException fEx)
            {
                _logger.Error("Employee Add Company: " + fEx.Message);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey(fEx.Message));
                return PartialView("_Edit", model);
            }            
        }
    }
}