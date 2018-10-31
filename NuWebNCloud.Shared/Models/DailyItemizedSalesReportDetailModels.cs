using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class DailyItemizedSalesReportDetailModels 
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int ItemTypeId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CategoryTypeId { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
        public string OrderDetailId { get; set; } // Updated 05082018, for report new DB, R_PosSaleDetail
        public string ParentId { get; set; } // Updated 05082018, for report new DB, R_PosSaleDetail

    }
    public class DailyItemizedSalesReportDetailForSetModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CategoryTypeId { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
        public string OrderDetailId { get; set; } // Updated 05082018, for report new DB, R_PosSaleDetail
    }

    public class DailyItemizedSalesReportDetailPushDataModels : BaseReportWithTimeMode
    {
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public List<DailyItemizedSalesReportDetailModels> ListDailyItemizedSales { get; set; }
        public List<DailyItemizedSalesReportDetailForSetModels> ListDailyItemizedSalesForSet { get; set; }
        public DailyItemizedSalesReportDetailPushDataModels()
        {
            ListDailyItemizedSales = new List<DailyItemizedSalesReportDetailModels>();
            ListDailyItemizedSalesForSet = new List<DailyItemizedSalesReportDetailForSetModels>();
        }
    }
}
