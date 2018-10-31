using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public class G_Industry
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public string AreaName { get; set; }
    }
}
