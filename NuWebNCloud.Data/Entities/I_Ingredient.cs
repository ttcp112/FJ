using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public partial class I_Ingredient
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsPurchase { get; set; }
        public bool IsCheckStock { get; set; }
        public bool IsSelfMode { get; set; }

        public string BaseUOMId { get; set; }
        public string BaseUOMName { get; set; }
        
        public string ReceivingUOMId { get; set; }
        public double ReceivingQty { get; set; }

        public double QtyTolerance { get; set; }

        public int Status { get; set; }
        public double PurchasePrice { get; set; }
        public double SalePrice { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string  XeroId { get; set; }

        public double? ReOrderQty { get; set; }
        public double? MinAlertQty { get; set; }

        public bool? StockAble { get; set; }
    }
}
