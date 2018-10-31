using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class CompanyApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertCompany(List<CompanyModels> lstInfo)
        {
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                CompanyFactory CompanyFactory = new CompanyFactory();
               // result.IsOk = CompanyFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
