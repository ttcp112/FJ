using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using NuWebNCloud.Shared.InitData;

namespace NuWebNCloud.Web.Api
{
    public class InitDataApiController : ApiController
    {
        [HttpGet]
        public bool InitModuleData()
        {
            var item = new InitModuleData();
            item.InsertModule();
            return true;
        }
        public InitDataApiController()
        {
           
        }
    }
}