namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStatusForPODetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Purchase_Order_Detail", "Status", c => c.Int());
            AddColumn("dbo.I_ReceiptNoteDetail", "Status", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_ReceiptNoteDetail", "Status");
            DropColumn("dbo.I_Purchase_Order_Detail", "Status");
        }
    }
}
