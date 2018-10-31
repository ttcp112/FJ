namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDataEntryTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_DataEntry",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        EntryCode = c.String(nullable: false, maxLength: 50),
                        EntryDate = c.DateTime(nullable: false),
                        BusinessId = c.String(nullable: false, maxLength: 50),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_DataEntryDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        DataEntryId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        CloseBal = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.I_DataEntryDetail");
            DropTable("dbo.I_DataEntry");
        }
    }
}
