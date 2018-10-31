namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIsIncludeOnSaleInItemSaleTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "IsIncludeSale", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "IsIncludeSale");
        }
    }
}
