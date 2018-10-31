using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ItemizedSalesAnalysisReportDetailModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public string BusinessId { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public int ItemTypeId { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        public double TotalAmount { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
        public List<ItemizedSalesAnalysisReportDetailModels> ListChilds { get; set; }

        // Updated 03132018, for report new DB, R_PosSale, R_PosSaleDetail
        public string OrderDetailId { get; set; } 
        public string ParentId { get; set; }
        public string ReceiptId { get; set; }
        public string CreditNoteNo { get; set; }

        public ItemizedSalesAnalysisReportDetailModels()
        {
            ListChilds = new List<ItemizedSalesAnalysisReportDetailModels>();
         }

    }
    public class ItemizedSalesDetailTotal {
        public ItemizedSalesDetailValueTotal BREAKFAST { get; set; }
        public ItemizedSalesDetailValueTotal LUNCH { get; set; }
        public ItemizedSalesDetailValueTotal DINNER { get; set; }
    }
    public class ItemizedSalesDetailValueTotal
    {
        public double Qty { get; set; }
        public double Amount { get; set; }
        public ItemizedSalesDetailValueTotal()
        {
            Qty = 0;
            Amount = 0;
        }
    }
}
