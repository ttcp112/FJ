using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class RevenueTempDataModels
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public double ReceiptTotal { get; set; }
        public string BusinessId { get; set; }
    }
}
