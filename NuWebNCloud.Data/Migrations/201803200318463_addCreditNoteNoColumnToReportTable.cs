namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCreditNoteNoColumnToReportTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.R_AuditTrailReport", "CreditNoteNo", c => c.String(maxLength: 50));
            AddColumn("dbo.R_ClosedReceiptReport", "CreditNoteNo", c => c.String(maxLength: 50));
            AddColumn("dbo.R_DailyReceiptReport", "CreditNoteNo", c => c.String(maxLength: 50));
            AddColumn("dbo.R_DailySalesReport", "CreditNoteNo", c => c.String(maxLength: 50));
            AddColumn("dbo.R_HourlySalesReport", "CreditNoteNo", c => c.String(maxLength: 50));
            AddColumn("dbo.R_PosSale", "CreditNoteNo", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.R_PosSale", "CreditNoteNo");
            DropColumn("dbo.R_HourlySalesReport", "CreditNoteNo");
            DropColumn("dbo.R_DailySalesReport", "CreditNoteNo");
            DropColumn("dbo.R_DailyReceiptReport", "CreditNoteNo");
            DropColumn("dbo.R_ClosedReceiptReport", "CreditNoteNo");
            DropColumn("dbo.R_AuditTrailReport", "CreditNoteNo");
        }
    }
}
