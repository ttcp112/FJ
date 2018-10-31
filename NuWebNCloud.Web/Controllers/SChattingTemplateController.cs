using ClosedXML.Excel;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings;
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
    public class SChattingTemplateController : HQController
    {
        private ChatTemplateFactory _factory = null;

        public SChattingTemplateController()
        {
            _factory = new ChatTemplateFactory();
        }
        // GET: SChattingTemplate
        public ActionResult Index()
        {
            var model = new ChatTemplateViewModels();
            return View(model);
        }

        public ActionResult Search()
        {
            var model = new ChatTemplateViewModels();
            try
            {
                var datas = _factory.GetListChattingTemplate(CurrentUser.ListOrganizationId, false,null,null);
                model.ListItemArtist = datas.Where(x=>x.ChatTemplateType == (int)Commons.EChatTemplate.Artiste).ToList();
                model.ListItemCus = datas.Where(x => x.ChatTemplateType == (int)Commons.EChatTemplate.Customer).ToList();
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetListChatting_error", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            ChatTemplateModels model = new ChatTemplateModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ChatTemplateModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name is required"));

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                //====================
                string msg = "";
                model.ListOrgID = CurrentUser.ListOrganizationId;
                bool result = _factory.InsertOrUpdateChattingTemplate(model, ref msg);
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
                NSLog.Logger.Error("ChattingTemplateCreate_error", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ChatTemplateModels GetDetail(string id)
        {
            try
            {
                ChatTemplateModels model = _factory.GetListChattingTemplate(CurrentUser.ListOrganizationId, false, null,id).FirstOrDefault();
                if (model == null)
                    model = new ChatTemplateModels();
                return model;
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ChattingGetDetail_Error:", ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            ChatTemplateModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            ChatTemplateModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(ChatTemplateModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name is required"));
                
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //====================
                string msg = "";
                model.ListOrgID = CurrentUser.ListOrganizationId;
                var result = _factory.InsertOrUpdateChattingTemplate(model, ref msg);
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
                NSLog.Logger.Error("Chatting_Edit : ", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Delete(ChatTemplateModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteChatting(model.ID, ref msg);
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
                NSLog.Logger.Error("Chatting_Delete : ", ex);
                ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Chatting Template"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public PartialViewResult Delete(string id)
        {
            ChatTemplateModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        public ActionResult Export()
        {
            ChatTemplateModels model = new ChatTemplateModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(ChatTemplateModels model)
        {
            try
            {
                //if (model.ListType == null)
                //{
                //    ModelState.AddModelError("ListType", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose type."));
                //    return View(model);
                //}

                XLWorkbook wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Sheet1");

                StatusResponse response = _factory.Export(ref ws, CurrentUser.ListOrganizationId,model.ChatTemplateType);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Chatting Template").Replace(" ", "_")));

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
                NSLog.Logger.Error("Chatting Template Export : ", e);
                //return new HttpStatusCodeResult(400, e.Message);
                //ModelState.AddModelError("ListStores", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult Import()
        {
            ChatTemplateModels model = new ChatTemplateModels();
            model.ListMerchants = CurrentUser.ListOrganizations != null? CurrentUser.ListOrganizations.Select(ss=> new SelectListItem() {Value = ss.ID, Text = ss.Name }).ToList(): new List<SelectListItem>();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(ChatTemplateModels model)
        {
            try
            {
                if (model.MerchantsSelected == null)
                {
                    ModelState.AddModelError("MerchantsSelected", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose merchant"));
                    model.ListMerchants = CurrentUser.ListOrganizations != null ? CurrentUser.ListOrganizations.Select(ss => new SelectListItem() { Value = ss.ID, Text = ss.Name }).ToList() : new List<SelectListItem>();
                    return View(model);
                }
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    model.ListMerchants = CurrentUser.ListOrganizations != null ? CurrentUser.ListOrganizations.Select(ss => new SelectListItem() { Value = ss.ID, Text = ss.Name }).ToList() : new List<SelectListItem>();
                    return View(model);
                }

                ImportModel importModel = new ImportModel();
                string msg = "";
                StatusResponse response = _factory.Import(model.ExcelUpload, model.MerchantsSelected, ref importModel, ref msg);
                if (!response.Status)
                {
                    ModelState.AddModelError("", response.MsgError);
                    model.ListMerchants = CurrentUser.ListOrganizations != null ? CurrentUser.ListOrganizations.Select(ss => new SelectListItem() { Value = ss.ID, Text = ss.Name }).ToList() : new List<SelectListItem>();
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
                    NSLog.Logger.Info("Chatting_Import :", msg);
                    ModelState.AddModelError("ExcelUpload", msg);
                    model.ListMerchants = CurrentUser.ListOrganizations != null ? CurrentUser.ListOrganizations.Select(ss => new SelectListItem() { Value = ss.ID, Text = ss.Name }).ToList() : new List<SelectListItem>();
                    return View(model);
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("Chatting_Import_Error : ", e);
                ModelState.AddModelError("ExcelUpload", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }
    }
}