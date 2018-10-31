using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory
{
    public class ScheduleTrackingLogFactory
    {
        public void InsertScheduleEmailReport(ScheduleEmailModel model)
        {
            using (var cxt = new NuWebContext())
            {
                G_ScheduleTrackingLog log = new G_ScheduleTrackingLog();
                log.Id = Guid.NewGuid().ToString();
                log.DateSend = model.DateSend;
                log.ReportId = model.ReportId;
                //log.StoreId = model.StoreId;
                log.IsSend = model.IsSend;
                
                cxt.G_ScheduleTrackingLog.Add(log);
                cxt.SaveChanges();
            }
        }
    }
}
