namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStartedCloseDayInStockCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_DataEntry", "StartedOn", c => c.DateTime());
            AddColumn("dbo.I_DataEntry", "ClosedOn", c => c.DateTime());
            AddColumn("dbo.I_DataEntryDetail", "Wastage", c => c.Double());
            AddColumn("dbo.I_StockCount", "StartedOn", c => c.DateTime());
            AddColumn("dbo.I_StockCount", "ClosedOn", c => c.DateTime());
            AddColumn("dbo.I_StockCountDetail", "Wastage", c => c.Double());
            DropColumn("dbo.I_DataEntryDetail", "Wast");
            DropColumn("dbo.I_StockCountDetail", "Wast");
        }
        
        public override void Down()
        {
            AddColumn("dbo.I_StockCountDetail", "Wast", c => c.Double());
            AddColumn("dbo.I_DataEntryDetail", "Wast", c => c.Double());
            DropColumn("dbo.I_StockCountDetail", "Wastage");
            DropColumn("dbo.I_StockCount", "ClosedOn");
            DropColumn("dbo.I_StockCount", "StartedOn");
            DropColumn("dbo.I_DataEntryDetail", "Wastage");
            DropColumn("dbo.I_DataEntry", "ClosedOn");
            DropColumn("dbo.I_DataEntry", "StartedOn");
        }
    }
}
