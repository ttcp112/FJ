namespace NuWebNCloud.Shared.Models.Settings.RSVPProductMapping
{
    public class ProductItemModels
    {
        public string ID { get; set; }
        public string ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ItemCode { get; set; }
        /*Client*/
        public string ProductName { get; set; }
        public int ProductType { get; set; }
        public string StoreID { get; set; }
        public int StoreOffSet { get; set; }
        public int OffSet { get; set; }
        public byte Status { get; set; }
    }
}
