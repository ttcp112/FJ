using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ResultModels
    {
        public bool IsOk { get; set; }
        public string Message { get; set; }
        public object RawData { get; set; }
    }
}
