namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStockTransferNoColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Stock_Transfer", "StockTransferNo", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Stock_Transfer", "StockTransferNo");
        }
    }
}
