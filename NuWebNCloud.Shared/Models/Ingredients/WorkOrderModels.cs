using NuWebNCloud.Shared.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class WorkOrderModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public string Code { get; set; }
        public DateTime WODate { get; set; }
        public DateTime DateCompleted { get; set; }
        public string Note { get; set; }
        public double? Total { get; set; }
        public int Status { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
        public string WONumber { get; set; }
        public string ColorAlert { get; set; }
        public string Symbol { get; set; }

        public List<string> ListStores { get; set; }

        public List<WOIngredient> ListItem { get; set; }
        public List<ReceiptNoteSelfMadeDetailModels> ListItemForRN { get; set; }

        public SStoreModels Store { get; set; }

        public WorkOrderModels()
        {
            ListStores = new List<string>();
            ListItem = new List<WOIngredient>();
            ListItemForRN = new List<ReceiptNoteSelfMadeDetailModels>();

            WODate = DateTime.Now;
            DateCompleted = DateTime.Now;
        }
    }

    public class WorkOrderModelsViewModels
    {
        public List<WorkOrderModels> ListItem { get; set; }
        public string StoreID { get; set; }
        public DateTime? ApplyFrom { get; set; }
        public DateTime? ApplyTo { get; set; }
        public WorkOrderModelsViewModels()
        {
            ListItem = new List<WorkOrderModels>();
        }
    }

    public class WOIngredient
    {
        public string Id { get; set; }

        public bool IsSelect { get; set; }
        public string IngredientId { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }
        public string Description { get; set; }
        public double WorkPrice { get; set; }

        public string BaseUOM { get; set; }
        public double IngReceivingQty { get; set; }

        public double BaseQty { get; set; }

        public int OffSet { get; set; }
        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }

        public int Delete { get; set; }

        public double ReceiptNoteQty { get; set; }
        public double ReturnReceiptNoteQty { get; set; }

        public double ReceivedQty { get; set; }
        public double ReceivingQty { get; set; }
        public double RemainingQty { get; set; }

        /*isVisible*/
        public bool IsVisible { get; set; }
    }

    public class WOIngredientViewModels
    {
        public int Type { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string StoreId { get; set; }
        public List<WOIngredient> ListItemView { get; set; }

        public WOIngredientViewModels()
        {
            ListItemView = new List<WOIngredient>();
        }
    }

}
