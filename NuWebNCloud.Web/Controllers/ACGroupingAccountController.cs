using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.AccessControl;
using NuWebNCloud.Shared.Factory.Sandbox;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.AccessControl;
using NuWebNCloud.Shared.Models.Sandbox;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class ACGroupingAccountController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private RoleOrganizationFactory _ROfactory = null;
        private RoleOnStoreFactory _RoSfactory = null;
        private ModulePermissionFactory _MPfactory = null;
        private UserRoleFactory _UserRoleFactory = null;
        private EmployeeFactory _Empfactory = null;
        List<SelectListItem> lstStore = new List<SelectListItem>();
        List<CompanyItem> lstCompany = new List<CompanyItem>();

        //G_Role
        public ACGroupingAccountController()
        {
            _ROfactory = new RoleOrganizationFactory();
            _RoSfactory = new RoleOnStoreFactory();
            _MPfactory = new ModulePermissionFactory();
            _UserRoleFactory = new UserRoleFactory();
            _Empfactory = new EmployeeFactory();
            if (!string.IsNullOrEmpty(CurrentUser.OrganizationName))
            {
                List<SelectListItem> listOrganization = new List<SelectListItem>();
                listOrganization.Add(new SelectListItem()
                {
                    Value = CurrentUser.ListOrganizationId[0].ToString(),
                    Text = CurrentUser.OrganizationName
                });
                ViewBag.ListOrganization = listOrganization;
            }
            ViewBag.ListStore = GetListStore();
            var ListCompanyStore = GetListStoreRole();
            lstStore = ViewBag.ListStore;
            //=================
            var listCompany = ListCompanyStore.GroupBy(x => x.CompanyId).Select(x => x.FirstOrDefault());
            foreach (var item in listCompany)
            {
                var listStore = ListCompanyStore.Where(x => x.CompanyId.Equals(item.CompanyId)).ToList();
                CompanyItem company = new CompanyItem();
                company.CompanyId = item.CompanyId;
                company.CompanyName = item.CompanyName;
                foreach (var itemStore in listStore)
                {
                    company.ListStore.Add(new StoreItem
                    {
                        StoreId = itemStore.Id,
                        StoreName = itemStore.Name
                    });
                }
                lstCompany.Add(company);
            }
        }

        public ActionResult Index()
        {
            try
            {
                RoleOrganizationViewModels model = new RoleOrganizationViewModels();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("G_Role_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(RoleOrganizationViewModels model)
        {
            try
            {
                var data = _ROfactory.GetData(CurrentUser.ListOrganizationId[0]);
                foreach (var item in data)
                {
                    item.OrganizationName = CurrentUser.OrganizationName;
                    var listRoS = _RoSfactory.GetData(item.Id, lstStore);
                    if (listRoS != null)
                    {
                        listRoS = listRoS.Where(o => !string.IsNullOrEmpty(o.StoreName)).OrderBy(o => o.StoreName).ToList();
                        item.RO = string.Join("<br/>", listRoS.Where(z => z.IsActive).Select(x => x.StoreName).ToList());
                    }
                    else
                    {
                        item.RO = "None";
                    }
                }
                model.ListItem = data;
            }
            catch (Exception e)
            {
                _logger.Error("G_Role_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            RoleOrganizationModels model = new RoleOrganizationModels();
            model.OrganizationId = CurrentUser.ListOrganizationId[0];
            //model.GetListStore(lstStore);
            model.GetModule();
            model.ListCompany = lstCompany;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(RoleOrganizationModels model)
        {
            try
            {
                int countUnSelect = model.ListCompany
                             .Select(x => x.ListStore.Count(z => z.Checked == false))
                             .Sum();

                int countTotalStore = model.ListCompany
                             .Select(x => x.ListStore.Count())
                             .Sum();
                //var countUnSelect = model.ListStore.Count(x => x.Checked == false);
                if (countUnSelect == countTotalStore/*model.ListStore.Count*/)
                {
                    ModelState.AddModelError("ListStore", CurrentUser.GetLanguageTextFromKey("Please choose at least Store"));
                }
                if (!ModelState.IsValid)
                {
                    model.GetModule();
                    return View(model);
                }
                foreach (var item in model.ListCompany)
                    model.ListStore.AddRange(item.ListStore.Where(x => x.Checked || !string.IsNullOrEmpty(x.Id)).ToList());
                //====================
                string msg = "";
                model.CreatedUser = CurrentUser.UserName;
                model.ModifiedUser = CurrentUser.UserName;
                model.ListModule.ForEach(x =>
                {
                    x.IsActive = true;
                    if (string.IsNullOrEmpty(x.ModuleParentID))
                    {
                        x.ModuleParentID = "";
                    }
                    //=======
                    if (x.Name.ToLower().Equals("reports"))
                    {
                        x.IsAction = x.IsView;
                        if (x.ListChild != null && x.ListChild.Count > 0)
                        {
                            x.ListChild.ForEach(z =>
                            {
                                z.IsAction = z.IsView;
                                z.IsActive = z.IsView;
                            });
                        }
                    }
                });
                bool result = _ROfactory.Insert(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("G_Role_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public RoleOrganizationModels GetDetail(string id)
        {
            try
            {
                RoleOrganizationModels model = _ROfactory.GetDetail(id);
                model.OrganizationName = CurrentUser.OrganizationName;
                model.GetModule();

                var listMP = _MPfactory.GetData(id);
                var modelListModule = model.ListModule;
                SetModulePermisson(ref modelListModule, listMP);
                //======
                model.ListCompany = lstCompany;
                //model.GetListStore(lstStore);
                var listRO = _RoSfactory.GetData(id, lstStore);
                if (listRO != null)
                {
                    model.ListCompany.ForEach(z =>
                    {
                        z.ListStore.ForEach(x =>
                        {
                            var RO = listRO.Where(o => o.StoreId.Equals(x.StoreId)).FirstOrDefault();
                            x.Checked = RO == null ? false : RO.IsActive;
                            x.Id = RO == null ? "" : RO.Id;
                        });
                        //==============checked for Parent(Company)
                        int countStore = z.ListStore.Count();
                        int countStoreCheck = z.ListStore.Count(x => x.Checked);
                        z.Checked = countStore == countStoreCheck;
                    });
                    //model.ListStore.ForEach(x =>
                    //{
                    //    var RO = listRO.Where(z => z.StoreId.Equals(x.StoreId)).FirstOrDefault();
                    //    x.Checked = RO == null ? false : RO.IsActive;
                    //    x.Id = RO == null ? "" : RO.Id;
                    //});
                    //===================
                    model.RO = string.Join("<br/>", listRO.Where(z => z.IsActive).Select(x => x.StoreName).ToList());
                }
                //model.ListStore = model.ListStore.OrderByDescending(x => x.Checked).ToList();
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("G_Role_Detail: " + ex);
                return null;
            }
        }

        public void SetModulePermisson(ref List<ModulePermissionModels> ListModule, List<ModulePermissionModels> listMP)
        {
            if (ListModule != null)
            {
                var listData = ListModule.ToList();
                foreach (var item in listData)
                {
                    var modelListModule = item.ListChild;
                    SetModulePermisson(ref modelListModule, listMP);
                    var modPer = listMP.Where(z => z.ModuleID.Equals(item.ModuleID)).FirstOrDefault();
                    if (modPer != null)
                    {
                        item.Id = modPer.ModuleID;
                        item.IsAction = modPer.IsAction;
                        item.IsActive = modPer.IsActive;
                        item.IsView = modPer.IsView;
                    }
                }
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            RoleOrganizationModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            RoleOrganizationModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(RoleOrganizationModels model)
        {
            try
            {
                //if (string.IsNullOrEmpty(model.Name))
                //    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Role Name is required"));

                //==========
                //var countUnSelect = model.ListStore.Count(x => x.Checked == false);
                //if (countUnSelect == model.ListStore.Count)
                //{
                //    ModelState.AddModelError("ListStore", CurrentUser.GetLanguageTextFromKey("Please choose at least Store"));
                //}

                int countUnSelect = model.ListCompany
                             .Select(x => x.ListStore.Count(z => z.Checked == false))
                             .Sum();

                int countTotalStore = model.ListCompany
                             .Select(x => x.ListStore.Count())
                             .Sum();
                //var countUnSelect = model.ListStore.Count(x => x.Checked == false);
                if (countUnSelect == countTotalStore/*model.ListStore.Count*/)
                {
                    ModelState.AddModelError("ListStore", CurrentUser.GetLanguageTextFromKey("Please choose at least Store"));
                }

                if (!ModelState.IsValid)
                {
                    model = GetDetail(model.Id);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                foreach (var item in model.ListCompany)
                    model.ListStore.AddRange(item.ListStore.Where(x => x.Checked || !string.IsNullOrEmpty(x.Id)).ToList());

                model.ModifiedUser = CurrentUser.UserName;
                model.CreatedUser = CurrentUser.UserName;
                model.ListModule.ForEach(x =>
                {
                    x.IsActive = true;
                    if (string.IsNullOrEmpty(x.ModuleParentID))
                    {
                        x.ModuleParentID = "";
                    }
                    //=======
                    if (x.Name.ToLower().Equals("reports"))
                    {
                        x.IsAction = x.IsView;
                        if (x.ListChild != null && x.ListChild.Count > 0)
                        {
                            x.ListChild.ForEach(z =>
                            {
                                z.IsAction = z.IsView;
                                z.IsActive = z.IsView;
                            });
                        }
                    }
                });
                string msg = "";
                bool result = _ROfactory.Update(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    model = GetDetail(model.Id);
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("G_Role_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            RoleOrganizationModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(RoleOrganizationModels model)
        {
            try
            {
                string msg = "";
                var result = _ROfactory.Delete(model.Id, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("G_Role_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Have an error when you delete a role"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        //UserRole
        public ActionResult ViewAccounts()
        {
            try
            {
                EmployeeViewModels model = new EmployeeViewModels();
                ///GetListStoreGroup();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("ViewAccounts_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult SearchUser(EmployeeViewModels model)
        {
            try
            {
                var data = _Empfactory.GetListEmployee(model.StoreID, null, CurrentUser.ListOrganizationId);
                List<EmployeeModels> lstNews = new List<EmployeeModels>();
                foreach (var item in data)
                {
                    item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
                    //if (item.ListEmpStore != null && item.ListEmpStore.Any())
                    for (int i = 0; i < item.ListEmpStore.Count; i++)
                    {
                        if (i == 0)
                        {
                            item.StoreName = item.ListEmpStore[i].StoreName;
                            item.StoreID = item.ListEmpStore[i].StoreID;
                            //============
                            //var lstRoleId = _UserRoleFactory.GetDataEmployee(item.ID).Select(x => x.RoleID).ToList();
                            item.RoleName = _ROfactory.ListRoleNameByEmp(item.ID, item.StoreID);
                        }
                        else
                        {
                            var newItem = new EmployeeModels();
                            newItem.ID = item.ID;
                            newItem.Name = item.Name;
                            newItem.Email = item.Email;
                            newItem.RoleID = item.RoleName;
                            newItem.ImageData = item.ImageData;
                            newItem.ImageURL = item.ImageURL;
                            newItem.StoreName = item.ListEmpStore[i].StoreName;
                            newItem.StoreID = item.ListEmpStore[i].StoreID;
                            //var lstRoleId = _UserRoleFactory.GetDataEmployee(item.ID).Select(x => x.RoleID).ToList();
                            newItem.RoleName = _ROfactory.ListRoleNameByEmp(newItem.ID, newItem.StoreID);

                            lstNews.Add(newItem);
                        }
                    }
                }
                data.AddRange(lstNews);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    data = data.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();
                model.ListItem = data;
            }
            catch (Exception e)
            {
                _logger.Error("Account_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListDataUser", model);
        }

        public List<SelectListItem> GetSelectListRoles(string StoreID)
        {
            List<RoleOnStoreModels> lstData = _RoSfactory.GetDataRole(StoreID);
            List<SelectListItem> slcRole = new List<SelectListItem>();
            if (lstData != null)
            {
                foreach (var role in lstData)
                {
                    slcRole.Add(new SelectListItem
                    {
                        Text = role.RoleName,
                        Value = role.RoleId
                    });
                }
            }
            return slcRole;
        }

        public EmployeeModels GetDetailEmployee(string storeId, string id)
        {
            EmployeeModels model = new EmployeeModels();
            var lstTmp = _Empfactory.GetListEmployee(storeId, id);
            if (lstTmp != null && lstTmp.Any())
            {
                model = lstTmp[0];
                if (model.ListEmpStore != null)
                {
                    model.StoreID = model.ListEmpStore[0].StoreID;
                    model.StoreName = model.ListEmpStore[0].StoreName;
                }
            }
            return model;
        }

        public ActionResult ApprovalRole(string StoreId, string id)
        {
            var emp = GetDetailEmployee(StoreId, id);
            UserRoleModels model = new UserRoleModels();
            model.EmployeeID = emp.ID;
            model.EmployeeEmail = emp.Email;
            model.EmployeeName = emp.Name;
            model.StoreName = emp.StoreName;

            var lstRole = GetSelectListRoles(StoreId);
            foreach (var item in lstRole)
            {
                model.ListRole.Add(new RoleItem
                {
                    RoleId = item.Value,
                    RoleName = item.Text
                });
            }
            //======
            var listROEmp = _UserRoleFactory.GetDataEmployee(id);
            model.ListRole.ForEach(x =>
            {
                var UR = listROEmp.Where(z => z.RoleID.Equals(x.RoleId)).FirstOrDefault();
                x.Checked = UR == null ? false : UR.IsActive;
                x.Id = UR == null ? "" : UR.Id;
            });
            model.ListRole = model.ListRole.OrderByDescending(x => x.Checked).ToList();
            return PartialView("_ApprovalRole", model);
        }

        [HttpPost]
        public ActionResult ApprovalRole(UserRoleModels model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_ApprovalRole", model);
                }
                List<UserRoleModels> listModel = new List<UserRoleModels>();
                foreach (var item in model.ListRole)
                {
                    listModel.Add(new UserRoleModels
                    {
                        Id = item == null ? "" : item.Id,
                        RoleID = item.RoleId,
                        EmployeeID = model.EmployeeID,
                        IsActive = item.Checked,

                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        CreatedUser = CurrentUser.UserName,
                        ModifiedUser = CurrentUser.UserName,
                    });
                }
                string msg = "";
                bool result = _UserRoleFactory.InsertOrUpdate(listModel, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("ListRole", msg);
                    return PartialView("_ApprovalRole", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Approval_Role: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}