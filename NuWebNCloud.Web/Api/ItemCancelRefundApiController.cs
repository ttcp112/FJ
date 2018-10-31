using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;

namespace NuWebNCloud.Web.Api
{
    public class ItemCancelRefundApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertItemCancelRefundsData(List<ItemizedCancelRefundModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert AuditTrailReport data.......................");
            NSLog.Logger.Info("Start insert ItemCancelRefunds data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                ItemizedCancelRefundFactory itemizedCancelRefundFactory = new ItemizedCancelRefundFactory();
                result.IsOk = itemizedCancelRefundFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
