using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class RPEmployeeItemModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }

        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public string OffSet { get; set; }
    }
}
