namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.G_BusinessDay",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        StartedOn = c.DateTime(nullable: false),
                        ClosedOn = c.DateTime(),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 255),
                        LastUserModified = c.String(maxLength: 255),
                        LastDateModified = c.DateTime(),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_DateOfWeeks",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        DayNumber = c.Int(nullable: false),
                        DayName = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 255),
                        LastUserModified = c.String(nullable: false, maxLength: 255),
                        LastDateModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_Industry",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsPublic = c.Boolean(nullable: false),
                        Status = c.Byte(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 255),
                        ModifiedUser = c.String(nullable: false, maxLength: 255),
                        LastModified = c.DateTime(nullable: false),
                        AreaName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.G_OrderTip",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        OrderId = c.String(nullable: false, maxLength: 50),
                        Amount = c.Double(nullable: false),
                        PaymentId = c.String(nullable: false, maxLength: 50),
                        PaymentName = c.String(nullable: false, maxLength: 150),
                        Mode = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_PaymentMenthod",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        OrderId = c.String(nullable: false, maxLength: 50),
                        PaymentId = c.String(nullable: false, maxLength: 50),
                        PaymentName = c.String(nullable: false, maxLength: 250),
                        Amount = c.Double(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 255),
                        LastUserModified = c.String(nullable: false, maxLength: 255),
                        LastDateModified = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_ScheduleTask",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReportId = c.String(nullable: false, maxLength: 100),
                        EmailSubject = c.String(nullable: false, maxLength: 250),
                        Email = c.String(nullable: false, maxLength: 250),
                        Cc = c.String(maxLength: 1000),
                        Bcc = c.String(maxLength: 1000),
                        DayOfWeeks = c.String(nullable: false, maxLength: 50),
                        Hour = c.Int(nullable: false),
                        Minute = c.Int(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        IsDaily = c.Boolean(nullable: false),
                        IsMonth = c.Boolean(nullable: false),
                        LastSuccessUtc = c.DateTime(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(),
                        LastUserModified = c.String(nullable: false),
                        LastDateModified = c.DateTime(nullable: false),
                        StoreName = c.String(maxLength: 250),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_ScheduleTrackingLog",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReportId = c.String(nullable: false, maxLength: 350),
                        DateSend = c.DateTime(nullable: false),
                        IsSend = c.Boolean(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_Tax",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 350),
                        IsActive = c.Boolean(nullable: false),
                        Percent = c.Double(nullable: false),
                        TaxType = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        UserCreated = c.String(nullable: false, maxLength: 350),
                        StoreId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_TrackingLog",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        TableName = c.String(nullable: false, maxLength: 200),
                        JsonContent = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        IsDone = c.Boolean(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_User",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 350),
                        Password = c.String(nullable: false),
                        Name = c.String(nullable: false, maxLength: 350),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 255),
                        LastUserModified = c.String(nullable: false, maxLength: 255),
                        LastDateModified = c.DateTime(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Ingredient",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Code = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 3000),
                        BaseUOMName = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        Status = c.Int(),
                        PurchasePrice = c.Double(nullable: false),
                        SalePrice = c.Double(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(maxLength: 250),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Ingredient_UOM",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        UOMId = c.String(maxLength: 50),
                        BaseUOM = c.Double(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(maxLength: 250),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_IngredientTrackLog",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Code = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_InventoryManagement",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        Quantity = c.Double(nullable: false),
                        Price = c.Double(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_ReceiptNote",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReceiptNo = c.String(maxLength: 50),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(maxLength: 250),
                        UpdatedDate = c.DateTime(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_ReceiptNoteDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReceiptNoteId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        Quantity = c.Double(nullable: false),
                        Price = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Recipe_Ingredient",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        MixtureIngredientId = c.String(nullable: false, maxLength: 50),
                        UOMId = c.String(nullable: false, maxLength: 50),
                        Usage = c.Double(nullable: false),
                        Status = c.Byte(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(nullable: false, maxLength: 250),
                        UpdatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Recipe_Item",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemName = c.String(nullable: false, maxLength: 250),
                        ItemType = c.Int(nullable: false),
                        UOMId = c.String(nullable: false, maxLength: 50),
                        Usage = c.Double(nullable: false),
                        Status = c.Byte(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(nullable: false, maxLength: 250),
                        UpdatedDate = c.DateTime(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Recipe_Modifier",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        ModifierId = c.String(nullable: false, maxLength: 50),
                        ModifierName = c.String(nullable: false, maxLength: 250),
                        UOMId = c.String(nullable: false, maxLength: 50),
                        Usage = c.Double(nullable: false),
                        Status = c.Byte(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(nullable: false, maxLength: 250),
                        UpdatedDate = c.DateTime(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_StockUsage",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemName = c.String(nullable: false, maxLength: 250),
                        Quantity = c.Double(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_StoreSetting",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        ReorderingQuantity = c.Double(nullable: false),
                        MinAltert = c.Double(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(maxLength: 250),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_UnitOfMeasure",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Code = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(nullable: false),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_UsageManagement",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        DateFrom = c.DateTime(nullable: false),
                        DateTo = c.DateTime(nullable: false),
                        IsStockInventory = c.Boolean(nullable: false),
                        StoreId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_UsageManagementDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        UsageManagementId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        Usage = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_UsageManagementItemDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IndexList = c.Int(nullable: false),
                        UsageManagementDetailId = c.String(nullable: false, maxLength: 50),
                        BusinessDay = c.String(nullable: false, maxLength: 50),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemName = c.String(nullable: false, maxLength: 250),
                        Qty = c.Double(nullable: false),
                        Usage = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_UsageManagementXeroTrackLog",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        StoreId = c.String(nullable: false, maxLength: 50),
                        ToDate = c.DateTime(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_AuditTrailReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReceiptDate = c.DateTime(nullable: false),
                        BusinessDayId = c.String(nullable: false, maxLength: 50),
                        ReceiptID = c.String(nullable: false, maxLength: 50),
                        ReceiptNo = c.String(nullable: false, maxLength: 50),
                        ReceiptStatus = c.Int(nullable: false),
                        TransNo = c.String(nullable: false, maxLength: 50),
                        CashierId = c.String(nullable: false, maxLength: 50),
                        CashierName = c.String(nullable: false, maxLength: 350),
                        Discount = c.Double(nullable: false),
                        TotalRefund = c.Double(nullable: false),
                        ReceiptTotal = c.Double(nullable: false),
                        CancelAmount = c.Double(nullable: false),
                        StoreId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_CashInOutReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessDayId = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        DrawerId = c.String(nullable: false, maxLength: 50),
                        DrawerName = c.String(nullable: false, maxLength: 350),
                        CashValue = c.Double(nullable: false),
                        StartOn = c.DateTime(nullable: false),
                        EndOn = c.DateTime(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 350),
                        CashType = c.Int(nullable: false),
                        Mode = c.Int(nullable: false),
                        ShiftStartOn = c.DateTime(nullable: false),
                        ShiftEndOn = c.DateTime(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_ClosedReceiptReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        CashierId = c.String(nullable: false, maxLength: 50),
                        CashierName = c.String(nullable: false, maxLength: 350),
                        NoOfPersion = c.Int(nullable: false),
                        Total = c.Double(nullable: false),
                        OrderId = c.String(maxLength: 50),
                        ReceiptNo = c.String(nullable: false, maxLength: 100),
                        TableNo = c.String(nullable: false, maxLength: 100),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_DailyReceiptReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        BusinessDayId = c.String(nullable: false, maxLength: 50),
                        ReceiptId = c.String(nullable: false, maxLength: 50),
                        ReceiptNo = c.String(nullable: false, maxLength: 50),
                        NoOfPerson = c.Int(nullable: false),
                        ReceiptTotal = c.Double(nullable: false),
                        Discount = c.Double(nullable: false),
                        ServiceCharge = c.Double(nullable: false),
                        GST = c.Double(nullable: false),
                        Tips = c.Double(nullable: false),
                        Rounding = c.Double(nullable: false),
                        PromotionValue = c.Double(nullable: false),
                        NetSales = c.Double(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_DailySalesReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        OrderId = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        NoOfPerson = c.Int(nullable: false),
                        ReceiptTotal = c.Double(nullable: false),
                        Discount = c.Double(nullable: false),
                        Tip = c.Double(nullable: false),
                        PromotionValue = c.Double(nullable: false),
                        ServiceCharge = c.Double(nullable: false),
                        GST = c.Double(nullable: false),
                        Rounding = c.Double(nullable: false),
                        Refund = c.Double(nullable: false),
                        NetSales = c.Double(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_DetailItemizedSalesAnalysisReportHeader",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        CategoryId = c.String(nullable: false, maxLength: 50),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemName = c.String(nullable: false, maxLength: 350),
                        ItemTypeId = c.Int(nullable: false),
                        Qty = c.Double(nullable: false),
                        Price = c.Double(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_DiscountAndMiscReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        MiscValue = c.Double(nullable: false),
                        DiscountValue = c.Double(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_DiscountDetailsReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessDayId = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ReceiptId = c.String(nullable: false, maxLength: 50),
                        ReceiptNo = c.String(nullable: false, maxLength: 50),
                        CashierId = c.String(nullable: false, maxLength: 50),
                        CashierName = c.String(nullable: false, maxLength: 350),
                        DiscountId = c.String(nullable: false, maxLength: 50),
                        DiscountName = c.String(nullable: false, maxLength: 350),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemCode = c.String(nullable: false, maxLength: 50),
                        ItemName = c.String(nullable: false, maxLength: 500),
                        Qty = c.Double(nullable: false),
                        ItemPrice = c.Double(nullable: false),
                        DiscountAmount = c.Double(nullable: false),
                        DiscountType = c.Int(nullable: false),
                        IsDiscountValue = c.Boolean(nullable: false),
                        BillTotal = c.Double(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_HourlyItemizedSalesReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ItemTypeId = c.Int(nullable: false),
                        CategoryId = c.String(nullable: false, maxLength: 50),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        TotalPrice = c.Double(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_HourlySalesReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReceiptTotal = c.Double(nullable: false),
                        NetSales = c.Double(nullable: false),
                        ReceiptId = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        NoOfPerson = c.Int(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_ItemizedSalesAnalysisReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemCode = c.String(nullable: false, maxLength: 50),
                        ItemTypeId = c.Int(nullable: false),
                        ItemName = c.String(nullable: false, maxLength: 350),
                        Price = c.Double(nullable: false),
                        ExtraPrice = c.Double(nullable: false),
                        TotalPrice = c.Double(nullable: false),
                        Quantity = c.Double(nullable: false),
                        Discount = c.Double(nullable: false),
                        Cost = c.Double(nullable: false),
                        CategoryId = c.String(nullable: false, maxLength: 50),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        ServiceCharge = c.Double(nullable: false),
                        Tax = c.Double(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        PromotionAmount = c.Double(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_NoSaleDetailReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        CashierId = c.String(nullable: false, maxLength: 50),
                        CashierName = c.String(nullable: false, maxLength: 350),
                        DrawerId = c.String(nullable: false, maxLength: 50),
                        DrawerName = c.String(nullable: false, maxLength: 350),
                        Reason = c.String(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_ReceiptsbyPaymentMethodsReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        PaymentId = c.String(nullable: false, maxLength: 50),
                        PaymentName = c.String(nullable: false, maxLength: 350),
                        CreatedDate = c.DateTime(nullable: false),
                        ReceiptNo = c.String(nullable: false, maxLength: 50),
                        ReceiptRefund = c.Double(nullable: false),
                        ReceiptTotal = c.Double(nullable: false),
                        Total = c.Double(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_Refund",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        OrderId = c.String(),
                        ReceiptDate = c.DateTime(nullable: false),
                        TotalRefund = c.Double(nullable: false),
                        Description = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        BusinessDayId = c.String(),
                        ServiceCharged = c.Double(nullable: false),
                        Tax = c.Double(nullable: false),
                        Discount = c.Double(nullable: false),
                        Promotion = c.Double(nullable: false),
                        CreatedUser = c.String(),
                        StoreId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_RefundDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        RefundId = c.String(nullable: false, maxLength: 50),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemName = c.String(nullable: false, maxLength: 250),
                        ItemType = c.Int(nullable: false),
                        Qty = c.Double(nullable: false),
                        ServiceCharged = c.Double(nullable: false),
                        Tax = c.Double(nullable: false),
                        PromotionAmount = c.Double(nullable: false),
                        PriceValue = c.Double(nullable: false),
                        DiscountAmount = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_TimeClockReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        DayOfWeeks = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 50),
                        UserName = c.String(nullable: false, maxLength: 350),
                        DateTimeIn = c.DateTime(nullable: false),
                        DateTimeOut = c.DateTime(nullable: false),
                        Early = c.Double(nullable: false),
                        Late = c.Double(nullable: false),
                        HoursWork = c.Double(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_TopSellingProductsReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemName = c.String(nullable: false, maxLength: 500),
                        Qty = c.Double(nullable: false),
                        Discount = c.Double(nullable: false),
                        Amount = c.Double(nullable: false),
                        Total = c.Double(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.R_TopSellingProductsReport");
            DropTable("dbo.R_TimeClockReport");
            DropTable("dbo.R_RefundDetail");
            DropTable("dbo.R_Refund");
            DropTable("dbo.R_ReceiptsbyPaymentMethodsReport");
            DropTable("dbo.R_NoSaleDetailReport");
            DropTable("dbo.R_ItemizedSalesAnalysisReport");
            DropTable("dbo.R_HourlySalesReport");
            DropTable("dbo.R_HourlyItemizedSalesReport");
            DropTable("dbo.R_DiscountDetailsReport");
            DropTable("dbo.R_DiscountAndMiscReport");
            DropTable("dbo.R_DetailItemizedSalesAnalysisReportHeader");
            DropTable("dbo.R_DailySalesReport");
            DropTable("dbo.R_DailyReceiptReport");
            DropTable("dbo.R_ClosedReceiptReport");
            DropTable("dbo.R_CashInOutReport");
            DropTable("dbo.R_AuditTrailReport");
            DropTable("dbo.I_UsageManagementXeroTrackLog");
            DropTable("dbo.I_UsageManagementItemDetail");
            DropTable("dbo.I_UsageManagementDetail");
            DropTable("dbo.I_UsageManagement");
            DropTable("dbo.I_UnitOfMeasure");
            DropTable("dbo.I_StoreSetting");
            DropTable("dbo.I_StockUsage");
            DropTable("dbo.I_Recipe_Modifier");
            DropTable("dbo.I_Recipe_Item");
            DropTable("dbo.I_Recipe_Ingredient");
            DropTable("dbo.I_ReceiptNoteDetail");
            DropTable("dbo.I_ReceiptNote");
            DropTable("dbo.I_InventoryManagement");
            DropTable("dbo.I_IngredientTrackLog");
            DropTable("dbo.I_Ingredient_UOM");
            DropTable("dbo.I_Ingredient");
            DropTable("dbo.G_User");
            DropTable("dbo.G_TrackingLog");
            DropTable("dbo.G_Tax");
            DropTable("dbo.G_ScheduleTrackingLog");
            DropTable("dbo.G_ScheduleTask");
            DropTable("dbo.G_PaymentMenthod");
            DropTable("dbo.G_OrderTip");
            DropTable("dbo.G_Industry");
            DropTable("dbo.G_DateOfWeeks");
            DropTable("dbo.G_BusinessDay");
        }
    }
}
