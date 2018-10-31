using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class GeneralSettingApiModels
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public List<SettingDTO> ListSettings { get; set; }
        public List<DeliverySettingDTO> ListDeliverySetting { get; set; }
        //for Invoice Setting
        public InvoiceSettingDTO Setting { get; set; }
        public List<InvoiceDTO> ListInvoice { get; set; }

        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public int Type { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }

        public string Region { get; set; }
    }
}
