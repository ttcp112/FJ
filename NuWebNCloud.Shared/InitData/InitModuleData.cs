using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NLog;

namespace NuWebNCloud.Shared.InitData
{
    public class InitModuleData
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        string UserName = "Super Admin";

        public void InsertModule()
        {
            using (var cxt = new NuWebContext())
            {
                try
                {
                    List<G_Module> lst = new List<G_Module>();
                    /*Reports 1*/
                    lst.Add(new G_Module() { Id = "be308d51-d251-484e-a10a-819930d13747", Name = "Reports", Controller = "Reports", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    //=> List Child Reports
                    lst.Add(new G_Module() { Id = "5eae92ae-326c-41a7-96db-b7bb06c62486", Name = "Audit Trail", Controller = "AuditTrailReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "f183ff1b-d9db-4eb0-9596-c753ec8a2329", Name = "Cash Out", Controller = "CashOutReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    lst.Add(new G_Module() { Id = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b70", Name = "Cash In", Controller = "CashInReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 });
                    lst.Add(new G_Module() { Id = "497664ea-4249-48a5-b625-6f92fa72596f", Name = "Credit Invoices", Controller = "CreditInvoicesReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 });//
                    lst.Add(new G_Module() { Id = "e2679844-ef50-47f1-857e-d0b8df53f4fb", Name = "Closed Receipts", Controller = "ClosedReceiptReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 });
                    lst.Add(new G_Module() { Id = "08df6e0e-b5cb-49e2-a8c2-a2860fd58383", Name = "Daily Receipts", Controller = "DailyReceiptReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 6 });
                    lst.Add(new G_Module() { Id = "13162ee5-c658-4f73-8a15-70d313144c0b", Name = "Daily Sales", Controller = "DailySalesReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 7 });

                    lst.Add(new G_Module() { Id = "13162ea5-c658-4f73-8a15-70d313155a0c", Name = "Daily Detail Itemized Sales Analysis", Controller = "DailyItemizedSalesAnalysisDetailReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 8 });

                    lst.Add(new G_Module() { Id = "068f394a-24b2-4837-9a94-fa95cfa06407", Name = "Detail Itemized Sales Analysis", Controller = "ItemizedSalesAnalysisDetailReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 9 });
                    lst.Add(new G_Module() { Id = "f183ff1b-d9db-4eb0-9596-c753ec8a2330", Name = "Discount Details", Controller = "DiscountDetailsReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 10 });

                    //hourly itemized
                    lst.Add(new G_Module() { Id = "f183ff1b-d9db-4eb0-9596-c753ec8a2340", Name = "Hourly Itemized Sales Report", Controller = "HourlyItemizedSalesReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 11 });

                    lst.Add(new G_Module() { Id = "07daa0f4-41ea-472a-a75e-056ad73cd516", Name = "Hourly Sales", Controller = "HourlySalesReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 12 });
                    lst.Add(new G_Module() { Id = "415a7c80-c884-44bc-ae15-d9dfd58a4e8a", Name = "Itemized Sales Analysis", Controller = "ItemizedSalesAnalysisReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 13 });
                    //no sale
                    lst.Add(new G_Module() { Id = "415a7c80-c884-44bc-ae15-d9dfd58a4e8b", Name = "No Sale Report", Controller = "NoSaleDetailReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 14 });

                    lst.Add(new G_Module() { Id = "32d6d85c-3898-42f0-8902-fca311e6c314", Name = "Receipts by Payment Menthod", Controller = "ReceiptsbyPaymentMethodsReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 15 });
                    lst.Add(new G_Module() { Id = "69f3479f-406e-4f69-bafc-6d1655e8e3e4", Name = "Time Clock Summary", Controller = "TimeClockSummaryReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 16 });
                    lst.Add(new G_Module() { Id = "4e7fff46-e002-4edc-88fc-a6e5e2ed98f6", Name = "Top Selling Products", Controller = "TopSellingProductsReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 17 });

                    //28/08/2017
                    lst.Add(new G_Module() { Id = "33d3a38f-f4f0-4b44-bb05-6a6371211107", Name = "FJ Daily Sales -> Setting", Controller = "SettingForDailySales", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 18 });
                    lst.Add(new G_Module() { Id = "33d3a38f-f4f0-4b44-bb05-6a6371211206", Name = "FJ Daily Sales -> Report", Controller = "FJDailySalesReport", ParentID = "be308d51-d251-484e-a10a-819930d13747", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 19 });

                    /*Ingredient 2*/
                    lst.Add(new G_Module() { Id = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b88", Name = "Inventory Management", Controller = "Ingredient", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    //=> List Child Ingredient
                    //Recipe
                    lst.Add(new G_Module() { Id = "6a30572f-5873-4a29-a809-9aefbc1393a8", Name = "Recipe", Controller = "Recipe", ParentID = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b88", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "9127114d-3184-45c8-a854-c9e58dddcfde", Name = "Ingredients", Controller = "IngIngredients", ParentID = "6a30572f-5873-4a29-a809-9aefbc1393a8", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "b2a13b3a-a12e-45da-a95e-3be40afbc812", Name = "Recipes", Controller = "IngRecipes", ParentID = "6a30572f-5873-4a29-a809-9aefbc1393a8", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    lst.Add(new G_Module() { Id = "46116f70-a35f-4d5d-9879-819352959eab", Name = "UOMs", Controller = "IngUOMs", ParentID = "6a30572f-5873-4a29-a809-9aefbc1393a8", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 });
                    lst.Add(new G_Module() { Id = "810dd873-0d6e-40d5-a64c-9093de25d1a6", Name = "Store Settings", Controller = "IngStoreSettings", ParentID = "6a30572f-5873-4a29-a809-9aefbc1393a8", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 });
                    lst.Add(new G_Module() { Id = "bc0dfae8-37fb-4963-8995-810ce25cb8eb", Name = "Stock Management", Controller = "IngStockManagement", ParentID = "6a30572f-5873-4a29-a809-9aefbc1393a8", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 });
                    lst.Add(new G_Module() { Id = "1ceeec2a-30b8-4e9b-8a69-671ea8310b11", Name = "Taxes for Purchasing", Controller = "IngTaxesPurchasing", ParentID = "6a30572f-5873-4a29-a809-9aefbc1393a8", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 6 });
                    //Purchasing
                    lst.Add(new G_Module() { Id = "ab5894eb-e42c-49e8-a5b6-240427c86cb9", Name = "Purchasing", Controller = "Purchasing", ParentID = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b88", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    lst.Add(new G_Module() { Id = "bc0dfae8-37fb-4963-8995-810ce25cb8cb", Name = "Suppliers", Controller = "IngSuppliers", ParentID = "ab5894eb-e42c-49e8-a5b6-240427c86cb9", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "7db3e848-366f-4752-84e9-dd77ef73252c", Name = "Purchase Orders", Controller = "IngPurchaseOrders", ParentID = "ab5894eb-e42c-49e8-a5b6-240427c86cb9", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });

                    //Ingredient - Inventory
                    lst.Add(new G_Module() { Id = "8f7a1e2a-3ac6-4aef-8e1f-8090d7ac8563", Name = "Inventory", Controller = "Inventory", ParentID = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b88", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 });
                    lst.Add(new G_Module() { Id = "07daa0f4-41ea-472a-a75e-056ad73cd517", Name = "Received Notes", Controller = "IngReceiptNote", ParentID = "8f7a1e2a-3ac6-4aef-8e1f-8090d7ac8563", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "c53498a8-72eb-4afb-9829-93d8812e9ba7", Name = "Stock Transfer", Controller = "IngStockTransfer", ParentID = "8f7a1e2a-3ac6-4aef-8e1f-8090d7ac8563", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    lst.Add(new G_Module() { Id = "a099ad96-823a-4c8d-b258-95fb9fa44ba7", Name = "Usage Management", Controller = "UsageManagement", ParentID = "8f7a1e2a-3ac6-4aef-8e1f-8090d7ac8563", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 });
                    lst.Add(new G_Module() { Id = "b1baad3f-6b96-4f09-8a35-aefe974d4cf7", Name = "Stock Count", Controller = "IngStockCount", ParentID = "a099ad96-823a-4c8d-b258-95fb9fa44ba7", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "545ae137-3575-4848-81bc-7da0c342a3a7", Name = "Ingredients Usage", Controller = "IngIngredientUsage", ParentID = "a099ad96-823a-4c8d-b258-95fb9fa44ba7", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    lst.Add(new G_Module() { Id = "a099ad96-823a-4c8d-b258-95fb9fa44ba8", Name = "Data Entry", Controller = "IngDataEntry", ParentID = "8f7a1e2a-3ac6-4aef-8e1f-8090d7ac8563", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 });
                    //Ingredient - Report
                    lst.Add(new G_Module() { Id = "33d3a38f-f4f0-4b44-bb05-6a1171233308", Name = "Reports", Controller = "Report", ParentID = "fa2af3a6-064a-4f2d-b7f3-18da5faf4b88", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 });
                    lst.Add(new G_Module() { Id = "33d3a38f-f4f0-4b44-bb05-6a1171244409", Name = "Daily Transactions", Controller = "IngDailyTransactionsReport", ParentID = "33d3a38f-f4f0-4b44-bb05-6a1171233308", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "44d3a38f-f4f0-4b44-bb05-6a1171255509", Name = "Monthly Transactions", Controller = "IngDailyTransactionsReport", ParentID = "33d3a38f-f4f0-4b44-bb05-6a1171233308", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });

                    /*Sandbox 3*/
                    lst.Add(new G_Module() { Id = "b78bc2d4-c5c8-4cad-8741-17d504f4a3ae", Name = "Sandbox", Controller = "Sandbox", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 });
                    //=> List Child Sandbox
                    lst.Add(new G_Module() { Id = "7820e093-5007-4051-a1e6-055cb51d7e85", Name = "Customers", Controller = "SBCustomer", ParentID = "b78bc2d4-c5c8-4cad-8741-17d504f4a3ae", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "7820e093-5117-4051-a1e6-044cb51d7e85", Name = "Employees", Controller = "SBEmployee", ParentID = "b78bc2d4-c5c8-4cad-8741-17d504f4a3ae", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    lst.Add(new G_Module() { Id = "50262919-8a38-44f0-ba6a-2a515d0f2342", Name = "Inventory", Controller = "SBInventory", ParentID = "b78bc2d4-c5c8-4cad-8741-17d504f4a3ae", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 });
                    //===> List Child Sandbox [Inventory]
                    lst.Add(new G_Module() { Id = "289cbb7e-0562-4c22-af8c-d5e431dbbef0", Name = "Dishes", Controller = "SBInventoryDishes", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "58c23ab9-6acc-4dea-b098-b629d6daacc0", Name = "Modifiers", Controller = "SBInventoryModifier", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    lst.Add(new G_Module() { Id = "2feae0c6-e6c5-4ef8-95ea-53845a41cebe", Name = "Set Menus", Controller = "SBInventorySetMenu", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 });
                    lst.Add(new G_Module() { Id = "a4eff739-319f-4463-9169-11b3a8655496", Name = "Discounts", Controller = "SBInventoryDiscount", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 });
                    lst.Add(new G_Module() { Id = "df1107d3-e786-48aa-8cfb-be9bacc90be4", Name = "Categories", Controller = "SBInventoryCategories", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 });
                    lst.Add(new G_Module() { Id = "22a9dcd5-ad33-4592-8830-2d48df0fa223", Name = "Promotions", Controller = "SBInventoryPromotion", ParentID = "50262919-8a38-44f0-ba6a-2a515d0f2342", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 6 });
                    /*Settings 4*/
                    lst.Add(new G_Module() { Id = "bade53c6-3427-473e-a243-01d7dd51d4cb", Name = "Settings", Controller = "Settings", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 });
                    //=> List Child Settings
                    lst.Add(new G_Module() { Id = "582af43e-7b25-4797-af02-fb4c27c8e319", Name = "Store Information", Controller = "SStoreInformation", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });
                    lst.Add(new G_Module() { Id = "8422ce5c-d8a5-4612-87a8-f0ca0c84e62e", Name = "General Settings", Controller = "SGeneralSetting", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 2 });
                    lst.Add(new G_Module() { Id = "95a3747a-7eb4-4115-bb67-6417ace96138", Name = "Zones", Controller = "SZone", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 3 });
                    lst.Add(new G_Module() { Id = "33d3a38f-f4f0-4b44-bb05-6a6371205a07", Name = "Table List", Controller = "STable", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 4 });
                    lst.Add(new G_Module() { Id = "acb47a93-ec17-4f91-8b40-5f4792802c63", Name = "Time Slot", Controller = "SSeason", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 });
                    lst.Add(new G_Module() { Id = "07daa0f4-41ea-472a-a75e-056ad73cd515", Name = "Taxes", Controller = "STax", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 6 });
                    lst.Add(new G_Module() { Id = "561ca930-0e17-4aff-a877-b5701417583a", Name = "Tip & Service Charge", Controller = "STipServiceCharge", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 7 });
                    lst.Add(new G_Module() { Id = "34d1ddeb-f0b5-4fcc-9466-fd9300921c91", Name = "Payment Methods", Controller = "SPayment", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 8 });
                    lst.Add(new G_Module() { Id = "c07fbb96-6ec3-42db-b079-b26b4f0428b4", Name = "Currency", Controller = "SDefaultCurrency", ParentID = "bade53c6-3427-473e-a243-01d7dd51d4cb", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 9 });

                    /*Access Control 5*/
                    lst.Add(new G_Module() { Id = "d4626838-4e8f-4ad3-ac9c-4ba490e464c7", Name = "Access Control", Controller = "AccessControl", ParentID = string.Empty, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 5 });
                    lst.Add(new G_Module() { Id = "a0a8ad28-8091-40cc-9a9c-7c88a550ebe6", Name = "Roles + Accounts", Controller = "ACGroupingAccount", ParentID = "d4626838-4e8f-4ad3-ac9c-4ba490e464c7", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now, CreatedUser = UserName, ModifiedUser = UserName, IndexNum = 1 });

                    cxt.G_Module.AddRange(lst);
                    cxt.SaveChanges();
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("Init Module Error", ex);
                    //_logger.Info("---------------------------------Init Module-----------------------------------");
                    //_logger.Error(ex);
                }
            }
        }
    }
}
