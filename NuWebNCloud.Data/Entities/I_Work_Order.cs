using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_Work_Order : BaseEntity
    {
        public string Code { get; set; }
        public DateTime WODate { get; set; }
        public DateTime DateCompleted { get; set; }
        public string Note { get; set; }
        public double? Total { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
    }
}
