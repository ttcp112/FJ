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
    public class AuditTrailReportApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels InsertAuditTrailReport(List<AuditTrailReportModels> lstInfo)
        {
            //_logger.Info("==========================================================");
            //_logger.Info("Start insert AuditTrailReport data.......................");
            NSLog.Logger.Info("Start insert Audit Trail data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                AuditTrailReportFactory auditTrailReportFactory = new AuditTrailReportFactory();
                result.IsOk = auditTrailReportFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
