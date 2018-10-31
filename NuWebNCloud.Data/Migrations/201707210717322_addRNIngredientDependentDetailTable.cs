namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRNIngredientDependentDetailTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_ReceiptNoteForSeftMadeDependentDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        RNSelfMadeDetailId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        StockOutQty = c.Double(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.I_ReceiptNoteForSeftMadeDependentDetail");
        }
    }
}
