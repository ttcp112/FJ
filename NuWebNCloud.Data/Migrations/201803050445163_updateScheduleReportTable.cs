namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateScheduleReportTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.G_ScheduleTaskOnStore",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ScheduleTaskId = c.String(nullable: false, maxLength: 50),
                        StoreId = c.String(nullable: false, maxLength: 60),
                        Description = c.String(maxLength: 450),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 250),
                        LastUserModified = c.String(nullable: false, maxLength: 250),
                        LastDateModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.G_ScheduleTask", t => t.ScheduleTaskId)
                .Index(t => t.ScheduleTaskId);
            
            AddColumn("dbo.G_ScheduleTask", "ReportName", c => c.String(maxLength: 350));
            AddColumn("dbo.G_ScheduleTask", "IsWeekly", c => c.Boolean(nullable: false));
            AddColumn("dbo.G_ScheduleTask", "IsMonthly", c => c.Boolean(nullable: false));
            AddColumn("dbo.G_ScheduleTrackingLog", "StoreIds", c => c.String(nullable: false, maxLength: 1000));
            AddColumn("dbo.G_ScheduleTrackingLog", "Description", c => c.String(maxLength: 1000));
            AlterColumn("dbo.G_ScheduleTask", "EmailSubject", c => c.String(nullable: false, maxLength: 450));
            AlterColumn("dbo.G_ScheduleTask", "CreatedUser", c => c.String(nullable: false, maxLength: 250));
            AlterColumn("dbo.G_ScheduleTask", "LastUserModified", c => c.String(nullable: false, maxLength: 250));
            AlterColumn("dbo.G_ScheduleTrackingLog", "ReportId", c => c.String(nullable: false, maxLength: 60));
            AlterColumn("dbo.G_ScheduleTrackingLog", "DateSend", c => c.DateTime());
            DropColumn("dbo.G_ScheduleTask", "IsMonth");
            DropColumn("dbo.G_ScheduleTask", "LastSuccessUtc");
            DropColumn("dbo.G_ScheduleTask", "StoreName");
            DropColumn("dbo.G_ScheduleTask", "StoreId");
            DropColumn("dbo.G_ScheduleTrackingLog", "StoreId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.G_ScheduleTrackingLog", "StoreId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.G_ScheduleTask", "StoreId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.G_ScheduleTask", "StoreName", c => c.String(maxLength: 250));
            AddColumn("dbo.G_ScheduleTask", "LastSuccessUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.G_ScheduleTask", "IsMonth", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.G_ScheduleTaskOnStore", "ScheduleTaskId", "dbo.G_ScheduleTask");
            DropIndex("dbo.G_ScheduleTaskOnStore", new[] { "ScheduleTaskId" });
            AlterColumn("dbo.G_ScheduleTrackingLog", "DateSend", c => c.DateTime(nullable: false));
            AlterColumn("dbo.G_ScheduleTrackingLog", "ReportId", c => c.String(nullable: false, maxLength: 350));
            AlterColumn("dbo.G_ScheduleTask", "LastUserModified", c => c.String(nullable: false));
            AlterColumn("dbo.G_ScheduleTask", "CreatedUser", c => c.String());
            AlterColumn("dbo.G_ScheduleTask", "EmailSubject", c => c.String(nullable: false, maxLength: 250));
            DropColumn("dbo.G_ScheduleTrackingLog", "Description");
            DropColumn("dbo.G_ScheduleTrackingLog", "StoreIds");
            DropColumn("dbo.G_ScheduleTask", "IsMonthly");
            DropColumn("dbo.G_ScheduleTask", "IsWeekly");
            DropColumn("dbo.G_ScheduleTask", "ReportName");
            DropTable("dbo.G_ScheduleTaskOnStore");
        }
    }
}
