using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.Zone
{
    public class ZoneApiModels
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string DeviceName { get; set; }

        //=======================
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public string CreatedUser { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
