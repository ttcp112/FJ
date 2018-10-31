using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class G_ModulePermission
    {
        public string Id { get; set; }

        public string RoleID { get; set; }
        public string ModuleID { get; set; }
        public string ModuleParentID { get; set; }

        public bool IsView { get; set; }
        public bool IsAction { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
