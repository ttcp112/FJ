using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class InforImportModel
    {
        public bool IsValidRow { get; set; }
        public List<string> StoresAffeted { get; set; }
        public List<string> StoresFailed { get; set; }
        public List<string> Errors { get; set; }

        public InforImportModel()
        {
            IsValidRow = true;
            StoresAffeted = new List<string>();
            StoresFailed = new List<string>();
            Errors = new List<string>();
        }
    }
}
