using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import.Promotion
{
    public class PromotionImportResultModels
    {
        public List<PromotionImportResultItem> ListImport { get; set; }
        public int TotalRowExcel { get; set; }
        public string LevelName { get; set; }

        public PromotionImportResultModels()
        {
            ListImport = new List<PromotionImportResultItem>();
        }
    }
}
