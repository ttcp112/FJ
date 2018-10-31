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
    public class RoleOnStoreFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public RoleOnStoreFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool InsertOrUpdate(List<RoleOnStoreModels> listModel, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<G_RoleOnStore> lstInsert = new List<G_RoleOnStore>();
                    List<G_RoleOnStore> lstUpdate = new List<G_RoleOnStore>();
                    List<G_RoleOnStore> lstDelete = new List<G_RoleOnStore>();

                    G_RoleOnStore item = null;
                    foreach (var model in listModel)
                    {
                        var itemExist = cxt.G_RoleOnStore.Where(x => x.Id.Equals(model.Id)).FirstOrDefault();
                        //Insert
                        if (itemExist == null)
                        {
                            if (!model.IsActive)
                                continue;
                            item = new G_RoleOnStore();
                            item.Id = Guid.NewGuid().ToString();

                            item.RoleId = model.RoleId;
                            item.StoreId = model.StoreId;
                            item.IsActive = model.IsActive;

                            item.CreatedDate = model.CreatedDate;
                            item.CreatedUser = model.CreatedUser;
                            item.ModifiedDate = model.CreatedDate;
                            item.ModifiedUser = model.ModifiedUser;

                            lstInsert.Add(item);
                        }
                        else //Update
                        {
                            //Delete
                            if (!model.IsActive)
                            {
                                lstDelete.Add(itemExist);
                            }
                            else //Update
                            {
                                item = itemExist;
                                item.Id = model.Id;
                                item.IsActive = model.IsActive;
                                item.ModifiedUser = model.ModifiedUser;
                                item.ModifiedDate = DateTime.Now;
                                lstUpdate.Add(item);
                            }

                        }
                    }
                    //Insert
                    if (lstInsert.Count > 0)
                    {
                        cxt.G_RoleOnStore.AddRange(lstInsert);
                        cxt.SaveChanges();
                    }
                    //Update
                    if (lstUpdate.Count > 0)
                    {
                        cxt.SaveChanges();
                    }
                    //Delete
                    if (lstDelete.Count > 0)
                    {
                        cxt.G_RoleOnStore.RemoveRange(lstDelete);
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

        public bool Delete(string RoleId, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.G_RoleOnStore
                                     where tb.RoleId.Equals(RoleId)
                                     select tb).ToList();
                    cxt.G_RoleOnStore.RemoveRange(lstResult);
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

        public List<RoleOnStoreModels> GetData(string RoleID, List<SelectListItem> lstStore)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from ro in cxt.G_RoleOnStore
                                     where ro.RoleId == RoleID && ro.IsActive
                                     select new RoleOnStoreModels
                                     {
                                         Id = ro.Id,
                                         RoleId = ro.RoleId,
                                         StoreId = ro.StoreId,
                                         IsActive = ro.IsActive,
                                     }).GroupBy(x => x.StoreId).Select(x => x.FirstOrDefault()).ToList();
                    lstResult.ForEach(x =>
                    {
                        //x.StoreName = lstStore.Where(z => z.Value.Equals(x.StoreId)).FirstOrDefault().Text;
                        var obj = lstStore.Where(z => z.Value.Equals(x.StoreId)).FirstOrDefault();
                        if (obj != null)
                        {
                            x.StoreName = obj.Text;
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

        public RoleOnStoreModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from ro in cxt.G_RoleOnStore
                                 where ro.Id == ID && ro.IsActive
                                 select new RoleOnStoreModels()
                                 {
                                     Id = ro.Id,
                                     RoleId = ro.RoleId,
                                     StoreId = ro.StoreId,
                                     IsActive = ro.IsActive,

                                     CreatedDate = ro.CreatedDate,
                                     CreatedUser = ro.CreatedUser,
                                     ModifiedDate = ro.ModifiedDate,
                                     ModifiedUser = ro.ModifiedUser
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

        public List<RoleOnStoreModels> GetDataRole(string StoreID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from ros in cxt.G_RoleOnStore
                                     from ro in cxt.G_RoleOrganization
                                     where ros.StoreId.Equals(StoreID) && ros.IsActive && ros.RoleId.Equals(ro.Id) && ro.IsActive
                                     select new RoleOnStoreModels
                                     {
                                         Id = ros.Id,
                                         RoleId = ros.RoleId,
                                         StoreId = ros.StoreId,
                                         RoleName = ro.Name
                                     }).GroupBy(x => x.RoleId).Select(x => x.FirstOrDefault()).ToList();
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
