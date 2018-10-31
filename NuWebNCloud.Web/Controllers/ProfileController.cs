using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Sandbox;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class ProfileController : HQController
    {
        private EmployeeFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ProfileController()
        {
            _factory = new EmployeeFactory();

        }
        // GET: Profile
        public ActionResult Index()
        {
            if (Session["User"] == null)
                return RedirectToAction("Index", "Home");
            UserModels user = new UserModels();

            return View(CurrentUser);
        }

        public EmployeeModels GetDetail(string id)
        {
            try
            {
                EmployeeModels model = _factory.GetListEmployee(null, id)[0];
                model.HiredDate = model.HiredDate.ToLocalTime();
                model.BirthDate = model.BirthDate.ToLocalTime();
                //==========
                RoleWorkingTime rWorkingTime = null;
                int OffSet = 0;
                if (model.ListEmpStore != null)
                {
                    foreach (var emp in model.ListEmpStore)
                    {
                        rWorkingTime = new RoleWorkingTime();
                        rWorkingTime.ListWorkingTime.Clear();
                        //if (model.ListEmpStore != null)
                        //{
                        var empOnStore = model.ListEmpStore.Where(x => x.StoreID.Equals(emp.StoreID)).FirstOrDefault();
                        if (empOnStore != null)
                        {
                            model.StoreID = empOnStore.StoreID;
                            model.StoreName = empOnStore.StoreName;

                            model.RoleID = empOnStore.RoleID;
                            model.RoleName = empOnStore.RoleName;

                            //===========
                            rWorkingTime.OffSet = OffSet;
                            rWorkingTime.Status = (int)Commons.EStatus.Actived;

                            rWorkingTime.RoleID = empOnStore.RoleID;
                            rWorkingTime.RoleName = empOnStore.RoleName;

                            rWorkingTime.StoreID = empOnStore.StoreID;
                            rWorkingTime.StoreName = empOnStore.StoreName;

                            rWorkingTime.GetListRole(rWorkingTime.StoreID, CurrentUser.ListOrganizationId);
                            //===========
                        }
                        //============
                        var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                        model.ListStore = lstStore == null ? new List<SelectListItem>() : lstStore;
                        foreach (var item in model.ListStore)
                        {
                            item.Disabled = model.ListEmpStore.Where(x => x.StoreID.Equals(item.Value)).FirstOrDefault() == null ? false : true;
                            item.Selected = model.ListEmpStore.Where(x => x.StoreID.Equals(item.Value)).FirstOrDefault() == null ? false : true;  //item.Value.Equals(StoreId);
                        }
                        //}
                        if (model.ListWorkingTime != null)
                        {

                            foreach (var itemStore in model.ListWorkingTime)
                            {
                                if (string.IsNullOrEmpty(itemStore.StoreID))
                                {
                                    itemStore.StoreID = emp.StoreID;
                                }
                            }
                            //=========
                            //model.ListWorkingTime = model.ListWorkingTime.Where(x => x.StoreID.Equals(emp.StoreID)).OrderBy(x => x.Day).ToList();
                            var lstWorkingTime = model.ListWorkingTime.Where(x => x.StoreID.Equals(emp.StoreID)).OrderBy(x => x.Day).ToList();
                            for (int i = 0; i < lstWorkingTime.Count; i++)
                            {
                                var item = lstWorkingTime[i];
                                if (item.IsOffline)
                                {
                                    item.From = "OFF";
                                    item.To = "OFF";
                                }
                                else
                                {
                                    item.From = item.FromTime.ToLocalTime().ToString("HH:mm");
                                    item.To = item.ToTime.ToLocalTime().ToString("HH:mm");
                                }
                                switch (item.Day)
                                {
                                    case 2:
                                        item.StrDate = "Mon";
                                        break;
                                    case 3:
                                        item.StrDate = "Tue";
                                        break;
                                    case 4:
                                        item.StrDate = "Wed";
                                        break;
                                    case 5:
                                        item.StrDate = "Thu";
                                        break;
                                    case 6:
                                        item.StrDate = "Fri";
                                        break;
                                    case 7:
                                        item.StrDate = "Sat";
                                        break;
                                    case 8:
                                        item.StrDate = "Sun";
                                        break;
                                }
                                rWorkingTime.ListWorkingTime.Add(item);
                            }
                        }
                        model.ListRoleWorkingTime.Add(rWorkingTime);
                        OffSet++;
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Employee_Detail: " + ex);
                return null;
            }
        }

        public ActionResult ChangePassword()
        {
            EmployeeModels model = GetDetail(CurrentUser.UserId);
            return View(model);
        }

        [HttpPost]
        public ActionResult ChangePassword(EmployeeModels model)
        {
            string _newPassword = "", _oldPassword = "", _comfirmPassword = "";
            try
            {
                _newPassword = model.NewPassword;
                _oldPassword = model.OldPassword;
                _comfirmPassword = model.ConfirmPassword;
                if (string.IsNullOrEmpty(model.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", CurrentUser.GetLanguageTextFromKey("Current Password field is required"));
                }

                if (string.IsNullOrEmpty(model.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", CurrentUser.GetLanguageTextFromKey("New Password field is required"));
                }

                if (string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", CurrentUser.GetLanguageTextFromKey("Confirm New Password field is required"));
                }

                if (!model.NewPassword.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", CurrentUser.GetLanguageTextFromKey("Confirm New Password and new password incorrect"));
                }

                if (!ModelState.IsValid)
                {
                    model = GetDetail(model.ID);
                    model.OldPassword = _oldPassword;
                    model.NewPassword = _newPassword;
                    model.ConfirmPassword = _comfirmPassword;
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View(model);
                }

                string msg = "";
                string mec = CurrentUser.ListOrganizationId[0];
                var result = _factory.ChangePassword(model, ref msg, mec);
                if (result)
                {
                    return RedirectToAction("Logout", "Home", new { area = "" });
                }
                else
                {
                    ModelState.AddModelError("ConfirmPassword", msg);
                    model = GetDetail(model.ID);
                    model.OldPassword = _oldPassword;
                    model.NewPassword = _newPassword;
                    model.ConfirmPassword = _comfirmPassword;
                    /*===*/
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Employee_Change Password: " + ex);
                ModelState.AddModelError("ConfirmPassword", CurrentUser.GetLanguageTextFromKey(ex.Message));
                model = GetDetail(model.ID);
                model.OldPassword = _oldPassword;
                model.NewPassword = _newPassword;
                model.ConfirmPassword = _comfirmPassword;
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return View(model);
            }
        }
    }
}