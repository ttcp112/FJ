using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.DefaultCurency
{
    public class DefaultCurrencyApiModels
    {
        public string StoreName { get; set; }

        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool IsSelected { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string Mode { get; set; }
        public string CreatedUser { get; set; }
        public string StoreId { get; set; }
        public string Id { get; set; }
        public string DeviceName { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public List<string> ListStoreID { get; set; }
        public List<DefaultCurrencyModels> ListCurrency { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
