using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class InventoryModels
    {
        public string StoreId { get; set; }
        public string IngredientId { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double POQty { get; set; }
        public double ReceiptQty { get; set; }
        public double ReturnQty { get; set; }
    }
    public class InventoryTransferModels
    {
        public string IssueStoreId { get; set; }
        public string ReceiveStoreId { get; set; }
        public string IngredientId { get; set; }
        public double IssueQty { get; set; }
        public double ReceiveQty { get; set; }
        public double Price { get; set; }
    }

    public class InventoryRequest
    {
        public string StoreId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public class InventoryInputModel
    {
        public int Index { get; set; }
        public string ListInventoryDetailId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string UOMName { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}")]
        public double QtyInput { get; set; }
        public List<InventoryInputDetailModel> ListDetail { get; set; }

        public InventoryInputModel()
        {
            ListDetail = new List<InventoryInputDetailModel>();
        }
    }
    public class InventoryInputDetailModel
    {
        public int Index { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime CreatedDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}")]
        public double Qty { get; set; }
    }

    public class InventoryTrackLogModel {
        public string StoreId { get; set; }
        public string IngredientId { get; set; }
        public int TypeCode { get; set; }
        public string TypeCodeId { get; set; }
        public double CurrentQty { get; set; }
        public double NewQty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
