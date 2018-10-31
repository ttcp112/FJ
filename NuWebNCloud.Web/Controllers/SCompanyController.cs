using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models.Settings.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    public class SCompanyController : HQController
    {       
        public SCompanyController()
        {
            
        }
        public CompanyModels GetDetail(string id)
        {
            try
            {
                CompanyModels model = EditCompanyFatory.Company.GetDetailCompany(id, CurrentUser.ListOrganizationId)[0];
                return model;
            }
            catch (Exception ex)
            {
                NSLog.Logger.Info("Get Detail Company", ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {

            CompanyModels model = GetDetail(id);
            return PartialView("_View", model);
        }
        public PartialViewResult Edit(string id)
        {
            CompanyModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(CompanyModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name is required"));

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //====================
                string msg = "";
                var result = EditCompanyFatory.Company.UpdateCompany(model, ref msg, CurrentUser.ListOrganizationId);
                if (result)
                {
                    return RedirectToAction("Index", "SStoreInformation");
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
                NSLog.Logger.Info("Edit Company", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}