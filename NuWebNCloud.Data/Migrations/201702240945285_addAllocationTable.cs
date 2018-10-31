namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAllocationTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_Allocation",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ApplyDate = c.DateTime(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_AllocationDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        AllocationId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        OpenBal = c.Double(nullable: false),
                        CloseBal = c.Double(nullable: false),
                        Sales = c.Double(nullable: false),
                        ActualSold = c.Double(nullable: false),
                        Damage = c.Double(nullable: false),
                        Wast = c.Double(nullable: false),
                        Others = c.Double(nullable: false),
                        Reasons = c.String(nullable: false, maxLength: 3000),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.I_DataEntryDetail", "OpenBal", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_DataEntryDetail", "OpenBal");
            DropTable("dbo.I_AllocationDetail");
            DropTable("dbo.I_Allocation");
        }
    }
}
