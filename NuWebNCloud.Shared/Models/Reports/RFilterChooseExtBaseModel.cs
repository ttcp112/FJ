using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class RFilterChooseExtBaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public bool Checked { get; set; }
        public string ParentId { get; set; }
        public int Code { get; set; }
        public double FixedPayAmount { get; set; }
        public List<RFilterChooseExtBaseModel> ListChilds { get; set; }
        public RFilterChooseExtBaseModel()
        {
            ListChilds = new List<RFilterChooseExtBaseModel>();
        }
    }
}
