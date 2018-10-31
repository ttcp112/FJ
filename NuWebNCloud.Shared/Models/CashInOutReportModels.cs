using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class CashInOutReportModels
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BusinessDayId { get; set; }

        public string DrawerId { get; set; }

        public string DrawerName { get; set; }

        public double CashValue { get; set; }

        public DateTime StartOn { get; set; }

        public DateTime EndOn { get; set; }

        public string UserName { get; set; }

        public int CashType { get; set; }

        public string Reason { get; set; }
        public int Mode { get; set; }
        public DateTime ShiftStartOn { get; set; }
        public DateTime ShiftEndOn { get; set; }
        public string Remark { get; set; }
    }
}
