using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NuWebNCloud.Shared.Utilities
{
    public class ApiResponseXero
    {
        /// <summary>
        /// url = ThirdParty.ApiURL + url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="paramInfos"></param>
        /// <param name="paramBodys"></param>
        /// <returns></returns>
        public static object Post<T>(string url, Dictionary<string, string> paramInfos, object paramBodys)
        {
            //Commons.XeroURL + "/" + 
            var client = new RestClient(url);
            var req = new RestRequest("");
            req.Method = Method.POST;
            if (paramInfos != null)
            {
                foreach (var param in paramInfos)
                {
                    req.AddParameter(param.Key, param.Value);
                }
            }
            if (paramBodys != null)
            {
                req.AddJsonBody(paramBodys);
            }
            var response = client.Execute(req);
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                T responseObj = jss.Deserialize<T>(response.Content);
                return responseObj;
            }
            return null;
        }
    }
}
