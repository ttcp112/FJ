using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Factory.AccessControl;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Factory
{
    public class UserFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private POSMerchantConfigFactory _POSMerchantConfigFactory = null;

        public UserFactory()
        {
            _baseFactory = new BaseFactory();
            _POSMerchantConfigFactory = new POSMerchantConfigFactory();
        }
        public bool Insert(List<UserModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<G_User> lstInsert = new List<G_User>();
                        G_User itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new G_User();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.Name = item.Name;
                            itemInsert.Email = item.Email;
                            itemInsert.Password = item.Password;
                            itemInsert.IsActive = item.IsActive;
                            lstInsert.Add(itemInsert);
                        }
                        cxt.G_User.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            //var jsonContent = JsonConvert.SerializeObject(lstInfo);
            //_baseFactory.InsertTrackingLog("G_User", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public UserModels IsCheckLogin(string email, string password, bool isExtendUrl)
        {
            UserModels user = null;
            try
            {
                if (!isExtendUrl)
                    password = CommonHelper.GetSHA512(password);
                EmployeeApiModels paraBody = new EmployeeApiModels();
                paraBody.Email = email;
                paraBody.Password = password;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Login, null, paraBody);

                if (result.Success)
                {
                    user = new UserModels();
                    user.Email = email;
                    user.Name = email;
                    dynamic data = result.Data;

                    //==================ListIndustry
                    var listIndustry = data["ListIndustry"];
                    user.ListIndustry = new List<string>();
                    foreach (var item in listIndustry)
                    {
                        user.ListIndustry.Add(item["ID"]);
                    }
                    //==================ListOrganization
                    var listOrganizationId = data["ListOrganization"];
                    user.ListOrganizationId = new List<string>();
                    foreach (var item in listOrganizationId)
                    {
                        user.ListOrganizationId.Add(item);
                    }
                    //==============================================
                    try
                    {
                        user.IsSuperAdmin = data["IsSuperAdmin"];
                        //==================ListOrganization
                        var listOrganizations = data["ListOrganizationDTO"];
                        foreach (var item in listOrganizations)
                        {
                            user.OrganizationName = item["Name"];
                            user.ListOrganizations.Add(new OrganizationDTO() { ID = item["ID"], Name = item["Name"] });
                        }
                    }
                    catch
                    {
                        user.IsSuperAdmin = false;
                        user.OrganizationName = "";
                    }

                    //====
                    var EmployeeID = data["EmployeeID"];
                    var EmployeeName = data["Name"];
                    var PinCode = data["PinCode"];
                    var ImageURL = data["ImageURL"];
                    user.UserId = EmployeeID;
                    user.Name = EmployeeName;
                    user.ImageURL = ImageURL;

                    var ListStoreRespone = data["ListStore"];
                    List<StoreModelsRespone> listStore = new List<StoreModelsRespone>();
                    StoreModelsRespone storeInfo = null;
                    foreach (dynamic item in ListStoreRespone)
                    {
                        storeInfo = new StoreModelsRespone();
                        storeInfo.ID = item["ID"];
                        storeInfo.Name = item["Name"];
                        storeInfo.AppKey = item["AppKey"];
                        storeInfo.AppSerect = item["AppSecret"];
                        if (CommonHelper.IsPropertyExist(item, "IsWithPoins"))
                        {
                            storeInfo.IsWithPoins = item["IsWithPoins"];
                        }
                        try
                        {
                            if (CommonHelper.IsPropertyExist(item["ThirdParty"], "StoreID"))
                            {
                                //AppRegistrationID: ThirdPartyID
                                // StoreID: -StoreID
                                //NS - XERO - INTEGRATION: Code
                                storeInfo.ThirdParty.StoreID = item["ThirdParty"]["StoreID"];
                                storeInfo.ThirdParty.ThirdPartyID = item["ThirdParty"]["ThirdPartyID"];
                                storeInfo.ThirdParty.Code = item["ThirdParty"]["Code"];
                                storeInfo.ThirdParty.ApiURL = item["ThirdParty"]["ApiURL"];
                                storeInfo.ThirdParty.IsIntegrate = item["ThirdParty"]["IsIntegrate"];
                                storeInfo.ThirdParty.Type = item["ThirdParty"]["Type"];
                                storeInfo.ThirdParty.IPAddress = item["ThirdParty"]["IPAddress"];
                                storeInfo.ThirdParty.IsIntegrate = item["ThirdParty"]["IsIntegrate"];
                            }
                        }
                        catch (Exception ex)
                        {
                            NSLog.Logger.Error("ThirdParty's not found!", ex);
                        }

                        if (CommonHelper.IsPropertyExist(item, "IsActive"))
                        {
                            if (item["IsActive"])
                            {
                                user.ListStoreID.Add(item["ID"]);
                                //=======
                                listStore.Add(storeInfo);
                            }

                        }
                        else
                        {
                            user.ListStoreID.Add(item["ID"]);
                            //=======
                            listStore.Add(storeInfo);
                        }
                    }
                    user.listStore = listStore;
                    //========
                    if (listStore != null && listStore.Count > 0)
                    {
                        //Commons.AppKey = listStore[0].AppKey.ToString();
                        //Commons.AppSecret = listStore[0].AppSerect.ToString();
                        user.AppKey = listStore[0].AppKey.ToString();
                        user.AppSecret = listStore[0].AppSerect.ToString();
                    }
                    //list setting for report
                    if (CommonHelper.IsPropertyExist(data, "ListSetting"))
                    {
                        var lstSettings = data["ListSetting"];
                        foreach (dynamic item in lstSettings)
                        {
                            user.ListSetting.Add(new GeneralSettingDTO
                            {
                                StoreID = item["StoreID"],
                                Code = item["Code"],
                                Name = item["Name"],
                                Value = item["Value"],
                            });
                        }
                    }

                }
                NSLog.Logger.Info("User login info", user);
                return user;
            }
            catch (Exception e)
            {
                //_logger.Error("Login: " + e);
                NSLog.Logger.Error("IsCheckLogin Login Error", e);
                return user;
            }
        }
        public UserModels IsCheckLoginExtend(string email, string password)
        {
            UserModels user = null;
            try
            {
                password = CommonHelper.GetSHA512(password);
                EmployeeApiModels paraBody = new EmployeeApiModels();
                paraBody.Email = email;
                paraBody.Password = password;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Login, null, paraBody);

                if (result.Success)
                {
                    user = new UserModels();
                    user.Email = email;
                    user.Name = email;
                    dynamic data = result.Data;

                    //==================ListIndustry
                    var listIndustry = data["ListIndustry"];
                    user.ListIndustry = new List<string>();
                    foreach (var item in listIndustry)
                    {
                        user.ListIndustry.Add(item["ID"]);
                    }
                    //==================ListOrganization
                    var listOrganizationId = data["ListOrganization"];
                    user.ListOrganizationId = new List<string>();
                    foreach (var item in listOrganizationId)
                    {
                        user.ListOrganizationId.Add(item);
                    }
                    //==============================================
                    try
                    {
                        user.IsSuperAdmin = data["IsSuperAdmin"];
                        //==================ListOrganization
                        var listOrganizations = data["ListOrganizationDTO"];
                        foreach (var item in listOrganizations)
                        {
                            user.OrganizationName = item["Name"];
                            user.ListOrganizations.Add(new OrganizationDTO() { ID = item["ID"], Name = item["Name"] });
                        }
                    }
                    catch
                    {
                        user.IsSuperAdmin = false;
                        user.OrganizationName = "";
                    }

                    //====
                    var EmployeeID = data["EmployeeID"];
                    var EmployeeName = data["Name"];
                    var PinCode = data["PinCode"];
                    var ImageURL = data["ImageURL"];
                    user.UserId = EmployeeID;
                    user.Name = EmployeeName;
                    user.ImageURL = ImageURL;

                    var ListStoreRespone = data["ListStore"];
                    List<StoreModelsRespone> listStore = new List<StoreModelsRespone>();
                    foreach (dynamic item in ListStoreRespone)
                    {
                        user.ListStoreID.Add(item["ID"]);
                        //=======
                        listStore.Add(new StoreModelsRespone
                        {
                            ID = item["ID"],
                            Name = item["Name"],
                            AppKey = item["AppKey"],
                            AppSerect = item["AppSecret"],
                        });
                    }
                    user.listStore = listStore;
                    //========
                    if (listStore != null && listStore.Count > 0)
                    {
                        //Commons.AppKey = listStore[0].AppKey.ToString();
                        //Commons.AppSecret = listStore[0].AppSerect.ToString();
                        user.AppKey = listStore[0].AppKey.ToString();
                        user.AppSecret = listStore[0].AppSerect.ToString();
                    }
                    //list setting for report
                    if (CommonHelper.IsPropertyExist(data, "ListSetting"))
                    {
                        var lstSettings = data["ListSetting"];
                        foreach (dynamic item in lstSettings)
                        {
                            user.ListSetting.Add(new GeneralSettingDTO
                            {
                                StoreID = item["StoreID"],
                                Code = item["Code"],
                                Name = item["Name"],
                                Value = item["Value"],
                            });
                        }
                    }

                }
                return user;
            }
            catch (Exception e)
            {
                //_logger.Error("Login: " + e);
                NSLog.Logger.Error("IsCheckLogin Login Error", e);
                return user;
            }
        }

        public List<UserModels> GetAllEmployee(List<string> ListStoreID)
        {
            var lstData = new List<UserModels>();
            try
            {
                GetAllEmployee request = new GetAllEmployee();
                request.ListStoreID = ListStoreID;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetAllEmployee, null, request);
                if (result.Success)
                {
                    dynamic data = result.Data;
                    var ListEmployeeWeb = data["ListEmployeeWeb"];
                    foreach (var item in ListEmployeeWeb)
                    {
                        lstData.Add(new UserModels
                        {
                            UserId = item["EmployeeID"],
                            Name = item["EmployeeName"],
                            StoreId = item["StoreID"],
                            StoreName = item["StoreName"]
                        });
                    }
                }
                return lstData;
            }
            catch (Exception e)
            {
                _logger.Error("GetAllEmployee: " + e);
                return lstData;
            }
        }

        public List<MerchantExtendModel> GetListMerchantExtend(string empConfigId)
        {
            List<MerchantExtendModel> lstReturn = new List<MerchantExtendModel>();
            MerchantExtendModel obj = null;
            using (NuWebContext cxt = new NuWebContext())
            {
                var queryMerchant = cxt.G_EmployeeOnMerchantExtend.Where(ww => ww.POSEmployeeConfigId == empConfigId && ww.IsActived).ToList();
                if (queryMerchant != null && queryMerchant.Any())
                {
                    var lstPOSAPIMerchanttId = queryMerchant.Select(ss => ss.POSAPIMerchantConfigId).ToList();
                    var queryMerchantConfig = cxt.G_POSAPIMerchantConfig.Where(ww => lstPOSAPIMerchanttId.Contains(ww.Id)).ToList();
                    var lstMerchantExtendId = queryMerchant.Select(ss => ss.Id).ToList();
                    //get list stores
                    var queryStores = cxt.G_EmployeeOnStoreExtend.Where(ww => lstMerchantExtendId.Contains(ww.EmpOnMerchantExtendId) && ww.IsActived).ToList();
                    foreach (var item in queryMerchant)
                    {
                        obj = new MerchantExtendModel();
                        obj.HostApiURL = queryMerchantConfig.Where(ww => ww.Id == item.POSAPIMerchantConfigId).Select(ss => ss.POSAPIUrl).FirstOrDefault();
                        obj.ListStoreIds = queryStores.Where(ww => ww.EmpOnMerchantExtendId == item.Id).Select(ss => ss.StoreExtendId).ToList();
                        if (obj.ListStoreIds != null && obj.ListStoreIds.Any())
                        {
                            lstReturn.Add(obj);
                        }
                    }
                }
            }
            return lstReturn;
        }
        public void GetValueCommons(string email, string password, bool isExtendUrl, ref bool isExist, ref UserSession userReturn)
        {
            //UserSession userReturn = new UserSession();
            var model = _POSMerchantConfigFactory.GetValueCommons(email, password, isExtendUrl);
            if (model != null)
            {
                Commons.BreakfastStart = model.BreakfastStart;
                Commons.BreakfastEnd = model.BreakfastEnd;
                Commons.LunchStart = model.LunchStart;
                Commons.LunchEnd = model.LunchEnd;
                Commons.DinnerStart = model.DinnerStart;
                Commons.DinnerEnd = model.DinnerEnd;

                Commons._ftpHost = model.FTPHost;
                Commons._userName = model.FTPUser;
                Commons._password = model.FTPPassword;
                Commons._PublicImages = string.IsNullOrEmpty(model.ImageBaseUrl) ? " " : model.ImageBaseUrl;
                Commons._hostApi = model.POSAPIUrl;

                userReturn.BreakfastStart = model.BreakfastStart;
                userReturn.BreakfastEnd = model.BreakfastEnd;
                userReturn.LunchStart = model.LunchStart;
                userReturn.LunchEnd = model.LunchEnd;
                userReturn.DinnerStart = model.DinnerStart;
                userReturn.DinnerEnd = model.DinnerEnd;
                userReturn.FTPHost = model.FTPHost;
                userReturn.FTPUser = model.FTPUser;
                userReturn.FTPPassword = model.FTPPassword;
                userReturn.PublicImages = string.IsNullOrEmpty(model.ImageBaseUrl) ? " " : model.ImageBaseUrl;
                userReturn.HostApi = model.POSAPIUrl;
                userReturn.POSInstanceVersion = model.POSInstanceVersion;
                userReturn.EmployeeConfigId = model.EmpConfigId;
                userReturn.WebHostUrl = model.WebHostUrl;

                //2017-12-26
                //get multi location settings
                using (var db = new NuWebContext())
                {
                    var multiConfig = db.G_MultiLocationConfig.Where(ww => ww.POSEmployeeConfigId == model.EmpConfigId && ww.IsActived).FirstOrDefault();
                    if (multiConfig != null)
                    {
                        userReturn.CountryCode = multiConfig.CountryCode;
                        userReturn.UrlWebHost = multiConfig.UrlWebHost;
                    }
                }

                isExist = true;
            }
        }

        public ResultModels LoginExtend(LoginModel model)
        {
            ResultModels result = new ResultModels();
            result.IsOk = true;
            UserRoleFactory _UserRoleFactory = new UserRoleFactory();
            LanguageFactory _LanguageFactory = new LanguageFactory();
            bool isExistUser = false;
            UserFactory factoy = new UserFactory();
            UserSession userSession = new UserSession();
            GetValueCommons(model.Email, model.Password, model.IsFormOtherLink, ref isExistUser, ref userSession);
            if (isExistUser)
            {
                UserModels User = factoy.IsCheckLogin(model.Email, model.Password, false);
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

                    //update 18/12/2017
                    //check location 
                    //var countryCode = _baseFactory.GetCountryCode();
                    //get setting from db
                    // end update 18/12/2017
                    //userSession.WebUrlHost = "http://api.nupos.com.sg/nuweb";

                    System.Web.HttpContext.Current.Session.Add("User", userSession);

                }
                else
                {
                    result.Message = "Email/Password is incorrect!";
                    result.IsOk = false;
                    //model.ListLanguage = _LanguageFactory.GetListLang(ref model);
                }
            }
            else
            {
                result.Message = "Email/Password is incorrect!";
                result.IsOk = false;
            }
            return result;
        }

        public ResultModels ExecuteLoginExtend(LoginModel model)
        {
            ResultModels result = new ResultModels();
            string url = model.UrlWebHost + "/" + Commons.LoginExtend;
            result = (ResultModels)ApiResponse.PostWithoutHostConfig<ResultModels>(url, null, model);
            return result;
        }
    }
}
