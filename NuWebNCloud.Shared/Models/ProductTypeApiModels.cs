using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ProductTypeApiModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string CreatedUser { get; set; }
    }
}
