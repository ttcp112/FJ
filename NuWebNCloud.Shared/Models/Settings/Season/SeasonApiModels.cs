using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.Season
{
    public class SeasonApiModels
    {
        public string StoreID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string IsRepeat { get; set; }
        public List<RepeatModels> ListRepeat { get; set; }
        public bool IsInventory { get; set; }
        public bool Unlimited { get; set; }

        public SeasonModels SeasonDTO { get; set; }

        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }

        public List<string> ListStoreID { get; set; }
        public List<SeasonModels> ListSeason { get; set; }

        //=====
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
