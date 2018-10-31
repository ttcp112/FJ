namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addreOrderAndMinAlert : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Ingredient", "ReOrderQty", c => c.Double());
            AddColumn("dbo.I_Ingredient", "MinAlertQty", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Ingredient", "MinAlertQty");
            DropColumn("dbo.I_Ingredient", "ReOrderQty");
        }
    }
}
