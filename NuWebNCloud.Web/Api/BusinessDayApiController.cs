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
    public class BusinessDayApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertBusinessDay(List<BusinessDayModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert BusinessDay data.......................");
            NSLog.Logger.Info("Start insert Business Day data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                BusinessDayFactory businessDayFactory = new BusinessDayFactory();
                result.IsOk = businessDayFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
