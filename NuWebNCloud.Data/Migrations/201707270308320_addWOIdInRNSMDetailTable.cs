namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addWOIdInRNSMDetailTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_ReceiptNoteForSeftMadeDetail", "WOId", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_ReceiptNoteForSeftMadeDetail", "WOId");
        }
    }
}
