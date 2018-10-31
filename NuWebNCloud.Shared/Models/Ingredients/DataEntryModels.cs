using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class DataEntryModels
    {
        public string Id { get; set; }
        public string EntryCode { get; set; }
        public DateTime EntryDate { get; set; }

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

        public List<DataEntryDetailModels> ListItem { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public DataEntryModels()
        {
            EntryDate = DateTime.Now;
            ListItem = new List<DataEntryDetailModels>();
            ListBusinessDate = new List<SelectListItem>();
            IsVisible = true;
        }
    }

    public class DataEntryViewModels
    {
        public string StoreId { get; set; }
        public List<DataEntryModels> ListItem { get; set; }
        public DataEntryViewModels()
        {
            ListItem = new List<DataEntryModels>();
        }
    }

}
