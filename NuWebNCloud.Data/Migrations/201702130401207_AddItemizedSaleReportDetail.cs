namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddItemizedSaleReportDetail : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.R_ItemizedSalesAnalysisReportDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        ItemId = c.String(nullable: false, maxLength: 50),
                        ItemCode = c.String(nullable: false, maxLength: 50),
                        ItemTypeId = c.Int(nullable: false),
                        ItemName = c.String(nullable: false, maxLength: 350),
                        ParentId = c.String(nullable: false, maxLength: 50),
                        Price = c.Double(nullable: false),
                        Quantity = c.Double(nullable: false),
                        TotalAmount = c.Double(nullable: false),
                        CategoryId = c.String(nullable: false, maxLength: 50),
                        CategoryName = c.String(nullable: false, maxLength: 350),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.R_ItemizedSalesAnalysisReportDetail");
        }
    }
}
