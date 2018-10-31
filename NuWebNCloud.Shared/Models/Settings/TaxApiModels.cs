using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class TaxApiModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public double Percent { get; set; }
        public bool IsActive { get; set; }
        public int TaxType { get; set; }
        public int Type { get; set; }

        public List<TaxModels> ListTax { get; set; }
        public List<string> ListStoreID { get; set; }
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public bool IsWeb { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }

        public List<string> ListOrgID { get; set; }

        public List<string> ListProductID { get; set; }
        public TaxModels Tax { get; set; }
        public string Rate { get; set; }
    }
}
