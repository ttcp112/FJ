using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class EmployeeApiModels
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string StoreId { get; set; }
    }

    public class GetAllEmployee
    {
        public List<string> ListStoreID { get; set; }
    }
}
