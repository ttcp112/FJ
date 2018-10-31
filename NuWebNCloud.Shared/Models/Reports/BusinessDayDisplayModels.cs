using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class BusinessDayDisplayModels
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string DateDisplay { get; set; }
        public string  StoreId { get; set; }
        public string Id { get; set; }
    }
}
