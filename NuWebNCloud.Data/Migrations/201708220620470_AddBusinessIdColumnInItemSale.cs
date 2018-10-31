namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBusinessIdColumnInItemSale : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_DailySalesReport", "BusinessId", c => c.String());
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "BusinessId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "BusinessId");
            DropColumn("dbo.R_DailySalesReport", "BusinessId");
        }
    }
}
