using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class ImportItem
    {
        public string Name { get; set; }
        public List<string> ListFailStoreName { get; set; }
        public List<string> ListSuccessStoreName { get; set; }
        public string StoreNames { get; set; }
        public int Count { get; set; }
        public List<string> ListErrorMsg { get; set; }

        public ImportItem(string Name = "", string StoreNames = "", int Count = 0)
        {
            this.Name = Name;
            this.StoreNames = StoreNames;
            this.Count = Count;

            ListFailStoreName = new List<string>();
            ListSuccessStoreName = new List<string>();
            ListErrorMsg = new List<string>();
        }
    }
}
