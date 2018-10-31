using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class BaseReportDataModel
    {
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public DateTime FromMonth { get; set; } = DateTime.Now;
        public DateTime ToMonth { get; set; } = DateTime.Now;
        public List<string> ListStores { get; set; }
        public int Mode { get; set; }
        public int TopTake { get; set; }
        public int ItemType { get; set; }
        public List<StoreInfo> ListStoreInfos { get; set; }
        public BaseReportDataModel()
        {
            ListStoreInfos = new List<StoreInfo>();
        }
    }
    public class StoreInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
