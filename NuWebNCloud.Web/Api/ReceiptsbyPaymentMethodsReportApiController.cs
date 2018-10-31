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
    public class ReceiptsbyPaymentMethodsReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertReceiptsbyPaymentMethodsReport(List<ReceiptsbyPaymentMethodsReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert ReceiptsbyPaymentMethods data.......................");
            NSLog.Logger.Info("Start insert Receipts by Payment Methods data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                ReceiptsbyPaymentMethodsReportFactory receiptsbyPaymentMethodsReportFactory = new ReceiptsbyPaymentMethodsReportFactory();
                result.IsOk = receiptsbyPaymentMethodsReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
