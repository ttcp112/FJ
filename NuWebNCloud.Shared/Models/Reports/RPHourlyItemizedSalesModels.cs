using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class RPHourlyItemizedSalesModels : BaseReportModel
    {
        [Required]
        public TimeSpan FromTime { get; set; }

        [Required]
        public TimeSpan ToTime { get; set; }

        //public List<SelectListItem> Hours { get; set; }

        public RPHourlyItemizedSalesModels()
        {
            //Hours = new List<SelectListItem>();
        }
    }
}
