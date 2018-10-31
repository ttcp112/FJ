using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_CashInOutReport:BaseEntity
    {
        public string BusinessDayId { get; set; }
        public DateTime CreatedDate { get; set; }

        public string DrawerId { get; set; }

        public string DrawerName { get; set; }

        public double CashValue { get; set; }

        public DateTime StartOn { get; set; }

        public DateTime EndOn { get; set; }

        public string UserName { get; set; }

        public int CashType { get; set; }

        public int Mode { get; set; }
        public DateTime ShiftStartOn { get; set; }
        public DateTime ShiftEndOn { get; set; }
        public string Reason { get; set; }
    }
}
