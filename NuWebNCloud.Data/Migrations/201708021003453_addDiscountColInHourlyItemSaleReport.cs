namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDiscountColInHourlyItemSaleReport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_HourlyItemizedSalesReport", "Discount", c => c.Double(nullable: false));
            AddColumn("dbo.R_HourlyItemizedSalesReport", "Promotion", c => c.Double(nullable: false));
            AddColumn("dbo.R_HourlyItemizedSalesReport", "BusinessId", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_HourlyItemizedSalesReport", "BusinessId");
            DropColumn("dbo.R_HourlyItemizedSalesReport", "Promotion");
            DropColumn("dbo.R_HourlyItemizedSalesReport", "Discount");
        }
    }
}
