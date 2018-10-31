namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDataEntryNewtable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_StockCount",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Code = c.String(nullable: false, maxLength: 50),
                        StockCountDate = c.DateTime(nullable: false),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                        IsAutoCreated = c.Boolean(),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_StockCountDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        StockCountId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        CloseBal = c.Double(nullable: false),
                        OpenBal = c.Double(),
                        Damage = c.Double(),
                        Wast = c.Double(),
                        OtherQty = c.Double(),
                        Reasons = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.I_Allocation", "BusinessId", c => c.String(maxLength: 50));
            AddColumn("dbo.I_DataEntryDetail", "Damage", c => c.Double());
            AddColumn("dbo.I_DataEntryDetail", "Wast", c => c.Double());
            AddColumn("dbo.I_DataEntryDetail", "OrderQty", c => c.Double());
            AddColumn("dbo.I_DataEntryDetail", "Reasons", c => c.String());
            AddColumn("dbo.I_ReceiptNote", "BusinessId", c => c.String(maxLength: 50));
            AddColumn("dbo.I_Return_Note", "BusinessId", c => c.String(maxLength: 50));
            AddColumn("dbo.I_Stock_Transfer", "BusinessId", c => c.String(maxLength: 50));
            AlterColumn("dbo.I_DataEntry", "BusinessId", c => c.String(maxLength: 50));
            DropColumn("dbo.I_DataEntry", "IsAutoCreated");
            DropColumn("dbo.I_DataEntryDetail", "CloseBal");
            DropColumn("dbo.I_DataEntryDetail", "OpenBal");
        }
        
        public override void Down()
        {
            AddColumn("dbo.I_DataEntryDetail", "OpenBal", c => c.Double());
            AddColumn("dbo.I_DataEntryDetail", "CloseBal", c => c.Double(nullable: false));
            AddColumn("dbo.I_DataEntry", "IsAutoCreated", c => c.Boolean());
            AlterColumn("dbo.I_DataEntry", "BusinessId", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.I_Stock_Transfer", "BusinessId");
            DropColumn("dbo.I_Return_Note", "BusinessId");
            DropColumn("dbo.I_ReceiptNote", "BusinessId");
            DropColumn("dbo.I_DataEntryDetail", "Reasons");
            DropColumn("dbo.I_DataEntryDetail", "OrderQty");
            DropColumn("dbo.I_DataEntryDetail", "Wast");
            DropColumn("dbo.I_DataEntryDetail", "Damage");
            DropColumn("dbo.I_Allocation", "BusinessId");
            DropTable("dbo.I_StockCountDetail");
            DropTable("dbo.I_StockCount");
        }
    }
}
