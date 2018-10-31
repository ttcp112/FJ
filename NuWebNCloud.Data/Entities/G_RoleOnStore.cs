using System;

namespace NuWebNCloud.Data.Entities
{
    public class G_RoleOnStore
    {
        public string Id { get; set; }
        public string RoleId { get; set; }
        public string StoreId { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
