namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUpdateReturnNoteDetailAndInventoryTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_InventoryManagement", "POQty", c => c.Double());
            AddColumn("dbo.I_Return_Note_Detail", "ReturnNoteId", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Return_Note_Detail", "ReturnNoteId");
            DropColumn("dbo.I_InventoryManagement", "POQty");
        }
    }
}
