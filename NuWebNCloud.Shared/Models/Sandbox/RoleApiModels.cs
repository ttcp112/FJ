using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox
{
    public class RoleApiModels
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public List<RoleModuleDTO> ListRoleModule { get; set; }
        public List<RoleRawModuleDTO> ListRoleDrawer { get; set; }
        //===========
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }
    }
}
