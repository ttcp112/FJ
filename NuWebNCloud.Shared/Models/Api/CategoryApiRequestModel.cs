using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class CategoryApiRequestModel
    {
        public List<string> ListStoreIds { get; set; }
        public int Type { get; set; }
        public string HostUrl { get; set; }
    }
}
