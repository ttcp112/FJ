namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIsGiftCardColumnAndIsIncludeColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.G_PaymentMenthod", "IsInclude", c => c.Boolean());
            AddColumn("dbo.R_Refund", "IsGiftCard", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_Refund", "IsGiftCard");
            DropColumn("dbo.G_PaymentMenthod", "IsInclude");
        }
    }
}
