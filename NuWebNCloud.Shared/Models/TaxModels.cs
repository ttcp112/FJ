using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class TaxModels
    {
        public string StoreId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public double Percent { get; set; }
        public int TaxType { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserCreated { get; set; }
    }
}
