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
    public class ItemizedSalesDetailReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertItemizedSalesDetailReport(List<ItemizedSalesAnalysisReportDetailModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert ItemizedSalesDetail data.......................");
            NSLog.Logger.Info("Start insert Itemized Sales Detail data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                ItemizedSalesAnalysisReportDetailFactory itemizedSalesDetailReportFactory =
                    new ItemizedSalesAnalysisReportDetailFactory();
                result.IsOk = itemizedSalesDetailReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
