﻿using System;
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
    public class CashInOutReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public ResultModels InsertCashInOutReport(List<CashInOutReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert CashInOut data.......................");
            NSLog.Logger.Info("Start insert Cash In Out data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                CashInOutReportFactory cashInOutReportFactory = new CashInOutReportFactory();
                result.IsOk = cashInOutReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
