using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_Country
    {
        public string Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string ZipCode { get; set; }
        public bool IsActived { get; set; }
    }
}
