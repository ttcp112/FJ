using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class I_ReceiptNoteForSeftMade : BaseEntity
    {
        public string ReceiptNo { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string ReceiptBy { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string BusinessId { get; set; }
    }
}
