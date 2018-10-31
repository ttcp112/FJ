using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Areas.Retailer.Controllers
{
    public class HomeController : Controller
    {
        [NuAuth]
        // GET: Retailer/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}