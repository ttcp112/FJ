using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class IndustryApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertIndustry(List<IndustryModels> lstInfo)
        {
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                IndustryFactory IndustryFactory = new IndustryFactory();
                //result.IsOk = IndustryFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
