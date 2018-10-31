using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class RPHourlySalesModels : BaseReportModel
    {
        [Required]
        public TimeSpan FromHour { get; set; }

        [Required]
        public TimeSpan ToHour { get; set; }

        //public List<SelectListItem> Hours { get; set; }

        public RPHourlySalesModels()
        {
            //Hours = new List<SelectListItem>();
        }
    }
}
