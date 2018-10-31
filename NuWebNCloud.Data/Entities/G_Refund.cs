using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class G_Refund:BaseEntity
    {
        public string OrderId { get; set; }
        public double TotalRefund { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
