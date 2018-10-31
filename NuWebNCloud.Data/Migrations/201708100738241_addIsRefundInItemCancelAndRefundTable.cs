namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIsRefundInItemCancelAndRefundTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_ItemizedCancelOrRefundData", "IsRefund", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_ItemizedCancelOrRefundData", "IsRefund");
        }
    }
}
