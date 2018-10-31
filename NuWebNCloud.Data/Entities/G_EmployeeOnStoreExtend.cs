using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_EmployeeOnStoreExtend
    {
        public string Id { get; set; }
        public string EmpOnMerchantExtendId { get; set; }
        public string StoreExtendId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActived { get; set; }

        public virtual G_EmployeeOnMerchantExtend G_EmployeeOnMerchantExtend { get; set; }
    }
}
