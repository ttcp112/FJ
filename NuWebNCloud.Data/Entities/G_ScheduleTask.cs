using System;

namespace NuWebNCloud.Data.Entities
{
    /// <summary>
    /// Schedule task
    /// </summary>
    public partial class G_ScheduleTask 
    {
        public string Id { get; set; }
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public string EmailSubject { get; set; }
        public string Email { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        //public string DayOfWeeks { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public bool Enabled { get; set; }
        public bool IsDaily { get; set; }
        public bool IsWeekly { get; set; }
        public bool IsMonthly { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string LastUserModified { get; set; }
        public DateTime LastDateModified { get; set; }

    }
}
