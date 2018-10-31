using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings.Zone;
using NuWebNCloud.Shared.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;using NuWebNCloud.Shared.Utilities;
using System.Web.Mvc;


namespace NuWebNCloud.Shared.Models.Settings
{
    public class TableModels
    {
        public string Index { get; set; } /* Index in excel file */

        public string ID { get; set; }
        [_AttributeForLanguage("Table Name is required")]  
        public string Name { get; set; }
        [_AttributeForLanguage("Please choose store")]
        public string StoreID { get; set; }

        public string StoreName { get; set; }
        [_AttributeForLanguage("Please choose Zone")]
        public string ZoneID { get; set; }
        public string ZoneName { get; set; }
        
        public List<SelectListItem> ListZone { get; set; }
        [_AttributeForLanguage("The Cover field is required.")]
        [_AttributeForLanguageRange(0, 999999999, ErrorMessage = "Please enter a value greater than or equal to 1")]      
        public int Cover { get; set; }


        public byte ViewMode { get; set; }
        public List<SelectListItem> ListTableStyle { get; set; }

        public double XPoint { get; set; }
        public double YPoint { get; set; }
        public double ZPoint { get; set; }
        public bool IsActive { get; set; }
        public bool IsShowInReservation { get; set; }
        public bool IsTemp { get; set; }
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        //==============
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }

        public List<string> ListStores { get; set; }

        public TableModels()
        {
            ListZone = new List<SelectListItem>();
            ListTableStyle = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Circle.ToString()), Value = Commons.ETableStyle.Circle.ToString("d")},
                new SelectListItem() { Text =  _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Rectangle.ToString()), Value = Commons.ETableStyle.Rectangle.ToString("d")},
                new SelectListItem() { Text =  _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Square.ToString()), Value = Commons.ETableStyle.Square.ToString("d")}
                //new SelectListItem() { Text = Commons.ETableStyle.Other.ToString(), Value = Commons.ETableStyle.Other.ToString("d")}
            };
        }
    }

    public class TableViewModels
    {
        public string StoreID { get; set; }
        public List<TableModels> ListItem { get; set; }
        public TableViewModels()
        {
            ListItem = new List<TableModels>();
        }
    }
}
