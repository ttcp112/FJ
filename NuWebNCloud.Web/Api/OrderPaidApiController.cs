using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class OrderPaidApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertOrderPaid(List<OrderPaidModels> lstInfo)
        {
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                OrderPaidFactory OrderPaidFactory = new OrderPaidFactory();
                //result.IsOk = OrderPaidFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
