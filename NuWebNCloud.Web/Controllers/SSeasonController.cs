using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings.Season;
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
    public class SSeasonController : HQController
    {
        private SeasonFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public SSeasonController()
        {
            _factory = new SeasonFactory();
            ViewBag.ListStore = GetListStore();
        }
        // GET: SSeason
        public ActionResult Index()
        {
            try
            {
                SeasonModelsView model = new SeasonModelsView();
                model.StoreID = CurrentUser.StoreId;
                return View(model);

            }
            catch (Exception ex)
            {
                _logger.Error("Season_Index:" + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(SeasonModelsView model)
        {
            try
            {
                var data = _factory.GetListSeason(model.StoreID, null, CurrentUser.ListOrganizationId);
                foreach (var item in data)
                {

                    if (item.StartTime.Value.Date == Commons._UnlimitedDate.Date || item.EndTime.Value.Date == Commons._UnlimitedDate.Date)
                    {
                        item.Unlimited = true;
                    }
                    else
                    {
                        item.StartTime = item.StartTime.Value.ToLocalTime();
                        item.EndTime = item.EndTime.Value.ToLocalTime();
                    }
                    item.StartDate = new DateTime(item.StartDate.Year, item.StartDate.Month, item.StartDate.Day, item.StartTime.Value.Hour, item.StartTime.Value.Minute, item.StartTime.Value.Second);
                    item.EndDate = new DateTime(item.EndDate.Year, item.EndDate.Month, item.EndDate.Day, item.EndTime.Value.Hour, item.EndTime.Value.Minute, item.EndTime.Value.Second);
                    //=============
                    if (item.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                    {
                        item.ListWeekDayV2.ForEach(x =>
                        {
                            if (item.ListDay.Contains(x.Index))
                            {
                                x.IsActive = true;
                                x.Status = 1;
                            }
                        });
                    }
                    else if (item.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                    {
                        item.ListMonthDayV2.ForEach(x =>
                        {
                            if (item.ListDay.Contains(x.Index))
                            {
                                x.IsActive = true;
                                x.Status = 1;
                            }
                        });
                    }
                }
                model.List_Season = data;
            }
            catch (Exception ex)
            {
                _logger.Error("Season_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            SeasonModels model = new SeasonModels();
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(SeasonModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                {
                    ModelState.AddModelError("StoreID", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store"));
                }
                else if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name field is required"));
                }
                //# Date
                if (model.StartDate > model.EndDate)
                    ModelState.AddModelError("StartDate", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Start Date must be less than To Date."));

                if (model.Unlimited)
                {
                    model.StartTime = Commons._MinDate; //new DateTime(1900, 1, 1, 0, 0, 0);
                    model.EndTime = Commons._MinDate;   //new DateTime(1900, 1, 1, 0, 0, 0);
                }
                else
                {
                    model.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, model.TStartTime.Hours, model.TStartTime.Minutes, model.TStartTime.Days);
                    model.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, model.TEndTime.Hours, model.TEndTime.Minutes, model.TEndTime.Days);
                }
                //# Repeat Type
                if (model.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                {
                    if (model.ListWeekDayV2.Count(x => x.Status == (byte)Commons.EStatus.Deleted) == 7)
                        ModelState.AddModelError("RepeatType", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please select days in a week"));
                    else
                        model.ListDay = model.ListWeekDayV2.Where(x => x.Status != (byte)Commons.EStatus.Deleted).Select(x => x.Index).ToList();
                }
                else if (model.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                {
                    if (model.ListMonthDayV2.Count(x => x.Status == (byte)Commons.EStatus.Deleted) == 31)
                        ModelState.AddModelError("RepeatType", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please select days in a month"));
                    else
                        model.ListDay = model.ListMonthDayV2.Where(x => x.Status != (byte)Commons.EStatus.Deleted).Select(x => x.Index).ToList();
                }
                //============
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                //===========
                string msg = "";
                bool result = _factory.InsertOrUpdateSeason(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //return RedirectToAction("Create");
                    msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                    ModelState.AddModelError("Name", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Season_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

        }

        public SeasonModels GetDetail(string StoreID, string id)
        {
            try
            {
                SeasonModels model = _factory.GetListSeason(StoreID, id)[0];
                if (model.StartTime.Value.Date == Commons._UnlimitedDate.Date || model.EndTime.Value.Date == Commons._UnlimitedDate.Date)
                {
                    model.Unlimited = true;

                    model.TStartTime = TimeSpan.Zero;
                    model.TEndTime = TimeSpan.Zero;
                }
                else
                {
                    model.StartTime = model.StartTime.Value.ToLocalTime();
                    model.EndTime = model.EndTime.Value.ToLocalTime();

                    model.TStartTime = model.StartTime.Value.TimeOfDay;
                    model.TEndTime = model.EndTime.Value.TimeOfDay;
                }
                

                if (model.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                {
                    model.ListWeekDayV2.ForEach(x =>
                    {
                        if (model.ListDay.Contains(x.Index))
                        {
                            x.IsActive = true;
                            x.Status = 1;
                        }
                    });
                }
                else if (model.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                {
                    model.ListMonthDayV2.ForEach(x =>
                    {
                        if (model.ListDay.Contains(x.Index))
                        {
                            x.IsActive = true;
                            x.Status = 1;
                        }
                    });
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Season_Detail: " + ex);
                return null;
            }
        }
        [HttpGet]
        public new PartialViewResult View(string StoreID, string id)
        {
            SeasonModels model = GetDetail(StoreID, id);
            model.StartDate = model.StartDate.ToLocalTime();
            model.EndDate = model.EndDate.ToLocalTime();
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string StoreID, string id)
        {
            SeasonModels model = GetDetail(StoreID, id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(SeasonModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose Store."));
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name field is required"));

                //# Date
                if (model.StartDate > model.EndDate)
                    ModelState.AddModelError("StartDate", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Start Date must be less than To Date."));

                if (model.Unlimited)
                {
                    model.StartTime = Commons._MinDate; // new DateTime(1900, 01, 01, 0, 0, 0);
                    model.EndTime = Commons._MinDate;   //new DateTime(1900, 01, 01, 0, 0, 0);
                }
                else
                {
                    model.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, model.TStartTime.Hours, model.TStartTime.Minutes, model.TStartTime.Days);
                    model.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, model.TEndTime.Hours, model.TEndTime.Minutes, model.TEndTime.Days);
                }
                //# Repeat Type
                if (model.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                {
                    if (model.ListWeekDayV2.Count(x => x.Status == (byte)Commons.EStatus.Deleted) == 7)
                        ModelState.AddModelError("RepeatType", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please select days in a week"));
                    else
                        model.ListDay = model.ListWeekDayV2.Where(x => x.Status != (byte)Commons.EStatus.Deleted).Select(x => x.Index).ToList();
                }
                else if (model.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                {
                    if (model.ListMonthDayV2.Count(x => x.Status == (byte)Commons.EStatus.Deleted) == 31)
                        ModelState.AddModelError("RepeatType", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please select days in a month"));
                    else
                        model.ListDay = model.ListMonthDayV2.Where(x => x.Status != (byte)Commons.EStatus.Deleted).Select(x => x.Index).ToList();
                }

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //====================
                string msg = "";
                var result = _factory.InsertOrUpdateSeason(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    //return PartialView("_Edit", model);
                    ModelState.AddModelError("Name", msg);
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Season_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public PartialViewResult Delete(string StoreID, string id)
        {
            SeasonModels model = GetDetail(StoreID, id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(SeasonModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteSeason(model.ID, ref msg);
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
                _logger.Error("Season_Delete: " + ex);
                ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Season"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult Import()
        {
            SeasonModels model = new SeasonModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(SeasonModels model)
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
                    ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("File excel cannot be null"));
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
                    _logger.Error("Season_Import: " + msg);
                    ModelState.AddModelError("ExcelUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e) 
            {
                _logger.Error("Season_Import: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            SeasonModels model = new SeasonModels();
            return View(model);
        }


        [HttpPost]
        public ActionResult Export(SeasonModels model)
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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Season").Replace(" ", "_")));

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
                _logger.Error("Season_Export: " + e);
                //return new HttpStatusCodeResult(400, e.Message);
                ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }
    }
}