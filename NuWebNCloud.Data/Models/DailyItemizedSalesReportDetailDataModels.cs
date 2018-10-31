using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class DailyItemizedSalesReportDetailDataModels
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
    public class DailyItemizedSalesReportDetailForSetDataModels
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
    public class DailyItemizedSalesReportDetailPushDataResultModels 
    {
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public List<DailyItemizedSalesReportDetailDataModels> ListDailyItemizedSales { get; set; }
        public List<DailyItemizedSalesReportDetailForSetDataModels> ListDailyItemizedSalesForSet { get; set; }
        public DailyItemizedSalesReportDetailPushDataResultModels()
        {
            ListDailyItemizedSales = new List<DailyItemizedSalesReportDetailDataModels>();
            ListDailyItemizedSalesForSet = new List<DailyItemizedSalesReportDetailForSetDataModels>();
        }
    }
}
