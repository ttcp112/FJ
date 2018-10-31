namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNoIncludeOnSaletable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.R_NoIncludeOnSaleDataReport",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        OrderId = c.String(nullable: false, maxLength: 50),
                        ReceiptNo = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        CategoryId = c.String(nullable: false, maxLength: 50),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        ProductId = c.String(nullable: false, maxLength: 50),
                        ProductName = c.String(nullable: false, maxLength: 350),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Price = c.Double(nullable: false),
                        Amount = c.Double(nullable: false),
                        Tax = c.Double(nullable: false),
                        ServiceCharged = c.Double(nullable: false),
                        DiscountAmount = c.Double(nullable: false),
                        PromotionAmount = c.Double(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.R_NoIncludeOnSaleDataReport");
        }
    }
}
