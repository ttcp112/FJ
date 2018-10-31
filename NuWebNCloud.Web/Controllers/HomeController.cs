using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Charts;
using NuWebNCloud.Shared.Models.Charts;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using System.Threading.Tasks;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class HomeController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private LanguageFactory _LanguageFactory = null;
        private DashBoardFactory _dashBoardFactory = null;

        public HomeController()
        {
            _LanguageFactory = new LanguageFactory();
            _dashBoardFactory = new DashBoardFactory();
        }

        //public List<SelectListItem> GetListLang()
        //{
        //    List<SelectListItem> ListLang = new List<SelectListItem>();
        //    var data = _LanguageFactory.GetListLanguage();
        //    data.ForEach(x =>
        //    {
        //        ListLang.Add(new SelectListItem
        //        {
        //            Text = x.Name,
        //            Value = x.Id
        //        });
        //    });
        //    return ListLang;
        //}

        //public List<SelectListItem> GetListLangFromApi()
        //{
        //    List<SelectListItem> ListLang = new List<SelectListItem>();
        //    var data = _LanguageFactory.GetLanguages();
        //    data.ForEach(x =>
        //    {
        //        ListLang.Add(new SelectListItem
        //        {
        //            Text = x.Name,
        //            Value = x.Id
        //        });
        //    });
        //    return ListLang;
        //}

        public ActionResult Index(bool IsFromNuPos = false)
        {
            try
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~\\Uploads"));
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("CreateDirectory Error", ex);
            }
            DashBoardModels model = new DashBoardModels();
            model.ListStoreIds.Add(CurrentUser.DefaultStoreId);

            //Updated 04022018, for set css menu toggle
            //CurrentUser.IsFromNuPos = IsFromNuPos;
            ViewBag.IsFromNuPos = CurrentUser.IsFromNuPos;

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                if (Session["User"] == null)
                    return RedirectToAction("Login", new { area = "" });

                FormsAuthentication.SignOut();
                Session.Remove("User");
                NuAuthAttribute.listUserModule.Clear();
                Session.Remove("GetListStore_View");
                Session.Remove("GetListStore_View_V1");
                Session.Remove("ListStoresInfo"); // Updated 07172018, for reports

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.Error("Logout Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        /*Import Language*/
        public ActionResult Import()
        {
            SandboxImportModel model = new SandboxImportModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(SandboxImportModel model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", "Please choose language.");
                    return View(model);
                }
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ListStores", "Excel filename cannot be null");
                    return View(model);
                }

                ImportModel importModel = new ImportModel();
                string msg = "";
                // read excel file
                if (model.ExcelUpload != null && model.ExcelUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ExcelUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);

                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ExcelUpload.SaveAs(filePath);

                    importModel = _LanguageFactory.Import(filePath, model.ListStores[0], "Admin", ref msg);
                    //delete file excel after insert to database
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                if (msg.Equals(""))
                {
                    return View("ImportDetail", importModel);
                }
                else
                {
                    _logger.Error("Language_Import: " + msg);
                    ModelState.AddModelError("ListStores", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Language_Import: " + e);
                ModelState.AddModelError("ListStores", "Import file have error.");
                return View(model);
            }
        }

        #region For Home Charts
        #region Revenue
        [HttpPost]
        public async Task<JsonResult> RevenueWeekChart(BaseChartRequestModels request)
        {
            List<object> returnData = new List<object>();
            List<string> lstLable = new List<string>();
            List<int> lstTC = new List<int>();
            List<double> lstReceiptTotal = new List<double>();

            if (request.DateFrom != null)
            {
                request.DateFrom = new DateTime(request.DateFrom.Year, request.DateFrom.Month, request.DateFrom.Day, 0, 0, 0);
            }

            if (request.DateTo != null)
            {
                request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);
            }

            // Get data for report
            var data = await _dashBoardFactory.GetRevenueWeekReportAsync(request).ConfigureAwait(false);

            lstLable = data.Select(ss => ss.Date).ToList();
            lstTC = data.Select(ss => ss.Receipt.TC).ToList();
            lstReceiptTotal = data.Select(ss => ss.Receipt.ReceiptTotal).ToList();

            returnData.Add(lstLable);
            returnData.Add(lstTC);
            returnData.Add(lstReceiptTotal);

            //Source data returned as JSON  
            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> RevenueMonthChart(BaseChartRequestModels request)
        {
            List<object> returnData = new List<object>();
            List<string> lstLable = new List<string>();
            List<int> lstTC = new List<int>();
            List<double> lstReceiptTotal = new List<double>();

            if (request.DateFrom != null)
            {
                request.DateFrom = new DateTime(request.DateFrom.Year, request.DateFrom.Month, request.DateFrom.Day, 0, 0, 0);
            }

            if (request.DateTo != null)
            {
                request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);
            }
            // Get data for report
            var data = await _dashBoardFactory.GetRevenueMonthReportAsync(request).ConfigureAwait(false);

            lstLable = data.Select(ss => ss.Date).ToList();
            lstTC = data.Select(ss => ss.Receipt.TC).ToList();
            lstReceiptTotal = data.Select(ss => ss.Receipt.ReceiptTotal).ToList();

            returnData.Add(lstLable);
            returnData.Add(lstTC);
            returnData.Add(lstReceiptTotal);

            //Source data returned as JSON  
            return Json(returnData, JsonRequestBehavior.AllowGet);
        }
        #endregion Revenue

        #region Hourly Sales report
        [HttpPost]
        public async Task<JsonResult> HourlySaleChart(HourlySaleChartRequestModels request)
        {
            HourlySaleChartResponseModels returnData = new HourlySaleChartResponseModels();

            if (request.DateFrom != null)
            {
                request.DateFrom = new DateTime(request.DateFrom.Year, request.DateFrom.Month, request.DateFrom.Day, 0, 0, 0);
            }

            if (request.DateTo != null)
            {
                request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);
            }

            // Get datetime depend on business day
            returnData = await _dashBoardFactory.GetHourlySaleChartReportAsync(request).ConfigureAwait(false);

            //returnData.ListTC = returnData.ListTC.Distinct().ToList();
            return Json(returnData, JsonRequestBehavior.AllowGet);
        }
        #endregion Hourly Sales report

        #region Categories report[HttpPost]
        public async Task<JsonResult> CategoryChart(CategoryChartRequestModels request)
        {
            CategoryChartResponseModels returnData = new CategoryChartResponseModels();

            if (request.Type == 1)//month
            {
                if (request.DateMonthFrom != null)
                {
                    request.DateFrom = new DateTime(request.DateMonthFrom.Year, request.DateMonthFrom.Month, request.DateMonthFrom.Day, 0, 0, 0);
                }

                if (request.DateMonthTo != null)
                {
                    request.DateTo = new DateTime(request.DateMonthTo.Year, request.DateMonthTo.Month, request.DateMonthTo.Day, 23, 59, 59);
                }
            }
            else
            {

                if (request.DateFrom != null)
                {
                    request.DateFrom = new DateTime(request.DateFrom.Year, request.DateFrom.Month, request.DateFrom.Day, 0, 0, 0);
                }

                if (request.DateTo != null)
                {
                    request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);
                }
            }
            returnData = await _dashBoardFactory.GetCategoryChartReportAsync(request).ConfigureAwait(false);

            // Report new DB
            //returnData = _dashBoardFactory.GetCategoryChartReport_NewDB(request);

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }
        #endregion Categories report

        #region Top Selling report
        [HttpPost]
        public async Task<JsonResult> TopSellingChart(TopSellingChartRequestModels request)
        {
            TopSellingChartReponseModels returnData = new TopSellingChartReponseModels();

            if (request.DateFrom != null)
            {
                request.DateFrom = new DateTime(request.DateFrom.Year, request.DateFrom.Month, request.DateFrom.Day, 0, 0, 0);
            }

            if (request.DateTo != null)
            {
                request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);
            }

            returnData = await _dashBoardFactory.GetTopSellingChartReportAsync(request).ConfigureAwait(false);

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }
        #endregion Top Selling report
        #endregion For Home Charts
    }
}