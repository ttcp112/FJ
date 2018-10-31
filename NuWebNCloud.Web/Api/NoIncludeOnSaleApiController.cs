using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//using NuWebNCloud.Shared.InitData;

namespace NuWebNCloud.Web.Api
{
    public class NoIncludeOnSaleApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InserNoIncludeOnSaleReport(List<NoIncludeOnSaleDataReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert NoIncludeOnSale data.......................");
            NSLog.Logger.Info("Start NoIncludeOnSale data.......................", lstInfo);
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                NoIncludeOnSaleDataFactory noIncludeOnSaleDataFactory = new NoIncludeOnSaleDataFactory();
                result.IsOk = noIncludeOnSaleDataFactory.Insert(lstInfo);
            }
            return result;
        }

    }
}
