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

    public class HourlyItemizedSalesReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertHourlyItemizedSalesReport(List<HourlyItemizedSalesReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert HourlyItemizedSales data.......................");
            NSLog.Logger.Info("Start insert Hourly Itemized Sales data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                HourlyItemizedSalesReportFactory hourlyItemizedSalesReportFactory = new HourlyItemizedSalesReportFactory();
                result.IsOk = hourlyItemizedSalesReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
