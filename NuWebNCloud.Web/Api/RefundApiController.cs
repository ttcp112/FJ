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
    public class RefundApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertRefund(List<RefundReportDTO> lstInfo)
        {
            //_logger.Info(lstInfo);
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert refund data.......................");
            NSLog.Logger.Info("Start insert refund data.......................", lstInfo);
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                RefundFactory refundFactory = new RefundFactory();
                result.IsOk = refundFactory.Insert(lstInfo);
            }
          
            return result;
        }
    }
}
