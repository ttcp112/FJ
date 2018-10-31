namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTaxInfoTOPODetailTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Purchase_Order_Detail", "TaxAmount", c => c.Double());
            AddColumn("dbo.I_Purchase_Order_Detail", "TaxId", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Purchase_Order_Detail", "TaxId");
            DropColumn("dbo.I_Purchase_Order_Detail", "TaxAmount");
        }
    }
}
