using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class PaymentMenthodApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
       
        [HttpPost]
        public ResultModels InsertPaymentMenthod(List<PaymentModels> lstInfo)
        {
            NSLog.Logger.Info("Start insert order paid sale data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                OrderPaymentMethodFactory PaymentMenthodFactory = new OrderPaymentMethodFactory();
                result.IsOk = PaymentMenthodFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
