using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class RFilterCategoryModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public int Seq { get; set; }
        public bool Checked { get; set; }
        public string ParentId { get; set; }
        public string CategoryID { get; set; }
        public string CategoryName { get; set; }
        public List<RFilterCategoryModel> ListChilds { get; set; }
        public RFilterCategoryModel()
        {
            ListChilds = new List<RFilterCategoryModel>();
        }

    }

    // Updated 09132017
    public class RFilterCategoryV1Model
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public int Seq { get; set; }
        public bool Checked { get; set; }
        public string ParentId { get; set; }
        public string Level { get; set; }
        public string GLCode { get; set; }
    }

    public class RFilterCategoryV1ReportModel
    {
        public string CateId { get; set; }
        public string CateName { get; set; }
        public bool Checked { get; set; }
        public string StoreId { get; set; }
        public string ParentId { get; set; }
        public string Level { get; set; }
        public int Seq { get; set; }
        public List<string> ListCateChildChecked { get; set; }
        public double SubTotalQty { get; set; }
        public double SubTotalItem { get; set; }
        public double SubTotalPercent { get; set; }
        public double SubTotalDiscount { get; set; }
        public double SubTotalPromotion { get; set; }
        public double SubTotalUnitCost { get; set; }
        public double SubTotalTotalCost { get; set; }
        public double SubTotalCP { get; set; }
        public double BreakfastTotalQty { get; set; }
        public double BreakfastTotalItem { get; set; }
        public double BreakfastTotalPercent { get; set; }
        public double BreakfastTotalDiscount { get; set; }
        public double BreakfastTotalPromotion { get; set; }
        public double BreakfastTotalUnitCost { get; set; }
        public double BreakfastTotalTotalCost { get; set; }
        public double BreakfastTotalCP { get; set; }
        public double LunchTotalQty { get; set; }
        public double LunchTotalItem { get; set; }
        public double LunchTotalPercent { get; set; }
        public double LunchTotalDiscount { get; set; }
        public double LunchTotalPromotion { get; set; }
        public double LunchTotalUnitCost { get; set; }
        public double LunchTotalTotalCost { get; set; }
        public double LunchTotalCP { get; set; }
        public double DinnerTotalQty { get; set; }
        public double DinnerTotalItem { get; set; }
        public double DinnerTotalPercent { get; set; }
        public double DinnerTotalDiscount { get; set; }
        public double DinnerTotalPromotion { get; set; }
        public double DinnerTotalUnitCost { get; set; }
        public double DinnerTotalTotalCost { get; set; }
        public double DinnerTotalCP { get; set; }
        public RFilterCategoryV1ReportModel()
        {
            ListCateChildChecked = new List<string>();
        }
    }

    


}
