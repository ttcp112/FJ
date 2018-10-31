using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.HQForFnB
{
    public class HQForFnBAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "HQForFnB";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "HQForFnB_default",
                "HQForFnB/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 new[] { "NuWebNCloud.Web.Areas.HQForFnB.Controllers" } // Fixbug Multiple Areas Same Controllers [Home]
            );
        }
    }
}