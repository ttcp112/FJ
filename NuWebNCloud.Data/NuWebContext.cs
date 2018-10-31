using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Reflection;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Data.Mapping;

namespace NuWebNCloud.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using NuWebNCloud.Data.Entities;
    using System.Collections.Generic;
    using NuWebNCloud.Data.Models;

    public partial class NuWebContext : DbContext
    {
        public NuWebContext()
            : base("name=NuWebConnectionString")
        {
            //Database.SetInitializer<NuWebContext>(new MigrateDatabaseToLatestVersion<NuWebContext, NuWebNCloud.Data.Migrations.Configuration>("NuWebConnectionString"));
            Database.SetInitializer<NuWebContext>(new CreateDatabaseIfNotExists<NuWebContext>());
            ((IObjectContextAdapter)this).ObjectContext.ContextOptions.LazyLoadingEnabled = true;
            //this.Configuration.AutoDetectChangesEnabled = false;
        }

        public virtual DbSet<R_AuditTrailReport> R_AuditTrailReport { get; set; }
        public virtual DbSet<R_CashInOutReport> R_CashInOutReport { get; set; }
        public virtual DbSet<R_ClosedReceiptReport> R_ClosedReceiptReport { get; set; }
        public virtual DbSet<R_DailyReceiptReport> R_DailyReceiptReport { get; set; }
        public virtual DbSet<R_DailySalesReport> R_DailySalesReport { get; set; }
        public virtual DbSet<R_DetailItemizedSalesAnalysisReportHeader> R_DetailItemizedSalesAnalysisReportHeader { get; set; }
        public virtual DbSet<R_DiscountDetailsReport> R_DiscountDetailsReport { get; set; }
        public virtual DbSet<R_HourlyItemizedSalesReport> R_HourlyItemizedSalesReport { get; set; }
        public virtual DbSet<R_HourlySalesReport> R_HourlySalesReport { get; set; }
        public virtual DbSet<R_ItemizedSalesAnalysisReport> R_ItemizedSalesAnalysisReport { get; set; }
        public virtual DbSet<R_DiscountAndMiscReport> R_DiscountAndMiscReport { get; set; }
        public virtual DbSet<R_ReceiptsbyPaymentMethodsReport> R_ReceiptsbyPaymentMethodsReport { get; set; }
        public virtual DbSet<R_TimeClockReport> R_TimeClockReport { get; set; }
        public virtual DbSet<R_TopSellingProductsReport> R_TopSellingProductsReport { get; set; }
        public virtual DbSet<G_TrackingLog> G_TrackingLog { get; set; }
        public virtual DbSet<G_User> G_User { get; set; }
        public virtual DbSet<G_Tax> G_Tax { get; set; }
        public virtual DbSet<R_NoSaleDetailReport> R_NoSaleDetailReport { get; set; }
        public virtual DbSet<G_PaymentMenthod> G_PaymentMenthod { get; set; }
        public virtual DbSet<G_DateOfWeeks> G_DateOfWeeks { get; set; }
        public virtual DbSet<G_ScheduleTask> G_ScheduleTask { get; set; }
        public virtual DbSet<G_ScheduleTrackingLog> G_ScheduleTrackingLog { get; set; }
        public virtual DbSet<G_Industry> G_Industry { get; set; }
        public virtual DbSet<G_BusinessDay> G_BusinessDay { get; set; }
        public virtual DbSet<G_OrderTip> G_OrderTip { get; set; }
        //public virtual DbSet<G_Refund> G_Refund { get; set; }

        //ingredients
        public virtual DbSet<I_Ingredient> I_Ingredient { get; set; }
        public virtual DbSet<I_IngredientTrackLog> I_IngredientTrackLog { get; set; }
        public virtual DbSet<I_Ingredient_UOM> I_Ingredient_UOM { get; set; }
        public virtual DbSet<I_ReceiptNote> I_ReceiptNote { get; set; }
        public virtual DbSet<I_ReceiptNoteDetail> I_ReceiptNoteDetail { get; set; }
        public virtual DbSet<I_Recipe_Item> I_Recipe_Item { get; set; }
        public virtual DbSet<I_Recipe_Modifier> I_Recipe_Modifier { get; set; }
        public virtual DbSet<I_Recipe_Ingredient> I_Recipe_Ingredient { get; set; }
        public virtual DbSet<I_StockUsage> I_StockUsage { get; set; }
        public virtual DbSet<I_StoreSetting> I_StoreSetting { get; set; }
        public virtual DbSet<I_UnitOfMeasure> I_UnitOfMeasure { get; set; }
        public virtual DbSet<I_UsageManagementXeroTrackLog> I_UsageManagementXeroTrackLog { get; set; }
        public virtual DbSet<I_InventoryManagement> I_InventoryManagement { get; set; }
        public virtual DbSet<I_UsageManagement> I_UsageManagement { get; set; }
        public virtual DbSet<I_UsageManagementDetail> I_UsageManagementDetail { get; set; }
        public virtual DbSet<I_UsageManagementItemDetail> I_UsageManagementItemDetail { get; set; }

        public virtual DbSet<R_Refund> R_Refund { get; set; }
        public virtual DbSet<R_RefundDetail> R_RefundDetail { get; set; }

        //Role - Permission | 22/12/2016 trongntn
        public virtual DbSet<G_RoleOrganization> G_RoleOrganization { get; set; }
        public virtual DbSet<G_RoleOnStore> G_RoleOnStore { get; set; }
        public virtual DbSet<G_UserRole> G_UserRole { get; set; }

        public virtual DbSet<G_Module> G_Module { get; set; }
        public virtual DbSet<G_ModulePermission> G_ModulePermission { get; set; }
        public virtual DbSet<R_ItemizedSalesAnalysisReportDetail> R_ItemizedSalesAnalysisReportDetail { get; set; }
        //ingredient Ver2
        public virtual DbSet<I_Country> I_Country { get; set; }
        public virtual DbSet<I_Ingredient_Supplier> I_Ingredient_Supplier { get; set; }
        public virtual DbSet<I_Purchase_Order> I_Purchase_Order { get; set; }
        public virtual DbSet<I_Purchase_Order_Detail> I_Purchase_Order_Detail { get; set; }
        public virtual DbSet<I_Receipt_Purchase_Order> I_Receipt_Purchase_Order { get; set; }
        public virtual DbSet<I_Return_Note> I_Return_Note { get; set; }
        public virtual DbSet<I_Return_Note_Detail> I_Return_Note_Detail { get; set; }
        public virtual DbSet<I_Stock_Transfer> I_Stock_Transfer { get; set; }
        public virtual DbSet<I_Stock_Transfer_Detail> I_Stock_Transfer_Detail { get; set; }
        public virtual DbSet<I_Supplier> I_Supplier { get; set; }
        public virtual DbSet<I_DataEntry> I_DataEntry { get; set; }
        public virtual DbSet<I_DataEntryDetail> I_DataEntryDetail { get; set; }

        public virtual DbSet<I_Allocation> I_Allocation { get; set; }
        public virtual DbSet<I_AllocationDetail> I_AllocationDetail { get; set; }
        public virtual DbSet<I_InventoryManagementTrackLog> I_InventoryManagementTrackLog { get; set; }
        public virtual DbSet<I_StockCount> I_StockCount { get; set; }
        public virtual DbSet<I_StockCountDetail> I_StockCountDetail { get; set; }
        public virtual DbSet<G_POSAPIMerchantConfig> G_POSAPIMerchantConfig { get; set; }
        public virtual DbSet<G_POSEmployeeConfig> G_POSEmployeeConfig { get; set; }
        public virtual DbSet<R_NoIncludeOnSaleDataReport> R_NoIncludeOnSaleDataReport { get; set; }
        public virtual DbSet<I_ReceiptNoteForSeftMade> I_ReceiptNoteForSeftMade { get; set; }
        public virtual DbSet<I_ReceiptNoteForSeftMadeDetail> I_ReceiptNoteForSeftMadeDetail { get; set; }
        //Daily item sale report
        public virtual DbSet<R_DailyItemizedSalesReportDetail> R_DailyItemizedSalesReportDetail { get; set; }
        public virtual DbSet<R_DailyItemizedSalesReportDetailForSet> R_DailyItemizedSalesReportDetailForSet { get; set; }

        //langauge
        public virtual DbSet<G_Language> G_Language { get; set; }
        public virtual DbSet<G_LanguageLink> G_LanguageLink { get; set; }
        public virtual DbSet<G_LanguageLinkDetail> G_LanguageLinkDetail { get; set; }
        //WO
        public virtual DbSet<I_Work_Order> I_Work_Order { get; set; }
        public virtual DbSet<I_Work_Order_Detail> I_Work_Order_Detail { get; set; }
        public virtual DbSet<I_ReceiptSelfMade_Work_Order> I_ReceiptSelfMade_Work_Order { get; set; }
        public virtual DbSet<I_ReceiptNoteForSeftMadeDependentDetail> I_ReceiptNoteForSeftMadeDependentDetail { get; set; }
        // cancel & refund item
        public virtual DbSet<R_ItemizedCancelOrRefundData> R_ItemizedCancelOrRefundData { get; set; }
        public virtual DbSet<R_ShiftLog> R_ShiftLog { get; set; }
        //2017 - 11 -16
        public virtual DbSet<G_EmployeeOnMerchantExtend> G_EmployeeOnMerchantExtend { get; set; }
        public virtual DbSet<G_EmployeeOnStoreExtend> G_EmployeeOnStoreExtend { get; set; }
        //2017-12-26
        public virtual DbSet<G_MultiLocationConfig> G_MultiLocationConfig { get; set; }
        //2018-01-31
        public virtual DbSet<R_PosSale> R_PosSale { get; set; }
        public virtual DbSet<R_PosSaleDetail> R_PosSaleDetail { get; set; }
        //2018-02-27
        public virtual DbSet<G_FJDailySaleReportSetting> G_FJDailySaleReportSetting { get; set; }
        //2018-03-06
        public virtual DbSet<G_ScheduleTaskOnStore> G_ScheduleTaskOnStore { get; set; }
        //2018-08-10
        public virtual DbSet<G_GeneralSetting> G_GeneralSetting { get; set; }
        public virtual DbSet<G_SettingOnStore> G_SettingOnStore { get; set; }

        public virtual DbSet<I_StockSale> I_StockSale { get; set; }
        public virtual DbSet<I_StockSaleDetail> I_StockSaleDetail { get; set; }
        public virtual DbSet<I_StockSaleUsageDetail> I_StockSaleUsageDetail { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var typesToRegister = Assembly.GetAssembly(typeof(NuWebContext)).GetTypes().Where(type => type.Namespace != null
              && type.Namespace.Equals(typeof(NuWebContext).Namespace))
            .Where(type => type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }
            //...or do it manually below. For example,
            //modelBuilder.Configurations.Add(new LanguageMap());
        }

        #region Functions helper for report

        private static readonly Func<NuWebContext, BaseReportDataModel, List<ItemizedSalesAnalysisReportDataModels>> getItemsNoIncludeSale_New = (db, model) =>
        {
            return (from tb in db.R_ItemizedSalesAnalysisReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                        && tb.Mode == model.Mode

                    select new ItemizedSalesAnalysisReportDataModels
                    {
                        StoreId = tb.StoreId,
                        CreatedDate = tb.CreatedDate,
                        CategoryId = tb.CategoryId,
                        CategoryName = tb.CategoryName,
                        ExtraPrice = tb.ExtraPrice,
                        TotalPrice = tb.TotalPrice,
                        GLAccountCode = tb.GLAccountCode,
                        IsIncludeSale = tb.IsIncludeSale,
                        BusinessId = tb.BusinessId,
                        ServiceCharge = tb.ServiceCharge,
                        Tax = tb.Tax,
                        ExtraAmount = tb.ExtraAmount.HasValue ? tb.ExtraAmount.Value : 0,
                        TotalAmount = tb.TotalAmount.HasValue ? tb.TotalAmount.Value : 0,
                        TotalDiscount = tb.TotalDiscount.HasValue ? tb.TotalDiscount : 0,
                        ItemTotal = tb.TotalPrice + tb.ExtraPrice,
                        PromotionAmount = tb.PromotionAmount,
                        ReceiptId = tb.ReceiptId,
                        TaxType = tb.TaxType,
                        GiftCardId = tb.GiftCardId,
                        PoinsOrderId = tb.PoinsOrderId,
                        ItemTypeId = tb.ItemTypeId
                    }).ToList();
        };

        public List<ItemizedSalesAnalysisReportDataModels> GetItemsNoIncludeSale_New(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getItemsNoIncludeSale_New.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }


        private static readonly Func<NuWebContext, BaseReportDataModel, List<DailySalesReportInsertDataModels>> getDataReceipt_WithCreditNote = (db, model) =>
        {
            return (from ss in db.R_DailySalesReport.AsNoTracking()
                    where model.ListStores.Contains(ss.StoreId)
                        && model.FromDate <= ss.CreatedDate && model.ToDate >= ss.CreatedDate
                        && ss.Mode == model.Mode

                    select new DailySalesReportInsertDataModels
                    {
                        StoreId = ss.StoreId,
                        BusinessId = ss.BusinessId,
                        CreatedDate = ss.CreatedDate,
                        NoOfPerson = ss.NoOfPerson,
                        ReceiptTotal = ss.ReceiptTotal,      // Total R_Receipt
                        Discount = ss.Discount,
                        ServiceCharge = ss.ServiceCharge,
                        GST = ss.GST,
                        Rounding = ss.Rounding,
                        Refund = ss.Refund,
                        NetSales = ss.NetSales,
                        Tip = ss.Tip,
                        PromotionValue = ss.PromotionValue,
                        CreditNoteNo = ss.CreditNoteNo,
                        OrderId = ss.OrderId
                    }).ToList();
        };

        public List<DailySalesReportInsertDataModels> GetDataReceipt_WithCreditNote(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataReceipt_WithCreditNote.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        private static readonly Func<NuWebContext, BaseReportDataModel, List<PaymentDataModels>> getDataPaymentItems = (db, model) =>
        {
            return (from tb in db.G_PaymentMenthod.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                        && tb.Mode == model.Mode

                    select new PaymentDataModels
                    {
                        StoreId = tb.StoreId,
                        CreatedDate = tb.CreatedDate,
                        PaymentId = tb.PaymentId,
                        PaymentCode = tb.PaymentCode,
                        PaymentName = tb.PaymentName,
                        Amount = tb.Amount,
                        OrderId = tb.OrderId,
                        IsInclude = tb.IsInclude,
                    }).ToList();
        };

        public List<PaymentDataModels> GetDataPaymentItems(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataPaymentItems.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        private static readonly Func<NuWebContext, BaseReportDataModel, List<NoSaleDataModels>> getNoSaleDatas = (db, model) =>
        {
            return (from tb in db.R_NoSaleDetailReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate

                    select new NoSaleDataModels
                    {
                        Id = tb.Id,
                        StoreId = tb.StoreId,
                        CreatedDate = tb.CreatedDate,
                    }).ToList();
        };
        public List<NoSaleDataModels> GetNoSaleDatas(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getNoSaleDatas.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        private static readonly Func<NuWebContext, BaseReportDataModel, List<RevenueTempDataModels>> getRevenueDataChart = (db, model) =>
        {
            return (from tb in db.R_DailyReceiptReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                        && tb.Mode == model.Mode
                        && string.IsNullOrEmpty(tb.CreditNoteNo) // Only Receipt

                    select new RevenueTempDataModels
                    {
                        Id = tb.Id,
                        CreatedDate = tb.CreatedDate,
                        ReceiptTotal = tb.ReceiptTotal,
                        BusinessId = tb.BusinessDayId
                    }).ToList();
        };
        public List<RevenueTempDataModels> GetRevenueDataChart(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getRevenueDataChart.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        private static readonly Func<NuWebContext, BaseReportDataModel, List<TopSellingChartTmpDataModels>> getTopSellingChartReport = (db, model) =>
        {


            if (model.ItemType == 0)
            {
                return (from tb in db.R_TopSellingProductsReport.AsNoTracking()
                        where model.ListStores.Contains(tb.StoreId)
                            && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                            && tb.Mode == model.Mode
                        group tb by tb.ItemId into g
                        orderby g.Sum(s => s.Qty) descending
                        select new TopSellingChartTmpDataModels
                        {
                            ItemName = g.FirstOrDefault().ItemName,
                            Qty = g.Sum(s => s.Qty),
                            Amount = g.Sum(s => s.Amount)
                        }).Take(model.TopTake).ToList();
            }

            return (from tb in db.R_TopSellingProductsReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                        && tb.Mode == model.Mode
                    group tb by tb.ItemId into g
                    orderby g.Sum(s => s.Amount) descending
                    select new TopSellingChartTmpDataModels
                    {
                        ItemName = g.FirstOrDefault().ItemName,
                        Qty = g.Sum(s => s.Qty),
                        Amount = g.Sum(s => s.Amount)
                    }).Take(model.TopTake).ToList();
        };
        public List<TopSellingChartTmpDataModels> GetTopSellingChartReport(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getTopSellingChartReport.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }


        private static readonly Func<NuWebContext, BaseReportDataModel, List<CategoryTmpChartResponseDataModels>> getCategoryChartDataReport = (db, model) =>
        {

            return (from tb in db.R_ItemizedSalesAnalysisReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                        && tb.Mode == model.Mode
                        && !string.IsNullOrEmpty(tb.GLAccountCode) // Only Receipt

                    select new CategoryTmpChartResponseDataModels
                    {
                        GLAccountCode = tb.GLAccountCode,
                        CategoryName = tb.CategoryName,
                        Amount = (tb.TotalAmount.Value - tb.TotalDiscount.Value - tb.PromotionAmount - ((tb.TaxType == 2) ? tb.Tax : 0)),
                        CreateDate = tb.CreatedDate,
                        CategoryId = tb.CategoryId
                    }).ToList();
        };
        public List<CategoryTmpChartResponseDataModels> GetCategoryChartDataReport(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getCategoryChartDataReport.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        //For itemsale report
        private static readonly Func<NuWebContext, BaseReportDataModel, List<ItemizedSalesAnalysisReportDataModels>> getDataForItemSaleReport = (db, model) =>
        {

            return (from tb in db.R_ItemizedSalesAnalysisReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                        && tb.Mode == model.Mode
                        && tb.ItemTypeId != 9

                    select new ItemizedSalesAnalysisReportDataModels
                    {
                        CreatedDate = tb.CreatedDate,
                        StoreId = tb.StoreId,
                        // StoreName = model.ListStoreInfos.Where(ww => ww.Id == tb.StoreId).Select(ss => ss.Name).FirstOrDefault(),
                        CategoryId = tb.CategoryId,
                        CategoryName = tb.CategoryName,
                        ItemTypeId = tb.ItemTypeId,
                        ItemId = tb.ItemId,
                        ItemName = tb.ItemName,
                        Quantity = tb.Quantity,
                        Price = tb.Price,
                        Discount = tb.Discount,
                        Tax = tb.Tax,
                        ServiceCharge = tb.ServiceCharge,
                        Cost = tb.Cost,
                        ItemTotal = tb.TotalPrice + tb.ExtraPrice,
                        Percent = 0,
                        TotalCost = tb.Cost * (double)tb.Quantity,
                        ItemCode = tb.ItemCode,
                        PromotionAmount = tb.PromotionAmount,
                        // Updated 9202017
                        IsIncludeSale = tb.IsIncludeSale,
                        ExtraAmount = tb.ExtraAmount,
                        TotalAmount = tb.TotalAmount,
                        TotalDiscount = tb.TotalDiscount,
                        GiftCardId = tb.GiftCardId,
                        PoinsOrderId = tb.PoinsOrderId,
                        ReceiptId = tb.ReceiptId,
                        TaxType = tb.TaxType,
                    }).ToList();
        };
        public List<ItemizedSalesAnalysisReportDataModels> GetDataForItemSaleReport(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataForItemSaleReport.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        //For FJ daily sale
        private static readonly Func<NuWebContext, BaseReportDataModel, List<FJDailySalesReportDataModels>> getDataReceipt_WithCreditNoteForFJ = (db, model) =>
        {
            return (from ss in db.R_DailySalesReport.AsNoTracking()
                    where model.ListStores.Contains(ss.StoreId)
                        && model.FromDate <= ss.CreatedDate && model.ToDate >= ss.CreatedDate
                        && ss.Mode == model.Mode

                    select new FJDailySalesReportDataModels
                    {
                        StoreId = ss.StoreId,
                        CreatedDate = ss.CreatedDate,
                        ReceiptTotal = ss.ReceiptTotal,      // Total R_Receipt
                        Discount = ss.Discount,
                        ServiceCharge = ss.ServiceCharge,
                        GST = ss.GST,
                        Rounding = ss.Rounding,
                        Refund = ss.Refund,
                        NetSales = ss.NetSales,
                        CreditNoteNo = ss.CreditNoteNo,
                        ReceiptId = ss.OrderId
                    }).ToList();
        };

        public List<FJDailySalesReportDataModels> GetDataReceipt_WithCreditNoteForFJ(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataReceipt_WithCreditNoteForFJ.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        private static readonly Func<NuWebContext, BaseReportDataModel, List<RefundDataReportDTO>> getListRefundWithoutDetailsByReceiptId = (db, model) =>
        {
            return (from r in db.R_Refund.AsNoTracking()
                    where model.ListStores.Contains(r.StoreId)
                        && model.FromDate <= r.CreatedDate && model.ToDate >= r.CreatedDate
                    // && r.Mode == model.Mode
                    select new RefundDataReportDTO
                    {
                        Id = r.Id,
                        BusinessDayId = r.BusinessDayId,
                        CreatedDate = r.CreatedDate,
                        Description = r.Description,
                        StoreId = r.StoreId,
                        TotalRefund = r.TotalRefund,
                        Promotion = r.Promotion,
                        ServiceCharged = r.ServiceCharged,
                        OrderId = r.OrderId,
                        IsGiftCard = r.IsGiftCard.HasValue ? r.IsGiftCard.Value : false,
                    }).ToList();
        };

        public List<RefundDataReportDTO> GetListRefundWithoutDetailsByReceiptId(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getListRefundWithoutDetailsByReceiptId.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }
        //for close receipt report
        private static readonly Func<NuWebContext, BaseReportDataModel, List<ClosedReceiptReportDataModels>> getDataForCloseReceiptReport = (db, model) =>
        {
            var lstData = (from tb in db.R_ClosedReceiptReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                     && tb.Mode == model.Mode
                    select new ClosedReceiptReportDataModels
                    {
                        CreatedDate = tb.CreatedDate,
                        ReceiptNo = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.ReceiptNo : tb.CreditNoteNo, // Updated 03302018, refund Gift Card
                        CashierName = tb.CashierName,
                        CashierId = tb.CashierId,
                        TableNo = tb.TableNo,
                        NoOfPersion = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.NoOfPersion : 0, // if CreditNote, Number of person = 0
                        Total = string.IsNullOrEmpty(tb.CreditNoteNo) ? tb.Total : (tb.Total * (-1)), // Updated 03302018, refund Gift Card, amount with negative value
                        StoreId = tb.StoreId,
                        OrderId = tb.OrderId,
                        CreditNoteNo = tb.CreditNoteNo
                    }).ToList();
            var tips = db.G_OrderTip.AsNoTracking().Where(ww => model.ListStores.Contains(ww.StoreId) && ww.CreatedDate >= model.FromDate
                  && ww.CreatedDate <= model.ToDate && ww.Mode == model.Mode).ToList();

            if (lstData != null && lstData.Count > 0)
            {
                lstData = lstData.OrderBy(oo => oo.CreatedDate).ToList();
                //list order have tips
                var lstOrderTipIds = tips.Select(ss => ss.OrderId).ToList();
                var lstDataHaveTips = lstData.Where(ww => lstOrderTipIds.Contains(ww.OrderId)).ToList();
                foreach (var item in lstDataHaveTips)
                {
                    var obj = lstData.Where(ww => ww.OrderId == item.OrderId).FirstOrDefault();
                    obj.Total += tips.Where(ww => ww.OrderId == item.OrderId).Sum(ss => ss.Amount);
                }
            }
            return lstData;
        };

        public List<ClosedReceiptReportDataModels> GetDataForCloseReceiptReport(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataForCloseReceiptReport.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        //for daily receipt
        private static readonly Func<NuWebContext, BaseReportDataModel, List<DailyReceiptReportDataModels>> getDataForDailyReceiptReport = (db, model) =>
        {
            return (from tb in db.R_DailyReceiptReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                     && tb.Mode == model.Mode
                    select new DailyReceiptReportDataModels
                    {
                        BusinessDayId = tb.BusinessDayId,
                        ReceiptId = tb.ReceiptId,
                        ReceiptNo = tb.ReceiptNo,
                        StoreId = tb.StoreId,
                        CreatedDate = tb.CreatedDate,
                        NoOfPerson = tb.NoOfPerson,
                        ReceiptTotal = tb.ReceiptTotal,      // Total R_Receipt
                        Discount = tb.Discount,
                        ServiceCharge = tb.ServiceCharge,
                        GST = tb.GST,
                        Tips = tb.Tips,
                        Rounding = tb.Rounding,
                        NetSales = tb.NetSales,
                        PromotionValue = tb.PromotionValue,
                        CreditNoteNo = tb.CreditNoteNo
                    }).ToList();
        };

        public List<DailyReceiptReportDataModels> GetDataForDailyReceiptReport(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataForDailyReceiptReport.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        private static readonly Func<NuWebContext, BaseReportDataModel, List<ItemizedSalesAnalysisReportDataModels>> getListMisc = (db, model) =>
        {
            return (from tb in db.R_ItemizedSalesAnalysisReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                     && tb.Mode == model.Mode
                     && tb.ItemTypeId == 9
                    select new ItemizedSalesAnalysisReportDataModels
                    {
                        StoreId = tb.StoreId,
                        CreatedDate = tb.CreatedDate,
                        ReceiptId = tb.ReceiptId,
                        BusinessId = tb.BusinessId,
                        TotalPrice = tb.TotalPrice
                    }).ToList();
        };

        public List<ItemizedSalesAnalysisReportDataModels> GetListMisc(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getListMisc.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        //end for daily receipt

        //for daily itemized sale report
        private static readonly Func<NuWebContext, BaseReportDataModel, DailyItemizedSalesReportDetailPushDataResultModels> getDataForDailyItemizedSale = (db, model) =>
        {
            DailyItemizedSalesReportDetailPushDataResultModels objResult = new DailyItemizedSalesReportDetailPushDataResultModels();
            var queryDish = (from x in db.R_DailyItemizedSalesReportDetail.AsNoTracking()
                             where model.ListStores.Contains(x.StoreId)
                                 && model.FromDate <= x.CreatedDate && model.ToDate >= x.CreatedDate
                              && x.Mode == model.Mode
                             select new DailyItemizedSalesReportDetailDataModels
                             {
                                 CategoryId = x.CategoryId,
                                 CategoryName = x.CategoryName,
                                 ItemCode = x.ItemCode,
                                 ItemId = x.ItemId,
                                 ItemName = x.ItemName,
                                 StoreId = x.StoreId,
                                 CreatedDate = x.CreatedDate,
                                 BusinessId = x.BusinessId,
                                 Quantity = x.Quantity,
                                 Price = x.Price,
                                 ItemTypeId = 1,// dish
                                 CategoryTypeId = x.CategoryTypeId
                             }).ToList();

            var querySet = (from x in db.R_DailyItemizedSalesReportDetailForSet.AsNoTracking()
                            where model.ListStores.Contains(x.StoreId)
                                && model.FromDate <= x.CreatedDate && model.ToDate >= x.CreatedDate
                             && x.Mode == model.Mode
                            select new DailyItemizedSalesReportDetailForSetDataModels
                            {
                                CategoryId = x.CategoryId,
                                CategoryName = x.CategoryName,
                                StoreId = x.StoreId,
                                CreatedDate = x.CreatedDate,
                                Quantity = x.Quantity,
                                Price = x.Price,
                                BusinessId = x.BusinessId,
                                CategoryTypeId = x.CategoryTypeId

                            }).ToList();

            objResult.ListDailyItemizedSales = queryDish;
            objResult.ListDailyItemizedSalesForSet = querySet;

            return objResult;
        };

        public DailyItemizedSalesReportDetailPushDataResultModels GetDataForDailyItemizedSale(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataForDailyItemizedSale.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        //end for daily itemized sale report

        //for discount detail
        private static readonly Func<NuWebContext, BaseReportDataModel, List<DiscountDetailsReportDataModels>> getDataForDiscountDetail = (db, model) =>
        {
            return (from tb in db.R_DiscountDetailsReport.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                     && tb.Mode == model.Mode
                     && ((!string.IsNullOrEmpty(tb.PromotionId) && tb.PromotionValue != 0) || (!string.IsNullOrEmpty(tb.DiscountId) && tb.DiscountAmount != 0))

                    select new DiscountDetailsReportDataModels
                    {
                        Id = tb.Id,
                        CashierId = tb.CashierId,
                        CashierName = tb.CashierName,
                        CreatedDate = tb.CreatedDate,
                        DiscountAmount = tb.DiscountAmount,
                        DiscountId = tb.DiscountId,
                        DiscountName = tb.DiscountName,
                        ItemId = tb.ItemId,
                        ItemCode = tb.ItemCode,
                        ItemName = tb.ItemName,
                        ItemPrice = tb.ItemPrice,
                        Qty = tb.Qty,
                        ReceiptNo = tb.ReceiptNo,
                        StoreId = tb.StoreId,
                        IsDiscountValue = tb.IsDiscountValue,
                        DiscountType = tb.DiscountType,
                        BillTotal = tb.BillTotal,

                        ReceiptId = tb.ReceiptId,
                        PromotionId = tb.PromotionId,
                        PromotionName = tb.PromotionName,
                        PromotionValue = tb.PromotionValue.HasValue ? tb.PromotionValue.Value : 0
                    }).ToList();
        };

        public List<DiscountDetailsReportDataModels> GetDataForDiscountDetail(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataForDiscountDetail.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }
        //end for discount detail

        #region for item sale detail
        private static readonly Func<NuWebContext, BaseReportDataModel, List<ItemizedSalesAnalysisReportDetailDataModels>> getDataForItemSaleDetail = (db, model) =>
        {
            return (from tb in db.R_ItemizedSalesAnalysisReportDetail.AsNoTracking()
                    where model.ListStores.Contains(tb.StoreId)
                        && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                     && tb.Mode == model.Mode
                     && tb.ItemTypeId != 9

                    select new ItemizedSalesAnalysisReportDetailDataModels
                    {
                        Id = tb.Id,
                        CreatedDate = tb.CreatedDate,
                        BusinessId = tb.BusinessId,
                        StoreId = tb.StoreId,
                        CategoryId = tb.CategoryId,
                        CategoryName = tb.CategoryName,
                        ItemTypeId = tb.ItemTypeId,
                        ItemId = tb.ItemId,
                        ItemCode = tb.ItemCode,
                        ItemName = tb.ItemName,
                        ParentId = tb.ParentId,
                        Quantity = tb.Quantity,
                        Price = tb.Price,
                        TotalAmount = tb.TotalAmount
                    }).ToList();
        };

        public List<ItemizedSalesAnalysisReportDetailDataModels> GetDataForItemSaleDetail(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataForItemSaleDetail.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }
        #endregion end for item sale detail

        #region discount & misc
        private static readonly Func<NuWebContext, BaseReportDataModel, List<DiscountAndMiscReportDataModels>> getReceiptDiscountAndMisc = (db, model) =>
        {
            var lstData = new List<DiscountAndMiscReportDataModels>();
            var query = (from tb in db.R_DiscountAndMiscReport.AsNoTracking()
                         where model.ListStores.Contains(tb.StoreId)
                             && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                          && tb.Mode == model.Mode

                         select new
                         {
                             tb.StoreId,
                             tb.CreatedDate,
                             tb.DiscountValue,
                             tb.MiscValue
                         }).ToList();
            if (query != null && query.Any())
            {
                lstData = query.GroupBy(gg => new
                {
                    Hour = gg.CreatedDate.Hour,
                    StoreId = gg.StoreId,
                })
                    .Select(g => new DiscountAndMiscReportDataModels
                    {
                        StoreId = g.Key.StoreId,
                        Hour = g.Key.Hour,
                        TimeSpanHour = new TimeSpan(g.Key.Hour, 0, 0),
                        DiscountValue = g.Sum(ss => ss.DiscountValue),
                        MiscValue = g.Sum(ss => ss.MiscValue)
                    }).ToList();
            }

            return lstData;
        };

        public List<DiscountAndMiscReportDataModels> GetReceiptDiscountAndMisc(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getReceiptDiscountAndMisc.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        private static readonly Func<NuWebContext, BaseReportDataModel, List<DiscountAndMiscReportDataModels>> getDiscountTotal = (db, model) =>
        {
            var lstData = new List<DiscountAndMiscReportDataModels>();
            var query = (from tb in db.R_DiscountDetailsReport.AsNoTracking()
                         where model.ListStores.Contains(tb.StoreId)
                             && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                          && tb.Mode == model.Mode

                         select new
                         {
                             tb.StoreId,
                             tb.CreatedDate,
                             tb.DiscountAmount
                         }).ToList();
            if (query != null && query.Any())
            {
                lstData = query.GroupBy(gg => new
                {
                    Hour = gg.CreatedDate.Hour,
                    StoreId = gg.StoreId,
                })
                        .Select(g => new DiscountAndMiscReportDataModels
                        {
                            StoreId = g.Key.StoreId,
                            Hour = g.Key.Hour,
                            TimeSpanHour = new TimeSpan(g.Key.Hour, 0, 0),
                            DiscountValue = g.Sum(ss => ss.DiscountAmount)
                        }).ToList();
            }

            return lstData;
        };

        public List<DiscountAndMiscReportDataModels> GetDiscountTotal(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDiscountTotal.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }
        #endregion end discount & misc

        #region discount summary
        private static readonly Func<NuWebContext, BaseReportDataModel, List<DiscountSummaryReportDataModels>> getDataTotalSalesForDiscountSummary = (db, model) =>
        {
            var query = new List<DiscountSummaryReportDataModels>();
             query = (from tb in db.R_ClosedReceiptReport.AsNoTracking()
                         where model.ListStores.Contains(tb.StoreId)
                             && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                          && tb.Mode == model.Mode
                          && string.IsNullOrEmpty(tb.CreditNoteNo) // Only receipts

                         select new DiscountSummaryReportDataModels
                         {
                             ReceiptId = tb.OrderId,
                             StoreId = tb.StoreId,
                             CreateDate = tb.CreatedDate,
                             Amount = tb.Total,
                             IsTotalStore = true
                         }).ToList();
            if (query != null && query.Any())
            {
                var tips = db.G_OrderTip.Where(ww => model.ListStores.Contains(ww.StoreId) && ww.CreatedDate >= model.FromDate
                    && ww.CreatedDate <= model.ToDate && ww.Mode == model.Mode).ToList();

                query = query.OrderBy(oo => oo.CreateDate).ToList();
                foreach (var item in query)
                {
                    item.Amount += tips.Where(ww => ww.OrderId == item.ReceiptId).Sum(ss => ss.Amount);
                }
            }

            return query;
        };

        public List<DiscountSummaryReportDataModels> GetDataTotalSalesForDiscountSummary(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataTotalSalesForDiscountSummary.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }

        private static readonly Func<NuWebContext, BaseReportDataModel, List<DiscountSummaryReportDataModels>> getDataDiscountForDiscountSummary = (db, model) =>
        {
           return (from tb in db.R_DiscountDetailsReport.AsNoTracking()
                     where model.ListStores.Contains(tb.StoreId)
                         && model.FromDate <= tb.CreatedDate && model.ToDate >= tb.CreatedDate
                      && tb.Mode == model.Mode
                              && !string.IsNullOrEmpty(tb.DiscountId)
                              && tb.DiscountAmount != 0
                     select new DiscountSummaryReportDataModels
                     {
                         ReceiptId = tb.ReceiptId,
                         StoreId = tb.StoreId,
                         CreateDate = tb.CreatedDate,
                         Amount = tb.DiscountAmount,
                         DiscountId = tb.DiscountId,
                         DiscountName = tb.DiscountName
                     }).ToList();

        };

        public List<DiscountSummaryReportDataModels> GetDataDiscountForDiscountSummary(BaseReportDataModel model)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            var data = getDataDiscountForDiscountSummary.Invoke(this, model);
            this.Configuration.AutoDetectChangesEnabled = true;
            return data;
        }
        #endregion end discount summary
        #endregion End Functions helper for report
    }
}
