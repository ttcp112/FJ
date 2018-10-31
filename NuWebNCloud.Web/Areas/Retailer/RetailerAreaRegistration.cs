using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.Retailer
{
    public class RetailerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Retailer";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Retailer_default",
                "Retailer/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "NuWebNCloud.Web.Areas.Retailer.Controllers" } // Fixbug Multiple Areas Same Controllers [Home]
            );
        }
    }
}