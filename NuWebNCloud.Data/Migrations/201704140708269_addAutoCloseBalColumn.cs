namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAutoCloseBalColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_StockCountDetail", "AutoCloseBal", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_StockCountDetail", "AutoCloseBal");
        }
    }
}
