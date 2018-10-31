using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.ItemizedSalesAnalysisReport
{
    public class ItemizedSalesAnalysisOuletTotalModels
    {
        public string StoreName { get; set; }
        public string CategoryName { get; set; }
        public double Qty { get; set; }
        public double ItemTotal { get; set; }
        public double Percent { get; set; }
        public double Discount { get; set; }
        public double UnitCost { get; set; }
        public double TotalCost { get; set; }
        public double CP { get; set; }
        public double Promotion { get; set; }

        public List<ItemizedSalesAnalysisSubTotalModels> ListItemizedSalesAnalysisSubTotal { get; set; }
        public List<ItemizedSalesAnalysisSubTotalSetMenuModels> ListItemizedSalesAnalysisSubTotalSetMenu { get; set; }
    }
}
