namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMorningColumnInMerchantConfig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.G_POSAPIMerchantConfig", "MorningStart", c => c.Time(precision: 7));
            AddColumn("dbo.G_POSAPIMerchantConfig", "MorningEnd", c => c.Time(precision: 7));
            AddColumn("dbo.G_POSAPIMerchantConfig", "MidDayStart", c => c.Time(precision: 7));
            AddColumn("dbo.G_POSAPIMerchantConfig", "MidDayEnd", c => c.Time(precision: 7));
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "GLAccountCode", c => c.String());
            AddColumn("dbo.R_NoIncludeOnSaleDataReport", "GLAccountCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_NoIncludeOnSaleDataReport", "GLAccountCode");
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "GLAccountCode");
            DropColumn("dbo.G_POSAPIMerchantConfig", "MidDayEnd");
            DropColumn("dbo.G_POSAPIMerchantConfig", "MidDayStart");
            DropColumn("dbo.G_POSAPIMerchantConfig", "MorningEnd");
            DropColumn("dbo.G_POSAPIMerchantConfig", "MorningStart");
        }
    }
}
