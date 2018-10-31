namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStockSaleTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_StockSale",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        OrderId = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        BusinessId = c.String(maxLength: 50),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_StockSaleDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        StockSaleId = c.String(nullable: false, maxLength: 50),
                        ProductId = c.String(nullable: false, maxLength: 50),
                        ProductName = c.String(maxLength: 350),
                        Qty = c.Int(nullable: false),
                        IsCheckStock = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.I_StockSale", t => t.StockSaleId)
                .Index(t => t.StockSaleId);
            
            CreateTable(
                "dbo.I_StockSaleUsageDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        StockSaleDetailId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        Usage = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.I_Ingredient", t => t.IngredientId)
                .ForeignKey("dbo.I_StockSaleDetail", t => t.StockSaleDetailId)
                .Index(t => t.StockSaleDetailId)
                .Index(t => t.IngredientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.I_StockSaleUsageDetail", "StockSaleDetailId", "dbo.I_StockSaleDetail");
            DropForeignKey("dbo.I_StockSaleUsageDetail", "IngredientId", "dbo.I_Ingredient");
            DropForeignKey("dbo.I_StockSaleDetail", "StockSaleId", "dbo.I_StockSale");
            DropIndex("dbo.I_StockSaleUsageDetail", new[] { "IngredientId" });
            DropIndex("dbo.I_StockSaleUsageDetail", new[] { "StockSaleDetailId" });
            DropIndex("dbo.I_StockSaleDetail", new[] { "StockSaleId" });
            DropTable("dbo.I_StockSaleUsageDetail");
            DropTable("dbo.I_StockSaleDetail");
            DropTable("dbo.I_StockSale");
        }
    }
}
