namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTaxTypeColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_ItemizedSalesAnalysisReport", "TaxType", c => c.Int());
            AddColumn("dbo.R_PosSaleDetail", "TaxType", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_PosSaleDetail", "TaxType");
            DropColumn("dbo.R_ItemizedSalesAnalysisReport", "TaxType");
        }
    }
}
