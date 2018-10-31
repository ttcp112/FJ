using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class NoSaleDetailReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertNoSaleDetailReport(List<NoSaleDetailReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert NoSale data.......................");
            NSLog.Logger.Info("Start insert No Sale data.......................", lstInfo);
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                NoSaleDetailReportFactory noSaleDetailReportFactory = new NoSaleDetailReportFactory();
                result.IsOk = noSaleDetailReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
