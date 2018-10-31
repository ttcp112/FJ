namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addisDiscountTotalColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_HourlyItemizedSalesReport", "IsDiscountTotal", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_HourlyItemizedSalesReport", "IsDiscountTotal");
        }
    }
}
