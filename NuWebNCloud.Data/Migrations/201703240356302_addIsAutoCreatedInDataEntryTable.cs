namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIsAutoCreatedInDataEntryTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_DataEntry", "IsAutoCreated", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_DataEntry", "IsAutoCreated");
        }
    }
}
