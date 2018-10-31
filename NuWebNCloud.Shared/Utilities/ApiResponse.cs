using NuWebNCloud.Shared.Models;
using RestSharp;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NuWebNCloud.Shared.Utilities
{
    public class ApiResponse
    {
       
        public static object Post<T>(string url, Dictionary<string, string> paramInfos, object paramBodys)
        {
            UserSession currentUser = null;
            if(System.Web.HttpContext.Current.Session != null)
                currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
            if (currentUser != null)
                Commons._hostApi = currentUser.HostApi;

            var client = new RestClient(Commons._hostApi + "/" + url);
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

        public static object Put<T>(string url, Dictionary<string, string> paramInfos, object paramBodys)
        {
            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
            if (currentUser != null)
                Commons._hostApi = currentUser.HostApi;
            var client = new RestClient(Commons._hostApi + "/" + url);
            var req = new RestRequest("");
            req.Method = Method.PUT;
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

        public static object Get<T>(string url, Dictionary<string, string> paramInfos, object paramBodys)
        {
            UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
            if (currentUser != null)
                Commons._hostApi = currentUser.HostApi;

            var client = new RestClient(Commons._hostApi + "/" + url);
            var req = new RestRequest("");
            req.Method = Method.GET;
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

        public static object GetWithoutHostConfig<T>(string url, Dictionary<string, string> paramInfos, object paramBodys)
        {
            var client = new RestClient(url);
            var req = new RestRequest("");
            req.Method = Method.GET;
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
        public static object GetWithoutHostConfigReturnContent<T>(string url, Dictionary<string, string> paramInfos, object paramBodys)
        {
            var client = new RestClient(url);
            var req = new RestRequest("");
            req.Method = Method.GET;
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
                return response.Content;
            }
            return null;
        }
        public static object PostWithoutHostConfig<T>(string url, Dictionary<string, string> paramInfos, object paramBodys)
        {
           
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
