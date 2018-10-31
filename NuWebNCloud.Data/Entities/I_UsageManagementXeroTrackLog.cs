using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class I_UsageManagementXeroTrackLog
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
