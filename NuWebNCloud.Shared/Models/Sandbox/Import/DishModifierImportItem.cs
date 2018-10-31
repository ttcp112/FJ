using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class DishModifierImportItem
    {
        public int ModifierIndex { get; set; }

        public int Sequence { get; set; }

        public int TabIndex { get; set; }

        public string Name { get; set; }

        public double ExtraPrice { get; set; }

        public InforImportModel Infor { get; set; }

        public int RowCount { get; set; }

        public DishModifierImportItem()
        {
            Infor = new InforImportModel();
        }
    }
}
