using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngUsageManagementController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        StockUsageFactory _stockUsageFactory = new StockUsageFactory();
        UsageManagementFactory _usageManagementFactory = new UsageManagementFactory();
        // GET: IngUsageManagement
        public ActionResult Index()
        {
            UsageManagementRequest model = new UsageManagementRequest();
            model.DateFrom = DateTime.Now;
            model.DateTo = DateTime.Now;
            ViewBag.ListStore = GetListStore();
            return View(model);
        }
        [HttpPost]
        public ActionResult Search(UsageManagementRequest model)
        {
            var result = new List<UsageManagementModel>();
            try
            {
                result = _usageManagementFactory.GetUsageManagement(model);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", result);
        }
        public ActionResult LoadDetail(string usageManagementDetailId)
        {
            var result = new UsageManagementModel();
            try
            {

                List<string> lstUsageManagementDetailId = usageManagementDetailId.Split('|').ToList();
                result = _usageManagementFactory.GetUsageManagementItemDetail(lstUsageManagementDetailId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListDataDetail", result);
        }
        public ActionResult LoadDetail2(List<UsageManagementDetailModel> items)
        {
            var result = new UsageManagementModel();
            try
            {
                result.ListDetail = items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListDataDetail", result);
        }

        #region Export
        [HttpPost]
        public ActionResult Export(UsageManagementRequest model)
        {
            try
            {
                //IngredientModel model = new IngredientModel();
                XLWorkbook wb = new XLWorkbook();
                var wsdata = wb.Worksheets.Add("Usage");
                var data = _stockUsageFactory.Export(ref wsdata, model);

                if (!data.IsOk)
                {
                    ModelState.AddModelError("Usage", data.Message);
                    return View(data);
                }
                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = CommonHelper.GetExportFileName("UsageManagement").Replace(" ", "_");
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", fileName));

                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                ViewBag.IsSuccess = true;

                return View(model);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        #endregion

        [HttpPost]
        public ActionResult PushDataToXero(UsageManagementRequest model)
        {
            //var result = new List<UsageManagementModel>();
            try
            {
                var result = _usageManagementFactory.PushDataToXero(model);
                //var  result = true;
                if (result)
                {
                    //TempData["Success"] = "Added Successfully!";
                    return Json(new { success = true, responseText = "Your data successfully sent!" }, JsonRequestBehavior.AllowGet);
                    //return View(model);
                }
                //return Content("Data added successfully");
                else
                    //return Content ("Push data failed");
                    return Json(new { success = false, responseText = "Your data is push failed!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                //return new HttpStatusCodeResult(400, e.Message);
                return Json(new { success = false, responseText = "Your data is push failed!" }, JsonRequestBehavior.AllowGet);
            }
        }
        #region Xero
        #endregion
    }
}