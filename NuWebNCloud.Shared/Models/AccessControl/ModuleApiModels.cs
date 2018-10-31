using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.AccessControl
{
    public class ModuleApiModels : BaseApiRequestModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
    }
}
