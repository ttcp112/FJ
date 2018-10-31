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
    public class UserApiController : ApiController
    {
        [HttpPost]
        //public ResultModels InsertUser(List<UserModels> lstInfo)
        //{
        //    var result = new ResultModels();
        //    if (lstInfo != null && lstInfo.Any())
        //    {
        //        UserFactory userFactory = new UserFactory();
        //        result.IsOk = userFactory.Insert(lstInfo);
        //    }
        //    return result;
        //}
        public ResultModels LoginExtend(LoginModel request)
        {
            var result = new ResultModels();
            if (request != null && !string.IsNullOrEmpty(request.Email))
            {
                UserFactory userFactory = new UserFactory();
                result = userFactory.LoginExtend(request);
            }
            return result;
        }
    }
}
