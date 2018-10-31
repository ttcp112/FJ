using NLog;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Settings;
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
    public class SettingSchedulerReportController : HQController
    {
        private SchedulerReportFactory _factory { get; set; }
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SettingSchedulerReportController()
        {
            _factory = new SchedulerReportFactory();
        }

        public ActionResult Index()
        {
            try
            {
                SettingSchedulerTaskViewModels model = new SettingSchedulerTaskViewModels();
                SettingSchedulerTaskModels temp = new SettingSchedulerTaskModels();
                temp.GetListReportName();
                model.ListSchedulerTaskModels = _factory.GetData();
                foreach (var item in model.ListSchedulerTaskModels)
                {
                    if (temp.ListNameReport.Any())
                    {
                        item.ReportName = temp.ListNameReport.Where(o => o.Value.Equals(item.ReportId)).Select(s => s.Text).FirstOrDefault();
                    }
                    item.StoreName = GetStoreName(item.StoreId);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("SettingSchedulerTask_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Create()
        {
            SettingSchedulerTaskModels model = new SettingSchedulerTaskModels();
            //model.GetDateOfWeek();
            //===============
            var user = System.Web.HttpContext.Current.Session["User"] as UserSession;
            List<string> listOrganizationId = new List<string>();
            if (user != null)
            {
                listOrganizationId = user.ListOrganizationId;
            }
            model.GetDataStore(listOrganizationId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(SettingSchedulerTaskModels model)
        {
            try
            {
                //model.GetDateOfWeek();
                //===============
                var user = System.Web.HttpContext.Current.Session["User"] as UserSession;
                List<string> listOrganizationId = new List<string>();
                if (user != null)
                {
                    listOrganizationId = user.ListOrganizationId;
                }
                var storeName = model.GetDataStore(listOrganizationId).Where(x => x.Value == model.StoreId).Select(x => x.Text).FirstOrDefault();
                model.StoreName = storeName;
                //==================
                if (model.ReportId == null)
                {
                    ModelState.AddModelError("ReportId", "Please choose Report!!!");
                }
                var _obj = Request.Form["ReportId"].Split(',');
                if (!ModelState.IsValid)
                {
                    foreach (var item in _obj)
                    {
                        model.ListReportID.Add(item);
                    }
                    return View(model);
                }

                foreach (var item in _obj)
                {
                    string _report = item;
                    ResultModels listStatusResponses = _factory.CreateSchedulerTask(model, _report);
                }          
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error("SettingSchedulerTask_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public SettingSchedulerTaskModels GetDetail(string id, string StoreId)
        {
            try
            {
                return _factory.FindById(id, StoreId);
            }
            catch (Exception ex)
            {
                _logger.Error("SchedulerTaskModels_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id, string StoreId)
        {
            SettingSchedulerTaskModels model = GetDetail(id, StoreId);
            model.GetListReportName();
            model.ReportName = model.ListNameReport.Where(o => o.Value.Equals(model.ReportId)).Select(s => s.Text).FirstOrDefault();
            if (model.StoreId != null)
            {
                model.StoreName = GetStoreName(model.StoreId);
            }            
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id, string StoreId)
        {
            SettingSchedulerTaskModels model = GetDetail(id, StoreId);
            //===============
            var user = System.Web.HttpContext.Current.Session["User"] as UserSession;
            List<string> listOrganizationId = new List<string>();
            if (user != null)
            {
                listOrganizationId = user.ListOrganizationId;
            }
            model.GetDataStore(listOrganizationId);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(SettingSchedulerTaskModels model)
        {
            try
            {
                //model.GetDateOfWeek();
                //===============
                var user = System.Web.HttpContext.Current.Session["User"] as UserSession;
                List<string> listOrganizationId = new List<string>();
                if (user != null)
                {
                    listOrganizationId = user.ListOrganizationId;
                }
                var storeName = model.GetDataStore(listOrganizationId).Where(x => x.Value == model.StoreId).Select(x => x.Text).FirstOrDefault();
                model.StoreName = storeName;
                //string DayOfWeeks = "";
                //if (Request.Form["DayOfWeeks"] == null)
                //{
                //    //ModelState.AddModelError("DayOfWeeks", "Please choose Day of week!!!");
                //    DayOfWeeks = Request.Form["DoW"].ToString();
                //}
                //else
                //{
                //    DayOfWeeks = Request.Form["DayOfWeeks"].ToString();
                //}
                //model.DayOfWeeks = DayOfWeeks;
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                
                ResultModels result = _factory.UpdateCurrentSchedulerTask(model);
                if (result.IsOk) // Success
                {
                    //return RedirectToAction("Index");
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else //Fail
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Organization_Edit: " + ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id, string StoreId)
        {
            SettingSchedulerTaskModels model = GetDetail(id, StoreId);
            model.GetListReportName();
            model.ReportName = model.ListNameReport.Where(o => o.Value.Equals(model.ReportId)).Select(s => s.Text).FirstOrDefault();
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(SettingSchedulerTaskModels model)
        {
            try
            {
                ResultModels result = _factory.DeleteUpdateCurrentSchedulerTask(model);
                if (result.IsOk)
                {
                    //return RedirectToAction("Index");
                }
                else
                {
                    model.GetListReportName();
                    model.ReportName = model.ListNameReport.Where(o => o.Value.Equals(model.ReportId)).Select(s => s.Text).FirstOrDefault();
                    ModelState.AddModelError("ReportId", "Have a error when you deleted an Scheduler Report");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("SchedulerTaskModels_Delete: " + ex);
                ModelState.AddModelError("Name", "Have a error when you deleted an Scheduler Report");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public JsonResult GetMake(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            var items = new List<SelectItem>();

            string[] idList = id.Split(new char[] { ',' });
            foreach (var idStr in idList)
            {
                int idInt;
                if (int.TryParse(idStr, out idInt))
                {
                    items.Add(_makes.FirstOrDefault(m => m.id == idInt));
                }
            }

            return Json(items, JsonRequestBehavior.AllowGet); ;
        }

        private IEnumerable<SelectItem> _makes = new List<SelectItem>
        {
            new SelectItem { id = 1, text = "Sunday" },
            new SelectItem { id = 2, text = "Monday" },
            new SelectItem { id = 3, text = "Tuesday" },
            new SelectItem { id = 4, text = "Wednesday" },
            new SelectItem { id = 5, text = "Thursday" },
            new SelectItem { id = 6, text = "Friday" },
            new SelectItem { id = 7, text = "Saturday" }
        };
        private string GetStoreName(string StoreID)
        {
            string StoreName = "";
            var lstStore = GetListStore();
            StoreName = lstStore.Where(o => o.Value.Equals(StoreID)).Select(s => s.Text).FirstOrDefault();
            return StoreName;
        }
    }

    public class SelectItem
    {
        public int id { get; set; }
        public string text { get; set; }
    }
}