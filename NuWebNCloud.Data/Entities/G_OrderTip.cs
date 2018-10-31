using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_OrderTip:BaseEntity
    {
        public string OrderId { get; set; }
        public double Amount { get; set; }
        public string PaymentId { get; set; }
        public string PaymentName { get; set; }
        public int Mode { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
