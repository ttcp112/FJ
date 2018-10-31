namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPromotionClmToDiscoutDetailTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_DiscountDetailsReport", "PromotionId", c => c.String(maxLength: 60));
            AddColumn("dbo.R_DiscountDetailsReport", "PromotionName", c => c.String(maxLength: 2000));
            AddColumn("dbo.R_DiscountDetailsReport", "PromotionValue", c => c.Double());
            AddColumn("dbo.R_DiscountDetailsReport", "PromotionType", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_DiscountDetailsReport", "PromotionType");
            DropColumn("dbo.R_DiscountDetailsReport", "PromotionValue");
            DropColumn("dbo.R_DiscountDetailsReport", "PromotionName");
            DropColumn("dbo.R_DiscountDetailsReport", "PromotionId");
        }
    }
}
