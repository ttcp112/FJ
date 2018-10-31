using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public abstract class BaseEntity
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
    }
}
