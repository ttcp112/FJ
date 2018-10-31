using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NuWebNCloud.Shared.Models.Settings.DefaultCurency
{
    public class DefaultCurrencyModels 
    {
        public string Index { get; set; } /* Index in excel file */

        public string StoreName { get; set; }
        public bool IsSelected { get; set; }
        [_AttributeForLanguage("The Name field is required")]
        public string Name { get; set; }
        public string Status { get; set; }
        [_AttributeForLanguage("The Symbol field is required.")]
        public string Symbol { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string Mode { get; set; }
        public string CreatedUser { get; set; }
        [_AttributeForLanguage("Please choose store")]
        public string StoreId { get; set; }
        public string Id { get; set; }
        public string DeviceName { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public List<string> ListStores { get; set; }
        public List<ListCurrency> ListCurrency { get; set; }

        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }
    }

    public class ListCurrency
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool IsSelected { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
    }

}
