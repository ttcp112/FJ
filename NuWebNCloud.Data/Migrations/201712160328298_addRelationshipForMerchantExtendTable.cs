namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRelationshipForMerchantExtendTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.G_EmployeeOnMerchantExtend", "POSEmployeeConfigId", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.G_EmployeeOnMerchantExtend", "POSAPIMerchantConfigId", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.G_EmployeeOnStoreExtend", "EmpOnMerchantExtendId", c => c.String(nullable: false, maxLength: 50));
            CreateIndex("dbo.G_EmployeeOnMerchantExtend", "POSEmployeeConfigId");
            CreateIndex("dbo.G_EmployeeOnMerchantExtend", "POSAPIMerchantConfigId");
            CreateIndex("dbo.G_EmployeeOnStoreExtend", "EmpOnMerchantExtendId");
            AddForeignKey("dbo.G_EmployeeOnMerchantExtend", "POSAPIMerchantConfigId", "dbo.G_POSAPIMerchantConfig", "Id");
            AddForeignKey("dbo.G_EmployeeOnMerchantExtend", "POSEmployeeConfigId", "dbo.G_POSEmployeeConfig", "Id");
            AddForeignKey("dbo.G_EmployeeOnStoreExtend", "EmpOnMerchantExtendId", "dbo.G_EmployeeOnMerchantExtend", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.G_EmployeeOnStoreExtend", "EmpOnMerchantExtendId", "dbo.G_EmployeeOnMerchantExtend");
            DropForeignKey("dbo.G_EmployeeOnMerchantExtend", "POSEmployeeConfigId", "dbo.G_POSEmployeeConfig");
            DropForeignKey("dbo.G_EmployeeOnMerchantExtend", "POSAPIMerchantConfigId", "dbo.G_POSAPIMerchantConfig");
            DropIndex("dbo.G_EmployeeOnStoreExtend", new[] { "EmpOnMerchantExtendId" });
            DropIndex("dbo.G_EmployeeOnMerchantExtend", new[] { "POSAPIMerchantConfigId" });
            DropIndex("dbo.G_EmployeeOnMerchantExtend", new[] { "POSEmployeeConfigId" });
            AlterColumn("dbo.G_EmployeeOnStoreExtend", "EmpOnMerchantExtendId", c => c.String(nullable: false, maxLength: 60));
            AlterColumn("dbo.G_EmployeeOnMerchantExtend", "POSAPIMerchantConfigId", c => c.String(nullable: false, maxLength: 60));
            AlterColumn("dbo.G_EmployeeOnMerchantExtend", "POSEmployeeConfigId", c => c.String(nullable: false, maxLength: 60));
        }
    }
}
