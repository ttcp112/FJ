namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addLanguageTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.G_Language",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 150),
                        Symbol = c.String(nullable: false, maxLength: 50),
                        Status = c.Int(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 250),
                        ModifiedUser = c.String(nullable: false, maxLength: 250),
                        LastModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_LanguageLink",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 250),
                        ModifiedUser = c.String(nullable: false, maxLength: 250),
                        LastModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_LanguageLinkDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        LanguageId = c.String(nullable: false, maxLength: 50),
                        LanguageLinkId = c.String(nullable: false, maxLength: 50),
                        Text = c.String(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUser = c.String(nullable: false, maxLength: 250),
                        ModifiedUser = c.String(nullable: false, maxLength: 250),
                        LastModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.G_LanguageLinkDetail");
            DropTable("dbo.G_LanguageLink");
            DropTable("dbo.G_Language");
        }
    }
}
