using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class TipServiceChargeApiModels
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }

        public bool IsApplyForEatIn { get; set; }
        public bool IsApplyForTakeAway { get; set; }
        public bool IsIncludedOnBill { get; set; }
        public bool IsAllowTip { get; set; }
        public bool IsCurrency { get; set; }
        public double Value { get; set; }
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
    }
}
