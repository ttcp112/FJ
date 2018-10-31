using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_TrackingLog : BaseEntity
    {
        public string TableName { get; set; }
        public string JsonContent { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDone { get; set; }
    }
}
