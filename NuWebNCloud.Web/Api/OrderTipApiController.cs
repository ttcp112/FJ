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
    public class OrderTipApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertOrderTip(List<OrderTipModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert OrderTip data.......................");
            NSLog.Logger.Info("Start insert Order Tip data.......................", lstInfo);
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                OrderTipFactory orderTipFactory = new OrderTipFactory();
                result.IsOk = orderTipFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
