using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class DiscountAndMiscReportModels
    {
        public string StoreId { get; set; }
        public double MiscValue { get; set; }
        public double DiscountValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Hour { get; set; }
        public TimeSpan TimeSpanHour { get; set; }
        /*properties view*/
        public string StoreName { get; set; }
        public int Mode { get; set; }
        // Updated 05092018, for report new DB
        public bool IsDiscountTotal { get; set; } 
        public int ItemTypeId { get; set; }

    }
}
