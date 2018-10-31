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
    public class TopSellingProductsReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertTopSellingProductsReport(List<TopSellingProductsReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            NSLog.Logger.Info("Start insert Top Selling data.......................", lstInfo);
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                TopSellingProductsReportFactory topSellingProductsReportFactory = new TopSellingProductsReportFactory();
                result.IsOk = topSellingProductsReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
