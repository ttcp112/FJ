using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class IngredientModel
    {
        public string Index { get; set; }

        public string Id { get; set; }

        [_AttributeForLanguage("Please choose company.")]
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }

        //[Required(ErrorMessage = "Code field is required")]
        [_AttributeForLanguage("Code field is required")]
        public string Code { get; set; }

        //[Required(ErrorMessage = "Name field is required")]
        [_AttributeForLanguage("Name field is required")]
        public string Name { get; set; }

        public string Description { get; set; }
        public string BaseUOMName { get; set; }

        public bool IsPurchase { get; set; }
        public bool IsSelfMode { get; set; }
        public bool IsStockable { get; set; }
        public bool IsCheckStock { get; set; }

        //[Required(ErrorMessage = "Please choose Base UOM")]
        [_AttributeForLanguage("Please choose Base UOM")]
        public string BaseUOMId { get; set; }

        //[Required(ErrorMessage = "Please choose Receiving UOM")]
        [_AttributeForLanguage("Please choose Receiving UOM")]
        public string ReceivingUOMId { get; set; }
        public string ReceivingUOMName { get; set; }

        //[Required(ErrorMessage = "Receiving Quantity field is required")]
        [_AttributeForLanguage("Receiving Quantity field is required")]
        //[Range(0, 999999999, ErrorMessage = "Please enter a value greater than or equal to 0")]
        [_AttributeForLanguageRange(0, 999999999, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double ReceivingQty { get; set; }

        public List<IngredientUOMModels> ListIngUOM { get; set; }

        //[Range(0, 100, ErrorMessage = "please input value from 0 to 100")]
        [_AttributeForLanguageRange(0, 100, ErrorMessage = "please input value from 0 to 100")]
        [_AttributeForLanguage("The Quantity Tolerance field is required")]
        public double QtyTolerance { get; set; }

        public List<SelectListItem> ListUOM { get; set; }

        //[Required]
        //[Range(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        [_AttributeForLanguage("The Purchase Price field is required")]
        public double PurchasePrice { get; set; }

        //[Required]
        //[Range(0, double.MaxValue, ErrorMessage = "Please enter a value greate than or equal to 0")]
        [_AttributeForLanguage("The Sale Price field is required")]
        public double SalePrice { get; set; }
        public double? ReOrderQty { get; set; }
        public double? MinAlertQty { get; set; }
        public double Qty { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }

        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }
        public string XeroId { get; set; }
        public bool Checked { get; set; }

        public List<Ingredients_SupplierModel> ListIngSupplier { get; set; }
        public List<Ingredients_SupplierModel> ListIngSupplierUnSelected { get; set; }


        /*Export - Import*/
        public List<string> ListCompany { get; set; }

        public List<string> ListStoreId { get; set; }

        public IngredientModel()
        {
            ReceivingQty = 1;

            IsActive = true;
            IsPurchase = true;
            ListIngUOM = new List<IngredientUOMModels>();
            ListCompany = new List<string>();

            ListIngSupplier = new List<Ingredients_SupplierModel>();
            ListIngSupplierUnSelected = new List<Ingredients_SupplierModel>();
            ReOrderQty = 0;
            MinAlertQty = 0;
        }

        public void GetFillData(List<string> lstMerchatIds)
        {
            ListUOM = new List<SelectListItem>();
            UnitOfMeasureFactory _UOMFactory = new UnitOfMeasureFactory();

            var lstItem = _UOMFactory.GetData(lstMerchatIds).Where(x => x.IsActive).ToList();
            if (lstItem != null)
            {
                foreach (UnitOfMeasureModel uom in lstItem)
                    ListUOM.Add(new SelectListItem
                    {
                        Text = uom.Name,
                        Value = uom.Id
                    });
            }
        }
    }

    public class IngredientViewModel
    {
        public string CompanyId { get; set; }
        public List<IngredientModel> ListItem { get; set; }
        public IngredientViewModel()
        {
            ListItem = new List<IngredientModel>();
        }
    }

    public class IngredientImportModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BaseUOMName { get; set; }
        public bool IsActive { get; set; }
    }

    /*Import Result*/
    public class IngredientImportResultModels
    {
        public List<IngredientImportResultItem> ListImport { get; set; }
        public int TotalRowExcel { get; set; }
        public string LevelName { get; set; }

        public IngredientImportResultModels()
        {
            ListImport = new List<IngredientImportResultItem>();
        }
    }

    public class IngredientImportResultItem
    {
        public string Name { get; set; }
        public List<string> ListFailCompanyName { get; set; }
        public List<string> ListSuccessCompanyName { get; set; }
        public List<IngredientErrorItem> ErrorItems { get; set; }

        public IngredientImportResultItem()
        {
            ListFailCompanyName = new List<string>();
            ListSuccessCompanyName = new List<string>();
            ErrorItems = new List<IngredientErrorItem>();
        }
    }

    public class IngredientErrorItem
    {
        public string GroupName { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class IngredientReceivingModels
    {
        public string StoreId { get; set; }
        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public double ReceivingQty { get; set; }
    }
}
