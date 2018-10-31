using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class PrinterApiModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string DeviceName { get; set; }
        public bool IsOnlyShowActiveItem { get; set; }

        public string CreatedUser { get; set; }

        //=====
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }
    }
}
