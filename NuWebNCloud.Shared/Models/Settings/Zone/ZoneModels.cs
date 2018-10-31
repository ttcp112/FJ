using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Models.Settings.Zone
{
    public class ZoneModels
    {
        public string ID { get; set; }
        [_AttributeForLanguage("Zone Name is required")]
        public string Name { get; set; }
        [_AttributeForLanguage("Please choose store")]
        public string StoreID { get; set; }

        public string StoreName { get; set; }
        //public string DeviceName { get; set; }
        public string Description { get; set; }

        //[Range(0,255)]
        [_AttributeForLanguage("Number of rows is required")]
        [_AttributeForLanguageRange(0, 255, ErrorMessage = "Please enter a value greater than equal to 0 and maximun is 255")]
        public int Width { get; set; }

        //[Range(0, 255, ErrorMessage = "Please enter a value greater than equal to 0 and maximun is 255")]
        [_AttributeForLanguage("Number of columns")]
        [_AttributeForLanguageRange(0, 255, ErrorMessage = "Please enter a value greater than equal to 0 and maximun is 255")]
        public int Height { get; set; }
        
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
    }
}
