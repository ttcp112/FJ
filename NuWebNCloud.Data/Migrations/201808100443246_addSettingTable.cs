namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSettingTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.G_SettingOnStore",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        StoreId = c.String(nullable: false, maxLength: 50),
                        SettingId = c.String(nullable: false, maxLength: 50),
                        Value = c.String(nullable: false),
                        Status = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(),
                        CreatedUser = c.String(maxLength: 250),
                        LastDateModified = c.DateTime(),
                        LastUserModified = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.G_GeneralSetting", t => t.SettingId)
                .Index(t => t.SettingId);
            
            CreateTable(
                "dbo.G_GeneralSetting",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Code = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 250),
                        DisplayName = c.String(nullable: false, maxLength: 350),
                        Value = c.String(nullable: false),
                        Status = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(),
                        CreatedUser = c.String(maxLength: 250),
                        LastDateModified = c.DateTime(),
                        LastUserModified = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.G_SettingOnStore", "SettingId", "dbo.G_GeneralSetting");
            DropIndex("dbo.G_SettingOnStore", new[] { "SettingId" });
            DropTable("dbo.G_GeneralSetting");
            DropTable("dbo.G_SettingOnStore");
        }
    }
}
