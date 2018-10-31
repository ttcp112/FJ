using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class CountryFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public CountryFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(CountryModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    I_Country item = new I_Country();
                    item.Id = Guid.NewGuid().ToString();
                    item.ShortName = model.ShortName;
                    item.FullName = model.FullName;
                    item.ZipCode = model.ZipCode;
                    item.IsActived = model.IsActived;
                    cxt.I_Country.Add(item);
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

        public bool Update(CountryModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = (from tb in cxt.I_Country
                                      where tb.Id == model.Id
                                      select tb).FirstOrDefault();

                    itemUpdate.ShortName = model.ShortName;
                    itemUpdate.FullName = model.FullName;
                    itemUpdate.ZipCode = model.ZipCode;
                    itemUpdate.IsActived = model.IsActived;

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
                    I_Country itemDelete = (from tb in cxt.I_Country
                                            where tb.Id == Id
                                            select tb).FirstOrDefault();
                    //cxt.I_Country.Remove(itemDelete);
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

        public List<CountryModels> GetData()
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Country
                                     select new CountryModels()
                                     {
                                         Id = tb.Id,
                                         ShortName = tb.ShortName,
                                         FullName = tb.FullName,
                                         ZipCode = tb.ZipCode,
                                         IsActived = tb.IsActived,
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

        public CountryModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Country
                                 where tb.Id == ID
                                 select new CountryModels()
                                 {
                                     Id = tb.Id,
                                     ShortName = tb.ShortName,
                                     FullName = tb.FullName,
                                     ZipCode = tb.ZipCode,
                                     IsActived = tb.IsActived,
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
    }
}
