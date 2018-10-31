using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class ResponseApiModels
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Error { get; set; }
        public object Data { get; set; }
        public object RawData { get; set; }
    }
}
