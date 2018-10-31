using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.TransactionReasonSetting
{
   public class ReasonViewModels
    {
        public string StoreID { get; set; }
        public List<string> LstStoreID { get; set; }
        public List<ReasonModels> LstReason { get; set; }
        public ReasonViewModels()
        {
            LstReason = new List<ReasonModels>();
        }
    }
}
