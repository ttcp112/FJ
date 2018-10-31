using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_StockCount : BaseEntity
    {
        public string Code { get; set; }
        public DateTime StockCountDate { get; set; }
        public string BusinessId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
        public bool? IsAutoCreated { get; set; }
        public int Status { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
    }
}
