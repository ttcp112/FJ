namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTableCancelAndRefund : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.R_ItemizedCancelOrRefundData",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        OrderId = c.String(nullable: false, maxLength: 50),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemCode = c.String(nullable: false, maxLength: 50),
                        ItemTypeId = c.Int(nullable: false),
                        ItemName = c.String(nullable: false, maxLength: 450),
                        Price = c.Double(nullable: false),
                        Quantity = c.Double(nullable: false),
                        Amount = c.Double(nullable: false),
                        CategoryId = c.String(nullable: false, maxLength: 50),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        CancelUser = c.String(nullable: false, maxLength: 350),
                        RefundUser = c.String(nullable: false, maxLength: 350),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.R_AuditTrailReport", "PromotionAmount", c => c.Double());
            AddColumn("dbo.R_DiscountDetailsReport", "UserDiscount", c => c.String(maxLength: 350));
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_DiscountDetailsReport", "UserDiscount");
            DropColumn("dbo.R_AuditTrailReport", "PromotionAmount");
            DropTable("dbo.R_ItemizedCancelOrRefundData");
        }
    }
}
