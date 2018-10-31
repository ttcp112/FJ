using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class SetMenuImportResultModels
    {
        public List<SetMenuImportResultItem> ListImport { get; set; }
        public int TotalRowExcel { get; set; }
        public string LevelName { get; set; }
        public string Name { get; set; }

        public SetMenuImportResultModels()
        {
            ListImport = new List<SetMenuImportResultItem>();
        }
    }
}
