using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Web.App_Start;
using System;
using System.Net;
using System.Web.Mvc;
using System.Linq;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SRegisterAccountController : HQController
    {
        SRegisterAccountFactory _sRegisterAccountFactory = null;
        public SRegisterAccountController()
        {
            _sRegisterAccountFactory = new SRegisterAccountFactory();
        }
        // GET: SRegisterAccount
        public ActionResult Index()
        {
            ViewBag.QRCodeText = _sRegisterAccountFactory.GetQRCode(CurrentUser.ListOrganizationId.FirstOrDefault());
            return View();
        }

        public ActionResult CreateQRCode(string DeviceName)
        {
            try
            {
               
                string msg = "";
                string AppRegisteredID = "";
                if (CurrentUser.ListOrganizationId != null && CurrentUser.ListOrganizationId.Count > 0)
                    AppRegisteredID = CurrentUser.ListOrganizationId[0];
                bool result = _sRegisterAccountFactory.CreateQRCode(AppRegisteredID, DeviceName, CurrentUser.UserId, ref msg);
                if (result)
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Content(CurrentUser.GetLanguageTextFromKey(msg));
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("CreateQRCode Error: ", ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content(ex.Message);
            }
        }
    }
}