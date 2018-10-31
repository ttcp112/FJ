namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateOptionalSupplierInGRNTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.I_ReceiptNote", "SupplierId", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.I_ReceiptNote", "SupplierId", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
