using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class ImportModel
    {
        public List<ImportItem> ListImport { get; set; }
        public int TotalRowExcel { get; set; }
        public string LevelName { get; set; }

        public ImportModel()
        {
            ListImport = new List<ImportItem>();
        }
    }
}
