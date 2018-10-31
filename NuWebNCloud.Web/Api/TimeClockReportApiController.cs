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
    public class TimeClockReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public ResultModels InsertTimeClockReport(List<TimeClockReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert TimeClock data.......................");
            NSLog.Logger.Info("Start insert Time Clock data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                TimeClockReportFactory timeClockReportFactory = new TimeClockReportFactory();
                result.IsOk = timeClockReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
