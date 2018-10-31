using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class DetailItemizedSalesAnalysisReportDetailModels
    {
        public Guid StoreId { get; set; }
        public Guid ItemizedSalesAnalysisId { get; set; }

        public string ChildItemId { get; set; }
        public string ChildItemName { get; set; }

        public double Qty { get; set; }

        public double Price { get; set; }

        public int Mode { get; set; }
    }
}
