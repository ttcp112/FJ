namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FJDailySaleReportSettingTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.G_FJDailySaleReportSetting",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        GLAccountCodes = c.String(nullable: false, maxLength: 250),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 255),
                        LastUserModified = c.String(maxLength: 255),
                        LastDateModified = c.DateTime(),
                        IsActived = c.Boolean(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.G_FJDailySaleReportSetting");
        }
    }
}
