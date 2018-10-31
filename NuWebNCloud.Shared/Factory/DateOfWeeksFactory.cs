using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory
{
    public class DateOfWeeksFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public DateOfWeeksFactory()
        {
            _baseFactory = new BaseFactory();
        }
        public bool Insert(List<DateOfWeeksModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<G_DateOfWeeks> lstInsert = new List<G_DateOfWeeks>();
                        G_DateOfWeeks itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new G_DateOfWeeks();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.DayNumber = item.DayNumber;
                            itemInsert.DayName = item.DayName;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.CreatedUser = item.CreatedUser;
                            itemInsert.LastUserModified = item.LastUserModified;
                            itemInsert.LastDateModified = item.LastDateModified;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.G_DateOfWeeks.AddRange(lstInsert);
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
            //_baseFactory.InsertTrackingLog("G_DateOfWeeks", jsonContent, "DateOfWeeksId", result);

            return result;
        }

        public List<DateOfWeeksModels> GetData()
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var lstData = (from d in cxt.G_DateOfWeeks
                               orderby d.DayNumber
                               select new DateOfWeeksModels
                               {
                                   DayNumber = d.DayNumber,
                                   DayName = d.DayName,
                                   CreatedDate = d.CreatedDate,
                                   CreatedUser = d.CreatedUser,
                                   Id = d.Id,
                                   LastDateModified = d.LastDateModified,
                                   LastUserModified = d.LastUserModified
                               }).ToList();
                return lstData;
            }
        }
    }
}
