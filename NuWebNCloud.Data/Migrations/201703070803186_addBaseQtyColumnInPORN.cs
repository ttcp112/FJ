namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBaseQtyColumnInPORN : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Purchase_Order_Detail", "BaseQty", c => c.Double());
            AddColumn("dbo.I_ReceiptNoteDetail", "BaseReceivingQty", c => c.Double());
            AddColumn("dbo.I_Recipe_Ingredient", "BaseUsage", c => c.Double());
            AddColumn("dbo.I_Recipe_Item", "BaseUsage", c => c.Double());
            AddColumn("dbo.I_Recipe_Modifier", "BaseUsage", c => c.Double());
            AddColumn("dbo.I_Return_Note_Detail", "ReturnBaseQty", c => c.Double());
            AddColumn("dbo.I_Stock_Transfer_Detail", "IssueBaseQty", c => c.Double());
            AddColumn("dbo.I_Stock_Transfer_Detail", "ReceiveBaseQty", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Stock_Transfer_Detail", "ReceiveBaseQty");
            DropColumn("dbo.I_Stock_Transfer_Detail", "IssueBaseQty");
            DropColumn("dbo.I_Return_Note_Detail", "ReturnBaseQty");
            DropColumn("dbo.I_Recipe_Modifier", "BaseUsage");
            DropColumn("dbo.I_Recipe_Item", "BaseUsage");
            DropColumn("dbo.I_Recipe_Ingredient", "BaseUsage");
            DropColumn("dbo.I_ReceiptNoteDetail", "BaseReceivingQty");
            DropColumn("dbo.I_Purchase_Order_Detail", "BaseQty");
        }
    }
}
