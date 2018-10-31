namespace NuWebNCloud.Data.Migrations
{
    using Entities;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<NuWebNCloud.Data.NuWebContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(NuWebNCloud.Data.NuWebContext context)
        {
            //  This method will be called after migrating to the latest version.
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            //string reportMainModule = Guid.NewGuid().ToString();
            //string sandboxMainModule = Guid.NewGuid().ToString();
            //string settingMainModule = Guid.NewGuid().ToString();
            //context.G_Module.AddOrUpdate(
            //    //report
            //    new Entities.G_Module() { Id = reportMainModule, Name = "Reports", ParentID = "", Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Audit Trail", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Cash Out", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Cash In", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Closed Receipts", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Daily Receipts", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Daily Sales", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Discount Details", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Hourly Sales", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Itemized Sales Analysis", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Receipts by Payment Method", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Time Clock Summary", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Top Selling Products", ParentID = reportMainModule, Status = 1, Code = 1, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    //Ingredient
            //    //Sandbox
            //    new Entities.G_Module() { Id = sandboxMainModule, Name = "Sandbox", ParentID = "", Status = 1, Code = 3, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    new Entities.G_Module() { Id = Guid.NewGuid().ToString(), Name = "Employees", ParentID = sandboxMainModule, Status = 1, Code = 3, CreatedDate = DateTime.Now, CreatedUser = "admin", ModifiedDate = DateTime.Now, ModifiedUser = "admin" },
            //    //Settings
            //    );
            /*Init Module - Editor by trongntn 06-01-2017*/
            
            //string UserName = "Super Admin";
            //context.G_Module.AddOrUpdate(
            //    /*Reports*/
            //    new G_Module { Id = "be308d51-d251-484e-a10a-819930d13747", Name = "Reports", Controller = "Reports", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName , IndexNum = 1},
            //    //=> List Child Reports
            //    new G_Module { Id = "5eae92ae-326c-41a7-96db-b7bb06c62486", Name = "Audit Trail", Controller = "AuditTrailReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName , IndexNum = 1},
            //    new G_Module { Id = "f183ff1b-d9db-4eb0-9596-c753ec8a2329", Name = "Cash Out", Controller = "CashOutReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 },
            //    new G_Module { Id = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b70", Name = "Cash In", Controller = "CashInReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 },
            //    new G_Module { Id = "e2679844-ef50-47f1-857e-d0b8df53f4fb", Name = "Closed Receipts", Controller = "ClosedReceiptReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 },
            //    new G_Module { Id = "08df6e0e-b5cb-49e2-a8c2-a2860fd58383", Name = "Daily Receipts", Controller = "DailyReceiptReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 },
            //    new G_Module { Id = "13162ee5-c658-4f73-8a15-70d313144c0b", Name = "Daily Sales", Controller = "DailySalesReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 6 },
            //    new G_Module { Id = "f183ff1b-d9db-4eb0-9596-c753ec8a2330", Name = "Discount Details", Controller = "DiscountDetailsReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 7 },
            //    new G_Module { Id = "07daa0f4-41ea-472a-a75e-056ad73cd516", Name = "Hourly Sales", Controller = "HourlySalesReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 8 },
            //    new G_Module { Id = "415a7c80-c884-44bc-ae15-d9dfd58a4e8a", Name = "Itemized Sales Analysis", Controller = "ItemizedSalesAnalysisReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 9 },
            //    new G_Module { Id = "32d6d85c-3898-42f0-8902-fca311e6c314", Name = "Receipts by Payment Menthod", Controller = "ReceiptsbyPaymentMethodsReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 10 },
            //    new G_Module { Id = "69f3479f-406e-4f69-bafc-6d1655e8e3e4", Name = "Time Clock Summary", Controller = "TimeClockSummaryReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 11 },
            //    new G_Module { Id = "4e7fff46-e002-4edc-88fc-a6e5e2ed98f6", Name = "Top Selling Products", Controller = "TopSellingProductsReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 12 },


            //    /*Sandbox*/
            //    new G_Module { Id = "b78bc2d4-c5c8-4cad-8741-17d504f4a3ae", Name = "Sandbox", Controller = "Sandbox", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 },
            //    //=> List Child Sandbox
            //    new G_Module { Id = "7820e093-5117-4051-a1e6-044cb51d7e85", Name = "Employees", Controller = "SBEmployee", ParentID = "b78bc2d4-c5c8-4cad-8741-17d504f4a3ae", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 },
            //    new G_Module { Id = "50262919-8a38-44f0-ba6a-2a515d0f2342", Name = "Inventory", Controller = "SBInventory", ParentID = "b78bc2d4-c5c8-4cad-8741-17d504f4a3ae", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2},
            //    //===> List Child Sandbox [Inventory]
            //    new G_Module { Id = "289cbb7e-0562-4c22-af8c-d5e431dbbef0", Name = "Dishes", Controller = "SBInventoryDishes", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 },
            //    new G_Module { Id = "58c23ab9-6acc-4dea-b098-b629d6daacc0", Name = "Modifiers", Controller = "SBInventoryModifier", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 },
            //    new G_Module { Id = "2feae0c6-e6c5-4ef8-95ea-53845a41cebe", Name = "Set Menus", Controller = "SBInventorySetMenu", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 },
            //    new G_Module { Id = "a4eff739-319f-4463-9169-11b3a8655496", Name = "Discounts", Controller = "SBInventoryDiscount", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4},
            //    new G_Module { Id = "df1107d3-e786-48aa-8cfb-be9bacc90be4", Name = "Categories", Controller = "SBInventoryCategories", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 },
            //    new G_Module { Id = "22a9dcd5-ad33-4592-8830-2d48df0fa223", Name = "Promotions", Controller = "SBInventoryPromotion", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 6 },
            //    //new G_Module { Id = Guid.NewGuid().ToString(), Name = "Gift Cards", Controller = "SBInventoryGiftCard", ParentID = ParentIdLvl2, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName },
                
                
            //    /*Home*/
            //    //new G_Module { Id = ParentIdLvl1 = Guid.NewGuid().ToString(), Name = "Home", Controller = "Home", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName },

            //    /*Settings*/
            //    new G_Module { Id = "bade53c6-3427-473e-a243-01d7dd51d4cb", Name = "Settings", Controller = "Settings", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 },
            //    //=> List Child Settings
            //    new G_Module { Id = "582af43e-7b25-4797-af02-fb4c27c8e319", Name = "Store Informations", Controller = "SStoreInformation", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 },
            //    new G_Module { Id = "8422ce5c-d8a5-4612-87a8-f0ca0c84e62e", Name = "General Settings", Controller = "SGeneralSetting", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 },
            //    new G_Module { Id = "95a3747a-7eb4-4115-bb67-6417ace96138", Name = "Zones", Controller = "SZone", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 },
            //    new G_Module { Id = "33d3a38f-f4f0-4b44-bb05-6a6371205a07", Name = "Table List", Controller = "STable", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 },
            //    new G_Module { Id = "acb47a93-ec17-4f91-8b40-5f4792802c63", Name = "Time Slot", Controller = "SSeason", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 },
            //    new G_Module { Id = "07daa0f4-41ea-472a-a75e-056ad73cd515", Name = "Taxes", Controller = "STax", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 6 },
            //    new G_Module { Id = "561ca930-0e17-4aff-a877-b5701417583a", Name = "Tip & Service Charges", Controller = "STipServiceCharge", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 7 },
            //    new G_Module { Id = "34d1ddeb-f0b5-4fcc-9466-fd9300921c91", Name = "Payment Methods", Controller = "SPayment", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 8 },
            //    new G_Module { Id = "c07fbb96-6ec3-42db-b079-b26b4f0428b4", Name = "Currency", Controller = "SDefaultCurrency", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 9 },
                
            //    //new G_Module { Id = Guid.NewGuid().ToString(), Name = "Scheduled Reports", Controller = "SettingSchedulerReport", ParentID = ParentIdLvl1, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName },

            //    /*Ingredient*/
            //    new G_Module { Id = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b88", Name = "Ingredient", Controller = "Ingredient", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 },
            //    //=> List Child Ingredient
            //    new G_Module { Id = "6a30572f-5873-4a29-a809-9aefbc1393a8", Name = "Data", Controller = "IngData", ParentID = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b88", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 },
            //    //=> List Child Ingredient[Data]
            //    new G_Module { Id = "ab5894eb-e42c-49e8-a5b6-240427c86cb9", Name = "Recipes", Controller = "IngRecipes", ParentID = "6a30572f-5873-4a29-a809-9aefbc1393a8", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 },
            //    //new G_Module { Id = Guid.NewGuid().ToString(), Name = "Ingredients", Controller = "IngIngredients", ParentID = ParentIdLvl2, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName },
            //    new G_Module { Id = "30ea6dfc-6590-40e5-8403-d4438b41968d", Name = "Receipt Notes", Controller = "IngReceiptNote", ParentID = "6a30572f-5873-4a29-a809-9aefbc1393a8", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 },

            //    //=> List Child Ingredient
            //        new G_Module { Id = "8f7a1e2a-3ac6-4aef-8e1f-8090d7ac8563", Name = "Report", Controller = "IngReport", ParentID = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b88", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 },
            //    //=> List Child Ingredient[Report]
            //    new G_Module { Id = "07daa0f4-41ea-472a-a75e-056ad73cd517", Name = "Stock Management", Controller = "IngStockManagement", ParentID = "8f7a1e2a-3ac6-4aef-8e1f-8090d7ac8563", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 },
            //    new G_Module { Id = "d8e5d057-ad66-4e22-a84c-d906552c96e8", Name = "Usage Management", Controller = "IngUsageManagement", ParentID = "8f7a1e2a-3ac6-4aef-8e1f-8090d7ac8563", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 },

            //    /*Access Control*/
            //    new G_Module { Id = "d4626838-4e8f-4ad3-ac9c-4ba490e464c7", Name = "Access Control", Controller = "AccessControl", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 },
            //    //new G_Module { Id = Guid.NewGuid().ToString(), Name = "Modules", Controller = "ACModule", ParentID = ParentIdLvl1, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName },
            //    new G_Module { Id = "a0a8ad28-8091-40cc-9a9c-7c88a550ebe6", Name = "Roles + Accounts", Controller = "ACGroupingAccount", ParentID = "d4626838-4e8f-4ad3-ac9c-4ba490e464c7", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 }
            //);
        }
    }
}
