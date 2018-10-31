using NuWebNCloud.Shared.Factory.AccessControl;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.AccessControl
{
    public class ModuleModels
    {
        //Service
        //public string ID { get; set; }
        //public string Name { get; set; }
        //public List<RoleModuleDTO> ListRoleModule { get; set; }
        //public List<RoleRawModuleDTO> ListRoleDrawer { get; set; }
        ////===========
        //public string CreatedUser { get; set; }
        //public string AppKey { get; set; }
        //public string AppSecret { get; set; }
        //public RegisterTokenModels RegisterToken { get; set; }
        //public byte Mode { get; set; }

        public string Id { get; set; }
        [Required]
        public string Name { get; set; }

        public string ParentName { get; set; }
        public string ParentID { get; set; }
        public byte Status { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int IndexNum { get; set; }

        [Required]
        public string Controller { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }

        public List<SelectListItem> ListParent { get; set; }

        public List<ModuleModels> ListChild { get; set; }
        public ModuleModels()
        {
            ListParent = new List<SelectListItem>();
            ListChild = new List<ModuleModels>();
        }

        public void GetParent()
        {
            ModuleFactory _factory = new ModuleFactory();
            var data = _factory.GetParent();
            foreach (ModuleModels item in data)
            {
                ListParent.Add(new SelectListItem
                {
                    Value = item.Id,
                    Text = item.Name
                });
            }
        }
    }

    public class ModuleViewModels
    {
        public List<ModuleModels> ListItem { get; set; }
        public ModuleViewModels()
        {
            ListItem = new List<ModuleModels>();
        }
    }
}
