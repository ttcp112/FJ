using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Charts
{
    public class DashBoardModels
    {
        public List<string> ListStoreIds { get; set; }
        public RevenueChartWeekRequestModels RevenueChartWeekRequestModels { get; set; }
        public RevenueChartMonthRequestModels RevenueChartMonthRequestModels { get; set; }
        public HourlySaleChartRequestModels HourlySaleChartRequestModels { get; set; }
        public CategoryChartRequestModels CategoryChartRequestModels { get; set; }
        public TopSellingChartRequestModels TopSellingChartRequestModels { get; set; }
        public DashBoardModels()
        {
            ListStoreIds = new List<string>();
            RevenueChartWeekRequestModels = new RevenueChartWeekRequestModels();
            RevenueChartMonthRequestModels = new RevenueChartMonthRequestModels();
            HourlySaleChartRequestModels = new HourlySaleChartRequestModels();
            CategoryChartRequestModels = new CategoryChartRequestModels();
            TopSellingChartRequestModels = new TopSellingChartRequestModels();
        }
    }

    public class BaseChartRequestModels
    {
        public List<string> ListStoreIds { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public BaseChartRequestModels()
        {
            ListStoreIds = new List<string>();
        }
    }
    public class BaseChartWeekRequestModels : BaseChartRequestModels
    {
        public BaseChartWeekRequestModels()
        {
            //DateFrom = CommonHelper.GetFirstDayOfWeek();
            //DateTo = DateFrom.AddDays(6);

            DateTo = DateTime.Now.AddDays(-1);
            DateFrom = DateTo.AddDays(-6);
        }
    }
    public class BaseChartMonthRequestModels : BaseChartRequestModels
    {
        public BaseChartMonthRequestModels()
        {
            DateFrom = CommonHelper.GetFirstDayOfTwoMonthsAgo();
            DateTo = CommonHelper.GetLastDayOfMonth();
        }
    }
    public class RevenueResponseModels
    {
        public string Date { get; set; }
        public RevenueResponseValueModels Receipt { get; set; }
        public RevenueResponseModels()
        {
            Receipt = new RevenueResponseValueModels();
        }
    }
    public class RevenueResponseValueModels
    {
        public int TC { get; set; }
        public double ReceiptTotal { get; set; }
    }
    public class RevenueTempModels
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public double ReceiptTotal { get; set; }
        public string BusinessId { get; set; }
    }
    #region Revenue
    public class RevenueChartWeekRequestModels : BaseChartWeekRequestModels
    {

    }
    public class RevenueChartMonthRequestModels : BaseChartMonthRequestModels
    {

    }
    #endregion End Revenue
    #region Hourly sale
    public class HourlySaleChartRequestModels : BaseChartRequestModels
    {
        public TimeSpan TimeFrom { get; set; }
        public TimeSpan TimeTo { get; set; }
        public Boolean IsMonth { get; set; }
        public DateTime WeekDateFrom { get; set; }
        public DateTime WeekDateTo { get; set; }
        public DateTime MonthDateFrom { get; set; }
        public DateTime MonthDateTo { get; set; }
        public HourlySaleChartRequestModels()
        {
            //WeekDateFrom = CommonHelper.GetFirstDayOfWeek();
            //WeekDateTo = WeekDateFrom.AddDays(6);
            WeekDateTo = DateTime.Now.AddDays(-1);
            WeekDateFrom = WeekDateTo.AddDays(-6);


            MonthDateFrom = CommonHelper.GetFirstDayOfTwoMonthsAgo();
            MonthDateTo = CommonHelper.GetLastDayOfMonth();
        }
    }
    public class HourlySaleChartModels
    {
        public string Time { get; set; }
        public double ReceiptTotal { get; set; }
        public int TC { get; set; }
        public double TA { get; set; }
    }
    public class HourlySaleChartResponseModels
    {
        public List<string> ListTimes { get; set; }
        public List<double> ListReceiptTotals { get; set; }
        public List<int> ListTC { get; set; }
        public List<double> ListTA { get; set; }
        public HourlySaleChartResponseModels()
        {
            ListTimes = new List<string>();
            ListReceiptTotals = new List<double>();
            ListTC = new List<int>();
            ListTA = new List<double>();
        }
    }
    #endregion End hourly sale

    #region Category
    public class CategoryChartRequestModels : BaseChartWeekRequestModels
    {
        /// <summary>
        /// 0: week
        /// 1: month
        /// </summary>
        public int Type { get; set; }
        public DateTime DateMonthFrom { get; set; }
        public DateTime DateMonthTo { get; set; }

        public CategoryChartRequestModels()
        {
            Type = 0;
            DateMonthFrom = CommonHelper.GetFirstDayOfTwoMonthsAgo();
            DateMonthTo = CommonHelper.GetLastDayOfMonth();
        }
    }

    public class CategoryChartResponseModels
    {
        public List<string> GLAccountCode { get; set; }
        public List<double> ListReceiptTotals { get; set; }

        public List<CategoryDetailChartResponseModels> ListCategoryDetail { get; set; }
        public CategoryChartResponseModels()
        {
            GLAccountCode = new List<string>();
            ListReceiptTotals = new List<double>();
            ListCategoryDetail = new List<CategoryDetailChartResponseModels>();
        }
    }
    public class CategoryDetailChartResponseModels
    {
        public string GLAccountCode { get; set; }
        public List<string> CategoryName { get; set; }
        public List<string> Colors { get; set; }
        public List<double> ListReceiptTotals { get; set; }

        public CategoryDetailChartResponseModels()
        {
            CategoryName = new List<string>();
            ListReceiptTotals = new List<double>();
            Colors = new List<string>();
        }
    }
    public class CategoryTmpChartResponseModels
    {
        public string GLAccountCode { get; set; }
        public string CategoryName { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public string CategoryId { get; set; }
        public string ReceiptId { get; set; }
    }
    #endregion End Category

    #region Top Selling
    public class TopSellingChartRequestModels : HourlySaleChartRequestModels
    {
        public int TopSell { get; set; }
        public int ItemType { get; set; }
        public List<SelectListItem> ListTopSell { get; set; }
        public List<SelectListItem> ListItemType { get; set; }
        public TopSellingChartRequestModels()
        {
            TopSell = 10;
            ItemType = 0;
            ListTopSell = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "1", Value = "1" },
                new SelectListItem() { Text = "2", Value = "2" },
                new SelectListItem() { Text = "3", Value = "3" },
                new SelectListItem() { Text = "4", Value = "4" },
                new SelectListItem() { Text = "5", Value = "5" },
                new SelectListItem() { Text = "6", Value = "6" },
                new SelectListItem() { Text = "7", Value = "7" },
                new SelectListItem() { Text = "8", Value = "8" },
                new SelectListItem() { Text = "9", Value = "9" },
                new SelectListItem() { Text = "10", Value = "10" },
            };
            ListItemType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("By Value"), Value = "0" },
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("By Amount"), Value = "1" },
            };
        }
    }

    public class TopSellingChartReponseModels
    {
        public string[] labels { get; set; } // List rank label
        public List<ItemTopSellingChartReponseModels> datasets { get; set; }// List items data
        public TopSellingChartReponseModels()
        {
            datasets = new List<ItemTopSellingChartReponseModels>();
        }
    }

    public class ItemTopSellingChartReponseModels
    {
        public string label { get; set; } // Item name
        public string backgroundColor { get; set; } // Point color
        public string[] data { get; set; } // Value of item (quanlity or amount)
        public Boolean showLine { get; set; }
        public ItemTopSellingChartReponseModels() {
        }
    }

    public class TopSellingChartTmpModels
    {
        public string ItemName { get; set; }
        public double Qty { get; set; }
        public double Amount { get; set; }
    }
    #endregion Top Selling
}
