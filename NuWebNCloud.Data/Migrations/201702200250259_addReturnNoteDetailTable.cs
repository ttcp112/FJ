namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addReturnNoteDetailTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_Return_Note_Detail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReceiptNoteDetailId = c.String(nullable: false, maxLength: 50),
                        ReceivedQty = c.Double(nullable: false),
                        ReturnQty = c.Double(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.I_Return_Note_Detail");
        }
    }
}
