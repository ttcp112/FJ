using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class TaxApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertTax(List<TaxModels> lstInfo)
        {
            NSLog.Logger.Info("Start insert Tax data.......................", lstInfo);

            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                TaxFactory taxFactory = new TaxFactory();
                result.IsOk = taxFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
