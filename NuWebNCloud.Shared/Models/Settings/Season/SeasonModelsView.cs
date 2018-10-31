using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings.Season
{
   public class SeasonModelsView
    {
        public string StoreID { get; set; }
        public List<SeasonModels> List_Season { get; set; }
        public SeasonModelsView()
        {
            List_Season = new List<SeasonModels>();
        }
    }
}
