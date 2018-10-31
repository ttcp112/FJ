using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace NuWebNCloud.Web.Api
{
    public class ApiTestController : ApiController
    {
        [HttpGet]
        public string GetString()
        {
            return "Hi guy, welcome!";
        }
        [HttpGet]
        public void Test()
        {
            StockUsageFactory _stockUsageFactory = new StockUsageFactory();
            var lstUsage = new List<UsageManagementModel>();
            lstUsage.Add(new UsageManagementModel() { Id = "37863172-ae98-4454-880d-327e1044d038", Code = "002", Usage = 0.05 });
            _stockUsageFactory.PushDataToXero(new UsageManagementRequest() { DateTo = DateTime.Now, StoreId = "4c05d27c-a42c-4038-b4f8-91b914a031c7" }, lstUsage);
            }

    }
}
