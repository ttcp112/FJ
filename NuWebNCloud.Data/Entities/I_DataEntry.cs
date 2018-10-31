using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_DataEntry: BaseEntity
    {
        public string EntryCode { get; set; }
        public DateTime EntryDate { get; set; }
        public string BusinessId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
    }
}
