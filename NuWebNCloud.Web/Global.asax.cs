using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NuWebNCloud.Web.App_Start;
using System.Threading;
using System.Globalization;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Shared;

namespace NuWebNCloud.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelHelper());
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            System.IO.Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/Uploads"));
            System.IO.Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/ExportReports"));
            Commons.ServerExportPath = Server.MapPath("~/ExportReports/");
        }
    }
}
