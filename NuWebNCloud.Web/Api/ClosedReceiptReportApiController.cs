using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;

namespace NuWebNCloud.Web.Api
{
    public class ClosedReceiptReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertClosedReceiptReport(List<ClosedReceiptReportModels> lstInfo)
        {
            //_logger.Info(lstInfo);
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert ClosedReceipt data.......................");
            NSLog.Logger.Info("Start insert Closed Receipt data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                ClosedReceiptReportFactory closedReceiptReportFactory = new ClosedReceiptReportFactory();
                result.IsOk = closedReceiptReportFactory.Insert(lstInfo);
            }
            ////Check delete tracking log
            //BaseFactory baseFactory = new BaseFactory();
            //baseFactory.DeleteTrackingLog();
            return result;
        }
    }
}
