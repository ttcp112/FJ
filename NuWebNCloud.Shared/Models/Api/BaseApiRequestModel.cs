using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class BaseApiRequestModel
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string StoreId { get; set; }
        public string CreatedUser { get; set; }
        public string Mode { get; set; }
    }
}
