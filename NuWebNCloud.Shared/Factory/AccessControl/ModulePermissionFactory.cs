using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.AccessControl
{
    public class ModulePermissionFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public ModulePermissionFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool InsertOrUpdate(List<ModulePermissionModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<G_ModulePermission> listInsert = new List<G_ModulePermission>();
                    List<G_ModulePermission> listUpdate = new List<G_ModulePermission>();
                    List<G_ModulePermission> listDelete = new List<G_ModulePermission>();

                    G_ModulePermission item = null;
                    foreach (var model in models)
                    {
                        var itemExist = cxt.G_ModulePermission.Where(x => x.Id.Equals(model.Id)).FirstOrDefault();
                        if (model.IsAction)
                        {
                            model.IsView = true;
                            model.IsActive = true;
                        }
                        //if (!model.IsAction || !model.IsView)
                        //{
                          model.IsActive = true;
                        //}
                        if (itemExist == null) //Insert
                        {
                            if (!model.IsView && !model.IsAction && !model.IsActive)
                                continue;
                            item = new G_ModulePermission();
                            item.Id = Guid.NewGuid().ToString();
                            item.RoleID = model.RoleID;
                            item.ModuleID = model.ModuleID;
                            item.ModuleParentID = model.ModuleParentID;
                            item.IsView = model.IsView;
                            item.IsAction = model.IsAction;
                            item.IsActive = model.IsActive;
                            item.CreatedDate = model.CreatedDate;
                            item.CreatedUser = model.CreatedUser;
                            item.ModifiedDate = model.ModifiedDate;
                            item.ModifiedUser = model.ModifiedUser;
                            listInsert.Add(item);
                        }
                        else //Update
                        {
                            //Delete
                            if (!model.IsView && !model.IsAction && !model.IsActive)
                            {
                                listDelete.Add(itemExist);
                            }
                            else
                            {
                                item = itemExist;
                                item.Id = model.Id;
                                item.IsView = model.IsView;
                                item.IsAction = model.IsAction;
                                item.IsActive = model.IsActive;
                                item.ModifiedUser = model.ModifiedUser;
                                item.ModifiedDate = model.ModifiedDate;
                                listUpdate.Add(item);
                            }
                        }
                    }

                    //Insert
                    if (listInsert.Count > 0)
                    {
                        cxt.G_ModulePermission.AddRange(listInsert);
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
                        cxt.G_ModulePermission.RemoveRange(listDelete);
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

        public bool Delete(string RoleID, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from mp in cxt.G_ModulePermission
                                     where mp.RoleID.Equals(RoleID)
                                     select mp).ToList();
                    cxt.G_ModulePermission.RemoveRange(lstResult);
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

        public List<ModulePermissionModels> GetData(string RoleID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from mp in cxt.G_ModulePermission
                                     where mp.RoleID.Equals(RoleID)
                                     select new ModulePermissionModels()
                                     {
                                         Id = mp.Id,
                                         RoleID = mp.RoleID,
                                         ModuleID = mp.ModuleID,
                                         IsView = mp.IsView,
                                         IsAction = mp.IsAction,
                                         IsActive = mp.IsActive,

                                         CreatedDate = mp.CreatedDate,
                                         CreatedUser = mp.CreatedUser,
                                         ModifiedDate = mp.ModifiedDate,
                                         ModifiedUser = mp.ModifiedUser
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
    }
}
