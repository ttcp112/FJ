using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class DishGroupImportItem
    {
        public int GroupIndex { get; set; }

        public int Seq { get; set; }

        public int DishIndex { get; set; }

        public string DishName { get; set; }

        public string Name { get; set; }

        public string DisplayMessage { get; set; }

        public int Quantity { get; set; }

        public InforImportModel Infor { get; set; }

        public int RowCount { get; set; }

        public DishGroupImportItem()
        {
            Infor = new InforImportModel();
        }
    }
}
