using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;

namespace NuWebNCloud.Web.Api
{
    public class StoreApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertStore(List<StoreModels> lstInfo)
        {
            var result = new ResultModels();
            if (lstInfo != null )
            {
                //StoreFactory storeFactory = new StoreFactory();
                //result.IsOk = storeFactory.Insert(lstInfo);
            }
            return result;
        }
    }
}
