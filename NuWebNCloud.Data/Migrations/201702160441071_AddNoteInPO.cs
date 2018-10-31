namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNoteInPO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Purchase_Order", "Note", c => c.String(maxLength: 3000));
            AddColumn("dbo.I_StoreSetting", "StoreId", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_StoreSetting", "StoreId");
            DropColumn("dbo.I_Purchase_Order", "Note");
        }
    }
}
