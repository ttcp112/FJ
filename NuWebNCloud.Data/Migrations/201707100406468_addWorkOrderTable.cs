namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addWorkOrderTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_Work_Order_Detail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        WorkOrderId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        Qty = c.Double(nullable: false),
                        UnitPrice = c.Double(),
                        Amount = c.Double(),
                        ReceiptNoteQty = c.Double(),
                        ReturnReceiptNoteQty = c.Double(),
                        Status = c.Int(),
                        BaseQty = c.Double(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Work_Order",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Code = c.String(nullable: false, maxLength: 50),
                        WODate = c.DateTime(nullable: false),
                        DateCompleted = c.DateTime(nullable: false),
                        Note = c.String(maxLength: 3000),
                        Total = c.Double(),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.I_Work_Order");
            DropTable("dbo.I_Work_Order_Detail");
        }
    }
}
