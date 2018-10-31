using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public partial class I_UnitOfMeasure
    {
        public string Id { get; set; }  
        public string OrganizationId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int? Status { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
