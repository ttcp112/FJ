using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class CompanyModels
    {
        public string Id { get; set; }
        public string OrganizationID { get; set; }
        public string OrganizationName { get; set; }

        public string Name { get; set; }
        public int Status { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string LastUserModified { get; set; }
        public DateTime LastDateModified { get; set; }
        public string ApiUrlExtend { get; set; }
        // Updated 07122018
        public List<StoreModels> ListStores { get; set; }
        public CompanyModels()
        {
            ListStores = new List<StoreModels>();
        }
    }
}
