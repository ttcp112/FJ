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
    public class ItemizedSalesAnalysisReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertItemizedSalesAnalysisReport(List<ItemizedSalesAnalysisReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert ItemizedSales data.......................");
            NSLog.Logger.Info("Start insert Itemized Sales data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                ItemizedSalesAnalysisReportFactory itemizedSalesAnalysisReportFactory =
                    new ItemizedSalesAnalysisReportFactory();
                result.IsOk = itemizedSalesAnalysisReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
