using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class ChatTemplateApiModels :BaseApiRequestModel
    {
        public List<string> ListOrgID { get; set; }
        public ChatTemplateModels ChatTemplateDTO { get; set; }
        public bool IsActive { get; set; }
        public int? ChatTemplateType { get; set; }
        public string Id { get; set; }
        public List<ChatTemplateModels> ListTemplate { get; set; }
    }
}
