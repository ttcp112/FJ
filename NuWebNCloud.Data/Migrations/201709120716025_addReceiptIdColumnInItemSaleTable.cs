namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addReceiptIdColumnInItemSaleTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "ReceiptId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "ReceiptId");
        }
    }
}
