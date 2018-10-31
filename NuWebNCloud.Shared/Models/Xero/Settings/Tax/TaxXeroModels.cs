using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero.Settings.Tax
{
    public class TaxXeroModels
    {
        public string Name { get; set; }
        public string TaxType { get; set; }
        public bool CanApplyToAssets { get; set; }
        public bool CanApplyToEquity { get; set; }
        public bool CanApplyToExpenses { get; set; }
        public bool CanApplyToLiabilities { get; set; }
        public bool CanApplyToRevenue {get;set;}
        public string DisplayTaxRate { get; set; }
        public string EffectiveRate { get; set; }
        public List<TaxXeroComponentModels> TaxComponents { get; set; }

        public TaxXeroModels()
        {
            TaxComponents = new List<TaxXeroComponentModels>();
        }
    }

    public class TaxXeroComponentModels
    {
        public string Name { get; set; } 
        public string Rate { get; set; }
        public bool IsCompound { get; set; }
    }

    public class TaxXeroRequestModels : XeroBaseModel {
        public string ApiURL { get; set; }
    }

    public class ResponseTaxXeroModels
    {
        public bool Success { get; set; }
        public object Error { get; set; }
        public object Data { get; set; }
        public object Message { get; set; }
        public List<TaxXeroModels> RawData { get; set; }
    }
}
