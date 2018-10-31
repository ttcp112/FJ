using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class StockCountModels
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public DateTime StockCountDate { get; set; }

        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public string BusinessId { get; set; }
        public string BusinessValue { get; set; }
        public List<SelectListItem> ListBusinessDate { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
        public bool IsAutoCreated { get; set; }

        public bool IsVisible { get; set; }
        public bool IsConfirm { get; set; }
        public int Status { get; set; }
        public double Damage { get; set; }
        public double Wast { get; set; }
        public double OtherQty { get; set; }

        public DateTime? StartedOn { get; set; }
        public DateTime? ClosedOn { get; set; }

        public List<StockCountDetailModels> ListItem { get; set; }

        public StockCountModels()
        {
            StockCountDate = DateTime.Now;
            ListItem = new List<StockCountDetailModels>();
            ListBusinessDate = new List<SelectListItem>();
        }
    }

    public class StockCountViewModels
    {
        public string StoreId { get; set; }
        public List<StockCountModels> ListItem { get; set; }
        public StockCountViewModels()
        {
            ListItem = new List<StockCountModels>();
        }
    }

}
