using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.ScreenSaverMode
{
    public class ScreenSaverModeApiModels
    {
        public List<ScreenSaverModeApiModel> ListImageDTO { get; set; }
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
    }
}
