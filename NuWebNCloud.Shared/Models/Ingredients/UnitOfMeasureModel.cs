using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class UnitOfMeasureModel
    {
        public string Id { get; set; }

        [_AttributeForLanguage("Code field is required")]
        //[Required(ErrorMessage = "Code field is required")]
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string OrganizationId { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }
        public List<string> ListStores { get; set; }

        public string Index { get; set; }

        public UnitOfMeasureModel()
        {
            IsActive = true;
            ListStores = new List<string>();
        }
    }

    public class UnitOfMeasureViewModel
    {
        public List<UnitOfMeasureModel> ListItem { get; set; }
        public UnitOfMeasureViewModel()
        {
            ListItem = new List<UnitOfMeasureModel>();
        }
    }
}
