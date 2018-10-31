using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;

    public partial class R_TimeClockReport : BaseEntity
    {
        public DateTime CreatedDate { get; set; }
        public int DayOfWeeks { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime DateTimeIn { get; set; }
        public DateTime DateTimeOut { get; set; }
        public double Early { get; set; }
        public double Late { get; set; }
        public double HoursWork { get; set; }
        public int Mode { get; set; }
        public string BusinessId { get; set; }
    }
}
