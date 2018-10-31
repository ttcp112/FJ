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
    public class DailyDetailItemSaleReportApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertDailyDetailItemSaleReport(List<DailyItemizedSalesReportDetailPushDataModels> lstInfo)
        {

            NSLog.Logger.Info("Start insert daily item sale detail data.......................", lstInfo);
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                DailyItemizedSalesReportDetailFactory dailyItemSalesReportFactory = new DailyItemizedSalesReportDetailFactory();
                result.IsOk = dailyItemSalesReportFactory.Insert(lstInfo);
            }
            return result;
        }
      
    }
}
