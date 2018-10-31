using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;

namespace NuWebNCloud.Web.Api
{
    public class TimeClockReportApiMigrationController : ApiController
    {
        [HttpPost]
        public ResultModels InsertTimeClockReport(List<TimeClockReportModels> lstInfo)
        {
            NSLog.Logger.Info("Start insert [TimeClock for Migration] data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                TimeClockReportFactory timeClockReportFactory = new TimeClockReportFactory();
                result.IsOk = timeClockReportFactory.InsertForMigration(lstInfo);
            }
            return result;
        }
    }
}
