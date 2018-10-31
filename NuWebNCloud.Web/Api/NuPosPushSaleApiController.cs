using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;

namespace NuWebNCloud.Web.Api
{
    public class NuPosPushSaleApiController : ApiController
    {
        [HttpPost]
        public ResultModels InsertDataSaleFromPOS(PosSaleReportReturnModels datas)
        {

            NSLog.Logger.Info("Start InsertDataSaleFromPOS (Pos Sale).......................", datas);

            var result = new ResultModels();
            if (datas != null && (datas.PosSaleReportDTOs != null && datas.PosSaleReportDTOs.Any()))
            {
                PosSaleFactory posSaleFactory = new PosSaleFactory();
                result.IsOk = posSaleFactory.Insert(datas);
            }
            return result;
        }

        [HttpPost]
        public ResultModels MergeData(NuPosPushRequest model)
        {
            var result = new ResultModels();
            if (model == null)
                model = new NuPosPushRequest();

            if (model.ListStore == null)
            {
                result.IsOk = false;
                result.Message = "[ListStore] is required";
            }  
            else
            {
                PosSaleFactory posSaleFactory = new PosSaleFactory();
                result.IsOk = posSaleFactory.MergeData(model.ListStore, model.Month, model.Year);
            }
            return result;
        }
    }


    public class NuPosPushRequest
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public List<string> ListStore { get; set; }
    }
}
