using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.ItemizedSalesAnalysisReport
{
    public class ItemizedSalesAnalysisModels
    {
        public List<ItemizedSalesAnalysisOuletTotalModels> ListItemizedSalesAnalysisOuletTotal { get; set; }
        public ItemizedSalesAnalysisTotalModels ItemizedSalesAnalysisTotal { get; set; }
        public List<ListPeriodChooseModels> ListPeriodChoosed { get; set; }
        public ItemizedSalesAnalysisModels()
        {
            ListItemizedSalesAnalysisOuletTotal = new List<ItemizedSalesAnalysisOuletTotalModels>();
            ItemizedSalesAnalysisTotal = new ItemizedSalesAnalysisTotalModels();
            ListPeriodChoosed = new List<ListPeriodChooseModels>();
        }
    }
    #region New
    public class ItemizedSalesPeriodValueTotal
    {
        public double Qty { get; set; }
        public double ItemTotal { get; set; }
        public double Percent { get; set; }
        public double Discount { get; set; }
        public double UnitCost { get; set; }
        public double TotalCost { get; set; }
        public double CP { get; set; }
        public double Promotion { get; set; }
        public ItemizedSalesPeriodValueTotal()
        {
            Qty = 0;
            ItemTotal = 0;
            Percent = 0;
            Discount = 0;
            UnitCost = 0;
            TotalCost = 0;
            CP = 0;
            Promotion = 0;
        }
    }
    public class ItemizedSalesNewTotal
    {
        public double ItemTotal { get; set; }
        public double SCTotal { get; set; }
        public double TaxTotal { get; set; }
        public double DiscountTotal { get; set; }
        public double PromotionTotal { get; set; }
        public ItemizedSalesNewTotal()
        {
            ItemTotal = 0;
            SCTotal = 0;
            TaxTotal = 0;
            DiscountTotal = 0;
            PromotionTotal = 0;
        }
    }
    #endregion EndNew
}
