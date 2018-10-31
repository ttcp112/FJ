using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Factory.AccessControl;
using NuWebNCloud.Shared;

namespace NuWebNCloud.Web.Api
{
    public class UtilitiesApiController : ApiController
    {
        [HttpPost]
        public bool ClearDataSaleForReport(ClearDataSaleReportModels request)
        {
            NSLog.Logger.Info("ClearDataSaleForReport Request", request);
            UtilitiesFactory utilitiesFactory = new UtilitiesFactory();
            var result = utilitiesFactory.DelDataSalereport(request);
            NSLog.Logger.Info("ClearDataSaleForReport Result", result);

            return result.IsOk;
        }

        [HttpPost]
        public ResultModels CreateOrUpdateMerchantExtend(MerchantExtendRequestModel request)
        {
            NSLog.Logger.Info("CreateOrUpdateMerchantExtend Request", request);
            UtilitiesFactory utilitiesFactory = new UtilitiesFactory();
            var result = utilitiesFactory.CreateOrUpdateMerchantExtend(request);
            NSLog.Logger.Info("CreateOrUpdateMerchantExtend Result", result);

            return result;
        }

        [HttpGet]
        public ResultModels LoginOnDevice(string email, string pwd, string langId, bool isNuPos)
        {
            NSLog.Logger.Info("LoginOnDevice Start:", "Email: " + email
                                                + "Password: " + pwd
                                                + "LangId: " + langId
                                                + "IsNuPos: " + isNuPos);


            ResultModels result = new ResultModels();

            UserFactory factoy = new UserFactory();
            UserRoleFactory _UserRoleFactory = new UserRoleFactory();
            LanguageFactory _LanguageFactory = new LanguageFactory();

            //=========
            LoginModel model = new LoginModel();
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
                        result.Message = "Email is not authorized to login";
                        NSLog.Logger.Info("LoginOnDevice Result:", result);
                        return result;
                    }
                }

                if (User.IsSuperAdmin)
                    model.ListStoreID = User.ListStoreID;
                else
                    model.ListStoreID = _UserRoleFactory.GetStoreEmpAccess(User.UserId);

                //End Check Role of Account
                //Check merchant extend here
                UserSession userSession = new UserSession();
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
                userSession.IndustryId = "";// model.IndustryId;
                userSession.ListIndustry = User.ListIndustry;

                userSession.ListOrganizationId = User.ListOrganizationId;
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
                userSession.LanguageId = langId;// model.LanguageId;
                /*Get List Key Language*/

                var lstLangData = _LanguageFactory.GetLanguageDataToTrans(langId/*model.LanguageId*/);
                //var a = _LanguageFactory.GetLanguageDataToTranslateForLogin(model.LanguageId);
                //var lstLangData = _LanguageFactory.GetListLanguageForText(model.LanguageId);
                foreach (var item in lstLangData)
                {
                    if (!userSession.ListLanguageText.ContainsKey(item.LanguageLinkName))
                    {
                        userSession.ListLanguageText.Add(item.LanguageLinkName, item.Text);
                    }
                }
                userSession.RememberMe = true;// model.RememberMe;
                Commons._RememberMe = true;// model.RememberMe;
                Commons._LanguageId = langId;// model.LanguageId;

                ////=====End Get Language For DateTimePicker
                userSession.ListSetting = User.ListSetting;

                //update 18/12/2017

                //// end update 18/12/2017
                //userSession.WebUrlHost = "http://api.nupos.com.sg/nuweb";

                System.Web.HttpContext.Current.Session.Add("User", userSession);
                //NSLog.Logger.Info("Check Return URL", returnUrl);
                //if (returnUrl == null)
                //    return RedirectToAction("Index", "Home", new { area = "" });
                //else
                //    return Redirect(returnUrl);

                result.IsOk = true;
                result.Message = "OK";
                NSLog.Logger.Info("LoginOnDevice Result:", result);
            }
            return result;
        }

        [HttpGet]
        public ResultModels IsDirty()
        {
            NSLog.Logger.Info("IsDirty Request");
            UtilitiesFactory utilitiesFactory = new UtilitiesFactory();
            var result = utilitiesFactory.IsDirty();
            NSLog.Logger.Info("IsDirty Result", result);

            return result;
        }
    }
}
