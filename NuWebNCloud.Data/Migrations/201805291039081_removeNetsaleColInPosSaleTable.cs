namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeNetsaleColInPosSaleTable : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.R_PosSale", "NetSales");
        }
        
        public override void Down()
        {
            AddColumn("dbo.R_PosSale", "NetSales", c => c.Double(nullable: false));
        }
    }
}
