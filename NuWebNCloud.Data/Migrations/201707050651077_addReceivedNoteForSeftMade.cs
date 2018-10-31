namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addReceivedNoteForSeftMade : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_ReceiptNoteForSeftMade",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReceiptNo = c.String(maxLength: 50),
                        ReceiptDate = c.DateTime(),
                        ReceiptBy = c.String(maxLength: 50),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(maxLength: 50),
                        UpdatedDate = c.DateTime(nullable: false),
                        BusinessId = c.String(maxLength: 50),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_ReceiptNoteForSeftMadeDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReceiptNoteId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(),
                        ReceivingQty = c.Double(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                        Status = c.Int(),
                        BaseReceivingQty = c.Double(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.I_ReceiptNoteForSeftMadeDetail");
            DropTable("dbo.I_ReceiptNoteForSeftMade");
        }
    }
}
