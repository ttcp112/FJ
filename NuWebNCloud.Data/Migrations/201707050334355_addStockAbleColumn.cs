namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStockAbleColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Ingredient", "StockAble", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Ingredient", "StockAble");
        }
    }
}
