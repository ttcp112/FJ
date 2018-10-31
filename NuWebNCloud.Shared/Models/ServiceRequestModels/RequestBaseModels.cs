using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class RequestBaseModels
    {
        public string CreatedUser { get; set; }
        public string MerchantID { get; set; }
        public string StoreID { get; set; }
        public string ID { get; set; }
        public string DeviceName { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public bool IsShowActive { get; set; }
        public List<string> ListStoreID { get; set; }
        public bool IsIntergrate { get; set; }
        public bool IsWithNupos { get; set; }
         public string AppRegisteredID { get; set; }
        public bool IsAssort { get; set; }

        public RequestBaseModels()
        {
            PageSize = 6000;
            PageIndex = 1;

            ListStoreID = new List<string>();
        }
    }
}
