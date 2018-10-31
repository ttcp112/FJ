namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateRole : DbMigration
    {
        public override void Up()
        {
            //Role
            //try
            //{
            //    DropTable("dbo.G_Role");
            //}
            //catch
            //{

            //}
           
            CreateTable(
               "dbo.G_RoleOnStore",
               c => new
               {
                   Id = c.String(nullable: false, maxLength: 100),
                   RoleId = c.DateTime(nullable: false),
                   StoreId = c.String(nullable: false, maxLength: 100),
                   IsActive = c.Boolean(nullable: false),
                   CreatedDate = c.DateTime(nullable: false),
                   CreatedUser = c.String(nullable: false, maxLength: 255),
                   ModifiedUser = c.String(nullable: false, maxLength: 255),
                   ModifiedDate = c.DateTime(nullable: false),

               })
               .PrimaryKey(t => t.Id);
            try
            {

           
            CreateTable(
              "dbo.G_RoleOrganization",
              c => new
              {
                  Id = c.String(nullable: false, maxLength: 100),
                  RoleId = c.DateTime(nullable: false),
                  RoleName = c.String(nullable: false, maxLength: 100),
                  OrganizationName = c.String(nullable: false, maxLength: 100),
                  StoreId = c.String(nullable: false, maxLength: 100),
                  StoreName = c.String(nullable: false, maxLength: 100),
                  IsActive = c.Boolean(nullable: false),
                  CreatedDate = c.DateTime(nullable: false),
                  CreatedUser = c.String(nullable: false, maxLength: 255),
                  ModifiedUser = c.String(nullable: false, maxLength: 255),
                  ModifiedDate = c.DateTime(nullable: false),

              })
              .PrimaryKey(t => t.Id);
            }
            catch
            {

            }
            //module
            //DropColumn("dbo.G_Module", "Status");
            //DropColumn("dbo.G_Module", "Code");
            //AddColumn("dbo.G_Module", "Controller", c => c.String(nullable: false, maxLength: 100));
            ////module permission
            //DropColumn("dbo.G_ModulePermission", "Status");
            ////G_RoleOrganization
            //DropColumn("dbo.G_RoleOrganization", "Status");
            ////G_UserRole
            //DropColumn("dbo.G_UserRole", "Status");
            //DropColumn("dbo.G_UserRole", "StoreId");
        }

        public override void Down()
        {
            DropTable("dbo.G_RoleOrganization");
            //DropColumn("dbo.G_Module", "Controller");
            try
            {
                DropTable("dbo.G_RoleOnStore");
            }
            catch 
            {

            }
        }
    }
}
