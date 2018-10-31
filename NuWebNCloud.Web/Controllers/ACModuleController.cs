using NLog;
using NuWebNCloud.Shared.Factory.AccessControl;
using NuWebNCloud.Shared.Models.AccessControl;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class ACModuleController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private ModuleFactory _factory = null;
        private List<ModuleModels> _ListModule = null;

        public ACModuleController()
        {
            _factory = new ModuleFactory();
            //================
            //ViewBag.ListStore = GetListStore();
            _ListModule = new List<ModuleModels>();
        }

        public ActionResult Index()
        {
            try
            {
                ModuleViewModels model = new ModuleViewModels();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Module_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(ModuleViewModels model)
        {
            try
            {
                var data = _factory.GetData();

                List<ModuleModels> lstModule = data;
                GetListModuleChild(lstModule, "");
                model.ListItem = _ListModule;
            }
            catch (Exception e)
            {
                _logger.Error("Module_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public List<ModuleModels> GetListModuleChild(List<ModuleModels> lstModule, string ParentId)
        {
            var lst = new List<ModuleModels>();
            var listData = lstModule.Where(x => x.ParentID.Equals(ParentId)).ToList();
            foreach (var item in listData)
            {
                var listChild = GetListModuleChild(lstModule, item.Id);
                ModuleModels module = new ModuleModels()
                {
                    Controller = item.Controller,
                    Id = item.Id,
                    Name = item.Name,

                    ParentName = item.Name,
                    ParentID = item.ParentID == null ? "" : item.ParentID,

                    Status = item.Status,
                    ListChild = listChild
                };
                if (ParentId.Equals(""))
                    _ListModule.Add(module);
                else
                    lst.Add(module);
            }
            return lst;
        }

        public ActionResult Create()
        {
            ModuleModels model = new ModuleModels();
            model.GetParent();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ModuleModels model)
        {
            try
            {

                if (model.ParentID != null)
                {
                    //var parentName = model.ListProductType.Find(x => x.Value.Equals(model.ProductTypeID)).Text.ToLower();
                    //if (!parentName.Equals("dish"))
                    //{
                    //    model.ParentID = null;
                    //}
                }
                if (!ModelState.IsValid)
                {
                    model.GetParent();
                    return View(model);
                }
                //====================
                string msg = "";
                model.CreatedUser = CurrentUser.Email;
                model.ModifiedUser = CurrentUser.Email;

                bool result = _factory.Insert(model, ref msg);
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
                _logger.Error("Module_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ModuleModels GetDetail(string id)
        {
            try
            {
                ModuleModels model = _factory.GetDetail(id);
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Module_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            ModuleModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            ModuleModels model = GetDetail(id);
            model.GetParent();
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(ModuleModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", "Module Name is required");
                if (!ModelState.IsValid)
                {
                    model = GetDetail(model.Id);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                model.ModifiedUser = CurrentUser.Email;
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
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Module_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            ModuleModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(ModuleModels model)
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
                _logger.Error("Module_Delete: " + ex);
                ModelState.AddModelError("Name", "Have an error when you delete a module");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult ViewAccounts()
        {
            return View();
        }
    }
}