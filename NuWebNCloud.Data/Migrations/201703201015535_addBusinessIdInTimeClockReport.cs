namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBusinessIdInTimeClockReport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_TimeClockReport", "BusinessId", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_TimeClockReport", "BusinessId");
        }
    }
}
