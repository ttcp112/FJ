namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addMultiLocationTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.G_MultiLocationConfig",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        POSEmployeeConfigId = c.String(nullable: false, maxLength: 50),
                        CountryCode = c.String(nullable: false, maxLength: 60),
                        UrlWebHost = c.String(nullable: false, maxLength: 3000),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.G_POSEmployeeConfig", t => t.POSEmployeeConfigId)
                .Index(t => t.POSEmployeeConfigId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.G_MultiLocationConfig", "POSEmployeeConfigId", "dbo.G_POSEmployeeConfig");
            DropIndex("dbo.G_MultiLocationConfig", new[] { "POSEmployeeConfigId" });
            DropTable("dbo.G_MultiLocationConfig");
        }
    }
}
