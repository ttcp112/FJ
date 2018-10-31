using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_PaymentMenthod : BaseEntity
    {
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public int? PaymentCode { get; set; }
        public string PaymentName { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string LastUserModified { get; set; }
        public DateTime LastDateModified { get; set; }
        public int Mode { get; set; }
        public bool? IsInclude { get; set; }
    }
}
