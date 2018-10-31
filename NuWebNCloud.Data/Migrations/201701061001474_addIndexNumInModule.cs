namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIndexNumInModule : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.G_Module", "IndexNum", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.G_Module", "IndexNum");
        }
    }
}
