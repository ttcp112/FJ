using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class PosMerchantConfigApiController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private POSMerchantConfigFactory _POSMerchantConfigFactory = null;

        public PosMerchantConfigApiController()
        {
            _POSMerchantConfigFactory = new POSMerchantConfigFactory();
        }

        //public async Task<string> Postear(string url, string postdata)
        //{
        //    HttpClientHandler handler = new HttpClientHandler();
        //    handler.UseCookies = true;
        //    handler.Proxy = WebRequest.DefaultWebProxy;

        //    HttpClient client = new HttpClient(handler as HttpMessageHandler);

        //    HttpContent content = new StringContent(postdata);
        //    content.Headers.Add("Keep-Alive", "true");
        //    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        //    HttpResponseMessage response = await client.PostAsync(url, content);

        //    Stream stream = await response.Content.ReadAsStreamAsync();

        //    return new StreamReader(stream).ReadToEnd();
        //}

        [HttpGet]
        public async Task<ResponeMerchantConfig> GetListMerchantConfig()
        {
            //_logger.Info("GetListMerchantConfig");
            NSLog.Logger.Info("Start [Get List Merchant Config] data.......................");
            return await _POSMerchantConfigFactory.GetListMerchantConfig();
        }

        [HttpPost]
        public ResponeMerchantConfig InsertPosApiMerchantConfig(MerchantConfigApiModels info)
        {
            //_logger.Info(info);
            NSLog.Logger.Info("Start [Insert Pos Api Merchant Config] data.......................", info);

            var respoone = new ResponeMerchantConfig();
            if (info != null)
            {
                respoone = _POSMerchantConfigFactory.Insert(info);
            }
            return respoone;
        }

        [HttpPost]
        public ResultModels InsertPosApiEmployeeConfig(EmployeeConfigApiModels info)
        {
            //_logger.Info(info);
            NSLog.Logger.Info("Start [Insert Pos Api Employee Config] data.......................", info);

            var result = new ResultModels();
            if (info != null)
            {
                result.IsOk = _POSMerchantConfigFactory.InsertEmployee(info);
            }
            return result;
        }

        [HttpPost]
        public ResultModels UpdatePosApiEmployeeConfig(EmployeeConfigApiModels info)
        {
            //_logger.Info(info);
            NSLog.Logger.Info("Start [Update Pos Api Employee Config from POS] data.......................", info);

            var result = new ResultModels();
            if (info != null)
            {
                result.IsOk = _POSMerchantConfigFactory.UpdateEmployee(info);
            }
            return result;
        }

        [HttpPost]
        public ResponeMerchantConfig RegisterAccountFromCSC(MerchantConfigApiModels info)
        {
            //_logger.Info(info);
            NSLog.Logger.Info("Start [RegisterAccountFromCSC] data.......................", info);

            var response = new ResponeMerchantConfig();
            if (info != null)
            {
                response = _POSMerchantConfigFactory.RegisterAccountFromCSC(info);
            }
            return response;
        }
    }
}
