namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateMerchantNameColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.G_POSAPIMerchantConfig", "NuPOSInstance", c => c.String(maxLength: 250));
            DropColumn("dbo.G_POSAPIMerchantConfig", "MerchantName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.G_POSAPIMerchantConfig", "MerchantName", c => c.String(maxLength: 250));
            DropColumn("dbo.G_POSAPIMerchantConfig", "NuPOSInstance");
        }
    }
}
