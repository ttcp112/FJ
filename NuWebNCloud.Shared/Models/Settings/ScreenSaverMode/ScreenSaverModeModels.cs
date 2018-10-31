using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NuWebNCloud.Shared.Models.Settings.ScreenSaverMode
{
    public class ScreenSaverModeModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string ImageURL { get; set; }
        public string ImageData { get; set; }
        public int OffSet { get; set; }
        public bool IsDelete { get; set; }
        public string ImageName { get; set; }
        public HttpPostedFileBase[] PictureUpload { get; set; }
        public byte[] PictureByte { get; set; }
        public string StoreID { get; set; }
        public List<ScreenSaverModeModels> ListProduct { get; set; }

        public ScreenSaverModeModels()
        {
            ListProduct = new List<ScreenSaverModeModels>();
        }
    }

    public class ScreenSaverModeApiModel
    {
        public string ID { get; set; }
        public bool IsActive { get; set; }
        public string ImageURL { get; set; }
        public string StoreID { get; set; }
    }

    public class ScreenSaverModeViewModels
    {
        public string StoreID { get; set; }
        public List<StoreModels> ListItem { get; set; }
        public ScreenSaverModeViewModels()
        {
            ListItem = new List<StoreModels>();
        }
    }
}
