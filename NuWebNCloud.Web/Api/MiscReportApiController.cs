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
    public class MiscReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertMiscReport(List<DiscountAndMiscReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert Misc data.......................");
            NSLog.Logger.Info("Start insert Misc data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                DiscountAndMiscReportFactory miscReportFactory = new DiscountAndMiscReportFactory();
                result.IsOk = miscReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
