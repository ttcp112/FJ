namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateReceiptDetailTable : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.I_ReceiptNoteDetail", "IngredientId");
            DropColumn("dbo.I_ReceiptNoteDetail", "IngredientCode");
            DropColumn("dbo.I_ReceiptNoteDetail", "UOMId");
            DropColumn("dbo.I_ReceiptNoteDetail", "Quantity");
            DropColumn("dbo.I_ReceiptNoteDetail", "Price");
        }
        
        public override void Down()
        {
            AddColumn("dbo.I_ReceiptNoteDetail", "Price", c => c.Double(nullable: false));
            AddColumn("dbo.I_ReceiptNoteDetail", "Quantity", c => c.Double(nullable: false));
            AddColumn("dbo.I_ReceiptNoteDetail", "UOMId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_ReceiptNoteDetail", "IngredientCode", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_ReceiptNoteDetail", "IngredientId", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
