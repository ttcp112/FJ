namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBusinessReceivingId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.I_Stock_Transfer", "BusinessReceiveId", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.I_Stock_Transfer", "BusinessReceiveId");
        }
    }
}
