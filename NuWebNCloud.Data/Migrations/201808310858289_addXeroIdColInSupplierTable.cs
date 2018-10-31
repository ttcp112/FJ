namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addXeroIdColInSupplierTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Supplier", "XeroId", c => c.String(maxLength: 60));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Supplier", "XeroId");
        }
    }
}
