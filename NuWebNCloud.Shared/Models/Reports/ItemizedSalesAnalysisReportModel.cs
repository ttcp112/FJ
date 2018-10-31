using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class ItemizedSalesAnalysisReportModel: BaseReportWithTimeMode
    {
        public bool Breakfast { get; set; }
        public bool Lunch { get; set; }
        public bool Dinner { get; set; }
        public bool IsShowMultiLevel { get; set; }
        // Updated 09132017
        public List<RFilterCategoryV1Model> ListCategoriesV1 { get; set; }
        public List<StoreCateV1> ListStoreCateV1 { get; set; }

        public ItemizedSalesAnalysisReportModel()
        {
            Breakfast = true;
            Lunch = true;
            Dinner = true;

            // Updated 09132017
            ListCategoriesV1 = new List<RFilterCategoryV1Model>();
            ListStoreCateV1 = new List<StoreCateV1>();
        }
    }

    // Updated 09132017
    public class StoreCateV1
    {
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int OffSet { get; set; }
        public List<RFilterCategoryV1Model> ListCategoriesSel { get; set; }
    }
}
