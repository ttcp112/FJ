namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPosSaleTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.R_PosSale",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        OrderId = c.String(nullable: false, maxLength: 50),
                        OrderNo = c.String(nullable: false, maxLength: 50),
                        ReceiptNo = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        OrderStatus = c.Int(nullable: false),
                        TableNo = c.String(nullable: false, maxLength: 50),
                        NoOfPersion = c.Int(nullable: false),
                        CancelAmount = c.Double(nullable: false),
                        ReceiptTotal = c.Double(nullable: false),
                        Discount = c.Double(nullable: false),
                        Tip = c.Double(nullable: false),
                        PromotionValue = c.Double(nullable: false),
                        ServiceCharge = c.Double(nullable: false),
                        GST = c.Double(nullable: false),
                        Rounding = c.Double(nullable: false),
                        Refund = c.Double(nullable: false),
                        NetSales = c.Double(nullable: false),
                        CashierId = c.String(nullable: false, maxLength: 50),
                        CashierName = c.String(nullable: false, maxLength: 350),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 60),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_PosSaleDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        OrderId = c.String(nullable: false, maxLength: 50),
                        OrderDetailId = c.String(nullable: false, maxLength: 50),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemTypeId = c.Int(nullable: false),
                        ParentId = c.String(nullable: false, maxLength: 50),
                        ItemCode = c.String(nullable: false, maxLength: 50),
                        ItemName = c.String(nullable: false, maxLength: 350),
                        CategoryId = c.String(nullable: false, maxLength: 50),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        GLAccountCode = c.String(maxLength: 50),
                        Quantity = c.Double(nullable: false),
                        Price = c.Double(nullable: false),
                        ExtraPrice = c.Double(nullable: false),
                        TotalAmount = c.Double(nullable: false),
                        Discount = c.Double(nullable: false),
                        Cost = c.Double(nullable: false),
                        ServiceCharge = c.Double(nullable: false),
                        Tax = c.Double(nullable: false),
                        PromotionAmount = c.Double(nullable: false),
                        PoinsOrderId = c.String(maxLength: 50),
                        GiftCardId = c.String(maxLength: 50),
                        IsIncludeSale = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 60),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.R_PosSaleDetail");
            DropTable("dbo.R_PosSale");
        }
    }
}
