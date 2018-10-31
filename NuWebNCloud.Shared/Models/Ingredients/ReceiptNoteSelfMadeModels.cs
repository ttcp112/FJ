using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class ReceiptNoteSelfMadeModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string ReceiptBy { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string BusinessId { get; set; }

        public string WONo { get; set; }
        public string WOId { get; set; }

        public List<ReceiptNoteSelfMadeDetailModels> ListItem { get; set; }
        public List<ReceiptNoteSelfMadeDetailModels> ListItemForSelect { get; set; }

        public List<WorkOrderModels> ListWorkOrder { get; set; }

        public ReceiptNoteSelfMadeModels()
        {
            ListItem = new List<ReceiptNoteSelfMadeDetailModels>();
            ListItemForSelect = new List<ReceiptNoteSelfMadeDetailModels>();
            ListWorkOrder = new List<Ingredients.WorkOrderModels>();
            ReceiptDate = DateTime.Now;
        }
    }

    public class ReceiptNoteSelfMadeViewModels
    {
        public string StoreID { get; set; }
        public List<string> ListSupplierId { get; set; }
        public DateTime? ReceiptNoteDate { get; set; }
        public List<ReceiptNoteSelfMadeModels> ListItem { get; set; }
        public ReceiptNoteSelfMadeViewModels()
        {
            ListItem = new List<ReceiptNoteSelfMadeModels>();
        }
    }

    public class ReceiptNoteSelfMadeViewDetailModels
    {
        public ReceiptNoteSelfMadeModels ReceiptNote { get; set; }
        public List<ReceiptNoteDetailModels> ListItem { get; set; }
    }

    public class ReceiptNoteSelfMadeIngredient
    {
        public bool IsSelect { get; set; }
        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Description { get; set; }
        public string BaseUOM { get; set; }

        public int OffSet { get; set; }
        public double Qty { get; set; }
        public int Delete { get; set; }
    }

    public class ReceiptNoteSelfMadeIngredientViewModels
    {
        public int Type { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string StoreId { get; set; }
        public List<ReceiptNoteSelfMadeIngredient> ListItem { get; set; }

        public ReceiptNoteSelfMadeIngredientViewModels()
        {
            ListItem = new List<ReceiptNoteSelfMadeIngredient>();
        }
    }
    public class ErrorEnoughModels
    {
        public string MixIngredientId { get; set; }
        public List<string> ListIngredientNameNotEnough { get; set; }
        public ErrorEnoughModels()
        {
            ListIngredientNameNotEnough = new List<string>();
        }
    }
}
