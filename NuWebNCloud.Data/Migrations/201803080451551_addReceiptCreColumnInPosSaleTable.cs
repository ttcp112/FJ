namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addReceiptCreColumnInPosSaleTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_PosSale", "ReceiptCreatedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_PosSale", "ReceiptCreatedDate");
        }
    }
}
