using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class TableApiModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public string ZoneID { get; set; }
        public int Cover { get; set; }
        public byte ViewMode { get; set; }
        public double XPoint { get; set; }
        public double YPoint { get; set; }
        public double ZPoint { get; set; }
        public bool IsActive { get; set; }
        public bool IsShowInReservation { get; set; }
        public bool IsTemp { get; set; }

        public List<TableModels> ListTable { get; set; }
        public List<string> ListStoreID { get; set; }
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
