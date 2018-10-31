using System;

namespace NuWebNCloud.Data.Entities
{
    /// <summary>
    /// Schedule task on store
    /// </summary>
    public partial class G_ScheduleTaskOnStore 
    {
        public string Id { get; set; }
        public string ScheduleTaskId { get; set; }
        public string StoreId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string LastUserModified { get; set; }
        public DateTime LastDateModified { get; set; }

        public virtual G_ScheduleTask G_ScheduleTask { get; set; }
    }
}
