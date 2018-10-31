using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class StockTransferModels
    {
        public string Id { get; set; }
        public string StockTransferNo { get; set; }

        public string IssueStoreId { get; set; }
        public string IssueStoreName { get; set; }

        public string ReceiveStoreId { get; set; }
        public string ReceiveStoreName { get; set; }

        public string RequestBy { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime RequestDate { get; set; }

        public string IssueBy { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime IssueDate { get; set; }

        public string ReceiveBy { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime ReceiveDate { get; set; }

        public double ItemsTotal { get; set; }
        public bool IsActive { get; set; }

        public List<StockTransferDetailModels> ListItem { get; set; }

        public List<SelectListItem> ListEmployee { get; set; }
        public List<SelectListItem> ListEmployeeReceive { get; set; }

        public StockTransferModels()
        {
            RequestDate = DateTime.Now;
            IssueDate = DateTime.Now;
            ReceiveDate = DateTime.Now;

            ListEmployee = new List<SelectListItem>();
            ListEmployeeReceive = new List<SelectListItem>();
        }
    }

    public class StockTransferViewModels
    {
        public string StockTransferNo { get; set; }

        public string IssuingStoreId { get; set; }
        public string ReceivingStoreId { get; set; }

        public DateTime? ApplyFrom { get; set; }
        public DateTime? ApplyTo { get; set; }

        public bool IssueDate { get; set; }
        public bool ReveieDate { get; set; }

        public List<StockTransferModels> ListItem { get; set; }

        public StockTransferViewModels()
        {
            ListItem = new List<StockTransferModels>();
        }
    }

    public class STIngredientViewModels
    {
        public int Type { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string StoreId { get; set; }
        public List<StockTransferDetailModels> ListItemView { get; set; }

        public STIngredientViewModels()
        {
            ListItemView = new List<StockTransferDetailModels>();
        }
    }
}

