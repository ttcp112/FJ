using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.ItemizedSalesAnalysisReport
{
    public class ItemizedSalesAnalysisTotalModels
    {
        public double Qty { get; set; }

        public double ItemTotal { get; set; }
        public double Percent { get; set; }
        public double Discount { get; set; }
        public double UnitCost { get; set; }
        public double TotalCost { get; set; }
        public double CP { get; set; }

        public double SalesTotal { get; set; }
        public double NonSales { get; set; }
        public double SCTotal { get; set; }
        public double TaxTotal { get; set; }
        public double DiscTotal { get; set; }
        public double GrandTotal { get; set; }

        public double ReceiptNo { get; set; }
        public double NoOfPerson { get; set; }
        public double AvgReceipt { get; set; }
        public double AvgNoOfReceipt { get; set; }
        public double Promotion { get; set; }

        public List<ListPeriodTotalModels> ListPeriodTotal { get; set; }
    }
}
