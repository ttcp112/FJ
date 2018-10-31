using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.AccessControl;
using NuWebNCloud.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NuWebNCloud.Web.App_Start
{
    public class NuAuthAttribute : AuthorizeAttribute
    {
        private UserSession _CurrentUser;
        private string  _lstControllerBefore = string.Empty;

        private string ActionType;

        private string Controller;
        private string Action;

        private List<string> _Views = new List<string> { "index", "default", "view", "detail", "get", "load", "filter", "search", "apply","ingredient", "product", "modifier" };
        private List<string> _ControllerDenies = new List<string> { "ACModule", "IngIngredients" };
        private List<String> _ViewTimeoutSession = new List<string>
        {
            "LoadIngredient", "LoadIngredientIngredient",       //Ingrident
            "AddTab", "AddDishes", "CheckDish",                 //SetMenu 
            "AddModifiers","LoadModifiers","CheckModifier",      //Dish
            "LoadDetail",
            "AddSubPayment"                                 //Payment
        };

        /*Factory*/
        private UserRoleFactory _UserRoleFactory = null;
        public static List<UserModule> listUserModule = new List<UserModule>();
        public static List<string> _ListMenuUserModule = new List<string>();

        public NuAuthAttribute()
        {
            _UserRoleFactory = new UserRoleFactory();
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (HttpContext.Current.Session["User"] == null)
                _CurrentUser = new UserSession();
            else
                _CurrentUser = (UserSession)HttpContext.Current.Session["User"];

            if (_CurrentUser.UserId != null)
            {
                if (listUserModule == null)
                {
                    listUserModule = new List<UserModule>();
                }
                if (listUserModule.Count == 0)
                {
                    if (_CurrentUser.ListOrganizationId != null && _CurrentUser.ListOrganizationId.Count > 0)
                        listUserModule = _UserRoleFactory.GetModuleEmp(_CurrentUser.UserId, _CurrentUser.ListOrganizationId[0]);
                    else
                        listUserModule = _UserRoleFactory.GetModuleEmp(_CurrentUser.UserId, string.Empty);
                }
            }
            //Alias Controller //Action
            if (_CurrentUser.IsSuperAdmin)
            {
                _ListMenuUserModule = new List<string>();
                listUserModule = new List<UserModule>();
            }
            Controller = httpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            Action = httpContext.Request.RequestContext.RouteData.Values["action"].ToString().ToLower();
            if (listUserModule != null && listUserModule.Count > 0)
            {
                _ListMenuUserModule = new List<string>();
                _ListMenuUserModule = listUserModule.Select(x => x.Controller).ToList();
            }
            bool isViewAction = _Views.Any(s => Action.Contains(s));
            ActionType = isViewAction ? "View" : "Action";
            return IsPermission();
        }

        protected bool IsPermission()
        {
            try
            {
                if (!_CurrentUser.IsAuthenticated) // If user not logged in, require login
                    return false;
                else
                {
                    //if (_ControllerDenies.Contains(Controller))
                    //    return false;
                    if (_CurrentUser.IsSuperAdmin || Controller.ToLower().Equals("home") || Controller.ToLower().Equals("profile"))
                    {
                        Commons.IsAction = true;
                        return true;
                    }

                    //if (listUserModule.Count == 0)
                    //{
                    //    HttpContext.Current.Session.Remove("User");
                    //}
                    bool IsModPer = false;
                    Commons.IsShowNotAuthorized = false;
                    if (ActionType.ToLower().Equals("view"))//View
                    {

                        //IsModPer = listUserModule.Any(x => x.Controller.Equals(Controller) && x.IsActive && x.IsView);
                        IsModPer = listUserModule.Any(x => x.Controller.Equals(Controller) && x.IsView);
                        Commons.IsAction = listUserModule.Any(x => x.Controller.Equals(Controller) && x.IsAction);
                        if (Commons.IsNotAuthorized == true)
                            Commons.IsShowNotAuthorized = true;
                    }
                    else //Action
                    {
                        //IsModPer = listUserModule.Any(x => x.Controller.Equals(Controller) && x.IsActive && x.IsAction);
                        IsModPer = listUserModule.Any(x => x.Controller.Equals(Controller) && x.IsAction);
                        Commons.IsAction = IsModPer;
                        if (IsModPer == false)
                            Commons.IsShowNotAuthorized = true;
                    }
                    if (IsModPer)
                    {
                        Commons.IsNotAuthorized = false;
                        Commons.Controller = Controller;
                        if(Action == "index" || Action== "ingredient"
                            || Action == "product" || Action == "modifier")
                        Commons.Action = Action;
                    }
                    return IsModPer;
                }
            }
            catch (Exception e)
            {
                string error = e.ToString();
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //var a = _lstControllerBefore;
            if (!_CurrentUser.IsAuthenticated)
            {
                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();

                bool isChange = false;
                if (_ViewTimeoutSession.Contains(action))
                {
                    isChange = true;
                }
                if (isChange) //TimeoutSession
                {
                    filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Error",
                            action = "TimeOutSession",
                            area = string.Empty,
                        })
                    );
                }
                else //Login
                {
                    string url = filterContext.HttpContext.Request.Url.ToString().Replace("/Report", "");
                    url = filterContext.HttpContext.Request.Url.ToString().Replace("/Logout", "");

                    filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Login",
                            action = "Index",
                            area = string.Empty,
                            isAjax = filterContext.HttpContext.Request.IsAjaxRequest(),
                            //returnUrl = filterContext.HttpContext.Request.Url.ToString().Replace("/Report", "")
                            returnUrl = url
                        })
                    );
                }
            }
            else
            {
                //if (filterContext.HttpContext.Request.IsAjaxRequest())
                //{
                //    filterContext.HttpContext.Response.StatusCode = 403;
                //    Commons.IsNotAuthorized = false;
                //    filterContext.Result = new JsonResult { Data = "401Unauthorized", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //}
                //else
                //{
                //    Commons.IsNotAuthorized = true;
                //    filterContext.Result = new RedirectToRouteResult(
                //        new RouteValueDictionary(
                //            new
                //            {
                //                controller = Commons.Controller,
                //                action = Commons.Action,
                //                area = string.Empty,
                //            })
                //        );
                //}

                filterContext.Result = new RedirectToRouteResult(
                          new RouteValueDictionary(
                              new
                              {
                                  controller = "Error",
                                  action = "Unauthorised",
                                  area = string.Empty,
                              })
                          );
            }
        }
    }
}