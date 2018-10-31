using System.Collections.Generic;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Settings.RSVPProductMapping
{
    public class RSVPProductMappingModels
    {
        public string StoreID { get; set; }
        public byte Type { get; set; }
        public List<RSVPStoreProducMappingModels> ListRSVPStoreProductMapping { get; set; }

        /*For Client*/
        public string StoreName { get; set; }
        public List<SelectListItem> ListStoreView { get; set; }
        public RSVPProductMappingModels()
        {
            ListRSVPStoreProductMapping = new List<RSVPStoreProducMappingModels>();
            ListStoreView = new List<SelectListItem>();
        }
    }

    public class RSVPProductMappingViewModels
    {
        public List<RSVPStoreProducMappingModels> ListItem { get; set; }
        public RSVPProductMappingViewModels()
        {
            ListItem = new List<RSVPStoreProducMappingModels>();
        }
    }
}
