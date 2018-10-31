using System.Collections.Generic;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory.Product
{
    public class InteProductApplyItemModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public int ProductType { get; set; }
        public List<InteStoreLiteDTO> ListStore { get; set; }
        public InteProductApplyItemModels()
        {
            ListStore = new List<InteStoreLiteDTO>();
        }
    }
}
