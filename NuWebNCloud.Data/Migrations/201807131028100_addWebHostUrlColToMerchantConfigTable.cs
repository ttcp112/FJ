namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addWebHostUrlColToMerchantConfigTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.G_POSAPIMerchantConfig", "WebHostUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.G_POSAPIMerchantConfig", "WebHostUrl");
        }
    }
}
