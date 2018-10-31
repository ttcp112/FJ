namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addGCIdInItemSaleTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "PoinsOrderId", c => c.String(maxLength: 60));
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "GiftCardId", c => c.String(maxLength: 60));
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "GiftCardId");
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "PoinsOrderId");
        }
    }
}
