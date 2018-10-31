using NuWebNCloud.Shared.Factory.AccessControl;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.AccessControl
{
    public class RoleOrganizationModels
    {
        public string Id { get; set; }
        
        [_AttributeForLanguage("Role Name is required")]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public byte Status { get; set; }
        [_AttributeForLanguage("Please choose Organization")]
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public List<ModulePermissionModels> ListModule { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string RO { get; set; }

        public List<StoreItem> ListStore { get; set; }

        public List<CompanyItem> ListCompany { get; set; }
        public RoleOrganizationModels()
        {
            ListStore = new List<StoreItem>();
            ListCompany = new List<CompanyItem>();
            IsActive = true;
        }

        public void GetListStore(List<SelectListItem> lstStore)
        {
            foreach (var item in lstStore)
            {
                ListStore.Add(new StoreItem
                {
                    StoreId = item.Value,
                    StoreName = item.Text
                });
            }
        }

        public void GetModule()
        {
            ListModule = new List<ModulePermissionModels>();
            ModuleFactory mFactory = new ModuleFactory();
            List<ModuleModels> lstModule = mFactory.GetData();
            ModulePermissionModels module = new ModulePermissionModels();
            GetListModule(lstModule, "");
        }

        public List<ModulePermissionModels> GetListModule(List<ModuleModels> lstModule, string ParentId)
        {
            var lst = new List<ModulePermissionModels>();
            var listData = lstModule.Where(x => x.ParentID.Equals(ParentId)).ToList();
            foreach (var item in listData)
            {
                var listChild = GetListModule(lstModule, item.Id);
                ModulePermissionModels module = new ModulePermissionModels()
                {
                    Controller = item.Controller,
                    Id = item.Id,
                    IsAction = false,
                    IsActive = false,
                    IsView = false,
                    Name = item.Name,

                    ModuleID = item.Id, 
                    ModuleParentID = item.ParentID == null ? "" : item.ParentID,

                    ListChild = listChild
                };
                if (ParentId.Equals(""))
                    ListModule.Add(module);
                else
                    lst.Add(module);
            }
            return lst;
        }
    }

    public class RoleOrganizationViewModels
    {
        public string StoreId { get; set; }
        public List<RoleOrganizationModels> ListItem { get; set; }
        public RoleOrganizationViewModels()
        {
            ListItem = new List<RoleOrganizationModels>();
        }
    }
}
