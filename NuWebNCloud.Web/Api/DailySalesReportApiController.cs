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
    public class DailySalesReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertDailySalesReport(List<DailySalesReportInsertModels> lstInfo)
        {
          
            NSLog.Logger.Info("Start insert Daily sale data.......................", lstInfo);
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                DailySalesReportFactory dailySalesReportFactory = new DailySalesReportFactory();
                result.IsOk = dailySalesReportFactory.Insert(lstInfo);
            }
            return result;
        }
        [HttpPost]
        public ResultModels InsertShiftLog(List<ShiftLogModels> lstShiftLogs)
        {
            NSLog.Logger.Info("Start insert ShiftLog data.......................", lstShiftLogs);
            var result = new ResultModels();
            if (lstShiftLogs != null && lstShiftLogs.Any())
            {
                DailySalesReportFactory dailySalesReportFactory = new DailySalesReportFactory();
                result.IsOk = dailySalesReportFactory.InsertShiftLogs(lstShiftLogs);
            }
            return result;
        }
    }
}
