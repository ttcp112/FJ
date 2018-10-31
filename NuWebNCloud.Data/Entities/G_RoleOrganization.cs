using System;

namespace NuWebNCloud.Data.Entities
{
    public class G_RoleOrganization
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public string OrganizationId { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
