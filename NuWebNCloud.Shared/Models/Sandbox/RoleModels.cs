using NuWebNCloud.Shared.Models.Api;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Sandbox
{
    public class RoleModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public List<RoleModuleDTO> ListModule { get; set; }
        public List<RoleRawModuleDTO> ListDrawer { get; set; }

        public List<RoleModuleDTO> ListRoleModule { get; set; }
        public List<RoleRawModuleDTO> ListRoleDrawer { get; set; }

        //===========
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }
    }

    public class RoleModuleDTO
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsView { get; set; }
        public bool IsAction { get; set; }
        public bool IsActive { get; set; }
        public int Code { get; set; }
        public List<RoleModuleDTO> ListChild { get; set; }
    }

    public class RoleRawModuleDTO
    {
        public string DrawerID { get; set; }
        public string DrawerName { get; set; }
        public bool IsAction { get; set; }
        public bool IsView { get; set; }
        public bool IsActive { get; set; }

    }
    public class RoleViewModels
    {
        public string StoreID { get; set; }
        public List<RoleModels> ListItem { get; set; }
        public RoleViewModels()
        {
            ListItem = new List<RoleModels>();
        }
    }
}
