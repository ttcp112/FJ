using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class DetailItemizedSalesAnalysisReportHeaderModels
    {
        public string StoreId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemTypeId { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
    }
}
