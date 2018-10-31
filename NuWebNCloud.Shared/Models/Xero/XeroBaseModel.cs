using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero
{
    public class XeroBaseModel
    {
        [Required]
        public string AppRegistrationId { get; set; }

         public string AccessToken { get; set; }

        [Required]
        public string StoreId { get; set; }
    }
}
