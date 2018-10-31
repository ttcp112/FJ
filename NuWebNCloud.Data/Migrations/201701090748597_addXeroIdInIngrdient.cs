namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addXeroIdInIngrdient : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Ingredient", "XeroId", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Ingredient", "XeroId");
        }
    }
}
