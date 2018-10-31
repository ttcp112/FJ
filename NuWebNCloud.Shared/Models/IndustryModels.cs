using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class IndustryModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }
        public int Status { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string LastUserModified { get; set; }
        public DateTime LastDateModified { get; set; }

        public string AreaName { get; set; }
    }
}
