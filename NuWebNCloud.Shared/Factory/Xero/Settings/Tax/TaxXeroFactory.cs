using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Xero.Settings.Tax;
using NuWebNCloud.Shared.Utilities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NuWebNCloud.Shared.Factory.Xero.Settings.Tax
{
    public class TaxXeroFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static BaseFactory _baseFactory = new BaseFactory();

        public List<TaxXeroModels> GetTaxXero(TaxXeroRequestModels item)
        {
            List<TaxXeroModels> listdata = new List<TaxXeroModels>();
            try
            {
                var result = (ResponseApiModels)ApiResponseXero.Post<ResponseApiModels>(item.ApiURL+"/"+ Commons.XeroApi_GetTax, null, item);
                dynamic data = result;
                if(data.Success)
                {
                    var lstZ = data.RawData;
                    var lstContent = JsonConvert.SerializeObject(lstZ);
                    listdata = JsonConvert.DeserializeObject<List<TaxXeroModels>>(lstContent);
                }

                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("GetTaxXero_GetList: " + e);
                return listdata;
            }
        }

        
    }
}
