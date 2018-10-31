using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_MultiLocationConfig
    {
        public string Id { get; set; }
        public string POSEmployeeConfigId { get; set; }
        public string CountryCode { get; set; }
        public string UrlWebHost { get; set; }
        public bool IsActived { get; set; }
        public virtual G_POSEmployeeConfig G_POSEmployeeConfig { get; set; }
    }
}
