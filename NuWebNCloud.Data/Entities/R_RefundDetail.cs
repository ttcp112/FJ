using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Entities
{
    public partial class R_RefundDetail
    {
        public string Id { get; set; }
        public string RefundId { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; }
        public double Qty { get; set; }
        public double ServiceCharged { get; set; }
        public double Tax { get; set; }
        public double PromotionAmount { get; set; }
        public double PriceValue { get; set; }
        public double DiscountAmount { get; set; }
    }
}
