using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class OrderPaidModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }

        public string ReceiptNo { get; set; }
        public string Amount { get; set; }
        public string PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
