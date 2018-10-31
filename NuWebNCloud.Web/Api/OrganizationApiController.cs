using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class OrganizationApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertOrganization(List<OrganizationModels> lstInfo)
        {
            var result = new ResultModels();
            if (lstInfo != null && lstInfo.Any())
            {
                //OrganizationFactory organizationFactory = new OrganizationFactory();
                //result.IsOk = organizationFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
