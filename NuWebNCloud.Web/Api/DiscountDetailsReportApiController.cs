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
    public class DiscountDetailsReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public ResultModels InsertDiscountDetailsReport(List<DiscountDetailsReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert DiscountDetails data.......................");
            NSLog.Logger.Info("Start insert Discount Details data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                DiscountDetailsReportFactory discountDetailsReportFactory = new DiscountDetailsReportFactory();
                result.IsOk = discountDetailsReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
