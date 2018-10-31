using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_Return_Note
    {
        public string Id { get; set; }
        public string ReturnNoteNo { get; set; }
        public string ReceiptNoteId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
        public string BusinessId { get; set; }
    }
}
