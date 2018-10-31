using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Settings.RSVPProductMapping
{
    public class RSVPProductMappingApiModels
    {
        public List<string> ListOrgID { get; set; }
        public List<string> ListStoreID { get; set; }

        public string ID { get; set; }
        public string CreatedUser { get; set; }
        public byte Type { get; set; }
        public List<RSVPStoreProducMappingModels> ListRSVPStoreProductMapping { get; set; }

        public RSVPProductMappingApiModels()
        {
            ListOrgID = new List<string>();
            ListStoreID = new List<string>();

            ListRSVPStoreProductMapping = new List<RSVPStoreProducMappingModels>();
        }
    }
}
