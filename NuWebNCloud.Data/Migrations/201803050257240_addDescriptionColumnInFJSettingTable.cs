namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDescriptionColumnInFJSettingTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.G_FJDailySaleReportSetting", "Description", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.G_FJDailySaleReportSetting", "Description");
        }
    }
}
