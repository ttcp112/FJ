using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.AccessControl
{
    public class RoleOrganizationFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private ModulePermissionFactory _MPFactory = null;
        private RoleOnStoreFactory _RoSfactory = null;
        private List<ModulePermissionModels> ListModPer = null;

        public RoleOrganizationFactory()
        {
            _baseFactory = new BaseFactory();
            _MPFactory = new ModulePermissionFactory();
            _RoSfactory = new RoleOnStoreFactory();
            ListModPer = new List<ModulePermissionModels>();
        }

        public bool Insert(RoleOrganizationModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemExsit = cxt.G_RoleOrganization.Any(x => x.Name.ToLower().Equals(model.Name.ToLower()) && x.OrganizationId == model.OrganizationId);
                    if (itemExsit)
                    {
                        result = false;
                        msg = "Role's name [" + model.Name + "] is duplicated";
                    }
                    else
                    {
                        string RoleID = Guid.NewGuid().ToString();
                        string msgChild = "";

                        G_RoleOrganization item = new G_RoleOrganization();
                        item.Id = RoleID;
                        item.Name = model.Name;
                        item.IsActive = model.IsActive;
                        item.OrganizationId = model.OrganizationId;
                        item.CreatedDate = DateTime.Now;
                        item.CreatedUser = model.CreatedUser;
                        item.ModifiedDate = DateTime.Now;
                        item.ModifiedUser = model.ModifiedUser;
                        cxt.G_RoleOrganization.Add(item);
                        cxt.SaveChanges();

                        //===========Get List Module Permission
                        GetListModulePermission(model.ListModule, "");
                        ListModPer.ForEach(x =>
                        {
                            x.Id = "";
                            x.RoleID = RoleID;
                            x.CreatedDate = DateTime.Now;
                            x.CreatedUser = model.CreatedUser;
                            x.ModifiedDate = DateTime.Now;
                            x.ModifiedUser = model.ModifiedUser;
                            x.Status = (byte)Commons.EStatus.Actived;
                        });
                        _MPFactory.InsertOrUpdate(ListModPer, ref msgChild);
                        //=========== Get List Approval Store
                        List<RoleOnStoreModels> listRoStore = new List<RoleOnStoreModels>();
                        foreach (var roleonStore in model.ListStore)
                        {
                            listRoStore.Add(new RoleOnStoreModels
                            {
                                CreatedDate = DateTime.Now,
                                CreatedUser = model.CreatedUser,
                                ModifiedUser = model.ModifiedUser,
                                ModifiedDate = DateTime.Now,

                                Id = roleonStore.Id,
                                IsActive = roleonStore.Checked,
                                StoreId = roleonStore.StoreId,
                                RoleId = RoleID
                            });
                        }
                        _RoSfactory.InsertOrUpdate(listRoStore, ref msgChild);
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

        public bool Update(RoleOrganizationModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemExsit = cxt.G_RoleOrganization.Where(x => x.Name.ToLower().Equals(model.Name.ToLower()) && x.OrganizationId == model.OrganizationId).FirstOrDefault();
                    bool isExsit = false;
                    if (itemExsit != null)
                    {
                        if (!itemExsit.Id.Equals(model.Id))
                        {
                            result = false;
                            isExsit = true;
                            msg = "Role's name [" + model.Name + "] is duplicated";
                        }
                    }

                    if(!isExsit)
                    {
                        var itemUpdate = (from tb in cxt.G_RoleOrganization
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();
                        itemUpdate.Id = model.Id;
                        itemUpdate.Name = model.Name;
                        itemUpdate.IsActive = model.IsActive;
                        itemUpdate.ModifiedUser = model.ModifiedUser;
                        itemUpdate.ModifiedDate = DateTime.Now;
                        cxt.SaveChanges();
                        //===========Get List Module Permission
                        string RoleID = itemUpdate.Id;
                        string msgChild = "";

                        GetListModulePermission(model.ListModule, "");
                        //Update
                        var listMP = _MPFactory.GetData(RoleID);
                        ListModPer.ForEach(x =>
                        {
                            var itemExist = listMP.Where(z => z.RoleID.Equals(RoleID) && z.ModuleID.Equals(x.ModuleID)).FirstOrDefault();
                            x.Id = itemExist == null ? "" : itemExist.Id;
                            x.RoleID = RoleID;
                            x.CreatedDate = DateTime.Now;
                            x.CreatedUser = model.CreatedUser;
                            x.ModifiedDate = DateTime.Now;
                            x.ModifiedUser = model.ModifiedUser;
                            x.Status = (byte)Commons.EStatus.Actived;
                        });
                        _MPFactory.InsertOrUpdate(ListModPer, ref msgChild);

                        //=========== Get List Approval Store
                        List<RoleOnStoreModels> listRoStore = new List<RoleOnStoreModels>();
                        foreach (var roleonStore in model.ListStore)
                        {
                            listRoStore.Add(new RoleOnStoreModels
                            {
                                CreatedDate = DateTime.Now,
                                CreatedUser = model.CreatedUser,
                                ModifiedUser = model.ModifiedUser,
                                ModifiedDate = DateTime.Now,
                                Id = roleonStore.Id,
                                IsActive = roleonStore.Checked,
                                StoreId = roleonStore.StoreId,
                                RoleId = RoleID
                            });
                        }
                        _RoSfactory.InsertOrUpdate(listRoStore, ref msgChild);
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

        public bool Delete(string Id, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var isExist = (from mp in cxt.G_ModulePermission
                                   from ro in cxt.G_RoleOrganization
                                   from ros in cxt.G_RoleOnStore
                                   from ur in cxt.G_UserRole
                                   where mp.RoleID.Equals(ro.Id) && ros.RoleId.Equals(ro.Id) && ur.RoleID.Equals(ro.Id)
                                            && ro.Id.Equals(Id)
                                   select new { mp, ro, ros, ur }).FirstOrDefault();
                    if (isExist != null)
                    {
                        result = false;
                        msg = "This role is already in use and unable to be deleted.";
                    }
                    else
                    {
                        string msgChild = "";
                        _MPFactory.Delete(Id, ref msgChild);
                        _RoSfactory.Delete(Id, ref msgChild);

                        G_RoleOrganization itemDelete = (from tb in cxt.G_RoleOrganization
                                                         where tb.Id == Id
                                                         select tb).FirstOrDefault();
                        cxt.G_RoleOrganization.Remove(itemDelete);
                        cxt.SaveChanges();
                        //=========
                        
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

        public List<RoleOrganizationModels> GetData(string OrganizationId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from r in cxt.G_RoleOrganization
                                     where /*r.IsActive && */r.OrganizationId.Equals(OrganizationId)
                                     select new RoleOrganizationModels
                                     {
                                         Id = r.Id,
                                         Name = r.Name,
                                         IsActive = r.IsActive,
                                         OrganizationId = r.OrganizationId,

                                         CreatedDate = r.CreatedDate,
                                         CreatedUser = r.CreatedUser,
                                         ModifiedDate = r.ModifiedDate,
                                         ModifiedUser = r.ModifiedUser
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

        public RoleOrganizationModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from r in cxt.G_RoleOrganization
                                 where r.Id == ID
                                 select new RoleOrganizationModels()
                                 {
                                     Id = r.Id,
                                     Name = r.Name,
                                     IsActive = r.IsActive,
                                     OrganizationId = r.OrganizationId,

                                     CreatedDate = r.CreatedDate,
                                     CreatedUser = r.CreatedUser,
                                     ModifiedDate = r.ModifiedDate,
                                     ModifiedUser = r.ModifiedUser
                                 }).FirstOrDefault();
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<ModulePermissionModels> GetListModulePermission(List<ModulePermissionModels> lstModule, string ParentId)
        {
            var lst = new List<ModulePermissionModels>();
            if (lstModule != null)
            {
                var listData = lstModule.Where(x => x.ModuleParentID.Equals(ParentId)).ToList();
                foreach (var item in listData)
                {
                    var listChild = GetListModulePermission(item.ListChild, item.Id);
                    ModulePermissionModels module = new ModulePermissionModels()
                    {
                        Controller = item.Controller,
                        Id = item.Id,
                        IsAction = item.IsAction,
                        IsActive = item.IsActive,
                        IsView = item.IsView,
                        Name = item.Name,
                        ListChild = listChild,

                        ModuleID = item.ModuleID,
                        ModuleParentID = item.ModuleParentID,

                    };
                    ListModPer.Add(module);
                }
            }
            return lst;
        }

        public string ListRoleName(List<string> lstRoleId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var models = (from r in cxt.G_RoleOrganization
                                  where r.IsActive && lstRoleId.Contains(r.Id)
                                  select new RoleOrganizationModels()
                                  {
                                      Name = r.Name
                                  }).ToList();
                    string RoleName = string.Join(", ", models.Select(x => x.Name));
                    return RoleName;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return "";
                }
            }
        }

        public string ListRoleNameByEmp(string empId, string storeId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    string RoleName = string.Empty;
                    var lstRolesId = cxt.G_UserRole.Where(ww => ww.EmployeeID == empId && ww.IsActive).Select(ss => ss.RoleID).ToList();
                    if(lstRolesId != null && lstRolesId.Any())
                    {
                        var lstRolesByStoreId = cxt.G_RoleOnStore.Where(ww => ww.StoreId == storeId && lstRolesId.Contains(ww.RoleId) && ww.IsActive).Select(ss => ss.RoleId).ToList();
                        if (lstRolesByStoreId != null && lstRolesByStoreId.Any())
                        {
                            var models = (from r in cxt.G_RoleOrganization
                                          where r.IsActive && lstRolesByStoreId.Contains(r.Id)
                                          select new RoleOrganizationModels()
                                          {
                                              Name = r.Name
                                          }).ToList();
                           RoleName = string.Join(", ", models.Select(x => x.Name));
                        }
                    }
                   
                    return RoleName;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return "";
                }
            }
        }
    }
}
