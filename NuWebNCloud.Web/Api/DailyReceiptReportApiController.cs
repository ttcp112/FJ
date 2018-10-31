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
    public class DailyReceiptReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public ResultModels InsertDailyReceiptReport(List<DailyReceiptReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert DailyReceipt data.......................");
            NSLog.Logger.Info("Start insert Daily Receipt data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                DailyReceiptReportFactory dailyReceiptReportFactory = new DailyReceiptReportFactory();
                result.IsOk = dailyReceiptReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
