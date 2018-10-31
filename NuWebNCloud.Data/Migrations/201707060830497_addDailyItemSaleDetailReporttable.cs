namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDailyItemSaleDetailReporttable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.R_DailyItemizedSalesReportDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 100),
                        ItemId = c.String(nullable: false, maxLength: 250),
                        ItemCode = c.String(maxLength: 100),
                        ItemName = c.String(nullable: false, maxLength: 350),
                        ItemTypeId = c.Int(nullable: false),
                        CategoryId = c.String(nullable: false, maxLength: 100),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        CategoryTypeId = c.Int(nullable: false),
                        Price = c.Double(nullable: false),
                        Quantity = c.Double(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.R_DailyItemizedSalesReportDetailForSet",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 100),
                        CategoryId = c.String(nullable: false, maxLength: 100),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        CategoryTypeId = c.Int(nullable: false),
                        Price = c.Double(nullable: false),
                        Quantity = c.Double(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.R_DailyItemizedSalesReportDetailForSet");
            DropTable("dbo.R_DailyItemizedSalesReportDetail");
        }
    }
}
