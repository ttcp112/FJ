using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.AccessControl
{
    public class ModuleFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public ModuleFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(ModuleModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    G_Module item = new G_Module();
                    item.Id = Guid.NewGuid().ToString();

                    item.Name = model.Name;
                    item.Controller = model.Controller;
                    item.ParentID = model.ParentID == null ? "" : model.ParentID;
                    item.CreatedDate = DateTime.Now;
                    item.CreatedUser = model.CreatedUser;
                    item.ModifiedDate = DateTime.Now;
                    item.ModifiedUser = model.ModifiedUser;
                    item.IndexNum = model.IndexNum;

                    cxt.G_Module.Add(item);
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

        public bool Update(ModuleModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = (from tb in cxt.G_Module
                                      where tb.Id == model.Id
                                      select tb).FirstOrDefault();

                    itemUpdate.Id = model.Id;
                    itemUpdate.Name = model.Name;
                    itemUpdate.Controller = model.Controller;
                    itemUpdate.ParentID = model.ParentID == null ? "" : model.ParentID;
                    itemUpdate.ModifiedUser = model.ModifiedUser;
                    itemUpdate.ModifiedDate = DateTime.Now;
                    itemUpdate.IndexNum = model.IndexNum;
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

        public bool Delete(string Id, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    // 
                    var isExist = (from mp in cxt.G_ModulePermission
                                   from ro in cxt.G_RoleOrganization
                                   where mp.ModuleID.Equals(Id) && mp.RoleID.Equals(ro.Id)
                                   select new { mp, ro }).FirstOrDefault();
                    if (isExist != null)
                    {
                        result = false;
                        msg = "This module is already in use and unable to be deleted.";
                    }
                    else
                    {
                        G_Module itemDelete = (from tb in cxt.G_Module
                                               where tb.Id == Id
                                               select tb).FirstOrDefault();
                        cxt.G_Module.Remove(itemDelete);
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

        public List<ModuleModels> GetData()
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from m in cxt.G_Module
                                     orderby m.IndexNum.Value
                                     select new ModuleModels()
                                     {
                                         Id = m.Id,
                                         Name = m.Name,
                                         ParentID = m.ParentID,
                                         ParentName = "",
                                         Controller = m.Controller,
                                         IndexNum = m.IndexNum ?? 0,

                                         CreatedDate = m.CreatedDate,
                                         CreatedUser = m.CreatedUser,
                                         ModifiedDate = m.ModifiedDate,
                                         ModifiedUser = m.ModifiedUser
                                     }).ToList();
                    lstResult.ForEach(x =>
                    {
                        if (x.ParentID != null)
                        {
                            if (!x.ParentID.Equals(""))
                            {
                                var item = lstResult.Where(z => z.Id.Equals(x.ParentID)).FirstOrDefault();
                                string parentName = "";
                                if (!item.ParentID.Equals(""))
                                {
                                    parentName = " [" + lstResult.Where(z => z.Id.Equals(item.ParentID)).FirstOrDefault().Name + "]";
                                }
                                x.ParentName = item.Name + parentName;
                            }
                        }
                    });
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<ModuleModels> GetParent()
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from m in cxt.G_Module
                                     orderby m.ParentID
                                     select new ModuleModels()
                                     {
                                         Id = m.Id,
                                         Name = m.Name,
                                         IndexNum = m.IndexNum ?? 0,
                                         ParentID = m.ParentID
                                     }).ToList();

                    lstResult.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x.ParentID))
                        {
                            x.Name = "   [" + lstResult.Where(z => z.Id.Equals(x.ParentID)).FirstOrDefault().Name + "] " + x.Name;
                        }

                    });
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public ModuleModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from m in cxt.G_Module
                                 where m.Id == ID
                                 select new ModuleModels()
                                 {
                                     Id = m.Id,
                                     Name = m.Name,
                                     ParentID = m.ParentID,
                                     Controller = m.Controller,
                                     IndexNum = m.IndexNum ?? 0,
                                     CreatedDate = m.CreatedDate,
                                     CreatedUser = m.CreatedUser,
                                     ModifiedDate = m.ModifiedDate,
                                     ModifiedUser = m.ModifiedUser
                                 }).FirstOrDefault();

                    var lstResult = (from m in cxt.G_Module
                                     select new ModuleModels()
                                     {
                                         Id = m.Id,
                                         Name = m.Name,
                                         ParentID = m.ParentID,
                                         ParentName = "",
                                     }).ToList();

                    if (!model.ParentID.Equals(""))
                    {
                        var item = lstResult.Where(z => z.Id.Equals(model.ParentID)).FirstOrDefault();
                        string parentName = "";
                        if (!item.ParentID.Equals(""))
                        {
                            parentName = " [" + lstResult.Where(z => z.Id.Equals(item.ParentID)).FirstOrDefault().Name + "]";
                        }
                        model.ParentName = item.Name + parentName;
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
    }
}
