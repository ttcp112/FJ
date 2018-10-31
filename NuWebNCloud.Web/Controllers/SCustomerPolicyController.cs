using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SCustomerPolicyController : Controller
    {
        // GET: SCustomerPolicy
        public ActionResult Index()
        {
            return View();
        }
    }
}