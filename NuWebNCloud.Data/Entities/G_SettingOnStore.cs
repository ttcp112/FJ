using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_SettingOnStore: BaseEntity
    {
        public string SettingId { get; set; }
        public string Value { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string LastUserModified { get; set; }
        public DateTime LastDateModified { get; set; }
        public virtual G_GeneralSetting G_GeneralSetting { get; set; }

    }
}
