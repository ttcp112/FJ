using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class ReceiptNoteDetailModels
    {
        public string Id { get; set; }
        public string ReceiptNoteId { get; set; }
        public string PurchaseOrderDetailId { get; set; }

        public double ReceivedQty { get; set; }
        public double ReceivingQty { get; set; }
        public double RemainingQty { get; set; }

        public bool IsActived { get; set; }

        //public string IngredientId { get; set; }
        //public string IngredientCode { get; set; }
        //public string IngredientName { get; set; }
        //public string BaseUOM { get; set; }
        //public double Quantity { get; set; }
        //public double Price { get; set; }
    }


    public class ExportReceiptNoteDetail
    {
        public int Index { get; set; }
        public string ReceiptNoteNo { get; set; }
        public string PONumber { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UOM { get; set; }

        public double OrderQuantity { get; set; }
        public double ReceivingQuantity { get; set; }
        public double RemainingQuantity { get; set; }
        public double ReturnQty { get; set; }
    }
}
