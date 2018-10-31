
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
        public class MerchantExtendRequestModel
        {
            public string POSAPIMerchantConfigExtendId { get; set; }
            public string UserName { get; set; }
            public List<string> ListStoreExtendIds { get; set; }
            public MerchantExtendRequestModel()
            {
            ListStoreExtendIds = new List<string>();
            }
        }
}
