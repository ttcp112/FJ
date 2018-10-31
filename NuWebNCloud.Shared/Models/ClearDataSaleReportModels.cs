using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ClearDataSaleReportModels
    {
        public List<string> ListStoreIds { get; set; }
        public DateTime DFrom { get; set; }
        public DateTime DTo { get; set; }
    }
}
