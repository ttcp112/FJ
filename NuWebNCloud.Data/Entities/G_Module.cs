using System;

namespace NuWebNCloud.Data.Entities
{
    public class G_Module
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentID { get; set; }
        public string Controller { get; set; }
        //public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? IndexNum { get; set; }
    }
}
