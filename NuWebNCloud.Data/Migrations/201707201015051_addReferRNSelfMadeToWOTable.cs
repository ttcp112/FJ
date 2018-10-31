namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addReferRNSelfMadeToWOTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_ReceiptSelfMade_Work_Order",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        RNSelfMadeId = c.String(nullable: false, maxLength: 50),
                        WorkOrderId = c.String(nullable: false, maxLength: 50),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.I_ReceiptSelfMade_Work_Order");
        }
    }
}
