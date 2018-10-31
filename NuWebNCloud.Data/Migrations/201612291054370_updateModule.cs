namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateModule : DbMigration
    {
        public override void Up()
        {
            //module
             DropColumn("dbo.G_Module", "Code");
            AddColumn("dbo.G_Module", "Controller", c => c.String(nullable: false, maxLength: 100));
            //roleOrganization
            DropColumn("dbo.G_RoleOrganization", "RoleId");
            DropColumn("dbo.G_RoleOrganization", "RoleName");
            DropColumn("dbo.G_RoleOrganization", "OrganizationName");
            DropColumn("dbo.G_RoleOrganization", "StoreId");
            DropColumn("dbo.G_RoleOrganization", "StoreName");

            AddColumn("dbo.G_RoleOrganization", "Name", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.G_RoleOrganization", "OrganizationId", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.G_Module", "Controller");
            AddColumn("dbo.G_Module", "Code", c => c.String(nullable: false, maxLength: 100));
            //roleOrganization
            DropColumn("dbo.G_RoleOrganization", "Name");
            DropColumn("dbo.G_RoleOrganization", "OrganizationId");
            AddColumn("dbo.G_RoleOrganization", "RoleId", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.G_RoleOrganization", "RoleName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.G_RoleOrganization", "OrganizationName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.G_RoleOrganization", "StoreId", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.G_RoleOrganization", "StoreName", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
