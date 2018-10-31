using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NLog;

namespace NuWebNCloud.Web.Api
{
    public class HourlySalesReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertHourlySalesReport(List<HourlySalesReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert HourlySales data.......................");
            NSLog.Logger.Info("Start insert Hourly Sales data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                HourlySalesReportFactory hourlySalesReportFactory = new HourlySalesReportFactory();
                result.IsOk = hourlySalesReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
