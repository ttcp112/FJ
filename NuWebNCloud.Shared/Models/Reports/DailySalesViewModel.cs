using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class DailySalesViewModel: BaseReportModel
    {
        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public Boolean IsIncludeShift { get; set; }
    }
}
