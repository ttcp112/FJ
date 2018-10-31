namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPamentCodeColumnInOrderPaid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.G_PaymentMenthod", "PaymentCode", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.G_PaymentMenthod", "PaymentCode");
        }
    }
}
