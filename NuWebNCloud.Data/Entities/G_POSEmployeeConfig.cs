using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_POSEmployeeConfig
    {
        public string Id { get; set; }
        public string POSAPIMerchantConfigId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActived { get; set; }
    }
}
