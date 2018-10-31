using System.Collections.Generic;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory.Product
{
    public class InteProductLiteModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string ProductCode { get; set; }
        public string BarCode { get; set; }
        public string CategoryName { get; set; }
        public string StoreName { get; set; }
        public List<string> ListStoreName { get; set; }
        public List<string> ListCategoryName { get; set; }
        public List<InteStoreLiteDTO> ListStore { get; set; }
        public bool IsSelected { get; set; }
        public InteProductLiteModels()
        {
            ListStoreName = new List<string>();
            ListStore = new List<InteStoreLiteDTO>();
        }
    }

    public class InteStoreLiteDTO
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
}
