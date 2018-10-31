using NuWebNCloud.Shared.Models.Settings.Zone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.Zone
{
   public class ZonesModelsView
    {
        public string StoreID { get; set; }
        public List<ZoneModels> List_Zones { get; set; }
        public ZonesModelsView()
        {
            List_Zones = new List<ZoneModels>();
        }
    }
}
