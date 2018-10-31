using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class PrinterModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public string PrinterID { get; set; }
        public string PrinterName { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }
        public bool IsActive { get; set; }

        public string CreatedUser { get; set; }

        public int Status { get; set; }

        public List<string> ListOrgID { get; set; }
        //=====
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }
        public bool IsMapProduct { get; set; } // Updated 09122018, for set IsActive when edit
    }
}
