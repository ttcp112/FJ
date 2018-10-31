using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class ACGroupingAreasController : Controller
    {
        // GET: ACGroupingAreas
        public ActionResult Index()
        {
            return View();
        }
    }
}