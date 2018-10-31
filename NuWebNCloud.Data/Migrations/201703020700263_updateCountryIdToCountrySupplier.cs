namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateCountryIdToCountrySupplier : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Supplier", "Country", c => c.String(maxLength: 250));
            DropColumn("dbo.I_Supplier", "CountryId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.I_Supplier", "CountryId", c => c.String(maxLength: 50));
            DropColumn("dbo.I_Supplier", "Country");
        }
    }
}
