namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMerchantExtendConfigTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.G_EmployeeOnStoreExtend",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        EmpOnMerchantExtendId = c.String(nullable: false, maxLength: 60),
                        StoreExtendId = c.String(nullable: false, maxLength: 60),
                        CreatedDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.G_EmployeeOnMerchantExtend",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        POSEmployeeConfigId = c.String(nullable: false, maxLength: 60),
                        POSAPIMerchantConfigId = c.String(nullable: false, maxLength: 60),
                        CreatedDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.G_EmployeeOnMerchantExtend");
            DropTable("dbo.G_EmployeeOnStoreExtend");
        }
    }
}
