using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using NuWebNCloud.Web.Controllers;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.HQForFnB.Controllers
{
    [NuAuth]
    public class SettingForDailySalesController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private POSMerchantConfigFactory _pOSMerchantConfigFactory = new POSMerchantConfigFactory();
        public ActionResult Index()
        {
            MerchantConfigApiModels model = _pOSMerchantConfigFactory.GetTimeSettingForFJDailySale(CurrentUser.HostApi);
            return View(model);
        }
        [HttpPost]
        public ActionResult Index(MerchantConfigApiModels input)
        {
            var result = _pOSMerchantConfigFactory.UpdateTimeSettingForFJDailySale(input);
            ViewBag.result = "Update Successfully!";
            return View(input);
        }

    }
}