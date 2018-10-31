using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class G_ScheduleTrackingLog
    {
        public string Id { get; set; }
        public string ReportId { get; set; }
        public string StoreIds { get; set; }
        public string Description { get; set; }
        public DateTime? DateSend { get; set; }
        public bool IsSend { get; set; }
    }
}
