using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ScheduleEmailModel
    {
        public string StoreId { get; set; }
        public string ReportId { get; set; }
        public bool IsSend { get; set; }
        public DateTime DateSend { get; set; }

    }
}
