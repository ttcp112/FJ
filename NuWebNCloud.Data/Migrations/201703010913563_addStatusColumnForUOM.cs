namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStatusColumnForUOM : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Supplier", "Status", c => c.Int());
            AddColumn("dbo.I_UnitOfMeasure", "Status", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_UnitOfMeasure", "Status");
            DropColumn("dbo.I_Supplier", "Status");
        }
    }
}
