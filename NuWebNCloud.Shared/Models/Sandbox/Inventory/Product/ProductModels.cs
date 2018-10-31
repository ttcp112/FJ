using NuWebNCloud.Shared.Models.Settings.Season;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Product
{
    public class ProductModels : SBInventoryBaseModel
    {
        public string Index { get; set; } /* Index in excel file */

        public string ID { get; set; }

        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public List<string> ListStoreID { get; set; }

        public string ParentID { get; set; }
        public string ParentName { get; set; }

        public string TypeID { get; set; }
        public string TypeName { get; set; }

        //[_AttributeForLanguage("Please choose Category for store")]
        public string CategoryID { get; set; }

        [_AttributeForLanguage("The Sequence field is required.")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int OrderByIndex { get; set; }

        [_AttributeForLanguage("Name field is required")]
        public string Name { get; set; }

        [_AttributeForLanguage("Code field is required")]
        public string ProductCode { get; set; }

        public string ProductTypeID { get; set; }
        public int ProductTypeCode { get; set; }

        public string BarCode { get; set; }

        public string Description { get; set; }
        public string PrintOutText { get; set; }

        [_AttributeForLanguage("Cost field is required")]
        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double Cost { get; set; }

        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Unit { get; set; }

        public string Measure { get; set; }

        [_AttributeForLanguage("The Quantity field is required.")]
        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double Quantity { get; set; }

        [_AttributeForLanguage("The Limit field is required.")]
        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Limit { get; set; }

        public bool IsAllowedDiscount { get; set; }
        public bool IsActive { get; set; }
        public bool IsCheckedStock { get; set; }
        public bool IsAllowedOpenPrice { get; set; }
        public bool IsPrintedOnCheck { get; set; }

        public DateTime? ExpiredDate { get; set; }

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

        public string sServiceCharge { get; set; }
        public double ServiceCharge { get; set; }
        public bool IsForce { get; set; }
        public bool IsOptional { get; set; }

        public string ProductSeason { get; set; }
        public List<ProductSeasonDTO> ListProductSeason { get; set; }
        public List<SeasonModels> ListSeason { get; set; }

        [_AttributeForLanguage("Please choose default Status")]
        public byte DefaultState { get; set; }

        public bool IsShowMessage { get; set; }
        public string Info { get; set; }
        public string Message { get; set; }
        public bool IsShowInReservation { get; set; }


        public double Price { get; set; }
        public double DefaultPrice { get; set; }
        public string SeasonPriceID { get; set; }
        public double SeasonPrice { get; set; }
        public double ExtraPrice { get; set; }

        public bool IsAddition { get; set; }
        public string ProductSeasonPOS { get; set; }
        public List<SeasonModels> ListSeasonPOS { get; set; }
        public List<ProductSeasonDTO> ListProductSeasonPOS { get; set; }
        public double Tax { get; set; }

        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }

        [DataType(DataType.Upload)]
        [FileSize(10240000)]
        [FileTypes("zip")]
        public HttpPostedFileBase ImageZipUpload { get; set; }

        //===========
        public List<GroupProductModels> ListGroup { get; set; }

        //Printer
        public List<string> lPrinter { get; set; }
        public List<PrinterOnProductModels> ListPrinter { get; set; }
        public List<PrinterModels> LstPrinter { get; set; }

        //Import
        public string Printer { get; set; }

        public ProductModels()
        {
            //ExpiredDate = DateTime.Now;
            ListProductSeason = new List<ProductSeasonDTO>();
            ListSeason = new List<SeasonModels>();
            ListSeasonPOS = new List<SeasonModels>();
            ListProductSeasonPOS = new List<ProductSeasonDTO>();
            ListGroup = new List<GroupProductModels>();
            ListPrinter = new List<PrinterOnProductModels>();

            LstPrinter = new List<PrinterModels>();
        }
    }

    public class ProductSeasonDTO
    {
        public string StoreID { get; set; }
        public string SeasonID { get; set; }
        public string SeasonName { get; set; }
        public bool IsPOS { get; set; }
    }
}
