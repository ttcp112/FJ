using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
using System;
using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory
{
    public class DiscountModels : SandboxImportModel
    {
        //public string IndexImport { get; set; }/* Index of in excel file */
        public string Index { get; set; } /* Index of in excel file */
        public string ID { get; set; }

       // [Required(ErrorMessage = "Please choose Store.")]
       [_AttributeForLanguage("Please choose store.")]
        public string StoreID { get; set; }

        public string StoreName { get; set; }

        //[Required]
        [_AttributeForLanguage("The Name field is required")]
        public string Name { get; set; }
        public string Description { get; set; }

        // [Range(0, 9999999999, ErrorMessage = "Please enter a value greater than or equal to 1. ")]
        [_AttributeForLanguage("The Value field is required")]
        [_AttributeForLanguageRange(0, 9999999999, ErrorMessage = "Please enter a value greater than or equal to 1. ")]
        public double Value { get; set; }

        public byte Type { get; set; }
        public bool BType { get; set; }
        public bool IsAllowOpenValue { get; set; }
        public bool IsApplyTotalBill { get; set; }
        public bool IsActive { get; set; }
        public byte Status { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
    }
}
