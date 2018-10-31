using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Factory.AccessControl
{
    public class UserRoleFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public UserRoleFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(UserRoleModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    G_UserRole item = new G_UserRole();
                    item.Id = Guid.NewGuid().ToString();
                    item.RoleID = model.RoleID;
                    item.EmployeeID = model.EmployeeID;
                    item.IsActive = model.IsActive;
                    //======
                    item.CreatedDate = DateTime.Now;
                    item.CreatedUser = model.CreatedUser;
                    item.ModifiedDate = DateTime.Now;
                    item.ModifiedUser = model.ModifiedUser;

                    cxt.G_UserRole.Add(item);
                    cxt.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        public bool InsertOrUpdate(List<UserRoleModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<G_UserRole> listInsert = new List<G_UserRole>();
                    List<G_UserRole> listUpdate = new List<G_UserRole>();
                    List<G_UserRole> listDelete = new List<G_UserRole>();
                    G_UserRole item = null;
                    foreach (var model in models)
                    {
                        var itemExsit = cxt.G_UserRole.Where(x => x.Id.Equals(model.Id)).FirstOrDefault();
                        if (itemExsit == null) //Insert
                        {
                            if (!model.IsActive)
                                continue;
                            item = new G_UserRole();
                            item.Id = Guid.NewGuid().ToString();
                            item.RoleID = model.RoleID;
                            item.EmployeeID = model.EmployeeID;
                            item.IsActive = model.IsActive;
                            //======
                            item.CreatedDate = model.CreatedDate;
                            item.CreatedUser = model.CreatedUser;
                            item.ModifiedDate = model.ModifiedDate;
                            item.ModifiedUser = model.ModifiedUser;
                            listInsert.Add(item);
                        }
                        else
                        {
                            //Delete
                            if (!model.IsActive)
                                listDelete.Add(itemExsit);
                            else //Update
                            {
                                item = itemExsit;
                                item.Id = itemExsit.Id;

                                item.ModifiedUser = model.ModifiedUser;
                                item.ModifiedDate = model.ModifiedDate;
                                listUpdate.Add(item);
                            }
                        }
                    }

                    //Insert
                    if (listInsert.Count > 0)
                    {
                        cxt.G_UserRole.AddRange(listInsert);
                        cxt.SaveChanges();
                    }
                    //Update
                    if (listUpdate.Count > 0)
                    {
                        cxt.SaveChanges();
                    }
                    //Delete
                    if (listDelete.Count > 0)
                    {
                        cxt.G_UserRole.RemoveRange(listDelete);
                        cxt.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        //public bool Update(UserRoleModels model, ref string msg)
        //{
        //    bool result = true;
        //    using (NuWebContext cxt = new NuWebContext())
        //    {
        //        try
        //        {
        //            var itemUpdate = (from tb in cxt.G_UserRole
        //                              where tb.Id == model.Id
        //                              select tb).FirstOrDefault();

        //            itemUpdate.Id = model.Id;
        //            itemUpdate.RoleID = model.RoleID;
        //            itemUpdate.EmployeeID = model.EmployeeID;
        //            itemUpdate.IsActive = model.IsActive;

        //            itemUpdate.ModifiedUser = model.ModifiedUser;
        //            itemUpdate.ModifiedDate = DateTime.Now;

        //            cxt.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error(ex);
        //            result = false;
        //        }
        //        finally
        //        {
        //            if (cxt != null)
        //                cxt.Dispose();
        //        }
        //    }
        //    return result;
        //}

        //public bool Update(List<UserRoleModels> listModel, ref string msg)
        //{
        //    bool result = true;
        //    using (NuWebContext cxt = new NuWebContext())
        //    {
        //        try
        //        {
        //            List<UserRoleModels> lstUpdate = new List<UserRoleModels>();
        //            foreach (var model in listModel)
        //            {
        //                var itemUpdate = (from tb in cxt.G_UserRole
        //                                  where tb.Id == model.Id
        //                                  select tb).FirstOrDefault();

        //                itemUpdate.Id = model.Id;
        //                itemUpdate.IsActive = model.IsActive;

        //                itemUpdate.ModifiedUser = model.ModifiedUser;
        //                itemUpdate.ModifiedDate = DateTime.Now;
        //            }
        //            cxt.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error(ex);
        //            result = false;
        //        }
        //        finally
        //        {
        //            if (cxt != null)
        //                cxt.Dispose();
        //        }
        //    }
        //    return result;
        //}

        public List<UserRoleModels> GetDataEmployee(string EmpID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from ur in cxt.G_UserRole
                                     where ur.EmployeeID.Equals(EmpID) && ur.IsActive
                                     select new UserRoleModels
                                     {
                                         Id = ur.Id,
                                         EmployeeID = ur.EmployeeID,
                                         RoleID = ur.RoleID,
                                         IsActive = ur.IsActive,

                                         CreatedDate = ur.CreatedDate,
                                         CreatedUser = ur.CreatedUser,
                                         ModifiedDate = ur.ModifiedDate,
                                         ModifiedUser = ur.ModifiedUser
                                     }).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<UserRoleModels> GetDataEmployeeWithStore(string EmpID, string storeId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from ur in cxt.G_UserRole
                                     where ur.EmployeeID.Equals(EmpID) && ur.IsActive
                                     select new UserRoleModels
                                     {
                                         Id = ur.Id,
                                         EmployeeID = ur.EmployeeID,
                                         RoleID = ur.RoleID,
                                         IsActive = ur.IsActive,

                                         CreatedDate = ur.CreatedDate,
                                         CreatedUser = ur.CreatedUser,
                                         ModifiedDate = ur.ModifiedDate,
                                         ModifiedUser = ur.ModifiedUser
                                     }).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }


        public UserRoleModels GetDetail(string ID, List<SelectListItem> lstStore)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from ur in cxt.G_UserRole
                                 where ur.Id == ID
                                 select new UserRoleModels()
                                 {
                                     Id = ur.Id,
                                     EmployeeID = ur.EmployeeID,
                                     RoleID = ur.RoleID,
                                     IsActive = ur.IsActive,

                                     CreatedDate = ur.CreatedDate,
                                     CreatedUser = ur.CreatedUser,
                                     ModifiedDate = ur.ModifiedDate,
                                     ModifiedUser = ur.ModifiedUser
                                 }).FirstOrDefault();
                    var item = lstStore.Where(z => z.Value.Equals(model.StoreId)).FirstOrDefault();
                    if (item != null)
                    {
                        model.StoreName = item.Text;
                    }
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<UserModule> GetModuleEmp(string EmpID, string OrganizationId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from ur in cxt.G_UserRole
                                     from mp in cxt.G_ModulePermission
                                     from m in cxt.G_Module
                                     from ro in cxt.G_RoleOrganization
                                     where ur.EmployeeID.Equals(EmpID) && (mp.IsView || mp.IsAction) &&
                                           ur.RoleID.Equals(mp.RoleID) && mp.ModuleID.Equals(m.Id)
                                           && ur.RoleID.Equals(ro.Id) && ro.OrganizationId.Equals(OrganizationId)
                                           && ro.IsActive
                                     select new UserModule
                                     {
                                         ModuleName = m.Name,
                                         Controller = m.Controller,
                                         RoleId = ro.Id,
                                         IsAction = mp.IsAction,
                                         IsActive = mp.IsActive,
                                         IsView = mp.IsView
                                     }).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }
        public UserModule CheckModuleForEmp(string EmpID, string OrganizationId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from ur in cxt.G_UserRole
                                     from mp in cxt.G_ModulePermission
                                     from m in cxt.G_Module
                                     from ro in cxt.G_RoleOrganization
                                     where ur.EmployeeID.Equals(EmpID) && (mp.IsView || mp.IsAction) &&
                                           ur.RoleID.Equals(mp.RoleID) && mp.ModuleID.Equals(m.Id)
                                           && ur.RoleID.Equals(ro.Id) && ro.OrganizationId.Equals(OrganizationId)
                                           && ro.IsActive
                                     select new UserModule
                                     {
                                         ModuleName = m.Name,
                                         Controller = m.Controller,
                                         RoleId = ro.Id,
                                         IsAction = mp.IsAction,
                                         IsActive = mp.IsActive,
                                         IsView = mp.IsView
                                     }).FirstOrDefault();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<string> GetStoreEmpAccess(string EmpID, string OrganizationId = null)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstStoreId = new List<string>();
                    var lstRoleIds = cxt.G_UserRole.Where(ww => ww.EmployeeID == EmpID && ww.IsActive).Select(ss => ss.RoleID).ToList();
                    if(lstRoleIds != null && lstRoleIds.Any())
                    {
                        lstStoreId = cxt.G_RoleOnStore.Where(ww => lstRoleIds.Contains(ww.RoleId) && ww.IsActive).Select(ss => ss.StoreId).ToList();
                    }
                    return lstStoreId;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return new List<string>();
                }
            }
        }

    }

    public class UserModule
    {
        public string ModuleName { get; set; }
       
        public string Controller { get; set; }
        public string StoreId { get; set; }
        public bool IsView { get; set; }
        public bool IsAction { get; set; }
        public bool IsActive { get; set; }
        public string RoleId { get; set; }

    }
}
