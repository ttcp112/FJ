using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class ChatTemplateModels
    {
        public int Index { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int ChatTemplateType { get; set; } //enum EChatTemplate
        public List<string> ListOrgID { get; set; }

        public string sType { get { return (this.ChatTemplateType == (int)Commons.EChatTemplate.Artiste ? "Artiste" : "Customer"); }}
        public List<SelectListItem> ListType { get; set; }
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }
        public List<SelectListItem> ListMerchants { get; set; }
        public List<string> MerchantsSelected { get; set; }
        public ChatTemplateModels()
        {
            ListType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Artiste"), Value = Commons.EChatTemplate.Artiste.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Customer"), Value = Commons.EChatTemplate.Customer.ToString("d")},
            };
        }
    }

    public class ChatTemplateViewModels
    {
        public List<string> ListOrgID { get; set; }
        public bool IsActive { get; set; }
        public int ChatTemplateType { get; set; }
        public List<ChatTemplateModels> ListItemCus { get; set; }
        public List<ChatTemplateModels> ListItemArtist { get; set; }
        public string Id { get; set; }

        public ChatTemplateViewModels()
        {
            ListItemArtist = new List<ChatTemplateModels>();
            ListItemCus = new List<ChatTemplateModels>();
        }
    }
}
