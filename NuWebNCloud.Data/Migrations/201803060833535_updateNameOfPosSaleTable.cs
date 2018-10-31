namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateNameOfPosSaleTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_PosSale", "NoOfPerson", c => c.Int(nullable: false));
            DropColumn("dbo.R_PosSale", "NoOfPersion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.R_PosSale", "NoOfPersion", c => c.Int(nullable: false));
            DropColumn("dbo.R_PosSale", "NoOfPerson");
        }
    }
}
