using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class DishImportResultModels
    {
        public List<DishImportResultItem> ListImport { get; set; }
        public int TotalRowExcel { get; set; }
        public string LevelName { get; set; }

        public DishImportResultModels()
        {
            ListImport = new List<DishImportResultItem>();
        }
    }
}
