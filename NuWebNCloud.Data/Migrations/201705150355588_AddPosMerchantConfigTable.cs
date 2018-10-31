namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPosMerchantConfigTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.G_POSEmployeeConfig",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        POSAPIMerchantConfigId = c.String(nullable: false, maxLength: 60),
                        UserName = c.String(nullable: false, maxLength: 350),
                        Password = c.String(nullable: false, maxLength: 350),
                        CreatedDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_POSAPIMerchantConfig",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        MerchantName = c.String(maxLength: 250),
                        POSAPIUrl = c.String(nullable: false, maxLength: 350),
                        FTPHost = c.String(nullable: false, maxLength: 350),
                        FTPUser = c.String(nullable: false, maxLength: 350),
                        FTPPassword = c.String(nullable: false, maxLength: 350),
                        ImageBaseUrl = c.String(nullable: false, maxLength: 350),
                        BreakfastStart = c.String(nullable: false, maxLength: 50),
                        BreakfastEnd = c.String(nullable: false, maxLength: 50),
                        LunchStart = c.String(nullable: false, maxLength: 50),
                        LunchEnd = c.String(nullable: false, maxLength: 50),
                        DinnerStart = c.String(nullable: false, maxLength: 50),
                        DinnerEnd = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.G_POSAPIMerchantConfig");
            DropTable("dbo.G_POSEmployeeConfig");
        }
    }
}
