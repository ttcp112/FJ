namespace NuWebNCloud.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTableIngredient2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.I_Country",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ShortName = c.String(nullable: false, maxLength: 10),
                        FullName = c.String(nullable: false, maxLength: 255),
                        ZipCode = c.String(nullable: false, maxLength: 50),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Ingredient_Supplier",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        SupplierId = c.String(nullable: false, maxLength: 50),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Purchase_Order",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        Code = c.String(nullable: false, maxLength: 50),
                        SupplierId = c.String(nullable: false, maxLength: 50),
                        PODate = c.DateTime(nullable: false),
                        DeliveryDate = c.DateTime(nullable: false),
                        TaxType = c.Int(nullable: false),
                        TaxPercen = c.Double(nullable: false),
                        SubTotal = c.Double(nullable: false),
                        TaxAmount = c.Double(nullable: false),
                        Additional = c.Double(nullable: false),
                        AdditionalReason = c.String(maxLength: 3000),
                        Total = c.Double(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Purchase_Order_Detail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        PurchaseOrderId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(nullable: false, maxLength: 50),
                        Qty = c.Double(nullable: false),
                        UnitPrice = c.Double(nullable: false),
                        Amount = c.Double(nullable: false),
                        ReceiptNoteQty = c.Double(nullable: false),
                        ReturnReceiptNoteQty = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Receipt_Purchase_Order",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReceiptNoteId = c.String(nullable: false, maxLength: 50),
                        PurchaseOrderId = c.String(nullable: false, maxLength: 50),
                        PurchaseOrderNo = c.String(nullable: false, maxLength: 50),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Return_Note",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        ReturnNoteNo = c.String(nullable: false, maxLength: 20),
                        ReceiptNoteId = c.String(nullable: false, maxLength: 50),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Stock_Transfer",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        IssueStoreId = c.String(nullable: false, maxLength: 50),
                        ReceiveStoreId = c.String(maxLength: 50),
                        RequestBy = c.String(nullable: false, maxLength: 50),
                        RequestDate = c.DateTime(nullable: false),
                        IssueBy = c.String(nullable: false, maxLength: 50),
                        IssueDate = c.DateTime(nullable: false),
                        ReceiveBy = c.String(nullable: false, maxLength: 50),
                        ReceiveDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Stock_Transfer_Detail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        StockTransferId = c.String(nullable: false, maxLength: 50),
                        IngredientId = c.String(maxLength: 50),
                        RequestQty = c.Double(nullable: false),
                        IssueQty = c.Double(nullable: false),
                        ReceiveQty = c.Double(nullable: false),
                        UOMId = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.I_Supplier",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        CompanyId = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 355),
                        Address = c.String(maxLength: 3000),
                        City = c.String(maxLength: 40),
                        CountryId = c.String(maxLength: 50),
                        ZipCode = c.String(maxLength: 50),
                        Phone1 = c.String(maxLength: 50),
                        Phone2 = c.String(maxLength: 50),
                        Fax = c.String(maxLength: 3000),
                        Email = c.String(maxLength: 255),
                        ContactInfo = c.String(maxLength: 3000),
                        CreatedBy = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifierBy = c.String(nullable: false, maxLength: 50),
                        ModifierDate = c.DateTime(nullable: false),
                        IsActived = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.I_Ingredient", "CompanyId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_Ingredient", "IsPurchase", c => c.Boolean(nullable: false));
            AddColumn("dbo.I_Ingredient", "IsCheckStock", c => c.Boolean(nullable: false));
            AddColumn("dbo.I_Ingredient", "IsSelfMode", c => c.Boolean(nullable: false));
            AddColumn("dbo.I_Ingredient", "BaseUOMId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_Ingredient", "ReceivingUOMId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_Ingredient", "ReceivingQty", c => c.Double(nullable: false));
            AddColumn("dbo.I_Ingredient", "QtyTolerance", c => c.Double(nullable: false));
            AddColumn("dbo.I_Ingredient_UOM", "ReceivingQty", c => c.Double(nullable: false));
            AddColumn("dbo.I_Ingredient_UOM", "IsActived", c => c.Boolean(nullable: false));
            AddColumn("dbo.I_ReceiptNote", "SupplierId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_ReceiptNote", "ReceiptDate", c => c.DateTime());
            AddColumn("dbo.I_ReceiptNote", "ReceiptBy", c => c.String(maxLength: 50));
            AddColumn("dbo.I_ReceiptNoteDetail", "PurchaseOrderDetailId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_ReceiptNoteDetail", "IngredientCode", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_ReceiptNoteDetail", "UOMId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_ReceiptNoteDetail", "ReceivedQty", c => c.Double(nullable: false));
            AddColumn("dbo.I_ReceiptNoteDetail", "ReceivingQty", c => c.Double(nullable: false));
            AddColumn("dbo.I_ReceiptNoteDetail", "RemainingQty", c => c.Double(nullable: false));
            AddColumn("dbo.I_ReceiptNoteDetail", "IsActived", c => c.Boolean(nullable: false));
            AddColumn("dbo.I_UnitOfMeasure", "OrganizationId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.I_UnitOfMeasure", "Description", c => c.String(maxLength: 500));
            AlterColumn("dbo.I_Ingredient", "Name", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.I_Ingredient_UOM", "CreatedBy", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.I_Ingredient_UOM", "UpdatedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.I_ReceiptNote", "CreatedBy", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.I_ReceiptNote", "UpdatedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.I_UnitOfMeasure", "CreatedBy", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.I_UnitOfMeasure", "UpdatedBy", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.I_UnitOfMeasure", "UpdatedBy", c => c.String(nullable: false));
            AlterColumn("dbo.I_UnitOfMeasure", "CreatedBy", c => c.String(nullable: false, maxLength: 250));
            AlterColumn("dbo.I_ReceiptNote", "UpdatedBy", c => c.String(maxLength: 250));
            AlterColumn("dbo.I_ReceiptNote", "CreatedBy", c => c.String(nullable: false, maxLength: 250));
            AlterColumn("dbo.I_Ingredient_UOM", "UpdatedBy", c => c.String(maxLength: 250));
            AlterColumn("dbo.I_Ingredient_UOM", "CreatedBy", c => c.String(nullable: false, maxLength: 250));
            AlterColumn("dbo.I_Ingredient", "Name", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.I_UnitOfMeasure", "Description");
            DropColumn("dbo.I_UnitOfMeasure", "OrganizationId");
            DropColumn("dbo.I_ReceiptNoteDetail", "IsActived");
            DropColumn("dbo.I_ReceiptNoteDetail", "RemainingQty");
            DropColumn("dbo.I_ReceiptNoteDetail", "ReceivingQty");
            DropColumn("dbo.I_ReceiptNoteDetail", "ReceivedQty");
            DropColumn("dbo.I_ReceiptNoteDetail", "UOMId");
            DropColumn("dbo.I_ReceiptNoteDetail", "IngredientCode");
            DropColumn("dbo.I_ReceiptNoteDetail", "PurchaseOrderDetailId");
            DropColumn("dbo.I_ReceiptNote", "ReceiptBy");
            DropColumn("dbo.I_ReceiptNote", "ReceiptDate");
            DropColumn("dbo.I_ReceiptNote", "SupplierId");
            DropColumn("dbo.I_Ingredient_UOM", "IsActived");
            DropColumn("dbo.I_Ingredient_UOM", "ReceivingQty");
            DropColumn("dbo.I_Ingredient", "QtyTolerance");
            DropColumn("dbo.I_Ingredient", "ReceivingQty");
            DropColumn("dbo.I_Ingredient", "ReceivingUOMId");
            DropColumn("dbo.I_Ingredient", "BaseUOMId");
            DropColumn("dbo.I_Ingredient", "IsSelfMode");
            DropColumn("dbo.I_Ingredient", "IsCheckStock");
            DropColumn("dbo.I_Ingredient", "IsPurchase");
            DropColumn("dbo.I_Ingredient", "CompanyId");
            DropTable("dbo.I_Supplier");
            DropTable("dbo.I_Stock_Transfer_Detail");
            DropTable("dbo.I_Stock_Transfer");
            DropTable("dbo.I_Return_Note");
            DropTable("dbo.I_Receipt_Purchase_Order");
            DropTable("dbo.I_Purchase_Order_Detail");
            DropTable("dbo.I_Purchase_Order");
            DropTable("dbo.I_Ingredient_Supplier");
            DropTable("dbo.I_Country");
        }
    }
}
