namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addShiftLogTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.R_ShiftLog",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        StartedOn = c.DateTime(nullable: false),
                        ClosedOn = c.DateTime(nullable: false),
                        StartedStaff = c.String(nullable: false, maxLength: 450),
                        ClosedStaff = c.String(nullable: false, maxLength: 450),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.R_ShiftLog");
        }
    }
}
