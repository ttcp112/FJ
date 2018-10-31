using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NuWebNCloud.Shared.Models.Ingredients.TaxPurchasing
{
    public class TaxPurchasingViewModels
    {
        public string StoreID { get; set; }
        public List<NuWebNCloud.Shared.Models.Settings.TaxModels> ListItem { get; set; }
        public TaxPurchasingViewModels()
        {
            ListItem = new List<NuWebNCloud.Shared.Models.Settings.TaxModels>();
        }
    }
}
