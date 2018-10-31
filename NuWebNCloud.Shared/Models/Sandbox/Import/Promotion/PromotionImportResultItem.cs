using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import.Promotion
{
    public class PromotionImportResultItem
    {
        public string Name { get; set; }
        public List<string> ListFailStoreName { get; set; }
        public List<string> ListSuccessStoreName { get; set; }
        public List<PromotionErrorItem> ErrorItems { get; set; }

        public PromotionImportResultItem()
        {
            ListFailStoreName = new List<string>();
            ListSuccessStoreName = new List<string>();
            ErrorItems = new List<PromotionErrorItem>();
        }
    }
    public class PromotionErrorItem
    {
        public string GroupName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
