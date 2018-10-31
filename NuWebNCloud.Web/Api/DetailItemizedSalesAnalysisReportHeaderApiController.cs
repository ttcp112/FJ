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
    public class DetailItemizedSalesAnalysisReportHeaderApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public ResultModels InsertDetailItemizedSalesAnalysisReportHeader(List<DetailItemizedSalesAnalysisReportHeaderModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert R_DetailItemizedSalesAnalysisReportHeader data.......................");
            NSLog.Logger.Info("Start insert Detail Itemized Sales Analysis Report Header data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                DetailItemizedSalesAnalysisReportHeaderFactory detailItemizedSalesAnalysisReportHeaderFactory =
                    new DetailItemizedSalesAnalysisReportHeaderFactory();
                result.IsOk = detailItemizedSalesAnalysisReportHeaderFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
