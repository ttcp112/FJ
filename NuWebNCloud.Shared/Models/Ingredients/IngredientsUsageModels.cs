using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class IngredientsUsageModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date { get; set; }
        public string DateDisplay { get; set; }
        public string BusinessDayDisplay { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public string IngredientId { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }

        public double UnitCost { get; set; }
        public double OpenBal { get; set; }
        public double StockIn { get; set; }
        public double CloseBal { get; set; }
        public double ActualSold { get; set; }
        public double Adjust { get; set; }
        public double? AutoCloseBal { get; set; }

        public double Sales { get; set; }
        public double Damage { get; set; }
        public double Wast { get; set; }

        public double Others { get; set; }
        public string Reasons { get; set; }

        public bool IsActive { get; set; }
        public bool IsAllocation { get; set; }
        public bool IsExistAllocation { get; set; }
        public double VarianceQty { get; set; }
        public string BusinessId { get; set; }
        public double StockOut { get; set; }
        public bool IsSeftMade { get; set; }
        public bool IsStockAble { get; set; }
        //public List<IngredientsUsageModels> ListItem { get; set; }

        //public IngredientsUsageModels()
        //{
        //    ListItem = new List<IngredientsUsageModels>();
        //}
    }

    //public class IngredientsUsageViewModels
    //{
    //    public string StoreId { get; set; }
    //    public DateTime ApplyForm { get; set; }
    //    public DateTime ApplyTo { get; set; }
    //    public List<IngredientsUsageModels> ListItem { get; set; }
    //    //public IngredientsUsageViewModels()
    //    //{
    //    //    ApplyForm = DateTime.Now;
    //    //    ApplyTo = DateTime.Now;
    //    //    ListItem = new List<IngredientsUsageModels>();
    //    //}
    //}

    public class IngredientsUsageRequestViewModels
    {
        public string StoreId { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime ApplyFrom { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime ApplyTo { get; set; }
        public List<IngredientsUsageModels> ListItem { get; set; }
        public List<string> ListSelected { get; set; }
        public string BusinessId { get; set; }
        public IngredientsUsageRequestViewModels()
        {
            ApplyFrom = DateTime.Now;
            ApplyTo = DateTime.Now;
            ListItem = new List<IngredientsUsageModels>();
        }
    }
}
