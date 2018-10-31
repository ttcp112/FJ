using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class HourlyItemizedSalesReportModels
    {
        public string StoreId { get; set; }
        public int ItemTypeId { get; set; }

        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public double TotalPrice { get; set; }

        public int Hour { get; set; }

        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
        public double Discount { get; set; }
        public double Promotion { get; set; }
        public string BusinessId { get; set; }

        public string ItemId { get; set; }
        public bool IsDiscountTotal { get; set; }
        public double Tax { get; set; }
    }
}
