using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero
{
    public class XeroIngredientModel: XeroBaseModel
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public double PurchaseUnitPrice { get; set; }

        public double SaleUnitPrice { get; set; }

        public string InventoryAssetAccountCode { get; set; }
        public string Description { get; set; }
        public double QtyOH { get; set; }
        public bool IsTrackedAsInventory { get; set; }
    }
    public class XeroIngredientModelResponse {
        public string Id { get; set; }
        public string Code { get; set; }
    }
    public  class ResponseXeroModelBase
    {
        public bool Success { get; set; }
        public XeroIngredientModelResponse Data { get; set; }
    }
}
