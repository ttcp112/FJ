using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class RegisterTokenModels
    {
        public string ConsumerKey { get; set; } = "";
        public string ConsumerSecret { get; set; } = "";
        public string TokenKey { get; set; } = "";
        public string TokenSecret { get; set; } = "";
        public DateTime ExpiresAt { get; set; } = DateTime.Now;
        public string Session { get; set; } = "";
        public DateTime SessionExpiresAt { get; set; } = DateTime.Now;
        public string OrganisationId { get; set; } = "";
        public string UserId { get; set; } = "";
        public bool HasExpired { get; set; } = false;
        public bool HasSessionExpired { get; set; } = false;
        public string MerchantId { get; set; } = "";
    }
}
