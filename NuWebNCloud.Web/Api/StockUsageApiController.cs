using NLog;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class StockUsageApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [HttpPost]
        public ResultModels Insert(StockUsageRequestModel info)
        {
            //_logger.Info(info);
            NSLog.Logger.Info("Start insert [Stock Usage] data.......................", info);

            var result = new ResultModels();
            if (info.ListDetails != null && info.ListDetails.Any())
            {
                StockUsageFactory stockUsageFactory = new StockUsageFactory();
                result.IsOk = stockUsageFactory.Insert(info);
            }else
            {
                //AutoCreate DataEntry
                StockCountFactory _stockCountFactory = new StockCountFactory();
                Task.Run(() => _stockCountFactory.AutoCreatedStockCount(info.CompanyId, info.StoreId, info.BusinessId, info.DateFrom, info.DateTo, null));
            }
            return result;
        }
    }
}
