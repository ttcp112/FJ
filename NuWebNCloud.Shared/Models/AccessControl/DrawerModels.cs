using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.AccessControl
{
    public class DrawerModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }
        public string KickCode { get; set; }

        //===========
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }
    }

    public class DrawerViewModels
    {
        public string StoreID { get; set; }
        public List<DrawerModels> ListItem { get; set; }
        public DrawerViewModels()
        {
            ListItem = new List<DrawerModels>();
        }
    }
}
