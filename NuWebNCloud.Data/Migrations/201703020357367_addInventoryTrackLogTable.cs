namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addInventoryTrackLogTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_InventoryManagementTrackLog",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        TypeCode = c.Int(nullable: false),
                        TypeCodeId = c.String(maxLength: 50),
                        CurrentQty = c.Double(nullable: false),
                        NewQty = c.Double(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.I_InventoryManagementTrackLog");
        }
    }
}
