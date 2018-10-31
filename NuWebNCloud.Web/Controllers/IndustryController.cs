using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IndustryController : Controller
    {
        IndustryFactory _industryF;
        public IndustryController()
        {
            _industryF = new IndustryFactory();
        }

        // GET: Industry
        public ActionResult Index()
        {
            var user = System.Web.HttpContext.Current.Session["User"] as UserSession;

            //Get List Areas
            var areaNames = RouteTable.Routes.OfType<Route>()
                        .Where(d => d.DataTokens != null && d.DataTokens.ContainsKey("area") && (d.DataTokens["area"].ToString() != "HelpPage"))
                        .Select(r => r.DataTokens["area"]).ToArray();
            List<string> listArea = new List<string>();
            foreach (var item in areaNames)
            {
                listArea.Add(item.ToString());
            }
            List<IndustryModels> listIndustry = _industryF.GetData(user.ListIndustry);
            ViewBag.listA = listIndustry;
            return View();
        }

        public ActionResult GoInside(string name)
        {
            var user = System.Web.HttpContext.Current.Session["User"] as UserSession;
            user.IndustryId = name;
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}