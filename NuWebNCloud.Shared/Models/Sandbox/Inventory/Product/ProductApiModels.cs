using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Product
{
    public class ProductApiModels
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public string ParentID { get; set; }
        public string ParentName { get; set; }
        public string TypeID { get; set; }
        public string TypeName { get; set; }
        public string CategoryID { get; set; }
        public string CategoryName { get; set; }

        public int OrderByIndex { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public int ProductTypeCode { get; set; }
        public string ProductTypeID { get; set; }

        public string BarCode { get; set; }
        public string Description { get; set; }
        public string PrintOutText { get; set; }
        public double Cost { get; set; }
        public int Unit { get; set; }
        public string Measure { get; set; }
        public double Quantity { get; set; }
        public int Limit { get; set; }
        public bool IsAllowedDiscount { get; set; }
        public bool IsActive { get; set; }
        public bool IsCheckedStock { get; set; }
        public bool IsAllowedOpenPrice { get; set; }
        public bool IsPrintedOnCheck { get; set; }
        public DateTime ExpiredDate { get; set; }
        public bool HasServiceCharge { get; set; }
        public bool IsExtraFood { get; set; }
        public bool IsComingSoon { get; set; }
        public string ColorCode { get; set; }
        public string ImageURL { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public double ExtraPrice { get; set; }
        public double ServiceChargeValue { get; set; }

        public bool IsForce { get; set; }
        public bool IsOptional { get; set; }
        public string ProductSeason { get; set; }
        public List<ProductSeasonDTO> ListProductSeason { get; set; }

        public byte DefaultState { get; set; }
        public bool IsShowMessage { get; set; }
        public string Info { get; set; }
        public string Message { get; set; }
        public bool IsShowInReservation { get; set; }
        //=====
        public List<string> ListStoreID { get; set; }
        public List<ProductModels> ListProduct { get; set; }
        //=====
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }

        public List<GroupProductModels> ListGroup { get; set; }
        public List<PrinterOnProductModels> ListPrinter { get; set; }

        public double Price { get; set; }
        public string SeasonPriceID { get; set; }
        public double SeasonPrice { get; set; }

        public ProductModels ProductDTO { get; set; }
        //public List<ProductModels> ListProduct { get; set; }

        public int ProductType { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
