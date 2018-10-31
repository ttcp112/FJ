using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.DefaultCurency
{
    public class DefautCurrencyViewModels
    {
        public string StoreID { get; set; }
        public List<DefaultCurrencyModels> List_DefaultCurrency { get; set; }
        public DefautCurrencyViewModels()
        {
            List_DefaultCurrency = new List<DefaultCurrencyModels>();
        }
    }
}
