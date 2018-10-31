using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_POSAPIMerchantConfig
    {
        public string Id { get; set; }
        public string NuPOSInstance { get; set; }
        public string POSAPIUrl { get; set; }
        public string FTPHost { get; set; }
        public string FTPUser { get; set; }
        public string FTPPassword { get; set; }
        public string ImageBaseUrl { get; set; }

        public string BreakfastStart { get; set; }
        public string BreakfastEnd { get; set; }

        public string LunchStart { get; set; }
        public string LunchEnd { get; set; }

        public string DinnerStart { get; set; }
        public string DinnerEnd { get; set; }

        public DateTime CreatedDate { get; set; }
        public bool IsActived { get; set; }
        public int? POSInstanceVersion { get; set; }
        public TimeSpan? MorningStart { get; set; }
        public TimeSpan? MorningEnd { get; set; }
        public TimeSpan? MidDayStart { get; set; }
        public TimeSpan? MidDayEnd { get; set; }
        public string WebHostUrl { get; set; }
    }
}
