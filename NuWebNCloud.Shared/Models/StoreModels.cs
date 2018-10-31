using NuWebNCloud.Shared.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NuWebNCloud.Shared.Models
{
    public class StoreModels
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string IndustryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string GSTRegNo { get; set; }
        public string TimeZone { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public DateTime LastDateModified { get; set; }
        public string LastUserModified { get; set; }
        public string StoreCode { get; set; }
        public bool IsIncludeTax { get; set; }
        public string HostUrlExtend { get; set; }
        public string NameExtend { get; set; }
        public string OrganizationName { get; set; }
        // Updated 07172018
        public string OrganizationId { get; set; }
        
    }

    public class StoreModelsRespone
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string AppKey { get; set; }
        public string AppSerect { get; set; }
        public bool IsActive { get; set; }
        public bool IsWithPoins { get; set; }
        public TerminalDTO ThirdParty { get; set; }
        public StoreModelsRespone()
        {
            ThirdParty = new TerminalDTO();
        }
    }
    public class TerminalDTO
    {
        public string StoreID { get; set; }

        public bool IsIntegrate { get; set; }

        public string ApiURL { get; set; }

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public string DrawerID { get; set; }

        public int Type { get; set; }

        public string ThirdPartyID { get; set; }
        public string Code { get; set; }
    }
}
