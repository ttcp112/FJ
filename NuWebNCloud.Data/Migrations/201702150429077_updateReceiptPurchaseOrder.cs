namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateReceiptPurchaseOrder : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.I_Receipt_Purchase_Order", "PurchaseOrderNo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.I_Receipt_Purchase_Order", "PurchaseOrderNo", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
