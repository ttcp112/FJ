using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngStockManagementController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private StockManagementFactory _factory = null;

        public IngStockManagementController()
        {
            _factory = new StockManagementFactory();
            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                StockManagementViewModels model = new StockManagementViewModels();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("ReceiptNote_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(StockManagementViewModels model)
        {
            try
            {
                if(model.ListStore == null)
                {
                    model.ListStore = new List<string>();
                    var stores = GetListStore();
                    if(stores != null && stores.Count > 0)
                    {
                        model.ListStore = stores.Select(ss => ss.Value).ToList();
                    }

                }
                var datas = _factory.GetData(model.ListStore);
                List<SelectListItem> vbStore = ViewBag.ListStore;
                foreach (var item in datas)
                {
                    item.StoreName = vbStore.Where(x => x.Value.Equals(item.StoreId)).FirstOrDefault().Text;
                }
                model.ListItem = datas;
            }
            catch (Exception e)
            {
                _logger.Error("StockManagement_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Export()
        {
            StockManagementViewModels model = new StockManagementViewModels();
            return View(model);
        }


        [HttpPost]
        public ActionResult Export(StockManagementViewModels model)
        {
            try
            {
                if (model.ListStore == null || model.ListStore.Count ==0)
                {
                    ModelState.AddModelError("ListStore", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please choose store")+ ".");
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Sheet1");

                List<SelectListItem> vbStore = ViewBag.ListStore;
                StatusResponse response = _factory.Export(ref ws, model.ListStore, vbStore);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("StockManagement").Replace(" ", "_")));

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
                _logger.Error("StockManagement_Export: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }
    }
}