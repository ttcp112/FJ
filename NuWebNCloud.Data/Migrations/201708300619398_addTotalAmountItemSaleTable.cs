namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTotalAmountItemSaleTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "TotalDiscount", c => c.Double());
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "TotalAmount", c => c.Double());
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "ExtraAmount", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "ExtraAmount");
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "TotalAmount");
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "TotalDiscount");
        }
    }
}
