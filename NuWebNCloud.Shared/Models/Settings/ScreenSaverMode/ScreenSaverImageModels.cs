using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.ScreenSaverMode
{
    public class ScreenSaverImageModels
    {
        public int OffSet { get; set; }
        public List<ImageModels> ListImage { get; set; }

        public ScreenSaverImageModels()
        {
            ListImage = new List<ImageModels>();
        }
    }

    public class ImageModels
    {
        public string ImageURL { get; set; }
    }
}
