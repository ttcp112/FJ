namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addVersionColumnInMerchantConfigTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.G_POSAPIMerchantConfig", "POSInstanceVersion", c => c.Int());
            AddColumn("dbo.R_HourlyItemizedSalesReport", "ItemId", c => c.String(maxLength: 60));
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_HourlyItemizedSalesReport", "ItemId");
            DropColumn("dbo.G_POSAPIMerchantConfig", "POSInstanceVersion");
        }
    }
}
