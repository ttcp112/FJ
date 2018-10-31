namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addShiftInNoSaleTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_NoSaleDetailReport", "BusinessId", c => c.String(maxLength: 60));
            AddColumn("dbo.R_NoSaleDetailReport", "ShiftId", c => c.String(maxLength: 60));
            AddColumn("dbo.R_NoSaleDetailReport", "StartedShift", c => c.DateTime());
            AddColumn("dbo.R_NoSaleDetailReport", "ClosedShift", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_NoSaleDetailReport", "ClosedShift");
            DropColumn("dbo.R_NoSaleDetailReport", "StartedShift");
            DropColumn("dbo.R_NoSaleDetailReport", "ShiftId");
            DropColumn("dbo.R_NoSaleDetailReport", "BusinessId");
        }
    }
}
