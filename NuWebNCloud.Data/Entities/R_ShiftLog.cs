using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_ShiftLog : BaseEntity
    {
        public string BusinessId { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime ClosedOn { get; set; }
        public string StartedStaff { get; set; }
        public string ClosedStaff { get; set; }
      
    }
}
