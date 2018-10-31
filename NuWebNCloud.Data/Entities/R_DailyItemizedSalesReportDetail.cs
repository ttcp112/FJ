using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class R_DailyItemizedSalesReportDetail : BaseEntity
    {
        public string BusinessId { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int ItemTypeId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CategoryTypeId { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
    }
}
