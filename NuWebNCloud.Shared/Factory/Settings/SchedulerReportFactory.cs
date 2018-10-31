using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Settings;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class SchedulerReportFactory
    {
        private ResultModels StatusResponse { get; set; }
        private List<ResultModels> ListStatusResponses { get; set; }
        public SchedulerReportFactory()
        {
            StatusResponse = new ResultModels();
            ListStatusResponses = new List<ResultModels>();
        }

        public List<SettingSchedulerTaskModels> GetData()
        {
            using (var cxt = new NuWebContext())
            {
                var listData = cxt.G_ScheduleTask.Join(cxt.G_ScheduleTaskOnStore, o => o.Id, s => s.ScheduleTaskId, (o, s) => new { o, s }).Select(item => new SettingSchedulerTaskModels
                {
                    ID = item.o.Id,
                    ReportId = item.o.ReportId,
                    Email = item.o.Email,
                    Cc = item.o.Cc,
                    Bcc = item.o.Bcc,
                    Hour = item.o.Hour,
                    Minute = item.o.Minute,
                    Enabled = item.o.Enabled,
                    IsDaily = item.o.IsDaily,
                    IsMonth = item.o.IsMonthly,

                    EmailSubject = item.o.EmailSubject,
                    //DayOfWeeks = item.DayOfWeeks,
                    CreatedDate = item.o.CreatedDate,
                    CreatedUser = item.o.CreatedUser,
                    StoreId = item.s.StoreId,
                    Description = item.s.Description,
                    //StoreName = item.StoreName
                }).ToList();
                return listData;
            }
        }

        public ResultModels CreateSchedulerTask(SettingSchedulerTaskModels model, string _report)
        {
            using (var cxt = new NuWebContext())
            {
                try
                {
                    var schedulerTask = cxt.G_ScheduleTask.FirstOrDefault(x => x.ReportId == _report);
                    var schedulerTaskOnStore = cxt.G_ScheduleTaskOnStore.FirstOrDefault(x => x.StoreId == model.StoreId && x.ScheduleTaskId == model.ID);
                    if (schedulerTask != null && schedulerTaskOnStore != null) //Update
                    {
                        using (var dbContextTransaction = cxt.Database.BeginTransaction())
                        {
                            try
                            {
                                schedulerTask.Id = schedulerTask.Id;
                                schedulerTask.ReportId = schedulerTask.ReportId;
                                schedulerTask.EmailSubject = model.EmailSubject;
                                schedulerTask.Email = model.Email;
                                schedulerTask.Cc = model.Cc;
                                schedulerTask.Bcc = model.Bcc;
                                //schedulerTask.DayOfWeeks = model.DayOfWeeks;
                                schedulerTask.Hour = model.Hour;
                                schedulerTask.Minute = model.Minute;
                                schedulerTask.Enabled = model.Enabled;
                                schedulerTask.IsDaily = model.IsDaily;
                                //schedulerTask.IsMonth = model.IsMonth;

                                schedulerTask.LastDateModified = DateTime.Now;

                                //schedulerTask.StoreId = model.StoreId;
                                //for schedule on store
                                schedulerTaskOnStore.Id = schedulerTaskOnStore.Id;
                                schedulerTaskOnStore.ScheduleTaskId = schedulerTask.Id;
                                schedulerTaskOnStore.StoreId = schedulerTaskOnStore.StoreId;
                                schedulerTaskOnStore.Description = model.Description;
                                schedulerTaskOnStore.CreatedDate = schedulerTaskOnStore.CreatedDate;
                                schedulerTaskOnStore.CreatedUser = "SA";
                                schedulerTaskOnStore.LastUserModified = "SA";
                                schedulerTaskOnStore.LastDateModified = DateTime.Now;

                                cxt.SaveChanges();
                                dbContextTransaction.Commit();
                            }
                            catch (Exception)
                            {
                                dbContextTransaction.Rollback();
                            }
                        }
                    }
                    else // Insert
                    {
                        using (var dbContextTransaction = cxt.Database.BeginTransaction())
                        {
                            try
                            {
                                G_ScheduleTask itemInsert = new G_ScheduleTask();
                                itemInsert.Id = Guid.NewGuid().ToString();
                                itemInsert.CreatedDate = DateTime.Now;
                                itemInsert.CreatedUser = "SA";
                                itemInsert.LastDateModified = DateTime.Now;
                                //itemInsert.LastSuccessUtc = DateTime.Now;
                                itemInsert.LastUserModified = "SA";
                                itemInsert.EmailSubject = model.EmailSubject;

                                itemInsert.ReportId = _report;
                                itemInsert.Email = model.Email;
                                itemInsert.Cc = model.Cc;
                                itemInsert.Bcc = model.Bcc;
                                itemInsert.IsDaily = model.IsDaily;
                                //itemInsert.IsMonth = model.IsMonth;
                                itemInsert.Hour = model.Hour;
                                itemInsert.Minute = model.Minute;
                                itemInsert.Enabled = model.Enabled;

                                cxt.G_ScheduleTask.Add(itemInsert);

                                //for Schedule On Store
                                G_ScheduleTaskOnStore itemInsertOnStore = new G_ScheduleTaskOnStore();
                                itemInsertOnStore.Id = Guid.NewGuid().ToString();
                                itemInsertOnStore.ScheduleTaskId = itemInsert.Id;
                                itemInsertOnStore.StoreId = model.StoreId;
                                itemInsertOnStore.Description = model.Description;
                                itemInsertOnStore.CreatedDate = DateTime.Now;
                                itemInsertOnStore.CreatedUser = "SA";
                                itemInsertOnStore.LastUserModified = "SA";
                                itemInsertOnStore.LastDateModified = DateTime.Now;
                                cxt.G_ScheduleTaskOnStore.Add(itemInsertOnStore);
                                cxt.SaveChanges();
                                dbContextTransaction.Commit();
                            }
                            catch (Exception)
                            {
                                dbContextTransaction.Rollback();
                            }
                        }
                    }
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        string error = string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            string error2 = string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    //throw;
                }
            }
            return StatusResponse;
        }

        public SettingSchedulerTaskModels FindById(string Id, string StoreId)
        {
            using (var cxt = new NuWebContext())
            {
                SettingSchedulerTaskModels model = new SettingSchedulerTaskModels();
                model = cxt.G_ScheduleTask.Join(cxt.G_ScheduleTaskOnStore, o =>o.Id, s=>s.ScheduleTaskId, (o,s)=> new{ o,s}).Where(item => item.o.Id == Id && item.s.StoreId == StoreId).Select(item => new SettingSchedulerTaskModels
                {
                    ID = item.o.Id,
                    ReportId = item.o.ReportId,
                    Email = item.o.Email,
                    Cc = item.o.Cc,
                    Bcc = item.o.Bcc,
                    Hour = item.o.Hour,
                    Minute = item.o.Minute,
                    Enabled = item.o.Enabled,
                    IsDaily = item.o.IsDaily,
                    IsMonth = item.o.IsMonthly,
                    CreatedDate = item.o.CreatedDate,
                    CreatedUser = item.o.CreatedUser,
                    //DayOfWeeks = item.DayOfWeeks,
                    EmailSubject = item.o.EmailSubject,
                    LastDateModified = item.o.LastDateModified,
                    //LastSuccessUtc = item.o.LastSuccessUtc,
                    LastUserModified = item.o.LastUserModified,
                    StoreId = item.s.StoreId,
                    Description = item.s.Description,
                    //StoreName = item.StoreName
                }).FirstOrDefault();
                //model.GetDateOfWeek();
                return model;
            }
        }

        public ResultModels UpdateCurrentSchedulerTask(SettingSchedulerTaskModels model)
        {
            try
            {
                using (var cxt = new NuWebContext())
                {
                    using (var dbContextTransaction = cxt.Database.BeginTransaction())
                    {
                        try
                        {
                            var schedulerTask = cxt.G_ScheduleTask.FirstOrDefault(x => x.Id == model.ID && x.ReportId == model.ReportId);
                            schedulerTask.EmailSubject = model.EmailSubject;
                            schedulerTask.Email = model.Email;
                            schedulerTask.Cc = model.Cc;
                            schedulerTask.Bcc = model.Bcc;
                            schedulerTask.Hour = model.Hour;
                            schedulerTask.Minute = model.Minute;
                            schedulerTask.Enabled = model.Enabled;
                            schedulerTask.IsDaily = model.IsDaily;
                            schedulerTask.IsMonthly = model.IsMonth;
                            schedulerTask.LastDateModified = DateTime.Now;

                            // for scheduler task on store
                            var schedulerTaskOnStore = cxt.G_ScheduleTaskOnStore.FirstOrDefault(x => x.ScheduleTaskId == model.ID /*&& x.StoreId == model.StoreId*/);
                            schedulerTaskOnStore.Id = schedulerTaskOnStore.Id;
                            schedulerTaskOnStore.ScheduleTaskId = schedulerTask.Id;
                            schedulerTaskOnStore.StoreId = model.StoreId;
                            schedulerTaskOnStore.Description = model.Description;
                            schedulerTaskOnStore.LastDateModified = DateTime.Now;

                            cxt.SaveChanges();
                            dbContextTransaction.Commit();

                            StatusResponse.IsOk = true;
                            StatusResponse.Message = "Scheduler Task update successful.";
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                StatusResponse.IsOk = false;
                StatusResponse.Message = "Scheduler Task update fail!!!";
            }
            return StatusResponse;
        }

        public ResultModels DeleteUpdateCurrentSchedulerTask(SettingSchedulerTaskModels model)
        {
            try
            {
                using (var cxt = new NuWebContext())
                {
                    using (var dbContextTransaction = cxt.Database.BeginTransaction())
                    {
                        try
                        {
                            var schedulerTaskOnStore = cxt.G_ScheduleTaskOnStore.FirstOrDefault(x => x.ScheduleTaskId == model.ID);
                            cxt.G_ScheduleTaskOnStore.Remove(schedulerTaskOnStore);
                            var schedulerTask = cxt.G_ScheduleTask.FirstOrDefault(x => x.Id == model.ID && x.ReportId == model.ReportId);
                            cxt.G_ScheduleTask.Remove(schedulerTask);
                            cxt.SaveChanges();
                            dbContextTransaction.Commit();

                            StatusResponse.IsOk = true;
                            StatusResponse.Message = "Scheduler Task delete successful.";
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception)
            {
                StatusResponse.IsOk = false;
                StatusResponse.Message = "Scheduler Task delete fail!!!";
            }
            return StatusResponse;
        }
    }
}
