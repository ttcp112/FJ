using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class TimeClockReportModels
    {
        public DateTime CreatedDate { get; set; }
        public string StoreId { get; set; }
        public int DayOfWeeks { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime DateTimeIn { get; set; }
        public DateTime DateTimeOut { get; set; }
        public int Mode { get; set; }
        public double HoursWork { get; set; }
        public double Late { get; set; }
        public double Early { get; set; }
        public string BusinessId { get; set; }
    }
}
