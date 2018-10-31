namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeDayOfWeeksColumnInScheduleTaskTable : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.G_ScheduleTask", "DayOfWeeks");
        }
        
        public override void Down()
        {
            AddColumn("dbo.G_ScheduleTask", "DayOfWeeks", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
