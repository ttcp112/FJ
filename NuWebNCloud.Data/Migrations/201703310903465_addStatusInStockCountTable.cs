namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStatusInStockCountTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_StockCount", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_StockCount", "Status");
        }
    }
}
