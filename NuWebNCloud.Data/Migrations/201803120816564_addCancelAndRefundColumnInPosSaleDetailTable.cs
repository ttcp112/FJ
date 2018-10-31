namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCancelAndRefundColumnInPosSaleDetailTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_PosSaleDetail", "IsDiscountTotal", c => c.Boolean());
            AddColumn("dbo.R_PosSaleDetail", "CancelUser", c => c.String(maxLength: 350));
            AddColumn("dbo.R_PosSaleDetail", "RefundUser", c => c.String(maxLength: 350));
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_PosSaleDetail", "RefundUser");
            DropColumn("dbo.R_PosSaleDetail", "CancelUser");
            DropColumn("dbo.R_PosSaleDetail", "IsDiscountTotal");
        }
    }
}
