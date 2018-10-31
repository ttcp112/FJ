using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class PaymentMethodApiModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
        public bool IsActive { get; set; }
        public bool IsHasConfirmCode { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public byte Code { get; set; }
        public string GLAccountCode { get; set; }
        //public List<PaymentMethodModels> ListChild { get; set; }

        public PaymentMethodModels PayMethodDTO { get; set; }
        //=====================
        public List<PaymentMethodModels> ListPaymentMethod { get; set; }
        public List<string> ListStoreID { get; set; }
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
