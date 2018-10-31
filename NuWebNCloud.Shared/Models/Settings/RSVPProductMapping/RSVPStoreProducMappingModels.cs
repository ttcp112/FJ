using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Settings.RSVPProductMapping
{
    public class RSVPStoreProducMappingModels
    {
        public string StoreID { get; set; }
        public List<ProductItemModels> ListRSVPProductMapping { get; set; }

        /*For Client*/
        public string ID { get; set; }
        public int OffSet { get; set; }
        public int Status { get; set; }
        public bool IsDeleteTemplate { get; set; }        
        public string StoreName { get; set; }

        public RSVPStoreProducMappingModels()
        {
            ListRSVPProductMapping = new List<ProductItemModels>();
        }
    }
}
