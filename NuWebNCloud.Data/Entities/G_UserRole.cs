using System;

namespace NuWebNCloud.Data.Entities
{
    public class G_UserRole
    {
        public string Id { get; set; }
        public string RoleID { get; set; }
        public string EmployeeID { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
