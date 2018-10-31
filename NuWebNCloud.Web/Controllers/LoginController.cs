using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.AccessControl;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api.Language;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Linq;
using System.Configuration;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Shared.Factory.Settings;

namespace NuWebNCloud.Web.Controllers
{
    public class LoginController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private UserRoleFactory _UserRoleFactory = null;
        private LanguageFactory _LanguageFactory = null;
        private BaseFactory _baseFactory = null;
        private UserFactory _userFactory = null;

        public LoginController()
        {
            _userFactory = new UserFactory();
            _UserRoleFactory = new UserRoleFactory();
            _LanguageFactory = new LanguageFactory();
            _baseFactory = new BaseFactory();

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            ViewBag.Version = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        // GET: Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index(bool isAjax = false, string returnUrl = null)
        {
            if (Session["User"] != null)
                return RedirectToAction("Index", "Home", new { area = "" });

            LoginModel model = new LoginModel(returnUrl);
            model.ListLanguage = new List<SelectListItem>();
            model.ListLanguage = GetListLang(ref model);


            //=====Check Remember
            model.RememberMe = Commons._RememberMe;
            if (model.RememberMe)
            {
                if (!string.IsNullOrEmpty(Commons._LanguageId))
                {
                    model.LanguageId = Commons._LanguageId;
                    model.ListLanguage.ForEach(x =>
                    {
                        x.Selected = x.Value.Equals(model.LanguageId);
                    });
                }
            }

            //===========
            if (isAjax)
                //return PartialView("_Login", model);
                return RedirectToAction("Index", "Login", new { area = "" });
            else
                return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(LoginModel model, string returnUrl = null)
        {
            try
            {
                if (Session["User"] != null)
                    return RedirectToAction("Index", "Home", new { area = "" });

                if (ModelState.IsValid)
                {
                    bool isExistUser = false;
                    UserFactory factoy = new UserFactory();
                    UserSession userSession = new UserSession();
                    _userFactory.GetValueCommons(model.Email, model.Password, model.IsFormOtherLink, ref isExistUser, ref userSession);
                    if (isExistUser)
                    {
                        if (ConfigurationManager.AppSettings["MultiLocation"] != null)
                        {
                            //check location 
                            var countryCode = _baseFactory.GetCountryCode();
                            ////check with setting from db
                            if (!string.IsNullOrEmpty(userSession.CountryCode)
                                && countryCode.ToLower() != userSession.CountryCode.ToLower())
                            {
                                var isMultiLocation = bool.Parse(ConfigurationManager.AppSettings["MultiLocation"]);
                                if (isMultiLocation && !model.IsFormOtherLink)
                                {
                                    //model.UrlWebHost = "http://localhost:4040/";//"http://115.75.188.151:2020/";
                                    //userSession.UrlWebHost = "http://localhost:4040/";
                                    model.Password = CommonHelper.GetSHA512(model.Password);
                                    return Redirect(userSession.UrlWebHost + "/Login/index2?langId=" + model.LanguageId + "&email=" + model.Email + "&pwd=" + model.Password);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(userSession.WebHostUrl))
                        {
                            model.Password = CommonHelper.GetSHA512(model.Password);
                            return Redirect(userSession.WebHostUrl + "/Login/index2?langId=" + model.LanguageId + "&email=" + model.Email + "&pwd=" + model.Password);
                        }
                        UserModels User = factoy.IsCheckLogin(model.Email, model.Password, model.IsFormOtherLink);
                        bool isValid = (User != null);
                        if (isValid)
                        {
                            UserModule roles = null;
                            if (!User.IsSuperAdmin)
                            {
                                roles = _UserRoleFactory.CheckModuleForEmp(User.UserId, User.ListOrganizationId[0]);
                                if (roles == null || string.IsNullOrEmpty(roles.Controller))
                                {
                                    ModelState.AddModelError("", "Email is not authorized to login");
                                    return View(model);
                                }
                            }

                            if (User.IsSuperAdmin)
                                model.ListStoreID = User.ListStoreID;
                            else
                                model.ListStoreID = _UserRoleFactory.GetStoreEmpAccess(User.UserId);

                            //End Check Role of Account
                            //Check merchant extend here
                            userSession.ListMerchantExtends = factoy.GetListMerchantExtend(userSession.EmployeeConfigId);
                            if (userSession.ListMerchantExtends != null && userSession.ListMerchantExtends.Count > 0)
                            {
                                userSession.IsMerchantExtend = true;
                                var lstStoreExtend = (from p in userSession.ListMerchantExtends
                                                      where p.ListStoreIds.Any()
                                                      select p)
                                                        .SelectMany(p => p.ListStoreIds).ToList();

                                model.ListStoreID.AddRange(lstStoreExtend);
                            }
                            //End check merchant extend here

                            //UserSession userSession = new UserSession();
                            userSession.Email = User.Email;
                            userSession.UserName = User.Name;
                            userSession.UserId = User.UserId;
                            userSession.UserType = 0;
                            userSession.IsAuthenticated = true;
                            userSession.ImageUrl = null;
                            if (!string.IsNullOrEmpty(User.ImageURL))
                                userSession.ImageUrl = User.ImageURL;
                            userSession.IndustryId = model.IndustryId;
                            userSession.ListIndustry = User.ListIndustry;

                            userSession.ListOrganizationId = User.ListOrganizationId;
                            userSession.ListOrganizations = User.ListOrganizations;
                            userSession.IsSuperAdmin = User.IsSuperAdmin;
                            userSession.OrganizationName = User.OrganizationName;

                            userSession.listStore = User.listStore;

                            //if (User.IsSuperAdmin)
                            //    model.ListStoreID = User.ListStoreID;
                            //else
                            //    model.ListStoreID = _UserRoleFactory.GetStoreEmpAccess(User.UserId);

                            userSession.ListStoreID = model.ListStoreID;
                            if (model.ListStoreID.Count == 1)
                            {
                                userSession.StoreId = model.ListStoreID[0];
                            }
                            model.StoreName = User.StoreName;

                            userSession.IndustryId = "HQForFnB";
                            userSession.AppKey = User.AppKey;
                            userSession.AppSecret = User.AppSecret;
                            userSession.LanguageId = model.LanguageId;
                            userSession.DefaultStoreId = model.ListStoreID.FirstOrDefault();
                            /*Get List Key Language*/

                            var lstLangData = _LanguageFactory.GetLanguageDataToTrans(model.LanguageId);
                            //var a = _LanguageFactory.GetLanguageDataToTranslateForLogin(model.LanguageId);
                            //var lstLangData = _LanguageFactory.GetListLanguageForText(model.LanguageId);
                            foreach (var item in lstLangData)
                            {
                                if (!userSession.ListLanguageText.ContainsKey(item.LanguageLinkName))
                                {
                                    userSession.ListLanguageText.Add(item.LanguageLinkName, item.Text);
                                }
                            }
                            userSession.RememberMe = model.RememberMe;

                            Commons._RememberMe = model.RememberMe;
                            if (model.RememberMe)
                            {
                                Commons._LanguageId = model.LanguageId;
                            }

                            ////=====End Get Language For DateTimePicker
                            userSession.ListSetting = User.ListSetting;

                            //update 18/12/2017

                            //// end update 18/12/2017
                            //userSession.WebUrlHost = "http://api.nupos.com.sg/nuweb";

                            // Get CurrencySymbol
                            DefaultCurrencyFactory _factory = new DefaultCurrencyFactory();
                            var listCurrencies = _factory.GetListDefaultCurrency(null, null, userSession.ListOrganizationId);
                            string currency = listCurrencies.Where(w => w.IsSelected).Select(s => s.Symbol).FirstOrDefault();
                            if (!string.IsNullOrEmpty(currency))
                            {
                                userSession.CurrencySymbol = currency;
                            }
                            else
                            {
                                userSession.CurrencySymbol = "$";
                            }

                            Session.Add("User", userSession);

                            NSLog.Logger.Info("Check Return URL", returnUrl);
                            if (returnUrl == null)
                                return RedirectToAction("Index", "Home", new { area = "" });
                            else
                                return Redirect(returnUrl);

                        }
                        else
                        {
                            ModelState.AddModelError("", "Email/Password is incorrect!");
                            model.ListLanguage = GetListLang(ref model);
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email/Password is incorrect!");
                        model.ListLanguage = GetListLang(ref model);
                        return View(model);
                    }
                }
                else
                {
                    model.ListLanguage = GetListLang(ref model);
                    NSLog.Logger.Info("!ModelState.IsValid", model);
                    return View(model);// Return Error page
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("Login Error", ex);
                //_logger.Error("Login Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index2(string langId = null, string email = null, string pwd = null, bool isAjax = false)
        {
            if (Session["User"] != null)
                return RedirectToAction("Index", "Home", new { area = "" });

            LoginModel model = new LoginModel();
            model.ListLanguage = new List<SelectListItem>();
            model.ListLanguage = GetListLang(ref model);
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(pwd))
            {
                ViewBag.Email = email;
                ViewBag.Pwd = pwd;
                if (!string.IsNullOrEmpty(langId))
                {
                    ViewBag.LangId = langId;
                    model.LanguageId = langId;
                    model.ListLanguage.ForEach(x =>
                    {
                        x.Selected = x.Value.Equals(model.LanguageId);
                    });
                }

                model.Email = email;
                model.Password = pwd;
                model.IsFormOtherLink = true;
            }

            //=====Check Remember
            model.RememberMe = Commons._RememberMe;
            //if (model.RememberMe)
            //{
            //    if (!string.IsNullOrEmpty(Commons._LanguageId))
            //    {
            //        model.LanguageId = Commons._LanguageId;
            //        model.ListLanguage.ForEach(x =>
            //        {
            //            x.Selected = x.Value.Equals(model.LanguageId);
            //        });
            //    }
            //}
            //===========
            return View("Index", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult LoginFromNuPos(string storeId = null, string langId = null, string email = null, string pwd = null)
        {
            try
            {
                LoginModel model = new LoginModel();
                model.Email = email;
                model.Password = pwd;
                model.LanguageId = langId;
               

                if (Session["User"] != null)
                    return RedirectToAction("Index", "Home", new { area = "", IsFromNuPos = true });

                if (ModelState.IsValid)
                {
                    bool isExistUser = false;
                    UserFactory factoy = new UserFactory();
                    UserSession userSession = new UserSession();
                    _userFactory.GetValueCommons(email, pwd, true, ref isExistUser, ref userSession);
                    if (isExistUser)
                    {
                      
                        UserModels User = factoy.IsCheckLogin(email, pwd, true);
                        bool isValid = (User != null);
                        if (isValid)
                        {
                            UserModule roles = null;
                            if (!User.IsSuperAdmin)
                            {
                                roles = _UserRoleFactory.CheckModuleForEmp(User.UserId, User.ListOrganizationId[0]);
                                if (roles == null || string.IsNullOrEmpty(roles.Controller))
                                {
                                    ModelState.AddModelError("", "Email is not authorized to login");
                                    return View(model);
                                }
                            }

                            if (User.IsSuperAdmin)
                                model.ListStoreID = User.ListStoreID;
                            else
                                model.ListStoreID = _UserRoleFactory.GetStoreEmpAccess(User.UserId);

                            //End Check Role of Account
                            //Check merchant extend here
                            userSession.ListMerchantExtends = factoy.GetListMerchantExtend(userSession.EmployeeConfigId);
                            if (userSession.ListMerchantExtends != null && userSession.ListMerchantExtends.Count > 0)
                            {
                                userSession.IsMerchantExtend = true;
                                var lstStoreExtend = (from p in userSession.ListMerchantExtends
                                                      where p.ListStoreIds.Any()
                                                      select p)
                                                        .SelectMany(p => p.ListStoreIds).ToList();

                                model.ListStoreID.AddRange(lstStoreExtend);
                            }
                            //End check merchant extend here

                            //UserSession userSession = new UserSession();
                            userSession.Email = User.Email;
                            userSession.UserName = User.Name;
                            userSession.UserId = User.UserId;
                            userSession.UserType = 0;
                            userSession.IsAuthenticated = true;
                            userSession.ImageUrl = null;
                            if (!string.IsNullOrEmpty(User.ImageURL))
                                userSession.ImageUrl = User.ImageURL;
                            userSession.IndustryId = model.IndustryId;
                            userSession.ListIndustry = User.ListIndustry;

                            userSession.ListOrganizationId = User.ListOrganizationId;
                            userSession.IsSuperAdmin = User.IsSuperAdmin;
                            userSession.OrganizationName = User.OrganizationName;

                            userSession.listStore = User.listStore;

                            userSession.ListStoreID = model.ListStoreID;
                            if (model.ListStoreID.Count == 1)
                            {
                                userSession.StoreId = model.ListStoreID[0];
                            }
                            model.StoreName = User.StoreName;

                            userSession.IndustryId = "HQForFnB";
                            userSession.AppKey = User.AppKey;
                            userSession.AppSecret = User.AppSecret;
                            userSession.LanguageId = model.LanguageId;
                            userSession.DefaultStoreId = storeId;
                            /*Get List Key Language*/

                            var lstLangData = _LanguageFactory.GetLanguageDataToTrans(model.LanguageId);
                            foreach (var item in lstLangData)
                            {
                                if (!userSession.ListLanguageText.ContainsKey(item.LanguageLinkName))
                                {
                                    userSession.ListLanguageText.Add(item.LanguageLinkName, item.Text);
                                }
                            }
                            userSession.RememberMe = model.RememberMe;

                            Commons._RememberMe = model.RememberMe;
                            if (model.RememberMe)
                            {
                                Commons._LanguageId = model.LanguageId;
                            }

                            ////=====End Get Language For DateTimePicker
                            userSession.ListSetting = User.ListSetting;
                            userSession.IsFromNuPos = true;
                            Session.Add("User", userSession);

                            //NSLog.Logger.Info("Check Return URL", returnUrl);
                            //if (returnUrl == null)
                                return RedirectToAction("Index", "Home", new { area = "", IsFromNuPos = true });
                            //else
                            //    return Redirect(returnUrl);

                        }
                        else
                        {
                            ModelState.AddModelError("", "Email/Password is incorrect!");
                            model.ListLanguage = GetListLang(ref model);
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email/Password is incorrect!");
                        model.ListLanguage = GetListLang(ref model);
                        return View(model);
                    }
                }
                else
                {
                    model.ListLanguage = GetListLang(ref model);
                    NSLog.Logger.Info("!ModelState.IsValid", model);
                    return View(model);// Return Error page
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("Login Error", ex);
                //_logger.Error("Login Error: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult CheckSessionAvailable()
        {
            bool returnData = false;
            if (System.Web.HttpContext.Current.Session["User"] != null)
                returnData = true;
                return Json(returnData, JsonRequestBehavior.AllowGet);
        }
        public List<SelectListItem> GetListLang(ref LoginModel model)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            //List<LanguageModels> lstData = _LanguageFactory.GetListLanguage();
            List<LanguageModels> lstData = _LanguageFactory.GetLanguages();
            foreach (var item in lstData)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id,
                    Selected = item.IsDefault
                });
                if (item.IsDefault)
                {
                    model.LanguageId = item.Id;
                }
            }
            return result;
        }

        public ActionResult GetDataforLanguageID(string LanguageID)
        {
            string msg = "";
            var listData = _LanguageFactory.GetLanguageDataToTranslateForLogin(LanguageID);
            if (listData != null)
            {
                string Password = "";
                string LoginBelow = "";
                string Email = "";
                string Remember = "";
                string Version = "";
                string LoginAbove = "";
                foreach (var item in listData)
                {
                    if (item.LanguageLinkName == "Password")
                        Password = item.Text;
                    if (item.LanguageLinkName == "Log in")
                        LoginBelow = item.Text;
                    if (item.LanguageLinkName == "Email")
                        Email = item.Text;
                    if (item.LanguageLinkName == "Remember me")
                        Remember = item.Text;
                    if (item.LanguageLinkName == "Version")
                        Version = item.Text;
                    if (item.LanguageLinkName == "Login")
                        LoginAbove = item.Text;
                }

                string str = "{'Password':'" + Password + "', 'LoginBelow':'" + LoginBelow + "', 'Email':'" + Email + "', 'Remember':'" + Remember + "', 'Version':'" + Version + "', 'LoginAbove':'" + LoginAbove + "'}";
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                object obj = jsSer.Deserialize(str, typeof(object));
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            else
                return new HttpStatusCodeResult(400, msg);
        }
    }
}