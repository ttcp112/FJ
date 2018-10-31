using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class DetailItemizedSalesAnalysisModifiersviewModels: BaseReportModel
    {

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<string> lstStore { get; set; }      
        public DetailItemizedSalesAnalysisModifiersviewModels()
        {
            
        }
    }
    public class Modifier
    {
        public string ModifierId { get; set; }
        public string ModifierName { get; set; }
        public string StoreId { get; set; }
        public string Company { get; set; }
    }
    public class RFilterModifierModel
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
        public List<RFilterModifierModel> ListChilds { get; set; }
        public RFilterModifierModel()
        {
            ListChilds = new List<RFilterModifierModel>();
        }

    }

    public class CategoryOnStore 
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public List<CategoryOfDish> ListCategory { get; set; }
        public CategoryOnStore()
        {
            ListCategory = new List<CategoryOfDish>();
        }

    }
    public class CategoryOfDish
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<Dish> ListDish { get; set; }
        public CategoryOfDish ()
        {
            ListDish = new List<Dish>();
        }
    }
    public class Dish
    {
        public string DishId { get; set; }
        public string DishName { get; set; }
        public List<ModifierOnDish> ListModifierOnDish { get; set; }
        public Dish()
        {
            ListModifierOnDish = new List<ModifierOnDish>();
        }
    }

    public class ModifierOnDish
    {
        public string ModifierId { get; set; }
        public string ModifierName { get; set; }
        public double DishQuantity { get; set; }
        public double DishAmount { get; set; }
    }
    //use for sum Amount to report
    public class SumAmount
    {
        public string ModifierId { get; set; }
        public double TotalAmount { get; set; }
    }
}