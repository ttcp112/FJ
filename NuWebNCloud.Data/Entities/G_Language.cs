using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_Language
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Status { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
    }
}
