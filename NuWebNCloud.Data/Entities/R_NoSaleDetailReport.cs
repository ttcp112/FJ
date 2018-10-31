using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class R_NoSaleDetailReport : BaseEntity
    {
        public DateTime CreatedDate { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public string DrawerId { get; set; }
        public string DrawerName { get; set; }
        public string Reason { get; set; }

        public int Mode { get; set; }

        //new 08/08/2017
        public string BusinessId { get; set; }
        public string ShiftId { get; set; }
        public DateTime? StartedShift { get; set; }
        public DateTime? ClosedShift { get; set; }
    }
}
