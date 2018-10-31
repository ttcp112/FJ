using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion
{
    public class PromotionProductDTO
    {
        public string ProductID { get; set; }
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string CategoryID { get; set; }

        public byte ItemType { get; set; }

        public int OffSet { get; set; }
        public byte Status { get; set; }
        public bool Checked { get; set; }
        public bool IsAllowDiscount { get; set; }
    }
}
